using UnityEngine;

namespace RoyalSmash.Skills
{
    /// <summary>
    /// Configurable parameters for Royal Smash player skills (MegaBall & InfinityBall).
    /// All values exposed via ScriptableObject per Section 2 of Master Build Prompt.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "RoyalSmash/Config/SkillConfig")]
    public class SkillConfig : ScriptableObject
    {
        [Header("Mega Ball (Skill 1)")]
        [Tooltip("Relative diameter multiplier of Mega Ball compared to normal projectile (default 2.0).")]
        [SerializeField] private float _megaBallDiameterMultiplier = 2.0f;

        [Tooltip("Mass multiplier applied to Mega Ball Rigidbody for greater momentum and impact force.")]
        [SerializeField] private float _megaBallMassMultiplier = 2.5f;

        [Tooltip("Initial number of free uses provided to player.")]
        [SerializeField] private int _megaBallDefaultUses = 3;

        [Tooltip("Minimum level required to unlock Mega Ball (default 5).")]
        [SerializeField] private int _megaBallUnlockLevel = 5;

        [Tooltip("Gold cost to purchase additional use of Mega Ball.")]
        [SerializeField] private int _megaBallUsePrice = 50;

        [Header("Infinity Ball (Skill 2)")]
        [Tooltip("Active duration in seconds where firing cooldown is bypassed (default 5.0s).")]
        [SerializeField] private float _infinityBallDuration = 5.0f;

        [Tooltip("Rapid fire cooldown in seconds while Infinity Ball is active (default 0.06s).")]
        [SerializeField] private float _infinityBallRapidCooldown = 0.06f;

        [Tooltip("Initial number of free uses provided to player.")]
        [SerializeField] private int _infinityBallDefaultUses = 3;

        [Tooltip("Minimum level required to unlock Infinity Ball (default 10).")]
        [SerializeField] private int _infinityBallUnlockLevel = 10;

        [Tooltip("Gold cost to purchase additional use of Infinity Ball.")]
        [SerializeField] private int _infinityBallUsePrice = 100;

        // Public Accessors
        public float MegaBallDiameterMultiplier => _megaBallDiameterMultiplier;
        public float MegaBallMassMultiplier => _megaBallMassMultiplier;
        public int MegaBallDefaultUses => _megaBallDefaultUses;
        public int MegaBallUnlockLevel => _megaBallUnlockLevel;
        public int MegaBallUsePrice => _megaBallUsePrice;

        public float InfinityBallDuration => _infinityBallDuration;
        public float InfinityBallRapidCooldown => _infinityBallRapidCooldown;
        public int InfinityBallDefaultUses => _infinityBallDefaultUses;
        public int InfinityBallUnlockLevel => _infinityBallUnlockLevel;
        public int InfinityBallUsePrice => _infinityBallUsePrice;
    }
}
