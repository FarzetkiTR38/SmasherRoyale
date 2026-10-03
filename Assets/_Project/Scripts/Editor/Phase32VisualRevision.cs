using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace RoyalSmash.Editor
{
    /// <summary>Incremental art revisions that leave level and physics data intact.</summary>
    public static class Phase32VisualRevision
    {
        private const string ArtRoot = "Assets/_Project/Art/SkyCitadel";
        private const string MainScene = "Assets/_Project/Scenes/MainGame.unity";
        private const string UndoLabel = "Phase 32 lighting and material foundation";
        private const string ReflectionPath = ArtRoot + "/Lighting/CitadelCourtyard.exr";

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/02 - Apply Lighting Foundation")]
        public static void ApplyLightingFoundation()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != MainScene)
                throw new InvalidOperationException("Open MainGame in Edit Mode first.");

            // Validate dependencies before changing shared assets. No legacy builder calls.
            Material gold = RequireMaterial("AntiqueGold");
            Material sapphire = RequireMaterial("RoyalSapphire");
            Material steel = RequireMaterial("ObsidianSteel");
            Material stone = RequireMaterial("IvoryStone");
            Material wood = RequireMaterial("WarmWood");
            var floor = AssetDatabase.LoadAssetAtPath<Material>(ArtRoot + "/Materials/M_MarbleFloor.mat");
            if (floor == null) throw new InvalidOperationException("Existing marble floor material is missing.");
            var environment = GameObject.Find("SkyCitadelEnvironment");
            var sunObject = GameObject.Find("Directional Light");
            var sun = sunObject != null ? sunObject.GetComponent<Light>() : null;
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ArtRoot + "/CitadelVolume.asset");
            if (environment == null || sun == null || profile == null ||
                !profile.TryGet(out Bloom bloom) ||
                !profile.TryGet(out ColorAdjustments colors) ||
                !profile.TryGet(out Tonemapping tonemapping))
                throw new InvalidOperationException("Expected the existing citadel environment, sun and volume.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoLabel);

            SetLit(gold, new Color(1f, .73f, .22f), .64f, .74f);
            SetLit(sapphire, new Color(.025f, .17f, .62f), .48f, .72f);
            SetLit(steel, new Color(.095f, .135f, .205f), .64f, .59f);
            SetLit(stone, new Color(1f, .97f, .91f), .025f, .43f);
            SetLit(wood, new Color(.82f, .35f, .10f), 0f, .32f);
            // Stage 32.6 owns the surface-specific floor shader once installed.
            if (floor.shader.name == "Universal Render Pipeline/Lit")
                SetLit(floor, Color.white, .035f, .53f);

            Undo.RecordObject(sun, UndoLabel);
            sun.color = new Color(1f, .955f, .86f);
            sun.intensity = 1.72f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = .76f;
            EditorUtility.SetDirty(sun);

            var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
            {
                Undo.RecordObject(cameraData, UndoLabel);
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                EditorUtility.SetDirty(cameraData);
            }

            // A weak cool fill separates navy metal from dark joints without extra shadows.
            Transform fillTransform = environment.transform.Find("CitadelCoolFill");
            if (fillTransform == null)
            {
                var go = new GameObject("CitadelCoolFill");
                Undo.RegisterCreatedObjectUndo(go, UndoLabel);
                go.transform.SetParent(environment.transform, false);
                fillTransform = go.transform;
            }
            var fill = fillTransform.GetComponent<Light>();
            if (fill == null) fill = Undo.AddComponent<Light>(fillTransform.gameObject);
            Undo.RecordObjects(new UnityEngine.Object[] { fill, fillTransform }, UndoLabel);
            fillTransform.localRotation = Quaternion.Euler(28f, 145f, 0f);
            fill.type = LightType.Directional;
            fill.color = new Color(.54f, .75f, 1f);
            fill.intensity = .30f;
            fill.shadows = LightShadows.None;
            fill.cullingMask = sun.cullingMask;

            Undo.RecordObjects(new UnityEngine.Object[] { profile, bloom, colors, tonemapping }, UndoLabel);
            bloom.active = true;
            bloom.intensity.Override(.24f);
            bloom.threshold.Override(1.2f);
            bloom.scatter.Override(.48f);
            bloom.clamp.Override(4f);
            colors.active = true;
            colors.postExposure.Override(0f);
            colors.contrast.Override(10f);
            colors.saturation.Override(9f);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.Neutral);
            foreach (var component in new VolumeComponent[] { bloom, colors, tonemapping })
                EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(profile);

            // Bake once in the editor. No per-frame cubemap render or runtime script.
            Transform probeTransform = environment.transform.Find("CitadelCourtyardReflection");
            if (probeTransform == null)
            {
                var go = new GameObject("CitadelCourtyardReflection");
                Undo.RegisterCreatedObjectUndo(go, UndoLabel);
                go.transform.SetParent(environment.transform, false);
                probeTransform = go.transform;
            }
            var probe = probeTransform.GetComponent<ReflectionProbe>();
            if (probe == null) probe = Undo.AddComponent<ReflectionProbe>(probeTransform.gameObject);
            Undo.RecordObjects(new UnityEngine.Object[] { probe, probeTransform }, UndoLabel);
            probeTransform.position = new Vector3(0f, 2.8f, 4.5f);
            probe.mode = ReflectionProbeMode.Baked;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.resolution = 128;
            probe.hdr = true;
            probe.size = new Vector3(36f, 20f, 40f);
            probe.center = Vector3.zero;
            probe.blendDistance = 2f;
            probe.boxProjection = false;
            probe.intensity = 1.1f;
            probe.nearClipPlane = .15f;
            probe.farClipPlane = 100f;
            probe.clearFlags = ReflectionProbeClearFlags.Skybox;
            // Targets/projectiles/UI are transient and must not become baked reflections.
            probe.cullingMask = ~((1 << 5) | (1 << 6) | (1 << 7));
            Directory.CreateDirectory(ArtRoot + "/Lighting");
            if (!Lightmapping.BakeReflectionProbe(probe, ReflectionPath))
                throw new InvalidOperationException("Courtyard reflection bake failed; scene is not saved.");
            AssetDatabase.ImportAsset(ReflectionPath, ImportAssetOptions.ForceSynchronousImport);
            var reflection = AssetDatabase.LoadAssetAtPath<Cubemap>(ReflectionPath);
            if (reflection == null)
                throw new InvalidOperationException("Baked reflection did not import as a Cubemap.");
            // A manually assigned bakedTexture is not persisted without scene lighting data.
            // Keep the editor-baked asset in the serialized custom slot across reloads/builds.
            probe.mode = ReflectionProbeMode.Custom;
            probe.customBakedTexture = reflection;
            EditorUtility.SetDirty(probe);
            EditorUtility.SetDirty(fill);

            foreach (var material in new[] { gold, sapphire, steel, stone, wood, floor })
                AssetDatabase.SaveAssetIfDirty(material);
            AssetDatabase.SaveAssetIfDirty(profile);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
            Debug.Log("[Phase32] Lighting foundation applied. Verify the Game view before saving MainGame.");
        }

        private static Material RequireMaterial(string name)
        {
            string path = ArtRoot + "/Materials/M_" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool polishedMetal = material != null && material.shader != null &&
                (name == "AntiqueGold" || name == "RoyalSapphire") &&
                material.shader.name == "RoyalSmash/Citadel Polished Metal";
            if (material == null || material.shader == null ||
                (material.shader.name != "Universal Render Pipeline/Lit" && !polishedMetal))
                throw new InvalidOperationException("Expected existing URP Lit material: " + path);
            return material;
        }

        private static void SetLit(Material material, Color color, float metallic, float smoothness)
        {
            Undo.RecordObject(material, UndoLabel);
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            // Preserve shader, texture maps, UV transforms and asset identity.
            EditorUtility.SetDirty(material);
        }
    }
}
