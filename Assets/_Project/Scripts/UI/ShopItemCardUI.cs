using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Economy;
using RoyalSmash.Save;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Component representing an individual shop product card or buy button.
    /// Handles price display, localized currency text, owned state for No Ads,
    /// and purchase triggering with double-tap lock.
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

        [SerializeField] private ShopItemData _itemData;

        public ShopItemData ItemData => _itemData;

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

        public void Setup(ShopItemData data)
        {
            _itemData = data;
            RefreshDisplay();
        }

        public void BindReferences(
            TextMeshProUGUI titleTxt, TextMeshProUGUI descTxt, TextMeshProUGUI priceTxt,
            Button buyBtn, Image iconImg, GameObject badgeRt = null, TextMeshProUGUI badgeTxt = null)
        {
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
            if (_itemData == null) return;

            if (_titleText != null) _titleText.text = _itemData.Title;
            if (_descriptionText != null) _descriptionText.text = _itemData.Description;

            if (_iconImage != null && _itemData.Icon != null)
            {
                _iconImage.sprite = _itemData.Icon;
                _iconImage.color = _itemData.ThemeColor;
            }

            if (_badgeRoot != null)
            {
                bool hasBadge = !string.IsNullOrEmpty(_itemData.BadgeText);
                _badgeRoot.SetActive(hasBadge);
                if (hasBadge && _badgeText != null)
                {
                    _badgeText.text = _itemData.BadgeText;
                }
            }

            // Check if No Ads is already owned
            bool isNoAdsOwned = _itemData.GrantsNoAds &&
                                SaveManager.Instance != null &&
                                SaveManager.Instance.Data != null &&
                                SaveManager.Instance.Data.NoAdsPurchased;

            if (isNoAdsOwned)
            {
                if (_priceText != null) _priceText.text = "OWNED";
                if (_buyButton != null)
                {
                    _buyButton.interactable = false;
                    var img = _buyButton.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.4f, 0.4f, 0.45f, 0.8f);
                }
            }
            else
            {
                // Check for live store localized price if available, else fallback to configured price string
                string priceToDisplay = _itemData.PriceString;
                if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized && !string.IsNullOrEmpty(_itemData.ProductId))
                {
                    priceToDisplay = IAPManager.Instance.GetLocalizedPriceString(_itemData.ProductId, _itemData.PriceString);
                }

                if (_priceText != null) _priceText.text = priceToDisplay;

                if (_buyButton != null)
                {
                    _buyButton.interactable = IAPManager.Instance == null || !IAPManager.Instance.IsPurchaseInProgress;
                }
            }
        }

        private void HandleBuyClicked()
        {
            if (_itemData == null)
            {
                Debug.LogWarning($"[ShopItemCardUI] Cannot buy: ItemData is null on {gameObject.name}");
                return;
            }

            Debug.Log($"[ShopItemCardUI] Buy clicked for '{_itemData.Title}' (Id={_itemData.Id}, ProductId={_itemData.ProductId}, Price={_itemData.PriceString})");

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.PurchaseItem(_itemData);
            }
            else
            {
                Debug.LogError("[ShopItemCardUI] IAPManager.Instance is null! Cannot initiate purchase.");
            }
        }

        private void HandlePurchaseStateChanged(bool inProgress)
        {
            if (_buyButton != null)
            {
                bool isNoAdsOwned = _itemData != null && _itemData.GrantsNoAds &&
                                    SaveManager.Instance != null &&
                                    SaveManager.Instance.Data != null &&
                                    SaveManager.Instance.Data.NoAdsPurchased;

                _buyButton.interactable = !inProgress && !isNoAdsOwned;
            }
        }

        private void HandleStoreInitialized()
        {
            RefreshDisplay();
        }
    }
}
