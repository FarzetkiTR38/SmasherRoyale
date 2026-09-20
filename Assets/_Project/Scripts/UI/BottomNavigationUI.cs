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

        /// <summary>
        /// Switches active tab and updates UI visibility.
        /// </summary>
        public void SwitchTab(NavigationTab tab)
        {
            _currentTab = tab;

            // Update Pages
            if (_shopPage != null) _shopPage.SetActive(tab == NavigationTab.Shop);
            if (_battlePage != null) _battlePage.SetActive(tab == NavigationTab.Battle);
            if (_leaderboardPage != null) _leaderboardPage.SetActive(tab == NavigationTab.Leaderboard);

            // Update Colors
            UpdateTabVisuals(_shopTabText, _shopTabIcon, tab == NavigationTab.Shop);
            UpdateTabVisuals(_battleTabText, _battleTabIcon, tab == NavigationTab.Battle);
            UpdateTabVisuals(_leaderboardTabText, _leaderboardTabIcon, tab == NavigationTab.Leaderboard);

            OnTabChanged?.Invoke(_currentTab);
        }

        private void UpdateTabVisuals(TextMeshProUGUI text, Image icon, bool isActive)
        {
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
