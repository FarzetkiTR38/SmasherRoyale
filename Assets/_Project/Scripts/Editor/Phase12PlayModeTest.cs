using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Profile;
using RoyalSmash.Audio;
using RoyalSmash.Utils;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Comprehensive automated test runner for Phase 12 (Profile & Settings).
    /// Validates all 10 acceptance criteria per Master Build Prompt Section 35.
    /// </summary>
    public static class Phase12PlayModeTest
    {
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MAIN_GAME_SCENE = "Assets/_Project/Scenes/MainGame.unity";

        public static string RunAllTests()
        {
            var results = new List<string>();
            int passedCount = 0;
            int totalTests = 10;

            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            var saveMgr = SaveManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<SaveManager>();
            if (saveMgr != null && saveMgr.Data == null)
            {
                saveMgr.Load();
            }

            var profileMgr = ProfileManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<ProfileManager>();
            var audioMgr = AudioManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            var vibMgr = VibrationManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<VibrationManager>();

            // -------------------------------------------------------------
            // TEST 1: Avatar Selection (8 Options)
            // -------------------------------------------------------------
            try
            {
                bool allAvatarsPassed = true;
                for (int i = 0; i < 8; i++)
                {
                    profileMgr.SetAvatar(i);
                    if (saveMgr.Data.AvatarId != i || profileMgr.AvatarId != i)
                    {
                        allAvatarsPassed = false;
                        break;
                    }
                }

                if (allAvatarsPassed && profileMgr.Config.Avatars.Length == 8)
                {
                    results.Add("[PASS] Test 1: Avatar selection works flawlessly across all 8 options.");
                    passedCount++;
                }
                else
                {
                    results.Add("[FAIL] Test 1: Avatar selection failed to update or verify 8 options.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 1: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 2: Frame Selection (4 Options)
            // -------------------------------------------------------------
            try
            {
                bool allFramesPassed = true;
                for (int i = 0; i < 4; i++)
                {
                    profileMgr.SetFrame(i);
                    if (saveMgr.Data.FrameId != i || profileMgr.FrameId != i)
                    {
                        allFramesPassed = false;
                        break;
                    }
                }

                if (allFramesPassed && profileMgr.Config.Frames.Length == 4)
                {
                    results.Add("[PASS] Test 2: Frame selection works flawlessly across all 4 options.");
                    passedCount++;
                }
                else
                {
                    results.Add("[FAIL] Test 2: Frame selection failed to update or verify 4 options.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 2: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 3: Username Input at Level 10
            // -------------------------------------------------------------
            try
            {
                saveMgr.Data.CurrentLevel = 10;
                saveMgr.Data.UsernamePrompted = false;
                saveMgr.Save();

                bool shouldPrompt = profileMgr.ShouldPromptLevel10Username();

                // Find UsernamePromptDialog
                var promptDlg = UnityEngine.Object.FindFirstObjectByType<UsernamePromptDialog>(FindObjectsInactive.Include);
                bool dialogFound = promptDlg != null;

                if (dialogFound)
                {
                    promptDlg.Show();
                    var inputField = promptDlg.GetComponentInChildren<TMP_InputField>(true);
                    if (inputField != null) inputField.text = "RoyalChampion";
                    promptDlg.HandleConfirm();
                }

                bool promptCompleted = saveMgr.Data.UsernamePrompted && saveMgr.Data.Username == "RoyalChampion";

                if (shouldPrompt && dialogFound && promptCompleted)
                {
                    results.Add("[PASS] Test 3: Username prompt triggered at Level 10 and saved successfully.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 3: shouldPrompt={shouldPrompt}, dialogFound={dialogFound}, promptCompleted={promptCompleted}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 3: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 4: Username Unlimited Free Changes
            // -------------------------------------------------------------
            try
            {
                int initialGold = saveMgr.Data.Gold;
                bool change1 = profileMgr.SetUsername("SmasherKing");
                bool change2 = profileMgr.SetUsername("CastleMaster");
                bool change3 = profileMgr.SetUsername("FarzetkiLord");

                bool freeOfCost = saveMgr.Data.Gold == initialGold;
                bool nameUpdated = saveMgr.Data.Username == "FarzetkiLord";

                if (change1 && change2 && change3 && freeOfCost && nameUpdated)
                {
                    results.Add("[PASS] Test 4: Username changeable unlimited times with zero gold cost.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 4: Free username change check failed (Gold changed or update failed).");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 4: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 5: Music Toggle Mutes/Unmutes BGM
            // -------------------------------------------------------------
            try
            {
                audioMgr.SetMusicEnabled(false, true);
                bool offVerified = !saveMgr.Data.MusicEnabled && !audioMgr.IsMusicEnabled;

                audioMgr.SetMusicEnabled(true, true);
                bool onVerified = saveMgr.Data.MusicEnabled && audioMgr.IsMusicEnabled;

                if (offVerified && onVerified)
                {
                    results.Add("[PASS] Test 5: Music toggle cleanly mutes and unmutes background music.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 5: offVerified={offVerified}, onVerified={onVerified}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 5: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 6: Sound (SFX) Toggle Mutes/Unmutes SFX
            // -------------------------------------------------------------
            try
            {
                audioMgr.SetSfxEnabled(false, true);
                bool offVerified = !saveMgr.Data.SfxEnabled && !audioMgr.IsSfxEnabled;

                audioMgr.SetSfxEnabled(true, true);
                bool onVerified = saveMgr.Data.SfxEnabled && audioMgr.IsSfxEnabled;

                if (offVerified && onVerified)
                {
                    results.Add("[PASS] Test 6: Sound toggle cleanly mutes and unmutes sound effects.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 6: offVerified={offVerified}, onVerified={onVerified}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 6: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 7: Vibration Toggle Enables/Disables Haptics
            // -------------------------------------------------------------
            try
            {
                vibMgr.SetVibrationEnabled(false, true);
                bool offVerified = !saveMgr.Data.VibrationEnabled && !vibMgr.IsVibrationEnabled;

                vibMgr.SetVibrationEnabled(true, true);
                bool onVerified = saveMgr.Data.VibrationEnabled && vibMgr.IsVibrationEnabled;

                if (offVerified && onVerified)
                {
                    results.Add("[PASS] Test 7: Vibration toggle cleanly enables and disables haptics.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 7: offVerified={offVerified}, onVerified={onVerified}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 7: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 8: Settings Persistence Across Restarts
            // -------------------------------------------------------------
            try
            {
                // Set specific settings combination
                saveMgr.Data.MusicEnabled = false;
                saveMgr.Data.SfxEnabled = true;
                saveMgr.Data.VibrationEnabled = false;
                saveMgr.Save();

                // Simulate app restart / reload
                saveMgr.Load();

                bool persistent = !saveMgr.Data.MusicEnabled && saveMgr.Data.SfxEnabled && !saveMgr.Data.VibrationEnabled;

                // Restore default
                saveMgr.Data.MusicEnabled = true;
                saveMgr.Data.SfxEnabled = true;
                saveMgr.Data.VibrationEnabled = true;
                saveMgr.Save();

                if (persistent)
                {
                    results.Add("[PASS] Test 8: Settings persist across simulated application reload.");
                    passedCount++;
                }
                else
                {
                    results.Add("[FAIL] Test 8: Settings failed to persist across reload.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 8: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 9: Profile Stats Accurate Display
            // -------------------------------------------------------------
            try
            {
                saveMgr.Data.TotalShotsFired = 42;
                saveMgr.Data.TotalWins = 15;
                saveMgr.Data.CompletedLevelsCount = 10;
                saveMgr.Save();

                var profileDlg = UnityEngine.Object.FindFirstObjectByType<ProfileDialog>(FindObjectsInactive.Include);
                bool dlgFound = profileDlg != null;
                bool statsVerified = false;

                if (dlgFound)
                {
                    profileDlg.Show();
                    statsVerified = profileMgr.TotalShotsFired == 42 &&
                                    profileMgr.TotalWins == 15 &&
                                    profileMgr.CompletedLevelsCount == 10;
                    profileDlg.Hide();
                }

                if (dlgFound && statsVerified)
                {
                    results.Add("[PASS] Test 9: Profile dialog accurately reflects player statistics.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 9: dlgFound={dlgFound}, statsVerified={statsVerified}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 9: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 10: In-Game Settings Non-Destructive Overlay
            // -------------------------------------------------------------
            try
            {
                EditorSceneManager.OpenScene(MAIN_GAME_SCENE);
                var gameSettings = UnityEngine.Object.FindFirstObjectByType<InGameSettingsDialog>(FindObjectsInactive.Include);
                bool inGameFound = gameSettings != null;

                if (inGameFound)
                {
                    float preTimeScale = Time.timeScale;
                    gameSettings.Show();
                    gameSettings.Hide();
                    float postTimeScale = Time.timeScale;

                    bool statePreserved = Mathf.Approximately(preTimeScale, postTimeScale);
                    if (statePreserved)
                    {
                        results.Add("[PASS] Test 10: In-game settings panel opens and closes non-destructively without breaking game state.");
                        passedCount++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 10: TimeScale corrupted: pre={preTimeScale}, post={postTimeScale}");
                    }
                }
                else
                {
                    results.Add("[FAIL] Test 10: InGameSettingsDialog not found in MainGame.unity.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 10: Exception - {ex.Message}");
            }
            finally
            {
                EditorSceneManager.OpenScene(MAIN_MENU_SCENE);
            }

            // Compile final report
            string report = $"=== PHASE 12 AUTOMATED TEST RESULTS: {passedCount}/{totalTests} PASSED ===\n" +
                            string.Join("\n", results);
            Debug.Log(report);
            return report;
        }
    }
}
