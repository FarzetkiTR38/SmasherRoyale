using System;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Audio
{
    /// <summary>
    /// Central audio manager for Royal Smash.
    /// Controls Background Music and Sound Effects (SFX) channels.
    /// Synchronizes directly with persistent settings in PlayerSaveData.
    /// Follows Master Build Prompt Section 20 specifications.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float _defaultMusicVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float _defaultSfxVolume = 1f;

        public bool IsMusicEnabled { get; private set; } = true;
        public bool IsSfxEnabled { get; private set; } = true;

        public event Action<bool> OnMusicStateChanged;
        public event Action<bool> OnSfxStateChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                InitializeSources();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SyncWithSaveData();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveLoaded += HandleSaveLoaded;
            }
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
            }
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void InitializeSources()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
                _musicSource.volume = _defaultMusicVolume;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
                _sfxSource.volume = _defaultSfxVolume;
            }
        }

        private void HandleSaveLoaded(PlayerSaveData data)
        {
            SyncWithSaveData();
        }

        /// <summary>
        /// Synchronizes audio channels with current save data settings.
        /// </summary>
        public void SyncWithSaveData()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (save != null)
            {
                SetMusicEnabled(save.MusicEnabled, false);
                SetSfxEnabled(save.SfxEnabled, false);
            }
            else
            {
                SetMusicEnabled(true, false);
                SetSfxEnabled(true, false);
            }
        }

        /// <summary>
        /// Enables or mutes background music.
        /// </summary>
        public void SetMusicEnabled(bool enabled, bool updateSave = true)
        {
            IsMusicEnabled = enabled;
            if (_musicSource != null)
            {
                _musicSource.mute = !enabled;
            }

            if (updateSave && SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.MusicEnabled = enabled;
                SaveManager.Instance.Save();
            }

            OnMusicStateChanged?.Invoke(enabled);
            Debug.Log($"[AudioManager] Music set to {(enabled ? "ON" : "OFF")}.");
        }

        /// <summary>
        /// Enables or mutes sound effects.
        /// </summary>
        public void SetSfxEnabled(bool enabled, bool updateSave = true)
        {
            IsSfxEnabled = enabled;
            if (_sfxSource != null)
            {
                _sfxSource.mute = !enabled;
            }

            if (updateSave && SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.SfxEnabled = enabled;
                SaveManager.Instance.Save();
            }

            OnSfxStateChanged?.Invoke(enabled);
            Debug.Log($"[AudioManager] SFX set to {(enabled ? "ON" : "OFF")}.");
        }

        /// <summary>
        /// Plays a sound effect clip if SFX is enabled.
        /// </summary>
        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (!IsSfxEnabled || clip == null) return;
            if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        /// <summary>
        /// Plays background music track.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (_musicSource != null)
            {
                _musicSource.clip = clip;
                _musicSource.mute = !IsMusicEnabled;
                _musicSource.Play();
            }
        }
    }
}
