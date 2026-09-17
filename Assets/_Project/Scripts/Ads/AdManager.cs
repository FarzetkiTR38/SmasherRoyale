using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Central manager for all game advertisements (Interstitials and Rewarded Videos).
    /// Enforces No Ads purchase bypass, cooldown rules, idempotency, and safe fallback handling.
    /// Follows Master Build Prompt Section 15 & Section 35 (Phase 14).
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private AdConfig _config;

        private IAdProvider _provider;
        private bool _isAdShowing = false;
        private float _lastInterstitialTime = -999f;

        // Events
        public event Action<string> OnInterstitialShown;
        public event Action<string> OnRewardedAdCompleted;
        public event Action<string> OnAdFailed;

        public bool IsAdShowing => _isAdShowing;
        public AdConfig Config => _config;
        public IAdProvider Provider => _provider;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeManager();
        }

        private void InitializeManager()
        {
            if (_config == null)
            {
#if UNITY_EDITOR
                _config = UnityEditor.AssetDatabase.LoadAssetAtPath<AdConfig>(
                    "Assets/_Project/ScriptableObjects/Config/AdConfig.asset");
#endif
                if (_config == null)
                {
                    _config = ScriptableObject.CreateInstance<AdConfig>();
                }
            }

            // By default or in simulation mode, initialize SimulatedAdProvider
            if (_config.EnableSimulationMode || _provider == null)
            {
                var simProvider = new SimulatedAdProvider();
                _provider = simProvider;
                _provider.Initialize(_config);
            }
        }

        /// <summary>
        /// Registers the active UI simulation dialog if present in the scene.
        /// </summary>
        public void RegisterSimulatedDialog(SimulatedAdDialog dialog)
        {
            if (_provider is SimulatedAdProvider simProvider)
            {
                simProvider.SetDialog(dialog);
            }
        }

        /// <summary>
        /// Replaces the active ad provider (e.g. when connecting real Google Mobile Ads SDK).
        /// </summary>
        public void SetProvider(IAdProvider provider)
        {
            _provider = provider;
            _provider?.Initialize(_config);
        }

        /// <summary>
        /// Returns whether an interstitial ad can be displayed right now.
        /// Bypassed if player purchased 'No Ads'.
        /// </summary>
        public bool CanShowInterstitial()
        {
            if (_isAdShowing) return false;

            // Section 15.1: No Ads purchase permanently removes mandatory/interstitial ads
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.NoAdsPurchased)
            {
                return false;
            }

            // Cooldown check
            if (_config != null && (Time.unscaledTime - _lastInterstitialTime) < _config.InterstitialCooldownSeconds)
            {
                return false;
            }

            return _provider != null && _provider.IsInterstitialReady();
        }

        /// <summary>
        /// Returns whether a rewarded ad is ready to show.
        /// Section 15.2: Always available even if No Ads is purchased.
        /// </summary>
        public bool CanShowRewarded()
        {
            if (_isAdShowing) return false;
            return _provider != null && _provider.IsRewardedReady();
        }

        /// <summary>
        /// Evaluates level progression milestone (every 5 levels) and triggers an interstitial if applicable.
        /// </summary>
        public void CheckAndShowProgressionInterstitial(int completedLevel, Action onComplete = null)
        {
            int interval = _config != null ? _config.InterstitialLevelInterval : 5;

            if (completedLevel > 0 && completedLevel % interval == 0)
            {
                Debug.Log($"[AdManager] Level {completedLevel} reached progression interval ({interval}). Attempting interstitial...");
                ShowInterstitial($"progression_level_{completedLevel}", onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Triggers an interstitial ad after a level defeat if configured and allowed.
        /// </summary>
        public void ShowLoseInterstitial(Action onComplete = null)
        {
            if (_config != null && !_config.ShowInterstitialOnLose)
            {
                onComplete?.Invoke();
                return;
            }

            Debug.Log("[AdManager] Defeat evaluated. Attempting lose interstitial...");
            ShowInterstitial("level_lose", onComplete);
        }

        /// <summary>
        /// Displays an interstitial ad if allowed. Calls onComplete immediately if skipped or failed.
        /// </summary>
        public void ShowInterstitial(string placement, Action onComplete = null)
        {
            if (!CanShowInterstitial())
            {
                Debug.Log($"[AdManager] Interstitial for '{placement}' skipped (No Ads owned, cooldown, or not ready).");
                onComplete?.Invoke();
                return;
            }

            _isAdShowing = true;
            _lastInterstitialTime = Time.unscaledTime;

            _provider.ShowInterstitial(
                onClosed: () =>
                {
                    _isAdShowing = false;
                    OnInterstitialShown?.Invoke(placement);
                    onComplete?.Invoke();
                },
                onError: (err) =>
                {
                    _isAdShowing = false;
                    Debug.LogWarning($"[AdManager] Interstitial error for '{placement}': {err}. Continuing gameplay...");
                    OnAdFailed?.Invoke(err);
                    onComplete?.Invoke();
                }
            );
        }

        /// <summary>
        /// Displays a rewarded ad. Enforces idempotent reward granting (only once per view).
        /// Section 15.2 & 15.3: Available with No Ads; reward ONLY on successful completion.
        /// </summary>
        public void ShowRewarded(string placement, Action onRewardGranted, Action onClosed = null, Action<string> onFailed = null)
        {
            if (_isAdShowing)
            {
                Debug.LogWarning("[AdManager] Cannot show rewarded ad: another ad is already active.");
                onFailed?.Invoke("Ad already active");
                return;
            }

            if (_provider == null || !_provider.IsRewardedReady())
            {
                Debug.LogWarning("[AdManager] Rewarded ad not ready.");
                onFailed?.Invoke("Ad not ready");
                return;
            }

            _isAdShowing = true;
            bool rewardClaimed = false;

            _provider.ShowRewarded(
                onRewarded: () =>
                {
                    // Idempotent safeguard: grant exactly once
                    if (!rewardClaimed)
                    {
                        rewardClaimed = true;
                        Debug.Log($"[AdManager] Rewarded ad '{placement}' completed! Granting reward.");
                        onRewardGranted?.Invoke();
                        OnRewardedAdCompleted?.Invoke(placement);
                    }
                },
                onClosed: () =>
                {
                    _isAdShowing = false;
                    onClosed?.Invoke();
                },
                onError: (err) =>
                {
                    _isAdShowing = false;
                    Debug.LogWarning($"[AdManager] Rewarded ad failed '{placement}': {err}. No reward granted.");
                    OnAdFailed?.Invoke(err);
                    onFailed?.Invoke(err);
                }
            );
        }

        // Test Helper
        public void ResetInterstitialCooldownForTesting()
        {
            _lastInterstitialTime = -999f;
            _isAdShowing = false;
        }
    }
}
