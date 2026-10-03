using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Ads;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Master automation tool for configuring the Hybrid Ad System (AdMob + Unity Ads + Fallback).
    /// Serializes verified production AdMob IDs into AdConfig.asset, sets up mediation waterfall,
    /// and configures AdManager in the Main Menu scene.
    /// </summary>
    public static class HybridAdSystemSetup
    {
        public const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/AdConfig.asset";
        public const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";

        // Verified Production Credentials provided by User
        public const string ADMOB_APP_ID_ANDROID = "ca-app-pub-3581074606829094~9819287835";
        public const string ADMOB_BANNER_ID_ANDROID = "ca-app-pub-3581074606829094/5880042827";
        public const string ADMOB_INTERSTITIAL_ID_ANDROID = "ca-app-pub-3581074606829094/8701733002";
        public const string ADMOB_REWARDED_INTERSTITIAL_ID_ANDROID = "ca-app-pub-3581074606829094/2731135308";
        public const string ADMOB_REWARDED_ID_ANDROID = "ca-app-pub-3581074606829094/8510161316";

        [MenuItem("RoyalSmash/Ads/Configure Hybrid Ad System")]
        public static string SetupAll()
        {
            Debug.Log("=== [HybridAdSystemSetup] Configuring Hybrid Mediation Ad System ===");

            // 1. Configure AdConfig ScriptableObject
            AdConfig config = CreateOrUpdateAdConfig();

            // 2. Wire AdManager in MainMenu.unity
            string sceneResult = SetupMainMenuScene(config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"[HybridAdSystemSetup] SUCCESS: Configured AdMob + Unity Ads Hybrid System in {CONFIG_PATH}.\n{sceneResult}";
            Debug.Log(summary);
            return summary;
        }

        public static AdConfig CreateOrUpdateAdConfig()
        {
            string dir = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            AdConfig config = AssetDatabase.LoadAssetAtPath<AdConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AdConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            // Mediation & Waterfall settings
            config.WaterfallStrategy = WaterfallStrategy.AdMobFirst_UnityAdsFallback;
            config.EnableSimulationMode = false;

            // Production AdMob Credentials (Android)
            config.AdMobAppIdAndroid = ADMOB_APP_ID_ANDROID;
            config.BannerAdUnitIdAndroid = ADMOB_BANNER_ID_ANDROID;
            config.InterstitialAdUnitIdAndroid = ADMOB_INTERSTITIAL_ID_ANDROID;
            config.RewardedInterstitialAdUnitIdAndroid = ADMOB_REWARDED_INTERSTITIAL_ID_ANDROID;
            config.RewardedAdUnitIdAndroid = ADMOB_REWARDED_ID_ANDROID;

            // Production AdMob Credentials (iOS Fallback mapping)
            config.BannerAdUnitIdIOS = ADMOB_BANNER_ID_ANDROID;
            config.InterstitialAdUnitIdIOS = ADMOB_INTERSTITIAL_ID_ANDROID;
            config.RewardedInterstitialAdUnitIdIOS = ADMOB_REWARDED_INTERSTITIAL_ID_ANDROID;
            config.RewardedAdUnitIdIOS = ADMOB_REWARDED_ID_ANDROID;

            // Unity Ads Mediation Partner
            config.UnityAdsGameIdAndroid = "5728491";
            config.UnityAdsGameIdIOS = "5728490";
            config.UnityAdsBannerPlacement = "Banner_Android";
            config.UnityAdsInterstitialPlacement = "Interstitial_Android";
            config.UnityAdsRewardedPlacement = "Rewarded_Android";
            config.UnityAdsTestMode = false;

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"[HybridAdSystemSetup] AdConfig.asset updated with AdMob App ID '{ADMOB_APP_ID_ANDROID}' and all 4 ad unit IDs.");
            return config;
        }

        public static string SetupMainMenuScene(AdConfig config)
        {
            if (EditorSceneManager.GetActiveScene().path != MAIN_MENU_SCENE)
            {
                EditorSceneManager.OpenScene(MAIN_MENU_SCENE);
            }

            var adMgr = UnityEngine.Object.FindFirstObjectByType<AdManager>();
            if (adMgr == null)
            {
                var go = new GameObject("AdManager");
                adMgr = go.AddComponent<AdManager>();
                Debug.Log("[HybridAdSystemSetup] Created AdManager GameObject in scene.");
            }

            // Set serialized config via SerializedObject
            var so = new SerializedObject(adMgr);
            var propConfig = so.FindProperty("_config");
            if (propConfig != null)
            {
                propConfig.objectReferenceValue = config;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(adMgr);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            return $"Wired AdManager in {MAIN_MENU_SCENE} with AdConfig (Waterfall: {config.WaterfallStrategy}).";
        }
    }
}
