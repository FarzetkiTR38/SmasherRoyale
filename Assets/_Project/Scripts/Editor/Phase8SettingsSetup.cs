using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.UI;

public class Phase8SettingsSetup
{
    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    private static GameObject CreateChildUI(string name, GameObject parent)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    public static string Execute()
    {
        try
        {
            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainGame.unity",
                "Assets/_Project/Scenes/PhysicsTestScene.unity"
            };

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);
                GameObject canvasGo = GameObject.Find("MainCanvas");
                if (canvasGo == null) continue;

                // 1. Remove old HUD_ExitButton
                Transform oldExitBtn = canvasGo.transform.Find("HUD_ExitButton");
                if (oldExitBtn != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldExitBtn.gameObject);
                }

                // 2. Create HUD_SettingsButton (top-right, Y = -96 to align with Ball Counter)
                GameObject settingsBtnGo = CreateChildUI("HUD_SettingsButton", canvasGo);
                RectTransform setBtnRect = settingsBtnGo.GetComponent<RectTransform>();
                setBtnRect.anchorMin = new Vector2(1f, 1f);
                setBtnRect.anchorMax = new Vector2(1f, 1f);
                setBtnRect.pivot = new Vector2(1f, 1f);
                setBtnRect.anchoredPosition = new Vector2(-48f, -96f);
                setBtnRect.sizeDelta = new Vector2(80f, 80f);

                Image setBtnImg = GetOrAddComponent<Image>(settingsBtnGo);
                setBtnImg.color = new Color(0.18f, 0.12f, 0.28f, 0.92f);
                Outline setBtnOutline = GetOrAddComponent<Outline>(settingsBtnGo);
                setBtnOutline.effectColor = new Color(1f, 0.85f, 0.2f, 0.9f);
                setBtnOutline.effectDistance = new Vector2(2f, -2f);

                GetOrAddComponent<Button>(settingsBtnGo);
                GetOrAddComponent<HUDSettingsButton>(settingsBtnGo);

                GameObject iconGo = CreateChildUI("Icon", settingsBtnGo);
                RectTransform iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(46f, 46f);

                var oldTmp = iconGo.GetComponent<TextMeshProUGUI>();
                if (oldTmp != null) UnityEngine.Object.DestroyImmediate(oldTmp);

                Image iconImg = GetOrAddComponent<Image>(iconGo);
                string iconPath = "Assets/_Project/Textures/SettingsIcon.png";
                Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // 3. Create InGameSettingsDialog
                GameObject dialogGo = CreateChildUI("InGameSettingsDialog", canvasGo);
                RectTransform dialogRect = dialogGo.GetComponent<RectTransform>();
                dialogRect.anchorMin = Vector2.zero;
                dialogRect.anchorMax = Vector2.one;
                dialogRect.offsetMin = Vector2.zero;
                dialogRect.offsetMax = Vector2.zero;

                GameObject contentGo = CreateChildUI("Content", dialogGo);
                RectTransform contentRect = contentGo.GetComponent<RectTransform>();
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = Vector2.zero;
                contentRect.offsetMax = Vector2.zero;

                Image backdrop = GetOrAddComponent<Image>(contentGo);
                backdrop.color = new Color(0f, 0f, 0f, 0.75f);

                // Card
                GameObject cardGo = CreateChildUI("Card", contentGo);
                RectTransform cardRect = cardGo.GetComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.sizeDelta = new Vector2(600f, 620f);

                Image cardImg = GetOrAddComponent<Image>(cardGo);
                cardImg.color = new Color(0.16f, 0.10f, 0.26f, 0.98f);
                Outline cardOutline = GetOrAddComponent<Outline>(cardGo);
                cardOutline.effectColor = new Color(1f, 0.85f, 0.2f, 1f);
                cardOutline.effectDistance = new Vector2(3f, -3f);

                // Title
                GameObject titleGo = CreateChildUI("Title", cardGo);
                RectTransform titleRect = titleGo.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -40f);
                titleRect.sizeDelta = new Vector2(500f, 60f);
                TextMeshProUGUI titleTmp = GetOrAddComponent<TextMeshProUGUI>(titleGo);
                titleTmp.text = "AYARLAR";
                titleTmp.fontSize = 38;
                titleTmp.fontStyle = FontStyles.Bold;
                titleTmp.alignment = TextAlignmentOptions.Center;
                titleTmp.color = new Color(1f, 0.85f, 0.25f, 1f);

                // Close Button ("✕")
                GameObject closeBtnGo = CreateChildUI("CloseBtn", cardGo);
                RectTransform closeRect = closeBtnGo.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-20f, -20f);
                closeRect.sizeDelta = new Vector2(50f, 50f);
                Image closeBtnImg = GetOrAddComponent<Image>(closeBtnGo);
                closeBtnImg.color = new Color(0.4f, 0.2f, 0.2f, 0.8f);
                Button closeBtnComp = GetOrAddComponent<Button>(closeBtnGo);

                GameObject closeTextGo = CreateChildUI("Text", closeBtnGo);
                RectTransform closeTextRect = closeTextGo.GetComponent<RectTransform>();
                closeTextRect.anchorMin = Vector2.zero;
                closeTextRect.anchorMax = Vector2.one;
                closeTextRect.offsetMin = Vector2.zero;
                closeTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI closeTmp = GetOrAddComponent<TextMeshProUGUI>(closeTextGo);
                closeTmp.text = "✕";
                closeTmp.fontSize = 28;
                closeTmp.alignment = TextAlignmentOptions.Center;
                closeTmp.color = Color.white;

                // Music Toggle
                GameObject musicBtnGo = CreateChildUI("MusicToggle", cardGo);
                RectTransform musicRect = musicBtnGo.GetComponent<RectTransform>();
                musicRect.anchorMin = new Vector2(0.5f, 0.5f);
                musicRect.anchorMax = new Vector2(0.5f, 0.5f);
                musicRect.pivot = new Vector2(0.5f, 0.5f);
                musicRect.anchoredPosition = new Vector2(0f, 85f);
                musicRect.sizeDelta = new Vector2(480f, 65f);
                Image musicImg = GetOrAddComponent<Image>(musicBtnGo);
                musicImg.color = new Color(0.24f, 0.16f, 0.38f, 0.95f);
                Button musicBtnComp = GetOrAddComponent<Button>(musicBtnGo);

                GameObject musicLblGo = CreateChildUI("Label", musicBtnGo);
                RectTransform musicLblRect = musicLblGo.GetComponent<RectTransform>();
                musicLblRect.anchorMin = new Vector2(0f, 0f);
                musicLblRect.anchorMax = new Vector2(0.6f, 1f);
                musicLblRect.offsetMin = new Vector2(25f, 0f);
                musicLblRect.offsetMax = Vector2.zero;
                TextMeshProUGUI musicLblTmp = GetOrAddComponent<TextMeshProUGUI>(musicLblGo);
                musicLblTmp.text = "🎵 Müzik";
                musicLblTmp.fontSize = 26;
                musicLblTmp.alignment = TextAlignmentOptions.MidlineLeft;
                musicLblTmp.color = Color.white;

                GameObject musicStatusGo = CreateChildUI("Status", musicBtnGo);
                RectTransform musicStatusRect = musicStatusGo.GetComponent<RectTransform>();
                musicStatusRect.anchorMin = new Vector2(0.6f, 0f);
                musicStatusRect.anchorMax = new Vector2(1f, 1f);
                musicStatusRect.offsetMin = Vector2.zero;
                musicStatusRect.offsetMax = new Vector2(-25f, 0f);
                TextMeshProUGUI musicStatusTmp = GetOrAddComponent<TextMeshProUGUI>(musicStatusGo);
                musicStatusTmp.text = "AÇIK";
                musicStatusTmp.fontSize = 26;
                musicStatusTmp.fontStyle = FontStyles.Bold;
                musicStatusTmp.alignment = TextAlignmentOptions.MidlineRight;
                musicStatusTmp.color = new Color(0.3f, 1f, 0.3f);

                // SFX Toggle
                GameObject sfxBtnGo = CreateChildUI("SfxToggle", cardGo);
                RectTransform sfxRect = sfxBtnGo.GetComponent<RectTransform>();
                sfxRect.anchorMin = new Vector2(0.5f, 0.5f);
                sfxRect.anchorMax = new Vector2(0.5f, 0.5f);
                sfxRect.pivot = new Vector2(0.5f, 0.5f);
                sfxRect.anchoredPosition = new Vector2(0f, 10f);
                sfxRect.sizeDelta = new Vector2(480f, 65f);
                Image sfxImg = GetOrAddComponent<Image>(sfxBtnGo);
                sfxImg.color = new Color(0.24f, 0.16f, 0.38f, 0.95f);
                Button sfxBtnComp = GetOrAddComponent<Button>(sfxBtnGo);

                GameObject sfxLblGo = CreateChildUI("Label", sfxBtnGo);
                RectTransform sfxLblRect = sfxLblGo.GetComponent<RectTransform>();
                sfxLblRect.anchorMin = new Vector2(0f, 0f);
                sfxLblRect.anchorMax = new Vector2(0.6f, 1f);
                sfxLblRect.offsetMin = new Vector2(25f, 0f);
                sfxLblRect.offsetMax = Vector2.zero;
                TextMeshProUGUI sfxLblTmp = GetOrAddComponent<TextMeshProUGUI>(sfxLblGo);
                sfxLblTmp.text = "🔊 Ses Efektleri";
                sfxLblTmp.fontSize = 26;
                sfxLblTmp.alignment = TextAlignmentOptions.MidlineLeft;
                sfxLblTmp.color = Color.white;

                GameObject sfxStatusGo = CreateChildUI("Status", sfxBtnGo);
                RectTransform sfxStatusRect = sfxStatusGo.GetComponent<RectTransform>();
                sfxStatusRect.anchorMin = new Vector2(0.6f, 0f);
                sfxStatusRect.anchorMax = new Vector2(1f, 1f);
                sfxStatusRect.offsetMin = Vector2.zero;
                sfxStatusRect.offsetMax = new Vector2(-25f, 0f);
                TextMeshProUGUI sfxStatusTmp = GetOrAddComponent<TextMeshProUGUI>(sfxStatusGo);
                sfxStatusTmp.text = "AÇIK";
                sfxStatusTmp.fontSize = 26;
                sfxStatusTmp.fontStyle = FontStyles.Bold;
                sfxStatusTmp.alignment = TextAlignmentOptions.MidlineRight;
                sfxStatusTmp.color = new Color(0.3f, 1f, 0.3f);

                // Exit Level Button
                GameObject exitLvlBtnGo = CreateChildUI("ExitLevelBtn", cardGo);
                RectTransform exitLvlRect = exitLvlBtnGo.GetComponent<RectTransform>();
                exitLvlRect.anchorMin = new Vector2(0.5f, 0.5f);
                exitLvlRect.anchorMax = new Vector2(0.5f, 0.5f);
                exitLvlRect.pivot = new Vector2(0.5f, 0.5f);
                exitLvlRect.anchoredPosition = new Vector2(0f, -80f);
                exitLvlRect.sizeDelta = new Vector2(480f, 70f);
                Image exitLvlImg = GetOrAddComponent<Image>(exitLvlBtnGo);
                exitLvlImg.color = new Color(0.75f, 0.2f, 0.2f, 0.95f);
                Button exitLvlBtnComp = GetOrAddComponent<Button>(exitLvlBtnGo);

                GameObject exitLvlTextGo = CreateChildUI("Text", exitLvlBtnGo);
                RectTransform exitLvlTextRect = exitLvlTextGo.GetComponent<RectTransform>();
                exitLvlTextRect.anchorMin = Vector2.zero;
                exitLvlTextRect.anchorMax = Vector2.one;
                exitLvlTextRect.offsetMin = Vector2.zero;
                exitLvlTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI exitLvlTmp = GetOrAddComponent<TextMeshProUGUI>(exitLvlTextGo);
                exitLvlTmp.text = "SEVİYEDEN ÇIK";
                exitLvlTmp.fontSize = 26;
                exitLvlTmp.fontStyle = FontStyles.Bold;
                exitLvlTmp.alignment = TextAlignmentOptions.Center;
                exitLvlTmp.color = Color.white;

                // Resume Button
                GameObject resumeBtnGo = CreateChildUI("ResumeBtn", cardGo);
                RectTransform resumeRect = resumeBtnGo.GetComponent<RectTransform>();
                resumeRect.anchorMin = new Vector2(0.5f, 0.5f);
                resumeRect.anchorMax = new Vector2(0.5f, 0.5f);
                resumeRect.pivot = new Vector2(0.5f, 0.5f);
                resumeRect.anchoredPosition = new Vector2(0f, -170f);
                resumeRect.sizeDelta = new Vector2(480f, 70f);
                Image resumeImg = GetOrAddComponent<Image>(resumeBtnGo);
                resumeImg.color = new Color(0.18f, 0.65f, 0.35f, 0.95f);
                Button resumeBtnComp = GetOrAddComponent<Button>(resumeBtnGo);

                GameObject resumeTextGo = CreateChildUI("Text", resumeBtnGo);
                RectTransform resumeTextRect = resumeTextGo.GetComponent<RectTransform>();
                resumeTextRect.anchorMin = Vector2.zero;
                resumeTextRect.anchorMax = Vector2.one;
                resumeTextRect.offsetMin = Vector2.zero;
                resumeTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI resumeTmp = GetOrAddComponent<TextMeshProUGUI>(resumeTextGo);
                resumeTmp.text = "DEVAM ET";
                resumeTmp.fontSize = 28;
                resumeTmp.fontStyle = FontStyles.Bold;
                resumeTmp.alignment = TextAlignmentOptions.Center;
                resumeTmp.color = Color.white;

                // Wire up InGameSettingsDialog
                var dialogComp = GetOrAddComponent<InGameSettingsDialog>(dialogGo);
                dialogComp.BindReferences(contentGo, closeBtnComp, resumeBtnComp, exitLvlBtnComp,
                    musicBtnComp, musicStatusTmp, sfxBtnComp, sfxStatusTmp);

                contentGo.SetActive(false);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Phase8SettingsSetup] Configured Settings in {scenePath}");
            }

            return "SUCCESS: Configured HUD Settings button and dialog across scenes.";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.ToString();
        }
    }
}
