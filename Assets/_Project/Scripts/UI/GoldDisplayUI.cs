using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Economy;

namespace RoyalSmash.UI
{
    /// <summary>
    /// UI Widget displaying current player Gold with thousands formatting (e.g. 🪙 1,000)
    /// and tactile scale punch micro-animations on balance change.
    /// Follows Master Build Prompt Section 13.3 and Phase 10 specifications.
    /// </summary>
    public class GoldDisplayUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private RectTransform _containerCard;
        [SerializeField] private Button _addGoldButton;

        [Header("Animation Settings")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _punchDuration = 0.25f;

        private Coroutine _punchCoroutine;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            if (_containerCard == null)
            {
                _containerCard = GetComponent<RectTransform>();
            }

            if (_containerCard != null)
            {
                _originalScale = _containerCard.localScale;
            }

            if (_addGoldButton != null)
            {
                _addGoldButton.onClick.AddListener(HandleAddGoldClicked);
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
            if (_addGoldButton != null)
            {
                _addGoldButton.onClick.RemoveListener(HandleAddGoldClicked);
            }
        }

        public void BindReferences(TextMeshProUGUI goldText, RectTransform container, Button addBtn = null)
        {
            _goldText = goldText;
            _containerCard = container;
            _addGoldButton = addBtn;

            if (_containerCard != null)
            {
                _originalScale = _containerCard.localScale;
            }

            if (_addGoldButton != null)
            {
                _addGoldButton.onClick.RemoveAllListeners();
                _addGoldButton.onClick.AddListener(HandleAddGoldClicked);
            }

            RefreshDisplay();
        }

        private void SubscribeEvents()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= HandleGoldChanged;
                GoldManager.Instance.OnGoldChanged += HandleGoldChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GoldManager.HasInstance)
            {
                GoldManager.Instance.OnGoldChanged -= HandleGoldChanged;
            }
        }

        private void HandleGoldChanged(int newTotal, int delta)
        {
            UpdateText(newTotal);

            if (delta != 0 && gameObject.activeInHierarchy)
            {
                TriggerPunch();
            }
        }

        public void RefreshDisplay()
        {
            int gold = GoldManager.Instance != null ? GoldManager.Instance.CurrentGold : 1000;
            UpdateText(gold);
        }

        private void UpdateText(int amount)
        {
            if (_goldText != null)
            {
                _goldText.text = amount.ToString("N0");
            }
        }

        public void TriggerPunch()
        {
            if (_containerCard == null || !gameObject.activeInHierarchy) return;

            if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            float elapsed = 0f;
            Vector3 targetScale = _originalScale * _punchScale;

            // Scale up
            while (elapsed < _punchDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / (_punchDuration * 0.5f));
                _containerCard.localScale = Vector3.Lerp(_originalScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            // Scale back down
            elapsed = 0f;
            while (elapsed < _punchDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / (_punchDuration * 0.5f));
                _containerCard.localScale = Vector3.Lerp(targetScale, _originalScale, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            _containerCard.localScale = _originalScale;
            _punchCoroutine = null;
        }

        private void HandleAddGoldClicked()
        {
            Debug.Log("[GoldDisplayUI] Add Gold clicked -> Switching to Shop.");
            var bottomNav = FindFirstObjectByType<BottomNavigationUI>();
            if (bottomNav != null)
            {
                bottomNav.SwitchTab(NavigationTab.Shop);
            }
        }
    }
}
