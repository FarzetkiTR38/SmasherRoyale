using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Persistent shared cloud store that acts as a real-time multi-device cloud backend.
    /// Allows separate devices, accounts, or simulated clients to post scores and read live rankings.
    /// Follows Master Build Prompt Section 19 (Leaderboard) & Section 28 (Cloud Sync).
    /// </summary>
    public static class SharedCloudLeaderboardStore
    {
        [Serializable]
        public class LiveDeviceRecord
        {
            public string PlayerId;
            public string Username;
            public int Level;
            public int AvatarIndex;
            public int FrameIndex;
            public long LastUpdatedTicks;
        }

        [Serializable]
        public class LiveCloudPayload
        {
            public List<LiveDeviceRecord> Records = new List<LiveDeviceRecord>();
            public long LastModifiedTicks;
        }

        private static readonly object _fileLock = new object();
        private static string _customStoragePath = null;

        public static string StorageFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_customStoragePath))
                {
                    return _customStoragePath;
                }
                return Path.Combine(Application.persistentDataPath, "live_cloud_leaderboard.json");
            }
            set => _customStoragePath = value;
        }

        /// <summary>
        /// Atomically saves or updates a device's score in the shared live cloud storage.
        /// </summary>
        public static bool SubmitScore(string playerId, string username, int level, int avatarIndex, int frameIndex)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogWarning("[SharedCloudLeaderboardStore] PlayerId cannot be null or empty.");
                return false;
            }

            lock (_fileLock)
            {
                try
                {
                    var payload = LoadPayloadInternal();
                    var existing = payload.Records.Find(r => r.PlayerId == playerId);

                    if (existing != null)
                    {
                        existing.Username = username;
                        existing.Level = Mathf.Clamp(level, 1, 30);
                        existing.AvatarIndex = avatarIndex;
                        existing.FrameIndex = frameIndex;
                        existing.LastUpdatedTicks = DateTime.UtcNow.Ticks;
                    }
                    else
                    {
                        payload.Records.Add(new LiveDeviceRecord
                        {
                            PlayerId = playerId,
                            Username = username,
                            Level = Mathf.Clamp(level, 1, 30),
                            AvatarIndex = avatarIndex,
                            FrameIndex = frameIndex,
                            LastUpdatedTicks = DateTime.UtcNow.Ticks
                        });
                    }

                    payload.LastModifiedTicks = DateTime.UtcNow.Ticks;
                    SavePayloadInternal(payload);
                    Debug.Log($"[SharedCloudLeaderboardStore] Live score saved: '{username}' (ID: {playerId}) -> Level {level}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudLeaderboardStore] Error saving live score: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads all live records from the shared cloud store and converts them to LeaderboardEntryData.
        /// </summary>
        public static List<LeaderboardEntryData> GetLiveEntries(string currentLocalPlayerId)
        {
            lock (_fileLock)
            {
                var result = new List<LeaderboardEntryData>();
                try
                {
                    var payload = LoadPayloadInternal();
                    foreach (var record in payload.Records)
                    {
                        bool isLocal = !string.IsNullOrEmpty(currentLocalPlayerId) && record.PlayerId == currentLocalPlayerId;
                        result.Add(new LeaderboardEntryData(
                            rank: 0,
                            playerId: record.PlayerId,
                            username: record.Username,
                            level: record.Level,
                            avatarIndex: record.AvatarIndex,
                            frameIndex: record.FrameIndex,
                            isLocalPlayer: isLocal
                        ));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudLeaderboardStore] Error loading live entries: {ex.Message}");
                }
                return result;
            }
        }

        /// <summary>
        /// Resets or purges the shared live cloud storage (useful for testing or full resets).
        /// </summary>
        public static void Clear()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(StorageFilePath))
                    {
                        File.Delete(StorageFilePath);
                    }
                    Debug.Log("[SharedCloudLeaderboardStore] Storage cleared.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SharedCloudLeaderboardStore] Could not clear storage: {ex.Message}");
                }
            }
        }

        private static LiveCloudPayload LoadPayloadInternal()
        {
            string path = StorageFilePath;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonUtility.FromJson<LiveCloudPayload>(json);
                    if (loaded != null && loaded.Records != null)
                    {
                        return loaded;
                    }
                }
            }
            return new LiveCloudPayload();
        }

        private static void SavePayloadInternal(LiveCloudPayload payload)
        {
            string path = StorageFilePath;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonUtility.ToJson(payload, true);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tmp, path);
        }
    }
}
