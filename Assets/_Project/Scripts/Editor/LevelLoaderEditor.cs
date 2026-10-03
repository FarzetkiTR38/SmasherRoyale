using UnityEditor;
using UnityEngine;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Custom Inspector for LevelLoader.
    /// Provides 1-click in-editor level previewing for all 30 Citadel levels directly in the SceneView
    /// without needing to enter Play Mode.
    /// </summary>
    [CustomEditor(typeof(LevelLoader))]
    public class LevelLoaderEditor : UnityEditor.Editor
    {
        private static int _previewLevelId = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var loader = (LevelLoader)target;

            EditorGUILayout.Space(14);
            EditorGUILayout.LabelField("In-Editor Level Previewer (30 Levels)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select any level from 1 to 30 below to view and inspect it live in the Scene view without entering Play Mode.", MessageType.Info);

            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
            if (db == null)
            {
                db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
            }

            if (db == null)
            {
                EditorGUILayout.HelpBox("LevelDatabase asset not found in project!", MessageType.Warning);
                return;
            }

            _previewLevelId = EditorGUILayout.IntSlider("Level To Preview", _previewLevelId, 1, 30);

            var data = db.GetLevel(_previewLevelId);
            if (data != null)
            {
                EditorGUILayout.LabelField($"Level {_previewLevelId}: \"{data.LevelName}\" | {data.Objects?.Length ?? 0} Blocks | {data.Stands?.Length ?? 0} Stands | {data.BallCount} Balls", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button($"▶ Preview Level {_previewLevelId} in Scene", GUILayout.Height(32)))
            {
                if (data != null)
                {
                    loader.LoadLevel(data);
                    SceneView.RepaintAll();
                }
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("✕ Clear Preview", GUILayout.Height(32), GUILayout.Width(110)))
            {
                loader.ClearLevel();
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Open Full Level Designer Window (Palette & Editing)", GUILayout.Height(26)))
            {
                LevelDesignerWindow.OpenWindow();
            }
        }
    }
}
