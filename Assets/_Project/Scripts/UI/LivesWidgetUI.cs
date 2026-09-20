using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Lives;

namespace RoyalSmash.UI
{
    /// <summary>
    /// UI Widget displaying current lives and live regeneration countdown timer.
    /// Follows Master Build Prompt Section 11 & Mobile UI/UX Skill patterns.
    /// </summary>
    public class LivesWidgetUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _addLifeButton;
        [SerializeField] private OutOfLivesDialog _outOfLivesDialog;

        private void Start()
        {
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.OnLivesChanged += HandleLivesChanged;
                LivesManager.Instance.OnRegenTimerUpdated += HandleTimerUpdated;
                UpdateDisplay(LivesManager.Instance.CurrentLives, LivesManager.Instance.MaxLives);
            }

            if (_addLifeButton != null)
            {
                _addLifeButton.onClick.AddListener(HandleAddLifeClicked);
            }
        }

        private void OnDestroy()
        {
            if (LivesManager.HasInstance)
            {
                LivesManager.Instance.OnLivesChanged -= HandleLivesChanged;
                LivesManager.Instance.OnRegenTimerUpdated -= HandleTimerUpdated;
            }

            if (_addLifeButton != null)
            {
                _addLifeButton.onClick.RemoveListener(HandleAddLifeClicked);
            }
        }

        public void BindReferences(TextMeshProUGUI livesText, TextMeshProUGUI timerText, Button addBtn, OutOfLivesDialog dialog)
        {
            _livesText = livesText;
            _timerText = timerText;
            _addLifeButton = addBtn;
            _outOfLivesDialog = dialog;

            if (_addLifeButton != null)
            {
                _addLifeButton.onClick.RemoveAllListeners();
                _addLifeButton.onClick.AddListener(HandleAddLifeClicked);
            }
        }

        private void HandleLivesChanged(int current, int max)
        {
            UpdateDisplay(current, max);
        }

        private void HandleTimerUpdated(TimeSpan timeRemaining)
        {
            if (_timerText == null) return;

            if (LivesManager.Instance != null && LivesManager.Instance.AreLivesFull)
            {
                _timerText.text = "DOLU";
            }
            else
            {
                _timerText.text = $"{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
            }
        }

        private void UpdateDisplay(int current, int max)
        {
            if (_livesText != null)
            {
                _livesText.text = $"{current}/{max}";
            }

            if (_timerText != null)
            {
                if (current >= max)
                {
                    _timerText.text = "DOLU";
                }
                else if (LivesManager.Instance != null)
                {
                    TimeSpan remaining = LivesManager.Instance.GetTimeUntilNextLife();
                    _timerText.text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }
            }
        }

        private void HandleAddLifeClicked()
        {
            if (_outOfLivesDialog != null)
            {
                _outOfLivesDialog.Show();
            }
            else
            {
                var dialog = FindFirstObjectByType<OutOfLivesDialog>();
                if (dialog != null) dialog.Show();
            }
        }
    }
}
