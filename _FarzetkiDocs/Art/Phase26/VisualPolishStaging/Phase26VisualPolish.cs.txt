using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

namespace RoyalSmash.Editor
{
    /// <summary>Revises the existing citadel palette without rebuilding scene or prefab content.</summary>
    public static class Phase26VisualPolish
    {
        private const string ART_ROOT = "Assets/_Project/Art/SkyCitadel";
        private const string MAIN_SCENE = "Assets/_Project/Scenes/MainGame.unity";
        private const string UNDO_LABEL = "Polish Sky Citadel materials and lighting";

        [MenuItem("Royal Smash/Sky Citadel/Phase 26 Visual Polish")]
        public static void Apply()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != MAIN_SCENE)
                throw new InvalidOperationException("Phase 26 visual polish requires MainGame in Edit Mode.");

            // Resolve every required asset before modifying anything. GUIDs, shaders, maps,
            // map transforms, meshes, prefab overrides and gameplay components remain intact.
            Material stone = RequireMaterial("IvoryStone");
            Material gold = RequireMaterial("AntiqueGold");
            Material blue = RequireMaterial("RoyalSapphire");
            Material dark = RequireMaterial("ObsidianSteel");
            Material wood = RequireMaterial("WarmWood");
            Material energy = RequireMaterial("ArcaneEnergy");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ART_ROOT + "/CitadelVolume.asset");
            if (profile == null) throw new InvalidOperationException("Existing CitadelVolume profile is missing.");
            if (!energy.HasProperty("_CoreColor"))
                throw new InvalidOperationException("Import the revised CitadelEnergy shader before applying the palette.");

            Light sun = null;
            foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (light.gameObject.scene == scene && light.name == "Directional Light" && light.type == LightType.Directional)
                    sun = light;
            if (sun == null) throw new InvalidOperationException("MainGame Directional Light is missing.");

            bool profileInScene = false;
            foreach (Volume volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (volume.gameObject.scene == scene && volume.sharedProfile == profile) profileInScene = true;
            if (!profileInScene) throw new InvalidOperationException("MainGame does not use the existing CitadelVolume profile.");

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UNDO_LABEL);

            SetLit(stone, new Color(0.995f, 0.965f, 0.885f), 0.025f, 0.36f);
            SetLit(gold, new Color(1f, 0.72f, 0.24f), 0.52f, 0.75f);
            SetLit(blue, new Color(0.025f, 0.22f, 0.70f), 0.24f, 0.65f);
            SetLit(dark, new Color(0.09f, 0.16f, 0.24f), 0.42f, 0.56f);
            SetLit(wood, new Color(0.70f, 0.32f, 0.115f), 0f, 0.30f);

            Undo.RecordObject(energy, UNDO_LABEL);
            energy.SetColor("_BaseColor", new Color(0.015f, 0.20f, 0.92f));
            energy.SetColor("_GlowColor", new Color(0.025f, 1.15f, 1.8f));
            energy.SetColor("_CoreColor", new Color(0.30f, 1.65f, 2.25f));
            energy.SetFloat("_NoiseScale", 8f);
            energy.SetFloat("_FlowSpeed", 0.13f);
            energy.SetFloat("_RimStrength", 0.34f);
            energy.SetFloat("_SpecularStrength", 0.28f);
            EditorUtility.SetDirty(energy);

            // Retain the user's light direction and shadow geometry; lift color separation.
            Undo.RecordObject(sun, UNDO_LABEL);
            sun.color = new Color(1f, 0.97f, 0.90f);
            sun.intensity = 1.50f;
            sun.shadowStrength = 0.64f;
            EditorUtility.SetDirty(sun);

            Undo.RecordObject(profile, UNDO_LABEL);
            Bloom bloom = GetOrAdd<Bloom>(profile);
            Undo.RecordObject(bloom, UNDO_LABEL);
            bloom.active = true;
            bloom.intensity.Override(0.26f);
            bloom.threshold.Override(1.12f);
            bloom.scatter.Override(0.48f);
            bloom.clamp.Override(4f);
            EditorUtility.SetDirty(bloom);

            ColorAdjustments colors = GetOrAdd<ColorAdjustments>(profile);
            Undo.RecordObject(colors, UNDO_LABEL);
            colors.active = true;
            colors.postExposure.Override(-0.03f);
            colors.contrast.Override(7f);
            colors.saturation.Override(7f);
            EditorUtility.SetDirty(colors);

            // Neutral compresses the small HDR energy/specular peaks without bleaching cyan.
            Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
            Undo.RecordObject(tonemapping, UNDO_LABEL);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.Neutral);
            EditorUtility.SetDirty(tonemapping);
            EditorUtility.SetDirty(profile);

            // Save only these assets; the caller owns final scene save and visual verification.
            foreach (Material material in new[] { stone, gold, blue, dark, wood, energy })
                AssetDatabase.SaveAssetIfDirty(material);
            AssetDatabase.SaveAssetIfDirty(profile);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            SceneView.RepaintAll();
            Debug.Log("[Phase26VisualPolish] Existing palette, flowing energy, sun and volume revised. MainGame is ready for visual verification and scene save.");
        }

        private static Material RequireMaterial(string name)
        {
            string path = ART_ROOT + "/Materials/M_" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) throw new InvalidOperationException("Missing existing material: " + path);
            return material;
        }

        private static void SetLit(Material material, Color color, float metallic, float smoothness)
        {
            Undo.RecordObject(material, UNDO_LABEL);
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing)) return existing;
            T component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            Undo.RegisterCreatedObjectUndo(component, UNDO_LABEL);
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
