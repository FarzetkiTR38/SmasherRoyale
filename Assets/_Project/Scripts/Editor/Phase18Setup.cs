using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Haptics;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Autonomous setup script for Phase 18: Haptic Feedback.
    /// Wires HapticManager into MainMenu and MainGame scenes.
    /// </summary>
    public static class Phase18Setup
    {
        [MenuItem("RoyalSmash/Phase 18/Run Full Haptic Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[Phase18Setup] Starting Phase 18 Haptic Setup...");

            ConfigureScene("Assets/_Project/Scenes/MainMenu.unity");
            ConfigureScene("Assets/_Project/Scenes/MainGame.unity");

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase18Setup] Phase 18 Haptic Setup successfully completed!");
        }

        private static void ConfigureScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var mgr = Object.FindFirstObjectByType<HapticManager>();
            if (mgr == null)
            {
                var go = new GameObject("HapticManager");
                mgr = go.AddComponent<HapticManager>();
                Debug.Log($"[Phase18Setup] Created HapticManager in {scenePath}");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
