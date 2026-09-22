using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    public static class MainMenuRevampTool
    {
        [MenuItem("Tools/RoyalSmash/Apply MainMenu Revamp")]
        public static void ApplyRevamp()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity");

            // 1. Load Sprites & Textures
            string bgPath = "Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_01 (1).png";
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            var bgTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(bgPath);

            var playGoldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/btn_play_gold.png");
            var stageCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/panel_stage_card.png");
            var pillEmptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/widget_pill_empty.png");
            var coinIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/icon_coin.png");
            var lightningIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/icon_lightning.png");
            var plusIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/icon_plus_green.png");
            var settingsGearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/btn_settings_gear.png");
            var avatarRexSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/MainMenuSliced/avatar_rex.png");

            var shopActiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_04 (5).png");
            var shopInactiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_03 (4).png");
            var battleActiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_05 (7).png");
            var battleInactiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_05 (6).png");
            var leadActiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_07 (9).png");
            var leadInactiveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 19 Eyl 2026 23_28_06 (8).png");

            var canvasObj = GameObject.Find("MainMenuCanvas");
            if (canvasObj == null)
            {
                Debug.LogError("[MainMenuRevampTool] MainMenuCanvas not found!");
                return;
            }

            // 2. Background
            var bgObj = canvasObj.transform.Find("Background")?.gameObject;
            if (bgObj != null)
            {
                var rawImg = bgObj.GetComponent<RawImage>();
                if (rawImg != null)
                {
                    rawImg.texture = bgTexture;
                    rawImg.color = Color.white;
                }
                else
                {
                    var img = bgObj.GetComponent<Image>();
                    if (img == null) img = bgObj.AddComponent<Image>();
                    img.sprite = bgSprite;
                    img.color = Color.white;
                }

                var bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
            }

            // Disable VignetteOverlay so Rift Raiders artwork is vivid and clear
            var vignetteObj = canvasObj.transform.Find("VignetteOverlay")?.gameObject;
            if (vignetteObj != null) vignetteObj.SetActive(false);

            // 3. TopBar
            var topBarObj = canvasObj.transform.Find("TopBar")?.gameObject;
            if (topBarObj != null)
            {
                var topBarRt = topBarObj.GetComponent<RectTransform>();
                topBarRt.anchorMin = new Vector2(0f, 1f);
                topBarRt.anchorMax = new Vector2(1f, 1f);
                topBarRt.pivot = new Vector2(0.5f, 1f);
                topBarRt.anchoredPosition = new Vector2(0f, -30f);
                topBarRt.sizeDelta = new Vector2(0f, 170f);

                var tbImg = topBarObj.GetComponent<Image>();
                if (tbImg != null) tbImg.color = new Color(0, 0, 0, 0);

                // ProfileButton (Rex + Level 27 badge)
                var profBtnObj = topBarObj.transform.Find("ProfileButton")?.gameObject;
                if (profBtnObj != null)
                {
                    var rt = profBtnObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 0.5f);
                    rt.anchorMax = new Vector2(0f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(95f, -15f);
                    rt.sizeDelta = new Vector2(145f, 145f);

                    var pImg = profBtnObj.GetComponent<Image>();
                    if (pImg != null && avatarRexSprite != null)
                    {
                        pImg.sprite = avatarRexSprite;
                        pImg.color = Color.white;
                    }
                    for (int i = 0; i < profBtnObj.transform.childCount; i++)
                    {
                        profBtnObj.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // SettingsButton
                var setBtnObj = topBarObj.transform.Find("SettingsButton")?.gameObject;
                if (setBtnObj != null)
                {
                    var rt = setBtnObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(1f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-95f, -15f);
                    rt.sizeDelta = new Vector2(125f, 125f);

                    var sImg = setBtnObj.GetComponent<Image>();
                    if (sImg != null && settingsGearSprite != null)
                    {
                        sImg.sprite = settingsGearSprite;
                        sImg.color = Color.white;
                    }
                    for (int i = 0; i < setBtnObj.transform.childCount; i++)
                    {
                        setBtnObj.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // GoldWidget
                var goldWgtObj = topBarObj.transform.Find("GoldWidget")?.gameObject;
                if (goldWgtObj != null)
                {
                    var rt = goldWgtObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-135f, -15f);
                    rt.sizeDelta = new Vector2(265f, 92f);

                    var gImg = goldWgtObj.GetComponent<Image>();
                    if (gImg != null && pillEmptySprite != null)
                    {
                        gImg.sprite = pillEmptySprite;
                        gImg.color = Color.white;
                        gImg.type = Image.Type.Simple;
                    }

                    // Left Coin Icon
                    var iconChild = goldWgtObj.transform.Find("Icon")?.gameObject;
                    if (iconChild != null)
                    {
                        iconChild.SetActive(true);
                        var oldTmp = iconChild.GetComponent<TextMeshProUGUI>();
                        if (oldTmp != null) Object.DestroyImmediate(oldTmp);

                        var iRt = iconChild.GetComponent<RectTransform>();
                        iRt.anchorMin = new Vector2(0f, 0.5f);
                        iRt.anchorMax = new Vector2(0f, 0.5f);
                        iRt.pivot = new Vector2(0.5f, 0.5f);
                        iRt.anchoredPosition = new Vector2(40f, 0f);
                        iRt.sizeDelta = new Vector2(72f, 72f);

                        var iImg = iconChild.GetComponent<Image>();
                        if (iImg == null) iImg = iconChild.AddComponent<Image>();
                        iImg.sprite = coinIconSprite;
                        iImg.color = Color.white;
                    }

                    // Gold Number TMP
                    var txtChild = goldWgtObj.transform.Find("Text")?.gameObject;
                    if (txtChild != null)
                    {
                        var txtRt = txtChild.GetComponent<RectTransform>();
                        txtRt.anchorMin = new Vector2(0.5f, 0.5f);
                        txtRt.anchorMax = new Vector2(0.5f, 0.5f);
                        txtRt.pivot = new Vector2(0.5f, 0.5f);
                        txtRt.anchoredPosition = new Vector2(18f, 0f);
                        txtRt.sizeDelta = new Vector2(115f, 60f);

                        var tmp = txtChild.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.fontSize = 36;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = Color.white;
                            tmp.text = "1.945";
                        }
                    }

                    // Right Plus Button
                    var addBtnObj = goldWgtObj.transform.Find("AddBtn")?.gameObject;
                    if (addBtnObj == null)
                    {
                        addBtnObj = new GameObject("AddBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                        addBtnObj.transform.SetParent(goldWgtObj.transform, false);
                    }
                    var addRt = addBtnObj.GetComponent<RectTransform>();
                    addRt.anchorMin = new Vector2(1f, 0.5f);
                    addRt.anchorMax = new Vector2(1f, 0.5f);
                    addRt.pivot = new Vector2(0.5f, 0.5f);
                    addRt.anchoredPosition = new Vector2(-40f, 0f);
                    addRt.sizeDelta = new Vector2(62f, 62f);

                    var addImg = addBtnObj.GetComponent<Image>();
                    addImg.sprite = plusIconSprite;
                    addImg.color = Color.white;
                }

                // LivesWidget
                var livesWgtObj = topBarObj.transform.Find("LivesWidget")?.gameObject;
                if (livesWgtObj != null)
                {
                    var rt = livesWgtObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(145f, -15f);
                    rt.sizeDelta = new Vector2(265f, 92f);

                    var lImg = livesWgtObj.GetComponent<Image>();
                    if (lImg != null && pillEmptySprite != null)
                    {
                        lImg.sprite = pillEmptySprite;
                        lImg.color = Color.white;
                        lImg.type = Image.Type.Simple;
                    }

                    // Left Lightning Icon
                    var iconChild = livesWgtObj.transform.Find("Icon")?.gameObject;
                    if (iconChild != null)
                    {
                        iconChild.SetActive(true);
                        var iRt = iconChild.GetComponent<RectTransform>();
                        iRt.anchorMin = new Vector2(0f, 0.5f);
                        iRt.anchorMax = new Vector2(0f, 0.5f);
                        iRt.pivot = new Vector2(0.5f, 0.5f);
                        iRt.anchoredPosition = new Vector2(40f, 0f);
                        iRt.sizeDelta = new Vector2(62f, 72f);

                        var iImg = iconChild.GetComponent<Image>();
                        if (iImg == null) iImg = iconChild.AddComponent<Image>();
                        iImg.sprite = lightningIconSprite;
                        iImg.color = Color.white;
                    }

                    // Lives Text TMP
                    var txtChild = livesWgtObj.transform.Find("Text")?.gameObject;
                    if (txtChild != null)
                    {
                        var txtRt = txtChild.GetComponent<RectTransform>();
                        txtRt.anchorMin = new Vector2(0.5f, 0.5f);
                        txtRt.anchorMax = new Vector2(0.5f, 0.5f);
                        txtRt.pivot = new Vector2(0.5f, 0.5f);
                        txtRt.anchoredPosition = new Vector2(18f, 0f);
                        txtRt.sizeDelta = new Vector2(115f, 60f);

                        var tmp = txtChild.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.fontSize = 36;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = Color.white;
                            tmp.text = "5/5";
                        }
                    }

                    // Right Plus Button
                    var addBtnChild = livesWgtObj.transform.Find("AddBtn")?.gameObject;
                    if (addBtnChild != null)
                    {
                        var addRt = addBtnChild.GetComponent<RectTransform>();
                        addRt.anchorMin = new Vector2(1f, 0.5f);
                        addRt.anchorMax = new Vector2(1f, 0.5f);
                        addRt.pivot = new Vector2(0.5f, 0.5f);
                        addRt.anchoredPosition = new Vector2(-40f, 0f);
                        addRt.sizeDelta = new Vector2(62f, 62f);

                        var addImg = addBtnChild.GetComponent<Image>();
                        if (addImg != null)
                        {
                            addImg.sprite = plusIconSprite;
                            addImg.color = Color.white;
                        }
                        var addTxt = addBtnChild.transform.Find("Text")?.gameObject;
                        if (addTxt != null) addTxt.SetActive(false);
                    }
                }
            }

            // 4. Center Battle Area (BattlePage)
            var battlePageObj = canvasObj.transform.Find("PagesRoot/BattlePage")?.gameObject;
            if (battlePageObj != null)
            {
                // Deactivate left and right offers to reveal characters artwork
                var offL = battlePageObj.transform.Find("Offer_Left")?.gameObject;
                if (offL != null) offL.SetActive(false);
                var offR = battlePageObj.transform.Find("Offer_Right")?.gameObject;
                if (offR != null) offR.SetActive(false);

                // Stage Container Card (LevelCard)
                var levelCardObj = battlePageObj.transform.Find("LevelCard")?.gameObject;
                if (levelCardObj != null)
                {
                    var lcRt = levelCardObj.GetComponent<RectTransform>();
                    lcRt.anchorMin = new Vector2(0.5f, 0f);
                    lcRt.anchorMax = new Vector2(0.5f, 0f);
                    lcRt.pivot = new Vector2(0.5f, 0f);
                    lcRt.anchoredPosition = new Vector2(0f, 240f);
                    lcRt.sizeDelta = new Vector2(800f, 290f);

                    var lcImg = levelCardObj.GetComponent<Image>();
                    if (lcImg != null && stageCardSprite != null)
                    {
                        lcImg.sprite = stageCardSprite;
                        lcImg.color = Color.white;
                        lcImg.type = Image.Type.Simple;
                    }

                    var subObj = levelCardObj.transform.Find("Subtitle")?.gameObject;
                    if (subObj != null) subObj.SetActive(false);

                    var lvlTxtObj = levelCardObj.transform.Find("LevelText")?.gameObject;
                    if (lvlTxtObj != null)
                    {
                        var ltRt = lvlTxtObj.GetComponent<RectTransform>();
                        ltRt.anchorMin = new Vector2(0.5f, 1f);
                        ltRt.anchorMax = new Vector2(0.5f, 1f);
                        ltRt.pivot = new Vector2(0.5f, 1f);
                        ltRt.anchoredPosition = new Vector2(0f, -22f);
                        ltRt.sizeDelta = new Vector2(500f, 65f);

                        var tmp = lvlTxtObj.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = "Aşama 24";
                            tmp.fontSize = 46;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = Color.white;
                        }
                    }
                }

                // Play Button
                var playBtnObj = levelCardObj.transform.Find("PlayButton")?.gameObject;
                if (playBtnObj == null)
                {
                    playBtnObj = battlePageObj.transform.Find("PlayButton")?.gameObject;
                    if (playBtnObj != null) playBtnObj.transform.SetParent(levelCardObj.transform, false);
                }

                if (playBtnObj != null)
                {
                    var pbRt = playBtnObj.GetComponent<RectTransform>();
                    pbRt.anchorMin = new Vector2(0.5f, 0f);
                    pbRt.anchorMax = new Vector2(0.5f, 0f);
                    pbRt.pivot = new Vector2(0.5f, 0f);
                    pbRt.anchoredPosition = new Vector2(0f, 22f);
                    pbRt.sizeDelta = new Vector2(730f, 160f);

                    var pbImg = playBtnObj.GetComponent<Image>();
                    if (pbImg != null && playGoldSprite != null)
                    {
                        pbImg.sprite = playGoldSprite;
                        pbImg.color = Color.white;
                        pbImg.type = Image.Type.Simple;
                    }

                    var pbTxtObj = playBtnObj.transform.Find("Text")?.gameObject;
                    if (pbTxtObj != null)
                    {
                        var txtRt = pbTxtObj.GetComponent<RectTransform>();
                        txtRt.anchorMin = Vector2.zero;
                        txtRt.anchorMax = Vector2.one;
                        txtRt.offsetMin = Vector2.zero;
                        txtRt.offsetMax = Vector2.zero;

                        var tmp = pbTxtObj.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = "DEVAM ET >>";
                            tmp.fontSize = 50;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = new Color(0.04f, 0.08f, 0.16f, 1f);
                        }
                    }
                }
            }

            // 5. Bottom Navigation
            var bottomNavObj = canvasObj.transform.Find("BottomNav")?.gameObject;
            if (bottomNavObj != null)
            {
                var bnRt = bottomNavObj.GetComponent<RectTransform>();
                bnRt.anchorMin = new Vector2(0f, 0f);
                bnRt.anchorMax = new Vector2(1f, 0f);
                bnRt.pivot = new Vector2(0.5f, 0f);
                bnRt.anchoredPosition = new Vector2(0f, 15f);
                bnRt.sizeDelta = new Vector2(0f, 210f);

                var bnImg = bottomNavObj.GetComponent<Image>();
                if (bnImg != null) bnImg.color = new Color(0, 0, 0, 0);

                // ShopTab
                var shopTab = bottomNavObj.transform.Find("ShopTab")?.gameObject;
                if (shopTab != null)
                {
                    var rt = shopTab.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(-350f, 0f);
                    rt.sizeDelta = new Vector2(330f, 190f);

                    var img = shopTab.GetComponent<Image>();
                    if (img != null && shopInactiveSprite != null)
                    {
                        img.sprite = shopInactiveSprite;
                        img.color = Color.white;
                    }
                    for (int i = 0; i < shopTab.transform.childCount; i++)
                    {
                        shopTab.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // BattleTab (Ana Menü - Active)
                var battleTab = bottomNavObj.transform.Find("BattleTab")?.gameObject;
                if (battleTab != null)
                {
                    var rt = battleTab.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 10f);
                    rt.sizeDelta = new Vector2(355f, 205f);

                    var img = battleTab.GetComponent<Image>();
                    if (img != null && battleActiveSprite != null)
                    {
                        img.sprite = battleActiveSprite;
                        img.color = Color.white;
                    }
                    for (int i = 0; i < battleTab.transform.childCount; i++)
                    {
                        battleTab.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // LeaderboardTab
                var leadTab = bottomNavObj.transform.Find("LeaderboardTab")?.gameObject;
                if (leadTab != null)
                {
                    var rt = leadTab.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(350f, 0f);
                    rt.sizeDelta = new Vector2(330f, 190f);

                    var img = leadTab.GetComponent<Image>();
                    if (img != null && leadInactiveSprite != null)
                    {
                        img.sprite = leadInactiveSprite;
                        img.color = Color.white;
                    }
                    for (int i = 0; i < leadTab.transform.childCount; i++)
                    {
                        leadTab.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // Assign BottomNavigationUI sprites
                var navUI = bottomNavObj.GetComponent<BottomNavigationUI>();
                if (navUI != null)
                {
                    navUI.SetTabSprites(
                        shopActiveSprite, shopInactiveSprite,
                        battleActiveSprite, battleInactiveSprite,
                        leadActiveSprite, leadInactiveSprite
                    );
                }
            }

            // 6. Bind references on MainMenuController
            var mmc = canvasObj.GetComponent<MainMenuController>();
            if (mmc != null)
            {
                var lvlTxt = canvasObj.transform.Find("PagesRoot/BattlePage/LevelCard/LevelText")?.GetComponent<TextMeshProUGUI>();
                var playBtn = canvasObj.transform.Find("PagesRoot/BattlePage/LevelCard/PlayButton")?.GetComponent<Button>();
                var profBtn = canvasObj.transform.Find("TopBar/ProfileButton")?.GetComponent<Button>();
                var setBtn = canvasObj.transform.Find("TopBar/SettingsButton")?.GetComponent<Button>();
                var goldWgt = canvasObj.transform.Find("TopBar/GoldWidget")?.GetComponent<GoldDisplayUI>();
                var livesWgt = canvasObj.transform.Find("TopBar/LivesWidget")?.GetComponent<LivesWidgetUI>();
                var setDlg = canvasObj.transform.Find("SettingsDialog")?.GetComponent<InGameSettingsDialog>();
                var oolDlg = canvasObj.transform.Find("OutOfLivesDialog")?.GetComponent<OutOfLivesDialog>();
                var nav = bottomNavObj?.GetComponent<BottomNavigationUI>();
                var profDlg = canvasObj.transform.Find("Dialogs/ProfileDialog")?.GetComponent<ProfileDialog>();
                var usrDlg = canvasObj.transform.Find("Dialogs/UsernamePromptDialog")?.GetComponent<UsernamePromptDialog>();

                mmc.BindReferences(lvlTxt, playBtn, profBtn, setBtn, goldWgt, livesWgt, setDlg, oolDlg, nav, null, profDlg, usrDlg);
            }

            // 7. Save Scene
            EditorUtility.SetDirty(canvasObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MainMenuRevampTool] MainMenu revamp applied and scene saved successfully!");
        }
    }
}
