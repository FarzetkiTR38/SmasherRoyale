using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated verification test suite for Phase 33:
    /// - LevelDesignerWindow integration
    /// - Multi-stand and scaled stand runtime spawning in LevelLoader
    /// - Complete 30-level high-density content validation (14-28 blocks per level)
    /// </summary>
    public static class Phase33LevelDesignerTest
    {
        private const string CITADEL_DB_PATH = "Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset";

        [MenuItem("Tools/Royal Smash/Run Phase 33 QA Test Suite")]
        public static void RunTestSuite()
        {
            var results = new List<string>();
            int passed = 0;
            int total = 5;

            // TEST 1: Database & 30 Levels Block Density
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                if (db == null) throw new Exception("CitadelLevelDatabase.asset not found!");
                if (db.TotalLevels != 30) throw new Exception($"Expected 30 levels, found {db.TotalLevels}");

                int totalBlocks = 0;
                for (int i = 1; i <= 30; i++)
                {
                    var lvl = db.GetLevel(i);
                    if (lvl == null) throw new Exception($"Level {i} is null!");
                    if (lvl.Objects == null || lvl.Objects.Length < 14)
                        throw new Exception($"Level {i} has only {lvl.Objects?.Length} blocks! Minimum is 14.");
                    totalBlocks += lvl.Objects.Length;
                }

                passed++;
                results.Add($"[PASS] Test 1: All 30 Levels Verified! Total blocks: {totalBlocks} (avg: {totalBlocks / 30f:F1} blocks/level, all >= 14).");
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 1: {ex.Message}");
            }

            // TEST 2: Multi-Stand Architecture
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                int[] dualStandLevels = { 6, 10, 13, 17, 22, 24, 27 };
                foreach (int id in dualStandLevels)
                {
                    var lvl = db.GetLevel(id);
                    if (lvl.Stands == null || lvl.Stands.Length != 2)
                        throw new Exception($"Level {id} must have 2 stands! Found: {lvl.Stands?.Length}");
                }

                passed++;
                results.Add($"[PASS] Test 2: Multi-Stand Architecture Verified across Levels {string.Join(", ", dualStandLevels)}!");
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 2: {ex.Message}");
            }

            // TEST 3: Block Variety Coverage (All 5 Types Present)
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                var typeCounts = new Dictionary<ObjectType, int>();
                foreach (ObjectType t in Enum.GetValues(typeof(ObjectType))) typeCounts[t] = 0;

                for (int i = 1; i <= 30; i++)
                {
                    var lvl = db.GetLevel(i);
                    foreach (var o in lvl.Objects) typeCounts[o.Type]++;
                }

                foreach (var kvp in typeCounts)
                {
                    if (kvp.Value == 0) throw new Exception($"ObjectType.{kvp.Key} has 0 occurrences across all levels!");
                }

                passed++;
                results.Add($"[PASS] Test 3: Block Variety Verified! Wood: {typeCounts[ObjectType.Box]}, Stone: {typeCounts[ObjectType.Stone]}, Armored: {typeCounts[ObjectType.Cone]}, Crystal: {typeCounts[ObjectType.Column]}, Core: {typeCounts[ObjectType.PaintBottle]}.");
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 3: {ex.Message}");
            }

            // TEST 4: Milestone Events
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                var l5 = db.GetLevel(5);
                var l10 = db.GetLevel(10);
                var l30 = db.GetLevel(30);

                bool hasMega = Array.Exists(l5.SpecialEvents, e => e.Type == LevelEventType.UnlockMegaBall);
                bool hasInf = Array.Exists(l10.SpecialEvents, e => e.Type == LevelEventType.UnlockInfinityBall);
                bool hasFinal = Array.Exists(l30.SpecialEvents, e => e.Type == LevelEventType.FinalLevel);

                if (!hasMega) throw new Exception("Level 5 missing UnlockMegaBall!");
                if (!hasInf) throw new Exception("Level 10 missing UnlockInfinityBall!");
                if (!hasFinal) throw new Exception("Level 30 missing FinalLevel!");

                passed++;
                results.Add("[PASS] Test 4: Progression & Milestone Events Verified (L5 MegaBall, L10 InfinityBall, L30 FinalLevel).");
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 4: {ex.Message}");
            }

            // TEST 5: LevelLoader Multi-Stand Runtime Spawning
            try
            {
                var loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
                if (loader == null)
                {
                    var go = new GameObject("TestLevelLoader");
                    loader = go.AddComponent<LevelLoader>();
                }

                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                var l6 = db.GetLevel(6); // Dual stands level

                loader.LoadLevel(l6);
                if (loader.SpawnedStands.Count != 2)
                    throw new Exception($"LevelLoader spawned {loader.SpawnedStands.Count} stands for Level 6! Expected 2.");

                if (loader.SpawnedObjects.Count < 18)
                    throw new Exception($"LevelLoader spawned {loader.SpawnedObjects.Count} objects for Level 6! Expected at least 18.");

                loader.ClearLevel();
                if (loader.SpawnedObjects.Count != 0 || loader.SpawnedStands.Count != 0)
                    throw new Exception("ClearLevel did not reset spawned objects or stands!");

                passed++;
                results.Add("[PASS] Test 5: LevelLoader Multi-Stand Runtime Spawning and Cleanup Verified!");
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 5: {ex.Message}");
            }

            Debug.Log($"<b>=== PHASE 33 QA TEST SUITE: {passed}/{total} PASSED ===</b>\n" + string.Join("\n", results));
        }
    }
}
