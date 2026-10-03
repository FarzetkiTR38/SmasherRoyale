using System;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Generates the standard EnvironmentConfig asset with the 3 milestone themes.
    /// </summary>
    public static class EnvironmentConfigGenerator
    {
        [MenuItem("RoyalSmash/Setup/Generate Environment Config")]
        public static EnvironmentConfig GenerateAsset()
        {
            string path = "Assets/_Project/ScriptableObjects/Config/EnvironmentConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(path);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EnvironmentConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            var theme1 = new EnvironmentTheme
            {
                ThemeName = "Castle Courtyard",
                StartLevel = 1,
                EndLevel = 10,
                SunLightColor = new Color(1.0f, 0.96f, 0.88f),
                SunLightIntensity = 1.15f,
                SunEulerAngles = new Vector3(50f, -30f, 0f),
                SkyTopColor = new Color(0.35f, 0.65f, 0.95f),
                SkyHorizonColor = new Color(0.75f, 0.88f, 0.98f),
                GroundColor = new Color(0.28f, 0.52f, 0.22f),
                EnableFog = true,
                FogColor = new Color(0.72f, 0.85f, 0.95f),
                FogStartDistance = 18f,
                FogEndDistance = 48f
            };

            var theme2 = new EnvironmentTheme
            {
                ThemeName = "Royal Ramparts (Sunset)",
                StartLevel = 11,
                EndLevel = 20,
                SunLightColor = new Color(1.0f, 0.78f, 0.55f),
                SunLightIntensity = 1.25f,
                SunEulerAngles = new Vector3(35f, -45f, 0f),
                SkyTopColor = new Color(0.45f, 0.35f, 0.70f),
                SkyHorizonColor = new Color(0.95f, 0.65f, 0.45f),
                GroundColor = new Color(0.42f, 0.35f, 0.25f),
                EnableFog = true,
                FogColor = new Color(0.92f, 0.68f, 0.55f),
                FogStartDistance = 16f,
                FogEndDistance = 42f
            };

            var theme3 = new EnvironmentTheme
            {
                ThemeName = "Grand Palace Arena",
                StartLevel = 21,
                EndLevel = 30,
                SunLightColor = new Color(0.95f, 0.92f, 1.0f),
                SunLightIntensity = 1.35f,
                SunEulerAngles = new Vector3(60f, -20f, 0f),
                SkyTopColor = new Color(0.25f, 0.20f, 0.55f),
                SkyHorizonColor = new Color(0.65f, 0.55f, 0.85f),
                GroundColor = new Color(0.30f, 0.25f, 0.40f),
                EnableFog = true,
                FogColor = new Color(0.55f, 0.48f, 0.75f),
                FogStartDistance = 14f,
                FogEndDistance = 40f
            };

            typeof(EnvironmentConfig).GetField("_themes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(config, new EnvironmentTheme[] { theme1, theme2, theme3 });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"[EnvironmentConfigGenerator] Created and configured EnvironmentConfig at '{path}'.");

            return config;
        }
    }
}
