using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Save;
using RoyalSmash.Audio;
using RoyalSmash.Utils;

namespace RoyalSmash.UI
{
    /// <summary>
    /// In-game and Main Menu Settings modal dialog (Section 20).
    /// Provides 3 toggles: Music (BGM), Sound (SFX), and Vibration (Haptics).
    /// Safely preserves gameplay state when opened/closed.
    /// </summary>
    public class InGameSettingsDialog : MonoBehaviour
    {
        [Header("Root & Dialog Elements")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitLevelButton;

        [Header("Toggles (Section 20)")]
        [SerializeField] private Button _musicToggleButton;
        [SerializeField] private TextMeshProUGUI _musicStatusText;
        [SerializeField] private Image _musicIconImage;
        [SerializeField] private Image _musicSwitchImage;
        [SerializeField] private Button _sfxToggleButton;
        [SerializeField] private TextMeshProUGUI _sfxStatusText;
        [SerializeField] private Image _sfxIconImage;
        [SerializeField] private Image _sfxSwitchImage;
        [SerializeField] private Button _vibrationToggleButton;
        [SerializeField] private TextMeshProUGUI _vibrationStatusText;
        [SerializeField] private Image _vibrationIconImage;
        [SerializeField] private Image _vibrationSwitchImage;

        [Header("Switch Sprites")]
        [SerializeField] private Sprite _switchOnSprite;
        [SerializeField] private Sprite _switchOffSprite;

        [Header("Extra Buttons (Main Menu Settings)")]
        [SerializeField] private Button _supportButton;
        [SerializeField] private Button _termsButton;
        [SerializeField] private Button _privacyButton;

        public static InGameSettingsDialog Instance { get; private set; }
        public bool IsVisible => _contentRoot != null ? _contentRoot.activeSelf : gameObject.activeSelf;

        private float _savedTimeScale = 1f;
        private bool _isTimePaused = false;
        private bool _previewMusic = true;
        private bool _previewSfx = true;
        private bool _previewVib = true;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Hide);
            if (_exitLevelButton != null) _exitLevelButton.onClick.AddListener(HandleExitLevelClicked);
            if (_musicToggleButton != null) _musicToggleButton.onClick.AddListener(ToggleMusic);
            if (_sfxToggleButton != null) _sfxToggleButton.onClick.AddListener(ToggleSfx);
            if (_vibrationToggleButton != null) _vibrationToggleButton.onClick.AddListener(ToggleVibration);
            if (_supportButton != null) _supportButton.onClick.AddListener(HandleSupportClicked);
            if (_termsButton != null) _termsButton.onClick.AddListener(HandleTermsClicked);
            if (_privacyButton != null) _privacyButton.onClick.AddListener(HandlePrivacyClicked);
        }

        private void Start()
        {
            UpdateToggleVisuals();
        }

        private void OnDestroy()
        {
            if (_supportButton != null) _supportButton.onClick.RemoveListener(HandleSupportClicked);
            if (_termsButton != null) _termsButton.onClick.RemoveListener(HandleTermsClicked);
            if (_privacyButton != null) _privacyButton.onClick.RemoveListener(HandlePrivacyClicked);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Hide);
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(Hide);
            if (_exitLevelButton != null) _exitLevelButton.onClick.RemoveListener(HandleExitLevelClicked);
            if (_musicToggleButton != null) _musicToggleButton.onClick.RemoveListener(ToggleMusic);
            if (_sfxToggleButton != null) _sfxToggleButton.onClick.RemoveListener(ToggleSfx);
            if (_vibrationToggleButton != null) _vibrationToggleButton.onClick.RemoveListener(ToggleVibration);

            if (_isTimePaused)
            {
                Time.timeScale = _savedTimeScale;
                _isTimePaused = false;
            }
            if (Instance == this) Instance = null;
        }

        public void BindReferences(GameObject root, Button closeBtn, Button resumeBtn, Button exitBtn,
            Button musicBtn, TextMeshProUGUI musicTxt, Button sfxBtn, TextMeshProUGUI sfxTxt,
            Button vibBtn = null, TextMeshProUGUI vibTxt = null)
        {
            _contentRoot = root;
            _closeButton = closeBtn;
            _resumeButton = resumeBtn;
            _exitLevelButton = exitBtn;
            _musicToggleButton = musicBtn;
            _musicStatusText = musicTxt;
            _sfxToggleButton = sfxBtn;
            _sfxStatusText = sfxTxt;
            _vibrationToggleButton = vibBtn;
            _vibrationStatusText = vibTxt;

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Hide);
            }
            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveAllListeners();
                _resumeButton.onClick.AddListener(Hide);
            }
            if (_exitLevelButton != null)
            {
                _exitLevelButton.onClick.RemoveAllListeners();
                _exitLevelButton.onClick.AddListener(HandleExitLevelClicked);
            }
            if (_musicToggleButton != null)
            {
                _musicToggleButton.onClick.RemoveAllListeners();
                _musicToggleButton.onClick.AddListener(ToggleMusic);
            }
            if (_sfxToggleButton != null)
            {
                _sfxToggleButton.onClick.RemoveAllListeners();
                _sfxToggleButton.onClick.AddListener(ToggleSfx);
            }
            if (_vibrationToggleButton != null)
            {
                _vibrationToggleButton.onClick.RemoveAllListeners();
                _vibrationToggleButton.onClick.AddListener(ToggleVibration);
            }

            UpdateToggleVisuals();
        }

        public void Show()
        {
            UpdateToggleVisuals();

            // Safe non-destructive pause during active gameplay
            bool isGameplay = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGame";
            if (isGameplay && !_isTimePaused && Application.isPlaying)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _isTimePaused = true;
            }

            if (_contentRoot != null) _contentRoot.SetActive(true);
            else gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_isTimePaused && Application.isPlaying)
            {
                Time.timeScale = _savedTimeScale > 0f ? _savedTimeScale : 1f;
                _isTimePaused = false;
            }

            if (_contentRoot != null) _contentRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void HandleExitLevelClicked()
        {
            Hide();
            var exitConfirm = FindFirstObjectByType<ExitConfirmDialog>();
            if (exitConfirm != null)
            {
                exitConfirm.Show();
            }
            else
            {
                Debug.LogWarning("[InGameSettingsDialog] ExitConfirmDialog not found in scene!");
            }
        }

        public void ToggleMusic()
        {
            var saveMgr = SaveManager.Instance;
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.MusicEnabled : _previewMusic;
            bool newState = !current;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicEnabled(newState, true);
            }
            else if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.MusicEnabled = newState;
                saveMgr.Save();
            }
            else
            {
                _previewMusic = newState;
            }

            UpdateToggleVisuals();
        }

        public void ToggleSfx()
        {
            var saveMgr = SaveManager.Instance;
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.SfxEnabled : _previewSfx;
            bool newState = !current;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSfxEnabled(newState, true);
            }
            else if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.SfxEnabled = newState;
                saveMgr.Save();
            }
            else
            {
                _previewSfx = newState;
            }

            UpdateToggleVisuals();
        }

        public void ToggleVibration()
        {
            var saveMgr = SaveManager.Instance;
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.VibrationEnabled : _previewVib;
            bool newState = !current;

            if (VibrationManager.Instance != null)
            {
                VibrationManager.Instance.SetVibrationEnabled(newState, true);
            }
            else if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.VibrationEnabled = newState;
                saveMgr.Save();
            }
            else
            {
                _previewVib = newState;
            }

            UpdateToggleVisuals();
        }

        public void UpdateToggleVisuals()
        {
            var saveMgr = SaveManager.Instance;
            bool musicOn = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.MusicEnabled : _previewMusic;
            bool sfxOn = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.SfxEnabled : _previewSfx;
            bool vibOn = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.VibrationEnabled : _previewVib;

            Color onColor = new Color(0.3f, 1f, 0.3f);
            Color offColor = new Color(0.7f, 0.7f, 0.7f);

            if (_musicStatusText != null)
            {
                _musicStatusText.text = musicOn ? "ON" : "OFF";
                _musicStatusText.color = musicOn ? onColor : offColor;
            }

            if (_sfxStatusText != null)
            {
                _sfxStatusText.text = sfxOn ? "ON" : "OFF";
                _sfxStatusText.color = sfxOn ? onColor : offColor;
            }

            if (_vibrationStatusText != null)
            {
                _vibrationStatusText.text = vibOn ? "ON" : "OFF";
                _vibrationStatusText.color = vibOn ? onColor : offColor;
            }

            Color iconActive = Color.white;
            Color iconInactive = new Color(0.35f, 0.45f, 0.6f, 0.55f);

            if (_musicIconImage != null) _musicIconImage.color = musicOn ? iconActive : iconInactive;
            if (_sfxIconImage != null) _sfxIconImage.color = sfxOn ? iconActive : iconInactive;
            if (_vibrationIconImage != null) _vibrationIconImage.color = vibOn ? iconActive : iconInactive;

            if (_musicSwitchImage != null)
            {
                if (musicOn && _switchOnSprite != null) _musicSwitchImage.sprite = _switchOnSprite;
                else if (!musicOn && _switchOffSprite != null) _musicSwitchImage.sprite = _switchOffSprite;
            }
            if (_sfxSwitchImage != null)
            {
                if (sfxOn && _switchOnSprite != null) _sfxSwitchImage.sprite = _switchOnSprite;
                else if (!sfxOn && _switchOffSprite != null) _sfxSwitchImage.sprite = _switchOffSprite;
            }
            if (_vibrationSwitchImage != null)
            {
                if (vibOn && _switchOnSprite != null) _vibrationSwitchImage.sprite = _switchOnSprite;
                else if (!vibOn && _switchOffSprite != null) _vibrationSwitchImage.sprite = _switchOffSprite;
            }
        }

        private void HandleSupportClicked()
        {
            Debug.Log("[InGameSettingsDialog] Support button clicked!");
            Application.OpenURL("mailto:support@smasherroyale.com");
        }

        private void HandleTermsClicked()
        {
            Debug.Log("[InGameSettingsDialog] Terms & Privacy button clicked!");
            Application.OpenURL("https://smasherroyale.com/terms");
        }

        private void HandlePrivacyClicked()
        {
            Debug.Log("[InGameSettingsDialog] Privacy Options button clicked!");
            Application.OpenURL("https://smasherroyale.com/privacy");
        }
    }
}
