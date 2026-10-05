#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    /// <summary>
    /// Explicit, scoped Phase 34 visual revision. Edits only the five card/header
    /// prefabs listed below and the presentation asset's shared layout measurements.
    /// Never opens, rebuilds or saves a scene, panel prefab, or preview prefab.
    /// Catalog bindings, item data and existing purchase Buttons are preserved.
    /// Root imports and assigns the reviewed text-free skins before this operation.
    /// </summary>
    public static class Phase34ShopVisualRevision
    {
        private const string Folder = "Assets/_Project/Prefabs/UI/ShopDynamic/";
        private const string PresentationPath = "Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset";
        private static readonly string[] PrefabPaths =
        {
            Folder + "PF_ShopFeatured.prefab",
            Folder + "PF_ShopNoAds.prefab",
            Folder + "PF_ShopCompact.prefab",
            Folder + "PF_ShopBundle.prefab",
            Folder + "PF_ShopSectionHeader.prefab"
        };

        [Serializable]
        public sealed class RevisionReport
        {
            public string capturedUtc;
            public string[] modifiedPrefabPaths;
            public bool cardBindingsUnchanged;
            public bool purchaseButtonsUnchanged;
            public bool sceneSaved;
            public bool noAdsHeroAssigned;
            public float featuredHeight;
            public float noAdsHeight;
            public float compactHeight;
            public float bundleHeight;
            public float sectionHeight;
            public float sectionSpacing;
        }

        private sealed class CardParts
        {
            public GameObject Root;
            public ShopItemCardUI Card;
            public RectTransform Rect;
            public Image Background;
            public Image Artwork;
            public RectTransform ArtworkRoot;
            public TextMeshProUGUI Title;
            public TextMeshProUGUI Description;
            public TextMeshProUGUI Price;
            public TextMeshProUGUI State;
            public TextMeshProUGUI Timer;
            public Button Buy;
            public Image BuyBackground;
            public Image Badge;
            public TextMeshProUGUI BadgeText;
            public RectTransform Rewards;
            public TextMeshProUGUI[] RewardTexts;
            public Image[] RewardIcons;
            public string OriginalCardJson;
            public string OriginalButtonJson;
        }

        /// <summary>
        /// Call after importing and binding the new green rectangle, red ribbon and
        /// centered section strip. The optional hero is an independent Image on the
        /// No Ads prefab; null preserves any previously assigned hero.
        /// </summary>
        public static RevisionReport ApplyPrefabVisuals(Sprite noAdsHero = null)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Visual prefab revision requires Edit Mode.");
            ShopPresentationConfig config = AssetDatabase.LoadAssetAtPath<ShopPresentationConfig>(PresentationPath);
            if (config == null || config.BuyBackground == null || config.BadgeBackground == null || config.SectionBackground == null)
                throw new InvalidOperationException("Assign the reviewed shop buy, badge and section skin sprites first.");

            var loaded = new List<GameObject>();
            try
            {
                // Preflight all five assets before changing or saving even the first.
                foreach (string path in PrefabPaths)
                {
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                        throw new InvalidOperationException("Missing existing shop prefab: " + path);
                    loaded.Add(PrefabUtility.LoadPrefabContents(path));
                }
                CardParts featured = ReadCard(loaded[0]);
                CardParts noAds = ReadCard(loaded[1]);
                CardParts compact = ReadCard(loaded[2]);
                CardParts bundle = ReadCard(loaded[3]);
                if (loaded[4].GetComponent<Image>() == null || loaded[4].GetComponentInChildren<TextMeshProUGUI>(true) == null)
                    throw new InvalidOperationException("The section header must retain its root Image and editable TMP title.");

                ConfigureFeatured(featured, config);
                ConfigureNoAds(noAds, config, noAdsHero);
                ConfigureHorizontal(compact, config, false);
                ConfigureHorizontal(bundle, config, true);
                ConfigureSection(loaded[4], config);

                foreach (CardParts card in new[] { featured, noAds, compact, bundle })
                {
                    // References remain identical even when StateText is reparented
                    // into the green buy surface. No Setup/Bind/Refresh/Purchase calls.
                    if (EditorJsonUtility.ToJson(card.Card, false) != card.OriginalCardJson)
                        throw new InvalidOperationException("Visual revision changed the card binding: " + card.Root.name);
                    if (EditorJsonUtility.ToJson(card.Buy, false) != card.OriginalButtonJson)
                        throw new InvalidOperationException("Visual revision changed the purchase Button: " + card.Root.name);
                    if (card.Buy.GetComponent<RectTransform>().rect.height < 70f)
                        throw new InvalidOperationException("Purchase touch area became smaller than 70 units: " + card.Root.name);
                }

                for (int i = 0; i < loaded.Count; i++)
                {
                    bool success;
                    PrefabUtility.SaveAsPrefabAsset(loaded[i], PrefabPaths[i], out success);
                    if (!success) throw new InvalidOperationException("Could not save revised prefab: " + PrefabPaths[i]);
                }

                // Only these authored display measurements are changed on config.
                // Runtime uses each family's reward geometry to grow 3/4-row cards.
                config.FeaturedCardHeight = 340f;
                config.NoAdsCardHeight = 260f;
                config.CompactCardHeight = 195f;
                config.BundleCardHeight = 304f;
                config.SectionHeaderHeight = 56f;
                config.SectionSpacing = 14f;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
                return new RevisionReport
                {
                    capturedUtc = DateTime.UtcNow.ToString("o"),
                    modifiedPrefabPaths = (string[])PrefabPaths.Clone(),
                    cardBindingsUnchanged = true,
                    purchaseButtonsUnchanged = true,
                    sceneSaved = false,
                    noAdsHeroAssigned = HasHero(noAds.Root),
                    featuredHeight = 340f,
                    noAdsHeight = 260f,
                    compactHeight = 195f,
                    bundleHeight = 304f,
                    sectionHeight = 56f,
                    sectionSpacing = 14f
                };
            }
            finally
            {
                foreach (GameObject root in loaded)
                    if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureHorizontal(CardParts card, ShopPresentationConfig config, bool bundle)
        {
            float cardHeight = bundle ? 304f : 195f;
            BaseCard(card, config, cardHeight, false);
            if (bundle)
            {
                Region(card.ArtworkRoot, 0f, 0f, .44f, 1f, 10f, 10f, -2f, -18f);
                Region(card.Title.rectTransform, .44f, 1f, 1f, 1f, 2f, -82f, -12f, -18f);
                Style(card.Title, 27f, 18f, TextAlignmentOptions.MidlineLeft, true, true);
                Region(card.Rewards, .44f, 0f, 1f, 1f, 2f, 116f, -12f, -94f);
                ConfigureRewards(card, 28f, 23f, 24f);
                Region(card.Description.rectTransform, .44f, 0f, 1f, 1f, 2f, 116f, -12f, -94f);
                Style(card.Description, 22f, 16f, TextAlignmentOptions.TopLeft, false, true);
                Region((RectTransform)card.Buy.transform, .44f, 0f, 1f, 0f, 0f, 12f, -12f, 108f);
                PutPriceAndStateInsideButton(card, 29f);
                Region(card.Badge.rectTransform, 0f, 1f, .44f, 1f, 4f, -44f, 2f, -4f);
                ConfigureBadge(card, 19f);
            }
            else
            {
                // Compact card (Altın Çantası, Altın Sandığı, Mega Top, Sonsuz Top)
                // Height: 195f
                Region(card.ArtworkRoot, 0f, 0f, .44f, 1f, 8f, 8f, -2f, -12f);
                Region(card.Title.rectTransform, .44f, 1f, 1f, 1f, 4f, -54f, -14f, -12f);
                Style(card.Title, 25f, 17f, TextAlignmentOptions.MidlineLeft, true, true);
                Region(card.Rewards, .44f, 0f, 1f, 1f, 4f, 94f, -14f, -56f);
                ConfigureRewards(card, 26f, 22f, 24f);
                Region(card.Description.rectTransform, .44f, 0f, 1f, 1f, 4f, 94f, -14f, -56f);
                Style(card.Description, 20f, 15f, TextAlignmentOptions.TopLeft, false, true);
                // Buy button: aligned cleanly at bottom-right, neat margins (bottom 12f, top 94f -> height 82f, right -14f)
                Region((RectTransform)card.Buy.transform, .44f, 0f, 1f, 0f, 4f, 12f, -14f, 94f);
                PutPriceAndStateInsideButton(card, 28f);
                Region(card.Badge.rectTransform, 0f, 1f, .44f, 1f, 4f, -40f, 2f, -4f);
                ConfigureBadge(card, 18f);
            }
        }

        private static void ConfigureFeatured(CardParts card, ShopPresentationConfig config)
        {
            BaseCard(card, config, 340f, true);
            Region(card.ArtworkRoot, .49f, 0f, 1f, 1f, -8f, 86f, -12f, -10f);
            Region(card.Title.rectTransform, 0f, 1f, .56f, 1f, 24f, -118f, -2f, -48f);
            Style(card.Title, 39f, 25f, TextAlignmentOptions.MidlineLeft, true, true);
            Region(card.Rewards, 0f, 0f, .52f, 1f, 26f, 82f, -6f, -128f);
            ConfigureRewards(card, 34f, 28f, 30f);
            Region(card.Description.rectTransform, 0f, 0f, .52f, 1f, 26f, 82f, -6f, -128f);
            Region((RectTransform)card.Buy.transform, .56f, 0f, 1f, 0f, 2f, 12f, -18f, 108f);
            PutPriceAndStateInsideButton(card, 35f);
            TopLeft(card.Badge.rectTransform, 6f, 4f, 260f, 42f);
            ConfigureBadge(card, 22f);
            if (card.Timer != null)
            {
                Region(card.Timer.rectTransform, 0f, 0f, .52f, 0f, 28f, 24f, -6f, 68f);
                Style(card.Timer, 32f, 25f, TextAlignmentOptions.MidlineLeft, true, false);
            }
        }

        private static void ConfigureNoAds(CardParts card, ShopPresentationConfig config, Sprite heroSprite)
        {
            BaseCard(card, config, 260f, true);
            // The existing no-ads symbol keeps its live artwork binding on the left.
            Region(card.ArtworkRoot, 0f, 0f, .22f, 1f, 14f, 20f, -8f, -48f);
            Region(card.Title.rectTransform, .23f, 1f, .67f, 1f, 0f, -104f, -8f, -40f);
            Style(card.Title, 37f, 25f, TextAlignmentOptions.MidlineLeft, true, true);
            Region(card.Description.rectTransform, .23f, 1f, .65f, 1f, 0f, -174f, -10f, -110f);
            Style(card.Description, 26f, 20f, TextAlignmentOptions.TopLeft, false, true);
            Region(card.Rewards, .23f, 0f, .65f, 1f, 0f, 24f, -10f, -180f);
            ConfigureRewards(card, 28f, 23f, 24f);
            Region((RectTransform)card.Buy.transform, .66f, 0f, 1f, 0f, 0f, 12f, -18f, 108f);
            PutPriceAndStateInsideButton(card, 32f);
            TopLeft(card.Badge.rectTransform, 6f, 4f, 218f, 42f);
            ConfigureBadge(card, 22f);

            Transform existing = card.Root.transform.Find("HeroArtworkRoot");
            if (heroSprite != null || existing != null)
            {
                RectTransform heroRoot = existing != null ? existing as RectTransform : NewRect("HeroArtworkRoot", card.Root.transform);
                if (heroRoot == null) throw new InvalidOperationException("HeroArtworkRoot must have a RectTransform.");
                // Character may extend behind the button's lower foreground area.
                // Its Image is independent, editable, and not a purchase/info source.
                Region(heroRoot, .65f, 0f, 1f, 1f, 0f, 62f, -10f, -6f);
                heroRoot.SetSiblingIndex(card.ArtworkRoot.GetSiblingIndex() + 1);
                Transform imageTransform = heroRoot.Find("HeroArtwork");
                Image hero = imageTransform != null ? imageTransform.GetComponent<Image>() : NewRect("HeroArtwork", heroRoot).gameObject.AddComponent<Image>();
                if (hero == null) throw new InvalidOperationException("HeroArtwork must retain an Image component.");
                if (heroSprite != null) hero.sprite = heroSprite;
                Stretch(hero.rectTransform, Vector2.zero, Vector2.zero);
                hero.type = Image.Type.Simple;
                hero.preserveAspect = true;
                hero.color = Color.white;
                hero.raycastTarget = false;
                hero.enabled = hero.sprite != null;
            }
        }

        private static void BaseCard(CardParts card, ShopPresentationConfig config, float height, bool wide)
        {
            card.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            LayoutElement sizing = card.Root.GetComponent<LayoutElement>();
            if (sizing == null) throw new InvalidOperationException("Card root is missing its layout component.");
            sizing.minHeight = height;
            sizing.preferredHeight = height;
            sizing.flexibleHeight = 0f;
            Stretch(card.Background.rectTransform, Vector2.zero, Vector2.zero);
            Stretch(card.Artwork.rectTransform, Vector2.zero, Vector2.zero);
            card.Artwork.type = Image.Type.Simple;
            card.Artwork.preserveAspect = true;
            card.Artwork.raycastTarget = false;
            SetSkin(card.BuyBackground, config.BuyBackground);
            SetSkin(card.Badge, config.BadgeBackground);
            card.BuyBackground.preserveAspect = false;
            card.Badge.preserveAspect = false;
            card.BuyBackground.color = Color.white;
            card.Badge.color = Color.white;
            card.Badge.raycastTarget = false;
            card.BuyBackground.pixelsPerUnitMultiplier = 1.5f;
        }

        private static void PutPriceAndStateInsideButton(CardParts card, float fontSize)
        {
            card.Price.rectTransform.SetParent(card.Buy.transform, false);
            Region(card.Price.rectTransform, 0f, 0f, 1f, 1f, 8f, 0f, -8f, 0f);
            Style(card.Price, fontSize, 18f, TextAlignmentOptions.Center, true, false);
            card.Price.raycastTarget = false;
            card.State.rectTransform.SetParent(card.Buy.transform, false);
            Region(card.State.rectTransform, 0f, 0f, 1f, 0f, 8f, 10f, -8f, 30f);
            Style(card.State, 15f, 12f, TextAlignmentOptions.Center, false, false);
            card.State.text = string.Empty;
            card.State.gameObject.SetActive(false);
            card.State.raycastTarget = false;
        }

        private static void ConfigureRewards(CardParts card, float rowHeight, float fontSize, float iconSize)
        {
            VerticalLayoutGroup layout = card.Rewards.GetComponent<VerticalLayoutGroup>();
            if (layout == null) throw new InvalidOperationException("Rewards requires its existing VerticalLayoutGroup.");
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 3f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            for (int i = 0; i < card.RewardTexts.Length; i++)
            {
                TextMeshProUGUI label = card.RewardTexts[i];
                RectTransform row = label.transform.parent as RectTransform;
                LayoutElement sizing = row.GetComponent<LayoutElement>();
                if (sizing == null) throw new InvalidOperationException("Each reward row must retain its LayoutElement.");
                sizing.minHeight = rowHeight;
                sizing.preferredHeight = rowHeight;
                sizing.flexibleHeight = 0f;
                Stretch(label.rectTransform, new Vector2(iconSize + 4f, 0f), Vector2.zero);
                Style(label, fontSize, 17f, TextAlignmentOptions.MidlineLeft, true, false);
                Image icon = card.RewardIcons[i];
                if (icon == null) continue;
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0f, .5f);
                icon.rectTransform.pivot = new Vector2(0f, .5f);
                icon.rectTransform.anchoredPosition = Vector2.zero;
                icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }
        }

        private static void ConfigureBadge(CardParts card, float size)
        {
            Stretch(card.BadgeText.rectTransform, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            Style(card.BadgeText, size, 14f, TextAlignmentOptions.Center, true, false);
            card.BadgeText.raycastTarget = false;
        }

        private static void ConfigureSection(GameObject root, ShopPresentationConfig config)
        {
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 56f);
            LayoutElement sizing = root.GetComponent<LayoutElement>();
            if (sizing == null) throw new InvalidOperationException("Section header is missing its existing LayoutElement.");
            sizing.minHeight = sizing.preferredHeight = 56f;
            sizing.flexibleHeight = 0f;
            Image skin = root.GetComponent<Image>();
            skin.sprite = config.SectionBackground;
            skin.type = Image.Type.Simple;
            skin.preserveAspect = true;
            skin.raycastTarget = false;
            skin.color = Color.white;
            TextMeshProUGUI title = root.GetComponentInChildren<TextMeshProUGUI>(true);
            Region(title.rectTransform, .22f, 0f, .78f, 1f, 0f, 4f, 0f, -4f);
            Style(title, 29f, 21f, TextAlignmentOptions.Center, true, false);
            title.raycastTarget = false;
        }

        private static CardParts ReadCard(GameObject root)
        {
            ShopItemCardUI component = root.GetComponent<ShopItemCardUI>();
            if (component == null) throw new InvalidOperationException("Shop prefab has no existing ShopItemCardUI: " + root.name);
            var serialized = new SerializedObject(component);
            var result = new CardParts
            {
                Root = root,
                Card = component,
                Rect = root.GetComponent<RectTransform>(),
                Background = Reference<Image>(serialized, "_backgroundImage"),
                Artwork = Reference<Image>(serialized, "_artworkImage"),
                Title = Reference<TextMeshProUGUI>(serialized, "_titleText"),
                Description = Reference<TextMeshProUGUI>(serialized, "_descriptionText"),
                Price = Reference<TextMeshProUGUI>(serialized, "_priceText"),
                State = Reference<TextMeshProUGUI>(serialized, "_stateText"),
                Timer = Reference<TextMeshProUGUI>(serialized, "_timerText", false),
                Buy = Reference<Button>(serialized, "_buyButton"),
                BuyBackground = Reference<Image>(serialized, "_buyBackgroundImage"),
                Badge = Reference<Image>(serialized, "_badgeImage"),
                BadgeText = Reference<TextMeshProUGUI>(serialized, "_badgeText"),
                RewardTexts = new[]
                {
                    Reference<TextMeshProUGUI>(serialized, "_goldRewardText"),
                    Reference<TextMeshProUGUI>(serialized, "_megaRewardText"),
                    Reference<TextMeshProUGUI>(serialized, "_infinityRewardText"),
                    Reference<TextMeshProUGUI>(serialized, "_noAdsRewardText")
                },
                RewardIcons = new[]
                {
                    Reference<Image>(serialized, "_goldRewardIcon", false),
                    Reference<Image>(serialized, "_megaRewardIcon", false),
                    Reference<Image>(serialized, "_infinityRewardIcon", false),
                    Reference<Image>(serialized, "_noAdsRewardIcon", false)
                },
                OriginalCardJson = EditorJsonUtility.ToJson(component, false)
            };
            result.ArtworkRoot = result.Artwork.transform.parent as RectTransform;
            result.Rewards = result.RewardTexts[0].transform.parent.parent as RectTransform;
            if (result.ArtworkRoot == null || result.Rewards == null || result.BuyBackground.gameObject != result.Buy.gameObject)
                throw new InvalidOperationException("Existing artwork/reward/buy hierarchy did not match the reviewed prefab contract.");
            result.OriginalButtonJson = EditorJsonUtility.ToJson(result.Buy, false);
            return result;
        }

        private static T Reference<T>(SerializedObject serialized, string fieldName, bool required = true) where T : UnityEngine.Object
        {
            SerializedProperty field = serialized.FindProperty(fieldName);
            if (field == null) throw new InvalidOperationException("ShopItemCardUI is missing field " + fieldName);
            T value = field.objectReferenceValue as T;
            if (value == null && required) throw new InvalidOperationException("ShopItemCardUI has an unassigned " + fieldName);
            return value;
        }

        private static void Style(TextMeshProUGUI text, float maximum, float minimum,
            TextAlignmentOptions alignment, bool bold, bool wrap)
        {
            text.fontSize = maximum;
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
            text.alignment = alignment;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
            text.margin = Vector4.zero;
            text.raycastTarget = false;
        }

        private static void SetSkin(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }

        private static bool HasHero(GameObject root)
        {
            Transform hero = root.transform.Find("HeroArtworkRoot/HeroArtwork");
            Image image = hero != null ? hero.GetComponent<Image>() : null;
            return image != null && image.sprite != null;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect, Vector2 minimum, Vector2 maximum)
        {
            Region(rect, 0f, 0f, 1f, 1f, minimum.x, minimum.y, maximum.x, maximum.y);
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
    }
}
#endif
