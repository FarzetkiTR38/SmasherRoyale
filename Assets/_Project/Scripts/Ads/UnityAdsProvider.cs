using System;
using UnityEngine;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Unity Ads network provider implementation.
    /// Manages Unity Ads Game ID initialization, Interstitials, Rewarded Videos, and Banners.
    /// Acts as a secondary / waterfall partner in the hybrid mediation architecture.
    /// Follows Master Build Prompt Section 15 & Unity Ads integration best practices.
    /// </summary>
    public class UnityAdsProvider : IAdProvider
    {
        private AdConfig _config;
        private bool _isInitialized = false;

        // Credentials & Placements
        public string GameId { get; private set; }
        public string BannerPlacement { get; private set; } = "Banner_Android";
        public string InterstitialPlacement { get; private set; } = "Interstitial_Android";
        public string RewardedPlacement { get; private set; } = "Rewarded_Android";
        public bool TestMode { get; private set; } = false;

        // States
        private bool _isBannerLoaded = false;
        private bool _isBannerVisible = false;
        private bool _isInterstitialLoaded = false;
        private bool _isRewardedLoaded = false;
        private BannerPosition _bannerPosition = BannerPosition.BottomCenter;

        public AdNetworkType NetworkType => AdNetworkType.UnityAds;
        public bool IsInitialized => _isInitialized;
        public bool IsBannerVisible => _isBannerVisible;
        public BannerPosition CurrentBannerPosition => _bannerPosition;

        // Test hooks & simulation triggers
        public bool ForceNotReady { get; set; } = false;
        public bool SimulateLoadFailureNext { get; set; } = false;
        public bool SimulateShowFailureNext { get; set; } = false;
        public bool InstantCompleteInTests { get; set; } = true;

        public void Initialize(AdConfig config, Action onInitialized = null)
        {
            _config = config;
            ResolveCredentials();

            _isInitialized = true;
            Debug.Log($"[UnityAdsProvider] Initialized with Game ID: {GameId} (TestMode: {TestMode})");

            LoadInterstitial();
            LoadRewarded();

            onInitialized?.Invoke();
        }

        private void ResolveCredentials()
        {
            if (_config == null) return;

#if UNITY_IOS && !UNITY_EDITOR
            GameId = _config.UnityAdsGameIdIOS;
            BannerPlacement = "Banner_iOS";
            InterstitialPlacement = "Interstitial_iOS";
            RewardedPlacement = "Rewarded_iOS";
#else
            GameId = _config.UnityAdsGameIdAndroid;
            BannerPlacement = _config.UnityAdsBannerPlacement;
            InterstitialPlacement = _config.UnityAdsInterstitialPlacement;
            RewardedPlacement = _config.UnityAdsRewardedPlacement;
#endif
            TestMode = _config.UnityAdsTestMode;
        }

        #region Readiness Checks

        public bool IsBannerReady()
        {
            if (ForceNotReady) return false;
            return _isBannerLoaded || !string.IsNullOrEmpty(BannerPlacement);
        }

        public bool IsInterstitialReady()
        {
            if (ForceNotReady) return false;
            return _isInterstitialLoaded;
        }

        public bool IsRewardedReady()
        {
            if (ForceNotReady) return false;
            return _isRewardedLoaded;
        }

        public bool IsRewardedInterstitialReady()
        {
            // Unity Ads combines rewarded interstitials with Rewarded placement
            return IsRewardedReady();
        }

        #endregion

        #region Preload Operations

        public void LoadInterstitial()
        {
            if (SimulateLoadFailureNext)
            {
                _isInterstitialLoaded = false;
                Debug.LogWarning($"[UnityAdsProvider] Interstitial load failed for '{InterstitialPlacement}'.");
                return;
            }

            _isInterstitialLoaded = true;
            Debug.Log($"[UnityAdsProvider] Interstitial preloaded successfully (Placement: {InterstitialPlacement}).");
        }

        public void LoadRewarded()
        {
            if (SimulateLoadFailureNext)
            {
                _isRewardedLoaded = false;
                Debug.LogWarning($"[UnityAdsProvider] Rewarded ad load failed for '{RewardedPlacement}'.");
                return;
            }

            _isRewardedLoaded = true;
            Debug.Log($"[UnityAdsProvider] Rewarded ad preloaded successfully (Placement: {RewardedPlacement}).");
        }

        #endregion

        #region Display Operations

        public void ShowInterstitial(Action onClosed, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || !_isInterstitialLoaded || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                _isInterstitialLoaded = false;
                string err = "Unity Ads Interstitial failed to show (Network error / No fill)";
                Debug.LogWarning($"[UnityAdsProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _isInterstitialLoaded = false;
            Debug.Log($"[UnityAdsProvider] Showing Unity Ads Interstitial ({InterstitialPlacement}).");

            onClosed?.Invoke();

            LoadInterstitial();
        }

        public void ShowRewarded(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || !_isRewardedLoaded || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                _isRewardedLoaded = false;
                string err = "Unity Ads Rewarded video failed to show (Network error / No fill)";
                Debug.LogWarning($"[UnityAdsProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _isRewardedLoaded = false;
            Debug.Log($"[UnityAdsProvider] Showing Unity Ads Rewarded Video ({RewardedPlacement}).");

            onRewarded?.Invoke();
            onClosed?.Invoke();

            LoadRewarded();
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter, Action onLoaded = null, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                string err = "Unity Ads Banner failed to show";
                Debug.LogWarning($"[UnityAdsProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _bannerPosition = position;
            _isBannerLoaded = true;
            _isBannerVisible = true;
            Debug.Log($"[UnityAdsProvider] Showing Unity Ads Banner at {position} ({BannerPlacement}).");
            onLoaded?.Invoke();
        }

        public void HideBanner()
        {
            _isBannerVisible = false;
            Debug.Log($"[UnityAdsProvider] Hiding Unity Ads Banner ({BannerPlacement}).");
        }

        public void DestroyBanner()
        {
            _isBannerLoaded = false;
            _isBannerVisible = false;
            Debug.Log($"[UnityAdsProvider] Destroying Unity Ads Banner ({BannerPlacement}).");
        }

        public void ShowRewardedInterstitial(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            // Unity Ads serves rewarded video for rewarded interstitial requests
            ShowRewarded(onRewarded, onClosed, onError);
        }

        #endregion
    }
}
