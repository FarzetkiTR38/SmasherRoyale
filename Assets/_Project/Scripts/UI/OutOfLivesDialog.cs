using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Lives;
using RoyalSmash.Save;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Modal dialog displayed when player runs out of lives or clicks '+' on the lives widget.
    /// Offers options to purchase +1 life with Gold or watch a Rewarded Ad.
    /// Follows Master Build Prompt Section 11 & Section 1.3.
    /// </summary>
    public class OutOfLivesDialog : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TextMeshProUGUI _goldCostText;
        [SerializeField] private Button _buyWithGoldButton;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _closeButton;

        public event Action OnLifeAcquired;

        private void Awake()
        {
            if (_buyWithGoldButton != null) _buyWithGoldButton.onClick.AddListener(HandleBuyWithGold);
            if (_watchAdButton != null) _watchAdButton.onClick.AddListener(HandleWatchAd);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        public void BindReferences(GameObject root, TextMeshProUGUI costText, Button buyBtn, Button adBtn, Button closeBtn)
        {
            _contentRoot = root;
            _goldCostText = costText;
            _buyWithGoldButton = buyBtn;
            _watchAdButton = adBtn;
            _closeButton = closeBtn;

            if (_buyWithGoldButton != null)
            {
                _buyWithGoldButton.onClick.RemoveAllListeners();
                _buyWithGoldButton.onClick.AddListener(HandleBuyWithGold);
            }
            if (_watchAdButton != null)
            {
                _watchAdButton.onClick.RemoveAllListeners();
                _watchAdButton.onClick.AddListener(HandleWatchAd);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show()
        {
            if (_contentRoot != null) _contentRoot.SetActive(true);
            else gameObject.SetActive(true);

            UpdateCostDisplay();
        }

        public void Hide()
        {
            if (_contentRoot != null) _contentRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void UpdateCostDisplay()
        {
            if (_goldCostText != null && LivesManager.Instance != null && LivesManager.Instance.Config != null)
            {
                _goldCostText.text = $"{LivesManager.Instance.Config.LifePriceGold} Altın";
            }
        }

        private void HandleBuyWithGold()
        {
            if (LivesManager.Instance != null)
            {
                bool success = LivesManager.Instance.BuyLifeWithGold();
                if (success)
                {
                    Debug.Log("[OutOfLivesDialog] Successfully purchased +1 life with Gold!");
                    OnLifeAcquired?.Invoke();
                    Hide();
                }
                else
                {
                    Debug.LogWarning("[OutOfLivesDialog] Gold purchase failed: insufficient Gold or lives full.");
                }
            }
        }

        private void HandleWatchAd()
        {
            if (RoyalSmash.Ads.AdManager.Instance != null)
            {
                RoyalSmash.Ads.AdManager.Instance.ShowRewarded(
                    "out_of_lives",
                    onRewardGranted: () =>
                    {
                        if (LivesManager.Instance != null)
                        {
                            LivesManager.Instance.GrantRewardedAdLife();
                            Debug.Log("[OutOfLivesDialog] Rewarded ad completed! Granted +1 life.");
                            OnLifeAcquired?.Invoke();
                        }
                    },
                    onClosed: () =>
                    {
                        Hide();
                    },
                    onFailed: (err) =>
                    {
                        Debug.LogWarning($"[OutOfLivesDialog] Ad failed: {err}");
                    }
                );
            }
            else
            {
                if (LivesManager.Instance != null)
                {
                    LivesManager.Instance.GrantRewardedAdLife();
                    OnLifeAcquired?.Invoke();
                }
                Hide();
            }
        }
    }
}

