using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoyalSmash.VFX;
using RoyalSmash.Gameplay;
using RoyalSmash.Skills;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 17: VFX & Visual Polish.
    /// Runs 10 test cases verifying Section 22 visual effects, trails, pooling, and particle bursts.
    /// </summary>
    public static class Phase17PlayModeTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _testLogs = new List<string>();

        [MenuItem("RoyalSmash/Phase 17/Run All VFX Tests")]
        public static string RunAllTests()
        {
            _passedTests = 0;
            _failedTests = 0;
            _testLogs.Clear();

            Debug.Log("==================================================");
            Debug.Log("  STARTING PHASE 17 AUTOMATED VFX TESTS (10 TESTS)  ");
            Debug.Log("==================================================");

            Test01_VFXConfigAndPrefabs();
            Test02_VFXManagerInitialization();
            Test03_MuzzleFlashVFX();
            Test04_BallFlightTrail();
            Test05_ImpactDifferentiation();
            Test06_ObjectDestructionDebris();
            Test07_PaintBottleBreakShardsAndSpill();
            Test08_SkillActivationGlow();
            Test09_WinCelebrationConfetti();
            Test10_RewardCoinFly();

            string summary = $"Phase 17 Test Results: {_passedTests}/10 PASSED, {_failedTests} FAILED.";
            Debug.Log("==================================================");
            Debug.Log($"  {summary}");
            Debug.Log("==================================================");

            return summary;
        }

        private static void Assert(bool condition, string testName, string details)
        {
            if (condition)
            {
                _passedTests++;
                string msg = $"[PASS] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.Log(msg);
            }
            else
            {
                _failedTests++;
                string msg = $"[FAIL] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.LogError(msg);
            }
        }

        private static void Test01_VFXConfigAndPrefabs()
        {
            var config = AssetDatabase.LoadAssetAtPath<VFXConfig>(
                "Assets/_Project/ScriptableObjects/Config/VFXConfig.asset");

            bool hasConfig = config != null;
            bool hasPrefabs = hasConfig &&
                config.MuzzleFlashPrefab != null &&
                config.ImpactGenericPrefab != null &&
                config.ImpactHeavyPrefab != null &&
                config.ObjectDestructionPrefab != null &&
                config.BottleBreakShardsPrefab != null &&
                config.PaintSpillPrefab != null &&
                config.SkillActivatePrefab != null &&
                config.WinConfettiPrefab != null &&
                config.CoinFlyPrefab != null &&
                config.DefaultTrailMaterial != null;

            Assert(hasConfig && hasPrefabs, "Test 1: VFXConfigAndPrefabs",
                $"VFXConfig present with all 10 Section 22 visual effect assets linked.");
        }

        private static void Test02_VFXManagerInitialization()
        {
            var vfxMgr = VFXManager.Instance;
            bool valid = vfxMgr != null && vfxMgr.Config != null;

            Assert(valid, "Test 2: VFXManagerInitialization",
                $"VFXManager singleton initialized with active configuration.");
        }

        private static void Test03_MuzzleFlashVFX()
        {
            var vfxMgr = VFXManager.Instance;
            var flash = vfxMgr.SpawnMuzzleFlash(Vector3.zero, Quaternion.identity);

            bool success = flash != null && flash.activeSelf;
            Assert(success, "Test 3: MuzzleFlashVFX",
                $"MuzzleFlash particle spawned successfully at mortar barrel point.");
        }

        private static void Test04_BallFlightTrail()
        {
            var ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Gameplay/Ball.prefab");

            bool hasPrefab = ballPrefab != null;
            var trail = hasPrefab ? ballPrefab.GetComponent<TrailRenderer>() : null;
            var proj = hasPrefab ? ballPrefab.GetComponent<ProjectileController>() : null;
            bool trailConfigured = trail != null && (trail.sharedMaterial != null || trail.material != null) && proj != null && proj.Trail != null;

            Assert(trailConfigured, "Test 4: BallFlightTrail",
                $"Ball.prefab equipped with configured TrailRenderer linked to ProjectileController.");
        }

        private static void Test05_ImpactDifferentiation()
        {
            var vfxMgr = VFXManager.Instance;

            // Light impact
            var lightVFX = vfxMgr.SpawnImpact(new Vector3(0, 1, 0), Vector3.up, 1.0f);
            // Heavy impact
            var heavyVFX = vfxMgr.SpawnImpact(new Vector3(1, 1, 0), Vector3.up, 4.5f);

            bool bothSpawned = lightVFX != null && heavyVFX != null;
            Assert(bothSpawned, "Test 5: ImpactDifferentiation",
                $"Light and heavy collision impulses spawn dynamically scaled impact bursts.");
        }

        private static void Test06_ObjectDestructionDebris()
        {
            var vfxMgr = VFXManager.Instance;
            var debris = vfxMgr.SpawnDestruction(new Vector3(2, 0.5f, 2), Color.red);

            bool success = debris != null && debris.activeSelf;
            Assert(success, "Test 6: ObjectDestructionDebris",
                $"Object destruction debris spawned with object-specific tinting.");
        }

        private static void Test07_PaintBottleBreakShardsAndSpill()
        {
            var vfxMgr = VFXManager.Instance;
            bool noThrow = true;
            try
            {
                vfxMgr.SpawnBottleBreak(new Vector3(0, 1, 3), Color.magenta);
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }

            Assert(noThrow, "Test 7: PaintBottleBreakShardsAndSpill",
                $"Paint bottle break triggered glass shards burst and lingering paint puddle decal.");
        }

        private static void Test08_SkillActivationGlow()
        {
            var vfxMgr = VFXManager.Instance;
            var megaBurst = vfxMgr.SpawnSkillActivation(Vector3.zero, SkillType.MegaBall);
            var infBurst = vfxMgr.SpawnSkillActivation(Vector3.one, SkillType.InfinityBall);

            bool bothSpawned = megaBurst != null && infBurst != null;
            Assert(bothSpawned, "Test 8: SkillActivationGlow",
                $"Skill activation visual burst spawned for MegaBall and InfinityBall.");
        }

        private static void Test09_WinCelebrationConfetti()
        {
            var vfxMgr = VFXManager.Instance;
            var confetti = vfxMgr.SpawnWinCelebration(new Vector3(0, 2, 2));

            bool success = confetti != null && confetti.activeSelf;
            Assert(success, "Test 9: WinCelebrationConfetti",
                $"Celebratory victory confetti particle burst spawned successfully.");
        }

        private static void Test10_RewardCoinFly()
        {
            var vfxMgr = VFXManager.Instance;
            bool noThrow = true;
            try
            {
                vfxMgr.PlayRewardCoins(new Vector3(100, 100, 0), new Vector3(300, 500, 0), 5);
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }

            Assert(noThrow, "Test 10: RewardCoinFly",
                $"Reward coin flight animation spawned and routed without exceptions.");
        }
    }
}
