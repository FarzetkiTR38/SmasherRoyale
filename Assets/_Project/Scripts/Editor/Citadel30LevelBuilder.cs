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
    /// 1. Levels 1 & 2 are strictly preserved from user design.
    /// 2. Levels 3-30 feature dense, elegant architectural patterns (checkerboards, arches, fortresses, colonnades, vaults).
    /// 3. Deep double-row stands have 100% IDENTICAL front and back rows (FRONT_Z=8.61, BACK_Z=9.39) for true physical thickness.
    /// 4. Multi-stand levels use SingleLeg stands at X = +-2.20m with matching towers.
    /// 5. Uniform block scale (0.78, 0.78, 0.78) with 0.78m grid step for perfectly flush, gapless alignment.
    /// </summary>
    public static class Citadel30LevelBuilder
    {
        private const float BASE_Y = 1.86f;
        private const float STEP_Y = 0.78f;
        private const float STEP_X = 0.78f;
        private const float STAND_Z = 9.00f;
        private const float FRONT_Z = 8.61f; // 9.00f - (0.78f * 0.5f) -> 0 gap with BACK_Z
        private const float BACK_Z = 9.39f;  // 9.00f + (0.78f * 0.5f) -> perfectly flush twin row
        private static readonly Vector3 BLOCK_SCALE = new Vector3(0.78f, 0.78f, 0.78f);

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

                // Preserve user's handcrafted Level 1 and Level 2
                if (levelId == 1 || levelId == 2)
                {
                    levelList.Add(levelData);
                    continue;
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

            Debug.Log("[Citadel30LevelBuilder] Successfully regenerated Citadel levels 3-30 with patterned architecture! (Levels 1 & 2 preserved)");
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

        private static void AddSymmetricThickBlock(List<ObjectPlacement> objs, ObjectType type, float x, int layer, bool isRequired = true)
        {
            if (Mathf.Abs(x) < 0.01f)
            {
                AddThickBlock(objs, type, 0f, layer, isRequired);
            }
            else
            {
                AddThickBlock(objs, type, -Mathf.Abs(x), layer, isRequired);
                AddThickBlock(objs, type, Mathf.Abs(x), layer, isRequired);
            }
        }

        private static void AddDualTowerBlock(List<ObjectPlacement> objs, ObjectType type, float standX, float localX, int layer, bool isRequired = true)
        {
            // Left tower
            objs.Add(CreateBlock(type, -standX + localX, layer, FRONT_Z, isRequired));
            objs.Add(CreateBlock(type, -standX + localX, layer, BACK_Z, isRequired));
            // Right tower
            objs.Add(CreateBlock(type, standX + localX, layer, FRONT_Z, isRequired));
            objs.Add(CreateBlock(type, standX + localX, layer, BACK_Z, isRequired));
        }

        private static StandPlacement[] SingleDeepStand(float scaleX = 1.35f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(0f, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.35f),
                    Rotation = Vector3.zero,
                    Type = StandType.StandardTwoLeg
                }
            };
        }

        private static StandPlacement[] DualSideBySideStands(float xOffset = 2.20f, float scaleX = 0.75f)
        {
            return new[]
            {
                new StandPlacement
                {
                    Position = new Vector3(-xOffset, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.0f),
                    Rotation = Vector3.zero,
                    Type = StandType.SingleLeg
                },
                new StandPlacement
                {
                    Position = new Vector3(xOffset, 0f, STAND_Z),
                    Scale = new Vector3(scaleX, 1.0f, 1.0f),
                    Rotation = Vector3.zero,
                    Type = StandType.SingleLeg
                }
            };
        }

        public static void BuildLevelData(int id, LevelData data)
        {
            var stands = SingleDeepStand(1.35f);
            var objs = new List<ObjectPlacement>();
            var events = new List<LevelEvent>();
            int balls = 26;
            int reward = 50 + (id - 1) * 10;
            string name = $"Level {id}";

            // Grid column centers
            float c1 = 0.39f;   // 0.5 * STEP_X
            float c2 = 1.17f;   // 1.5 * STEP_X
            float c3 = 1.95f;   // 2.5 * STEP_X

            float o0 = 0.00f;
            float o1 = 0.78f;   // 1.0 * STEP_X
            float o2 = 1.56f;   // 2.0 * STEP_X

            switch (id)
            {
                // =========================================================================
                // TIER 1: LEVELS 1-5 (Citadel Gateways & Foundations)
                // =========================================================================
                case 1:
                case 2:
                    // Handled directly - user preserves custom Level 1 & 2
                    return;

                case 3:
                    name = "Stone Fortress Wall";
                    balls = 26;
                    stands = SingleDeepStand(1.30f);
                    // 5-wide castellated wall: foundation (Stone) -> framed wall (Wood/Stone) -> battlements (Armored)
                    // Layer 0 (Foundation - 10 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone, o0, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, o1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, o2, 0);
                    // Layer 1 (Framed Wall - 10 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,   o0, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,   o1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, o2, 1);
                    // Layer 2 (Crenellated Battlements - 10 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  o0, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,   o1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  o2, 2);
                    break;

                case 4:
                    name = "The Royal Step Pyramid";
                    balls = 27;
                    stands = SingleDeepStand(1.35f);
                    // 4-tier step pyramid: 6 -> 4 -> 2 -> 2 apex
                    // Layer 0 (Base 6-wide - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Box,   c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c3, 0);
                    // Layer 1 (Terrace 4-wide - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,   c2, 1);
                    // Layer 2 (Keep 2-wide - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  c1, 2);
                    // Layer 3 (Apex Capstone - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  c1, 3);
                    break;

                case 5:
                    name = "The Gatekeeper Bastion";
                    balls = 28;
                    stands = SingleDeepStand(1.35f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockMegaBall, Data = "MegaBall unlocked!" });
                    // Fortified gatehouse with flanking watchtowers and grand central arch
                    // Layer 0 (Foundation - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c3, 0);
                    // Layer 1 (Flank Towers + Central Passage - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,   c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,   c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c3, 1);
                    // Layer 2 (Archway Spanner - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  c3, 2);
                    // Layer 3 (Tower Crowns - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,  c3, 3);
                    break;

                // =========================================================================
                // TIER 2: LEVELS 6-10 (Citadel Heights)
                // =========================================================================
                case 6:
                    name = "Twin Bastions";
                    balls = 28;
                    stands = DualSideBySideStands(2.20f, 0.75f);
                    // Dual single-leg stands: 2 towers with layered Stone -> Wood -> Armored battlements
                    // Layer 0 (Foundation - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone, 2.20f, -c1, 0);
                    AddDualTowerBlock(objs, ObjectType.Stone, 2.20f,  c1, 0);
                    // Layer 1 (Body - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Box,   2.20f, -c1, 1);
                    AddDualTowerBlock(objs, ObjectType.Box,   2.20f,  c1, 1);
                    // Layer 2 (Parapets - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,  2.20f, -c1, 2);
                    AddDualTowerBlock(objs, ObjectType.Cone,  2.20f,  c1, 2);
                    // Layer 3 (Watch spires - 4 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f, -c1, 3);
                    break;

                case 7:
                    name = "Citadel Colonnade";
                    balls = 29;
                    stands = SingleDeepStand(1.40f);
                    // Classical colonnade with 4 crystal pillars on stone stylobate
                    // Layer 0 (Stylobate - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 0);
                    // Layer 1 (Pillars - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column, c3, 1);
                    // Layer 2 (Entablature - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c3, 2);
                    // Layer 3 (Pediment Crest - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c1, 3);
                    break;

                case 8:
                    name = "The Armored Bunker";
                    balls = 29;
                    stands = SingleDeepStand(1.35f);
                    // Fortified bunker enclosing core reactors
                    // Layer 0 (Heavy base - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (Core Reactor Vault - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (Armored Roof - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    break;

                case 9:
                    name = "Citadel Aqueduct";
                    balls = 30;
                    stands = SingleDeepStand(1.40f);
                    // Elevated aqueduct with triple piers and upper crystal conduit
                    // Layer 0 (Piers - 6 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  o0, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  o2, 0);
                    // Layer 1 (Pier extensions - 6 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  o0, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  o2, 1);
                    // Layer 2 (Spanning Deck - 10 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,    o0, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    o1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    o2, 2);
                    // Layer 3 (Energy Conduit - 10 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, o0, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column, o1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   o2, 3);
                    break;

                case 10:
                    name = "The Royal Citadel Gate";
                    balls = 30;
                    stands = DualSideBySideStands(2.20f, 0.75f);
                    events.Add(new LevelEvent { Type = LevelEventType.UnlockInfinityBall, Data = "InfinityBall unlocked!" });
                    events.Add(new LevelEvent { Type = LevelEventType.PromptUsername, Data = "Prompt Username" });
                    // Monumental dual-stand citadel gate with soaring crystal pinnacles
                    // Layer 0 (Pylons Base - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone,  2.20f, -c1, 0);
                    AddDualTowerBlock(objs, ObjectType.Stone,  2.20f,  c1, 0);
                    // Layer 1 (Mid Pylons - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone,  2.20f, -c1, 1);
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f,  c1, 1);
                    // Layer 2 (Crystal Lanterns - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f, -c1, 2);
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f,  c1, 2);
                    // Layer 3 (Armored Pinnacles - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,   2.20f, -c1, 3);
                    AddDualTowerBlock(objs, ObjectType.Cone,   2.20f,  c1, 3);
                    break;

                // =========================================================================
                // TIER 3: LEVELS 11-15 (The Crystal Spire)
                // =========================================================================
                case 11:
                    name = "The Arcane Vault";
                    balls = 30;
                    stands = SingleDeepStand(1.35f);
                    // Diamond checkerboard of Stone and Crystal enclosing power Cores
                    // Layer 0 (Checkerboard Foundation - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (Vault Chamber - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 1);
                    // Layer 2 (Upper Vault - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 2);
                    // Layer 3 (Apex Vault Crest - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    break;

                case 12:
                    name = "Triple Spire Keep";
                    balls = 31;
                    stands = SingleDeepStand(1.40f);
                    // 3 distinct spires rising from a shared stone terrace
                    // Layer 0 (Shared Terrace - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 0);
                    // Layer 1 (Three Spire Bases - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 1);
                    // Layer 2 (Crystal Shafts - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column, c3, 2);
                    // Layer 3 (Central Apex & Outer Pinnacles - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 3);
                    break;

                case 13:
                    name = "Citadel Tiered Fortress";
                    balls = 31;
                    stands = SingleDeepStand(1.40f);
                    // 3 concentric stepped tiers: 6 -> 4 -> 2 -> crown
                    // Layer 0 (Outer Ring - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 0);
                    // Layer 1 (Middle Tier - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c2, 1);
                    // Layer 2 (Inner Sanctum - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column, c2, 2);
                    // Layer 3 (Fortress Peak - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c1, 3);
                    break;

                case 14:
                    name = "Ironclad Redoubt";
                    balls = 32;
                    stands = SingleDeepStand(1.40f);
                    // Heavy armored bastion with core reactor core
                    // Layer 0 (Solid Footing - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (Energy Cores - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (Armored Deck - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (Watch Battlements - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 3);
                    break;

                case 15:
                    name = "Sanctuary of Sapphire";
                    balls = 32;
                    stands = SingleDeepStand(1.40f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Sapphire Spire content unlocked" });
                    // Cathedral facade with arched stained-glass crystal rosette window
                    // Layer 0 (Plinth - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 0);
                    // Layer 1 (Portal & Buttresses - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 1);
                    // Layer 2 (Rose Window - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column, c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 2);
                    // Layer 3 (Gothic Spires - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 3);
                    break;

                // =========================================================================
                // TIER 4: LEVELS 16-20 (Royal Bastion)
                // =========================================================================
                case 16:
                    name = "Fortress Ramparts";
                    balls = 32;
                    stands = SingleDeepStand(1.45f);
                    // Wide fortified rampart with 4 battlements
                    // Layer 0 (Foundation - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 0);
                    // Layer 1 (Wall - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 1);
                    // Layer 2 (Wall Upper - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,    c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 2);
                    // Layer 3 (Rampart Battlements - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 3);
                    break;

                case 17:
                    name = "Twin Citadel Towers";
                    balls = 33;
                    stands = DualSideBySideStands(2.20f, 0.75f);
                    // Dual single-leg stands: 4-layer high towers with core chambers
                    // Layer 0 (Base - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone,       2.20f, -c1, 0);
                    AddDualTowerBlock(objs, ObjectType.Stone,       2.20f,  c1, 0);
                    // Layer 1 (Core Chamber - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.PaintBottle, 2.20f, -c1, 1);
                    AddDualTowerBlock(objs, ObjectType.Box,         2.20f,  c1, 1);
                    // Layer 2 (Crystal Tower - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column,      2.20f, -c1, 2);
                    AddDualTowerBlock(objs, ObjectType.Column,      2.20f,  c1, 2);
                    // Layer 3 (Crown - 8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f, -c1, 3);
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f,  c1, 3);
                    break;

                case 18:
                    name = "The Obsidian Barricade";
                    balls = 33;
                    stands = SingleDeepStand(1.40f);
                    // Dense interlocking weave of Armored, Stone, and Core
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 3);
                    break;

                case 19:
                    name = "Royal Treasury Vault";
                    balls = 34;
                    stands = SingleDeepStand(1.40f);
                    // Subterranean-style gold/core vault
                    // Layer 0 (Solid Granite Floor - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (Gold & Power Stacks - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (Upper Vault - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (Vault Roof - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 3);
                    break;

                case 20:
                    name = "Grand Citadel Citadel";
                    balls = 34;
                    stands = SingleDeepStand(1.45f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Royal Bastion mastered" });
                    // Multi-tiered imperial palace fortress
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 3);
                    // Layer 4 (Imperial Apex - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 4);
                    break;

                // =========================================================================
                // TIER 5: LEVELS 21-25 (Sky Fortresses)
                // =========================================================================
                case 21:
                    name = "Celestial Fortress";
                    balls = 34;
                    stands = SingleDeepStand(1.45f);
                    // Diamond-faceted sky castle
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 3);
                    break;

                case 22:
                    name = "Twin Sky Spires";
                    balls = 35;
                    stands = DualSideBySideStands(2.20f, 0.75f);
                    // Slender high aerodynamic celestial towers on dual single-leg stands
                    // Layer 0 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone,  2.20f, -c1, 0);
                    AddDualTowerBlock(objs, ObjectType.Stone,  2.20f,  c1, 0);
                    // Layer 1 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f, -c1, 1);
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f,  c1, 1);
                    // Layer 2 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,   2.20f, -c1, 2);
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f,  c1, 2);
                    // Layer 3 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,   2.20f, -c1, 3);
                    AddDualTowerBlock(objs, ObjectType.Cone,   2.20f,  c1, 3);
                    // Layer 4 (Pinnacles - 4 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column, 2.20f,   0f, 4);
                    break;

                case 23:
                    name = "The Aegis Wall";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // Triple-layered defensive bulwark
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 3);
                    break;

                case 24:
                    name = "Citadel Core Reactor";
                    balls = 35;
                    stands = SingleDeepStand(1.45f);
                    // Concentrated reactor core with crystal conduits
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 2);
                    // Layer 3 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 3);
                    break;

                case 25:
                    name = "Imperial Bastion";
                    balls = 36;
                    stands = SingleDeepStand(1.50f);
                    events.Add(new LevelEvent { Type = LevelEventType.NewContentIntroduction, Data = "Imperial Bastion unlocked" });
                    // Imperial castle with 4 towers and central keep (5 layers tall)
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 3);
                    // Layer 4 (Pinnacle Spires - 4 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 4);
                    break;

                // =========================================================================
                // TIER 6: LEVELS 26-30 (The Citadel Throne)
                // =========================================================================
                case 26:
                    name = "The Monarch's Gate";
                    balls = 36;
                    stands = SingleDeepStand(1.50f);
                    // Grand ceremonial triumphal arch
                    // Layer 0 (Plinths - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 0);
                    // Layer 1 (Flank Pylons - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,  c3, 1);
                    // Layer 2 (Pylon Shafts - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 2);
                    // Layer 3 (Grand Arch Spanner - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column, c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c3, 3);
                    // Layer 4 (Attica Crest - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column, c1, 4);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,   c2, 4);
                    break;

                case 27:
                    name = "Twin Colossi Pillars";
                    balls = 37;
                    stands = DualSideBySideStands(2.20f, 0.75f);
                    // Giant monolithic multi-layered pillars on dual single-leg stands
                    // Layer 0 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f, -c1, 0);
                    AddDualTowerBlock(objs, ObjectType.Stone,       2.20f,  c1, 0);
                    // Layer 1 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Stone,       2.20f, -c1, 1);
                    AddDualTowerBlock(objs, ObjectType.Column,      2.20f,  c1, 1);
                    // Layer 2 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Column,      2.20f, -c1, 2);
                    AddDualTowerBlock(objs, ObjectType.PaintBottle, 2.20f,  c1, 2);
                    // Layer 3 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f, -c1, 3);
                    AddDualTowerBlock(objs, ObjectType.Column,      2.20f,  c1, 3);
                    // Layer 4 (8 blocks)
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f, -c1, 4);
                    AddDualTowerBlock(objs, ObjectType.Cone,        2.20f,  c1, 4);
                    break;

                case 28:
                    name = "The Sovereign Keep";
                    balls = 38;
                    stands = SingleDeepStand(1.50f);
                    // 5-story keep with patterned facade
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 3);
                    // Layer 4 (8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 4);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 4);
                    break;

                case 29:
                    name = "Citadel Apex Citadel";
                    balls = 38;
                    stands = SingleDeepStand(1.50f);
                    // Grand sky sanctuary
                    // Layer 0 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 1);
                    // Layer 2 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c3, 2);
                    // Layer 3 (12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c3, 3);
                    // Layer 4 (8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 4);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c2, 4);
                    break;

                case 30:
                    name = "The Royal Throne of Citadel";
                    balls = 40;
                    stands = SingleDeepStand(1.50f);
                    events.Add(new LevelEvent { Type = LevelEventType.FinalLevel, Data = "Citadel Mastered! All 30 Levels Conquered!" });
                    // Epic Throne Chamber: Dais -> Arms & Seat -> Energy Core Heart -> Backrest -> Crown
                    // Layer 0 (Throne Dais - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Stone,       c2, 0);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 0);
                    // Layer 1 (Throne Seat & Armrests - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Box,         c2, 1);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 1);
                    // Layer 2 (Power Core Heart - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.PaintBottle, c1, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 2);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 2);
                    // Layer 3 (Royal Backrest - 12 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c1, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Column,      c2, 3);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 3);
                    // Layer 4 (The Royal Crown & Pinnacles - 8 blocks)
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c1, 4);
                    AddSymmetricThickBlock(objs, ObjectType.Cone,        c3, 4);
                    break;
            }

            data.SetData(id, name, balls, reward, stands, objs.ToArray(), events.ToArray());
        }
    }
}
