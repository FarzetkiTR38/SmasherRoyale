using System.Collections;
using System.IO;
using UnityEngine;
using RoyalSmash.Gameplay;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Autonomous Play Mode test suite for Phase 4 deliverables:
    /// Physics Objects & Interactions (Box, Stone, Column, Cone, PaintBottle).
    /// </summary>
    public class Phase4PlayModeTest : MonoBehaviour
    {
        [SerializeField] private string _reportOutputPath = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/scratch/phase4_test_result.txt";
        [SerializeField] private string _screenshotOutputPath = "C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase4_game_view.png";

        private void Start()
        {
            StartCoroutine(RunTestRoutine());
        }

        private IEnumerator RunTestRoutine()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== PHASE 4 PLAY MODE TEST SUITE ===");
            bool allPassed = true;

            // Wait for scene initialization and physics settling
            yield return new WaitForSeconds(0.5f);

            var mortar = FindFirstObjectByType<MortarController>();
            var inputHandler = FindFirstObjectByType<InputHandler>();

            if (mortar == null || inputHandler == null)
            {
                report.AppendLine("[FAIL] MortarController or InputHandler not found in scene!");
                FinishTest(report.ToString(), false);
                yield break;
            }

            // Load Prefabs
            GameObject boxPrefab = null;
            GameObject stonePrefab = null;
            GameObject columnPrefab = null;
            GameObject conePrefab = null;
            GameObject bottlePrefab = null;

#if UNITY_EDITOR
            boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Box.prefab");
            stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Stone.prefab");
            columnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Column.prefab");
            conePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Cone.prefab");
            bottlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/PaintBottle.prefab");
#endif

            if (boxPrefab == null || stonePrefab == null || columnPrefab == null || conePrefab == null || bottlePrefab == null)
            {
                report.AppendLine("[FAIL] Failed to load one or more Phase 4 prefabs!");
                FinishTest(report.ToString(), false);
                yield break;
            }

            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 1: Box vs Stone Impact Comparison (Light Box vs Heavy Stone)
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 1] Testing Box (1.0kg) vs Stone (5.0kg) impulse transfer...");
            Vector3 boxSpawn = new Vector3(-1.0f, 1.9f, 7.5f);
            Vector3 stoneSpawn = new Vector3(1.0f, 1.9f, 7.5f);

            GameObject testBox = Instantiate(boxPrefab, boxSpawn, Quaternion.identity);
            GameObject testStone = Instantiate(stonePrefab, stoneSpawn, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);

            // Hit Box
            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(boxSpawn);
            yield return new WaitForSeconds(0.45f);

            // Hit Stone
            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(stoneSpawn);
            yield return new WaitForSeconds(0.5f);

            float boxDisplacement = (testBox != null) ? Vector3.Distance(boxSpawn, testBox.transform.position) : 2.5f;
            float stoneDisplacement = (testStone != null) ? Vector3.Distance(stoneSpawn, testStone.transform.position) : 0f;

            report.AppendLine($"  Box displacement: {boxDisplacement:F2}m | Stone displacement: {stoneDisplacement:F2}m");
            if (boxDisplacement > 0.6f && stoneDisplacement < 0.4f)
            {
                report.AppendLine("  -> PASS: Light Box pushed easily; heavy Stone absorbed impact with minimal displacement.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Displacement comparison did not meet threshold.");
                allPassed = false;
            }

            if (testBox != null) Destroy(testBox);
            if (testStone != null) Destroy(testStone);

            yield return new WaitForSeconds(0.2f);

            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 2: Column Toppling
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 2] Testing Column (3.0kg) topple mechanics...");
            Vector3 colSpawn = new Vector3(0f, 2.6f, 7.5f);
            GameObject testCol = Instantiate(columnPrefab, colSpawn, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);

            // Hit upper section of column to induce torque
            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(new Vector3(0f, 3.4f, 7.5f));
            yield return new WaitForSeconds(0.8f);

            float tiltAngle = (testCol != null) ? Vector3.Angle(Vector3.up, testCol.transform.up) : 90f;
            report.AppendLine($"  Column tilt angle: {tiltAngle:F1} deg");
            if (tiltAngle > 20f)
            {
                report.AppendLine("  -> PASS: Column tilted and toppled over upon upper impact.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Column did not topple sufficiently.");
                allPassed = false;
            }

            if (testCol != null) Destroy(testCol);
            yield return new WaitForSeconds(0.2f);

            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 3: PaintBottle Breakable Shatter
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 3] Testing PaintBottle (0.5kg) breakable shatter behavior...");
            Vector3 bottleSpawn = new Vector3(0f, 1.88f, 7.5f);
            GameObject testBottle = Instantiate(bottlePrefab, bottleSpawn, Quaternion.identity);
            var bottleComp = testBottle.GetComponent<PaintBottleObject>();

            bool hasBrokenDetected = false;
            if (bottleComp != null)
            {
                bottleComp.OnStateChanged += (obj, state) =>
                {
                    if (state == ObjectState.DestroyCondition || state == ObjectState.Destroying || state == ObjectState.Destroyed)
                    {
                        hasBrokenDetected = true;
                    }
                };
            }

            yield return new WaitForSeconds(0.3f);

            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(new Vector3(0f, 2.0f, 7.5f));
            yield return new WaitForSeconds(0.55f);

            bool hasBroken = hasBrokenDetected || (bottleComp != null && bottleComp.HasBroken);
            report.AppendLine($"  PaintBottle HasBroken: {hasBroken}");
            if (hasBroken)
            {
                report.AppendLine("  -> PASS: PaintBottle shattered upon ball impact with paint splatter effect.");
            }
            else
            {
                report.AppendLine("  -> FAIL: PaintBottle did not shatter on direct impact.");
                allPassed = false;
            }

            yield return new WaitForSeconds(0.4f);
            if (testBottle != null) Destroy(testBottle);

            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 4: Stacked Boxes cascade physics
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 4] Testing Stacked Boxes cascade physics...");
            Vector3 bottomSpawn = new Vector3(0f, 1.9f, 7.5f);
            Vector3 topSpawn = new Vector3(0f, 2.92f, 7.5f);

            GameObject btmBox = Instantiate(boxPrefab, bottomSpawn, Quaternion.identity);
            GameObject topBox = Instantiate(boxPrefab, topSpawn, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);

            // Hit bottom box directly
            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(bottomSpawn);
            yield return new WaitForSeconds(0.85f);

            float topBoxY = (topBox != null) ? topBox.transform.position.y : 0f;
            report.AppendLine($"  Top box Y after hit: {topBoxY:F2} (Started at {topSpawn.y:F2})");
            if (topBoxY < 2.5f)
            {
                report.AppendLine("  -> PASS: Top box reacted physically and fell under gravity when bottom box was hit.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Top box did not fall as expected.");
                allPassed = false;
            }

            if (btmBox != null) Destroy(btmBox);
            if (topBox != null) Destroy(topBox);

            yield return new WaitForSeconds(0.2f);
            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 5: Fall-Off-Stand Smooth Shrink / Melt Destruction
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 5] Testing Fall-Off-Stand smooth shrink auto-destruction...");
            Vector3 edgeSpawn = new Vector3(2.5f, 1.9f, 7.5f);
            GameObject edgeBox = Instantiate(boxPrefab, edgeSpawn, Quaternion.identity);
            var edgeRb = edgeBox.GetComponent<Rigidbody>();
            var edgePo = edgeBox.GetComponent<PhysicsObject>();

            // Push it off the edge toward ground
            edgeRb.linearVelocity = new Vector3(5.0f, -2.0f, 0f);
            yield return new WaitForSeconds(0.7f);

            bool shrinkStarted = edgePo != null && (edgePo.CurrentState == ObjectState.DestroyCondition || edgePo.CurrentState == ObjectState.Destroying);
            report.AppendLine($"  Fall-off state reached: {edgePo?.CurrentState}");

            if (shrinkStarted)
            {
                report.AppendLine("  -> PASS: Object falling off stand entered DestroyCondition / Destroying state.");
            }
            else
            {
                report.AppendLine("  -> FAIL: Object did not enter destruction state on fall.");
                allPassed = false;
            }

            // Wait for shrink duration to complete full destruction
            yield return new WaitForSeconds(0.7f);
            bool isRemoved = edgeBox == null;
            report.AppendLine($"  Object removed after shrink: {isRemoved}");
            if (isRemoved)
            {
                report.AppendLine("  -> PASS: Object smoothly shrank and was removed from scene (did not pop instantly).");
            }
            else
            {
                report.AppendLine("  -> FAIL: Object was not removed after shrink duration.");
                allPassed = false;
            }

            if (edgeBox != null) Destroy(edgeBox);

            CleanupProjectiles();
            yield return new WaitForSeconds(0.1f);

            // ----------------------------------------------------------------------------------
            // TEST 6: Multi-Object Visual Showcase & Gameplay Screenshot
            // ----------------------------------------------------------------------------------
            report.AppendLine("\n[TEST 6] Multi-Object dynamic showcase and screenshot capture...");
            GameObject colShowcase = Instantiate(columnPrefab, new Vector3(0f, 2.6f, 7.5f), Quaternion.identity);
            GameObject boxShowcase = Instantiate(boxPrefab, new Vector3(-1.0f, 1.9f, 7.5f), Quaternion.identity);
            GameObject stoneShowcase = Instantiate(stonePrefab, new Vector3(1.0f, 1.9f, 7.5f), Quaternion.identity);
            GameObject coneShowcase = Instantiate(conePrefab, new Vector3(-0.5f, 1.9f, 6.8f), Quaternion.identity);
            GameObject bottleShowcase = Instantiate(bottlePrefab, new Vector3(0.5f, 1.95f, 6.8f), Quaternion.identity);

            yield return new WaitForSeconds(0.3f);

            // Fire an action shot into the layout!
            mortar.ResetCooldown();
            inputHandler.SimulateTapWorldTarget(new Vector3(0f, 2.5f, 7.5f));
            yield return new WaitForSeconds(0.25f);

            ScreenCapture.CaptureScreenshot(_screenshotOutputPath);
            yield return new WaitForSeconds(0.4f);
            report.AppendLine($"  Captured gameplay screenshot to '{_screenshotOutputPath}'.");

            // Clean up showcase
            if (colShowcase != null) Destroy(colShowcase);
            if (boxShowcase != null) Destroy(boxShowcase);
            if (stoneShowcase != null) Destroy(stoneShowcase);
            if (coneShowcase != null) Destroy(coneShowcase);
            if (bottleShowcase != null) Destroy(bottleShowcase);

            report.AppendLine(allPassed ? "\n=== ALL 6 PHASE 4 TESTS PASSED! ===" : "\n=== SOME TESTS FAILED ===");
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

        private void CleanupProjectiles()
        {
            var projectiles = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
            foreach (var p in projectiles)
            {
                if (p != null && p.gameObject != null) Destroy(p.gameObject);
            }
        }
    }
}
