#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RoyalSmash.Economy;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    /// <summary>
    /// Explicit Phase 34 authoring entry points. Never runs on import, enters Play Mode,
    /// opens MainMenu, saves a scene, changes catalog economics, or touches player saves.
    /// Art must already be imported and reviewed before calling BuildPrefabAssets().
    /// MainMenu migration is a separate operation after the preview has passed visual QA.
    /// </summary>
    public static class Phase34DynamicShopBuilder
    {
        public const string Folder = "Assets/_Project/Prefabs/UI/ShopDynamic";
        public const string FeaturedPath = Folder + "/PF_ShopFeatured.prefab";
        public const string NoAdsPath = Folder + "/PF_ShopNoAds.prefab";
        public const string CompactPath = Folder + "/PF_ShopCompact.prefab";
        public const string BundlePath = Folder + "/PF_ShopBundle.prefab";
        public const string SectionPath = Folder + "/PF_ShopSectionHeader.prefab";
        public const string PanelPath = Folder + "/PF_DynamicShopPanel.prefab";
        public const string PreviewPath = Folder + "/PF_DynamicShop_Preview.prefab";
        public const string PresentationPath = "Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset";
        public const string CatalogPath = "Assets/_Project/ScriptableObjects/Config/ShopConfig.asset";
        public const string MainMenuPath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string CloseSpritePath = "Assets/_Project/Art/Sprites/UI/Shop/Button_Close.png";

        [Serializable]
        public sealed class BuildReport
        {
            public string capturedUtc;
            public string[] prefabPaths;
            public int cardCount;
            public bool catalogUnchanged;
            public bool sceneSaved;
        }

        [Serializable]
        public sealed class MigrationReport
        {
            public string capturedUtc;
            public bool changed;
            public bool sceneWasDirty;
            public bool sceneSaved;
            public bool catalogUnchanged;
            public int cardCount;
            public string[] itemIds;
            public string replacedRoot;
        }

        private enum CardKind { Featured, NoAds, Compact, Bundle }

        /// <summary>
        /// Initial legacy authoring only: writes seven explicit prefab assets without
        /// saving a scene. Existing horizontal revision assets must be maintained with
        /// Phase34ShopVisualRevision.ApplyPrefabVisuals instead of regenerating them.
        /// </summary>
        public static BuildReport BuildPrefabAssets()
        {
            RequireEditMode();
            RequireUnrevisedCardPrefabs();
            ShopConfig catalog = RequireAsset<ShopConfig>(CatalogPath);
            ShopPresentationConfig presentation = RequireAsset<ShopPresentationConfig>(PresentationPath);
            ValidatePresentation(catalog, presentation);
            string catalogBefore = EditorJsonUtility.ToJson(catalog, false);
            EnsureFolder(Folder);
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                SaveCard(previewScene, catalog, presentation, CardKind.Featured, FeaturedPath, catalog.LimitedTimeOffer);
                SaveCard(previewScene, catalog, presentation, CardKind.NoAds, NoAdsPath, catalog.NoAdsItem);
                SaveCard(previewScene, catalog, presentation, CardKind.Compact, CompactPath, FirstProduct(catalog.GoldPacks));
                SaveCard(previewScene, catalog, presentation, CardKind.Bundle, BundlePath, FirstProduct(catalog.Bundles));

                GameObject section = CreateSection(previewScene, presentation);
                SavePrefab(section, SectionPath);
                UnityEngine.Object.DestroyImmediate(section);

                GameObject panel = CreatePanel(previewScene, catalog, presentation);
                ShopCatalogPresenter presenter = panel.GetComponent<ShopCatalogPresenter>();
                presenter.Rebuild();
                ValidateCards(presenter, catalog);
                GameObject savedPanel = SavePrefab(panel, PanelPath);
                ValidateCards(savedPanel.GetComponent<ShopCatalogPresenter>(), catalog);
                UnityEngine.Object.DestroyImmediate(panel);

                GameObject preview = NewRoot("PF_DynamicShop_Preview", previewScene);
                Canvas canvas = preview.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
                CanvasScaler scaler = preview.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = .5f;
                preview.AddComponent<GraphicRaycaster>();
                Image backdrop = CreateImage("PreviewBackdrop", preview.transform, null, false);
                Stretch(backdrop.rectTransform, Vector2.zero, Vector2.zero);
                backdrop.color = new Color(.015f, .028f, .065f, 1f);
                GameObject sample = (GameObject)PrefabUtility.InstantiatePrefab(RequireAsset<GameObject>(PanelPath), preview.transform);
                sample.name = "ShopFrameContainer";
                // This separate preview has the same central content bounds as MainMenu.
                RectTransform sampleRect = sample.GetComponent<RectTransform>();
                sampleRect.anchorMin = new Vector2(0f, .12f);
                sampleRect.anchorMax = new Vector2(1f, .88f);
                sampleRect.offsetMin = new Vector2(58f, 0f);
                sampleRect.offsetMax = new Vector2(-58f, 0f);
                RecordPrefabOverrides(sample, sampleRect);
                GameObject savedPreview = SavePrefab(preview, PreviewPath);
                int cardCount = ValidateCards(savedPreview.GetComponentInChildren<ShopCatalogPresenter>(true), catalog).Length;
                UnityEngine.Object.DestroyImmediate(preview);
                bool unchanged = catalogBefore == EditorJsonUtility.ToJson(catalog, false);
                if (!unchanged) throw new InvalidOperationException("The shop catalog unexpectedly changed while authoring prefabs.");
                return new BuildReport
                {
                    capturedUtc = DateTime.UtcNow.ToString("o"),
                    prefabPaths = new[] { FeaturedPath, NoAdsPath, CompactPath, BundlePath, SectionPath, PanelPath, PreviewPath },
                    cardCount = cardCount,
                    catalogUnchanged = unchanged,
                    sceneSaved = false
                };
            }
            finally { EditorSceneManager.ClosePreviewScene(previewScene); }
        }

        private static void RequireUnrevisedCardPrefabs()
        {
            // The revised cards place their existing StateText inside BuyButton.
            // Check serialized references so renamed children remain protected, and
            // reject before this legacy generator writes even the first prefab.
            foreach (string path in new[] { FeaturedPath, NoAdsPath, CompactPath, BundlePath })
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                ShopItemCardUI card = prefab != null ? prefab.GetComponent<ShopItemCardUI>() : null;
                if (card == null) continue;
                var serialized = new SerializedObject(card);
                var state = serialized.FindProperty("_stateText").objectReferenceValue as TextMeshProUGUI;
                var buy = serialized.FindProperty("_buyButton").objectReferenceValue as Button;
                if (state != null && buy != null && state.transform.IsChildOf(buy.transform))
                    throw new InvalidOperationException("Horizontal shop prefabs are already authored. " +
                        "Use Phase34ShopVisualRevision.ApplyPrefabVisuals for scoped visual maintenance; " +
                        "refresh the existing presenter for catalog changes. Full legacy regeneration would replace the reviewed layout.");
            }
        }

        /// <summary>
        /// Replaces only the existing ShopFrameContainer in an already open MainMenu.
        /// The existing ShopPage, ShopUI, feedback dialog, top bar and bottom navigation stay.
        /// The root owns backups and explicitly saves after review. Reapplying is a no-op.
        /// </summary>
        public static MigrationReport ApplyToOpenMainMenu()
        {
            RequireEditMode();
            Scene scene = SceneManager.GetSceneByPath(MainMenuPath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("MainMenu must already be open; migration never loads or saves it.");
            ShopConfig catalog = RequireAsset<ShopConfig>(CatalogPath);
            ShopPresentationConfig presentation = RequireAsset<ShopPresentationConfig>(PresentationPath);
            ValidatePresentation(catalog, presentation);
            GameObject prefab = RequireAsset<GameObject>(PanelPath);
            ShopCatalogPresenter prefabPresenter = prefab.GetComponent<ShopCatalogPresenter>();
            if (prefabPresenter == null) throw new InvalidOperationException("Build and review the dynamic shop preview first.");
            ValidateCards(prefabPresenter, catalog);
            Transform shop = FindShop(scene);
            if (shop == null) throw new InvalidOperationException("MainMenuCanvas/PagesRoot/ShopPage was not found.");
            ShopUI controller = shop.GetComponent<ShopUI>();
            Transform oldFrame = shop.Find("ShopFrameContainer");
            if (controller == null || oldFrame == null)
                throw new InvalidOperationException("The existing ShopUI or frame is missing.");
            var report = new MigrationReport
            {
                capturedUtc = DateTime.UtcNow.ToString("o"),
                sceneWasDirty = scene.isDirty,
                sceneSaved = false,
                catalogUnchanged = true,
                replacedRoot = "MainMenuCanvas/PagesRoot/ShopPage/ShopFrameContainer"
            };
            ShopCatalogPresenter alreadyMigrated = oldFrame.GetComponent<ShopCatalogPresenter>();
            if (alreadyMigrated != null)
            {
                report.itemIds = ValidateCards(alreadyMigrated, catalog);
                report.cardCount = report.itemIds.Length;
                return report;
            }

            string catalogBefore = EditorJsonUtility.ToJson(catalog, false);
            var controllerBefore = new SerializedObject(controller);
            UnityEngine.Object feedback = RequireProperty(controllerBefore, "_feedbackDialog").objectReferenceValue;
            // Resolve every expected property before mutating the hierarchy.
            foreach (string field in new[] { "_config", "_presentation", "_catalogPresenter", "_closeButton", "_limitedOfferTimerText", "_itemCards" })
                RequireProperty(controllerBefore, field);
            int oldSiblingIndex = oldFrame.GetSiblingIndex();
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Phase 34: Replace Royal Bazar content with dynamic prefabs");
            try
            {
                // Capture immediately: presenter.Rebuild publishes its cards into
                // the controller before the migration finishes or might throw.
                Undo.RegisterCompleteObjectUndo(controller, "Wire dynamic shop controller");
                GameObject newFrame = (GameObject)PrefabUtility.InstantiatePrefab(prefab, shop);
                Undo.RegisterCreatedObjectUndo(newFrame, "Create dynamic shop frame");
                newFrame.name = "ShopFrameContainer";
                newFrame.transform.SetSiblingIndex(oldSiblingIndex);
                Stretch(newFrame.GetComponent<RectTransform>(), new Vector2(58f, 0f), new Vector2(-58f, 0f));
                RecordPrefabOverrides(newFrame, newFrame.transform);
                ShopCatalogPresenter presenter = newFrame.GetComponent<ShopCatalogPresenter>();
                SetReferences(presenter, ("_shopUI", controller));
                presenter.Rebuild();
                string[] ids = ValidateCards(presenter, catalog);
                Transform closeTransform = newFrame.transform.Find("CloseButton");
                Button closeButton = closeTransform != null ? closeTransform.GetComponent<Button>() : null;
                if (closeButton == null) throw new InvalidOperationException("The new shop close button is missing.");

                var serialized = new SerializedObject(controller);
                RequireProperty(serialized, "_config").objectReferenceValue = catalog;
                RequireProperty(serialized, "_presentation").objectReferenceValue = presentation;
                RequireProperty(serialized, "_catalogPresenter").objectReferenceValue = presenter;
                RequireProperty(serialized, "_closeButton").objectReferenceValue = closeButton;
                RequireProperty(serialized, "_limitedOfferTimerText").objectReferenceValue = presenter.LimitedOfferTimerText;
                RequireProperty(serialized, "_feedbackDialog").objectReferenceValue = feedback;
                SerializedProperty cards = RequireProperty(serialized, "_itemCards");
                cards.arraySize = presenter.Cards.Count;
                for (int i = 0; i < presenter.Cards.Count; i++)
                    cards.GetArrayElementAtIndex(i).objectReferenceValue = presenter.Cards[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                RecordPrefabOverrides(presenter, controller);
                Undo.DestroyObjectImmediate(oldFrame.gameObject);
                report.changed = true;
                report.itemIds = ids;
                report.cardCount = ids.Length;
                report.catalogUnchanged = catalogBefore == EditorJsonUtility.ToJson(catalog, false);
                if (!report.catalogUnchanged) throw new InvalidOperationException("Catalog changed during visual migration.");
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(undoGroup);
                return report;
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }

        private static void SaveCard(Scene scene, ShopConfig catalog, ShopPresentationConfig config,
            CardKind kind, string path, ShopItemData example)
        {
            if (example == null) throw new InvalidOperationException("Each card family requires a catalog example.");
            GameObject root = CreateCard(scene, catalog, config, kind, example);
            root.GetComponent<ShopItemCardUI>().RefreshDisplay();
            GameObject saved = SavePrefab(root, path);
            ShopItemCardUI savedCard = saved.GetComponent<ShopItemCardUI>();
            if (savedCard == null || !ReferenceEquals(savedCard.ItemData, example))
                throw new InvalidOperationException("Saved card lost its catalog binding: " + path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static GameObject CreateCard(Scene scene, ShopConfig catalog, ShopPresentationConfig config,
            CardKind kind, ShopItemData product)
        {
            bool featured = kind == CardKind.Featured;
            bool noAds = kind == CardKind.NoAds;
            bool bundle = kind == CardKind.Bundle;
            bool wide = featured || noAds;
            float height = featured ? config.FeaturedCardHeight : noAds ? config.NoAdsCardHeight :
                bundle ? config.BundleCardHeight : config.CompactCardHeight;
            GameObject root = NewRoot("PF_Shop" + kind, scene);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(wide ? 888f : 435f, height);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minWidth = 0f;
            layout.preferredHeight = height;
            layout.flexibleWidth = 1f;
            Image background = CreateImage("Background", root.transform,
                featured ? config.FeaturedBackground : noAds ? config.NoAdsBackground :
                bundle && config.BundleBackground != null ? config.BundleBackground : config.CompactBackground, true);
            Stretch(background.rectTransform, Vector2.zero, Vector2.zero);

            RectTransform artworkRoot = CreateRect("ArtworkRoot", root.transform);
            if (featured) Region(artworkRoot, .51f, 0f, 1f, 1f, 0f, 110f, -18f, -18f);
            else if (noAds) Region(artworkRoot, 0f, 0f, .24f, 1f, 20f, 20f, -6f, -50f);
            else Region(artworkRoot, 0f, 0f, .48f, 1f, 14f, 122f, -4f, -102f);
            Image artwork = CreateImage("Artwork", artworkRoot, null, false);
            Stretch(artwork.rectTransform, Vector2.zero, Vector2.zero);
            artwork.preserveAspect = true;

            Image badge = CreateImage("BadgeRoot", root.transform, config.BadgeBackground, true);
            TopLeft(badge.rectTransform, 18f, 6f, wide ? 238f : 200f, 40f);
            TextMeshProUGUI badgeText = CreateText("BadgeText", badge.transform, config, 21f, Color.white, TextAlignmentOptions.Center);
            Stretch(badgeText.rectTransform, new Vector2(10f, 4f), new Vector2(-10f, -4f));
            badgeText.fontStyle = FontStyles.Bold;

            TextMeshProUGUI title = CreateText("TitleText", root.transform, config,
                featured ? 38f : noAds ? 36f : 31f, config.TitleColor, TextAlignmentOptions.MidlineLeft);
            title.fontStyle = FontStyles.Bold;
            if (featured) Region(title.rectTransform, 0f, 1f, .55f, 1f, 28f, -124f, -8f, -48f);
            else if (noAds) Region(title.rectTransform, .25f, 1f, 1f, 1f, 8f, -92f, -24f, -30f);
            else Region(title.rectTransform, 0f, 1f, 1f, 1f, 20f, -104f, -20f, -46f);

            TextMeshProUGUI description = CreateText("DescriptionText", root.transform, config, 24f, config.BodyColor, TextAlignmentOptions.TopLeft);
            if (noAds) Region(description.rectTransform, .25f, 1f, .64f, 1f, 8f, -176f, -10f, -98f);
            else TopLeft(description.rectTransform, 24f, 120f, 300f, 42f);
            description.gameObject.SetActive(noAds);

            RectTransform rewards = CreateRect("Rewards", root.transform);
            if (featured) Region(rewards, 0f, 1f, .52f, 1f, 30f, -274f, -8f, -132f);
            else if (noAds) Region(rewards, .25f, 0f, .64f, 0f, 8f, 20f, -8f, 66f);
            else Region(rewards, .46f, 0f, 1f, 1f, 2f, 146f, -14f, -112f);
            VerticalLayoutGroup rewardLayout = rewards.gameObject.AddComponent<VerticalLayoutGroup>();
            rewardLayout.childAlignment = TextAnchor.UpperLeft;
            rewardLayout.childControlWidth = true;
            rewardLayout.childControlHeight = true;
            rewardLayout.childForceExpandWidth = true;
            rewardLayout.childForceExpandHeight = false;
            rewardLayout.spacing = 4f;
            float rewardSize = wide ? 28f : 24f;
            TextMeshProUGUI gold = Reward(rewards, "Gold", config, rewardSize, new Color(1f, .89f, .48f), out Image goldIcon);
            TextMeshProUGUI mega = Reward(rewards, "Mega", config, rewardSize, new Color(.95f, .98f, 1f), out Image megaIcon);
            TextMeshProUGUI infinity = Reward(rewards, "Infinity", config, rewardSize, new Color(.72f, .90f, 1f), out Image infinityIcon);
            TextMeshProUGUI ads = Reward(rewards, "NoAds", config, noAds ? 23f : rewardSize, config.BodyColor, out Image noAdsIcon);

            Image buyImage = CreateImage("BuyButton", root.transform, config.BuyBackground, true);
            if (featured) Region(buyImage.rectTransform, .55f, 0f, 1f, 0f, 10f, 18f, -20f, 114f);
            else if (noAds) Region(buyImage.rectTransform, .64f, 0f, 1f, 0f, 0f, 18f, -18f, 114f);
            else Region(buyImage.rectTransform, 0f, 0f, 1f, 0f, 16f, 16f, -16f, 112f);
            buyImage.raycastTarget = true;
            Button buy = buyImage.gameObject.AddComponent<Button>();
            buy.targetGraphic = buyImage;
            ColorBlock buttonColors = buy.colors;
            buttonColors.highlightedColor = new Color(1f, 1f, .93f);
            buttonColors.pressedColor = new Color(.70f, .88f, .70f);
            buttonColors.disabledColor = new Color(.55f, .60f, .62f, .92f);
            buy.colors = buttonColors;
            Navigation navigation = buy.navigation;
            navigation.mode = Navigation.Mode.None;
            buy.navigation = navigation;
            TextMeshProUGUI price = CreateText("PriceText", buy.transform, config, wide ? 34f : 30f, Color.white, TextAlignmentOptions.Center);
            Stretch(price.rectTransform, new Vector2(12f, 10f), new Vector2(-12f, -10f));
            price.fontStyle = FontStyles.Bold;
            price.enableWordWrapping = false;
            TextMeshProUGUI state = CreateText("StateText", root.transform, config, 18f, new Color(.75f, .88f, 1f), TextAlignmentOptions.Center);
            if (featured) Region(state.rectTransform, .55f, 0f, 1f, 0f, 10f, 117f, -20f, 141f);
            else if (noAds) Region(state.rectTransform, .64f, 0f, 1f, 0f, 0f, 117f, -18f, 141f);
            else Region(state.rectTransform, 0f, 0f, 1f, 0f, 16f, 114f, -16f, 140f);
            TextMeshProUGUI timer = null;
            if (featured)
            {
                timer = CreateText("TimerText", root.transform, config, 30f, config.TitleColor, TextAlignmentOptions.MidlineLeft);
                Region(timer.rectTransform, 0f, 0f, .52f, 0f, 30f, 28f, -8f, 72f);
                timer.text = "24:00:00";
                timer.fontStyle = FontStyles.Bold;
            }

            ShopItemCardUI card = root.AddComponent<ShopItemCardUI>();
            SetReferences(card,
                ("_titleText", title), ("_descriptionText", description), ("_priceText", price),
                ("_buyButton", buy), ("_iconImage", null), ("_badgeRoot", badge.gameObject), ("_badgeText", badgeText),
                ("_catalog", catalog), ("_presentation", config), ("_backgroundImage", background),
                ("_artworkImage", artwork), ("_goldRewardText", gold), ("_megaRewardText", mega),
                ("_infinityRewardText", infinity), ("_noAdsRewardText", ads), ("_stateText", state),
                ("_goldRewardIcon", goldIcon), ("_megaRewardIcon", megaIcon),
                ("_infinityRewardIcon", infinityIcon), ("_noAdsRewardIcon", noAdsIcon),
                ("_timerText", timer), ("_buyBackgroundImage", buyImage), ("_badgeImage", badge));
            var serialized = new SerializedObject(card);
            RequireProperty(serialized, "_catalogItemId").stringValue = product.Id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static GameObject CreateSection(Scene scene, ShopPresentationConfig config)
        {
            GameObject root = NewRoot("PF_ShopSectionHeader", scene);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(888f, config.SectionHeaderHeight);
            Image image = root.AddComponent<Image>();
            image.sprite = config.SectionBackground;
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredHeight = config.SectionHeaderHeight;
            TextMeshProUGUI title = CreateText("SectionTitle", root.transform, config, 30f, config.TitleColor, TextAlignmentOptions.Center);
            Stretch(title.rectTransform, new Vector2(34f, 6f), new Vector2(-34f, -6f));
            title.fontStyle = FontStyles.Bold;
            title.text = config.GoldSectionTitle;
            return root;
        }

        private static GameObject CreatePanel(Scene scene, ShopConfig catalog, ShopPresentationConfig config)
        {
            GameObject root = NewRoot("PF_DynamicShopPanel", scene);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(964f, 1459f);
            Image frame = CreateImage("FrameBackground", root.transform, config.ShopFrame, true);
            Stretch(frame.rectTransform, Vector2.zero, Vector2.zero);
            Image header = CreateImage("HeaderBackground", root.transform, config.HeaderBackground, false);
            Region(header.rectTransform, 0f, 1f, 1f, 1f, 72f, -170f, -92f, -8f);
            header.preserveAspect = true;
            TextMeshProUGUI shopTitle = CreateText("ShopTitle", header.transform, config, 57f, config.TitleColor, TextAlignmentOptions.Center);
            Region(shopTitle.rectTransform, 0f, 0f, 1f, 1f, 35f, 12f, -35f, -30f);
            shopTitle.text = config.ShopTitle;
            shopTitle.fontStyle = FontStyles.Bold;

            Image closeImage = CreateImage("CloseButton", root.transform, AssetDatabase.LoadAssetAtPath<Sprite>(CloseSpritePath), false);
            RectTransform closeRect = closeImage.rectTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-14f, -20f);
            closeRect.sizeDelta = new Vector2(104f, 104f);
            closeImage.raycastTarget = true;
            closeImage.preserveAspect = true;
            Button close = closeImage.gameObject.AddComponent<Button>();
            close.targetGraphic = closeImage;
            if (closeImage.sprite == null)
            {
                closeImage.color = new Color(.65f, .06f, .11f);
                TextMeshProUGUI closeSymbol = CreateText("CloseSymbol", close.transform, config, 58f, Color.white, TextAlignmentOptions.Center);
                Stretch(closeSymbol.rectTransform, Vector2.zero, Vector2.zero);
                closeSymbol.text = "×";
            }

            RectTransform scrollRect = CreateRect("ContentScrollView", root.transform);
            Stretch(scrollRect, new Vector2(38f, 40f), new Vector2(-38f, -184f));
            ScrollRect scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;
            RectTransform viewport = CreateRect("Viewport", scrollRect);
            Stretch(viewport, Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            // Raycast surface allows drag scrolling in the empty space between products.
            Image dragSurface = viewport.gameObject.AddComponent<Image>();
            dragSurface.color = Color.clear;
            dragSurface.raycastTarget = true;
            RectTransform content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 0f;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            scroll.viewport = viewport;
            ShopCatalogPresenter presenter = root.AddComponent<ShopCatalogPresenter>();
            SetReferences(presenter,
                ("_catalog", catalog), ("_presentation", config), ("_contentRoot", content),
                ("_featuredPrefab", RequireAsset<GameObject>(FeaturedPath).GetComponent<ShopItemCardUI>()),
                ("_noAdsPrefab", RequireAsset<GameObject>(NoAdsPath).GetComponent<ShopItemCardUI>()),
                ("_compactPrefab", RequireAsset<GameObject>(CompactPath).GetComponent<ShopItemCardUI>()),
                ("_bundlePrefab", RequireAsset<GameObject>(BundlePath).GetComponent<ShopItemCardUI>()),
                ("_sectionHeaderPrefab", RequireAsset<GameObject>(SectionPath)),
                ("_shopTitle", shopTitle), ("_frameImage", frame), ("_headerImage", header));
            return root;
        }

        private static TextMeshProUGUI Reward(RectTransform parent, string kind,
            ShopPresentationConfig config, float fontSize, Color color, out Image icon)
        {
            RectTransform row = CreateRect(kind + "Reward", parent);
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 10f;
            layout.minHeight = fontSize + 10f;
            Sprite sprite = kind == "Gold" ? config.GoldRewardIcon : kind == "Mega" ? config.MegaRewardIcon
                : kind == "Infinity" ? config.InfinityRewardIcon : config.NoAdsRewardIcon;
            icon = CreateImage(kind + "RewardIcon", row, sprite, false);
            icon.preserveAspect = true;
            icon.enabled = sprite != null;
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0f, .5f);
            icon.rectTransform.pivot = new Vector2(0f, .5f);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            icon.rectTransform.sizeDelta = new Vector2(32f, 32f);
            TextMeshProUGUI label = CreateText(kind + "RewardText", row, config, fontSize, color, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform, new Vector2(40f, 0f), Vector2.zero);
            label.fontStyle = FontStyles.Bold;
            label.enableWordWrapping = false;
            return label;
        }

        private static GameObject NewRoot(string name, Scene scene)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.layer = 5;
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.layer = 5;
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, bool sliced)
        {
            Image image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent,
            ShopPresentationConfig config, float size, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = CreateRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = config.Font;
            label.fontSize = size;
            label.enableAutoSizing = true;
            label.fontSizeMin = size * .68f;
            label.fontSizeMax = size;
            label.color = color;
            label.alignment = alignment;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Truncate;
            label.raycastTarget = false;
            label.margin = Vector4.zero;
            label.text = string.Empty;
            return label;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private static void Region(RectTransform rect, float x0, float y0, float x1, float y1,
            float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void TopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetReferences(UnityEngine.Object target,
            params (string name, UnityEngine.Object value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var field in fields)
                RequireProperty(serialized, field.name).objectReferenceValue = field.value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty RequireProperty(SerializedObject serialized, string name)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null) throw new InvalidOperationException(serialized.targetObject.GetType().Name + " is missing serialized field " + name);
            return property;
        }

        private static string[] ValidateCards(ShopCatalogPresenter presenter, ShopConfig catalog)
        {
            // Read serialized hierarchy, including prefab assets where Awake never ran.
            ShopItemCardUI[] cards = presenter.GetComponentsInChildren<ShopItemCardUI>(true);
            var ids = new List<string>();
            var found = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopItemCardUI card in cards)
            {
                ShopItemData item = card.ItemData;
                if (item == null || !ReferenceEquals(item, catalog.FindItemById(item.Id)) || !found.Add(item.Id))
                    throw new InvalidOperationException("The dynamic shop contains a missing, stale, or duplicate product binding.");
                ids.Add(item.Id);
            }
            foreach (ShopItemData item in catalog.GetAllItems())
                if (item != null && !found.Contains(item.Id))
                    throw new InvalidOperationException("The dynamic shop is missing current catalog item " + item.Id);
            return ids.ToArray();
        }

        private static void ValidatePresentation(ShopConfig catalog, ShopPresentationConfig config)
        {
            if (config.Font == null) throw new InvalidOperationException("Assign the reviewed TMP font before building shop prefabs.");
            Sprite[] required = { config.ShopFrame, config.HeaderBackground, config.SectionBackground,
                config.FeaturedBackground, config.CompactBackground, config.NoAdsBackground,
                config.BuyBackground, config.BadgeBackground };
            foreach (Sprite sprite in required)
                if (sprite == null) throw new InvalidOperationException("Complete and review all eight text-free skin slots before building.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopItemData item in catalog.GetAllItems())
            {
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id))
                    throw new InvalidOperationException("Shop catalog has a missing or duplicate stable Id.");
                ShopProductArtwork artwork = config.FindArtwork(item.Id);
                if (artwork == null || artwork.Artwork == null)
                    throw new InvalidOperationException("An independent, reviewed artwork is required for " + item.Id);
            }
        }

        private static ShopItemData FirstProduct(List<ShopItemData> products)
        {
            if (products != null)
                foreach (ShopItemData product in products)
                    if (product != null) return product;
            return null;
        }

        private static Transform FindShop(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "MainMenuCanvas") return root.transform.Find("PagesRoot/ShopPage");
            return null;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing required asset: " + path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int split = path.LastIndexOf('/');
            string parent = path.Substring(0, split);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(split + 1));
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            bool success;
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path, out success);
            if (!success || saved == null) throw new InvalidOperationException("Could not write prefab " + path);
            return saved;
        }

        private static void RecordPrefabOverrides(params UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object target in objects)
                if (target != null && PrefabUtility.IsPartOfPrefabInstance(target))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Phase 34 prefab authoring requires Edit Mode.");
        }
    }
}
#endif
