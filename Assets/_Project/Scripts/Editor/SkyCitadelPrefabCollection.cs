using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Exports the current art into an inspectable catalog without rebuilding MainGame.</summary>
    public static class SkyCitadelPrefabCollection
    {
        public const string Folder = "Assets/_Project/Prefabs/SkyCitadel";
        private const string Art = "Assets/_Project/Art/SkyCitadel";
        private static readonly string[] Variants = { "wood", "stone", "crystal", "armored", "core" };
        private static readonly string[] Names = { "Wood", "Stone", "Crystal", "Armored", "Core" };

        [MenuItem("Royal Smash/Sky Citadel/Export Prefab Collection")]
        public static void Export()
        {
            var main = SceneManager.GetActiveScene();
            if (Application.isPlaying || main.path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Export the collection from MainGame in Edit Mode.");
            var sourceMortar = GameObject.Find("Mortar");
            if (sourceMortar == null) throw new InvalidOperationException("MainGame Mortar is missing.");
            EnsureFolder(Folder);
            EnsureFolder(Folder + "/Blocks");
            var preview = EditorSceneManager.NewPreviewScene();
            var workspace = new GameObject("CollectionExportWorkspace");
            SceneManager.MoveGameObjectToScene(workspace, preview);
            try
            {
                var blocks = new GameObject[Variants.Length];
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = SaveVariant(Load(Art + "/Prefabs/PF_" + Variants[i] + ".prefab"),
                        Folder + "/Blocks/PF_Block_0" + (i + 1) + "_" + Names[i] + ".prefab", workspace.transform);
                var stand = SaveVariant(Load(Art + "/Prefabs/PF_CitadelStand.prefab"),
                    Folder + "/PF_03_Stand.prefab", workspace.transform);
                var ball = SaveVariant(Load(Art + "/Prefabs/PF_CitadelBall.prefab"),
                    Folder + "/PF_04_EnergyBall.prefab", workspace.transform);
                var mortar = UnityEngine.Object.Instantiate(sourceMortar, workspace.transform);
                mortar.name = "PF_02_Mortar_Complete";
                Unpack(mortar);
                mortar.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                var controller = mortar.GetComponent<MortarController>();
                var serialized = new SerializedObject(controller);
                serialized.FindProperty("_inputHandler").objectReferenceValue = null;
                serialized.FindProperty("_aimTransform").objectReferenceValue = mortar.transform.Find("Visuals");
                serialized.FindProperty("_firePoint").objectReferenceValue = mortar.transform.Find("Visuals/BarrelPivot/FirePoint");
                serialized.FindProperty("_ballPrefab").objectReferenceValue = ball;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var mortarPrefab = Save(mortar, Folder + "/PF_02_Mortar_Complete.prefab");

                var data = AssetDatabase.LoadAssetAtPath<LevelData>(Art + "/Levels/LevelData_Citadel01.asset");
                if (data == null) throw new InvalidOperationException("Citadel level data is missing.");
                var tower = Child(workspace.transform, "PF_01_Tower_26Blocks");
                foreach (var placement in data.Stands)
                    Place(stand, tower.transform, placement.Position - new Vector3(0, 0, 7.5f), placement.Rotation, placement.Scale);
                int[] counts = new int[blocks.Length];
                foreach (var placement in data.Objects)
                {
                    int type = (int)placement.Type;
                    var block = Place(blocks[type], tower.transform, placement.Position - new Vector3(0, 0, 7.5f),
                        placement.Rotation, placement.Scale);
                    block.name = Names[type] + "_" + (++counts[type]).ToString("00");
                }
                var towerPrefab = Save(tower, Folder + "/PF_01_Tower_26Blocks.prefab");

                var gallery = Child(workspace.transform, "PF_00_SkyCitadel_Showcase");
                var stone = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/M_IvoryStone.mat");
                var dark = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/M_ObsidianSteel.mat");
                var gold = AssetDatabase.LoadAssetAtPath<Material>(Art + "/Materials/M_AntiqueGold.mat");
                Panel(gallery.transform, "DisplayFloor", new Vector3(0, -.2f, 0), new Vector3(15, .18f, 6), stone);
                Panel(gallery.transform, "CatalogBackdrop", new Vector3(0, 4.1f, 2.7f), new Vector3(15, 9, .15f), dark);
                Label(gallery.transform, "SKY CITADEL  /  PREFAB COLLECTION", new Vector3(0, 8.1f, -.9f), 4, 15);
                Label(gallery.transform, "5 BLOCK TYPES  |  26 BLOCKS IN TOWER", new Vector3(0, 7.5f, -.9f), 2.6f, 14);
                for (int i = 0; i < blocks.Length; i++)
                {
                    float x = (i - 2) * 2.85f;
                    Visual(blocks[i], gallery.transform, "0" + (i + 1) + "_" + Names[i],
                        new Vector3(x, 6.15f, 0), new Vector3(-10, -25, 0), Vector3.one * 1.15f);
                    Panel(gallery.transform, Names[i] + "_DisplayShelf", new Vector3(x, 5.48f, 0), new Vector3(2.3f, .08f, 1.6f), gold);
                    Label(gallery.transform, "0" + (i + 1) + "  " + Names[i].ToUpperInvariant(),
                        new Vector3(x, 5.03f, -.9f), 2.8f, 2.8f);
                    Label(gallery.transform, counts[i] + " / 26", new Vector3(x, 4.62f, -.9f), 2.4f, 2.8f);
                }
                Visual(towerPrefab, gallery.transform, "Tower_26_Blocks_With_Stand",
                    new Vector3(-3.7f, 0, .4f), Vector3.zero, Vector3.one * .72f);
                Label(gallery.transform, "26 BLOCKS + STAND", new Vector3(-3.7f, -.65f, -1.1f), 3, 6.3f);
                Visual(mortarPrefab, gallery.transform, "Complete_Mortar_All_Sides",
                    new Vector3(2.3f, 1.85f, .1f), new Vector3(0, -125, 0), Vector3.one * 1.8f);
                Label(gallery.transform, "COMPLETE MORTAR", new Vector3(2.3f, -.65f, -1.1f), 3, 4.9f);
                Visual(ball, gallery.transform, "Energy_Ball",
                    new Vector3(6, 1.2f, -.4f), Vector3.zero, Vector3.one * .85f);
                Label(gallery.transform, "ENERGY BALL", new Vector3(6, -.65f, -1.1f), 2.3f, 2.7f);
                var catalog = Save(gallery, Folder + "/PF_00_SkyCitadel_Showcase.prefab");
                AssetDatabase.SetLabels(catalog, new[] { "SkyCitadel", "Showcase", "StartHere" });
                AssetDatabase.SaveAssets();
                Debug.Log("[Sky Citadel] Exported 10 prefabs: showcase, tower, complete mortar, stand, ball and 5 block types.");
                Selection.activeObject = catalog;
                EditorGUIUtility.PingObject(catalog);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        [MenuItem("Royal Smash/Sky Citadel/Open Prefab Collection")]
        public static void Open()
        {
            var path = Folder + "/PF_00_SkyCitadel_Showcase.prefab";
            var prefab = Load(path);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            PrefabStageUtility.OpenPrefab(path);
            // Prefab Mode performs its own framing when entering. Frame after that transition.
            EditorApplication.delayCall += () =>
            {
                var view = SceneView.lastActiveSceneView;
                if (view == null) return;
                view.drawGizmos = false;
                view.LookAt(new Vector3(0, 3.7f, 0), Quaternion.Euler(4, 0, 0), 9, true, true);
                view.Repaint();
            };
        }

        private static GameObject SaveVariant(GameObject source, string path, Transform parent)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = Path.GetFileNameWithoutExtension(path);
            return Save(instance, path);
        }

        private static GameObject Save(GameObject root, string path)
        {
            var result = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (result == null) throw new InvalidOperationException("Could not save " + path);
            UnityEngine.Object.DestroyImmediate(root);
            return result;
        }

        private static GameObject Place(GameObject prefab, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            var result = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            result.transform.localPosition = position;
            result.transform.localRotation = Quaternion.Euler(rotation);
            result.transform.localScale = scale;
            return result;
        }

        private static GameObject Visual(GameObject prefab, Transform parent, string name, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            var result = Place(prefab, parent, position, rotation, scale);
            Unpack(result);
            result.name = name;
            // The catalog is a visual display. Independent gameplay prefabs retain these components.
            foreach (var script in result.GetComponentsInChildren<MonoBehaviour>(true))
                UnityEngine.Object.DestroyImmediate(script);
            foreach (var collider in result.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            foreach (var body in result.GetComponentsInChildren<Rigidbody>(true))
                UnityEngine.Object.DestroyImmediate(body);
            return result;
        }

        private static void Unpack(GameObject instance)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(instance))
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        private static GameObject Child(Transform parent, string name)
        {
            var child = new GameObject(name);
            SceneManager.MoveGameObjectToScene(child, parent.gameObject.scene);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Panel(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SceneManager.MoveGameObjectToScene(panel, parent.gameObject.scene);
            panel.name = name;
            panel.transform.SetParent(parent, false);
            panel.transform.localPosition = position;
            panel.transform.localScale = scale;
            panel.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(panel.GetComponent<Collider>());
        }

        private static void Label(Transform parent, string text, Vector3 position, float size, float width)
        {
            var label = Child(parent, text);
            label.transform.localPosition = position;
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1, .88f, .58f);
            tmp.rectTransform.sizeDelta = new Vector2(width, .7f);
        }

        private static GameObject Load(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new InvalidOperationException("Missing prefab: " + path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }
    }
}
