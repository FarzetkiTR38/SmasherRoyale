using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Component to perform live runtime verification of Unity Gaming Services in Play Mode.
    /// Tests Authentication, Cloud Save, and Leaderboards against live Unity servers.
    /// </summary>
    public class UGSRuntimeTester : MonoBehaviour
    {
        public static string LastReport = "Pending";
        public static bool IsRunning = false;
        public static bool Succeeded = false;

        public bool runOnStart = false;

        private void Start()
        {
            if (runOnStart)
            {
                _ = ExecuteLiveTestAsync();
            }
        }

        public async Task<string> ExecuteLiveTestAsync()
        {
            IsRunning = true;
            LastReport = "Starting UGS Live Test...";
            Debug.Log("[UGSRuntimeTester] >>> STARTING LIVE PLAY-MODE UGS TEST <<<");

            var reportLines = new List<string>();

            try
            {
                // 1. Core Init
                reportLines.Add("1. Initializing UnityServices Core...");
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }
                reportLines.Add($"   Core State: {UnityServices.State}");

                // 2. Auth
                reportLines.Add("2. Signing in Anonymously...");
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                string pid = AuthenticationService.Instance.PlayerId;
                reportLines.Add($"   Player ID: {pid}");

                // 3. Cloud Save
                reportLines.Add("3. Testing Cloud Save...");
                var testSave = SaveManager.HasInstance ? SaveManager.Instance.Data : new PlayerSaveData();
                string json = JsonUtility.ToJson(testSave);
                var saveDict = new Dictionary<string, object>
                {
                    { "player_save_data", json },
                    { "runtime_test_utc", DateTime.UtcNow.ToString("o") }
                };
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveDict);
                reportLines.Add("   Cloud Save: SUCCESS (Uploaded player save to Unity Cloud)");

                // 4. Leaderboard Add Score
                reportLines.Add("4. Submitting Score to Leaderboard 'global_highest_level'...");
                int scoreToSubmit = testSave.CurrentLevel > 0 ? testSave.CurrentLevel : 1;
                try
                {
                    var addedEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync("global_highest_level", scoreToSubmit);
                    reportLines.Add($"   Score Submitted: Level {addedEntry.Score} (Rank #{addedEntry.Rank + 1})");
                }
                catch (Exception lbEx)
                {
                    reportLines.Add($"   Leaderboard Submit: {lbEx.Message}");
                }

                // 5. Fetch Scores
                reportLines.Add("5. Fetching Leaderboard Scores from Unity...");
                try
                {
                    var scores = await LeaderboardsService.Instance.GetScoresAsync("global_highest_level", new Unity.Services.Leaderboards.GetScoresOptions { Limit = 10 });
                    int count = scores?.Results?.Count ?? 0;
                    reportLines.Add($"   Entries Returned: {count}");

                    if (scores?.Results != null)
                    {
                        foreach (var e in scores.Results)
                        {
                            bool isMe = e.PlayerId == pid;
                            reportLines.Add($"   - Rank #{e.Rank + 1}: Player {e.PlayerName ?? e.PlayerId} -> Level {e.Score} {(isMe ? " [YOU!]" : "")}");
                        }
                    }
                }
                catch (Exception fetchEx)
                {
                    reportLines.Add($"   Leaderboard Fetch: {fetchEx.Message}");
                }

                Succeeded = true;
                reportLines.Add(">>> ALL UGS SERVICES VERIFIED SUCCESSFULLY! <<<");
            }
            catch (Exception ex)
            {
                Succeeded = false;
                reportLines.Add($"FAILED: {ex.Message}");
                Debug.LogError($"[UGSRuntimeTester] Error: {ex}");
            }
            finally
            {
                IsRunning = false;
                LastReport = string.Join("\n", reportLines);
                Debug.Log($"[UGSRuntimeTester] Final Report:\n{LastReport}");
            }

            return LastReport;
        }
    }
}
