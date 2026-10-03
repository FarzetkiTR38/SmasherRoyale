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
        [SerializeField] private float _launchSpeed = 25f;

        [Tooltip("Rigidbody mass in kg.")]
        [SerializeField] private float _mass = 1.5f;

        [Tooltip("Physical impact impulse (N·s / push punch) transferred to hit blocks on direct impact.")]
        [SerializeField] private float _impactImpulse = 35f;

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
        [SerializeField] private float _fireCooldown = 0.15f;

        [Tooltip("Default number of balls available per level.")]
        [SerializeField] private int _defaultBallCount = 25;

        public float LaunchSpeed => _launchSpeed;
        public float Mass => _mass;
        public float ImpactImpulse => _impactImpulse;
        public float Bounciness => _bounciness;
        public float Friction => _friction;
        public float Lifetime => _lifetime;
        public float FadeDuration => _fadeDuration;
        public float FireCooldown => _fireCooldown;
        public int DefaultBallCount => _defaultBallCount;

        public void SetBallistics(float launchSpeed, float mass, float impactImpulse, float bounciness, float friction)
        {
            _launchSpeed = launchSpeed;
            _mass = mass;
            _impactImpulse = impactImpulse;
            _bounciness = bounciness;
            _friction = friction;
        }

        public void SetFiringRules(float fireCooldown, int defaultBallCount, float lifetime, float fadeDuration)
        {
            _fireCooldown = fireCooldown;
            _defaultBallCount = defaultBallCount;
            _lifetime = lifetime;
            _fadeDuration = fadeDuration;
        }
    }
}
