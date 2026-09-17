using UnityEngine;

namespace RoyalSmash.Economy
{
    /// <summary>
    /// Configuration for the Gold economy parameters, pricing, and starting balances.
    /// Follows Master Build Prompt Section 1.4 & Section 13.
    /// </summary>
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "RoyalSmash/Config/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Starting Economy")]
        [Tooltip("Initial gold balance allocated to new players. Configurable (default: 1000).")]
        [SerializeField] private int _startingGold = 1000;

        [Header("Gold Sinks & Purchases")]
        [Tooltip("Gold cost to purchase +1 life when depleted.")]
        [SerializeField] private int _lifePurchasePrice = 100;

        [Tooltip("Gold cost to purchase +1 Mega Ball skill use.")]
        [SerializeField] private int _megaBallPurchasePrice = 50;

        [Tooltip("Gold cost to purchase +1 Infinity Ball skill use.")]
        [SerializeField] private int _infinityBallPurchasePrice = 50;

        public int StartingGold => _startingGold;
        public int LifePurchasePrice => _lifePurchasePrice;
        public int MegaBallPurchasePrice => _megaBallPurchasePrice;
        public int InfinityBallPurchasePrice => _infinityBallPurchasePrice;
    }
}
