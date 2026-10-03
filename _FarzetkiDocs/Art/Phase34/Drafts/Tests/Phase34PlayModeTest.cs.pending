using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using RoyalSmash.Economy;
using RoyalSmash.Save;
using RoyalSmash.Skills;
using RoyalSmash.UI;
using Object = UnityEngine.Object;

// The production IAPManager still exposes Unity IAP's legacy controller adapter.
#pragma warning disable CS0618

namespace RoyalSmash.EditorTools
{
    /// <summary>
    /// Explicit Phase 34 checks. Presentation never calls a purchase API. Purchase tests are
    /// separately invoked, Editor simulation only, and require an external fresh save manifest.
    /// Stop Play and restore every session JSON byte/hash after either suite.
    /// </summary>
    public static class Phase34PlayModeTest
    {
        private const string Output = "_FarzetkiDocs/Art/Phase34/";
        private const string Prefabs = "Assets/_Project/Prefabs/UI/ShopDynamic/";
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

        [Serializable] public sealed class CheckResult
        {
            public string name, detail;
            public bool passed;
        }

        [Serializable] public sealed class Report
        {
            public string suite, scope, startedUtc, completedUtc, status = "running", backupManifest;
            public int passed, failed;
            public int screenWidth, screenHeight;
            public Rect safeArea;
            public List<CheckResult> checks = new List<CheckResult>();
        }

        public static Report Current { get; private set; }
        public static string Status => Current == null ? "idle" : Current.status;
        private static Action _tick;
        private static Action _cleanup;
        private static double _deadline, _stepAt;
        private static int _stage;

        public static string RunPresentationTests()
        {
            RequirePlay();
            Require(_tick == null, "Another Phase 34 suite is running.");
            Begin("presentation", "Live UI and isolated cloned catalog/presentation. No purchase or fulfillment calls.");
            GameObject sandbox = null;
            ShopConfig catalog = null;
            ShopPresentationConfig presentation = null;
            var iap = IAPManager.Instance;
            bool originalOwned = SaveManager.Instance.Data.NoAdsPurchased;
            try
            {
                var live = LivePresenter();
                var liveCatalog = Field<ShopConfig>(live, "_catalog");
                var livePresentation = Field<ShopPresentationConfig>(live, "_presentation");
                Check("Eight cards resolve the authoritative catalog definitions", () =>
                {
                    Require(live.Cards.Count == 8, "Expected baseline eight cards, found " + live.Cards.Count);
                    Require(live.Cards.Select(c => c.ItemData.Id).Distinct().Count() == 8, "Duplicate visible stable Id.");
                    foreach (var card in live.Cards)
                        Require(ReferenceEquals(card.ItemData, liveCatalog.FindItemById(card.ItemData.Id)), card.name + " uses a copied definition.");
                    Require(liveCatalog.FindItemById("gold_pouch_800").ProductId == "gold_douch_800", "Published SKU changed.");
                });
                Check("Every card has editable TMP title/price/rewards and separate Images", () =>
                {
                    foreach (var card in live.Cards)
                    {
                        Require(Field<TMP_Text>(card, "_titleText") != null, card.name + " missing title.");
                        Require(Field<TMP_Text>(card, "_priceText") != null, card.name + " missing price.");
                        Require(Field<Button>(card, "_buyButton") != null, card.name + " missing buy button.");
                        Require(Field<Image>(card, "_artworkImage") != null, card.name + " missing separate artwork.");
                        VerifyRewards(card);
                    }
                });
                Check("Displayed sprites do not use old baked product panels", () =>
                {
                    string[] banned = { "Card_KingsDeal.png", "Card_NoAds.png", "Section_GoldPacks.png", "Section_BallPacks.png", "Section_ValueBundles.png" };
                    foreach (var image in live.GetComponentsInChildren<Image>(true))
                    {
                        string path = AssetDatabase.GetAssetPath(image.sprite);
                        Require(!banned.Any(b => path.EndsWith("/" + b, StringComparison.Ordinal)), "Baked sprite remains: " + path);
                    }
                });
                Check("Four reusable product prefabs and section header exist", () =>
                {
                    foreach (string name in new[] { "PF_ShopFeatured", "PF_ShopNoAds", "PF_ShopCompact", "PF_ShopBundle" })
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
                        Require(asset != null && asset.GetComponent<ShopItemCardUI>() != null, name + " missing card component.");
                    }
                    Require(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PF_ShopSectionHeader.prefab") != null, "Section header missing.");
                });

                catalog = Object.Instantiate(liveCatalog);
                presentation = Object.Instantiate(livePresentation);
                sandbox = new GameObject("Phase34_PresentationSandbox");
                sandbox.SetActive(false);
                var preview = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PF_DynamicShop_Preview.prefab");
                Require(preview != null, "Dynamic preview prefab missing.");
                var clone = Object.Instantiate(preview, sandbox.transform);
                var presenter = clone.GetComponentInChildren<ShopCatalogPresenter>(true);
                Require(presenter != null, "Preview has no presenter.");
                SetField(presenter, "_catalog", catalog);
                SetField(presenter, "_presentation", presentation);
                SetField(presenter, "_shopUI", null);
                presenter.Rebuild();
                var item = catalog.GoldPacks.First(p => p != null);
                string id = item.Id;
                string originalSku = item.ProductId;
                Check("Cloned product edit updates title, badge and visible reward numbers", () =>
                {
                    item.Title = "Çok Uzun Türkçe Şampiyonların Altın Hazinesi";
                    item.BadgeText = "%37 DENEME";
                    item.GoldReward = 4321;
                    item.MegaBallReward = 7;
                    item.InfinityBallReward = 9;
                    presenter.RefreshCatalog();
                    var card = Card(presenter, id);
                    Require(Field<TMP_Text>(card, "_titleText").text == item.Title, "Title did not update.");
                    Require(Field<TMP_Text>(card, "_badgeText").text == item.BadgeText, "Badge did not update.");
                    VerifyRewards(card);
                    Require(liveCatalog.GoldPacks[0].GoldReward != 4321, "Clone test changed the source catalog.");
                });
                Check("Zero reward rows disappear and skill title has no stale x-count", () =>
                {
                    item.GoldReward = 0;
                    item.InfinityBallReward = 0;
                    presenter.RefreshCatalog();
                    VerifyRewards(Card(presenter, id));
                    var skill = catalog.SkillPacks.First(p => p != null);
                    skill.MegaBallReward = 23;
                    var card = Card(presenter, skill.Id);
                    card.RefreshDisplay();
                    Require(!Field<TMP_Text>(card, "_titleText").text.Contains("(x5)"), "Stale authored x5 remains in title.");
                    VerifyRewards(card);
                });
                Check("Unknown store metadata uses the catalog fallback price", () =>
                {
                    item.ProductId = "phase34_unregistered_metadata";
                    item.PriceString = "₺123,45";
                    var card = Card(presenter, id);
                    card.RefreshDisplay();
                    Require(Field<TMP_Text>(card, "_priceText").text == item.PriceString, "Fallback price not shown.");
                    Require(Field<TMP_Text>(card, "_stateText").text == presentation.FallbackPriceLabel, "Fallback is not identified by the editable offline label.");
                    item.ProductId = originalSku;
                });
                Check("Real store metadata overrides fallback, including another currency", () =>
                {
                    Require(iap.IsInitialized, "Store not initialized; wait for Editor fake store before running this suite.");
                    var controller = Field<IStoreController>(iap, "_storeController");
                    var product = controller.products.WithID(originalSku);
                    Require(product != null && product.metadata != null, "No Editor product metadata.");
                    var property = typeof(ProductMetadata).GetProperty("localizedPriceString");
                    string before = product.metadata.localizedPriceString;
                    try
                    {
                        property.SetValue(product.metadata, "1.234,56 €");
                        var card = Card(presenter, id);
                        card.RefreshDisplay();
                        Require(Field<TMP_Text>(card, "_priceText").text == "1.234,56 €", "Metadata did not override fallback.");
                        Require(string.IsNullOrEmpty(Field<TMP_Text>(card, "_stateText").text), "Store price is still marked offline.");
                    }
                    finally { property.SetValue(product.metadata, before); }
                });
                Check("Gold currency never displays store metadata", () =>
                {
                    item.Currency = PriceCurrency.InGameGold;
                    item.CostInGold = 9876;
                    var card = Card(presenter, id);
                    card.RefreshDisplay();
                    Require(Field<TMP_Text>(card, "_priceText").text.Contains(item.CostInGold.ToString("N0", CultureInfo.CurrentCulture)), "Gold cost not shown.");
                    item.Currency = PriceCurrency.RealMoneyIAP;
                });
                Check("Added products appear without editing the scene", () =>
                {
                    catalog.GoldPacks.Add(new ShopItemData { Id = "phase34_extra_gold", ProductId = "phase34_extra_sku", Title = "Yeni Altın", GoldReward = 77 });
                    presenter.RefreshCatalog();
                    Require(presenter.Cards.Count == 9, "New product did not create a ninth card.");
                    Require(Card(presenter, "phase34_extra_gold").ItemData.GoldReward == 77, "New card bound to wrong definition.");
                });
                Check("Catalog order controls rendered card order", () =>
                {
                    catalog.GoldPacks.Reverse();
                    presenter.RefreshCatalog();
                    var order = presenter.Cards.Where(c => catalog.GoldPacks.Contains(c.ItemData)).Select(c => c.ItemData.Id).ToArray();
                    Require(order.SequenceEqual(catalog.GoldPacks.Where(p => p != null).Select(p => p.Id)), "Gold display order ignored catalog order.");
                });
                Check("Removed product becomes unpurchasable then disappears", () =>
                {
                    var card = Card(presenter, id);
                    catalog.GoldPacks.Remove(item);
                    card.RefreshDisplay();
                    Require(card.ItemData == null && !Field<Button>(card, "_buyButton").interactable, "Removed binding still purchasable.");
                    presenter.RefreshCatalog();
                    Require(!presenter.Cards.Any(c => c.ItemData != null && c.ItemData.Id == id), "Removed product still rendered.");
                });
                Check("Null list entries are ignored by the presenter", () =>
                {
                    int before = presenter.Cards.Count;
                    catalog.GoldPacks.Add(null);
                    presenter.RefreshCatalog();
                    Require(presenter.Cards.Count == before, "Null entry changed card count.");
                });
                Check("Presentation title and reward labels update independently of economics", () =>
                {
                    presentation.ShopTitle = "DENEME PAZARI";
                    presentation.GoldRewardLabel = "Düzenlenen Altın";
                    presenter.RefreshCatalog();
                    Require(Field<TMP_Text>(presenter, "_shopTitle").text == presentation.ShopTitle, "Shop title not editable.");
                    var goldCard = Card(presenter, "phase34_extra_gold");
                    Require(Field<TMP_Text>(goldCard, "_goldRewardText").text.Contains(presentation.GoldRewardLabel), "Reward label not editable.");
                    Require(goldCard.ItemData.GoldReward == 77, "Presentation changed economics.");
                });
                Check("Section order and titles are live presentation fields", () =>
                {
                    presentation.SectionOrder = new List<ShopSectionKind> { ShopSectionKind.Bundles, ShopSectionKind.Gold, ShopSectionKind.Skills, ShopSectionKind.Featured, ShopSectionKind.NoAds };
                    presentation.BundleSectionTitle = "DÜZENLENEN PAKETLER";
                    presenter.RefreshCatalog();
                    Require(presenter.Cards[0].ItemData.Id == catalog.Bundles[0].Id, "Section order did not move bundles to the front.");
                    Require(presenter.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == presentation.BundleSectionTitle), "Edited section header not visible.");
                });
                Check("Shared skin change reaches each corresponding card", () =>
                {
                    Require(presentation.FeaturedBackground != null, "Featured skin missing.");
                    presentation.CompactBackground = presentation.FeaturedBackground;
                    presenter.RefreshCatalog();
                    foreach (var card in presenter.Cards.Where(c => c.ItemData.ItemType == ShopItemType.GoldPack || c.ItemData.ItemType == ShopItemType.SkillPack))
                    {
                        var artwork = presentation.FindArtwork(card.ItemData.Id);
                        if (artwork != null && artwork.BackgroundOverride != null) continue;
                        Require(Field<Image>(card, "_backgroundImage").sprite == presentation.FeaturedBackground, "Shared skin did not update " + card.ItemData.Id);
                    }
                });
                Check("Owned No Ads is disabled and shows the presentation label", () =>
                {
                    SaveManager.Instance.Data.NoAdsPurchased = true;
                    var card = Card(presenter, catalog.NoAdsItem.Id);
                    card.RefreshDisplay();
                    Require(!Field<Button>(card, "_buyButton").interactable, "Owned No Ads is enabled.");
                    Require(Field<TMP_Text>(card, "_priceText").text == presentation.OwnedLabel, "Owned wording ignores presentation.");
                    SaveManager.Instance.Data.NoAdsPurchased = originalOwned;
                });
                Check("Global pending lock disables every live card and unlocks safely", () =>
                {
                    SetPending(iap, true);
                    foreach (var card in live.Cards)
                    {
                        card.RefreshDisplay();
                        Require(!Field<Button>(card, "_buyButton").interactable, "Card remained enabled while another item was pending.");
                        if (!(card.ItemData.GrantsNoAds && originalOwned)) Require(Field<TMP_Text>(card, "_stateText").text == livePresentation.PendingLabel, "Pending label missing.");
                    }
                    SetPending(iap, false);
                    foreach (var card in live.Cards)
                    {
                        card.RefreshDisplay();
                        Require(Field<Button>(card, "_buyButton").interactable == !(card.ItemData.GrantsNoAds && originalOwned), "Unlock or owned state incorrect.");
                    }
                });
            }
            catch (Exception ex) { Record("Suite setup", false, ex.ToString()); }
            finally
            {
                SaveManager.Instance.Data.NoAdsPurchased = originalOwned;
                if (iap != null && iap.IsPurchaseInProgress) SetPending(iap, false);
                if (sandbox != null) Object.DestroyImmediate(sandbox);
                if (catalog != null) Object.DestroyImmediate(catalog);
                if (presentation != null) Object.DestroyImmediate(presentation);
                Complete();
            }
            return JsonUtility.ToJson(Current, true);
        }

        public static string RunResponsiveLayoutTests(string captureLabel)
        {
            RequirePlay();
            Require(_tick == null, "Another suite is running.");
            Require(!string.IsNullOrEmpty(captureLabel) && captureLabel.All(c => char.IsLetterOrDigit(c) || c == '-'), "Use a simple capture label, e.g. 1080x1920.");
            var live = LivePresenter();
            Require(live.gameObject.activeInHierarchy, "Open Shop before responsive inspection.");
            var scroll = live.GetComponentInChildren<ScrollRect>(true);
            Require(scroll != null && scroll.content != null && scroll.viewport != null, "Dynamic scroll references missing.");
            Begin("layout-" + captureLabel, "Current GameView layout metrics. Capture and visually inspect this exact screen separately; this test does not claim an image was reviewed.");
            Current.screenWidth = Screen.width;
            Current.screenHeight = Screen.height;
            Current.safeArea = Screen.safeArea;
            float originalScroll = scroll.verticalNormalizedPosition;
            try
            {
                Canvas.ForceUpdateCanvases();
                Check("Scroll viewport stays inside the current screen safe area", () =>
                {
                    var viewport = ScreenRect(scroll.viewport);
                    Require(ContainsRect(Screen.safeArea, viewport, 2), "Viewport outside safe area: " + viewport + " / " + Screen.safeArea);
                    Require(scroll.vertical && !scroll.horizontal, "Shop scroll axes are incorrect.");
                });
                Check("All purchase targets meet the shared 96-unit minimum", () =>
                {
                    foreach (var card in live.Cards)
                    {
                        Rect rect = ((RectTransform)Field<Button>(card, "_buyButton").transform).rect;
                        Require(rect.width >= 95.9f && rect.height >= 95.9f, card.ItemData.Id + " buy target is " + rect.size);
                    }
                });
                Check("Active dynamic text fits its own TMP bounds", () =>
                {
                    foreach (var text in live.GetComponentsInChildren<TMP_Text>(false))
                    {
                        if (string.IsNullOrWhiteSpace(text.text)) continue;
                        text.ForceMeshUpdate(true, true);
                        Require(!text.isTextOverflowing, text.name + " overflows: " + text.text);
                    }
                });
                Check("Reward rows stay in their own group and clear purchase/status text", () =>
                {
                    foreach (var card in live.Cards) VerifyRewardLayout(card);
                });
                Check("First product is reachable at top of scroll", () =>
                {
                    scroll.verticalNormalizedPosition = 1;
                    Canvas.ForceUpdateCanvases();
                    var viewport = ScreenRect(scroll.viewport);
                    var first = ScreenRect((RectTransform)live.Cards[0].transform);
                    Require(ContainsRect(viewport, first, 3), "First card clipped at scroll top: " + first + " / " + viewport);
                });
                Check("Final product is reachable at bottom of scroll", () =>
                {
                    scroll.verticalNormalizedPosition = 0;
                    Canvas.ForceUpdateCanvases();
                    var viewport = ScreenRect(scroll.viewport);
                    var last = ScreenRect((RectTransform)live.Cards[live.Cards.Count - 1].transform);
                    Require(ContainsRect(viewport, last, 3), "Last card clipped at scroll bottom: " + last + " / " + viewport);
                });
                Check("Every card remains inside the content width", () =>
                {
                    Rect content = ScreenRect(scroll.content);
                    foreach (var card in live.Cards)
                    {
                        Rect rect = ScreenRect((RectTransform)card.transform);
                        Require(rect.xMin >= content.xMin - 3 && rect.xMax <= content.xMax + 3, card.ItemData.Id + " escapes the column width.");
                    }
                });
            }
            finally
            {
                scroll.verticalNormalizedPosition = originalScroll;
                Canvas.ForceUpdateCanvases();
                Complete();
            }
            return JsonUtility.ToJson(Current, true);
        }

        /// <summary>
        /// Measures an active independent preview with long Turkish wording, large
        /// live amounts and another currency. Its clones are destroyed before return;
        /// root captures the reviewed production screen separately.
        /// </summary>
        public static string RunEditedTextLayoutTests(string captureLabel)
        {
            RequirePlay();
            Require(_tick == null, "Another suite is running.");
            Require(!string.IsNullOrEmpty(captureLabel) && captureLabel.All(c => char.IsLetterOrDigit(c) || c == '-'), "Use a simple capture label.");
            Begin("edited-layout-" + captureLabel, "Active cloned preview geometry with long Turkish titles, large catalog quantities and euro fallback. No purchases or source-asset edits; separate screenshots still required.");
            Current.screenWidth = Screen.width;
            Current.screenHeight = Screen.height;
            Current.safeArea = Screen.safeArea;
            GameObject sandbox = null;
            ShopConfig catalog = null;
            ShopPresentationConfig presentation = null;
            try
            {
                ShopCatalogPresenter live = LivePresenter();
                catalog = Object.Instantiate(Field<ShopConfig>(live, "_catalog"));
                presentation = Object.Instantiate(Field<ShopPresentationConfig>(live, "_presentation"));
                foreach (ShopItemData item in catalog.GetAllItems())
                {
                    if (item == null) continue;
                    item.Title = "Çok Uzun Türkçe Şampiyonların Altın Hazinesi";
                    item.ProductId = "phase34_layout_metadata_" + item.Id;
                    item.PriceString = "1.234,56 €";
                    // Exercise larger quantities across the existing product families.
                    if (item.GoldReward > 0) item.GoldReward = 9876543;
                    if (item.MegaBallReward > 0) item.MegaBallReward = 4321;
                    if (item.InfinityBallReward > 0) item.InfinityBallReward = 8765;
                }
                ShopItemData editedGold = catalog.GoldPacks.First(item => item != null);
                editedGold.GoldReward = 9876543;
                editedGold.MegaBallReward = 4321;
                editedGold.InfinityBallReward = 8765;
                presentation.ShopTitle = "KRALLIĞIN DÜZENLENEN ROYAL BAZAR MAĞAZASI";
                sandbox = new GameObject("Phase34_EditedLayoutSandbox");
                sandbox.SetActive(false);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PF_DynamicShop_Preview.prefab");
                Require(prefab != null, "Dynamic preview prefab missing.");
                GameObject preview = Object.Instantiate(prefab, sandbox.transform);
                ShopCatalogPresenter presenter = preview.GetComponentInChildren<ShopCatalogPresenter>(true);
                Require(presenter != null, "Preview presenter missing.");
                SetField(presenter, "_catalog", catalog);
                SetField(presenter, "_presentation", presentation);
                SetField(presenter, "_shopUI", null);
                presenter.Rebuild();
                sandbox.SetActive(true);
                Canvas.ForceUpdateCanvases();
                Check("Long titles and another currency use visible TMP bounds", () =>
                {
                    foreach (TMP_Text text in presenter.GetComponentsInChildren<TMP_Text>(false))
                    {
                        if (string.IsNullOrWhiteSpace(text.text)) continue;
                        text.ForceMeshUpdate(true, true);
                        Require(!text.isTextOverflowing, text.name + " overflows edited text: " + text.text);
                    }
                    foreach (ShopItemCardUI card in presenter.Cards)
                    {
                        Require(Field<TMP_Text>(card, "_titleText").text == card.ItemData.Title, "Long title did not reach card.");
                        if (!card.ItemData.GrantsNoAds || !SaveManager.Instance.Data.NoAdsPurchased)
                            Require(Field<TMP_Text>(card, "_priceText").text == "1.234,56 €", "Alternate currency did not reach card.");
                    }
                });
                Check("Large reward quantities fit separate rows without purchase/status overlap", () =>
                {
                    foreach (ShopItemCardUI card in presenter.Cards)
                    {
                        VerifyRewards(card);
                        VerifyRewardLayout(card);
                    }
                    Require(((RectTransform)Card(presenter, editedGold.Id).transform).rect.height >= 367.9f,
                        "Compact height did not grow for three reward rows.");
                    Require(Field<ShopConfig>(live, "_catalog").GoldPacks[0].GoldReward != editedGold.GoldReward,
                        "Edited layout changed source catalog economics.");
                });
                Check("Reward amount refresh retains cards; a fourth reward rebuilds and grows the row", () =>
                {
                    ShopItemCardUI before = Card(presenter, editedGold.Id);
                    editedGold.GoldReward++;
                    presenter.RefreshCatalog();
                    Require(ReferenceEquals(before, Card(presenter, editedGold.Id)), "A quantity-only edit rebuilt the catalog.");
                    editedGold.GrantsNoAds = true;
                    presenter.RefreshCatalog();
                    Canvas.ForceUpdateCanvases();
                    ShopItemCardUI after = Card(presenter, editedGold.Id);
                    Require(!ReferenceEquals(before, after), "Reward composition change did not rebuild the catalog.");
                    Require(((RectTransform)after.transform).rect.height >= 405.9f, "Compact height did not grow for four reward rows.");
                    VerifyRewards(after);
                    VerifyRewardLayout(after);
                    Require(!Field<TMP_Text>(after, "_descriptionText").gameObject.activeSelf,
                        "Mixed reward card reactivated legacy prose over its four reward rows.");
                });
            }
            catch (Exception ex) { Record("Edited layout setup", false, ex.ToString()); }
            finally
            {
                if (sandbox != null) Object.DestroyImmediate(sandbox);
                if (catalog != null) Object.DestroyImmediate(catalog);
                if (presentation != null) Object.DestroyImmediate(presentation);
                Canvas.ForceUpdateCanvases();
                Complete();
            }
            return JsonUtility.ToJson(Current, true);
        }

        private static Rect ScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var canvas = rect.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            var b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
        }

        private static bool ContainsRect(Rect outer, Rect inner, float tolerance) => inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance && inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;

        public static string BeginLifecycleTests()
        {
            RequirePlay();
            Require(_tick == null, "Another suite is running.");
            var nav = Object.FindFirstObjectByType<BottomNavigationUI>();
            var live = LivePresenter();
            var shop = live.GetComponentInParent<ShopUI>();
            if (shop == null) shop = Object.FindFirstObjectByType<ShopUI>(FindObjectsInactive.Include);
            Require(nav != null && shop != null, "MainMenu navigation/shop unavailable.");
            var close = Field<Button>(shop, "_closeButton");
            Require(close != null, "Close button not bound.");
            var originalTab = nav.CurrentTab;
            float originalTime = shop.TimeRemaining;
            Begin("lifecycle", "Real navigation, close button and countdown over multiple frames. No purchase calls.");
            _cleanup = () => { shop.TimeRemaining = originalTime; nav.SwitchTab(originalTab); };
            nav.SwitchTab(NavigationTab.Shop);
            shop.TimeRemaining = 120f;
            _stage = 0;
            _stepAt = EditorApplication.timeSinceStartup + .6;
            _deadline = EditorApplication.timeSinceStartup + 15;
            _tick = () =>
            {
                if (EditorApplication.timeSinceStartup < _stepAt) return;
                if (_stage == 0)
                {
                    Require(shop.TimeRemaining < 119.8f && shop.TimeRemaining > 115f, "Initial countdown not ticking once per frame.");
                    Record("Countdown runs on initial shop opening", true);
                    close.onClick.Invoke();
                    Require(nav.CurrentTab == NavigationTab.Battle && !shop.gameObject.activeInHierarchy, "Close does not return to Battle.");
                    Record("Close button returns to Battle", true);
                    nav.SwitchTab(NavigationTab.Leaderboard);
                    Require(!shop.gameObject.activeInHierarchy, "Shop remains active on Leaderboard.");
                    Record("Leaderboard navigation hides Shop", true);
                    nav.SwitchTab(NavigationTab.Shop);
                    shop.TimeRemaining = 90f;
                    _stage = 1;
                    _stepAt = EditorApplication.timeSinceStartup + .7;
                }
                else if (_stage == 1)
                {
                    Require(shop.TimeRemaining < 89.8f && shop.TimeRemaining > 85f, "Countdown failed to resume after reopen.");
                    Require(live.LimitedOfferTimerText != null && live.LimitedOfferTimerText.text.Contains(":"), "Reopened timer has no text.");
                    Record("Countdown resumes after close/reopen", true);
                    for (int i = 0; i < 4; i++) { nav.SwitchTab(NavigationTab.Battle); nav.SwitchTab(NavigationTab.Shop); }
                    shop.TimeRemaining = 60f;
                    _stage = 2;
                    _stepAt = EditorApplication.timeSinceStartup + .7;
                }
                else
                {
                    Require(shop.TimeRemaining < 59.8f && shop.TimeRemaining > 58.2f, "Repeated reopening stopped or multiplied the countdown.");
                    Record("Repeated reopening keeps one countdown", true);
                    Complete();
                }
            };
            EditorApplication.update += Tick;
            return "Lifecycle running; inspect Phase34PlayModeTest.Status and Current.";
        }

        public static string BeginEditorPurchaseRegression(string freshBackupManifestPath)
        {
            RequirePlay();
            Require(_tick == null, "Another suite is running.");
            Require(!string.IsNullOrWhiteSpace(freshBackupManifestPath) && File.Exists(freshBackupManifestPath), "Fresh session save backup manifest must exist before purchase simulation.");
            var iap = IAPManager.Instance;
            var save = SaveManager.Instance;
            var live = LivePresenter();
            Require(live.gameObject.activeInHierarchy, "Open the Shop before purchase regression.");
            Require(live.Cards.Count == 8, "Expected eight migrated cards.");
            Require(GoldManager.HasInstance && SkillManager.HasInstance, "Existing gold and skill managers must be ready before purchase simulation.");
            // Validate cleanup reflection before changing any flags or save fields.
            Field<int>(GoldManager.Instance, "_currentGold");
            Field<int>(SkillManager.Instance, "_megaBallUses");
            Field<int>(SkillManager.Instance, "_infinityBallUses");
            var cards = live.Cards.ToArray();
            bool originalFakeUI = iap.UseFakeStoreUIInEditor;
            bool originalFailure = iap.SimulateNextPurchaseFailure;
            var originalBalance = Balance.Capture();
            var originalSaveBalance = Balance.CaptureSaveData();
            bool originalOwned = save.Data.NoAdsPurchased;
            int completions = 0, failures = 0;
            ShopItemData completedItem = null;
            Action<ShopItemData> success = item => { completions++; completedItem = item; };
            Action<string> failure = _ => failures++;
            Begin("purchase", "Editor simulated transactions via eight real buy buttons, lock, owned and failure. No real payment; does not claim duplicate store-callback idempotency.");
            Current.backupManifest = Path.GetFullPath(freshBackupManifestPath);
            iap.UseFakeStoreUIInEditor = false;
            iap.SimulateNextPurchaseFailure = false;
            save.Data.NoAdsPurchased = false;
            iap.OnPurchaseCompleted += success;
            iap.OnPurchaseFailed += failure;
            _cleanup = () =>
            {
                iap.OnPurchaseCompleted -= success;
                iap.OnPurchaseFailed -= failure;
                // The suite requires an idle manager before starting and owns its
                // simulated transaction. Do not let a failed assertion leave it running.
                if (iap.IsPurchaseInProgress) { iap.StopAllCoroutines(); SetPending(iap, false); }
                iap.UseFakeStoreUIInEditor = originalFakeUI;
                iap.SimulateNextPurchaseFailure = originalFailure;
                originalBalance.RestoreInMemory();
                originalSaveBalance.RestoreSaveData();
                save.Data.NoAdsPurchased = originalOwned;
                foreach (var card in live.Cards) card.RefreshDisplay();
                foreach (var display in Object.FindObjectsByType<GoldDisplayUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    display.RefreshDisplay();
                var dialog = Object.FindFirstObjectByType<PurchaseFeedbackDialog>(FindObjectsInactive.Include);
                if (dialog != null) dialog.Hide();
            };
            _stage = 0;
            _deadline = EditorApplication.timeSinceStartup + 30;
            Balance before = Balance.Capture();
            Action beginOne = () =>
            {
                completions = failures = 0;
                completedItem = null;
                before = Balance.Capture();
                var card = cards[_stage];
                card.RefreshDisplay();
                Require(Field<Button>(card, "_buyButton").interactable, "Buy button unexpectedly disabled for " + card.ItemData.Id);
                Field<Button>(card, "_buyButton").onClick.Invoke();
                Require(iap.IsPurchaseInProgress, "Button did not initiate a pending Editor transaction.");
                Require(cards.All(c => !Field<Button>(c, "_buyButton").interactable), "Other card remained enabled during transaction.");
                if (_stage == 0)
                {
                    Require(!iap.PurchaseItem(card.ItemData), "Concurrent request bypassed lock.");
                    Record("Second simultaneous purchase is rejected", true, "Expected IAP guard warning is recorded by the Console.");
                }
                _stepAt = EditorApplication.timeSinceStartup + .4;
            };
            try { beginOne(); }
            catch (Exception ex) { Record("Purchase setup", false, ex.ToString()); Complete(); return JsonUtility.ToJson(Current, true); }
            _tick = () =>
            {
                if (EditorApplication.timeSinceStartup < _stepAt || iap.IsPurchaseInProgress) return;
                if (_stage < cards.Length)
                {
                    var item = cards[_stage].ItemData;
                    Require(completions == 1 && failures == 0 && ReferenceEquals(completedItem, item), "Wrong or repeated completion event for " + item.Id);
                    Require(Balance.Capture().MatchesRewards(before, item), "Reward delta differs from catalog or fulfillment repeated: " + item.Id);
                    Require(!item.GrantsNoAds || save.Data.NoAdsPurchased, "No Ads grant missing.");
                    Record("Single correct fulfillment: " + item.Id, true, item.ProductId);
                    _stage++;
                    if (_stage < cards.Length) { beginOne(); return; }
                    var owned = cards.First(c => c.ItemData.GrantsNoAds);
                    owned.RefreshDisplay();
                    Require(!Field<Button>(owned, "_buyButton").interactable && !iap.PurchaseItem(owned.ItemData), "Owned No Ads can be bought again.");
                    Record("Owned No Ads cannot be repurchased", true);
                    before = Balance.Capture();
                    completions = failures = 0;
                    iap.SimulateNextPurchaseFailure = true;
                    var failCard = cards.First(c => !c.ItemData.GrantsNoAds);
                    failCard.RefreshDisplay();
                    Field<Button>(failCard, "_buyButton").onClick.Invoke();
                    Require(iap.IsPurchaseInProgress, "Failure simulation did not start.");
                    _stepAt = EditorApplication.timeSinceStartup + .4;
                    return;
                }
                Require(completions == 0 && failures == 1 && Balance.Capture().EqualsBalance(before), "Failed transaction changed rewards or callbacks.");
                Require(cards.Where(c => !c.ItemData.GrantsNoAds).All(c => Field<Button>(c, "_buyButton").interactable), "Failed transaction left cards locked.");
                Record("Simulated failure grants nothing and releases all purchase locks", true, "Expected simulated-error warning is recorded by the Console.");
                var dialog = Object.FindFirstObjectByType<PurchaseFeedbackDialog>(FindObjectsInactive.Include);
                Require(dialog != null && Field<GameObject>(dialog, "_contentRoot").activeInHierarchy, "Failure dialog not visible.");
                Require(!string.IsNullOrWhiteSpace(Field<TMP_Text>(dialog, "_messageText").text), "Failure dialog has no message.");
                Record("Failure feedback is visible and readable", true);
                Complete();
            };
            EditorApplication.update += Tick;
            return "Editor purchase simulation running; inspect Phase34PlayModeTest.Status and Current. Restore session JSON hashes after stopping Play.";
        }

        private struct Balance
        {
            public int gold, mega, infinity;
            public static Balance Capture() => new Balance
            {
                gold = GoldManager.Instance.CurrentGold,
                mega = SkillManager.Instance.GetRemainingUses(SkillType.MegaBall),
                infinity = SkillManager.Instance.GetRemainingUses(SkillType.InfinityBall)
            };
            public static Balance CaptureSaveData() => new Balance
            {
                gold = SaveManager.Instance.Data.Gold,
                mega = SaveManager.Instance.Data.MegaBallUses,
                infinity = SaveManager.Instance.Data.InfinityBallUses
            };
            public bool EqualsBalance(Balance other) => gold == other.gold && mega == other.mega && infinity == other.infinity;
            public bool MatchesRewards(Balance before, ShopItemData item) => gold == before.gold + item.GoldReward && mega == before.mega + item.MegaBallReward && infinity == before.infinity + item.InfinityBallReward;
            public void RestoreInMemory()
            {
                SetField(GoldManager.Instance, "_currentGold", gold);
                SetField(SkillManager.Instance, "_megaBallUses", mega);
                SetField(SkillManager.Instance, "_infinityBallUses", infinity);
                RestoreSaveData();
            }
            public void RestoreSaveData()
            {
                var data = SaveManager.Instance.Data;
                data.Gold = gold; data.MegaBallUses = mega; data.InfinityBallUses = infinity;
            }
        }

        private static void RequirePlay()
        {
            Require(Application.isEditor && Application.isPlaying, "Run in Editor Play Mode after fresh external save backup.");
            Require(IAPManager.HasInstance && !IAPManager.Instance.IsPurchaseInProgress, "Wait for idle IAP manager.");
            Require(SaveManager.HasInstance && SaveManager.Instance.Data != null, "SaveManager is not ready.");
        }
        private static ShopCatalogPresenter LivePresenter()
        {
            ShopCatalogPresenter presenter = Object.FindObjectsByType<ShopCatalogPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(p => p.gameObject.scene.path == Phase34DynamicShopBuilder.MainMenuPath && p.GetComponentInParent<ShopUI>(true) != null);
            Require(presenter != null, "The migrated MainMenu ShopPage presenter was not found.");
            return presenter;
        }
        private static ShopItemCardUI Card(ShopCatalogPresenter presenter, string id) => presenter.Cards.First(c => c.ItemData != null && c.ItemData.Id == id);
        private static T Field<T>(object target, string name) => (T)RequiredField(target, name).GetValue(target);
        private static void SetField(object target, string name, object value) => RequiredField(target, name).SetValue(target, value);
        private static FieldInfo RequiredField(object target, string name)
        {
            Require(target != null, "Cannot inspect field on a null object: " + name);
            FieldInfo field = target.GetType().GetField(name, Private);
            Require(field != null, target.GetType().Name + " is missing test field " + name);
            return field;
        }
        private static void SetPending(IAPManager iap, bool value) => typeof(IAPManager).GetMethod("SetPurchaseInProgress", Private).Invoke(iap, new object[] { value });
        private static void VerifyRewards(ShopItemCardUI card)
        {
            var item = card.ItemData;
            VerifyRow(card, "_goldRewardText", item.GoldReward);
            VerifyRow(card, "_megaRewardText", item.MegaBallReward);
            VerifyRow(card, "_infinityRewardText", item.InfinityBallReward);
            var noAds = Field<TMP_Text>(card, "_noAdsRewardText");
            Require(noAds != null && noAds.transform.parent.gameObject.activeSelf == item.GrantsNoAds, card.name + " No Ads reward row state wrong.");
        }
        private static void VerifyRow(ShopItemCardUI card, string field, int amount)
        {
            var text = Field<TMP_Text>(card, field);
            Require(text != null, card.name + " missing " + field);
            Require(text.transform.parent.gameObject.activeSelf == (amount > 0), card.name + " reward row visibility wrong: " + field);
            if (amount > 0) Require(text.text.Contains(amount.ToString("N0", CultureInfo.CurrentCulture)), card.name + " reward text differs from catalog: " + text.text);
        }
        private static void VerifyRewardLayout(ShopItemCardUI card)
        {
            Rect buy = ScreenRect((RectTransform)Field<Button>(card, "_buyButton").transform);
            TMP_Text state = Field<TMP_Text>(card, "_stateText");
            foreach (string field in new[] { "_goldRewardText", "_megaRewardText", "_infinityRewardText", "_noAdsRewardText" })
            {
                TMP_Text text = Field<TMP_Text>(card, field);
                Require(text != null && text.transform.parent != null && text.transform.parent != card.transform,
                    card.name + " reward lacks a dedicated row parent: " + field);
                Transform row = text.transform.parent;
                Require(row.parent != null && row.parent.GetComponent<VerticalLayoutGroup>() != null,
                    card.name + " reward row is outside the shared reward layout: " + field);
                if (!row.gameObject.activeInHierarchy) continue;
                Rect rect = ScreenRect((RectTransform)row);
                Require(ContainsRect(ScreenRect((RectTransform)row.parent), rect, 2), card.name + " reward escapes its group: " + field);
                Require(!OverlapsInterior(rect, buy, 1), card.name + " reward overlaps BuyButton: " + field);
                if (state != null && state.gameObject.activeInHierarchy)
                    Require(!OverlapsInterior(rect, ScreenRect(state.rectTransform), 1), card.name + " reward overlaps StateText: " + field);
            }
        }
        private static bool OverlapsInterior(Rect a, Rect b, float tolerance) =>
            Math.Min(a.xMax, b.xMax) - Math.Max(a.xMin, b.xMin) > tolerance &&
            Math.Min(a.yMax, b.yMax) - Math.Max(a.yMin, b.yMin) > tolerance;
        private static void Begin(string suite, string scope)
        {
            Current = new Report { suite = suite, scope = scope, startedUtc = DateTime.UtcNow.ToString("O") };
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Check(string name, Action action)
        {
            try { action(); Record(name, true); }
            catch (Exception ex) { Record(name, false, ex.ToString()); }
        }
        private static void Record(string name, bool passed, string detail = null)
        {
            Current.checks.Add(new CheckResult { name = name, passed = passed, detail = detail });
            if (passed) Current.passed++; else Current.failed++;
        }
        private static void Tick()
        {
            try
            {
                Require(Application.isPlaying, "Play Mode ended before suite completion.");
                Require(EditorApplication.timeSinceStartup < _deadline, "Suite timed out.");
                _tick?.Invoke();
            }
            catch (Exception ex) { Record("Asynchronous suite", false, ex.ToString()); Complete(); }
        }
        private static void Complete()
        {
            EditorApplication.update -= Tick;
            _tick = null;
            try { _cleanup?.Invoke(); }
            catch (Exception ex) { Record("Cleanup", false, ex.ToString()); }
            finally { _cleanup = null; }
            Current.completedUtc = DateTime.UtcNow.ToString("O");
            Current.status = Current.failed == 0 ? "passed" : "failed";
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "34-test-" + Current.suite + ".json", JsonUtility.ToJson(Current, true));
            Debug.Log("[Phase34] " + Current.suite + ": " + Current.passed + " passed, " + Current.failed + " failed. External session JSON restore still required.");
        }
    }
}
