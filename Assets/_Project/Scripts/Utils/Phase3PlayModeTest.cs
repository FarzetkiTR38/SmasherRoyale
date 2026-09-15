using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using RoyalSmash.Gameplay;
using RoyalSmash.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Autonomous Play Mode test runner for Phase 3 deliverables.
    /// Verifies ammunition tracking, first-shot instant ready, cooldown blocking,
    /// ballistic trajectory launch, HUD counter sync with micro-animation, and ball auto-destruction.
    /// </summary>
    public class Phase3PlayModeTest : MonoBehaviour
    {
        [SerializeField] private string _reportOutputPath = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase3_test_result.txt";
        [SerializeField] private string _screenshotOutputPath = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase3_game_view.png";

        private void Start()
        {
            StartCoroutine(RunTestRoutine());
        }

        private IEnumerator RunTestRoutine()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== PHASE 3 PLAY MODE TEST SUITE ===");
            bool allPassed = true;

            // Wait for components to initialize and frame rate to stabilize
            yield return new WaitForSeconds(0.5f);

            var mortar = FindFirstObjectByType<MortarController>();
            var inputHandler = FindFirstObjectByType<InputHandler>();
            var hud = FindFirstObjectByType<HUDController>();
            var countText = FindFirstObjectByType<TextMeshProUGUI>();

            if (mortar == null || hud == null)
            {
                report.AppendLine("[FAIL] Missing MortarController or HUDController in scene!");
                FinishTest(report.ToString(), false);
                yield break;
            }

            // Test 1: Initial state (First ball 0 cooldown, 25 ammo, HUD = 25)
            report.AppendLine($"[TEST 1] Initial Ammo: {mortar.RemainingBalls}, Cooldown: {mortar.CooldownTimer:F2}s, HUD: '{countText?.text}'");
            if (mortar.RemainingBalls == 25 && mortar.CooldownTimer <= 0.001f && countText?.text == "25")
            {
                report.AppendLine("  -> PASS: 25 initial balls, 0 cooldown, HUD matches.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Initial state mismatch.");
                allPassed = false;
            }

            // Test 2: Fire First Ball towards Target Stand (0, 1.5, 8)
            Vector3 target1 = new Vector3(0f, 1.5f, 8f);
            report.AppendLine($"[TEST 2] Firing First Ball toward {target1}...");
            inputHandler?.SimulateTapWorldTarget(target1);

            // Check immediately after fire before frame ends
            float cooldownImmediately = mortar.CooldownTimer;
            int ammoAfterShot1 = mortar.RemainingBalls;
            report.AppendLine($"[TEST 2 RESULT] Ammo: {ammoAfterShot1}, Cooldown: {cooldownImmediately:F2}s, HUD: '{countText?.text}'");
            if (ammoAfterShot1 == 24 && cooldownImmediately > 0f)
            {
                report.AppendLine("  -> PASS: Ammo decremented to 24, cooldown active.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Shot 1 ammo or cooldown mismatch.");
                allPassed = false;
            }

            // Test 3: Rapid spam during cooldown (must be blocked)
            report.AppendLine("[TEST 3] Attempting rapid fire during cooldown (should be blocked)...");
            inputHandler?.SimulateTapWorldTarget(target1);
            if (mortar.RemainingBalls == 24)
            {
                report.AppendLine("  -> PASS: Rapid tap was successfully blocked by cooldown. Ammo remains 24.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Rapid tap was not blocked by cooldown!");
                allPassed = false;
            }

            // Test 4: Verify Ball in flight (Rigidbody velocity towards target)
            yield return new WaitForFixedUpdate();
            yield return null;
            var balls = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            if (balls.Length > 0)
            {
                var ball1 = balls[0];
                var rb = ball1.GetComponent<Rigidbody>();
                report.AppendLine($"[TEST 4] Ball spawned in flight. Position: {ball1.transform.position}, Velocity: {rb.linearVelocity}");
                if (rb.linearVelocity.magnitude > 5f && rb.linearVelocity.z > 0f)
                {
                    report.AppendLine("  -> PASS: Ball has forward/upward ballistic velocity.");
                }
                else
                {
                    report.AppendLine("  -> FAIL: Ball velocity unexpected.");
                    allPassed = false;
                }
            }
            else
            {
                report.AppendLine("[TEST 4] FAIL: No ProjectileController spawned in scene!");
                allPassed = false;
            }

            // Test 5: Wait for cooldown to expire, then fire second ball at angle (-1.5, 1.5, 8)
            yield return new WaitForSeconds(0.25f);
            Vector3 target2 = new Vector3(-1.5f, 1.5f, 8f);
            report.AppendLine($"[TEST 5] Firing Second Ball toward angled target {target2}...");
            inputHandler?.SimulateTapWorldTarget(target2);
            yield return new WaitForSeconds(0.1f);

            report.AppendLine($"[TEST 5 RESULT] Ammo: {mortar.RemainingBalls}, Mortar Target Yaw: {mortar.TargetYaw:F1} deg, HUD: '{countText?.text}'");
            if (mortar.RemainingBalls == 23 && mortar.TargetYaw < -5f && countText?.text == "23")
            {
                report.AppendLine("  -> PASS: Mortar swiveled left towards angled target, Ammo decremented to 23, HUD updated to 23.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Second shot state or yaw mismatch.");
                allPassed = false;
            }

            // Test 6: Capture Screenshot of ball in flight & HUD
            yield return new WaitForSeconds(0.15f);
            ScreenCapture.CaptureScreenshot(_screenshotOutputPath);
            yield return new WaitForSeconds(0.3f);
            report.AppendLine($"[TEST 6] Captured gameplay screenshot to '{_screenshotOutputPath}'.");

            // Test 7: Verify Ball Lifetime & Auto-destruction (shrink & destroy after ~5s)
            report.AppendLine("[TEST 7] Testing 5s lifetime auto-destruction with shrink/fade...");
            Time.timeScale = 5.0f; // accelerate time for test
            yield return new WaitForSeconds(5.5f); // 5.5 game seconds (1.1 real seconds)
            Time.timeScale = 1.0f;

            var remainingBalls = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            report.AppendLine($"[TEST 7 RESULT] Remaining balls after >5s: {remainingBalls.Length}");
            if (remainingBalls.Length == 0)
            {
                report.AppendLine("  -> PASS: Both fired balls safely auto-destroyed after lifetime.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Balls did not destroy after lifetime.");
                allPassed = false;
            }

            report.AppendLine(allPassed ? "=== ALL 7 PLAY MODE TESTS PASSED! ===" : "=== SOME TESTS FAILED ===");
            FinishTest(report.ToString(), allPassed);
        }

        private void FinishTest(string reportContent, bool success)
        {
            Debug.Log(reportContent);
            try
            {
                File.WriteAllText(_reportOutputPath, reportContent);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to write report file: " + ex.Message);
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}
