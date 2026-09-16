using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.Economy;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.UI;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated Play Mode test suite for Phase 5: Stand & Level Completion Logic.
    /// Validates Win detection, Victory delay, Physics settling check, Defeat condition,
    /// Locked reward formula (Base + Remaining * 2), Dual platform support, and Win/Lose UI panels.
    /// </summary>
    public class Phase5PlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase5_test_result.txt";
        private const string WIN_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase5_win_panel.png";
        private const string LOSE_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase5_lose_panel.png";

        private IEnumerator Start()
        {
            // Wait for scene initialization
            yield return new WaitForSeconds(0.5f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PHASE 5 PLAY MODE TEST SUITE ===");
            report.AppendLine($"Execution Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var levelMgr = LevelManager.Instance;
            var mortar = FindFirstObjectByType<MortarController>();
            var winPanel = FindFirstObjectByType<WinPanel>(FindObjectsInactive.Include);
            var losePanel = FindFirstObjectByType<LosePanel>(FindObjectsInactive.Include);

            if (levelMgr == null || mortar == null)
            {
                report.AppendLine("CRITICAL ERROR: LevelManager or MortarController missing from scene!");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // Clean existing scene targets for isolated testing
            var existingObjects = FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None);
            foreach (var o in existingObjects) Destroy(o.gameObject);
            yield return new WaitForSeconds(0.3f);

            levelMgr.ScanAndRegisterSceneTargets();

            // ----------------------------------------------------------------------------------
            // TEST 1: Locked Gold Reward Formula (Section 1.4 / 10.2)
            // Total Gold = Base Level Reward + (Remaining Balls * 2)
            // ----------------------------------------------------------------------------------
            report.AppendLine("[TEST 1] Testing Locked Gold Reward Formula: Base + (Remaining * 2)...");
            var rewardConfig = levelMgr.RewardConfig;
            int r0 = rewardConfig.CalculateReward(0);
            int r5 = rewardConfig.CalculateReward(5);
            int r25 = rewardConfig.CalculateReward(25);

            report.AppendLine($"  0 Balls: {r0} Gold (Expected: 50)");
            report.AppendLine($"  5 Balls: {r5} Gold (Expected: 60)");
            report.AppendLine($"  25 Balls: {r25} Gold (Expected: 100)");

            bool mathCorrect = (r0 == 50) && (r5 == 60) && (r25 == 100) && (rewardConfig.RemainingBallMultiplier == 2);
            if (mathCorrect)
            {
                report.AppendLine("  -> PASS: Reward calculation adheres strictly to locked Base + (Remaining * 2) formula.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Reward formula did not match locked requirement!");
                allPassed = false;
            }

            // ----------------------------------------------------------------------------------
            // TEST 2: Dual-Platform Stand Support (Section 8)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Testing Stand Dual-Platform configuration support...");
            var standCtrl = FindFirstObjectByType<StandController>();
            if (standCtrl != null)
            {
                var dualConfig = ScriptableObject.CreateInstance<StandConfig>();
                var fHasSec = typeof(StandConfig).GetField("_hasSecondaryPlatform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fHasSec != null) fHasSec.SetValue(dualConfig, true);

                standCtrl.ApplyConfiguration(dualConfig);
                yield return new WaitForSeconds(0.1f);

                var secHolder = standCtrl.transform.Find("SecondaryPlatform");
                bool secActive = secHolder != null && secHolder.gameObject.activeSelf;
                report.AppendLine($"  Secondary Platform Active: {secActive}");

                if (secActive)
                {
                    report.AppendLine("  -> PASS: Stand dynamically configures secondary platform layout.");
                }
                else
                {
                    report.AppendLine("  -> FAIL: Secondary platform was not enabled by StandConfig.");
                    allPassed = false;
                }

                // Restore single platform for gameplay test
                standCtrl.ApplyConfiguration();
            }
            else
            {
                report.AppendLine("  -> FAIL: StandController not found in scene.");
                allPassed = false;
            }

            // ----------------------------------------------------------------------------------
            // TEST 3: In-Flight Ball Grace Period (Section 10.3)
            // Defeat must NOT trigger while a ball is still in mid-air flying toward target
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Testing In-Flight Ball Grace Period (No premature lose)...");
            var boxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Box.prefab");
            if (boxPrefab == null) boxPrefab = Resources.Load<GameObject>("Prefabs/Gameplay/Box");

            GameObject graceTarget = Instantiate(boxPrefab, new Vector3(0f, 1.8f, 7.5f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            levelMgr.ScanAndRegisterSceneTargets();

            // Set ammo to 1, fire, ammo reaches 0 while ball is in flight
            mortar.SetBallCount(1);
            mortar.ResetCooldown();
            mortar.Fire(new Vector3(0f, 1.8f, 7.5f));

            // Immediately check state: balls = 0, but ball in flight -> MUST NOT BE LOST
            yield return new WaitForSeconds(0.1f);
            bool prematureLose = (levelMgr.CurrentState == LevelState.Lost);
            report.AppendLine($"  State while ball in air: {levelMgr.CurrentState} (Premature Lose: {prematureLose})");

            if (!prematureLose)
            {
                report.AppendLine("  -> PASS: System respects in-flight projectile grace period, avoiding premature defeat.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Level declared lost prematurely while projectile was still in flight!");
                allPassed = false;
            }

            if (graceTarget != null) Destroy(graceTarget);
            CleanupProjectiles();
            yield return new WaitForSeconds(0.5f);
            levelMgr.ResetState();

            // ----------------------------------------------------------------------------------
            // TEST 4: Defeat Condition (Remaining Balls = 0 AND Remaining Targets > 0)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Testing Defeat Condition & LosePanel appearance...");
            CleanupProjectiles();
            levelMgr.ResetState();
            GameObject loseTarget = Instantiate(boxPrefab, new Vector3(0f, 1.8f, 7.5f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            levelMgr.ScanAndRegisterSceneTargets();

            mortar.SetBallCount(0);
            levelMgr.CheckConditions();

            // Wait for Defeat evaluation routine (1.0s wait + 0.5s buffer)
            yield return new WaitForSeconds(1.5f);

            bool isLost = (levelMgr.CurrentState == LevelState.Lost);
            bool isLosePanelVisible = losePanel != null && losePanel.IsVisible;
            report.AppendLine($"  LevelState: {levelMgr.CurrentState} | LosePanel Visible: {isLosePanelVisible}");

            if (isLost && isLosePanelVisible)
            {
                report.AppendLine("  -> PASS: Defeat detected accurately and LosePanel displayed.");
                yield return new WaitForSeconds(0.5f);
                ScreenCapture.CaptureScreenshot(LOSE_SCREENSHOT_PATH);
                yield return new WaitForSeconds(2.0f);
                report.AppendLine($"  Captured LosePanel screenshot to '{LOSE_SCREENSHOT_PATH}'.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Defeat condition was not triggered properly.");
                allPassed = false;
            }

            if (losePanel != null) losePanel.HideImmediate();
            if (loseTarget != null) Destroy(loseTarget);
            CleanupProjectiles();
            yield return new WaitForSeconds(0.5f);
            levelMgr.ResetState();

            // ----------------------------------------------------------------------------------
            // TEST 5: Win Condition, Physics Settling & Victory Delay (~2s)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Testing Win Condition, Physics Settling & Victory Delay...");
            levelMgr.ResetState();
            GameObject winTarget = Instantiate(boxPrefab, new Vector3(0f, 1.8f, 7.5f), Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
            levelMgr.ScanAndRegisterSceneTargets();

            mortar.SetBallCount(10);
            report.AppendLine($"  Starting with 1 target and {mortar.RemainingBalls} balls.");

            // Clear target via TriggerDestroyCondition so event fires synchronously
            var winPo = winTarget.GetComponent<PhysicsObject>();
            if (winPo != null) winPo.TriggerDestroyCondition("Cleared by test");
            Destroy(winTarget);
            float clearTime = Time.time;
            yield return new WaitForSeconds(0.1f);

            // Immediately after clear: should be EvaluatingWin, NOT yet Won (delay in progress)
            bool delayWorking = (levelMgr.CurrentState == LevelState.EvaluatingWin);
            report.AppendLine($"  State immediately after clear: {levelMgr.CurrentState} (Evaluating: {delayWorking})");

            // Wait for victory delay (~2 seconds)
            yield return new WaitForSeconds(2.4f);

            float totalDelay = Time.time - clearTime;
            bool isWon = (levelMgr.CurrentState == LevelState.Won);
            bool isWinPanelVisible = winPanel != null && winPanel.IsVisible;
            report.AppendLine($"  State after delay ({totalDelay:F1}s): {levelMgr.CurrentState} | WinPanel Visible: {isWinPanelVisible}");

            if (isWon && isWinPanelVisible && delayWorking)
            {
                report.AppendLine("  -> PASS: Win condition triggered after physics settling and ~2s victory delay.");
                yield return new WaitForSeconds(0.5f);
                ScreenCapture.CaptureScreenshot(WIN_SCREENSHOT_PATH);
                yield return new WaitForSeconds(1.5f);
                report.AppendLine($"  Captured WinPanel screenshot to '{WIN_SCREENSHOT_PATH}'.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Victory delay or WinPanel display failed.");
                allPassed = false;
            }

            // ----------------------------------------------------------------------------------
            // TEST SUMMARY
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n" + (allPassed ? "=== ALL 5 PHASE 5 ACCEPTANCE TESTS PASSED! ===" : "=== SOME PHASE 5 TESTS FAILED ==="));

            File.WriteAllText(REPORT_PATH, report.ToString());
            Debug.Log(report.ToString());

            // Wait 1s and stop Play Mode
            yield return new WaitForSeconds(1.0f);
            UnityEditor.EditorApplication.isPlaying = false;
        }

        private void CleanupProjectiles()
        {
            var projectiles = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            foreach (var p in projectiles)
            {
                if (p != null) Destroy(p.gameObject);
            }
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ClearActiveProjectiles();
            }
        }
    }
}
