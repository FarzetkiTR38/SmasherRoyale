using UnityEngine;

namespace RoyalSmash.Audio
{
    /// <summary>
    /// Configuration asset containing all SFX clips, BGM tracks, and acoustic parameters.
    /// Follows Master Build Prompt Section 21 & Section 35 (Phase 16).
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "RoyalSmash/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Background Music Tracks")]
        [SerializeField] private AudioClip _bgmMainMenu;
        [SerializeField] private AudioClip _bgmGameplay;

        [Header("Combat & Launch")]
        [SerializeField] private AudioClip _ballLaunch;

        [Header("Object-Specific Impacts (Section 21.1)")]
        [SerializeField] private AudioClip _boxImpact;
        [SerializeField] private AudioClip _stoneImpact;
        [SerializeField] private AudioClip _columnImpact;
        [SerializeField] private AudioClip _coneImpact;
        [SerializeField] private AudioClip _paintBottleBreak;
        [SerializeField] private AudioClip _objectDestruction;

        [Header("UI & Game States")]
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _winFanfare;
        [SerializeField] private AudioClip _loseSound;
        [SerializeField] private AudioClip _rewardSound;

        [Header("Dynamics & Anti-Clipping (Section 35)")]
        [Tooltip("Slight random pitch variance (+/- range) for natural sound variety.")]
        [Range(0f, 0.2f)] [SerializeField] private float _pitchRandomizationRange = 0.06f;
        [Tooltip("Maximum simultaneous sound voices before throttling.")]
        [Range(1, 16)] [SerializeField] private int _maxSimultaneousVoices = 8;
        [Tooltip("Minimum collision impulse required to trigger an impact sound.")]
        [SerializeField] private float _minCollisionImpulse = 0.5f;

        // Public Accessors
        public AudioClip BgmMainMenu { get => _bgmMainMenu; set => _bgmMainMenu = value; }
        public AudioClip BgmGameplay { get => _bgmGameplay; set => _bgmGameplay = value; }

        public AudioClip BallLaunch { get => _ballLaunch; set => _ballLaunch = value; }
        public AudioClip BoxImpact { get => _boxImpact; set => _boxImpact = value; }
        public AudioClip StoneImpact { get => _stoneImpact; set => _stoneImpact = value; }
        public AudioClip ColumnImpact { get => _columnImpact; set => _columnImpact = value; }
        public AudioClip ConeImpact { get => _coneImpact; set => _coneImpact = value; }
        public AudioClip PaintBottleBreak { get => _paintBottleBreak; set => _paintBottleBreak = value; }
        public AudioClip ObjectDestruction { get => _objectDestruction; set => _objectDestruction = value; }

        public AudioClip ButtonClick { get => _buttonClick; set => _buttonClick = value; }
        public AudioClip WinFanfare { get => _winFanfare; set => _winFanfare = value; }
        public AudioClip LoseSound { get => _loseSound; set => _loseSound = value; }
        public AudioClip RewardSound { get => _rewardSound; set => _rewardSound = value; }

        public float PitchRandomizationRange => _pitchRandomizationRange;
        public int MaxSimultaneousVoices => _maxSimultaneousVoices;
        public float MinCollisionImpulse => _minCollisionImpulse;
    }
}
