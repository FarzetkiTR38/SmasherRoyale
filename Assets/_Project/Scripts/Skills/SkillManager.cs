using System;
using System.Collections;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Skills
{
    /// <summary>
    /// Central manager for active player skills: MegaBall and InfinityBall.
    /// Handles unlock states, remaining uses, active timers, skill stacking, and decoupled event dispatching.
    /// Follows Master Build Prompt Section 12 & Save System Skill patterns.
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        private static SkillManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static SkillManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<SkillManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("SkillManager");
                        _instance = go.AddComponent<SkillManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private SkillConfig _config;

        [Header("Mega Ball State")]
        [SerializeField] private bool _megaBallUnlocked;
        [SerializeField] private int _megaBallUses = 3;
        [SerializeField] private bool _megaBallActive;

        [Header("Infinity Ball State")]
        [SerializeField] private bool _infinityBallUnlocked;
        [SerializeField] private int _infinityBallUses = 3;
        [SerializeField] private bool _infinityBallActive;
        [SerializeField] private float _infinityBallTimeRemaining;

        private Coroutine _infinityBallRoutine;

        // Events: (SkillType skill, bool isUnlocked, int remainingUses, bool isActive, float durationRemaining)
        public event Action<SkillType, bool, int, bool, float> OnSkillStateChanged;
        public event Action<SkillType> OnSkillActivated;
        public event Action<SkillType> OnSkillDeactivated;

        public SkillConfig Config => _config;

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
                InitializeDefaults();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SyncWithSaveData();
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

        private void InitializeDefaults()
        {
            if (_config == null)
            {
                _config = Resources.Load<SkillConfig>("Config/SkillConfig");
            }

            if (_config != null)
            {
                _megaBallUses = _config.MegaBallDefaultUses;
                _infinityBallUses = _config.InfinityBallDefaultUses;
            }
        }

        /// <summary>
        /// Synchronizes skill inventory and unlock flags with persistent SaveManager data.
        /// </summary>
        public void SyncWithSaveData()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                var data = saveMgr.Data;
                _megaBallUses = data.MegaBallUses;
                _infinityBallUses = data.InfinityBallUses;

                int megaUnlock = _config != null ? _config.MegaBallUnlockLevel : 5;
                int infUnlock = _config != null ? _config.InfinityBallUnlockLevel : 10;

                _megaBallUnlocked = data.MegaBallUnlocked || data.CurrentLevel >= megaUnlock;
                _infinityBallUnlocked = data.InfinityBallUnlocked || data.CurrentLevel >= infUnlock;
            }

            NotifySkillState(SkillType.MegaBall);
            NotifySkillState(SkillType.InfinityBall);
        }

        private void SaveStateToDisk()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.MegaBallUses = _megaBallUses;
                saveMgr.Data.InfinityBallUses = _infinityBallUses;
                saveMgr.Data.MegaBallUnlocked = _megaBallUnlocked;
                saveMgr.Data.InfinityBallUnlocked = _infinityBallUnlocked;
                saveMgr.Save();
            }
        }

        /// <summary>
        /// Updates unlock states based on active player level (Level 5 for MegaBall, Level 10 for InfinityBall).
        /// </summary>
        public void UpdateUnlockStateForLevel(int playerLevel)
        {
            int megaUnlock = _config != null ? _config.MegaBallUnlockLevel : 5;
            int infUnlock = _config != null ? _config.InfinityBallUnlockLevel : 10;

            _megaBallUnlocked = playerLevel >= megaUnlock;
            _infinityBallUnlocked = playerLevel >= infUnlock;

            SaveStateToDisk();

            NotifySkillState(SkillType.MegaBall);
            NotifySkillState(SkillType.InfinityBall);
        }

        public bool IsSkillUnlocked(SkillType skill)
        {
            return skill switch
            {
                SkillType.MegaBall => _megaBallUnlocked,
                SkillType.InfinityBall => _infinityBallUnlocked,
                _ => false
            };
        }

        public int GetRemainingUses(SkillType skill)
        {
            return skill switch
            {
                SkillType.MegaBall => _megaBallUses,
                SkillType.InfinityBall => _infinityBallUses,
                _ => 0
            };
        }

        public bool IsSkillActive(SkillType skill)
        {
            return skill switch
            {
                SkillType.MegaBall => _megaBallActive,
                SkillType.InfinityBall => _infinityBallActive,
                _ => false
            };
        }

        public float GetActiveTimer(SkillType skill)
        {
            if (skill == SkillType.InfinityBall) return _infinityBallTimeRemaining;
            return 0f;
        }

        /// <summary>
        /// Attempts to activate a skill. Consumes 1 use and applies active modifier.
        /// Supports stacking: both MegaBall and InfinityBall can be active simultaneously.
        /// </summary>
        public bool ActivateSkill(SkillType skill)
        {
            if (!IsSkillUnlocked(skill))
            {
                Debug.LogWarning($"[SkillManager] Cannot activate {skill}: Skill is locked!");
                return false;
            }

            if (GetRemainingUses(skill) <= 0)
            {
                Debug.LogWarning($"[SkillManager] Cannot activate {skill}: No uses remaining!");
                return false;
            }

            if (IsSkillActive(skill))
            {
                Debug.Log($"[SkillManager] {skill} is already active.");
                return false;
            }

            switch (skill)
            {
                case SkillType.MegaBall:
                    _megaBallUses = Mathf.Max(0, _megaBallUses - 1);
                    _megaBallActive = true;
                    SaveStateToDisk();
                    if (RoyalSmash.Audio.AudioManager.Instance != null)
                    {
                        RoyalSmash.Audio.AudioManager.Instance.PlaySkillMegaBall();
                    }
                    if (RoyalSmash.VFX.VFXManager.Instance != null)
                    {
                        RoyalSmash.VFX.VFXManager.Instance.SpawnSkillActivation(transform.position, SkillType.MegaBall);
                    }
                    if (RoyalSmash.Haptics.HapticManager.Instance != null)
                    {
                        RoyalSmash.Haptics.HapticManager.Instance.TriggerSkillActivationHaptic();
                    }
                    OnSkillActivated?.Invoke(SkillType.MegaBall);
                    NotifySkillState(SkillType.MegaBall);
                    Debug.Log($"[SkillManager] Mega Ball activated! ({_megaBallUses} uses left)");
                    return true;

                case SkillType.InfinityBall:
                    _infinityBallUses = Mathf.Max(0, _infinityBallUses - 1);
                    _infinityBallActive = true;
                    SaveStateToDisk();
                    float duration = _config != null ? _config.InfinityBallDuration : 5.0f;
                    _infinityBallTimeRemaining = duration;

                    if (_infinityBallRoutine != null) StopCoroutine(_infinityBallRoutine);
                    _infinityBallRoutine = StartCoroutine(InfinityBallCountdownRoutine(duration));

                    if (RoyalSmash.Audio.AudioManager.Instance != null)
                    {
                        RoyalSmash.Audio.AudioManager.Instance.PlaySkillInfinityBall();
                    }
                    if (RoyalSmash.VFX.VFXManager.Instance != null)
                    {
                        RoyalSmash.VFX.VFXManager.Instance.SpawnSkillActivation(transform.position, SkillType.InfinityBall);
                    }
                    if (RoyalSmash.Haptics.HapticManager.Instance != null)
                    {
                        RoyalSmash.Haptics.HapticManager.Instance.TriggerSkillActivationHaptic();
                    }
                    OnSkillActivated?.Invoke(SkillType.InfinityBall);
                    NotifySkillState(SkillType.InfinityBall);
                    Debug.Log($"[SkillManager] Infinity Ball activated for {duration:F1}s! ({_infinityBallUses} uses left)");
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Consumes the active MegaBall one-shot buff (called immediately after Mortar fires the MegaBall).
        /// </summary>
        public void ConsumeMegaBall()
        {
            if (_megaBallActive)
            {
                _megaBallActive = false;
                OnSkillDeactivated?.Invoke(SkillType.MegaBall);
                NotifySkillState(SkillType.MegaBall);
                Debug.Log("[SkillManager] Mega Ball consumed by shot.");
            }
        }

        /// <summary>
        /// Manually deactivates a skill (e.g. on level restart or completion).
        /// </summary>
        public void ResetActiveSkills()
        {
            if (_megaBallActive)
            {
                _megaBallActive = false;
                OnSkillDeactivated?.Invoke(SkillType.MegaBall);
                NotifySkillState(SkillType.MegaBall);
            }

            if (_infinityBallActive)
            {
                if (_infinityBallRoutine != null)
                {
                    StopCoroutine(_infinityBallRoutine);
                    _infinityBallRoutine = null;
                }
                _infinityBallActive = false;
                _infinityBallTimeRemaining = 0f;
                OnSkillDeactivated?.Invoke(SkillType.InfinityBall);
                NotifySkillState(SkillType.InfinityBall);
            }
        }

        private IEnumerator InfinityBallCountdownRoutine(float duration)
        {
            _infinityBallTimeRemaining = duration;
            while (_infinityBallTimeRemaining > 0f)
            {
                _infinityBallTimeRemaining -= Time.deltaTime;
                NotifySkillState(SkillType.InfinityBall);
                yield return null;
            }

            _infinityBallTimeRemaining = 0f;
            _infinityBallActive = false;
            _infinityBallRoutine = null;

            OnSkillDeactivated?.Invoke(SkillType.InfinityBall);
            NotifySkillState(SkillType.InfinityBall);
            Debug.Log("[SkillManager] Infinity Ball duration expired.");
        }

        public void SetSkillUnlocked(SkillType skill, bool unlocked)
        {
            if (skill == SkillType.MegaBall) _megaBallUnlocked = unlocked;
            else if (skill == SkillType.InfinityBall) _infinityBallUnlocked = unlocked;
            SaveStateToDisk();
            NotifySkillState(skill);
        }

        public void SetRemainingUses(SkillType skill, int uses)
        {
            if (skill == SkillType.MegaBall) _megaBallUses = Mathf.Max(0, uses);
            else if (skill == SkillType.InfinityBall) _infinityBallUses = Mathf.Max(0, uses);
            SaveStateToDisk();
            NotifySkillState(skill);
        }

        public void AddUses(SkillType skill, int amount)
        {
            SetRemainingUses(skill, GetRemainingUses(skill) + amount);
        }

        public void AddSkillUses(SkillType skill, int amount)
        {
            AddUses(skill, amount);
        }

        /// <summary>
        /// Purchases additional skill uses with Gold via GoldManager.
        /// </summary>
        public bool BuySkillUsesWithGold(SkillType skill, int amount = 1)
        {
            if (Economy.GoldManager.Instance != null)
            {
                return Economy.GoldManager.Instance.BuySkillUses(skill, amount);
            }
            return false;
        }

        public void NotifySkillState(SkillType skill)
        {
            bool isUnlocked = IsSkillUnlocked(skill);
            int uses = GetRemainingUses(skill);
            bool isActive = IsSkillActive(skill);
            float timer = GetActiveTimer(skill);

            OnSkillStateChanged?.Invoke(skill, isUnlocked, uses, isActive, timer);
        }
    }
}
