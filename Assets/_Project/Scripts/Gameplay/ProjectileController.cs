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

        private void OnCollisionEnter(Collision collision)
        {
            // Restore gravity upon impact so ball bounces and falls naturally
            if (_rigidbody != null)
            {
                _rigidbody.useGravity = true;
            }

            if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
            {
                float impulse = _config != null ? _config.ImpactImpulse : 35f;
                if (impulse > 0f)
                {
                    Vector3 hitDir = _rigidbody != null && _rigidbody.linearVelocity.sqrMagnitude > 0.1f
                        ? _rigidbody.linearVelocity.normalized
                        : (collision.contacts.Length > 0 ? -collision.contacts[0].normal : transform.forward);

                    if (_isMegaBall) impulse *= 2.0f;

                    Vector3 hitPos = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                    collision.rigidbody.AddForceAtPosition(hitDir * impulse, hitPos, ForceMode.Impulse);
                }
            }
        }

        /// <summary>
        /// Launch the ball directly toward the specified 3D world target coordinate.
        /// Flies straight without mid-air gravity drop, restoring gravity upon impact.
        /// </summary>
        /// <param name="targetPoint">3D world coordinate to hit.</param>
        /// <param name="config">Configuration containing launch speed, lifetime, and fade durations.</param>
        public void Launch(Vector3 targetPoint, ProjectileConfig config)
        {
            _config = config;

            // Direct straight flight: disable gravity in air, enable upon collision
            _rigidbody.useGravity = false;
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

            // Direct linear velocity towards target point
            Vector3 origin = transform.position;
            Vector3 direction = (targetPoint - origin).normalized;
            float launchSpeed = _config != null ? _config.LaunchSpeed : 25f;

            _rigidbody.linearVelocity = direction * launchSpeed;

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
