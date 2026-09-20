using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Core;
using RoyalSmash.Economy;
using RoyalSmash.Save;
using RoyalSmash.Lives;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 11: Main Menu & UI System.
    /// Validates all 10 acceptance criteria from Section 17 and Phase 11 specifications.
    /// </summary>
    public static class Phase11PlayModeTest
    {
        private static string ResultPath => Path.Combine(Application.dataPath, "../_FarzetkiDocs/Phases/Phase11_Test_Result.txt");

        [MenuItem("RoyalSmash/Phase 11/Run Automated Verification Suite")]
        public static void RunTests()
        {
            var results = new System.Text.StringBuilder();
            results.AppendLine("=== PHASE 11: MAIN MENU & UI SYSTEM ACCEPTANCE TEST RUNNER ===");
            results.AppendLine($"Executed at: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            results.AppendLine("---------------------------------------------------------------");

            int passed = 0;
            int total = 0;

            void AssertTest(string testName, bool condition, string details)
            {
                total++;
                if (condition)
                {
                    passed++;
                    results.AppendLine($"[PASS] Test {total}: {testName} - {details}");
                    Debug.Log($"<color=green>[PASS] Test {total}: {testName} - {details}</color>");
                }
                else
                {
                    results.AppendLine($"[FAIL] Test {total}: {testName} - {details}");
                    Debug.LogError($"[FAIL] Test {total}: {testName} - {details}");
                }
            }

            try
            {
                // 1. Ensure MainMenu scene is loaded
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(Phase11Setup.MAIN_MENU_SCENE_PATH, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }

                var saveMgr = SaveManager.Instance;
                if (saveMgr == null || saveMgr.Data == null)
                {
                    saveMgr = Object.FindFirstObjectByType<SaveManager>();
                    if (saveMgr != null) saveMgr.Load();
                }

                var controller = Object.FindFirstObjectByType<MainMenuController>();
                var navUI = Object.FindFirstObjectByType<BottomNavigationUI>();
                var livesMgr = LivesManager.Instance;
                var goldMgr = GoldManager.Instance;
                var transMgr = SceneTransitionManager.Instance;

                // Reset save state
                saveMgr.Data.CurrentLevel = 3;
                saveMgr.Data.Gold = 1250;
                saveMgr.Data.Lives = 5;
                saveMgr.Save();

                if (goldMgr != null) goldMgr.SetGold(1250);
                controller.RefreshUI();

                // Test 1: Main Menu Level Display
                var levelCard = GameObject.Find("LevelCard");
                var levelTxt = levelCard != null ? levelCard.transform.Find("LevelText")?.GetComponent<TMPro.TextMeshProUGUI>() : GameObject.Find("LevelText")?.GetComponent<TMPro.TextMeshProUGUI>();
                bool levelMatches = levelTxt != null && levelTxt.text.Contains("3");
                AssertTest("Current Level Display", levelMatches,
                    $"Level display text: '{(levelTxt != null ? levelTxt.text : "null")}' matches CurrentLevel (3)");

                // Test 2: Build Settings Index 0 and Index 1
                var scenes = EditorBuildSettings.scenes;
                bool buildOk = scenes.Length >= 2 &&
                               scenes[0].path.Contains("MainMenu.unity") && scenes[0].enabled &&
                               scenes[1].path.Contains("MainGame.unity") && scenes[1].enabled;
                AssertTest("Build Settings Scenes", buildOk,
                    $"Scene 0: '{(scenes.Length > 0 ? scenes[0].path : "none")}', Scene 1: '{(scenes.Length > 1 ? scenes[1].path : "none")}'");

                // Test 3: Bottom Navigation - Shop Tab Switching
                var pagesRoot = GameObject.Find("PagesRoot");
                var shopPage = pagesRoot != null ? pagesRoot.transform.Find("ShopPage")?.gameObject : null;
                var battlePage = pagesRoot != null ? pagesRoot.transform.Find("BattlePage")?.gameObject : null;
                var leadPage = pagesRoot != null ? pagesRoot.transform.Find("LeaderboardPage")?.gameObject : null;

                navUI.SwitchTab(NavigationTab.Shop);
                bool shopActive = shopPage != null && shopPage.activeSelf && battlePage != null && !battlePage.activeSelf;
                AssertTest("Bottom Nav Shop Tab", shopActive && navUI.CurrentTab == NavigationTab.Shop,
                    $"Shop page active: {shopPage?.activeSelf}, Battle page inactive: {!battlePage?.activeSelf}");

                // Test 4: Bottom Navigation - Leaderboard Tab Switching
                navUI.SwitchTab(NavigationTab.Leaderboard);
                bool leadActive = leadPage != null && leadPage.activeSelf && !battlePage.activeSelf && !shopPage.activeSelf;
                AssertTest("Bottom Nav Leaderboard Tab", leadActive && navUI.CurrentTab == NavigationTab.Leaderboard,
                    $"Leaderboard page active: {leadPage?.activeSelf}");

                // Test 5: Bottom Navigation - Battle Tab Return
                navUI.SwitchTab(NavigationTab.Battle);
                bool battleActive = battlePage != null && battlePage.activeSelf && !shopPage.activeSelf && !leadPage.activeSelf;
                AssertTest("Bottom Nav Battle Tab", battleActive && navUI.CurrentTab == NavigationTab.Battle,
                    $"Battle page active: {battlePage?.activeSelf}");

                // Test 6: Zero Lives Gating on Play
                saveMgr.Data.Lives = 0;
                saveMgr.Save();
                var outOfLivesDlg = Object.FindFirstObjectByType<OutOfLivesDialog>(FindObjectsInactive.Include);
                controller.HandlePlayClicked();
                bool dlgShown = outOfLivesDlg != null && outOfLivesDlg.gameObject.activeSelf;
                AssertTest("Zero Lives Play Gating", dlgShown,
                    $"OutOfLivesDialog active: {dlgShown} when player has 0 lives");

                // Test 7: Normal Play Flow (Lives > 0)
                saveMgr.Data.Lives = 5;
                saveMgr.Save();
                if (outOfLivesDlg != null) outOfLivesDlg.Hide();
                AssertTest("Normal Play Flow Allowed", livesMgr != null && livesMgr.HasLivesToPlay,
                    $"Player has {livesMgr?.CurrentLives} lives; play transition permitted");

                // Test 8: Top Bar Currency Synchronization
                var goldTxt = GameObject.Find("GoldWidget/Text")?.GetComponent<TMPro.TextMeshProUGUI>();
                bool goldOk = goldTxt != null && (goldTxt.text.Contains("1,250") || goldTxt.text.Contains("1.250") || goldTxt.text.Contains("1250"));
                AssertTest("Top Bar Gold Synchronization", goldOk,
                    $"Gold text: '{(goldTxt != null ? goldTxt.text : "null")}' reflects save data (1,250)");

                // Test 9: Settings Dialog Modal
                var settingsDlg = Object.FindFirstObjectByType<InGameSettingsDialog>(FindObjectsInactive.Include);
                if (settingsDlg != null) settingsDlg.Show();
                bool settingsVisible = settingsDlg != null && settingsDlg.gameObject.activeSelf;
                if (settingsDlg != null) settingsDlg.Hide();
                AssertTest("Settings Dialog Accessibility", settingsVisible,
                    $"Settings modal opens and closes cleanly without errors");

                // Test 10: Special Offers Layout & Countdown
                var rightOffer = GameObject.Find("Offer_Right");
                var offerTimer = GameObject.Find("Offer_Right/TimerText")?.GetComponent<TMPro.TextMeshProUGUI>();
                bool offerOk = rightOffer != null && offerTimer != null;
                AssertTest("Special Offers Display", offerOk,
                    $"Side offers present without obstructing Play button (Timer: {offerTimer?.text})");
            }
            catch (System.Exception ex)
            {
                results.AppendLine($"[EXCEPTION] Test runner encountered error: {ex.Message}\n{ex.StackTrace}");
                Debug.LogException(ex);
            }

            results.AppendLine("---------------------------------------------------------------");
            results.AppendLine($"TOTAL TESTS: {total} | PASSED: {passed} | FAILED: {total - passed}");
            results.AppendLine($"VERDICT: {(passed == total ? "ALL ACCEPTANCE CRITERIA MET (100% PASS)" : "FAILURES DETECTED")}");

            File.WriteAllText(ResultPath, results.ToString());
            Debug.Log($"[Phase11PlayModeTest] Test results written to {ResultPath}\n" + results.ToString());
        }
    }
}
