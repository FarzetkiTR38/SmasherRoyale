using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Profile;
using RoyalSmash.Save;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Modal dialog for inspecting and editing player profile.
    /// Supports selecting from 8 default avatars, 4 default frames,
    /// unlimited free username editing, and viewing stats (Section 18).
    /// </summary>
    public class ProfileDialog : MonoBehaviour
    {
        [Header("Dialog Roots")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private Button _closeButton;

        [Header("Current Profile Preview")]
        [SerializeField] private Image _currentAvatarImage;
        [SerializeField] private Image _currentFrameImage;
        [SerializeField] private TextMeshProUGUI _currentUsernameText;
        [SerializeField] private TMP_InputField _usernameInputField;
        [SerializeField] private Button _editUsernameButton;
        [SerializeField] private Button _saveUsernameButton;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private TextMeshProUGUI _totalShotsText;
        [SerializeField] private TextMeshProUGUI _totalWinsText;
        [SerializeField] private TextMeshProUGUI _completedLevelsText;

        [Header("Selection Lists")]
        [SerializeField] private Button[] _avatarButtons = new Button[8];
        [SerializeField] private Image[] _avatarSelectionOutlines = new Image[8];
        [SerializeField] private Button[] _frameButtons = new Button[4];
        [SerializeField] private Image[] _frameSelectionOutlines = new Image[4];

        public static ProfileDialog Instance { get; private set; }

        private bool _isEditingUsername = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_editUsernameButton != null) _editUsernameButton.onClick.AddListener(StartEditingUsername);
            if (_saveUsernameButton != null) _saveUsernameButton.onClick.AddListener(SaveUsername);

            BindSelectionButtons();
        }

        private void Start()
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.OnProfileUpdated += HandleProfileUpdated;
                ProfileManager.Instance.OnStatsUpdated += RefreshStats;
            }
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (ProfileManager.HasInstance)
            {
                ProfileManager.Instance.OnProfileUpdated -= HandleProfileUpdated;
                ProfileManager.Instance.OnStatsUpdated -= RefreshStats;
            }
            if (Instance == this) Instance = null;
        }

        public void BindReferences(
            GameObject root, Button closeBtn,
            Image avatarImg, Image frameImg, TextMeshProUGUI userTxt,
            TMP_InputField userInp, Button editBtn, Button saveBtn,
            TextMeshProUGUI levelTxt, TextMeshProUGUI shotsTxt, TextMeshProUGUI winsTxt, TextMeshProUGUI clearedTxt,
            Button[] avButtons, Image[] avOutlines,
            Button[] frButtons, Image[] frOutlines)
        {
            _contentRoot = root;
            _closeButton = closeBtn;
            _currentAvatarImage = avatarImg;
            _currentFrameImage = frameImg;
            _currentUsernameText = userTxt;
            _usernameInputField = userInp;
            _editUsernameButton = editBtn;
            _saveUsernameButton = saveBtn;

            _currentLevelText = levelTxt;
            _totalShotsText = shotsTxt;
            _totalWinsText = winsTxt;
            _completedLevelsText = clearedTxt;

            _avatarButtons = avButtons;
            _avatarSelectionOutlines = avOutlines;
            _frameButtons = frButtons;
            _frameSelectionOutlines = frOutlines;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Hide);
            }
            if (_editUsernameButton != null)
            {
                _editUsernameButton.onClick.RemoveAllListeners();
                _editUsernameButton.onClick.AddListener(StartEditingUsername);
            }
            if (_saveUsernameButton != null)
            {
                _saveUsernameButton.onClick.RemoveAllListeners();
                _saveUsernameButton.onClick.AddListener(SaveUsername);
            }

            BindSelectionButtons();
            RefreshDisplay();
        }

        private void BindSelectionButtons()
        {
            if (_avatarButtons != null)
            {
                for (int i = 0; i < _avatarButtons.Length; i++)
                {
                    int index = i;
                    if (_avatarButtons[i] != null)
                    {
                        _avatarButtons[i].onClick.RemoveAllListeners();
                        _avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
                    }
                }
            }

            if (_frameButtons != null)
            {
                for (int i = 0; i < _frameButtons.Length; i++)
                {
                    int index = i;
                    if (_frameButtons[i] != null)
                    {
                        _frameButtons[i].onClick.RemoveAllListeners();
                        _frameButtons[i].onClick.AddListener(() => SelectFrame(index));
                    }
                }
            }
        }

        public void Show()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelOpen();
            }

            _isEditingUsername = false;
            UpdateUsernameEditState();
            RefreshDisplay();
            if (_contentRoot != null) _contentRoot.SetActive(true);
            else gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelClose();
            }

            if (_contentRoot != null) _contentRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void RefreshDisplay()
        {
            var pm = ProfileManager.Instance;
            if (pm == null) return;

            var avatar = pm.CurrentAvatar;
            var frame = pm.CurrentFrame;

            if (_currentAvatarImage != null && avatar != null)
            {
                if (avatar.Icon != null) _currentAvatarImage.sprite = avatar.Icon;
                _currentAvatarImage.color = avatar.ThemeColor;
            }

            if (_currentFrameImage != null && frame != null)
            {
                if (frame.BorderSprite != null) _currentFrameImage.sprite = frame.BorderSprite;
                _currentFrameImage.color = frame.BorderColor;
            }

            if (_currentUsernameText != null)
            {
                _currentUsernameText.text = pm.Username;
            }

            if (_usernameInputField != null)
            {
                _usernameInputField.text = pm.Username;
            }

            // Update selection outlines
            int curAv = pm.AvatarId;
            if (_avatarSelectionOutlines != null)
            {
                for (int i = 0; i < _avatarSelectionOutlines.Length; i++)
                {
                    if (_avatarSelectionOutlines[i] != null)
                    {
                        _avatarSelectionOutlines[i].gameObject.SetActive(i == curAv);
                    }
                }
            }

            int curFr = pm.FrameId;
            if (_frameSelectionOutlines != null)
            {
                for (int i = 0; i < _frameSelectionOutlines.Length; i++)
                {
                    if (_frameSelectionOutlines[i] != null)
                    {
                        _frameSelectionOutlines[i].gameObject.SetActive(i == curFr);
                    }
                }
            }

            // Populate avatar buttons and frame buttons icons from Config
            if (pm.Config != null)
            {
                if (_avatarButtons != null)
                {
                    for (int i = 0; i < _avatarButtons.Length && i < pm.Config.Avatars.Length; i++)
                    {
                        if (_avatarButtons[i] != null)
                        {
                            var img = _avatarButtons[i].GetComponent<Image>();
                            if (img != null)
                            {
                                img.sprite = pm.Config.Avatars[i].Icon;
                                img.color = pm.Config.Avatars[i].ThemeColor;
                            }
                        }
                    }
                }

                if (_frameButtons != null)
                {
                    for (int i = 0; i < _frameButtons.Length && i < pm.Config.Frames.Length; i++)
                    {
                        if (_frameButtons[i] != null)
                        {
                            var img = _frameButtons[i].GetComponent<Image>();
                            if (img != null)
                            {
                                img.sprite = pm.Config.Frames[i].BorderSprite;
                                img.color = pm.Config.Frames[i].BorderColor;
                            }
                        }
                    }
                }
            }

            RefreshStats();
        }

        private void RefreshStats()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            int level = save != null ? save.CurrentLevel : 1;
            int shots = save != null ? save.TotalShotsFired : 0;
            int wins = save != null ? save.TotalWins : 0;
            int cleared = save != null ? save.CompletedLevelsCount : 0;

            if (_currentLevelText != null) _currentLevelText.text = $"LEVEL {level}";
            if (_totalShotsText != null) _totalShotsText.text = shots.ToString("N0");
            if (_totalWinsText != null) _totalWinsText.text = wins.ToString("N0");
            if (_completedLevelsText != null) _completedLevelsText.text = cleared.ToString("N0");
        }

        private void HandleProfileUpdated(string user, int av, int fr)
        {
            RefreshDisplay();
        }

        public void SelectAvatar(int id)
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.SetAvatar(id);
            }
        }

        public void SelectFrame(int id)
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.SetFrame(id);
            }
        }

        private void StartEditingUsername()
        {
            _isEditingUsername = true;
            UpdateUsernameEditState();
            if (_usernameInputField != null)
            {
                _usernameInputField.ActivateInputField();
            }
        }

        public void SaveUsername()
        {
            if (_usernameInputField != null && ProfileManager.Instance != null)
            {
                string text = _usernameInputField.text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ProfileManager.Instance.SetUsername(text);
                }
            }
            _isEditingUsername = false;
            UpdateUsernameEditState();
        }

        private void UpdateUsernameEditState()
        {
            if (_currentUsernameText != null)
            {
                _currentUsernameText.gameObject.SetActive(!_isEditingUsername);
            }
            if (_editUsernameButton != null)
            {
                _editUsernameButton.gameObject.SetActive(!_isEditingUsername);
            }
            if (_usernameInputField != null)
            {
                _usernameInputField.gameObject.SetActive(_isEditingUsername);
            }
            if (_saveUsernameButton != null)
            {
                _saveUsernameButton.gameObject.SetActive(_isEditingUsername);
            }
        }
    }
}
