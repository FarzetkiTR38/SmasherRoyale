using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Levels;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite for Phase 19: Level Content & Balancing.
    /// Runs 10 test cases verifying the 30-level progression map, milestone events, and Level 30 finale rules.
    /// </summary>
    public static class Phase19PlayModeTest
    {
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static List<string> _testLogs = new List<string>();

        [MenuItem("RoyalSmash/Phase 19/Run All Level Tests")]
        public static string RunAllTests()
        {
            _passedTests = 0;
            _failedTests = 0;
            _testLogs.Clear();

            Debug.Log("==================================================");
            Debug.Log("  STARTING PHASE 19 AUTOMATED LEVEL TESTS (10 TESTS)  ");
            Debug.Log("==================================================");

            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(
                "Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");

            Test01_All30LevelsExist(db);
            Test02_BallCountCurve(db);
            Test03_ObjectCountsAndValidity(db);
            Test04_Level05MegaBallUnlock(db);
            Test05_Level10InfinityBallAndUsername(db);
            Test06_Level15PaintBottleIntro(db);
            Test07_DualStandLevels(db);
            Test08_MultiLayerLaterLevels(db);
            Test09_Level30GrandFinaleRule(db);
            Test10_LevelLoaderExecution(db);

            string summary = $"Phase 19 Test Results: {_passedTests}/10 PASSED, {_failedTests} FAILED.";
            Debug.Log("==================================================");
            Debug.Log($"  {summary}");
            Debug.Log("==================================================");

            return summary;
        }

        private static void Assert(bool condition, string testName, string details)
        {
            if (condition)
            {
                _passedTests++;
                string msg = $"[PASS] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.Log(msg);
            }
            else
            {
                _failedTests++;
                string msg = $"[FAIL] {testName}: {details}";
                _testLogs.Add(msg);
                Debug.LogError(msg);
            }
        }

        private static void Test01_All30LevelsExist(LevelDatabase db)
        {
            bool hasDb = db != null;
            bool count30 = hasDb && db.TotalLevels == 30;
            bool allValid = true;

            if (count30)
            {
                for (int i = 1; i <= 30; i++)
                {
                    if (db.GetLevel(i) == null)
                    {
                        allValid = false;
                        break;
                    }
                }
            }
            else
            {
                allValid = false;
            }

            Assert(allValid, "Test 1: All30LevelsExist",
                $"LevelDatabase contains exactly 30 valid LevelData assets (Found: {db?.TotalLevels ?? 0}).");
        }

        private static void Test02_BallCountCurve(LevelDatabase db)
        {
            if (db == null || db.TotalLevels < 30)
            {
                Assert(false, "Test 2: BallCountCurve", "Database not loaded.");
                return;
            }

            var l1 = db.GetLevel(1);
            var l30 = db.GetLevel(30);
            bool curveValid = l1.BallCount >= 22 && l30.BallCount <= 15 && l1.BallCount > l30.BallCount;

            Assert(curveValid, "Test 2: BallCountCurve",
                $"Ball economy tightens from Level 1 ({l1.BallCount} balls) down to Level 30 ({l30.BallCount} balls).");
        }

        private static void Test03_ObjectCountsAndValidity(LevelDatabase db)
        {
            if (db == null)
            {
                Assert(false, "Test 3: ObjectCountsAndValidity", "Database not loaded.");
                return;
            }

            bool allHaveObjects = true;
            for (int i = 1; i <= 30; i++)
            {
                var lvl = db.GetLevel(i);
                if (lvl.Objects == null || lvl.Objects.Length == 0 || lvl.Stands == null || lvl.Stands.Length == 0)
                {
                    allHaveObjects = false;
                    break;
                }
            }

            Assert(allHaveObjects, "Test 3: ObjectCountsAndValidity",
                "All 30 levels possess valid stand configurations and positive target object counts.");
        }

        private static void Test04_Level05MegaBallUnlock(LevelDatabase db)
        {
            var l5 = db?.GetLevel(5);
            bool hasEvent = false;
            bool hasStone = false;

            if (l5 != null && l5.SpecialEvents != null)
            {
                foreach (var ev in l5.SpecialEvents)
                {
                    if (ev.Type == LevelEventType.UnlockMegaBall) hasEvent = true;
                }
            }
            if (l5 != null && l5.Objects != null)
            {
                foreach (var obj in l5.Objects)
                {
                    if (obj.Type == ObjectType.Stone) hasStone = true;
                }
            }

            Assert(hasEvent && hasStone, "Test 4: Level05MegaBallUnlock",
                "Level 5 unlocks MegaBall and introduces Stone target objects.");
        }

        private static void Test05_Level10InfinityBallAndUsername(LevelDatabase db)
        {
            var l10 = db?.GetLevel(10);
            bool hasInf = false;
            bool hasUser = false;

            if (l10 != null && l10.SpecialEvents != null)
            {
                foreach (var ev in l10.SpecialEvents)
                {
                    if (ev.Type == LevelEventType.UnlockInfinityBall) hasInf = true;
                    if (ev.Type == LevelEventType.PromptUsername) hasUser = true;
                }
            }

            Assert(hasInf && hasUser, "Test 5: Level10InfinityBallAndUsername",
                "Level 10 unlocks InfinityBall and prompts for nickname/username.");
        }

        private static void Test06_Level15PaintBottleIntro(LevelDatabase db)
        {
            var l15 = db?.GetLevel(15);
            bool hasBottle = false;

            if (l15 != null && l15.Objects != null)
            {
                foreach (var obj in l15.Objects)
                {
                    if (obj.Type == ObjectType.PaintBottle) hasBottle = true;
                }
            }

            Assert(hasBottle, "Test 6: Level15PaintBottleIntro",
                "Level 15 milestone introduces breakable PaintBottle targets.");
        }

        private static void Test07_DualStandLevels(LevelDatabase db)
        {
            var l12 = db?.GetLevel(12);
            var l22 = db?.GetLevel(22);

            bool l12Dual = l12 != null && l12.Stands != null && l12.Stands.Length >= 2;
            bool l22Dual = l22 != null && l22.Stands != null && l22.Stands.Length >= 2;

            Assert(l12Dual && l22Dual, "Test 7: DualStandLevels",
                "Levels 12 and 22 feature dual stand puzzle layouts.");
        }

        private static void Test08_MultiLayerLaterLevels(LevelDatabase db)
        {
            var l25 = db?.GetLevel(25);
            bool hasDepthVariance = false;

            if (l25 != null && l25.Objects != null && l25.Objects.Length > 1)
            {
                float z0 = l25.Objects[0].Position.z;
                foreach (var obj in l25.Objects)
                {
                    if (Mathf.Abs(obj.Position.z - z0) > 0.3f)
                    {
                        hasDepthVariance = true;
                        break;
                    }
                }
            }

            Assert(hasDepthVariance, "Test 8: MultiLayerLaterLevels",
                "Later fortress levels feature dual-row front and back depth layers.");
        }

        private static void Test09_Level30GrandFinaleRule(LevelDatabase db)
        {
            var l30 = db?.GetLevel(30);
            bool isLast = db != null && db.IsLastLevel(30);
            bool hasFinalEvent = false;

            if (l30 != null && l30.SpecialEvents != null)
            {
                foreach (var ev in l30.SpecialEvents)
                {
                    if (ev.Type == LevelEventType.FinalLevel) hasFinalEvent = true;
                }
            }

            Assert(isLast && hasFinalEvent, "Test 9: Level30GrandFinaleRule",
                "Level 30 marked as FinalLevel with no 'Next Level' progression per Section 1.2 Rule 14.");
        }

        private static void Test10_LevelLoaderExecution(LevelDatabase db)
        {
            var loaderGo = new GameObject("Test_LevelLoader");
            var loader = loaderGo.AddComponent<LevelLoader>();

            var l1 = db?.GetLevel(1);
            bool noThrow = true;
            try
            {
                loader.LoadLevel(l1);
                loader.ClearLevel();
            }
            catch (Exception ex)
            {
                noThrow = false;
                Debug.LogException(ex);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(loaderGo);
            }

            Assert(noThrow, "Test 10: LevelLoaderExecution",
                "LevelLoader instantiated and cleared Level 1 cleanly without runtime exceptions.");
        }
    }
}
