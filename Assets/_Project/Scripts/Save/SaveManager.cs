using System;
using System.IO;
using UnityEngine;
using RoyalSmash.Utils;
using RoyalSmash.Cloud;

namespace RoyalSmash.Save
{
    /// <summary>
    /// Central manager for saving and loading player progress.
    /// Uses robust atomic JSON serialization to Application.persistentDataPath.
    /// Supports automatic cloud save restoration on app reinstall via unique Device ID.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static SaveManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<SaveManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SaveManager");
                        _instance = go.AddComponent<SaveManager>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                    if (_instance != null && _instance._data == null)
                    {
                        _instance.Load();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private const string SAVE_FILE_NAME = "player_save.json";
        private const string BACKUP_FILE_NAME = "player_save_backup.json";

        [SerializeField] private bool _dontDestroyOnLoad = true;

        private PlayerSaveData _data;
        public PlayerSaveData Data => _data;

        public event Action<PlayerSaveData> OnSaveLoaded;
        public event Action<PlayerSaveData> OnSaveUpdated;
        public event Action<PlayerSaveData> OnSaveRestoredFromCloud;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private string BackupFilePath => Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);

        private void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this;
                if (_dontDestroyOnLoad && transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                Load();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
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
        /// Loads save data from disk. If absent or corrupted, loads backup or creates fresh default data.
        /// </summary>
        public void Load()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _data = JsonUtility.FromJson<PlayerSaveData>(json);
                    if (_data != null)
                    {
                        ValidateData();
                        Debug.Log($"[SaveManager] Save loaded successfully. Level: {_data.CurrentLevel}, TutorialCompleted: {_data.TutorialCompleted}");
                        OnSaveLoaded?.Invoke(_data);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Primary save read failed: {ex.Message}. Attempting backup...");
                }
            }

            // Attempt backup if primary failed or missing
            if (File.Exists(BackupFilePath))
            {
                try
                {
                    string jsonBackup = File.ReadAllText(BackupFilePath);
                    _data = JsonUtility.FromJson<PlayerSaveData>(jsonBackup);
                    if (_data != null)
                    {
                        ValidateData();
                        Debug.Log("[SaveManager] Backup save loaded successfully.");
                        OnSaveLoaded?.Invoke(_data);
                        Save(); // Restore primary
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Backup save read failed: {ex.Message}");
                }
            }

            // Create default data
            _data = new PlayerSaveData();
            ValidateData();

            // Check if remote cloud store has existing progress for this Device ID (Uninstall & Reinstall Recovery)
            if (!string.IsNullOrEmpty(_data.DeviceId) && SharedCloudSaveStore.HasDeviceProgress(_data.DeviceId))
            {
                var cloudSave = SharedCloudSaveStore.LoadDeviceProgress(_data.DeviceId);
                if (cloudSave != null && (cloudSave.CurrentLevel > _data.CurrentLevel || cloudSave.CurrentLevel == _data.CurrentLevel && (cloudSave.Gold > _data.Gold || cloudSave.NoAdsPurchased)))
                {
                    _data = cloudSave.Clone();
                    ValidateData();
                    Debug.Log($"[SaveManager] App reinstall detected! Restored stats from Cloud for Device ID '{_data.DeviceId}': Level {_data.CurrentLevel}, Gold {_data.Gold}, NoAds={_data.NoAdsPurchased}");
                    Save();
                    OnSaveLoaded?.Invoke(_data);
                    OnSaveRestoredFromCloud?.Invoke(_data);
                    return;
                }
            }

            Debug.Log("[SaveManager] Fresh default save data initialized.");
            Save();
            OnSaveLoaded?.Invoke(_data);
        }

        private void ValidateData()
        {
            if (_data == null) _data = new PlayerSaveData();
            string currentDeviceId = DeviceIdProvider.GetDeviceId();
            if (string.IsNullOrEmpty(_data.DeviceId) || (DeviceIdProvider.HasOverride && _data.DeviceId != currentDeviceId))
            {
                _data.DeviceId = currentDeviceId;
            }
            if (_data.RewardedLevelIds == null)
            {
                _data.RewardedLevelIds = new System.Collections.Generic.List<int>();
            }
            if (string.IsNullOrEmpty(_data.PlayerId))
            {
                _data.PlayerId = "player_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            }
            if (string.IsNullOrEmpty(_data.Username))
            {
                _data.Username = "Player";
            }
            if (_data.AvatarId < 0 || _data.AvatarId > 7)
            {
                _data.AvatarId = 0;
            }
            if (_data.FrameId < 0 || _data.FrameId > 3)
            {
                _data.FrameId = 0;
            }
        }

        /// <summary>
        /// Saves current player data to disk atomically and syncs to remote cloud device store.
        /// </summary>
        public void Save()
        {
            if (_data == null) _data = new PlayerSaveData();
            ValidateData();

            _data.LastSaveTicks = DateTime.UtcNow.Ticks;

            try
            {
                string json = JsonUtility.ToJson(_data, true);
                string tempPath = SaveFilePath + ".tmp";

                File.WriteAllText(tempPath, json);

                if (File.Exists(SaveFilePath))
                {
                    File.Copy(SaveFilePath, BackupFilePath, true);
                }

                File.Copy(tempPath, SaveFilePath, true);
                File.Delete(tempPath);

                // Persist to remote cloud store indexed by Device ID
                if (!string.IsNullOrEmpty(_data.DeviceId))
                {
                    SharedCloudSaveStore.SaveDeviceProgress(_data.DeviceId, _data);
                }

                OnSaveUpdated?.Invoke(_data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save write failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies cloud save data to local state adhering to Section 28 "Latest/highest progress wins".
        /// </summary>
        public void ApplyCloudSave(PlayerSaveData cloudData, bool persistImmediately = true)
        {
            if (cloudData == null) return;
            string currentDeviceId = _data != null && !string.IsNullOrEmpty(_data.DeviceId) 
                ? _data.DeviceId 
                : DeviceIdProvider.GetDeviceId();

            _data = cloudData.Clone();
            _data.DeviceId = currentDeviceId;
            ValidateData();

            if (persistImmediately)
            {
                Save();
            }

            OnSaveRestoredFromCloud?.Invoke(_data);
            OnSaveUpdated?.Invoke(_data);
        }

        /// <summary>
        /// Simulates deleting the app and reinstalling it:
        /// Wipes local persistent files (player_save.json & backup) and in-memory cache,
        /// then calls Load() to test automatic cloud restoration via unique Device ID.
        /// </summary>
        public bool SimulateUninstallAndReinstall()
        {
            try
            {
                if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
                if (File.Exists(BackupFilePath)) File.Delete(BackupFilePath);
                _data = null;

                Load();
                return _data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] SimulateUninstallAndReinstall failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Marks tutorial as completed and persists to disk.
        /// </summary>
        public void SetTutorialCompleted(bool completed = true)
        {
            if (_data == null) _data = new PlayerSaveData();
            _data.TutorialCompleted = completed;
            Save();
            Debug.Log($"[SaveManager] TutorialCompleted set to: {completed}");
        }

        /// <summary>
        /// Resets all player progress to factory defaults.
        /// </summary>
        public void ResetSave()
        {
            _data = new PlayerSaveData();
            Save();
            Debug.Log("[SaveManager] Save reset to defaults.");
            OnSaveLoaded?.Invoke(_data);
        }
    }
}
