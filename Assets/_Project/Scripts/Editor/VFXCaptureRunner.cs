#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using RoyalSmash.Gameplay;
using RoyalSmash.VFX;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated Play Mode test to fire cannon, verify trail & particle physics, and capture high-resolution screenshots.
    /// </summary>
    public static class VFXCaptureRunner
    {
        private static bool _pendingCapture = false;

        [MenuItem("RoyalSmash/Tests/Capture VFX In Action")]
        public static void StartCapture()
        {
            if (!EditorApplication.isPlaying)
            {
                _pendingCapture = true;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.isPlaying = true;
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && _pendingCapture)
            {
                _pendingCapture = false;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                var holder = new GameObject("VFXCaptureRunner");
                holder.AddComponent<VFXCaptureBehaviour>();
            }
        }
    }

    public class VFXCaptureBehaviour : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);

            var mortar = FindFirstObjectByType<MortarController>();
            var input = FindFirstObjectByType<InputHandler>();

            if (mortar != null && input != null)
            {
                Debug.Log("[VFXCapture] Aiming and firing mortar at bottles...");
                Vector3 target = new Vector3(0.0f, 2.8f, 7.5f);
                mortar.ResetCooldown();
                input.SimulateTapWorldTarget(target);

                // Wait for ball to be in mid-flight
                yield return new WaitForSeconds(0.18f);
                ScreenCapture.CaptureScreenshot("Assets/Screenshots/vfx_midair_royal_trail.png");
                Debug.Log("[VFXCapture] Captured mid-air trail screenshot!");

                // Wait for impact & bottle burst
                yield return new WaitForSeconds(0.22f);
                ScreenCapture.CaptureScreenshot("Assets/Screenshots/vfx_bottle_shatter_splash.png");
                Debug.Log("[VFXCapture] Captured bottle shatter & splash screenshot!");

                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log("[VFXCapture] Simulation complete. Exiting Play Mode.");
            EditorApplication.isPlaying = false;
            Destroy(gameObject);
        }
    }
}
#endif
