using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Core;
using RoyalSmash.Save;
using RoyalSmash.Profile;
using RoyalSmash.Economy;
using RoyalSmash.Lives;
using RoyalSmash.Skills;
using RoyalSmash.Audio;
using RoyalSmash.Haptics;
using RoyalSmash.VFX;
using RoyalSmash.Cloud;
using RoyalSmash.Ads;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Editor automation tool that constructs the BootScene, configures the UI template matching the reference screenshot,
    /// sets up all persistent managers, and registers BootScene at Build Index 0.
    /// </summary>
    public static class BootSceneSetup
    {
        public const string BOOT_SCENE_PATH = "Assets/_Project/Scenes/BootScene.unity";

        [MenuItem("RoyalSmash/Setup/Create & Configure BootScene")]
        public static void CreateAndConfigureBootScene()
        {
            Debug.Log("[BootSceneSetup] Creating BootScene.unity...");

            // Create a new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Setup Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.07f, 0.17f); // Deep cosmic royal blue
            camGo.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 0f, -10f);

            // 2. Setup EventSystem
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<EventSystem>();
            eventGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 3. Setup Canvas
            var canvasGo = new GameObject("BootCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 4. Background Image (Template slot for full-screen art)
            var bgGo = new GameObject("BackgroundImage");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.08f, 0.22f, 1f); // Rich dark blue gradient placeholder
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Optional Vignette overlay
            var vignetteGo = new GameObject("AtmosphericVignette");
            vignetteGo.transform.SetParent(bgGo.transform, false);
            var vigImg = vignetteGo.AddComponent<Image>();
            vigImg.color = new Color(0.02f, 0.04f, 0.12f, 0.45f);
            var vigRect = vignetteGo.GetComponent<RectTransform>();
            vigRect.anchorMin = Vector2.zero;
            vigRect.anchorMax = Vector2.one;
            vigRect.offsetMin = Vector2.zero;
            vigRect.offsetMax = Vector2.zero;

            // 5. Logo Container (Template slot for logo sprite)
            var logoGo = new GameObject("LogoContainer");
            logoGo.transform.SetParent(canvasGo.transform, false);
            var logoImg = logoGo.AddComponent<Image>();
            logoImg.color = new Color(1f, 1f, 1f, 0.15f); // Subtle frame
            var logoRect = logoGo.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.72f);
            logoRect.anchorMax = new Vector2(0.5f, 0.72f);
            logoRect.sizeDelta = new Vector2(760, 320);

            var logoTitleGo = new GameObject("LogoTitle_Placeholder");
            logoTitleGo.transform.SetParent(logoGo.transform, false);
            var logoTmp = logoTitleGo.AddComponent<TextMeshProUGUI>();
            logoTmp.text = "ROYAL SMASH!";
            logoTmp.fontSize = 72;
            logoTmp.fontStyle = FontStyles.Bold;
            logoTmp.alignment = TextAlignmentOptions.Center;
            logoTmp.color = new Color(1.0f, 0.85f, 0.25f); // Golden yellow
            var logoSubGo = new GameObject("LogoSubtitle");
            logoSubGo.transform.SetParent(logoGo.transform, false);
            var logoSubTmp = logoSubGo.AddComponent<TextMeshProUGUI>();
            logoSubTmp.text = "PHYSICS PUZZLE";
            logoSubTmp.fontSize = 32;
            logoSubTmp.fontStyle = FontStyles.Bold;
            logoSubTmp.alignment = TextAlignmentOptions.Center;
            logoSubTmp.color = new Color(0.35f, 0.85f, 1.0f); // Bright electric cyan
            var subRect = logoSubGo.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, -60);

            // 6. Bottom Section (Hint, Slider, Status)
            var bottomGo = new GameObject("BottomSection");
            bottomGo.transform.SetParent(canvasGo.transform, false);
            var bottomRect = bottomGo.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0.5f, 0.12f);
            bottomRect.anchorMax = new Vector2(0.5f, 0.12f);
            bottomRect.sizeDelta = new Vector2(960, 260);

            // Hint Text
            var hintGo = new GameObject("HintText");
            hintGo.transform.SetParent(bottomGo.transform, false);
            var hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.text = "Her Rift bir zayıf nokta saklar.";
            hintTmp.fontSize = 34;
            hintTmp.fontStyle = FontStyles.Bold;
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.color = Color.white;
            var hintRect = hintGo.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, 65);
            hintRect.sizeDelta = new Vector2(920, 70);

            // Slider
            var sliderGo = new GameObject("LoadingSlider");
            sliderGo.transform.SetParent(bottomGo.transform, false);
            var slider = sliderGo.AddComponent<Slider>();
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(0, 0);
            sliderRect.sizeDelta = new Vector2(880, 26);

            // Slider Background
            var sliderBgGo = new GameObject("Background");
            sliderBgGo.transform.SetParent(sliderGo.transform, false);
            var sliderBgImg = sliderBgGo.AddComponent<Image>();
            sliderBgImg.color = new Color(0.05f, 0.11f, 0.25f, 0.9f); // Dark translucent blue track
            var sliderBgRect = sliderBgGo.GetComponent<RectTransform>();
            sliderBgRect.anchorMin = Vector2.zero;
            sliderBgRect.anchorMax = Vector2.one;
            sliderBgRect.offsetMin = Vector2.zero;
            sliderBgRect.offsetMax = Vector2.zero;

            // Slider Fill Area
            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.2f);
            fillAreaRect.anchorMax = new Vector2(1, 0.8f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.12f, 0.84f, 1.0f); // Vibrant glowing cyan
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            // Status Text (YÜKLENİYOR...)
            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(bottomGo.transform, false);
            var statusTmp = statusGo.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "YÜKLENİYOR...";
            statusTmp.fontSize = 32;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.characterSpacing = 6f;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(0.6f, 0.88f, 1.0f);
            var statusRect = statusGo.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, -50);
            statusRect.sizeDelta = new Vector2(920, 50);

            // 7. Setup BootController and wire fields
            var bootControllerGo = new GameObject("BootController");
            var bootController = bootControllerGo.AddComponent<BootController>();

            typeof(BootController).GetField("_backgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootController, bgImg);
            typeof(BootController).GetField("_logoImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootController, logoImg);
            typeof(BootController).GetField("_loadingSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootController, slider);
            typeof(BootController).GetField("_hintText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootController, hintTmp);
            typeof(BootController).GetField("_statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootController, statusTmp);

            // 8. Add all 12 persistent global managers to BootScene
            CreateManagerGameObject<SaveManager>("SaveManager");
            CreateManagerGameObject<ProfileManager>("ProfileManager");
            CreateManagerGameObject<GoldManager>("GoldManager");
            CreateManagerGameObject<LivesManager>("LivesManager");
            CreateManagerGameObject<SkillManager>("SkillManager");
            CreateManagerGameObject<AudioManager>("AudioManager");
            CreateManagerGameObject<HapticManager>("HapticManager");
            CreateManagerGameObject<VFXManager>("VFXManager");
            CreateManagerGameObject<CloudSyncManager>("CloudSyncManager");
            CreateManagerGameObject<LeaderboardManager>("LeaderboardManager");
            CreateManagerGameObject<AdManager>("AdManager");
            CreateManagerGameObject<IAPManager>("IAPManager");
            CreateManagerGameObject<SceneTransitionManager>("SceneTransitionManager");
            CreateManagerGameObject<RoyalSmash.Utils.VibrationManager>("VibrationManager");

            // Save the newly created BootScene
            EditorSceneManager.SaveScene(newScene, BOOT_SCENE_PATH);
            Debug.Log($"[BootSceneSetup] Successfully saved BootScene at '{BOOT_SCENE_PATH}'.");

            // 9. Register in EditorBuildSettings as Index 0
            RegisterBootSceneInBuildSettings();
        }

        private static void CreateManagerGameObject<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.AddComponent<T>();
        }

        /// <summary>
        /// Ensures BootScene is placed at index 0 of EditorBuildSettings.
        /// </summary>
        public static void RegisterBootSceneInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();

            // Always add BootScene first (Index 0)
            scenes.Add(new EditorBuildSettingsScene(BOOT_SCENE_PATH, true));

            // Then add MainMenu and MainGame
            string menuPath = "Assets/_Project/Scenes/MainMenu.unity";
            string gamePath = "Assets/_Project/Scenes/MainGame.unity";

            if (File.Exists(menuPath)) scenes.Add(new EditorBuildSettingsScene(menuPath, true));
            if (File.Exists(gamePath)) scenes.Add(new EditorBuildSettingsScene(gamePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[BootSceneSetup] Updated EditorBuildSettings with {scenes.Count} scenes. Index 0: {BOOT_SCENE_PATH}");
        }
    }
}
