using System.Collections;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Individual 3D physics fragment of a shattered Core block.
    /// Handles physical tumble, collision with stand/blocks, and smooth shrink-fade despawn.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ShatteredDebrisPiece : MonoBehaviour
    {
        private Rigidbody _rb;
        private Collider _col;
        private Vector3 _initialScale;
        private bool _isInitialized = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _initialScale = transform.localScale;
        }

        /// <summary>
        /// Activates physics and applies explosion and directional impulses.
        /// </summary>
        public void Initialize(Vector3 impactPoint, Vector3 impactVelocity, float explosionForce)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_col == null) _col = GetComponent<Collider>();
            _initialScale = transform.localScale;

            // Detach from parent so root scale/transform changes don't affect pieces
            transform.SetParent(null, true);

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.useGravity = true;
                _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rb.interpolation = RigidbodyInterpolation.Interpolate;

                // Calculate explosive trajectory: radial spread in X/Y with forward spray towards camera (-Z)
                Vector3 offset = transform.position - impactPoint;
                Vector3 outwardDir = new Vector3(
                    (Mathf.Abs(offset.x) > 0.02f ? Mathf.Sign(offset.x) : (Random.value > 0.5f ? 1f : -1f)) * Random.Range(0.6f, 1.4f),
                    Random.Range(0.4f, 1.3f),             // Upward arc
                    Random.Range(-1.5f, -0.3f)            // Burst towards camera (-Z) into full player view
                ).normalized;

                float speed = Mathf.Clamp(explosionForce * Random.Range(0.85f, 1.35f), 3.2f, 7.5f);
                Vector3 initialVelocity = outwardDir * speed;

                _rb.linearVelocity = initialVelocity;

                // 3D angular tumble
                _rb.angularVelocity = new Vector3(
                    Random.Range(-12f, 12f),
                    Random.Range(-12f, 12f),
                    Random.Range(-12f, 12f)
                );
            }

            if (Application.isPlaying)
            {
                StartCoroutine(LifetimeRoutine());
            }
        }

        private IEnumerator LifetimeRoutine()
        {
            // Let the piece physically tumble, bounce off blocks and stand for 1.8 seconds
            yield return new WaitForSeconds(1.8f);

            // Disable collider so it doesn't block ongoing gameplay
            if (_col != null) _col.enabled = false;
            if (_rb != null)
            {
                _rb.linearVelocity *= 0.3f;
                _rb.angularVelocity *= 0.3f;
            }

            // Smoothly shrink to zero over 0.45 seconds
            float elapsed = 0f;
            float duration = 0.45f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // EaseIn back or smooth cubic shrink
                float scaleFactor = 1f - (t * t);
                transform.localScale = startScale * scaleFactor;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
