using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Configuration data for projectile flight physics, lifetime, cooldown, and level ammunition.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "RoyalSmash/Config/ProjectileConfig")]
    public class ProjectileConfig : ScriptableObject
    {
        [Header("Ballistics & Physics")]
        [Tooltip("Horizontal launch speed towards the target point (m/s).")]
        [SerializeField] private float _launchSpeed = 22f;

        [Tooltip("Rigidbody mass in kg.")]
        [SerializeField] private float _mass = 1.5f;

        [Tooltip("Bounciness of the ball physic material (0-1).")]
        [SerializeField] private float _bounciness = 0.45f;

        [Tooltip("Friction of the ball physic material.")]
        [SerializeField] private float _friction = 0.3f;

        [Header("Lifetime & Destruction")]
        [Tooltip("Total lifetime before auto-destruction in seconds.")]
        [SerializeField] private float _lifetime = 5.0f;

        [Tooltip("Duration of the shrink/fade effect at the end of lifetime.")]
        [SerializeField] private float _fadeDuration = 1.0f;

        [Header("Firing Rules")]
        [Tooltip("Cooldown delay in seconds between consecutive shots.")]
        [SerializeField] private float _fireCooldown = 0.5f;

        [Tooltip("Default number of balls available per level.")]
        [SerializeField] private int _defaultBallCount = 25;

        public float LaunchSpeed => _launchSpeed;
        public float Mass => _mass;
        public float Bounciness => _bounciness;
        public float Friction => _friction;
        public float Lifetime => _lifetime;
        public float FadeDuration => _fadeDuration;
        public float FireCooldown => _fireCooldown;
        public int DefaultBallCount => _defaultBallCount;
    }
}
