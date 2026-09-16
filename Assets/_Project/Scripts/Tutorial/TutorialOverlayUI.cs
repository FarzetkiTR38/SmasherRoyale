using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.Tutorial
{
    /// <summary>
    /// UI Controller for the First-Launch Tutorial overlay.
    /// Renders a dynamic pulsing target ring around the designated target object in screen space,
    /// an animated bobbing pointer hand, an instruction banner, and attention pulses for invalid taps.
    /// Strictly non-blocking so touches and taps pass freely to InputHandler.
    /// </summary>
    public class TutorialOverlayUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _targetRingRect;
        [SerializeField] private Image _targetRingImage;
        [SerializeField] private RectTransform _pointerHandRect;
        [SerializeField] private TextMeshProUGUI _instructionText;
        [SerializeField] private RectTransform _bannerRect;

        [Header("Runtime State")]
        private Transform _targetTransform;
        private TutorialConfig _config;
        private Camera _mainCamera;
        private bool _isActive;
        private Coroutine _pulseCoroutine;
        private Coroutine _fadeCoroutine;
        private Color _originalRingColor = new Color(1f, 0.85f, 0.15f, 0.9f); // Royal Gold

        public bool IsActive => _isActive;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Ensure tutorial UI elements never block raycasts or capture pointer clicks
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            if (_targetRingImage != null)
            {
                _originalRingColor = _targetRingImage.color;
            }

            _mainCamera = Camera.main;
        }

        public void BindReferences(CanvasGroup group, RectTransform ringRect, Image ringImg, RectTransform handRect, TextMeshProUGUI text, RectTransform banner)
        {
            _canvasGroup = group;
            _targetRingRect = ringRect;
            _targetRingImage = ringImg;
            _pointerHandRect = handRect;
            _instructionText = text;
            _bannerRect = banner;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            if (_targetRingImage != null)
            {
                _originalRingColor = _targetRingImage.color;
            }
        }

        public void ShowTutorial(Transform target, TutorialConfig config)
        {
            _targetTransform = target;
            _config = config;
            _isActive = true;
            _mainCamera = Camera.main;

            if (_instructionText != null && _config != null)
            {
                _instructionText.text = _config.InstructionText;
            }

            gameObject.SetActive(true);

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, _config != null ? _config.FadeDuration : 0.35f));

            UpdatePositions();
        }

        public void HideTutorial()
        {
            _isActive = false;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            float duration = _config != null ? _config.FadeDuration : 0.35f;
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, duration, () => gameObject.SetActive(false)));
        }

        public void HideImmediate()
        {
            _isActive = false;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void PulseTargetAttention()
        {
            if (!_isActive) return;
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(AttentionPulseRoutine());
        }

        private IEnumerator AttentionPulseRoutine()
        {
            if (_targetRingRect == null) yield break;

            Vector3 originalScale = Vector3.one;
            Vector3 peakScale = Vector3.one * 1.35f;
            Color warningColor = new Color(1f, 0.25f, 0.25f, 1f);

            if (_instructionText != null && _config != null)
            {
                _instructionText.text = _config.InvalidTapHint;
            }

            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float sinT = Mathf.Sin(t * Mathf.PI);

                _targetRingRect.localScale = Vector3.Lerp(originalScale, peakScale, sinT);
                if (_targetRingImage != null)
                {
                    _targetRingImage.color = Color.Lerp(_originalRingColor, warningColor, sinT);
                }

                yield return null;
            }

            _targetRingRect.localScale = originalScale;
            if (_targetRingImage != null) _targetRingImage.color = _originalRingColor;
            if (_instructionText != null && _config != null)
            {
                _instructionText.text = _config.InstructionText;
            }
            _pulseCoroutine = null;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete = null)
        {
            if (_canvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
            _fadeCoroutine = null;
        }

        private void Update()
        {
            if (!_isActive || _targetTransform == null) return;
            UpdatePositions();
            AnimateWidgets();
        }

        private void UpdatePositions()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null || _targetTransform == null) return;

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetTransform.position);

            if (screenPos.z < 0) return;

            if (_targetRingRect != null)
            {
                _targetRingRect.position = screenPos;
            }

            if (_pointerHandRect != null)
            {
                float bobOffset = 0f;
                if (_config != null)
                {
                    bobOffset = Mathf.Sin(Time.time * _config.FingerBobSpeed) * _config.FingerBobDistance;
                }
                Vector3 handBasePos = screenPos + new Vector3(0f, -115f + bobOffset, 0f);
                _pointerHandRect.position = handBasePos;
            }
        }

        private void AnimateWidgets()
        {
            if (_config == null || _targetRingRect == null) return;

            if (_pulseCoroutine == null)
            {
                float pulse = 1.0f + 0.08f * Mathf.Sin(Time.time * _config.RingPulseSpeed);
                _targetRingRect.localScale = new Vector3(pulse, pulse, 1f);
            }
        }
    }
}
