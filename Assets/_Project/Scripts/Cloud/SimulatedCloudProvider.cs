using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Mock cloud backend provider simulating cloud persistence, offline mode, and network dropouts.
    /// Implements Section 28.1 "Latest local progress wins" rule.
    /// </summary>
    public class SimulatedCloudProvider : ICloudSyncProvider
    {
        private readonly LeaderboardConfig _config;
        private List<LeaderboardEntryData> _remoteLeaderboard = new List<LeaderboardEntryData>();
        private PlayerSaveData _cloudData;

        public bool IsOnline { get; private set; } = true;
        public bool SimulateServiceOutage { get; set; } = false;
        public PlayerSaveData CloudData => _cloudData;

        public SimulatedCloudProvider(LeaderboardConfig config)
        {
            _config = config;
            InitializeRemoteData();
        }

        private void InitializeRemoteData()
        {
            if (_config != null)
            {
                _remoteLeaderboard = _config.GenerateDefaultCompetitors();
            }
            else
            {
                _remoteLeaderboard = new List<LeaderboardEntryData>();
            }
        }

        public void SetOnlineState(bool isOnline)
        {
            IsOnline = isOnline;
            Debug.Log($"[SimulatedCloudProvider] Online state changed: IsOnline={IsOnline}");
        }

        public void SyncProgress(PlayerSaveData localData, Action<bool, string> onComplete = null)
        {
            if (!IsOnline || SimulateServiceOutage)
            {
                Debug.LogWarning("[SimulatedCloudProvider] SyncProgress failed: Network offline or service outage.");
                onComplete?.Invoke(false, "Network offline");
                return;
            }

            if (localData == null)
            {
                onComplete?.Invoke(false, "Local data null");
                return;
            }

            // Section 28.1: "Latest local progress wins" rule
            if (_cloudData == null || localData.CurrentLevel >= _cloudData.CurrentLevel)
            {
                _cloudData = localData.Clone();
                Debug.Log($"[SimulatedCloudProvider] Cloud updated to Level {_cloudData.CurrentLevel} (Latest local progress wins).");
            }
            else
            {
                Debug.Log($"[SimulatedCloudProvider] Cloud already has Level {_cloudData.CurrentLevel}. Local Level {localData.CurrentLevel} retained.");
            }

            // Update or add local player into remote leaderboard
            UpdatePlayerInLeaderboard(localData);

            onComplete?.Invoke(true, "Sync successful");
        }

        public void FetchLeaderboard(Action<bool, List<LeaderboardEntryData>, string> onComplete)
        {
            if (!IsOnline || SimulateServiceOutage)
            {
                Debug.LogWarning("[SimulatedCloudProvider] FetchLeaderboard failed: Network offline or service outage.");
                onComplete?.Invoke(false, null, "Offline or service unavailable");
                return;
            }

            // Sort descending by Level and re-rank
            SortAndRankLeaderboard();

            // Return deep copy
            var copy = _remoteLeaderboard.Select(e => e.Clone()).ToList();
            onComplete?.Invoke(true, copy, "Success");
        }

        private void UpdatePlayerInLeaderboard(PlayerSaveData data)
        {
            string playerId = "local_player";
            var existing = _remoteLeaderboard.FirstOrDefault(e => e.PlayerId == playerId);

            if (existing != null)
            {
                existing.Username = data.Username;
                existing.Level = data.CurrentLevel;
                existing.AvatarIndex = data.AvatarIndex;
                existing.FrameIndex = data.FrameIndex;
                existing.IsLocalPlayer = true;
            }
            else
            {
                _remoteLeaderboard.Add(new LeaderboardEntryData(
                    rank: 0,
                    playerId: playerId,
                    username: data.Username,
                    level: data.CurrentLevel,
                    avatarIndex: data.AvatarIndex,
                    frameIndex: data.FrameIndex,
                    isLocalPlayer: true
                ));
            }

            SortAndRankLeaderboard();
        }

        private void SortAndRankLeaderboard()
        {
            // Primary Sort: Current Level (descending)
            _remoteLeaderboard = _remoteLeaderboard
                .OrderByDescending(e => e.Level)
                .ThenBy(e => e.Username)
                .ToList();

            for (int i = 0; i < _remoteLeaderboard.Count; i++)
            {
                _remoteLeaderboard[i].Rank = i + 1;
            }
        }
    }
}
