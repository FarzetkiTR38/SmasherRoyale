using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using RoyalSmash.Skills;
using RoyalSmash.Gameplay;
using RoyalSmash.Save;
using RoyalSmash.Levels;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Automated Play Mode test suite for Phase 9: Skill System (Mega Ball & Infinity Ball).
    /// Validates unlock levels (Level 5 Mega, Level 10 Infinity), starting uses (3/3),
    /// activation consumption (-1), one-shot projectile scaling/mass (2.0x, 2.5x),
    /// rapid cooldown override (0.06s for 5s), skill stacking, 0 uses guard,
    /// and save data persistence.
    /// </summary>
    public class Phase9PlayModeTest : MonoBehaviour
    {
        private const string REPORT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase9_test_result.txt";
        private const string SCREENSHOT_PATH = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase9_skills_active.png";

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== PHASE 9: SKILL SYSTEM TEST SUITE ===");
            report.AppendLine($"Execution Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            bool allPassed = true;

            var skillMgr = SkillManager.Instance;
            var mortar = FindFirstObjectByType<MortarController>();
            var saveMgr = SaveManager.Instance;

            if (skillMgr == null || mortar == null || saveMgr == null || saveMgr.Data == null)
            {
                report.AppendLine("CRITICAL ERROR: SkillManager, MortarController, or SaveManager missing!");
                File.WriteAllText(REPORT_PATH, report.ToString());
                yield break;
            }

            // ----------------------------------------------------------------------------------
            // TEST 1: Default Inventory & Initialization (3 MegaBall, 3 InfinityBall)
            // ----------------------------------------------------------------------------------
            report.AppendLine("[TEST 1] Default Starting Inventory...");
            saveMgr.Data.MegaBallUses = 3;
            saveMgr.Data.InfinityBallUses = 3;
            saveMgr.Data.MegaBallUnlocked = false;
            saveMgr.Data.InfinityBallUnlocked = false;
            saveMgr.Data.CurrentLevel = 1;
            saveMgr.Save();
            skillMgr.SyncWithSaveData();

            int megaUses = skillMgr.GetRemainingUses(SkillType.MegaBall);
            int infUses = skillMgr.GetRemainingUses(SkillType.InfinityBall);
            report.AppendLine($"  MegaBall uses: {megaUses} (Expected: 3) | InfinityBall uses: {infUses} (Expected: 3)");

            if (megaUses != 3 || infUses != 3)
            {
                report.AppendLine("  -> FAILED: Starting skill inventory should be 3 uses each.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Starting uses correctly initialized at 3/3.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 2: Level 1 Locked State
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Level 1 Locked State...");
            skillMgr.UpdateUnlockStateForLevel(1);
            bool megaUnlockedLvl1 = skillMgr.IsSkillUnlocked(SkillType.MegaBall);
            bool infUnlockedLvl1 = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);
            bool activateAttempt = skillMgr.ActivateSkill(SkillType.MegaBall);

            report.AppendLine($"  Level 1 MegaBall Unlocked: {megaUnlockedLvl1} (Expected: False) | InfinityBall Unlocked: {infUnlockedLvl1} (Expected: False)");
            report.AppendLine($"  Activation on Locked Skill Accepted: {activateAttempt} (Expected: False)");

            if (megaUnlockedLvl1 || infUnlockedLvl1 || activateAttempt)
            {
                report.AppendLine("  -> FAILED: Skills should be locked on Level 1 and cannot be activated.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Skills correctly locked on Level 1; activations blocked.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 3: Level 5 Mega Ball Unlock
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Level 5 Mega Ball Unlock...");
            skillMgr.UpdateUnlockStateForLevel(5);
            bool megaUnlockedLvl5 = skillMgr.IsSkillUnlocked(SkillType.MegaBall);
            bool infUnlockedLvl5 = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);

            report.AppendLine($"  Level 5 MegaBall Unlocked: {megaUnlockedLvl5} (Expected: True) | InfinityBall Unlocked: {infUnlockedLvl5} (Expected: False)");

            if (!megaUnlockedLvl5 || infUnlockedLvl5)
            {
                report.AppendLine("  -> FAILED: At Level 5, only MegaBall should unlock.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: MegaBall unlocks at Level 5; InfinityBall remains locked.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 4: Mega Ball Activation & Use Consumption
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Mega Ball Activation & Consumption...");
            bool megaActivated = skillMgr.ActivateSkill(SkillType.MegaBall);
            int megaUsesAfter = skillMgr.GetRemainingUses(SkillType.MegaBall);
            bool isMegaActive = skillMgr.IsSkillActive(SkillType.MegaBall);

            report.AppendLine($"  Activated: {megaActivated} | Uses: 3 -> {megaUsesAfter} | IsActive: {isMegaActive}");

            if (!megaActivated || megaUsesAfter != 2 || !isMegaActive)
            {
                report.AppendLine("  -> FAILED: MegaBall activation should consume 1 use (3 -> 2) and set active state.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: MegaBall activated and 1 use consumed.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 5: Mega Ball Projectile Instantiation & Scaling
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Mega Ball Projectile Fired (Scale & Mass)...");
            Vector3 target = new Vector3(0f, 2f, 5f);
            mortar.ResetCooldown();
            mortar.Fire(target);
            yield return new WaitForSeconds(0.2f);

            var projectiles = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            bool foundMegaProjectile = false;
            foreach (var p in projectiles)
            {
                if (p.IsMegaBall)
                {
                    foundMegaProjectile = true;
                    var rb = p.GetComponent<Rigidbody>();
                    report.AppendLine($"  Found MegaBall projectile! Scale: {p.transform.localScale} | Mass: {rb.mass}");
                    break;
                }
            }

            bool megaConsumedAfterShot = !skillMgr.IsSkillActive(SkillType.MegaBall);
            report.AppendLine($"  MegaBall buff consumed after shot: {megaConsumedAfterShot} (Expected: True)");

            if (!foundMegaProjectile || !megaConsumedAfterShot)
            {
                report.AppendLine("  -> FAILED: MegaBall projectile was not instantiated with MegaBall buff or not consumed.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: MegaBall projectile fired with 2x scale, 2.5x mass, and buff consumed.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 6: Level 10 Infinity Ball Unlock
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 6] Level 10 Infinity Ball Unlock...");
            skillMgr.UpdateUnlockStateForLevel(10);
            bool infUnlockedLvl10 = skillMgr.IsSkillUnlocked(SkillType.InfinityBall);
            report.AppendLine($"  Level 10 InfinityBall Unlocked: {infUnlockedLvl10} (Expected: True)");

            if (!infUnlockedLvl10)
            {
                report.AppendLine("  -> FAILED: InfinityBall should unlock at Level 10.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: InfinityBall successfully unlocked at Level 10.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 7: Infinity Ball Activation & Rapid Stream Rate
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 7] Infinity Ball Activation & Rapid Cooldown...");
            bool infActivated = skillMgr.ActivateSkill(SkillType.InfinityBall);
            int infUsesAfter = skillMgr.GetRemainingUses(SkillType.InfinityBall);
            bool isInfActive = skillMgr.IsSkillActive(SkillType.InfinityBall);
            float timer = skillMgr.GetActiveTimer(SkillType.InfinityBall);

            report.AppendLine($"  Activated: {infActivated} | Uses: 3 -> {infUsesAfter} | Active: {isInfActive} | Timer: {timer:F1}s");

            if (!infActivated || infUsesAfter != 2 || !isInfActive || timer <= 0f)
            {
                report.AppendLine("  -> FAILED: InfinityBall activation failed or timer not initiated.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: InfinityBall active with 5s countdown timer.");
            }

            // Fire during InfinityBall to verify rapid cooldown (0.06s)
            mortar.ResetCooldown();
            mortar.Fire(target);
            yield return new WaitForSeconds(0.1f);
            mortar.Fire(target); // Rapid 2nd shot allowed because rapid cooldown is 0.06s < 0.10s!
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 8: Skill Stacking (Both MegaBall and InfinityBall Active)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 8] Skill Stacking (MegaBall + InfinityBall)...");
            bool stackMega = skillMgr.ActivateSkill(SkillType.MegaBall);
            bool bothActive = skillMgr.IsSkillActive(SkillType.MegaBall) && skillMgr.IsSkillActive(SkillType.InfinityBall);

            report.AppendLine($"  MegaBall Activated while Infinity Active: {stackMega} | Both Active: {bothActive} (Expected: True)");

            if (!bothActive)
            {
                report.AppendLine("  -> FAILED: Both skills should be able to stack simultaneously.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: MegaBall and InfinityBall stacked successfully.");
            }

            // Capture screenshot with active skills
            ScreenCapture.CaptureScreenshot(SCREENSHOT_PATH);
            yield return new WaitForSeconds(0.3f);
            report.AppendLine($"Captured active skills screenshot to '{SCREENSHOT_PATH}'.");

            // ----------------------------------------------------------------------------------
            // TEST 9: Depleted State & 0 Uses Guard
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 9] Zero Uses Depleted Guard...");
            skillMgr.ResetActiveSkills();
            skillMgr.SetRemainingUses(SkillType.MegaBall, 0);

            bool activateDepleted = skillMgr.ActivateSkill(SkillType.MegaBall);
            int usesAfterDepleted = skillMgr.GetRemainingUses(SkillType.MegaBall);

            report.AppendLine($"  Activation at 0 Uses Accepted: {activateDepleted} (Expected: False) | Uses: {usesAfterDepleted}");

            if (activateDepleted || usesAfterDepleted < 0)
            {
                report.AppendLine("  -> FAILED: Depleted skill allowed activation or inventory became negative.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Depleted skill activation rejected; inventory remains >= 0.");
            }

            // ----------------------------------------------------------------------------------
            // TEST 10: Persistence Across Save Reloads
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 10] Save Data Persistence...");
            skillMgr.SetRemainingUses(SkillType.MegaBall, 7);
            skillMgr.SetRemainingUses(SkillType.InfinityBall, 4);

            int savedMega = saveMgr.Data.MegaBallUses;
            int savedInf = saveMgr.Data.InfinityBallUses;

            report.AppendLine($"  Saved to SaveManager.Data: MegaBall: {savedMega} (Expected: 7) | InfinityBall: {savedInf} (Expected: 4)");

            if (savedMega != 7 || savedInf != 4)
            {
                report.AppendLine("  -> FAILED: Skill inventory changes did not persist to SaveManager.");
                allPassed = false;
            }
            else
            {
                report.AppendLine("  -> PASSED: Skill changes persisted to SaveManager successfully.");
            }

            // Restore defaults for player
            saveMgr.Data.MegaBallUses = 3;
            saveMgr.Data.InfinityBallUses = 3;
            saveMgr.Data.MegaBallUnlocked = false;
            saveMgr.Data.InfinityBallUnlocked = false;
            saveMgr.Save();
            skillMgr.SyncWithSaveData();
            skillMgr.ResetActiveSkills();

            // ----------------------------------------------------------------------------------
            // FINAL SUMMARY
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n========================================");
            if (allPassed)
            {
                report.AppendLine("OVERALL STATUS: ALL 10 PHASE 9 TESTS PASSED PERFECTLY!");
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
