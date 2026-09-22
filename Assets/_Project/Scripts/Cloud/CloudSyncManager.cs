using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    [Serializable]
    internal class LeaderboardCacheWrapper
    {
        public List<LeaderboardEntryData> Entries = new List<LeaderboardEntryData>();
        public string LastCachedTimeUtc;
    }

    /// <summary>
    /// Coordinates cloud progress synchronization and local leaderboard caching.
    /// Follows Master Build Prompt Section 28 (Latest local progress wins) & Section 19.4 (Offline cache).
    /// </summary>
    public class CloudSyncManager : MonoBehaviour
    {
        private static CloudSyncManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static CloudSyncManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<CloudSyncManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("CloudSyncManager");
                        _instance = go.AddComponent<CloudSyncManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private LeaderboardConfig _config;

        [Header("Backend Mode")]
        [Tooltip("When true, connects to official Unity Gaming Services (Authentication, Cloud Save, Leaderboard). When false, runs simulated local offline backend.")]
        [SerializeField] private bool _useUGS = true;

        private ICloudSyncProvider _provider;

        public event Action<bool> OnOnlineStateChanged;
        public event Action OnSyncCompleted;

        public bool UseUGS
        {
            get => _useUGS;
            set
            {
                _useUGS = value;
                _provider = _useUGS ? (ICloudSyncProvider)new UGSCloudProvider(Config) : new SimulatedCloudProvider(Config);
            }
        }

        public ICloudSyncProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = _useUGS ? (ICloudSyncProvider)new UGSCloudProvider(Config) : new SimulatedCloudProvider(Config);
                }
                return _provider;
            }
        }

        public bool IsOnline => Provider != null && Provider.IsOnline;

        public LeaderboardConfig Config
        {
            get
            {
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
                return _config;
            }
        }

        private string _cacheFilePath;
        private string CacheFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_cacheFilePath))
                {
                    _cacheFilePath = Path.Combine(Application.persistentDataPath, "leaderboard_cache.json");
                }
                return _cacheFilePath;
            }
        }

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

            InitializeManager();
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

        private void InitializeManager()
        {
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

            if (_provider == null)
            {
                _provider = new SimulatedCloudProvider(_config);
            }
        }

        public void SetProvider(ICloudSyncProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Toggles network online status. When reconnected, automatically triggers progress sync.
        /// </summary>
        public void SetOnlineState(bool online)
        {
            if (Provider != null)
            {
                Provider.SetOnlineState(online);
            }

            OnOnlineStateChanged?.Invoke(online);

            if (online)
            {
                Debug.Log("[CloudSyncManager] Internet reconnected! Triggering auto-sync...");
                TriggerSync();
            }
        }

        /// <summary>
        /// Pushes local player data to the cloud.
        /// </summary>
        public void TriggerSync(Action<bool, string> onComplete = null)
        {
            var saveMgr = SaveManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<SaveManager>();
            if (saveMgr == null || saveMgr.Data == null)
            {
                onComplete?.Invoke(false, "SaveManager or data not available");
                return;
            }

            if (Provider == null)
            {
                onComplete?.Invoke(false, "Provider null");
                return;
            }

            Provider.SyncProgress(saveMgr.Data, (success, msg) =>
            {
                if (success)
                {
                    OnSyncCompleted?.Invoke();
                }
                onComplete?.Invoke(success, msg);
            });
        }

        /// <summary>
        /// Saves leaderboard entries to local disk cache for offline availability.
        /// </summary>
        public void SaveCache(List<LeaderboardEntryData> entries)
        {
            try
            {
                var wrapper = new LeaderboardCacheWrapper
                {
                    Entries = entries ?? new List<LeaderboardEntryData>(),
                    LastCachedTimeUtc = DateTime.UtcNow.ToString("o")
                };

                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(CacheFilePath, json);
                Debug.Log($"[CloudSyncManager] Saved {wrapper.Entries.Count} leaderboard entries to local cache.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CloudSyncManager] Error saving leaderboard cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads cached leaderboard entries from disk. Falls back to config defaults if cache is missing.
        /// </summary>
        public List<LeaderboardEntryData> LoadCache()
        {
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    string json = File.ReadAllText(CacheFilePath);
                    var wrapper = JsonUtility.FromJson<LeaderboardCacheWrapper>(json);
                    if (wrapper != null && wrapper.Entries != null && wrapper.Entries.Count > 0)
                    {
                        return wrapper.Entries;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CloudSyncManager] Error reading cache file: {ex.Message}");
            }

            // Fallback to default mock competitors
            if (_config != null)
            {
                return _config.GenerateDefaultCompetitors();
            }

            return new List<LeaderboardEntryData>();
        }
    }
}
