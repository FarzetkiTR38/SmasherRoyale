using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Profile;
using RoyalSmash.Audio;
using RoyalSmash.Utils;
using RoyalSmash.UI;
using RoyalSmash.Save;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automation script for setting up Phase 12 (Profile & Settings).
    /// Creates 8 avatar textures, 4 frame textures, ProfileConfig ScriptableObject,
    /// ProfileDialog & UsernamePromptDialog in MainMenu.unity, and updates InGameSettingsDialog in both scenes.
    /// </summary>
    public static class Phase12Setup
    {
        private const string PROFILE_TEX_DIR = "Assets/_Project/Textures/Profile";
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset";
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MAIN_GAME_SCENE = "Assets/_Project/Scenes/MainGame.unity";

        [MenuItem("RoyalSmash/Phase 12/Setup Profile and Settings")]
        public static void RunFullSetup()
        {
            Debug.Log("=== STARTING PHASE 12 AUTOMATED SETUP ===");

            // 1. Generate Textures
            GenerateProfileTextures();

            // 2. Create ProfileConfig Asset
            ProfileConfig config = CreateOrUpdateProfileConfig();

            // 3. Setup MainMenu scene
            SetupMainMenuScene(config);

            // 4. Setup MainGame scene
            SetupMainGameScene();

            // Return to MainMenu
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== PHASE 12 AUTOMATED SETUP COMPLETED SUCCESSFULLY ===");
        }

        private static void GenerateProfileTextures()
        {
            if (!Directory.Exists(PROFILE_TEX_DIR))
            {
                Directory.CreateDirectory(PROFILE_TEX_DIR);
            }

            Color[] avatarColors = new Color[]
            {
                new Color(0.25f, 0.55f, 0.95f), // Knight (Royal Blue)
                new Color(0.20f, 0.75f, 0.35f), // Archer (Forest Green)
                new Color(0.95f, 0.75f, 0.15f), // King (Imperial Gold)
                new Color(0.80f, 0.30f, 0.75f), // Queen (Regal Purple)
                new Color(0.55f, 0.25f, 0.85f), // Wizard (Mystic Violet)
                new Color(0.95f, 0.40f, 0.60f), // Princess (Rose Pink)
                new Color(0.15f, 0.70f, 0.75f), // Rogue (Teal Jade)
                new Color(0.90f, 0.45f, 0.15f)  // Blacksmith (Fiery Amber)
            };

            for (int i = 0; i < 8; i++)
            {
                string path = $"{PROFILE_TEX_DIR}/Avatar_{i}.png";
                if (!File.Exists(path))
                {
                    Texture2D tex = CreateAvatarTexture(avatarColors[i], i);
                    byte[] png = tex.EncodeToPNG();
                    File.WriteAllBytes(path, png);
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }

            Color[] frameColors = new Color[]
            {
                new Color(0.70f, 0.45f, 0.25f), // Bronze
                new Color(0.80f, 0.85f, 0.90f), // Silver
                new Color(1.00f, 0.85f, 0.20f), // Gold
                new Color(0.65f, 0.20f, 0.90f)  // Amethyst
            };

            for (int i = 0; i < 4; i++)
            {
                string path = $"{PROFILE_TEX_DIR}/Frame_{i}.png";
                if (!File.Exists(path))
                {
                    Texture2D tex = CreateFrameTexture(frameColors[i], i);
                    byte[] png = tex.EncodeToPNG();
                    File.WriteAllBytes(path, png);
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }

            AssetDatabase.Refresh();

            // Configure texture import settings as UI Sprites
            for (int i = 0; i < 8; i++)
            {
                ConfigureSpriteImporter($"{PROFILE_TEX_DIR}/Avatar_{i}.png");
            }
            for (int i = 0; i < 4; i++)
            {
                ConfigureSpriteImporter($"{PROFILE_TEX_DIR}/Frame_{i}.png");
            }
        }

        private static void ConfigureSpriteImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreateAvatarTexture(Color baseColor, int index)
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.45f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        // Inner circle gradient
                        float t = dist / radius;
                        Color c = Color.Lerp(baseColor, baseColor * 0.7f, t);

                        // Highlight crest in top half
                        if (y > size * 0.55f && Mathf.Abs(x - center.x) < radius * 0.5f)
                        {
                            c = Color.Lerp(c, Color.white, 0.25f);
                        }

                        // Border outline
                        if (dist > radius - 4f)
                        {
                            c = Color.white;
                        }

                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateFrameTexture(Color borderColor, int index)
        {
            int size = 144;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float outerRadius = size * 0.48f;
            float innerRadius = size * 0.38f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= outerRadius && dist >= innerRadius)
                    {
                        float t = (dist - innerRadius) / (outerRadius - innerRadius);
                        Color c = Color.Lerp(borderColor * 1.2f, borderColor * 0.8f, t);

                        // Corner stud decorations at diagonals
                        float angle = Mathf.Atan2(y - center.y, x - center.x);
                        if (Mathf.Abs(Mathf.Sin(angle * 4f)) > 0.92f)
                        {
                            c = Color.Lerp(c, Color.white, 0.6f);
                        }

                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static ProfileConfig CreateOrUpdateProfileConfig()
        {
            string dir = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            ProfileConfig config = AssetDatabase.LoadAssetAtPath<ProfileConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ProfileConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            string[] avatarNames = new string[] { "Knight", "Archer", "King", "Queen", "Wizard", "Princess", "Rogue", "Blacksmith" };
            Color[] avatarColors = new Color[]
            {
                new Color(0.25f, 0.55f, 0.95f),
                new Color(0.20f, 0.75f, 0.35f),
                new Color(0.95f, 0.75f, 0.15f),
                new Color(0.80f, 0.30f, 0.75f),
                new Color(0.55f, 0.25f, 0.85f),
                new Color(0.95f, 0.40f, 0.60f),
                new Color(0.15f, 0.70f, 0.75f),
                new Color(0.90f, 0.45f, 0.15f)
            };

            for (int i = 0; i < 8; i++)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{PROFILE_TEX_DIR}/Avatar_{i}.png");
                config.Avatars[i] = new AvatarData
                {
                    Id = i,
                    Name = avatarNames[i],
                    ThemeColor = avatarColors[i],
                    Icon = s
                };
            }

            string[] frameNames = new string[] { "Bronze", "Silver", "Gold", "Amethyst" };
            Color[] frameColors = new Color[]
            {
                new Color(0.70f, 0.45f, 0.25f),
                new Color(0.80f, 0.85f, 0.90f),
                new Color(1.00f, 0.85f, 0.20f),
                new Color(0.65f, 0.20f, 0.90f)
            };

            for (int i = 0; i < 4; i++)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>($"{PROFILE_TEX_DIR}/Frame_{i}.png");
                config.Frames[i] = new FrameData
                {
                    Id = i,
                    Name = frameNames[i],
                    BorderColor = frameColors[i],
                    BorderSprite = s
                };
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static void SetupMainMenuScene(ProfileConfig config)
        {
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            // 1. Ensure Managers exist
            EnsureManagers(config);

            // 2. Find MainCanvas
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Phase12Setup] MainCanvas not found in MainMenu.unity!");
                return;
            }

            Transform dialogsRoot = canvas.transform.Find("Dialogs");
            if (dialogsRoot == null)
            {
                var go = new GameObject("Dialogs", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                dialogsRoot = go.transform;
            }

            // 3. Create or update ProfileDialog
            ProfileDialog profileDlg = BuildProfileDialog(dialogsRoot, config);

            // 4. Create or update UsernamePromptDialog
            UsernamePromptDialog userPromptDlg = BuildUsernamePromptDialog(dialogsRoot);

            // 5. Update InGameSettingsDialog with Vibration toggle
            InGameSettingsDialog settingsDlg = UpdateSettingsDialog(dialogsRoot);

            // 6. Connect MainMenuController references
            MainMenuController mmc = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            if (mmc != null)
            {
                Transform topBar = canvas.transform.Find("SafeArea/TopBar") ?? canvas.transform.Find("TopBar");
                Button profileBtn = topBar != null ? topBar.Find("ProfileButton")?.GetComponent<Button>() : null;
                Button settingsBtn = topBar != null ? topBar.Find("SettingsButton")?.GetComponent<Button>() : null;
                GoldDisplayUI goldWgt = topBar != null ? topBar.Find("GoldWidget")?.GetComponent<GoldDisplayUI>() : null;
                LivesWidgetUI livesWgt = topBar != null ? topBar.Find("LivesWidget")?.GetComponent<LivesWidgetUI>() : null;

                Transform centerStage = canvas.transform.Find("SafeArea/CenterStage") ?? canvas.transform.Find("CenterStage");
                TextMeshProUGUI levelTxt = centerStage != null ? centerStage.Find("CurrentLevelBadge/LevelNumber")?.GetComponent<TextMeshProUGUI>() : null;
                Button playBtn = centerStage != null ? centerStage.Find("PlayButton")?.GetComponent<Button>() : null;

                OutOfLivesDialog outOfLivesDlg = dialogsRoot.Find("OutOfLivesDialog")?.GetComponent<OutOfLivesDialog>();
                BottomNavigationUI nav = UnityEngine.Object.FindFirstObjectByType<BottomNavigationUI>();

                // Configure Profile Button visuals inside TopBar
                if (profileBtn != null)
                {
                    ConfigureProfileButtonHierarchy(profileBtn.transform, config);
                }

                mmc.BindReferences(levelTxt, playBtn, profileBtn, settingsBtn,
                    goldWgt, livesWgt, settingsDlg, outOfLivesDlg, nav, null,
                    profileDlg, userPromptDlg);

                EditorUtility.SetDirty(mmc);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Phase12Setup] MainMenu.unity configured successfully.");
        }

        private static void SetupMainGameScene()
        {
            EditorSceneManager.OpenScene(MAIN_GAME_SCENE);

            // Ensure AudioManager & VibrationManager
            EnsureManagers(AssetDatabase.LoadAssetAtPath<ProfileConfig>(CONFIG_PATH));

            // Update InGameSettingsDialog
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform dialogsRoot = canvas.transform.Find("Dialogs") ?? canvas.transform;
                UpdateSettingsDialog(dialogsRoot);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Phase12Setup] MainGame.unity configured successfully.");
        }

        private static void EnsureManagers(ProfileConfig config)
        {
            // AudioManager
            AudioManager am = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            if (am == null)
            {
                var go = new GameObject("AudioManager");
                am = go.AddComponent<AudioManager>();
            }

            // VibrationManager
            VibrationManager vm = UnityEngine.Object.FindFirstObjectByType<VibrationManager>();
            if (vm == null)
            {
                var go = new GameObject("VibrationManager");
                vm = go.AddComponent<VibrationManager>();
            }

            // ProfileManager
            ProfileManager pm = UnityEngine.Object.FindFirstObjectByType<ProfileManager>();
            if (pm == null)
            {
                var go = new GameObject("ProfileManager");
                pm = go.AddComponent<ProfileManager>();
            }
            if (pm != null && config != null)
            {
                pm.SetConfig(config);
                EditorUtility.SetDirty(pm);
            }
        }

        private static void ConfigureProfileButtonHierarchy(Transform profileBtnTrans, ProfileConfig config)
        {
            // Ensure avatar and frame child images exist for dynamic styling
            Transform iconTrans = profileBtnTrans.Find("AvatarIcon");
            if (iconTrans == null)
            {
                var go = new GameObject("AvatarIcon", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(profileBtnTrans, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                iconTrans = go.transform;
            }

            Transform frameTrans = profileBtnTrans.Find("FrameBorder");
            if (frameTrans == null)
            {
                var go = new GameObject("FrameBorder", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(profileBtnTrans, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = new Vector2(10, 10);
                frameTrans = go.transform;
            }

            if (config != null)
            {
                var iconImg = iconTrans.GetComponent<Image>();
                var frameImg = frameTrans.GetComponent<Image>();
                if (iconImg != null && config.Avatars.Length > 0)
                {
                    iconImg.sprite = config.Avatars[0].Icon;
                    iconImg.color = config.Avatars[0].ThemeColor;
                }
                if (frameImg != null && config.Frames.Length > 0)
                {
                    frameImg.sprite = config.Frames[0].BorderSprite;
                    frameImg.color = config.Frames[0].BorderColor;
                }
            }
        }

        private static ProfileDialog BuildProfileDialog(Transform parent, ProfileConfig config)
        {
            Transform existing = parent.Find("ProfileDialog");
            GameObject dlgGo;
            if (existing != null)
            {
                dlgGo = existing.gameObject;
            }
            else
            {
                dlgGo = new GameObject("ProfileDialog", typeof(RectTransform));
                dlgGo.transform.SetParent(parent, false);
            }

            RectTransform rootRt = dlgGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            ProfileDialog pd = dlgGo.GetComponent<ProfileDialog>() ?? dlgGo.AddComponent<ProfileDialog>();

            // Darkened backdrop overlay
            Transform bgTrans = dlgGo.transform.Find("DarkOverlay");
            if (bgTrans == null)
            {
                var bgGo = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(dlgGo.transform, false);
                var rt = bgGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                var img = bgGo.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.75f);
                bgTrans = bgGo.transform;
            }

            // Center Panel
            Transform panelTrans = dlgGo.transform.Find("CenterPanel");
            if (panelTrans == null)
            {
                var pGo = new GameObject("CenterPanel", typeof(RectTransform), typeof(Image));
                pGo.transform.SetParent(dlgGo.transform, false);
                var rt = pGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(480, 720);
                var img = pGo.GetComponent<Image>();
                img.color = new Color(0.12f, 0.14f, 0.20f, 0.98f);
                panelTrans = pGo.transform;
            }

            // Header Title
            TextMeshProUGUI titleTxt = CreateOrGetTMP(panelTrans, "TitleText", "PLAYER PROFILE", 28, new Vector2(0, 310), new Vector2(400, 50));
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(1f, 0.85f, 0.3f);

            // Close button
            Button closeBtn = CreateOrGetButton(panelTrans, "CloseButton", "X", new Vector2(200, 315), new Vector2(44, 44), new Color(0.8f, 0.25f, 0.25f));

            // Current Avatar preview with Frame
            Transform prevRoot = panelTrans.Find("AvatarPreviewRoot");
            if (prevRoot == null)
            {
                var pr = new GameObject("AvatarPreviewRoot", typeof(RectTransform));
                pr.transform.SetParent(panelTrans, false);
                var rt = pr.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-120, 200);
                rt.sizeDelta = new Vector2(110, 110);
                prevRoot = pr.transform;
            }

            Image curAvatarImg = CreateOrGetImage(prevRoot, "CurrentAvatar", config?.GetAvatar(0)?.Icon, new Vector2(90, 90));
            Image curFrameImg = CreateOrGetImage(prevRoot, "CurrentFrame", config?.GetFrame(0)?.BorderSprite, new Vector2(110, 110));

            // Username display and edit
            TextMeshProUGUI userTxt = CreateOrGetTMP(panelTrans, "UsernameDisplay", "Player", 24, new Vector2(60, 220), new Vector2(220, 40));
            userTxt.alignment = TextAlignmentOptions.MidlineLeft;
            userTxt.fontStyle = FontStyles.Bold;

            Button editBtn = CreateOrGetButton(panelTrans, "EditUsernameBtn", "EDIT", new Vector2(160, 220), new Vector2(60, 32), new Color(0.25f, 0.55f, 0.85f));
            Button saveBtn = CreateOrGetButton(panelTrans, "SaveUsernameBtn", "SAVE", new Vector2(160, 220), new Vector2(60, 32), new Color(0.2f, 0.75f, 0.3f));
            saveBtn.gameObject.SetActive(false);

            TMP_InputField userInp = CreateOrGetInputField(panelTrans, "UsernameInput", new Vector2(40, 220), new Vector2(180, 36));
            userInp.gameObject.SetActive(false);

            // Stats bar container
            Transform statsTrans = panelTrans.Find("StatsContainer");
            if (statsTrans == null)
            {
                var sGo = new GameObject("StatsContainer", typeof(RectTransform), typeof(Image));
                sGo.transform.SetParent(panelTrans, false);
                var rt = sGo.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, 105);
                rt.sizeDelta = new Vector2(430, 60);
                var img = sGo.GetComponent<Image>();
                img.color = new Color(0.18f, 0.20f, 0.28f, 0.9f);
                statsTrans = sGo.transform;
            }

            TextMeshProUGUI levelTxt = CreateOrGetTMP(statsTrans, "LevelStat", "LEVEL 1", 16, new Vector2(-140, 0), new Vector2(90, 40));
            TextMeshProUGUI shotsTxt = CreateOrGetTMP(statsTrans, "ShotsStat", "0", 16, new Vector2(-50, -8), new Vector2(80, 24));
            CreateOrGetTMP(statsTrans, "ShotsLabel", "SHOTS", 10, new Vector2(-50, 12), new Vector2(80, 16)).color = new Color(0.7f, 0.7f, 0.7f);

            TextMeshProUGUI winsTxt = CreateOrGetTMP(statsTrans, "WinsStat", "0", 16, new Vector2(45, -8), new Vector2(80, 24));
            CreateOrGetTMP(statsTrans, "WinsLabel", "WINS", 10, new Vector2(45, 12), new Vector2(80, 16)).color = new Color(0.7f, 0.7f, 0.7f);

            TextMeshProUGUI clearedTxt = CreateOrGetTMP(statsTrans, "ClearedStat", "0", 16, new Vector2(135, -8), new Vector2(80, 24));
            CreateOrGetTMP(statsTrans, "ClearedLabel", "CLEARED", 10, new Vector2(135, 12), new Vector2(80, 16)).color = new Color(0.7f, 0.7f, 0.7f);

            // Avatar Selection Section
            CreateOrGetTMP(panelTrans, "AvatarsHeader", "SELECT AVATAR (8 AVAILABLE)", 16, new Vector2(-70, 50), new Vector2(280, 30)).color = new Color(1f, 0.85f, 0.3f);

            Button[] avButtons = new Button[8];
            Image[] avOutlines = new Image[8];

            for (int i = 0; i < 8; i++)
            {
                int row = i / 4;
                int col = i % 4;
                Vector2 pos = new Vector2(-150 + col * 100, -10 - row * 85);

                Transform itemTrans = panelTrans.Find($"AvatarItem_{i}");
                if (itemTrans == null)
                {
                    var itemGo = new GameObject($"AvatarItem_{i}", typeof(RectTransform));
                    itemTrans = itemGo.transform;
                    itemTrans.SetParent(panelTrans, false);
                }
                var itemRt = itemTrans.GetComponent<RectTransform>();
                itemRt.anchoredPosition = pos;
                itemRt.sizeDelta = new Vector2(76, 76);

                Image outline = CreateOrGetImage(itemTrans, "SelectionOutline", null, new Vector2(82, 82));
                outline.color = new Color(1f, 0.9f, 0.2f, 1f);
                outline.gameObject.SetActive(i == 0);
                avOutlines[i] = outline;

                Button btn = CreateOrGetButton(itemTrans, "Button", "", Vector2.zero, new Vector2(74, 74), Color.white);
                Image btnImg = btn.GetComponent<Image>();
                if (config != null && i < config.Avatars.Length)
                {
                    btnImg.sprite = config.Avatars[i].Icon;
                    btnImg.color = config.Avatars[i].ThemeColor;
                }
                avButtons[i] = btn;
            }

            // Frame Selection Section
            CreateOrGetTMP(panelTrans, "FramesHeader", "SELECT FRAME (4 AVAILABLE)", 16, new Vector2(-70, -170), new Vector2(280, 30)).color = new Color(1f, 0.85f, 0.3f);

            Button[] frButtons = new Button[4];
            Image[] frOutlines = new Image[4];

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = new Vector2(-150 + i * 100, -225);

                Transform itemTrans = panelTrans.Find($"FrameItem_{i}");
                if (itemTrans == null)
                {
                    var itemGo = new GameObject($"FrameItem_{i}", typeof(RectTransform));
                    itemTrans = itemGo.transform;
                    itemTrans.SetParent(panelTrans, false);
                }
                var itemRt = itemTrans.GetComponent<RectTransform>();
                itemRt.anchoredPosition = pos;
                itemRt.sizeDelta = new Vector2(76, 76);

                Image outline = CreateOrGetImage(itemTrans, "SelectionOutline", null, new Vector2(82, 82));
                outline.color = new Color(1f, 0.9f, 0.2f, 1f);
                outline.gameObject.SetActive(i == 0);
                frOutlines[i] = outline;

                Button btn = CreateOrGetButton(itemTrans, "Button", "", Vector2.zero, new Vector2(74, 74), Color.white);
                Image btnImg = btn.GetComponent<Image>();
                if (config != null && i < config.Frames.Length)
                {
                    btnImg.sprite = config.Frames[i].BorderSprite;
                    btnImg.color = config.Frames[i].BorderColor;
                }
                frButtons[i] = btn;
            }

            pd.BindReferences(dlgGo, closeBtn,
                curAvatarImg, curFrameImg, userTxt,
                userInp, editBtn, saveBtn,
                levelTxt, shotsTxt, winsTxt, clearedTxt,
                avButtons, avOutlines,
                frButtons, frOutlines);

            dlgGo.SetActive(false);
            return pd;
        }

        private static UsernamePromptDialog BuildUsernamePromptDialog(Transform parent)
        {
            Transform existing = parent.Find("UsernamePromptDialog");
            GameObject dlgGo;
            if (existing != null)
            {
                dlgGo = existing.gameObject;
            }
            else
            {
                dlgGo = new GameObject("UsernamePromptDialog", typeof(RectTransform));
                dlgGo.transform.SetParent(parent, false);
            }

            RectTransform rootRt = dlgGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            UsernamePromptDialog upd = dlgGo.GetComponent<UsernamePromptDialog>() ?? dlgGo.AddComponent<UsernamePromptDialog>();

            // Dark Overlay
            Transform bgTrans = dlgGo.transform.Find("DarkOverlay");
            if (bgTrans == null)
            {
                var bgGo = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(dlgGo.transform, false);
                var rt = bgGo.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                var img = bgGo.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.8f);
            }

            // Center Panel
            Transform panelTrans = dlgGo.transform.Find("CenterPanel");
            if (panelTrans == null)
            {
                var pGo = new GameObject("CenterPanel", typeof(RectTransform), typeof(Image));
                pGo.transform.SetParent(dlgGo.transform, false);
                var rt = pGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(440, 320);
                var img = pGo.GetComponent<Image>();
                img.color = new Color(0.12f, 0.14f, 0.22f, 0.98f);
                panelTrans = pGo.transform;
            }

            TextMeshProUGUI titleTxt = CreateOrGetTMP(panelTrans, "TitleText", "LEVEL 10 REACHED!", 26, new Vector2(0, 105), new Vector2(400, 40));
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(1f, 0.85f, 0.2f);

            TextMeshProUGUI subTxt = CreateOrGetTMP(panelTrans, "SubtitleText", "Choose your Royal Name:", 18, new Vector2(0, 55), new Vector2(380, 30));
            subTxt.color = new Color(0.85f, 0.85f, 0.9f);

            TMP_InputField input = CreateOrGetInputField(panelTrans, "InputField", new Vector2(0, 0), new Vector2(320, 48));

            Button confirmBtn = CreateOrGetButton(panelTrans, "ConfirmButton", "CONFIRM", new Vector2(0, -75), new Vector2(220, 50), new Color(0.2f, 0.75f, 0.35f));

            upd.BindReferences(dlgGo, input, confirmBtn, titleTxt, subTxt);
            dlgGo.SetActive(false);
            return upd;
        }

        private static InGameSettingsDialog UpdateSettingsDialog(Transform parent)
        {
            Transform existing = parent.Find("InGameSettingsDialog");
            if (existing == null)
            {
                Debug.LogWarning("[Phase12Setup] InGameSettingsDialog not found in scene!");
                return null;
            }

            InGameSettingsDialog isd = existing.GetComponent<InGameSettingsDialog>();
            if (isd == null) return null;

            Transform panelTrans = existing.Find("CenterPanel") ?? existing.Find("Panel") ?? existing.Find("Content") ?? existing;

            // Ensure Title has ample width, appropriate font size, and does not wrap
            foreach (var tmp in existing.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.name == "Title" || tmp.text.Contains("SETTING"))
                {
                    var rt = tmp.GetComponent<RectTransform>();
                    if (rt != null) rt.sizeDelta = new Vector2(380, 60);
                    tmp.fontSize = 32;
                    tmp.enableWordWrapping = false;
                }
            }

            // Check if VibrationRow exists
            Transform vibRow = panelTrans.Find("VibrationRow");
            if (vibRow == null)
            {
                var rowGo = new GameObject("VibrationRow", typeof(RectTransform));
                rowGo.transform.SetParent(panelTrans, false);
                var rt = rowGo.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -50); // Under SFX row
                rt.sizeDelta = new Vector2(360, 50);
                vibRow = rowGo.transform;

                // Label
                TextMeshProUGUI vibLabel = CreateOrGetTMP(vibRow, "Label", "VIBRATION", 20, new Vector2(-70, 0), new Vector2(180, 40));
                vibLabel.alignment = TextAlignmentOptions.MidlineLeft;

                // Toggle Button
                Button vibBtn = CreateOrGetButton(vibRow, "VibrationToggleBtn", "ON", new Vector2(100, 0), new Vector2(100, 40), new Color(0.2f, 0.22f, 0.30f));
                TextMeshProUGUI vibStatus = vibBtn.GetComponentInChildren<TextMeshProUGUI>();

                // Re-bind references
                Transform closeBtnTrans = panelTrans.Find("CloseButton");
                Transform resumeBtnTrans = panelTrans.Find("ResumeButton");
                Transform exitBtnTrans = panelTrans.Find("ExitButton") ?? panelTrans.Find("ExitLevelButton");

                Transform musicBtnTrans = panelTrans.Find("MusicRow/MusicToggleBtn") ?? panelTrans.Find("MusicToggleBtn");
                Transform sfxBtnTrans = panelTrans.Find("SfxRow/SfxToggleBtn") ?? panelTrans.Find("SfxToggleBtn");

                Button closeBtn = closeBtnTrans?.GetComponent<Button>();
                Button resumeBtn = resumeBtnTrans?.GetComponent<Button>();
                Button exitBtn = exitBtnTrans?.GetComponent<Button>();
                Button musicBtn = musicBtnTrans?.GetComponent<Button>();
                TextMeshProUGUI musicTxt = musicBtn != null ? musicBtn.GetComponentInChildren<TextMeshProUGUI>() : null;
                Button sfxBtn = sfxBtnTrans?.GetComponent<Button>();
                TextMeshProUGUI sfxTxt = sfxBtn != null ? sfxBtn.GetComponentInChildren<TextMeshProUGUI>() : null;

                isd.BindReferences(existing.gameObject, closeBtn, resumeBtn, exitBtn,
                    musicBtn, musicTxt, sfxBtn, sfxTxt, vibBtn, vibStatus);
            }

            isd.UpdateToggleVisuals();
            return isd;
        }

        private static TextMeshProUGUI CreateOrGetTMP(Transform parent, string name, string text, float fontSize, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateOrGetButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color bgColor)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = bgColor;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;

            if (!string.IsNullOrEmpty(text))
            {
                CreateOrGetTMP(go.transform, "Text", text, 18, Vector2.zero, size);
            }

            return btn;
        }

        private static Image CreateOrGetImage(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            return img;
        }

        private static TMP_InputField CreateOrGetInputField(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(0.18f, 0.20f, 0.28f, 1f);

            var input = go.GetComponent<TMP_InputField>();
            input.characterLimit = 16;

            // Text Component
            TextMeshProUGUI textComp = CreateOrGetTMP(go.transform, "Text", "", 18, Vector2.zero, size);
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            textComp.margin = new Vector4(10, 0, 10, 0);

            // Placeholder
            TextMeshProUGUI placeholder = CreateOrGetTMP(go.transform, "Placeholder", "Enter username...", 18, Vector2.zero, size);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.color = new Color(0.5f, 0.5f, 0.6f);
            placeholder.margin = new Vector4(10, 0, 10, 0);

            input.textComponent = textComp;
            input.placeholder = placeholder;
            return input;
        }
    }
}
