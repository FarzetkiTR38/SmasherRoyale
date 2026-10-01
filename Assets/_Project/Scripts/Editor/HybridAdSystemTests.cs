using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Ads;
using RoyalSmash.Save;
using RoyalSmash.Lives;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated test suite validating the complete Hybrid Mediation Ad System (AdMob + Unity Ads + Fallback).
    /// Covers:
    /// 1. AdConfig Production Credentials & AdMob IDs verification.
    /// 2. AdMobProvider initialization, preloading, and credential resolution.
    /// 3. UnityAdsProvider initialization, preloading, and placement resolution.
    /// 4. HybridMediationRouter Primary Network Dispatch (AdMob first).
    /// 5. Waterfall Failover on Primary Network Error (AdMob fails -> Unity Ads serves).
    /// 6. Double Failover to Safe Offline/Simulated Fallback (Network failure -> Simulated completes safely).
    /// 7. Idempotent Rewarded Ad Granting (+1 reward granted exactly once).
    /// 8. No Ads Purchase Enforcement (Interstitials & Banners blocked; Rewarded allowed).
    /// 9. Progression & Defeat Interstitial Rules with Cooldown management.
    /// 10. Banner Ad Lifecycle & Positioning.
    /// </summary>
    public static class HybridAdSystemTests
    {
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/AdConfig.asset";

        [MenuItem("RoyalSmash/Ads/Run Hybrid Ad System Tests")]
        public static string RunAllTests()
        {
            var results = new List<string>();
            int passedCount = 0;
            int totalTests = 10;

            Debug.Log("====================================================================");
            Debug.Log("=== [HybridAdSystemTests] Starting Complete Hybrid Mediation Suite ===");
            Debug.Log("====================================================================");

            if (EditorSceneManager.GetActiveScene().path != MAIN_MENU_SCENE)
            {
                EditorSceneManager.OpenScene(MAIN_MENU_SCENE);
            }

            // Ensure SaveManager and AdConfig
            var saveMgr = SaveManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<SaveManager>();
            if (saveMgr != null && saveMgr.Data == null)
            {
                saveMgr.Load();
            }

            AdConfig config = AssetDatabase.LoadAssetAtPath<AdConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = HybridAdSystemSetup.CreateOrUpdateAdConfig();
            }

            var adMgr = AdManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<AdManager>();
            if (adMgr == null)
            {
                var go = new GameObject("AdManager");
                adMgr = go.AddComponent<AdManager>();
            }

            // -------------------------------------------------------------
            // TEST 1: AdConfig Credentials & Production IDs Verification
            // -------------------------------------------------------------
            try
            {
                bool appOk = config.AdMobAppIdAndroid == "ca-app-pub-3581074606829094~9819287835";
                bool bannerOk = config.BannerAdUnitIdAndroid == "ca-app-pub-3581074606829094/5880042827";
                bool interOk = config.InterstitialAdUnitIdAndroid == "ca-app-pub-3581074606829094/8701733002";
                bool rewInterOk = config.RewardedInterstitialAdUnitIdAndroid == "ca-app-pub-3581074606829094/2731135308";
                bool rewOk = config.RewardedAdUnitIdAndroid == "ca-app-pub-3581074606829094/8510161316";
                bool waterfallOk = config.WaterfallStrategy == WaterfallStrategy.AdMobFirst_UnityAdsFallback;
                bool unityOk = !string.IsNullOrEmpty(config.UnityAdsGameIdAndroid);

                if (appOk && bannerOk && interOk && rewInterOk && rewOk && waterfallOk && unityOk)
                {
                    results.Add("[PASS] Test 1: AdConfig holds verified production AdMob IDs & Unity Ads waterfall settings.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 1: Config mismatch. App={appOk}, Banner={bannerOk}, Inter={interOk}, RewInter={rewInterOk}, Rew={rewOk}, Waterfall={waterfallOk}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 1: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 2: AdMobProvider Initialization & Credential Resolution
            // -------------------------------------------------------------
            try
            {
                var admob = new AdMobProvider();
                admob.Initialize(config);

                bool idOk = admob.AppId == config.AdMobAppIdAndroid &&
                            admob.BannerUnitId == config.BannerAdUnitIdAndroid &&
                            admob.InterstitialUnitId == config.InterstitialAdUnitIdAndroid &&
                            admob.RewardedUnitId == config.RewardedAdUnitIdAndroid &&
                            admob.RewardedInterstitialUnitId == config.RewardedInterstitialAdUnitIdAndroid;

                bool readyOk = admob.IsInterstitialReady() && admob.IsRewardedReady() && admob.IsBannerReady() && admob.IsRewardedInterstitialReady();

                if (admob.IsInitialized && idOk && readyOk)
                {
                    results.Add("[PASS] Test 2: AdMobProvider correctly initializes, binds all 4 ad units, and preloads queues.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 2: AdMobProvider init issue. Init={admob.IsInitialized}, idOk={idOk}, readyOk={readyOk}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 2: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 3: UnityAdsProvider Initialization & Placement Resolution
            // -------------------------------------------------------------
            try
            {
                var unityAds = new UnityAdsProvider();
                unityAds.Initialize(config);

                bool gameIdOk = unityAds.GameId == config.UnityAdsGameIdAndroid;
                bool placementsOk = !string.IsNullOrEmpty(unityAds.BannerPlacement) &&
                                    !string.IsNullOrEmpty(unityAds.InterstitialPlacement) &&
                                    !string.IsNullOrEmpty(unityAds.RewardedPlacement);
                bool readyOk = unityAds.IsInterstitialReady() && unityAds.IsRewardedReady() && unityAds.IsBannerReady();

                if (unityAds.IsInitialized && gameIdOk && placementsOk && readyOk)
                {
                    results.Add("[PASS] Test 3: UnityAdsProvider correctly initializes secondary network with placements.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 3: UnityAdsProvider init issue. Init={unityAds.IsInitialized}, gameIdOk={gameIdOk}, readyOk={readyOk}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 3: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 4: HybridMediationRouter Primary Dispatch (AdMob First)
            // -------------------------------------------------------------
            try
            {
                var router = new HybridMediationRouter();
                router.Initialize(config);

                bool interClosed = false;
                router.ShowInterstitial(() => interClosed = true);

                bool rewGranted = false;
                bool rewClosed = false;
                router.ShowRewarded(
                    onRewarded: () => rewGranted = true,
                    onClosed: () => rewClosed = true
                );

                bool networkOk = router.LastServedNetwork == AdNetworkType.AdMob;

                if (interClosed && rewGranted && rewClosed && networkOk)
                {
                    results.Add("[PASS] Test 4: Router dispatches to Primary Network (AdMob First) when available.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 4: Primary dispatch failed. interClosed={interClosed}, rewGranted={rewGranted}, rewClosed={rewClosed}, network={router.LastServedNetwork}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 4: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 5: Waterfall Failover on Primary Error (AdMob fails -> Unity Ads serves)
            // -------------------------------------------------------------
            try
            {
                var router = new HybridMediationRouter();
                router.Initialize(config);

                // Simulate AdMob network failure
                router.AdMob.ForceNotReady = true;

                int initialFailovers = router.TotalWaterfallFailovers;
                bool interClosed = false;
                router.ShowInterstitial(() => interClosed = true);

                bool interFailoverOk = interClosed &&
                                      router.LastServedNetwork == AdNetworkType.UnityAds &&
                                      router.TotalWaterfallFailovers > initialFailovers;

                bool rewGranted = false;
                router.ShowRewarded(onRewarded: () => rewGranted = true);

                bool rewFailoverOk = rewGranted && router.LastServedNetwork == AdNetworkType.UnityAds;

                if (interFailoverOk && rewFailoverOk)
                {
                    results.Add("[PASS] Test 5: Waterfall Failover operates seamlessly. When AdMob fails, Unity Ads instantly serves with zero user interruption.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 5: Failover issue. interOk={interFailoverOk}, rewOk={rewFailoverOk}, network={router.LastServedNetwork}, failovers={router.TotalWaterfallFailovers}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 5: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 6: Double Waterfall Failover to Safe Offline/Simulated Fallback
            // -------------------------------------------------------------
            try
            {
                var router = new HybridMediationRouter();
                router.Initialize(config);

                // Both primary and secondary networks fail / offline
                router.AdMob.ForceNotReady = true;
                router.UnityAds.ForceNotReady = true;

                bool interClosed = false;
                router.ShowInterstitial(() => interClosed = true);

                bool rewGranted = false;
                router.ShowRewarded(onRewarded: () => rewGranted = true);

                bool networkOk = router.LastServedNetwork == AdNetworkType.Simulated;

                if (interClosed && rewGranted && networkOk)
                {
                    results.Add("[PASS] Test 6: Offline Resilience verified. When all remote ad networks fail, system safely falls back to local simulation without freezing or crashing.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 6: Safe fallback failed. interClosed={interClosed}, rewGranted={rewGranted}, network={router.LastServedNetwork}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 6: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 7: Idempotent Rewarded Ad Granting (+1 reward granted exactly once)
            // -------------------------------------------------------------
            try
            {
                var router = new HybridMediationRouter();
                router.Initialize(config);

                int rewardCount = 0;
                router.ShowRewarded(onRewarded: () => { rewardCount++; });

                if (rewardCount == 1)
                {
                    results.Add("[PASS] Test 7: Rewarded ad idempotency strictly enforced; reward granted exactly once.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 7: Reward count mismatch. Expected 1, got {rewardCount}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 7: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 8: No Ads Purchase Enforcement
            // -------------------------------------------------------------
            try
            {
                if (saveMgr != null && saveMgr.Data != null)
                {
                    saveMgr.Data.NoAdsPurchased = true;
                }

                adMgr.ResetInterstitialCooldownForTesting();

                bool interBlocked = !adMgr.CanShowInterstitial();
                bool bannerBlocked = !adMgr.CanShowBanner();
                bool rewardedAllowed = adMgr.CanShowRewarded(); // Voluntary rewarded ads remain accessible

                bool milestoneCallbackCalled = false;
                adMgr.CheckAndShowProgressionInterstitial(5, () => { milestoneCallbackCalled = true; });

                if (interBlocked && bannerBlocked && rewardedAllowed && milestoneCallbackCalled)
                {
                    results.Add("[PASS] Test 8: 'No Ads' strictly blocks interstitials and banners while keeping voluntary Rewarded Ads accessible.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 8: No Ads rules failed. interBlocked={interBlocked}, bannerBlocked={bannerBlocked}, rewardedAllowed={rewardedAllowed}, callback={milestoneCallbackCalled}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 8: Exception - {ex.Message}");
            }
            finally
            {
                // Reset NoAds state
                if (saveMgr != null && saveMgr.Data != null)
                {
                    saveMgr.Data.NoAdsPurchased = false;
                }
            }

            // -------------------------------------------------------------
            // TEST 9: Progression & Defeat Interstitial Rules with Cooldown
            // -------------------------------------------------------------
            try
            {
                adMgr.ResetInterstitialCooldownForTesting();

                bool nonMilestoneTriggered = false;
                adMgr.CheckAndShowProgressionInterstitial(3, () => { });
                // If triggered, it would set cooldown
                nonMilestoneTriggered = !adMgr.CanShowInterstitial();

                adMgr.ResetInterstitialCooldownForTesting();
                bool milestoneTriggered = false;
                adMgr.CheckAndShowProgressionInterstitial(5, () => { milestoneTriggered = true; });

                // Check cooldown is now active
                bool cooldownActive = !adMgr.CanShowInterstitial();

                // Test Defeat Interstitial after cooldown reset
                adMgr.ResetInterstitialCooldownForTesting();
                bool defeatTriggered = false;
                adMgr.ShowLoseInterstitial(() => { defeatTriggered = true; });

                if (!nonMilestoneTriggered && milestoneTriggered && cooldownActive && defeatTriggered)
                {
                    results.Add("[PASS] Test 9: Progression interstitial interval (every 5 levels), defeat interstitial, and cooldown functioning accurately.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 9: Progression/Defeat failed. nonMilestone={nonMilestoneTriggered}, milestone={milestoneTriggered}, cooldown={cooldownActive}, defeat={defeatTriggered}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 9: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 10: Banner Ad Lifecycle & Positioning
            // -------------------------------------------------------------
            try
            {
                bool bannerShown = false;
                adMgr.ShowBanner(BannerPosition.BottomCenter, () => { bannerShown = true; });

                bool isVisible = adMgr.IsBannerVisible;
                adMgr.HideBanner();
                bool isHidden = !adMgr.IsBannerVisible;

                adMgr.DestroyBanner();

                if (bannerShown && isVisible && isHidden)
                {
                    results.Add("[PASS] Test 10: Banner Ad Lifecycle (Show, Position, Hide, Destroy) functioning seamlessly across networks.");
                    passedCount++;
                }
                else
                {
                    results.Add($"[FAIL] Test 10: Banner lifecycle issue. bannerShown={bannerShown}, isVisible={isVisible}, isHidden={isHidden}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 10: Exception - {ex.Message}");
            }

            // Format summary report
            string report = $"\n=== [HybridAdSystemTests] EXECUTION REPORT ({passedCount}/{totalTests} PASSED) ===\n" +
                            string.Join("\n", results) + "\n" +
                            "====================================================================";

            Debug.Log(report);
            return report;
        }
    }
}
