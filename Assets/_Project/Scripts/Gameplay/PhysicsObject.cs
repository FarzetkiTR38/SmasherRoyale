using System;
using System.Collections;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Base component for all placeable target puzzle objects on the stand.
    /// Manages the object state machine (Idle -> Hit -> Moving -> DestroyCondition -> Destroying -> Destroyed),
    /// impulse reactions from balls, fall-off detection, and smooth shrink/melt destruction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class PhysicsObject : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected ObjectConfig _config;

        [Header("Runtime State")]
        [SerializeField] private ObjectState _currentState = ObjectState.Idle;

        protected Rigidbody _rigidbody;
        protected Collider _collider;
        protected Renderer _renderer;
        protected Material _instancedMaterial;
        protected Vector3 _originalScale;
        protected Coroutine _destroyCoroutine;

        // Events
        public event Action<PhysicsObject, ObjectState> OnStateChanged;
        public event Action<PhysicsObject, Collision> OnObjectHit;
        public static event Action<PhysicsObject> OnAnyObjectDestroyed;

        public ObjectState CurrentState => _currentState;
        public ObjectConfig Config => _config;
        public Rigidbody Rigidbody => _rigidbody;
        public Collider Collider => _collider;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponentInChildren<Renderer>();
            _originalScale = transform.localScale;

            if (_renderer != null)
            {
                _instancedMaterial = _renderer.material;
            }

            ApplyConfiguration();
        }

        protected virtual void Start()
        {
            // Set layer to PhysicsObject (Layer 7)
            gameObject.layer = 7;

            if (RoyalSmash.Levels.LevelManager.Instance != null)
            {
                RoyalSmash.Levels.LevelManager.Instance.RegisterTarget(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instancedMaterial != null)
            {
                Destroy(_instancedMaterial);
            }

            if (RoyalSmash.Levels.LevelManager.Instance != null)
            {
                RoyalSmash.Levels.LevelManager.Instance.UnregisterTarget(this);
            }
        }

        /// <summary>
        /// Apply mass, physics properties, and collision settings from ObjectConfig.
        /// </summary>
        public virtual void ApplyConfiguration()
        {
            if (_rigidbody != null && _config != null)
            {
                _rigidbody.mass = _config.Mass;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        protected virtual void FixedUpdate()
        {
            // Detect falling off the stand
            if (_currentState != ObjectState.DestroyCondition &&
                _currentState != ObjectState.Destroying &&
                _currentState != ObjectState.Destroyed)
            {
                float thresholdY = _config != null ? _config.FallOffThresholdY : 0.2f;
                if (transform.position.y < thresholdY)
                {
                    TriggerDestroyCondition("Fell below stand threshold");
                }
                else if (_rigidbody != null && _currentState == ObjectState.Hit)
                {
                    // If moving, transition to Moving state
                    if (_rigidbody.linearVelocity.sqrMagnitude > 0.05f)
                    {
                        SetState(ObjectState.Moving);
                    }
                }
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            // Check if hit by projectile or heavy moving object
            bool isProjectile = collision.gameObject.layer == 6 || collision.gameObject.GetComponent<ProjectileController>() != null;

            if (isProjectile)
            {
                HandleProjectileHit(collision);
            }
            else if (collision.gameObject.layer == 9) // Ground layer
            {
                // Falling onto the ground off the stand triggers destruction
                TriggerDestroyCondition("Collided with Ground layer");
            }
        }

        protected virtual void HandleProjectileHit(Collision collision)
        {
            if (_currentState == ObjectState.Idle)
            {
                SetState(ObjectState.Hit);
            }

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                ObjectType type = _config != null ? _config.ObjectType : ObjectType.Box;
                RoyalSmash.Audio.AudioManager.Instance.PlayImpact(type, collision.impulse.magnitude);
            }

            if (RoyalSmash.VFX.VFXManager.Instance != null)
            {
                Vector3 contactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                Vector3 contactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector3.up;
                RoyalSmash.VFX.VFXManager.Instance.SpawnImpact(contactPoint, contactNormal, collision.impulse.magnitude);
            }

            if (RoyalSmash.Haptics.HapticManager.Instance != null)
            {
                ObjectType type = _config != null ? _config.ObjectType : ObjectType.Box;
                RoyalSmash.Haptics.HapticManager.Instance.TriggerImpactHaptic(type, collision.impulse.magnitude);
            }

            OnObjectHit?.Invoke(this, collision);
            Debug.Log($"[PhysicsObject] {gameObject.name} hit by projectile. Impulse: {collision.impulse.magnitude:F2}");
        }

        /// <summary>
        /// Transition to a new state and notify listeners.
        /// </summary>
        public void SetState(ObjectState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            OnStateChanged?.Invoke(this, _currentState);
        }

        /// <summary>
        /// Trigger the destroy condition (e.g. falling off stand or taking fatal impact).
        /// </summary>
        public virtual void TriggerDestroyCondition(string reason = "")
        {
            if (_currentState == ObjectState.DestroyCondition ||
                _currentState == ObjectState.Destroying ||
                _currentState == ObjectState.Destroyed)
            {
                return;
            }

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayObjectDestruction();
            }

            if (RoyalSmash.VFX.VFXManager.Instance != null)
            {
                Color objColor = _instancedMaterial != null && _instancedMaterial.HasProperty("_BaseColor")
                    ? _instancedMaterial.GetColor("_BaseColor")
                    : Color.white;
                RoyalSmash.VFX.VFXManager.Instance.SpawnDestruction(transform.position, objColor);
            }

            Debug.Log($"[PhysicsObject] {gameObject.name} destroy condition triggered: {reason}");
            SetState(ObjectState.DestroyCondition);

            if (_destroyCoroutine != null) StopCoroutine(_destroyCoroutine);
            _destroyCoroutine = StartCoroutine(DestroyRoutine());
        }

        /// <summary>
        /// Graceful destruction routine: disables physics/colliders, smoothly shrinks and fades, then destroys.
        /// Objects never pop instantly.
        /// </summary>
        protected virtual IEnumerator DestroyRoutine()
        {
            SetState(ObjectState.Destroying);

            // Disable collider so it doesn't block other falling objects during shrink
            if (_collider != null) _collider.enabled = false;

            // Make kinematic to prevent glitchy physics while shrinking
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
            }

            float duration = _config != null ? _config.ShrinkDuration : 0.5f;
            float elapsed = 0f;

            Color initialColor = Color.white;
            if (_instancedMaterial != null && _instancedMaterial.HasProperty("_BaseColor"))
            {
                initialColor = _instancedMaterial.GetColor("_BaseColor");
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smoothly scale to zero
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);

                // Fade alpha if material supports it
                if (_instancedMaterial != null && _instancedMaterial.HasProperty("_BaseColor"))
                {
                    Color faded = initialColor;
                    faded.a = 1f - t;
                    _instancedMaterial.SetColor("_BaseColor", faded);
                }

                yield return null;
            }

            SetState(ObjectState.Destroyed);
            OnAnyObjectDestroyed?.Invoke(this);

            Destroy(gameObject);
        }
    }
}
