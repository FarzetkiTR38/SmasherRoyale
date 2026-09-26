using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Programmatic generator for all 30 Sky Citadel levels.
    /// Follows Master Build Prompt Section 9 & 10 and Phase 28 specifications:
    /// - Citadel block dimensions (0.74m cube with 0.745m center grid spacing).
    /// - Stand tabletop surface at Y = 1.475m (Layer 0 center Y = 1.845m, Layer step Y = 0.746m).
    /// - IsRequiredTarget distinction for targets vs strategic obstacles.
    /// - Milestone events: L5 MegaBall, L10 InfinityBall & Username, L15/20/25 NewContent, L30 FinalLevel.
    /// </summary>
    public static class Citadel30LevelBuilder
    {
        private const float BASE_Y = 1.845f;
        private const float LAYER_STEP_Y = 0.746f;
        private const float STEP_X = 0.745f;
        private const float STAND_Z = 7.50f;
        private const float FRONT_Z = 7.15f;
        private const float BACK_Z = 7.85f;
        private static readonly Vector3 BLOCK_SCALE = new Vector3(0.74f, 0.74f, 0.74f);

        [MenuItem("RoyalSmash/Levels/Generate All 30 Citadel Levels")]
        public static void GenerateAllLevels()
        {
            const string levelsFolder = "Assets/_Project/Art/SkyCitadel/Levels";

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
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Citadel30LevelBuilder] Successfully generated and calibrated all 30 Citadel levels!");
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

        private static void BuildLevelData(int id, LevelData data)
        {
            var stands = new StandPlacement[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, STAND_Z),
                    Scale = new Vector3(1f, 1f, 0.5f),
                    Rotation = Vector3.zero
                }
            };

            var objs = new List<ObjectPlacement>();
            var events = new List<LevelEvent>();
            int balls = 20;
            int reward = 50 + (id - 1) * 5;
            string name = $"Level {id}";

            switch (id)
            {
                // ==================== TIER 1: LEVELS 1-4 (Foundations) ====================
                case 1:
                    name = "Citadel Foundations";
                    balls = 20;
                    reward = 50;
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    break;

                case 2:
                    name = "Triple Spire";
                    balls = 20;
                    reward = 55;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 1));
                    break;

                case 3:
                    name = "Stone Arch";
                    balls = 20;
                    reward = 60;
                    // Pillars are stone obstacles (not required to win, but provide foundation)
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 2));
                    break;

                case 4:
                    name = "Mini Pyramid";
                    balls = 20;
                    reward = 65;
                    // Layer 0: 3 base
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    // Layer 1: 2 mid
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 1));
                    // Layer 2: 1 core top
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    break;

                // ==================== TIER 2: LEVELS 5-9 (Mega Ball Unlock) ====================
                case 5:
                    name = "The Gatekeeper";
                    balls = 22;
                    reward = 75;
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "3" });
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "MegaBall" });
                    // Armored flank obstacles
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 2));
                    break;

                case 6:
                    name = "Crystal Tower";
                    balls = 22;
                    reward = 80;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    break;

                case 7:
                    name = "Domino Causeway";
                    balls = 22;
                    reward = 85;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 2f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 2f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 1));
                    break;

                case 8:
                    name = "Twin Keeps";
                    balls = 22;
                    reward = 90;
                    // Left Keep
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 1.5f, 2));
                    // Right Keep
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 1.5f, 2));
                    // Central bridge target
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    break;

                case 9:
                    name = "Armored Bastion";
                    balls = 22;
                    reward = 95;
                    // Front armored obstacles shield the core
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0, STAND_Z, isRequired: true));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 2));
                    break;

                // ==================== TIER 3: LEVELS 10-14 (Infinity Ball Unlock) ====================
                case 10:
                    name = "Royal Fortress";
                    balls = 25;
                    reward = 110;
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "3" });
                    events.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Player" });
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "InfinityBall" });
                    // Layer 0: 5 blocks
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X * 2f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X * 2f, 0, STAND_Z, isRequired: false));
                    // Layer 1: 4 blocks
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 1));
                    // Layer 2: 3 blocks
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 2));
                    break;

                case 11:
                    name = "Heavy Barricade";
                    balls = 25;
                    reward = 115;
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    break;

                case 12:
                    name = "Triple Citadel Spire";
                    balls = 25;
                    reward = 120;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 1.5f, 1));
                    break;

                case 13:
                    name = "Fragile Foundation";
                    balls = 25;
                    reward = 125;
                    // Fragile columns at the base, heavy top
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.75f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.75f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X * 0.5f, 2, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X * 0.5f, 2, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    break;

                case 14:
                    name = "Great Bastion Wall";
                    balls = 25;
                    reward = 130;
                    for (int x = -2; x <= 2; x++)
                        objs.Add(CreateBlock(x == 0 ? ObjectType.PaintBottle : ObjectType.Box, x * STEP_X, 0));
                    for (int x = -1; x <= 1; x++)
                        objs.Add(CreateBlock(ObjectType.Column, x * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    break;

                // ==================== TIER 4: LEVELS 15-19 (Advanced Physics) ====================
                case 15:
                    name = "Citadel Watchtower";
                    balls = 25;
                    reward = 140;
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "AdvancedTower" });
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3));
                    break;

                case 16:
                    name = "Crossroads Keep";
                    balls = 25;
                    reward = 145;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, 0f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    break;

                case 17:
                    name = "The Royal Vault";
                    balls = 25;
                    reward = 150;
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, STEP_X * 0.5f, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X * 1.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    break;

                case 18:
                    name = "Pendulum Pillars";
                    balls = 25;
                    reward = 155;
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.2f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 1.2f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.2f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.2f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 1.2f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.2f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.6f, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.6f, 3));
                    break;

                case 19:
                    name = "Iron Ramparts";
                    balls = 25;
                    reward = 160;
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X * 2f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X * 2f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X * 1.5f, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X * 1.5f, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    break;

                // ==================== TIER 5: LEVELS 20-24 (Two-Layer Tactical) ====================
                case 20:
                    name = "Citadel Colosseum";
                    balls = 28;
                    reward = 175;
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "TwoLayers" });
                    // Front Layer: Armored & stone shields
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 0, FRONT_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X, 0, FRONT_Z, isRequired: false));
                    // Back Layer: High-value targets
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.75f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.75f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2, BACK_Z));
                    break;

                case 21:
                    name = "Frontline Defense";
                    balls = 28;
                    reward = 180;
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X * 1.5f, 0, FRONT_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Cone, 0f, 0, FRONT_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X * 1.5f, 0, FRONT_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.5f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.5f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2, BACK_Z));
                    break;

                case 22:
                    name = "Twin Sky Castles";
                    balls = 28;
                    reward = 185;
                    // Left Tower
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 1.5f, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 2, FRONT_Z));
                    // Right Tower
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 0, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 1.5f, 1, FRONT_Z));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 2, FRONT_Z));
                    // Central Vault in back layer
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1, BACK_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2, BACK_Z));
                    break;

                case 23:
                    name = "Cantilever Fortress";
                    balls = 28;
                    reward = 190;
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X * 0.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X * 0.5f, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 2));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    break;

                case 24:
                    name = "Diamond Citadel";
                    balls = 28;
                    reward = 195;
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 2f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 2f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 4));
                    break;

                // ==================== TIER 6: LEVELS 25-29 (High Citadel Siege) ====================
                case 25:
                    name = "Sky Citadel Outer Gate";
                    balls = 30;
                    reward = 210;
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "SiegeCitadel" });
                    for (int x = -2; x <= 2; x++)
                    {
                        bool isFlank = Mathf.Abs(x) == 2;
                        objs.Add(CreateBlock(isFlank ? ObjectType.Cone : ObjectType.Box, x * STEP_X, 0, STAND_Z, isRequired: !isFlank));
                    }
                    for (int x = -1; x <= 1; x++)
                        objs.Add(CreateBlock(x == 0 ? ObjectType.PaintBottle : ObjectType.Column, x * STEP_X, 1));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, 0f, 3));
                    break;

                case 26:
                    name = "The Spire Keep";
                    balls = 30;
                    reward = 220;
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 0));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 0, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 0.5f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 4));
                    break;

                case 27:
                    name = "Garrison Stronghold";
                    balls = 30;
                    reward = 230;
                    for (int x = -2; x <= 2; x++)
                        objs.Add(CreateBlock(Mathf.Abs(x) == 2 ? ObjectType.Cone : ObjectType.Box, x * STEP_X, 0, FRONT_Z, isRequired: Mathf.Abs(x) != 2));
                    for (int x = -1; x <= 1; x++)
                        objs.Add(CreateBlock(ObjectType.Column, x * STEP_X, 0, BACK_Z));
                    for (int x = -1; x <= 1; x++)
                        objs.Add(CreateBlock(x == 0 ? ObjectType.PaintBottle : ObjectType.Box, x * STEP_X, 1, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2, STAND_Z));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3, STAND_Z));
                    break;

                case 28:
                    name = "Cataclysm Keep";
                    balls = 30;
                    reward = 240;
                    for (int x = -2; x <= 2; x++)
                        objs.Add(CreateBlock(ObjectType.Box, x * STEP_X, 0));
                    for (int x = -2; x <= 2; x++)
                    {
                        if (x == 0) objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 1));
                        else objs.Add(CreateBlock(ObjectType.Column, x * STEP_X, 1));
                    }
                    objs.Add(CreateBlock(ObjectType.Stone, -STEP_X, 2, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Stone, STEP_X, 2, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 4));
                    break;

                case 29:
                    name = "Citadel Supreme";
                    balls = 30;
                    reward = 250;
                    // Triple tower fortress
                    for (int x = -2; x <= 2; x++)
                    {
                        bool isCenter = x == 0;
                        objs.Add(CreateBlock(isCenter ? ObjectType.PaintBottle : ObjectType.Box, x * STEP_X, 0));
                    }
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 2f, 1));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 2f, 1));
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 2f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 2));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 2f, 2));
                    objs.Add(CreateBlock(ObjectType.Cone, -STEP_X, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Cone, STEP_X, 1, STAND_Z, isRequired: false));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    break;

                // ==================== TIER 7: LEVEL 30 (Grand MVP Finale) ====================
                case 30:
                    name = "The Crown Citadel";
                    balls = 35;
                    reward = 300;
                    events.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Victory" });

                    // Layer 0: 7 blocks base
                    for (int x = -3; x <= 3; x++)
                    {
                        bool isFlank = Mathf.Abs(x) == 3;
                        objs.Add(CreateBlock(isFlank ? ObjectType.Cone : ObjectType.Box, x * STEP_X * 0.85f, 0, STAND_Z, isRequired: !isFlank));
                    }
                    // Layer 1: 5 blocks
                    for (int x = -2; x <= 2; x++)
                    {
                        ObjectType t = (x == 0) ? ObjectType.PaintBottle : ObjectType.Column;
                        objs.Add(CreateBlock(t, x * STEP_X * 0.9f, 1));
                    }
                    // Layer 2: 4 blocks battlements
                    objs.Add(CreateBlock(ObjectType.Box, -STEP_X * 1.5f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, -STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.PaintBottle, STEP_X * 0.5f, 2));
                    objs.Add(CreateBlock(ObjectType.Box, STEP_X * 1.5f, 2));
                    // Layer 3: 3 blocks crown base
                    objs.Add(CreateBlock(ObjectType.Column, -STEP_X * 0.75f, 3));
                    objs.Add(CreateBlock(ObjectType.Box, 0f, 3));
                    objs.Add(CreateBlock(ObjectType.Column, STEP_X * 0.75f, 3));
                    // Layer 4: The Royal Crown Core!
                    objs.Add(CreateBlock(ObjectType.PaintBottle, 0f, 4));
                    break;
            }

            data.SetData(id, name, balls, reward, stands, objs.ToArray(), events.ToArray());
        }
    }
}
