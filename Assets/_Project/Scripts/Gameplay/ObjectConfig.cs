using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Configuration data for a specific physics object type.
    /// Defines mass, friction, bounciness, breakability, and destruction timings.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectConfig", menuName = "RoyalSmash/Config/ObjectConfig")]
    public class ObjectConfig : ScriptableObject
    {
        [Header("Object Identity")]
        [SerializeField] private ObjectType _objectType = ObjectType.Box;
        [SerializeField] private string _displayName = "Box";

        [Header("Physics Properties")]
        [Tooltip("Mass in kilograms for Rigidbody.")]
        [SerializeField] private float _mass = 1.0f;

        [Tooltip("Bounciness for the physics material (0-1).")]
        [SerializeField] private float _bounciness = 0.2f;

        [Tooltip("Friction for the physics material.")]
        [SerializeField] private float _friction = 0.4f;

        [Header("Breakable Behavior")]
        [Tooltip("If true, this object shatters on sufficient impact (e.g. PaintBottle).")]
        [SerializeField] private bool _isBreakable = false;

        [Tooltip("Minimum impact impulse/force required to shatter this object.")]
        [SerializeField] private float _breakForceThreshold = 4.0f;

        [Header("Destruction Settings")]
        [Tooltip("Duration in seconds of the shrink-and-fade destruction animation.")]
        [SerializeField] private float _shrinkDuration = 0.5f;

        [Tooltip("Fall-off Y coordinate threshold below which the object is considered cleared from stand.")]
        [SerializeField] private float _fallOffThresholdY = 0.2f;

        [Header("Audio & Effects")]
        [SerializeField] private string _impactSoundKey = "wood_impact";

        public ObjectType ObjectType => _objectType;
        public string DisplayName => _displayName;
        public float Mass => _mass;
        public float Bounciness => _bounciness;
        public float Friction => _friction;
        public bool IsBreakable => _isBreakable;
        public float BreakForceThreshold => _breakForceThreshold;
        public float ShrinkDuration => _shrinkDuration;
        public float FallOffThresholdY => _fallOffThresholdY;
        public string ImpactSoundKey => _impactSoundKey;
    }
}
