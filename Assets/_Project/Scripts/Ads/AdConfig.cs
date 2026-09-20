using UnityEngine;

namespace RoyalSmash.Ads
{
    /// <summary>
    /// Configuration data for in-game advertisements, intervals, and provider unit IDs.
    /// Follows Master Build Prompt Section 15 & Section 35 (Phase 14).
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "RoyalSmash/Config/AdConfig")]
    public class AdConfig : ScriptableObject
    {
        [Header("Simulation & Test Settings")]
        [Tooltip("When true, simulated ads are played without requiring external SDK initialization.")]
        [SerializeField] private bool _enableSimulationMode = true;
        [Tooltip("Playback duration in seconds for simulated ad dialog.")]
        [SerializeField] private float _simulatedAdDuration = 1.5f;

        [Header("Interstitial Rules")]
        [Tooltip("Shows an interstitial ad every N level completions (default 5).")]
        [SerializeField] private int _interstitialLevelInterval = 5;
        [Tooltip("Shows an interstitial ad when player loses all balls in a level.")]
        [SerializeField] private bool _showInterstitialOnLose = true;
        [Tooltip("Minimum cooldown seconds between consecutive interstitials.")]
        [SerializeField] private float _interstitialCooldownSeconds = 20f;

        [Header("AdMob Android Credentials (Placeholders / Test IDs)")]
        [SerializeField] private string _adMobAppIdAndroid = "ca-app-pub-3940256099942544~3347511713";
        [SerializeField] private string _interstitialAdUnitIdAndroid = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _rewardedAdUnitIdAndroid = "ca-app-pub-3940256099942544/5224354917";

        [Header("AdMob iOS Credentials (Placeholders / Test IDs)")]
        [SerializeField] private string _adMobAppIdIOS = "ca-app-pub-3940256099942544~1458002511";
        [SerializeField] private string _interstitialAdUnitIdIOS = "ca-app-pub-3940256099942544/4411468910";
        [SerializeField] private string _rewardedAdUnitIdIOS = "ca-app-pub-3940256099942544/1712485313";

        // Public Accessors
        public bool EnableSimulationMode { get => _enableSimulationMode; set => _enableSimulationMode = value; }
        public float SimulatedAdDuration => _simulatedAdDuration;
        public int InterstitialLevelInterval => _interstitialLevelInterval;
        public bool ShowInterstitialOnLose => _showInterstitialOnLose;
        public float InterstitialCooldownSeconds => _interstitialCooldownSeconds;

        public string AdMobAppIdAndroid => _adMobAppIdAndroid;
        public string InterstitialAdUnitIdAndroid => _interstitialAdUnitIdAndroid;
        public string RewardedAdUnitIdAndroid => _rewardedAdUnitIdAndroid;

        public string AdMobAppIdIOS => _adMobAppIdIOS;
        public string InterstitialAdUnitIdIOS => _interstitialAdUnitIdIOS;
        public string RewardedAdUnitIdIOS => _rewardedAdUnitIdIOS;
    }
}
