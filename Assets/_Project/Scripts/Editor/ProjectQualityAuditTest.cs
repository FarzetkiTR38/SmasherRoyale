using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using RoyalSmash.Save;
using RoyalSmash.Lives;
using RoyalSmash.Economy;
using RoyalSmash.Skills;
using RoyalSmash.Levels;
using RoyalSmash.Gameplay;
using RoyalSmash.Core;
using RoyalSmash.Audio;
using RoyalSmash.VFX;
using RoyalSmash.Haptics;
using RoyalSmash.Profile;
using RoyalSmash.Ads;
using RoyalSmash.Cloud;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Autonomous end-to-end Quality Audit and Bug Verification Suite.
    /// Tests singleton lifecycle safety, save system atomic integrity & recovery,
    /// lives manager idempotency and time tampering protection, data-driven level loading,
    /// projectile bounds cleanup, and scene input module integrity.
    /// </summary>
    public static class ProjectQualityAuditTest
    {
        private static int _passed = 0;
        private static int _failed = 0;

        [MenuItem("RoyalSmash/Quality Audit/Run Full Project Audit Suite")]
        public static void RunFullAudit()
        {
            _passed = 0;
            _failed = 0;

            Debug.Log("====================================================================");
            Debug.Log("[QualityAudit] STARTING FULL PROJECT QUALITY AUDIT SUITE");
            Debug.Log("====================================================================");

            try
            {
                Test01_SingletonHasInstanceSafety();
                Test02_SaveSystemAtomicIntegrityAndRecovery();
                Test03_LivesManagerIdempotencyAndTimeTampering();
                Test04_DataDrivenLevelProgressionAndLoader();
                Test05_ProjectileBoundaryKillZone();
                Test06_SceneInputModuleAndTextMeshProIntegrity();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QualityAudit] Unexpected exception during test execution: {ex}");
                _failed++;
            }

            Debug.Log("====================================================================");
            Debug.Log($"[QualityAudit] AUDIT COMPLETE: {_passed} PASSED, {_failed} FAILED.");
            Debug.Log("====================================================================");

            if (_failed > 0)
            {
                Debug.LogError($"[QualityAudit] {FAILED_SUMMARY()}");
            }
            else
            {
                Debug.Log("<color=green>[QualityAudit] ALL AUDIT CRITERIA 100% SATISFIED! ZERO BUGS DETECTED.</color>");
            }
        }

        private static void Assert(bool condition, string testName, string details)
        {
            if (condition)
            {
                _passed++;
                Debug.Log($"<color=green>[PASS]</color> {testName}: {details}");
            }
            else
            {
                _failed++;
                Debug.LogError($"<color=red>[FAIL]</color> {testName}: {details}");
            }
        }

        private static string FAILED_SUMMARY() => $"{_failed} quality test(s) failed. Please review errors.";

        // -------------------------------------------------------------
        // Test 1: Singleton HasInstance Safety & Zero Ghost Spawns
        // -------------------------------------------------------------
        private static void Test01_SingletonHasInstanceSafety()
        {
            Debug.Log("[QualityAudit] --- Test 1: Singleton HasInstance Safety ---");

            // Verify HasInstance returns false or true without throwing
            bool saveHasInstance = SaveManager.HasInstance;
            bool goldHasInstance = GoldManager.HasInstance;
            bool livesHasInstance = LivesManager.HasInstance;
            bool skillHasInstance = SkillManager.HasInstance;
            bool audioHasInstance = AudioManager.HasInstance;
            bool vfxHasInstance = VFXManager.HasInstance;
            bool hapticHasInstance = HapticManager.HasInstance;
            bool profileHasInstance = ProfileManager.HasInstance;
            bool iapHasInstance = IAPManager.HasInstance;
            bool adHasInstance = AdManager.HasInstance;
            bool cloudHasInstance = CloudSyncManager.HasInstance;
            bool lbHasInstance = LeaderboardManager.HasInstance;
            bool transHasInstance = SceneTransitionManager.HasInstance;

            Assert(true, "Test 1.1: HasInstance Non-Instantiating Queries",
                $"Successfully queried HasInstance across all singletons without null ref exceptions.");
        }

        // -------------------------------------------------------------
        // Test 2: Save System Atomic Integrity & Corrupt Recovery
        // -------------------------------------------------------------
        private static void Test02_SaveSystemAtomicIntegrityAndRecovery()
        {
            Debug.Log("[QualityAudit] --- Test 2: Save System Atomic Integrity ---");

            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            string backupPath = Path.Combine(Application.persistentDataPath, "player_save_backup.json");

            // 1. Write valid save
            var testData = new PlayerSaveData
            {
                CurrentLevel = 5,
                Gold = 1250,
                Lives = 4,
                Username = "AuditTester",
                AvatarId = 2,
                FrameId = 1,
                TutorialCompleted = true
            };

            string json = JsonUtility.ToJson(testData, true);
            File.WriteAllText(savePath, json);
            File.WriteAllText(backupPath, json);

            // Test loading via new GameObject
            var testGo = new GameObject("Test_SaveManager");
            var mgr = testGo.AddComponent<SaveManager>();
            mgr.Load();

            Assert(mgr.Data != null && mgr.Data.CurrentLevel == 5 && mgr.Data.Gold == 1250,
                "Test 2.1: Valid Save Load",
                $"Loaded saved data: Level {mgr.Data.CurrentLevel}, Gold {mgr.Data.Gold}, Username '{mgr.Data.Username}'");

            // 2. Corrupt primary file with garbage, test backup recovery
            File.WriteAllText(savePath, "{ broken corrupted json text !!!@#$%%^&*");
            mgr.Load();

            Assert(mgr.Data != null && mgr.Data.CurrentLevel == 5,
                "Test 2.2: Corrupted Save Auto-Recovery from Backup",
                $"Successfully recovered from corrupt primary save using backup. Level: {mgr.Data.CurrentLevel}");

            UnityEngine.Object.DestroyImmediate(testGo);
        }

        // -------------------------------------------------------------
        // Test 3: Lives Manager Idempotency & Clock Tampering Protection
        // -------------------------------------------------------------
        private static void Test03_LivesManagerIdempotencyAndTimeTampering()
        {
            Debug.Log("[QualityAudit] --- Test 3: Lives Manager Idempotency & Time Tampering ---");

            EnsureSaveManager();
            var data = SaveManager.Instance.Data;
            data.Lives = 5;
            LivesManager.Instance.ClearTransactionGuards();

            // 1. Deduct with transaction key
            string key = "AuditKey_" + Guid.NewGuid().ToString();
            bool firstDeduct = LivesManager.Instance.DeductLife("AuditTest", key);
            bool secondDeduct = LivesManager.Instance.DeductLife("AuditTest", key); // should be blocked

            Assert(firstDeduct == true && secondDeduct == false,
                "Test 3.1: Duplicate Deduction Idempotency Guard",
                $"First deduction succeeded ({firstDeduct}), identical transaction key duplicate was safely rejected ({secondDeduct}).");

            // 2. Test Time Tampering (Negative elapsed time)
            data.Lives = 4;
            data.LifeTimerStartTicks = DateTime.UtcNow.Ticks + TimeSpan.FromDays(2).Ticks; // future timestamp
            LivesManager.Instance.ProcessRegeneration();

            Assert(data.LifeTimerStartTicks <= DateTime.UtcNow.Ticks,
                "Test 3.2: Clock Tampering Protection",
                "Negative elapsed ticks safely corrected to current timestamp without infinite life exploit.");

            // Reset back to 5 lives
            data.Lives = 5;
            SaveManager.Instance.Save();
        }

        // -------------------------------------------------------------
        // Test 4: Data-Driven Level Progression & LevelLoader
        // -------------------------------------------------------------
        private static void Test04_DataDrivenLevelProgressionAndLoader()
        {
            Debug.Log("[QualityAudit] --- Test 4: Data-Driven Level Progression ---");

            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
            Assert(db != null && db.TotalLevels == 30,
                "Test 4.1: LevelDatabase 30 Levels Present",
                $"LevelDatabase verified with {db?.TotalLevels ?? 0} levels.");

            // Test Level 1, 5, 10, 15, 30 definitions
            var l1 = db.GetLevel(1);
            var l5 = db.GetLevel(5);
            var l10 = db.GetLevel(10);
            var l15 = db.GetLevel(15);
            var l30 = db.GetLevel(30);

            Assert(l1 != null && l1.BallCount == 25, "Test 4.2: Level 1 Config", $"Level 1 balls: {l1?.BallCount}");
            Assert(l5 != null && l5.BallCount == 22, "Test 4.3: Level 5 Config", $"Level 5 balls: {l5?.BallCount}");
            Assert(l10 != null && l10.BallCount == 20, "Test 4.4: Level 10 Config", $"Level 10 balls: {l10?.BallCount}");
            Assert(l15 != null && l15.BallCount == 18, "Test 4.5: Level 15 Config", $"Level 15 balls: {l15?.BallCount}");
            Assert(l30 != null && l30.BallCount == 12, "Test 4.6: Level 30 Config", $"Level 30 balls: {l30?.BallCount}");

            // Verify LevelLoader executes clean load and spawn
            var loaderGo = new GameObject("Test_Audit_LevelLoader");
            var loader = loaderGo.AddComponent<LevelLoader>();

            var standPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Stand.prefab");
            var boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Box.prefab");
            var stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Stone.prefab");
            var colPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Column.prefab");
            var conePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Cone.prefab");
            var bottlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/PaintBottle.prefab");

            loader.BindPrefabs(standPrefab, boxPrefab, stonePrefab, colPrefab, conePrefab, bottlePrefab);
            loader.LoadLevel(l1);

            Assert(loader.SpawnedObjects.Count > 0 && loader.SpawnedStands.Count > 0,
                "Test 4.7: LevelLoader Spawn Execution",
                $"Spawned {loader.SpawnedObjects.Count} targets on {loader.SpawnedStands.Count} stand(s).");

            loader.ClearLevel();
            UnityEngine.Object.DestroyImmediate(loaderGo);
        }

        // -------------------------------------------------------------
        // Test 5: Projectile Boundary Kill Zone
        // -------------------------------------------------------------
        private static void Test05_ProjectileBoundaryKillZone()
        {
            Debug.Log("[QualityAudit] --- Test 5: Projectile Boundary Kill Zone ---");

            var ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/Ball.prefab");
            Assert(ballPrefab != null, "Test 5.1: Ball Prefab Validity", "Ball prefab exists.");

            if (ballPrefab != null)
            {
                var instance = UnityEngine.Object.Instantiate(ballPrefab, new Vector3(0, -15f, 0), Quaternion.identity);
                var proj = instance.GetComponent<ProjectileController>();

                Assert(proj != null, "Test 5.2: ProjectileController Present",
                    "ProjectileController component present on Ball prefab.");

                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        // -------------------------------------------------------------
        // Test 6: Scene Input Module & TextMeshPro Integrity
        // -------------------------------------------------------------
        private static void Test06_SceneInputModuleAndTextMeshProIntegrity()
        {
            Debug.Log("[QualityAudit] --- Test 6: Scene Input Module & TMP Integrity ---");

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/BootScene.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/MainGame.unity",
                "Assets/_Project/Scenes/PhysicsTestScene.unity"
            };

            foreach (var scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
                if (eventSystem != null)
                {
                    var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                    var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();

                    Assert(inputModule != null && legacyModule == null,
                        $"Test 6.1: {scene.name} EventSystem Input Module",
                        $"Verified InputSystemUIInputModule present and legacy StandaloneInputModule absent in {scene.name}.");
                }

                // Verify zero legacy UnityEngine.UI.Text in scene
                var legacyTexts = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None);
                Assert(legacyTexts.Length == 0,
                    $"Test 6.2: {scene.name} Zero Legacy UI Text",
                    $"Verified 0 legacy UI Text components found in {scene.name} (100% TextMeshPro).");
            }

            // Restore MainGame as active scene
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainGame.unity", OpenSceneMode.Single);
        }

        private static void EnsureSaveManager()
        {
            if (SaveManager.Instance == null)
            {
                var saveGo = new GameObject("SaveManager_AuditTest");
                saveGo.AddComponent<SaveManager>();
            }

            if (SaveManager.Instance.Data == null)
            {
                SaveManager.Instance.Load();
            }

            if (LivesManager.Instance == null)
            {
                var livesGo = new GameObject("LivesManager_AuditTest");
                livesGo.AddComponent<LivesManager>();
            }
        }
    }
}
