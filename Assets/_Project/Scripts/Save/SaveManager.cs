using System;
using System.IO;
using UnityEngine;

namespace RoyalSmash.Save
{
    /// <summary>
    /// Central manager for saving and loading player progress.
    /// Uses robust atomic JSON serialization to Application.persistentDataPath.
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
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("SaveManager");
                        _instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
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
            Debug.Log("[SaveManager] Fresh default save data initialized.");
            Save();
            OnSaveLoaded?.Invoke(_data);
        }

        private void ValidateData()
        {
            if (_data == null) _data = new PlayerSaveData();
            if (_data.RewardedLevelIds == null)
            {
                _data.RewardedLevelIds = new System.Collections.Generic.List<int>();
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
        /// Saves current player data to disk atomically.
        /// </summary>
        public void Save()
        {
            if (_data == null) _data = new PlayerSaveData();

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

                OnSaveUpdated?.Invoke(_data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save write failed: {ex.Message}");
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
