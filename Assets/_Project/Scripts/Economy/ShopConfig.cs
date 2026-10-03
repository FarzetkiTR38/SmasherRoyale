using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Economy
{
    public enum ShopItemType
    {
        GoldPack,
        SkillPack,
        Bundle,
        LimitedOffer,
        NoAds
    }

    public enum PriceCurrency
    {
        RealMoneyIAP,
        InGameGold
    }

    [Serializable]
    public class ShopItemData
    {
        public string Id;
        public string ProductId;
        public string Title;
        [TextArea(2, 3)] public string Description;
        public ShopItemType ItemType;
        public PriceCurrency Currency = PriceCurrency.RealMoneyIAP;
        public string PriceString = "$0.99";
        public int CostInGold = 0;

        [Header("Rewards Fulfilled")]
        public int GoldReward = 0;
        public int MegaBallReward = 0;
        public int InfinityBallReward = 0;
        public bool GrantsNoAds = false;

        [Header("Visual Presentation")]
        public Sprite Icon;
        public Color ThemeColor = Color.white;
        public string BadgeText = "";
    }

    /// <summary>
    /// Configuration for all Shop and IAP offerings.
    /// Follows Master Build Prompt Section 14 specifications.
    /// All prices, product IDs, and rewards are data-driven.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopConfig", menuName = "RoyalSmash/Economy/ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        [Header("Limited-Time Offer")]
        public ShopItemData LimitedTimeOffer;

        [Header("No Ads (One-Time IAP)")]
        public ShopItemData NoAdsItem;

        [Header("Gold Packs")]
        public List<ShopItemData> GoldPacks = new List<ShopItemData>();

        [Header("Skill Packs")]
        public List<ShopItemData> SkillPacks = new List<ShopItemData>();

        [Header("Value Bundles")]
        public List<ShopItemData> Bundles = new List<ShopItemData>();

        public List<ShopItemData> GetAllItems()
        {
            var list = new List<ShopItemData>();
            if (LimitedTimeOffer != null) list.Add(LimitedTimeOffer);
            if (NoAdsItem != null) list.Add(NoAdsItem);
            if (GoldPacks != null) list.AddRange(GoldPacks);
            if (SkillPacks != null) list.AddRange(SkillPacks);
            if (Bundles != null) list.AddRange(Bundles);
            return list;
        }

        public ShopItemData FindItemById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (LimitedTimeOffer != null && (LimitedTimeOffer.Id == id || LimitedTimeOffer.ProductId == id)) return LimitedTimeOffer;
            if (NoAdsItem != null && (NoAdsItem.Id == id || NoAdsItem.ProductId == id)) return NoAdsItem;

            if (GoldPacks != null)
            {
                foreach (var item in GoldPacks)
                {
                    if (item != null && (item.Id == id || item.ProductId == id)) return item;
                }
            }

            if (SkillPacks != null)
            {
                foreach (var item in SkillPacks)
                {
                    if (item != null && (item.Id == id || item.ProductId == id)) return item;
                }
            }

            if (Bundles != null)
            {
                foreach (var item in Bundles)
                {
                    if (item != null && (item.Id == id || item.ProductId == id)) return item;
                }
            }

            return null;
        }
    }
}
