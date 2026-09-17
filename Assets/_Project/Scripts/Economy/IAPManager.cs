using System;
using System.Collections;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Skills;

namespace RoyalSmash.Economy
{
    /// <summary>
    /// Central manager for In-App Purchases and Shop transactions.
    /// Handles double-click prevention, item fulfillment, error states, and save persistence.
    /// Follows Master Build Prompt Section 14, 15, and Phase 13 specifications.
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        private static IAPManager _instance;
        public static IAPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<IAPManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("IAPManager");
                        _instance = go.AddComponent<IAPManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
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

        public bool IsPurchaseInProgress { get; private set; } = false;
        public bool SimulateNextPurchaseFailure { get; set; } = false;

        public event Action<ShopItemData> OnPurchaseCompleted;
        public event Action<string> OnPurchaseFailed;
        public event Action<bool> OnPurchaseStateChanged;

        private void Awake()
        {
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

        public void SetConfig(ShopConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Initiates a purchase transaction with double-click guard and validation.
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

            StartCoroutine(ExecutePurchaseRoutine(item, onComplete));
            return true;
        }

        private IEnumerator ExecutePurchaseRoutine(ShopItemData item, Action<bool, string> onComplete)
        {
            SetPurchaseInProgress(true);

            // Simulate slight network/store latency
            if (Application.isPlaying)
            {
                yield return new WaitForSecondsRealtime(0.25f);
            }

            // Check for simulated failure
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

            // If purchased with In-Game Gold
            if (item.Currency == PriceCurrency.InGameGold)
            {
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
            }

            // Fulfill Rewards
            FulfillRewards(item);

            Debug.Log($"[IAPManager] Successfully fulfilled purchase for '{item.Title}'.");
            OnPurchaseCompleted?.Invoke(item);
            onComplete?.Invoke(true, "Purchase successful!");

            SetPurchaseInProgress(false);
        }

        private void FulfillRewards(ShopItemData item)
        {
            // Gold Reward
            if (item.GoldReward > 0 && GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(item.GoldReward, $"IAPReward_{item.Id}");
            }

            // Mega Ball Uses
            if (item.MegaBallReward > 0 && SkillManager.Instance != null)
            {
                SkillManager.Instance.AddUses(SkillType.MegaBall, item.MegaBallReward);
            }

            // Infinity Ball Uses
            if (item.InfinityBallReward > 0 && SkillManager.Instance != null)
            {
                SkillManager.Instance.AddUses(SkillType.InfinityBall, item.InfinityBallReward);
            }

            // No Ads Grant
            if (item.GrantsNoAds && SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.NoAdsPurchased = true;
                SaveManager.Instance.Save();
                Debug.Log("[IAPManager] No Ads permanently unlocked on player save.");
            }
        }

        private void SetPurchaseInProgress(bool inProgress)
        {
            IsPurchaseInProgress = inProgress;
            OnPurchaseStateChanged?.Invoke(inProgress);
        }
    }
}
