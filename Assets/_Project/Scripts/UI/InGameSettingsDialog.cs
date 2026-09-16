using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Save;

namespace RoyalSmash.UI
{
    /// <summary>
    /// In-game Settings modal dialog opened via HUD settings button.
    /// Provides audio toggles, resume action, and level exit button that triggers ExitConfirmDialog.
    /// </summary>
    public class InGameSettingsDialog : MonoBehaviour
    {
        [Header("Root & Dialog Elements")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitLevelButton;

        [Header("Audio Toggles")]
        [SerializeField] private Button _musicToggleButton;
        [SerializeField] private TextMeshProUGUI _musicStatusText;
        [SerializeField] private Button _sfxToggleButton;
        [SerializeField] private TextMeshProUGUI _sfxStatusText;

        public static InGameSettingsDialog Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Hide);
            if (_exitLevelButton != null) _exitLevelButton.onClick.AddListener(HandleExitLevelClicked);
            if (_musicToggleButton != null) _musicToggleButton.onClick.AddListener(ToggleMusic);
            if (_sfxToggleButton != null) _sfxToggleButton.onClick.AddListener(ToggleSfx);
        }

        private void Start()
        {
            UpdateToggleVisuals();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindReferences(GameObject root, Button closeBtn, Button resumeBtn, Button exitBtn,
            Button musicBtn, TextMeshProUGUI musicTxt, Button sfxBtn, TextMeshProUGUI sfxTxt)
        {
            _contentRoot = root;
            _closeButton = closeBtn;
            _resumeButton = resumeBtn;
            _exitLevelButton = exitBtn;
            _musicToggleButton = musicBtn;
            _musicStatusText = musicTxt;
            _sfxToggleButton = sfxBtn;
            _sfxStatusText = sfxTxt;

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

            UpdateToggleVisuals();
        }

        public void Show()
        {
            UpdateToggleVisuals();
            if (_contentRoot != null) _contentRoot.SetActive(true);
            else gameObject.SetActive(true);
        }

        public void Hide()
        {
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

        private void ToggleMusic()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.MusicEnabled = !saveMgr.Data.MusicEnabled;
                saveMgr.Save();
            }
            UpdateToggleVisuals();
        }

        private void ToggleSfx()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.Data != null)
            {
                saveMgr.Data.SfxEnabled = !saveMgr.Data.SfxEnabled;
                saveMgr.Save();
            }
            UpdateToggleVisuals();
        }

        private void UpdateToggleVisuals()
        {
            var saveMgr = SaveManager.Instance;
            bool musicOn = saveMgr == null || saveMgr.Data == null || saveMgr.Data.MusicEnabled;
            bool sfxOn = saveMgr == null || saveMgr.Data == null || saveMgr.Data.SfxEnabled;

            if (_musicStatusText != null)
            {
                _musicStatusText.text = musicOn ? "AÇIK" : "KAPALI";
                _musicStatusText.color = musicOn ? new Color(0.3f, 1f, 0.3f) : new Color(0.8f, 0.8f, 0.8f);
            }

            if (_sfxStatusText != null)
            {
                _sfxStatusText.text = sfxOn ? "AÇIK" : "KAPALI";
                _sfxStatusText.color = sfxOn ? new Color(0.3f, 1f, 0.3f) : new Color(0.8f, 0.8f, 0.8f);
            }
        }
    }
}
