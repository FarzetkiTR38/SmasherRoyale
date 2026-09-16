using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Skills;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Interactive HUD button representing an active player skill (MegaBall or InfinityBall).
    /// Displays lock status, remaining uses badge, active timer countdown, and handles user tap activation.
    /// Follows Mobile Casual UI best practices with TextMeshPro and responsive feedback.
    /// </summary>
    public class SkillButtonUI : MonoBehaviour
    {
        [Header("Skill Binding")]
        [SerializeField] private SkillType _skillType;

        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _lockBadge;
        [SerializeField] private GameObject _activeGlow;
        [SerializeField] private TextMeshProUGUI _usesText;
        [SerializeField] private TextMeshProUGUI _timerText;

        public SkillType BoundSkill => _skillType;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void Start()
        {
            SubscribeEvents();
            RefreshDisplay();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        private void SubscribeEvents()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.OnSkillStateChanged -= HandleSkillStateChanged;
                SkillManager.Instance.OnSkillStateChanged += HandleSkillStateChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.OnSkillStateChanged -= HandleSkillStateChanged;
            }
        }

        private void HandleSkillStateChanged(SkillType skill, bool isUnlocked, int remainingUses, bool isActive, float durationRemaining)
        {
            if (skill == _skillType)
            {
                UpdateView(isUnlocked, remainingUses, isActive, durationRemaining);
            }
        }

        public void RefreshDisplay()
        {
            if (SkillManager.Instance != null)
            {
                bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(_skillType);
                int uses = SkillManager.Instance.GetRemainingUses(_skillType);
                bool isActive = SkillManager.Instance.IsSkillActive(_skillType);
                float timer = SkillManager.Instance.GetActiveTimer(_skillType);

                UpdateView(isUnlocked, uses, isActive, timer);
            }
        }

        private void UpdateView(bool isUnlocked, int remainingUses, bool isActive, float durationRemaining)
        {
            // 1. Lock State
            if (_lockBadge != null)
            {
                _lockBadge.SetActive(!isUnlocked);
            }

            // 2. Active Glow / Visual Feedback
            if (_activeGlow != null)
            {
                _activeGlow.SetActive(isActive);
            }

            // 3. Uses Badge
            if (_usesText != null)
            {
                if (!isUnlocked)
                {
                    _usesText.text = "";
                }
                else if (remainingUses > 0)
                {
                    _usesText.text = remainingUses.ToString();
                }
                else
                {
                    _usesText.text = "0";
                }
            }

            // 4. Timer Text (for timer-based skills like InfinityBall)
            if (_timerText != null)
            {
                if (isActive && durationRemaining > 0f)
                {
                    _timerText.gameObject.SetActive(true);
                    _timerText.text = $"{durationRemaining:F1}s";
                }
                else
                {
                    _timerText.gameObject.SetActive(false);
                }
            }

            // 5. Button Interactivity
            if (_button != null)
            {
                _button.interactable = isUnlocked && remainingUses > 0 && !isActive;
            }
        }

        private void HandleButtonClicked()
        {
            if (SkillManager.Instance != null)
            {
                bool success = SkillManager.Instance.ActivateSkill(_skillType);
                if (success)
                {
                    StartCoroutine(PunchAnimation());
                }
            }
        }

        private IEnumerator PunchAnimation()
        {
            transform.localScale = Vector3.one * 1.15f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, elapsed / 0.2f);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }
    }
}
