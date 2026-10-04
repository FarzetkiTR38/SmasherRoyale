using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Profile;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Modal dialog prompted at Level 10 requesting the player to enter their Royal Username.
    /// Follows Master Build Prompt Section 18.1 requirement.
    /// </summary>
    public class UsernamePromptDialog : MonoBehaviour
    {
        [Header("Roots & Elements")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        public static UsernamePromptDialog Instance { get; private set; }

        public event Action<string> OnUsernameSubmitted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (_confirmButton != null) _confirmButton.onClick.AddListener(HandleConfirm);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindReferences(GameObject root, TMP_InputField input, Button confirmBtn,
            TextMeshProUGUI title = null, TextMeshProUGUI subtitle = null)
        {
            _contentRoot = root;
            _usernameInput = input;
            _confirmButton = confirmBtn;
            _titleText = title;
            _subtitleText = subtitle;

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(HandleConfirm);
            }
        }

        public void Show()
        {
            if (_usernameInput != null)
            {
                string cur = ProfileManager.Instance != null ? ProfileManager.Instance.Username : "Player";
                _usernameInput.text = cur == "Player" ? "" : cur;
                _usernameInput.ActivateInputField();
            }

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelOpen();
            }

            // Explicitly activate dialog root GameObject
            gameObject.SetActive(true);

            if (_contentRoot != null && _contentRoot != gameObject)
            {
                _contentRoot.SetActive(true);
                UIPanelAnimator.AnimateIn(_contentRoot);
            }
            else
            {
                UIPanelAnimator.AnimateIn(gameObject);
            }
        }

        public void Hide()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelClose();
            }

            if (_contentRoot != null && _contentRoot != gameObject && _contentRoot.activeInHierarchy)
            {
                UIPanelAnimator.AnimateOut(_contentRoot, () =>
                {
                    if (_contentRoot != null) _contentRoot.SetActive(false);
                    gameObject.SetActive(false);
                });
            }
            else if (gameObject.activeInHierarchy)
            {
                UIPanelAnimator.AnimateOut(gameObject, () => gameObject.SetActive(false));
            }
            else
            {
                if (_contentRoot != null) _contentRoot.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        public void HandleConfirm()
        {
            string chosenName = _usernameInput != null ? _usernameInput.text.Trim() : "";
            if (string.IsNullOrWhiteSpace(chosenName))
            {
                chosenName = "RoyalSmasher";
            }

            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.SetUsername(chosenName);
                ProfileManager.Instance.MarkUsernamePrompted();
            }

            OnUsernameSubmitted?.Invoke(chosenName);
            Debug.Log($"[UsernamePromptDialog] Username confirmed at Level 10: '{chosenName}'.");
            Hide();
        }
    }
}
