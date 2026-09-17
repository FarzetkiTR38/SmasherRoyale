using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Haptics;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 18: Haptic Feedback.
    /// Runs 10 test cases verifying Section 23 vibration mapping, intensity scaling, and toggle suppression.
    /// </summary>
    public static class Phase18PlayModeTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _testLogs = new List<string>();

        [MenuItem("RoyalSmash/Phase 18/Run All Haptic Tests")]
        public static string RunAllTests()
        {
            _passedTests = 0;
            _failedTests = 0;
            _testLogs.Clear();

            Debug.Log("==================================================");
            Debug.Log("  STARTING PHASE 18 AUTOMATED HAPTIC TESTS (10 TESTS)  ");
            Debug.Log("==================================================");

            Test01_HapticManagerInitialization();
            Test02_BallFireHaptic();
            Test03_LightImpactHaptic_Box();
            Test04_LightImpactHaptic_Cone();
            Test05_HeavyImpactHaptic_Stone();
            Test06_HeavyImpactHaptic_Column();
            Test07_PaintBottleBreakHaptic();
            Test08_SkillActivationHaptic();
            Test09_WinAndLoseHaptics();
            Test10_VibrationToggleSuppression();

            string summary = $"Phase 18 Test Results: {_passedTests}/10 PASSED, {_failedTests} FAILED.";
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

        private static void Test01_HapticManagerInitialization()
        {
            var mgr = HapticManager.Instance;
            bool valid = mgr != null;
            mgr.SetVibrationEnabled(true, false);

            Assert(valid && mgr.IsVibrationEnabled, "Test 1: HapticManagerInitialization",
                "HapticManager singleton initialized and vibration enabled.");
        }

        private static void Test02_BallFireHaptic()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerFireHaptic();

            bool success = mgr.LastTriggeredHaptic == HapticType.Medium;
            Assert(success, "Test 2: BallFireHaptic",
                $"Ball launch triggered Medium haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test03_LightImpactHaptic_Box()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerImpactHaptic(ObjectType.Box, 1.2f);

            bool success = mgr.LastTriggeredHaptic == HapticType.Light;
            Assert(success, "Test 3: LightImpactHaptic_Box",
                $"Box impact triggered Light haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test04_LightImpactHaptic_Cone()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerImpactHaptic(ObjectType.Cone, 1.0f);

            bool success = mgr.LastTriggeredHaptic == HapticType.Light;
            Assert(success, "Test 4: LightImpactHaptic_Cone",
                $"Cone impact triggered Light haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test05_HeavyImpactHaptic_Stone()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerImpactHaptic(ObjectType.Stone, 1.5f);

            bool success = mgr.LastTriggeredHaptic == HapticType.Heavy;
            Assert(success, "Test 5: HeavyImpactHaptic_Stone",
                $"Stone impact triggered Heavy haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test06_HeavyImpactHaptic_Column()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerImpactHaptic(ObjectType.Column, 1.5f);

            bool success = mgr.LastTriggeredHaptic == HapticType.Heavy;
            Assert(success, "Test 6: HeavyImpactHaptic_Column",
                $"Column impact triggered Heavy haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test07_PaintBottleBreakHaptic()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerBottleBreakHaptic();

            bool success = mgr.LastTriggeredHaptic == HapticType.MediumStrong;
            Assert(success, "Test 7: PaintBottleBreakHaptic",
                $"Paint bottle break triggered Medium-Strong haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test08_SkillActivationHaptic()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerSkillActivationHaptic();

            bool success = mgr.LastTriggeredHaptic == HapticType.Medium;
            Assert(success, "Test 8: SkillActivationHaptic",
                $"Skill activation triggered Medium haptic feedback (Result: {mgr.LastTriggeredHaptic}).");
        }

        private static void Test09_WinAndLoseHaptics()
        {
            var mgr = HapticManager.Instance;
            mgr.TriggerWinHaptic();
            bool winHeavy = mgr.LastTriggeredHaptic == HapticType.Heavy;

            mgr.TriggerLoseHaptic();
            bool loseLight = mgr.LastTriggeredHaptic == HapticType.Light;

            Assert(winHeavy && loseLight, "Test 9: WinAndLoseHaptics",
                $"Win triggered Strong haptic and Lose triggered Light haptic.");
        }

        private static void Test10_VibrationToggleSuppression()
        {
            var mgr = HapticManager.Instance;
            int countBefore = mgr.TotalHapticsTriggered;

            // Turn OFF vibration
            mgr.SetVibrationEnabled(false, false);
            mgr.TriggerFireHaptic();
            mgr.TriggerWinHaptic();
            int countDuringOff = mgr.TotalHapticsTriggered;
            bool blocked = countDuringOff == countBefore;

            // Turn ON vibration
            mgr.SetVibrationEnabled(true, false);
            mgr.TriggerFireHaptic();
            int countAfterOn = mgr.TotalHapticsTriggered;
            bool resumed = countAfterOn > countDuringOff;

            Assert(blocked && resumed, "Test 10: VibrationToggleSuppression",
                $"Vibration toggle suppresses all haptics when OFF and cleanly resumes when ON.");
        }
    }
}
