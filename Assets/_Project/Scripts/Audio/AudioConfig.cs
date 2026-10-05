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
        [Header("Background Music Tracks - Active & Alternatives")]
        [SerializeField] private AudioClip _bgmMainMenu;
        [SerializeField] private AudioClip _bgmMainMenuAlt1;
        [SerializeField] private AudioClip _bgmMainMenuAlt2;
        [SerializeField] private AudioClip _bgmGameplay;
        [SerializeField] private AudioClip _bgmGameplayAlt1;
        [SerializeField] private AudioClip _bgmGameplayAlt2;

        [Header("Combat & Launch - Active & Alternatives")]
        [SerializeField] private AudioClip _ballLaunch;
        [SerializeField] private AudioClip _ballLaunchAlt1;
        [SerializeField] private AudioClip _ballLaunchAlt2;

        [SerializeField] private AudioClip _cannonAim;
        [SerializeField] private AudioClip _cannonAimAlt1;
        [SerializeField] private AudioClip _cannonAimAlt2;

        [SerializeField] private AudioClip _ballBounce;
        [SerializeField] private AudioClip _ballBounceAlt1;
        [SerializeField] private AudioClip _ballBounceAlt2;

        [Header("Skills - Active & Alternatives")]
        [SerializeField] private AudioClip _skillMegaBall;
        [SerializeField] private AudioClip _skillMegaBallAlt1;
        [SerializeField] private AudioClip _skillMegaBallAlt2;

        [SerializeField] private AudioClip _skillInfinityBall;
        [SerializeField] private AudioClip _skillInfinityBallAlt1;
        [SerializeField] private AudioClip _skillInfinityBallAlt2;

        [Header("Object Impacts - Active & Alternatives")]
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

        [Header("Object Destruction - Active & Alternatives")]
        [SerializeField] private AudioClip _boxDestroy;
        [SerializeField] private AudioClip _boxDestroyAlt1;
        [SerializeField] private AudioClip _boxDestroyAlt2;

        [SerializeField] private AudioClip _stoneDestroy;
        [SerializeField] private AudioClip _stoneDestroyAlt1;
        [SerializeField] private AudioClip _stoneDestroyAlt2;

        [SerializeField] private AudioClip _columnDestroy;
        [SerializeField] private AudioClip _columnDestroyAlt1;
        [SerializeField] private AudioClip _columnDestroyAlt2;

        [SerializeField] private AudioClip _coneDestroy;
        [SerializeField] private AudioClip _coneDestroyAlt1;
        [SerializeField] private AudioClip _coneDestroyAlt2;

        [SerializeField] private AudioClip _paintBottleBreak;
        [SerializeField] private AudioClip _paintBottleBreakAlt1;
        [SerializeField] private AudioClip _paintBottleBreakAlt2;

        [SerializeField] private AudioClip _objectDestruction;
        [SerializeField] private AudioClip _objectDestructionAlt1;
        [SerializeField] private AudioClip _objectDestructionAlt2;

        [SerializeField] private AudioClip _comboDestroy;
        [SerializeField] private AudioClip _comboDestroyAlt1;
        [SerializeField] private AudioClip _comboDestroyAlt2;

        [Header("UI & Menus - Active & Alternatives")]
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _buttonClickAlt1;
        [SerializeField] private AudioClip _buttonClickAlt2;

        [SerializeField] private AudioClip _panelOpen;
        [SerializeField] private AudioClip _panelOpenAlt1;
        [SerializeField] private AudioClip _panelOpenAlt2;

        [SerializeField] private AudioClip _panelClose;
        [SerializeField] private AudioClip _panelCloseAlt1;
        [SerializeField] private AudioClip _panelCloseAlt2;

        [SerializeField] private AudioClip _shopPurchase;
        [SerializeField] private AudioClip _shopPurchaseAlt1;
        [SerializeField] private AudioClip _shopPurchaseAlt2;

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

        [SerializeField] private AudioClip _starAward;
        [SerializeField] private AudioClip _starAwardAlt1;
        [SerializeField] private AudioClip _starAwardAlt2;

        [SerializeField] private AudioClip _nextLevelTransition;
        [SerializeField] private AudioClip _nextLevelTransitionAlt1;
        [SerializeField] private AudioClip _nextLevelTransitionAlt2;

        [Header("Dynamics & Anti-Clipping (Section 35)")]
        [Tooltip("Slight random pitch variance (+/- range) for natural sound variety.")]
        [Range(0f, 0.2f)] [SerializeField] private float _pitchRandomizationRange = 0.06f;
        [Tooltip("Maximum simultaneous sound voices before throttling.")]
        [Range(1, 16)] [SerializeField] private int _maxSimultaneousVoices = 12;
        [Tooltip("Minimum collision impulse required to trigger an impact sound.")]
        [SerializeField] private float _minCollisionImpulse = 0.5f;

        // Primary Accessors (Backwards Compatible)
        public AudioClip BgmMainMenu { get => _bgmMainMenu; set => _bgmMainMenu = value; }
        public AudioClip BgmGameplay { get => _bgmGameplay; set => _bgmGameplay = value; }

        public AudioClip BallLaunch { get => _ballLaunch; set => _ballLaunch = value; }
        public AudioClip CannonAim { get => _cannonAim; set => _cannonAim = value; }
        public AudioClip BallBounce { get => _ballBounce; set => _ballBounce = value; }

        public AudioClip SkillMegaBall { get => _skillMegaBall; set => _skillMegaBall = value; }
        public AudioClip SkillInfinityBall { get => _skillInfinityBall; set => _skillInfinityBall = value; }

        public AudioClip BoxImpact { get => _boxImpact; set => _boxImpact = value; }
        public AudioClip StoneImpact { get => _stoneImpact; set => _stoneImpact = value; }
        public AudioClip ColumnImpact { get => _columnImpact; set => _columnImpact = value; }
        public AudioClip ConeImpact { get => _coneImpact; set => _coneImpact = value; }

        public AudioClip BoxDestroy { get => _boxDestroy != null ? _boxDestroy : _objectDestruction; set => _boxDestroy = value; }
        public AudioClip StoneDestroy { get => _stoneDestroy != null ? _stoneDestroy : _objectDestruction; set => _stoneDestroy = value; }
        public AudioClip ColumnDestroy { get => _columnDestroy != null ? _columnDestroy : _objectDestruction; set => _columnDestroy = value; }
        public AudioClip ConeDestroy { get => _coneDestroy != null ? _coneDestroy : _objectDestruction; set => _coneDestroy = value; }

        public AudioClip PaintBottleBreak { get => _paintBottleBreak; set => _paintBottleBreak = value; }
        public AudioClip ObjectDestruction { get => _objectDestruction; set => _objectDestruction = value; }
        public AudioClip ComboDestroy { get => _comboDestroy != null ? _comboDestroy : _objectDestruction; set => _comboDestroy = value; }

        public AudioClip ButtonClick { get => _buttonClick; set => _buttonClick = value; }
        public AudioClip PanelOpen { get => _panelOpen; set => _panelOpen = value; }
        public AudioClip PanelClose { get => _panelClose != null ? _panelClose : _buttonClick; set => _panelClose = value; }
        public AudioClip ShopPurchase { get => _shopPurchase != null ? _shopPurchase : _rewardSound; set => _shopPurchase = value; }

        public AudioClip GameStart { get => _gameStart; set => _gameStart = value; }
        public AudioClip WinFanfare { get => _winFanfare; set => _winFanfare = value; }
        public AudioClip LoseSound { get => _loseSound; set => _loseSound = value; }
        public AudioClip RewardSound { get => _rewardSound; set => _rewardSound = value; }
        public AudioClip StarAward { get => _starAward != null ? _starAward : _rewardSound; set => _starAward = value; }
        public AudioClip NextLevelTransition { get => _nextLevelTransition != null ? _nextLevelTransition : _panelOpen; set => _nextLevelTransition = value; }

        // Alternative Accessors
        public AudioClip BgmMainMenuAlt1 { get => _bgmMainMenuAlt1; set => _bgmMainMenuAlt1 = value; }
        public AudioClip BgmMainMenuAlt2 { get => _bgmMainMenuAlt2; set => _bgmMainMenuAlt2 = value; }
        public AudioClip BgmGameplayAlt1 { get => _bgmGameplayAlt1; set => _bgmGameplayAlt1 = value; }
        public AudioClip BgmGameplayAlt2 { get => _bgmGameplayAlt2; set => _bgmGameplayAlt2 = value; }

        public AudioClip BallLaunchAlt1 { get => _ballLaunchAlt1; set => _ballLaunchAlt1 = value; }
        public AudioClip BallLaunchAlt2 { get => _ballLaunchAlt2; set => _ballLaunchAlt2 = value; }
        public AudioClip CannonAimAlt1 { get => _cannonAimAlt1; set => _cannonAimAlt1 = value; }
        public AudioClip CannonAimAlt2 { get => _cannonAimAlt2; set => _cannonAimAlt2 = value; }
        public AudioClip BallBounceAlt1 { get => _ballBounceAlt1; set => _ballBounceAlt1 = value; }
        public AudioClip BallBounceAlt2 { get => _ballBounceAlt2; set => _ballBounceAlt2 = value; }

        public AudioClip SkillMegaBallAlt1 { get => _skillMegaBallAlt1; set => _skillMegaBallAlt1 = value; }
        public AudioClip SkillMegaBallAlt2 { get => _skillMegaBallAlt2; set => _skillMegaBallAlt2 = value; }
        public AudioClip SkillInfinityBallAlt1 { get => _skillInfinityBallAlt1; set => _skillInfinityBallAlt1 = value; }
        public AudioClip SkillInfinityBallAlt2 { get => _skillInfinityBallAlt2; set => _skillInfinityBallAlt2 = value; }

        public AudioClip BoxImpactAlt1 { get => _boxImpactAlt1; set => _boxImpactAlt1 = value; }
        public AudioClip BoxImpactAlt2 { get => _boxImpactAlt2; set => _boxImpactAlt2 = value; }
        public AudioClip StoneImpactAlt1 { get => _stoneImpactAlt1; set => _stoneImpactAlt1 = value; }
        public AudioClip StoneImpactAlt2 { get => _stoneImpactAlt2; set => _stoneImpactAlt2 = value; }
        public AudioClip ColumnImpactAlt1 { get => _columnImpactAlt1; set => _columnImpactAlt1 = value; }
        public AudioClip ColumnImpactAlt2 { get => _columnImpactAlt2; set => _columnImpactAlt2 = value; }
        public AudioClip ConeImpactAlt1 { get => _coneImpactAlt1; set => _coneImpactAlt1 = value; }
        public AudioClip ConeImpactAlt2 { get => _coneImpactAlt2; set => _coneImpactAlt2 = value; }

        public AudioClip BoxDestroyAlt1 { get => _boxDestroyAlt1; set => _boxDestroyAlt1 = value; }
        public AudioClip BoxDestroyAlt2 { get => _boxDestroyAlt2; set => _boxDestroyAlt2 = value; }
        public AudioClip StoneDestroyAlt1 { get => _stoneDestroyAlt1; set => _stoneDestroyAlt1 = value; }
        public AudioClip StoneDestroyAlt2 { get => _stoneDestroyAlt2; set => _stoneDestroyAlt2 = value; }
        public AudioClip ColumnDestroyAlt1 { get => _columnDestroyAlt1; set => _columnDestroyAlt1 = value; }
        public AudioClip ColumnDestroyAlt2 { get => _columnDestroyAlt2; set => _columnDestroyAlt2 = value; }
        public AudioClip ConeDestroyAlt1 { get => _coneDestroyAlt1; set => _coneDestroyAlt1 = value; }
        public AudioClip ConeDestroyAlt2 { get => _coneDestroyAlt2; set => _coneDestroyAlt2 = value; }

        public AudioClip PaintBottleBreakAlt1 { get => _paintBottleBreakAlt1; set => _paintBottleBreakAlt1 = value; }
        public AudioClip PaintBottleBreakAlt2 { get => _paintBottleBreakAlt2; set => _paintBottleBreakAlt2 = value; }

        public AudioClip ObjectDestructionAlt1 { get => _objectDestructionAlt1; set => _objectDestructionAlt1 = value; }
        public AudioClip ObjectDestructionAlt2 { get => _objectDestructionAlt2; set => _objectDestructionAlt2 = value; }

        public AudioClip ComboDestroyAlt1 { get => _comboDestroyAlt1; set => _comboDestroyAlt1 = value; }
        public AudioClip ComboDestroyAlt2 { get => _comboDestroyAlt2; set => _comboDestroyAlt2 = value; }

        public AudioClip ButtonClickAlt1 { get => _buttonClickAlt1; set => _buttonClickAlt1 = value; }
        public AudioClip ButtonClickAlt2 { get => _buttonClickAlt2; set => _buttonClickAlt2 = value; }
        public AudioClip PanelOpenAlt1 { get => _panelOpenAlt1; set => _panelOpenAlt1 = value; }
        public AudioClip PanelOpenAlt2 { get => _panelOpenAlt2; set => _panelOpenAlt2 = value; }
        public AudioClip PanelCloseAlt1 { get => _panelCloseAlt1; set => _panelCloseAlt1 = value; }
        public AudioClip PanelCloseAlt2 { get => _panelCloseAlt2; set => _panelCloseAlt2 = value; }
        public AudioClip ShopPurchaseAlt1 { get => _shopPurchaseAlt1; set => _shopPurchaseAlt1 = value; }
        public AudioClip ShopPurchaseAlt2 { get => _shopPurchaseAlt2; set => _shopPurchaseAlt2 = value; }

        public AudioClip GameStartAlt1 { get => _gameStartAlt1; set => _gameStartAlt1 = value; }
        public AudioClip GameStartAlt2 { get => _gameStartAlt2; set => _gameStartAlt2 = value; }
        public AudioClip WinFanfareAlt1 { get => _winFanfareAlt1; set => _winFanfareAlt1 = value; }
        public AudioClip WinFanfareAlt2 { get => _winFanfareAlt2; set => _winFanfareAlt2 = value; }
        public AudioClip LoseSoundAlt1 { get => _loseSoundAlt1; set => _loseSoundAlt1 = value; }
        public AudioClip LoseSoundAlt2 { get => _loseSoundAlt2; set => _loseSoundAlt2 = value; }
        public AudioClip RewardSoundAlt1 { get => _rewardSoundAlt1; set => _rewardSoundAlt1 = value; }
        public AudioClip RewardSoundAlt2 { get => _rewardSoundAlt2; set => _rewardSoundAlt2 = value; }
        public AudioClip StarAwardAlt1 { get => _starAwardAlt1; set => _starAwardAlt1 = value; }
        public AudioClip StarAwardAlt2 { get => _starAwardAlt2; set => _starAwardAlt2 = value; }
        public AudioClip NextLevelTransitionAlt1 { get => _nextLevelTransitionAlt1; set => _nextLevelTransitionAlt1 = value; }
        public AudioClip NextLevelTransitionAlt2 { get => _nextLevelTransitionAlt2; set => _nextLevelTransitionAlt2 = value; }

        public float PitchRandomizationRange => _pitchRandomizationRange;
        public int MaxSimultaneousVoices => _maxSimultaneousVoices;
        public float MinCollisionImpulse => _minCollisionImpulse;
    }
}
