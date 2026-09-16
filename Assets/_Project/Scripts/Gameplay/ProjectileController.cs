using System.Collections;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Controls projectile flight physics, world-coordinate trajectory ballistics,
    /// and graceful auto-destruction (shrink/fade-out) over its lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class ProjectileController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Renderer _renderer;

        private ProjectileConfig _config;
        private Vector3 _originalScale;
        private Material _instancedMaterial;
        private Coroutine _lifetimeCoroutine;

        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();

            _originalScale = transform.localScale;

            if (_renderer != null)
            {
                _instancedMaterial = _renderer.material;
            }
        }

        private void OnDestroy()
        {
            if (_instancedMaterial != null)
            {
                Destroy(_instancedMaterial);
            }

            if (RoyalSmash.Levels.LevelManager.Instance != null)
            {
                RoyalSmash.Levels.LevelManager.Instance.UnregisterProjectile(this);
            }
        }

        /// <summary>
        /// Launch the ball toward the specified 3D world target coordinate with real physics.
        /// Accounts for gravity to hit the target point accurately.
        /// </summary>
        /// <param name="targetPoint">3D world coordinate to hit.</param>
        /// <param name="config">Configuration containing launch speed, lifetime, and fade durations.</param>
        public void Launch(Vector3 targetPoint, ProjectileConfig config)
        {
            _config = config;

            // Ensure physics settings
            _rigidbody.useGravity = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (_config != null)
            {
                _rigidbody.mass = _config.Mass;
            }

            // Calculate ballistic arc
            Vector3 origin = transform.position;
            Vector3 displacement = targetPoint - origin;
            Vector3 displacementXZ = new Vector3(displacement.x, 0f, displacement.z);
            float distanceXZ = displacementXZ.magnitude;

            float launchSpeed = _config != null ? _config.LaunchSpeed : 22f;
            if (distanceXZ < 0.1f) distanceXZ = 0.1f;

            float timeToTarget = distanceXZ / launchSpeed;
            float gravity = Mathf.Abs(Physics.gravity.y);

            // Kinematic formula for vertical velocity to arrive at target Y at timeToTarget
            float vy = (displacement.y + 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;

            Vector3 initialVelocity = (displacementXZ.normalized * launchSpeed) + (Vector3.up * vy);
            _rigidbody.linearVelocity = initialVelocity;

            if (RoyalSmash.Levels.LevelManager.Instance != null)
            {
                RoyalSmash.Levels.LevelManager.Instance.RegisterProjectile(this);
            }

            // Begin lifetime tracker
            if (_lifetimeCoroutine != null) StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            float totalLifetime = _config != null ? _config.Lifetime : 5.0f;
            float fadeDuration = _config != null ? _config.FadeDuration : 1.0f;
            float activeDuration = Mathf.Max(0.1f, totalLifetime - fadeDuration);

            // Normal flight phase
            yield return new WaitForSeconds(activeDuration);

            // Shrink and fade phase (does not pop instantly)
            float elapsed = 0f;
            Color initialColor = Color.red;
            if (_instancedMaterial != null && _instancedMaterial.HasProperty("_BaseColor"))
            {
                initialColor = _instancedMaterial.GetColor("_BaseColor");
            }

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                // Smoothly shrink scale to zero
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);

                // Fade alpha
                if (_instancedMaterial != null && _instancedMaterial.HasProperty("_BaseColor"))
                {
                    Color faded = initialColor;
                    faded.a = 1f - t;
                    _instancedMaterial.SetColor("_BaseColor", faded);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
