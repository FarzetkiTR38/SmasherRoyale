using UnityEngine;

namespace RoyalSmash.Economy
{
    /// <summary>
    /// Configuration for level completion rewards based on the locked economy specification:
    /// Total Gold = Base Level Reward + (Remaining Balls * RemainingBallMultiplier).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelRewardConfig", menuName = "RoyalSmash/Config/LevelRewardConfig")]
    public class LevelRewardConfig : ScriptableObject
    {
        [Header("Reward Parameters")]
        [Tooltip("Base gold reward granted upon clearing a level.")]
        [SerializeField] private int _baseLevelReward = 50;

        [Tooltip("Gold multiplier per remaining unused ball. LOCKED requirement: multiplier is 2.")]
        [SerializeField] private int _remainingBallMultiplier = 2;

        public int BaseLevelReward => _baseLevelReward;
        public int RemainingBallMultiplier => _remainingBallMultiplier;

        /// <summary>
        /// Calculates total reward gold based on remaining ammunition using the locked formula.
        /// </summary>
        public int CalculateReward(int remainingBalls)
        {
            int clampedBalls = Mathf.Max(0, remainingBalls);
            return _baseLevelReward + (clampedBalls * _remainingBallMultiplier);
        }
    }
}
