using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Persistent live multi-device cloud provider.
    /// Supports multi-device synchronization, live competitor merging, offline fallback, and Section 28 rules.
    /// Follows Master Build Prompt Section 19 (Leaderboard) & Section 28 (Cloud Sync).
    /// </summary>
    public class SimulatedCloudProvider : ICloudSyncProvider
    {
        private LeaderboardConfig _config;
        private PlayerSaveData _cloudData;

        public bool IsOnline { get; private set; } = true;
        public bool SimulateServiceOutage { get; set; } = false;
        public PlayerSaveData CloudData => _cloudData;

        public SimulatedCloudProvider(LeaderboardConfig config = null)
        {
            _config = config;
            if (_config == null)
            {
#if UNITY_EDITOR
                _config = UnityEditor.AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(
                    "Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset");
#endif
                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<LeaderboardConfig>();
                }
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

            // Persist player score to shared live cloud storage
            string playerId = !string.IsNullOrEmpty(localData.PlayerId) ? localData.PlayerId : "local_player";
            SharedCloudLeaderboardStore.SubmitScore(
                playerId: playerId,
                username: localData.Username,
                level: localData.CurrentLevel,
                avatarIndex: localData.AvatarIndex,
                frameIndex: localData.FrameIndex
            );

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

            var merged = BuildMergedLeaderboard();
            onComplete?.Invoke(true, merged, "Success");
        }

        /// <summary>
        /// Merges persistent live devices with default 1-30 mock competitors, sorts descending by level, and assigns ranks.
        /// </summary>
        public List<LeaderboardEntryData> BuildMergedLeaderboard(string localPlayerIdOverride = null)
        {
            string localPlayerId = localPlayerIdOverride;
            if (string.IsNullOrEmpty(localPlayerId))
            {
                if (_cloudData != null && !string.IsNullOrEmpty(_cloudData.PlayerId))
                {
                    localPlayerId = _cloudData.PlayerId;
                }
                else if (SaveManager.HasInstance && SaveManager.Instance.Data != null && !string.IsNullOrEmpty(SaveManager.Instance.Data.PlayerId))
                {
                    localPlayerId = SaveManager.Instance.Data.PlayerId;
                }
                else
                {
                    localPlayerId = "local_player";
                }
            }

            // 1. Ensure config is present and start with default 35 mock competitors (levels strictly 2-29)
            if (_config == null)
            {
#if UNITY_EDITOR
                _config = UnityEditor.AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(
                    "Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset");
#endif
                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<LeaderboardConfig>();
                }
            }

            var baseList = _config != null ? _config.GenerateDefaultCompetitors() : new List<LeaderboardEntryData>();
            var merged = new Dictionary<string, LeaderboardEntryData>();

            foreach (var bot in baseList)
            {
                bot.IsLocalPlayer = false;
                merged[bot.PlayerId] = bot;
            }

            // 2. Overlay all live devices from shared cloud store
            var liveEntries = SharedCloudLeaderboardStore.GetLiveEntries(localPlayerId);
            foreach (var live in liveEntries)
            {
                merged[live.PlayerId] = live;
            }

            // 3. Ensure local player is represented if not yet saved in store
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                var localData = SaveManager.Instance.Data;
                string pid = !string.IsNullOrEmpty(localData.PlayerId) ? localData.PlayerId : localPlayerId;
                if (!merged.ContainsKey(pid))
                {
                    merged[pid] = new LeaderboardEntryData(
                        rank: 0,
                        playerId: pid,
                        username: localData.Username,
                        level: localData.CurrentLevel,
                        avatarIndex: localData.AvatarIndex,
                        frameIndex: localData.FrameIndex,
                        isLocalPlayer: true
                    );
                }
            }

            // 4. Sort strictly descending by Level, then ascending by Username
            var sortedList = merged.Values
                .OrderByDescending(e => e.Level)
                .ThenBy(e => e.Username)
                .ToList();

            // 5. Reassign 1-based ranks and ensure local player flag is strictly accurate
            for (int i = 0; i < sortedList.Count; i++)
            {
                sortedList[i].Rank = i + 1;
                sortedList[i].IsLocalPlayer = (!string.IsNullOrEmpty(localPlayerId) && sortedList[i].PlayerId == localPlayerId);
            }

            return sortedList;
        }
    }
}
