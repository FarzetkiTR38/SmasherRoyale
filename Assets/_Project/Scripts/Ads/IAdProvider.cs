using System;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Common interface for ad networks (Mock/Simulated, Google Mobile Ads, Unity Ads, etc.).
    /// </summary>
    public interface IAdProvider
    {
        /// <summary>
        /// Initializes the ad network provider with configuration.
        /// </summary>
        void Initialize(AdConfig config, Action onInitialized = null);

        /// <summary>
        /// Returns whether an interstitial ad is loaded and ready to display.
        /// </summary>
        bool IsInterstitialReady();

        /// <summary>
        /// Returns whether a rewarded ad is loaded and ready to display.
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
    }
}
