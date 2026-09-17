using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Autonomous setup script for Phase 19: Level Content & Balancing.
    /// Builds all 30 LevelData assets, LevelDatabase, and configures LevelLoader in MainGame scene.
    /// </summary>
    public static class Phase19Setup
    {
        private const string STAND_PREFAB = "Assets/_Project/Prefabs/Gameplay/Stand.prefab";
        private const string BOX_PREFAB = "Assets/_Project/Prefabs/Gameplay/Box.prefab";
        private const string STONE_PREFAB = "Assets/_Project/Prefabs/Gameplay/Stone.prefab";
        private const string COLUMN_PREFAB = "Assets/_Project/Prefabs/Gameplay/Column.prefab";
        private const string CONE_PREFAB = "Assets/_Project/Prefabs/Gameplay/Cone.prefab";
        private const string BOTTLE_PREFAB = "Assets/_Project/Prefabs/Gameplay/PaintBottle.prefab";

        [MenuItem("RoyalSmash/Phase 19/Run Full Level Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[Phase19Setup] Starting Phase 19 Level Setup...");

            // 1. Generate all 30 levels and database
            var database = LevelGenerator30.GenerateAllLevels();

            // 2. Configure MainGame scene with LevelLoader
            ConfigureMainGameScene(database);

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase19Setup] Phase 19 Level Setup successfully completed!");
        }

        private static void ConfigureMainGameScene(LevelDatabase database)
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainGame.unity", OpenSceneMode.Single);

            var loader = Object.FindFirstObjectByType<LevelLoader>();
            if (loader == null)
            {
                var go = new GameObject("LevelLoader");
                loader = go.AddComponent<LevelLoader>();
                Debug.Log("[Phase19Setup] Created LevelLoader in MainGame.unity");
            }

            var stand = AssetDatabase.LoadAssetAtPath<GameObject>(STAND_PREFAB);
            var box = AssetDatabase.LoadAssetAtPath<GameObject>(BOX_PREFAB);
            var stone = AssetDatabase.LoadAssetAtPath<GameObject>(STONE_PREFAB);
            var col = AssetDatabase.LoadAssetAtPath<GameObject>(COLUMN_PREFAB);
            var cone = AssetDatabase.LoadAssetAtPath<GameObject>(CONE_PREFAB);
            var bottle = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTLE_PREFAB);

            loader.BindPrefabs(stand, box, stone, col, cone, bottle);
            EditorUtility.SetDirty(loader);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
