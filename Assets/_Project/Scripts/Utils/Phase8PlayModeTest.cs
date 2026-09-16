using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using RoyalSmash.Lives;
using RoyalSmash.Save;
using RoyalSmash.UI;
using RoyalSmash.Core;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated Play Mode test suite for Phase 8: Lives System & Exit Handling.
    /// Validates starting lives (5/5), life deduction on defeat (-1), win preservation (0 life loss),
    /// level exit deduction (-1), ungraceful app close detection (-1), duplicate deduction prevention,
    /// passive regeneration timer (+1 per 30m), Gold life purchase (-100 Gold -> +1 Life),
    /// and 0 lives gating.
    /// </summary>
    public class Phase8PlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase8_test_result.txt";
        private const string HUD_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase8_hud_lives.png";
        private const string MODAL_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase8_out_of_lives_dialog.png";

        private IEnumerator Start()
        {
            // Initial stabilization
            yield return new WaitForSeconds(0.6f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PHASE 8: LIVES SYSTEM & EXIT HANDLING TEST SUITE ===");
            report.AppendLine($"Execution Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var livesMgr = LivesManager.Instance;
            var saveMgr = SaveManager.Instance;
            var lifecycle = AppLifecycleHandler.Instance ?? FindFirstObjectByType<AppLifecycleHandler>();

            if (livesMgr == null || saveMgr == null || saveMgr.Data == null)
            {
                report.AppendLine("CRITICAL ERROR: LivesManager or SaveManager missing!");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // Reset to full 5 lives state
            saveMgr.Data.Lives = 5;
            saveMgr.Data.LifeTimerStartTicks = 0;
            saveMgr.Data.WasInActiveLevel = false;
            saveMgr.Data.Gold = 1000;
            saveMgr.Save();
            livesMgr.ClearTransactionGuards();

            // Capture initial HUD with Lives widget visible
            ScreenCapture.CaptureScreenshot(HUD_SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.4f);
            report.AppendLine($"Captured initial HUD with lives widget to '{HUD_SCREENSHOT_PATH}'.");

            // ----------------------------------------------------------------------------------
            // TEST 1: Starting Lives Capacity (5/5)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 1] Starting Lives Capacity...");
            int initialLives = livesMgr.CurrentLives;
            int maxLives = livesMgr.MaxLives;
            bool isFull = livesMgr.AreLivesFull;

            report.AppendLine($"  Lives: {initialLives}/{maxLives} | Full: {isFull} (Expected: 5/5, True)");
            if (initialLives != 5 || maxLives != 5 || !isFull)
            {
                report.AppendLine("  -> FAILED: Initial lives should be exactly 5/5 and marked full.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Starting lives correctly initialized at 5/5.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 2: Defeat Life Deduction (-1 Life) & Timer Start
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Defeat Life Deduction & Timer Initiation...");
            bool deductedDefeat = livesMgr.DeductLife("LevelLost", "Test_Defeat_1");
            int livesAfterDefeat = livesMgr.CurrentLives;
            bool timerStarted = saveMgr.Data.LifeTimerStartTicks > 0;
            TimeSpan timeUntilNext = livesMgr.GetTimeUntilNextLife();

            report.AppendLine($"  Deducted: {deductedDefeat} | Lives: 5 -> {livesAfterDefeat} | Timer Started: {timerStarted} ({timeUntilNext.Minutes}m {timeUntilNext.Seconds}s remaining)");
            if (!deductedDefeat || livesAfterDefeat != 4 || !timerStarted)
            {
                report.AppendLine("  -> FAILED: Defeat should deduct exactly 1 life (5 -> 4) and start regeneration timer.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Defeat deducted exactly 1 life and started 30-minute timer.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 3: Duplicate Deduction Protection
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Duplicate Deduction Protection...");
            bool duplicateAttempt = livesMgr.DeductLife("LevelLost", "Test_Defeat_1");
            int livesAfterDuplicate = livesMgr.CurrentLives;

            report.AppendLine($"  Duplicate Call Accepted: {duplicateAttempt} (Expected: False) | Lives: {livesAfterDuplicate} (Expected: 4)");
            if (duplicateAttempt || livesAfterDuplicate != 4)
            {
                report.AppendLine("  -> FAILED: Duplicate deduction for same event key was not blocked.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Duplicate deduction safely rejected.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 4: Level Exit Life Deduction (-1 Life)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Active Level Exit Deduction...");
            bool deductedExit = livesMgr.DeductLife("LevelExit", "Test_Exit_1");
            int livesAfterExit = livesMgr.CurrentLives;

            report.AppendLine($"  Deducted: {deductedExit} | Lives: 4 -> {livesAfterExit} (Expected: 3)");
            if (!deductedExit || livesAfterExit != 3)
            {
                report.AppendLine("  -> FAILED: Level exit should deduct exactly 1 life.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Level exit deducted 1 life correctly.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 5: Ungraceful App Close Detection (-1 Life on Restart)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Ungraceful App Close Detection...");
            saveMgr.Data.WasInActiveLevel = true; // Simulate app killed while in level
            saveMgr.Save();

            if (lifecycle != null)
            {
                lifecycle.CheckPreviousSessionUngracefulExit();
            }

            int livesAfterCloseCheck = livesMgr.CurrentLives;
            bool wasInLevelCleared = !saveMgr.Data.WasInActiveLevel;

            report.AppendLine($"  Lives: 3 -> {livesAfterCloseCheck} (Expected: 2) | WasInActiveLevel Cleared: {wasInLevelCleared}");
            if (livesAfterCloseCheck != 2 || !wasInLevelCleared)
            {
                report.AppendLine("  -> FAILED: Ungraceful app close did not deduct life or failed to reset flag.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Ungraceful app close detected and penalized accurately.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 6: Offline Passive Life Regeneration (+1 Life per 30m)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 6] Passive Life Regeneration (30 Minutes)...");
            // Simulate 31 minutes elapsed
            saveMgr.Data.LifeTimerStartTicks -= TimeSpan.FromMinutes(31).Ticks;
            saveMgr.Save();

            livesMgr.ProcessRegeneration();
            int livesAfterRegen = livesMgr.CurrentLives;

            report.AppendLine($"  Lives After 31m Regen: 2 -> {livesAfterRegen} (Expected: 3)");
            if (livesAfterRegen != 3)
            {
                report.AppendLine("  -> FAILED: Life regeneration failed to grant +1 life after 30+ minutes.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Passive life regeneration granted +1 life accurately.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 7: Gold Purchase (+1 Life for 100 Gold)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 7] Gold Life Purchase (+1 Life for 100 Gold)...");
            int initialGold = saveMgr.Data.Gold;
            bool purchaseSuccess = livesMgr.BuyLifeWithGold();
            int livesAfterBuy = livesMgr.CurrentLives;
            int goldAfterBuy = saveMgr.Data.Gold;

            report.AppendLine($"  Purchase Success: {purchaseSuccess} | Lives: 3 -> {livesAfterBuy} | Gold: {initialGold} -> {goldAfterBuy} (-100)");
            if (!purchaseSuccess || livesAfterBuy != 4 || goldAfterBuy != initialGold - 100)
            {
                report.AppendLine("  -> FAILED: Gold life purchase failed or deducted incorrect amount.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Gold life purchase granted +1 life and deducted 100 Gold.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 8: Zero Lives Gating & Dialog Trigger
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 8] Zero Lives Gating & OutOfLives Modal...");
            saveMgr.Data.Lives = 0;
            saveMgr.Save();

            bool hasLives = livesMgr.HasLivesToPlay;
            var outOfLivesDialog = FindFirstObjectByType<OutOfLivesDialog>();
            if (outOfLivesDialog != null)
            {
                outOfLivesDialog.Show();
            }

            yield return new WaitForSeconds(0.4f);
            ScreenCapture.CaptureScreenshot(MODAL_SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.4f);
            report.AppendLine($"  HasLivesToPlay: {hasLives} (Expected: False)");
            report.AppendLine($"  Captured OutOfLives modal dialog to '{MODAL_SCREENSHOT_PATH}'.");

            if (hasLives)
            {
                report.AppendLine("  -> FAILED: HasLivesToPlay should be false at 0 lives.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Zero lives gating correctly reports false.");
            }

            if (outOfLivesDialog != null)
            {
                outOfLivesDialog.Hide();
            }

            // ----------------------------------------------------------------------------------
            // RESTORE FULL LIVES FOR USER
            // ----------------------------------------------------------------------------------
            saveMgr.Data.Lives = 5;
            saveMgr.Data.LifeTimerStartTicks = 0;
            saveMgr.Data.WasInActiveLevel = false;
            saveMgr.Save();

            // ----------------------------------------------------------------------------------
            // FINAL SUMMARY
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n========================================");
            if (allPassed)
            {
                report.AppendLine("OVERALL STATUS: ALL 8 PHASE 8 TESTS PASSED PERFECTLY!");
            }
            else
            {
                report.AppendLine("OVERALL STATUS: SOME TESTS FAILED.");
            }
            report.AppendLine("========================================");

            File.WriteAllText(REPORT_PATH, report.ToString());
            Debug.Log(report.ToString());

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
