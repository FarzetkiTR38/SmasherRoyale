using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Levels;

namespace RoyalSmash.Economy
{
    /// <summary>
    /// Central manager for the game's Gold currency economy.
    /// Handles earning, spending, duplicate reward prevention, transaction auditing, and disk persistence.
    /// Follows Master Build Prompt Section 1.4, Section 13, and Phase 10 specifications.
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        private static GoldManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static GoldManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<GoldManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("GoldManager");
                        _instance = go.AddComponent<GoldManager>();
                        DontDestroyOnLoad(go);
                    }
                    if (_instance != null && _instance._currentGold == 0)
                    {
                        _instance.InitializeGold();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private EconomyConfig _economyConfig;
        [SerializeField] private LevelRewardConfig _rewardConfig;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private int _currentGold = 1000;

        /// <summary>
        /// Current gold balance in memory.
        /// </summary>
        public int CurrentGold => _currentGold;

        public EconomyConfig Config => _economyConfig;
        public LevelRewardConfig RewardConfig => _rewardConfig;

        /// <summary>
        /// Fired whenever gold balance changes.
        /// Param 1: new total balance, Param 2: delta (positive for gain, negative for spend).
        /// </summary>
        public event Action<int, int> OnGoldChanged;

        /// <summary>
        /// Fired when level completion reward is evaluated.
        /// Parameters: levelIndex, baseReward, ballBonus, totalGoldAwarded, wasFirstTimeReward.
        /// </summary>
        public event Action<int, int, int, int, bool> OnLevelRewardProcessed;

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
                InitializeGold();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SubscribeEvents();
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
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
                SaveManager.Instance.OnSaveLoaded += HandleSaveLoaded;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
                LevelManager.Instance.OnLevelWon += HandleLevelWon;
            }
        }

        private void UnsubscribeEvents()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
            }
        }

        private void HandleSaveLoaded(PlayerSaveData data)
        {
            if (data != null)
            {
                _currentGold = data.Gold;
                OnGoldChanged?.Invoke(_currentGold, 0);
            }
        }

        private void HandleLevelWon(int levelIndex, int baseReward, int remainingBalls, int totalGold)
        {
            ProcessLevelCompletionReward(levelIndex, remainingBalls);
        }

        /// <summary>
        /// Loads gold balance from save data or fallback config.
        /// </summary>
        public void InitializeGold()
        {
            int defaultStarting = _economyConfig != null ? _economyConfig.StartingGold : 1000;

            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                _currentGold = SaveManager.Instance.Data.Gold;
            }
            else
            {
                _currentGold = defaultStarting;
            }

            Debug.Log($"[GoldManager] Initialized with {_currentGold} Gold.");
            OnGoldChanged?.Invoke(_currentGold, 0);
        }

        /// <summary>
        /// Checks if player can afford the specified price.
        /// </summary>
        public bool CanAfford(int amount)
        {
            return amount >= 0 && _currentGold >= amount;
        }

        /// <summary>
        /// Adds gold to player balance and persists atomically.
        /// </summary>
        public void AddGold(int amount, string reason = "General")
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[GoldManager] Attempted to add non-positive gold ({amount}). Ignored.");
                return;
            }

            _currentGold += amount;
            PersistGold();

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayRewardSound();
            }

            Debug.Log($"[GoldManager] Added {amount} Gold ({reason}). New Balance: {_currentGold}");
            OnGoldChanged?.Invoke(_currentGold, amount);
        }

        /// <summary>
        /// Deducts gold from player balance if affordable.
        /// </summary>
        public bool SpendGold(int amount, string reason = "General")
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[GoldManager] Negative spend requested ({amount}). Ignored.");
                return false;
            }

            if (!CanAfford(amount))
            {
                Debug.LogWarning($"[GoldManager] Insufficient Gold for '{reason}' ({_currentGold} < {amount}).");
                return false;
            }

            _currentGold -= amount;
            PersistGold();

            Debug.Log($"[GoldManager] Spent {amount} Gold ({reason}). New Balance: {_currentGold}");
            OnGoldChanged?.Invoke(_currentGold, -amount);
            return true;
        }

        /// <summary>
        /// Checks if the level completion reward was already granted for this level.
        /// </summary>
        public bool HasClaimedLevelReward(int levelIndex)
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                var list = SaveManager.Instance.Data.RewardedLevelIds;
                if (list != null)
                {
                    return list.Contains(levelIndex);
                }
            }
            return false;
        }

        /// <summary>
        /// Processes level win reward with strict duplicate reward prevention.
        /// First completion awards Base + (Remaining Balls * 2).
        /// Replays award 0 Gold.
        /// </summary>
        public int ProcessLevelCompletionReward(int levelIndex, int remainingBalls)
        {
            int baseReward = _rewardConfig != null ? _rewardConfig.BaseLevelReward : 50;
            int multiplier = _rewardConfig != null ? _rewardConfig.RemainingBallMultiplier : 2;
            int clampedBalls = Mathf.Max(0, remainingBalls);
            int ballBonus = clampedBalls * multiplier;
            int calculatedTotal = baseReward + ballBonus;

            // Duplicate reward check
            if (HasClaimedLevelReward(levelIndex))
            {
                Debug.Log($"[GoldManager] Level {levelIndex} was already rewarded. Duplicate reward prevented (Granted 0 Gold).");
                OnLevelRewardProcessed?.Invoke(levelIndex, baseReward, ballBonus, 0, false);
                return 0;
            }

            // Grant first-time reward
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                if (SaveManager.Instance.Data.RewardedLevelIds == null)
                {
                    SaveManager.Instance.Data.RewardedLevelIds = new System.Collections.Generic.List<int>();
                }
                SaveManager.Instance.Data.RewardedLevelIds.Add(levelIndex);
            }

            AddGold(calculatedTotal, $"Level_{levelIndex}_Victory");
            OnLevelRewardProcessed?.Invoke(levelIndex, baseReward, ballBonus, calculatedTotal, true);
            return calculatedTotal;
        }

        /// <summary>
        /// Purchases +1 life with gold using configured pricing.
        /// </summary>
        public bool BuyLife()
        {
            int price = _economyConfig != null ? _economyConfig.LifePurchasePrice : 100;
            if (Lives.LivesManager.Instance != null)
            {
                if (Lives.LivesManager.Instance.AreLivesFull)
                {
                    Debug.Log("[GoldManager] Lives already full.");
                    return false;
                }

                if (SpendGold(price, "LifePurchase"))
                {
                    Lives.LivesManager.Instance.AddLife(1, "GoldPurchase");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Purchases extra uses for a skill with gold.
        /// </summary>
        public bool BuySkillUses(Skills.SkillType skill, int usesCount = 1)
        {
            int pricePerUse = skill switch
            {
                Skills.SkillType.MegaBall => _economyConfig != null ? _economyConfig.MegaBallPurchasePrice : 50,
                Skills.SkillType.InfinityBall => _economyConfig != null ? _economyConfig.InfinityBallPurchasePrice : 50,
                _ => 50
            };

            int totalPrice = pricePerUse * usesCount;

            if (SpendGold(totalPrice, $"SkillPurchase_{skill}_{usesCount}x"))
            {
                if (Skills.SkillManager.Instance != null)
                {
                    Skills.SkillManager.Instance.AddUses(skill, usesCount);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets gold balance directly (used for initialization/tests).
        /// </summary>
        public void SetGold(int amount)
        {
            int previous = _currentGold;
            _currentGold = Mathf.Max(0, amount);
            PersistGold();
            OnGoldChanged?.Invoke(_currentGold, _currentGold - previous);
        }

        private void PersistGold()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Gold = _currentGold;
                SaveManager.Instance.Save();
            }
        }
    }
}
