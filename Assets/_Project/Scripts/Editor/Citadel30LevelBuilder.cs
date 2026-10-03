using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Master builder for all 30 Royal Smash Citadel levels.
    /// Overhauled to ensure:
    /// 1. All stands and blocks moved to Z = 9.00m (pushed back from 7.50m for improved perspective & ball arc).
    /// 2. Deep double-row stands (Scale.z = 1.35) have 100% IDENTICAL front and back rows:
    ///    Every block at (x, y, FRONT_Z=8.55) has an exact twin at (x, y, BACK_Z=9.45) for true physical thickness.
    /// 3. ZERO overlapping or interpenetrating blocks across all 30 levels.
    /// 4. Side-by-side dual stands perfectly aligned at Y = 0.0m.
    /// </summary>
    public static class Citadel30LevelBuilder
    {
        private const float BASE_Y = 1.85f;
        private const float STEP_Y = 0.76f;
        private const float STEP_X = 0.78f;
        private const float STAND_Z = 9.00f;
        private const float FRONT_Z = 8.55f;
        private const float BACK_Z = 9.45f;
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

                // Also keep legacy ScriptableObjects/Levels/ folder in exact sync
                string legacyPath = $"Assets/_Project/ScriptableObjects/Levels/LevelData_{levelId:D2}.asset";
                LevelData legacyLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(legacyPath);
                if (legacyLevelData != null)
                {
                    BuildLevelData(levelId, legacyLevelData);
                    EditorUtility.SetDirty(legacyLevelData);
                }
            }

            var citadelDb = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
            if (citadelDb != null)
            {
                citadelDb.SetLevels(levelList.ToArray());
                EditorUtility.SetDirty(citadelDb);
            }

            var coreDb = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
            if (coreDb != null)
            {
                coreDb.SetLevels(levelList.ToArray());
                EditorUtility.SetDirty(coreDb);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Citadel30LevelBuilder] Successfully regenerated all 30 Citadel levels at Z=9.00m with identical 2-row thickness!");
        }

        private static float GetLayerY(int layer) => BASE_Y + (layer * STEP_Y);

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

        /// <summary>
        /// Creates a thick block pair: identical block placed at FRONT_Z and BACK_Z.
        /// Guaranteed identical facade and back row for physical depth and structural thickness.
        /// </summary>
        private static void AddThickBlock(List<ObjectPlacement> objs, ObjectType type, float x, int layer, bool isRequired = true)
        {
            objs.Add(CreateBlock(type, x, layer, FRONT_Z, isRequired));
            objs.Add(CreateBlock(type, x, layer, BACK_Z, isRequired));
        }

        private static StandPlacement[] SingleDeepStand(float scaleX = 1.35f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.35f),
                    Rotation = Vector3.zero
                }
            };
        }

        private static StandPlacement[] DualSideBySideStands(float xOffset = 2.10f, float scaleX = 0.75f)
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

        public static void BuildLevelData(int id, LevelData data)
        {
            var stands = SingleDeepStand(1.25f);
            var objs = new List<ObjectPlacement>();
            var events = new List<LevelEvent>();
            int balls = 25;
            int reward = 50 + (id - 1) * 10;
            string name = $"Level {id}";

            switch (id)
            {
                // =========================================================================
                // TIER 1: LEVELS 1-5 (Citadel Gateways & Foundations)
                // =========================================================================
                case 1:
                    name = "The Grand Gateway";
                    balls = 25;
                    stands = SingleDeepStand(1.25f);
                    // 7 base blocks, doubled front & back = 14 thick blocks
                    AddThickBlock(objs, ObjectType.Stone, -STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,   0f,      0);
                    AddThickBlock(objs, ObjectType.Stone,  STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 2);
                    break;

                case 2:
                    name = "Dual Watchtowers";
                    balls = 26;
                    stands = DualSideBySideStands(2.10f, 0.75f);
                    // Left Watchtower (6 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.10f - 0.5f * STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.10f + 0.5f * STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.10f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.10f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  -2.10f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  -2.10f + 0.5f * STEP_X, 2, STAND_Z));
                    // Right Watchtower (6 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.10f - 0.5f * STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.10f + 0.5f * STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.10f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.10f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  2.10f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  2.10f + 0.5f * STEP_X, 2, STAND_Z));
                    break;

                case 3:
                    name = "Stone Fortress Wall";
                    balls = 26;
                    stands = SingleDeepStand(1.30f);
                    // 8 base blocks doubled = 16 thick blocks
                    AddThickBlock(objs, ObjectType.Stone, -1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,   -0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,    0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone,  1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 2);
                    break;

                case 4:
                    name = "The Royal Step Pyramid";
                    balls = 27;
                    stands = SingleDeepStand(1.35f);
                    // 8 base pyramid blocks doubled = 16 thick blocks
                    AddThickBlock(objs, ObjectType.Stone, -1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,   -0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,    0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone,  1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 2);
                    break;

                case 5:
                    name = "The Gatekeeper Bastion";
                    balls = 28;
                    stands = SingleDeepStand(1.35f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "Mega Ball Unlocked!" });
                    // 9 base bastion blocks doubled = 18 thick blocks
                    AddThickBlock(objs, ObjectType.Stone, -1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone, -0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone,  0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone,  1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box, 0f, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                // =========================================================================
                // TIER 2: LEVELS 6-10 (Dynamic Destruction & Chains)
                // =========================================================================
                case 6:
                    name = "Twin Bastions";
                    balls = 28;
                    stands = DualSideBySideStands(2.10f, 0.8f);
                    // Left Bastion (8 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.10f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.10f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.10f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.10f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.10f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -2.10f, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.10f - 0.5f * STEP_X, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.10f + 0.5f * STEP_X, 3, STAND_Z));
                    // Right Bastion (8 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.10f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.10f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.10f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.10f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.10f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 2.10f, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.10f - 0.5f * STEP_X, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.10f + 0.5f * STEP_X, 3, STAND_Z));
                    break;

                case 7:
                    name = "Sapphire Spire Citadel";
                    balls = 29;
                    stands = SingleDeepStand(1.35f);
                    // 10 base blocks doubled = 20 thick blocks
                    AddThickBlock(objs, ObjectType.Stone, -1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,   -0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box,    0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone,  1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, 0f, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 3);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 4);
                    break;

                case 8:
                    name = "The Armored Bunker";
                    balls = 29;
                    stands = SingleDeepStand(1.35f);
                    // 10 base blocks doubled = 20 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 2 ? ObjectType.Stone : ObjectType.Box, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, 0f, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 9:
                    name = "Citadel Aqueduct";
                    balls = 30;
                    stands = SingleDeepStand(1.40f);
                    // 11 base blocks doubled = 22 thick blocks
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column,  0.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 10:
                    name = "The Royal Citadel Gate";
                    balls = 30;
                    stands = DualSideBySideStands(2.15f, 0.8f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "Infinity Ball Unlocked!" });
                    events.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Prompt Player Profile" });
                    // Left Gate Tower (10 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.15f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.15f,     1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.15f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.15f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.15f - 0.5f * STEP_X, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.15f + 0.5f * STEP_X, 3, STAND_Z));
                    // Right Gate Tower (10 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.15f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.15f,     1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.15f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.15f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.15f - 0.5f * STEP_X, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.15f + 0.5f * STEP_X, 3, STAND_Z));
                    break;

                // =========================================================================
                // TIER 3: LEVELS 11-15 (Tactical Puzzle Destructions)
                // =========================================================================
                case 11:
                    name = "The Arcane Vault";
                    balls = 31;
                    stands = SingleDeepStand(1.35f);
                    // 11 base blocks doubled = 22 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 2 ? ObjectType.Stone : ObjectType.Box, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box, -0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Box,  0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 12:
                    name = "Triple Spire Keep";
                    balls = 31;
                    stands = SingleDeepStand(1.40f);
                    // 11 base blocks doubled = 22 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f,        1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Cone, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Column, 0f,           2);
                    AddThickBlock(objs, ObjectType.Cone,  1.5f * STEP_X, 2);
                    break;

                case 13:
                    name = "Citadel Tiered Fortress";
                    balls = 32;
                    stands = SingleDeepStand(1.40f);
                    // 12 base blocks doubled = 24 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 14:
                    name = "Ironclad Redoubt";
                    balls = 32;
                    stands = SingleDeepStand(1.40f);
                    // 12 base blocks doubled = 24 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 2);
                    break;

                case 15:
                    name = "Sanctuary of Sapphire";
                    balls = 33;
                    stands = SingleDeepStand(1.40f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sapphire Temple Unlocked" });
                    // 12 base blocks doubled = 24 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f,        1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, -1.5f * STEP_X, 3);
                    AddThickBlock(objs, ObjectType.Cone,  1.5f * STEP_X, 3);
                    break;

                // =========================================================================
                // TIER 4: LEVELS 16-20 (Grand Fortresses & High Towers)
                // =========================================================================
                case 16:
                    name = "The High Cathedral";
                    balls = 33;
                    stands = SingleDeepStand(1.40f);
                    // 13 base blocks doubled = 26 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f,        2);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 17:
                    name = "The Twin Monoliths";
                    balls = 33;
                    stands = DualSideBySideStands(2.15f, 0.8f);
                    // Left Monolith (11 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.15f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.15f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.15f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -2.15f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.15f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.15f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.15f, 4, STAND_Z));
                    // Right Monolith (11 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.15f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.15f - 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.15f + 0.5f * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.15f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 2.15f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.15f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.15f, 4, STAND_Z));
                    break;

                case 18:
                    name = "Palace of Shields";
                    balls = 34;
                    stands = SingleDeepStand(1.40f);
                    // 13 base blocks doubled = 26 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 3 ? ObjectType.Stone : ObjectType.Box, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f,        1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Stone,  STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 19:
                    name = "The Arcane Observatory";
                    balls = 34;
                    stands = SingleDeepStand(1.40f);
                    // 13 base blocks doubled = 26 thick blocks
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,    0f,             1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f,        2);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 4);
                    break;

                case 20:
                    name = "Grand Citadel Stronghold";
                    balls = 34;
                    stands = SingleDeepStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Citadel Stronghold Unlocked" });
                    // 14 base blocks doubled = 28 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,   -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box,    0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Stone,  STEP_X, 2);
                    break;

                // =========================================================================
                // TIER 5: LEVELS 21-25 (Master Siege Fortresses)
                // =========================================================================
                case 21:
                    name = "The Citadel Coliseum";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // 14 base blocks doubled = 28 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, Mathf.Abs(c) >= 2 ? ObjectType.Column : ObjectType.Box, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Stone, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Column,  0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 22:
                    name = "Dual Fortress Bastions";
                    balls = 35;
                    stands = DualSideBySideStands(2.20f, 0.8f);
                    // Left Heavy Bastion (12 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.20f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.20f,     1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.20f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.20f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.20f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.20f - 0.5f * STEP_X, 4, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.20f + 0.5f * STEP_X, 4, STAND_Z));
                    // Right Heavy Bastion (12 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.20f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.20f,     1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.20f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.20f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.20f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.20f - 0.5f * STEP_X, 4, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.20f + 0.5f * STEP_X, 4, STAND_Z));
                    break;

                case 23:
                    name = "The Great Crystal Pyramid";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // 14 base blocks doubled = 28 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Box, c * STEP_X, 0);
                    for (int c = -2; c <= 2; c++)
                        AddThickBlock(objs, ObjectType.Column, c * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 24:
                    name = "Vault of the Celestials";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // 14 base blocks doubled = 28 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Stone,  0.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 25:
                    name = "Royal Sky Fortress";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sky Fortress Unlocked" });
                    // 15 base blocks doubled = 30 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Box, -2f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -1f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Column, 1f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box, 2f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Cone, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone,  1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 2);
                    break;

                // =========================================================================
                // TIER 6: LEVELS 26-30 (Epic Finale & Citadel Apex)
                // =========================================================================
                case 26:
                    name = "The Crown Bastion";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // 15 base blocks doubled = 30 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone,  -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone,   0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Column,  STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 27:
                    name = "Citadel of the Ancients";
                    balls = 36;
                    stands = DualSideBySideStands(2.20f, 0.8f);
                    // Left Ancient Tower (13 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.20f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -2.20f,          1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.20f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -2.20f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -2.20f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2.20f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.20f - 0.5f * STEP_X, 4, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -2.20f + 0.5f * STEP_X, 4, STAND_Z));
                    // Right Ancient Tower (13 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f - STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f,          0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f + STEP_X, 0, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.20f - STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   2.20f,          1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.20f + STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 2.20f - 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 2.20f + 0.5f * STEP_X, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 2.20f, 3, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.20f - 0.5f * STEP_X, 4, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 2.20f + 0.5f * STEP_X, 4, STAND_Z));
                    break;

                case 28:
                    name = "The Core Reactor Spire";
                    balls = 36;
                    stands = SingleDeepStand(1.45f);
                    // 15 base blocks doubled = 30 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Stone,  STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 29:
                    name = "The Emperor's Redoubt";
                    balls = 36;
                    stands = SingleDeepStand(1.50f);
                    // 16 base blocks doubled = 32 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, -0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle,  0.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  1.5f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone, -STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Column, 0f, 2);
                    AddThickBlock(objs, ObjectType.Stone,  STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;

                case 30:
                    name = "The Crown Citadel";
                    balls = 36;
                    stands = SingleDeepStand(1.50f);
                    events.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Level 30 Finale (Grand Victory)" });
                    // 16 base blocks doubled = 32 thick blocks
                    for (int c = -3; c <= 3; c++)
                        AddThickBlock(objs, Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0);
                    AddThickBlock(objs, ObjectType.Column, -2f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Stone,  -1f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 1);
                    AddThickBlock(objs, ObjectType.Stone,   1f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Column,  2f * STEP_X, 1);
                    AddThickBlock(objs, ObjectType.Box, -1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.PaintBottle, 0f, 2);
                    AddThickBlock(objs, ObjectType.Box,  1.5f * STEP_X, 2);
                    AddThickBlock(objs, ObjectType.Cone, 0f, 3);
                    break;
            }

            data.SetData(id, name, balls, reward, stands, objs.ToArray(), events.ToArray());
        }
    }
}
