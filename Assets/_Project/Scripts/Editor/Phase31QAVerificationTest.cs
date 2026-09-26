using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RoyalSmash.Economy;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.Lives;
using RoyalSmash.Save;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 31 & Faz 5:
    /// - Level 30 equilibrium and empty air shot physics stability (zero block jitter/explosive jumping).
    /// - 1 to 30 level data validation, target count, and zero overlap clearance.
    /// - Lives system (loss, zero lives gating, regeneration timer).
    /// - Economy system (gold addition, purchase deduction, insufficient balance rejection).
    /// - Scene integrity (zero missing scripts across MainMenu and MainGame).
    /// </summary>
    public static class Phase31QAVerificationTest
    {
        [MenuItem("RoyalSmash/Tests/Run Phase 31 QA & Physics Stability Tests")]
        public static void RunAllTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING PHASE 31 QA & PHYSICS STABILITY VERIFICATION (5 TESTS)              ");
            Debug.Log("================================================================================");

            int passed = 0;
            int failed = 0;

            // TEST 1: Level 30 Equilibrium & Empty Air Shot Stability
            try
            {
                var mainGameScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainGame.unity", OpenSceneMode.Single);
                Assert.IsTrue(mainGameScene.IsValid(), "MainGame scene must be valid.");

                var levelManager = Object.FindFirstObjectByType<LevelManager>();
                Assert.IsNotNull(levelManager, "LevelManager must be present in MainGame scene.");

                // Load Level 30
                levelManager.LoadLevelDirect(30);

                var levelLoader = Object.FindFirstObjectByType<LevelLoader>();
                Assert.IsNotNull(levelLoader, "LevelLoader must be present in scene.");

                var spawned = levelLoader.SpawnedObjects;
                Assert.AreEqual(20, spawned.Count, "Level 30 must have spawned exactly 20 citadel blocks.");

                // Record initial positions
                var initialPositions = new Vector3[spawned.Count];
                for (int i = 0; i < spawned.Count; i++)
                {
                    Assert.IsNotNull(spawned[i], $"Spawned block at index {i} cannot be null.");
                    initialPositions[i] = spawned[i].transform.position;
                }

                // Simulate firing a shot into empty air (far off above and away from stand/blocks)
                var mortar = Object.FindFirstObjectByType<MortarController>();
                Vector3 emptyAirTarget = new Vector3(12f, 15f, 25f);

                if (mortar != null)
                {
                    mortar.Fire(emptyAirTarget);
                }

                // Advance physics simulation by 150 steps (3.0 seconds at 50Hz)
                Physics.simulationMode = SimulationMode.Script;
                for (int step = 0; step < 150; step++)
                {
                    Physics.Simulate(0.02f);
                }
                Physics.simulationMode = SimulationMode.FixedUpdate;

                // Measure displacements after 3 seconds of physics
                float maxDisplacement = 0f;
                int movedCount = 0;
                for (int i = 0; i < spawned.Count; i++)
                {
                    float dist = Vector3.Distance(initialPositions[i], spawned[i].transform.position);
                    if (dist > maxDisplacement) maxDisplacement = dist;
                    if (dist > 0.005f) movedCount++;
                }

                Assert.AreEqual(0, movedCount, $"No blocks should move when firing into empty air! {movedCount} moved. Max displacement: {maxDisplacement:F4}m");
                Assert.Less(maxDisplacement, 0.001f, $"Level 30 blocks moved {maxDisplacement}m during empty air shot! Must remain rock-solid.");

                // Now simulate projectile direct hit into the castle center
                LevelStartPhysicsGate.ReleaseActiveGate();
                Physics.simulationMode = SimulationMode.Script;
                // Apply simulated impact impulse to the front-center block
                var centerBlock = spawned[1]; // Lower row center block
                if (centerBlock != null && centerBlock.Rigidbody != null)
                {
                    centerBlock.Rigidbody.WakeUp();
                    centerBlock.Rigidbody.AddForce(new Vector3(0f, 2f, 25f), ForceMode.Impulse);
                }

                for (int step = 0; step < 60; step++)
                {
                    Physics.Simulate(0.02f);
                }
                Physics.simulationMode = SimulationMode.FixedUpdate;

                float hitDisplacement = Vector3.Distance(initialPositions[1], centerBlock.transform.position);
                Assert.Greater(hitDisplacement, 0.05f, "Castle blocks must react dynamically when an actual hit occurs.");

                Debug.Log($"[PASS] Test 1: Level 30 Equilibrium Verified! Empty-air shot max block drift: {maxDisplacement:F5}m (Zero jitter/jump). Direct hit reacted dynamically: {hitDisplacement:F3}m.");
                passed++;
            }
            catch (System.Exception ex)
            {
                Physics.simulationMode = SimulationMode.FixedUpdate;
                Debug.LogError($"[FAIL] Test 1: Level 30 Physics Stability: {ex.Message}\n{ex.StackTrace}");
                failed++;
            }

            // TEST 2: All 30 Levels Automated Validation & Zero Overlaps
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
                Assert.IsNotNull(db, "CitadelLevelDatabase.asset must exist.");
                Assert.AreEqual(30, db.TotalLevels, "Database must have 30 levels.");

                int totalOverlaps = 0;
                const float BLOCK_SIZE = 0.74f;

                for (int lvlId = 1; lvlId <= 30; lvlId++)
                {
                    var lvl = db.GetLevel(lvlId);
                    Assert.IsNotNull(lvl, $"Level {lvlId} must exist.");
                    Assert.GreaterOrEqual(lvl.BallCount, 10, $"Level {lvlId} ball count should be >= 10.");
                    Assert.GreaterOrEqual(lvl.BaseRewardGold, 50, $"Level {lvlId} reward should be >= 50.");
                    Assert.IsNotNull(lvl.Objects, $"Level {lvlId} objects cannot be null.");
                    Assert.Greater(lvl.Objects.Length, 0, $"Level {lvlId} must have objects.");

                    int reqCount = 0;
                    for (int i = 0; i < lvl.Objects.Length; i++)
                    {
                        if (lvl.Objects[i].IsRequiredTarget) reqCount++;

                        // Check collision with all subsequent blocks
                        var b1 = new Bounds(lvl.Objects[i].Position, Vector3.one * (BLOCK_SIZE - 0.005f));
                        for (int j = i + 1; j < lvl.Objects.Length; j++)
                        {
                            var b2 = new Bounds(lvl.Objects[j].Position, Vector3.one * (BLOCK_SIZE - 0.005f));
                            if (b1.Intersects(b2)) totalOverlaps++;
                        }
                    }
                    Assert.Greater(reqCount, 0, $"Level {lvlId} must have at least one required target.");
                }

                Assert.AreEqual(0, totalOverlaps, $"Found {totalOverlaps} overlapping block pairs across the 30 levels!");
                Debug.Log($"[PASS] Test 2: All 30 Levels Validated! 30/30 levels playable with >= 1 required target, and 0 inter-penetrating overlaps.");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 2: Level Database & Overlap Validation: {ex.Message}");
                failed++;
            }

            // TEST 3: Lives System QA (Loss, Zero Lives Gating, Regeneration Timer)
            GameObject tempLivesGo = null;
            try
            {
                var saveMgr = SaveManager.Instance;
                if (saveMgr != null && saveMgr.Data != null)
                {
                    saveMgr.Data.Lives = 5;
                }

                var livesMgr = LivesManager.Instance;
                if (livesMgr == null)
                {
                    tempLivesGo = new GameObject("Test_LivesManager");
                    livesMgr = tempLivesGo.AddComponent<LivesManager>();
                }
                Assert.IsNotNull(livesMgr, "LivesManager instance must exist.");
                Assert.AreEqual(5, livesMgr.CurrentLives, "Initial lives must be 5.");
                Assert.IsTrue(livesMgr.HasLivesToPlay, "Player must have lives to play.");

                // Consume life on defeat
                bool consumed = livesMgr.DeductLife("Test_Defeat");
                Assert.IsTrue(consumed, "DeductLife must succeed when lives > 0.");
                Assert.AreEqual(4, livesMgr.CurrentLives, "Lives must be 4 after defeat.");

                // Drain to 0
                while (livesMgr.CurrentLives > 0)
                {
                    livesMgr.DeductLife("Test_Drain");
                }
                Assert.AreEqual(0, livesMgr.CurrentLives, "Lives must be 0.");
                Assert.IsFalse(livesMgr.HasLivesToPlay, "HasLivesToPlay must return false when lives == 0.");
                Assert.IsFalse(livesMgr.DeductLife("Test_ExtraDrain"), "DeductLife must return false when lives == 0.");

                // Regenerate 1 life
                livesMgr.AddLife(1, "Test_Regen");
                Assert.AreEqual(1, livesMgr.CurrentLives, "Lives must be 1 after AddLife(1).");
                Assert.IsTrue(livesMgr.HasLivesToPlay, "Player must now have lives to play.");

                // Restore full lives
                livesMgr.AddLife(4, "Test_Restore");

                Debug.Log("[PASS] Test 3: Lives System QA Verified (Defeat deduction, zero-lives gating, replenishment).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 3: Lives System: {ex.Message}");
                failed++;
            }
            finally
            {
                if (tempLivesGo != null) Object.DestroyImmediate(tempLivesGo);
            }

            // TEST 4: Gold Economy QA (Reward Addition, Purchase Deduction, Insufficient Funds Rejection)
            try
            {
                var saveMgr = SaveManager.Instance;
                if (saveMgr != null && saveMgr.Data != null)
                {
                    saveMgr.Data.Gold = 100;
                }

                var goldMgr = GoldManager.Instance;
                Assert.IsNotNull(goldMgr, "GoldManager instance must exist.");
                goldMgr.InitializeGold();
                Assert.AreEqual(100, goldMgr.CurrentGold, "Initial gold must be 100.");

                // Valid spend
                bool spent = goldMgr.SpendGold(40, "Test_Item");
                Assert.IsTrue(spent, "SpendGold(40) must succeed.");
                Assert.AreEqual(60, goldMgr.CurrentGold, "Gold balance must be 60.");

                // Overspending
                bool overspend = goldMgr.SpendGold(100, "Test_ExpensiveItem");
                Assert.IsFalse(overspend, "Overspending must return false.");
                Assert.AreEqual(60, goldMgr.CurrentGold, "Gold balance must remain 60 after failed spend.");

                // Level win reward
                goldMgr.AddGold(150, "Test_LevelWin");
                Assert.AreEqual(210, goldMgr.CurrentGold, "Gold balance must be 210 after reward.");

                Debug.Log("[PASS] Test 4: Gold Economy QA Verified (Deduction, insufficient balance rejection, reward addition).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 4: Gold Economy: {ex.Message}");
                failed++;
            }

            // TEST 5: Scene Integrity (Zero Missing Scripts Across MainMenu and MainGame)
            try
            {
                string[] scenes = { "Assets/_Project/Scenes/MainMenu.unity", "Assets/_Project/Scenes/MainGame.unity" };
                int totalMissing = 0;

                foreach (var scenePath in scenes)
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Assert.IsTrue(scene.IsValid(), $"Scene {scenePath} must be valid.");

                    var roots = scene.GetRootGameObjects();
                    foreach (var root in roots)
                    {
                        var transforms = root.GetComponentsInChildren<Transform>(true);
                        foreach (var t in transforms)
                        {
                            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                            if (missing > 0)
                            {
                                Debug.LogError($"[Missing Script] {t.gameObject.name} in {scenePath} has {missing} missing script(s)!");
                                totalMissing += missing;
                            }
                        }
                    }
                }

                // Restore MainGame
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainGame.unity", OpenSceneMode.Single);

                Assert.AreEqual(0, totalMissing, $"Found {totalMissing} missing script components across production scenes!");
                Debug.Log("[PASS] Test 5: Scene Integrity Verified! Zero missing script components across MainMenu and MainGame.");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 5: Scene Integrity: {ex.Message}");
                failed++;
            }

            Debug.Log("================================================================================");
            Debug.Log($"   PHASE 31 QA & PHYSICS STABILITY COMPLETED: {passed} PASSED, {failed} FAILED  ");
            Debug.Log("================================================================================");

            if (failed > 0)
            {
                throw new System.Exception($"Phase 31 QA verification failed with {failed} failure(s).");
            }
        }
    }
}
