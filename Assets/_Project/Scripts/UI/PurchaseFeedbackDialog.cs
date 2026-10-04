using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Modal dialog displaying purchase confirmation or transaction error details.
    /// Follows Master Build Prompt Section 35 Phase 13 requirements.
    /// </summary>
    public class PurchaseFeedbackDialog : MonoBehaviour
    {
        [Header("Dialog Elements")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _confirmButton;

        public static PurchaseFeedbackDialog Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (_confirmButton != null) _confirmButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindReferences(GameObject root, TextMeshProUGUI title, TextMeshProUGUI message, Image icon, Button confirm)
        {
            _contentRoot = root;
            _titleText = title;
            _messageText = message;
            _iconImage = icon;
            _confirmButton = confirm;

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(Hide);
            }
        }

        public void ShowSuccess(string title, string message, Sprite icon = null)
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayShopPurchase();
            }

            if (_titleText != null)
            {
                _titleText.text = string.IsNullOrEmpty(title) ? "PURCHASE SUCCESSFUL!" : title;
                _titleText.color = new Color(0.3f, 1f, 0.4f);
            }
            if (_messageText != null) _messageText.text = message;

            if (_iconImage != null)
            {
                _iconImage.gameObject.SetActive(icon != null);
                if (icon != null) _iconImage.sprite = icon;
            }

            SetVisible(true);
        }

        public void ShowError(string message)
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelOpen();
            }

            if (_titleText != null)
            {
                _titleText.text = "TRANSACTION FAILED";
                _titleText.color = new Color(1f, 0.35f, 0.35f);
            }
            if (_messageText != null)
            {
                _messageText.text = string.IsNullOrEmpty(message) ? "Could not complete purchase. Please try again." : message;
            }

            if (_iconImage != null) _iconImage.gameObject.SetActive(false);

            SetVisible(true);
        }

        public void Hide()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelClose();
            }
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (_contentRoot != null) _contentRoot.SetActive(visible);
            else gameObject.SetActive(visible);
        }
    }
}
