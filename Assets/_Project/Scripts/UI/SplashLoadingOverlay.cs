using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Manages the full-screen visual Splash and Loading sequence with rotating tips,
    /// animated progress bar, and smooth CanvasGroup alpha fading.
    /// Follows Master Build Prompt Section 29 and Royal Smash Full Docs Sections 35, 36, 300, 301.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SplashLoadingOverlay : MonoBehaviour
    {
        private static SplashLoadingOverlay _instance;
        public static SplashLoadingOverlay Instance => _instance;

        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private CanvasGroup _splashGroup;
        [SerializeField] private CanvasGroup _loadingGroup;

        [Header("Splash Elements")]
        [SerializeField] private TextMeshProUGUI _gameTitleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Loading Elements")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Image _progressFill;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _tipText;

        [Header("Animation Durations")]
        [SerializeField] private float _fadeDuration = 0.4f;
        [SerializeField] private float _tipInterval = 3.5f;

        private static readonly string[] GAMEPLAY_TIPS = new string[]
        {
            "Aim for the base of tall columns to trigger massive chain reactions!",
            "Mega Ball creates heavy physical impact that topples even dense stone blocks.",
            "Infinity Ball grants unlimited rapid firing with zero cooldown for 5 seconds.",
            "Balls that bounce back can strike and knock down secondary targets!",
            "Clearing all objects with balls remaining awards bonus Gold (+2 Gold per ball).",
            "Paint bottles shatter on impact and coat the stand in slippery paint.",
            "Drag to swivel and aim your mortar without accidentally firing.",
            "Tap anywhere on the target structure to fire directly at that world coordinate."
        };

        private Coroutine _tipRotationRoutine;
        private Coroutine _fadeRoutine;

        public CanvasGroup MainCanvasGroup
        {
            get
            {
                if (_mainCanvasGroup == null) EnsureComponents();
                return _mainCanvasGroup;
            }
        }

        public bool IsVisible => MainCanvasGroup != null && MainCanvasGroup.alpha > 0.01f;

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

            EnsureComponents();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void EnsureComponents()
        {
            if (_mainCanvasGroup == null)
            {
                _mainCanvasGroup = GetComponent<CanvasGroup>();
                if (_mainCanvasGroup == null) _mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Displays the initial Splash title, holding for displayDuration, then fades to the loading screen.
        /// </summary>
        public IEnumerator PlaySplashSequence(float displayDuration = 1.2f)
        {
            EnsureComponents();
            _mainCanvasGroup.alpha = 1f;
            _mainCanvasGroup.blocksRaycasts = true;

            if (_splashGroup != null) _splashGroup.alpha = 1f;
            if (_loadingGroup != null) _loadingGroup.alpha = 0f;

            if (_gameTitleText != null) _gameTitleText.text = "ROYAL SMASH!";
            if (_subtitleText != null) _subtitleText.text = "PHYSICS PUZZLE";

            yield return new WaitForSeconds(displayDuration);

            // Cross-fade from splash to loading
            yield return StartCoroutine(CrossFadeSplashToLoading(0.35f));
        }

        private IEnumerator CrossFadeSplashToLoading(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (_splashGroup != null) _splashGroup.alpha = 1f - t;
                if (_loadingGroup != null) _loadingGroup.alpha = t;

                yield return null;
            }

            if (_splashGroup != null) _splashGroup.alpha = 0f;
            if (_loadingGroup != null) _loadingGroup.alpha = 1f;

            StartTipRotation();
        }

        /// <summary>
        /// Updates the progress bar and percentage display.
        /// </summary>
        public void SetProgress(float progress01)
        {
            float clamped = Mathf.Clamp01(progress01);

            if (_progressBar != null) _progressBar.value = clamped;
            if (_progressFill != null) _progressFill.fillAmount = clamped;
            if (_progressText != null) _progressText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
        }

        /// <summary>
        /// Starts cycling through random gameplay tips.
        /// </summary>
        public void StartTipRotation()
        {
            if (_tipRotationRoutine != null) StopCoroutine(_tipRotationRoutine);
            _tipRotationRoutine = StartCoroutine(TipRotationCoroutine());
        }

        private IEnumerator TipRotationCoroutine()
        {
            int tipIndex = UnityEngine.Random.Range(0, GAMEPLAY_TIPS.Length);
            while (true)
            {
                if (_tipText != null)
                {
                    _tipText.text = GAMEPLAY_TIPS[tipIndex];
                }
                tipIndex = (tipIndex + 1) % GAMEPLAY_TIPS.Length;
                yield return new WaitForSecondsRealtime(_tipInterval);
            }
        }

        /// <summary>
        /// Smoothly fades out the entire overlay and disables raycasts.
        /// </summary>
        public IEnumerator FadeOutOverlay(float customDuration = -1f)
        {
            if (_tipRotationRoutine != null)
            {
                StopCoroutine(_tipRotationRoutine);
                _tipRotationRoutine = null;
            }

            float duration = customDuration > 0f ? customDuration : _fadeDuration;
            float startAlpha = _mainCanvasGroup != null ? _mainCanvasGroup.alpha : 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (_mainCanvasGroup != null)
                {
                    _mainCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                }
                yield return null;
            }

            if (_mainCanvasGroup != null)
            {
                _mainCanvasGroup.alpha = 0f;
                _mainCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Smoothly fades in the overlay to black to mask a scene transition.
        /// </summary>
        public IEnumerator FadeInOverlay(float customDuration = -1f)
        {
            EnsureComponents();
            float duration = customDuration > 0f ? customDuration : _fadeDuration;
            float startAlpha = _mainCanvasGroup.alpha;
            float elapsed = 0f;

            _mainCanvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _mainCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
                yield return null;
            }

            _mainCanvasGroup.alpha = 1f;
        }

        public string GetRandomTip()
        {
            return GAMEPLAY_TIPS[UnityEngine.Random.Range(0, GAMEPLAY_TIPS.Length)];
        }
    }
}
