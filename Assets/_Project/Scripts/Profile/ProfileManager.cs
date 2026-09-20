using System;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Profile
{
    /// <summary>
    /// Central manager for the player's profile and basic gameplay statistics.
    /// Handles avatar and frame selection, unlimited free username changes,
    /// Level 10 username prompt validation, and stats tracking.
    /// Follows Master Build Prompt Section 18 specifications.
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        private static ProfileManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static ProfileManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<ProfileManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("ProfileManager");
                        _instance = go.AddComponent<ProfileManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private ProfileConfig _config;

        public ProfileConfig Config
        {
            get
            {
                if (_config == null)
                {
#if UNITY_EDITOR
                    _config = UnityEditor.AssetDatabase.LoadAssetAtPath<ProfileConfig>("Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset");
#endif
                }
                return _config;
            }
        }

        public event Action<string, int, int> OnProfileUpdated;
        public event Action OnStatsUpdated;

        public string Username
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null && !string.IsNullOrEmpty(save.Username) ? save.Username : "Player";
            }
        }

        public int AvatarId
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null ? save.AvatarId : 0;
            }
        }

        public int FrameId
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null ? save.FrameId : 0;
            }
        }

        public int TotalShotsFired
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null ? save.TotalShotsFired : 0;
            }
        }

        public int TotalWins
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null ? save.TotalWins : 0;
            }
        }

        public int CompletedLevelsCount
        {
            get
            {
                var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
                return save != null ? save.CompletedLevelsCount : 0;
            }
        }

        public AvatarData CurrentAvatar => Config != null ? Config.GetAvatar(AvatarId) : null;
        public FrameData CurrentFrame => Config != null ? Config.GetFrame(FrameId) : null;

        private void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
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

        public void SetConfig(ProfileConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Updates the player's username. Unlimited and completely free (Section 18.2).
        /// </summary>
        public bool SetUsername(string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername)) return false;

            newUsername = newUsername.Trim();
            if (newUsername.Length > 16)
            {
                newUsername = newUsername.Substring(0, 16);
            }

            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.Username = newUsername;
                saveMgr.Save();
                NotifyProfileUpdated();
                Debug.Log($"[ProfileManager] Username updated to: '{newUsername}'.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Selects an avatar from the 8 available defaults (0..7).
        /// </summary>
        public void SetAvatar(int avatarId)
        {
            if (avatarId < 0 || avatarId > 7) return;

            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.AvatarId = avatarId;
                saveMgr.Save();
                NotifyProfileUpdated();
                Debug.Log($"[ProfileManager] Avatar selected: {avatarId}.");
            }
        }

        /// <summary>
        /// Selects a frame border from the 4 available defaults (0..3).
        /// </summary>
        public void SetFrame(int frameId)
        {
            if (frameId < 0 || frameId > 3) return;

            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.FrameId = frameId;
                saveMgr.Save();
                NotifyProfileUpdated();
                Debug.Log($"[ProfileManager] Frame selected: {frameId}.");
            }
        }

        /// <summary>
        /// Determines if the Level 10 username prompt should be triggered.
        /// Triggers when player reaches Level 10 and has not yet been prompted.
        /// </summary>
        public bool ShouldPromptLevel10Username()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (save == null) return false;
            return save.CurrentLevel >= 10 && !save.UsernamePrompted;
        }

        /// <summary>
        /// Marks the Level 10 username prompt as completed.
        /// </summary>
        public void MarkUsernamePrompted()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.UsernamePrompted = true;
                saveMgr.Save();
            }
        }

        public void RecordShotFired()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.TotalShotsFired++;
                saveMgr.Save();
                OnStatsUpdated?.Invoke();
            }
        }

        public void RecordWin()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.TotalWins++;
                saveMgr.Data.CompletedLevelsCount++;
                saveMgr.Save();
                OnStatsUpdated?.Invoke();
            }
        }

        private void NotifyProfileUpdated()
        {
            OnProfileUpdated?.Invoke(Username, AvatarId, FrameId);
        }
    }
}
