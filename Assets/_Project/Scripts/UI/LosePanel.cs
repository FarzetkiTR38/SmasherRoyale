using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Levels;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Displays defeat popup when all balls are depleted with target objects still remaining.
    /// Features Retry button (reloads level) and Main Menu button.
    /// </summary>
    public class LosePanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _reasonText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Animation Settings")]
        [SerializeField] private float _popDuration = 0.35f;

        public bool IsVisible => _panelRoot != null ? _panelRoot.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (_panelRoot == null || _panelRoot == gameObject)
            {
                var content = transform.Find("Content");
                if (content != null)
                {
                    _panelRoot = content.gameObject;
                }
                else
                {
                    var card = transform.Find("CardContainer");
                    if (card != null) _panelRoot = card.gameObject;
                }
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(HandleRetryClicked);
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            }

            HideImmediate();
        }

        public void BindReferences(GameObject root, TextMeshProUGUI title, TextMeshProUGUI reason, Button retryBtn, Button menuBtn)
        {
            _panelRoot = root;
            _titleText = title;
            _reasonText = reason;
            _retryButton = retryBtn;
            _mainMenuButton = menuBtn;
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(HandleRetryClicked);
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            }
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(HandleRetryClicked);
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
            }
        }

        private void SubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLost -= HandleLevelLost;
                LevelManager.Instance.OnLevelLost += HandleLevelLost;
            }
        }

        private void UnsubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLost -= HandleLevelLost;
            }
        }

        private void HandleLevelLost(int level, int remainingTargets)
        {
            Show(level, remainingTargets);
        }

        /// <summary>
        /// Displays the Lose Panel with level information and remaining targets count.
        /// </summary>
        public void Show(int level, int remainingTargets)
        {
            if (_panelRoot == null) return;

            if (_titleText != null)
            {
                _titleText.text = "SEVİYE BAŞARISIZ!";
            }

            if (_reasonText != null)
            {
                _reasonText.text = $"Top kalmadı!\n({remainingTargets} hedef kaldı)";
            }

            _panelRoot.SetActive(true);

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayLoseSound();
            }

            if (RoyalSmash.Haptics.HapticManager.Instance != null)
            {
                RoyalSmash.Haptics.HapticManager.Instance.TriggerLoseHaptic();
            }

            StopAllCoroutines();
            StartCoroutine(PopInRoutine());

            if (RoyalSmash.Ads.AdManager.Instance != null)
            {
                RoyalSmash.Ads.AdManager.Instance.ShowLoseInterstitial();
            }
        }

        public void HideImmediate()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private IEnumerator PopInRoutine()
        {
            if (_panelRoot != null)
            {
                _panelRoot.transform.localScale = Vector3.one;
            }

            Transform cardTarget = transform.Find("CardContainer") ?? transform.Find("Content/CardContainer") ?? transform.Find("Panel_Lose");
            if (cardTarget == null && _panelRoot != null)
            {
                cardTarget = _panelRoot.transform.Find("CardContainer") ?? _panelRoot.transform.Find("Panel_Lose");
            }
            if (cardTarget == null)
            {
                cardTarget = _panelRoot != null ? _panelRoot.transform : transform;
            }

            cardTarget.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _popDuration);
                // Spring overshoot: quick rise to 1.10, gentle settle to 1.0
                float scale = (t < 0.7f)
                    ? Mathf.Lerp(0f, 1.10f, t / 0.7f)
                    : Mathf.Lerp(1.10f, 1.0f, (t - 0.7f) / 0.3f);
                cardTarget.localScale = Vector3.one * scale;
                yield return null;
            }

            cardTarget.localScale = Vector3.one;
        }

        private void HandleRetryClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
            HideImmediate();
        }

        private void HandleMainMenuClicked()
        {
            Debug.Log("[LosePanel] Main Menu clicked.");
            if (Core.SceneTransitionManager.Instance != null)
            {
                Core.SceneTransitionManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            HideImmediate();
        }
    }
}
