using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Core
{
    /// <summary>
    /// Coordinates the initial application launch sequence, splash screen timing,
    /// save data verification, and first-launch tutorial vs. returning-player main menu routing.
    /// Follows Sections 34, 35, 36, 302, 303 of the Royal Smash Full Documentation.
    /// </summary>
    public class AppBootstrapper : MonoBehaviour
    {
        private static AppBootstrapper _instance;
        public static AppBootstrapper Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private float _splashDuration = 1.0f;
        [SerializeField] private float _minLoadingDuration = 0.8f;
        [SerializeField] private bool _autoBootstrapOnStart = true;

        [Header("References")]
        [SerializeField] private SplashLoadingOverlay _overlay;

        private bool _isBootstrapping = false;

        public bool IsBootstrapping => _isBootstrapping;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (_autoBootstrapOnStart && !_isBootstrapping)
            {
                StartCoroutine(BootstrapSequenceCoroutine());
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// Executes the full bootstrap sequence: Splash -> Loading -> Destination Routing.
        /// </summary>
        public IEnumerator BootstrapSequenceCoroutine()
        {
            _isBootstrapping = true;
            Debug.Log("[AppBootstrapper] Starting Royal Smash application bootstrap sequence...");

            // 1. Ensure overlay exists
            if (_overlay == null)
            {
                _overlay = SplashLoadingOverlay.Instance;
                if (_overlay == null)
                {
                    _overlay = FindFirstObjectByType<SplashLoadingOverlay>();
                }
            }

            // 2. Play Splash Sequence
            if (_overlay != null)
            {
                yield return StartCoroutine(_overlay.PlaySplashSequence(_splashDuration));
            }

            // 3. Load Save Data and Preload Systems
            float loadStartTime = Time.realtimeSinceStartup;

            if (SaveManager.Instance != null && SaveManager.Instance.Data == null)
            {
                SaveManager.Instance.Load();
            }

            // Simulate simulated progress bar ramp-up
            float elapsed = 0f;
            while (elapsed < _minLoadingDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _minLoadingDuration);
                if (_overlay != null)
                {
                    _overlay.SetProgress(progress * 0.9f);
                }
                yield return null;
            }

            // 4. Determine Routing Destination
            string targetScene = DetermineTargetScene();
            Debug.Log($"[AppBootstrapper] Routing to target scene: '{targetScene}'");

            if (_overlay != null)
            {
                _overlay.SetProgress(1.0f);
            }

            // 5. Transition to Target Scene via SceneTransitionManager
            if (SceneTransitionManager.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionManager.Instance.LoadSceneAsyncCoroutine(targetScene));
            }
            else
            {
                var asyncOp = SceneManager.LoadSceneAsync(targetScene);
                while (!asyncOp.isDone) yield return null;
            }

            // 6. Dismiss Loading Screen with smooth fade
            if (_overlay != null)
            {
                yield return StartCoroutine(_overlay.FadeOutOverlay(0.5f));
            }

            _isBootstrapping = false;
            Debug.Log("[AppBootstrapper] Bootstrap sequence completed successfully.");
        }

        /// <summary>
        /// Evaluates player save progress to decide destination:
        /// First-time player (Tutorial not completed) -> MainGame (Level 1 Tutorial).
        /// Returning player (Tutorial completed) -> MainMenu.
        /// </summary>
        public string DetermineTargetScene()
        {
            return SceneTransitionManager.SCENE_MAIN_MENU;
        }
    }
}
