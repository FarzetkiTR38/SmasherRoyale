using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Cloud;
using RoyalSmash.Profile;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Editor automation tool setting up Leaderboard & Cloud Sync (Phase 15).
    /// Instantiates LeaderboardConfig, configures LeaderboardPage in MainMenu, wires managers and UI.
    /// </summary>
    public static class Phase15Setup
    {
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset";
        private const string PROFILE_CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset";
        private const string ROW_PREFAB_PATH = "Assets/_Project/Prefabs/UI/LeaderboardRow.prefab";
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";

        [MenuItem("RoyalSmash/Phase 15/Setup Leaderboard and Cloud Sync")]
        public static void RunFullSetup()
        {
            Debug.Log("=== STARTING PHASE 15 AUTOMATED SETUP ===");

            // 1. Create or load LeaderboardConfig
            var config = CreateOrLoadLeaderboardConfig();

            // 2. Load ProfileConfig
            var profileConfig = AssetDatabase.LoadAssetAtPath<ProfileConfig>(PROFILE_CONFIG_PATH);

            // 3. Create LeaderboardRow Prefab
            var rowPrefab = CreateOrUpdateRowPrefab();

            // 4. Setup MainMenu Scene
            SetupMainMenuScene(config, profileConfig, rowPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== PHASE 15 AUTOMATED SETUP COMPLETED SUCCESSFULLY ===");
        }

        public static LeaderboardConfig CreateOrLoadLeaderboardConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LeaderboardConfig>();
                config.EnableSimulatedNetwork = true;

                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Phase15Setup] Created new LeaderboardConfig at {CONFIG_PATH}");
            }
            return config;
        }

        private static GameObject CreateOrUpdateRowPrefab()
        {
            var prefabDir = "Assets/_Project/Prefabs/UI";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ROW_PREFAB_PATH);
            if (existing != null) return existing;

            var rowGo = new GameObject("LeaderboardRow", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LeaderboardEntryRowUI), typeof(LayoutElement));
            var rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(340f, 52f);

            var layoutElem = rowGo.GetComponent<LayoutElement>();
            layoutElem.minHeight = 52f;
            layoutElem.preferredHeight = 52f;
            layoutElem.flexibleWidth = 1f;

            var bgImg = rowGo.GetComponent<Image>();
            bgImg.color = new Color(0.12f, 0.14f, 0.20f, 0.9f);

            var outline = rowGo.GetComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.77f, 0.25f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);
            outline.enabled = false;

            // Rank Text
            var rankGo = new GameObject("RankText", typeof(RectTransform), typeof(TextMeshProUGUI));
            rankGo.transform.SetParent(rowGo.transform, false);
            var rankRect = rankGo.GetComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0f, 0.5f);
            rankRect.anchorMax = new Vector2(0f, 0.5f);
            rankRect.anchoredPosition = new Vector2(24f, 0f);
            rankRect.sizeDelta = new Vector2(40f, 30f);
            var rankText = rankGo.GetComponent<TextMeshProUGUI>();
            rankText.text = "#1";
            rankText.fontSize = 16;
            rankText.fontStyle = FontStyles.Bold;
            rankText.alignment = TextAlignmentOptions.Center;
            rankText.color = new Color(1f, 0.84f, 0f);

            // Avatar Container
            var avatarRoot = new GameObject("AvatarRoot", typeof(RectTransform));
            avatarRoot.transform.SetParent(rowGo.transform, false);
            var avRootRect = avatarRoot.GetComponent<RectTransform>();
            avRootRect.anchorMin = new Vector2(0f, 0.5f);
            avRootRect.anchorMax = new Vector2(0f, 0.5f);
            avRootRect.anchoredPosition = new Vector2(68f, 0f);
            avRootRect.sizeDelta = new Vector2(36f, 36f);

            var avatarGo = new GameObject("AvatarIcon", typeof(RectTransform), typeof(Image));
            avatarGo.transform.SetParent(avatarRoot.transform, false);
            var avRect = avatarGo.GetComponent<RectTransform>();
            avRect.anchorMin = Vector2.zero;
            avRect.anchorMax = Vector2.one;
            avRect.offsetMin = Vector2.zero;
            avRect.offsetMax = Vector2.zero;
            var avImg = avatarGo.GetComponent<Image>();

            var frameGo = new GameObject("FrameBorder", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(avatarRoot.transform, false);
            var frRect = frameGo.GetComponent<RectTransform>();
            frRect.anchorMin = Vector2.zero;
            frRect.anchorMax = Vector2.one;
            frRect.offsetMin = new Vector2(-2, -2);
            frRect.offsetMax = new Vector2(2, 2);
            var frImg = frameGo.GetComponent<Image>();

            // Username Text
            var userGo = new GameObject("UsernameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            userGo.transform.SetParent(rowGo.transform, false);
            var userRect = userGo.GetComponent<RectTransform>();
            userRect.anchorMin = new Vector2(0f, 0.5f);
            userRect.anchorMax = new Vector2(1f, 0.5f);
            userRect.offsetMin = new Vector2(98f, -15f);
            userRect.offsetMax = new Vector2(-80f, 15f);
            var userText = userGo.GetComponent<TextMeshProUGUI>();
            userText.text = "Sir Lancelot";
            userText.fontSize = 14;
            userText.fontStyle = FontStyles.Bold;
            userText.alignment = TextAlignmentOptions.MidlineLeft;
            userText.color = Color.white;
            userText.overflowMode = TextOverflowModes.Ellipsis;

            // Level Badge
            var levelGo = new GameObject("LevelBadge", typeof(RectTransform), typeof(Image));
            levelGo.transform.SetParent(rowGo.transform, false);
            var lvlRect = levelGo.GetComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(1f, 0.5f);
            lvlRect.anchorMax = new Vector2(1f, 0.5f);
            lvlRect.anchoredPosition = new Vector2(-42f, 0f);
            lvlRect.sizeDelta = new Vector2(64f, 26f);
            var lvlBg = levelGo.GetComponent<Image>();
            lvlBg.color = new Color(0.2f, 0.25f, 0.35f, 0.85f);

            var lvlTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            lvlTextGo.transform.SetParent(levelGo.transform, false);
            var lvlTextRect = lvlTextGo.GetComponent<RectTransform>();
            lvlTextRect.anchorMin = Vector2.zero;
            lvlTextRect.anchorMax = Vector2.one;
            lvlTextRect.offsetMin = Vector2.zero;
            lvlTextRect.offsetMax = Vector2.zero;
            var lvlText = lvlTextGo.GetComponent<TextMeshProUGUI>();
            lvlText.text = "Lvl 30";
            lvlText.fontSize = 12;
            lvlText.fontStyle = FontStyles.Bold;
            lvlText.alignment = TextAlignmentOptions.Center;
            lvlText.color = new Color(1.0f, 0.85f, 0.3f);

            var rowUI = rowGo.GetComponent<LeaderboardEntryRowUI>();
            rowUI.BindReferences(bgImg, outline, rankText, avImg, frImg, userText, lvlText);

            var prefab = PrefabUtility.SaveAsPrefabAsset(rowGo, ROW_PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(rowGo);
            Debug.Log($"[Phase15Setup] Created LeaderboardRow prefab at {ROW_PREFAB_PATH}");
            return prefab;
        }

        private static void SetupMainMenuScene(LeaderboardConfig config, ProfileConfig profileConfig, GameObject rowPrefab)
        {
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            // Ensure Managers as individual root objects matching project architecture
            var cloudSyncGo = GameObject.Find("CloudSyncManager");
            if (cloudSyncGo == null) cloudSyncGo = new GameObject("CloudSyncManager");
            var cloudSyncMgr = cloudSyncGo.GetComponent<CloudSyncManager>();
            if (cloudSyncMgr == null) cloudSyncMgr = cloudSyncGo.AddComponent<CloudSyncManager>();
            var soCloud = new SerializedObject(cloudSyncMgr);
            var propCfg = soCloud.FindProperty("_config");
            if (propCfg != null) { propCfg.objectReferenceValue = config; soCloud.ApplyModifiedProperties(); }

            var leadGo = GameObject.Find("LeaderboardManager");
            if (leadGo == null) leadGo = new GameObject("LeaderboardManager");
            var leadMgr = leadGo.GetComponent<LeaderboardManager>();
            if (leadMgr == null) leadMgr = leadGo.AddComponent<LeaderboardManager>();

            // Setup LeaderboardPage in Canvas (under PagesRoot)
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            Transform leadPageTransform = canvas != null ? canvas.transform.Find("PagesRoot/LeaderboardPage") : null;
            if (leadPageTransform == null)
            {
                leadPageTransform = GameObject.Find("LeaderboardPage")?.transform;
            }

            if (leadPageTransform == null)
            {
                Debug.LogError("[Phase15Setup] LeaderboardPage not found in Canvas or PagesRoot!");
                return;
            }

            var leadPageGo = leadPageTransform.gameObject;
            var leadUI = leadPageGo.GetComponent<LeaderboardUI>();
            if (leadUI == null) leadUI = leadPageGo.AddComponent<LeaderboardUI>();

            // Clean old child placeholders
            while (leadPageTransform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(leadPageTransform.GetChild(0).gameObject);
            }

            // 1. Header (Title, Status Badge, Refresh Button)
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(leadPageTransform, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -90f);
            headerRect.sizeDelta = new Vector2(360f, 65f);

            var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);
            titleRect.sizeDelta = new Vector2(320f, 30f);
            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "ROYAL LEADERBOARD";
            titleTmp.fontSize = 20;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.95f, 0.77f, 0.25f);

            // Status Badge
            var badgeGo = new GameObject("StatusBadge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(headerGo.transform, false);
            var badgeRect = badgeGo.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 0f);
            badgeRect.anchorMax = new Vector2(0.5f, 0f);
            badgeRect.anchoredPosition = new Vector2(-30f, 12f);
            badgeRect.sizeDelta = new Vector2(170f, 22f);
            var badgeImg = badgeGo.GetComponent<Image>();
            badgeImg.color = new Color(0.08f, 0.32f, 0.15f, 0.85f);

            var badgeTxtGo = new GameObject("BadgeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeTxtGo.transform.SetParent(badgeGo.transform, false);
            var badgeTxtRect = badgeTxtGo.GetComponent<RectTransform>();
            badgeTxtRect.anchorMin = Vector2.zero;
            badgeTxtRect.anchorMax = Vector2.one;
            badgeTxtRect.offsetMin = Vector2.zero;
            badgeTxtRect.offsetMax = Vector2.zero;
            var badgeTmp = badgeTxtGo.GetComponent<TextMeshProUGUI>();
            badgeTmp.text = "🟢 LIVE ONLINE";
            badgeTmp.fontSize = 11;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.alignment = TextAlignmentOptions.Center;
            badgeTmp.color = new Color(0.2f, 0.9f, 0.4f);

            // Refresh Button
            var refreshBtnGo = new GameObject("RefreshButton", typeof(RectTransform), typeof(Image), typeof(Button));
            refreshBtnGo.transform.SetParent(headerGo.transform, false);
            var refreshBtnRect = refreshBtnGo.GetComponent<RectTransform>();
            refreshBtnRect.anchorMin = new Vector2(0.5f, 0f);
            refreshBtnRect.anchorMax = new Vector2(0.5f, 0f);
            refreshBtnRect.anchoredPosition = new Vector2(90f, 12f);
            refreshBtnRect.sizeDelta = new Vector2(65f, 22f);
            var refreshBtnImg = refreshBtnGo.GetComponent<Image>();
            refreshBtnImg.color = new Color(0.25f, 0.32f, 0.45f, 0.9f);
            var refreshBtn = refreshBtnGo.GetComponent<Button>();

            var refreshTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            refreshTxtGo.transform.SetParent(refreshBtnGo.transform, false);
            var refreshTxtRect = refreshTxtGo.GetComponent<RectTransform>();
            refreshTxtRect.anchorMin = Vector2.zero;
            refreshTxtRect.anchorMax = Vector2.one;
            refreshTxtRect.offsetMin = Vector2.zero;
            refreshTxtRect.offsetMax = Vector2.zero;
            var refreshTmp = refreshTxtGo.GetComponent<TextMeshProUGUI>();
            refreshTmp.text = "Refresh";
            refreshTmp.fontSize = 11;
            refreshTmp.fontStyle = FontStyles.Bold;
            refreshTmp.alignment = TextAlignmentOptions.Center;
            refreshTmp.color = Color.white;

            // 2. Scroll Area (ScrollRect, Viewport, Content)
            var scrollGo = new GameObject("LeaderboardScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(leadPageTransform, false);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -25f);
            scrollRect.sizeDelta = new Vector2(350f, 430f);

            var scrollBg = scrollGo.GetComponent<Image>();
            scrollBg.color = new Color(0.08f, 0.1f, 0.15f, 0.4f);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportMask = viewportGo.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollComp = scrollGo.GetComponent<ScrollRect>();
            scrollComp.content = contentRect;
            scrollComp.viewport = viewportRect;
            scrollComp.horizontal = false;
            scrollComp.vertical = true;
            scrollComp.movementType = ScrollRect.MovementType.Clamped;

            // 3. Sticky Player Row (Bottom pinned)
            var stickyGo = UnityEngine.Object.Instantiate(rowPrefab, leadPageTransform);
            stickyGo.name = "StickyPlayerRow";
            var stickyRect = stickyGo.GetComponent<RectTransform>();
            stickyRect.anchorMin = new Vector2(0.5f, 0f);
            stickyRect.anchorMax = new Vector2(0.5f, 0f);
            stickyRect.anchoredPosition = new Vector2(0f, 82f); // Just above BottomNavigation
            stickyRect.sizeDelta = new Vector2(350f, 54f);

            var stickyRowUI = stickyGo.GetComponent<LeaderboardEntryRowUI>();

            // Bind UI
            leadUI.BindReferences(titleTmp, badgeTmp, badgeImg, refreshBtn, contentGo.transform, rowPrefab, stickyRowUI, profileConfig);

            // Keep LeaderboardPage inactive by default (Battle page active)
            leadPageGo.SetActive(false);

            EditorUtility.SetDirty(leadPageGo);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Phase15Setup] LeaderboardPage successfully configured in MainMenu.");
        }
    }
}
