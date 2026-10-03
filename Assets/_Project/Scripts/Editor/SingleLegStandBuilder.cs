using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Generates and maintains the Single-Leg (Narrow) Stand prefab used for multi-stand levels.
    /// Preserves the original PF_CitadelStand completely untouched.
    /// </summary>
    public static class SingleLegStandBuilder
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        public const string PREFAB_PATH = ART + "/Prefabs/PF_CitadelStand_SingleLeg.prefab";
        private const string MESHES_PATH = ART + "/Meshes/Phase32_StandSingleLeg_";

        [MenuItem("Royal Smash/Sky Citadel/Build Single-Leg Stand Prefab", false, 50)]
        public static GameObject BuildPrefab()
        {
            Material ivory = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_IvoryStone.mat");
            Material gold = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_AntiqueGold.mat");
            Material steel = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_ObsidianSteel.mat");
            Material blue = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_RoyalSapphire.mat");
            Material cyan = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_Phase32StandCyan.mat");

            Mesh bevel = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            Mesh octagon = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/Phase32_Stand_OctagonalPrism.asset");
            Mesh shield = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/Phase32_Stand_Shield.asset");
            Mesh gem = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/Phase32_Stand_Gem.asset");

            if (ivory == null || gold == null || steel == null || blue == null || cyan == null)
            {
                Debug.LogError("[SingleLegStandBuilder] Missing Citadel materials!");
                return null;
            }

            if (bevel == null || octagon == null || shield == null || gem == null)
            {
                Debug.LogError("[SingleLegStandBuilder] Missing Citadel base meshes!");
                return null;
            }

            // Create working root GameObject in memory
            var root = new GameObject("PF_CitadelStand_SingleLeg");
            int standLayer = LayerMask.NameToLayer("Stand");
            if (standLayer != -1) root.layer = standLayer;

            try
            {
                // 1. StandPresentation (Contains the physical BoxCollider and fallback geometry)
                // Width = 3.20m, Height = 0.25m, Depth = 1.90m
                var presentation = new GameObject("StandPresentation");
                presentation.transform.SetParent(root.transform, false);
                presentation.transform.localPosition = new Vector3(0f, 1.35f, 0f);
                presentation.transform.localScale = new Vector3(3.20f, 0.25f, 1.90f);
                if (standLayer != -1) presentation.layer = standLayer;

                var tabletop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tabletop.name = "MarbleTabletop";
                tabletop.transform.SetParent(presentation.transform, false);
                tabletop.transform.localPosition = Vector3.zero;
                tabletop.transform.localScale = Vector3.one;
                if (standLayer != -1) tabletop.layer = standLayer;
                var tabletopRenderer = tabletop.GetComponent<MeshRenderer>();
                if (tabletopRenderer != null) tabletopRenderer.sharedMaterial = ivory;

                var boxCollider = tabletop.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.center = Vector3.zero;
                    boxCollider.size = Vector3.one;
                }

                // Single Center Pillar in Presentation
                CreatePresentationPillar(presentation.transform, ivory, gold, blue, standLayer);

                // 2. Phase32StandDetails (High quality Sky Citadel PBR detailing)
                var details = new GameObject("Phase32StandDetails");
                details.transform.SetParent(root.transform, false);
                details.transform.localPosition = Vector3.zero;
                details.transform.localScale = Vector3.one;
                if (standLayer != -1) details.layer = standLayer;

                var b = new Builder(bevel, octagon, shield, gem);
                b.Register("Ivory", ivory);
                b.Register("Gold", gold);
                b.Register("Steel", steel);
                b.Register("Sapphire", blue);
                b.Register("Energy", cyan);

                // Build beam of width 3.20m
                BuildNarrowBeam(b, ivory, gold, steel, blue, cyan);

                // Build ONE center leg at X = 0
                BuildCenterFoot(b, 0f, ivory, gold, steel, blue, cyan);

                // Build end banners on left and right edges (X = +- 1.40f)
                for (int side = -1; side <= 1; side += 2)
                {
                    BuildBanner(b, side * 1.40f, ivory, gold, steel, blue, cyan);
                }

                b.Bake(details.transform, MESHES_PATH);

                // Save as Prefab
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
                Debug.Log($"[SingleLegStandBuilder] Successfully created Single-Leg Stand Prefab at: {PREFAB_PATH}");
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreatePresentationPillar(Transform parent, Material ivory, Material gold, Material sapphire, int layer)
        {
            // Center pillar components (local X = 0)
            CreateChildMesh(parent, PrimitiveType.Cylinder, "IvoryPedestal", new Vector3(0f, -2.80f, 0f), new Vector3(0.12f, 2.10f, 0.40f), ivory, layer);
            CreateChildMesh(parent, PrimitiveType.Cylinder, "PedestalCapital", new Vector3(0f, -0.95f, 0f), new Vector3(0.18f, 0.30f, 0.60f), gold, layer);
            CreateChildMesh(parent, PrimitiveType.Cylinder, "PedestalFoot", new Vector3(0f, -4.90f, 0f), new Vector3(0.22f, 0.50f, 0.74f), gold, layer);
            CreateChildMesh(parent, PrimitiveType.Cube, "PedestalInset", new Vector3(0f, -2.85f, -0.19f), new Vector3(0.05f, 2.40f, 0.03f), sapphire, layer);
        }

        private static void CreateChildMesh(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, int layer)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (layer != -1) go.layer = layer;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            var coll = go.GetComponent<Collider>();
            if (coll != null) UnityEngine.Object.DestroyImmediate(coll);
        }

        private static void BuildNarrowBeam(Builder b, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            // Beam length: 3.20m (width along X)
            b.Box(ivory, V(0, 1.255f, 0), V(3.16f, 0.38f, 1.89f));
            b.Box(steel, V(0, 1.078f, 0), V(3.23f, 0.17f, 1.96f));
            b.Box(gold, V(0, 1.436f, 0), V(3.24f, 0.065f, 1.97f));
            b.Box(gold, V(0, 1.022f, -0.99f), V(3.15f, 0.054f, 0.11f));

            // Side frieze details along X
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(steel, V(side * 0.85f, 1.235f, -0.974f), V(1.00f, 0.18f, 0.058f));
                b.Box(blue, V(side * 0.85f, 1.225f, -1.017f), V(0.92f, 0.105f, 0.046f));
                b.Box(cyan, V(side * 0.85f, 1.226f, -1.045f), V(0.75f, 0.027f, 0.016f));
                b.Box(gold, V(side * 0.85f, 1.137f, -1.047f), V(0.90f, 0.042f, 0.071f));
                b.Box(gold, V(side * 1.35f, 1.392f, 0), V(0.40f, 0.13f, 2.02f));
                b.Box(steel, V(side * 1.45f, 1.24f, -1.045f), V(0.12f, 0.20f, 0.095f));
                b.Oct(cyan, V(side * 1.45f, 1.26f, -1.108f), V(0.054f, 0.016f, 0.054f), Quaternion.Euler(90, 0, 0));
                b.Oct(gold, V(side * 0.65f, 1.32f, -1.052f), V(0.036f, 0.017f, 0.036f), Quaternion.Euler(90, 0, 0));
            }

            // Center Diamond Crest directly aligned above the single center leg
            b.Box(steel, V(0, 1.12f, -1.028f), V(0.565f, 0.45f, 0.095f));
            b.Box(gold, V(0, 1.115f, -1.115f), V(0.49f, 0.49f, 0.15f), 45f);
            b.Box(blue, V(0, 1.115f, -1.207f), V(0.378f, 0.378f, 0.062f), 45f);
            b.Gem(cyan, V(0, 1.115f, -1.247f), V(0.415f, 0.49f, 0.26f));
        }

        private static void BuildCenterFoot(Builder b, float x, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            b.Oct(steel, V(x, 0.15f, 0), V(1.27f, 0.30f, 1.77f));
            b.Oct(gold, V(x, 0.048f, 0), V(1.29f, 0.055f, 1.80f));
            b.Oct(gold, V(x, 0.286f, 0), V(1.17f, 0.045f, 1.62f));
            b.Oct(steel, V(x, 0.327f, 0), V(0.99f, 0.070f, 1.42f));
            b.Oct(ivory, V(x, 0.705f, 0), V(0.755f, 0.77f, 1.34f));
            b.Oct(gold, V(x, 0.371f, 0), V(0.82f, 0.06f, 1.40f));
            b.Oct(gold, V(x, 1.095f, 0), V(0.97f, 0.10f, 1.49f));
            b.Oct(steel, V(x, 1.14f, 0), V(1.02f, 0.047f, 1.52f));
            b.Box(steel, V(x, 0.704f, -0.682f), V(0.225f, 0.65f, 0.078f));
            b.Box(blue, V(x, 0.704f, -0.731f), V(0.15f, 0.58f, 0.048f));
            b.Box(cyan, V(x, 0.715f, -0.762f), V(0.068f, 0.45f, 0.022f));
            b.Box(gold, V(x, 0.397f, -0.755f), V(0.24f, 0.05f, 0.075f));
            b.Box(gold, V(x, 1.012f, -0.755f), V(0.24f, 0.05f, 0.075f));
            b.Box(blue, V(x, 0.155f, -0.912f), V(0.52f, 0.135f, 0.045f));
            b.Box(cyan, V(x, 0.163f, -0.944f), V(0.35f, 0.037f, 0.015f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(gold, V(x + side * 0.455f, 0.164f, -0.632f), V(0.105f, 0.17f, 0.10f), -side * 12f);
                b.Oct(gold, V(x + side * 0.315f, 0.189f, -0.867f), V(0.055f, 0.021f, 0.055f), Quaternion.Euler(90, 0, 0));
            }
        }

        private static void BuildBanner(Builder b, float x, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            b.Shield(steel, V(x, 0.99f, -1.125f), V(0.455f, 0.765f, 0.085f));
            b.Shield(gold, V(x, 0.99f, -1.168f), V(0.424f, 0.735f, 0.065f));
            b.Shield(blue, V(x, 0.997f, -1.211f), V(0.343f, 0.631f, 0.035f));
            b.Box(gold, V(x, 1.343f, -1.235f), V(0.47f, 0.080f, 0.079f));
            b.Box(ivory, V(x, 1.035f, -1.239f), V(0.14f, 0.14f, 0.018f), 45f);
            b.Box(blue, V(x, 1.035f, -1.255f), V(0.084f, 0.084f, 0.015f), 45f);
            b.Gem(cyan, V(x, 1.035f, -1.268f), V(0.060f, 0.083f, 0.030f));
            for (int side = -1; side <= 1; side += 2)
                b.Box(ivory, V(x + side * 0.072f, 0.922f, -1.243f), V(0.025f, 0.21f, 0.016f), side * 35f);
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        private static Mesh StoreMesh(string path, Mesh mesh)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                existing = mesh;
            }
            else
            {
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(existing);
            }
            AssetDatabase.SaveAssetIfDirty(existing);
            return existing;
        }

        private sealed class Builder
        {
            private readonly Mesh _box, _octagon, _shield, _gem;
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            private readonly Dictionary<Material, string> _names = new Dictionary<Material, string>();

            public Builder(Mesh box, Mesh octagon, Mesh shield, Mesh gem)
            {
                _box = box;
                _octagon = octagon;
                _shield = shield;
                _gem = gem;
            }

            public void Register(string name, Material material)
            {
                _parts.Add(material, new List<CombineInstance>());
                _names.Add(material, name);
            }

            public void Box(Material m, Vector3 p, Vector3 s, float angle = 0) => Add(_box, m, p, s, Quaternion.Euler(0, 0, angle));
            public void Oct(Material m, Vector3 p, Vector3 s) => Add(_octagon, m, p, s, Quaternion.identity);
            public void Oct(Material m, Vector3 p, Vector3 s, Quaternion q) => Add(_octagon, m, p, s, q);
            public void Shield(Material m, Vector3 p, Vector3 s) => Add(_shield, m, p, s, Quaternion.identity);
            public void Gem(Material m, Vector3 p, Vector3 s) => Add(_gem, m, p, s, Quaternion.identity);

            private void Add(Mesh mesh, Material material, Vector3 position, Vector3 scale, Quaternion rotation)
            {
                _parts[material].Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });
            }

            public void Bake(Transform root, string meshBasePath)
            {
                foreach (var group in _parts)
                {
                    if (group.Value.Count == 0) continue;
                    var combined = new Mesh();
                    combined.name = "Phase32_StandSingleLeg_" + _names[group.Key];
                    combined.CombineMeshes(group.Value.ToArray(), true, true);
                    combined.RecalculateBounds();
                    combined.RecalculateTangents();

                    string meshPath = meshBasePath + _names[group.Key] + ".asset";
                    Mesh stored = StoreMesh(meshPath, combined);

                    Transform child = root.Find(_names[group.Key]);
                    if (child == null)
                    {
                        child = new GameObject(_names[group.Key]).transform;
                        child.SetParent(root, false);
                    }
                    child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    child.localScale = Vector3.one;
                    child.gameObject.layer = root.gameObject.layer;

                    MeshFilter filter = child.GetComponent<MeshFilter>();
                    if (filter == null) filter = child.gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer == null) renderer = child.gameObject.AddComponent<MeshRenderer>();
                    filter.sharedMesh = stored;
                    renderer.sharedMaterial = group.Key;
                    renderer.enabled = true;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
            }
        }
    }
}
