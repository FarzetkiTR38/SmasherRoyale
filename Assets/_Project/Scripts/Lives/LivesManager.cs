using System;
using System.Collections.Generic;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Lives
{
    /// <summary>
    /// Central manager for the Lives System and Offline/Online Regeneration.
    /// Handles life deduction on defeat, ungraceful app exit, level exit,
    /// passive regeneration timers (+1 life / 30m), and Gold/Ad life purchases.
    /// Follows Master Build Prompt Section 11 & Save System Skill patterns.
    /// </summary>
    public class LivesManager : MonoBehaviour
    {
        private static LivesManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static LivesManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<LivesManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("LivesManager");
                        _instance = go.AddComponent<LivesManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private LivesConfig _config;

        [Header("Runtime State")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        // Events
        public event Action<int, int> OnLivesChanged;
        public event Action<int> OnLifeRegenerated;
        public event Action<TimeSpan> OnRegenTimerUpdated;
        public event Action OnOutOfLives;

        // Duplicate deduction protection
        private readonly HashSet<string> _processedDeductions = new HashSet<string>();
        private float _lastTimerDispatch;

        public LivesConfig Config => _config;

        public int CurrentLives
        {
            get
            {
                if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
                {
                    return SaveManager.Instance.Data.Lives;
                }
                return _config != null ? _config.StartingLives : 5;
            }
        }

        public int MaxLives => _config != null ? _config.MaxLives : 5;
        public bool HasLivesToPlay => CurrentLives > 0;
        public bool AreLivesFull => CurrentLives >= MaxLives;

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

                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<LivesConfig>();
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            ProcessRegeneration();
            DispatchLivesChanged();
        }

        private void Update()
        {
            if (!AreLivesFull)
            {
                ProcessRegeneration();

                // Dispatch timer update approximately once per second
                if (Time.time - _lastTimerDispatch >= 1.0f)
                {
                    _lastTimerDispatch = Time.time;
                    OnRegenTimerUpdated?.Invoke(GetTimeUntilNextLife());
                }
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
        /// Deducts 1 life for the given reason (e.g. "LevelLost", "LevelExit", "AppClose").
        /// Uses transactionKey to strictly prevent duplicate deductions for the same event.
        /// </summary>
        public bool DeductLife(string reason, string transactionKey = null)
        {
            if (!string.IsNullOrEmpty(transactionKey))
            {
                if (_processedDeductions.Contains(transactionKey))
                {
                    Debug.Log($"[LivesManager] Duplicate deduction skipped for key: {transactionKey}");
                    return false;
                }
                _processedDeductions.Add(transactionKey);
            }

            var saveMgr = SaveManager.Instance;
            if (saveMgr == null || saveMgr.Data == null)
            {
                Debug.LogWarning("[LivesManager] SaveManager unavailable to deduct life.");
                return false;
            }

            var data = saveMgr.Data;
            if (data.Lives <= 0)
            {
                Debug.Log("[LivesManager] Already at 0 lives. Cannot deduct further.");
                OnOutOfLives?.Invoke();
                return false;
            }

            bool wasFull = data.Lives >= MaxLives;
            data.Lives = Mathf.Max(0, data.Lives - 1);

            // If lives were full, start the regeneration timer now
            if (wasFull && data.Lives < MaxLives)
            {
                data.LifeTimerStartTicks = DateTime.UtcNow.Ticks;
            }

            saveMgr.Save();
            Debug.Log($"[LivesManager] Deducted 1 life ({reason}). Remaining: {data.Lives}/{MaxLives}");

            DispatchLivesChanged();

            if (data.Lives == 0)
            {
                OnOutOfLives?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Adds lives up to MaxLives.
        /// </summary>
        public bool AddLife(int count = 1, string reason = "Reward")
        {
            if (count <= 0) return false;

            var saveMgr = SaveManager.Instance;
            if (saveMgr == null || saveMgr.Data == null) return false;

            var data = saveMgr.Data;
            if (data.Lives >= MaxLives)
            {
                Debug.Log($"[LivesManager] Lives already full ({data.Lives}/{MaxLives}). Cannot add more.");
                return false;
            }

            data.Lives = Mathf.Min(MaxLives, data.Lives + count);

            if (data.Lives >= MaxLives)
            {
                data.LifeTimerStartTicks = 0; // Stop regen timer
            }

            saveMgr.Save();
            Debug.Log($"[LivesManager] Added {count} life ({reason}). Current: {data.Lives}/{MaxLives}");

            DispatchLivesChanged();
            return true;
        }

        /// <summary>
        /// Calculates offline and online passive life regeneration.
        /// Restores +1 life per LifeRegenMinutes, preserving fractional remainder time.
        /// </summary>
        public void ProcessRegeneration()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null || saveMgr.Data == null) return;

            var data = saveMgr.Data;
            if (data.Lives >= MaxLives)
            {
                data.LifeTimerStartTicks = 0;
                return;
            }

            if (data.LifeTimerStartTicks <= 0)
            {
                data.LifeTimerStartTicks = DateTime.UtcNow.Ticks;
                saveMgr.Save();
                return;
            }

            long currentTicks = DateTime.UtcNow.Ticks;
            long elapsedTicks = currentTicks - data.LifeTimerStartTicks;
            if (elapsedTicks < 0)
            {
                // Time tampering / invalid clock protection: reset timer to current
                data.LifeTimerStartTicks = currentTicks;
                saveMgr.Save();
                return;
            }

            TimeSpan elapsed = new TimeSpan(elapsedTicks);
            double regenMinutes = _config != null ? _config.LifeRegenMinutes : 30.0;
            if (regenMinutes <= 0) regenMinutes = 30.0;

            int livesGained = (int)(elapsed.TotalMinutes / regenMinutes);
            if (livesGained > 0)
            {
                int newLives = Mathf.Min(MaxLives, data.Lives + livesGained);
                int actualGained = newLives - data.Lives;
                data.Lives = newLives;

                if (data.Lives >= MaxLives)
                {
                    data.LifeTimerStartTicks = 0; // Stopped at full capacity
                }
                else
                {
                    // Carry over remainder minutes
                    double remainderMinutes = elapsed.TotalMinutes % regenMinutes;
                    data.LifeTimerStartTicks = currentTicks - (long)(remainderMinutes * TimeSpan.TicksPerMinute);
                }

                saveMgr.Save();
                Debug.Log($"[LivesManager] Regenerated +{actualGained} life! Current: {data.Lives}/{MaxLives}");

                OnLifeRegenerated?.Invoke(actualGained);
                DispatchLivesChanged();
            }
        }

        /// <summary>
        /// Returns remaining time until next life regeneration.
        /// Returns TimeSpan.Zero if lives are full.
        /// </summary>
        public TimeSpan GetTimeUntilNextLife()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null || saveMgr.Data == null) return TimeSpan.Zero;

            var data = saveMgr.Data;
            if (data.Lives >= MaxLives || data.LifeTimerStartTicks <= 0)
            {
                return TimeSpan.Zero;
            }

            long elapsedTicks = DateTime.UtcNow.Ticks - data.LifeTimerStartTicks;
            TimeSpan elapsed = new TimeSpan(Math.Max(0, elapsedTicks));
            double regenMinutes = _config != null ? _config.LifeRegenMinutes : 30.0;
            double remainderMinutes = elapsed.TotalMinutes % regenMinutes;
            double minutesRemaining = regenMinutes - remainderMinutes;

            return TimeSpan.FromMinutes(Math.Max(0, minutesRemaining));
        }

        /// <summary>
        /// Purchases +1 life with Gold.
        /// </summary>
        public bool BuyLifeWithGold()
        {
            if (AreLivesFull)
            {
                Debug.Log("[LivesManager] Lives already at maximum capacity.");
                return false;
            }

            int price = _config != null ? _config.LifePriceGold : 100;

            if (Economy.GoldManager.Instance != null)
            {
                if (!Economy.GoldManager.Instance.SpendGold(price, "LifePurchase"))
                {
                    return false;
                }
                AddLife(1, "GoldPurchase");
                return true;
            }

            var saveMgr = SaveManager.Instance;
            if (saveMgr == null || saveMgr.Data == null) return false;

            if (saveMgr.Data.Gold < price)
            {
                Debug.LogWarning($"[LivesManager] Insufficient Gold ({saveMgr.Data.Gold} < {price}).");
                return false;
            }

            saveMgr.Data.Gold -= price;
            AddLife(1, "GoldPurchase");
            saveMgr.Save();
            return true;
        }

        /// <summary>
        /// Grants +1 life from Rewarded Ad completion.
        /// </summary>
        public bool GrantRewardedAdLife()
        {
            int reward = _config != null ? _config.RewardedAdLifeReward : 1;
            return AddLife(reward, "RewardedAd");
        }

        public void ClearTransactionGuards()
        {
            _processedDeductions.Clear();
        }

        private void DispatchLivesChanged()
        {
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        }

        // -------------------------------------------------------------
        // Context Menu Helpers for Debugging & Testing
        // -------------------------------------------------------------
        [ContextMenu("Deduct 1 Life")]
        private void MenuDeductLife() => DeductLife("DebugMenu");

        [ContextMenu("Add 1 Life")]
        private void MenuAddLife() => AddLife(1, "DebugMenu");

        [ContextMenu("Set 0 Lives")]
        private void MenuSetZeroLives()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Lives = 0;
                SaveManager.Instance.Data.LifeTimerStartTicks = DateTime.UtcNow.Ticks;
                SaveManager.Instance.Save();
                DispatchLivesChanged();
                OnOutOfLives?.Invoke();
            }
        }

        [ContextMenu("Refill Lives to 5")]
        private void MenuRefillLives()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Lives = MaxLives;
                SaveManager.Instance.Data.LifeTimerStartTicks = 0;
                SaveManager.Instance.Save();
                DispatchLivesChanged();
            }
        }

        [ContextMenu("Fast Forward 30 Mins (Regen +1)")]
        private void MenuFastForward30Mins()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.LifeTimerStartTicks -= TimeSpan.FromMinutes(31).Ticks;
                ProcessRegeneration();
            }
        }
    }
}
