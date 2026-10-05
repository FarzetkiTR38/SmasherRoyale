using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Levels;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Displays victory popup when a level is cleared.
    /// Shows Level Completed title, Base Gold, Remaining Ball Bonus (N x 2), Total Gold, and Continue button.
    /// </summary>
    public class WinPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private TextMeshProUGUI _baseRewardText;
        [SerializeField] private TextMeshProUGUI _remainingBonusText;
        [SerializeField] private TextMeshProUGUI _totalGoldText;
        [SerializeField] private Button _continueButton;

        [Header("Animation Settings")]
        [SerializeField] private float _popDuration = 0.35f;

        public bool IsVisible => _panelRoot != null ? _panelRoot.activeSelf : gameObject.activeSelf;

        private void Awake()
        {
            if (_panelRoot == null || _panelRoot == gameObject)
            {
                var content = transform.Find("Content");
                if (content != null)
                {
                    _panelRoot = content.gameObject;
                }
                else
                {
                    var card = transform.Find("CardContainer");
                    if (card != null) _panelRoot = card.gameObject;
                }
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(HandleContinueClicked);
            }

            HideImmediate();
        }

        public void BindReferences(GameObject root, TextMeshProUGUI title, TextMeshProUGUI baseReward, TextMeshProUGUI bonus, TextMeshProUGUI totalGold, Button continueBtn)
        {
            _panelRoot = root;
            _levelTitleText = title;
            _baseRewardText = baseReward;
            _remainingBonusText = bonus;
            _totalGoldText = totalGold;
            _continueButton = continueBtn;
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        private void SubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
                LevelManager.Instance.OnLevelWon += HandleLevelWon;
            }

            if (Economy.GoldManager.Instance != null)
            {
                Economy.GoldManager.Instance.OnLevelRewardProcessed -= HandleRewardProcessed;
                Economy.GoldManager.Instance.OnLevelRewardProcessed += HandleRewardProcessed;
            }
        }

        private void UnsubscribeEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelWon -= HandleLevelWon;
            }

            if (Economy.GoldManager.HasInstance)
            {
                Economy.GoldManager.Instance.OnLevelRewardProcessed -= HandleRewardProcessed;
            }
        }

        private void HandleLevelWon(int level, int baseReward, int remainingBalls, int totalGold)
        {
            if (_panelRoot == null || !_panelRoot.activeSelf)
            {
                bool isDuplicate = (totalGold == 0);
                Show(level, baseReward, remainingBalls, totalGold, isDuplicate);
            }
        }

        private void HandleRewardProcessed(int level, int baseReward, int ballBonus, int totalGold, bool wasGranted)
        {
            int remainingBalls = ballBonus / 2;
            Show(level, baseReward, remainingBalls, totalGold, !wasGranted);
        }

        /// <summary>
        /// Displays the Win Panel populated with reward calculation data.
        /// </summary>
        public void Show(int level, int baseReward, int remainingBalls, int totalGold, bool isDuplicateReplay = false)
        {
            if (_panelRoot == null) return;

            if (_levelTitleText != null)
            {
                _levelTitleText.text = $"<color=#E0F4FF>SEVİYE {level}</color>\n<color=#FFDF20><size=115%>TAMAMLANDI!</size></color>";
            }

            if (isDuplicateReplay || totalGold == 0)
            {
                if (_baseRewardText != null) _baseRewardText.text = "Temel Ödül: <b><color=#FFD700>+0 Altın</color></b> (Tekrar)";
                if (_remainingBonusText != null) _remainingBonusText.text = $"Top Bonusu ({remainingBalls} x 2): <b><color=#FFD700>+0 Altın</color></b>";
                if (_totalGoldText != null) _totalGoldText.text = "<b><color=#FFE224>+0 ALTIN</color></b>\n<size=58%><color=#EDF4FA>(ALINDI)</color></size>";
            }
            else
            {
                if (_baseRewardText != null)
                {
                    _baseRewardText.text = $"Temel Ödül: <b><color=#FFD700>+{baseReward} Altın</color></b>";
                }

                if (_remainingBonusText != null)
                {
                    int ballBonus = remainingBalls * 2;
                    _remainingBonusText.text = $"Top Bonusu ({remainingBalls} x 2): <b><color=#FFD700>+{ballBonus} Altın</color></b>";
                }

                if (_totalGoldText != null)
                {
                    _totalGoldText.text = $"<b><color=#FFE224>+{totalGold} ALTIN</color></b>";
                }
            }

            if (_continueButton != null)
            {
                var btnText = _continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (level >= 30)
                {
                    if (btnText != null) btnText.text = "<b>ANA MENÜYE DÖN</b>";
                }
                else
                {
                    if (btnText != null) btnText.text = "<b>SONRAKİ SEVİYE >></b>";
                }
            }

            _panelRoot.SetActive(true);

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayWinSound();
            }

            if (RoyalSmash.VFX.VFXManager.Instance != null)
            {
                RoyalSmash.VFX.VFXManager.Instance.SpawnWinCelebration(transform.position);
            }

            if (RoyalSmash.Haptics.HapticManager.Instance != null)
            {
                RoyalSmash.Haptics.HapticManager.Instance.TriggerWinHaptic();
            }

            StopAllCoroutines();
            StartCoroutine(PopInRoutine());
            if (!isDuplicateReplay && totalGold > 0)
            {
                StartCoroutine(CountUpGoldRoutine(totalGold));
            }
        }

        public void HideImmediate()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private IEnumerator PopInRoutine()
        {
            if (_panelRoot != null)
            {
                _panelRoot.transform.localScale = Vector3.one;
            }

            Transform cardTarget = transform.Find("CardContainer") ?? transform.Find("Content/CardContainer") ?? transform.Find("Panel_Win");
            if (cardTarget == null && _panelRoot != null)
            {
                cardTarget = _panelRoot.transform.Find("CardContainer") ?? _panelRoot.transform.Find("Panel_Win");
            }
            if (cardTarget == null)
            {
                cardTarget = _panelRoot != null ? _panelRoot.transform : transform;
            }

            cardTarget.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _popDuration);
                // Spring overshoot: quick rise to 1.12, gentle settle to 1.0
                float scale = (t < 0.7f)
                    ? Mathf.Lerp(0f, 1.12f, t / 0.7f)
                    : Mathf.Lerp(1.12f, 1.0f, (t - 0.7f) / 0.3f);
                cardTarget.localScale = Vector3.one * scale;
                yield return null;
            }

            cardTarget.localScale = Vector3.one;
        }

        private IEnumerator CountUpGoldRoutine(int targetGold)
        {
            if (_totalGoldText == null) yield break;

            Vector3 originalScale = _totalGoldText.transform.localScale;
            float duration = 0.65f;
            float elapsed = 0f;
            int lastSoundStep = -1;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easeOut = 1f - Mathf.Pow(1f - progress, 3);
                int currentGold = Mathf.RoundToInt(Mathf.Lerp(0, targetGold, easeOut));

                _totalGoldText.text = $"<b><color=#FFE224>+{currentGold} ALTIN</color></b>";

                int soundStep = currentGold / 5;
                if (soundStep != lastSoundStep && RoyalSmash.Audio.AudioManager.Instance != null)
                {
                    lastSoundStep = soundStep;
                    RoyalSmash.Audio.AudioManager.Instance.PlayStarAward();
                }

                yield return null;
            }

            _totalGoldText.text = $"<b><color=#FFE224>+{targetGold} ALTIN</color></b>";

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayRewardSound();
            }

            // Golden sparkle punch pulse
            float punchElapsed = 0f;
            const float punchDuration = 0.25f;
            while (punchElapsed < punchDuration)
            {
                punchElapsed += Time.unscaledDeltaTime;
                float pt = punchElapsed / punchDuration;
                float scale = 1f + Mathf.Sin(pt * Mathf.PI) * 0.18f;
                _totalGoldText.transform.localScale = originalScale * scale;
                yield return null;
            }
            _totalGoldText.transform.localScale = originalScale;
        }

        private void HandleContinueClicked()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayNextLevelTransition();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.NextLevel();
            }
            HideImmediate();
        }
    }
}
