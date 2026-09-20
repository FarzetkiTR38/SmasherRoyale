using System;
using System.Collections;
using UnityEngine;
using RoyalSmash.UI;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Mock/Simulated implementation of IAdProvider for Unity Editor, automated testing, and development builds.
    /// Follows Master Build Prompt Section 15.
    /// </summary>
    public class SimulatedAdProvider : IAdProvider
    {
        private AdConfig _config;
        private SimulatedAdDialog _dialog;

        // Test hooks
        public bool SimulateLoadFailureNext { get; set; } = false;
        public bool SimulateUserCancelNext { get; set; } = false;
        public bool InstantCompleteInTests { get; set; } = false;

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

        public bool ForceNotReady { get; set; } = false;
        public bool SimulateDoubleRewardCallback { get; set; } = false;

        public bool IsInterstitialReady()
        {
            return !ForceNotReady;
        }

        public bool IsRewardedReady()
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
                    onRewarded?.Invoke(); // Duplicate call to test idempotency
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
                onCancelled: () =>
                {
                    onClosed?.Invoke();
                },
                onError: (err) => onError?.Invoke(err)
            );
        }
    }
}
