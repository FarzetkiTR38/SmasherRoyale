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
        [SerializeField] private TrailRenderer _trailRenderer;

        private ProjectileConfig _config;
        private Vector3 _originalScale;
        private Material _instancedMaterial;
        private Coroutine _lifetimeCoroutine;

        private bool _isMegaBall;

        public Rigidbody Rigidbody => _rigidbody;
        public bool IsMegaBall => _isMegaBall;
        public TrailRenderer Trail => _trailRenderer;

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
            if (_trailRenderer == null) _trailRenderer = GetComponent<TrailRenderer>();

            _originalScale = transform.localScale;

            if (_renderer != null)
            {
                _instancedMaterial = _renderer.material;
            }
        }

        private void FixedUpdate()
        {
            // Boundary kill zone: if ball falls below floor or flies too far off screen, clean up immediately
            if (transform.position.y < -10f || transform.position.sqrMagnitude > 4000f)
            {
                if (RoyalSmash.Levels.LevelManager.Instance != null)
                {
                    RoyalSmash.Levels.LevelManager.Instance.UnregisterProjectile(this);
                }
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instancedMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_instancedMaterial);
                }
                else
                {
                    DestroyImmediate(_instancedMaterial);
                }
            }

            if (RoyalSmash.Levels.LevelManager.Instance != null)
            {
                RoyalSmash.Levels.LevelManager.Instance.UnregisterProjectile(this);
            }
        }

        /// <summary>
        /// Applies MegaBall physical scaling and mass multiplication.
        /// </summary>
        public void ApplyMegaBall(float diameterMultiplier, float massMultiplier)
        {
            _isMegaBall = true;
            _originalScale = _originalScale * diameterMultiplier;
            transform.localScale = _originalScale;

            if (_rigidbody != null)
            {
                float baseMass = _config != null ? _config.Mass : _rigidbody.mass;
                _rigidbody.mass = baseMass * massMultiplier;
            }

            if (_renderer != null && _instancedMaterial != null)
            {
                // Vibrant gold/orange tint for MegaBall
                _instancedMaterial.color = new Color(1.0f, 0.65f, 0.10f);
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.startWidth = 0.55f;
                _trailRenderer.endWidth = 0.05f;
                _trailRenderer.startColor = new Color(1.0f, 0.65f, 0.10f, 0.9f);
                _trailRenderer.endColor = new Color(1.0f, 0.25f, 0.0f, 0f);
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

            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.emitting = true;
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
