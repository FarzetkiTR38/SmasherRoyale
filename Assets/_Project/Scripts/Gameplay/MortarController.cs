using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Controls the mortar / cannon aiming and firing.
    /// The mortar base remains stationary while the cannon head / barrel swivels horizontally
    /// to follow player touch / drag input within clamped angular limits.
    /// </summary>
    public class MortarController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MortarConfig _config;

        [Header("References")]
        [Tooltip("Transform that swivels horizontally to aim (Visuals or Turret).")]
        [SerializeField] private Transform _aimTransform;

        [Tooltip("Transform where balls spawn and launch.")]
        [SerializeField] private Transform _firePoint;

        [SerializeField] private InputHandler _inputHandler;

        private float _targetYaw;
        private float _currentYaw;
        private float _yawVelocity;
        private Vector3 _lastTargetPoint;
        private int _fireEventCount;

        public float CurrentYaw => _currentYaw;
        public float TargetYaw => _targetYaw;
        public Transform FirePoint => _firePoint;
        public Vector3 LastTargetPoint => _lastTargetPoint;
        public int FireEventCount => _fireEventCount;

        private void Awake()
        {
            if (_aimTransform == null)
            {
                var visuals = transform.Find("Visuals");
                if (visuals != null) _aimTransform = visuals;
            }

            if (_firePoint == null)
            {
                var foundFirePoint = transform.Find("Visuals/BarrelPivot/FirePoint");
                if (foundFirePoint != null) _firePoint = foundFirePoint;
            }
        }

        private void Start()
        {
            if (_inputHandler == null)
            {
                _inputHandler = FindFirstObjectByType<InputHandler>();
            }

            RegisterInputEvents();
        }

        private void OnEnable()
        {
            RegisterInputEvents();
        }

        private void OnDisable()
        {
            UnregisterInputEvents();
        }

        private void RegisterInputEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnAimTargetChanged += HandleAimTargetChanged;
                _inputHandler.OnTapFired -= HandleTapFired;
                _inputHandler.OnTapFired += HandleTapFired;
            }
        }

        private void UnregisterInputEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnTapFired -= HandleTapFired;
            }
        }

        private void Update()
        {
            UpdateAimRotation();
        }

        private void UpdateAimRotation()
        {
            if (_aimTransform == null) return;

            float smoothTime = _config != null ? _config.SmoothTime : 0.05f;

            // Smoothly damp towards target yaw
            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, smoothTime);

            // Apply horizontal yaw to aim transform (swivels left and right)
            _aimTransform.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        private void HandleAimTargetChanged(Vector3 worldTarget)
        {
            Vector3 direction = (worldTarget - transform.position).normalized;
            if (direction.sqrMagnitude < Mathf.Epsilon) return;

            float calculatedYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            float minYaw = _config != null ? _config.MinYaw : -45f;
            float maxYaw = _config != null ? _config.MaxYaw : 45f;

            _targetYaw = Mathf.Clamp(calculatedYaw, minYaw, maxYaw);
        }

        private void HandleTapFired(Vector3 worldTarget)
        {
            _lastTargetPoint = worldTarget;
            _fireEventCount++;

            // Ensure we are aiming directly at the tap target
            HandleAimTargetChanged(worldTarget);

            Debug.Log($"[MortarController] Fire command received! Target: {worldTarget} (Total fires: {_fireEventCount})");
        }

        /// <summary>
        /// Manually set the target yaw angle (for automated testing or resetting).
        /// </summary>
        public void SetTargetYaw(float yawAngle)
        {
            float minYaw = _config != null ? _config.MinYaw : -45f;
            float maxYaw = _config != null ? _config.MaxYaw : 45f;
            _targetYaw = Mathf.Clamp(yawAngle, minYaw, maxYaw);
        }
    }
}
