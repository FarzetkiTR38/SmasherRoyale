using System;
using System.IO;
using UnityEngine;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Utility component that listens for F12 key press to capture fullscreen screenshots
    /// and save them to the project root 'Screenshots/' folder.
    /// Runs across all scenes via BootScene and DontDestroyOnLoad.
    /// </summary>
    public class ScreenshotCapture : MonoBehaviour
    {
        private static ScreenshotCapture _instance;
        private string _screenshotDirectory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (_instance == null && Application.isPlaying)
            {
                var existing = FindFirstObjectByType<ScreenshotCapture>();
                if (existing == null)
                {
                    var go = new GameObject("[ScreenshotCapture]");
                    _instance = go.AddComponent<ScreenshotCapture>();
                    DontDestroyOnLoad(go);
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                InitializeDirectory();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDirectory()
        {
            _screenshotDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Screenshots"));
            if (!Directory.Exists(_screenshotDirectory))
            {
                Directory.CreateDirectory(_screenshotDirectory);
                Debug.Log($"[ScreenshotCapture] Created screenshots directory at: {_screenshotDirectory}");
            }
        }

        private void Update()
        {
            bool trigger = false;

#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.f12Key.wasPressedThisFrame)
            {
                trigger = true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F12))
            {
                trigger = true;
            }
#else
            try
            {
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    trigger = true;
                }
            }
            catch
            {
                // Ignored if legacy input is unavailable
            }
#endif

            if (trigger)
            {
                CaptureScreenshot();
            }
        }

        public void CaptureScreenshot()
        {
            if (string.IsNullOrEmpty(_screenshotDirectory))
            {
                InitializeDirectory();
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"RoyalSmash_{timestamp}_{Screen.width}x{Screen.height}.png";
            string fullPath = Path.Combine(_screenshotDirectory, fileName);

            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"[ScreenshotCapture] Fullscreen screenshot captured: {fullPath}");
        }
    }
}
