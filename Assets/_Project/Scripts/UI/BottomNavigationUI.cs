using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.UI
{
    public enum NavigationTab
    {
        Shop = 0,
        Battle = 1,
        Leaderboard = 2
    }

    /// <summary>
    /// Manages the 3 bottom navigation tabs (Shop, Battle, Leaderboard) in the Main Menu.
    /// Handles tab selection, active tab highlighting, and page view switching.
    /// Follows Master Build Prompt Section 17.2.
    /// </summary>
    public class BottomNavigationUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _shopTabButton;
        [SerializeField] private Button _battleTabButton;
        [SerializeField] private Button _leaderboardTabButton;

        [Header("Tab Visual Elements")]
        [SerializeField] private TextMeshProUGUI _shopTabText;
        [SerializeField] private TextMeshProUGUI _battleTabText;
        [SerializeField] private TextMeshProUGUI _leaderboardTabText;
        [SerializeField] private Image _shopTabIcon;
        [SerializeField] private Image _battleTabIcon;
        [SerializeField] private Image _leaderboardTabIcon;

        [Header("Page Containers")]
        [SerializeField] private GameObject _shopPage;
        [SerializeField] private GameObject _battlePage;
        [SerializeField] private GameObject _leaderboardPage;

        [Header("Styling")]
        [SerializeField] private Color _activeColor = new Color(1.0f, 0.85f, 0.25f, 1.0f); // Gold
        [SerializeField] private Color _inactiveColor = new Color(0.65f, 0.70f, 0.78f, 1.0f); // Muted slate

        [Header("Tab Sprites (Active / Inactive)")]
        [SerializeField] private Sprite _shopActiveSprite;
        [SerializeField] private Sprite _shopInactiveSprite;
        [SerializeField] private Sprite _battleActiveSprite;
        [SerializeField] private Sprite _battleInactiveSprite;
        [SerializeField] private Sprite _leaderboardActiveSprite;
        [SerializeField] private Sprite _leaderboardInactiveSprite;

        private NavigationTab _currentTab = NavigationTab.Battle;

        public NavigationTab CurrentTab => _currentTab;
        public event Action<NavigationTab> OnTabChanged;

        private void Awake()
        {
            if (_shopTabButton != null) _shopTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Shop));
            if (_battleTabButton != null) _battleTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Battle));
            if (_leaderboardTabButton != null) _leaderboardTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Leaderboard));
        }

        private void Start()
        {
            SwitchTab(NavigationTab.Battle);
        }

        public void BindReferences(
            Button shopBtn, Button battleBtn, Button leaderboardBtn,
            TextMeshProUGUI shopTxt, TextMeshProUGUI battleTxt, TextMeshProUGUI leadTxt,
            GameObject shopPg, GameObject battlePg, GameObject leadPg)
        {
            _shopTabButton = shopBtn;
            _battleTabButton = battleBtn;
            _leaderboardTabButton = leaderboardBtn;

            _shopTabText = shopTxt;
            _battleTabText = battleTxt;
            _leaderboardTabText = leadTxt;

            _shopPage = shopPg;
            _battlePage = battlePg;
            _leaderboardPage = leadPg;

            if (_shopTabButton != null)
            {
                _shopTabButton.onClick.RemoveAllListeners();
                _shopTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Shop));
            }
            if (_battleTabButton != null)
            {
                _battleTabButton.onClick.RemoveAllListeners();
                _battleTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Battle));
            }
            if (_leaderboardTabButton != null)
            {
                _leaderboardTabButton.onClick.RemoveAllListeners();
                _leaderboardTabButton.onClick.AddListener(() => SwitchTab(NavigationTab.Leaderboard));
            }

            SwitchTab(_currentTab);
        }

        public void SetTabSprites(
            Sprite shopActive, Sprite shopInactive,
            Sprite battleActive, Sprite battleInactive,
            Sprite leadActive, Sprite leadInactive)
        {
            _shopActiveSprite = shopActive;
            _shopInactiveSprite = shopInactive;
            _battleActiveSprite = battleActive;
            _battleInactiveSprite = battleInactive;
            _leaderboardActiveSprite = leadActive;
            _leaderboardInactiveSprite = leadInactive;

            SwitchTab(_currentTab);
        }

        /// <summary>
        /// Switches active tab and updates UI visibility.
        /// </summary>
        public void SwitchTab(NavigationTab tab)
        {
            if (_currentTab != tab && RoyalSmash.Audio.AudioManager.Instance != null && Application.isPlaying)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayButtonClick();
            }

            _currentTab = tab;

            // Update Pages
            if (_shopPage != null) _shopPage.SetActive(tab == NavigationTab.Shop);
            if (_battlePage != null) _battlePage.SetActive(tab == NavigationTab.Battle);
            if (_leaderboardPage != null) _leaderboardPage.SetActive(tab == NavigationTab.Leaderboard);

            // Update Colors & Sprites
            UpdateTabVisuals(_shopTabButton, _shopTabText, _shopTabIcon, _shopActiveSprite, _shopInactiveSprite, tab == NavigationTab.Shop);
            UpdateTabVisuals(_battleTabButton, _battleTabText, _battleTabIcon, _battleActiveSprite, _battleInactiveSprite, tab == NavigationTab.Battle);
            UpdateTabVisuals(_leaderboardTabButton, _leaderboardTabText, _leaderboardTabIcon, _leaderboardActiveSprite, _leaderboardInactiveSprite, tab == NavigationTab.Leaderboard);

            OnTabChanged?.Invoke(_currentTab);
        }

        private void UpdateTabVisuals(Button btn, TextMeshProUGUI text, Image icon, Sprite activeSprite, Sprite inactiveSprite, bool isActive)
        {
            if (btn != null && btn.GetComponent<Image>() is Image btnImg)
            {
                if (activeSprite != null && inactiveSprite != null)
                {
                    btnImg.sprite = isActive ? activeSprite : inactiveSprite;
                }
            }

            Color c = isActive ? _activeColor : _inactiveColor;
            if (text != null)
            {
                text.color = c;
                text.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            }
            if (icon != null)
            {
                icon.color = c;
            }
        }
    }
}
