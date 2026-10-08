#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Editor showcase map preview containing every Citadel block type on the Stand.
    /// Allows inspecting materials, geometries, and visual alignment before starting the game.
    /// </summary>
    public static class SkyCitadelPreview
    {
        [MenuItem("Tools/Royal Smash/Rebuild Showcase Preview")]
        public static void MenuRebuildShowcase()
        {
            var go = GameObject.Find("SkyCitadelPreview");
            if (go == null)
            {
                go = new GameObject("SkyCitadelPreview");
                go.tag = "EditorOnly";
            }
            BuildShowcaseHierarchy(go);
            EditorUtility.SetDirty(go);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
        }

        public static void BuildShowcaseHierarchy(GameObject root)
        {
            // Clear existing children
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }

            var woodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_wood.prefab");
            var stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_stone.prefab");
            var armoredPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_armored.prefab");
            var crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_crystal.prefab");
            var corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_core.prefab");

            if (woodPrefab == null || stonePrefab == null || armoredPrefab == null || crystalPrefab == null || corePrefab == null)
            {
                Debug.LogError("[SkyCitadelPreview] Could not find all 5 block prefabs in Assets/_Project/Art/SkyCitadel/Prefabs/!");
                return;
            }

            const float standZ = 9.00f;

            var layout = new (Vector3 pos, GameObject prefab, string label)[]
            {
                // Layer 0: Ground Foundation (7 blocks, Y = 1.845)
                (new Vector3(-2.265f, 1.845f, standZ), armoredPrefab, "01_Armored_Wing_L"),
                (new Vector3(-1.510f, 1.845f, standZ), stonePrefab,   "02_Stone_Base_L"),
                (new Vector3(-0.755f, 1.845f, standZ), woodPrefab,    "03_Wood_Base_ML"),
                (new Vector3( 0.000f, 1.845f, standZ), woodPrefab,    "04_Wood_Base_Center"),
                (new Vector3( 0.755f, 1.845f, standZ), woodPrefab,    "05_Wood_Base_MR"),
                (new Vector3( 1.510f, 1.845f, standZ), stonePrefab,   "06_Stone_Base_R"),
                (new Vector3( 2.265f, 1.845f, standZ), armoredPrefab, "07_Armored_Wing_R"),

                // Layer 1: Crystal Spires & Core (5 blocks, Y = 2.590)
                (new Vector3(-1.510f, 2.590f, standZ), crystalPrefab, "08_Crystal_Tower_L"),
                (new Vector3(-0.755f, 2.590f, standZ), crystalPrefab, "09_Crystal_Inner_L"),
                (new Vector3( 0.000f, 2.590f, standZ), corePrefab,    "10_Core_Heart"),
                (new Vector3( 0.755f, 2.590f, standZ), crystalPrefab, "11_Crystal_Inner_R"),
                (new Vector3( 1.510f, 2.590f, standZ), crystalPrefab, "12_Crystal_Tower_R"),

                // Layer 2: Mid Bastion & Energy Cells (4 blocks, Y = 3.335)
                (new Vector3(-1.1325f, 3.335f, standZ), woodPrefab,   "13_Wood_Tier_L"),
                (new Vector3(-0.3775f, 3.335f, standZ), corePrefab,   "14_Core_Chamber_L"),
                (new Vector3( 0.3775f, 3.335f, standZ), corePrefab,   "15_Core_Chamber_R"),
                (new Vector3( 1.1325f, 3.335f, standZ), woodPrefab,   "16_Wood_Tier_R"),

                // Layer 3: Upper Ramparts (3 blocks, Y = 4.080)
                (new Vector3(-0.755f, 4.080f, standZ), crystalPrefab, "17_Crystal_Crown_L"),
                (new Vector3( 0.000f, 4.080f, standZ), stonePrefab,   "18_Stone_Pedestal"),
                (new Vector3( 0.755f, 4.080f, standZ), crystalPrefab, "19_Crystal_Crown_R"),

                // Layer 4: Pinnacle Crown Core (1 block, Y = 4.825)
                (new Vector3( 0.000f, 4.825f, standZ), corePrefab,    "20_Core_Pinnacle")
            };

            foreach (var item in layout)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(item.prefab, root.transform);
                instance.name = item.label;
                instance.transform.position = item.pos;
                instance.transform.rotation = Quaternion.identity;

                var rb = instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }

            Debug.Log($"[SkyCitadelPreview] Successfully built showcase layout with {layout.Length} blocks across all 5 types.");
        }
    }
}
#endif
