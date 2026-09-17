using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using RoyalSmash.Economy;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    public static class Phase10Setup
    {
        private const string ECONOMY_CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/EconomyConfig.asset";
        private const string REWARD_CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/LevelRewardConfig.asset";

        [MenuItem("RoyalSmash/Phase 10/Setup Gold Economy & HUD")]
        public static void RunSetup()
        {
            var economyConfig = EnsureEconomyConfig();
            var rewardConfig = AssetDatabase.LoadAssetAtPath<LevelRewardConfig>(REWARD_CONFIG_PATH);

            SetupScene("Assets/_Project/Scenes/MainGame.unity", economyConfig, rewardConfig);
            SetupScene("Assets/_Project/Scenes/PhysicsTestScene.unity", economyConfig, rewardConfig);

            AssetDatabase.SaveAssets();
            Debug.Log("[Phase10Setup] Phase 10 Gold Economy & HUD Setup Completed Successfully!");
        }

        public static EconomyConfig EnsureEconomyConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<EconomyConfig>(ECONOMY_CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EconomyConfig>();
                string dir = Path.GetDirectoryName(ECONOMY_CONFIG_PATH);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(config, ECONOMY_CONFIG_PATH);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Phase10Setup] Created EconomyConfig at {ECONOMY_CONFIG_PATH}");
            }
            return config;
        }

        public static void SetupScene(string scenePath, EconomyConfig economyConfig, LevelRewardConfig rewardConfig)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Phase10Setup] Could not open scene: {scenePath}");
                return;
            }

            // 1. Setup GoldManager GameObject
            var goldMgrObj = GameObject.Find("GoldManager");
            if (goldMgrObj == null)
            {
                goldMgrObj = new GameObject("GoldManager");
                Undo.RegisterCreatedObjectUndo(goldMgrObj, "Create GoldManager");
            }

            var goldMgr = goldMgrObj.GetComponent<GoldManager>();
            if (goldMgr == null)
            {
                goldMgr = goldMgrObj.AddComponent<GoldManager>();
            }

            // Assign configs via SerializedObject
            var soGold = new SerializedObject(goldMgr);
            soGold.FindProperty("_economyConfig").objectReferenceValue = economyConfig;
            soGold.FindProperty("_rewardConfig").objectReferenceValue = rewardConfig;
            soGold.ApplyModifiedProperties();

            // 2. Setup HUD_GoldDisplay inside MainCanvas
            var mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                var hudGold = mainCanvas.transform.Find("HUD_GoldDisplay");
                GameObject goldObj;
                if (hudGold == null)
                {
                    goldObj = new GameObject("HUD_GoldDisplay");
                    goldObj.transform.SetParent(mainCanvas.transform, false);
                }
                else
                {
                    goldObj = hudGold.gameObject;
                }

                var rect = goldObj.GetComponent<RectTransform>();
                if (rect == null) rect = goldObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(30f, -30f);
                rect.sizeDelta = new Vector2(160f, 48f);

                // Background card image
                var img = goldObj.GetComponent<Image>();
                if (img == null) img = goldObj.AddComponent<Image>();
                img.color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

                // Icon
                var iconTrans = goldObj.transform.Find("Icon");
                GameObject iconObj;
                if (iconTrans == null)
                {
                    iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(goldObj.transform, false);
                }
                else
                {
                    iconObj = iconTrans.gameObject;
                }
                var iconRect = iconObj.GetComponent<RectTransform>();
                if (iconRect == null) iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(10f, 0f);
                iconRect.sizeDelta = new Vector2(32f, 32f);

                var iconTmp = iconObj.GetComponent<TextMeshProUGUI>();
                if (iconTmp == null) iconTmp = iconObj.AddComponent<TextMeshProUGUI>();
                iconTmp.text = "🪙";
                iconTmp.fontSize = 24;
                iconTmp.alignment = TextAlignmentOptions.Center;

                // Value Text
                var textTrans = goldObj.transform.Find("GoldText");
                GameObject textObj;
                if (textTrans == null)
                {
                    textObj = new GameObject("GoldText");
                    textObj.transform.SetParent(goldObj.transform, false);
                }
                else
                {
                    textObj = textTrans.gameObject;
                }
                var textRect = textObj.GetComponent<RectTransform>();
                if (textRect == null) textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.offsetMin = new Vector2(48f, 0f);
                textRect.offsetMax = new Vector2(-10f, 0f);

                var valueTmp = textObj.GetComponent<TextMeshProUGUI>();
                if (valueTmp == null) valueTmp = textObj.AddComponent<TextMeshProUGUI>();
                valueTmp.text = "1,000";
                valueTmp.fontSize = 22;
                valueTmp.fontStyle = FontStyles.Bold;
                valueTmp.color = new Color(1.0f, 0.85f, 0.25f, 1.0f); // Sleek Gold color
                valueTmp.alignment = TextAlignmentOptions.Left;

                // GoldDisplayUI component
                var displayUI = goldObj.GetComponent<GoldDisplayUI>();
                if (displayUI == null) displayUI = goldObj.AddComponent<GoldDisplayUI>();
                displayUI.BindReferences(valueTmp, rect, null);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Phase10Setup] Configured GoldManager and HUD_GoldDisplay in {scenePath}");
        }
    }
}
