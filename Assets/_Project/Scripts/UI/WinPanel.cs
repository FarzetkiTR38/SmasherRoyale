using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Levels;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Displays victory popup when a level is cleared.
    /// Shows Level Completed title, Base Gold, Remaining Ball Bonus (N x 2), Total Gold, and Continue button.
    /// </summary>
    public class WinPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private TextMeshProUGUI _baseRewardText;
        [SerializeField] private TextMeshProUGUI _remainingBonusText;
        [SerializeField] private TextMeshProUGUI _totalGoldText;
        [SerializeField] private Button _continueButton;

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

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(HandleContinueClicked);
            }

            HideImmediate();
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
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        private void SubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
                LevelManager.Instance.OnLevelWon += HandleLevelWon;
            }
        }

        private void UnsubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
            }
        }

        private void HandleLevelWon(int level, int baseReward, int remainingBalls, int totalGold)
        {
            Show(level, baseReward, remainingBalls, totalGold);
        }

        /// <summary>
        /// Displays the Win Panel populated with reward calculation data.
        /// </summary>
        public void Show(int level, int baseReward, int remainingBalls, int totalGold)
        {
            if (_panelRoot == null) return;

            if (_levelTitleText != null)
            {
                _levelTitleText.text = $"LEVEL {level} COMPLETED!";
            }

            if (_baseRewardText != null)
            {
                _baseRewardText.text = $"Base Reward: +{baseReward} Gold";
            }

            if (_remainingBonusText != null)
            {
                int ballBonus = remainingBalls * 2;
                _remainingBonusText.text = $"Ball Bonus ({remainingBalls} × 2): +{ballBonus} Gold";
            }

            if (_totalGoldText != null)
            {
                _totalGoldText.text = $"+{totalGold} GOLD";
            }

            _panelRoot.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(PopInRoutine());
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

            Transform cardTarget = transform.Find("Content/CardContainer") ?? transform.Find("CardContainer");
            if (cardTarget == null && _panelRoot != null)
            {
                cardTarget = _panelRoot.transform.Find("CardContainer");
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
                float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.05f;
                if (t >= 0.85f) scale = Mathf.Lerp(1.05f, 1.0f, (t - 0.85f) / 0.15f);
                cardTarget.localScale = Vector3.one * scale;
                yield return null;
            }

            cardTarget.localScale = Vector3.one;
        }

        private void HandleContinueClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.NextLevel();
            }
            HideImmediate();
        }
    }
}
