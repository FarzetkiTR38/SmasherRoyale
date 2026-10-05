using System.Collections;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Specialized breakable object that shatters on sufficient impact force.
    /// Spawns glass break and paint splatter visual feedback before disappearing.
    /// </summary>
    public class PaintBottleObject : PhysicsObject
    {
        [Header("Paint Bottle Settings")]
        [SerializeField] private Color _paintColor = new Color(0.04f, 0.22f, 0.82f, 1f); // Deep Royal Sapphire / Lacivert
        [SerializeField] private bool _hasBroken = false;

        public Color PaintColor => _paintColor;
        public bool HasBroken => _hasBroken;

        public void SetColor(Color color)
        {
            _paintColor = color;
            var rend = GetComponentInChildren<Renderer>();
            if (rend != null && rend.material != null)
            {
                if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", color);
                if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", color);
            }
        }

        protected override void Start()
        {
            base.Start();

            // Start in sleep state so grid remains rock-solid until projectile impact
            if (_rigidbody != null)
            {
                _rigidbody.Sleep();
            }
        }

        [Header("Fractured Mesh Settings")]
        [SerializeField] private GameObject _fracturedPrefab;

        public GameObject FracturedPrefab { get => _fracturedPrefab; set => _fracturedPrefab = value; }

        protected override void HandleProjectileHit(Collision collision)
        {
            base.HandleProjectileHit(collision);

            if (_hasBroken) return;

            // Direct ball hit shatters the paint bottle into physical 3D debris
            Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            Vector3 ballVelocity = collision.relativeVelocity.sqrMagnitude > 0.1f ? -collision.relativeVelocity : Vector3.forward * 12f;
            BreakBottle(impactPoint, ballVelocity);
        }

        /// <summary>
        /// Shatters the paint bottle into 3D physics debris pieces and triggers visual feedback.
        /// </summary>
        public void BreakBottle(Vector3 impactPoint, Vector3 impactVelocity = default)
        {
            if (_hasBroken) return;
            _hasBroken = true;

            if (impactVelocity == default)
            {
                impactVelocity = Vector3.forward * 12f;
            }

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayBottleBreak();
            }

            if (RoyalSmash.Haptics.HapticManager.Instance != null)
            {
                RoyalSmash.Haptics.HapticManager.Instance.TriggerBottleBreakHaptic();
            }

            // 1. Spawn 3D Fractured Physics Debris Pieces!
            if (_fracturedPrefab != null)
            {
                var fracturedInstance = Instantiate(_fracturedPrefab, transform.position, transform.rotation);
                fracturedInstance.transform.localScale = transform.localScale;

                var controller = fracturedInstance.GetComponent<FracturedCoreObject>();
                if (controller != null)
                {
                    controller.TriggerShatter(impactPoint, impactVelocity);
                }
            }

            // 2. Compact impact flash & sparkle VFX pop
            if (RoyalSmash.VFX.VFXManager.Instance != null)
            {
                RoyalSmash.VFX.VFXManager.Instance.SpawnBottleBreak(impactPoint, _paintColor);
            }

            Debug.Log($"[PaintBottleObject] Shattered into 3D physical pieces at {impactPoint}!");

            // Hide the bottle body immediately
            if (_renderer != null) _renderer.enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            if (_collider != null) _collider.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
            }

            if (_destroyCoroutine != null) StopCoroutine(_destroyCoroutine);
            _destroyCoroutine = StartCoroutine(BreakShatterRoutine());
        }

        private IEnumerator BreakShatterRoutine()
        {
            SetState(ObjectState.DestroyCondition);
            yield return null;
            SetState(ObjectState.Destroying);

            yield return new WaitForSeconds(0.4f);

            SetState(ObjectState.Destroyed);
            Destroy(gameObject);
        }
    }
}
