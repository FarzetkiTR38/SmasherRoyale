using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.UI;
using RoyalSmash.Cloud;
using RoyalSmash.Profile;

namespace RoyalSmash.EditorTools
{
    public static class LeaderboardRevampTool
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/UI/LeaderboardRow.prefab";
        private const string SlicedArtDir = "Assets/_Project/Art/Sprites/LeaderboardSliced";
        private const string FontDir = "Assets/_Project/Fonts";

        [MenuItem("Tools/RoyalSmash/Revamp Leaderboard UI")]
        public static void RevampLeaderboard()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Debug.Log("[LeaderboardRevampTool] Starting Leaderboard UI revamp...");

            // 1. Load Sprites & Fonts
            var bgHall = LoadSprite("bg_leaderboard_hall.png");
            var crestHeader = LoadSprite("crest_league_header.png");
            var ribbonSub = LoadSprite("ribbon_subtitle.png");
            var bubbleKrag = LoadSprite("bubble_krag_red.png");
            var charBoy = LoadSprite("char_hero_boy_bust.png");
            var charRobot = LoadSprite("char_robot_mascot.png");
            var charKrag = LoadSprite("char_krag_bust.png");

            var tabGold = LoadSprite("tab_btn_gold_small.png");
            var tabBlue1 = LoadSprite("tab_btn_blue_small_1.png");
            var tabBlue2 = LoadSprite("tab_btn_blue_small_2.png");

            var bannerLeft = LoadSprite("banner_vertical_left.png");
            var bannerRight = LoadSprite("banner_vertical_right.png");
            var outerFrame = LoadSprite("frame_leaderboard_outer.png");

            var cardGold = LoadSprite("card_rank_1_gold.png");
            var cardSilver = LoadSprite("card_rank_2_silver.png");
            var cardBronze = LoadSprite("card_rank_3_bronze.png");
            var cardBlue = LoadSprite("card_rank_normal_blue.png");

            var frameGold = LoadSprite("frame_avatar_gold.png");
            var frameSilver = LoadSprite("frame_avatar_silver.png");
            var frameBronze = LoadSprite("frame_avatar_bronze.png");
            var frameBlue = LoadSprite("frame_avatar_blue.png");
            var frameCyber = LoadSprite("frame_avatar_cyber.png");

            var avBoy = LoadSprite("avatar_boy_head.png");
            var avRobot = LoadSprite("avatar_robot_head.png");
            var avGirl = LoadSprite("avatar_girl_head.png");
            var avKrag = LoadSprite("avatar_krag_head.png");
            var circleMask = LoadSprite("mask_circle_solid.png");

            var fontYellow = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/Fazo_Font_Titles_yellow.asset");
            var fontWhite = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/Fazo_Font_Titles_white.asset");
            var fontBlue = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/Fazo_Font_Titles_blue.asset");
            var fontOrange = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/Fazo_Font_Orangeoutline.asset");

            // 2. Update ProfileConfig with HD avatars and frames
            UpdateProfileConfig(avBoy, avRobot, avGirl, avKrag, frameGold, frameSilver, frameBronze, frameBlue);

            // 3. Update LeaderboardRow Prefab using PrefabUtility.LoadPrefabContents
            UpdateRowPrefab(cardGold, cardSilver, cardBronze, cardBlue, circleMask, frameGold, fontWhite, fontYellow, fontOrange);

            var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            // 4. Build LeaderboardPage in MainMenuCanvas
            BuildLeaderboardPage(
                bgHall, crestHeader, ribbonSub, bubbleKrag,
                charBoy, charRobot, charKrag,
                tabGold, tabBlue1, tabBlue2,
                bannerLeft, bannerRight, outerFrame,
                fontYellow, fontWhite, fontBlue,
                rowPrefab);

            // 5. Update Bottom Navigation visuals for Leaderboard tab
            UpdateBottomNav(
                LoadSprite("nav_btn_gold_selected.png"),
                LoadSprite("nav_btn_blue_normal.png"),
                LoadSprite("icon_trophy_gold.png"),
                LoadSprite("icon_nav_home.png"),
                LoadSprite("icon_nav_shop.png"),
                fontWhite);

            // 6. Save scene and assets
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[LeaderboardRevampTool] Leaderboard UI revamp completed successfully!");
        }

        private static Sprite LoadSprite(string filename)
        {
            string path = $"{SlicedArtDir}/{filename}";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void UpdateProfileConfig(Sprite avBoy, Sprite avRobot, Sprite avGirl, Sprite avKrag,
            Sprite frameGold, Sprite frameSilver, Sprite frameBronze, Sprite frameBlue)
        {
            var cfg = AssetDatabase.LoadAssetAtPath<ProfileConfig>("Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset");
            if (cfg == null) return;

            var so = new SerializedObject(cfg);
            var avatarsProp = so.FindProperty("_avatars");
            if (avatarsProp != null && avatarsProp.arraySize >= 4)
            {
                if (avBoy != null) avatarsProp.GetArrayElementAtIndex(0).FindPropertyRelative("Icon").objectReferenceValue = avBoy;
                if (avRobot != null) avatarsProp.GetArrayElementAtIndex(1).FindPropertyRelative("Icon").objectReferenceValue = avRobot;
                if (avGirl != null) avatarsProp.GetArrayElementAtIndex(2).FindPropertyRelative("Icon").objectReferenceValue = avGirl;
                if (avKrag != null) avatarsProp.GetArrayElementAtIndex(3).FindPropertyRelative("Icon").objectReferenceValue = avKrag;
            }

            var framesProp = so.FindProperty("_frames");
            if (framesProp != null && framesProp.arraySize >= 4)
            {
                if (frameGold != null) framesProp.GetArrayElementAtIndex(0).FindPropertyRelative("BorderSprite").objectReferenceValue = frameGold;
                if (frameSilver != null) framesProp.GetArrayElementAtIndex(1).FindPropertyRelative("BorderSprite").objectReferenceValue = frameSilver;
                if (frameBronze != null) framesProp.GetArrayElementAtIndex(2).FindPropertyRelative("BorderSprite").objectReferenceValue = frameBronze;
                if (frameBlue != null) framesProp.GetArrayElementAtIndex(3).FindPropertyRelative("BorderSprite").objectReferenceValue = frameBlue;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(cfg);
        }

        private static void UpdateRowPrefab(Sprite cardGold, Sprite cardSilver, Sprite cardBronze, Sprite cardBlue,
            Sprite circleMask, Sprite defaultFrame, TMP_FontAsset fontWhite, TMP_FontAsset fontYellow, TMP_FontAsset fontOrange)
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(root.transform.GetChild(i).gameObject, true);
                }

                var rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(880, 126);

                var bgImg = root.GetComponent<Image>();
                if (bgImg == null) bgImg = root.AddComponent<Image>();
                bgImg.sprite = cardBlue;
                bgImg.type = Image.Type.Simple;
                bgImg.color = Color.white;

                var outline = root.GetComponent<Outline>();
                if (outline == null) outline = root.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.9f);
                outline.effectDistance = new Vector2(2, -2);
                outline.enabled = false;

                // 1. Rank Text (Centered inside laurel wreath / rank space at x=81.5)
                var rankGo = new GameObject("RankText", typeof(RectTransform), typeof(TextMeshProUGUI));
                rankGo.transform.SetParent(root.transform, false);
                var rankRect = rankGo.GetComponent<RectTransform>();
                rankRect.anchorMin = new Vector2(0, 0.5f);
                rankRect.anchorMax = new Vector2(0, 0.5f);
                rankRect.pivot = new Vector2(0.5f, 0.5f);
                rankRect.anchoredPosition = new Vector2(81.5f, 0);
                rankRect.sizeDelta = new Vector2(60, 60);
                var rankTmp = rankGo.GetComponent<TextMeshProUGUI>();
                rankTmp.font = fontWhite;
                rankTmp.fontSize = 36;
                rankTmp.fontStyle = FontStyles.Bold;
                rankTmp.alignment = TextAlignmentOptions.Center;
                rankTmp.text = "1";

                // 2. Avatar Container (Centered inside avatar ring at x=171.5)
                var avRoot = new GameObject("AvatarRoot", typeof(RectTransform));
                avRoot.transform.SetParent(root.transform, false);
                var avRootRect = avRoot.GetComponent<RectTransform>();
                avRootRect.anchorMin = new Vector2(0, 0.5f);
                avRootRect.anchorMax = new Vector2(0, 0.5f);
                avRootRect.pivot = new Vector2(0.5f, 0.5f);
                avRootRect.anchoredPosition = new Vector2(171.5f, 0);
                avRootRect.sizeDelta = new Vector2(88, 88);

                // Circular mask (centered, 68x68 so it sits perfectly inside the frame ring)
                var maskGo = new GameObject("Mask", typeof(RectTransform), typeof(Image), typeof(Mask));
                maskGo.transform.SetParent(avRoot.transform, false);
                var maskRect = maskGo.GetComponent<RectTransform>();
                maskRect.anchorMin = new Vector2(0.5f, 0.5f);
                maskRect.anchorMax = new Vector2(0.5f, 0.5f);
                maskRect.pivot = new Vector2(0.5f, 0.5f);
                maskRect.anchoredPosition = Vector2.zero;
                maskRect.sizeDelta = new Vector2(68, 68);
                var maskImg = maskGo.GetComponent<Image>();
                maskImg.sprite = circleMask;
                maskGo.GetComponent<Mask>().showMaskGraphic = false;

                // Avatar Icon inside mask
                var avIconGo = new GameObject("AvatarIcon", typeof(RectTransform), typeof(Image));
                avIconGo.transform.SetParent(maskGo.transform, false);
                var avIconRect = avIconGo.GetComponent<RectTransform>();
                avIconRect.anchorMin = Vector2.zero;
                avIconRect.anchorMax = Vector2.one;
                avIconRect.sizeDelta = Vector2.zero;
                var avIconImg = avIconGo.GetComponent<Image>();
                avIconImg.preserveAspect = true;

                // Frame Border on top of mask (98x98 so it neatly frames the face)
                var frameGo = new GameObject("FrameBorder", typeof(RectTransform), typeof(Image));
                frameGo.transform.SetParent(avRoot.transform, false);
                var frameRect = frameGo.GetComponent<RectTransform>();
                frameRect.anchorMin = new Vector2(0.5f, 0.5f);
                frameRect.anchorMax = new Vector2(0.5f, 0.5f);
                frameRect.pivot = new Vector2(0.5f, 0.5f);
                frameRect.anchoredPosition = Vector2.zero;
                frameRect.sizeDelta = new Vector2(98, 98);
                var frameImg = frameGo.GetComponent<Image>();
                frameImg.sprite = defaultFrame;
                frameImg.preserveAspect = true;

                // 3. Username Text (Starts cleanly at x=235 with left pivot)
                var userGo = new GameObject("UsernameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                userGo.transform.SetParent(root.transform, false);
                var userRect = userGo.GetComponent<RectTransform>();
                userRect.anchorMin = new Vector2(0, 0.5f);
                userRect.anchorMax = new Vector2(0, 0.5f);
                userRect.pivot = new Vector2(0, 0.5f); // Left pivot so text starts at x=235
                userRect.anchoredPosition = new Vector2(235, 0);
                userRect.sizeDelta = new Vector2(430, 52);
                var userTmp = userGo.GetComponent<TextMeshProUGUI>();
                userTmp.font = fontWhite;
                userTmp.fontSize = 32;
                userTmp.fontStyle = FontStyles.Bold;
                userTmp.alignment = TextAlignmentOptions.MidlineLeft;
                userTmp.color = Color.white;
                userTmp.text = "PlayerName";

                // 4. Stage Badge Container (Centered in right pill at x=-134 from right)
                var stageGo = new GameObject("StageBadge", typeof(RectTransform));
                stageGo.transform.SetParent(root.transform, false);
                var stageRect = stageGo.GetComponent<RectTransform>();
                stageRect.anchorMin = new Vector2(1, 0.5f);
                stageRect.anchorMax = new Vector2(1, 0.5f);
                stageRect.pivot = new Vector2(0.5f, 0.5f);
                stageRect.anchoredPosition = new Vector2(-134, 0);
                stageRect.sizeDelta = new Vector2(180, 75);

                var labelGo = new GameObject("StageLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(stageGo.transform, false);
                var labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 0.5f);
                labelRect.anchorMax = new Vector2(1, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(0, 16);
                labelRect.sizeDelta = new Vector2(0, 26);
                var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
                labelTmp.font = fontOrange != null ? fontOrange : fontWhite;
                labelTmp.fontSize = 19;
                labelTmp.fontStyle = FontStyles.Bold;
                labelTmp.alignment = TextAlignmentOptions.Center;
                labelTmp.color = new Color(1f, 0.90f, 0.65f);
                labelTmp.text = "Aşama";

                var valGo = new GameObject("StageValue", typeof(RectTransform), typeof(TextMeshProUGUI));
                valGo.transform.SetParent(stageGo.transform, false);
                var valRect = valGo.GetComponent<RectTransform>();
                valRect.anchorMin = new Vector2(0, 0.5f);
                valRect.anchorMax = new Vector2(1, 0.5f);
                valRect.pivot = new Vector2(0.5f, 0.5f);
                valRect.anchoredPosition = new Vector2(0, -14);
                valRect.sizeDelta = new Vector2(0, 38);
                var valTmp = valGo.GetComponent<TextMeshProUGUI>();
                valTmp.font = fontYellow != null ? fontYellow : fontWhite;
                valTmp.fontSize = 36;
                valTmp.fontStyle = FontStyles.Bold;
                valTmp.alignment = TextAlignmentOptions.Center;
                valTmp.color = new Color(1f, 0.96f, 0.45f);
                valTmp.text = "33709";

                var rowUI = root.GetComponent<LeaderboardEntryRowUI>();
                if (rowUI == null) rowUI = root.AddComponent<LeaderboardEntryRowUI>();
                rowUI.SetCardSprites(cardGold, cardSilver, cardBronze, cardBlue);
                rowUI.BindReferences(bgImg, outline, rankTmp, avIconImg, frameImg, userTmp, valTmp, labelTmp);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildLeaderboardPage(
            Sprite bgHall, Sprite crestHeader, Sprite ribbonSub, Sprite bubbleKrag,
            Sprite charBoy, Sprite charRobot, Sprite charKrag,
            Sprite tabGold, Sprite tabBlue1, Sprite tabBlue2,
            Sprite bannerLeft, Sprite bannerRight, Sprite outerFrame,
            TMP_FontAsset fontYellow, TMP_FontAsset fontWhite, TMP_FontAsset fontBlue,
            GameObject rowPrefab)
        {
            var pagesRoot = GameObject.Find("MainMenuCanvas/PagesRoot");
            if (pagesRoot == null) return;

            var lbPage = pagesRoot.transform.Find("LeaderboardPage")?.gameObject;
            if (lbPage == null)
            {
                lbPage = new GameObject("LeaderboardPage", typeof(RectTransform), typeof(LeaderboardUI));
                lbPage.transform.SetParent(pagesRoot.transform, false);
            }

            var lbRect = lbPage.GetComponent<RectTransform>();
            lbRect.anchorMin = Vector2.zero;
            lbRect.anchorMax = Vector2.one;
            lbRect.sizeDelta = Vector2.zero;
            lbRect.anchoredPosition = Vector2.zero;

            for (int i = lbPage.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(lbPage.transform.GetChild(i).gameObject);
            }

            // 1. Background (Grand Hall)
            var bgGo = new GameObject("Background_Hall", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(lbPage.transform, false);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = bgHall;
            bgImg.preserveAspect = false;

            // 2. Left Side Vertical Banner ("KEŞİF DAHA DAİMA İLERİ")
            var bannerLeftGo = new GameObject("Banner_Left", typeof(RectTransform), typeof(Image));
            bannerLeftGo.transform.SetParent(lbPage.transform, false);
            var bLeftRect = bannerLeftGo.GetComponent<RectTransform>();
            bLeftRect.anchorMin = new Vector2(0, 0.5f);
            bLeftRect.anchorMax = new Vector2(0, 0.5f);
            bLeftRect.pivot = new Vector2(0, 0.5f);
            bLeftRect.anchoredPosition = new Vector2(10, -90);
            bLeftRect.sizeDelta = new Vector2(105, 680);
            var bLeftImg = bannerLeftGo.GetComponent<Image>();
            bLeftImg.sprite = bannerLeft;
            bLeftImg.preserveAspect = true;

            var txtLeftGo = new GameObject("BannerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtLeftGo.transform.SetParent(bannerLeftGo.transform, false);
            var txtLeftRect = txtLeftGo.GetComponent<RectTransform>();
            txtLeftRect.anchorMin = new Vector2(0.5f, 0.5f);
            txtLeftRect.anchorMax = new Vector2(0.5f, 0.5f);
            txtLeftRect.sizeDelta = new Vector2(85, 450);
            txtLeftRect.anchoredPosition = new Vector2(0, -10);
            var txtLeftTmp = txtLeftGo.GetComponent<TextMeshProUGUI>();
            txtLeftTmp.font = fontWhite;
            txtLeftTmp.fontSize = 20;
            txtLeftTmp.fontStyle = FontStyles.Bold;
            txtLeftTmp.alignment = TextAlignmentOptions.Center;
            txtLeftTmp.lineSpacing = 10;
            txtLeftTmp.color = new Color(0.75f, 0.9f, 1f, 0.95f);
            txtLeftTmp.text = "KEŞİF\nDAHA\nDAİMA\nİLERİ";

            // 3. Right Side Vertical Banner ("YENİ DÜNYALAR DAHA CESUR AKINCILAR İÇİN")
            var bannerRightGo = new GameObject("Banner_Right", typeof(RectTransform), typeof(Image));
            bannerRightGo.transform.SetParent(lbPage.transform, false);
            var bRightRect = bannerRightGo.GetComponent<RectTransform>();
            bRightRect.anchorMin = new Vector2(1, 0.5f);
            bRightRect.anchorMax = new Vector2(1, 0.5f);
            bRightRect.pivot = new Vector2(1, 0.5f);
            bRightRect.anchoredPosition = new Vector2(-10, -90);
            bRightRect.sizeDelta = new Vector2(105, 680);
            var bRightImg = bannerRightGo.GetComponent<Image>();
            bRightImg.sprite = bannerRight;
            bRightImg.preserveAspect = true;

            var txtRightGo = new GameObject("BannerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtRightGo.transform.SetParent(bannerRightGo.transform, false);
            var txtRightRect = txtRightGo.GetComponent<RectTransform>();
            txtRightRect.anchorMin = new Vector2(0.5f, 0.5f);
            txtRightRect.anchorMax = new Vector2(0.5f, 0.5f);
            txtRightRect.sizeDelta = new Vector2(85, 480);
            txtRightRect.anchoredPosition = new Vector2(0, -10);
            var txtRightTmp = txtRightGo.GetComponent<TextMeshProUGUI>();
            txtRightTmp.font = fontWhite;
            txtRightTmp.fontSize = 18;
            txtRightTmp.fontStyle = FontStyles.Bold;
            txtRightTmp.alignment = TextAlignmentOptions.Center;
            txtRightTmp.lineSpacing = 8;
            txtRightTmp.color = new Color(0.75f, 0.9f, 1f, 0.95f);
            txtRightTmp.text = "YENİ\nDÜNYALAR\nDAHA\nCESUR\nAKINCILAR\nİÇİN";

            // 4. Header Section Container
            var headerGo = new GameObject("HeaderSection", typeof(RectTransform));
            headerGo.transform.SetParent(lbPage.transform, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0, -90);
            headerRect.sizeDelta = new Vector2(1080, 520);

            // 4a. Hero Boy (Top Left)
            var boyGo = new GameObject("HeroBoy", typeof(RectTransform), typeof(Image));
            boyGo.transform.SetParent(headerGo.transform, false);
            var boyRect = boyGo.GetComponent<RectTransform>();
            boyRect.anchorMin = new Vector2(0, 1);
            boyRect.anchorMax = new Vector2(0, 1);
            boyRect.pivot = new Vector2(0, 1);
            boyRect.anchoredPosition = new Vector2(5, -45);
            boyRect.sizeDelta = new Vector2(330, 480);
            var boyImg = boyGo.GetComponent<Image>();
            boyImg.sprite = charBoy;
            boyImg.preserveAspect = true;

            // 4b. Robot Mascot
            var robotGo = new GameObject("RobotMascot", typeof(RectTransform), typeof(Image));
            robotGo.transform.SetParent(headerGo.transform, false);
            var robotRect = robotGo.GetComponent<RectTransform>();
            robotRect.anchorMin = new Vector2(0, 1);
            robotRect.anchorMax = new Vector2(0, 1);
            robotRect.pivot = new Vector2(0, 1);
            robotRect.anchoredPosition = new Vector2(195, -125);
            robotRect.sizeDelta = new Vector2(130, 150);
            var robotImg = robotGo.GetComponent<Image>();
            robotImg.sprite = charRobot;
            robotImg.preserveAspect = true;

            // 4c. Krag Villain (Top Right)
            var kragGo = new GameObject("KragVillain", typeof(RectTransform), typeof(Image));
            kragGo.transform.SetParent(headerGo.transform, false);
            var kragRect = kragGo.GetComponent<RectTransform>();
            kragRect.anchorMin = new Vector2(1, 1);
            kragRect.anchorMax = new Vector2(1, 1);
            kragRect.pivot = new Vector2(1, 1);
            kragRect.anchoredPosition = new Vector2(15, -35);
            kragRect.sizeDelta = new Vector2(330, 480);
            var kragImg = kragGo.GetComponent<Image>();
            kragImg.sprite = charKrag;
            kragImg.preserveAspect = true;

            // 4d. Krag Speech Bubble
            var bubbleGo = new GameObject("KragBubble", typeof(RectTransform), typeof(Image));
            bubbleGo.transform.SetParent(headerGo.transform, false);
            var bubbleRect = bubbleGo.GetComponent<RectTransform>();
            bubbleRect.anchorMin = new Vector2(1, 1);
            bubbleRect.anchorMax = new Vector2(1, 1);
            bubbleRect.pivot = new Vector2(1, 1);
            bubbleRect.anchoredPosition = new Vector2(-10, -180);
            bubbleRect.sizeDelta = new Vector2(245, 90);
            var bubbleImg = bubbleGo.GetComponent<Image>();
            bubbleImg.sprite = bubbleKrag;
            bubbleImg.preserveAspect = true;

            var bubbleTxtGo = new GameObject("BubbleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bubbleTxtGo.transform.SetParent(bubbleGo.transform, false);
            var bTxtRect = bubbleTxtGo.GetComponent<RectTransform>();
            bTxtRect.anchorMin = Vector2.zero;
            bTxtRect.anchorMax = Vector2.one;
            bTxtRect.offsetMin = new Vector2(10, 8);
            bTxtRect.offsetMax = new Vector2(-15, -6);
            var bTxtTmp = bubbleTxtGo.GetComponent<TextMeshProUGUI>();
            bTxtTmp.font = fontWhite;
            bTxtTmp.fontSize = 20;
            bTxtTmp.fontStyle = FontStyles.Bold;
            bTxtTmp.alignment = TextAlignmentOptions.Center;
            bTxtTmp.color = Color.white;
            bTxtTmp.text = "<color=#FFDE59><b>KRAG</b></color>\n<size=75%>\"ZİRVEDE GÖRÜŞÜRÜZ!\"</size>";

            // 4e. Main League Header Crest
            var crestGo = new GameObject("LeagueCrest", typeof(RectTransform), typeof(Image));
            crestGo.transform.SetParent(headerGo.transform, false);
            var crestRect = crestGo.GetComponent<RectTransform>();
            crestRect.anchorMin = new Vector2(0.5f, 1);
            crestRect.anchorMax = new Vector2(0.5f, 1);
            crestRect.pivot = new Vector2(0.5f, 1);
            crestRect.anchoredPosition = new Vector2(0, -50);
            crestRect.sizeDelta = new Vector2(530, 275);
            var crestImg = crestGo.GetComponent<Image>();
            crestImg.sprite = crestHeader;
            crestImg.preserveAspect = true;

            // 4f. "AKINCILAR LİGİ" Title Text
            var title1Go = new GameObject("Title_Akincilar", typeof(RectTransform), typeof(TextMeshProUGUI));
            title1Go.transform.SetParent(crestGo.transform, false);
            var t1Rect = title1Go.GetComponent<RectTransform>();
            t1Rect.anchorMin = new Vector2(0.5f, 0);
            t1Rect.anchorMax = new Vector2(0.5f, 0);
            t1Rect.pivot = new Vector2(0.5f, 0.5f);
            t1Rect.anchoredPosition = new Vector2(0, 80);
            t1Rect.sizeDelta = new Vector2(460, 48);
            var t1Tmp = title1Go.GetComponent<TextMeshProUGUI>();
            t1Tmp.font = fontYellow != null ? fontYellow : fontWhite;
            t1Tmp.fontSize = 44;
            t1Tmp.fontStyle = FontStyles.Bold;
            t1Tmp.alignment = TextAlignmentOptions.Center;
            t1Tmp.color = Color.white;
            t1Tmp.text = "AKINCILAR";

            var title2Go = new GameObject("Title_Ligi", typeof(RectTransform), typeof(TextMeshProUGUI));
            title2Go.transform.SetParent(crestGo.transform, false);
            var t2Rect = title2Go.GetComponent<RectTransform>();
            t2Rect.anchorMin = new Vector2(0.5f, 0);
            t2Rect.anchorMax = new Vector2(0.5f, 0);
            t2Rect.pivot = new Vector2(0.5f, 0.5f);
            t2Rect.anchoredPosition = new Vector2(0, 38);
            t2Rect.sizeDelta = new Vector2(300, 42);
            var t2Tmp = title2Go.GetComponent<TextMeshProUGUI>();
            t2Tmp.font = fontBlue != null ? fontBlue : fontWhite;
            t2Tmp.fontSize = 38;
            t2Tmp.fontStyle = FontStyles.Bold;
            t2Tmp.alignment = TextAlignmentOptions.Center;
            t2Tmp.color = Color.white;
            t2Tmp.text = "LİGİ";

            // 4g. Subtitle Ribbon
            var subRibbonGo = new GameObject("SubtitleRibbon", typeof(RectTransform), typeof(Image));
            subRibbonGo.transform.SetParent(headerGo.transform, false);
            var subRect = subRibbonGo.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 1);
            subRect.anchorMax = new Vector2(0.5f, 1);
            subRect.pivot = new Vector2(0.5f, 1);
            subRect.anchoredPosition = new Vector2(0, -325);
            subRect.sizeDelta = new Vector2(460, 56);
            var subImg = subRibbonGo.GetComponent<Image>();
            subImg.sprite = ribbonSub;
            subImg.preserveAspect = true;

            var subTxtGo = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTxtGo.transform.SetParent(subRibbonGo.transform, false);
            var sTxtRect = subTxtGo.GetComponent<RectTransform>();
            sTxtRect.anchorMin = Vector2.zero;
            sTxtRect.anchorMax = Vector2.one;
            sTxtRect.sizeDelta = Vector2.zero;
            var sTxtTmp = subTxtGo.GetComponent<TextMeshProUGUI>();
            sTxtTmp.font = fontWhite;
            sTxtTmp.fontSize = 20;
            sTxtTmp.fontStyle = FontStyles.Bold;
            sTxtTmp.alignment = TextAlignmentOptions.Center;
            sTxtTmp.color = new Color(0.85f, 0.95f, 1f);
            sTxtTmp.text = "DAHA UZAĞA, DAHA YÜKSEĞE!";

            // 4h. Navigation Tabs Bar
            var tabsBarGo = new GameObject("TabsBar", typeof(RectTransform));
            tabsBarGo.transform.SetParent(headerGo.transform, false);
            var tabsRect = tabsBarGo.GetComponent<RectTransform>();
            tabsRect.anchorMin = new Vector2(0.5f, 1);
            tabsRect.anchorMax = new Vector2(0.5f, 1);
            tabsRect.pivot = new Vector2(0.5f, 1);
            tabsRect.anchoredPosition = new Vector2(0, -390);
            tabsRect.sizeDelta = new Vector2(920, 72);

            var tabsLayout = tabsBarGo.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 14;
            tabsLayout.childAlignment = TextAnchor.MiddleCenter;
            tabsLayout.childControlWidth = true;
            tabsLayout.childControlHeight = true;
            tabsLayout.childForceExpandWidth = true;
            tabsLayout.childForceExpandHeight = true;

            CreateTabButton(tabsBarGo.transform, "Tab_Dunya", "DÜNYA", tabGold, new Color(1f, 0.92f, 0.4f), fontWhite);
            CreateTabButton(tabsBarGo.transform, "Tab_Turkiye", "TÜRKİYE", tabBlue1, new Color(0.8f, 0.92f, 1f), fontWhite);
            CreateTabButton(tabsBarGo.transform, "Tab_Arkadaslar", "ARKADAŞLAR", tabBlue2, new Color(0.8f, 0.92f, 1f), fontWhite);

            // 5. Outer Royal Cyber Frame
            var frameGo = new GameObject("OuterFrame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(lbPage.transform, false);
            var fRect = frameGo.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0.5f);
            fRect.anchorMax = new Vector2(0.5f, 0.5f);
            fRect.pivot = new Vector2(0.5f, 0.5f);
            fRect.anchoredPosition = new Vector2(0, -195);
            fRect.sizeDelta = new Vector2(930, 990);
            var fImg = frameGo.GetComponent<Image>();
            fImg.sprite = outerFrame;
            fImg.preserveAspect = false;

            // 6. Scroll View
            var scrollGo = new GameObject("LeaderboardScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(lbPage.transform, false);
            var sRect = scrollGo.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.pivot = new Vector2(0.5f, 0.5f);
            sRect.anchoredPosition = new Vector2(0, -195);
            sRect.sizeDelta = new Vector2(890, 915);

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;

            // Viewport
            var viewGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewGo.transform.SetParent(scrollGo.transform, false);
            var viewRect = viewGo.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;

            // Content
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(4, 4, 8, 8);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewRect;
            scroll.content = contentRect;

            // 7. Wire up LeaderboardUI component
            var lbUI = lbPage.GetComponent<LeaderboardUI>();
            if (lbUI == null) lbUI = lbPage.AddComponent<LeaderboardUI>();

            var profileCfg = AssetDatabase.LoadAssetAtPath<ProfileConfig>("Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset");

            lbUI.BindReferences(
                t1Tmp,
                null,
                null,
                null,
                contentGo.transform,
                rowPrefab,
                null,
                profileCfg);

            // Pre-populate sample rows for editor preview matching mockup exactly
            var sampleEntries = new System.Collections.Generic.List<LeaderboardEntryData>
            {
                new LeaderboardEntryData(1, "p1", "MARBT", 33709, 0, 0, false),
                new LeaderboardEntryData(2, "p2", "koreakim", 27017, 1, 1, false),
                new LeaderboardEntryData(3, "p3", "RubyRush", 26665, 2, 2, false),
                new LeaderboardEntryData(4, "p4", "csy", 24846, 0, 3, false),
                new LeaderboardEntryData(5, "p5", "Bdclinic", 23656, 1, 3, false),
                new LeaderboardEntryData(6, "p6", "Meme", 23510, 3, 3, false),
                new LeaderboardEntryData(7, "p7", "QuasarQ", 23290, 0, 3, false),
                new LeaderboardEntryData(8, "p8", "ali hikmet", 22133, 2, 3, false),
            };
            lbUI.PopulateList(sampleEntries, false);
        }

        private static GameObject CreateTabButton(Transform parent, string goName, string text, Sprite sprite, Color textColor, TMP_FontAsset font)
        {
            var btnGo = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            var img = btnGo.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = false;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var tRect = txtGo.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.text = text;

            return btnGo;
        }

        private static void SetupNavTab(GameObject tabGo, string label, Sprite iconSprite, Sprite btnSprite, Color txtColor, bool isSelected, TMP_FontAsset font)
        {
            if (tabGo == null) return;

            var img = tabGo.GetComponent<Image>();
            if (img != null && btnSprite != null)
            {
                img.sprite = btnSprite;
                img.color = Color.white;
            }

            // Cleanly remove any old child elements (such as legacy text)
            for (int i = tabGo.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(tabGo.transform.GetChild(i).gameObject);
            }

            // Icon Image
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(tabGo.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0, 16);
            iconRect.sizeDelta = new Vector2(56, 56);

            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.color = isSelected ? new Color(1f, 0.95f, 0.6f) : new Color(0.75f, 0.88f, 1f);

            // Label Text
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(tabGo.transform, false);
            var txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0.5f, 0.5f);
            txtRect.anchorMax = new Vector2(0.5f, 0.5f);
            txtRect.pivot = new Vector2(0.5f, 0.5f);
            txtRect.anchoredPosition = new Vector2(0, -30);
            txtRect.sizeDelta = new Vector2(250, 30);

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = 21;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = txtColor;
            tmp.text = label;
        }

        private static void UpdateBottomNav(Sprite navSelectedGold, Sprite navNormalBlue, Sprite trophyIcon, Sprite homeIcon, Sprite shopIcon, TMP_FontAsset font)
        {
            var bottomNav = GameObject.Find("MainMenuCanvas/BottomNav");
            if (bottomNav == null) return;

            var shopTab = bottomNav.transform.Find("ShopTab")?.gameObject;
            var battleTab = bottomNav.transform.Find("BattleTab")?.gameObject;
            var leadTab = bottomNav.transform.Find("LeaderboardTab")?.gameObject;

            SetupNavTab(shopTab, "MAĞAZA", shopIcon, navNormalBlue, new Color(0.75f, 0.88f, 1f), false, font);
            SetupNavTab(battleTab, "ANA MENÜ", homeIcon, navNormalBlue, new Color(0.75f, 0.88f, 1f), false, font);
            SetupNavTab(leadTab, "SIRALAMA", trophyIcon, navSelectedGold, new Color(1f, 0.92f, 0.4f), true, font);

            var navUI = bottomNav.GetComponent<BottomNavigationUI>();
            if (navUI != null)
            {
                navUI.SetTabSprites(
                    navSelectedGold, navNormalBlue,
                    navSelectedGold, navNormalBlue,
                    navSelectedGold, navNormalBlue);
                navUI.SwitchTab(NavigationTab.Leaderboard);
            }
        }
    }
}
