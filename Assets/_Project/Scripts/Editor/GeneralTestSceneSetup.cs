using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Profile;
using RoyalSmash.Economy;
using RoyalSmash.Lives;
using RoyalSmash.Skills;
using RoyalSmash.Audio;
using RoyalSmash.Haptics;
using RoyalSmash.VFX;
using RoyalSmash.Cloud;
using RoyalSmash.Ads;
using RoyalSmash.Core;
using RoyalSmash.Debugging;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automation tool to build and maintain GeneralTestScene.unity.
    /// Copies MainGame.unity, embeds all 14 persistent global managers and the AdminManager,
    /// enabling 100% standalone play-mode testing without needing to go through BootScene -> MainMenu.
    /// </summary>
    public static class GeneralTestSceneSetup
    {
        public const string MAIN_GAME_SCENE_PATH = "Assets/_Project/Scenes/MainGame.unity";
        public const string GENERAL_TEST_SCENE_PATH = "Assets/_Project/Scenes/GeneralTestScene.unity";

        [MenuItem("RoyalSmash/Setup/Create or Update GeneralTestScene")]
        public static void CreateOrUpdateGeneralTestScene()
        {
            Debug.Log("[GeneralTestSceneSetup] Generating standalone GeneralTestScene...");

            // 1. Copy MainGame.unity to GeneralTestScene.unity
            if (!File.Exists(MAIN_GAME_SCENE_PATH))
            {
                Debug.LogError($"[GeneralTestSceneSetup] Source scene not found at '{MAIN_GAME_SCENE_PATH}'!");
                return;
            }

            AssetDatabase.CopyAsset(MAIN_GAME_SCENE_PATH, GENERAL_TEST_SCENE_PATH);
            AssetDatabase.Refresh();

            // 2. Open GeneralTestScene
            var scene = EditorSceneManager.OpenScene(GENERAL_TEST_SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[GeneralTestSceneSetup] Failed to open '{GENERAL_TEST_SCENE_PATH}'!");
                return;
            }

            // 3. Create or find [PERSISTENT_MANAGERS] root GameObject
            var bootstrapRoot = GameObject.Find("[PERSISTENT_MANAGERS]");
            if (bootstrapRoot == null)
            {
                bootstrapRoot = new GameObject("[PERSISTENT_MANAGERS]");
            }

            // 4. Ensure all 14 global persistent managers exist
            EnsureManager<SaveManager>("SaveManager", bootstrapRoot.transform);
            EnsureManager<ProfileManager>("ProfileManager", bootstrapRoot.transform);
            EnsureManager<GoldManager>("GoldManager", bootstrapRoot.transform);
            EnsureManager<LivesManager>("LivesManager", bootstrapRoot.transform);
            EnsureManager<SkillManager>("SkillManager", bootstrapRoot.transform);
            EnsureManager<AudioManager>("AudioManager", bootstrapRoot.transform);
            EnsureManager<HapticManager>("HapticManager", bootstrapRoot.transform);
            EnsureManager<VFXManager>("VFXManager", bootstrapRoot.transform);
            EnsureManager<CloudSyncManager>("CloudSyncManager", bootstrapRoot.transform);
            EnsureManager<LeaderboardManager>("LeaderboardManager", bootstrapRoot.transform);
            EnsureManager<AdManager>("AdManager", bootstrapRoot.transform);
            EnsureManager<IAPManager>("IAPManager", bootstrapRoot.transform);
            EnsureManager<SceneTransitionManager>("SceneTransitionManager", bootstrapRoot.transform);
            EnsureManager<RoyalSmash.Utils.VibrationManager>("VibrationManager", bootstrapRoot.transform);

            // 5. Create or configure AdminManager
            var adminGo = GameObject.Find("AdminManager");
            if (adminGo == null)
            {
                adminGo = new GameObject("AdminManager");
            }
            var adminComp = adminGo.GetComponent<AdminManager>();
            if (adminComp == null)
            {
                adminComp = adminGo.AddComponent<AdminManager>();
            }
            adminComp.targetLevel = 1;
            adminComp.showInGameGUI = true;

            // 6. Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[GeneralTestSceneSetup] Successfully created and saved '{GENERAL_TEST_SCENE_PATH}'.");

            // 7. Register in Build Settings
            RegisterInBuildSettings(GENERAL_TEST_SCENE_PATH);
        }

        private static void EnsureManager<T>(string name, Transform parent) where T : MonoBehaviour
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (existing == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent);
                go.AddComponent<T>();
            }
        }

        private static void RegisterInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    found = true;
                    s.enabled = true;
                    break;
                }
            }

            if (!found)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[GeneralTestSceneSetup] Registered '{scenePath}' in EditorBuildSettings.");
            }
        }
    }
}
