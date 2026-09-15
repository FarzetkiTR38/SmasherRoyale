using System.Collections;
using UnityEngine;
using TMPro;
using RoyalSmash.Gameplay;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Controls the in-game HUD, including the remaining ball counter display and micro-animations.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MortarController _mortarController;
        [SerializeField] private TextMeshProUGUI _ballCountText;
        [SerializeField] private RectTransform _ballCountCard;

        [Header("Animation Settings")]
        [SerializeField] private float _punchScale = 1.25f;
        [SerializeField] private float _punchDuration = 0.2f;

        private Coroutine _punchCoroutine;
        private Vector3 _originalCardScale = Vector3.one;

        private void Awake()
        {
            if (_ballCountCard != null)
            {
                _originalCardScale = _ballCountCard.localScale;
            }
        }

        private void Start()
        {
            if (_mortarController == null)
            {
                _mortarController = FindFirstObjectByType<MortarController>();
            }

            if (_mortarController != null)
            {
                _mortarController.OnBallCountChanged += UpdateBallCount;
                UpdateBallCount(_mortarController.RemainingBalls);
            }
        }

        private void OnDestroy()
        {
            if (_mortarController != null)
            {
                _mortarController.OnBallCountChanged -= UpdateBallCount;
            }
        }

        /// <summary>
        /// Update the remaining ball count text and trigger punch animation.
        /// </summary>
        public void UpdateBallCount(int remaining)
        {
            if (_ballCountText != null)
            {
                _ballCountText.text = remaining.ToString();
            }

            if (_ballCountCard != null && gameObject.activeInHierarchy)
            {
                if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
                _punchCoroutine = StartCoroutine(PunchCardRoutine());
            }
        }

        private IEnumerator PunchCardRoutine()
        {
            float elapsed = 0f;
            Vector3 targetPunch = _originalCardScale * _punchScale;

            // Scale up
            while (elapsed < _punchDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (_punchDuration * 0.5f);
                _ballCountCard.localScale = Vector3.Lerp(_originalCardScale, targetPunch, t);
                yield return null;
            }

            // Return to original
            elapsed = 0f;
            while (elapsed < _punchDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (_punchDuration * 0.5f);
                _ballCountCard.localScale = Vector3.Lerp(targetPunch, _originalCardScale, t);
                yield return null;
            }

            _ballCountCard.localScale = _originalCardScale;
            _punchCoroutine = null;
        }
    }
}
