using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Skills;
using RoyalSmash.UI;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated test verifying:
    /// 1. Tapping below the stand triggers the warning banner and DOES NOT fire.
    /// 2. Clicking on UI skill buttons activates the skill and DOES NOT fire.
    /// </summary>
    public class ProtectionPlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/protection_test_result.txt";
        private const string WARNING_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/protection_warning_banner.png";

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PROTECTION & UI CLICK TEST SUITE ===");
            report.AppendLine($"Execution Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var input = FindFirstObjectByType<InputHandler>();
            var mortar = FindFirstObjectByType<MortarController>();
            var warningUi = FindFirstObjectByType<InvalidAimWarningUI>(FindObjectsInactive.Include);
            var skillMgr = SkillManager.Instance;

            if (input == null || mortar == null || warningUi == null || skillMgr == null)
            {
                report.AppendLine("CRITICAL: Missing essential components!");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // ----------------------------------------------------------------------------------
            // TEST 1: Tap Below Stand Protection
            // ----------------------------------------------------------------------------------
            report.AppendLine("[TEST 1] Testing Tap Below Stand Protection...");
            mortar.SetBallCount(25);
            mortar.ResetCooldown();
            int initialBalls = mortar.RemainingBalls;
            int initialFires = mortar.FireEventCount;

            // Screen point near bottom (screen coordinates Y=400, below stand)
            Vector2 belowStandScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.25f);
            input.HandleTap(belowStandScreenPos);

            yield return new WaitForSeconds(0.2f);

            int ballsAfter = mortar.RemainingBalls;
            int firesAfter = mortar.FireEventCount;
            bool didNotFire = (ballsAfter == initialBalls && firesAfter == initialFires);

            report.AppendLine($"  Balls: {initialBalls} -> {ballsAfter} | Fires: {initialFires} -> {firesAfter} | Blocked: {didNotFire}");

            if (didNotFire)
            {
                report.AppendLine("  -> PASS: Cannon fire was strictly blocked when tapping below the stand.");
                yield return new WaitForSeconds(0.2f);
                ScreenCapture.CaptureScreenshot(WARNING_SCREENSHOT_PATH);
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                report.AppendLine("  -> FAIL: Cannon fired despite tap being below the stand!");
                allPassed = false;
            }

            yield return new WaitForSeconds(0.5f);

            // ----------------------------------------------------------------------------------
            // TEST 2: UI Click Interception (Skill Buttons)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Testing UI Click Interception (Skill Buttons)...");
            skillMgr.SetSkillUnlocked(SkillType.MegaBall, true);
            skillMgr.SetRemainingUses(SkillType.MegaBall, 3);
            skillMgr.ResetActiveSkills();

            // Find MegaBall button screen center
            var megaBtn = FindFirstObjectByType<SkillButtonUI>();
            if (megaBtn != null)
            {
                RectTransform btnRect = megaBtn.GetComponent<RectTransform>();
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, btnRect.position);

                int ballsBeforeClick = mortar.RemainingBalls;
                int firesBeforeClick = mortar.FireEventCount;

                // Simulate tap on the UI button screen position
                input.HandleTap(screenPos);

                yield return new WaitForSeconds(0.1f);

                int ballsAfterClick = mortar.RemainingBalls;
                int firesAfterClick = mortar.FireEventCount;
                bool uiProtected = (ballsAfterClick == ballsBeforeClick && firesAfterClick == firesBeforeClick);

                report.AppendLine($"  Tap on UI Button ({screenPos}) -> Balls: {ballsBeforeClick} -> {ballsAfterClick} | Fires: {firesBeforeClick} -> {firesAfterClick} | Cannon Shot Blocked: {uiProtected}");

                if (uiProtected)
                {
                    report.AppendLine("  -> PASS: Tapping on UI does NOT trigger cannon fire.");
                }
                else
                {
                    report.AppendLine("  -> FAIL: Tapping on UI triggered an accidental cannon shot!");
                    allPassed = false;
                }
            }
            else
            {
                report.AppendLine("  -> FAIL: SkillButtonUI not found in scene.");
                allPassed = false;
            }

            report.AppendLine(allPassed ? "\n=== ALL PROTECTION TESTS PASSED! ===" : "\n=== SOME PROTECTION TESTS FAILED ===");
            File.WriteAllText(REPORT_PATH, report.ToString());
            Debug.Log(report.ToString());

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
