using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Economy;
using RoyalSmash.Save;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Displays and purchases a catalog product. Existing inline data remains available
    /// for legacy cards, but a catalog binding always resolves the current catalog item.
    /// </summary>
    public class ShopItemCardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _badgeRoot;
        [SerializeField] private TextMeshProUGUI _badgeText;

        [Header("Editable Presentation")]
        [SerializeField] private ShopPresentationConfig _presentation;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _artworkImage;
        [SerializeField] private Image _buyBackgroundImage;
        [SerializeField] private Image _badgeImage;
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Live Rewards — each text has its own row parent")]
        [SerializeField] private TextMeshProUGUI _goldRewardText;
        [SerializeField] private TextMeshProUGUI _megaRewardText;
        [SerializeField] private TextMeshProUGUI _infinityRewardText;
        [SerializeField] private TextMeshProUGUI _noAdsRewardText;
        [SerializeField] private Image _goldRewardIcon;
        [SerializeField] private Image _megaRewardIcon;
        [SerializeField] private Image _infinityRewardIcon;
        [SerializeField] private Image _noAdsRewardIcon;

        [Header("Catalog Binding")]
        [SerializeField] private ShopConfig _catalog;
        [SerializeField] private string _catalogItemId;

        // Retained so unmigrated cards and Setup(data) keep their existing contract.
        [SerializeField] private ShopItemData _itemData;

        public ShopItemData ItemData
        {
            get
            {
                if (_catalog == null)
                {
                    // A lost catalog reference with an existing key is a broken binding,
                    // rather than permission to purchase its preserved inline copy.
                    return string.IsNullOrEmpty(_catalogItemId) ? _itemData : null;
                }
                if (string.IsNullOrWhiteSpace(_catalogItemId)) return null;

                ShopItemData item = _catalog.FindItemById(_catalogItemId);
                // FindItemById also accepts store ProductId aliases. A card binding
                // deliberately uses the stable catalog Id, never the billing SKU.
                return item != null && string.Equals(item.Id, _catalogItemId, StringComparison.Ordinal)
                    ? item
                    : null;
            }
        }

        private Image _ownedTintImage;
        private Color _unownedButtonColor;
        private bool _hasOwnedTint;
        private IAPManager _subscribedIap;

        public string CatalogItemId => _catalogItemId;
        public TextMeshProUGUI TimerText => _timerText;

        private void Awake()
        {
            if (_buyButton == null)
            {
                _buyButton = GetComponent<Button>();
            }

            if (_priceText == null)
            {
                _priceText = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (_buyButton != null)
            {
                _buyButton.onClick.RemoveListener(HandleBuyClicked);
                _buyButton.onClick.AddListener(HandleBuyClicked);
            }
        }

        private void OnEnable()
        {
            SubscribeToStore();
            RefreshDisplay();
        }

        private void Start() => RefreshDisplay();

        private void OnDisable() => UnsubscribeFromStore();

        private void OnDestroy()
        {
            UnsubscribeFromStore();
            if (_buyButton != null)
            {
                _buyButton.onClick.RemoveListener(HandleBuyClicked);
            }
        }

        /// <summary>
        /// Switches to the legacy explicit-data mode. Used by existing setup callers.
        /// </summary>
        public void Setup(ShopItemData data)
        {
            _catalog = null;
            _catalogItemId = null;
            _itemData = data;
            RefreshDisplay();
        }

        /// <summary>
        /// Binds a stable catalog Id. Invalid input leaves the current binding intact.
        /// </summary>
        public void BindProduct(ShopConfig catalog, string stableItemId)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (string.IsNullOrWhiteSpace(stableItemId))
            {
                throw new ArgumentException("A stable shop item Id is required.", nameof(stableItemId));
            }

            ShopItemData item = catalog.FindItemById(stableItemId);
            if (item == null || !string.Equals(item.Id, stableItemId, StringComparison.Ordinal))
            {
                throw new ArgumentException("The stable item Id is not present in this shop catalog.", nameof(stableItemId));
            }

            _catalog = catalog;
            _catalogItemId = stableItemId;
            // The inline copy is intentionally left untouched for migration rollback.
            RefreshDisplay(item);
        }

        public void BindReferences(
            TextMeshProUGUI titleTxt, TextMeshProUGUI descTxt, TextMeshProUGUI priceTxt,
            Button buyBtn, Image iconImg, GameObject badgeRt = null, TextMeshProUGUI badgeTxt = null)
        {
            if (_buyButton != null) _buyButton.onClick.RemoveListener(HandleBuyClicked);
            RestoreOwnedTint();

            _titleText = titleTxt;
            _descriptionText = descTxt;
            _priceText = priceTxt;
            _buyButton = buyBtn != null ? buyBtn : GetComponent<Button>();
            _iconImage = iconImg;
            _badgeRoot = badgeRt;
            _badgeText = badgeTxt;

            if (_buyButton != null)
            {
                _buyButton.onClick.RemoveListener(HandleBuyClicked);
                _buyButton.onClick.AddListener(HandleBuyClicked);
            }

            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            SubscribeToStore();
            ShopItemData item = ItemData;
            RefreshDisplay(item);
        }

        public void BindPresentation(ShopPresentationConfig presentation)
        {
            _presentation = presentation;
            RefreshDisplay();
        }

        private void RefreshDisplay(ShopItemData item, bool? purchaseInProgress = null)
        {
            if (item == null)
            {
                ClearDynamicDisplay();
                return;
            }

            ApplyPresentation(item);
            bool hasLiveRewards = _goldRewardText != null || _megaRewardText != null ||
                                  _infinityRewardText != null || _noAdsRewardText != null;
            if (_titleText != null) _titleText.text = hasLiveRewards ? GetDisplayTitle(item) : item.Title;
            if (_descriptionText != null)
            {
                // Legacy descriptions contain copied reward quantities. Dynamic cards
                // render those quantities once, directly from the reward fields below.
                bool showDescription = !hasLiveRewards || (item.ItemType == ShopItemType.NoAds && item.GrantsNoAds) ||
                                       (item.GoldReward <= 0 && item.MegaBallReward <= 0 && item.InfinityBallReward <= 0);
                _descriptionText.text = showDescription
                    ? (item.GrantsNoAds && _presentation != null ? _presentation.NoAdsDescription : item.Description)
                    : string.Empty;
                _descriptionText.gameObject.SetActive(showDescription && !string.IsNullOrEmpty(_descriptionText.text));
            }

            SetRewardRow(_goldRewardText, item.GoldReward, Label(_presentation != null ? _presentation.GoldRewardLabel : null, "Altın"));
            SetRewardRow(_megaRewardText, item.MegaBallReward, Label(_presentation != null ? _presentation.MegaRewardLabel : null, "Mega Top"));
            SetRewardRow(_infinityRewardText, item.InfinityBallReward, Label(_presentation != null ? _presentation.InfinityRewardLabel : null, "Sonsuz Top"));
            if (_noAdsRewardText != null)
            {
                _noAdsRewardText.text = item.GrantsNoAds ? Label(_presentation != null ? _presentation.NoAdsRewardLabel : null, "Reklamsız oyun") : string.Empty;
                SetRewardRowVisible(_noAdsRewardText, item.GrantsNoAds);
            }

            if (_iconImage != null && _iconImage != _artworkImage)
            {
                _iconImage.sprite = item.Icon;
                _iconImage.enabled = item.Icon != null;
                _iconImage.color = item.ThemeColor;
            }

            bool hasBadge = !string.IsNullOrEmpty(item.BadgeText);
            if (_badgeRoot != null) _badgeRoot.SetActive(hasBadge);
            if (_badgeText != null) _badgeText.text = hasBadge ? item.BadgeText : string.Empty;

            bool isNoAdsOwned = IsNoAdsOwned(item);
            if (isNoAdsOwned)
            {
                if (_priceText != null)
                {
                    _priceText.text = Label(_presentation != null ? _presentation.OwnedLabel : null, "SATIN ALINDI");
                    _priceText.alignment = TextAlignmentOptions.Center;
                    _priceText.rectTransform.anchorMin = Vector2.zero;
                    _priceText.rectTransform.anchorMax = Vector2.one;
                    _priceText.rectTransform.offsetMin = new Vector2(8f, 0f);
                    _priceText.rectTransform.offsetMax = new Vector2(-8f, 0f);
                }
                SetState(string.Empty);
                if (_buyButton != null) _buyButton.interactable = false;
                ApplyOwnedTint();
                return;
            }

            RestoreOwnedTint();
            IAPManager iap = Application.isPlaying && IAPManager.HasInstance ? IAPManager.Instance : null;
            string priceToDisplay;
            if (item.Currency == PriceCurrency.InGameGold)
            {
                priceToDisplay = item.CostInGold.ToString("N0", CultureInfo.CurrentCulture) + " " +
                                 Label(_presentation != null ? _presentation.GoldRewardLabel : null, "Altın");
            }
            else
            {
                string storePrice = iap != null && iap.IsInitialized && !string.IsNullOrEmpty(item.ProductId)
                    ? iap.GetLocalizedPriceString(item.ProductId, null) : null;
                priceToDisplay = string.IsNullOrEmpty(storePrice) ? item.PriceString : storePrice;
            }

            if (_priceText != null)
            {
                _priceText.text = priceToDisplay;
                _priceText.alignment = TextAlignmentOptions.Center;
                _priceText.rectTransform.anchorMin = Vector2.zero;
                _priceText.rectTransform.anchorMax = Vector2.one;
                _priceText.rectTransform.offsetMin = new Vector2(8f, 0f);
                _priceText.rectTransform.offsetMax = new Vector2(-8f, 0f);
            }
            bool locked = purchaseInProgress ?? (iap != null && iap.IsPurchaseInProgress);
            SetState(locked ? Label(_presentation != null ? _presentation.PendingLabel : null, "İŞLEM SÜRÜYOR…") : string.Empty);
            if (_buyButton != null) _buyButton.interactable = !locked;
        }

        private void ClearDynamicDisplay()
        {
            if (_titleText != null) _titleText.text = string.Empty;
            if (_descriptionText != null) _descriptionText.text = string.Empty;
            if (_priceText != null) _priceText.text = string.Empty;
            if (_badgeText != null) _badgeText.text = string.Empty;
            if (_badgeRoot != null) _badgeRoot.SetActive(false);
            SetRewardRow(_goldRewardText, 0, string.Empty);
            SetRewardRow(_megaRewardText, 0, string.Empty);
            SetRewardRow(_infinityRewardText, 0, string.Empty);
            if (_noAdsRewardText != null)
            {
                _noAdsRewardText.text = string.Empty;
                SetRewardRowVisible(_noAdsRewardText, false);
            }
            if (_artworkImage != null)
            {
                _artworkImage.sprite = null;
                _artworkImage.enabled = false;
            }
            SetState(Label(_presentation != null ? _presentation.UnavailableLabel : null, "KULLANILAMIYOR"));
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = Color.white;
                _iconImage.enabled = false;
            }
            if (_buyButton != null) _buyButton.interactable = false;
            RestoreOwnedTint();
        }

        private static bool IsNoAdsOwned(ShopItemData item)
        {
            if (item == null || !item.GrantsNoAds) return false;
            if (SaveManager.HasInstance && SaveManager.Instance != null &&
                SaveManager.Instance.Data != null && SaveManager.Instance.Data.NoAdsPurchased)
                return true;
            if (!Application.isPlaying)
            {
                var sm = UnityEngine.Object.FindFirstObjectByType<SaveManager>();
                if (sm != null)
                {
                    if (sm.Data == null) sm.Load();
                    return sm.Data != null && sm.Data.NoAdsPurchased;
                }
            }
            return false;
        }

        // Only restore a tint this component applied; leave unrelated button colors alone.
        private void ApplyOwnedTint()
        {
            Image image = _buyButton != null ? _buyButton.GetComponent<Image>() : null;
            if (image == null) return;
            if (!_hasOwnedTint || _ownedTintImage != image)
            {
                RestoreOwnedTint();
                _ownedTintImage = image;
                _unownedButtonColor = image.color;
                _hasOwnedTint = true;
            }
            image.color = new Color(0.4f, 0.4f, 0.45f, 0.8f);
        }

        private void RestoreOwnedTint()
        {
            if (_hasOwnedTint && _ownedTintImage != null)
            {
                _ownedTintImage.color = _unownedButtonColor;
            }
            _ownedTintImage = null;
            _hasOwnedTint = false;
        }

        private void HandleBuyClicked()
        {
            if (!Application.isPlaying) return;
            ShopItemData item = ItemData;
            if (item == null)
            {
                ClearDynamicDisplay();
                Debug.LogWarning($"[ShopItemCardUI] Cannot buy: no valid catalog item on {gameObject.name}");
                return;
            }

            Debug.Log($"[ShopItemCardUI] Buy clicked for '{item.Title}' (Id={item.Id}, ProductId={item.ProductId}, Price={item.PriceString})");

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.PurchaseItem(item);
            }
            else
            {
                Debug.LogError("[ShopItemCardUI] IAPManager.Instance is null! Cannot initiate purchase.");
            }
        }

        private void HandlePurchaseStateChanged(bool inProgress)
        {
            ShopItemData item = ItemData;
            RefreshDisplay(item, inProgress);
        }

        private void HandleStoreInitialized()
        {
            ShopItemData item = ItemData;
            RefreshDisplay(item);
        }

        private void SubscribeToStore()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            IAPManager current = IAPManager.Instance;
            if (_subscribedIap == current) return;
            UnsubscribeFromStore();
            _subscribedIap = current;
            if (_subscribedIap == null) return;
            _subscribedIap.OnPurchaseStateChanged += HandlePurchaseStateChanged;
            _subscribedIap.OnStoreInitialized += HandleStoreInitialized;
        }

        private void UnsubscribeFromStore()
        {
            if (_subscribedIap != null)
            {
                _subscribedIap.OnPurchaseStateChanged -= HandlePurchaseStateChanged;
                _subscribedIap.OnStoreInitialized -= HandleStoreInitialized;
            }
            _subscribedIap = null;
        }

        private void ApplyPresentation(ShopItemData item)
        {
            ShopProductArtwork art = _presentation != null ? _presentation.FindArtwork(item.Id) : null;
            if (_artworkImage != null)
            {
                _artworkImage.sprite = art != null && art.Artwork != null ? art.Artwork : item.Icon;
                _artworkImage.enabled = _artworkImage.sprite != null;
                _artworkImage.color = Color.white;
                _artworkImage.preserveAspect = true;
            }
            SetRewardIcon(_goldRewardIcon, _presentation != null ? _presentation.GoldRewardIcon : null);
            SetRewardIcon(_megaRewardIcon, _presentation != null ? _presentation.MegaRewardIcon : null);
            SetRewardIcon(_infinityRewardIcon, _presentation != null ? _presentation.InfinityRewardIcon : null);
            SetRewardIcon(_noAdsRewardIcon, _presentation != null ? _presentation.NoAdsRewardIcon : null);
            if (_presentation == null) return;
            Sprite background = _presentation.CompactBackground;
            switch (item.ItemType)
            {
                case ShopItemType.LimitedOffer: background = _presentation.FeaturedBackground; break;
                case ShopItemType.NoAds: background = _presentation.NoAdsBackground; break;
                case ShopItemType.Bundle:
                    if (_presentation.BundleBackground != null) background = _presentation.BundleBackground;
                    break;
            }
            if (art != null && art.BackgroundOverride != null) background = art.BackgroundOverride;
            SetSkin(_backgroundImage, background);
            SetSkin(_buyBackgroundImage, _presentation.BuyBackground);
            if (_buyBackgroundImage != null) _buyBackgroundImage.pixelsPerUnitMultiplier = 1.5f;
            SetSkin(_badgeImage, _presentation.BadgeBackground);
            ApplyTextStyle(_titleText, true);
            ApplyTextStyle(_descriptionText, false);
            ApplyTextStyle(_priceText, false);
            ApplyTextStyle(_badgeText, false);
            ApplyTextStyle(_goldRewardText, false);
            ApplyTextStyle(_megaRewardText, false);
            ApplyTextStyle(_infinityRewardText, false);
            ApplyTextStyle(_noAdsRewardText, false);
            ApplyTextStyle(_stateText, false);
            ApplyTextStyle(_timerText, false);
        }

        private void ApplyTextStyle(TextMeshProUGUI text, bool title)
        {
            if (text == null || _presentation == null) return;
            if (_presentation.Font != null && text.font != _presentation.Font) text.font = _presentation.Font;
            text.color = title ? _presentation.TitleColor : _presentation.BodyColor;
        }

        private static void SetSkin(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }

        private static void SetRewardIcon(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
            image.color = Color.white;
            image.preserveAspect = true;
        }

        private void SetState(string message)
        {
            if (_stateText != null)
            {
                _stateText.text = message;
                bool hasState = !string.IsNullOrEmpty(message);
                _stateText.gameObject.SetActive(hasState);
            }

            if (_buyButton != null && _priceText != null &&
                _priceText.transform.parent == _buyButton.transform)
            {
                bool hasState = _stateText != null && _stateText.gameObject.activeSelf && !string.IsNullOrEmpty(_stateText.text);
                RectTransform priceRect = _priceText.rectTransform;
                priceRect.anchorMin = Vector2.zero;
                priceRect.anchorMax = Vector2.one;
                if (hasState && _stateText != null)
                {
                    priceRect.offsetMin = new Vector2(8f, _stateText.rectTransform.offsetMax.y + 2f);
                    priceRect.offsetMax = new Vector2(-8f, -6f);
                }
                else
                {
                    priceRect.offsetMin = new Vector2(8f, 0f);
                    priceRect.offsetMax = new Vector2(-8f, 0f);
                }
            }
        }

        private static void SetRewardRow(TextMeshProUGUI text, int amount, string label)
        {
            if (text == null) return;
            text.text = amount > 0 ? amount.ToString("N0", CultureInfo.CurrentCulture) + " " + label : string.Empty;
            SetRewardRowVisible(text, amount > 0);
        }

        private static void SetRewardRowVisible(TextMeshProUGUI text, bool visible)
        {
            // The prefab contract gives every reward a dedicated row container.
            GameObject row = text.transform.parent != null ? text.transform.parent.gameObject : text.gameObject;
            if (row.activeSelf != visible) row.SetActive(visible);
        }

        private static string Label(string value, string fallback) => string.IsNullOrEmpty(value) ? fallback : value;

        internal static string GetDisplayTitle(ShopItemData item)
        {
            string title = item.Title ?? string.Empty;
            if (item.ItemType != ShopItemType.SkillPack || !title.EndsWith(")", StringComparison.Ordinal)) return title;
            int suffixStart = title.LastIndexOf("(x", StringComparison.OrdinalIgnoreCase);
            if (suffixStart < 0) return title;
            string quantity = title.Substring(suffixStart + 2, title.Length - suffixStart - 3);
            return int.TryParse(quantity, out _) ? title.Substring(0, suffixStart).TrimEnd() : title;
        }
    }
}
