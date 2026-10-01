using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Economy;
using RoyalSmash.Skills;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Production verification suite testing all 8 store products,
    /// button wiring, reward fulfillment, double-click guards, No Ads persistence, and error handling.
    /// </summary>
    public static class ShopProductionTests
    {
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("RoyalSmash/Shop/Run Production Shop Tests")]
        public static string RunTests()
        {
            var results = new List<string>();
            int passed = 0;
            int total = 10;

            if (EditorSceneManager.GetActiveScene().path != MAIN_MENU_SCENE)
            {
                EditorSceneManager.OpenScene(MAIN_MENU_SCENE);
            }

            var saveMgr = SaveManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<SaveManager>();
            if (saveMgr == null)
            {
                var saveGo = new GameObject("SaveManager");
                saveMgr = saveGo.AddComponent<SaveManager>();
            }
            if (saveMgr.Data == null)
            {
                saveMgr.Load();
            }

            var goldMgr = GoldManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<GoldManager>();
            if (goldMgr == null)
            {
                var goldGo = new GameObject("GoldManager");
                goldMgr = goldGo.AddComponent<GoldManager>();
            }

            var skillMgr = SkillManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<SkillManager>();
            if (skillMgr == null)
            {
                var skillGo = new GameObject("SkillManager");
                skillMgr = skillGo.AddComponent<SkillManager>();
            }

            var iapMgr = IAPManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<IAPManager>();
            if (iapMgr == null)
            {
                var iapGo = new GameObject("IAPManager");
                iapMgr = iapGo.AddComponent<IAPManager>();
            }

            var config = AssetDatabase.LoadAssetAtPath<ShopConfig>(ShopSystemSetup.CONFIG_PATH);
            iapMgr.SetConfig(config);

            var canvas = GameObject.Find("MainMenuCanvas");
            var shopPageTrans = canvas != null ? canvas.transform.Find("PagesRoot/ShopPage") : null;
            var shopPage = shopPageTrans != null ? shopPageTrans.gameObject : GameObject.Find("ShopPage");
            var shopUI = shopPage != null ? shopPage.GetComponent<ShopUI>() : null;

            // -------------------------------------------------------------
            // TEST 1: All 8 Buy Buttons have active Button components and ShopItemCardUI
            // -------------------------------------------------------------
            try
            {
                string[] btnNames = new string[]
                {
                    "BuyButton_Deal", "BuyButton_NoAds", "BuyButton_Pouch", "BuyButton_Chest",
                    "BuyButton_Mega", "BuyButton_Infinity", "BuyButton_Warrior", "BuyButton_Champion"
                };

                int validButtons = 0;
                foreach (var name in btnNames)
                {
                    var btnTrans = shopPage.transform.FindDeepChild(name);
                    if (btnTrans != null)
                    {
                        var btn = btnTrans.GetComponent<Button>();
                        var card = btnTrans.GetComponent<ShopItemCardUI>();
                        if (btn != null && card != null && card.ItemData != null)
                        {
                            validButtons++;
                        }
                    }
                }

                if (validButtons == 8)
                {
                    results.Add("[PASS] Test 1: All 8 shop buy buttons have active Button and ShopItemCardUI with valid item data.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 1: Valid buttons count: {validButtons}/8");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 1: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 2: King's Royal Deal fulfillment (3,500 Gold, 10 Mega, 10 Infinity)
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                int preMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int preInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                var dealItem = config.FindItemById("deal_kings_royal");
                bool success = false;
                iapMgr.PurchaseItem(dealItem, (ok, msg) => { success = ok; });

                int postGold = goldMgr.CurrentGold;
                int postMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int postInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                bool goldOk = postGold == preGold + 3500;
                bool megaOk = postMega == preMega + 10;
                bool infOk = postInf == preInf + 10;

                if (success && goldOk && megaOk && infOk)
                {
                    results.Add("[PASS] Test 2: King's Royal Deal fulfilled exact +3,500 Gold, +10 Mega Balls, +10 Infinity Balls.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 2: success={success}, goldOk={goldOk} (+{postGold - preGold}), megaOk={megaOk} (+{postMega - preMega}), infOk={infOk} (+{postInf - preInf})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 2: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 3: No Ads Pass fulfillment and permanent save persistence
            // -------------------------------------------------------------
            try
            {
                saveMgr.Data.NoAdsPurchased = false;
                saveMgr.Save();

                var noAdsItem = config.FindItemById("no_ads_pass");
                bool success = false;
                iapMgr.PurchaseItem(noAdsItem, (ok, msg) => { success = ok; });

                bool isNoAdsSaved = saveMgr.Data.NoAdsPurchased;

                // Test second purchase attempt rejection (already owned)
                bool secondAttemptAllowed = iapMgr.PurchaseItem(noAdsItem);

                if (success && isNoAdsSaved && !secondAttemptAllowed)
                {
                    results.Add("[PASS] Test 3: No Ads Pass granted lifetime unlock in PlayerSaveData and correctly rejected duplicate purchase.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 3: success={success}, isNoAdsSaved={isNoAdsSaved}, secondAttemptAllowed={secondAttemptAllowed}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 3: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 4: Gold Pouch (800 Gold) fulfillment
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                var item = config.FindItemById("gold_pouch_800");

                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postGold = goldMgr.CurrentGold;
                bool goldOk = postGold == preGold + 800;

                if (success && goldOk)
                {
                    results.Add("[PASS] Test 4: Altin Cantasi credited exact +800 Gold.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 4: success={success}, goldOk={goldOk} (+{postGold - preGold})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 4: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 5: Gold Chest (1,600 Gold) fulfillment
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                var item = config.FindItemById("gold_chest_1600");

                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postGold = goldMgr.CurrentGold;
                bool goldOk = postGold == preGold + 1600;

                if (success && goldOk)
                {
                    results.Add("[PASS] Test 5: Altin Sandigi credited exact +1,600 Gold.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 5: success={success}, goldOk={goldOk} (+{postGold - preGold})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 5: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 6: Skill Pack Mega Ball (+5) fulfillment
            // -------------------------------------------------------------
            try
            {
                int preMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                var item = config.FindItemById("skill_mega_5");

                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                bool megaOk = postMega == preMega + 5;

                if (success && megaOk)
                {
                    results.Add("[PASS] Test 6: Mega Top (x5) credited exact +5 Mega Ball uses.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 6: success={success}, megaOk={megaOk} (+{postMega - preMega})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 6: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 7: Skill Pack Infinity Ball (+5) fulfillment
            // -------------------------------------------------------------
            try
            {
                int preInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);
                var item = config.FindItemById("skill_infinity_5");

                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);
                bool infOk = postInf == preInf + 5;

                if (success && infOk)
                {
                    results.Add("[PASS] Test 7: Sonsuz Top (x5) credited exact +5 Infinity Ball uses.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 7: success={success}, infOk={infOk} (+{postInf - preInf})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 7: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 8: Savasci Paketi (2,000 Gold + 5 Mega + 5 Infinity)
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                int preMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int preInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                var item = config.FindItemById("bundle_warrior");
                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postGold = goldMgr.CurrentGold;
                int postMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int postInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                bool goldOk = postGold == preGold + 2000;
                bool megaOk = postMega == preMega + 5;
                bool infOk = postInf == preInf + 5;

                if (success && goldOk && megaOk && infOk)
                {
                    results.Add("[PASS] Test 8: Savasci Paketi credited +2,000 Gold, +5 Mega, +5 Infinity (Rift Crystal replaced).");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 8: success={success}, goldOk={goldOk}, megaOk={megaOk}, infOk={infOk}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 8: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 9: Sampiyon Paketi (8,000 Gold + 15 Mega + 15 Infinity)
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                int preMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int preInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                var item = config.FindItemById("bundle_champion");
                bool success = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { success = ok; });

                int postGold = goldMgr.CurrentGold;
                int postMega = skillMgr.GetRemainingUses(SkillType.MegaBall);
                int postInf = skillMgr.GetRemainingUses(SkillType.InfinityBall);

                bool goldOk = postGold == preGold + 8000;
                bool megaOk = postMega == preMega + 15;
                bool infOk = postInf == preInf + 15;

                if (success && goldOk && megaOk && infOk)
                {
                    results.Add("[PASS] Test 9: Sampiyon Paketi credited +8,000 Gold, +15 Mega, +15 Infinity (Rift Crystal replaced).");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 9: success={success}, goldOk={goldOk}, megaOk={megaOk}, infOk={infOk}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 9: Exception - {ex.Message}");
            }

            // -------------------------------------------------------------
            // TEST 10: Error handling & Double-Click lock prevention
            // -------------------------------------------------------------
            try
            {
                int preGold = goldMgr.CurrentGold;
                var item = config.FindItemById("gold_pouch_800");

                // Simulated failure
                iapMgr.SimulateNextPurchaseFailure = true;
                bool failedHandled = false;
                iapMgr.PurchaseItem(item, (ok, msg) => { if (!ok) failedHandled = true; });

                int postGold = goldMgr.CurrentGold;
                bool noRewardCredited = preGold == postGold;

                // Double click lock test
                typeof(IAPManager).GetMethod("SetPurchaseInProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(iapMgr, new object[] { true });

                bool concurrentPurchaseBlocked = !iapMgr.PurchaseItem(item);

                typeof(IAPManager).GetMethod("SetPurchaseInProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(iapMgr, new object[] { false });

                if (failedHandled && noRewardCredited && concurrentPurchaseBlocked)
                {
                    results.Add("[PASS] Test 10: Transaction failure safely handled with zero reward leaks; double-click guard blocked concurrent requests.");
                    passed++;
                }
                else
                {
                    results.Add($"[FAIL] Test 10: failedHandled={failedHandled}, noRewardCredited={noRewardCredited}, concurrentPurchaseBlocked={concurrentPurchaseBlocked}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"[FAIL] Test 10: Exception - {ex.Message}");
            }

            string report = $"=== SHOP & IAP PRODUCTION TEST RESULTS: {passed}/{total} PASSED ===\n" +
                            string.Join("\n", results);
            Debug.Log(report);
            return report;
        }

        private static Transform FindDeepChild(this Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = parent.GetChild(i).FindDeepChild(name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
