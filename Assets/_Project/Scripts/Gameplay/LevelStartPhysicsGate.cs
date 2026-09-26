using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>Holds a newly loaded puzzle exactly in place until its first successful shot.</summary>
    [DisallowMultipleComponent]
    public sealed class LevelStartPhysicsGate : MonoBehaviour
    {
        private struct HeldBody
        {
            public Rigidbody Body;
            public CollisionDetectionMode DetectionMode;
        }

        private readonly List<HeldBody> _heldBodies = new List<HeldBody>();
        private MortarController _mortar;
        public bool IsWaitingForFirstShot { get; private set; }
        public int HeldBodyCount => _heldBodies.Count;

        public static LevelStartPhysicsGate ActiveGate { get; private set; }

        public static void ReleaseActiveGate()
        {
            if (ActiveGate != null && ActiveGate.IsWaitingForFirstShot)
            {
                ActiveGate.Release();
            }
        }

        private void Awake()
        {
            ActiveGate = this;
        }

        private void OnDestroy()
        {
            if (ActiveGate == this) ActiveGate = null;
        }

        public void Prepare(IReadOnlyList<PhysicsObject> targets, MortarController mortar)
        {
            ActiveGate = this;
            Release();
            _mortar = mortar;
            for (int i = 0; i < targets.Count; i++)
            {
                var body = targets[i] != null ? targets[i].Rigidbody : null;
                if (body == null || body.isKinematic) continue;
                _heldBodies.Add(new HeldBody { Body = body, DetectionMode = body.collisionDetectionMode });
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                // Continuous modes intended for dynamic bodies are not valid while kinematic.
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                body.isKinematic = true;
            }
            IsWaitingForFirstShot = _heldBodies.Count > 0;
            // Note: We do NOT release on OnBallFired. We release when an actual impact occurs
            // (via LevelStartPhysicsGate.ReleaseActiveGate() in PhysicsObject/Stand collision).
        }

        /// <summary>Restore dynamic configuration upon first projectile impact.</summary>
        public void Release()
        {
            if (_mortar != null) _mortar.OnBallFired -= Release;
            _mortar = null;
            for (int i = 0; i < _heldBodies.Count; i++)
            {
                var held = _heldBodies[i];
                if (held.Body == null) continue;
                held.Body.isKinematic = false;
                held.Body.collisionDetectionMode = held.DetectionMode;
                held.Body.linearVelocity = Vector3.zero;
                held.Body.angularVelocity = Vector3.zero;
                held.Body.Sleep();
            }
            _heldBodies.Clear();
            IsWaitingForFirstShot = false;
        }

        private void OnDisable() => Release();
    }
}
