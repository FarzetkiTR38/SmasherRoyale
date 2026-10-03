using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using RoyalSmash.Save;
using RoyalSmash.Skills;

namespace RoyalSmash.Economy
{
    /// <summary>
    /// Central manager for In-App Purchases and Shop transactions.
    /// Implements Unity Purchasing IDetailedStoreListener with Google Play Billing integration
    /// and fallback simulation for Editor/offline test execution.
    /// Handles double-click prevention, item fulfillment, error states, and save persistence.
    /// Follows Master Build Prompt Section 14, 15, and Phase 13 specifications.
    /// </summary>
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        private static IAPManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static IAPManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<IAPManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("IAPManager");
                        _instance = go.AddComponent<IAPManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private ShopConfig _config;

        public ShopConfig Config
        {
            get
            {
                if (_config == null)
                {
#if UNITY_EDITOR
                    _config = UnityEditor.AssetDatabase.LoadAssetAtPath<ShopConfig>("Assets/_Project/ScriptableObjects/Config/ShopConfig.asset");
#endif
                }
                return _config;
            }
        }

        // Unity Purchasing Store references
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private bool _isInitializingPurchasing = false;

        public bool IsInitialized => _storeController != null;
        public bool IsPurchaseInProgress { get; private set; } = false;
        public bool SimulateNextPurchaseFailure { get; set; } = false;

        // Pending transaction tracking
        private ShopItemData _pendingItem;
        private Action<bool, string> _pendingCallback;

        // Public events
        public event Action<ShopItemData> OnPurchaseCompleted;
        public event Action<string> OnPurchaseFailed;
        public event Action<bool> OnPurchaseStateChanged;
        public event Action OnStoreInitialized;

        private void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializePurchasing();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void SetConfig(ShopConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Initializes the Unity In-App Purchasing service.
        /// Configures Google Play and Apple App Store product catalogs from ShopConfig.
        /// </summary>
        public void InitializePurchasing()
        {
            if (IsInitialized || _isInitializingPurchasing)
            {
                return;
            }

            if (Config == null)
            {
                Debug.LogWarning("[IAPManager] Cannot initialize purchasing: ShopConfig is null.");
                return;
            }

            _isInitializingPurchasing = true;
            Debug.Log("[IAPManager] Initializing Unity Purchasing...");

            try
            {
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                var allItems = Config.GetAllItems();

                foreach (var item in allItems)
                {
                    if (string.IsNullOrEmpty(item.ProductId)) continue;

                    // NoAds is Non-Consumable; Currency, Balls, and Bundles are Consumable
                    ProductType pType = item.GrantsNoAds ? ProductType.NonConsumable : ProductType.Consumable;
                    builder.AddProduct(item.ProductId, pType);
                    Debug.Log($"[IAPManager] Registered store product: {item.ProductId} ({pType})");
                }

                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception ex)
            {
                _isInitializingPurchasing = false;
                Debug.LogError($"[IAPManager] Unity Purchasing initialization exception: {ex.Message}");
            }
        }

        #region Unity Purchasing Listener Callbacks

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _isInitializingPurchasing = false;
            _storeController = controller;
            _extensionProvider = extensions;

            Debug.Log($"[IAPManager] Unity Purchasing successfully initialized. Available products: {controller.products.all.Length}");

            // Verify if non-consumable (No Ads) was previously purchased on this store account
            if (Config != null && Config.NoAdsItem != null && !string.IsNullOrEmpty(Config.NoAdsItem.ProductId))
            {
                var noAdsProduct = controller.products.WithID(Config.NoAdsItem.ProductId);
                if (noAdsProduct != null && noAdsProduct.hasReceipt)
                {
                    if (SaveManager.Instance != null && SaveManager.Instance.Data != null && !SaveManager.Instance.Data.NoAdsPurchased)
                    {
                        SaveManager.Instance.Data.NoAdsPurchased = true;
                        SaveManager.Instance.Save();
                        Debug.Log("[IAPManager] Restored 'No Ads' purchase from Google Play store receipt.");
                    }
                }
            }

            OnStoreInitialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _isInitializingPurchasing = false;
            Debug.LogWarning($"[IAPManager] Unity Purchasing initialization failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _isInitializingPurchasing = false;
            Debug.LogWarning($"[IAPManager] Unity Purchasing initialization failed: {error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string purchasedId = args.purchasedProduct.definition.id;
            Debug.Log($"[IAPManager] ProcessPurchase received for product: {purchasedId}");

            ShopItemData item = null;
            if (Config != null)
            {
                item = Config.FindItemById(purchasedId);
            }

            if (item == null && _pendingItem != null && (_pendingItem.ProductId == purchasedId || _pendingItem.Id == purchasedId))
            {
                item = _pendingItem;
            }

            if (item != null)
            {
                FulfillRewards(item);
                Debug.Log($"[IAPManager] Successfully processed store purchase for '{item.Title}'.");

                _pendingCallback?.Invoke(true, "Purchase successful!");
                OnPurchaseCompleted?.Invoke(item);
            }
            else
            {
                Debug.LogWarning($"[IAPManager] Purchased product '{purchasedId}' not found in ShopConfig!");
                _pendingCallback?.Invoke(true, "Purchase completed.");
            }

            ClearPendingTransaction();
            SetPurchaseInProgress(false);

            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string prodId = product != null ? product.definition.id : "Unknown";
            string errorMsg = $"Purchase failed for '{prodId}': {failureReason}";
            Debug.LogWarning($"[IAPManager] {errorMsg}");

            _pendingCallback?.Invoke(false, errorMsg);
            OnPurchaseFailed?.Invoke(errorMsg);

            ClearPendingTransaction();
            SetPurchaseInProgress(false);
        }

        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string prodId = product != null ? product.definition.id : "Unknown";
            string msg = failureDescription != null ? failureDescription.message : "Transaction failed";
            string errorMsg = $"Purchase failed for '{prodId}': {msg}";
            Debug.LogWarning($"[IAPManager] {errorMsg}");

            _pendingCallback?.Invoke(false, errorMsg);
            OnPurchaseFailed?.Invoke(errorMsg);

            ClearPendingTransaction();
            SetPurchaseInProgress(false);
        }

        #endregion

        /// <summary>
        /// Retrieves the localized price string formatted by Google Play Console (e.g. ₺149,99, $4.99).
        /// Returns fallback string if store is uninitialized or product not found.
        /// </summary>
        public string GetLocalizedPriceString(string productId, string fallback = null)
        {
            if (IsInitialized && !string.IsNullOrEmpty(productId))
            {
                var product = _storeController.products.WithID(productId);
                if (product != null && product.metadata != null && !string.IsNullOrEmpty(product.metadata.localizedPriceString))
                {
                    return product.metadata.localizedPriceString;
                }
            }
            return fallback;
        }

        [Header("Editor Testing Options")]
        public bool UseFakeStoreUIInEditor = false;

        /// <summary>
        /// Initiates a purchase transaction with double-click guard and validation.
        /// Works seamlessly with Google Play Billing when built for mobile,
        /// and provides instant test simulation with UI feedback in the Unity Editor.
        /// </summary>
        public bool PurchaseItem(ShopItemData item, Action<bool, string> onComplete = null)
        {
            if (item == null)
            {
                onComplete?.Invoke(false, "Invalid item");
                return false;
            }

            // Double-click prevention (Master Build Prompt Phase 13 Acceptance Criterion 8)
            if (IsPurchaseInProgress)
            {
                Debug.LogWarning($"[IAPManager] Purchase rejected: transaction already in progress for '{item.Title}'.");
                onComplete?.Invoke(false, "Transaction already in progress");
                return false;
            }

            // Check if No Ads is already owned
            if (item.GrantsNoAds && SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.NoAdsPurchased)
            {
                Debug.Log("[IAPManager] No Ads is already purchased.");
                onComplete?.Invoke(false, "Item already owned");
                return false;
            }

            // In-Game Gold purchase check
            if (item.Currency == PriceCurrency.InGameGold)
            {
                StartCoroutine(ExecuteInGameGoldPurchaseRoutine(item, onComplete));
                return true;
            }

            // Real Money IAP with Unity Purchasing on Mobile Builds (or if Editor FakeStore UI is enabled)
            bool shouldInitiateLiveStore = IsInitialized && !SimulateNextPurchaseFailure && (!Application.isEditor || UseFakeStoreUIInEditor);
            if (shouldInitiateLiveStore)
            {
                var product = _storeController.products.WithID(item.ProductId);
                if (product != null && product.availableToPurchase)
                {
                    SetPurchaseInProgress(true);
                    _pendingItem = item;
                    _pendingCallback = onComplete;

                    Debug.Log($"[IAPManager] Initiating Google Play store purchase for '{item.ProductId}' ({item.Title})...");
                    _storeController.InitiatePurchase(product);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[IAPManager] Product '{item.ProductId}' not available in store catalog. Falling back to test simulation.");
                }
            }

            // Synchronous execution in Editor test runners outside Play Mode
            if (!Application.isPlaying)
            {
                if (SimulateNextPurchaseFailure)
                {
                    SimulateNextPurchaseFailure = false;
                    string errorMsg = "Store transaction failed: User cancelled or network error.";
                    Debug.LogWarning($"[IAPManager] {errorMsg}");
                    OnPurchaseFailed?.Invoke(errorMsg);
                    onComplete?.Invoke(false, errorMsg);
                    return true;
                }

                FulfillRewards(item);
                Debug.Log($"[IAPManager] [SIMULATION-EDITOR] Fulfilled purchase for '{item.Title}'.");
                OnPurchaseCompleted?.Invoke(item);
                onComplete?.Invoke(true, "Purchase successful!");
                return true;
            }

            // Test simulation mode (Play Mode / Offline / Simulated Failure)
            StartCoroutine(ExecuteSimulatedPurchaseRoutine(item, onComplete));
            return true;
        }

        private IEnumerator ExecuteInGameGoldPurchaseRoutine(ShopItemData item, Action<bool, string> onComplete)
        {
            SetPurchaseInProgress(true);

            if (Application.isPlaying)
            {
                yield return new WaitForSecondsRealtime(0.15f);
            }

            if (GoldManager.Instance != null)
            {
                if (!GoldManager.Instance.SpendGold(item.CostInGold, $"ShopBuy_{item.Id}"))
                {
                    string goldError = "Insufficient Gold balance.";
                    Debug.LogWarning($"[IAPManager] {goldError}");
                    OnPurchaseFailed?.Invoke(goldError);
                    onComplete?.Invoke(false, goldError);
                    SetPurchaseInProgress(false);
                    yield break;
                }
            }

            FulfillRewards(item);
            Debug.Log($"[IAPManager] Successfully fulfilled in-game gold purchase for '{item.Title}'.");
            OnPurchaseCompleted?.Invoke(item);
            onComplete?.Invoke(true, "Purchase successful!");

            SetPurchaseInProgress(false);
        }

        private IEnumerator ExecuteSimulatedPurchaseRoutine(ShopItemData item, Action<bool, string> onComplete)
        {
            SetPurchaseInProgress(true);

            // Simulate network/store processing delay
            if (Application.isPlaying)
            {
                yield return new WaitForSecondsRealtime(0.25f);
            }

            // Check for simulated failure test flag
            if (SimulateNextPurchaseFailure)
            {
                SimulateNextPurchaseFailure = false;
                string errorMsg = "Store transaction failed: User cancelled or network error.";
                Debug.LogWarning($"[IAPManager] {errorMsg}");
                OnPurchaseFailed?.Invoke(errorMsg);
                onComplete?.Invoke(false, errorMsg);
                SetPurchaseInProgress(false);
                yield break;
            }

            // Fulfill Rewards
            FulfillRewards(item);

            Debug.Log($"[IAPManager] [SIMULATION] Successfully fulfilled purchase for '{item.Title}'.");
            OnPurchaseCompleted?.Invoke(item);
            onComplete?.Invoke(true, "Purchase successful!");

            SetPurchaseInProgress(false);
        }

        /// <summary>
        /// Fulfills in-game currencies, skill uses, and unlocks permanently.
        /// </summary>
        private void FulfillRewards(ShopItemData item)
        {
            if (item == null) return;

            // Gold Reward
            if (item.GoldReward > 0)
            {
                if (GoldManager.Instance != null)
                {
                    GoldManager.Instance.AddGold(item.GoldReward, $"IAPReward_{item.Id}");
                }
                else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
                {
                    SaveManager.Instance.Data.Gold += item.GoldReward;
                    SaveManager.Instance.Save();
                }
            }

            // Mega Ball Uses
            if (item.MegaBallReward > 0)
            {
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddUses(SkillType.MegaBall, item.MegaBallReward);
                }
                else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
                {
                    SaveManager.Instance.Data.MegaBallUses += item.MegaBallReward;
                    SaveManager.Instance.Save();
                }
            }

            // Infinity Ball Uses
            if (item.InfinityBallReward > 0)
            {
                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.AddUses(SkillType.InfinityBall, item.InfinityBallReward);
                }
                else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
                {
                    SaveManager.Instance.Data.InfinityBallUses += item.InfinityBallReward;
                    SaveManager.Instance.Save();
                }
            }

            // No Ads Grant
            if (item.GrantsNoAds)
            {
                if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
                {
                    SaveManager.Instance.Data.NoAdsPurchased = true;
                    SaveManager.Instance.Save();
                    Debug.Log("[IAPManager] No Ads permanently unlocked on player save.");
                }
            }
        }

        private void SetPurchaseInProgress(bool inProgress)
        {
            IsPurchaseInProgress = inProgress;
            OnPurchaseStateChanged?.Invoke(inProgress);
        }

        private void ClearPendingTransaction()
        {
            _pendingItem = null;
            _pendingCallback = null;
        }

        /// <summary>
        /// Restores previous purchases for non-consumable items.
        /// Primarily used on iOS or when transferring devices.
        /// </summary>
        public void RestorePurchases(Action<bool, string> onComplete = null)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(false, "Store not initialized");
                return;
            }

            var apple = _extensionProvider?.GetExtension<IAppleExtensions>();
            if (apple != null)
            {
                apple.RestoreTransactions((success, error) =>
                {
                    Debug.Log($"[IAPManager] Apple RestoreTransactions result: {success}, error: {error}");
                    onComplete?.Invoke(success, error);
                });
            }
            else
            {
                Debug.Log("[IAPManager] Google Play automatically handles transaction restoration.");
                onComplete?.Invoke(true, "Restoration complete");
            }
        }
    }
}
