using UnityEngine;

namespace RoyalSmash.Tutorial
{
    /// <summary>
    /// Configuration asset for First Launch Tutorial.
    /// Exposes all visual and interaction tuning parameters without magic numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "RoyalSmash/Config/TutorialConfig")]
    public class TutorialConfig : ScriptableObject
    {
        [Header("Targeting & Validation")]
        [Tooltip("Acceptable tap radius in screen pixels around the designated target center.")]
        [SerializeField] private float _targetRadiusPixels = 160f;

        [Header("Visual Feedback & Animation")]
        [SerializeField] private float _ringPulseSpeed = 2.5f;
        [SerializeField] private float _fingerBobSpeed = 3.0f;
        [SerializeField] private float _fingerBobDistance = 25f;
        [SerializeField] private float _fadeDuration = 0.35f;

        [Header("Localization / Text")]
        [SerializeField] private string _instructionText = "HEDEFE DOKUN VE FIRLAT!";
        [SerializeField] private string _invalidTapHint = "LÜTFEN İŞARETLİ ALANA DOKUNUN!";

        public float TargetRadiusPixels => _targetRadiusPixels;
        public float RingPulseSpeed => _ringPulseSpeed;
        public float FingerBobSpeed => _fingerBobSpeed;
        public float FingerBobDistance => _fingerBobDistance;
        public float FadeDuration => _fadeDuration;
        public string InstructionText => _instructionText;
        public string InvalidTapHint => _invalidTapHint;
    }
}
