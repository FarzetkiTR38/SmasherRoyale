using UnityEngine;

namespace RoyalSmash.Audio
{
    /// <summary>
    /// Configuration asset containing all SFX clips, BGM tracks, and acoustic parameters.
    /// Exposes primary (Option 1) and alternative (Options 2 & 3) audio clips for full customization.
    /// Follows Master Build Prompt Section 21 & Section 35.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "RoyalSmash/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Background Music Tracks - Active (Option 1)")]
        [SerializeField] private AudioClip _bgmMainMenu;
        [SerializeField] private AudioClip _bgmGameplay;

        [Header("Background Music Tracks - Alternatives (Options 2 & 3)")]
        [SerializeField] private AudioClip _bgmMainMenuAlt1;
        [SerializeField] private AudioClip _bgmMainMenuAlt2;
        [SerializeField] private AudioClip _bgmGameplayAlt1;
        [SerializeField] private AudioClip _bgmGameplayAlt2;

        [Header("Combat & Launch - Active & Alternatives")]
        [SerializeField] private AudioClip _ballLaunch;
        [SerializeField] private AudioClip _ballLaunchAlt1;
        [SerializeField] private AudioClip _ballLaunchAlt2;

        [Header("Object-Specific Impacts - Active & Alternatives")]
        [SerializeField] private AudioClip _boxImpact;
        [SerializeField] private AudioClip _boxImpactAlt1;
        [SerializeField] private AudioClip _boxImpactAlt2;

        [SerializeField] private AudioClip _stoneImpact;
        [SerializeField] private AudioClip _stoneImpactAlt1;
        [SerializeField] private AudioClip _stoneImpactAlt2;

        [SerializeField] private AudioClip _columnImpact;
        [SerializeField] private AudioClip _columnImpactAlt1;
        [SerializeField] private AudioClip _columnImpactAlt2;

        [SerializeField] private AudioClip _coneImpact;
        [SerializeField] private AudioClip _coneImpactAlt1;
        [SerializeField] private AudioClip _coneImpactAlt2;

        [SerializeField] private AudioClip _paintBottleBreak;
        [SerializeField] private AudioClip _paintBottleBreakAlt1;
        [SerializeField] private AudioClip _paintBottleBreakAlt2;

        [SerializeField] private AudioClip _objectDestruction;
        [SerializeField] private AudioClip _objectDestructionAlt1;
        [SerializeField] private AudioClip _objectDestructionAlt2;

        [Header("UI & Menus - Active & Alternatives")]
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _buttonClickAlt1;
        [SerializeField] private AudioClip _buttonClickAlt2;

        [SerializeField] private AudioClip _panelOpen;
        [SerializeField] private AudioClip _panelOpenAlt1;
        [SerializeField] private AudioClip _panelOpenAlt2;

        [Header("Game States - Active & Alternatives")]
        [SerializeField] private AudioClip _gameStart;
        [SerializeField] private AudioClip _gameStartAlt1;
        [SerializeField] private AudioClip _gameStartAlt2;

        [SerializeField] private AudioClip _winFanfare;
        [SerializeField] private AudioClip _winFanfareAlt1;
        [SerializeField] private AudioClip _winFanfareAlt2;

        [SerializeField] private AudioClip _loseSound;
        [SerializeField] private AudioClip _loseSoundAlt1;
        [SerializeField] private AudioClip _loseSoundAlt2;

        [SerializeField] private AudioClip _rewardSound;
        [SerializeField] private AudioClip _rewardSoundAlt1;
        [SerializeField] private AudioClip _rewardSoundAlt2;

        [Header("Dynamics & Anti-Clipping (Section 35)")]
        [Tooltip("Slight random pitch variance (+/- range) for natural sound variety.")]
        [Range(0f, 0.2f)] [SerializeField] private float _pitchRandomizationRange = 0.06f;
        [Tooltip("Maximum simultaneous sound voices before throttling.")]
        [Range(1, 16)] [SerializeField] private int _maxSimultaneousVoices = 8;
        [Tooltip("Minimum collision impulse required to trigger an impact sound.")]
        [SerializeField] private float _minCollisionImpulse = 0.5f;

        // Primary Accessors (Backwards Compatible)
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
        public AudioClip PanelOpen { get => _panelOpen; set => _panelOpen = value; }
        public AudioClip GameStart { get => _gameStart; set => _gameStart = value; }
        public AudioClip WinFanfare { get => _winFanfare; set => _winFanfare = value; }
        public AudioClip LoseSound { get => _loseSound; set => _loseSound = value; }
        public AudioClip RewardSound { get => _rewardSound; set => _rewardSound = value; }

        // Alternative Accessors
        public AudioClip BgmMainMenuAlt1 { get => _bgmMainMenuAlt1; set => _bgmMainMenuAlt1 = value; }
        public AudioClip BgmMainMenuAlt2 { get => _bgmMainMenuAlt2; set => _bgmMainMenuAlt2 = value; }
        public AudioClip BgmGameplayAlt1 { get => _bgmGameplayAlt1; set => _bgmGameplayAlt1 = value; }
        public AudioClip BgmGameplayAlt2 { get => _bgmGameplayAlt2; set => _bgmGameplayAlt2 = value; }

        public AudioClip BallLaunchAlt1 { get => _ballLaunchAlt1; set => _ballLaunchAlt1 = value; }
        public AudioClip BallLaunchAlt2 { get => _ballLaunchAlt2; set => _ballLaunchAlt2 = value; }

        public AudioClip BoxImpactAlt1 { get => _boxImpactAlt1; set => _boxImpactAlt1 = value; }
        public AudioClip BoxImpactAlt2 { get => _boxImpactAlt2; set => _boxImpactAlt2 = value; }
        public AudioClip StoneImpactAlt1 { get => _stoneImpactAlt1; set => _stoneImpactAlt1 = value; }
        public AudioClip StoneImpactAlt2 { get => _stoneImpactAlt2; set => _stoneImpactAlt2 = value; }
        public AudioClip ColumnImpactAlt1 { get => _columnImpactAlt1; set => _columnImpactAlt1 = value; }
        public AudioClip ColumnImpactAlt2 { get => _columnImpactAlt2; set => _columnImpactAlt2 = value; }
        public AudioClip ConeImpactAlt1 { get => _coneImpactAlt1; set => _coneImpactAlt1 = value; }
        public AudioClip ConeImpactAlt2 { get => _coneImpactAlt2; set => _coneImpactAlt2 = value; }

        public AudioClip PaintBottleBreakAlt1 { get => _paintBottleBreakAlt1; set => _paintBottleBreakAlt1 = value; }
        public AudioClip PaintBottleBreakAlt2 { get => _paintBottleBreakAlt2; set => _paintBottleBreakAlt2 = value; }

        public AudioClip ObjectDestructionAlt1 { get => _objectDestructionAlt1; set => _objectDestructionAlt1 = value; }
        public AudioClip ObjectDestructionAlt2 { get => _objectDestructionAlt2; set => _objectDestructionAlt2 = value; }

        public AudioClip ButtonClickAlt1 { get => _buttonClickAlt1; set => _buttonClickAlt1 = value; }
        public AudioClip ButtonClickAlt2 { get => _buttonClickAlt2; set => _buttonClickAlt2 = value; }
        public AudioClip PanelOpenAlt1 { get => _panelOpenAlt1; set => _panelOpenAlt1 = value; }
        public AudioClip PanelOpenAlt2 { get => _panelOpenAlt2; set => _panelOpenAlt2 = value; }

        public AudioClip GameStartAlt1 { get => _gameStartAlt1; set => _gameStartAlt1 = value; }
        public AudioClip GameStartAlt2 { get => _gameStartAlt2; set => _gameStartAlt2 = value; }
        public AudioClip WinFanfareAlt1 { get => _winFanfareAlt1; set => _winFanfareAlt1 = value; }
        public AudioClip WinFanfareAlt2 { get => _winFanfareAlt2; set => _winFanfareAlt2 = value; }
        public AudioClip LoseSoundAlt1 { get => _loseSoundAlt1; set => _loseSoundAlt1 = value; }
        public AudioClip LoseSoundAlt2 { get => _loseSoundAlt2; set => _loseSoundAlt2 = value; }
        public AudioClip RewardSoundAlt1 { get => _rewardSoundAlt1; set => _rewardSoundAlt1 = value; }
        public AudioClip RewardSoundAlt2 { get => _rewardSoundAlt2; set => _rewardSoundAlt2 = value; }

        public float PitchRandomizationRange => _pitchRandomizationRange;
        public int MaxSimultaneousVoices => _maxSimultaneousVoices;
        public float MinCollisionImpulse => _minCollisionImpulse;
    }
}
