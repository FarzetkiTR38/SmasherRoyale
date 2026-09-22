using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using RoyalSmash.Save;
using RoyalSmash.Profile;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Production implementation of ICloudSyncProvider using official Unity Gaming Services (UGS):
    /// - Unity Authentication (Anonymous login, device-linked)
    /// - Unity Cloud Save (Atomic JSON save sync)
    /// - Unity Leaderboards (Score and ranking synchronization)
    /// Adheres to Master Build Prompt Section 28 (Latest local progress wins) and Section 19 (Leaderboard).
    /// </summary>
    public class UGSCloudProvider : ICloudSyncProvider
    {
        private const string SAVE_DATA_KEY = "player_save_data";
        private const string LEADERBOARD_ID = "global_highest_level";

        private readonly LeaderboardConfig _config;
        private bool _isOnline = true;
        private bool _isInitialized = false;
        private bool _isSigningIn = false;

        public bool IsOnline => _isOnline && Application.internetReachability != NetworkReachability.NotReachable;

        public UGSCloudProvider(LeaderboardConfig config)
        {
            _config = config;
            _ = EnsureInitializedAsync();
        }

        public void SetOnlineState(bool isOnline)
        {
            _isOnline = isOnline;
            Debug.Log($"[UGSCloudProvider] Online state changed: IsOnline={IsOnline}");
        }

        /// <summary>
        /// Ensures UnityServices.InitializeAsync() and AuthenticationService.SignInAnonymouslyAsync() are completed.
        /// </summary>
        public async Task<bool> EnsureInitializedAsync()
        {
            if (_isInitialized && AuthenticationService.Instance.IsSignedIn)
            {
                return true;
            }

            if (!IsOnline)
            {
                Debug.LogWarning("[UGSCloudProvider] Cannot initialize UGS while offline.");
                return false;
            }

            if (_isSigningIn)
            {
                // Wait for existing sign-in task
                while (_isSigningIn)
                {
                    await Task.Delay(100);
                }
                return _isInitialized && AuthenticationService.Instance.IsSignedIn;
            }

            _isSigningIn = true;
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    Debug.Log("[UGSCloudProvider] Initializing Unity Gaming Services...");
                    await UnityServices.InitializeAsync();
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("[UGSCloudProvider] Signing in anonymously to Unity Authentication...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[UGSCloudProvider] Signed in successfully! Player ID: {AuthenticationService.Instance.PlayerId}");

                    // Sync player profile name if available
                    SyncProfileNameWithAuth();
                }

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UGSCloudProvider] UGS initialization/sign-in failed: {ex.Message}");
                return false;
            }
            finally
            {
                _isSigningIn = false;
            }
        }

        private async void SyncProfileNameWithAuth()
        {
            try
            {
                if (ProfileManager.HasInstance)
                {
                    string playerName = ProfileManager.Instance.Username;
                    if (!string.IsNullOrEmpty(playerName))
                    {
                        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[UGSCloudProvider] Optional player name sync notice: {ex.Message}");
            }
        }

        /// <summary>
        /// Pushes local player save data to Unity Cloud Save and updates the leaderboard.
        /// Follows Section 28.1 "Latest local progress wins" rule.
        /// </summary>
        public async void SyncProgress(PlayerSaveData localData, Action<bool, string> onComplete = null)
        {
            if (localData == null)
            {
                onComplete?.Invoke(false, "Local data null");
                return;
            }

            if (!IsOnline)
            {
                Debug.LogWarning("[UGSCloudProvider] SyncProgress aborted: Device is offline.");
                onComplete?.Invoke(false, "Offline");
                return;
            }

            bool ready = await EnsureInitializedAsync();
            if (!ready)
            {
                onComplete?.Invoke(false, "UGS services unavailable");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(localData);
                var dataPayload = new Dictionary<string, object>
                {
                    { SAVE_DATA_KEY, json }
                };

                // 1. Save to Unity Cloud Save
                await CloudSaveService.Instance.Data.Player.SaveAsync(dataPayload);
                Debug.Log($"[UGSCloudProvider] Successfully saved progress (Level {localData.CurrentLevel}) to Unity Cloud Save.");

                // 2. Submit score to Unity Leaderboards
                try
                {
                    await LeaderboardsService.Instance.AddPlayerScoreAsync(LEADERBOARD_ID, localData.CurrentLevel);
                    Debug.Log($"[UGSCloudProvider] Leaderboard updated for score {localData.CurrentLevel}.");
                }
                catch (Exception lbEx)
                {
                    Debug.LogWarning($"[UGSCloudProvider] Leaderboard score submission notice: {lbEx.Message}");
                }

                onComplete?.Invoke(true, "Sync successful");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UGSCloudProvider] SyncProgress failed: {ex.Message}");
                onComplete?.Invoke(false, ex.Message);
            }
        }

        /// <summary>
        /// Fetches the top global leaderboard entries from Unity Leaderboards.
        /// </summary>
        public async void FetchLeaderboard(Action<bool, List<LeaderboardEntryData>, string> onComplete)
        {
            if (!IsOnline)
            {
                Debug.LogWarning("[UGSCloudProvider] FetchLeaderboard aborted: Device is offline.");
                onComplete?.Invoke(false, null, "Offline");
                return;
            }

            bool ready = await EnsureInitializedAsync();
            if (!ready)
            {
                onComplete?.Invoke(false, null, "UGS services unavailable");
                return;
            }

            try
            {
                var options = new GetScoresOptions { Limit = 50 };
                var scoresPage = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, options);

                var entries = new List<LeaderboardEntryData>();
                string localPlayerId = AuthenticationService.Instance.PlayerId;

                if (scoresPage?.Results != null)
                {
                    foreach (var scoreItem in scoresPage.Results)
                    {
                        bool isLocal = !string.IsNullOrEmpty(localPlayerId) && scoreItem.PlayerId == localPlayerId;
                        string displayName = scoreItem.PlayerName;
                        if (string.IsNullOrEmpty(displayName))
                        {
                            displayName = isLocal ? "You (Champion)" : $"SmashLord_{scoreItem.PlayerId.Substring(0, Mathf.Min(4, scoreItem.PlayerId.Length))}";
                        }

                        var entry = new LeaderboardEntryData(
                            rank: scoreItem.Rank + 1, // UGS is 0-indexed; game displays 1-indexed
                            playerId: scoreItem.PlayerId,
                            username: displayName,
                            level: (int)scoreItem.Score,
                            avatarIndex: 0,
                            frameIndex: 0,
                            isLocalPlayer: isLocal
                        );
                        entries.Add(entry);
                    }
                }

                Debug.Log($"[UGSCloudProvider] Fetched {entries.Count} leaderboard entries from Unity Leaderboards.");
                onComplete?.Invoke(true, entries, "Success");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UGSCloudProvider] FetchLeaderboard notice: {ex.Message}");
                onComplete?.Invoke(false, null, ex.Message);
            }
        }
    }
}
