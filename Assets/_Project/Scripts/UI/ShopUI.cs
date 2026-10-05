using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Economy;

namespace RoyalSmash.UI
{
    /// <summary>Shop navigation, session countdown and existing IAP feedback.</summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ShopConfig _config;
        [SerializeField] private ShopPresentationConfig _presentation;
        [SerializeField] private ShopCatalogPresenter _catalogPresenter;

        [Header("Limited Offer Countdown")]
        [SerializeField] private TextMeshProUGUI _limitedOfferTimerText;
        [SerializeField] private float _countdownDurationSeconds = 86400f;

        [Header("Close Button")]
        [SerializeField] private Button _closeButton;

        [Header("Dialogs")]
        [SerializeField] private PurchaseFeedbackDialog _feedbackDialog;

        [Header("Item Cards")]
        [SerializeField] private List<ShopItemCardUI> _itemCards = new List<ShopItemCardUI>();
        [SerializeField] private float _timeRemaining = 86400f;

        private double _countdownDeadline;
        private double _nextRefreshAt;
        private IAPManager _subscribedIap;
        private ShopItemData _deferredSuccess;
        private string _deferredError;

        public static ShopUI Instance { get; private set; }
        public float TimeRemaining
        {
            get
            {
                if (Application.isPlaying) SynchronizeCountdown();
                return _timeRemaining > 0f ? _timeRemaining : CountdownDuration;
            }
            set
            {
                _timeRemaining = value > 0f ? value : CountdownDuration;
                _countdownDeadline = Time.realtimeSinceStartupAsDouble + _timeRemaining;
                RefreshTimerText();
            }
        }

        private float CountdownDuration => Mathf.Max(1f, _countdownDurationSeconds);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _timeRemaining = CountdownDuration;
            _countdownDeadline = Time.realtimeSinceStartupAsDouble + _timeRemaining;

            if (_closeButton == null)
            {
                Transform closeTransform = transform.Find("ShopFrameContainer/CloseButton");
                if (closeTransform != null) _closeButton = closeTransform.GetComponent<Button>();
            }
            if (_limitedOfferTimerText == null)
            {
                Transform timerTransform = transform.Find("ShopFrameContainer/ContentScrollView/Viewport/Content/Card_KingsDeal/TimerText");
                if (timerTransform != null) _limitedOfferTimerText = timerTransform.GetComponent<TextMeshProUGUI>();
            }
            if (_feedbackDialog == null)
                _feedbackDialog = FindFirstObjectByType<PurchaseFeedbackDialog>(FindObjectsInactive.Include);
            if (_catalogPresenter == null)
                _catalogPresenter = GetComponentInChildren<ShopCatalogPresenter>(true);
            if (_itemCards == null || _itemCards.Count == 0)
                _itemCards = new List<ShopItemCardUI>(GetComponentsInChildren<ShopItemCardUI>(true));
            BindCloseButton();
        }

        private void OnEnable()
        {
            SubscribeToStore();
            BindCloseButton();
            SynchronizeCountdown();
            RefreshAllCards();
            RefreshTimerText();
            ShowDeferredFeedback();
        }

        private void Start()
        {
            SubscribeToStore();
            RefreshAllCards();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartupAsDouble < _nextRefreshAt) return;
            _nextRefreshAt = Time.realtimeSinceStartupAsDouble + .25d;
            SubscribeToStore();
            SynchronizeCountdown();
            RefreshTimerText();
        }

        private void OnDisable()
        {
            SynchronizeCountdown();
            // Keep the one subscription while a purchase finishes on another page.
            // Feedback is deferred until reopening the shop instead of covering that page.
        }

        private void OnDestroy()
        {
            UnsubscribeFromStore();
            if (_closeButton != null) _closeButton.onClick.RemoveListener(HandleCloseClicked);
            if (Instance == this) Instance = null;
        }

        public void BindReferences(ShopConfig config, TextMeshProUGUI timerTxt,
            PurchaseFeedbackDialog feedbackDlg, List<ShopItemCardUI> cards)
        {
            _config = config;
            _limitedOfferTimerText = timerTxt;
            _feedbackDialog = feedbackDlg;
            _itemCards = cards ?? new List<ShopItemCardUI>();
            RefreshAllCards();
        }

        public void SetCatalogCards(ShopCatalogPresenter presenter, ShopPresentationConfig presentation,
            IReadOnlyList<ShopItemCardUI> cards, TextMeshProUGUI timer)
        {
            _catalogPresenter = presenter;
            _presentation = presentation;
            if (_itemCards == null) _itemCards = new List<ShopItemCardUI>();
            _itemCards.Clear();
            if (cards != null)
                for (int i = 0; i < cards.Count; i++) if (cards[i] != null) _itemCards.Add(cards[i]);
            _limitedOfferTimerText = timer;
            RefreshTimerText();
        }

        public void RefreshAllCards()
        {
            if (_catalogPresenter != null)
            {
                _catalogPresenter.RefreshCatalog();
                return;
            }
            if (_itemCards == null) return;
            foreach (ShopItemCardUI card in _itemCards)
                if (card != null) card.RefreshDisplay();
        }

        private void BindCloseButton()
        {
            if (_closeButton == null) return;
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
            _closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void HandleCloseClicked()
        {
            BottomNavigationUI navigation = FindFirstObjectByType<BottomNavigationUI>();
            if (navigation != null) navigation.SwitchTab(NavigationTab.Battle);
            else gameObject.SetActive(false);
        }

        private void SynchronizeCountdown()
        {
            if (!Application.isPlaying) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (_countdownDeadline <= 0d) _countdownDeadline = now + CountdownDuration;
            if (now >= _countdownDeadline)
                _countdownDeadline += (Math.Floor((now - _countdownDeadline) / CountdownDuration) + 1d) * CountdownDuration;
            _timeRemaining = (float)Math.Max(0d, _countdownDeadline - now);
        }

        private void RefreshTimerText()
        {
            if (_limitedOfferTimerText == null) return;
            TimeSpan remaining = TimeSpan.FromSeconds(Math.Ceiling(Math.Max(0f, _timeRemaining)));
            _limitedOfferTimerText.text = $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        private void SubscribeToStore()
        {
            if (!Application.isPlaying) return;
            IAPManager current = IAPManager.Instance;
            if (_subscribedIap == current) return;
            UnsubscribeFromStore();
            _subscribedIap = current;
            if (_subscribedIap == null) return;
            _subscribedIap.OnPurchaseCompleted += HandlePurchaseCompleted;
            _subscribedIap.OnPurchaseFailed += HandlePurchaseFailed;
        }

        private void UnsubscribeFromStore()
        {
            if (_subscribedIap != null)
            {
                _subscribedIap.OnPurchaseCompleted -= HandlePurchaseCompleted;
                _subscribedIap.OnPurchaseFailed -= HandlePurchaseFailed;
            }
            _subscribedIap = null;
        }

        private void HandlePurchaseCompleted(ShopItemData item)
        {
            if (item == null) return;
            if (!isActiveAndEnabled)
            {
                _deferredSuccess = item;
                _deferredError = null;
                return;
            }
            RefreshAllCards();
            ShowSuccess(item);
        }

        private void ShowSuccess(ShopItemData item)
        {
            if (_feedbackDialog == null) return;
            var summary = new StringBuilder();
            if (item.GoldReward > 0) summary.AppendLine($"+{item.GoldReward:N0} {(_presentation != null ? _presentation.GoldRewardLabel : "Altın")}");
            if (item.MegaBallReward > 0) summary.AppendLine($"+{item.MegaBallReward:N0} {(_presentation != null ? _presentation.MegaRewardLabel : "Mega Top")}");
            if (item.InfinityBallReward > 0) summary.AppendLine($"+{item.InfinityBallReward:N0} {(_presentation != null ? _presentation.InfinityRewardLabel : "Sonsuz Top")}");
            if (item.GrantsNoAds) summary.AppendLine(_presentation != null ? _presentation.NoAdsDescription : "Zorunlu reklamlar kaldırıldı!");
            ShopProductArtwork art = _presentation != null ? _presentation.FindArtwork(item.Id) : null;
            string title = ShopItemCardUI.GetDisplayTitle(item);
            _feedbackDialog.ShowSuccess(string.IsNullOrEmpty(title) ? "SATIN ALMA BAŞARILI" : title, summary.ToString().TrimEnd(),
                art != null && art.Artwork != null ? art.Artwork : item.Icon);
        }

        private void HandlePurchaseFailed(string errorMessage)
        {
            if (!isActiveAndEnabled)
            {
                _deferredError = errorMessage;
                _deferredSuccess = null;
                return;
            }
            if (_feedbackDialog != null) _feedbackDialog.ShowError(errorMessage);
        }

        private void ShowDeferredFeedback()
        {
            if (_deferredSuccess != null) ShowSuccess(_deferredSuccess);
            else if (_deferredError != null && _feedbackDialog != null) _feedbackDialog.ShowError(_deferredError);
            _deferredSuccess = null;
            _deferredError = null;
        }
    }
}
