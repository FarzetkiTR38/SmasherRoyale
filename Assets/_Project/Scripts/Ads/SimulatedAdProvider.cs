using System;
using UnityEngine;
using RoyalSmash.UI;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Mock/Simulated implementation of IAdProvider for Unity Editor, automated testing, development builds,
    /// and graceful offline fallback when no external ad networks are available.
    /// Follows Master Build Prompt Section 15.
    /// </summary>
    public class SimulatedAdProvider : IAdProvider
    {
        private AdConfig _config;
        private SimulatedAdDialog _dialog;

        public AdNetworkType NetworkType => AdNetworkType.Simulated;

        // Test hooks
        public bool SimulateLoadFailureNext { get; set; } = false;
        public bool SimulateUserCancelNext { get; set; } = false;
        public bool InstantCompleteInTests { get; set; } = false;
        public bool ForceNotReady { get; set; } = false;
        public bool SimulateDoubleRewardCallback { get; set; } = false;
        public bool IsBannerVisible { get; private set; } = false;
        public BannerPosition CurrentBannerPosition { get; private set; } = BannerPosition.BottomCenter;

        public void Initialize(AdConfig config, Action onInitialized = null)
        {
            _config = config;
            Debug.Log("[SimulatedAdProvider] Initialized in Simulation Mode.");
            onInitialized?.Invoke();
        }

        public void SetDialog(SimulatedAdDialog dialog)
        {
            _dialog = dialog;
        }

        public bool IsInterstitialReady()
        {
            return !ForceNotReady;
        }

        public bool IsRewardedReady()
        {
            return !ForceNotReady;
        }

        public bool IsBannerReady()
        {
            return !ForceNotReady;
        }

        public bool IsRewardedInterstitialReady()
        {
            return !ForceNotReady;
        }

        public void ShowInterstitial(Action onClosed, Action<string> onError = null)
        {
            if (SimulateLoadFailureNext)
            {
                SimulateLoadFailureNext = false;
                Debug.LogWarning("[SimulatedAdProvider] Interstitial load failed (Simulated).");
                onError?.Invoke("Simulated Interstitial Failure");
                return;
            }

            Debug.Log("[SimulatedAdProvider] Displaying Simulated Interstitial Ad.");

            if (InstantCompleteInTests || _dialog == null)
            {
                onClosed?.Invoke();
                return;
            }

            float duration = _config != null ? _config.SimulatedAdDuration : 1.5f;
            _dialog.ShowSimulatedAd(
                isRewarded: false,
                duration: duration,
                onCompleted: () => onClosed?.Invoke(),
                onCancelled: () => onClosed?.Invoke(),
                onError: (err) => onError?.Invoke(err)
            );
        }

        public void ShowRewarded(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (SimulateLoadFailureNext)
            {
                SimulateLoadFailureNext = false;
                Debug.LogWarning("[SimulatedAdProvider] Rewarded ad load failed (Simulated).");
                onError?.Invoke("Simulated Rewarded Ad Failure");
                return;
            }

            if (SimulateUserCancelNext)
            {
                SimulateUserCancelNext = false;
                Debug.Log("[SimulatedAdProvider] User cancelled rewarded ad before completion (Simulated).");
                onClosed?.Invoke();
                return;
            }

            Debug.Log("[SimulatedAdProvider] Displaying Simulated Rewarded Ad.");

            if (InstantCompleteInTests || _dialog == null)
            {
                onRewarded?.Invoke();
                if (SimulateDoubleRewardCallback)
                {
                    SimulateDoubleRewardCallback = false;
                    onRewarded?.Invoke();
                }
                onClosed?.Invoke();
                return;
            }

            float duration = _config != null ? _config.SimulatedAdDuration : 1.5f;
            _dialog.ShowSimulatedAd(
                isRewarded: true,
                duration: duration,
                onCompleted: () =>
                {
                    onRewarded?.Invoke();
                    onClosed?.Invoke();
                },
                onCancelled: () => onClosed?.Invoke(),
                onError: (err) => onError?.Invoke(err)
            );
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter, Action onLoaded = null, Action<string> onError = null)
        {
            if (SimulateLoadFailureNext)
            {
                SimulateLoadFailureNext = false;
                Debug.LogWarning("[SimulatedAdProvider] Banner load failed (Simulated).");
                onError?.Invoke("Simulated Banner Load Failure");
                return;
            }

            IsBannerVisible = true;
            CurrentBannerPosition = position;
            Debug.Log($"[SimulatedAdProvider] Displaying Simulated Banner at {position}.");
            onLoaded?.Invoke();
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
            Debug.Log("[SimulatedAdProvider] Hidden Simulated Banner.");
        }

        public void DestroyBanner()
        {
            IsBannerVisible = false;
            Debug.Log("[SimulatedAdProvider] Destroyed Simulated Banner.");
        }

        public void ShowRewardedInterstitial(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (SimulateLoadFailureNext)
            {
                SimulateLoadFailureNext = false;
                Debug.LogWarning("[SimulatedAdProvider] Rewarded Interstitial load failed (Simulated).");
                onError?.Invoke("Simulated Rewarded Interstitial Failure");
                return;
            }

            Debug.Log("[SimulatedAdProvider] Displaying Simulated Rewarded Interstitial Ad.");

            if (InstantCompleteInTests || _dialog == null)
            {
                onRewarded?.Invoke();
                onClosed?.Invoke();
                return;
            }

            float duration = _config != null ? _config.SimulatedAdDuration : 1.5f;
            _dialog.ShowSimulatedAd(
                isRewarded: true,
                duration: duration,
                onCompleted: () =>
                {
                    onRewarded?.Invoke();
                    onClosed?.Invoke();
                },
                onCancelled: () => onClosed?.Invoke(),
                onError: (err) => onError?.Invoke(err)
            );
        }
    }
}
