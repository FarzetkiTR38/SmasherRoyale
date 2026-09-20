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
    /// Automation script for setting up Phase 13 (Shop & IAP Framework).
    /// Creates ShopConfig ScriptableObject with all items, builds the scrollable
    /// shop interface inside MainMenu.unity, and wires IAPManager and ShopUI.
    /// </summary>
    public static class Phase13Setup
    {
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/ShopConfig.asset";
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("RoyalSmash/Phase 13/Setup Shop and IAP")]
        public static void RunFullSetup()
        {
            Debug.Log("=== STARTING PHASE 13 AUTOMATED SETUP ===");

            // 1. Create or update ShopConfig
            ShopConfig config = CreateOrUpdateShopConfig();

            // 2. Setup MainMenu scene
            SetupMainMenuScene(config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== PHASE 13 AUTOMATED SETUP COMPLETED SUCCESSFULLY ===");
        }

        private static ShopConfig CreateOrUpdateShopConfig()
        {
            string dir = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            ShopConfig config = AssetDatabase.LoadAssetAtPath<ShopConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ShopConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            // Limited-Time Offer
            config.LimitedTimeOffer = new ShopItemData
            {
                Id = "limited_deal",
                ProductId = "com.royalsmash.limited_deal",
                Title = "King's Royal Deal",
                Description = "3,500 Gold + 10 Mega Balls + 10 Infinity Balls",
                ItemType = ShopItemType.LimitedOffer,
                Currency = PriceCurrency.RealMoneyIAP,
                PriceString = "$4.99",
                GoldReward = 3500,
                MegaBallReward = 10,
                InfinityBallReward = 10,
                ThemeColor = new Color(1f, 0.85f, 0.2f),
                BadgeText = "60% OFF"
            };

            // No Ads (One-Time IAP)
            config.NoAdsItem = new ShopItemData
            {
                Id = "no_ads",
                ProductId = "com.royalsmash.no_ads",
                Title = "No Ads Pass",
                Description = "Permanently remove all forced interstitial ads!",
                ItemType = ShopItemType.NoAds,
                Currency = PriceCurrency.RealMoneyIAP,
                PriceString = "$2.99",
                GrantsNoAds = true,
                ThemeColor = new Color(0.9f, 0.35f, 0.35f),
                BadgeText = "LIFETIME"
            };

            // Gold Packs (4 items)
            config.GoldPacks = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "gold_pouch",
                    ProductId = "com.royalsmash.gold_pouch",
                    Title = "Pouch of Gold",
                    Description = "500 Gold",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$0.99",
                    GoldReward = 500,
                    ThemeColor = new Color(0.95f, 0.75f, 0.2f)
                },
                new ShopItemData
                {
                    Id = "gold_chest",
                    ProductId = "com.royalsmash.gold_chest",
                    Title = "Chest of Gold",
                    Description = "1,500 Gold",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$2.99",
                    GoldReward = 1500,
                    ThemeColor = new Color(0.95f, 0.80f, 0.2f),
                    BadgeText = "+10% BONUS"
                },
                new ShopItemData
                {
                    Id = "gold_vault",
                    ProductId = "com.royalsmash.gold_vault",
                    Title = "Vault of Gold",
                    Description = "5,000 Gold",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$7.99",
                    GoldReward = 5000,
                    ThemeColor = new Color(1.0f, 0.85f, 0.25f),
                    BadgeText = "+25% BONUS"
                },
                new ShopItemData
                {
                    Id = "gold_treasury",
                    ProductId = "com.royalsmash.gold_treasury",
                    Title = "Royal Treasury",
                    Description = "15,000 Gold",
                    ItemType = ShopItemType.GoldPack,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$19.99",
                    GoldReward = 15000,
                    ThemeColor = new Color(1.0f, 0.90f, 0.3f),
                    BadgeText = "BEST VALUE"
                }
            };

            // Skill Packs (4 items)
            config.SkillPacks = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "skill_mega_5",
                    ProductId = "com.royalsmash.skill_mega_5",
                    Title = "Mega Ball (5x)",
                    Description = "+5 Mega Ball uses",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.InGameGold,
                    PriceString = "200 Gold",
                    CostInGold = 200,
                    MegaBallReward = 5,
                    ThemeColor = new Color(0.95f, 0.45f, 0.2f)
                },
                new ShopItemData
                {
                    Id = "skill_infinity_5",
                    ProductId = "com.royalsmash.skill_infinity_5",
                    Title = "Infinity Ball (5x)",
                    Description = "+5 Infinity Ball uses",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.InGameGold,
                    PriceString = "300 Gold",
                    CostInGold = 300,
                    InfinityBallReward = 5,
                    ThemeColor = new Color(0.3f, 0.75f, 0.95f)
                },
                new ShopItemData
                {
                    Id = "skill_mega_15",
                    ProductId = "com.royalsmash.skill_mega_15",
                    Title = "Mega Ball (15x)",
                    Description = "+15 Mega Ball uses",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.InGameGold,
                    PriceString = "500 Gold",
                    CostInGold = 500,
                    MegaBallReward = 15,
                    ThemeColor = new Color(0.95f, 0.45f, 0.2f),
                    BadgeText = "+20% BONUS"
                },
                new ShopItemData
                {
                    Id = "skill_infinity_15",
                    ProductId = "com.royalsmash.skill_infinity_15",
                    Title = "Infinity Ball (15x)",
                    Description = "+15 Infinity Ball uses",
                    ItemType = ShopItemType.SkillPack,
                    Currency = PriceCurrency.InGameGold,
                    PriceString = "750 Gold",
                    CostInGold = 750,
                    InfinityBallReward = 15,
                    ThemeColor = new Color(0.3f, 0.75f, 0.95f),
                    BadgeText = "+20% BONUS"
                }
            };

            // Value Bundles (2 items)
            config.Bundles = new List<ShopItemData>
            {
                new ShopItemData
                {
                    Id = "bundle_warrior",
                    ProductId = "com.royalsmash.bundle_warrior",
                    Title = "Warrior Bundle",
                    Description = "2,000 Gold + 5 Mega + 5 Infinity",
                    ItemType = ShopItemType.Bundle,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$4.99",
                    GoldReward = 2000,
                    MegaBallReward = 5,
                    InfinityBallReward = 5,
                    ThemeColor = new Color(0.85f, 0.4f, 0.85f),
                    BadgeText = "POPULAR"
                },
                new ShopItemData
                {
                    Id = "bundle_champion",
                    ProductId = "com.royalsmash.bundle_champion",
                    Title = "Champion Bundle",
                    Description = "6,000 Gold + 15 Mega + 15 Infinity",
                    ItemType = ShopItemType.Bundle,
                    Currency = PriceCurrency.RealMoneyIAP,
                    PriceString = "$11.99",
                    GoldReward = 6000,
                    MegaBallReward = 15,
                    InfinityBallReward = 15,
                    ThemeColor = new Color(0.95f, 0.75f, 0.25f),
                    BadgeText = "ULTIMATE"
                }
            };

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void SetupMainMenuScene(ShopConfig config)
        {
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            // 1. Ensure IAPManager
            IAPManager iap = UnityEngine.Object.FindFirstObjectByType<IAPManager>();
            if (iap == null)
            {
                var go = new GameObject("IAPManager");
                iap = go.AddComponent<IAPManager>();
            }
            if (iap != null && config != null)
            {
                iap.SetConfig(config);
                EditorUtility.SetDirty(iap);
            }

            // 2. Find Canvas and Dialogs
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform dialogsRoot = canvas.transform.Find("Dialogs") ?? canvas.transform;
            PurchaseFeedbackDialog feedbackDlg = BuildFeedbackDialog(dialogsRoot);

            // 3. Find PagesRoot/ShopPage
            Transform pagesRoot = canvas.transform.Find("SafeArea/PagesRoot") ?? canvas.transform.Find("PagesRoot");
            if (pagesRoot == null) return;

            Transform shopPageTrans = pagesRoot.Find("ShopPage");
            if (shopPageTrans == null) return;

            // Clear old placeholder Card
            for (int i = shopPageTrans.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(shopPageTrans.GetChild(i).gameObject);
            }

            // 4. Build Full Scrollable Shop Screen
            ShopUI shopUI = BuildShopScreen(shopPageTrans, config, feedbackDlg);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Phase13Setup] MainMenu.unity configured with complete Shop UI.");
        }

        private static ShopUI BuildShopScreen(Transform parent, ShopConfig config, PurchaseFeedbackDialog feedbackDlg)
        {
            ShopUI shopUI = parent.GetComponent<ShopUI>() ?? parent.gameObject.AddComponent<ShopUI>();
            List<ShopItemCardUI> allCards = new List<ShopItemCardUI>();

            // Setup ShopPage RectTransform to fit between TopBar and BottomNavigation
            RectTransform pageRt = parent.GetComponent<RectTransform>();
            pageRt.anchorMin = new Vector2(0f, 0.12f);
            pageRt.anchorMax = new Vector2(1f, 0.88f);
            pageRt.offsetMin = Vector2.zero;
            pageRt.offsetMax = Vector2.zero;

            // ScrollRect
            GameObject scrollGo = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(parent, false);
            RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            Image scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.10f, 0.16f, 0.85f); // Sleek dark royal backing

            ScrollRect sr = scrollGo.GetComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            GameObject viewGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewGo.transform.SetParent(scrollGo.transform, false);
            RectTransform viewRt = viewGo.GetComponent<RectTransform>();
            viewRt.anchorMin = Vector2.zero;
            viewRt.anchorMax = Vector2.one;
            viewRt.offsetMin = new Vector2(10, 10);
            viewRt.offsetMax = new Vector2(-10, -10);

            var mask = viewGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            RectTransform contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 20, 40);
            vlg.spacing = 20;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = viewRt;
            sr.content = contentRt;

            // Title Header
            TextMeshProUGUI shopTitle = CreateTMP(contentGo.transform, "ShopHeader", "ROYAL BAZAAR", 32, new Vector2(400, 50));
            shopTitle.fontStyle = FontStyles.Bold;
            shopTitle.color = new Color(1f, 0.85f, 0.25f);
            shopTitle.alignment = TextAlignmentOptions.Center;

            // 1. Limited-Time Offer Section
            TextMeshProUGUI timerTxt = null;
            if (config != null && config.LimitedTimeOffer != null)
            {
                GameObject limitedCardGo = BuildFeaturedCard(contentGo.transform, config.LimitedTimeOffer, out timerTxt, true);
                ShopItemCardUI cardUI = limitedCardGo.GetComponent<ShopItemCardUI>();
                if (cardUI != null) allCards.Add(cardUI);
            }

            // 2. No Ads Section
            if (config != null && config.NoAdsItem != null)
            {
                TextMeshProUGUI dummy;
                GameObject noAdsGo = BuildFeaturedCard(contentGo.transform, config.NoAdsItem, out dummy, false);
                ShopItemCardUI cardUI = noAdsGo.GetComponent<ShopItemCardUI>();
                if (cardUI != null) allCards.Add(cardUI);
            }

            // 3. Gold Packs Section
            if (config != null && config.GoldPacks != null && config.GoldPacks.Count > 0)
            {
                CreateSectionHeader(contentGo.transform, "GOLD PACKS");
                GameObject gridGo = CreateItemGrid(contentGo.transform, "GoldGrid", 2);
                foreach (var pack in config.GoldPacks)
                {
                    GameObject cardGo = BuildGridCard(gridGo.transform, pack);
                    ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>();
                    if (cardUI != null) allCards.Add(cardUI);
                }
            }

            // 4. Skill Packs Section
            if (config != null && config.SkillPacks != null && config.SkillPacks.Count > 0)
            {
                CreateSectionHeader(contentGo.transform, "SKILL PACKS");
                GameObject gridGo = CreateItemGrid(contentGo.transform, "SkillsGrid", 2);
                foreach (var pack in config.SkillPacks)
                {
                    GameObject cardGo = BuildGridCard(gridGo.transform, pack);
                    ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>();
                    if (cardUI != null) allCards.Add(cardUI);
                }
            }

            // 5. Value Bundles Section
            if (config != null && config.Bundles != null && config.Bundles.Count > 0)
            {
                CreateSectionHeader(contentGo.transform, "VALUE BUNDLES");
                GameObject gridGo = CreateItemGrid(contentGo.transform, "BundlesGrid", 1);
                foreach (var bundle in config.Bundles)
                {
                    GameObject cardGo = BuildGridCard(gridGo.transform, bundle);
                    ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>();
                    if (cardUI != null) allCards.Add(cardUI);
                }
            }

            shopUI.BindReferences(config, timerTxt, feedbackDlg, allCards);
            EditorUtility.SetDirty(shopUI);
            return shopUI;
        }

        private static GameObject BuildFeaturedCard(Transform parent, ShopItemData data, out TextMeshProUGUI timerText, bool isCountdown)
        {
            GameObject cardGo = new GameObject(data.Id, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(ShopItemCardUI));
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 150);

            var le = cardGo.GetComponent<LayoutElement>();
            le.minHeight = 150;
            le.preferredHeight = 150;

            var img = cardGo.GetComponent<Image>();
            img.color = new Color(0.15f, 0.18f, 0.28f, 0.95f);

            // Badge
            GameObject badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(cardGo.transform, false);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 1f);
            badgeRt.anchorMax = new Vector2(0f, 1f);
            badgeRt.pivot = new Vector2(0f, 1f);
            badgeRt.anchoredPosition = new Vector2(0, 0);
            badgeRt.sizeDelta = new Vector2(110, 30);
            badgeGo.GetComponent<Image>().color = new Color(0.9f, 0.25f, 0.25f);

            TextMeshProUGUI badgeTmp = CreateTMP(badgeGo.transform, "Text", data.BadgeText, 14, new Vector2(100, 24));
            badgeTmp.fontStyle = FontStyles.Bold;

            // Title
            TextMeshProUGUI titleTmp = CreateTMP(cardGo.transform, "Title", data.Title, 22, new Vector2(240, 32));
            titleTmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, 35);
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = data.ThemeColor;

            // Description
            TextMeshProUGUI descTmp = CreateTMP(cardGo.transform, "Desc", data.Description, 14, new Vector2(250, 30));
            descTmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, 0);
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            descTmp.color = new Color(0.8f, 0.85f, 0.9f);

            // Timer (if limited offer)
            timerText = null;
            if (isCountdown)
            {
                timerText = CreateTMP(cardGo.transform, "Timer", "23:59:59", 16, new Vector2(150, 24));
                timerText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-90, -42);
                timerText.color = new Color(1f, 0.85f, 0.3f);
                timerText.fontStyle = FontStyles.Bold;
            }

            // Buy Button
            GameObject btnGo = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(cardGo.transform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1f, 0.5f);
            btnRt.anchorMax = new Vector2(1f, 0.5f);
            btnRt.pivot = new Vector2(1f, 0.5f);
            btnRt.anchoredPosition = new Vector2(-20, 0);
            btnRt.sizeDelta = new Vector2(130, 50);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.75f, 0.35f);

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            TextMeshProUGUI priceTmp = CreateTMP(btnGo.transform, "PriceText", data.PriceString, 20, new Vector2(120, 40));
            priceTmp.fontStyle = FontStyles.Bold;

            ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>();
            cardUI.Setup(data);
            cardUI.BindReferences(titleTmp, descTmp, priceTmp, btn, null, badgeGo, badgeTmp);
            EditorUtility.SetDirty(cardUI);

            return cardGo;
        }

        private static GameObject BuildGridCard(Transform parent, ShopItemData data)
        {
            GameObject cardGo = new GameObject(data.Id, typeof(RectTransform), typeof(Image), typeof(ShopItemCardUI));
            cardGo.transform.SetParent(parent, false);

            var rt = cardGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(210, 160);

            var img = cardGo.GetComponent<Image>();
            img.color = new Color(0.14f, 0.16f, 0.24f, 0.95f);

            // Optional Badge
            GameObject badgeGo = null;
            TextMeshProUGUI badgeTmp = null;
            if (!string.IsNullOrEmpty(data.BadgeText))
            {
                badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image));
                badgeGo.transform.SetParent(cardGo.transform, false);
                var badgeRt = badgeGo.GetComponent<RectTransform>();
                badgeRt.anchorMin = new Vector2(1f, 1f);
                badgeRt.anchorMax = new Vector2(1f, 1f);
                badgeRt.pivot = new Vector2(1f, 1f);
                badgeRt.anchoredPosition = new Vector2(0, 0);
                badgeRt.sizeDelta = new Vector2(95, 24);
                badgeGo.GetComponent<Image>().color = new Color(0.85f, 0.3f, 0.3f);

                badgeTmp = CreateTMP(badgeGo.transform, "Text", data.BadgeText, 11, new Vector2(90, 20));
                badgeTmp.fontStyle = FontStyles.Bold;
            }

            // Title
            TextMeshProUGUI titleTmp = CreateTMP(cardGo.transform, "Title", data.Title, 16, new Vector2(190, 26));
            titleTmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 45);
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = data.ThemeColor;

            // Description / Quantity
            TextMeshProUGUI descTmp = CreateTMP(cardGo.transform, "Desc", data.Description, 13, new Vector2(190, 24));
            descTmp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 15);
            descTmp.color = new Color(0.8f, 0.85f, 0.9f);

            // Buy Button
            GameObject btnGo = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(cardGo.transform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchoredPosition = new Vector2(0, -42);
            btnRt.sizeDelta = new Vector2(160, 42);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = data.Currency == PriceCurrency.InGameGold ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.2f, 0.72f, 0.35f);

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            TextMeshProUGUI priceTmp = CreateTMP(btnGo.transform, "PriceText", data.PriceString, 16, new Vector2(150, 36));
            priceTmp.fontStyle = FontStyles.Bold;

            ShopItemCardUI cardUI = cardGo.GetComponent<ShopItemCardUI>();
            cardUI.Setup(data);
            cardUI.BindReferences(titleTmp, descTmp, priceTmp, btn, null, badgeGo, badgeTmp);
            EditorUtility.SetDirty(cardUI);

            return cardGo;
        }

        private static GameObject CreateItemGrid(Transform parent, string name, int rowCount = 2)
        {
            GameObject gridGo = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            gridGo.transform.SetParent(parent, false);

            var glg = gridGo.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(210, 160);
            glg.spacing = new Vector2(15, 15);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.childAlignment = TextAnchor.MiddleCenter;

            var le = gridGo.GetComponent<LayoutElement>();
            le.minHeight = rowCount * 160 + (rowCount - 1) * 15;
            le.preferredHeight = le.minHeight;

            return gridGo;
        }

        private static void CreateSectionHeader(Transform parent, string title)
        {
            GameObject headerGo = new GameObject("Header_" + title, typeof(RectTransform), typeof(LayoutElement));
            headerGo.transform.SetParent(parent, false);
            var rt = headerGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(440, 36);

            var le = headerGo.GetComponent<LayoutElement>();
            le.minHeight = 36;
            le.preferredHeight = 36;

            TextMeshProUGUI tmp = CreateTMP(headerGo.transform, "Text", title, 20, new Vector2(440, 36));
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.85f, 0.3f);
        }

        private static PurchaseFeedbackDialog BuildFeedbackDialog(Transform parent)
        {
            Transform existing = parent.Find("PurchaseFeedbackDialog");
            GameObject dlgGo;
            if (existing != null)
            {
                dlgGo = existing.gameObject;
            }
            else
            {
                dlgGo = new GameObject("PurchaseFeedbackDialog", typeof(RectTransform));
                dlgGo.transform.SetParent(parent, false);
            }

            RectTransform rootRt = dlgGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            PurchaseFeedbackDialog pfd = dlgGo.GetComponent<PurchaseFeedbackDialog>() ?? dlgGo.AddComponent<PurchaseFeedbackDialog>();

            // Dark Backdrop
            Transform bgTrans = dlgGo.transform.Find("DarkOverlay");
            if (bgTrans == null)
            {
                var bgGo = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(dlgGo.transform, false);
                var rt = bgGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                bgGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            }

            // Panel
            Transform panelTrans = dlgGo.transform.Find("CenterPanel");
            if (panelTrans == null)
            {
                var pGo = new GameObject("CenterPanel", typeof(RectTransform), typeof(Image));
                pGo.transform.SetParent(dlgGo.transform, false);
                var rt = pGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(440, 300);
                pGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.98f);
                panelTrans = pGo.transform;
            }

            TextMeshProUGUI titleTxt = CreateTMP(panelTrans, "Title", "PURCHASE SUCCESSFUL!", 24, new Vector2(400, 40));
            titleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90);
            titleTxt.fontStyle = FontStyles.Bold;

            TextMeshProUGUI msgTxt = CreateTMP(panelTrans, "Message", "Item credited to your account.", 18, new Vector2(380, 80));
            msgTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
            msgTxt.color = new Color(0.85f, 0.9f, 0.95f);

            // Confirm button
            GameObject btnGo = new GameObject("ConfirmBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panelTrans, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchoredPosition = new Vector2(0, -85);
            btnRt.sizeDelta = new Vector2(200, 48);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.75f, 0.35f);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            CreateTMP(btnGo.transform, "Text", "OK", 20, new Vector2(180, 40)).fontStyle = FontStyles.Bold;

            pfd.BindReferences(dlgGo, titleTxt, msgTxt, null, btn);
            dlgGo.SetActive(false);
            return pfd;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float fontSize, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }
    }
}
