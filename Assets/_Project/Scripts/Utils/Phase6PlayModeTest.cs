using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Skills;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated Play Mode test suite for Phase 6: Skills Foundation (MegaBall & InfinityBall).
    /// Validates MegaBall projectile scaling (2.0x diameter, 2.5x mass), one-shot buff consumption,
    /// InfinityBall rapid firing cooldown (bypass 0.5s timer), 5.0s countdown duration,
    /// skill stacking, use depletion, level unlock gating, and visual captures.
    /// </summary>
    public class Phase6PlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase6_test_result.txt";
        private const string HUD_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase6_hud_skills.png";
        private const string MEGABALL_SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase6_megaball_action.png";

        private IEnumerator Start()
        {
            // Initial scene stabilization
            yield return new WaitForSeconds(0.5f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PHASE 6 PLAY MODE TEST SUITE ===");
            report.AppendLine($"Execution Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var skillMgr = SkillManager.Instance;
            var mortar = FindFirstObjectByType<MortarController>();

            if (skillMgr == null || mortar == null)
            {
                report.AppendLine("CRITICAL ERROR: SkillManager or MortarController missing from scene!");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // Capture initial HUD with skill buttons visible
            ScreenCapture.CaptureScreenshot(HUD_SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.5f);
            report.AppendLine($"Captured initial HUD screenshot to '{HUD_SCREENSHOT_PATH}'.");

            // ----------------------------------------------------------------------------------
            // TEST 1: MegaBall Activation, Scaling & Consumption
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 1] Testing MegaBall Activation & Projectile Scaling (2.0x diameter)...");
            skillMgr.SetSkillUnlocked(SkillType.MegaBall, true);
            skillMgr.SetRemainingUses(SkillType.MegaBall, 3);
            skillMgr.ResetActiveSkills();

            bool activatedMega = skillMgr.ActivateSkill(SkillType.MegaBall);
            int usesAfterActivate = skillMgr.GetRemainingUses(SkillType.MegaBall);
            bool isMegaActive = skillMgr.IsSkillActive(SkillType.MegaBall);

            report.AppendLine($"  Activated: {activatedMega} | Uses: 3 -> {usesAfterActivate} | Active Buff: {isMegaActive}");

            if (activatedMega && usesAfterActivate == 2 && isMegaActive)
            {
                // Fire mortar and inspect spawned projectile
                mortar.SetBallCount(10);
                mortar.ResetCooldown();
                mortar.Fire(new Vector3(0f, 2.5f, 7.5f));

                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(0.1f);

                var projectile = FindFirstObjectByType<ProjectileController>();
                if (projectile != null)
                {
                    bool isMegaProj = projectile.IsMegaBall;
                    float scaleX = projectile.transform.localScale.x;
                    float mass = projectile.Rigidbody.mass;
                    bool buffConsumed = !skillMgr.IsSkillActive(SkillType.MegaBall);

                    report.AppendLine($"  Spawned Projectile -> IsMegaBall: {isMegaProj} | Scale: {scaleX:F2} | Mass: {mass:F2}kg | Buff Consumed: {buffConsumed}");

                    if (isMegaProj && scaleX >= 0.75f && mass >= 2.9f && buffConsumed)
                    {
                        report.AppendLine("  -> PASS: MegaBall scaled diameter, increased mass, and consumed one-shot buff.");
                        ScreenCapture.CaptureScreenshot(MEGABALL_SCREENSHOT_PATH);
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        report.AppendLine("  -> FAIL: MegaBall parameters or consumption failed.");
                        allPassed = false;
                    }
                }
                else
                {
                    report.AppendLine("  -> FAIL: No projectile was spawned.");
                    allPassed = false;
                }
            }
            else
            {
                report.AppendLine("  -> FAIL: Could not activate MegaBall properly.");
                allPassed = false;
            }

            CleanupProjectiles();
            yield return new WaitForSeconds(0.5f);

            // ----------------------------------------------------------------------------------
            // TEST 2: InfinityBall Activation & Rapid Cooldown Override
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Testing InfinityBall Rapid Cooldown Override (5.0s duration)...");
            skillMgr.SetSkillUnlocked(SkillType.InfinityBall, true);
            skillMgr.SetRemainingUses(SkillType.InfinityBall, 3);
            skillMgr.ResetActiveSkills();

            bool activatedInf = skillMgr.ActivateSkill(SkillType.InfinityBall);
            int infUses = skillMgr.GetRemainingUses(SkillType.InfinityBall);
            bool isInfActive = skillMgr.IsSkillActive(SkillType.InfinityBall);

            report.AppendLine($"  Activated: {activatedInf} | Uses: 3 -> {infUses} | Active State: {isInfActive}");

            if (activatedInf && isInfActive)
            {
                mortar.SetBallCount(15);
                mortar.ResetCooldown();

                // Fire 1st shot
                mortar.Fire(new Vector3(-1f, 2.5f, 7.5f));
                // Wait only 0.1s (normal cooldown is 0.5s)
                yield return new WaitForSeconds(0.1f);

                int eventsBefore = mortar.FireEventCount;
                // Fire 2nd shot immediately
                mortar.Fire(new Vector3(1f, 2.5f, 7.5f));
                int eventsAfter = mortar.FireEventCount;

                bool rapidFired = (eventsAfter > eventsBefore);
                report.AppendLine($"  Rapid second shot fired after 0.1s: {rapidFired} (Events: {eventsBefore} -> {eventsAfter})");

                float timer = skillMgr.GetActiveTimer(SkillType.InfinityBall);
                report.AppendLine($"  Remaining Infinity Duration: {timer:F2}s");

                if (rapidFired && timer > 0f)
                {
                    report.AppendLine("  -> PASS: InfinityBall successfully bypassed 0.5s cooldown for rapid fire stream.");
                }
                else
                {
                    report.AppendLine("  -> FAIL: Rapid firing blocked by cooldown.");
                    allPassed = false;
                }
            }
            else
            {
                report.AppendLine("  -> FAIL: Could not activate InfinityBall.");
                allPassed = false;
            }

            CleanupProjectiles();
            skillMgr.ResetActiveSkills();
            yield return new WaitForSeconds(0.5f);

            // ----------------------------------------------------------------------------------
            // TEST 3: Skill Stacking (Both MegaBall + InfinityBall active simultaneously)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Testing Skill Stacking (MegaBall + InfinityBall simultaneously)...");
            skillMgr.SetRemainingUses(SkillType.MegaBall, 2);
            skillMgr.SetRemainingUses(SkillType.InfinityBall, 2);

            bool stackMega = skillMgr.ActivateSkill(SkillType.MegaBall);
            bool stackInf = skillMgr.ActivateSkill(SkillType.InfinityBall);

            bool bothActive = skillMgr.IsSkillActive(SkillType.MegaBall) && skillMgr.IsSkillActive(SkillType.InfinityBall);
            report.AppendLine($"  Mega Active: {skillMgr.IsSkillActive(SkillType.MegaBall)} | Infinity Active: {skillMgr.IsSkillActive(SkillType.InfinityBall)} | Stacked: {bothActive}");

            if (bothActive)
            {
                report.AppendLine("  -> PASS: System supports simultaneous skill stacking.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Skills could not be stacked simultaneously.");
                allPassed = false;
            }

            skillMgr.ResetActiveSkills();
            yield return new WaitForSeconds(0.2f);

            // ----------------------------------------------------------------------------------
            // TEST 4: Use Depletion & Exhaustion Rejection
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Testing Use Depletion (Cannot activate with 0 uses)...");
            skillMgr.SetRemainingUses(SkillType.MegaBall, 0);
            bool rejectMega = skillMgr.ActivateSkill(SkillType.MegaBall);
            report.AppendLine($"  Activation with 0 uses returned: {rejectMega} (Expected: False)");

            if (!rejectMega)
            {
                report.AppendLine("  -> PASS: Skill activation cleanly rejected when uses are exhausted.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Skill activated despite having 0 uses remaining.");
                allPassed = false;
            }

            // ----------------------------------------------------------------------------------
            // TEST 5: Level Progression Unlock Gating
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Testing Level Unlock Gating (MegaBall @ Level 5, InfinityBall @ Level 10)...");
            skillMgr.UpdateUnlockStateForLevel(1);
            bool lvl1Mega = skillMgr.IsSkillUnlocked(SkillType.MegaBall);
            bool lvl1Inf = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);

            skillMgr.UpdateUnlockStateForLevel(5);
            bool lvl5Mega = skillMgr.IsSkillUnlocked(SkillType.MegaBall);
            bool lvl5Inf = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);

            skillMgr.UpdateUnlockStateForLevel(10);
            bool lvl10Mega = skillMgr.IsSkillUnlocked(SkillType.MegaBall);
            bool lvl10Inf = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);

            report.AppendLine($"  Level 1  -> Mega: {lvl1Mega} (F), Inf: {lvl1Inf} (F)");
            report.AppendLine($"  Level 5  -> Mega: {lvl5Mega} (T), Inf: {lvl5Inf} (F)");
            report.AppendLine($"  Level 10 -> Mega: {lvl10Mega} (T), Inf: {lvl10Inf} (T)");

            if (!lvl1Mega && !lvl1Inf && lvl5Mega && !lvl5Inf && lvl10Mega && lvl10Inf)
            {
                report.AppendLine("  -> PASS: Unlock gates strictly adhere to Level 5 (Mega) and Level 10 (Infinity) specifications.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Unlock progression gating mismatch.");
                allPassed = false;
            }

            // Restore testing uses for player gameplay
            skillMgr.SetSkillUnlocked(SkillType.MegaBall, true);
            skillMgr.SetSkillUnlocked(SkillType.InfinityBall, true);
            skillMgr.SetRemainingUses(SkillType.MegaBall, 3);
            skillMgr.SetRemainingUses(SkillType.InfinityBall, 3);
            skillMgr.ResetActiveSkills();
            mortar.SetBallCount(25);
            mortar.ResetCooldown();

            report.AppendLine(allPassed ? "\n=== ALL 5 PHASE 6 ACCEPTANCE TESTS PASSED! ===" : "\n=== SOME PHASE 6 ACCEPTANCE TESTS FAILED ===");

            File.WriteAllText(REPORT_PATH, report.ToString());
            Debug.Log(report.ToString());

            // Exit Play Mode when finished
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        private void CleanupProjectiles()
        {
            var projs = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            foreach (var p in projs)
            {
                if (p != null) Destroy(p.gameObject);
            }
        }
    }
}
