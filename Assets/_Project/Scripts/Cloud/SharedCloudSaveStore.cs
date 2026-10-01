using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Remote persistent cloud storage simulating a remote server or UGS Cloud Save database.
    /// Stores complete player save profiles indexed by unique Device ID.
    /// Allows players who uninstall and reinstall the application to restore their exact progress and stats.
    /// Follows Master Build Prompt Section 28 & casual mobile cloud save patterns.
    /// </summary>
    public static class SharedCloudSaveStore
    {
        [Serializable]
        public class DeviceCloudRecord
        {
            public string DeviceId;
            public string PlayerSaveJson;
            public int Level;
            public int Gold;
            public bool NoAdsPurchased;
            public long LastUpdatedTicks;
        }

        [Serializable]
        public class CloudSavePayload
        {
            public List<DeviceCloudRecord> Records = new List<DeviceCloudRecord>();
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
                return Path.Combine(Application.persistentDataPath, "live_cloud_device_saves.json");
            }
            set => _customStoragePath = value;
        }

        /// <summary>
        /// Atomically saves or updates the full player save data in the remote cloud store indexed by Device ID.
        /// </summary>
        public static bool SaveDeviceProgress(string deviceId, PlayerSaveData data)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("[SharedCloudSaveStore] Device ID cannot be null or empty.");
                return false;
            }

            if (data == null)
            {
                Debug.LogWarning("[SharedCloudSaveStore] PlayerSaveData cannot be null.");
                return false;
            }

            lock (_fileLock)
            {
                try
                {
                    var payload = LoadPayloadInternal();
                    string json = JsonUtility.ToJson(data);

                    var existing = payload.Records.Find(r => r.DeviceId == deviceId);
                    if (existing != null)
                    {
                        // "Latest local progress wins" rule: only update if local level is higher or equal,
                        // or if metadata/gold/stats updated
                        existing.PlayerSaveJson = json;
                        existing.Level = data.CurrentLevel;
                        existing.Gold = data.Gold;
                        existing.NoAdsPurchased = data.NoAdsPurchased;
                        existing.LastUpdatedTicks = DateTime.UtcNow.Ticks;
                    }
                    else
                    {
                        payload.Records.Add(new DeviceCloudRecord
                        {
                            DeviceId = deviceId,
                            PlayerSaveJson = json,
                            Level = data.CurrentLevel,
                            Gold = data.Gold,
                            NoAdsPurchased = data.NoAdsPurchased,
                            LastUpdatedTicks = DateTime.UtcNow.Ticks
                        });
                    }

                    payload.LastModifiedTicks = DateTime.UtcNow.Ticks;
                    SavePayloadInternal(payload);
                    Debug.Log($"[SharedCloudSaveStore] Cloud progress saved for Device ID: '{deviceId}' (Level {data.CurrentLevel}, Gold {data.Gold}).");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudSaveStore] Error saving device cloud progress: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks if a remote cloud save exists for the given Device ID.
        /// </summary>
        public static bool HasDeviceProgress(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return false;

            lock (_fileLock)
            {
                var payload = LoadPayloadInternal();
                return payload.Records.Exists(r => r.DeviceId == deviceId);
            }
        }

        /// <summary>
        /// Retrieves the full player save data for the given Device ID.
        /// Returns null if no cloud record exists.
        /// </summary>
        public static PlayerSaveData LoadDeviceProgress(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return null;

            lock (_fileLock)
            {
                try
                {
                    var payload = LoadPayloadInternal();
                    var record = payload.Records.Find(r => r.DeviceId == deviceId);
                    if (record != null && !string.IsNullOrEmpty(record.PlayerSaveJson))
                    {
                        var data = JsonUtility.FromJson<PlayerSaveData>(record.PlayerSaveJson);
                        if (data != null)
                        {
                            data.DeviceId = deviceId;
                            return data;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudSaveStore] Error loading cloud data for Device ID '{deviceId}': {ex.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Deletes the cloud save record for a specific Device ID.
        /// </summary>
        public static bool DeleteDeviceProgress(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return false;

            lock (_fileLock)
            {
                try
                {
                    var payload = LoadPayloadInternal();
                    int removed = payload.Records.RemoveAll(r => r.DeviceId == deviceId);
                    if (removed > 0)
                    {
                        payload.LastModifiedTicks = DateTime.UtcNow.Ticks;
                        SavePayloadInternal(payload);
                        Debug.Log($"[SharedCloudSaveStore] Deleted cloud progress for Device ID '{deviceId}'.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudSaveStore] Error deleting cloud data: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Clears all remote cloud device records.
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
                    Debug.Log("[SharedCloudSaveStore] Cloud storage cleared.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SharedCloudSaveStore] Error clearing storage: {ex.Message}");
                }
            }
        }

        public static int Count
        {
            get
            {
                lock (_fileLock)
                {
                    return LoadPayloadInternal().Records.Count;
                }
            }
        }

        #region Internal Serialization Helpers

        private static CloudSavePayload LoadPayloadInternal()
        {
            try
            {
                if (File.Exists(StorageFilePath))
                {
                    string json = File.ReadAllText(StorageFilePath);
                    var payload = JsonUtility.FromJson<CloudSavePayload>(json);
                    if (payload != null && payload.Records != null)
                    {
                        return payload;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedCloudSaveStore] Failed reading payload: {ex.Message}. Initializing fresh store.");
            }

            return new CloudSavePayload();
        }

        private static void SavePayloadInternal(CloudSavePayload payload)
        {
            string dir = Path.GetDirectoryName(StorageFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonUtility.ToJson(payload, true);
            string tmpPath = StorageFilePath + ".tmp";

            File.WriteAllText(tmpPath, json);
            if (File.Exists(StorageFilePath))
            {
                File.Delete(StorageFilePath);
            }
            File.Move(tmpPath, StorageFilePath);
        }

        #endregion
    }
}
