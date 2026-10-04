using System;
using System.Collections;
using UnityEngine;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Reusable UI component providing fluid spring pop-in and graceful fade pop-out animations for modals and panels.
    /// Operates on unscaled time, ensuring seamless operation during gameplay pauses.
    /// Follows Phase 36 Visual Polish & UX standards.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelAnimator : MonoBehaviour
    {
        [Header("Pop-In Settings")]
        [Range(0.4f, 0.95f)] [SerializeField] private float _startScale = 0.72f;
        [Range(1.01f, 1.20f)] [SerializeField] private float _overshootScale = 1.05f;
        [SerializeField] private float _inDuration = 0.25f;

        [Header("Pop-Out Settings")]
        [Range(0.6f, 0.98f)] [SerializeField] private float _exitScale = 0.85f;
        [SerializeField] private float _outDuration = 0.16f;

        private CanvasGroup _canvasGroup;
        private Vector3 _originalScale = Vector3.one;
        private Coroutine _animRoutine;

        public bool IsAnimating => _animRoutine != null;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
        }

        private void OnEnable()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
        }

        private void OnDisable()
        {
            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }
        }

        /// <summary>
        /// Smoothly pops in the panel from scale + alpha 0 with a juicy overshoot bounce.
        /// </summary>
        public void Show(Action onComplete = null)
        {
            // Ensure parent chain is active so activeInHierarchy is guaranteed true
            Transform current = transform.parent;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                }
                current = current.parent;
            }

            gameObject.SetActive(true);

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;

            if (_animRoutine != null) StopCoroutine(_animRoutine);

            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                _animRoutine = StartCoroutine(PopInRoutine(onComplete));
            }
            else
            {
                // Instant show in editor mode or fallback
                transform.localScale = _originalScale;
                if (_canvasGroup != null) _canvasGroup.alpha = 1f;
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Gracefully shrinks and fades out the panel before disabling its GameObject.
        /// </summary>
        public void Hide(Action onComplete = null)
        {
            if (!Application.isPlaying || !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(PopOutRoutine(onComplete));
        }

        private IEnumerator PopInRoutine(Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startVec = _originalScale * _startScale;
            Vector3 peakVec = _originalScale * _overshootScale;

            transform.localScale = startVec;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            float popDuration = _inDuration * 0.70f;
            float settleDuration = _inDuration * 0.30f;

            // Phase 1: Rapid scale up from start to overshoot with alpha fade-in
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                // Ease out quad
                float easedT = 1f - (1f - t) * (1f - t);

                transform.localScale = Vector3.Lerp(startVec, peakVec, easedT);
                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
                yield return null;
            }

            // Phase 2: Elastic settle back to original scale (1.0x)
            elapsed = 0f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);
                float easedT = Mathf.SmoothStep(0f, 1f, t);

                transform.localScale = Vector3.Lerp(peakVec, _originalScale, easedT);
                if (_canvasGroup != null) _canvasGroup.alpha = 1f;
                yield return null;
            }

            transform.localScale = _originalScale;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            _animRoutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator PopOutRoutine(Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startVec = transform.localScale;
            Vector3 exitVec = _originalScale * _exitScale;
            float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;

            while (elapsed < _outDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _outDuration);
                float easedT = t * t; // Ease in quad

                transform.localScale = Vector3.Lerp(startVec, exitVec, easedT);
                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easedT);
                yield return null;
            }

            transform.localScale = _originalScale;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            gameObject.SetActive(false);
            _animRoutine = null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Static convenience helper to animate any GameObject's pop-in, dynamically attaching UIPanelAnimator if needed.
        /// </summary>
        public static void AnimateIn(GameObject target, Action onComplete = null)
        {
            if (target == null) return;
            var anim = target.GetComponent<UIPanelAnimator>();
            if (anim == null) anim = target.AddComponent<UIPanelAnimator>();
            anim.Show(onComplete);
        }

        /// <summary>
        /// Static convenience helper to animate any GameObject's pop-out, dynamically attaching UIPanelAnimator if needed.
        /// </summary>
        public static void AnimateOut(GameObject target, Action onComplete = null)
        {
            if (target == null) return;
            var anim = target.GetComponent<UIPanelAnimator>();
            if (anim == null) anim = target.AddComponent<UIPanelAnimator>();
            anim.Hide(onComplete);
        }
    }
}
