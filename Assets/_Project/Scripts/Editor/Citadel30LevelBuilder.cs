using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Comprehensive generator for all 30 Royal Smash Citadel levels.
    /// Overhauled in Phase 33 to provide rich, creative, high-density architectural layouts (14-28 blocks per level)
    /// across varied stand configurations (Single, Wide, Dual Platforms, and Tiered Stands).
    /// </summary>
    public static class Citadel30LevelBuilder
    {
        private const float BASE_Y = 1.845f;
        private const float LAYER_STEP_Y = 0.746f;
        private const float STEP_X = 0.755f;
        private const float STAND_Z = 7.50f;
        private static readonly Vector3 BLOCK_SCALE = new Vector3(0.74f, 0.74f, 0.74f);

        [MenuItem("Tools/Royal Smash/Generate All 30 Citadel Levels")]
        [MenuItem("RoyalSmash/Levels/Generate All 30 Citadel Levels")]
        public static void GenerateAllLevels()
        {
            const string levelsFolder = "Assets/_Project/Art/SkyCitadel/Levels";
            var levelList = new List<LevelData>();

            for (int levelId = 1; levelId <= 30; levelId++)
            {
                string assetPath = $"{levelsFolder}/LevelData_Citadel{levelId:D2}.asset";
                LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                if (levelData == null)
                {
                    levelData = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(levelData, assetPath);
                }

                BuildLevelData(levelId, levelData);
                EditorUtility.SetDirty(levelData);
                levelList.Add(levelData);
            }

            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
            if (db != null)
            {
                db.SetLevels(levelList.ToArray());
                EditorUtility.SetDirty(db);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Citadel30LevelBuilder] Successfully generated and calibrated all 30 Citadel levels with rich block layouts and multi-stand setups!");
        }

        private static float GetLayerY(int layer) => BASE_Y + (layer * LAYER_STEP_Y);

        private static ObjectPlacement CreateBlock(ObjectType type, float x, int layer, float z = STAND_Z, bool isRequired = true)
        {
            return new ObjectPlacement
            {
                Type = type,
                Position = new Vector3(x, GetLayerY(layer), z),
                Rotation = Vector3.zero,
                Scale = BLOCK_SCALE,
                IsRequiredTarget = isRequired,
                OverrideMass = false,
                CustomMass = 1.0f,
                PaintColor = Color.clear
            };
        }

        private static ObjectPlacement CreateCustomBlock(ObjectType type, Vector3 pos, bool isRequired = true)
        {
            return new ObjectPlacement
            {
                Type = type,
                Position = pos,
                Rotation = Vector3.zero,
                Scale = BLOCK_SCALE,
                IsRequiredTarget = isRequired,
                OverrideMass = false,
                CustomMass = 1.0f,
                PaintColor = Color.clear
            };
        }

        private static StandPlacement[] SingleStand(float scaleX = 1.0f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.0f),
                    Rotation = Vector3.zero
                }
            };
        }

        private static StandPlacement[] DualStands(float xOffset = 2.2f, float scaleX = 0.75f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(-xOffset, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.0f),
                    Rotation = Vector3.zero
                },
                new StandPlacement
                {
                    Position = new Vector3(xOffset, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.0f),
                    Rotation = Vector3.zero
                }
            };
        }

        private static StandPlacement[] TieredStands(float frontZ = 6.8f, float rearZ = 8.2f, float rearY = 0.9f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, frontZ),
                    Scale = new Vector3(1.0f, 0.85f, 1.0f),
                    Rotation = Vector3.zero
                },
                new StandPlacement
                {
                    Position = new Vector3(0f, rearY, rearZ),
                    Scale = new Vector3(1.0f, 1.0f, 1.0f),
                    Rotation = Vector3.zero
                }
            };
        }

        private static void BuildLevelData(int id, LevelData data)
        {
            var stands = SingleStand(1.0f);
            var objs = new List<ObjectPlacement>();
            var events = new List<LevelEvent>();
            int balls = 25;
            int reward = 50 + (id - 1) * 10;
            string name = $"Level {id}";

            switch (id)
            {
                // =========================================================================
                // TIER 1: LEVELS 1-5 (Citadel Gateways & Foundations, 14-18 blocks)
                // =========================================================================
                case 1:
                    name = "The Grand Gateway";
                    balls = 25;
                    stands = SingleStand(1.0f);
                    // Layer 0: 4 foundation blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0));
                    // Layer 1: 4 blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 1));
                    // Layer 2: 3 blocks (Keystone bridge)
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 2));
                    // Layer 3: 3 blocks (Crown)
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 3));
                    break;

                case 2:
                    name = "Dual Watchtowers";
                    balls = 25;
                    stands = SingleStand(1.3f);
                    // Left Tower: 3 layers of 2 blocks (6)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.0f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box,   -1.0f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.0f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -1.0f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -2.0f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -1.0f * STEP_X, 2));
                    // Right Tower: 3 layers of 2 blocks (6)
                    objs.Add(CreateBlock(ObjectType.Box,    1.0f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone,  2.0f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Column,  1.0f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Stone,  2.0f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  1.0f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column,  2.0f * STEP_X, 2));
                    // Central Connecting Bridge: (4 blocks)
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 3));
                    break;

                case 3:
                    name = "Stone Fortress Wall";
                    balls = 25;
                    stands = SingleStand(1.2f);
                    // Layer 0: 5 base blocks
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 2 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 5 wall blocks
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 4 rampart blocks
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 2));
                    // Layer 3: 2 pinnacle blocks
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0.5f * STEP_X, 3));
                    break;

                case 4:
                    name = "The Royal Step Pyramid";
                    balls = 26;
                    stands = SingleStand(1.25f);
                    // Layer 0: 6 base blocks
                    for (int c = 0; c < 6; c++)
                    {
                        float x = (-2.5f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Box, x, 0));
                    }
                    // Layer 1: 5 blocks
                    for (int c = 0; c < 5; c++)
                    {
                        float x = (-2f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 0 || c == 4 ? ObjectType.Stone : ObjectType.Box, x, 1));
                    }
                    // Layer 2: 4 blocks
                    for (int c = 0; c < 4; c++)
                    {
                        float x = (-1.5f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 1 || c == 2 ? ObjectType.PaintBottle : ObjectType.Column, x, 2));
                    }
                    // Layer 3: 2 crown blocks
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 3));
                    // Layer 4: 1 pinnacle
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    break;

                case 5:
                    name = "The Gatekeeper Bastion";
                    balls = 26;
                    stands = SingleStand(1.35f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "MegaBall Unlocked!" });
                    // Layer 0: 6 base blocks
                    for (int c = 0; c < 6; c++)
                    {
                        float x = (-2.5f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Stone, x, 0));
                    }
                    // Layer 1: 5 blocks
                    for (int c = 0; c < 5; c++)
                    {
                        float x = (-2f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Box, x, 1));
                    }
                    // Layer 2: 4 blocks
                    for (int c = 0; c < 4; c++)
                    {
                        float x = (-1.5f + c) * STEP_X;
                        objs.Add(CreateBlock(c == 0 || c == 3 ? ObjectType.Cone : ObjectType.Column, x, 2));
                    }
                    // Layer 3: 2 tower heads
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 3));
                    // Layer 4: 1 crown core
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    break;

                // =========================================================================
                // TIER 2: LEVELS 6-10 (Twin Bastions & Multi-Platforms, 18-22 blocks)
                // =========================================================================
                case 6:
                    name = "Twin Sky Platforms";
                    balls = 26;
                    stands = DualStands(2.2f, 0.75f);
                    // Left Platform (9 blocks)
                    for (int layer = 0; layer < 3; layer++)
                    {
                        objs.Add(CreateBlock(ObjectType.Stone, -2.2f - 0.5f * STEP_X, layer));
                        objs.Add(CreateBlock(ObjectType.Box,   -2.2f + 0.5f * STEP_X, layer));
                    }
                    objs.Add(CreateBlock(ObjectType.Cone,   -2.2f - 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column, -2.2f + 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.2f, 4));

                    // Right Platform (9 blocks)
                    for (int layer = 0; layer < 3; layer++)
                    {
                        objs.Add(CreateBlock(ObjectType.Box,   2.2f - 0.5f * STEP_X, layer));
                        objs.Add(CreateBlock(ObjectType.Stone, 2.2f + 0.5f * STEP_X, layer));
                    }
                    objs.Add(CreateBlock(ObjectType.Column, 2.2f - 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Cone,   2.2f + 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.2f, 4));
                    break;

                case 7:
                    name = "Sapphire Spire Citadel";
                    balls = 26;
                    stands = SingleStand(1.3f);
                    // Layer 0: 6 blocks
                    for (int c = 0; c < 6; c++)
                        objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 5 blocks
                    for (int c = 0; c < 5; c++)
                        objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Column, (-2f + c) * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = 0; c < 5; c++)
                        objs.Add(CreateBlock(ObjectType.Box, (-2f + c) * STEP_X, 2));
                    // Layer 3: 3 crystal spires
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 3));
                    // Layer 4: 1 pinnacle
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 4));
                    break;

                case 8:
                    name = "The Armored Bunker";
                    balls = 26;
                    stands = SingleStand(1.25f);
                    // Heavy armored shell surrounding interior cores
                    // Layer 0: 5 blocks (Cone, Stone, Stone, Stone, Cone)
                    objs.Add(CreateBlock(ObjectType.Cone, -2 * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, 2 * STEP_X, 0));
                    // Layer 1: 5 blocks (Cone, Box, PaintBottle, Box, Cone)
                    objs.Add(CreateBlock(ObjectType.Cone, -2 * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Cone, 2 * STEP_X, 1));
                    // Layer 2: 5 blocks (Cone, Box, PaintBottle, Box, Cone)
                    objs.Add(CreateBlock(ObjectType.Cone, -2 * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Cone, 2 * STEP_X, 2));
                    // Layer 3: 3 roof blocks
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 3));
                    // Layer 4: 2 turret crowns
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  0.5f * STEP_X, 4));
                    break;

                case 9:
                    name = "Citadel Aqueduct";
                    balls = 26;
                    stands = SingleStand(1.4f);
                    // Layer 0: 4 pillars
                    objs.Add(CreateBlock(ObjectType.Stone, -2.25f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.75f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.75f * STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone,  2.25f * STEP_X, 0));
                    // Layer 1: 4 upper pillars
                    objs.Add(CreateBlock(ObjectType.Stone, -2.25f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.75f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box,    0.75f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Stone,  2.25f * STEP_X, 1));
                    // Layer 2: Spanning bridge across all 6 columns
                    for (int c = 0; c < 6; c++)
                        objs.Add(CreateBlock(c == 2 || c == 3 ? ObjectType.PaintBottle : ObjectType.Box, (-2.5f + c) * STEP_X, 2));
                    // Layer 3: Waterway canal (Crystal columns)
                    for (int c = 0; c < 5; c++)
                        objs.Add(CreateBlock(ObjectType.Column, (-2f + c) * STEP_X, 3));
                    // Layer 4: 1 center pavilion
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4));
                    break;

                case 10:
                    name = "The Royal Citadel Gate";
                    balls = 28;
                    stands = DualStands(2.2f, 0.75f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "InfinityBall Unlocked!" });
                    events.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Enter Player Profile" });
                    // Left Gatehouse (10 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Stone, -2.2f - 0.5f * STEP_X, l));
                        objs.Add(CreateBlock(ObjectType.Box,   -2.2f + 0.5f * STEP_X, l));
                    }
                    objs.Add(CreateBlock(ObjectType.Cone,   -2.2f - 0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.2f + 0.5f * STEP_X, 4));

                    // Right Gatehouse (10 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Box,   2.2f - 0.5f * STEP_X, l));
                        objs.Add(CreateBlock(ObjectType.Stone, 2.2f + 0.5f * STEP_X, l));
                    }
                    objs.Add(CreateBlock(ObjectType.Cone,   2.2f + 0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.2f - 0.5f * STEP_X, 4));

                    // Spanning Arch Connectors (2 blocks)
                    objs.Add(CreateBlock(ObjectType.Column, -1.0f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  1.0f * STEP_X, 3));
                    break;

                // =========================================================================
                // TIER 3: LEVELS 11-15 (Arcane Vaults & Flying Bastions, 20-24 blocks)
                // =========================================================================
                case 11:
                    name = "The Arcane Vault";
                    balls = 27;
                    stands = SingleStand(1.3f);
                    // Layer 0: 6 foundation
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 blocks (Cone outer, 4 cores in vault chamber)
                    objs.Add(CreateBlock(ObjectType.Cone, -2.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -1.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  1.5f * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Cone,  2.5f * STEP_X, 1));
                    // Layer 2: 5 blocks
                    objs.Add(CreateBlock(ObjectType.Cone, -2f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -1f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 1f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Cone, 2f * STEP_X, 2));
                    // Layer 3: 3 roof blocks
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 3));
                    break;

                case 12:
                    name = "Triple Spire Keep";
                    balls = 27;
                    stands = SingleStand(1.35f);
                    // 3 Spires: Left (-1.8x), Center (0), Right (1.8x)
                    // Layer 0: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Box, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Stone : ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    // Layer 2: 4 spire pillars
                    objs.Add(CreateBlock(ObjectType.Column, -2f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.5f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.5f * STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column,  2f * STEP_X, 2));
                    // Layer 3: 3 spire tops
                    objs.Add(CreateBlock(ObjectType.Column, -2f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column, 2f * STEP_X, 3));
                    // Layer 4: 2 pinnacles
                    objs.Add(CreateBlock(ObjectType.Cone, -2f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  2f * STEP_X, 4));
                    break;

                case 13:
                    name = "Floating Tiered Bastions";
                    balls = 28;
                    stands = TieredStands(6.8f, 8.2f, 0.9f);
                    // Front Outpost at Z = 6.8f (10 blocks)
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, 6.8f));
                    for (int c = -1; c <= 1; c++) objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 1, 6.8f));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, 6.8f));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 3, 6.8f));

                    // Rear Elevated Citadel at Z = 8.2f, Y = 0.9m offset (12 blocks)
                    float rearBaseY = BASE_Y + 0.9f;
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateCustomBlock(ObjectType.Cone, new Vector3(c * STEP_X, rearBaseY, 8.2f)));
                    for (int c = -1; c <= 1; c++)
                        objs.Add(CreateCustomBlock(ObjectType.Column, new Vector3(c * STEP_X, rearBaseY + LAYER_STEP_Y, 8.2f)));
                    objs.Add(CreateCustomBlock(ObjectType.Box, new Vector3(-0.5f * STEP_X, rearBaseY + 2 * LAYER_STEP_Y, 8.2f)));
                    objs.Add(CreateCustomBlock(ObjectType.PaintBottle, new Vector3(0.5f * STEP_X, rearBaseY + 2 * LAYER_STEP_Y, 8.2f)));
                    objs.Add(CreateCustomBlock(ObjectType.Column, new Vector3(0f, rearBaseY + 3 * LAYER_STEP_Y, 8.2f)));
                    objs.Add(CreateCustomBlock(ObjectType.PaintBottle, new Vector3(0f, rearBaseY + 4 * LAYER_STEP_Y, 8.2f)));
                    break;

                case 14:
                    name = "Ironclad Redoubt";
                    balls = 28;
                    stands = SingleStand(1.3f);
                    // Layer 0: 6 foundation blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = 0; c < 5; c++) objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Column, (-2f + c) * STEP_X, 2));
                    // Layer 3: 3 blocks
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3));
                    // Layer 4: 2 pinnacle
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 4));
                    break;

                case 15:
                    name = "Sanctuary of Sapphire";
                    balls = 28;
                    stands = SingleStand(1.4f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sapphire Sanctuary Milestone" });
                    // Layer 0: 7 foundation blocks
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) >= 2 ? ObjectType.Stone : ObjectType.Box, c * STEP_X, 0));
                    // Layer 1: 7 crystal colonnade
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Column, c * STEP_X, 1));
                    // Layer 2: 5 sanctuary nave
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 2));
                    // Layer 3: 3 crown arches
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 3));
                    // Layer 4: 2 crystal spires
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  0.5f * STEP_X, 4));
                    break;

                // =========================================================================
                // TIER 4: LEVELS 16-20 (High Citadels & Royal Strongholds, 22-26 blocks)
                // =========================================================================
                case 16:
                    name = "The High Cathedral";
                    balls = 29;
                    stands = SingleStand(1.35f);
                    // Layer 0: 6 foundation
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 colonnade
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    // Layer 2: 5 nave
                    for (int c = 0; c < 5; c++) objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Column, (-2f + c) * STEP_X, 2));
                    // Layer 3: 4 upper gallery
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 2 twin belfries
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 4));
                    break;

                case 17:
                    name = "The Twin Monoliths";
                    balls = 30;
                    stands = DualStands(2.3f, 0.75f);
                    // Left Monolith: 4 layers of 3 blocks (12 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Stone, -2.3f - STEP_X, l));
                        objs.Add(CreateBlock(l == 2 ? ObjectType.PaintBottle : ObjectType.Box, -2.3f, l));
                        objs.Add(CreateBlock(ObjectType.Column, -2.3f + STEP_X, l));
                    }
                    // Right Monolith: 4 layers of 3 blocks (12 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Column, 2.3f - STEP_X, l));
                        objs.Add(CreateBlock(l == 2 ? ObjectType.PaintBottle : ObjectType.Box, 2.3f, l));
                        objs.Add(CreateBlock(ObjectType.Stone, 2.3f + STEP_X, l));
                    }
                    break;

                case 18:
                    name = "Palace of Shields";
                    balls = 30;
                    stands = SingleStand(1.35f);
                    // Layer 0: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 0 || c == 5 ? ObjectType.Cone : ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = 0; c < 5; c++) objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Cone, (-2f + c) * STEP_X, 2));
                    // Layer 3: 4 blocks
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Column, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 3 blocks
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 4));
                    break;

                case 19:
                    name = "The Arcane Observatory";
                    balls = 30;
                    stands = SingleStand(1.35f);
                    // Layer 0: 6 foundation blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Stone, (-2.5f + c) * STEP_X, 0));
                    // Layer 1: 6 blocks
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(c == 1 || c == 4 ? ObjectType.PaintBottle : ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = 0; c < 5; c++) objs.Add(CreateBlock(c == 2 ? ObjectType.PaintBottle : ObjectType.Column, (-2f + c) * STEP_X, 2));
                    // Layer 3: 4 telescope gantry
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Cone, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 3 celestial eye
                    objs.Add(CreateBlock(ObjectType.Column, -1.0f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Column,  1.0f * STEP_X, 4));
                    // Layer 5: 1 spire apex
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 5));
                    break;

                case 20:
                    name = "Grand Citadel Stronghold";
                    balls = 32;
                    stands = SingleStand(1.4f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Tier 4 Master Stronghold" });
                    // Layer 0: 7 blocks
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 7 blocks
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Column, c * STEP_X, 2));
                    // Layer 3: 4 blocks
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 2 tower heads
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4));
                    // Layer 5: 1 crown core
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 5));
                    break;

                // =========================================================================
                // TIER 5: LEVELS 21-25 (Master Demolition Citadels, 24-28 blocks)
                // =========================================================================
                case 21:
                    name = "The Citadel Coliseum";
                    balls = 32;
                    stands = SingleStand(1.45f);
                    // Layer 0: 7 foundation
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 7 wall
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 5 ring
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Cone, c * STEP_X, 2));
                    // Layer 3: 4 columns
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Column, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 2 arches
                    objs.Add(CreateBlock(ObjectType.Box, -0.75f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Box,  0.75f * STEP_X, 4));
                    break;

                case 22:
                    name = "Dual Fortress Bastions";
                    balls = 32;
                    stands = DualStands(2.3f, 0.75f);
                    // Left Bastion (13 blocks): 4 base, 4 mid, 3 high, 2 spires
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Stone, -2.3f + (-1.5f + c) * STEP_X, 0));
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box,   -2.3f + (-1.5f + c) * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Cone,        -2.3f - STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.3f, 2));
                    objs.Add(CreateBlock(ObjectType.Cone,        -2.3f + STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column, -2.3f - 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column, -2.3f + 0.5f * STEP_X, 3));

                    // Right Bastion (13 blocks)
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Stone, 2.3f + (-1.5f + c) * STEP_X, 0));
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box,   2.3f + (-1.5f + c) * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Cone,        2.3f - STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.3f, 2));
                    objs.Add(CreateBlock(ObjectType.Cone,        2.3f + STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column, 2.3f - 0.5f * STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Column, 2.3f + 0.5f * STEP_X, 3));
                    break;

                case 23:
                    name = "The Great Crystal Pyramid";
                    balls = 32;
                    stands = SingleStand(1.4f);
                    // 5-layer pyramid (7 + 6 + 5 + 4 + 3 + 1 = 26 blocks)
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Box, (-2.5f + c) * STEP_X, 1));
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Column, c * STEP_X, 2));
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    for (int c = -1; c <= 1; c++) objs.Add(CreateBlock(ObjectType.Column, c * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 5));
                    break;

                case 24:
                    name = "Vault of the Celestials";
                    balls = 32;
                    stands = TieredStands(6.8f, 8.2f, 0.9f);
                    // Front Bastion: 12 blocks
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, 6.8f));
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 1, 6.8f));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 2, 6.8f));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 2, 6.8f));

                    // Rear Vault: 14 blocks
                    float rY = BASE_Y + 0.9f;
                    for (int c = -2; c <= 2; c++) objs.Add(CreateCustomBlock(ObjectType.Cone, new Vector3(c * STEP_X, rY, 8.2f)));
                    for (int c = -2; c <= 2; c++) objs.Add(CreateCustomBlock(c == 0 || c == -2 || c == 2 ? ObjectType.PaintBottle : ObjectType.Column, new Vector3(c * STEP_X, rY + LAYER_STEP_Y, 8.2f)));
                    for (int c = -1; c <= 1; c++) objs.Add(CreateCustomBlock(ObjectType.Box, new Vector3(c * STEP_X, rY + 2 * LAYER_STEP_Y, 8.2f)));
                    objs.Add(CreateCustomBlock(ObjectType.PaintBottle, new Vector3(0f, rY + 3 * LAYER_STEP_Y, 8.2f)));
                    break;

                case 25:
                    name = "Royal Sky Fortress";
                    balls = 34;
                    stands = SingleStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Legendary Citadels" });
                    // Layer 0: 7 foundation
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) >= 2 ? ObjectType.Stone : ObjectType.Box, c * STEP_X, 0));
                    // Layer 1: 7 curtain wall
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : (c == 0 ? ObjectType.PaintBottle : ObjectType.Box), c * STEP_X, 1));
                    // Layer 2: 5 rampart
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Column, c * STEP_X, 2));
                    // Layer 3: 4 upper hall
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 3 towers
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4));
                    // Layer 5: 1 spire
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 5));
                    break;

                // =========================================================================
                // TIER 6: LEVELS 26-30 (Legendary Finale Citadels, 26-30 blocks)
                // =========================================================================
                case 26:
                    name = "The Crown Bastion";
                    balls = 34;
                    stands = SingleStand(1.4f);
                    // Layer 0: 7 blocks
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 7 blocks
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 5 blocks
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(ObjectType.Column, c * STEP_X, 2));
                    // Layer 3: 4 blocks
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 2 tower heads
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 4));
                    // Layer 5: 1 pinnacle
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 5));
                    break;

                case 27:
                    name = "Citadel of the Ancients";
                    balls = 35;
                    stands = DualStands(2.35f, 0.75f);
                    // Left Ancient Tower (14 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Stone, -2.35f - STEP_X, l));
                        objs.Add(CreateBlock(ObjectType.Box,   -2.35f, l));
                        objs.Add(CreateBlock(ObjectType.Column, -2.35f + STEP_X, l));
                    }
                    objs.Add(CreateBlock(ObjectType.Cone,        -2.35f - 0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.35f + 0.5f * STEP_X, 4));

                    // Right Ancient Tower (14 blocks)
                    for (int l = 0; l < 4; l++)
                    {
                        objs.Add(CreateBlock(ObjectType.Column, 2.35f - STEP_X, l));
                        objs.Add(CreateBlock(ObjectType.Box,    2.35f, l));
                        objs.Add(CreateBlock(ObjectType.Stone,  2.35f + STEP_X, l));
                    }
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.35f - 0.5f * STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,        2.35f + 0.5f * STEP_X, 4));
                    break;

                case 28:
                    name = "The Core Reactor Spire";
                    balls = 35;
                    stands = SingleStand(1.4f);
                    // 5 Reactor Cores stacked and shielded (28 blocks)
                    // Layer 0: 7 foundation
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 7 containment ring
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 5 shielding
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : (Mathf.Abs(c) == 2 ? ObjectType.Cone : ObjectType.Column), c * STEP_X, 2));
                    // Layer 3: 5 core chamber
                    for (int c = -2; c <= 2; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 3));
                    // Layer 4: 3 upper containment
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 4));
                    // Layer 5: 1 apex core
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 5));
                    break;

                case 29:
                    name = "The Emperor's Redoubt";
                    balls = 35;
                    stands = SingleStand(1.45f);
                    // Layer 0: 7 heavy armor & stone foundation
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(Mathf.Abs(c) >= 2 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0));
                    // Layer 1: 7 castle wall
                    for (int c = -3; c <= 3; c++) objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 1));
                    // Layer 2: 6 rampart
                    for (int c = 0; c < 6; c++) objs.Add(CreateBlock(ObjectType.Column, (-2.5f + c) * STEP_X, 2));
                    // Layer 3: 4 throne room
                    for (int c = 0; c < 4; c++) objs.Add(CreateBlock(ObjectType.Box, (-1.5f + c) * STEP_X, 3));
                    // Layer 4: 3 imperial crowns
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4));
                    // Layer 5: 1 sky pinnacle
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 5));
                    break;

                case 30:
                    name = "The Crown Citadel";
                    balls = 35;
                    stands = SingleStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Level 30 Finale (Grand Victory)" });
                    // Layer 0: 7 foundation blocks (Y = 1.845)
                    objs.Add(CreateBlock(ObjectType.Cone,  -2.265f, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, -1.510f, 0));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.755f, 0));
                    objs.Add(CreateBlock(ObjectType.Box,    0.000f, 0));
                    objs.Add(CreateBlock(ObjectType.Box,    0.755f, 0));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.510f, 0));
                    objs.Add(CreateBlock(ObjectType.Cone,   2.265f, 0));

                    // Layer 1: 7 crystal & core colonnade (Y = 2.590)
                    objs.Add(CreateBlock(ObjectType.Cone,   -2.265f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -1.510f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -0.755f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0.000f, 1));
                    objs.Add(CreateBlock(ObjectType.Column,  0.755f, 1));
                    objs.Add(CreateBlock(ObjectType.Column,  1.510f, 1));
                    objs.Add(CreateBlock(ObjectType.Cone,    2.265f, 1));

                    // Layer 2: 6 mid bastion (Y = 3.335)
                    objs.Add(CreateBlock(ObjectType.Box,   -1.8875f, 2));
                    objs.Add(CreateBlock(ObjectType.Box,   -1.1325f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.3775f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.3775f, 2));
                    objs.Add(CreateBlock(ObjectType.Box,    1.1325f, 2));
                    objs.Add(CreateBlock(ObjectType.Box,    1.8875f, 2));

                    // Layer 3: 4 upper ramparts (Y = 4.080)
                    objs.Add(CreateBlock(ObjectType.Column, -1.1325f, 3));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.3775f, 3));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.3775f, 3));
                    objs.Add(CreateBlock(ObjectType.Column,  1.1325f, 3));

                    // Layer 4: 3 high pinnacles (Y = 4.825)
                    objs.Add(CreateBlock(ObjectType.Cone, -0.755f, 4));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    objs.Add(CreateBlock(ObjectType.Cone,  0.755f, 4));

                    // Layer 5: 1 ultimate crown core (Y = 5.570)
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 5));
                    break;
            }

            data.SetData(id, name, balls, reward, stands, objs.ToArray(), events.ToArray());
        }
    }
}
