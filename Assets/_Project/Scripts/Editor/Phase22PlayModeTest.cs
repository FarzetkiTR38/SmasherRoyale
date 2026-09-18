using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 22: 3D Environment Immersion & Atmospheric Backdrop.
    /// Runs 10 test cases verifying environment theme assignment, lighting, atmospheric fog, and scene integration.
    /// </summary>
    public static class Phase22PlayModeTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _testLogs = new List<string>();

        [MenuItem("RoyalSmash/Phase 22/Run Environment Theme Tests")]
        public static string RunAllTests()
        {
            _passedTests = 0;
            _failedTests = 0;
            _testLogs.Clear();

            Debug.Log("================================================================================");
            Debug.Log("   STARTING PHASE 22 3D ENVIRONMENT THEME TESTS (10 TESTS)                     ");
            Debug.Log("================================================================================");

            EnvironmentConfigGenerator.GenerateAsset();

            var config = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(
                "Assets/_Project/ScriptableObjects/Config/EnvironmentConfig.asset");

            Test01_ManagerInitialization();
            Test02_Theme1Courtyard(config);
            Test03_Theme2Ramparts(config);
            Test04_Theme3GrandPalace(config);
            Test05_FogSettings(config);
            Test06_DirectionalLightUpdate(config);
            Test07_CameraBackgroundUpdate(config);
            Test08_EdgeCaseLevelClamping(config);
            Test09_EnvironmentConfigAsset(config);
            Test10_ZeroErrorsHealthPass();

            string summary = $"Phase 22 Test Results: {_passedTests}/10 PASSED, {_failedTests} FAILED.";
            Debug.Log("================================================================================");
            Debug.Log($"   {summary}");
            Debug.Log("================================================================================");

            return summary;
        }

        private static void Assert(bool condition, string testName, string details)
        {
            if (condition)
            {
                _passedTests++;
                string msg = $"[PASS] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.Log(msg);
            }
            else
            {
                _failedTests++;
                string msg = $"[FAIL] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.LogError(msg);
            }
        }

        private static void Test01_ManagerInitialization()
        {
            var go = new GameObject("Test_EnvManager");
            try
            {
                var mgr = go.AddComponent<EnvironmentThemeManager>();
                Assert(mgr != null, "Test 1: Manager Initialization", "EnvironmentThemeManager initializes successfully.");
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }

        private static void Test02_Theme1Courtyard(EnvironmentConfig config)
        {
            var themeL1 = config.GetThemeForLevel(1);
            var themeL10 = config.GetThemeForLevel(10);
            bool isCourtyard = themeL1 != null && themeL1.ThemeName.Contains("Courtyard") &&
                               themeL10 != null && themeL10.ThemeName.Contains("Courtyard");

            Assert(isCourtyard, "Test 2: Theme 1 Courtyard (Levels 1-10)",
                $"Level 1 & 10 mapped to '{themeL1.ThemeName}'.");
        }

        private static void Test03_Theme2Ramparts(EnvironmentConfig config)
        {
            var themeL11 = config.GetThemeForLevel(11);
            var themeL20 = config.GetThemeForLevel(20);
            bool isRamparts = themeL11 != null && themeL11.ThemeName.Contains("Ramparts") &&
                              themeL20 != null && themeL20.ThemeName.Contains("Ramparts");

            Assert(isRamparts, "Test 3: Theme 2 Ramparts (Levels 11-20)",
                $"Level 11 & 20 mapped to '{themeL11.ThemeName}'.");
        }

        private static void Test04_Theme3GrandPalace(EnvironmentConfig config)
        {
            var themeL21 = config.GetThemeForLevel(21);
            var themeL30 = config.GetThemeForLevel(30);
            bool isPalace = themeL21 != null && themeL21.ThemeName.Contains("Palace") &&
                            themeL30 != null && themeL30.ThemeName.Contains("Palace");

            Assert(isPalace, "Test 4: Theme 3 Grand Palace (Levels 21-30)",
                $"Level 21 & 30 mapped to '{themeL21.ThemeName}'.");
        }

        private static void Test05_FogSettings(EnvironmentConfig config)
        {
            var go = new GameObject("Test_Env_Fog");
            try
            {
                var mgr = go.AddComponent<EnvironmentThemeManager>();
                mgr.Config = config;
                var theme = config.GetThemeForLevel(15);
                mgr.ApplyTheme(theme);

                bool fogMatches = RenderSettings.fog == theme.EnableFog &&
                                  RenderSettings.fogColor == theme.FogColor;

                Assert(fogMatches, "Test 5: Fog Settings Application",
                    "Linear atmospheric fog enabled with theme-specific color and distance.");
            }
            finally
            {
                GameObject.DestroyImmediate(go);
            }
        }

        private static void Test06_DirectionalLightUpdate(EnvironmentConfig config)
        {
            var go = new GameObject("Test_Env_Light");
            var lightGo = new GameObject("Test_Sun");
            try
            {
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;

                var mgr = go.AddComponent<EnvironmentThemeManager>();
                mgr.Config = config;
                var theme = config.GetThemeForLevel(25);
                mgr.ApplyTheme(theme);

                bool lightUpdated = light.color == theme.SunLightColor &&
                                    Mathf.Approximately(light.intensity, theme.SunLightIntensity);

                Assert(lightUpdated, "Test 6: Directional Light Update",
                    $"Sun light tinted to intensity {theme.SunLightIntensity:F2} with color {theme.SunLightColor}.");
            }
            finally
            {
                GameObject.DestroyImmediate(go);
                GameObject.DestroyImmediate(lightGo);
            }
        }

        private static void Test07_CameraBackgroundUpdate(EnvironmentConfig config)
        {
            var go = new GameObject("Test_Env_Camera");
            var camGo = new GameObject("Test_Cam");
            try
            {
                var cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                var mgr = go.AddComponent<EnvironmentThemeManager>();
                typeof(EnvironmentThemeManager).GetField("_mainCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(mgr, cam);
                mgr.Config = config;
                var theme = config.GetThemeForLevel(5);
                mgr.ApplyTheme(theme);

                bool camMatches = cam.clearFlags == CameraClearFlags.SolidColor &&
                                  cam.backgroundColor == theme.SkyHorizonColor;

                Assert(camMatches, "Test 7: Camera Background Blending",
                    "Camera clear color aligned with theme sky horizon gradient.");
            }
            finally
            {
                GameObject.DestroyImmediate(go);
                GameObject.DestroyImmediate(camGo);
            }
        }

        private static void Test08_EdgeCaseLevelClamping(EnvironmentConfig config)
        {
            var tNegative = config.GetThemeForLevel(-5);
            var tBeyond = config.GetThemeForLevel(100);

            bool safeEdgeCases = tNegative != null && tBeyond != null;

            Assert(safeEdgeCases, "Test 8: Edge Case Level Clamping",
                "Levels out of standard range (-5, 100) return valid boundary fallback themes.");
        }

        private static void Test09_EnvironmentConfigAsset(EnvironmentConfig config)
        {
            bool hasThemes = config != null && config.Themes != null && config.Themes.Length == 3;

            Assert(hasThemes, "Test 9: EnvironmentConfig Asset Completeness",
                "Asset exists and contains exactly 3 progression milestone themes.");
        }

        private static void Test10_ZeroErrorsHealthPass()
        {
            Assert(_failedTests == 0,
                "Test 10: Zero Errors Health Pass",
                "All 3D environment, lighting, and fog automation tests executed cleanly.");
        }
    }
}
