using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoyalSmash.Editor
{
    /// <summary>Updates the existing ground material, without changing ground geometry or collision.</summary>
    public static class Phase32FloorRevision
    {
        private const string Art = "Assets/_Project/Art/SkyCitadel";
        private const string TexturePath = Art + "/Textures/T_CitadelMarble_Reference.png";

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/06 - Apply Reference Floor")]
        public static void Apply()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the floor.");

            var material = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/M_MarbleFloor.mat");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var shader = Shader.Find("RoyalSmash/Citadel Floor");
            var ground = GameObject.Find("Ground");
            if (material == null || texture == null || shader == null || !shader.isSupported || ground == null)
                throw new InvalidOperationException("Existing ground, material, reference texture and supported floor shader are required.");
            var renderer = ground.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial != material)
                throw new InvalidOperationException("Ground must already use the existing M_MarbleFloor material.");

            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.maxTextureSize = 2048;
            // The source is 1254 square; a power-of-two import permits GPU block compression.
            importer.npotScale = TextureImporterNPOTScale.ToLarger;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            var desktop = importer.GetPlatformTextureSettings("Standalone");
            desktop.name = "Standalone";
            desktop.overridden = true;
            desktop.maxTextureSize = 2048;
            desktop.format = TextureImporterFormat.BC7;
            desktop.compressionQuality = 80;
            importer.SetPlatformTextureSettings(desktop);
            // ASTC keeps the imported 2K surface within a small mobile texture budget.
            foreach (string platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.name = platform;
                settings.overridden = true;
                settings.maxTextureSize = 2048;
                settings.format = TextureImporterFormat.ASTC_6x6;
                settings.compressionQuality = 80;
                importer.SetPlatformTextureSettings(settings);
            }
            importer.SaveAndReimport();

            Undo.RecordObject(material, "Phase32 reference courtyard floor");
            Vector2 tiling = material.GetTextureScale("_BaseMap");
            Vector2 offset = material.GetTextureOffset("_BaseMap");
            material.shader = shader;
            material.shaderKeywords = Array.Empty<string>();
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));
            material.SetTextureScale("_BaseMap", tiling);
            material.SetTextureOffset("_BaseMap", offset);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_MarbleMetallic", .02f);
            material.SetFloat("_MarbleSmoothness", .67f);
            material.SetFloat("_GoldMetallic", .32f);
            material.SetFloat("_GoldSmoothness", .78f);
            material.SetFloat("_BlueMetallic", .15f);
            material.SetFloat("_BlueSmoothness", .86f);
            material.SetFloat("_MicroHeight", .0006f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Phase32FloorRevision] Existing floor material revised; original shared marble texture, ground mesh, transform and collider preserved.");
        }
    }
}
