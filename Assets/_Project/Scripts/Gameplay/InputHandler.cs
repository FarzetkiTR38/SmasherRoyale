using System;
using UnityEngine;
using UnityEngine.InputSystem;
using RoyalSmash.Tutorial;
using RoyalSmash.Lives;
using RoyalSmash.UI;

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
        public event Action<string> OnInvalidTapAttempted;

        // State tracking
        private Vector2 _touchStartPosition;
        private Vector2 _lastTouchPosition;
        private bool _isPressed;
        private bool _isDragging;
        private bool _isPointerOverUI;
        private StandController _standController;

        public bool IsDragging => _isDragging;

        private void Awake()
        {
            if (_gameplayCamera == null)
            {
                _gameplayCamera = Camera.main;
            }
            _standController = FindFirstObjectByType<StandController>();
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

                // Check if pointer began over a UI element (e.g. skill buttons, pause menu)
                if (IsPointerOverUI(currentPosition))
                {
                    _isPointerOverUI = true;
                    return;
                }
                _isPointerOverUI = false;

                // Zero lives check: prevent aiming when out of lives
                if (LivesManager.Instance != null && !LivesManager.Instance.HasLivesToPlay)
                {
                    var outOfLives = FindFirstObjectByType<OutOfLivesDialog>();
                    if (outOfLives != null) outOfLives.Show();
                    return;
                }

                // Immediately aim mortar toward the touch point
                Vector3 initialTarget = CalculateWorldTarget(currentPosition);
                OnAimTargetChanged?.Invoke(initialTarget);
            }
            // 2. Touch / Press Active (Held or Moved) - Smoothly rotate/aim mortar with cursor
            else if (isCurrentlyPressed && _isPressed)
            {
                if (_isPointerOverUI) return;

                float totalMoveDistance = Vector2.Distance(_touchStartPosition, currentPosition);
                float threshold = _config != null ? _config.DragThresholdPixels : 12f;
                if (!_isDragging && totalMoveDistance > threshold)
                {
                    _isDragging = true;
                    OnDragStarted?.Invoke();
                }

                // Continuously update mortar aim direction as mouse/finger moves
                Vector3 currentTarget = CalculateWorldTarget(currentPosition);
                OnAimTargetChanged?.Invoke(currentTarget);

                _lastTouchPosition = currentPosition;
            }
            // 3. Touch / Press Released - Fire!
            else if (!isCurrentlyPressed && _isPressed)
            {
                _isPressed = false;

                if (_isPointerOverUI)
                {
                    _isPointerOverUI = false;
                    return;
                }

                if (_isDragging)
                {
                    _isDragging = false;
                    OnDragEnded?.Invoke();
                }

                // Fire at the aimed/released world coordinate
                HandleTap(currentPosition);
            }
        }

        /// <summary>
        /// Checks if a screen position lies over an interactive UI element.
        /// </summary>
        public bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;

            // Check standalone pointer
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return true;

            // Check active touches on mobile touch screen
            if (Touchscreen.current != null)
            {
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var touch = Touchscreen.current.touches[i];
                    if (touch.isInProgress && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue()))
                    {
                        return true;
                    }
                }
            }

            // Fallback GraphicRaycaster check
            var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = screenPosition
            };
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
            return results.Count > 0;
        }

        /// <summary>
        /// Evaluates if the tapped screen position corresponds to a world coordinate below the stand platform.
        /// </summary>
        public bool IsPositionBelowStand(Vector2 screenPosition, out Vector3 worldTarget)
        {
            float minAllowedY = GetStandTabletopY() - 0.05f;

            if (_gameplayCamera == null) _gameplayCamera = Camera.main;
            Ray ray = _gameplayCamera.ScreenPointToRay(screenPosition);
            LayerMask mask = _config != null ? _config.TargetLayerMask : ~0;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
            {
                worldTarget = hit.point;
                if (hit.point.y < minAllowedY || hit.collider.gameObject.name.IndexOf("ground", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
                return false;
            }

            float targetZ = _standController != null ? _standController.transform.position.z : (_config != null ? _config.DefaultTargetDistance : 7.5f);
            Plane targetPlane = new Plane(Vector3.back, new Vector3(0f, 0f, targetZ));
            if (targetPlane.Raycast(ray, out float enterDistance))
            {
                worldTarget = ray.GetPoint(enterDistance);
                if (worldTarget.y < minAllowedY)
                {
                    return true;
                }
                return false;
            }

            worldTarget = ray.GetPoint(15f);
            return worldTarget.y < minAllowedY;
        }

        public float GetStandTabletopY()
        {
            if (_standController == null)
            {
                _standController = FindFirstObjectByType<StandController>();
            }
            if (_standController != null)
            {
                return _standController.TabletopSurfaceY;
            }
            return 1.45f;
        }

        /// <summary>
        /// Convert screen tap position to 3D world coordinates and trigger the fire event.
        /// Rejects taps on UI and taps below the stand with a descriptive warning.
        /// </summary>
        /// <param name="screenPosition">Screen position in pixels.</param>
        public void HandleTap(Vector2 screenPosition)
        {
            if (IsPointerOverUI(screenPosition))
            {
                Debug.Log("[InputHandler] Tap ignored: clicked on UI element.");
                return;
            }

            // Zero Lives Gating: block firing and display OutOfLivesDialog
            if (LivesManager.Instance != null && !LivesManager.Instance.HasLivesToPlay)
            {
                Debug.LogWarning("[InputHandler] Tap rejected: Player has 0 lives!");
                var outOfLives = FindFirstObjectByType<OutOfLivesDialog>();
                if (outOfLives != null) outOfLives.Show();
                return;
            }

            if (_gameplayCamera == null)
            {
                _gameplayCamera = Camera.main;
                if (_gameplayCamera == null) return;
            }

            // Check if tap target is below the stand
            if (IsPositionBelowStand(screenPosition, out Vector3 worldTarget))
            {
                Debug.Log($"[InputHandler] Tap rejected below stand (Y={worldTarget.y:F2} < {GetStandTabletopY():F2}).");
                OnInvalidTapAttempted?.Invoke("STANDIN ALT KISMINA TIKLAYAMAZSINIZ, OBJELERE TIKLAYIN!");
                return;
            }

            // Tutorial restriction: Player can only tap to fire at the highlighted target area on Level 1
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                if (!TutorialManager.Instance.IsTapAllowed(screenPosition, worldTarget))
                {
                    return;
                }
                // First guided shot unlocks full control immediately
                TutorialManager.Instance.CompleteTutorial();
            }

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
            if (_gameplayCamera == null) _gameplayCamera = Camera.main;
            Ray ray = _gameplayCamera.ScreenPointToRay(screenPosition);
            LayerMask mask = _config != null ? _config.TargetLayerMask : ~0;
            float minTargetY = GetStandTabletopY() - 0.05f;

            // 1. Raycast against physical objects (stand, blocks, colliders)
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
            {
                Vector3 hitPoint = hit.point;
                if (hitPoint.y < minTargetY) hitPoint.y = minTargetY;
                return hitPoint;
            }

            // 2. Intersect with virtual gameplay target plane at gameplay depth
            float targetZ = _standController != null ? _standController.transform.position.z : (_config != null ? _config.DefaultTargetDistance : 7.5f);
            Plane targetPlane = new Plane(Vector3.back, new Vector3(0f, 0f, targetZ));

            if (targetPlane.Raycast(ray, out float enterDistance))
            {
                Vector3 planePoint = ray.GetPoint(enterDistance);
                if (planePoint.y < minTargetY) planePoint.y = minTargetY;
                return planePoint;
            }

            // 3. Fallback depth
            Vector3 fallbackPoint = ray.GetPoint(15f);
            if (fallbackPoint.y < minTargetY) fallbackPoint.y = minTargetY;
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
