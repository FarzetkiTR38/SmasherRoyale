using UnityEngine;

namespace RoyalSmash.Lives
{
    /// <summary>
    /// Configuration asset for the Lives System.
    /// Exposes all life limits, regeneration timing, and economy prices without hardcoding.
    /// Follows Master Build Prompt Section 11 & Section 2.
    /// </summary>
    [CreateAssetMenu(fileName = "LivesConfig", menuName = "RoyalSmash/Config/LivesConfig")]
    public class LivesConfig : ScriptableObject
    {
        [Header("Life Capacity & Initialization")]
        [Tooltip("Maximum number of lives a player can store.")]
        [SerializeField] private int _maxLives = 5;

        [Tooltip("Starting lives on a new game.")]
        [SerializeField] private int _startingLives = 5;

        [Header("Regeneration Timing")]
        [Tooltip("Minutes required to passively regenerate +1 life.")]
        [SerializeField] private float _lifeRegenMinutes = 30.0f;

        [Header("Economy & Purchases")]
        [Tooltip("Gold cost to immediately purchase +1 life.")]
        [SerializeField] private int _lifePriceGold = 100;

        [Tooltip("Number of lives granted upon watching a Rewarded Ad.")]
        [SerializeField] private int _rewardedAdLifeReward = 1;

        public int MaxLives => _maxLives;
        public int StartingLives => _startingLives;
        public float LifeRegenMinutes => _lifeRegenMinutes;
        public int LifePriceGold => _lifePriceGold;
        public int RewardedAdLifeReward => _rewardedAdLifeReward;
    }
}
