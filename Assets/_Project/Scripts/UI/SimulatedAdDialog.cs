using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Visual simulated ad playback dialog for testing ad experiences in Unity Editor and dev builds.
    /// Provides full inspection of Interstitial and Rewarded ad flows without requiring live external ad SDKs.
    /// </summary>
    public class SimulatedAdDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _dialogRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _typeBadgeText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _simulateFailButton;
        [SerializeField] private Slider _progressSlider;

        private Action _onCompleted;
        private Action _onCancelled;
        private Action<string> _onError;
        private Coroutine _timerCoroutine;
        private bool _isRewarded;

        private void Awake()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(HandleCloseClicked);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(HandleCancelClicked);
            if (_simulateFailButton != null) _simulateFailButton.onClick.AddListener(HandleSimulateFailClicked);
            HideImmediate();
        }

        public void BindReferences(GameObject root, TextMeshProUGUI title, TextMeshProUGUI badge,
            TextMeshProUGUI timer, TextMeshProUGUI desc, Button closeBtn, Button cancelBtn, Button failBtn, Slider progress)
        {
            _dialogRoot = root;
            _titleText = title;
            _typeBadgeText = badge;
            _timerText = timer;
            _descriptionText = desc;
            _closeButton = closeBtn;
            _cancelButton = cancelBtn;
            _simulateFailButton = failBtn;
            _progressSlider = progress;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(HandleCloseClicked);
            }
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(HandleCancelClicked);
            }
            if (_simulateFailButton != null)
            {
                _simulateFailButton.onClick.RemoveAllListeners();
                _simulateFailButton.onClick.AddListener(HandleSimulateFailClicked);
            }
        }

        public void ShowSimulatedAd(bool isRewarded, float duration, Action onCompleted, Action onCancelled = null, Action<string> onError = null)
        {
            _isRewarded = isRewarded;
            _onCompleted = onCompleted;
            _onCancelled = onCancelled;
            _onError = onError;

            if (_dialogRoot != null) _dialogRoot.SetActive(true);
            else gameObject.SetActive(true);

            if (_titleText != null)
            {
                _titleText.text = isRewarded ? "REWARDED AD" : "SPONSORED BREAK";
            }
            if (_typeBadgeText != null)
            {
                _typeBadgeText.text = isRewarded ? "WATCH TO RECEIVE +1 LIFE" : "INTERSTITIAL AD";
            }
            if (_descriptionText != null)
            {
                _descriptionText.text = isRewarded
                    ? "Thank you for supporting Royal Smash! Your reward will be granted upon completion."
                    : "Royal Smash sponsored message. Thanks for playing!";
            }

            if (_closeButton != null) _closeButton.gameObject.SetActive(false);
            if (_cancelButton != null) _cancelButton.gameObject.SetActive(isRewarded);
            if (_simulateFailButton != null) _simulateFailButton.gameObject.SetActive(true);

            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(CountdownRoutine(duration));
        }

        private IEnumerator CountdownRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float remaining = Mathf.Max(0f, duration - elapsed);

                if (_timerText != null)
                {
                    _timerText.text = $"Ad finishes in {remaining:F1}s...";
                }
                if (_progressSlider != null && duration > 0f)
                {
                    _progressSlider.value = Mathf.Clamp01(elapsed / duration);
                }

                yield return null;
            }

            if (_timerText != null) _timerText.text = "Completed!";
            if (_progressSlider != null) _progressSlider.value = 1f;

            if (_closeButton != null)
            {
                _closeButton.gameObject.SetActive(true);
            }

            // Auto complete if desired or wait 0.3s
            yield return new WaitForSecondsRealtime(0.25f);
            HandleCloseClicked();
        }

        private void HandleCloseClicked()
        {
            HideImmediate();
            var cb = _onCompleted;
            _onCompleted = null;
            cb?.Invoke();
        }

        private void HandleCancelClicked()
        {
            HideImmediate();
            var cb = _onCancelled;
            _onCancelled = null;
            cb?.Invoke();
        }

        private void HandleSimulateFailClicked()
        {
            HideImmediate();
            var cb = _onError;
            _onError = null;
            cb?.Invoke("Simulated ad network load failure (Test Event)");
        }

        public void HideImmediate()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            if (_dialogRoot != null) _dialogRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
