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
        [SerializeField] private Color _paintColor = new Color(0.85f, 0.27f, 0.94f, 1f); // Vibrant magenta
        [SerializeField] private bool _hasBroken = false;

        public Color PaintColor => _paintColor;
        public bool HasBroken => _hasBroken;

        protected override void Start()
        {
            base.Start();

            // Start in sleep state so grid remains rock-solid until projectile impact
            if (_rigidbody != null)
            {
                _rigidbody.Sleep();
            }
        }

        protected override void HandleProjectileHit(Collision collision)
        {
            base.HandleProjectileHit(collision);

            if (_hasBroken) return;

            // Direct ball hit shatters the paint bottle
            Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            BreakBottle(impactPoint);
        }

        /// <summary>
        /// Shatters the paint bottle, triggering glass and paint splatter effects.
        /// </summary>
        public void BreakBottle(Vector3 impactPoint)
        {
            if (_hasBroken) return;
            _hasBroken = true;

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayBottleBreak();
            }

            if (RoyalSmash.VFX.VFXManager.Instance != null)
            {
                RoyalSmash.VFX.VFXManager.Instance.SpawnBottleBreak(impactPoint, _paintColor);
            }

            Debug.Log($"[PaintBottleObject] Shattered at {impactPoint} with {_paintColor} paint splash!");

            // Spawn procedural paint splat particle burst
            SpawnPaintSplat(impactPoint);

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

        private void SpawnPaintSplat(Vector3 impactPoint)
        {
            GameObject splatGo = new GameObject("PaintSplat_Burst");
            splatGo.transform.position = impactPoint;

            ParticleSystem ps = splatGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.startColor = _paintColor;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 6.0f);
            main.startLifetime = 0.6f;
            main.duration = 0.5f;
            main.loop = false;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            ps.Play();

            Destroy(splatGo, 1.2f);
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
