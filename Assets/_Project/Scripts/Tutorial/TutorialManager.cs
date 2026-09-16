using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Levels;

namespace RoyalSmash.Tutorial
{
    /// <summary>
    /// Central manager for the First-Launch Tutorial.
    /// Manages guided targeting, restricts unauthorized taps, and cleanly releases control upon first successful shot.
    /// Follows Master Build Prompt Phase 7 requirements.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private TutorialConfig _config;

        [Header("Target Binding")]
        [Tooltip("The designated target object player must hit during tutorial (e.g. front center Cone).")]
        [SerializeField] private Transform _targetTransform;

        [Header("UI Reference")]
        [SerializeField] private TutorialOverlayUI _overlayUI;

        private bool _isTutorialActive;
        private Camera _mainCamera;

        public bool IsTutorialActive => _isTutorialActive;
        public Transform TargetTransform => _targetTransform;
        public TutorialConfig Config => _config;

        public event Action OnTutorialStarted;
        public event Action OnTutorialCompleted;
        public event Action OnInvalidTapAttempted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _mainCamera = Camera.main;

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<TutorialConfig>();
            }
        }

        private void Start()
        {
            CheckAndInitializeTutorial();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Evaluates whether the tutorial should trigger (First launch & Level 1).
        /// </summary>
        public void CheckAndInitializeTutorial()
        {
            // 1. Check Save Data
            bool isCompleted = false;
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                isCompleted = SaveManager.Instance.Data.TutorialCompleted;
            }

            if (isCompleted)
            {
                Debug.Log("[TutorialManager] Tutorial already completed in save data. Skipping.");
                _isTutorialActive = false;
                if (_overlayUI != null) _overlayUI.HideImmediate();
                return;
            }

            // 2. Check Level (Level 1 only)
            int levelIndex = 1;
            if (LevelManager.Instance != null)
            {
                levelIndex = LevelManager.Instance.CurrentLevelIndex;
            }

            if (levelIndex != 1)
            {
                Debug.Log($"[TutorialManager] Current level is {levelIndex} (not Level 1). Skipping tutorial.");
                _isTutorialActive = false;
                if (_overlayUI != null) _overlayUI.HideImmediate();
                return;
            }

            // 3. Find target object if not manually set
            if (_targetTransform == null)
            {
                FindBestTutorialTarget();
            }

            // 4. Start Tutorial
            StartTutorial();
        }

        private void FindBestTutorialTarget()
        {
            // Pick front-most centered physics target (e.g. front-center Cone at Z~6.4)
            var physObjects = FindObjectsByType<RoyalSmash.Gameplay.PhysicsObject>(FindObjectsSortMode.None);
            Transform bestTarget = null;
            float minZ = float.MaxValue;

            foreach (var po in physObjects)
            {
                if (Mathf.Abs(po.transform.position.x) < 0.6f && po.transform.position.z < minZ)
                {
                    minZ = po.transform.position.z;
                    bestTarget = po.transform;
                }
            }

            if (bestTarget != null)
            {
                _targetTransform = bestTarget;
                return;
            }

            // Fallback: any PhysicsObject
            var anyPhys = FindFirstObjectByType<RoyalSmash.Gameplay.PhysicsObject>();
            if (anyPhys != null)
            {
                _targetTransform = anyPhys.transform;
            }
        }

        public void StartTutorial()
        {
            _isTutorialActive = true;
            Debug.Log("[TutorialManager] Tutorial Started! Guided input active.");

            if (_overlayUI != null)
            {
                _overlayUI.ShowTutorial(_targetTransform, _config);
            }

            OnTutorialStarted?.Invoke();
        }

        /// <summary>
        /// Validates if a tap at screenPosition is directed at the tutorial target.
        /// </summary>
        public bool IsTapAllowed(Vector2 screenPosition, Vector3 worldTarget)
        {
            if (!_isTutorialActive) return true;

            if (_targetTransform == null)
            {
                // Safety: if target missing, allow tap
                return true;
            }

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return true;

            Vector2 targetScreen = _mainCamera.WorldToScreenPoint(_targetTransform.position);
            float radius = _config != null ? _config.TargetRadiusPixels : 160f;

            float distance = Vector2.Distance(screenPosition, targetScreen);
            bool withinScreenRadius = distance <= radius;

            // Also check 3D world proximity
            float worldDist = Vector3.Distance(new Vector3(worldTarget.x, 0, worldTarget.z),
                                               new Vector3(_targetTransform.position.x, 0, _targetTransform.position.z));
            bool withinWorldRadius = worldDist <= 1.2f;

            if (withinScreenRadius || withinWorldRadius)
            {
                return true;
            }

            // Rejected
            Debug.Log($"[TutorialManager] Tap outside target rejected (dist={distance:F1}px > {radius}px).");
            OnInvalidTapAttempted?.Invoke();
            if (_overlayUI != null)
            {
                _overlayUI.PulseTargetAttention();
            }
            return false;
        }

        /// <summary>
        /// Called when the player successfully fires their first guided shot.
        /// Restores full control and permanently saves tutorial completion.
        /// </summary>
        public void CompleteTutorial()
        {
            if (!_isTutorialActive) return;

            _isTutorialActive = false;
            Debug.Log("[TutorialManager] First guided shot fired! Tutorial Completed. Releasing full control.");

            // Persist to save data
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetTutorialCompleted(true);
            }

            // Hide overlay
            if (_overlayUI != null)
            {
                _overlayUI.HideTutorial();
            }

            OnTutorialCompleted?.Invoke();
        }

        /// <summary>
        /// Public helper for testing to manually set completion state.
        /// </summary>
        public void SetCompletedForTesting(bool completed)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetTutorialCompleted(completed);
            }
            _isTutorialActive = !completed;
            if (completed && _overlayUI != null)
            {
                _overlayUI.HideImmediate();
            }
        }

        public void SetTargetTransform(Transform target)
        {
            _targetTransform = target;
        }
    }
}
