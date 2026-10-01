using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Central manager for all game advertisements (Hybrid Mediation: AdMob + Unity Ads + Fallback).
    /// Enforces No Ads purchase bypass, cooldown rules, idempotency, banner management, and safe fallback handling.
    /// Follows Master Build Prompt Section 15 & Section 35 (Phase 14).
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static AdManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<AdManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("AdManager");
                        _instance = go.AddComponent<AdManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private AdConfig _config;

        private IAdProvider _provider;
        private HybridMediationRouter _hybridRouter;
        private bool _isAdShowing = false;
        private bool _isBannerVisible = false;
        private float _lastInterstitialTime = -999f;

        // Events
        public event Action<string> OnInterstitialShown;
        public event Action<string> OnRewardedAdCompleted;
        public event Action<string> OnAdFailed;
        public event Action<AdNetworkType> OnAdNetworkServed;

        public bool IsAdShowing => _isAdShowing;
        public bool IsBannerVisible => _isBannerVisible;
        public AdConfig Config => _config;
        public HybridMediationRouter HybridRouter => _hybridRouter;
        public AdNetworkType ActiveNetwork => _hybridRouter != null ? _hybridRouter.LastServedNetwork : (_provider != null ? _provider.NetworkType : AdNetworkType.None);

        public IAdProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    InitializeManager();
                }
                return _provider;
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
            SubscribeToSaveEvents();
        }

        private void Start()
        {
            // Auto-display banner in MainMenu if enabled and player doesn't own No Ads
            if (_config != null && _config.AutoShowBannerInMainMenu && CanShowBanner())
            {
                ShowBanner(_config.BannerPosition);
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            UnsubscribeFromSaveEvents();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void SubscribeToSaveEvents()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveUpdated += HandleSaveUpdated;
            }
        }

        private void UnsubscribeFromSaveEvents()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveUpdated -= HandleSaveUpdated;
            }
        }

        private void HandleSaveUpdated(PlayerSaveData data)
        {
            if (data != null && data.NoAdsPurchased)
            {
                HideBanner();
            }
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

            // In simulation mode, use pure SimulatedAdProvider
            if (_config.EnableSimulationMode)
            {
                var simProvider = new SimulatedAdProvider();
                _provider = simProvider;
                _provider.Initialize(_config);
            }
            else
            {
                // Production Hybrid Mediation Router (AdMob + Unity Ads + Fallback)
                _hybridRouter = new HybridMediationRouter();
                _hybridRouter.Initialize(_config);
                _provider = _hybridRouter;
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
            else if (_hybridRouter != null && _hybridRouter.Simulated != null)
            {
                _hybridRouter.Simulated.SetDialog(dialog);
            }
        }

        /// <summary>
        /// Replaces the active ad provider (e.g. For custom test harness).
        /// </summary>
        public void SetProvider(IAdProvider provider)
        {
            _provider = provider;
            if (provider is HybridMediationRouter hybrid)
            {
                _hybridRouter = hybrid;
            }
            _provider?.Initialize(_config);
        }

        #region Interstitial Logic

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

            return Provider != null && Provider.IsInterstitialReady();
        }

        /// <summary>
        /// Returns whether a rewarded ad is ready to show.
        /// Section 15.2: Always available even if No Ads is purchased.
        /// </summary>
        public bool CanShowRewarded()
        {
            if (_isAdShowing) return false;
            return Provider != null && Provider.IsRewardedReady();
        }

        /// <summary>
        /// Returns whether a banner ad can be shown right now.
        /// </summary>
        public bool CanShowBanner()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.NoAdsPurchased)
            {
                return false;
            }
            return Provider != null && Provider.IsBannerReady();
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
        /// Displays an interstitial ad via hybrid mediation waterfall. Calls onComplete immediately if skipped or failed.
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

            Provider.ShowInterstitial(
                onClosed: () =>
                {
                    _isAdShowing = false;
                    OnInterstitialShown?.Invoke(placement);
                    OnAdNetworkServed?.Invoke(ActiveNetwork);
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

        #endregion

        #region Rewarded Video Logic

        /// <summary>
        /// Displays a rewarded ad via hybrid mediation waterfall. Enforces idempotent reward granting (only once per view).
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

            if (Provider == null || !Provider.IsRewardedReady())
            {
                Debug.LogWarning("[AdManager] Rewarded ad not ready.");
                onFailed?.Invoke("Ad not ready");
                return;
            }

            _isAdShowing = true;
            bool rewardClaimed = false;

            Provider.ShowRewarded(
                onRewarded: () =>
                {
                    if (!rewardClaimed)
                    {
                        rewardClaimed = true;
                        Debug.Log($"[AdManager] Rewarded ad '{placement}' completed! Granting reward.");
                        onRewardGranted?.Invoke();
                        OnRewardedAdCompleted?.Invoke(placement);
                        OnAdNetworkServed?.Invoke(ActiveNetwork);
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

        #endregion

        #region Banner Logic

        /// <summary>
        /// Displays a banner ad via hybrid mediation. Bypassed permanently if player owns No Ads.
        /// </summary>
        public void ShowBanner(BannerPosition? position = null, Action onLoaded = null, Action<string> onError = null)
        {
            if (!CanShowBanner())
            {
                Debug.Log("[AdManager] ShowBanner ignored: Banner not allowed (No Ads owned or not ready).");
                return;
            }

            BannerPosition pos = position ?? (_config != null ? _config.BannerPosition : BannerPosition.BottomCenter);
            _isBannerVisible = true;
            Provider.ShowBanner(pos, onLoaded, onError);
        }

        /// <summary>
        /// Hides the active banner ad.
        /// </summary>
        public void HideBanner()
        {
            _isBannerVisible = false;
            Provider?.HideBanner();
        }

        /// <summary>
        /// Destroys the active banner ad and releases native resources.
        /// </summary>
        public void DestroyBanner()
        {
            _isBannerVisible = false;
            Provider?.DestroyBanner();
        }

        #endregion

        #region Rewarded Interstitial Logic

        /// <summary>
        /// Displays a hybrid rewarded interstitial ad.
        /// </summary>
        public void ShowRewardedInterstitial(string placement, Action onRewardGranted, Action onClosed = null, Action<string> onFailed = null)
        {
            if (_isAdShowing)
            {
                onFailed?.Invoke("Ad already active");
                return;
            }

            if (Provider == null || !Provider.IsRewardedInterstitialReady())
            {
                onFailed?.Invoke("Rewarded Interstitial not ready");
                return;
            }

            _isAdShowing = true;
            bool rewardClaimed = false;

            Provider.ShowRewardedInterstitial(
                onRewarded: () =>
                {
                    if (!rewardClaimed)
                    {
                        rewardClaimed = true;
                        onRewardGranted?.Invoke();
                        OnRewardedAdCompleted?.Invoke(placement);
                        OnAdNetworkServed?.Invoke(ActiveNetwork);
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
                    OnAdFailed?.Invoke(err);
                    onFailed?.Invoke(err);
                }
            );
        }

        #endregion

        // Test Helper
        public void ResetInterstitialCooldownForTesting()
        {
            _lastInterstitialTime = -999f;
            _isAdShowing = false;
        }
    }
}
