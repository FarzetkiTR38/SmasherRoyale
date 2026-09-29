using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Economy;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Master controller for the Shop Screen in Main Menu.
    /// Coordinates item sections (Limited Offer, No Ads, Gold Packs, Skill Packs, Bundles),
    /// the 24-hour countdown timer, and transaction feedback dialogs.
    /// Follows Master Build Prompt Section 14 specifications.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ShopConfig _config;

        [Header("Limited Offer Countdown")]
        [SerializeField] private TextMeshProUGUI _limitedOfferTimerText;
        [SerializeField] private float _countdownDurationSeconds = 86400f; // 24 hours

        [Header("Close Button")]
        [SerializeField] private Button _closeButton;

        [Header("Dialogs")]
        [SerializeField] private PurchaseFeedbackDialog _feedbackDialog;

        [Header("Item Cards")]
        [SerializeField] private List<ShopItemCardUI> _itemCards = new List<ShopItemCardUI>();

        public static ShopUI Instance { get; private set; }

        [SerializeField] private float _timeRemaining = 86400f;
        public float TimeRemaining
        {
            get => _timeRemaining > 0f ? _timeRemaining : _countdownDurationSeconds;
            set => _timeRemaining = value;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _timeRemaining = _countdownDurationSeconds;

            if (_closeButton == null)
            {
                var closeTrans = transform.Find("ShopFrameContainer/CloseButton");
                if (closeTrans != null) _closeButton = closeTrans.GetComponent<Button>();
            }

            if (_limitedOfferTimerText == null)
            {
                var timerTrans = transform.Find("ShopFrameContainer/ContentScrollView/Viewport/Content/Card_KingsDeal/TimerText");
                if (timerTrans != null) _limitedOfferTimerText = timerTrans.GetComponent<TextMeshProUGUI>();
            }

            if (_feedbackDialog == null)
            {
                _feedbackDialog = FindFirstObjectByType<PurchaseFeedbackDialog>(FindObjectsInactive.Include);
            }

            if (_itemCards == null || _itemCards.Count == 0)
            {
                _itemCards = new List<ShopItemCardUI>(GetComponentsInChildren<ShopItemCardUI>(true));
            }
        }

        private void Start()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseCompleted += HandlePurchaseCompleted;
                IAPManager.Instance.OnPurchaseFailed += HandlePurchaseFailed;
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(HandleCloseClicked);
                _closeButton.onClick.AddListener(HandleCloseClicked);
            }

            StartCoroutine(CountdownTimerRoutine());
            RefreshAllCards();
        }

        private void OnDestroy()
        {
            if (IAPManager.HasInstance)
            {
                IAPManager.Instance.OnPurchaseCompleted -= HandlePurchaseCompleted;
                IAPManager.Instance.OnPurchaseFailed -= HandlePurchaseFailed;
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
            if (Instance == this) Instance = null;
        }

        private void HandleCloseClicked()
        {
            var bottomNav = FindObjectOfType<BottomNavigationUI>();
            if (bottomNav != null)
            {
                bottomNav.SwitchTab(NavigationTab.Battle);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void BindReferences(
            ShopConfig config,
            TextMeshProUGUI timerTxt,
            PurchaseFeedbackDialog feedbackDlg,
            List<ShopItemCardUI> cards)
        {
            _config = config;
            _limitedOfferTimerText = timerTxt;
            _feedbackDialog = feedbackDlg;
            _itemCards = cards ?? new List<ShopItemCardUI>();

            RefreshAllCards();
        }

        public void RefreshAllCards()
        {
            if (_itemCards != null)
            {
                foreach (var card in _itemCards)
                {
                    if (card != null) card.RefreshDisplay();
                }
            }
        }

        private IEnumerator CountdownTimerRoutine()
        {
            while (true)
            {
                _timeRemaining -= Time.unscaledDeltaTime;
                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = _countdownDurationSeconds; // Reset 24h cycle
                }

                if (_limitedOfferTimerText != null)
                {
                    TimeSpan t = TimeSpan.FromSeconds(Mathf.Max(0f, _timeRemaining));
                    _limitedOfferTimerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
                }

                yield return null;
            }
        }

        private void HandlePurchaseCompleted(ShopItemData item)
        {
            RefreshAllCards();

            if (_feedbackDialog != null)
            {
                string summary = "";
                if (item.GoldReward > 0) summary += $"+{item.GoldReward:N0} Gold\n";
                if (item.MegaBallReward > 0) summary += $"+{item.MegaBallReward} Mega Balls\n";
                if (item.InfinityBallReward > 0) summary += $"+{item.InfinityBallReward} Infinity Balls\n";
                if (item.GrantsNoAds) summary += "Forced ads removed permanently!\n";

                _feedbackDialog.ShowSuccess($"YOU GOT {item.Title.ToUpper()}!", summary.TrimEnd(), item.Icon);
            }
        }

        private void HandlePurchaseFailed(string errorMessage)
        {
            if (_feedbackDialog != null)
            {
                _feedbackDialog.ShowError(errorMessage);
            }
        }
    }
}
