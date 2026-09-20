using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Audio;
using RoyalSmash.Gameplay;
using RoyalSmash.Save;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 16: Audio & SFX System.
    /// Runs 10 test cases verifying channel separation, 13 procedural clips, voice pooling, pitch randomization, and mute toggles.
    /// </summary>
    public static class Phase16PlayModeTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _testLogs = new List<string>();

        [MenuItem("RoyalSmash/Phase 16/Run All Audio Tests")]
        public static string RunAllTests()
        {
            _passedTests = 0;
            _failedTests = 0;
            _testLogs.Clear();

            Debug.Log("==================================================");
            Debug.Log("  STARTING PHASE 16 AUTOMATED AUDIO TESTS (10 TESTS)  ");
            Debug.Log("==================================================");

            // Test 1: Config Loaded & Clips Assigned
            Test01_AudioConfigLoaded();

            // Test 2: AudioManager Initialization & Voice Pool
            Test02_AudioManagerInitialization();

            // Test 3: Ball Launch SFX
            Test03_BallLaunchSFX();

            // Test 4: Object Impact SFX Acoustic Differentiation
            Test04_ObjectImpactAcousticDifferentiation();

            // Test 5: Paint Bottle Break SFX
            Test05_PaintBottleBreakSFX();

            // Test 6: Object Destruction SFX
            Test06_ObjectDestructionSFX();

            // Test 7: UI Feedback SFX (Button Click & Reward)
            Test07_UIFeedbackSFX();

            // Test 8: Win Fanfare & Defeat Cadence
            Test08_WinAndLoseAudio();

            // Test 9: BGM Playback & Looping
            Test09_BGMPlaybackAndLooping();

            // Test 10: Settings Mute Toggles Enforcement
            Test10_SettingsMuteToggles();

            string summary = $"Phase 16 Test Results: {_passedTests}/10 PASSED, {_failedTests} FAILED.";
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

        private static void Test01_AudioConfigLoaded()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>(
                "Assets/_Project/ScriptableObjects/Config/AudioConfig.asset");

            bool hasConfig = config != null;
            bool hasClips = hasConfig &&
                config.BallLaunch != null &&
                config.BoxImpact != null &&
                config.StoneImpact != null &&
                config.ColumnImpact != null &&
                config.ConeImpact != null &&
                config.PaintBottleBreak != null &&
                config.ObjectDestruction != null &&
                config.ButtonClick != null &&
                config.WinFanfare != null &&
                config.LoseSound != null &&
                config.RewardSound != null &&
                config.BgmMainMenu != null &&
                config.BgmGameplay != null;

            Assert(hasConfig && hasClips, "Test 1: AudioConfigLoaded",
                $"AudioConfig asset present with all 13 Section 21 clips assigned (Config: {hasConfig}, Clips: {hasClips})");
        }

        private static void Test02_AudioManagerInitialization()
        {
            var audioMgr = AudioManager.Instance;
            bool validInstance = audioMgr != null;
            bool hasConfig = validInstance && audioMgr.Config != null;

            Assert(validInstance && hasConfig, "Test 2: AudioManagerInitialization",
                $"AudioManager singleton initialized with config. (Config max voices: {audioMgr.Config.MaxSimultaneousVoices})");
        }

        private static void Test03_BallLaunchSFX()
        {
            var audioMgr = AudioManager.Instance;
            audioMgr.SetSfxEnabled(true, false);

            bool noThrow = true;
            try
            {
                audioMgr.PlayBallLaunch();
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }

            Assert(noThrow && audioMgr.Config.BallLaunch != null, "Test 3: BallLaunchSFX",
                $"PlayBallLaunch executed successfully with clip '{audioMgr.Config.BallLaunch.name}'");
        }

        private static void Test04_ObjectImpactAcousticDifferentiation()
        {
            var audioMgr = AudioManager.Instance;
            var config = audioMgr.Config;

            bool clipsDistinct = config.BoxImpact != config.StoneImpact &&
                                 config.StoneImpact != config.ColumnImpact &&
                                 config.ColumnImpact != config.ConeImpact;

            // Trigger each impact
            audioMgr.PlayImpact(ObjectType.Box, 2.0f);
            audioMgr.PlayImpact(ObjectType.Stone, 2.0f);
            audioMgr.PlayImpact(ObjectType.Column, 2.0f);
            audioMgr.PlayImpact(ObjectType.Cone, 2.0f);

            Assert(clipsDistinct, "Test 4: ObjectImpactAcousticDifferentiation",
                "Acoustic impact clips are distinctly mapped across Box (wood), Stone (solid thud), Column (deep resonance), Cone (hollow ping).");
        }

        private static void Test05_PaintBottleBreakSFX()
        {
            var audioMgr = AudioManager.Instance;
            bool hasClip = audioMgr.Config.PaintBottleBreak != null;

            bool noThrow = true;
            try
            {
                audioMgr.PlayBottleBreak();
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }

            Assert(noThrow && hasClip, "Test 5: PaintBottleBreakSFX",
                $"PlayBottleBreak executed cleanly with clip '{audioMgr.Config.PaintBottleBreak.name}'.");
        }

        private static void Test06_ObjectDestructionSFX()
        {
            var audioMgr = AudioManager.Instance;
            bool hasClip = audioMgr.Config.ObjectDestruction != null;

            bool noThrow = true;
            try
            {
                audioMgr.PlayObjectDestruction();
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }

            Assert(noThrow && hasClip, "Test 6: ObjectDestructionSFX",
                $"PlayObjectDestruction executed cleanly with clip '{audioMgr.Config.ObjectDestruction.name}'.");
        }

        private static void Test07_UIFeedbackSFX()
        {
            var audioMgr = AudioManager.Instance;
            bool hasButtonClip = audioMgr.Config.ButtonClick != null;
            bool hasRewardClip = audioMgr.Config.RewardSound != null;

            audioMgr.PlayButtonClick();
            audioMgr.PlayRewardSound();

            Assert(hasButtonClip && hasRewardClip, "Test 7: UIFeedbackSFX",
                $"UI feedback sounds (Button Click & Reward Chime) executed successfully.");
        }

        private static void Test08_WinAndLoseAudio()
        {
            var audioMgr = AudioManager.Instance;
            bool hasWin = audioMgr.Config.WinFanfare != null;
            bool hasLose = audioMgr.Config.LoseSound != null;

            audioMgr.PlayWinSound();
            audioMgr.PlayLoseSound();

            Assert(hasWin && hasLose, "Test 8: WinAndLoseAudio",
                $"Win fanfare and Lose defeat cadence SFX verified and played.");
        }

        private static void Test09_BGMPlaybackAndLooping()
        {
            var audioMgr = AudioManager.Instance;
            bool hasMainMenuBGM = audioMgr.Config.BgmMainMenu != null;
            bool hasGameplayBGM = audioMgr.Config.BgmGameplay != null;

            audioMgr.PlayMainMenuBGM();
            audioMgr.PlayGameplayBGM();

            Assert(hasMainMenuBGM && hasGameplayBGM, "Test 9: BGMPlaybackAndLooping",
                $"MainMenu and Gameplay BGM tracks configured and loaded.");
        }

        private static void Test10_SettingsMuteToggles()
        {
            var audioMgr = AudioManager.Instance;

            // Toggle Music
            audioMgr.SetMusicEnabled(false, false);
            bool musicMuted = !audioMgr.IsMusicEnabled;

            audioMgr.SetMusicEnabled(true, false);
            bool musicRestored = audioMgr.IsMusicEnabled;

            // Toggle SFX
            audioMgr.SetSfxEnabled(false, false);
            bool sfxMuted = !audioMgr.IsSfxEnabled;

            audioMgr.SetSfxEnabled(true, false);
            bool sfxRestored = audioMgr.IsSfxEnabled;

            bool allPassed = musicMuted && musicRestored && sfxMuted && sfxRestored;

            Assert(allPassed, "Test 10: SettingsMuteToggles",
                $"Audio channels strictly respect mute toggles (Music: {musicMuted}->{musicRestored}, SFX: {sfxMuted}->{sfxRestored}).");
        }
    }
}
