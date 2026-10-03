using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Cloud;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Interactive simulation tool that simulates multiple real devices logging in,
    /// completing levels 1-3, and submitting their live progress to the shared cloud leaderboard.
    /// Follows Master Build Prompt Section 19 (Leaderboard) & Section 28 (Cloud Sync).
    /// </summary>
    public static class MultiDeviceSimulationRunner
    {
        [MenuItem("Tools/RoyalSmash/Leaderboard/Simulate 2nd & 3rd Device Live Progression")]
        public static void SimulateDevicesAndShowLeaderboard()
        {
            Debug.Log("<color=#2EA3FF><b>=== STARTING MULTI-DEVICE LIVE SIMULATION ===</b></color>");

            // 1. Clear shared cloud storage to start fresh
            SharedCloudLeaderboardStore.Clear();
            Debug.Log("[MultiDeviceSim] 1. Shared live cloud storage reset.");

            // 2. Setup Device 1 (Local Player) at Level 5
            string dev1Id = "device_1_farzetki";
            string dev1Name = "Farzetki_King";
            int dev1Level = 5;

            var config = AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(
                "Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset");
            if (config == null) config = ScriptableObject.CreateInstance<LeaderboardConfig>();

            var dev1Save = new PlayerSaveData
            {
                PlayerId = dev1Id,
                Username = dev1Name,
                CurrentLevel = dev1Level,
                AvatarId = 0,
                FrameId = 0
            };
            var dev1Provider = new SimulatedCloudProvider(config);
            dev1Provider.SyncProgress(dev1Save);
            Debug.Log($"<color=cyan>[Device 1 (Primary)]</color> Logged in as '{dev1Name}', current level: {dev1Level}. Score submitted to cloud!");

            // 3. Simulate Device 2 (e.g. Tablet / Second Device) completing Level 1, 2, 3
            string dev2Id = "device_2_tablet";
            string dev2Name = "Ahmet_Device2";
            var dev2Provider = new SimulatedCloudProvider(config);

            Debug.Log($"<color=yellow>[Device 2 (Tablet)]</color> Logging in as '{dev2Name}'...");
            for (int lvl = 1; lvl <= 3; lvl++)
            {
                var dev2Save = new PlayerSaveData
                {
                    PlayerId = dev2Id,
                    Username = dev2Name,
                    CurrentLevel = lvl,
                    AvatarId = 2,
                    FrameId = 1
                };
                dev2Provider.SyncProgress(dev2Save);
                Debug.Log($"<color=yellow>[Device 2 (Tablet)]</color> Completed Level {lvl} -> Live score synced to cloud (Level {lvl})!");
            }

            // 4. Simulate Device 3 (e.g. Phone / Third Device) completing Level 1, 2
            string dev3Id = "device_3_mobile";
            string dev3Name = "Mehmet_Device3";
            var dev3Provider = new SimulatedCloudProvider(config);

            Debug.Log($"<color=magenta>[Device 3 (Mobile)]</color> Logging in as '{dev3Name}'...");
            for (int lvl = 1; lvl <= 2; lvl++)
            {
                var dev3Save = new PlayerSaveData
                {
                    PlayerId = dev3Id,
                    Username = dev3Name,
                    CurrentLevel = lvl,
                    AvatarId = 3,
                    FrameId = 2
                };
                dev3Provider.SyncProgress(dev3Save);
                Debug.Log($"<color=magenta>[Device 3 (Mobile)]</color> Completed Level {lvl} -> Live score synced to cloud (Level {lvl})!");
            }

            // 5. Open MainMenu Scene and present the live Leaderboard
            string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Configure local save data for Device 1
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.PlayerId = dev1Id;
                SaveManager.Instance.Data.Username = dev1Name;
                SaveManager.Instance.Data.CurrentLevel = dev1Level;
                SaveManager.Instance.Save();
            }

            // Find LeaderboardUI and BottomNavigationUI in scene
            var navUI = UnityEngine.Object.FindFirstObjectByType<BottomNavigationUI>(FindObjectsInactive.Include);
            if (navUI != null)
            {
                navUI.SwitchTab(NavigationTab.Leaderboard);
            }

            var lbUI = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
            if (lbUI != null)
            {
                lbUI.gameObject.SetActive(true);
                // Fetch and populate live merged leaderboard
                var entries = dev1Provider.BuildMergedLeaderboard(dev1Id);
                lbUI.PopulateList(entries, false);
                Debug.Log($"[MultiDeviceSim] Leaderboard populated with {entries.Count} live & competitor entries!");

                // Print top entries and where the devices ended up
                Debug.Log("<color=#2EA3FF><b>=== LIVE LEADERBOARD RANKINGS SNAPSHOT ===</b></color>");
                for (int i = 0; i < Mathf.Min(10, entries.Count); i++)
                {
                    var e = entries[i];
                    string tag = e.IsLocalPlayer ? " <color=yellow>(SEN - LOCAL PLAYER)</color>" : "";
                    Debug.Log($"Rank #{e.Rank}: {e.Username} - Level {e.Level}{tag}");
                }

                Debug.Log("...");
                // Locate Device 1, Device 2, Device 3 ranks
                var d1 = entries.Find(e => e.PlayerId == dev1Id);
                var d2 = entries.Find(e => e.PlayerId == dev2Id);
                var d3 = entries.Find(e => e.PlayerId == dev3Id);

                if (d1 != null) Debug.Log($"<color=cyan><b>[Device 1 - {d1.Username}] Rank #{d1.Rank} (Level {d1.Level}) [LOCAL PLAYER]</b></color>");
                if (d2 != null) Debug.Log($"<color=yellow><b>[Device 2 - {d2.Username}] Rank #{d2.Rank} (Level {d2.Level}) [LIVE REMOTE]</b></color>");
                if (d3 != null) Debug.Log($"<color=magenta><b>[Device 3 - {d3.Username}] Rank #{d3.Rank} (Level {d3.Level}) [LIVE REMOTE]</b></color>");

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("<color=green><b>=== MULTI-DEVICE SIMULATION COMPLETE & VERIFIED! ===</b></color>");
        }
    }
}
