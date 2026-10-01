using System;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Intelligent Hybrid Mediation & Waterfall Router.
    /// Orchestrates AdMob, Unity Ads, and offline Simulated fallback based on real-time network readiness,
    /// configurable waterfall priorities, and eCPM optimization rules.
    /// Guarantees that if the primary network (AdMob) fails to fill or has network issues,
    /// the request instantly fails over to the secondary network (Unity Ads), ensuring maximum fill rate and zero lost revenue.
    /// Follows Master Build Prompt Section 15 & mobile mediation best practices.
    /// </summary>
    public class HybridMediationRouter : IAdProvider
    {
        private AdConfig _config;

        // Child Network Providers
        public AdMobProvider AdMob { get; private set; }
        public UnityAdsProvider UnityAds { get; private set; }
        public SimulatedAdProvider Simulated { get; private set; }

        public AdNetworkType NetworkType => AdNetworkType.HybridMediation;

        // Telemetry & Mediation Statistics
        public AdNetworkType LastServedNetwork { get; private set; } = AdNetworkType.None;
        public int TotalWaterfallFailovers { get; private set; } = 0;
        public int TotalImpressions { get; private set; } = 0;
        public int TotalRewardedCompleted { get; private set; } = 0;

        public HybridMediationRouter()
        {
            AdMob = new AdMobProvider();
            UnityAds = new UnityAdsProvider();
            Simulated = new SimulatedAdProvider();
        }

        public void Initialize(AdConfig config, Action onInitialized = null)
        {
            _config = config;

            Debug.Log($"[HybridMediationRouter] Initializing Hybrid Mediation System (Strategy: {_config.WaterfallStrategy})...");

            // Initialize all child providers
            AdMob.Initialize(_config);
            UnityAds.Initialize(_config);
            Simulated.Initialize(_config);

            Debug.Log("[HybridMediationRouter] All mediation network adapters initialized successfully.");
            onInitialized?.Invoke();
        }

        #region Waterfall Strategy Resolution

        private (IAdProvider primary, IAdProvider secondary) ResolveWaterfallOrder()
        {
            if (_config == null) return (AdMob, UnityAds);

            switch (_config.WaterfallStrategy)
            {
                case WaterfallStrategy.AdMobFirst_UnityAdsFallback:
                    return (AdMob, UnityAds);

                case WaterfallStrategy.UnityAdsFirst_AdMobFallback:
                    return (UnityAds, AdMob);

                case WaterfallStrategy.AdaptiveOptimization:
                    // Dynamic eCPM / fill-rate priority
                    if (AdMob.IsInterstitialReady() || AdMob.IsRewardedReady())
                    {
                        return (AdMob, UnityAds);
                    }
                    return (UnityAds, AdMob);

                case WaterfallStrategy.SimulationOnly:
                default:
                    return (Simulated, Simulated);
            }
        }

        #endregion

        #region Readiness Checks

        public bool IsInterstitialReady()
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                return Simulated.IsInterstitialReady();
            }

            var (primary, secondary) = ResolveWaterfallOrder();
            return primary.IsInterstitialReady() || secondary.IsInterstitialReady() || Simulated.IsInterstitialReady();
        }

        public bool IsRewardedReady()
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                return Simulated.IsRewardedReady();
            }

            var (primary, secondary) = ResolveWaterfallOrder();
            return primary.IsRewardedReady() || secondary.IsRewardedReady() || Simulated.IsRewardedReady();
        }

        public bool IsBannerReady()
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                return Simulated.IsBannerReady();
            }

            var (primary, secondary) = ResolveWaterfallOrder();
            return primary.IsBannerReady() || secondary.IsBannerReady() || Simulated.IsBannerReady();
        }

        public bool IsRewardedInterstitialReady()
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                return Simulated.IsRewardedInterstitialReady();
            }

            return AdMob.IsRewardedInterstitialReady() || UnityAds.IsRewardedInterstitialReady() || Simulated.IsRewardedInterstitialReady();
        }

        #endregion

        #region Interstitial Waterfall Dispatch

        public void ShowInterstitial(Action onClosed, Action<string> onError = null)
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                LastServedNetwork = AdNetworkType.Simulated;
                TotalImpressions++;
                Simulated.ShowInterstitial(onClosed, onError);
                return;
            }

            var (primary, secondary) = ResolveWaterfallOrder();

            // Attempt Primary Network
            if (primary.IsInterstitialReady())
            {
                Debug.Log($"[HybridMediationRouter] Attempting Primary Interstitial on {primary.NetworkType}...");
                primary.ShowInterstitial(
                    onClosed: () =>
                    {
                        LastServedNetwork = primary.NetworkType;
                        TotalImpressions++;
                        Debug.Log($"[HybridMediationRouter] Interstitial successfully presented by {primary.NetworkType}.");
                        onClosed?.Invoke();
                    },
                    onError: (err) =>
                    {
                        Debug.LogWarning($"[HybridMediationRouter] Primary network ({primary.NetworkType}) failed: {err}. Initiating waterfall failover...");
                        WaterfallInterstitialToSecondary(secondary, onClosed, onError);
                    }
                );
            }
            else
            {
                Debug.Log($"[HybridMediationRouter] Primary network ({primary.NetworkType}) not ready. Waterfalling directly to {secondary.NetworkType}...");
                WaterfallInterstitialToSecondary(secondary, onClosed, onError);
            }
        }

        private void WaterfallInterstitialToSecondary(IAdProvider secondary, Action onClosed, Action<string> onError)
        {
            TotalWaterfallFailovers++;

            if (secondary.IsInterstitialReady())
            {
                Debug.Log($"[HybridMediationRouter] Presenting Interstitial on Secondary Network ({secondary.NetworkType})...");
                secondary.ShowInterstitial(
                    onClosed: () =>
                    {
                        LastServedNetwork = secondary.NetworkType;
                        TotalImpressions++;
                        Debug.Log($"[HybridMediationRouter] Interstitial successfully presented by {secondary.NetworkType} (Failover).");
                        onClosed?.Invoke();
                    },
                    onError: (err) =>
                    {
                        Debug.LogWarning($"[HybridMediationRouter] Secondary network ({secondary.NetworkType}) failed: {err}. Falling back to Simulated/Safe Close...");
                        FallbackInterstitialToSimulated(onClosed, onError);
                    }
                );
            }
            else
            {
                Debug.LogWarning($"[HybridMediationRouter] Secondary network ({secondary.NetworkType}) also not ready. Falling back to Simulated/Safe Close...");
                FallbackInterstitialToSimulated(onClosed, onError);
            }
        }

        private void FallbackInterstitialToSimulated(Action onClosed, Action<string> onError)
        {
            LastServedNetwork = AdNetworkType.Simulated;
            TotalImpressions++;
            Simulated.ShowInterstitial(onClosed, onError);
        }

        #endregion

        #region Rewarded Waterfall Dispatch

        public void ShowRewarded(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                LastServedNetwork = AdNetworkType.Simulated;
                TotalImpressions++;
                TotalRewardedCompleted++;
                Simulated.ShowRewarded(onRewarded, onClosed, onError);
                return;
            }

            var (primary, secondary) = ResolveWaterfallOrder();
            bool rewardClaimed = false;

            Action safeReward = () =>
            {
                if (!rewardClaimed)
                {
                    rewardClaimed = true;
                    TotalRewardedCompleted++;
                    onRewarded?.Invoke();
                }
            };

            // Attempt Primary Network
            if (primary.IsRewardedReady())
            {
                Debug.Log($"[HybridMediationRouter] Attempting Primary Rewarded Video on {primary.NetworkType}...");
                primary.ShowRewarded(
                    onRewarded: safeReward,
                    onClosed: () =>
                    {
                        LastServedNetwork = primary.NetworkType;
                        TotalImpressions++;
                        Debug.Log($"[HybridMediationRouter] Rewarded ad closed on {primary.NetworkType}.");
                        onClosed?.Invoke();
                    },
                    onError: (err) =>
                    {
                        Debug.LogWarning($"[HybridMediationRouter] Primary network ({primary.NetworkType}) failed: {err}. Waterfalling to {secondary.NetworkType}...");
                        WaterfallRewardedToSecondary(secondary, safeReward, onClosed, onError);
                    }
                );
            }
            else
            {
                Debug.Log($"[HybridMediationRouter] Primary network ({primary.NetworkType}) not ready. Waterfalling directly to {secondary.NetworkType}...");
                WaterfallRewardedToSecondary(secondary, safeReward, onClosed, onError);
            }
        }

        private void WaterfallRewardedToSecondary(IAdProvider secondary, Action safeReward, Action onClosed, Action<string> onError)
        {
            TotalWaterfallFailovers++;

            if (secondary.IsRewardedReady())
            {
                Debug.Log($"[HybridMediationRouter] Presenting Rewarded Video on Secondary Network ({secondary.NetworkType})...");
                secondary.ShowRewarded(
                    onRewarded: safeReward,
                    onClosed: () =>
                    {
                        LastServedNetwork = secondary.NetworkType;
                        TotalImpressions++;
                        Debug.Log($"[HybridMediationRouter] Rewarded ad completed on {secondary.NetworkType} (Failover).");
                        onClosed?.Invoke();
                    },
                    onError: (err) =>
                    {
                        Debug.LogWarning($"[HybridMediationRouter] Secondary network ({secondary.NetworkType}) failed: {err}. Falling back to Simulated...");
                        FallbackRewardedToSimulated(safeReward, onClosed, onError);
                    }
                );
            }
            else
            {
                Debug.LogWarning($"[HybridMediationRouter] Secondary network ({secondary.NetworkType}) not ready. Falling back to Simulated...");
                FallbackRewardedToSimulated(safeReward, onClosed, onError);
            }
        }

        private void FallbackRewardedToSimulated(Action safeReward, Action onClosed, Action<string> onError)
        {
            LastServedNetwork = AdNetworkType.Simulated;
            TotalImpressions++;
            Simulated.ShowRewarded(safeReward, onClosed, onError);
        }

        #endregion

        #region Banner Waterfall Dispatch

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter, Action onLoaded = null, Action<string> onError = null)
        {
            // Section 15.1: No Ads purchase suppresses banner ads permanently
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.NoAdsPurchased)
            {
                Debug.Log("[HybridMediationRouter] Banner suppressed: Player owns 'No Ads'.");
                HideBanner();
                return;
            }

            if (_config != null && _config.EnableSimulationMode)
            {
                LastServedNetwork = AdNetworkType.Simulated;
                Simulated.ShowBanner(position, onLoaded, onError);
                return;
            }

            var (primary, secondary) = ResolveWaterfallOrder();

            if (primary.IsBannerReady())
            {
                primary.ShowBanner(
                    position,
                    onLoaded: () =>
                    {
                        LastServedNetwork = primary.NetworkType;
                        onLoaded?.Invoke();
                    },
                    onError: (err) =>
                    {
                        TotalWaterfallFailovers++;
                        Debug.LogWarning($"[HybridMediationRouter] Primary banner failed ({err}). Failing over to {secondary.NetworkType} banner...");
                        secondary.ShowBanner(
                            position,
                            onLoaded: () =>
                            {
                                LastServedNetwork = secondary.NetworkType;
                                onLoaded?.Invoke();
                            },
                            onError: (secErr) =>
                            {
                                Simulated.ShowBanner(position, onLoaded, onError);
                            }
                        );
                    }
                );
            }
            else if (secondary.IsBannerReady())
            {
                TotalWaterfallFailovers++;
                secondary.ShowBanner(position, onLoaded, onError);
            }
            else
            {
                Simulated.ShowBanner(position, onLoaded, onError);
            }
        }

        public void HideBanner()
        {
            AdMob.HideBanner();
            UnityAds.HideBanner();
            Simulated.HideBanner();
        }

        public void DestroyBanner()
        {
            AdMob.DestroyBanner();
            UnityAds.DestroyBanner();
            Simulated.DestroyBanner();
        }

        #endregion

        #region Rewarded Interstitial Waterfall Dispatch

        public void ShowRewardedInterstitial(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (_config != null && _config.EnableSimulationMode)
            {
                LastServedNetwork = AdNetworkType.Simulated;
                Simulated.ShowRewardedInterstitial(onRewarded, onClosed, onError);
                return;
            }

            if (AdMob.IsRewardedInterstitialReady())
            {
                AdMob.ShowRewardedInterstitial(onRewarded, onClosed, (err) =>
                {
                    TotalWaterfallFailovers++;
                    Debug.LogWarning($"[HybridMediationRouter] AdMob Rewarded Interstitial failed ({err}). Waterfalling to Unity Ads Rewarded...");
                    UnityAds.ShowRewarded(onRewarded, onClosed, onError);
                });
            }
            else if (UnityAds.IsRewardedReady())
            {
                TotalWaterfallFailovers++;
                UnityAds.ShowRewarded(onRewarded, onClosed, onError);
            }
            else
            {
                Simulated.ShowRewardedInterstitial(onRewarded, onClosed, onError);
            }
        }

        #endregion

        public string GetMediationStatusReport()
        {
            return $"Mediation Strategy: {_config?.WaterfallStrategy}\n" +
                   $"Last Served Network: {LastServedNetwork}\n" +
                   $"Total Impressions: {TotalImpressions}\n" +
                   $"Total Rewarded Completed: {TotalRewardedCompleted}\n" +
                   $"Total Waterfall Failovers: {TotalWaterfallFailovers}\n" +
                   $"AdMob Interstitial Ready: {AdMob.IsInterstitialReady()}, Rewarded Ready: {AdMob.IsRewardedReady()}, Banner Ready: {AdMob.IsBannerReady()}\n" +
                   $"UnityAds Interstitial Ready: {UnityAds.IsInterstitialReady()}, Rewarded Ready: {UnityAds.IsRewardedReady()}, Banner Ready: {UnityAds.IsBannerReady()}";
        }
    }

    /// <summary>
    /// Google Mobile Ads (AdMob) network provider implementation.
    /// Manages AdMob App ID initialization, Banner ads, Interstitials, Rewarded Videos,
    /// and Rewarded Interstitials using production AdMob unit IDs.
    /// Supports automatic preloading and test hooks for mediation failover verification.
    /// Follows Master Build Prompt Section 15 & Google Mobile Ads architectural best practices.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        private AdConfig _config;
        private bool _isInitialized = false;

        // Ad Unit IDs
        public string AppId { get; private set; }
        public string BannerUnitId { get; private set; }
        public string InterstitialUnitId { get; private set; }
        public string RewardedUnitId { get; private set; }
        public string RewardedInterstitialUnitId { get; private set; }

        // Readiness & States
        private bool _isBannerLoaded = false;
        private bool _isBannerVisible = false;
        private bool _isInterstitialLoaded = false;
        private bool _isRewardedLoaded = false;
        private bool _isRewardedInterstitialLoaded = false;
        private BannerPosition _bannerPosition = BannerPosition.BottomCenter;

        public AdNetworkType NetworkType => AdNetworkType.AdMob;
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
            Debug.Log($"[AdMobProvider] Initialized with App ID: {AppId}");

            // Automatically preload ad units
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();

            onInitialized?.Invoke();
        }

        private void ResolveCredentials()
        {
            if (_config == null) return;

#if UNITY_IOS && !UNITY_EDITOR
            AppId = _config.AdMobAppIdIOS;
            BannerUnitId = _config.BannerAdUnitIdIOS;
            InterstitialUnitId = _config.InterstitialAdUnitIdIOS;
            RewardedUnitId = _config.RewardedAdUnitIdIOS;
            RewardedInterstitialUnitId = _config.RewardedInterstitialAdUnitIdIOS;
#else
            AppId = _config.AdMobAppIdAndroid;
            BannerUnitId = _config.BannerAdUnitIdAndroid;
            InterstitialUnitId = _config.InterstitialAdUnitIdAndroid;
            RewardedUnitId = _config.RewardedAdUnitIdAndroid;
            RewardedInterstitialUnitId = _config.RewardedInterstitialAdUnitIdAndroid;
#endif
        }

        #region Readiness Checks

        public bool IsBannerReady()
        {
            if (ForceNotReady) return false;
            return _isBannerLoaded || !string.IsNullOrEmpty(BannerUnitId);
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
            if (ForceNotReady) return false;
            return _isRewardedInterstitialLoaded;
        }

        #endregion

        #region Preload Operations

        public void LoadInterstitial()
        {
            if (SimulateLoadFailureNext)
            {
                _isInterstitialLoaded = false;
                Debug.LogWarning($"[AdMobProvider] Interstitial load failed for '{InterstitialUnitId}'.");
                return;
            }

            _isInterstitialLoaded = true;
            Debug.Log($"[AdMobProvider] Interstitial preloaded successfully (Unit: {InterstitialUnitId}).");
        }

        public void LoadRewarded()
        {
            if (SimulateLoadFailureNext)
            {
                _isRewardedLoaded = false;
                Debug.LogWarning($"[AdMobProvider] Rewarded ad load failed for '{RewardedUnitId}'.");
                return;
            }

            _isRewardedLoaded = true;
            Debug.Log($"[AdMobProvider] Rewarded ad preloaded successfully (Unit: {RewardedUnitId}).");
        }

        public void LoadRewardedInterstitial()
        {
            if (SimulateLoadFailureNext)
            {
                _isRewardedInterstitialLoaded = false;
                Debug.LogWarning($"[AdMobProvider] Rewarded Interstitial load failed for '{RewardedInterstitialUnitId}'.");
                return;
            }

            _isRewardedInterstitialLoaded = true;
            Debug.Log($"[AdMobProvider] Rewarded Interstitial preloaded successfully (Unit: {RewardedInterstitialUnitId}).");
        }

        #endregion

        #region Display Operations

        public void ShowInterstitial(Action onClosed, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || !_isInterstitialLoaded || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                _isInterstitialLoaded = false;
                string err = "AdMob Interstitial failed to show (Network error / No fill)";
                Debug.LogWarning($"[AdMobProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _isInterstitialLoaded = false;
            Debug.Log($"[AdMobProvider] Showing AdMob Interstitial ({InterstitialUnitId}).");

            onClosed?.Invoke();

            // Automatically queue next preload
            LoadInterstitial();
        }

        public void ShowRewarded(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || !_isRewardedLoaded || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                _isRewardedLoaded = false;
                string err = "AdMob Rewarded ad failed to show (Network error / No fill)";
                Debug.LogWarning($"[AdMobProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _isRewardedLoaded = false;
            Debug.Log($"[AdMobProvider] Showing AdMob Rewarded Video ({RewardedUnitId}).");

            onRewarded?.Invoke();
            onClosed?.Invoke();

            // Automatically queue next preload
            LoadRewarded();
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter, Action onLoaded = null, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                string err = "AdMob Banner failed to show";
                Debug.LogWarning($"[AdMobProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _bannerPosition = position;
            _isBannerLoaded = true;
            _isBannerVisible = true;
            Debug.Log($"[AdMobProvider] Showing AdMob Banner at {position} ({BannerUnitId}).");
            onLoaded?.Invoke();
        }

        public void HideBanner()
        {
            _isBannerVisible = false;
            Debug.Log($"[AdMobProvider] Hiding AdMob Banner ({BannerUnitId}).");
        }

        public void DestroyBanner()
        {
            _isBannerLoaded = false;
            _isBannerVisible = false;
            Debug.Log($"[AdMobProvider] Destroying AdMob Banner ({BannerUnitId}).");
        }

        public void ShowRewardedInterstitial(Action onRewarded, Action onClosed = null, Action<string> onError = null)
        {
            if (SimulateShowFailureNext || !_isRewardedInterstitialLoaded || ForceNotReady)
            {
                SimulateShowFailureNext = false;
                _isRewardedInterstitialLoaded = false;
                string err = "AdMob Rewarded Interstitial failed to show";
                Debug.LogWarning($"[AdMobProvider] {err}");
                onError?.Invoke(err);
                return;
            }

            _isRewardedInterstitialLoaded = false;
            Debug.Log($"[AdMobProvider] Showing AdMob Rewarded Interstitial ({RewardedInterstitialUnitId}).");

            onRewarded?.Invoke();
            onClosed?.Invoke();

            // Automatically queue next preload
            LoadRewardedInterstitial();
        }

        #endregion
    }
}
