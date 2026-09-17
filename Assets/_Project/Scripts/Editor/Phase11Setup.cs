using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Core;
using RoyalSmash.Economy;
using RoyalSmash.Lives;
using RoyalSmash.Save;
using RoyalSmash.Skills;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    public static class Phase11Setup
    {
        public const string MAIN_MENU_SCENE_PATH = "Assets/_Project/Scenes/MainMenu.unity";
        public const string MAIN_GAME_SCENE_PATH = "Assets/_Project/Scenes/MainGame.unity";

        [MenuItem("RoyalSmash/Phase 11/Setup Main Menu & Build Settings")]
        public static void RunSetup()
        {
            SetupMainMenuScene();
            ConfigureBuildSettings();
            Debug.Log("[Phase11Setup] Phase 11 Main Menu setup and Build Settings configured successfully!");
        }

        public static void ConfigureBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MAIN_MENU_SCENE_PATH, true),
                new EditorBuildSettingsScene(MAIN_GAME_SCENE_PATH, true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[Phase11Setup] Registered MainMenu (Index 0) and MainGame (Index 1) in EditorBuildSettings.");
        }

        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.16f, 0.22f, 1f);
            camObj.tag = "MainCamera";
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camObj.transform.position = new Vector3(0f, 0f, -10f);

            // 2. Global Managers
            EnsureManagers();

            // 3. Canvas & UI
            var canvasObj = new GameObject("MainMenuCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 4. Background Image
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Textures/Environment/KingdomBackdrop.jpg");
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else
            {
                bgImg.color = new Color(0.15f, 0.22f, 0.32f, 1f);
            }

            // Vignette overlay
            var vigObj = new GameObject("VignetteOverlay");
            vigObj.transform.SetParent(canvasObj.transform, false);
            var vigRect = vigObj.AddComponent<RectTransform>();
            vigRect.anchorMin = Vector2.zero;
            vigRect.anchorMax = Vector2.one;
            vigRect.sizeDelta = Vector2.zero;
            var vigImg = vigObj.AddComponent<Image>();
            vigImg.color = new Color(0f, 0f, 0f, 0.40f);

            // 5. Pages Root
            var pagesRoot = new GameObject("PagesRoot");
            pagesRoot.transform.SetParent(canvasObj.transform, false);
            var pagesRect = pagesRoot.AddComponent<RectTransform>();
            pagesRect.anchorMin = Vector2.zero;
            pagesRect.anchorMax = Vector2.one;
            pagesRect.sizeDelta = Vector2.zero;

            // 5A. Battle Page (Center Stage)
            var battlePage = CreateBattlePage(pagesRoot.transform);

            // 5B. Shop Page
            var shopPage = CreatePlaceholderPage(pagesRoot.transform, "ShopPage", "ROYAL SHOP", "🪙 Gold Packs & Skill Bundles\n\nFull IAP store integration coming in Phase 13!");
            shopPage.SetActive(false);

            // 5C. Leaderboard Page
            var leaderboardPage = CreatePlaceholderPage(pagesRoot.transform, "LeaderboardPage", "LEADERBOARD", "🏆 Global Player Rankings\n\nOnline leaderboard & cloud sync coming in Phase 15!");
            leaderboardPage.SetActive(false);

            // 6. Top Bar
            var topBar = CreateTopBar(canvasObj.transform, out var profileBtn, out var settingsBtn, out var goldWgt, out var livesWgt);

            // 7. Bottom Navigation
            var bottomNav = CreateBottomNav(canvasObj.transform, shopPage, battlePage, leaderboardPage, out var bottomNavUI);

            // 8. Dialogs
            var outOfLivesDlg = CreateOutOfLivesDialog(canvasObj.transform);
            var settingsDlg = CreateSettingsDialog(canvasObj.transform);

            // 9. MainMenuController
            var controller = canvasObj.AddComponent<MainMenuController>();
            controller.BindReferences(
                battlePage.transform.Find("LevelCard/LevelText").GetComponent<TextMeshProUGUI>(),
                battlePage.transform.Find("PlayButton").GetComponent<Button>(),
                profileBtn,
                settingsBtn,
                goldWgt,
                livesWgt,
                settingsDlg,
                outOfLivesDlg,
                bottomNavUI,
                battlePage.transform.Find("Offer_Right/TimerText").GetComponent<TextMeshProUGUI>()
            );

            EditorSceneManager.SaveScene(scene, MAIN_MENU_SCENE_PATH);
            Debug.Log($"[Phase11Setup] Saved MainMenu scene to {MAIN_MENU_SCENE_PATH}");
        }

        private static void EnsureManagers()
        {
            var saveMgr = Object.FindFirstObjectByType<SaveManager>();
            if (saveMgr == null) new GameObject("SaveManager").AddComponent<SaveManager>();

            var goldMgr = Object.FindFirstObjectByType<GoldManager>();
            if (goldMgr == null)
            {
                var go = new GameObject("GoldManager");
                var gm = go.AddComponent<GoldManager>();
                var so = new SerializedObject(gm);
                so.FindProperty("_economyConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EconomyConfig>("Assets/_Project/ScriptableObjects/Config/EconomyConfig.asset");
                so.FindProperty("_rewardConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LevelRewardConfig>("Assets/_Project/ScriptableObjects/Config/LevelRewardConfig.asset");
                so.ApplyModifiedProperties();
            }

            var livesMgr = Object.FindFirstObjectByType<LivesManager>();
            if (livesMgr == null)
            {
                var go = new GameObject("LivesManager");
                var lm = go.AddComponent<LivesManager>();
                var so = new SerializedObject(lm);
                so.FindProperty("_config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LivesConfig>("Assets/_Project/ScriptableObjects/Config/LivesConfig.asset");
                so.ApplyModifiedProperties();
            }

            var skillMgr = Object.FindFirstObjectByType<SkillManager>();
            if (skillMgr == null)
            {
                var go = new GameObject("SkillManager");
                var sm = go.AddComponent<SkillManager>();
                var so = new SerializedObject(sm);
                so.FindProperty("_config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SkillConfig>("Assets/_Project/ScriptableObjects/Config/SkillConfig.asset");
                so.ApplyModifiedProperties();
            }

            var transMgr = Object.FindFirstObjectByType<SceneTransitionManager>();
            if (transMgr == null) new GameObject("SceneTransitionManager").AddComponent<SceneTransitionManager>();
        }

        private static GameObject CreateTopBar(Transform parent, out Button profileBtn, out Button settingsBtn, out GoldDisplayUI goldWgt, out LivesWidgetUI livesWgt)
        {
            var bar = new GameObject("TopBar");
            bar.transform.SetParent(parent, false);
            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -40f);
            rect.sizeDelta = new Vector2(0f, 120f);

            // Profile Button (Left)
            var profObj = new GameObject("ProfileButton");
            profObj.transform.SetParent(bar.transform, false);
            var profRect = profObj.AddComponent<RectTransform>();
            profRect.anchorMin = new Vector2(0f, 0.5f);
            profRect.anchorMax = new Vector2(0f, 0.5f);
            profRect.anchoredPosition = new Vector2(70f, 0f);
            profRect.sizeDelta = new Vector2(90f, 90f);
            var profImg = profObj.AddComponent<Image>();
            profImg.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);
            profileBtn = profObj.AddComponent<Button>();

            var profTxtObj = new GameObject("Icon");
            profTxtObj.transform.SetParent(profObj.transform, false);
            var profTmp = profTxtObj.AddComponent<TextMeshProUGUI>();
            profTmp.text = "👑";
            profTmp.fontSize = 42;
            profTmp.alignment = TextAlignmentOptions.Center;

            // Settings Button (Right)
            var setObj = new GameObject("SettingsButton");
            setObj.transform.SetParent(bar.transform, false);
            var setRect = setObj.AddComponent<RectTransform>();
            setRect.anchorMin = new Vector2(1f, 0.5f);
            setRect.anchorMax = new Vector2(1f, 0.5f);
            setRect.anchoredPosition = new Vector2(-70f, 0f);
            setRect.sizeDelta = new Vector2(90f, 90f);
            var setImg = setObj.AddComponent<Image>();
            setImg.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);
            settingsBtn = setObj.AddComponent<Button>();

            var setTxtObj = new GameObject("Icon");
            setTxtObj.transform.SetParent(setObj.transform, false);
            var setTmp = setTxtObj.AddComponent<TextMeshProUGUI>();
            setTmp.text = "⚙";
            setTmp.fontSize = 42;
            setTmp.alignment = TextAlignmentOptions.Center;

            // Gold Widget
            var goldObj = new GameObject("GoldWidget");
            goldObj.transform.SetParent(bar.transform, false);
            var goldRect = goldObj.AddComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.35f, 0.5f);
            goldRect.anchorMax = new Vector2(0.35f, 0.5f);
            goldRect.anchoredPosition = Vector2.zero;
            goldRect.sizeDelta = new Vector2(200f, 80f);
            var goldBg = goldObj.AddComponent<Image>();
            goldBg.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);

            var goldTxtObj = new GameObject("Text");
            goldTxtObj.transform.SetParent(goldObj.transform, false);
            var goldTextRect = goldTxtObj.AddComponent<RectTransform>();
            goldTextRect.anchorMin = Vector2.zero;
            goldTextRect.anchorMax = Vector2.one;
            goldTextRect.offsetMin = new Vector2(55f, 0f);
            goldTextRect.offsetMax = new Vector2(-15f, 0f);
            var goldTmp = goldTxtObj.AddComponent<TextMeshProUGUI>();
            goldTmp.text = "1,000";
            goldTmp.fontSize = 32;
            goldTmp.fontStyle = FontStyles.Bold;
            goldTmp.color = new Color(1.0f, 0.85f, 0.25f, 1f);
            goldTmp.alignment = TextAlignmentOptions.Left;

            var goldIcon = new GameObject("Icon");
            goldIcon.transform.SetParent(goldObj.transform, false);
            var giRect = goldIcon.AddComponent<RectTransform>();
            giRect.anchorMin = new Vector2(0f, 0.5f);
            giRect.anchorMax = new Vector2(0f, 0.5f);
            giRect.anchoredPosition = new Vector2(30f, 0f);
            giRect.sizeDelta = new Vector2(40f, 40f);
            var giTmp = goldIcon.AddComponent<TextMeshProUGUI>();
            giTmp.text = "🪙";
            giTmp.fontSize = 36;
            giTmp.alignment = TextAlignmentOptions.Center;

            goldWgt = goldObj.AddComponent<GoldDisplayUI>();
            goldWgt.BindReferences(goldTmp, goldRect);

            // Lives Widget
            var livesObj = new GameObject("LivesWidget");
            livesObj.transform.SetParent(bar.transform, false);
            var livesRect = livesObj.AddComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0.65f, 0.5f);
            livesRect.anchorMax = new Vector2(0.65f, 0.5f);
            livesRect.anchoredPosition = Vector2.zero;
            livesRect.sizeDelta = new Vector2(200f, 80f);
            var livesBg = livesObj.AddComponent<Image>();
            livesBg.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);

            var livesTxtObj = new GameObject("Text");
            livesTxtObj.transform.SetParent(livesObj.transform, false);
            var livesTextRect = livesTxtObj.AddComponent<RectTransform>();
            livesTextRect.anchorMin = Vector2.zero;
            livesTextRect.anchorMax = Vector2.one;
            livesTextRect.offsetMin = new Vector2(55f, 0f);
            livesTextRect.offsetMax = new Vector2(-45f, 0f);
            var livesTmp = livesTxtObj.AddComponent<TextMeshProUGUI>();
            livesTmp.text = "5/5";
            livesTmp.fontSize = 32;
            livesTmp.fontStyle = FontStyles.Bold;
            livesTmp.color = new Color(1.0f, 0.35f, 0.35f, 1f);
            livesTmp.alignment = TextAlignmentOptions.Left;

            var heartIcon = new GameObject("Icon");
            heartIcon.transform.SetParent(livesObj.transform, false);
            var hiRect = heartIcon.AddComponent<RectTransform>();
            hiRect.anchorMin = new Vector2(0f, 0.5f);
            hiRect.anchorMax = new Vector2(0f, 0.5f);
            hiRect.anchoredPosition = new Vector2(30f, 0f);
            hiRect.sizeDelta = new Vector2(40f, 40f);
            var hiTmp = heartIcon.AddComponent<TextMeshProUGUI>();
            hiTmp.text = "❤️";
            hiTmp.fontSize = 36;
            hiTmp.alignment = TextAlignmentOptions.Center;

            var addLifeObj = new GameObject("AddBtn");
            addLifeObj.transform.SetParent(livesObj.transform, false);
            var alRect = addLifeObj.AddComponent<RectTransform>();
            alRect.anchorMin = new Vector2(1f, 0.5f);
            alRect.anchorMax = new Vector2(1f, 0.5f);
            alRect.anchoredPosition = new Vector2(-22f, 0f);
            alRect.sizeDelta = new Vector2(36f, 36f);
            var alImg = addLifeObj.AddComponent<Image>();
            alImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
            var alBtn = addLifeObj.AddComponent<Button>();
            var alTxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            alTxt.transform.SetParent(addLifeObj.transform, false);
            alTxt.text = "+";
            alTxt.fontSize = 30;
            alTxt.fontStyle = FontStyles.Bold;
            alTxt.alignment = TextAlignmentOptions.Center;

            livesWgt = livesObj.AddComponent<LivesWidgetUI>();
            livesWgt.BindReferences(livesTmp, null, alBtn, null);

            return bar;
        }

        private static GameObject CreateBattlePage(Transform parent)
        {
            var page = new GameObject("BattlePage");
            page.transform.SetParent(parent, false);
            var rect = page.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Level Card (Center Top)
            var cardObj = new GameObject("LevelCard");
            cardObj.transform.SetParent(page.transform, false);
            var cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.62f);
            cardRect.anchorMax = new Vector2(0.5f, 0.62f);
            cardRect.sizeDelta = new Vector2(480f, 160f);
            var cardImg = cardObj.AddComponent<Image>();
            cardImg.color = new Color(0.10f, 0.14f, 0.22f, 0.92f);

            var subTxt = new GameObject("Subtitle").AddComponent<TextMeshProUGUI>();
            subTxt.transform.SetParent(cardObj.transform, false);
            subTxt.rectTransform.anchoredPosition = new Vector2(0f, 40f);
            subTxt.text = "CURRENT PROGRESSION";
            subTxt.fontSize = 24;
            subTxt.color = new Color(0.7f, 0.75f, 0.85f, 1f);
            subTxt.alignment = TextAlignmentOptions.Center;

            var lvlTxt = new GameObject("LevelText").AddComponent<TextMeshProUGUI>();
            lvlTxt.transform.SetParent(cardObj.transform, false);
            lvlTxt.rectTransform.anchoredPosition = new Vector2(0f, -10f);
            lvlTxt.text = "LEVEL 1";
            lvlTxt.fontSize = 58;
            lvlTxt.fontStyle = FontStyles.Bold;
            lvlTxt.color = new Color(1.0f, 0.88f, 0.35f, 1f);
            lvlTxt.alignment = TextAlignmentOptions.Center;

            // Big Play Button
            var playObj = new GameObject("PlayButton");
            playObj.transform.SetParent(page.transform, false);
            var playRect = playObj.AddComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.44f);
            playRect.anchorMax = new Vector2(0.5f, 0.44f);
            playRect.sizeDelta = new Vector2(480f, 140f);
            var playImg = playObj.AddComponent<Image>();
            playImg.color = new Color(0.20f, 0.75f, 0.30f, 1.0f); // Bright casual green
            playObj.AddComponent<Button>();

            var playTxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            playTxt.transform.SetParent(playObj.transform, false);
            playTxt.text = "PLAY";
            playTxt.fontSize = 62;
            playTxt.fontStyle = FontStyles.Bold;
            playTxt.color = Color.white;
            playTxt.alignment = TextAlignmentOptions.Center;

            // Left Offer Card
            var leftOffer = new GameObject("Offer_Left");
            leftOffer.transform.SetParent(page.transform, false);
            var loRect = leftOffer.AddComponent<RectTransform>();
            loRect.anchorMin = new Vector2(0.5f, 0.44f);
            loRect.anchorMax = new Vector2(0.5f, 0.44f);
            loRect.anchoredPosition = new Vector2(-360f, 0f);
            loRect.sizeDelta = new Vector2(180f, 220f);
            var loImg = leftOffer.AddComponent<Image>();
            loImg.color = new Color(0.15f, 0.18f, 0.26f, 0.90f);
            leftOffer.AddComponent<Button>();

            var loIcon = new GameObject("Icon").AddComponent<TextMeshProUGUI>();
            loIcon.transform.SetParent(leftOffer.transform, false);
            loIcon.rectTransform.anchoredPosition = new Vector2(0f, 35f);
            loIcon.text = "🎁";
            loIcon.fontSize = 52;
            loIcon.alignment = TextAlignmentOptions.Center;

            var loTxt = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            loTxt.transform.SetParent(leftOffer.transform, false);
            loTxt.rectTransform.anchoredPosition = new Vector2(0f, -40f);
            loTxt.text = "STARTER\nPACK";
            loTxt.fontSize = 22;
            loTxt.fontStyle = FontStyles.Bold;
            loTxt.alignment = TextAlignmentOptions.Center;

            // Right Offer Card (with countdown)
            var rightOffer = new GameObject("Offer_Right");
            rightOffer.transform.SetParent(page.transform, false);
            var roRect = rightOffer.AddComponent<RectTransform>();
            roRect.anchorMin = new Vector2(0.5f, 0.44f);
            roRect.anchorMax = new Vector2(0.5f, 0.44f);
            roRect.anchoredPosition = new Vector2(360f, 0f);
            roRect.sizeDelta = new Vector2(180f, 220f);
            var roImg = rightOffer.AddComponent<Image>();
            roImg.color = new Color(0.15f, 0.18f, 0.26f, 0.90f);
            rightOffer.AddComponent<Button>();

            var roIcon = new GameObject("Icon").AddComponent<TextMeshProUGUI>();
            roIcon.transform.SetParent(rightOffer.transform, false);
            roIcon.rectTransform.anchoredPosition = new Vector2(0f, 35f);
            roIcon.text = "⚡";
            roIcon.fontSize = 52;
            roIcon.alignment = TextAlignmentOptions.Center;

            var roTxt = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            roTxt.transform.SetParent(rightOffer.transform, false);
            roTxt.rectTransform.anchoredPosition = new Vector2(0f, -30f);
            roTxt.text = "SPECIAL\nDEAL";
            roTxt.fontSize = 22;
            roTxt.fontStyle = FontStyles.Bold;
            roTxt.alignment = TextAlignmentOptions.Center;

            var timerTxt = new GameObject("TimerText").AddComponent<TextMeshProUGUI>();
            timerTxt.transform.SetParent(rightOffer.transform, false);
            timerTxt.rectTransform.anchoredPosition = new Vector2(0f, -80f);
            timerTxt.text = "23:59:59";
            timerTxt.fontSize = 20;
            timerTxt.color = new Color(1.0f, 0.85f, 0.35f, 1f);
            timerTxt.alignment = TextAlignmentOptions.Center;

            return page;
        }

        private static GameObject CreatePlaceholderPage(Transform parent, string name, string title, string message)
        {
            var page = new GameObject(name);
            page.transform.SetParent(parent, false);
            var rect = page.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var card = new GameObject("Card");
            card.transform.SetParent(page.transform, false);
            var cr = card.AddComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.5f, 0.5f);
            cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(800f, 600f);
            card.AddComponent<Image>().color = new Color(0.10f, 0.14f, 0.22f, 0.95f);

            var t1 = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            t1.transform.SetParent(card.transform, false);
            t1.rectTransform.anchoredPosition = new Vector2(0f, 180f);
            t1.text = title;
            t1.fontSize = 54;
            t1.fontStyle = FontStyles.Bold;
            t1.color = new Color(1.0f, 0.88f, 0.35f, 1f);
            t1.alignment = TextAlignmentOptions.Center;

            var t2 = new GameObject("Desc").AddComponent<TextMeshProUGUI>();
            t2.transform.SetParent(card.transform, false);
            t2.rectTransform.sizeDelta = new Vector2(700f, 300f);
            t2.text = message;
            t2.fontSize = 32;
            t2.color = Color.white;
            t2.alignment = TextAlignmentOptions.Center;

            return page;
        }

        private static GameObject CreateBottomNav(Transform parent, GameObject shopPg, GameObject battlePg, GameObject leadPg, out BottomNavigationUI navUI)
        {
            var bar = new GameObject("BottomNav");
            bar.transform.SetParent(parent, false);
            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 160f);
            bar.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.98f);

            navUI = bar.AddComponent<BottomNavigationUI>();

            // Tab 1: Shop
            var shopBtnObj = CreateNavTab(bar.transform, "ShopTab", "🛒", "Shop", -340f);
            var battleBtnObj = CreateNavTab(bar.transform, "BattleTab", "⚔️", "Battle", 0f);
            var leadBtnObj = CreateNavTab(bar.transform, "LeaderboardTab", "🏆", "Rankings", 340f);

            navUI.BindReferences(
                shopBtnObj.GetComponent<Button>(),
                battleBtnObj.GetComponent<Button>(),
                leadBtnObj.GetComponent<Button>(),
                shopBtnObj.transform.Find("Text").GetComponent<TextMeshProUGUI>(),
                battleBtnObj.transform.Find("Text").GetComponent<TextMeshProUGUI>(),
                leadBtnObj.transform.Find("Text").GetComponent<TextMeshProUGUI>(),
                shopPg, battlePg, leadPg
            );

            return bar;
        }

        private static GameObject CreateNavTab(Transform parent, string name, string icon, string label, float posX)
        {
            var tab = new GameObject(name);
            tab.transform.SetParent(parent, false);
            var r = tab.AddComponent<RectTransform>();
            r.anchoredPosition = new Vector2(posX, 0f);
            r.sizeDelta = new Vector2(280f, 140f);
            tab.AddComponent<Button>();

            var ic = new GameObject("Icon").AddComponent<TextMeshProUGUI>();
            ic.transform.SetParent(tab.transform, false);
            ic.rectTransform.anchoredPosition = new Vector2(0f, 25f);
            ic.text = icon;
            ic.fontSize = 46;
            ic.alignment = TextAlignmentOptions.Center;

            var tx = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            tx.transform.SetParent(tab.transform, false);
            tx.rectTransform.anchoredPosition = new Vector2(0f, -30f);
            tx.text = label;
            tx.fontSize = 26;
            tx.fontStyle = FontStyles.Bold;
            tx.alignment = TextAlignmentOptions.Center;

            return tab;
        }

        private static OutOfLivesDialog CreateOutOfLivesDialog(Transform parent)
        {
            var dlgObj = new GameObject("OutOfLivesDialog");
            dlgObj.transform.SetParent(parent, false);
            var r = dlgObj.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            dlgObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var card = new GameObject("Content");
            card.transform.SetParent(dlgObj.transform, false);
            var cr = card.AddComponent<RectTransform>();
            cr.sizeDelta = new Vector2(750f, 650f);
            card.AddComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 1f);

            var title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            title.transform.SetParent(card.transform, false);
            title.rectTransform.anchoredPosition = new Vector2(0f, 220f);
            title.text = "OUT OF LIVES!";
            title.fontSize = 48;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(1f, 0.4f, 0.4f, 1f);
            title.alignment = TextAlignmentOptions.Center;

            var desc = new GameObject("Desc").AddComponent<TextMeshProUGUI>();
            desc.transform.SetParent(card.transform, false);
            desc.rectTransform.anchoredPosition = new Vector2(0f, 130f);
            desc.text = "Wait for life regeneration (1 life / 30m)\nor purchase instantly:";
            desc.fontSize = 28;
            desc.alignment = TextAlignmentOptions.Center;

            // Buy with gold btn
            var buyBtn = new GameObject("BuyGoldBtn");
            buyBtn.transform.SetParent(card.transform, false);
            var bbr = buyBtn.AddComponent<RectTransform>();
            bbr.anchoredPosition = new Vector2(0f, 10f);
            bbr.sizeDelta = new Vector2(500f, 90f);
            buyBtn.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.15f, 1f);
            var bBtn = buyBtn.AddComponent<Button>();
            var btxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            btxt.transform.SetParent(buyBtn.transform, false);
            btxt.text = "+1 Life (100 Gold)";
            btxt.fontSize = 32;
            btxt.fontStyle = FontStyles.Bold;
            btxt.color = Color.black;
            btxt.alignment = TextAlignmentOptions.Center;

            // Watch ad btn
            var adBtn = new GameObject("WatchAdBtn");
            adBtn.transform.SetParent(card.transform, false);
            var abr = adBtn.AddComponent<RectTransform>();
            abr.anchoredPosition = new Vector2(0f, -110f);
            abr.sizeDelta = new Vector2(500f, 90f);
            adBtn.AddComponent<Image>().color = new Color(0.25f, 0.65f, 0.95f, 1f);
            var aBtn = adBtn.AddComponent<Button>();
            var atxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            atxt.transform.SetParent(adBtn.transform, false);
            atxt.text = "Watch Ad (+1 Life)";
            atxt.fontSize = 32;
            atxt.fontStyle = FontStyles.Bold;
            atxt.color = Color.white;
            atxt.alignment = TextAlignmentOptions.Center;

            // Close btn
            var closeBtn = new GameObject("CloseBtn");
            closeBtn.transform.SetParent(card.transform, false);
            var cbr = closeBtn.AddComponent<RectTransform>();
            cbr.anchoredPosition = new Vector2(0f, -220f);
            cbr.sizeDelta = new Vector2(300f, 70f);
            closeBtn.AddComponent<Image>().color = new Color(0.3f, 0.35f, 0.45f, 1f);
            var cBtn = closeBtn.AddComponent<Button>();
            var ctxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            ctxt.transform.SetParent(closeBtn.transform, false);
            ctxt.text = "Close";
            ctxt.fontSize = 28;
            ctxt.alignment = TextAlignmentOptions.Center;

            var dlg = dlgObj.AddComponent<OutOfLivesDialog>();
            dlg.BindReferences(dlgObj, btxt, bBtn, aBtn, cBtn);
            dlgObj.SetActive(false);
            return dlg;
        }

        private static InGameSettingsDialog CreateSettingsDialog(Transform parent)
        {
            var dlgObj = new GameObject("SettingsDialog");
            dlgObj.transform.SetParent(parent, false);
            var r = dlgObj.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            dlgObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var card = new GameObject("Content");
            card.transform.SetParent(dlgObj.transform, false);
            var cr = card.AddComponent<RectTransform>();
            cr.sizeDelta = new Vector2(750f, 700f);
            card.AddComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 1f);

            var title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            title.transform.SetParent(card.transform, false);
            title.rectTransform.anchoredPosition = new Vector2(0f, 250f);
            title.text = "SETTINGS";
            title.fontSize = 52;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(1f, 0.88f, 0.35f, 1f);
            title.alignment = TextAlignmentOptions.Center;

            // Toggles
            var musicBtn = CreateToggleItem(card.transform, "MusicToggle", "Music", 130f, out var musicStatus);
            var sfxBtn = CreateToggleItem(card.transform, "SfxToggle", "Sound SFX", 30f, out var sfxStatus);
            var vibBtn = CreateToggleItem(card.transform, "VibrationToggle", "Vibration", -70f, out var vibStatus);

            // Close button
            var closeBtn = new GameObject("CloseBtn");
            closeBtn.transform.SetParent(card.transform, false);
            var cbr = closeBtn.AddComponent<RectTransform>();
            cbr.anchoredPosition = new Vector2(0f, -200f);
            cbr.sizeDelta = new Vector2(350f, 80f);
            closeBtn.AddComponent<Image>().color = new Color(0.2f, 0.7f, 0.35f, 1f);
            var cBtn = closeBtn.AddComponent<Button>();
            var ctxt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            ctxt.transform.SetParent(closeBtn.transform, false);
            ctxt.text = "Back";
            ctxt.fontSize = 32;
            ctxt.fontStyle = FontStyles.Bold;
            ctxt.alignment = TextAlignmentOptions.Center;

            var dlg = dlgObj.AddComponent<InGameSettingsDialog>();
            dlg.BindReferences(dlgObj, cBtn, null, null, musicBtn, musicStatus, sfxBtn, sfxStatus);
            dlgObj.SetActive(false);
            return dlg;
        }

        private static Button CreateToggleItem(Transform parent, string name, string label, float posY, out TextMeshProUGUI statusTmp)
        {
            var row = new GameObject(name);
            row.transform.SetParent(parent, false);
            var r = row.AddComponent<RectTransform>();
            r.anchoredPosition = new Vector2(0f, posY);
            r.sizeDelta = new Vector2(550f, 75f);
            row.AddComponent<Image>().color = new Color(0.18f, 0.22f, 0.30f, 1f);
            var btn = row.AddComponent<Button>();

            var lbl = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            lbl.transform.SetParent(row.transform, false);
            lbl.rectTransform.anchoredPosition = new Vector2(-120f, 0f);
            lbl.text = label;
            lbl.fontSize = 30;
            lbl.alignment = TextAlignmentOptions.Left;

            var st = new GameObject("Status").AddComponent<TextMeshProUGUI>();
            st.transform.SetParent(row.transform, false);
            st.rectTransform.anchoredPosition = new Vector2(180f, 0f);
            st.text = "ON";
            st.fontSize = 30;
            st.fontStyle = FontStyles.Bold;
            st.color = new Color(0.3f, 0.9f, 0.4f, 1f);
            st.alignment = TextAlignmentOptions.Right;

            statusTmp = st;
            return btn;
        }
    }
}
