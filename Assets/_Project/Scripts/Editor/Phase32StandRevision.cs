using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace RoyalSmash.Editor
{
    /// <summary>Adds baked decorative armor to the existing stand without changing its physics.</summary>
    public static class Phase32StandRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        private const string PREFAB = ART + "/Prefabs/PF_CitadelStand.prefab";
        private const string DETAILS = "Phase32StandDetails";
        private const string MESHES = ART + "/Meshes/Phase32_Stand_";

        [MenuItem("Royal Smash/Sky Citadel/Phase 32 Stand Revision")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the stand.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB) == null)
                throw new InvalidOperationException("The existing Citadel stand prefab is missing.");

            Material ivory = RequireMaterial("IvoryStone");
            Material gold = RequireMaterial("AntiqueGold");
            Material steel = RequireMaterial("ObsidianSteel");
            Material blue = RequireMaterial("RoyalSapphire");
            Mesh bevel = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            if (bevel == null) throw new InvalidOperationException("The existing Citadel beveled cube mesh is missing.");
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) throw new InvalidOperationException("URP Lit shader is missing.");
            Mesh octagon = ResolveMesh("OctagonalPrism", BuildOctagon);
            Mesh shield = ResolveMesh("Shield", BuildShield);
            Mesh gem = ResolveMesh("Gem", BuildGem);
            Material cyan = ResolveCyan(lit);

            var b = new Builder(bevel, octagon, shield, gem);
            b.Register("Ivory", ivory); b.Register("Gold", gold); b.Register("Steel", steel);
            b.Register("Sapphire", blue); b.Register("Energy", cyan);
            BuildBeam(b, ivory, gold, steel, blue, cyan);
            for (int side = -1; side <= 1; side += 2)
            {
                BuildFoot(b, side * 1.425f, ivory, gold, steel, blue, cyan);
                BuildBanner(b, side * 2.605f, ivory, gold, steel, blue, cyan);
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PREFAB);
            try
            {
                Transform details = contents.transform.Find(DETAILS);
                if (details == null)
                {
                    details = new GameObject(DETAILS).transform;
                    details.SetParent(contents.transform, false);
                }
                details.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                details.localScale = Vector3.one;
                details.gameObject.layer = contents.layer;
                // Keep legacy objects, colliders, scripts and the entire presentation transform.
                foreach (Renderer renderer in contents.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.transform.IsChildOf(details)) continue;
                    string name = renderer.gameObject.name;
                    if (name == "SapphireFrieze" || name == "LowerGoldEdge" ||
                        name == "FriezeDiamond" || name == "GoldCornice") renderer.enabled = false;
                }
                b.Bake(details);
                if (PrefabUtility.SaveAsPrefabAsset(contents, PREFAB) == null)
                    throw new InvalidOperationException("Could not save the revised stand prefab.");
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            Debug.Log("[Phase32StandRevision] Stand decorative meshes updated in place; physics, root, presentation and scene overrides preserved.");
        }

        private static void BuildBeam(Builder b, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            // Dimensions are in prefab-root coordinates: gameplay stand Z scale is 0.5.
            // All upper trim stays below the unchanged collider top at Y = 1.475.
            b.Box(ivory, V(0, 1.255f, 0), V(5.66f, .38f, 1.89f));
            b.Box(steel, V(0, 1.078f, 0), V(5.73f, .17f, 1.96f));
            b.Box(gold, V(0, 1.436f, 0), V(5.74f, .065f, 1.97f));
            b.Box(gold, V(0, 1.022f, -.99f), V(5.65f, .054f, .11f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(steel, V(side * 1.49f, 1.235f, -.974f), V(2.13f, .18f, .058f));
                b.Box(blue, V(side * 1.47f, 1.225f, -1.017f), V(1.99f, .105f, .046f));
                b.Box(cyan, V(side * 1.43f, 1.226f, -1.045f), V(1.64f, .027f, .016f));
                b.Box(gold, V(side * 1.65f, 1.137f, -1.047f), V(1.73f, .042f, .071f));
                b.Box(gold, V(side * 2.46f, 1.267f, -1.049f), V(.31f, .071f, .095f), side * 39f);
                b.Box(gold, V(side * .62f, 1.139f, -1.045f), V(.30f, .055f, .071f), side * -35f);
                b.Box(gold, V(side * 2.60f, 1.392f, 0), V(.52f, .13f, 2.02f));
                b.Box(steel, V(side * 2.76f, 1.24f, -1.045f), V(.13f, .20f, .095f));
                b.Oct(cyan, V(side * 2.76f, 1.26f, -1.108f), V(.054f, .016f, .054f), Quaternion.Euler(90, 0, 0));
                for (int bolt = 0; bolt < 3; bolt++)
                    b.Oct(gold, V(side * (.88f + bolt * .55f), 1.32f, -1.052f), V(.036f, .017f, .036f), Quaternion.Euler(90, 0, 0));
            }
            b.Box(steel, V(0, 1.12f, -1.028f), V(.565f, .45f, .095f));
            b.Box(gold, V(0, 1.115f, -1.115f), V(.49f, .49f, .15f), 45f);
            b.Box(blue, V(0, 1.115f, -1.207f), V(.378f, .378f, .062f), 45f);
            b.Gem(cyan, V(0, 1.115f, -1.247f), V(.415f, .49f, .26f));
        }

        private static void BuildFoot(Builder b, float x, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            b.Oct(steel, V(x, .15f, 0), V(1.27f, .30f, 1.77f));
            b.Oct(gold, V(x, .048f, 0), V(1.29f, .055f, 1.80f));
            b.Oct(gold, V(x, .286f, 0), V(1.17f, .045f, 1.62f));
            b.Oct(steel, V(x, .327f, 0), V(.99f, .070f, 1.42f));
            b.Oct(ivory, V(x, .705f, 0), V(.755f, .77f, 1.34f));
            b.Oct(gold, V(x, .371f, 0), V(.82f, .06f, 1.40f));
            b.Oct(gold, V(x, 1.095f, 0), V(.97f, .10f, 1.49f));
            b.Oct(steel, V(x, 1.14f, 0), V(1.02f, .047f, 1.52f));
            b.Box(steel, V(x, .704f, -.682f), V(.225f, .65f, .078f));
            b.Box(blue, V(x, .704f, -.731f), V(.15f, .58f, .048f));
            b.Box(cyan, V(x, .715f, -.762f), V(.068f, .45f, .022f));
            b.Box(gold, V(x, .397f, -.755f), V(.24f, .05f, .075f));
            b.Box(gold, V(x, 1.012f, -.755f), V(.24f, .05f, .075f));
            b.Box(blue, V(x, .155f, -.912f), V(.52f, .135f, .045f));
            b.Box(cyan, V(x, .163f, -.944f), V(.35f, .037f, .015f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(gold, V(x + side * .455f, .164f, -.632f), V(.105f, .17f, .10f), -side * 12f);
                b.Oct(gold, V(x + side * .315f, .189f, -.867f), V(.055f, .021f, .055f), Quaternion.Euler(90, 0, 0));
            }
        }

        private static void BuildBanner(Builder b, float x, Material ivory, Material gold, Material steel, Material blue, Material cyan)
        {
            b.Shield(steel, V(x, .99f, -1.125f), V(.455f, .765f, .085f));
            b.Shield(gold, V(x, .99f, -1.168f), V(.424f, .735f, .065f));
            b.Shield(blue, V(x, .997f, -1.211f), V(.343f, .631f, .035f));
            b.Box(gold, V(x, 1.343f, -1.235f), V(.47f, .080f, .079f));
            b.Box(ivory, V(x, 1.035f, -1.239f), V(.14f, .14f, .018f), 45f);
            b.Box(blue, V(x, 1.035f, -1.255f), V(.084f, .084f, .015f), 45f);
            b.Gem(cyan, V(x, 1.035f, -1.268f), V(.060f, .083f, .030f));
            for (int side = -1; side <= 1; side += 2)
                b.Box(ivory, V(x + side * .072f, .922f, -1.243f), V(.025f, .21f, .016f), side * 35f);
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        private static Material RequireMaterial(string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_" + name + ".mat");
            if (material == null) throw new InvalidOperationException("Missing existing material: " + name);
            return material;
        }

        private static Material ResolveCyan(Shader shader)
        {
            string path = ART + "/Materials/M_Phase32StandCyan.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = "M_Phase32StandCyan" }; AssetDatabase.CreateAsset(material, path); }
            material.SetColor("_BaseColor", new Color(.015f, .28f, .55f));
            material.SetColor("_EmissionColor", new Color(.015f, 1.55f, 2.5f));
            material.SetFloat("_Metallic", .18f); material.SetFloat("_Smoothness", .7f);
            // URP enables emission only for AnyEmissive flags; None is reset by validation.
            // Baked emission avoids a realtime-GI requirement while keeping the visible glow.
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static Mesh ResolveMesh(string name, Func<Mesh> create)
        {
            return StoreMesh(name, create());
        }

        private static Mesh StoreMesh(string name, Mesh mesh)
        {
            string path = MESHES + name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.name = "Phase32_Stand_" + name;
            if (existing == null) { AssetDatabase.CreateAsset(mesh, path); existing = mesh; }
            else { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); EditorUtility.SetDirty(existing); }
            AssetDatabase.SaveAssetIfDirty(existing);
            return existing;
        }

        private static Mesh BuildOctagon()
        {
            var data = new MeshData();
            float[] heights = { -.5f, -.36f, .36f, .5f };
            float[] radii = { .43f, .5f, .5f, .43f };
            for (int i = 0; i < 8; i++)
            {
                float a = (i + .5f) * Mathf.PI / 4f, next = (i + 1.5f) * Mathf.PI / 4f;
                for (int ring = 0; ring < 3; ring++)
                    data.Face(V(Mathf.Cos(a) * radii[ring], heights[ring], Mathf.Sin(a) * radii[ring]),
                        V(Mathf.Cos(next) * radii[ring], heights[ring], Mathf.Sin(next) * radii[ring]),
                        V(Mathf.Cos(next) * radii[ring + 1], heights[ring + 1], Mathf.Sin(next) * radii[ring + 1]),
                        V(Mathf.Cos(a) * radii[ring + 1], heights[ring + 1], Mathf.Sin(a) * radii[ring + 1]));
                foreach (int sign in new[] { -1, 1 })
                    data.Face(V(0, sign * .5f, 0), V(Mathf.Cos(a) * .43f, sign * .5f, Mathf.Sin(a) * .43f),
                        V(Mathf.Cos(next) * .43f, sign * .5f, Mathf.Sin(next) * .43f));
            }
            return data.Mesh();
        }

        private static Mesh BuildShield()
        {
            var outline = new[] { V(-.5f,.5f,0), V(.5f,.5f,0), V(.46f,-.27f,0), V(0,-.5f,0), V(-.46f,-.27f,0) };
            var data = new MeshData();
            for (int i = 0; i < outline.Length; i++)
            {
                Vector3 a = outline[i], next = outline[(i + 1) % outline.Length];
                data.Face(a + Vector3.back * .5f, next + Vector3.back * .5f, next + Vector3.forward * .5f, a + Vector3.forward * .5f);
                data.Face(Vector3.back * .5f, a + Vector3.back * .5f, next + Vector3.back * .5f);
                data.Face(Vector3.forward * .5f, a + Vector3.forward * .5f, next + Vector3.forward * .5f);
            }
            return data.Mesh();
        }

        private static Mesh BuildGem()
        {
            var data = new MeshData();
            var points = new[] { V(0,.5f,0), V(.5f,0,0), V(0,-.5f,0), V(-.5f,0,0) };
            for (int i = 0; i < 4; i++)
            {
                data.Face(V(0,0,-.5f), points[i], points[(i + 1) % 4]);
                data.Face(V(0,0,.12f), points[i], points[(i + 1) % 4]);
            }
            return data.Mesh();
        }

        private sealed class MeshData
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<int> _triangles = new List<int>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            public void Face(params Vector3[] points)
            {
                Vector3 center = Vector3.zero;
                foreach (Vector3 point in points) center += point;
                if (Vector3.Dot(Vector3.Cross(points[1] - points[0], points[2] - points[0]), center) < 0) Array.Reverse(points);
                int first = _vertices.Count;
                foreach (Vector3 point in points) { _vertices.Add(point); _uv.Add(new Vector2(point.x + .5f, point.y + .5f)); }
                for (int i = 1; i < points.Length - 1; i++) _triangles.AddRange(new[] { first, first + i, first + i + 1 });
            }
            public Mesh Mesh()
            {
                var mesh = new Mesh(); mesh.SetVertices(_vertices); mesh.SetTriangles(_triangles, 0); mesh.SetUVs(0, _uv);
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents(); return mesh;
            }
        }

        private sealed class Builder
        {
            private readonly Mesh _box, _octagon, _shield, _gem;
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            private readonly Dictionary<Material, string> _names = new Dictionary<Material, string>();
            public Builder(Mesh box, Mesh octagon, Mesh shield, Mesh gem) { _box = box; _octagon = octagon; _shield = shield; _gem = gem; }
            public void Register(string name, Material material) { _parts.Add(material, new List<CombineInstance>()); _names.Add(material, name); }
            public void Box(Material m, Vector3 p, Vector3 s, float angle = 0) => Add(_box, m, p, s, Quaternion.Euler(0, 0, angle));
            public void Oct(Material m, Vector3 p, Vector3 s) => Add(_octagon, m, p, s, Quaternion.identity);
            public void Oct(Material m, Vector3 p, Vector3 s, Quaternion q) => Add(_octagon, m, p, s, q);
            public void Shield(Material m, Vector3 p, Vector3 s) => Add(_shield, m, p, s, Quaternion.identity);
            public void Gem(Material m, Vector3 p, Vector3 s) => Add(_gem, m, p, s, Quaternion.identity);
            private void Add(Mesh mesh, Material material, Vector3 position, Vector3 scale, Quaternion rotation)
                => _parts[material].Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });

            public void Bake(Transform root)
            {
                foreach (var group in _parts)
                {
                    var combined = new Mesh(); combined.CombineMeshes(group.Value.ToArray(), true, true);
                    combined.RecalculateBounds(); combined.RecalculateTangents();
                    Mesh stored = StoreMesh(_names[group.Key], combined);
                    Transform child = root.Find(_names[group.Key]);
                    if (child == null) { child = new GameObject(_names[group.Key]).transform; child.SetParent(root, false); }
                    child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity); child.localScale = Vector3.one;
                    child.gameObject.layer = root.gameObject.layer;
                    MeshFilter filter = child.GetComponent<MeshFilter>();
                    if (filter == null) filter = child.gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer == null) renderer = child.gameObject.AddComponent<MeshRenderer>();
                    filter.sharedMesh = stored; renderer.sharedMaterial = group.Key; renderer.enabled = true;
                    renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
                }
            }
        }
    }
}
