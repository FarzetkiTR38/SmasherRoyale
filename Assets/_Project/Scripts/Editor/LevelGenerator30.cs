using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Levels;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Procedural generator that creates all 30 balanced LevelData assets and the LevelDatabase.
    /// Follows Master Build Prompt Section 9, Section 10, and Phase 19.
    /// </summary>
    public static class LevelGenerator30
    {
        private const string LEVELS_DIR = "Assets/_Project/ScriptableObjects/Levels";
        private const string DATABASE_PATH = "Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset";

        [MenuItem("RoyalSmash/Phase 19/Generate All 30 Levels")]
        public static LevelDatabase GenerateAllLevels()
        {
            if (!Directory.Exists(LEVELS_DIR))
            {
                Directory.CreateDirectory(LEVELS_DIR);
            }

            var levelList = new List<LevelData>();

            for (int i = 1; i <= 30; i++)
            {
                var levelData = GenerateLevelData(i);
                levelList.Add(levelData);
            }

            // Create or update LevelDatabase
            var database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DATABASE_PATH);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<LevelDatabase>();
                AssetDatabase.CreateAsset(database, DATABASE_PATH);
            }

            database.SetLevels(levelList.ToArray());
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelGenerator30] Successfully generated and configured all 30 levels in {DATABASE_PATH}");
            return database;
        }

        private static LevelData GenerateLevelData(int levelId)
        {
            string path = $"{LEVELS_DIR}/LevelData_{levelId:D2}.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(level, path);
            }

            // Progression curves
            int ballCount = GetBallCount(levelId);
            int baseReward = GetBaseReward(levelId);
            var stands = GetStandsForLevel(levelId);
            var objects = GetObjectsForLevel(levelId);
            var events = GetEventsForLevel(levelId);

            level.SetData(levelId, $"Level {levelId}", ballCount, baseReward, stands, objects, events);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static int GetBallCount(int levelId)
        {
            if (levelId <= 3) return 25;
            if (levelId <= 6) return 22;
            if (levelId <= 11) return 20;
            if (levelId <= 16) return 18;
            if (levelId <= 20) return 16;
            if (levelId <= 23) return 15;
            if (levelId <= 26) return 14;
            return 12; // Levels 27..30 tight economy
        }

        private static int GetBaseReward(int levelId)
        {
            return 50 + ((levelId - 1) * 5); // 50 to 195 gold
        }

        private static StandPlacement[] GetStandsForLevel(int levelId)
        {
            if (levelId == 12 || levelId == 22)
            {
                // Dual stand layout
                return new StandPlacement[]
                {
                    new StandPlacement { Position = new Vector3(-2.2f, 1.0f, 5.0f), Scale = new Vector3(2.5f, 0.3f, 2.0f) },
                    new StandPlacement { Position = new Vector3(2.2f, 1.0f, 5.0f), Scale = new Vector3(2.5f, 0.3f, 2.0f) }
                };
            }

            return new StandPlacement[]
            {
                new StandPlacement { Position = new Vector3(0f, 1.0f, 5.0f), Scale = new Vector3(4.0f, 0.3f, 2.5f) }
            };
        }

        private static ObjectPlacement[] GetObjectsForLevel(int levelId)
        {
            var list = new List<ObjectPlacement>();
            float standTopY = 1.15f;

            if (levelId == 1)
            {
                // Simple 4 boxes
                list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.6f, standTopY + 0.35f, 5.0f) });
                list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.6f, standTopY + 0.35f, 5.0f) });
                list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, standTopY + 1.05f, 5.0f) });
                list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, standTopY + 1.75f, 5.0f) });
            }
            else if (levelId < 5)
            {
                // Levels 2-4: Boxes & Cones
                int count = 4 + levelId;
                for (int i = 0; i < count; i++)
                {
                    float x = -1.2f + (i % 4) * 0.8f;
                    float y = standTopY + 0.35f + (i / 4) * 0.7f;
                    var type = (i % 3 == 0) ? ObjectType.Cone : ObjectType.Box;
                    list.Add(new ObjectPlacement { Type = type, Position = new Vector3(x, y, 5.0f) });
                }
            }
            else if (levelId < 10)
            {
                // Levels 5-9: Stones & Columns introduced
                int count = 6 + (levelId - 5);
                for (int i = 0; i < count; i++)
                {
                    float x = -1.2f + (i % 3) * 1.0f;
                    float y = standTopY + 0.4f + (i / 3) * 0.8f;
                    ObjectType type = (i % 3 == 0) ? ObjectType.Stone : (i % 4 == 0 ? ObjectType.Column : ObjectType.Box);
                    list.Add(new ObjectPlacement { Type = type, Position = new Vector3(x, y, 5.0f) });
                }
            }
            else if (levelId < 15)
            {
                // Levels 10-14: Cones + PaintBottle hints
                int count = 9 + (levelId - 10);
                for (int i = 0; i < count; i++)
                {
                    float x = -1.4f + (i % 4) * 0.9f;
                    float y = standTopY + 0.4f + (i / 4) * 0.8f;
                    ObjectType type = (i == count - 1) ? ObjectType.PaintBottle :
                                      (i % 3 == 0) ? ObjectType.Column :
                                      (i % 2 == 0) ? ObjectType.Stone : ObjectType.Box;
                    list.Add(new ObjectPlacement { Type = type, Position = new Vector3(x, y, 5.0f) });
                }
            }
            else if (levelId < 25)
            {
                // Levels 15-24: PaintBottles + Dual Row / Fortress
                int count = 12 + (levelId - 15) / 2;
                for (int i = 0; i < count; i++)
                {
                    float z = (i % 2 == 0) ? 4.6f : 5.4f; // Two layers
                    float x = -1.3f + (i % 4) * 0.85f;
                    float y = standTopY + 0.4f + (i / 4) * 0.85f;
                    ObjectType type = (i % 5 == 0) ? ObjectType.PaintBottle :
                                      (i % 4 == 0) ? ObjectType.Cone :
                                      (i % 3 == 0) ? ObjectType.Column :
                                      (i % 2 == 0) ? ObjectType.Stone : ObjectType.Box;
                    list.Add(new ObjectPlacement { Type = type, Position = new Vector3(x, y, z) });
                }
            }
            else
            {
                // Levels 25-30: Epic Castle / Finale
                int count = 16 + (levelId - 25) * 2;
                for (int i = 0; i < count; i++)
                {
                    float z = (i % 2 == 0) ? 4.5f : 5.5f;
                    float x = -1.5f + (i % 5) * 0.75f;
                    float y = standTopY + 0.4f + (i / 5) * 0.85f;
                    ObjectType type = (i % 6 == 0) ? ObjectType.PaintBottle :
                                      (i % 5 == 0) ? ObjectType.Cone :
                                      (i % 4 == 0) ? ObjectType.Column :
                                      (i % 2 == 0) ? ObjectType.Stone : ObjectType.Box;
                    list.Add(new ObjectPlacement { Type = type, Position = new Vector3(x, y, z) });
                }
            }

            return list.ToArray();
        }

        private static LevelEvent[] GetEventsForLevel(int levelId)
        {
            var list = new List<LevelEvent>();

            if (levelId == 5)
            {
                list.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "MegaBall unlocked!" });
                list.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Stone Object introduced!" });
            }
            else if (levelId == 10)
            {
                list.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "InfinityBall unlocked!" });
                list.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Level 10 Username Prompt" });
                list.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Cone & PaintBottle introduced!" });
            }
            else if (levelId == 15 || levelId == 20 || levelId == 25)
            {
                list.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = $"Milestone Level {levelId} reached!" });
            }
            else if (levelId == 30)
            {
                list.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Level 30 Finale (No Next Level)" });
            }

            return list.ToArray();
        }
    }
}
