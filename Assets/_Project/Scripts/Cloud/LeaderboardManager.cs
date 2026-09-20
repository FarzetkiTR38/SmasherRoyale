using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Coordinates leaderboard operations, sorting by level, offline fallback, and local player ranking.
    /// Follows Master Build Prompt Section 19 (Leaderboard).
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        private static LeaderboardManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static LeaderboardManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<LeaderboardManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("LeaderboardManager");
                        _instance = go.AddComponent<LeaderboardManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private List<LeaderboardEntryData> _currentEntries = new List<LeaderboardEntryData>();
        private bool _isCurrentDataCached = false;

        public List<LeaderboardEntryData> CurrentEntries => _currentEntries;
        public bool IsCurrentDataCached => _isCurrentDataCached;

        public event Action<List<LeaderboardEntryData>, bool> OnLeaderboardUpdated;

        private void Awake()
        {
            _isQuitting = false;
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Fetches the global leaderboard. Automatically falls back to offline cache if disconnected.
        /// Callback signature: (success, entries, isCached).
        /// </summary>
        public void FetchLeaderboard(Action<bool, List<LeaderboardEntryData>, bool> onComplete = null)
        {
            var syncMgr = CloudSyncManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<CloudSyncManager>();
            if (syncMgr == null || syncMgr.Provider == null)
            {
                // Fallback to local cache
                LoadFromCacheAndNotify(onComplete);
                return;
            }

            if (!syncMgr.IsOnline)
            {
                // Explicitly offline -> load cached
                Debug.Log("[LeaderboardManager] Offline: Loading cached leaderboard data.");
                LoadFromCacheAndNotify(onComplete);
                return;
            }

            // Attempt online fetch
            syncMgr.Provider.FetchLeaderboard((success, entries, msg) =>
            {
                if (success && entries != null && entries.Count > 0)
                {
                    _isCurrentDataCached = false;
                    _currentEntries = ProcessAndSortEntries(entries);

                    // Save to local cache
                    syncMgr.SaveCache(_currentEntries);

                    OnLeaderboardUpdated?.Invoke(_currentEntries, false);
                    onComplete?.Invoke(true, _currentEntries, false);
                }
                else
                {
                    Debug.LogWarning($"[LeaderboardManager] Online fetch failed ({msg}). Falling back to local cache.");
                    LoadFromCacheAndNotify(onComplete);
                }
            });
        }

        private void LoadFromCacheAndNotify(Action<bool, List<LeaderboardEntryData>, bool> onComplete)
        {
            var syncMgr = CloudSyncManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<CloudSyncManager>();
            var cached = syncMgr != null ? syncMgr.LoadCache() : new List<LeaderboardEntryData>();

            _isCurrentDataCached = true;
            _currentEntries = ProcessAndSortEntries(cached);

            OnLeaderboardUpdated?.Invoke(_currentEntries, true);
            onComplete?.Invoke(true, _currentEntries, true);
        }

        /// <summary>
        /// Ensures local player is present in the list, sorts descending by level, and computes 1-indexed ranks.
        /// </summary>
        public List<LeaderboardEntryData> ProcessAndSortEntries(List<LeaderboardEntryData> rawEntries)
        {
            var list = rawEntries != null ? new List<LeaderboardEntryData>(rawEntries) : new List<LeaderboardEntryData>();
            var saveMgr = SaveManager.Instance;

            string localPlayerId = "local_player";
            string localUsername = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.Username : "Player";
            int localLevel = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.CurrentLevel : 1;
            int localAvatar = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.AvatarIndex : 0;
            int localFrame = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.FrameIndex : 0;

            var playerEntry = list.FirstOrDefault(e => e.IsLocalPlayer || e.PlayerId == localPlayerId);
            if (playerEntry != null)
            {
                playerEntry.Username = localUsername;
                playerEntry.Level = localLevel;
                playerEntry.AvatarIndex = localAvatar;
                playerEntry.FrameIndex = localFrame;
                playerEntry.IsLocalPlayer = true;
            }
            else
            {
                list.Add(new LeaderboardEntryData(
                    rank: 0,
                    playerId: localPlayerId,
                    username: localUsername,
                    level: localLevel,
                    avatarIndex: localAvatar,
                    frameIndex: localFrame,
                    isLocalPlayer: true
                ));
            }

            // Sort: Current Level (descending), then alphabetically by Username
            list = list
                .OrderByDescending(e => e.Level)
                .ThenBy(e => e.Username)
                .ToList();

            // Re-assign ranks 1..N
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Rank = i + 1;
            }

            return list;
        }

        /// <summary>
        /// Retrieves the local player's entry from the current list.
        /// </summary>
        public LeaderboardEntryData GetLocalPlayerEntry()
        {
            return _currentEntries.FirstOrDefault(e => e.IsLocalPlayer);
        }
    }
}
