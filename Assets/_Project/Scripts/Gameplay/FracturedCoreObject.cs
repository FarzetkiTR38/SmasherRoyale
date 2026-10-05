using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Root controller for a fractured Core block instance.
    /// Manages detonation and activation of all child physical debris pieces.
    /// </summary>
    public class FracturedCoreObject : MonoBehaviour
    {
        [Header("Explosion Tuning")]
        [Tooltip("Outward impulse applied to each fractured shard")]
        [SerializeField] private float _explosionForce = 6.0f;

        /// <summary>
        /// Triggers full physical shatter of all child shards.
        /// </summary>
        public void TriggerShatter(Vector3 impactPoint, Vector3 impactVelocity)
        {
            var pieces = GetComponentsInChildren<ShatteredDebrisPiece>();
            var colliders = new System.Collections.Generic.List<Collider>();
            foreach (var piece in pieces)
            {
                var c = piece.GetComponent<Collider>();
                if (c != null) colliders.Add(c);
            }

            // Ignore collisions between debris shards so they explode freely without snagging
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    Physics.IgnoreCollision(colliders[i], colliders[j], true);
                }
            }

            foreach (var piece in pieces)
            {
                piece.Initialize(impactPoint, impactVelocity, _explosionForce);
            }

            // Destroy this empty parent container after pieces are detached
            if (Application.isPlaying)
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }
}
