using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Phase7TutorialSetup
{
    public static string Execute()
    {
        try
        {
            // 1. Create or load TutorialConfig.asset
            string configPath = "Assets/_Project/ScriptableObjects/Config/TutorialConfig.asset";
            var tutConfigType = Type.GetType("RoyalSmash.Tutorial.TutorialConfig, Assembly-CSharp");
            if (tutConfigType == null) return "Error: RoyalSmash.Tutorial.TutorialConfig type not found";

            ScriptableObject tutConfig = AssetDatabase.LoadAssetAtPath(configPath, tutConfigType) as ScriptableObject;
            if (tutConfig == null)
            {
                tutConfig = ScriptableObject.CreateInstance(tutConfigType);
                AssetDatabase.CreateAsset(tutConfig, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[Phase7TutorialSetup] Created TutorialConfig.asset");
            }

            // 2. Load Sprites
            string ringTexPath = "Assets/_Project/Textures/ReticleRing.png";
            Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ringTexPath);

            string arrowTexPath = "Assets/_Project/Textures/PointerArrow.png";
            Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(arrowTexPath);

            // 3. Setup Scenes: MainGame and PhysicsTestScene
            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainGame.unity",
                "Assets/_Project/Scenes/PhysicsTestScene.unity"
            };

            var tutMgrType = Type.GetType("RoyalSmash.Tutorial.TutorialManager, Assembly-CSharp");
            var tutOverlayType = Type.GetType("RoyalSmash.Tutorial.TutorialOverlayUI, Assembly-CSharp");
            var saveMgrType = Type.GetType("RoyalSmash.Save.SaveManager, Assembly-CSharp");

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath);

                // Ensure SaveManager
                GameObject saveMgrGo = GameObject.Find("SaveManager");
                if (saveMgrGo == null)
                {
                    saveMgrGo = new GameObject("SaveManager");
                    saveMgrGo.AddComponent(saveMgrType);
                }

                // Ensure MainCanvas
                GameObject canvasGo = GameObject.Find("MainCanvas");
                if (canvasGo == null)
                {
                    Debug.LogWarning($"MainCanvas not found in {scenePath}");
                    continue;
                }

                // Find Tutorial target (prefer front-center physics target, e.g. Cone at Z~6.4)
                Transform targetTransform = null;
                var physList = GameObject.FindObjectsByType<RoyalSmash.Gameplay.PhysicsObject>(FindObjectsSortMode.None);
                float minZ = float.MaxValue;
                foreach (var p in physList)
                {
                    if (Mathf.Abs(p.transform.position.x) < 0.6f && p.transform.position.z < minZ)
                    {
                        minZ = p.transform.position.z;
                        targetTransform = p.transform;
                    }
                }
                if (targetTransform == null)
                {
                    var colGo = GameObject.Find("Column");
                    if (colGo != null) targetTransform = colGo.transform;
                }

                // Build / Update TutorialOverlay under MainCanvas
                Transform existingOverlay = canvasGo.transform.Find("TutorialOverlay");
                GameObject overlayGo;
                if (existingOverlay != null)
                {
                    overlayGo = existingOverlay.gameObject;
                }
                else
                {
                    overlayGo = new GameObject("TutorialOverlay");
                    overlayGo.transform.SetParent(canvasGo.transform, false);
                }

                // Setup RectTransform full screen
                RectTransform overlayRect = overlayGo.GetComponent<RectTransform>();
                if (overlayRect == null) overlayRect = overlayGo.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                // CanvasGroup (Strictly non-blocking!)
                CanvasGroup cg = overlayGo.GetComponent<CanvasGroup>();
                if (cg == null) cg = overlayGo.AddComponent<CanvasGroup>();
                cg.blocksRaycasts = false;
                cg.interactable = false;
                cg.alpha = 1f;

                // Component TutorialOverlayUI
                Component tutOverlayComp = overlayGo.GetComponent(tutOverlayType);
                if (tutOverlayComp == null) tutOverlayComp = overlayGo.AddComponent(tutOverlayType);

                // Child 1: TargetRing
                Transform existingRing = overlayGo.transform.Find("TargetRing");
                GameObject ringGo = existingRing != null ? existingRing.gameObject : new GameObject("TargetRing");
                ringGo.transform.SetParent(overlayGo.transform, false);
                RectTransform ringRect = ringGo.GetComponent<RectTransform>();
                if (ringRect == null) ringRect = ringGo.AddComponent<RectTransform>();
                ringRect.sizeDelta = new Vector2(170f, 170f);
                Image ringImg = ringGo.GetComponent<Image>();
                if (ringImg == null) ringImg = ringGo.AddComponent<Image>();
                ringImg.sprite = ringSprite;
                ringImg.color = new Color(1f, 0.85f, 0.15f, 0.95f); // Vibrant Royal Gold
                ringImg.raycastTarget = false;

                // Child 2: PointerHand (Using PointerArrow image)
                Transform existingHand = overlayGo.transform.Find("PointerHand");
                GameObject handGo = existingHand != null ? existingHand.gameObject : new GameObject("PointerHand");
                handGo.transform.SetParent(overlayGo.transform, false);
                RectTransform handRect = handGo.GetComponent<RectTransform>();
                if (handRect == null) handRect = handGo.AddComponent<RectTransform>();
                handRect.sizeDelta = new Vector2(70f, 70f);

                var oldTmp = handGo.GetComponent<TextMeshProUGUI>();
                if (oldTmp != null) UnityEngine.Object.DestroyImmediate(oldTmp);

                Image handImg = handGo.GetComponent<Image>();
                if (handImg == null) handImg = handGo.AddComponent<Image>();
                handImg.sprite = arrowSprite;
                handImg.color = new Color(1f, 0.88f, 0.2f, 1f); // Vibrant Gold
                handImg.raycastTarget = false;

                // Child 3: InstructionBanner (Placed nicely at top-center)
                Transform existingBanner = overlayGo.transform.Find("InstructionBanner");
                GameObject bannerGo = existingBanner != null ? existingBanner.gameObject : new GameObject("InstructionBanner");
                bannerGo.transform.SetParent(overlayGo.transform, false);
                RectTransform bannerRect = bannerGo.GetComponent<RectTransform>();
                if (bannerRect == null) bannerRect = bannerGo.AddComponent<RectTransform>();
                bannerRect.anchorMin = new Vector2(0.5f, 1f);
                bannerRect.anchorMax = new Vector2(0.5f, 1f);
                bannerRect.pivot = new Vector2(0.5f, 1f);
                bannerRect.anchoredPosition = new Vector2(0f, -140f);
                bannerRect.sizeDelta = new Vector2(620f, 68f);

                Image bannerImg = bannerGo.GetComponent<Image>();
                if (bannerImg == null) bannerImg = bannerGo.AddComponent<Image>();
                bannerImg.color = new Color(0.12f, 0.08f, 0.22f, 0.92f);
                bannerImg.raycastTarget = false;

                Outline bannerOutline = bannerGo.GetComponent<Outline>();
                if (bannerOutline == null) bannerOutline = bannerGo.AddComponent<Outline>();
                bannerOutline.effectColor = new Color(1f, 0.85f, 0.2f, 0.90f);
                bannerOutline.effectDistance = new Vector2(2.5f, -2.5f);

                Transform existingText = bannerGo.transform.Find("Text");
                GameObject bannerTextGo = existingText != null ? existingText.gameObject : new GameObject("Text");
                bannerTextGo.transform.SetParent(bannerGo.transform, false);
                RectTransform bannerTextRect = bannerTextGo.GetComponent<RectTransform>();
                if (bannerTextRect == null) bannerTextRect = bannerTextGo.AddComponent<RectTransform>();
                bannerTextRect.anchorMin = Vector2.zero;
                bannerTextRect.anchorMax = Vector2.one;
                bannerTextRect.offsetMin = new Vector2(10f, 0f);
                bannerTextRect.offsetMax = new Vector2(-10f, 0f);

                TextMeshProUGUI bannerTmp = bannerTextGo.GetComponent<TextMeshProUGUI>();
                if (bannerTmp == null) bannerTmp = bannerTextGo.AddComponent<TextMeshProUGUI>();
                bannerTmp.text = "HEDEFE DOKUN VE FIRLAT!";
                bannerTmp.fontSize = 32;
                bannerTmp.fontStyle = FontStyles.Bold;
                bannerTmp.alignment = TextAlignmentOptions.Center;
                bannerTmp.color = Color.white;
                bannerTmp.raycastTarget = false;

                // Serialize TutorialOverlayUI references
                SerializedObject soUI = new SerializedObject(tutOverlayComp);
                soUI.FindProperty("_canvasGroup").objectReferenceValue = cg;
                soUI.FindProperty("_targetRingRect").objectReferenceValue = ringRect;
                soUI.FindProperty("_targetRingImage").objectReferenceValue = ringImg;
                soUI.FindProperty("_pointerHandRect").objectReferenceValue = handRect;
                soUI.FindProperty("_instructionText").objectReferenceValue = bannerTmp;
                soUI.FindProperty("_bannerRect").objectReferenceValue = bannerRect;
                soUI.ApplyModifiedProperties();

                // Setup TutorialManager GameObject
                GameObject tutMgrGo = GameObject.Find("TutorialManager");
                if (tutMgrGo == null)
                {
                    tutMgrGo = new GameObject("TutorialManager");
                }
                Component tutMgrComp = tutMgrGo.GetComponent(tutMgrType);
                if (tutMgrComp == null) tutMgrComp = tutMgrGo.AddComponent(tutMgrType);

                SerializedObject soMgr = new SerializedObject(tutMgrComp);
                soMgr.FindProperty("_config").objectReferenceValue = tutConfig;
                soMgr.FindProperty("_targetTransform").objectReferenceValue = targetTransform;
                soMgr.FindProperty("_overlayUI").objectReferenceValue = tutOverlayComp;
                soMgr.ApplyModifiedProperties();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Phase7TutorialSetup] Successfully configured scene: {scenePath} (target={targetTransform?.name})");
            }

            return "SUCCESS: Phase 7 Tutorial Setup complete across scenes.";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.ToString();
        }
    }
}
