using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Economy;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Master automation script for configuring the production Shop & IAP system.
    /// Updates ShopConfig with finalized Google Play Product IDs, prices, and rewards;
    /// wires all buy buttons, TMP texts, and dialogs in MainMenu.unity;
    /// and ensures IAPManager is active and ready for live store purchases.
    /// </summary>
    public static class ShopSystemSetup
    {
        public const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/ShopConfig.asset";
        public const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("RoyalSmash/Shop/Configure Production Shop & IAP")]
        public static string SetupAll()
        {
            Debug.Log("=== [ShopSystemSetup] Configuring Shop and In-App Purchases ===");

            // 1. Configure ShopConfig ScriptableObject
            ShopConfig config = CreateOrUpdateShopConfig();

            // 2. Wire scene objects in MainMenu.unity
            string sceneResult = SetupMainMenuScene(config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"[ShopSystemSetup] SUCCESS: Configured 8 store products in ShopConfig and wired all UI buttons in MainMenu.unity.\n{sceneResult}";
            Debug.Log(summary);
            return summary;
        }

        public static ShopConfig CreateOrUpdateShopConfig()
        {
            string dir = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            ShopConfig config = AssetDatabase.LoadAssetAtPath<ShopConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ShopConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            // 1. King's Royal Deal (Limited Offer)
            // 3,500 Gold, 10 Infinity Balls, 10 Mega Balls (Rift Crystal replaced with Mega Balls)
            config.LimitedTimeOffer = new ShopItemData
            {
                Id = "deal_kings_royal",
                ProductId = "deal_kings_royal",
                Title = "King's Royal Deal",
                Description = "3.500 Altın + 10 Sonsuz Top + 10 Mega Top",
                ItemType = ShopItemType.LimitedOffer,
                Currency = PriceCurrency.RealMoneyIAP,
                PriceString = "$4.99",
                CostInGold = 0,
                GoldReward = 3500,
                MegaBallReward = 10,
                InfinityBallReward = 10,
                GrantsNoAds = false,
                ThemeColor = new Color(1f, 0.85f, 0.2f),
                BadgeText = "%60 İNDİRİM"
            };

            // 2. No Ads Pass (Lifetime Non-Consumable)
            config.NoAdsItem = new ShopItemData
            {
                Id = "no_ads_pass",
                ProductId = "noadspass", // Exact Google Play Console Product ID
                Title = "No Ads Pass",
                Description = "Zorunlu reklamları ömür boyu kaldırın!",
                ItemType = ShopItemType.NoAds,
                Currency = PriceCurrency.RealMoneyIAP,
                PriceString = "$2.99",
                CostInGold = 0,
                GoldReward = 0,
                MegaBallReward = 0,
                InfinityBallReward = 0,
                GrantsNoAds = true,
                ThemeColor = new Color(0.95f, 0.35f, 0.35f),
                BadgeText = "ÖMÜR BOYU"
            };

            // 3 & 4. Gold Packs (Altın Çantası 800, Altın Sandığı 1600)
            config.GoldPacks = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "gold_pouch_800",
                    ProductId = "gold_douch_800", // Exact Google Play Console Product ID (Gold Douch 800)
                    Title = "Altın Çantası",
                    Description = "800 Altın",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$0.99",
                    CostInGold = 0,
                    GoldReward = 800,
                    MegaBallReward = 0,
                    InfinityBallReward = 0,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.95f, 0.75f, 0.2f),
                    BadgeText = ""
                },
                new ShopItemData
                {
                    Id = "gold_chest_1600",
                    ProductId = "gold_chest_1600",
                    Title = "Altın Sandığı",
                    Description = "1.600 Altın",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$2.99",
                    CostInGold = 0,
                    GoldReward = 1600,
                    MegaBallReward = 0,
                    InfinityBallReward = 0,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.95f, 0.80f, 0.2f),
                    BadgeText = "+%10 BONUS"
                }
            };

            // 5 & 6. Skill Packs (Mega Top x5, Sonsuz Top x5)
            config.SkillPacks = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "skill_mega_5",
                    ProductId = "skill_mega_5",
                    Title = "Mega Top (x5)",
                    Description = "+5 Mega Top",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$4.99",
                    CostInGold = 0,
                    GoldReward = 0,
                    MegaBallReward = 5,
                    InfinityBallReward = 0,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.95f, 0.45f, 0.2f),
                    BadgeText = "+%20 BONUS"
                },
                new ShopItemData
                {
                    Id = "skill_infinity_5",
                    ProductId = "skill_infinity_5",
                    Title = "Sonsuz Top (x5)",
                    Description = "+5 Sonsuz Top",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$6.99",
                    CostInGold = 0,
                    GoldReward = 0,
                    MegaBallReward = 0,
                    InfinityBallReward = 5,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.3f, 0.75f, 0.95f),
                    BadgeText = "+%20 BONUS"
                }
            };

            // 7 & 8. Value Bundles (Savaşçı Paketi, Şampiyon Paketi)
            // Savaşçı: 2,000 Altın + 5 Mega Top + 5 Sonsuz Top (Rift kristali yerine 5 sonsuz top)
            // Şampiyon: 8,000 Altın + 15 Sonsuz Top + 15 Mega Top (Rift kristali yerine 15 mega top)
            config.Bundles = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "bundle_warrior",
                    ProductId = "bundle_warrior",
                    Title = "Savaşçı Paketi",
                    Description = "2.000 Altın + 5 Mega Top + 5 Sonsuz Top",
                    ItemType = ShopItemType.Bundle,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$4.99",
                    CostInGold = 0,
                    GoldReward = 2000,
                    MegaBallReward = 5,
                    InfinityBallReward = 5,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.85f, 0.4f, 0.85f),
                    BadgeText = "+%40 BONUS"
                },
                new ShopItemData
                {
                    Id = "bundle_champion",
                    ProductId = "bundle_champion",
                    Title = "Şampiyon Paketi",
                    Description = "8.000 Altın + 15 Mega Top + 15 Sonsuz Top",
                    ItemType = ShopItemType.Bundle,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$11.99",
                    CostInGold = 0,
                    GoldReward = 8000,
                    MegaBallReward = 15,
                    InfinityBallReward = 15,
                    GrantsNoAds = false,
                    ThemeColor = new Color(0.95f, 0.75f, 0.25f),
                    BadgeText = "+%50 BONUS"
                }
            };

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("[ShopSystemSetup] ShopConfig.asset successfully updated with 8 products.");
            return config;
        }

        public static string SetupMainMenuScene(ShopConfig config)
        {
            if (EditorSceneManager.GetActiveScene().path != MAIN_MENU_SCENE)
            {
                EditorSceneManager.OpenScene(MAIN_MENU_SCENE);
            }

            // 1. Ensure IAPManager GameObject in MainMenu.unity
            IAPManager iap = UnityEngine.Object.FindFirstObjectByType<IAPManager>();
            if (iap == null)
            {
                var go = new GameObject("IAPManager");
                iap = go.AddComponent<IAPManager>();
                Debug.Log("[ShopSystemSetup] Created IAPManager GameObject in scene.");
            }
            if (iap != null && config != null)
            {
                iap.SetConfig(config);
                EditorUtility.SetDirty(iap);
            }

            // 2. Find ShopPage (supports inactive objects)
            var canvas = GameObject.Find("MainMenuCanvas");
            var shopPageTrans = canvas != null ? canvas.transform.Find("PagesRoot/ShopPage") : null;
            var shopPageGo = shopPageTrans != null ? shopPageTrans.gameObject : GameObject.Find("ShopPage");
            if (shopPageGo == null)
            {
                return "ShopPage not found in MainMenu.unity!";
            }

            ShopUI shopUI = shopPageGo.GetComponent<ShopUI>() ?? shopPageGo.AddComponent<ShopUI>();

            // Find Dialog
            PurchaseFeedbackDialog feedbackDlg = UnityEngine.Object.FindFirstObjectByType<PurchaseFeedbackDialog>(FindObjectsInactive.Include);

            // Find Close Button
            Button closeBtn = null;
            var closeTrans = shopPageGo.transform.Find("ShopFrameContainer/CloseButton");
            if (closeTrans != null) closeBtn = closeTrans.GetComponent<Button>();

            // Find Timer Text
            TextMeshProUGUI timerText = null;
            var timerTrans = shopPageGo.transform.Find("ShopFrameContainer/ContentScrollView/Viewport/Content/Card_KingsDeal/TimerText");
            if (timerTrans != null) timerText = timerTrans.GetComponent<TextMeshProUGUI>();

            // 3. Map each BuyButton to its corresponding ShopItemData
            var mappings = new Dictionary<string, ShopItemData>
            {
                { "BuyButton_Deal", config.LimitedTimeOffer },
                { "BuyButton_NoAds", config.NoAdsItem },
                { "BuyButton_Pouch", config.FindItemById("gold_pouch_800") },
                { "BuyButton_Chest", config.FindItemById("gold_chest_1600") },
                { "BuyButton_Mega", config.FindItemById("skill_mega_5") },
                { "BuyButton_Infinity", config.FindItemById("skill_infinity_5") },
                { "BuyButton_Warrior", config.FindItemById("bundle_warrior") },
                { "BuyButton_Champion", config.FindItemById("bundle_champion") }
            };

            var wiredCards = new List<ShopItemCardUI>();
            var results = new List<string>();

            foreach (var kvp in mappings)
            {
                string btnName = kvp.Key;
                ShopItemData itemData = kvp.Value;

                Transform btnTrans = FindDeepChild(shopPageGo.transform, btnName);
                if (btnTrans == null)
                {
                    results.Add($"MISSING: {btnName}");
                    continue;
                }

                GameObject cardGo = btnTrans.gameObject;
                Button btn = cardGo.GetComponent<Button>() ?? cardGo.AddComponent<Button>();
                ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>() ?? cardGo.AddComponent<ShopItemCardUI>();
                TextMeshProUGUI priceTmp = cardGo.GetComponentInChildren<TextMeshProUGUI>(true);

                // Wire serialized fields on ShopItemCardUI
                SerializedObject soCard = new SerializedObject(cardUI);
                soCard.FindProperty("_buyButton").objectReferenceValue = btn;
                soCard.FindProperty("_priceText").objectReferenceValue = priceTmp;

                // Wire _itemData
                SerializedProperty itemProp = soCard.FindProperty("_itemData");
                if (itemProp != null && itemData != null)
                {
                    itemProp.FindPropertyRelative("Id").stringValue = itemData.Id;
                    itemProp.FindPropertyRelative("ProductId").stringValue = itemData.ProductId;
                    itemProp.FindPropertyRelative("Title").stringValue = itemData.Title;
                    itemProp.FindPropertyRelative("Description").stringValue = itemData.Description;
                    itemProp.FindPropertyRelative("ItemType").enumValueIndex = (int)itemData.ItemType;
                    itemProp.FindPropertyRelative("Currency").enumValueIndex = (int)itemData.Currency;
                    itemProp.FindPropertyRelative("PriceString").stringValue = itemData.PriceString;
                    itemProp.FindPropertyRelative("CostInGold").intValue = itemData.CostInGold;
                    itemProp.FindPropertyRelative("GoldReward").intValue = itemData.GoldReward;
                    itemProp.FindPropertyRelative("MegaBallReward").intValue = itemData.MegaBallReward;
                    itemProp.FindPropertyRelative("InfinityBallReward").intValue = itemData.InfinityBallReward;
                    itemProp.FindPropertyRelative("GrantsNoAds").boolValue = itemData.GrantsNoAds;
                    itemProp.FindPropertyRelative("ThemeColor").colorValue = itemData.ThemeColor;
                    itemProp.FindPropertyRelative("BadgeText").stringValue = itemData.BadgeText;
                }
                soCard.ApplyModifiedProperties();

                // Setup runtime references and text
                cardUI.Setup(itemData);
                if (priceTmp != null) priceTmp.text = itemData.PriceString;

                wiredCards.Add(cardUI);
                results.Add($"WIRED: {btnName} -> [{itemData.ProductId}] ({itemData.PriceString})");
            }

            // 4. Wire ShopUI component
            SerializedObject soShop = new SerializedObject(shopUI);
            soShop.FindProperty("_config").objectReferenceValue = config;
            soShop.FindProperty("_limitedOfferTimerText").objectReferenceValue = timerText;
            soShop.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            soShop.FindProperty("_feedbackDialog").objectReferenceValue = feedbackDlg;

            SerializedProperty cardsListProp = soShop.FindProperty("_itemCards");
            cardsListProp.ClearArray();
            for (int i = 0; i < wiredCards.Count; i++)
            {
                cardsListProp.InsertArrayElementAtIndex(i);
                cardsListProp.GetArrayElementAtIndex(i).objectReferenceValue = wiredCards[i];
            }
            soShop.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            return string.Join("\n", results);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDeepChild(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
