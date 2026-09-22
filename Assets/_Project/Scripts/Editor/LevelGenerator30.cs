using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Levels;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Procedural level generator creating all 30 high-fidelity LevelData assets.
    /// Perfectly calibrated for the Royal Arena coordinate space matching PhysicsTestScene:
    /// - Royal Stand Tabletop at Y = 1.45f, Z = 7.50f
    /// - Tabletop Width: 5.5 units (X: -2.2f to +2.2f)
    /// - Front Row Z: 6.80f, Main/Back Row Z: 7.45f
    /// - Balanced progressive difficulty from Level 1 to Level 30.
    /// </summary>
    public static class LevelGenerator30
    {
        private const string LEVELS_DIR = "Assets/_Project/ScriptableObjects/Levels";
        private const string DATABASE_PATH = "Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset";

        private static readonly Color[] BottleColors = new Color[]
        {
            new Color(0.92f, 0.12f, 0.16f), // Red
            new Color(0.10f, 0.55f, 0.95f), // Blue
            new Color(1.00f, 0.82f, 0.08f), // Yellow
            new Color(0.96f, 0.20f, 0.58f), // Pink
            new Color(0.55f, 0.16f, 0.85f), // Purple
            new Color(1.00f, 0.50f, 0.05f)  // Orange
        };

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

            Debug.Log($"[LevelGenerator30] Successfully generated and configured all 30 calibrated levels in {DATABASE_PATH}");
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
            if (levelId <= 25) return 15;
            return 14; // Levels 26..30
        }

        private static int GetBaseReward(int levelId)
        {
            return 50 + ((levelId - 1) * 5); // 50 to 195 gold
        }

        private static StandPlacement[] GetStandsForLevel(int levelId)
        {
            // Consistent standard Royal Stand in main arena
            return new StandPlacement[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, 7.50f),
                    Scale = Vector3.one,
                    Rotation = Vector3.zero
                }
            };
        }

        private static ObjectPlacement[] GetObjectsForLevel(int levelId)
        {
            var list = new List<ObjectPlacement>();
            const float tableY = 1.45f;
            const float mainZ = 7.45f;
            const float frontZ = 6.80f;

            switch (levelId)
            {
                // ==========================================
                // TIER 1: LEVELS 1-3 (TUTORIAL & INTRO)
                // ==========================================
                case 1:
                    // Easy, satisfying 4-box pyramid
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.65f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.65f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 2.35f, mainZ) });
                    break;

                case 2:
                    // Twin guard towers with center low block
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.30f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.30f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(-1.30f, tableY + 2.35f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 0.50f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.30f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.30f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(1.30f, tableY + 2.35f, mainZ) });
                    break;

                case 3:
                    // Castle Bridge Arch
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.50f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.50f, tableY + 1.45f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.55f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.55f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(-0.55f, tableY + 2.35f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0.55f, tableY + 2.35f, mainZ) });
                    break;

                // ==========================================
                // TIER 2: LEVELS 4-6 (STONE BLOCKS INTRODUCED)
                // ==========================================
                case 4:
                    // Heavy Stone core surrounded by wood
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.20f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.20f, tableY + 0.50f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.60f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.60f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0f, tableY + 2.40f, mainZ) });
                    break;

                case 5:
                    // Milestone: Mega Ball Unlock - Big Sturdy Pyramid
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.40f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.40f, tableY + 0.50f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.95f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.95f, tableY + 1.45f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.50f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.50f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 3.30f, mainZ) });
                    break;

                case 6:
                    // Stone Vault (Heavy outer pillars, weak center)
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.50f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.50f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.50f, tableY + 1.45f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.75f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.75f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 3.30f, mainZ) });
                    break;

                // ==========================================
                // TIER 3: LEVELS 7-9 (PAINT BOTTLES INTRODUCED!)
                // ==========================================
                case 7:
                    // Paint Bottle Debut: Red & Blue explosive bottles
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(-1.20f, tableY + 0.45f, frontZ), PaintColor = BottleColors[0] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(1.20f, tableY + 0.45f, frontZ), PaintColor = BottleColors[1] });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.20f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.20f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.20f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.20f, tableY + 1.45f, mainZ) });
                    break;

                case 8:
                    // Color Cascade: 3 Bottles (Yellow, Pink, Blue)
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(-1.50f, tableY + 0.45f, frontZ), PaintColor = BottleColors[2] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(0f, tableY + 0.45f, frontZ), PaintColor = BottleColors[3] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(1.50f, tableY + 0.45f, frontZ), PaintColor = BottleColors[1] });

                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.10f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.10f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.10f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.10f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 2.40f, mainZ) });
                    break;

                case 9:
                    // Stone Fortress with Purple & Orange bottle weak points
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(-0.70f, tableY + 0.45f, frontZ), PaintColor = BottleColors[4] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(0.70f, tableY + 0.45f, frontZ), PaintColor = BottleColors[5] });

                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.60f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.70f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.70f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.60f, tableY + 0.50f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.15f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.15f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(-0.60f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0.60f, tableY + 2.40f, mainZ) });
                    break;

                // ==========================================
                // TIER 4: LEVEL 10 (INFINITY BALL UNLOCK!)
                // ==========================================
                case 10:
                    // Massive target board designed for Infinity Ball rapid-fire clearing!
                    // Front bottle line
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(-1.60f, tableY + 0.45f, frontZ), PaintColor = BottleColors[0] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(-0.80f, tableY + 0.45f, frontZ), PaintColor = BottleColors[2] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(0f, tableY + 0.45f, frontZ), PaintColor = BottleColors[1] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(0.80f, tableY + 0.45f, frontZ), PaintColor = BottleColors[3] });
                    list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(1.60f, tableY + 0.45f, frontZ), PaintColor = BottleColors[4] });

                    // Main wall
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-1.80f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-0.90f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(0.90f, tableY + 0.50f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(1.80f, tableY + 0.50f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(-1.35f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(-0.45f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0.45f, tableY + 1.45f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(1.35f, tableY + 1.45f, mainZ) });

                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(-0.90f, tableY + 2.40f, mainZ) });
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0.90f, tableY + 2.40f, mainZ) });
                    break;

                // ==========================================
                // TIER 5: LEVELS 11-20 (COLUMNS, ARCHES & BALANCED PUZZLES)
                // ==========================================
                default:
                    // Procedurally generated high-quality configurations for Levels 11..30
                    GenerateAdvancedLevel(levelId, list, tableY, mainZ, frontZ);
                    break;
            }

            return list.ToArray();
        }

        private static void GenerateAdvancedLevel(int levelId, List<ObjectPlacement> list, float tableY, float mainZ, float frontZ)
        {
            int seed = levelId * 1337;
            var rand = new System.Random(seed);

            int baseBlocks = Mathf.Min(18, 9 + (levelId / 2));
            bool hasColumns = levelId >= 11;
            bool hasBottles = levelId >= 7;

            // 1. Foundation Layer (3 to 5 pillars/blocks across stand width)
            int columnsCount = 3 + (levelId % 3); // 3, 4, or 5 columns
            float xSpan = 3.6f;
            float step = xSpan / (columnsCount - 1);

            for (int i = 0; i < columnsCount; i++)
            {
                float x = -xSpan * 0.5f + (i * step);
                ObjectType baseType = (i == 0 || i == columnsCount - 1) ? ObjectType.Stone : ObjectType.Box;

                if (hasColumns && i % 2 == 1 && levelId >= 13)
                {
                    // Tall column pillar (center height = tableY + 1.2f)
                    list.Add(new ObjectPlacement { Type = ObjectType.Column, Position = new Vector3(x, tableY + 1.20f, mainZ) });
                }
                else
                {
                    list.Add(new ObjectPlacement { Type = baseType, Position = new Vector3(x, tableY + 0.50f, mainZ) });
                    // Layer 2
                    ObjectType layer2Type = (i % 2 == 0) ? ObjectType.Box : ObjectType.Stone;
                    list.Add(new ObjectPlacement { Type = layer2Type, Position = new Vector3(x, tableY + 1.45f, mainZ) });
                }
            }

            // 2. Crossbeams & Arches (Layer 3)
            int bridgeCount = columnsCount - 1;
            for (int i = 0; i < bridgeCount; i++)
            {
                float x = -xSpan * 0.5f + (i * step) + (step * 0.5f);
                list.Add(new ObjectPlacement { Type = ObjectType.Box, Position = new Vector3(x, tableY + 2.40f, mainZ) });

                // Spire cones
                if (i % 2 == 0)
                {
                    list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(x, tableY + 3.30f, mainZ) });
                }
            }

            // 3. Front Row: Explosive Paint Bottles (1 to 4 bottles based on level)
            if (hasBottles)
            {
                int bottleCount = Mathf.Clamp(1 + (levelId / 6), 1, 5);
                float bSpan = 3.0f;
                float bStep = bottleCount > 1 ? bSpan / (bottleCount - 1) : 0f;

                for (int b = 0; b < bottleCount; b++)
                {
                    float bx = (bottleCount == 1) ? 0f : -bSpan * 0.5f + (b * bStep);
                    Color col = BottleColors[(b + levelId) % BottleColors.Length];
                    list.Add(new ObjectPlacement
                    {
                        Type = ObjectType.PaintBottle,
                        Position = new Vector3(bx, tableY + 0.45f, frontZ),
                        PaintColor = col
                    });
                }
            }

            // Level 30 Finale Spire bonus
            if (levelId == 30)
            {
                // Grand King Citadel pinnacle
                list.Add(new ObjectPlacement { Type = ObjectType.Stone, Position = new Vector3(0f, tableY + 3.35f, mainZ) });
                list.Add(new ObjectPlacement { Type = ObjectType.Cone, Position = new Vector3(0f, tableY + 4.25f, mainZ) });
                list.Add(new ObjectPlacement { Type = ObjectType.PaintBottle, Position = new Vector3(0f, tableY + 1.45f, frontZ), PaintColor = BottleColors[0] });
            }
        }

        private static LevelEvent[] GetEventsForLevel(int levelId)
        {
            var list = new List<LevelEvent>();

            if (levelId == 5)
            {
                list.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "MegaBall unlocked!" });
                list.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Stone Object introduced!" });
            }
            else if (levelId == 7)
            {
                list.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Paint Bottle introduced!" });
            }
            else if (levelId == 10)
            {
                list.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "InfinityBall unlocked!" });
                list.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Level 10 Username Prompt" });
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
