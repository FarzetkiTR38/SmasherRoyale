using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Tunable configuration data for mortar movement, boundaries, input thresholds, and targeting depth.
    /// </summary>
    [CreateAssetMenu(fileName = "MortarConfig", menuName = "RoyalSmash/Config/MortarConfig")]
    public class MortarConfig : ScriptableObject
    {
        [Header("Aiming Settings")]
        [Tooltip("Minimum horizontal rotation (yaw in degrees).")]
        [SerializeField] private float _minYaw = -45f;

        [Tooltip("Maximum horizontal rotation (yaw in degrees).")]
        [SerializeField] private float _maxYaw = 45f;

        [Tooltip("Smooth time for aiming rotation dampening (seconds).")]
        [SerializeField] private float _smoothTime = 0.05f;

        [Header("Input Settings")]
        [Tooltip("Movement threshold in pixels to classify an input as a drag rather than a tap.")]
        [SerializeField] private float _dragThresholdPixels = 12f;

        [Header("Targeting Settings")]
        [Tooltip("Default Z depth distance in world space where the target plane sits.")]
        [SerializeField] private float _defaultTargetDistance = 7.5f;

        [Tooltip("Target layer mask for raycasting against physics objects / stands.")]
        [SerializeField] private LayerMask _targetLayerMask = ~0;

        public float MinYaw => _minYaw;
        public float MaxYaw => _maxYaw;
        public float SmoothTime => _smoothTime;
        public float DragThresholdPixels => _dragThresholdPixels;
        public float DefaultTargetDistance => _defaultTargetDistance;
        public LayerMask TargetLayerMask => _targetLayerMask;
    }
}
