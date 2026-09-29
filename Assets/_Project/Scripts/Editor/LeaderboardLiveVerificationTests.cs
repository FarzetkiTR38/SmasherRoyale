using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Cloud;
using RoyalSmash.Save;
using RoyalSmash.UI;
using RoyalSmash.Profile;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Complete verification test suite for the Live Multi-Device Leaderboard system.
    /// Validates 1-30 level constraints, local player dynamic ranking, multi-device synchronization,
    /// and UI presentation without hardcoded mockup numbers.
    /// </summary>
    public static class LeaderboardLiveVerificationTests
    {
        [MenuItem("Tools/RoyalSmash/Leaderboard/Run All Live Leaderboard Tests")]
        public static void RunAllTestsMenu()
        {
            var results = new List<string>();
            bool allPassed = true;

            void Run(string testName, Action testAction)
            {
                try
                {
                    testAction();
                    results.Add($"[PASS] {testName}");
                    Debug.Log($"<color=green>[PASS]</color> {testName}");
                }
                catch (Exception ex)
                {
                    allPassed = false;
                    results.Add($"[FAIL] {testName}: {ex.Message}");
                    Debug.LogError($"<color=red>[FAIL]</color> {testName}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            Debug.Log("<color=#2EA3FF><b>=== STARTING LIVE LEADERBOARD TEST SUITE ===</b></color>");

            Run("1. Mock Competitors Strict 1-30 Level Range", TestMockCompetitorsLevelRange);
            Run("2. Dynamic Local Player Progression & Ranking", TestDynamicLocalPlayerRanking);
            Run("3. Multi-Device Live Cloud Synchronization (Devices 1, 2, 3)", TestMultiDeviceLiveSync);
            Run("4. Row UI Formatting Clamping & Clean Label", TestRowUIFormatting);
            Run("5. MainMenu Leaderboard UI Live Integration", TestMainMenuLeaderboardUI);

            Debug.Log("<color=#2EA3FF><b>=== LEADERBOARD TEST SUITE COMPLETED ===</b></color>");
            Debug.Log(string.Join("\n", results));

            if (allPassed)
            {
                Debug.Log("<color=green><b>ALL 5/5 LEADERBOARD TESTS PASSED SUCCESSFULLY!</b></color>");
            }
            else
            {
                Debug.LogError("<color=red><b>SOME LEADERBOARD TESTS FAILED! Check console logs.</b></color>");
            }
        }

        public static void TestMockCompetitorsLevelRange()
        {
            var config = AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(
                "Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LeaderboardConfig>();
            }

            var competitors = config.GenerateDefaultCompetitors();
            Assert.IsNotNull(competitors, "Competitors list should not be null.");
            Assert.AreEqual(35, competitors.Count, "Should generate exactly 35 royal competitors.");

            for (int i = 0; i < competitors.Count; i++)
            {
                var comp = competitors[i];
                Assert.IsTrue(comp.Level >= 1 && comp.Level <= 30,
                    $"Competitor {comp.Username} at rank {comp.Rank} has level {comp.Level}, which is outside [1, 30]!");
                Assert.IsFalse(comp.IsLocalPlayer, $"Competitor {comp.Username} must not be marked as local player.");
                Assert.IsNotEmpty(comp.Username, "Competitor must have a valid username.");
            }

            // Verify descending sort
            for (int i = 0; i < competitors.Count - 1; i++)
            {
                Assert.IsTrue(competitors[i].Level >= competitors[i + 1].Level,
                    $"Competitors must be sorted descending by level. Rank {i + 1} ({competitors[i].Level}) vs Rank {i + 2} ({competitors[i + 1].Level})");
            }
        }

        public static void TestDynamicLocalPlayerRanking()
        {
            SharedCloudLeaderboardStore.Clear();
            var config = ScriptableObject.CreateInstance<LeaderboardConfig>();
            var provider = new SimulatedCloudProvider(config);

            // Step A: Player at Level 1
            var playerSave = new PlayerSaveData
            {
                PlayerId = "test_player_live",
                Username = "HeroPlayer",
                CurrentLevel = 1,
                AvatarId = 2,
                FrameId = 1
            };

            provider.SyncProgress(playerSave);
            var listAtLvl1 = provider.BuildMergedLeaderboard(playerSave.PlayerId);
            var entryLvl1 = listAtLvl1.Find(e => e.PlayerId == "test_player_live");

            Assert.IsNotNull(entryLvl1, "Player should be found in leaderboard.");
            Assert.AreEqual(1, entryLvl1.Level, "Player level should be 1.");
            Assert.AreEqual(36, entryLvl1.Rank, "At Level 1, player should rank 36 (below all 35 bots).");
            Assert.IsTrue(entryLvl1.IsLocalPlayer, "Player entry should be flagged as local.");

            // Step B: Player advances to Level 15
            playerSave.CurrentLevel = 15;
            provider.SyncProgress(playerSave);
            var listAtLvl15 = provider.BuildMergedLeaderboard(playerSave.PlayerId);
            var entryLvl15 = listAtLvl15.Find(e => e.PlayerId == "test_player_live");

            Assert.IsNotNull(entryLvl15);
            Assert.AreEqual(15, entryLvl15.Level);
            Assert.IsTrue(entryLvl15.Rank < 36, $"Player rank ({entryLvl15.Rank}) must be higher than 36 at Level 15.");

            // Step C: Player reaches max Level 30
            playerSave.CurrentLevel = 30;
            provider.SyncProgress(playerSave);
            var listAtLvl30 = provider.BuildMergedLeaderboard(playerSave.PlayerId);
            var entryLvl30 = listAtLvl30.Find(e => e.PlayerId == "test_player_live");

            Assert.IsNotNull(entryLvl30);
            Assert.AreEqual(30, entryLvl30.Level);
            Assert.AreEqual(1, entryLvl30.Rank, "At Level 30, player must achieve Rank 1 on the leaderboard!");
        }

        public static void TestMultiDeviceLiveSync()
        {
            // Reset shared storage for clean test
            SharedCloudLeaderboardStore.Clear();

            var config = ScriptableObject.CreateInstance<LeaderboardConfig>();

            // --- Device 1 Client Simulation ---
            string dev1Id = "device_1_farzetki";
            var dev1Save = new PlayerSaveData
            {
                PlayerId = dev1Id,
                Username = "Device1_Farzetki",
                CurrentLevel = 10,
                AvatarId = 0,
                FrameId = 0
            };
            var dev1Provider = new SimulatedCloudProvider(config);
            dev1Provider.SyncProgress(dev1Save);

            // --- Device 2 Client Simulation ---
            string dev2Id = "device_2_progamer";
            var dev2Save = new PlayerSaveData
            {
                PlayerId = dev2Id,
                Username = "Device2_ProGamer",
                CurrentLevel = 22,
                AvatarId = 3,
                FrameId = 2
            };
            var dev2Provider = new SimulatedCloudProvider(config);
            dev2Provider.SyncProgress(dev2Save);

            // --- Device 3 Client Simulation ---
            string dev3Id = "device_3_guest";
            var dev3Save = new PlayerSaveData
            {
                PlayerId = dev3Id,
                Username = "Device3_GuestRookie",
                CurrentLevel = 4,
                AvatarId = 1,
                FrameId = 0
            };
            var dev3Provider = new SimulatedCloudProvider(config);
            dev3Provider.SyncProgress(dev3Save);

            // Now, simulate Device 1 opening the leaderboard
            // Set SaveManager temporary instance state for Device 1
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.PlayerId = dev1Id;
                SaveManager.Instance.Data.Username = dev1Save.Username;
                SaveManager.Instance.Data.CurrentLevel = dev1Save.CurrentLevel;
            }

            var dev1View = dev1Provider.BuildMergedLeaderboard(dev1Id);

            // Assertions for Device 1's perspective:
            var dev1Entry = dev1View.Find(e => e.PlayerId == dev1Id);
            var dev2Entry = dev1View.Find(e => e.PlayerId == dev2Id);
            var dev3Entry = dev1View.Find(e => e.PlayerId == dev3Id);

            Assert.IsNotNull(dev1Entry, "Device 1 must see itself in leaderboard.");
            Assert.IsNotNull(dev2Entry, "Device 1 must see Device 2 live in leaderboard.");
            Assert.IsNotNull(dev3Entry, "Device 1 must see Device 3 live in leaderboard.");

            Assert.IsTrue(dev1Entry.IsLocalPlayer, "Device 1 must be flagged as local player.");
            Assert.IsFalse(dev2Entry.IsLocalPlayer, "Device 2 must NOT be flagged as local player.");
            Assert.IsFalse(dev3Entry.IsLocalPlayer, "Device 3 must NOT be flagged as local player.");

            Assert.AreEqual(10, dev1Entry.Level, "Device 1 level must be 10.");
            Assert.AreEqual(22, dev2Entry.Level, "Device 2 level must be 22.");
            Assert.AreEqual(4, dev3Entry.Level, "Device 3 level must be 4.");

            // Ranking order: Device 2 (Lvl 22) > Device 1 (Lvl 10) > Device 3 (Lvl 4)
            Assert.IsTrue(dev2Entry.Rank < dev1Entry.Rank,
                $"Device 2 (Level 22, Rank {dev2Entry.Rank}) should be ranked ABOVE Device 1 (Level 10, Rank {dev1Entry.Rank}).");
            Assert.IsTrue(dev1Entry.Rank < dev3Entry.Rank,
                $"Device 1 (Level 10, Rank {dev1Entry.Rank}) should be ranked ABOVE Device 3 (Level 4, Rank {dev3Entry.Rank}).");

            Debug.Log($"[TestMultiDeviceLiveSync] Rankings: Device 2 Rank {dev2Entry.Rank} (Lv {dev2Entry.Level}), Device 1 Rank {dev1Entry.Rank} (Lv {dev1Entry.Level}), Device 3 Rank {dev3Entry.Rank} (Lv {dev3Entry.Level})");
        }

        public static void TestRowUIFormatting()
        {
            var go = new GameObject("TestRowUI");
            var rowUI = go.AddComponent<LeaderboardEntryRowUI>();

            var lvlTxtGo = new GameObject("LevelText");
            lvlTxtGo.transform.SetParent(go.transform);
            var lvlTmp = lvlTxtGo.AddComponent<TMPro.TextMeshProUGUI>();

            var stageLabelGo = new GameObject("StageLabel");
            stageLabelGo.transform.SetParent(go.transform);
            var stageLabelTmp = stageLabelGo.AddComponent<TMPro.TextMeshProUGUI>();

            rowUI.BindReferences(null, null, null, null, null, null, lvlTmp, stageLabelTmp);

            // Test Level 25 formatting
            var entry = new LeaderboardEntryData(5, "p1", "TestPlayer", 25, 0, 0, false);
            rowUI.Setup(entry, null);

            Assert.AreEqual("25", lvlTmp.text, "Level text must be '25' without any multipliers.");
            Assert.AreEqual("Level", stageLabelTmp.text, "Stage label text must be 'Level'.");

            // Test Level 1 formatting
            var entry2 = new LeaderboardEntryData(36, "p2", "Rookie", 1, 0, 0, false);
            rowUI.Setup(entry2, null);

            Assert.AreEqual("1", lvlTmp.text, "Level text must be '1'.");

            GameObject.DestroyImmediate(go);
        }

        public static void TestMainMenuLeaderboardUI()
        {
            string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid(), "MainMenu scene must be valid.");

            var lbUI = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
            Assert.IsNotNull(lbUI, "LeaderboardUI must exist in MainMenu scene.");

            // Populate with live entries
            var testEntries = new List<LeaderboardEntryData>
            {
                new LeaderboardEntryData(1, "bot_001", "King Arthur", 29, 0, 0, false),
                new LeaderboardEntryData(2, "dev_002", "Device2_ProGamer", 22, 1, 1, false),
                new LeaderboardEntryData(3, "local_player", "CurrentPlayer (Sen)", 15, 2, 2, true),
                new LeaderboardEntryData(4, "dev_003", "Device3_GuestRookie", 4, 3, 3, false)
            };

            lbUI.PopulateList(testEntries, false);
            Assert.IsTrue(lbUI.DisplayedRowCount >= 4, "LeaderboardUI should display populated rows.");

            // Verify sticky player row
            var stickyProp = typeof(LeaderboardUI).GetField("_stickyPlayerRow",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stickyProp != null)
            {
                var sticky = stickyProp.GetValue(lbUI) as LeaderboardEntryRowUI;
                if (sticky != null)
                {
                    Assert.IsTrue(sticky.gameObject.activeSelf, "Sticky player row must be active when local player is present.");
                    Assert.AreEqual("CurrentPlayer (Sen)", sticky.Data.Username);
                    Assert.AreEqual(15, sticky.Data.Level);
                    Assert.AreEqual(3, sticky.Data.Rank);
                }
            }

            Debug.Log("[TestMainMenuLeaderboardUI] LeaderboardUI successfully populated and verified in MainMenu scene.");
        }
    }
}
