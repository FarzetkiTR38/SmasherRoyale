using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RoyalSmash.Audio;
using RoyalSmash.Haptics;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Micro-animation component that attaches to UI buttons to provide tactile "juice",
    /// including squash-and-stretch press feedback, punch bounce on release, audio clicks, and haptics.
    /// Follows Royal Smash Full Documentation Section 344 (UX Acceptance Criteria).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
    {
        [Header("Scale Animation Settings")]
        [Range(0.7f, 1.0f)] [SerializeField] private float _pressedScale = 0.85f;
        [Range(1.0f, 1.25f)] [SerializeField] private float _punchScale = 1.05f;
        [SerializeField] private float _pressDuration = 0.07f;
        [SerializeField] private float _releaseDuration = 0.16f;

        [Header("Sensory Feedback")]
        [SerializeField] private bool _playClickAudio = true;
        [SerializeField] private bool _triggerHaptic = false; // Section 23: UI clicks do not vibrate by default

        private Vector3 _originalScale;
        private Coroutine _scaleRoutine;
        private Button _button;
        private bool _isPressed = false;

        public Vector3 OriginalScale => _originalScale;
        public float PressedScale => _pressedScale;
        public bool IsPressed => _isPressed;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_originalScale == Vector3.zero)
            {
                _originalScale = Vector3.one;
            }
            transform.localScale = _originalScale;
            _isPressed = false;
        }

        private void OnDisable()
        {
            if (_scaleRoutine != null)
            {
                StopCoroutine(_scaleRoutine);
                _scaleRoutine = null;
            }
            if (_originalScale != Vector3.zero)
            {
                transform.localScale = _originalScale;
            }
            _isPressed = false;
        }

        private Button TargetButton
        {
            get
            {
                if (_button == null) _button = GetComponent<Button>();
                return _button;
            }
        }

        public void SetBaseScale(Vector3 scale)
        {
            _originalScale = scale;
            if (!_isPressed)
            {
                transform.localScale = scale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (TargetButton != null && !TargetButton.interactable) return;

            _isPressed = true;
            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            _scaleRoutine = StartCoroutine(ScaleCoroutine(transform.localScale, _originalScale * _pressedScale, _pressDuration));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;

            if (TargetButton != null && !TargetButton.interactable) return;

            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            _scaleRoutine = StartCoroutine(BounceBackCoroutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;

            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            _scaleRoutine = StartCoroutine(ScaleCoroutine(transform.localScale, _originalScale, 0.10f));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (TargetButton != null && !TargetButton.interactable) return;

            if (_playClickAudio && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            if (_triggerHaptic && HapticManager.Instance != null)
            {
                HapticManager.Instance.PlayHaptic(HapticType.Light);
            }
        }

        private IEnumerator ScaleCoroutine(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth sine ease-in-out
                float easedT = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
                transform.localScale = Vector3.Lerp(from, to, easedT);
                yield return null;
            }
            transform.localScale = to;
            _scaleRoutine = null;
        }

        private IEnumerator BounceBackCoroutine()
        {
            // Overshoot slightly for juicy tactile feel, then settle smoothly to 1.0x
            Vector3 peakScale = _originalScale * _punchScale;
            float halfDuration = _releaseDuration * 0.45f;

            yield return StartCoroutine(ScaleCoroutine(transform.localScale, peakScale, halfDuration));
            yield return StartCoroutine(ScaleCoroutine(peakScale, _originalScale, _releaseDuration * 0.55f));
            transform.localScale = _originalScale;
            _scaleRoutine = null;
        }
    }
}
