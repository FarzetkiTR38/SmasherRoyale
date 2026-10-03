using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RoyalSmash.UI
{
    [Serializable]
    public sealed class ShopProductArtwork
    {
        [Tooltip("ShopConfig item Id; never a second purchase or reward definition.")]
        public string ItemId;
        public Sprite Artwork;
        public Sprite BackgroundOverride;
    }

    /// <summary>Editable shop wording and separate art. Economics remain in ShopConfig.</summary>
    [CreateAssetMenu(fileName = "ShopPresentationConfig", menuName = "RoyalSmash/UI/Shop Presentation")]
    public sealed class ShopPresentationConfig : ScriptableObject
    {
        [Header("Editable Labels")]
        public string ShopTitle = "ROYAL BAZAR";
        public string GoldSectionTitle = "ALTIN PAKETLERİ";
        public string SkillSectionTitle = "TOP PAKETLERİ";
        public string BundleSectionTitle = "DEĞER PAKETLERİ";
        public string OwnedLabel = "SATIN ALINDI";
        public string PendingLabel = "İŞLEM SÜRÜYOR…";
        public string UnavailableLabel = "KULLANILAMIYOR";
        public string GoldRewardLabel = "Altın";
        public string MegaRewardLabel = "Mega Top";
        public string InfinityRewardLabel = "Sonsuz Top";
        public string NoAdsDescription = "Zorunlu reklamları ömür boyu kaldırın!";

        [Header("Typography and Colours")]
        public TMP_FontAsset Font;
        public Color TitleColor = new Color(1f, .87f, .35f, 1f);
        public Color BodyColor = Color.white;

        [Header("Text-Free Skin Sprites — filled during 34.3")]
        public Sprite ShopFrame;
        public Sprite HeaderBackground;
        public Sprite SectionBackground;
        public Sprite FeaturedBackground;
        public Sprite CompactBackground;
        public Sprite NoAdsBackground;
        public Sprite BuyBackground;
        public Sprite BadgeBackground;

        [Header("Independent Product Artwork — filled during 34.3")]
        public List<ShopProductArtwork> Products = new List<ShopProductArtwork>();

        public ShopProductArtwork FindArtwork(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Products == null) return null;
            foreach (ShopProductArtwork product in Products)
                if (product != null && product.ItemId == itemId) return product;
            return null;
        }
    }
}
