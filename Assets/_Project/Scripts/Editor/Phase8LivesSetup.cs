using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Phase8LivesSetup
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
            // 1. Create or load LivesConfig.asset
            string configDir = "Assets/_Project/ScriptableObjects/Config";
            if (!AssetDatabase.IsValidFolder(configDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Config");
            }

            string configPath = "Assets/_Project/ScriptableObjects/Config/LivesConfig.asset";
            var livesConfigType = Type.GetType("RoyalSmash.Lives.LivesConfig, Assembly-CSharp");
            if (livesConfigType == null) return "Error: RoyalSmash.Lives.LivesConfig type not found";

            ScriptableObject livesConfig = AssetDatabase.LoadAssetAtPath(configPath, livesConfigType) as ScriptableObject;
            if (livesConfig == null)
            {
                livesConfig = ScriptableObject.CreateInstance(livesConfigType);
                AssetDatabase.CreateAsset(livesConfig, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[Phase8LivesSetup] Created LivesConfig.asset");
            }

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainGame.unity",
                "Assets/_Project/Scenes/PhysicsTestScene.unity"
            };

            var livesMgrType = Type.GetType("RoyalSmash.Lives.LivesManager, Assembly-CSharp");
            var lifecycleType = Type.GetType("RoyalSmash.Core.AppLifecycleHandler, Assembly-CSharp");
            var livesWidgetType = Type.GetType("RoyalSmash.UI.LivesWidgetUI, Assembly-CSharp");
            var outOfLivesType = Type.GetType("RoyalSmash.UI.OutOfLivesDialog, Assembly-CSharp");
            var exitConfirmType = Type.GetType("RoyalSmash.UI.ExitConfirmDialog, Assembly-CSharp");

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);

                // 1. LivesManager GameObject
                GameObject livesMgrGo = GameObject.Find("LivesManager");
                if (livesMgrGo == null) livesMgrGo = new GameObject("LivesManager");
                Component livesMgrComp = GetOrAddComponent(livesMgrGo, livesMgrType);

                SerializedObject soLives = new SerializedObject(livesMgrComp);
                soLives.FindProperty("_config").objectReferenceValue = livesConfig;
                soLives.ApplyModifiedProperties();

                // 2. AppLifecycleHandler GameObject
                GameObject lifecycleGo = GameObject.Find("AppLifecycleHandler");
                if (lifecycleGo == null) lifecycleGo = new GameObject("AppLifecycleHandler");
                GetOrAddComponent(lifecycleGo, lifecycleType);

                // 3. Find MainCanvas
                GameObject canvasGo = GameObject.Find("MainCanvas");
                if (canvasGo == null)
                {
                    Debug.LogWarning($"MainCanvas not found in {scenePath}");
                    continue;
                }

                // 4. OutOfLivesDialog (Modal Dialog)
                GameObject outOfLivesGo = CreateChildUI("OutOfLivesDialog", canvasGo);
                RectTransform oolRect = outOfLivesGo.GetComponent<RectTransform>();
                oolRect.anchorMin = Vector2.zero;
                oolRect.anchorMax = Vector2.one;
                oolRect.offsetMin = Vector2.zero;
                oolRect.offsetMax = Vector2.zero;

                GameObject oolContentGo = CreateChildUI("Content", outOfLivesGo);
                RectTransform oolContentRect = oolContentGo.GetComponent<RectTransform>();
                oolContentRect.anchorMin = Vector2.zero;
                oolContentRect.anchorMax = Vector2.one;
                oolContentRect.offsetMin = Vector2.zero;
                oolContentRect.offsetMax = Vector2.zero;

                Image oolBackdrop = GetOrAddComponent<Image>(oolContentGo);
                oolBackdrop.color = new Color(0f, 0f, 0f, 0.75f);

                // Card
                GameObject oolCardGo = CreateChildUI("Card", oolContentGo);
                RectTransform oolCardRect = oolCardGo.GetComponent<RectTransform>();
                oolCardRect.anchorMin = new Vector2(0.5f, 0.5f);
                oolCardRect.anchorMax = new Vector2(0.5f, 0.5f);
                oolCardRect.pivot = new Vector2(0.5f, 0.5f);
                oolCardRect.sizeDelta = new Vector2(620f, 580f);

                Image oolCardImg = GetOrAddComponent<Image>(oolCardGo);
                oolCardImg.color = new Color(0.18f, 0.10f, 0.28f, 0.98f);
                Outline oolCardOutline = GetOrAddComponent<Outline>(oolCardGo);
                oolCardOutline.effectColor = new Color(1f, 0.85f, 0.2f, 1f);
                oolCardOutline.effectDistance = new Vector2(3f, -3f);

                // Title
                GameObject oolTitleGo = CreateChildUI("Title", oolCardGo);
                RectTransform oolTitleRect = oolTitleGo.GetComponent<RectTransform>();
                oolTitleRect.anchorMin = new Vector2(0.5f, 1f);
                oolTitleRect.anchorMax = new Vector2(0.5f, 1f);
                oolTitleRect.pivot = new Vector2(0.5f, 1f);
                oolTitleRect.anchoredPosition = new Vector2(0f, -40f);
                oolTitleRect.sizeDelta = new Vector2(560f, 60f);
                TextMeshProUGUI oolTitleTmp = GetOrAddComponent<TextMeshProUGUI>(oolTitleGo);
                oolTitleTmp.text = "CANLARIN TÜKENDİ!";
                oolTitleTmp.fontSize = 38;
                oolTitleTmp.fontStyle = FontStyles.Bold;
                oolTitleTmp.alignment = TextAlignmentOptions.Center;
                oolTitleTmp.color = new Color(1f, 0.3f, 0.3f, 1f);

                // Description
                GameObject oolDescGo = CreateChildUI("Description", oolCardGo);
                RectTransform oolDescRect = oolDescGo.GetComponent<RectTransform>();
                oolDescRect.anchorMin = new Vector2(0.5f, 1f);
                oolDescRect.anchorMax = new Vector2(0.5f, 1f);
                oolDescRect.pivot = new Vector2(0.5f, 1f);
                oolDescRect.anchoredPosition = new Vector2(0f, -120f);
                oolDescRect.sizeDelta = new Vector2(520f, 90f);
                TextMeshProUGUI oolDescTmp = GetOrAddComponent<TextMeshProUGUI>(oolDescGo);
                oolDescTmp.text = "Oynamaya devam etmek için can satın alabilir veya reklam izleyebilirsin.";
                oolDescTmp.fontSize = 26;
                oolDescTmp.alignment = TextAlignmentOptions.Center;
                oolDescTmp.color = Color.white;

                // Buy with Gold Button
                GameObject oolBuyBtnGo = CreateChildUI("BuyWithGoldBtn", oolCardGo);
                RectTransform oolBuyBtnRect = oolBuyBtnGo.GetComponent<RectTransform>();
                oolBuyBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                oolBuyBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                oolBuyBtnRect.pivot = new Vector2(0.5f, 0.5f);
                oolBuyBtnRect.anchoredPosition = new Vector2(0f, -15f);
                oolBuyBtnRect.sizeDelta = new Vector2(460f, 75f);
                Image oolBuyBtnImg = GetOrAddComponent<Image>(oolBuyBtnGo);
                oolBuyBtnImg.color = new Color(0.95f, 0.65f, 0.1f, 1f);
                Button oolBuyBtnComp = GetOrAddComponent<Button>(oolBuyBtnGo);

                GameObject oolBuyTextGo = CreateChildUI("Text", oolBuyBtnGo);
                RectTransform oolBuyTextRect = oolBuyTextGo.GetComponent<RectTransform>();
                oolBuyTextRect.anchorMin = Vector2.zero;
                oolBuyTextRect.anchorMax = Vector2.one;
                oolBuyTextRect.offsetMin = Vector2.zero;
                oolBuyTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI oolBuyTextTmp = GetOrAddComponent<TextMeshProUGUI>(oolBuyTextGo);
                oolBuyTextTmp.text = "+1 CAN AL (100 ALTIN)";
                oolBuyTextTmp.fontSize = 28;
                oolBuyTextTmp.fontStyle = FontStyles.Bold;
                oolBuyTextTmp.alignment = TextAlignmentOptions.Center;
                oolBuyTextTmp.color = Color.white;

                // Watch Ad Button
                GameObject oolAdBtnGo = CreateChildUI("WatchAdBtn", oolCardGo);
                RectTransform oolAdBtnRect = oolAdBtnGo.GetComponent<RectTransform>();
                oolAdBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                oolAdBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                oolAdBtnRect.pivot = new Vector2(0.5f, 0.5f);
                oolAdBtnRect.anchoredPosition = new Vector2(0f, -110f);
                oolAdBtnRect.sizeDelta = new Vector2(460f, 75f);
                Image oolAdBtnImg = GetOrAddComponent<Image>(oolAdBtnGo);
                oolAdBtnImg.color = new Color(0.15f, 0.65f, 0.95f, 1f);
                Button oolAdBtnComp = GetOrAddComponent<Button>(oolAdBtnGo);

                GameObject oolAdTextGo = CreateChildUI("Text", oolAdBtnGo);
                RectTransform oolAdTextRect = oolAdTextGo.GetComponent<RectTransform>();
                oolAdTextRect.anchorMin = Vector2.zero;
                oolAdTextRect.anchorMax = Vector2.one;
                oolAdTextRect.offsetMin = Vector2.zero;
                oolAdTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI oolAdTextTmp = GetOrAddComponent<TextMeshProUGUI>(oolAdTextGo);
                oolAdTextTmp.text = "REKLAM İZLE (+1 CAN)";
                oolAdTextTmp.fontSize = 28;
                oolAdTextTmp.fontStyle = FontStyles.Bold;
                oolAdTextTmp.alignment = TextAlignmentOptions.Center;
                oolAdTextTmp.color = Color.white;

                // Close Button
                GameObject oolCloseBtnGo = CreateChildUI("CloseBtn", oolCardGo);
                RectTransform oolCloseBtnRect = oolCloseBtnGo.GetComponent<RectTransform>();
                oolCloseBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                oolCloseBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                oolCloseBtnRect.pivot = new Vector2(0.5f, 0.5f);
                oolCloseBtnRect.anchoredPosition = new Vector2(0f, -200f);
                oolCloseBtnRect.sizeDelta = new Vector2(300f, 60f);
                Image oolCloseBtnImg = GetOrAddComponent<Image>(oolCloseBtnGo);
                oolCloseBtnImg.color = new Color(0.35f, 0.35f, 0.45f, 0.9f);
                Button oolCloseBtnComp = GetOrAddComponent<Button>(oolCloseBtnGo);

                GameObject oolCloseTextGo = CreateChildUI("Text", oolCloseBtnGo);
                RectTransform oolCloseTextRect = oolCloseTextGo.GetComponent<RectTransform>();
                oolCloseTextRect.anchorMin = Vector2.zero;
                oolCloseTextRect.anchorMax = Vector2.one;
                oolCloseTextRect.offsetMin = Vector2.zero;
                oolCloseTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI oolCloseTextTmp = GetOrAddComponent<TextMeshProUGUI>(oolCloseTextGo);
                oolCloseTextTmp.text = "KAPAT";
                oolCloseTextTmp.fontSize = 24;
                oolCloseTextTmp.alignment = TextAlignmentOptions.Center;
                oolCloseTextTmp.color = Color.white;

                Component oolDialogComp = GetOrAddComponent(outOfLivesGo, outOfLivesType);
                SerializedObject soOol = new SerializedObject(oolDialogComp);
                soOol.FindProperty("_contentRoot").objectReferenceValue = oolContentGo;
                soOol.FindProperty("_goldCostText").objectReferenceValue = oolBuyTextTmp;
                soOol.FindProperty("_buyWithGoldButton").objectReferenceValue = oolBuyBtnComp;
                soOol.FindProperty("_watchAdButton").objectReferenceValue = oolAdBtnComp;
                soOol.FindProperty("_closeButton").objectReferenceValue = oolCloseBtnComp;
                soOol.ApplyModifiedProperties();

                oolContentGo.SetActive(false);

                // 5. ExitConfirmDialog (Modal Dialog)
                GameObject exitGo = CreateChildUI("ExitConfirmDialog", canvasGo);
                RectTransform exitRect = exitGo.GetComponent<RectTransform>();
                exitRect.anchorMin = Vector2.zero;
                exitRect.anchorMax = Vector2.one;
                exitRect.offsetMin = Vector2.zero;
                exitRect.offsetMax = Vector2.zero;

                GameObject exitContentGo = CreateChildUI("Content", exitGo);
                RectTransform exitContentRect = exitContentGo.GetComponent<RectTransform>();
                exitContentRect.anchorMin = Vector2.zero;
                exitContentRect.anchorMax = Vector2.one;
                exitContentRect.offsetMin = Vector2.zero;
                exitContentRect.offsetMax = Vector2.zero;

                Image exitBackdrop = GetOrAddComponent<Image>(exitContentGo);
                exitBackdrop.color = new Color(0f, 0f, 0f, 0.75f);

                GameObject exitCardGo = CreateChildUI("Card", exitContentGo);
                RectTransform exitCardRect = exitCardGo.GetComponent<RectTransform>();
                exitCardRect.anchorMin = new Vector2(0.5f, 0.5f);
                exitCardRect.anchorMax = new Vector2(0.5f, 0.5f);
                exitCardRect.pivot = new Vector2(0.5f, 0.5f);
                exitCardRect.sizeDelta = new Vector2(600f, 400f);

                Image exitCardImg = GetOrAddComponent<Image>(exitCardGo);
                exitCardImg.color = new Color(0.18f, 0.10f, 0.28f, 0.98f);
                Outline exitCardOutline = GetOrAddComponent<Outline>(exitCardGo);
                exitCardOutline.effectColor = new Color(1f, 0.85f, 0.2f, 1f);
                exitCardOutline.effectDistance = new Vector2(3f, -3f);

                GameObject exitTitleGo = CreateChildUI("Title", exitCardGo);
                RectTransform exitTitleRect = exitTitleGo.GetComponent<RectTransform>();
                exitTitleRect.anchorMin = new Vector2(0.5f, 1f);
                exitTitleRect.anchorMax = new Vector2(0.5f, 1f);
                exitTitleRect.pivot = new Vector2(0.5f, 1f);
                exitTitleRect.anchoredPosition = new Vector2(0f, -40f);
                exitTitleRect.sizeDelta = new Vector2(540f, 60f);
                TextMeshProUGUI exitTitleTmp = GetOrAddComponent<TextMeshProUGUI>(exitTitleGo);
                exitTitleTmp.text = "SEVİYEDEN ÇIKILSIN MI?";
                exitTitleTmp.fontSize = 36;
                exitTitleTmp.fontStyle = FontStyles.Bold;
                exitTitleTmp.alignment = TextAlignmentOptions.Center;
                exitTitleTmp.color = Color.white;

                GameObject exitWarningGo = CreateChildUI("Warning", exitCardGo);
                RectTransform exitWarningRect = exitWarningGo.GetComponent<RectTransform>();
                exitWarningRect.anchorMin = new Vector2(0.5f, 1f);
                exitWarningRect.anchorMax = new Vector2(0.5f, 1f);
                exitWarningRect.pivot = new Vector2(0.5f, 1f);
                exitWarningRect.anchoredPosition = new Vector2(0f, -120f);
                exitWarningRect.sizeDelta = new Vector2(500f, 80f);
                TextMeshProUGUI exitWarningTmp = GetOrAddComponent<TextMeshProUGUI>(exitWarningGo);
                exitWarningTmp.text = "Aktif seviyeden çıkarsanız 1 Can kaybedeceksiniz!";
                exitWarningTmp.fontSize = 26;
                exitWarningTmp.alignment = TextAlignmentOptions.Center;
                exitWarningTmp.color = new Color(1f, 0.4f, 0.4f, 1f);

                GameObject exitCancelBtnGo = CreateChildUI("CancelBtn", exitCardGo);
                RectTransform exitCancelBtnRect = exitCancelBtnGo.GetComponent<RectTransform>();
                exitCancelBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                exitCancelBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                exitCancelBtnRect.pivot = new Vector2(0.5f, 0.5f);
                exitCancelBtnRect.anchoredPosition = new Vector2(-120f, -110f);
                exitCancelBtnRect.sizeDelta = new Vector2(210f, 65f);
                Image exitCancelImg = GetOrAddComponent<Image>(exitCancelBtnGo);
                exitCancelImg.color = new Color(0.35f, 0.35f, 0.45f, 1f);
                Button exitCancelComp = GetOrAddComponent<Button>(exitCancelBtnGo);

                GameObject exitCancelTextGo = CreateChildUI("Text", exitCancelBtnGo);
                RectTransform exitCancelTextRect = exitCancelTextGo.GetComponent<RectTransform>();
                exitCancelTextRect.anchorMin = Vector2.zero;
                exitCancelTextRect.anchorMax = Vector2.one;
                exitCancelTextRect.offsetMin = Vector2.zero;
                exitCancelTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI exitCancelTextTmp = GetOrAddComponent<TextMeshProUGUI>(exitCancelTextGo);
                exitCancelTextTmp.text = "VAZGEÇ";
                exitCancelTextTmp.fontSize = 24;
                exitCancelTextTmp.fontStyle = FontStyles.Bold;
                exitCancelTextTmp.alignment = TextAlignmentOptions.Center;
                exitCancelTextTmp.color = Color.white;

                GameObject exitConfirmBtnGo = CreateChildUI("ConfirmBtn", exitCardGo);
                RectTransform exitConfirmBtnRect = exitConfirmBtnGo.GetComponent<RectTransform>();
                exitConfirmBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                exitConfirmBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                exitConfirmBtnRect.pivot = new Vector2(0.5f, 0.5f);
                exitConfirmBtnRect.anchoredPosition = new Vector2(120f, -110f);
                exitConfirmBtnRect.sizeDelta = new Vector2(210f, 65f);
                Image exitConfirmImg = GetOrAddComponent<Image>(exitConfirmBtnGo);
                exitConfirmImg.color = new Color(0.85f, 0.25f, 0.25f, 1f);
                Button exitConfirmComp = GetOrAddComponent<Button>(exitConfirmBtnGo);

                GameObject exitConfirmTextGo = CreateChildUI("Text", exitConfirmBtnGo);
                RectTransform exitConfirmTextRect = exitConfirmTextGo.GetComponent<RectTransform>();
                exitConfirmTextRect.anchorMin = Vector2.zero;
                exitConfirmTextRect.anchorMax = Vector2.one;
                exitConfirmTextRect.offsetMin = Vector2.zero;
                exitConfirmTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI exitConfirmTextTmp = GetOrAddComponent<TextMeshProUGUI>(exitConfirmTextGo);
                exitConfirmTextTmp.text = "ÇIK (-1 CAN)";
                exitConfirmTextTmp.fontSize = 24;
                exitConfirmTextTmp.fontStyle = FontStyles.Bold;
                exitConfirmTextTmp.alignment = TextAlignmentOptions.Center;
                exitConfirmTextTmp.color = Color.white;

                Component exitDialogComp = GetOrAddComponent(exitGo, exitConfirmType);
                SerializedObject soExit = new SerializedObject(exitDialogComp);
                soExit.FindProperty("_contentRoot").objectReferenceValue = exitContentGo;
                soExit.FindProperty("_confirmExitButton").objectReferenceValue = exitConfirmComp;
                soExit.FindProperty("_cancelButton").objectReferenceValue = exitCancelComp;
                soExit.ApplyModifiedProperties();

                exitContentGo.SetActive(false);

                // 6. LivesWidget belongs to Main Menu (Section 17 & Phase 11). Remove from gameplay scene if present
                Transform existingWidget = canvasGo.transform.Find("LivesWidget");
                if (existingWidget != null)
                {
                    UnityEngine.Object.DestroyImmediate(existingWidget.gameObject);
                }

                // 7. HUD Exit Button (under Ball Counter, top-left)
                GameObject exitBtnGo = CreateChildUI("HUD_ExitButton", canvasGo);
                RectTransform exitBtnRect = exitBtnGo.GetComponent<RectTransform>();
                exitBtnRect.anchorMin = new Vector2(0f, 1f);
                exitBtnRect.anchorMax = new Vector2(0f, 1f);
                exitBtnRect.pivot = new Vector2(0f, 1f);
                exitBtnRect.anchoredPosition = new Vector2(48f, -200f);
                exitBtnRect.sizeDelta = new Vector2(100f, 40f);

                Image exitBtnImg = GetOrAddComponent<Image>(exitBtnGo);
                exitBtnImg.color = new Color(0.22f, 0.12f, 0.32f, 0.85f);
                Outline exitBtnOutline = GetOrAddComponent<Outline>(exitBtnGo);
                exitBtnOutline.effectColor = new Color(0.85f, 0.3f, 0.3f, 0.85f);
                exitBtnOutline.effectDistance = new Vector2(1.5f, -1.5f);

                GetOrAddComponent<Button>(exitBtnGo);

                var hudExitType = Type.GetType("RoyalSmash.UI.HUDExitButton, Assembly-CSharp");
                if (hudExitType != null)
                {
                    GetOrAddComponent(exitBtnGo, hudExitType);
                }

                GameObject exitBtnTextGo = CreateChildUI("Text", exitBtnGo);
                RectTransform exitBtnTextRect = exitBtnTextGo.GetComponent<RectTransform>();
                exitBtnTextRect.anchorMin = Vector2.zero;
                exitBtnTextRect.anchorMax = Vector2.one;
                exitBtnTextRect.offsetMin = Vector2.zero;
                exitBtnTextRect.offsetMax = Vector2.zero;
                TextMeshProUGUI exitBtnTmp = GetOrAddComponent<TextMeshProUGUI>(exitBtnTextGo);
                exitBtnTmp.text = "ÇIKIŞ";
                exitBtnTmp.fontSize = 20;
                exitBtnTmp.fontStyle = FontStyles.Bold;
                exitBtnTmp.alignment = TextAlignmentOptions.Center;
                exitBtnTmp.color = new Color(1f, 0.6f, 0.6f, 1f);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Phase8LivesSetup] Successfully configured Lives system in {scenePath}");
            }

            return "SUCCESS: Phase 8 Lives system setup complete across scenes.";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.ToString();
        }
    }

    private static Component GetOrAddComponent(GameObject go, Type type)
    {
        Component comp = go.GetComponent(type);
        if (comp == null) comp = go.AddComponent(type);
        return comp;
    }
}
