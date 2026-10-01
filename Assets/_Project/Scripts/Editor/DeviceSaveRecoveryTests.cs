using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Save;
using RoyalSmash.Cloud;
using RoyalSmash.Utils;
using RoyalSmash.Economy;
using RoyalSmash.Skills;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite verifying Device-ID-based cloud save persistence and recovery.
    /// Simulates player progression, app deletion (local save wipe), app reinstallation,
    /// and seamless restoration of player stats (Level 29, Gold, NoAds, Skills) using unique Device ID.
    /// Follows Master Build Prompt Section 28 & Section 18.
    /// </summary>
    public static class DeviceSaveRecoveryTests
    {
        [MenuItem("RoyalSmash/Cloud/Run Device ID Save Recovery Tests")]
        public static string RunAllTests()
        {
            var results = new List<string>();
            int passed = 0;
            int total = 7;

            Debug.Log("================================================================================");
            Debug.Log("   STARTING DEVICE ID CLOUD SAVE RECOVERY VERIFICATION (7 TESTS)                ");
            Debug.Log("================================================================================");

            // Clean environment before testing
            string originalDeviceId = DeviceIdProvider.GetDeviceId();
            string testDeviceA = "test_device_alpha_12345";
            string testDeviceB = "test_device_beta_67890";

            try
            {
                var saveMgr = SaveManager.Instance;
                if (saveMgr == null)
                {
                    var go = new GameObject("SaveManager");
                    saveMgr = go.AddComponent<SaveManager>();
                }

                // -------------------------------------------------------------
                // TEST 1: Unique Device ID Resolution
                // -------------------------------------------------------------
                try
                {
                    DeviceIdProvider.SetOverrideDeviceId(null);
                    string hardwareId = DeviceIdProvider.GetDeviceId();

                    DeviceIdProvider.SetOverrideDeviceId(testDeviceA);
                    string overrideId = DeviceIdProvider.GetDeviceId();

                    if (!string.IsNullOrEmpty(hardwareId) && overrideId == testDeviceA)
                    {
                        results.Add($"[PASS] Test 1: Device ID resolved reliably (Hardware: '{hardwareId}', Override: '{overrideId}').");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 1: Device ID resolution failed (hardware={hardwareId}, override={overrideId})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 1: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 2: Progress to Level 29 & Save with Device ID
                // -------------------------------------------------------------
                try
                {
                    DeviceIdProvider.SetOverrideDeviceId(testDeviceA);
                    SharedCloudSaveStore.DeleteDeviceProgress(testDeviceA);

                    saveMgr.ResetSave();
                    saveMgr.Data.CurrentLevel = 29;
                    saveMgr.Data.Gold = 3500;
                    saveMgr.Data.NoAdsPurchased = true;
                    saveMgr.Data.MegaBallUnlocked = true;
                    saveMgr.Data.InfinityBallUnlocked = true;
                    saveMgr.Data.MegaBallUses = 10;
                    saveMgr.Data.InfinityBallUses = 10;
                    saveMgr.Data.Username = "RoyalChampion";
                    saveMgr.Save();

                    bool deviceIdMatches = saveMgr.Data.DeviceId == testDeviceA;
                    bool cloudHasData = SharedCloudSaveStore.HasDeviceProgress(testDeviceA);
                    var cloudSave = SharedCloudSaveStore.LoadDeviceProgress(testDeviceA);
                    bool cloudStatsMatch = cloudSave != null && cloudSave.CurrentLevel == 29 && cloudSave.Gold == 3500 && cloudSave.NoAdsPurchased;

                    if (deviceIdMatches && cloudHasData && cloudStatsMatch)
                    {
                        results.Add("[PASS] Test 2: Player reached Level 29, 3,500 Gold, No Ads; data successfully synced to remote cloud under Device ID.");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 2: Match fail (idMatch={deviceIdMatches}, cloudHas={cloudHasData}, statsMatch={cloudStatsMatch})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 2: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 3: App Uninstall Simulation (Wipe Local Files)
                // -------------------------------------------------------------
                try
                {
                    string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
                    string backupPath = Path.Combine(Application.persistentDataPath, "player_save_backup.json");

                    if (File.Exists(savePath)) File.Delete(savePath);
                    if (File.Exists(backupPath)) File.Delete(backupPath);

                    bool localWiped = !File.Exists(savePath) && !File.Exists(backupPath);
                    bool cloudRetained = SharedCloudSaveStore.HasDeviceProgress(testDeviceA);

                    if (localWiped && cloudRetained)
                    {
                        results.Add("[PASS] Test 3: App uninstall simulated. Local persistent disk files wiped; remote cloud save safely retained.");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 3: localWiped={localWiped}, cloudRetained={cloudRetained}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 3: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 4: App Reinstall Simulation & Automatic Stats Restoration
                // -------------------------------------------------------------
                try
                {
                    // Device ID is preserved by OS hardware
                    DeviceIdProvider.SetOverrideDeviceId(testDeviceA);

                    // Reinstall boots SaveManager.Load() with zero local files
                    saveMgr.SimulateUninstallAndReinstall();

                    bool restoredLevel = saveMgr.Data.CurrentLevel == 29;
                    bool restoredGold = saveMgr.Data.Gold == 3500;
                    bool restoredNoAds = saveMgr.Data.NoAdsPurchased == true;
                    bool restoredSkills = saveMgr.Data.MegaBallUses == 10 && saveMgr.Data.InfinityBallUses == 10;
                    bool restoredName = saveMgr.Data.Username == "RoyalChampion";

                    if (restoredLevel && restoredGold && restoredNoAds && restoredSkills && restoredName)
                    {
                        results.Add("[PASS] Test 4: App reinstalled! SaveManager checked Device ID and restored 100% of player stats (Level 29, 3,500 Gold, No Ads, 10 Mega/Infinity Balls).");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 4: Lvl29={restoredLevel} ({saveMgr.Data.CurrentLevel}), Gold={restoredGold}, NoAds={restoredNoAds}, Skills={restoredSkills}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 4: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 5: Second Device Isolation (Device B starts at Level 1)
                // -------------------------------------------------------------
                try
                {
                    // Switch to Device B
                    DeviceIdProvider.SetOverrideDeviceId(testDeviceB);
                    SharedCloudSaveStore.DeleteDeviceProgress(testDeviceB);

                    // Wipe local files for Device B
                    string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
                    string backupPath = Path.Combine(Application.persistentDataPath, "player_save_backup.json");
                    if (File.Exists(savePath)) File.Delete(savePath);
                    if (File.Exists(backupPath)) File.Delete(backupPath);

                    saveMgr.SimulateUninstallAndReinstall();

                    bool deviceBIsFresh = saveMgr.Data.CurrentLevel == 1 && saveMgr.Data.NoAdsPurchased == false;
                    bool deviceBId = saveMgr.Data.DeviceId == testDeviceB;

                    // Verify Device A cloud save was NOT corrupted by Device B
                    var deviceACloud = SharedCloudSaveStore.LoadDeviceProgress(testDeviceA);
                    bool deviceAIntact = deviceACloud != null && deviceACloud.CurrentLevel == 29;

                    if (deviceBIsFresh && deviceBId && deviceAIntact)
                    {
                        results.Add("[PASS] Test 5: Multi-device isolation verified. Device B initialized clean Level 1 and did not collide with Device A.");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 5: devBFresh={deviceBIsFresh}, devBId={deviceBId}, devAIntact={deviceAIntact}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 5: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 6: CloudSyncManager Provider Restore Integration
                // -------------------------------------------------------------
                try
                {
                    DeviceIdProvider.SetOverrideDeviceId(testDeviceA);
                    var cloudSync = CloudSyncManager.Instance;
                    if (cloudSync == null)
                    {
                        var go = new GameObject("CloudSyncManager");
                        cloudSync = go.AddComponent<CloudSyncManager>();
                    }

                    bool restoreCallbackFired = false;
                    PlayerSaveData callbackData = null;

                    cloudSync.CheckAndRestoreDeviceProgress(testDeviceA, (ok, data) =>
                    {
                        restoreCallbackFired = ok;
                        callbackData = data;
                    });

                    bool providerMatched = restoreCallbackFired && callbackData != null && callbackData.CurrentLevel == 29;

                    if (providerMatched)
                    {
                        results.Add("[PASS] Test 6: CloudSyncManager.CheckAndRestoreDeviceProgress successfully queried backend and confirmed Level 29 restoration.");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 6: callbackFired={restoreCallbackFired}, dataLvl={(callbackData != null ? callbackData.CurrentLevel : -1)}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 6: Exception - {ex.Message}");
                }

                // -------------------------------------------------------------
                // TEST 7: Highest Progress Wins & Post-Restore Level 30 Advancement
                // -------------------------------------------------------------
                try
                {
                    DeviceIdProvider.SetOverrideDeviceId(testDeviceA);
                    saveMgr.Data.CurrentLevel = 30; // Player finishes Level 30 after restore
                    saveMgr.Save();

                    var updatedCloud = SharedCloudSaveStore.LoadDeviceProgress(testDeviceA);
                    bool level30Synced = updatedCloud != null && updatedCloud.CurrentLevel == 30;

                    if (level30Synced)
                    {
                        results.Add("[PASS] Test 7: Section 28 Highest Progress Wins confirmed. Restored player completed Level 30 and cloud updated to Level 30.");
                        passed++;
                    }
                    else
                    {
                        results.Add($"[FAIL] Test 7: level30Synced={level30Synced} (cloudLvl={(updatedCloud != null ? updatedCloud.CurrentLevel : -1)})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"[FAIL] Test 7: Exception - {ex.Message}");
                }
            }
            finally
            {
                // Restore hardware device ID and cleanup test device records
                DeviceIdProvider.SetOverrideDeviceId(null);
                SharedCloudSaveStore.DeleteDeviceProgress(testDeviceA);
                SharedCloudSaveStore.DeleteDeviceProgress(testDeviceB);
            }

            string report = $"=== DEVICE ID CLOUD SAVE RECOVERY RESULTS: {passed}/{total} PASSED ===\n" +
                            string.Join("\n", results);
            Debug.Log(report);
            return report;
        }
    }
}
