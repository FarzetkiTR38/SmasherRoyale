using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RoyalSmash.Skills;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Interactive HUD button representing an active player skill (MegaBall or InfinityBall).
    /// Displays lock status, remaining uses badge, active timer countdown, and handles user tap activation.
    /// Implements IPointerClickHandler, IPointerDownHandler, IPointerUpHandler for guaranteed responsiveness.
    /// Follows Mobile Casual UI best practices with TextMeshPro and responsive tactile feedback.
    /// </summary>
    public class SkillButtonUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
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

        private float _lastClickTime = -1f;
        private Coroutine _scaleRoutine;

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
            transform.localScale = Vector3.one;
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

        public bool IsInteractable()
        {
            if (SkillManager.Instance == null) return false;
            return SkillManager.Instance.IsSkillUnlocked(_skillType) &&
                   SkillManager.Instance.GetRemainingUses(_skillType) > 0 &&
                   !SkillManager.Instance.IsSkillActive(_skillType);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            AnimateScale(Vector3.one * 0.90f, 0.08f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateScale(Vector3.one, 0.08f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            HandleButtonClicked();
        }

        private void HandleButtonClicked()
        {
            // Debounce guard to prevent double activation within the same frame or rapid succession
            if (Time.unscaledTime - _lastClickTime < 0.15f) return;
            _lastClickTime = Time.unscaledTime;

            Debug.Log($"[SkillButtonUI] HandleButtonClicked CALLED for {_skillType}!");
            if (SkillManager.Instance != null)
            {
                bool success = SkillManager.Instance.ActivateSkill(_skillType);
                Debug.Log($"[SkillButtonUI] ActivateSkill({_skillType}) returned {success}");
                if (success)
                {
                    StartCoroutine(PunchAnimation());
                }
            }
            else
            {
                Debug.LogError($"[SkillButtonUI] SkillManager.Instance is NULL when clicking {_skillType}!");
            }
        }

        private void AnimateScale(Vector3 targetScale, float duration)
        {
            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            _scaleRoutine = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
                yield return null;
            }
            transform.localScale = targetScale;
            _scaleRoutine = null;
        }

        private IEnumerator PunchAnimation()
        {
            if (_scaleRoutine != null)
            {
                StopCoroutine(_scaleRoutine);
                _scaleRoutine = null;
            }

            transform.localScale = Vector3.one * 1.20f;
            float elapsed = 0f;
            float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one * 1.20f, Vector3.one, elapsed / duration);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        [ContextMenu("Test Click")]
        public void TestClick()
        {
            HandleButtonClicked();
        }
    }
}
