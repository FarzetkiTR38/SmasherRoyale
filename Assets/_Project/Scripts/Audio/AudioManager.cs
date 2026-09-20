using System;
using System.Collections.Generic;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Audio
{
    /// <summary>
    /// Central audio manager for Royal Smash.
    /// Controls Background Music and Sound Effects (SFX) channels.
    /// Manages voice pooling, object-specific acoustic impacts, pitch variance, and anti-clipping.
    /// Follows Master Build Prompt Section 21 & Section 35 (Phase 16).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private AudioConfig _config;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float _defaultMusicVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float _defaultSfxVolume = 1f;

        private AudioSource[] _voicePool;
        private int _currentVoiceIndex = 0;
        private float _lastImpactTime = -999f;
        private const float MIN_IMPACT_INTERVAL = 0.04f; // 40ms impact throttle to prevent audio clipping

        public bool IsMusicEnabled { get; private set; } = true;
        public bool IsSfxEnabled { get; private set; } = true;

        public event Action<bool> OnMusicStateChanged;
        public event Action<bool> OnSfxStateChanged;

        public AudioConfig Config
        {
            get
            {
                if (_config == null)
                {
#if UNITY_EDITOR
                    _config = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioConfig>(
                        "Assets/_Project/ScriptableObjects/Config/AudioConfig.asset");
#endif
                    if (_config == null)
                    {
                        _config = ScriptableObject.CreateInstance<AudioConfig>();
                    }
                }
                return _config;
            }
            set => _config = value;
        }

        private void Awake()
        {
            _isQuitting = false;
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
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded += HandleSaveLoaded;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (SaveManager.HasInstance)
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

            // Create multi-voice pool for sound effects to prevent cutoff
            int voiceCount = Config != null ? Config.MaxSimultaneousVoices : 8;
            _voicePool = new AudioSource[voiceCount];
            for (int i = 0; i < voiceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.volume = _defaultSfxVolume;
                _voicePool[i] = src;
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

            if (_voicePool != null)
            {
                foreach (var voice in _voicePool)
                {
                    if (voice != null) voice.mute = !enabled;
                }
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
        public void PlaySfx(AudioClip clip, float volumeScale = 1f, bool randomizePitch = false)
        {
            if (!IsSfxEnabled || clip == null) return;

            AudioSource voice = GetAvailableVoice();
            if (voice != null)
            {
                if (randomizePitch && Config != null && Config.PitchRandomizationRange > 0f)
                {
                    float range = Config.PitchRandomizationRange;
                    voice.pitch = UnityEngine.Random.Range(1f - range, 1f + range);
                }
                else
                {
                    voice.pitch = 1f;
                }

                voice.PlayOneShot(clip, volumeScale);
            }
            else if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        private AudioSource GetAvailableVoice()
        {
            if (_voicePool == null || _voicePool.Length == 0) return _sfxSource;

            // Round-robin selection
            var voice = _voicePool[_currentVoiceIndex];
            _currentVoiceIndex = (_currentVoiceIndex + 1) % _voicePool.Length;
            return voice;
        }

        /// <summary>
        /// Plays background music track.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (_musicSource != null)
            {
                if (_musicSource.clip == clip && _musicSource.isPlaying) return;

                _musicSource.clip = clip;
                _musicSource.mute = !IsMusicEnabled;
                _musicSource.Play();
            }
        }

        // =========================================================================
        // Section 21 Specific SFX Methods
        // =========================================================================

        public void PlayBallLaunch()
        {
            if (Config != null && Config.BallLaunch != null)
            {
                PlaySfx(Config.BallLaunch, 1f, randomizePitch: true);
            }
        }

        public void PlayImpact(ObjectType type, float impulse = 1f)
        {
            if (Config == null) return;
            if (impulse < Config.MinCollisionImpulse) return;

            // Throttle rapid collisions to prevent audio distortion
            if (Time.unscaledTime - _lastImpactTime < MIN_IMPACT_INTERVAL) return;
            _lastImpactTime = Time.unscaledTime;

            float volume = Mathf.Clamp01(0.5f + (impulse * 0.1f));

            switch (type)
            {
                case ObjectType.Box:
                    PlaySfx(Config.BoxImpact, volume, randomizePitch: true);
                    break;
                case ObjectType.Stone:
                    PlaySfx(Config.StoneImpact, volume, randomizePitch: true);
                    break;
                case ObjectType.Column:
                    PlaySfx(Config.ColumnImpact, volume, randomizePitch: true);
                    break;
                case ObjectType.Cone:
                    PlaySfx(Config.ConeImpact, volume, randomizePitch: true);
                    break;
                case ObjectType.PaintBottle:
                    PlayBottleBreak();
                    break;
                default:
                    PlaySfx(Config.BoxImpact, volume, randomizePitch: true);
                    break;
            }
        }

        public void PlayBottleBreak()
        {
            if (Config != null && Config.PaintBottleBreak != null)
            {
                PlaySfx(Config.PaintBottleBreak, 1f, randomizePitch: true);
            }
        }

        public void PlayObjectDestruction()
        {
            if (Config != null && Config.ObjectDestruction != null)
            {
                PlaySfx(Config.ObjectDestruction, 0.9f, randomizePitch: true);
            }
        }

        public void PlayButtonClick()
        {
            if (Config != null && Config.ButtonClick != null)
            {
                PlaySfx(Config.ButtonClick, 0.8f, randomizePitch: false);
            }
        }

        public void PlayPanelOpen()
        {
            if (Config != null && Config.PanelOpen != null)
            {
                PlaySfx(Config.PanelOpen, 0.85f, randomizePitch: false);
            }
        }

        public void PlayGameStart()
        {
            if (Config != null && Config.GameStart != null)
            {
                PlaySfx(Config.GameStart, 1f, randomizePitch: false);
            }
        }

        public void PlayWinSound()
        {
            if (Config != null && Config.WinFanfare != null)
            {
                PlaySfx(Config.WinFanfare, 1f, randomizePitch: false);
            }
        }

        public void PlayLoseSound()
        {
            if (Config != null && Config.LoseSound != null)
            {
                PlaySfx(Config.LoseSound, 1f, randomizePitch: false);
            }
        }

        public void PlayRewardSound()
        {
            if (Config != null && Config.RewardSound != null)
            {
                PlaySfx(Config.RewardSound, 1f, randomizePitch: false);
            }
        }

        public void PlayMainMenuBGM()
        {
            if (Config != null && Config.BgmMainMenu != null)
            {
                PlayMusic(Config.BgmMainMenu);
            }
        }

        public void PlayGameplayBGM()
        {
            if (Config != null && Config.BgmGameplay != null)
            {
                PlayMusic(Config.BgmGameplay);
            }
        }
    }
}
