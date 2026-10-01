using UnityEngine;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Configuration data for Hybrid in-game advertisements (AdMob + Unity Ads + Fallback).
    /// Manages mediation waterfall strategies, provider unit IDs, banner placements, and intervals.
    /// Follows Master Build Prompt Section 15 & Section 35.
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "RoyalSmash/Config/AdConfig")]
    public class AdConfig : ScriptableObject
    {
        [Header("Mediation & Waterfall Strategy")]
        [Tooltip("Defines which ad network is queried first before falling over to secondary networks.")]
        [SerializeField] private WaterfallStrategy _waterfallStrategy = WaterfallStrategy.AdMobFirst_UnityAdsFallback;
        [Tooltip("When true, simulated ads run without requiring external SDK connection.")]
        [SerializeField] private bool _enableSimulationMode = false;
        [Tooltip("Playback duration in seconds for simulated ad dialog.")]
        [SerializeField] private float _simulatedAdDuration = 1.5f;

        [Header("Interstitial & Banner Rules")]
        [Tooltip("Shows an interstitial ad every N level completions (default 5).")]
        [SerializeField] private int _interstitialLevelInterval = 5;
        [Tooltip("Shows an interstitial ad when player loses all balls in a level.")]
        [SerializeField] private bool _showInterstitialOnLose = true;
        [Tooltip("Minimum cooldown seconds between consecutive interstitials.")]
        [SerializeField] private float _interstitialCooldownSeconds = 20f;
        [Tooltip("Default screen anchor position for banner ads.")]
        [SerializeField] private BannerPosition _bannerPosition = BannerPosition.BottomCenter;
        [Tooltip("When true, automatically loads and shows bottom banner in Main Menu (if player doesn't own No Ads).")]
        [SerializeField] private bool _autoShowBannerInMainMenu = true;

        [Header("Google Mobile Ads (AdMob) Android Credentials")]
        [SerializeField] private string _adMobAppIdAndroid = "ca-app-pub-3581074606829094~9819287835";
        [SerializeField] private string _bannerAdUnitIdAndroid = "ca-app-pub-3581074606829094/5880042827";
        [SerializeField] private string _interstitialAdUnitIdAndroid = "ca-app-pub-3581074606829094/8701733002";
        [SerializeField] private string _rewardedInterstitialAdUnitIdAndroid = "ca-app-pub-3581074606829094/2731135308";
        [SerializeField] private string _rewardedAdUnitIdAndroid = "ca-app-pub-3581074606829094/8510161316";

        [Header("Google Mobile Ads (AdMob) iOS Credentials")]
        [SerializeField] private string _adMobAppIdIOS = "ca-app-pub-3581074606829094~1458002511";
        [SerializeField] private string _bannerAdUnitIdIOS = "ca-app-pub-3581074606829094/5880042827";
        [SerializeField] private string _interstitialAdUnitIdIOS = "ca-app-pub-3581074606829094/8701733002";
        [SerializeField] private string _rewardedInterstitialAdUnitIdIOS = "ca-app-pub-3581074606829094/2731135308";
        [SerializeField] private string _rewardedAdUnitIdIOS = "ca-app-pub-3581074606829094/8510161316";

        [Header("Unity Ads Credentials")]
        [SerializeField] private string _unityAdsGameIdAndroid = "5728491";
        [SerializeField] private string _unityAdsGameIdIOS = "5728490";
        [SerializeField] private string _unityAdsBannerPlacement = "Banner_Android";
        [SerializeField] private string _unityAdsInterstitialPlacement = "Interstitial_Android";
        [SerializeField] private string _unityAdsRewardedPlacement = "Rewarded_Android";
        [SerializeField] private bool _unityAdsTestMode = false;

        // Public Accessors
        public WaterfallStrategy WaterfallStrategy { get => _waterfallStrategy; set => _waterfallStrategy = value; }
        public bool EnableSimulationMode { get => _enableSimulationMode; set => _enableSimulationMode = value; }
        public float SimulatedAdDuration => _simulatedAdDuration;
        public int InterstitialLevelInterval => _interstitialLevelInterval;
        public bool ShowInterstitialOnLose => _showInterstitialOnLose;
        public float InterstitialCooldownSeconds => _interstitialCooldownSeconds;
        public BannerPosition BannerPosition => _bannerPosition;
        public bool AutoShowBannerInMainMenu => _autoShowBannerInMainMenu;

        // AdMob Android
        public string AdMobAppIdAndroid { get => _adMobAppIdAndroid; set => _adMobAppIdAndroid = value; }
        public string BannerAdUnitIdAndroid { get => _bannerAdUnitIdAndroid; set => _bannerAdUnitIdAndroid = value; }
        public string InterstitialAdUnitIdAndroid { get => _interstitialAdUnitIdAndroid; set => _interstitialAdUnitIdAndroid = value; }
        public string RewardedInterstitialAdUnitIdAndroid { get => _rewardedInterstitialAdUnitIdAndroid; set => _rewardedInterstitialAdUnitIdAndroid = value; }
        public string RewardedAdUnitIdAndroid { get => _rewardedAdUnitIdAndroid; set => _rewardedAdUnitIdAndroid = value; }

        // AdMob iOS
        public string AdMobAppIdIOS { get => _adMobAppIdIOS; set => _adMobAppIdIOS = value; }
        public string BannerAdUnitIdIOS { get => _bannerAdUnitIdIOS; set => _bannerAdUnitIdIOS = value; }
        public string InterstitialAdUnitIdIOS { get => _interstitialAdUnitIdIOS; set => _interstitialAdUnitIdIOS = value; }
        public string RewardedInterstitialAdUnitIdIOS { get => _rewardedInterstitialAdUnitIdIOS; set => _rewardedInterstitialAdUnitIdIOS = value; }
        public string RewardedAdUnitIdIOS { get => _rewardedAdUnitIdIOS; set => _rewardedAdUnitIdIOS = value; }

        // Unity Ads
        public string UnityAdsGameIdAndroid { get => _unityAdsGameIdAndroid; set => _unityAdsGameIdAndroid = value; }
        public string UnityAdsGameIdIOS { get => _unityAdsGameIdIOS; set => _unityAdsGameIdIOS = value; }
        public string UnityAdsBannerPlacement { get => _unityAdsBannerPlacement; set => _unityAdsBannerPlacement = value; }
        public string UnityAdsInterstitialPlacement { get => _unityAdsInterstitialPlacement; set => _unityAdsInterstitialPlacement = value; }
        public string UnityAdsRewardedPlacement { get => _unityAdsRewardedPlacement; set => _unityAdsRewardedPlacement = value; }
        public bool UnityAdsTestMode { get => _unityAdsTestMode; set => _unityAdsTestMode = value; }
    }
}
