using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Attention-grabbing breathing pulse and highlight effect for key call-to-action buttons (e.g. Play / Battle button).
    /// Dynamically synchronizes with UIButtonJuice so press scaling (0.85x) takes precedence when touched.
    /// Follows Phase 36 Visual Polish specifications.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonPulseEffect : MonoBehaviour
    {
        [Header("Breathing Pulse Settings")]
        [Tooltip("Maximum scale expansion ratio (e.g. 0.045 = 1.00x to 1.045x).")]
        [Range(0.01f, 0.12f)] [SerializeField] private float _pulseMagnitude = 0.045f;

        [Tooltip("Speed/frequency of the breathing cycle in radians per second.")]
        [Range(1f, 8f)] [SerializeField] private float _pulseSpeed = 3.2f;

        [Header("Shimmer / Sheen Settings")]
        [Tooltip("Optional child Image or sheen overlay that sweeps across the button.")]
        [SerializeField] private RectTransform _sheenRect;
        [SerializeField] private float _sheenInterval = 3.5f;
        [SerializeField] private float _sheenDuration = 0.65f;

        private Vector3 _originalScale;
        private UIButtonJuice _buttonJuice;
        private Coroutine _sheenRoutine;
        private float _pulsePhase = 0f;

        public Vector3 OriginalScale => _originalScale;
        public float PulseMagnitude { get => _pulseMagnitude; set => _pulseMagnitude = value; }
        public float PulseSpeed { get => _pulseSpeed; set => _pulseSpeed = value; }

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;

            _buttonJuice = GetComponent<UIButtonJuice>();
            if (_sheenRect == null)
            {
                var sheenChild = transform.Find("Shine") ?? transform.Find("Sheen");
                if (sheenChild != null) _sheenRect = sheenChild.GetComponent<RectTransform>();
            }
        }

        private void OnEnable()
        {
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
            transform.localScale = _originalScale;
            _pulsePhase = 0f;

            if (_sheenRect != null)
            {
                if (_sheenRoutine != null) StopCoroutine(_sheenRoutine);
                _sheenRoutine = StartCoroutine(SheenSweepRoutine());
            }
        }

        private void OnDisable()
        {
            if (_sheenRoutine != null)
            {
                StopCoroutine(_sheenRoutine);
                _sheenRoutine = null;
            }
            if (_originalScale != Vector3.zero)
            {
                transform.localScale = _originalScale;
            }
        }

        private void Update()
        {
            // If the button is currently being held down by the player, give priority to UIButtonJuice press scale
            if (_buttonJuice != null && _buttonJuice.IsPressed)
            {
                return;
            }

            // Smooth organic sinusoidal breathing pulse
            _pulsePhase += Time.unscaledDeltaTime * _pulseSpeed;
            float pulse = Mathf.Sin(_pulsePhase) * _pulseMagnitude;
            transform.localScale = _originalScale * (1f + pulse);
        }

        private IEnumerator SheenSweepRoutine()
        {
            var parentRect = GetComponent<RectTransform>();
            if (_sheenRect == null || parentRect == null) yield break;

            float width = parentRect.rect.width > 10f ? parentRect.rect.width : 250f;
            float startX = -width * 0.8f;
            float endX = width * 0.8f;

            var cg = _sheenRect.GetComponent<CanvasGroup>();

            while (true)
            {
                yield return new WaitForSecondsRealtime(_sheenInterval);

                float elapsed = 0f;
                while (elapsed < _sheenDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / _sheenDuration);
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    Vector2 pos = _sheenRect.anchoredPosition;
                    pos.x = Mathf.Lerp(startX, endX, smoothT);
                    _sheenRect.anchoredPosition = pos;

                    if (cg != null)
                    {
                        // Peak alpha in the middle of sweep
                        cg.alpha = Mathf.Sin(t * Mathf.PI) * 0.75f;
                    }

                    yield return null;
                }

                // Reset position off-screen
                Vector2 resetPos = _sheenRect.anchoredPosition;
                resetPos.x = startX;
                _sheenRect.anchoredPosition = resetPos;
                if (cg != null) cg.alpha = 0f;
            }
        }
    }
}
