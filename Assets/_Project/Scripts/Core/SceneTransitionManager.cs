using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.UI;

namespace RoyalSmash.Core
{
    /// <summary>
    /// Central manager for asynchronous scene transitions between Main Menu and Gameplay.
    /// Handles smooth fade-to-black masking, loading progress reporting, and double-load protection.
    /// Follows Master Build Prompt Section 17 & Royal Smash Full Docs Section 268.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<SceneTransitionManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("SceneTransitionManager");
                        _instance = go.AddComponent<SceneTransitionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public const string SCENE_BOOT = "BootScene";
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_MAIN_GAME = "MainGame";

        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private float _defaultFadeDuration = 0.35f;

        private bool _isTransitioning = false;

        public bool IsTransitioning => _isTransitioning;

        public event Action<string> OnTransitionStarted;
        public event Action<float> OnTransitionProgress;
        public event Action<string> OnTransitionCompleted;

        private void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this;
                if (_dontDestroyOnLoad && transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// Transitions smoothly to the Main Menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene(SCENE_MAIN_MENU);
        }

        /// <summary>
        /// Transitions smoothly to the Gameplay scene.
        /// </summary>
        public void LoadGameplay()
        {
            LoadScene(SCENE_MAIN_GAME);
        }

        /// <summary>
        /// Loads a scene by name with animated fade transition and double-load protection.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"[SceneTransitionManager] Transition already in progress. Request for '{sceneName}' ignored.");
                return;
            }

            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }

        /// <summary>
        /// Asynchronous coroutine that fades out the current scene, loads the target scene, and fades in.
        /// </summary>
        public IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            if (_isTransitioning) yield break;
            _isTransitioning = true;

            Debug.Log($"[SceneTransitionManager] Initiating transition to scene: '{sceneName}'");
            OnTransitionStarted?.Invoke(sceneName);

            var overlay = SplashLoadingOverlay.Instance;
            if (overlay == null) overlay = FindFirstObjectByType<SplashLoadingOverlay>();

            // 1. Fade to black / overlay
            if (overlay != null)
            {
                yield return StartCoroutine(overlay.FadeInOverlay(_defaultFadeDuration));
            }
            else
            {
                yield return new WaitForSecondsRealtime(_defaultFadeDuration);
            }

            // 2. Load scene asynchronously
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = false;

                while (asyncOp.progress < 0.9f)
                {
                    float normalizedProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    OnTransitionProgress?.Invoke(normalizedProgress);
                    if (overlay != null)
                    {
                        overlay.SetProgress(normalizedProgress);
                    }
                    yield return null;
                }

                // Allow scene to activate
                asyncOp.allowSceneActivation = true;
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            OnTransitionProgress?.Invoke(1.0f);

            // Wait a frame for scene initialization to settle
            yield return null;

            // 3. Fade out overlay to reveal new scene
            if (overlay != null)
            {
                yield return StartCoroutine(overlay.FadeOutOverlay(_defaultFadeDuration));
            }

            _isTransitioning = false;
            OnTransitionCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneTransitionManager] Transition to '{sceneName}' completed.");
        }
    }
}
