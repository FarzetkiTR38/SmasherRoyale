using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using RoyalSmash.Audio;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Autonomous setup script for Phase 16: Audio & SFX System.
    /// Synthesizes Section 21 audio clips, generates AudioConfig.asset, and configures scenes.
    /// </summary>
    public static class Phase16Setup
    {
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/AudioConfig.asset";
        private const string SFX_DIR = "Assets/_Project/Audio/SFX/";
        private const string MUSIC_DIR = "Assets/_Project/Audio/Music/";

        [MenuItem("RoyalSmash/Phase 16/Run Full Audio Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[Phase16Setup] Starting Phase 16 Audio Setup...");

            // 1. Generate all procedural clips
            ProceduralAudioGenerator.GenerateAllClips();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 2. Setup AudioConfig ScriptableObject
            var config = SetupAudioConfig();

            // 3. Configure MainMenu scene
            ConfigureScene("Assets/_Project/Scenes/MainMenu.unity", config);

            // 4. Configure MainGame scene
            ConfigureScene("Assets/_Project/Scenes/MainGame.unity", config);

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase16Setup] Phase 16 Audio Setup successfully completed!");
        }

        public static AudioConfig SetupAudioConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AudioConfig>();
                string dir = Path.GetDirectoryName(CONFIG_PATH);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            SerializedObject so = new SerializedObject(config);

            AssignClip(so, "_ballLaunch", SFX_DIR + "ball_launch.wav");
            AssignClip(so, "_boxImpact", SFX_DIR + "box_impact.wav");
            AssignClip(so, "_stoneImpact", SFX_DIR + "stone_impact.wav");
            AssignClip(so, "_columnImpact", SFX_DIR + "column_impact.wav");
            AssignClip(so, "_coneImpact", SFX_DIR + "cone_impact.wav");
            AssignClip(so, "_paintBottleBreak", SFX_DIR + "bottle_break.wav");
            AssignClip(so, "_objectDestruction", SFX_DIR + "object_destruction.wav");
            AssignClip(so, "_buttonClick", SFX_DIR + "ui_click.wav");
            AssignClip(so, "_winFanfare", SFX_DIR + "win_fanfare.wav");
            AssignClip(so, "_loseSound", SFX_DIR + "lose_sound.wav");
            AssignClip(so, "_rewardSound", SFX_DIR + "reward_coin.wav");

            AssignClip(so, "_bgmMainMenu", MUSIC_DIR + "bgm_mainmenu.wav");
            AssignClip(so, "_bgmGameplay", MUSIC_DIR + "bgm_gameplay.wav");

            so.FindProperty("_maxSimultaneousVoices").intValue = 8;
            so.FindProperty("_pitchRandomizationRange").floatValue = 0.06f;
            so.FindProperty("_minCollisionImpulse").floatValue = 0.15f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Phase16Setup] AudioConfig populated and saved at {CONFIG_PATH}");
            return config;
        }

        private static void AssignClip(SerializedObject so, string propertyName, string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = clip;
                if (clip == null)
                {
                    Debug.LogWarning($"[Phase16Setup] Could not find AudioClip at {assetPath}");
                }
            }
        }

        private static void ConfigureScene(string scenePath, AudioConfig config)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find or create AudioManager
            var audioMgr = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            if (audioMgr == null)
            {
                var go = new GameObject("AudioManager");
                audioMgr = go.AddComponent<AudioManager>();
                Debug.Log($"[Phase16Setup] Created AudioManager in {scenePath}");
            }

            SerializedObject soMgr = new SerializedObject(audioMgr);
            soMgr.FindProperty("_config").objectReferenceValue = config;
            soMgr.ApplyModifiedProperties();
            EditorUtility.SetDirty(audioMgr);

            // Attach UIButtonSound to all UI Buttons in scene
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int buttonsHooked = 0;
            foreach (var btn in buttons)
            {
                if (btn.GetComponent<UIButtonSound>() == null)
                {
                    btn.gameObject.AddComponent<UIButtonSound>();
                    EditorUtility.SetDirty(btn.gameObject);
                    buttonsHooked++;
                }
            }

            Debug.Log($"[Phase16Setup] Hooked UIButtonSound to {buttonsHooked} buttons in {scenePath}");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
