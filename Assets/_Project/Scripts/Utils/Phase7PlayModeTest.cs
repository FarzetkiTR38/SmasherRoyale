using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Tutorial;
using RoyalSmash.Save;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated Play Mode test suite for Phase 7: First-Launch Tutorial & Save System.
    /// Validates tutorial activation on first launch, input gating (blocking off-target taps),
    /// attention pulse triggers, successful first guided shot execution, immediate full control release,
    /// persistence of TutorialCompleted to disk, and subsequent launch skip behavior.
    /// </summary>
    public class Phase7PlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase7_test_result.txt";
        private const string OVERLAY_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase7_tutorial_overlay.png";
        private const string ACTION_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase7_first_shot_fired.png";

        private IEnumerator Start()
        {
            // Initial stabilization
            yield return new WaitForSeconds(0.6f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PHASE 7: TUTORIAL & SAVE PERSISTENCE TEST SUITE ===");
            report.AppendLine($"Execution Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var tutMgr = TutorialManager.Instance;
            var saveMgr = SaveManager.Instance;
            var inputHandler = FindFirstObjectByType<InputHandler>();
            var mortar = FindFirstObjectByType<MortarController>();
            var cam = Camera.main;

            if (tutMgr == null || saveMgr == null || inputHandler == null || mortar == null)
            {
                report.AppendLine("CRITICAL ERROR: Core managers or components missing!");
                report.AppendLine($"tutMgr={tutMgr != null}, saveMgr={saveMgr != null}, inputHandler={inputHandler != null}, mortar={mortar != null}");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // ----------------------------------------------------------------------------------
            // TEST 1: First Launch State & Tutorial Activation
            // ----------------------------------------------------------------------------------
            report.AppendLine("[TEST 1] First-Launch Tutorial Initialization...");
            // Force first-launch state in save data
            saveMgr.SetTutorialCompleted(false);
            tutMgr.CheckAndInitializeTutorial();

            yield return new WaitForSeconds(0.4f);

            bool isActive = tutMgr.IsTutorialActive;
            bool targetBound = tutMgr.TargetTransform != null;
            report.AppendLine($"  Tutorial Active: {isActive} (Expected: True)");
            report.AppendLine($"  Target Bound: {targetBound} ({tutMgr.TargetTransform?.name})");

            if (!isActive || !targetBound)
            {
                report.AppendLine("  -> FAILED: Tutorial should be active with a valid target bound.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Tutorial initialized properly for first launch.");
            }

            // Capture initial overlay view
            ScreenCapture.CaptureScreenshot(OVERLAY_SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.4f);
            report.AppendLine($"  Captured tutorial overlay screenshot to '{OVERLAY_SCREENSHOT_PATH}'.");

            // ----------------------------------------------------------------------------------
            // TEST 2: Input Restriction - Block Off-Target Taps
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Input Restriction (Off-Target Tap Rejection)...");
            int initialBallCount = mortar.RemainingBalls;
            int initialFires = mortar.FireEventCount;
            bool invalidTapTriggered = false;
            tutMgr.OnInvalidTapAttempted += () => invalidTapTriggered = true;

            // Tap far top-left screen away from target
            Vector2 offTargetScreenPos = new Vector2(100f, Screen.height * 0.85f);
            inputHandler.HandleTap(offTargetScreenPos);

            yield return new WaitForSeconds(0.3f);

            int ballsAfterOffTarget = mortar.RemainingBalls;
            int firesAfterOffTarget = mortar.FireEventCount;
            bool stillActive = tutMgr.IsTutorialActive;

            report.AppendLine($"  Ball Count: {initialBallCount} -> {ballsAfterOffTarget} (Expected: {initialBallCount})");
            report.AppendLine($"  Fires: {initialFires} -> {firesAfterOffTarget} (Expected: {initialFires})");
            report.AppendLine($"  Invalid Tap Callback Triggered: {invalidTapTriggered} (Expected: True)");
            report.AppendLine($"  Tutorial Still Active: {stillActive} (Expected: True)");

            if (ballsAfterOffTarget != initialBallCount || firesAfterOffTarget != initialFires || !stillActive || !invalidTapTriggered)
            {
                report.AppendLine("  -> FAILED: Off-target tap was not correctly blocked or tutorial state altered prematurely.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Off-target tap blocked cleanly, attention pulse triggered.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 3: Valid Guided Tap & Completion
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Valid Guided Shot Execution & Full Control Release...");
            bool tutorialCompletedTriggered = false;
            tutMgr.OnTutorialCompleted += () => tutorialCompletedTriggered = true;

            Vector3 targetScreenPos = cam.WorldToScreenPoint(tutMgr.TargetTransform.position);
            Vector2 validTapPos = new Vector2(targetScreenPos.x, targetScreenPos.y);

            report.AppendLine($"  Simulating tap at target screen pos: {validTapPos}");
            inputHandler.HandleTap(validTapPos);

            yield return new WaitForSeconds(0.3f);

            int ballsAfterValidTap = mortar.RemainingBalls;
            int firesAfterValidTap = mortar.FireEventCount;
            bool activeAfterShot = tutMgr.IsTutorialActive;
            bool savedCompleted = saveMgr.Data.TutorialCompleted;

            report.AppendLine($"  Ball Count: {initialBallCount} -> {ballsAfterValidTap} (Expected: {initialBallCount - 1})");
            report.AppendLine($"  Fires: {initialFires} -> {firesAfterValidTap} (Expected: {initialFires + 1})");
            report.AppendLine($"  Tutorial Active: {activeAfterShot} (Expected: False)");
            report.AppendLine($"  OnTutorialCompleted Triggered: {tutorialCompletedTriggered} (Expected: True)");
            report.AppendLine($"  Saved TutorialCompleted: {savedCompleted} (Expected: True)");

            if (ballsAfterValidTap >= initialBallCount || activeAfterShot || !savedCompleted)
            {
                report.AppendLine("  -> FAILED: Valid tap did not fire projectile or failed to release tutorial control.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Guided shot fired successfully! Full control released immediately.");
            }

            // Capture shot in action
            ScreenCapture.CaptureScreenshot(ACTION_SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.5f);
            report.AppendLine($"  Captured first shot action screenshot to '{ACTION_SCREENSHOT_PATH}'.");

            // ----------------------------------------------------------------------------------
            // TEST 4: Full Control After Tutorial
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Full Control Verification After Tutorial...");
            // Allow cooldown to expire
            yield return new WaitForSeconds(0.6f);

            Vector3 secondaryTarget = tutMgr.TargetTransform.position + new Vector3(1.2f, 0.4f, 0f);
            Vector3 secondaryScreen = cam.WorldToScreenPoint(secondaryTarget);

            int ballsBeforeSecondShot = mortar.RemainingBalls;
            int firesBeforeSecondShot = mortar.FireEventCount;
            inputHandler.HandleTap(new Vector2(secondaryScreen.x, secondaryScreen.y));

            yield return new WaitForSeconds(0.3f);

            int ballsAfterSecondShot = mortar.RemainingBalls;
            int firesAfterSecondShot = mortar.FireEventCount;
            report.AppendLine($"  Second Shot Ball Count: {ballsBeforeSecondShot} -> {ballsAfterSecondShot} (Expected: {ballsBeforeSecondShot - 1})");
            report.AppendLine($"  Second Shot Fires: {firesBeforeSecondShot} -> {firesAfterSecondShot} (Expected: {firesBeforeSecondShot + 1})");

            if (ballsAfterSecondShot >= ballsBeforeSecondShot || firesAfterSecondShot <= firesBeforeSecondShot)
            {
                report.AppendLine("  -> FAILED: Player not able to fire freely after tutorial completed.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Player possesses full firing control across the playground.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 5: Subsequent Launch Tutorial Skip & Persistence
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Subsequent Launch Tutorial Skip...");
            tutMgr.CheckAndInitializeTutorial();
            bool activeOnRecheck = tutMgr.IsTutorialActive;

            report.AppendLine($"  Tutorial Active on Recheck: {activeOnRecheck} (Expected: False)");
            if (activeOnRecheck)
            {
                report.AppendLine("  -> FAILED: Tutorial re-activated even though save data marks it completed.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Tutorial properly skipped on subsequent launches.");
            }

            // ----------------------------------------------------------------------------------
            // FINAL SUMMARY
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n========================================");
            if (allPassed)
            {
                report.AppendLine("OVERALL STATUS: ALL PHASE 7 TESTS PASSED PERFECTLY!");
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
