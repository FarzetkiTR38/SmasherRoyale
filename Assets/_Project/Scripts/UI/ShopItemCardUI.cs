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

        private void Start()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseStateChanged += HandlePurchaseStateChanged;
                IAPManager.Instance.OnStoreInitialized += HandleStoreInitialized;
            }

            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (IAPManager.HasInstance)
            {
                IAPManager.Instance.OnPurchaseStateChanged -= HandlePurchaseStateChanged;
                IAPManager.Instance.OnStoreInitialized -= HandleStoreInitialized;
            }
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
            ShopItemData item = ItemData;
            RefreshDisplay(item);
        }

        private void RefreshDisplay(ShopItemData item, bool? purchaseInProgress = null)
        {
            if (item == null)
            {
                ClearDynamicDisplay();
                return;
            }

            if (_titleText != null) _titleText.text = item.Title;
            if (_descriptionText != null) _descriptionText.text = item.Description;

            if (_iconImage != null)
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
                if (_priceText != null) _priceText.text = "OWNED";
                if (_buyButton != null) _buyButton.interactable = false;
                ApplyOwnedTint();
                return;
            }

            RestoreOwnedTint();
            string priceToDisplay;
            if (item.Currency == PriceCurrency.InGameGold)
            {
                priceToDisplay = item.CostInGold.ToString("N0", CultureInfo.CurrentCulture) + " Altın";
            }
            else
            {
                priceToDisplay = item.PriceString;
                if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized && !string.IsNullOrEmpty(item.ProductId))
                {
                    priceToDisplay = IAPManager.Instance.GetLocalizedPriceString(item.ProductId, item.PriceString);
                }
            }

            if (_priceText != null) _priceText.text = priceToDisplay;
            if (_buyButton != null)
            {
                bool locked = purchaseInProgress ??
                              (IAPManager.Instance != null && IAPManager.Instance.IsPurchaseInProgress);
                _buyButton.interactable = !locked;
            }
        }

        private void ClearDynamicDisplay()
        {
            if (_titleText != null) _titleText.text = string.Empty;
            if (_descriptionText != null) _descriptionText.text = string.Empty;
            if (_priceText != null) _priceText.text = string.Empty;
            if (_badgeText != null) _badgeText.text = string.Empty;
            if (_badgeRoot != null) _badgeRoot.SetActive(false);
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
            return item.GrantsNoAds &&
                   SaveManager.Instance != null &&
                   SaveManager.Instance.Data != null &&
                   SaveManager.Instance.Data.NoAdsPurchased;
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
    }
}
