using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Handles mobile touch and pointer input.
    /// Accurately differentiates between Drag (aim/move mortar) and Tap (fire projectile at 3D world coordinate).
    /// Guarantees that ending a drag never triggers an accidental shot.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MortarConfig _config;

        [Header("References")]
        [SerializeField] private Camera _gameplayCamera;

        [Header("Targeting Settings")]
        [Tooltip("Minimum allowed world Y position for targeting (cannot target below ground/stand).")]
        [SerializeField] private float _minTargetY = 0.5f;

        // Events
        public event Action<Vector3> OnAimTargetChanged;
        public event Action OnDragStarted;
        public event Action OnDragEnded;
        public event Action<Vector3> OnTapFired;

        // State tracking
        private Vector2 _touchStartPosition;
        private Vector2 _lastTouchPosition;
        private bool _isPressed;
        private bool _isDragging;

        public bool IsDragging => _isDragging;

        private void Awake()
        {
            if (_gameplayCamera == null)
            {
                _gameplayCamera = Camera.main;
            }
        }

        private void Update()
        {
            ProcessPointerInput();
        }

        private void ProcessPointerInput()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            bool isCurrentlyPressed = pointer.press.isPressed;
            Vector2 currentPosition = pointer.position.ReadValue();

            // 1. Touch / Press Began
            if (isCurrentlyPressed && !_isPressed)
            {
                _isPressed = true;
                _touchStartPosition = currentPosition;
                _lastTouchPosition = currentPosition;
                _isDragging = false;

                // Immediately aim at the initial touch point
                Vector3 initialTarget = CalculateWorldTarget(currentPosition);
                OnAimTargetChanged?.Invoke(initialTarget);
            }
            // 2. Touch / Press Active (Held or Moved)
            else if (isCurrentlyPressed && _isPressed)
            {
                float totalMoveDistance = Vector2.Distance(_touchStartPosition, currentPosition);
                float threshold = _config != null ? _config.DragThresholdPixels : 12f;

                if (!_isDragging && totalMoveDistance > threshold)
                {
                    _isDragging = true;
                    OnDragStarted?.Invoke();
                }

                // While dragging or holding, continuously update the aim target
                Vector3 currentTarget = CalculateWorldTarget(currentPosition);
                OnAimTargetChanged?.Invoke(currentTarget);

                _lastTouchPosition = currentPosition;
            }
            // 3. Touch / Press Released
            else if (!isCurrentlyPressed && _isPressed)
            {
                _isPressed = false;

                if (_isDragging)
                {
                    // Finished dragging - NO FIRE on drag release
                    _isDragging = false;
                    OnDragEnded?.Invoke();
                }
                else
                {
                    // Pure Tap detected - Fire at world coordinate
                    HandleTap(currentPosition);
                }
            }
        }

        /// <summary>
        /// Convert screen tap position to 3D world coordinates and trigger the fire event.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels.</param>
        public void HandleTap(Vector2 screenPosition)
        {
            if (_gameplayCamera == null)
            {
                _gameplayCamera = Camera.main;
                if (_gameplayCamera == null) return;
            }

            Vector3 worldTarget = CalculateWorldTarget(screenPosition);
            OnAimTargetChanged?.Invoke(worldTarget);

            Debug.Log($"[InputHandler] Tap detected at screen {screenPosition} -> World target: {worldTarget}");

            OnTapFired?.Invoke(worldTarget);
        }

        /// <summary>
        /// Calculates the 3D world target point using raycasting against scene colliders
        /// or intersecting with the gameplay depth plane.
        /// </summary>
        public Vector3 CalculateWorldTarget(Vector2 screenPosition)
        {
            Ray ray = _gameplayCamera.ScreenPointToRay(screenPosition);
            LayerMask mask = _config != null ? _config.TargetLayerMask : ~0;

            // 1. Raycast against physical objects (stand, blocks, colliders)
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
            {
                Vector3 hitPoint = hit.point;
                if (hitPoint.y < _minTargetY) hitPoint.y = _minTargetY;
                return hitPoint;
            }

            // 2. Intersect with the virtual gameplay target plane at gameplay depth (Z = targetDistance)
            float targetZ = _config != null ? _config.DefaultTargetDistance : 4.0f;
            Plane targetPlane = new Plane(Vector3.back, new Vector3(0f, 0f, targetZ));

            if (targetPlane.Raycast(ray, out float enterDistance))
            {
                Vector3 planePoint = ray.GetPoint(enterDistance);
                if (planePoint.y < _minTargetY) planePoint.y = _minTargetY;
                return planePoint;
            }

            // 3. Fallback depth
            Vector3 fallbackPoint = ray.GetPoint(15f);
            if (fallbackPoint.y < _minTargetY) fallbackPoint.y = _minTargetY;
            return fallbackPoint;
        }

        /// <summary>
        /// Programmatic simulation method for automated testing.
        /// </summary>
        public void SimulateAimTarget(Vector3 worldTarget)
        {
            OnAimTargetChanged?.Invoke(worldTarget);
        }

        /// <summary>
        /// Programmatic simulation method for automated testing.
        /// </summary>
        public void SimulateTapAt(Vector2 screenPosition)
        {
            HandleTap(screenPosition);
        }

        /// <summary>
        /// Programmatic simulation method for automated testing directly with world coordinates.
        /// </summary>
        public void SimulateTapWorldTarget(Vector3 worldTarget)
        {
            OnAimTargetChanged?.Invoke(worldTarget);
            OnTapFired?.Invoke(worldTarget);
        }
    }
}
