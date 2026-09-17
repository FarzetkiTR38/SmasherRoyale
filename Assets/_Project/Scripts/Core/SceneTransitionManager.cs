using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoyalSmash.Core
{
    /// <summary>
    /// Central manager for scene transitions between Main Menu and Gameplay.
    /// Follows Master Build Prompt Section 17 & Phase 11 architecture.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SceneTransitionManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_MAIN_GAME = "MainGame";

        [SerializeField] private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (_dontDestroyOnLoad && transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// <summary>
        /// Transitions to the Main Menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            Debug.Log("[SceneTransitionManager] Loading Main Menu scene...");
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        /// <summary>
        /// Transitions to the Gameplay scene.
        /// </summary>
        public void LoadGameplay()
        {
            Debug.Log("[SceneTransitionManager] Loading Gameplay scene...");
            SceneManager.LoadScene(SCENE_MAIN_GAME);
        }

        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
