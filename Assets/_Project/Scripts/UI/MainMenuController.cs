using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Core;
using RoyalSmash.Save;
using RoyalSmash.Lives;
using RoyalSmash.Economy;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Master controller for the Main Menu scene.
    /// Manages level progression display, Play button gating, dialog popups, special offers, and bottom navigation.
    /// Follows Master Build Prompt Section 17 & Phase 11 specifications.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Progression Display")]
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private Button _playButton;

        [Header("Top Bar Buttons")]
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _settingsButton;

        [Header("Currency & Lives Widgets")]
        [SerializeField] private GoldDisplayUI _goldWidget;
        [SerializeField] private LivesWidgetUI _livesWidget;

        [Header("Dialogs")]
        [SerializeField] private InGameSettingsDialog _settingsDialog;
        [SerializeField] private OutOfLivesDialog _outOfLivesDialog;
        [SerializeField] private ProfileDialog _profileDialog;
        [SerializeField] private UsernamePromptDialog _usernamePromptDialog;

        [Header("Navigation")]
        [SerializeField] private BottomNavigationUI _bottomNavigation;

        [Header("Special Offers")]
        [SerializeField] private TextMeshProUGUI _offerCountdownText;
        [SerializeField] private Button _leftOfferButton;
        [SerializeField] private Button _rightOfferButton;

        private float _offerTimeRemaining = 86400f; // 24 hours countdown

        private void Awake()
        {
            if (_playButton != null) _playButton.onClick.AddListener(HandlePlayClicked);
            if (_profileButton != null) _profileButton.onClick.AddListener(HandleProfileClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(HandleSettingsClicked);
            if (_leftOfferButton != null) _leftOfferButton.onClick.AddListener(HandleLeftOfferClicked);
            if (_rightOfferButton != null) _rightOfferButton.onClick.AddListener(HandleRightOfferClicked);
        }

        private void Start()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayMainMenuBGM();
            }

            RefreshUI();
            StartCoroutine(OfferCountdownRoutine());
            CheckLevel10UsernamePrompt();
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(HandlePlayClicked);
            if (_profileButton != null) _profileButton.onClick.RemoveListener(HandleProfileClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            if (_leftOfferButton != null) _leftOfferButton.onClick.RemoveListener(HandleLeftOfferClicked);
            if (_rightOfferButton != null) _rightOfferButton.onClick.RemoveListener(HandleRightOfferClicked);
        }

        public void BindReferences(
            TextMeshProUGUI levelTxt, Button playBtn, Button profileBtn, Button settingsBtn,
            GoldDisplayUI goldWgt, LivesWidgetUI livesWgt,
            InGameSettingsDialog settingsDlg, OutOfLivesDialog outOfLivesDlg,
            BottomNavigationUI nav, TextMeshProUGUI offerTxt = null,
            ProfileDialog profileDlg = null, UsernamePromptDialog userPromptDlg = null)
        {
            _currentLevelText = levelTxt;
            _playButton = playBtn;
            _profileButton = profileBtn;
            _settingsButton = settingsBtn;
            _goldWidget = goldWgt;
            _livesWidget = livesWgt;
            _settingsDialog = settingsDlg;
            _outOfLivesDialog = outOfLivesDlg;
            _profileDialog = profileDlg;
            _usernamePromptDialog = userPromptDlg;
            _bottomNavigation = nav;
            _offerCountdownText = offerTxt;

            if (_playButton != null)
            {
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(HandlePlayClicked);
            }
            if (_profileButton != null)
            {
                _profileButton.onClick.RemoveAllListeners();
                _profileButton.onClick.AddListener(HandleProfileClicked);
            }
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(HandleSettingsClicked);
            }

            RefreshUI();
            CheckLevel10UsernamePrompt();
        }

        public void CheckLevel10UsernamePrompt()
        {
            if (RoyalSmash.Profile.ProfileManager.Instance != null &&
                RoyalSmash.Profile.ProfileManager.Instance.ShouldPromptLevel10Username())
            {
                if (_usernamePromptDialog != null)
                {
                    _usernamePromptDialog.Show();
                }
                else if (UsernamePromptDialog.Instance != null)
                {
                    UsernamePromptDialog.Instance.Show();
                }
            }
        }

        public void RefreshUI()
        {
            int currentLevel = 1;
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                currentLevel = SaveManager.Instance.Data.CurrentLevel;
            }

            if (_currentLevelText != null)
            {
                _currentLevelText.text = $"Aşama {currentLevel}";
            }

            if (_goldWidget != null)
            {
                _goldWidget.RefreshDisplay();
            }

            RefreshProfileButtonVisual();
        }

        public void RefreshProfileButtonVisual()
        {
            if (_profileButton != null && RoyalSmash.Profile.ProfileManager.Instance != null)
            {
                var curAv = RoyalSmash.Profile.ProfileManager.Instance.CurrentAvatar;
                var curFr = RoyalSmash.Profile.ProfileManager.Instance.CurrentFrame;
                var iconImg = _profileButton.transform.Find("AvatarIcon")?.GetComponent<Image>();
                var frameImg = _profileButton.transform.Find("FrameBorder")?.GetComponent<Image>();

                if (iconImg != null && curAv != null)
                {
                    if (curAv.Icon != null) iconImg.sprite = curAv.Icon;
                    iconImg.color = curAv.ThemeColor;
                }
                if (frameImg != null && curFr != null)
                {
                    if (curFr.BorderSprite != null) frameImg.sprite = curFr.BorderSprite;
                    frameImg.color = curFr.BorderColor;
                }
            }
        }

        public void HandlePlayClicked()
        {
            // Zero-Lives Gating (Master Build Prompt Section 1.3 Rule 18)
            if (LivesManager.Instance != null && !LivesManager.Instance.HasLivesToPlay)
            {
                Debug.LogWarning("[MainMenuController] Zero lives remaining. Opening OutOfLivesDialog.");
                if (_outOfLivesDialog != null)
                {
                    _outOfLivesDialog.Show();
                }
                return;
            }

            Debug.Log("[MainMenuController] Starting Level via SceneTransitionManager...");
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadGameplay();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
            }
        }

        private void HandleProfileClicked()
        {
            Debug.Log("[MainMenuController] Profile clicked.");
            if (_profileDialog != null)
            {
                _profileDialog.Show();
            }
            else if (ProfileDialog.Instance != null)
            {
                ProfileDialog.Instance.Show();
            }
        }

        private void HandleSettingsClicked()
        {
            if (_settingsDialog != null)
            {
                _settingsDialog.Show();
            }
        }

        private void HandleLeftOfferClicked()
        {
            Debug.Log("[MainMenuController] Special Starter Bundle clicked. (Shop IAP in Phase 13)");
        }

        private void HandleRightOfferClicked()
        {
            Debug.Log("[MainMenuController] Limited Time Offer clicked. (Shop IAP in Phase 13)");
        }

        private IEnumerator OfferCountdownRoutine()
        {
            while (_offerTimeRemaining > 0f)
            {
                _offerTimeRemaining -= Time.deltaTime;
                if (_offerCountdownText != null)
                {
                    TimeSpan span = TimeSpan.FromSeconds(Math.Max(0, _offerTimeRemaining));
                    _offerCountdownText.text = $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
                }
                yield return null;
            }
        }
    }
}
