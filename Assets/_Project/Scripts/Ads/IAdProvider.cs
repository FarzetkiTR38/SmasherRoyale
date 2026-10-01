using System;

namespace RoyalSmash.Ads
{
    public enum AdNetworkType
    {
        None = 0,
        AdMob = 1,
        UnityAds = 2,
        Simulated = 3,
        HybridMediation = 4
    }

    public enum WaterfallStrategy
    {
        AdMobFirst_UnityAdsFallback = 0,
        UnityAdsFirst_AdMobFallback = 1,
        AdaptiveOptimization = 2,
        SimulationOnly = 3
    }

    public enum BannerPosition
    {
        TopCenter = 0,
        BottomCenter = 1
    }

    /// <summary>
    /// Unified interface for ad networks (AdMob, Unity Ads, Simulated, Hybrid Mediation).
    /// Supports Banners, Interstitials, Rewarded Videos, and Rewarded Interstitials.
    /// Follows Master Build Prompt Section 15 & casual mobile mediation standards.
    /// </summary>
    public interface IAdProvider
    {
        /// <summary>
        /// Identifies the network type of this provider.
        /// </summary>
        AdNetworkType NetworkType { get; }

        /// <summary>
        /// Initializes the ad network provider with configuration.
        /// </summary>
        void Initialize(AdConfig config, Action onInitialized = null);

        /// <summary>
        /// Returns whether an interstitial ad is loaded and ready to display.
        /// </summary>
        bool IsInterstitialReady();

        /// <summary>
        /// Returns whether a rewarded video ad is loaded and ready to display.
        /// </summary>
        bool IsRewardedReady();

        /// <summary>
        /// Requests to show a full-screen interstitial ad.
        /// </summary>
        void ShowInterstitial(Action onClosed, Action<string> onError = null);

        /// <summary>
        /// Requests to show a rewarded video ad.
        /// onRewarded is ONLY invoked upon full, successful view.
        /// </summary>
        void ShowRewarded(Action onRewarded, Action onClosed = null, Action<string> onError = null);

        /// <summary>
        /// Returns whether a banner ad is loaded and ready to display.
        /// </summary>
        bool IsBannerReady();

        /// <summary>
        /// Requests to display a banner ad at the specified screen position.
        /// </summary>
        void ShowBanner(BannerPosition position = BannerPosition.BottomCenter, Action onLoaded = null, Action<string> onError = null);

        /// <summary>
        /// Hides the active banner without destroying it.
        /// </summary>
        void HideBanner();

        /// <summary>
        /// Destroys and cleans up active banner resources.
        /// </summary>
        void DestroyBanner();

        /// <summary>
        /// Returns whether a rewarded interstitial ad is loaded and ready to display.
        /// </summary>
        bool IsRewardedInterstitialReady();

        /// <summary>
        /// Requests to show a rewarded interstitial ad.
        /// </summary>
        void ShowRewardedInterstitial(Action onRewarded, Action onClosed = null, Action<string> onError = null);
    }
}
