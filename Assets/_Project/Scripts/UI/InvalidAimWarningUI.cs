using System.Collections;
using UnityEngine;
using TMPro;
using RoyalSmash.Gameplay;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Displays a temporary floating warning banner when player attempts to tap below the stand.
    /// Positioned between the mortar cannon and the stand platform.
    /// Features smooth pop-in, bounce, and fade-out animation.
    /// </summary>
    public class InvalidAimWarningUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _bannerRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private RectTransform _containerRect;

        [Header("Animation Settings")]
        [SerializeField] private float _displayDuration = 1.6f;
        [SerializeField] private float _fadeDuration = 0.35f;

        private Coroutine _displayCoroutine;
        private InputHandler _inputHandler;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_containerRect == null) _containerRect = GetComponent<RectTransform>();

            HideImmediate();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_inputHandler == null) _inputHandler = FindFirstObjectByType<InputHandler>();
            if (_inputHandler != null)
            {
                _inputHandler.OnInvalidTapAttempted -= HandleInvalidTap;
                _inputHandler.OnInvalidTapAttempted += HandleInvalidTap;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnInvalidTapAttempted -= HandleInvalidTap;
            }
        }

        private void HandleInvalidTap(string message)
        {
            ShowWarning(message);
        }

        /// <summary>
        /// Shows the warning banner with custom message and floating bounce animation.
        /// </summary>
        public void ShowWarning(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
            }

            if (_bannerRoot != null && _bannerRoot != gameObject)
            {
                _bannerRoot.SetActive(true);
            }

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }
            _displayCoroutine = StartCoroutine(DisplayRoutine());
        }

        public void HideImmediate()
        {
            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
                _displayCoroutine = null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_bannerRoot != null && _bannerRoot != gameObject)
            {
                _bannerRoot.SetActive(false);
            }
        }

        private IEnumerator DisplayRoutine()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = false;
            }

            // Punch scale bounce
            float punchDuration = 0.22f;
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;

            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / punchDuration);
                // Overshoot to 1.1x then settle to 1.0x
                float scale = 1.0f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                if (_containerRect != null)
                {
                    _containerRect.localScale = baseScale * scale;
                }
                yield return null;
            }

            if (_containerRect != null) _containerRect.localScale = baseScale;

            // Wait visible
            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _fadeDuration);
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f - progress;
                }
                yield return null;
            }

            HideImmediate();
        }
    }
}
