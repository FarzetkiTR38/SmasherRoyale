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
        [SerializeField] private Button _sfxToggleButton;
        [SerializeField] private TextMeshProUGUI _sfxStatusText;
        [SerializeField] private Button _vibrationToggleButton;
        [SerializeField] private TextMeshProUGUI _vibrationStatusText;

        public static InGameSettingsDialog Instance { get; private set; }

        private float _savedTimeScale = 1f;
        private bool _isTimePaused = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Hide);
            if (_exitLevelButton != null) _exitLevelButton.onClick.AddListener(HandleExitLevelClicked);
            if (_musicToggleButton != null) _musicToggleButton.onClick.AddListener(ToggleMusic);
            if (_sfxToggleButton != null) _sfxToggleButton.onClick.AddListener(ToggleSfx);
            if (_vibrationToggleButton != null) _vibrationToggleButton.onClick.AddListener(ToggleVibration);
        }

        private void Start()
        {
            UpdateToggleVisuals();
        }

        private void OnDestroy()
        {
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
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.MusicEnabled : true;
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

            UpdateToggleVisuals();
        }

        public void ToggleSfx()
        {
            var saveMgr = SaveManager.Instance;
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.SfxEnabled : true;
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

            UpdateToggleVisuals();
        }

        public void ToggleVibration()
        {
            var saveMgr = SaveManager.Instance;
            bool current = saveMgr != null && saveMgr.Data != null ? saveMgr.Data.VibrationEnabled : true;
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

            UpdateToggleVisuals();
        }

        public void UpdateToggleVisuals()
        {
            var saveMgr = SaveManager.Instance;
            bool musicOn = saveMgr == null || saveMgr.Data == null || saveMgr.Data.MusicEnabled;
            bool sfxOn = saveMgr == null || saveMgr.Data == null || saveMgr.Data.SfxEnabled;
            bool vibOn = saveMgr == null || saveMgr.Data == null || saveMgr.Data.VibrationEnabled;

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
        }
    }
}
