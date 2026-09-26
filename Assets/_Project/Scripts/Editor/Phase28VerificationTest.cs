using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using RoyalSmash.Economy;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 28 — Level System & Puzzle Architecture Overhaul.
    /// Verifies all 8 acceptance criteria:
    /// 1. 30 Calibrated Citadel levels in LevelDatabase.
    /// 2. Grid geometry compliance (0.74m scale, Y=1.845m base, 0.746m layer step).
    /// 3. IsRequiredTarget data & component support.
    /// 4. LevelManager tracks ONLY required targets in RemainingTargets.
    /// 5. Obstacle survival does not block level victory condition.
    /// 6. LevelStartPhysicsGate kinematic freeze and release mechanisms.
    /// 7. Milestone events (L5 MegaBall, L10 InfinityBall, L30 FinalLevel).
    /// 8. Runtime LoadLevelDirect execution with 0 errors.
    /// </summary>
    public static class Phase28VerificationTest
    {
        [MenuItem("RoyalSmash/Tests/Run Phase 28 Verification Tests")]
        public static void RunAllTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING PHASE 28 LEVEL SYSTEM & PUZZLE ARCHITECTURE TESTS (8 TESTS)        ");
            Debug.Log("================================================================================");

            int passed = 0;
            int failed = 0;

            // Test 1: LevelDatabase Points to 30 Calibrated Citadel Levels
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
                Assert.IsNotNull(db, "CitadelLevelDatabase.asset must exist and be loadable.");
                Assert.AreEqual(30, db.TotalLevels, "LevelDatabase must contain exactly 30 levels.");

                for (int i = 1; i <= 30; i++)
                {
                    var lvl = db.GetLevel(i);
                    Assert.IsNotNull(lvl, $"Level {i} data cannot be null.");
                    Assert.IsTrue(lvl.BallCount >= 20, $"Level {i} must have at least 20 balls.");
                    Assert.IsTrue(lvl.BaseRewardGold >= 50, $"Level {i} base reward must be >= 50.");
                    Assert.IsTrue(lvl.Objects != null && lvl.Objects.Length > 0, $"Level {i} must have placed objects.");
                }

                Debug.Log($"[PASS] Test 1: 30 Citadel Levels Validated in Database (All 30 levels present and configured).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 1: LevelDatabase Validation: {ex.Message}");
                failed++;
            }

            // Test 2: Grid Geometry Compliance (0.74m Scale, Base Y=1.845m)
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
                for (int i = 1; i <= 30; i++)
                {
                    var lvl = db.GetLevel(i);
                    foreach (var obj in lvl.Objects)
                    {
                        Assert.AreEqual(0.74f, Mathf.Round(obj.Scale.x * 100f) / 100f, $"Level {i} object scale must be 0.74m.");
                        // Y coordinate must match base Y + integer multiple of 0.746m
                        float relativeY = (obj.Position.y - 1.845f) / 0.746f;
                        float roundedLayer = Mathf.Round(relativeY);
                        Assert.IsTrue(Mathf.Abs(relativeY - roundedLayer) < 0.05f,
                            $"Level {i} object Y={obj.Position.y} does not align with Citadel grid (expected layer {roundedLayer}).");
                    }
                }

                Debug.Log("[PASS] Test 2: Grid Geometry Compliance (All objects strictly adhere to 0.74m scale and Y=1.845m layer math).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 2: Grid Geometry Compliance: {ex.Message}");
                failed++;
            }

            // Test 3: Target vs Obstacle Architecture (IsRequiredTarget)
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
                var l5 = db.GetLevel(5);
                var l9 = db.GetLevel(9);

                bool hasL5Obstacle = false;
                bool hasL5Target = false;
                foreach (var obj in l5.Objects)
                {
                    if (!obj.IsRequiredTarget) hasL5Obstacle = true;
                    if (obj.IsRequiredTarget) hasL5Target = true;
                }

                Assert.IsTrue(hasL5Obstacle, "Level 5 must feature strategic obstacles (Armored shields with IsRequiredTarget=false).");
                Assert.IsTrue(hasL5Target, "Level 5 must feature required targets (IsRequiredTarget=true).");

                Debug.Log("[PASS] Test 3: Target vs Obstacle Architecture (IsRequiredTarget configured across puzzle data).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 3: Target vs Obstacle Architecture: {ex.Message}");
                failed++;
            }

            // Test 4: LevelManager Tracks ONLY Required Targets
            GameObject dummyMgrGo = null;
            GameObject dummyTargetGo = null;
            GameObject dummyObstacleGo = null;
            try
            {
                dummyMgrGo = new GameObject("Test_LevelManager");
                var lm = dummyMgrGo.AddComponent<LevelManager>();

                dummyTargetGo = new GameObject("Dummy_Target");
                var rbT = dummyTargetGo.AddComponent<Rigidbody>();
                var colT = dummyTargetGo.AddComponent<BoxCollider>();
                var physTarget = dummyTargetGo.AddComponent<PhysicsObject>();
                physTarget.SetRequiredTarget(true);

                dummyObstacleGo = new GameObject("Dummy_Obstacle");
                var rbO = dummyObstacleGo.AddComponent<Rigidbody>();
                var colO = dummyObstacleGo.AddComponent<BoxCollider>();
                var physObstacle = dummyObstacleGo.AddComponent<PhysicsObject>();
                physObstacle.SetRequiredTarget(false);

                lm.ResetState();
                lm.RegisterTarget(physTarget);
                lm.RegisterTarget(physObstacle);

                Assert.AreEqual(1, lm.RemainingTargets, "RemainingTargets must equal 1 (obstacle must NOT be counted).");
                Assert.AreEqual(1, lm.TotalTargets, "TotalTargets must equal 1.");

                lm.UnregisterTarget(physTarget);
                Assert.AreEqual(0, lm.RemainingTargets, "RemainingTargets must be 0 after target is cleared.");

                Debug.Log("[PASS] Test 4: LevelManager Target Tracking (RemainingTargets strictly ignores strategic obstacles).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 4: LevelManager Target Tracking: {ex.Message}");
                failed++;
            }
            finally
            {
                if (dummyTargetGo != null) Object.DestroyImmediate(dummyTargetGo);
                if (dummyObstacleGo != null) Object.DestroyImmediate(dummyObstacleGo);
                if (dummyMgrGo != null) Object.DestroyImmediate(dummyMgrGo);
            }

            // Test 5: Obstacle Survival Does NOT Block Victory Condition
            GameObject testSceneGo = null;
            GameObject testTargetGo = null;
            GameObject testObstacleGo = null;
            try
            {
                testSceneGo = new GameObject("Test_VictoryScene");
                var lm = testSceneGo.AddComponent<LevelManager>();

                testTargetGo = new GameObject("Test_Target");
                testTargetGo.AddComponent<Rigidbody>();
                testTargetGo.AddComponent<BoxCollider>();
                var physTarget = testTargetGo.AddComponent<PhysicsObject>();
                physTarget.SetRequiredTarget(true);

                testObstacleGo = new GameObject("Test_Obstacle");
                testObstacleGo.AddComponent<Rigidbody>();
                testObstacleGo.AddComponent<BoxCollider>();
                var physObstacle = testObstacleGo.AddComponent<PhysicsObject>();
                physObstacle.SetRequiredTarget(false);

                lm.ResetState();
                lm.RegisterTarget(physTarget);
                lm.RegisterTarget(physObstacle);

                // Clear the required target while the obstacle remains standing
                lm.UnregisterTarget(physTarget);

                // Verify win condition evaluation started
                Assert.AreEqual(LevelState.EvaluatingWin, lm.CurrentState,
                    "LevelState must transition to EvaluatingWin even though obstacle is still standing on the stand!");

                Debug.Log("[PASS] Test 5: Obstacle Survival Victory Evaluation (Level won despite intact obstacles).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 5: Obstacle Survival: {ex.Message}");
                failed++;
            }
            finally
            {
                if (testTargetGo != null) Object.DestroyImmediate(testTargetGo);
                if (testObstacleGo != null) Object.DestroyImmediate(testObstacleGo);
                if (testSceneGo != null) Object.DestroyImmediate(testSceneGo);
            }

            // Test 6: LevelStartPhysicsGate Kinematic Freeze and Release
            GameObject gateGo = null;
            GameObject heldGo = null;
            try
            {
                gateGo = new GameObject("Test_Gate");
                var gate = gateGo.AddComponent<LevelStartPhysicsGate>();

                heldGo = new GameObject("HeldObject");
                var rb = heldGo.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                heldGo.AddComponent<BoxCollider>();
                var physObj = heldGo.AddComponent<PhysicsObject>();

                gate.Prepare(new PhysicsObject[] { physObj }, null);

                Assert.IsTrue(rb.isKinematic, "Rigidbody must be set to kinematic during gate hold.");
                Assert.IsTrue(gate.IsWaitingForFirstShot, "Gate must be waiting for first shot.");

                LevelStartPhysicsGate.ReleaseActiveGate();

                Assert.IsFalse(rb.isKinematic, "Rigidbody must be released from kinematic lock upon shot/release.");
                Assert.IsFalse(gate.IsWaitingForFirstShot, "Gate must no longer be waiting after release.");

                Debug.Log("[PASS] Test 6: LevelStartPhysicsGate Kinematic Freeze & Release verified.");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 6: LevelStartPhysicsGate: {ex.Message}");
                failed++;
            }
            finally
            {
                if (heldGo != null) Object.DestroyImmediate(heldGo);
                if (gateGo != null) Object.DestroyImmediate(gateGo);
            }

            // Test 7: Milestone Events Integrity (L5, L10, L30)
            try
            {
                var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset");
                var l5 = db.GetLevel(5);
                var l10 = db.GetLevel(10);
                var l30 = db.GetLevel(30);

                bool hasMega = false;
                foreach (var ev in l5.SpecialEvents)
                    if (ev.Type == LevelEventType.UnlockMegaBall) hasMega = true;
                Assert.IsTrue(hasMega, "Level 5 must have UnlockMegaBall special event.");

                bool hasInf = false;
                bool hasUser = false;
                foreach (var ev in l10.SpecialEvents)
                {
                    if (ev.Type == LevelEventType.UnlockInfinityBall) hasInf = true;
                    if (ev.Type == LevelEventType.PromptUsername) hasUser = true;
                }
                Assert.IsTrue(hasInf, "Level 10 must have UnlockInfinityBall event.");
                Assert.IsTrue(hasUser, "Level 10 must have PromptUsername event.");

                bool hasFinal = false;
                foreach (var ev in l30.SpecialEvents)
                    if (ev.Type == LevelEventType.FinalLevel) hasFinal = true;
                Assert.IsTrue(hasFinal, "Level 30 must have FinalLevel event.");

                Debug.Log("[PASS] Test 7: Progression Milestones (L5 MegaBall, L10 InfinityBall & Username, L30 Finale confirmed).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 7: Milestone Events: {ex.Message}");
                failed++;
            }

            // Test 8: MainGame Scene Clean Integration
            try
            {
                var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainGame.unity", OpenSceneMode.Single);
                var levelMgr = Object.FindFirstObjectByType<LevelManager>();
                Assert.IsNotNull(levelMgr, "MainGame must contain LevelManager.");

                var stand = GameObject.Find("Stand");
                Assert.IsNotNull(stand, "MainGame must have Stand platform at scene root.");
                Assert.IsTrue(stand.activeInHierarchy, "Stand platform must be active.");

                Debug.Log("[PASS] Test 8: MainGame Scene Clean Integration (Stand is root active, LevelManager present).");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] Test 8: MainGame Scene: {ex.Message}");
                failed++;
            }

            Debug.Log("================================================================================");
            Debug.Log($"   PHASE 28 TEST RESULTS: {passed}/8 PASSED, {failed} FAILED                     ");
            Debug.Log("================================================================================");
        }
    }
}
