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
    /// 1. ZERO overlapping or interpenetrating blocks (clean 0.78m X step, 0.76m Y step, 0.90m Z row step).
    /// 2. Deep single stands (Scale.z = 1.25) with authentic 2-row layout (front row Z=7.05, back row Z=7.95).
    /// 3. Perfectly aligned side-by-side dual stands (Y=0.0 on both, no floating upper platform).
    /// 4. Rich, satisfying physical puzzle progression for MVP.
    /// </summary>
    public static class Citadel30LevelBuilder
    {
        private const float BASE_Y = 1.85f;
        private const float STEP_Y = 0.76f;
        private const float STEP_X = 0.78f;
        private const float STAND_Z = 7.50f;
        private const float FRONT_Z = 7.05f;
        private const float BACK_Z = 7.95f;
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

            Debug.Log("[Citadel30LevelBuilder] Successfully regenerated all 30 Citadel levels with zero overlaps and calibrated 2-row layouts!");
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

        private static StandPlacement[] SingleDeepStand(float scaleX = 1.3f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.25f),
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
            var stands = SingleDeepStand(1.2f);
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
                    stands = SingleDeepStand(1.2f);
                    // Front row: 3 low barriers
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0, FRONT_Z));
                    // Back row: 2 stone pillars + keystone + paint bottle + roof cones
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0.5f * STEP_X, 3, BACK_Z));
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
                    stands = SingleDeepStand(1.25f);
                    // Front row: 4 foundation blocks
                    objs.Add(CreateBlock(ObjectType.Box, -1.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  1.5f * STEP_X, 0, FRONT_Z));
                    // Back row: stone defense with central paint bottle
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 3, BACK_Z));
                    break;

                case 4:
                    name = "The Royal Step Pyramid";
                    balls = 27;
                    stands = SingleDeepStand(1.3f);
                    // Front row: 3 step blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,     0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,     1, FRONT_Z));
                    // Back row: 5-3-1 Pyramid
                    objs.Add(CreateBlock(ObjectType.Stone, -2f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -1f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0f,          0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    1f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  2f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 3, BACK_Z));
                    break;

                case 5:
                    name = "The Gatekeeper Bastion";
                    balls = 28;
                    stands = SingleDeepStand(1.35f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "Mega Ball Unlocked!" });
                    // Front row: Heavy stone barricade
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Tower with protected explosive core
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
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
                    stands = SingleDeepStand(1.3f);
                    // Front row: 5 barrier blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -2f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -1f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,          0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    1f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  2f * STEP_X, 0, FRONT_Z));
                    // Back row: High crystal spire (13 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 5, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  0.5f * STEP_X, 5, BACK_Z));
                    break;

                case 8:
                    name = "The Armored Bunker";
                    balls = 29;
                    stands = SingleDeepStand(1.35f);
                    // Front row: Heavy stone defense wall
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 2 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Internal bunker vault
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 3, BACK_Z));
                    break;

                case 9:
                    name = "Citadel Aqueduct";
                    balls = 30;
                    stands = SingleDeepStand(1.4f);
                    // Front row: 4 aqueduct arches
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0f,     1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Upper water channel with paint bottles
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,          0f,     1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  0.5f * STEP_X, 3, BACK_Z));
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
                    // Front row: 5 barrier blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -2f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -1f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,          0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    1f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  2f * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Arcane Vault (13 blocks)
                    objs.Add(CreateBlock(ObjectType.Stone, -1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  1.5f * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 12:
                    name = "Triple Spire Keep";
                    balls = 31;
                    stands = SingleDeepStand(1.4f);
                    // Front row: 5 blocks
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    // Back row: 3 Rising Spires (15 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,             1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -1.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  1.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 13:
                    name = "Citadel Tiered Fortress";
                    balls = 32;
                    stands = SingleDeepStand(1.4f); // Fixed: Deeper single stand, no floating platform!
                    // Front row: Rampart barrier (7 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Grand Citadel Bastion (14 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    break;

                case 14:
                    name = "Ironclad Redoubt";
                    balls = 32;
                    stands = SingleDeepStand(1.35f);
                    // Front row: Heavy stone defense (6 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    // Back row: Fortress Keep (15 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    break;

                case 15:
                    name = "Sanctuary of Sapphire";
                    balls = 33;
                    stands = SingleDeepStand(1.4f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sapphire Temple Unlocked" });
                    // Front row: Temple colonnade (7 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 2 ? ObjectType.Column : ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Sacred Altar (16 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,     2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                // =========================================================================
                // TIER 4: LEVELS 16-20 (Grand Fortresses & High Towers)
                // =========================================================================
                case 16:
                    name = "The High Cathedral";
                    balls = 33;
                    stands = SingleDeepStand(1.4f);
                    // Front row: Cathedral portico (7 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 1, FRONT_Z));
                    // Back row: Grand Cathedral nave (16 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -1.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  1.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
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
                    stands = SingleDeepStand(1.4f);
                    // Front row: 7 shield blocks
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -1.5f * STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  1.5f * STEP_X, 1, FRONT_Z));
                    // Back row: Shield Palace Keep (16 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 19:
                    name = "The Arcane Observatory";
                    balls = 34;
                    stands = SingleDeepStand(1.4f);
                    // Front row: Observation ramparts (7 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 1, FRONT_Z));
                    // Back row: High Observatory Spire (17 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,             1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 5, BACK_Z));
                    break;

                case 20:
                    name = "Grand Citadel Stronghold";
                    balls = 34;
                    stands = SingleDeepStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Citadel Stronghold Unlocked" });
                    // Front row: 8 bastion blocks
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1, FRONT_Z));
                    // Back row: Citadel Keep (17 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                // =========================================================================
                // TIER 5: LEVELS 21-25 (Master Siege Fortresses)
                // =========================================================================
                case 21:
                    name = "The Citadel Coliseum";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // Front row: Coliseum ring wall (8 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) >= 2 ? ObjectType.Column : ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    // Back row: Imperial Arena Tribune (17 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,   -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
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
                    // Front row: Step tier (8 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  STEP_X, 1, FRONT_Z));
                    // Back row: Master Pyramid (17 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Column, c * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 24:
                    name = "Vault of the Celestials";
                    balls = 35;
                    stands = SingleDeepStand(1.45f); // Fixed: Deeper single stand, no floating platform!
                    // Front row: Outer celestial guard (8 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 1, FRONT_Z));
                    // Back row: Celestial Vault Chamber (17 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 25:
                    name = "Royal Sky Fortress";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sky Fortress Unlocked" });
                    // Front row: 8 fortress ramparts
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1, FRONT_Z));
                    // Back row: Sky Fortress Keep (18 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,    0f,     2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 5, BACK_Z));
                    break;

                // =========================================================================
                // TIER 6: LEVELS 26-30 (Epic Finale & Citadel Apex)
                // =========================================================================
                case 26:
                    name = "The Crown Bastion";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // Front row: 8 barrier blocks
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(ObjectType.Box, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 1, FRONT_Z));
                    // Back row: Crown Bastion Keep (18 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 5, BACK_Z));
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
                    // Front row: 8 containment shields
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, FRONT_Z));
                    // Back row: Reactor core tower (18 blocks)
                    for (int c = -2; c <= 2; c++)
                        objs.Add(CreateBlock(c == 0 ? ObjectType.PaintBottle : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 4, BACK_Z));
                    break;

                case 29:
                    name = "The Emperor's Redoubt";
                    balls = 36;
                    stands = SingleDeepStand(1.5f);
                    // Front row: Heavy imperial defenses (8 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 1, FRONT_Z));
                    // Back row: Imperial Redoubt Keep (19 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) >= 2 ? ObjectType.Cone : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  0.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  1.5f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  0.5f * STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 5, BACK_Z));
                    break;

                case 30:
                    name = "The Crown Citadel";
                    balls = 36;
                    stands = SingleDeepStand(1.5f);
                    events.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Level 30 Finale (Grand Victory)" });
                    // Front row: Imperial Grand Entrance (9 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Stone, c * STEP_X, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -STEP_X, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle,  STEP_X, 1, FRONT_Z));
                    // Back row: The Imperial Citadel Citadel (19 blocks)
                    for (int c = -3; c <= 3; c++)
                        objs.Add(CreateBlock(Mathf.Abs(c) == 3 ? ObjectType.Cone : ObjectType.Box, c * STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -2f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  -1f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,   1f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  2f * STEP_X, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, -0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone,  0.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box,  1.5f * STEP_X, 2, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column,  STEP_X, 3, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone,  STEP_X, 4, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 5, BACK_Z));
                    break;
            }

            data.SetData(id, name, balls, reward, stands, objs.ToArray(), events.ToArray());
        }
    }
}
