using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Cloud;
using RoyalSmash.Profile;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Master UI controller for the Leaderboard page in the Main Menu.
    /// Manages the scrollable list, connection status badge, refresh button, and sticky player row.
    /// Follows Master Build Prompt Section 19 & Section 35 (Phase 15).
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Header Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusBadgeText;
        [SerializeField] private Image _statusBadgeBg;
        [SerializeField] private Button _refreshButton;

        [Header("Scroll Area")]
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private GameObject _rowPrefab;

        [Header("Sticky Player Row")]
        [SerializeField] private LeaderboardEntryRowUI _stickyPlayerRow;

        [Header("Profile Configuration")]
        [SerializeField] private ProfileConfig _profileConfig;

        private readonly List<LeaderboardEntryRowUI> _spawnedRows = new List<LeaderboardEntryRowUI>();
        private bool _isRefreshing = false;

        public bool IsRefreshing => _isRefreshing;
        public int DisplayedRowCount => _spawnedRows.Count;

        private void Awake()
        {
            if (_refreshButton != null)
            {
                _refreshButton.onClick.AddListener(HandleRefreshClicked);
            }

            EnsureProfileConfig();
        }

        private void OnEnable()
        {
            EnsureProfileConfig();
            RefreshLeaderboard();
        }

        public void BindReferences(
            TextMeshProUGUI title, TextMeshProUGUI statusTxt, Image statusBg,
            Button refreshBtn, Transform container, GameObject rowPrefab,
            LeaderboardEntryRowUI stickyRow, ProfileConfig profileCfg)
        {
            _titleText = title;
            _statusBadgeText = statusTxt;
            _statusBadgeBg = statusBg;
            _refreshButton = refreshBtn;
            _rowContainer = container;
            _rowPrefab = rowPrefab;
            _stickyPlayerRow = stickyRow;
            _profileConfig = profileCfg;

            if (_refreshButton != null)
            {
                _refreshButton.onClick.RemoveAllListeners();
                _refreshButton.onClick.AddListener(HandleRefreshClicked);
            }
        }

        private void EnsureProfileConfig()
        {
            if (_profileConfig == null)
            {
#if UNITY_EDITOR
                _profileConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ProfileConfig>(
                    "Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset");
#endif
            }
        }

        public void RefreshLeaderboard()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            if (_statusBadgeText != null)
            {
                _statusBadgeText.text = "SYNCING...";
            }

            var leadMgr = LeaderboardManager.Instance;
            if (leadMgr != null)
            {
                leadMgr.FetchLeaderboard((success, entries, isCached) =>
                {
                    _isRefreshing = false;
                    PopulateList(entries, isCached);
                });
            }
            else
            {
                _isRefreshing = false;
            }
        }

        public void PopulateList(List<LeaderboardEntryData> entries, bool isCached)
        {
            // 1. Update Connection Status Badge
            UpdateStatusBadge(isCached);

            if (entries == null) return;

            // 2. Clean nulls & Hide existing spawned rows
            _spawnedRows.RemoveAll(r => r == null);
            foreach (var row in _spawnedRows)
            {
                row.gameObject.SetActive(false);
            }

            // 3. Populate rows
            for (int i = 0; i < entries.Count; i++)
            {
                LeaderboardEntryRowUI rowUI;
                if (i < _spawnedRows.Count)
                {
                    rowUI = _spawnedRows[i];
                    rowUI.gameObject.SetActive(true);
                }
                else
                {
                    if (_rowPrefab == null || _rowContainer == null) break;
                    var go = Instantiate(_rowPrefab, _rowContainer);
                    rowUI = go.GetComponent<LeaderboardEntryRowUI>();
                    if (rowUI == null) rowUI = go.AddComponent<LeaderboardEntryRowUI>();
                    _spawnedRows.Add(rowUI);
                }

                rowUI.Setup(entries[i], _profileConfig);
            }

            // 4. Update Sticky Player Row
            if (_stickyPlayerRow != null)
            {
                var playerEntry = entries.Find(e => e.IsLocalPlayer);
                if (playerEntry != null)
                {
                    _stickyPlayerRow.gameObject.SetActive(true);
                    _stickyPlayerRow.Setup(playerEntry, _profileConfig);
                }
                else
                {
                    _stickyPlayerRow.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateStatusBadge(bool isCached)
        {
            if (_statusBadgeText == null) return;

            if (isCached)
            {
                _statusBadgeText.text = "🟡 OFFLINE (CACHED)";
                _statusBadgeText.color = new Color(0.95f, 0.77f, 0.25f); // Amber / Gold
                if (_statusBadgeBg != null) _statusBadgeBg.color = new Color(0.35f, 0.28f, 0.08f, 0.8f);
            }
            else
            {
                _statusBadgeText.text = "🟢 LIVE ONLINE";
                _statusBadgeText.color = new Color(0.2f, 0.9f, 0.4f); // Green
                if (_statusBadgeBg != null) _statusBadgeBg.color = new Color(0.08f, 0.32f, 0.15f, 0.8f);
            }
        }

        private void HandleRefreshClicked()
        {
            Debug.Log("[LeaderboardUI] Manual refresh requested.");
            RefreshLeaderboard();
        }
    }
}
