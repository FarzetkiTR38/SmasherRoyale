using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Lives;

namespace RoyalSmash.Core
{
    /// <summary>
    /// Monitors application lifecycle states (pause, resume, quit) across mobile platforms.
    /// Handles ungraceful exit detection (app closed during active level), persists state to disk,
    /// and triggers life regeneration upon returning to the foreground.
    /// Follows Master Build Prompt Section 1.3 & Save System Skill patterns.
    /// </summary>
    public class AppLifecycleHandler : MonoBehaviour
    {
        public static AppLifecycleHandler Instance { get; private set; }

        [SerializeField] private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (_dontDestroyOnLoad && transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            CheckPreviousSessionUngracefulExit();
        }

        /// <summary>
        /// Evaluates if the previous play session ended while inside an active level.
        /// If so, deducts 1 life as ungraceful exit penalty (per Section 1.3).
        /// </summary>
        public void CheckPreviousSessionUngracefulExit()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                if (saveMgr.Data.WasInActiveLevel)
                {
                    Debug.LogWarning("[AppLifecycleHandler] Detected previous session ungraceful exit during active level! Deducting 1 life.");
                    if (LivesManager.Instance != null)
                    {
                        LivesManager.Instance.DeductLife("AppCloseDuringLevel", "PrevSessionUngracefulExit");
                    }
                    saveMgr.Data.WasInActiveLevel = false;
                    saveMgr.Save();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App entering background - persist state immediately
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.Save();
                }
            }
            else
            {
                // App returning to foreground - calculate passive life regeneration
                if (LivesManager.Instance != null)
                {
                    LivesManager.Instance.ProcessRegeneration();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Save();
            }
        }
    }
}
