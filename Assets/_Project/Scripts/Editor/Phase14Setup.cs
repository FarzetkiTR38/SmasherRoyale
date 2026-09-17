using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Ads;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated setup tool for Phase 14: Ads Framework (AdMob / Unity Ads Integration).
    /// Instantiates AdConfig ScriptableObject, wires AdManager and SimulatedAdDialog in scenes.
    /// </summary>
    public static class Phase14Setup
    {
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/AdConfig.asset";
        private const string MAIN_MENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MAIN_GAME_SCENE = "Assets/_Project/Scenes/MainGame.unity";

        [MenuItem("RoyalSmash/Phase 14/Setup Ads Framework")]
        public static void RunFullSetup()
        {
            Debug.Log("=== STARTING PHASE 14 AUTOMATED SETUP ===");

            // 1. Create or load AdConfig asset
            AdConfig config = CreateOrLoadAdConfig();

            // 2. Setup MainMenu scene
            SetupMainMenuScene(config);

            // 3. Setup MainGame scene
            SetupMainGameScene(config);

            // Open MainMenu as primary
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("=== PHASE 14 AUTOMATED SETUP COMPLETED SUCCESSFULLY ===");
        }

        public static AdConfig CreateOrLoadAdConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<AdConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AdConfig>();
                config.EnableSimulationMode = true;

                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Phase14Setup] Created new AdConfig at {CONFIG_PATH}");
            }
            return config;
        }

        private static void SetupMainMenuScene(AdConfig config)
        {
            EditorSceneManager.OpenScene(MAIN_MENU_SCENE);

            // Setup AdManager
            EnsureAdManagerInScene(config);

            // Setup SimulatedAdDialog in Canvas
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform dialogParent = canvas.transform.Find("Dialogs") ?? canvas.transform;
                var existingDialog = dialogParent.Find("SimulatedAdDialog");
                if (existingDialog == null)
                {
                    CreateSimulatedAdDialog(dialogParent);
                }
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Phase14Setup] MainMenu scene configured with AdManager and SimulatedAdDialog.");
        }

        private static void SetupMainGameScene(AdConfig config)
        {
            EditorSceneManager.OpenScene(MAIN_GAME_SCENE);

            // Setup AdManager
            EnsureAdManagerInScene(config);

            // Setup SimulatedAdDialog in Canvas
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                var existingDialog = canvas.transform.Find("SimulatedAdDialog");
                if (existingDialog == null)
                {
                    CreateSimulatedAdDialog(canvas.transform);
                }
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Phase14Setup] MainGame scene configured with AdManager and SimulatedAdDialog.");
        }

        private static void EnsureAdManagerInScene(AdConfig config)
        {
            var adMgr = UnityEngine.Object.FindFirstObjectByType<AdManager>(FindObjectsInactive.Include);
            if (adMgr == null)
            {
                var managersGo = GameObject.Find("[Managers]");
                GameObject targetGo;
                if (managersGo != null)
                {
                    targetGo = managersGo;
                }
                else
                {
                    targetGo = new GameObject("AdManager");
                }

                adMgr = targetGo.GetComponent<AdManager>();
                if (adMgr == null) adMgr = targetGo.AddComponent<AdManager>();
            }

            // Assign config field via SerializedObject
            var so = new SerializedObject(adMgr);
            var prop = so.FindProperty("_config");
            if (prop != null)
            {
                prop.objectReferenceValue = config;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(adMgr);
        }

        private static SimulatedAdDialog CreateSimulatedAdDialog(Transform parent)
        {
            var dialogGo = new GameObject("SimulatedAdDialog", typeof(RectTransform), typeof(SimulatedAdDialog));
            dialogGo.transform.SetParent(parent, false);

            var rootRect = dialogGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Fullscreen Dim Backdrop
            var backdropImg = dialogGo.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.85f);

            // Modal Card
            var cardGo = new GameObject("Card", typeof(RectTransform), typeof(Image));
            cardGo.transform.SetParent(dialogGo.transform, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(340f, 440f);
            var cardImg = cardGo.GetComponent<Image>();
            cardImg.color = new Color(0.11f, 0.13f, 0.19f, 0.98f); // Deep slate navy

            // Outline / Border
            var outline = cardGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.77f, 0.25f, 0.6f); // Royal Gold accent
            outline.effectDistance = new Vector2(2, -2);

            // Type Badge
            var badgeGo = new GameObject("TypeBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeGo.transform.SetParent(cardGo.transform, false);
            var badgeRect = badgeGo.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 1f);
            badgeRect.anchorMax = new Vector2(0.5f, 1f);
            badgeRect.anchoredPosition = new Vector2(0f, -30f);
            badgeRect.sizeDelta = new Vector2(300f, 30f);
            var badgeText = badgeGo.GetComponent<TextMeshProUGUI>();
            badgeText.text = "SPONSORED AD";
            badgeText.fontSize = 14;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.color = new Color(0.95f, 0.77f, 0.25f);

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -65f);
            titleRect.sizeDelta = new Vector2(300f, 35f);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "ROYAL SMASH BREAK";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Description / Banner simulation box
            var bannerGo = new GameObject("BannerBox", typeof(RectTransform), typeof(Image));
            bannerGo.transform.SetParent(cardGo.transform, false);
            var bannerRect = bannerGo.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 25f);
            bannerRect.sizeDelta = new Vector2(280f, 130f);
            var bannerImg = bannerGo.GetComponent<Image>();
            bannerImg.color = new Color(0.18f, 0.22f, 0.32f);

            var descGo = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(bannerGo.transform, false);
            var descRect = descGo.GetComponent<RectTransform>();
            descRect.anchorMin = Vector2.zero;
            descRect.anchorMax = Vector2.one;
            descRect.offsetMin = new Vector2(10f, 10f);
            descRect.offsetMax = new Vector2(-10f, -10f);
            var descText = descGo.GetComponent<TextMeshProUGUI>();
            descText.text = "Ad simulation playing...\nThank you for playing Royal Smash!";
            descText.fontSize = 14;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.85f, 0.88f, 0.95f);

            // Progress Slider
            var sliderGo = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(cardGo.transform, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0f, -65f);
            sliderRect.sizeDelta = new Vector2(280f, 12f);
            var slider = sliderGo.GetComponent<Slider>();
            slider.interactable = false;

            // Slider background
            var sliderBg = sliderGo.AddComponent<Image>();
            sliderBg.color = new Color(0.15f, 0.17f, 0.22f);

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = new Color(0.18f, 0.8f, 0.44f); // Emerald green
            slider.fillRect = fillRect;
            slider.value = 0f;

            // Timer Text
            var timerGo = new GameObject("TimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            timerGo.transform.SetParent(cardGo.transform, false);
            var timerRect = timerGo.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 0.5f);
            timerRect.anchorMax = new Vector2(0.5f, 0.5f);
            timerRect.anchoredPosition = new Vector2(0f, -90f);
            timerRect.sizeDelta = new Vector2(280f, 25f);
            var timerText = timerGo.GetComponent<TextMeshProUGUI>();
            timerText.text = "Ad finishes in 1.5s...";
            timerText.fontSize = 13;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = new Color(0.7f, 0.75f, 0.85f);

            // Button Bar
            var closeBtn = CreateButton(cardGo.transform, "CloseButton", "Continue", new Vector2(0f, -145f),
                new Vector2(260f, 38f), new Color(0.18f, 0.8f, 0.44f));

            var cancelBtn = CreateButton(cardGo.transform, "CancelButton", "Cancel Ad (No Reward)", new Vector2(0f, -185f),
                new Vector2(260f, 32f), new Color(0.85f, 0.25f, 0.25f));

            var failBtn = CreateButton(cardGo.transform, "FailButton", "Simulate Load Error", new Vector2(0f, 200f),
                new Vector2(160f, 24f), new Color(0.9f, 0.5f, 0.1f));

            var dialogComp = dialogGo.GetComponent<SimulatedAdDialog>();
            dialogComp.BindReferences(dialogGo, titleText, badgeText, timerText, descText, closeBtn, cancelBtn, failBtn, slider);

            dialogGo.SetActive(false);
            return dialogComp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = btnGo.GetComponent<Image>();
            img.color = bgColor;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnGo.GetComponent<Button>();
        }
    }
}
