using System.Collections;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Smooth, non-drifting camera screen shake system for Royal Smash.
    /// Provides organic Perlin noise vibration with rotational punch for physical impacts,
    /// block destruction, and Mega Ball explosions.
    /// Guarantees 100% zero positional drift upon shake completion.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake _instance;
        public static ScreenShake Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<ScreenShake>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Settings")]
        [SerializeField] private bool _enableShake = true;
        [Tooltip("Global intensity multiplier for all shakes.")]
        [SerializeField] private float _intensityMultiplier = 1.0f;

        private Vector3 _originalLocalPosition;
        private Quaternion _originalLocalRotation;
        private Coroutine _shakeCoroutine;
        private bool _isShaking;

        public bool IsShaking => _isShaking;
        public bool EnableShake
        {
            get => _enableShake;
            set => _enableShake = value;
        }

        public void Initialize()
        {
            _instance = this;
            _originalLocalPosition = transform.localPosition;
            _originalLocalRotation = transform.localRotation;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                Initialize();
            }
            else if (_instance != this)
            {
                if (Application.isPlaying) Destroy(this);
                else DestroyImmediate(this);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Recalibrates the baseline position if the camera moves dynamically in the scene.
        /// </summary>
        public void UpdateOriginalTransform(Vector3 localPosition, Quaternion localRotation)
        {
            _originalLocalPosition = localPosition;
            _originalLocalRotation = localRotation;
        }

        /// <summary>
        /// Triggers a customizable camera shake.
        /// </summary>
        /// <param name="duration">Duration of shake in seconds.</param>
        /// <param name="magnitude">Maximum displacement distance.</param>
        /// <param name="frequency">Vibration oscillation frequency.</param>
        public void PlayShake(float duration, float magnitude, float frequency = 25f)
        {
            if (!_enableShake || duration <= 0f || magnitude <= 0f) return;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude * _intensityMultiplier, frequency));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude, float frequency)
        {
            _isShaking = true;
            float elapsed = 0f;

            // Random seeds for noise decorrelation
            float seedX = Random.Range(0f, 1000f);
            float seedY = Random.Range(1000f, 2000f);
            float seedRot = Random.Range(2000f, 3000f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = Mathf.Clamp01(elapsed / duration);

                // Quadratic damping curve for punchy attack and smooth settle
                float damper = 1f - percent;
                damper = damper * damper;

                float sampleTime = elapsed * frequency;
                float offsetX = (Mathf.PerlinNoise(sampleTime, seedX) * 2f - 1f) * magnitude * damper;
                float offsetY = (Mathf.PerlinNoise(sampleTime, seedY) * 2f - 1f) * magnitude * damper;
                float offsetRoll = (Mathf.PerlinNoise(sampleTime, seedRot) * 2f - 1f) * (magnitude * 15f) * damper;

                transform.localPosition = _originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
                transform.localRotation = _originalLocalRotation * Quaternion.Euler(0f, 0f, offsetRoll);

                yield return null;
            }

            // Guaranteed zero-drift snapback
            transform.localPosition = _originalLocalPosition;
            transform.localRotation = _originalLocalRotation;
            _shakeCoroutine = null;
            _isShaking = false;
        }

        // =========================================================================
        // Static Global Convenience Triggers
        // =========================================================================

        /// <summary>
        /// Triggers standard light shake (e.g. projectile impact on ordinary wood block).
        /// </summary>
        public static void TriggerLight()
        {
            if (Instance != null)
            {
                Instance.PlayShake(0.12f, 0.05f, 30f);
            }
        }

        /// <summary>
        /// Triggers medium shake (e.g. heavy block collision, block falling off stand, high impulse).
        /// </summary>
        public static void TriggerMedium()
        {
            if (Instance != null)
            {
                Instance.PlayShake(0.22f, 0.12f, 22f);
            }
        }

        /// <summary>
        /// Triggers heavy cinematic shake (e.g. Mega Ball impact, explosive bottle shatter, major tower collapse).
        /// </summary>
        public static void TriggerHeavy()
        {
            if (Instance != null)
            {
                Instance.PlayShake(0.38f, 0.28f, 16f);
            }
        }

        /// <summary>
        /// Generic static shake invoker with custom parameters.
        /// </summary>
        public static void Shake(float duration, float magnitude, float frequency = 25f)
        {
            if (Instance != null)
            {
                Instance.PlayShake(duration, magnitude, frequency);
            }
        }
    }
}
