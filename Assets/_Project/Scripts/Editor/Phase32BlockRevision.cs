using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace RoyalSmash.Editor
{
    /// <summary>Revises existing block artwork while preserving the runtime primary renderer and physics.</summary>
    public static class Phase32BlockRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        private const string DETAILS = "Phase32BlockDetails";
        private const string WOOD_TEXTURE = ART + "/Textures/T_CitadelWood_Reference.png";
        private static readonly string[] Variants = { "wood", "stone", "armored" };
        private static readonly Quaternion[] Faces = {
            Quaternion.identity, Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, 180, 0),
            Quaternion.Euler(0, 270, 0), Quaternion.Euler(90, 0, 0), Quaternion.Euler(-90, 0, 0)
        };

        public static RevisionReport LastReport { get; private set; }

        [Serializable]
        public sealed class RevisionReport
        {
            public bool WoodTextureApplied;
            public string WoodTexturePath;
            public BlockReport[] Blocks;
        }

        [Serializable]
        public sealed class BlockReport
        {
            public string Variant, PrefabPath, Guid, PrimaryRendererPath;
            public bool ExistingTransformsPreserved, GameplayComponentsPreserved, PrimaryRendererPreserved;
            public int DisabledLegacyRenderers, EnabledRenderers, Vertices, Triangles, DecorativeColliders;
            public Vector3 VisualBoundsMin, VisualBoundsMax;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/07 - Apply Block Revision")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the blocks.");

            // Fail before modifying assets if any expected source prefab or material is missing.
            foreach (string variant in Variants)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(variant)) == null)
                    throw new InvalidOperationException("Missing existing source block: " + variant);
            Mesh bevel = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            if (bevel == null) throw new InvalidOperationException("The existing beveled cube mesh is missing.");
            Material ivory = RequireMaterial("IvoryStone"), gold = RequireMaterial("AntiqueGold");
            Material steel = RequireMaterial("ObsidianSteel"), blue = RequireMaterial("RoyalSapphire");
            Material cyan = RequireMaterial("Phase32StandCyan");
            Material woodSource = RequireMaterial("WarmWood");
            Texture2D woodTexture = PrepareWoodTexture();
            Material wood = ResolveWood(woodSource, woodTexture);

            var reports = new List<BlockReport>();
            foreach (string variant in Variants)
                reports.Add(Revise(variant, bevel, wood, ivory, gold, steel, blue, cyan));
            AssetDatabase.SaveAssets();
            LastReport = new RevisionReport {
                WoodTextureApplied = true, WoodTexturePath = WOOD_TEXTURE, Blocks = reports.ToArray()
            };
            Debug.Log("[Phase32BlockRevision] " + JsonUtility.ToJson(LastReport));
        }

        private static BlockReport Revise(string variant, Mesh bevel, Material wood, Material ivory,
            Material gold, Material steel, Material blue, Material cyan)
        {
            string path = PrefabPath(variant);
            string guid = AssetDatabase.AssetPathToGUID(path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var snapshot = new PreservationSnapshot(root);
                MeshRenderer primary = root.GetComponentInChildren<MeshRenderer>();
                string primaryName = variant == "wood" ? "M_WarmWood" : "M_IvoryStone";
                if (primary == null || primary.name != primaryName || !primary.enabled)
                    throw new InvalidOperationException("Unexpected primary renderer in " + path + ". Review before applying.");
                if (primary.GetComponent<MeshFilter>() == null)
                    throw new InvalidOperationException("The primary renderer must retain its existing MeshFilter.");
                BoxCollider collider = root.GetComponent<BoxCollider>();
                if (collider == null || collider.size != Vector3.one || collider.center != Vector3.zero)
                    throw new InvalidOperationException("Expected the existing unit BoxCollider on " + variant);
                Material body = variant == "wood" ? wood : ivory;
                using (var builder = new Builder(variant, bevel, body, gold, steel, blue, cyan))
                {
                    if (variant == "wood") BuildWood(builder, wood, steel, gold);
                    else BuildStone(builder, ivory, gold, steel, blue, cyan, variant == "armored");
                    Transform details = OwnRoot(root.transform, DETAILS);
                    int disabled = DisableLegacy(root, primary, details);
                    builder.Bake(root.transform, primary, details, body);
                    snapshot.Verify();
                    if (root.GetComponentInChildren<Renderer>() != primary || !primary.enabled ||
                        !primary.sharedMaterial.HasProperty("_BaseColor"))
                        throw new InvalidOperationException("PhysicsObject primary renderer contract changed.");
                    BlockReport report = Measure(root, details, primary, variant, path, guid, disabled);
                    if (report.EnabledRenderers > 6 || report.Triangles > 5000 || report.DecorativeColliders != 0)
                        throw new InvalidOperationException("Block art exceeds the renderer, triangle or collider budget: " + JsonUtility.ToJson(report));
                    // Detail remains within the unit collider, avoiding new visible intersections in tight stacks.
                    if (report.VisualBoundsMin.x < -.501f || report.VisualBoundsMin.y < -.501f || report.VisualBoundsMin.z < -.501f ||
                        report.VisualBoundsMax.x > .501f || report.VisualBoundsMax.y > .501f || report.VisualBoundsMax.z > .501f)
                        throw new InvalidOperationException("Block art exceeds its unit collider envelope: " + variant);
                    if (PrefabUtility.SaveAsPrefabAsset(root, path) == null)
                        throw new InvalidOperationException("Could not save " + path);
                    if (AssetDatabase.AssetPathToGUID(path) != guid)
                        throw new InvalidOperationException("The source prefab GUID changed.");
                    return report;
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BuildWood(Builder b, Material wood, Material steel, Material gold)
        {
            b.Box(wood, Vector3.zero, Vector3.one * .88f);
            foreach (Quaternion face in Faces)
            {
                // Individual recessed boards leave real seams. UVs follow each board's long axis.
                for (int plank = 0; plank < 4; plank++)
                    b.FaceBox(wood, face, V((plank - 1.5f) * .209f, 0, -.449f), V(.202f, .85f, .060f));
                foreach (int sign in new[] { -1, 1 })
                {
                    b.FaceBox(wood, face, V(0, sign * .405f, -.468f), V(.092f, .90f, .054f), 90);
                    b.FaceBox(wood, face, V(sign * .405f, 0, -.468f), V(.092f, .72f, .054f));
                }
                // Inset dark rebates and raised, beveled crossed struts catch separate highlights.
                foreach (int sign in new[] { -1, 1 })
                {
                    b.FaceBox(steel, face, V(0, 0, -.476f), V(.126f, 1.045f, .020f), sign * 45);
                    b.FaceBox(wood, face, V(0, 0, -.483f), V(.098f, 1.025f, .027f), sign * 45);
                }
                FaceBolts(b, face, gold, .410f, .040f);
            }
            CornerCaps(b, steel, .174f);
        }

        private static void BuildStone(Builder b, Material ivory, Material gold, Material steel,
            Material blue, Material cyan, bool armored)
        {
            b.Box(ivory, Vector3.zero, Vector3.one * .884f);
            foreach (Quaternion face in Faces)
            {
                // Three separately beveled slabs reproduce the horizontal masonry joints in the reference.
                for (int row = -1; row <= 1; row++)
                    b.FaceBox(ivory, face, V(0, row * .280f, -.451f), V(.855f, .270f, .068f));
                foreach (int sign in new[] { -1, 1 })
                {
                    b.FaceStrip(gold, face, V(0, sign * .140f, -.462f), V(.846f, .014f, .014f));
                    b.FaceBox(gold, face, V(0, sign * .442f, -.466f), V(.918f, .035f, .046f));
                    b.FaceBox(armored ? steel : gold, face, V(sign * .442f, 0, -.466f), V(.035f, .866f, .046f));
                    // Offset short seams stop at the horizontal joints and do not draw through the jewel.
                    b.FaceStrip(gold, face, V(sign * .19f, sign * .285f, -.488f), V(.010f, .240f, .006f));
                }
                b.FaceBox(steel, face, V(0, 0, -.478f), V(.269f, .269f, .018f), 45);
                // Both full backing plates must stay behind the jewel rim at Z=-.494.
                b.FaceBox(gold, face, V(0, 0, -.483f), V(.240f, .240f, .016f), 45);
                b.FaceBox(blue, face, V(0, 0, -.487f), V(.183f, .183f, .009f), 45);
                b.Gem(cyan, face, V(0, 0, -.494f), V(.190f, .251f, .010f));
                FaceBolts(b, face, armored ? gold : steel, armored ? .393f : .427f, armored ? .041f : .025f);
                if (armored)
                {
                    foreach (int sign in new[] { -1, 1 })
                    {
                        b.FaceBox(gold, face, V(sign * .405f, 0, -.485f), V(.055f, .596f, .020f));
                        b.FaceStrip(blue, face, V(0, sign * .413f, -.487f), V(.573f, .024f, .018f));
                    }
                }
            }
            CornerCaps(b, armored ? steel : gold, armored ? .218f : .137f);
        }

        private static void CornerCaps(Builder b, Material material, float size)
        {
            float center = .498f - size * .5f;
            foreach (int x in new[] { -1, 1 })
                foreach (int y in new[] { -1, 1 })
                    foreach (int z in new[] { -1, 1 })
                        b.Box(material, V(x * center, y * center, z * center), Vector3.one * size);
        }

        private static void FaceBolts(Builder b, Quaternion face, Material material, float offset, float size)
        {
            foreach (int x in new[] { -1, 1 })
                foreach (int y in new[] { -1, 1 })
                    b.Bolt(material, face, V(x * offset, y * offset, -.494f), V(size, size, .012f));
        }

        private static int DisableLegacy(GameObject root, MeshRenderer primary, Transform details)
        {
            int count = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == primary || renderer.transform.IsChildOf(details)) continue;
                string name = renderer.name;
                if (name == "M_AntiqueGold" || name == "M_ObsidianSteel" || name == "M_RoyalSapphire")
                { renderer.enabled = false; count++; }
            }
            return count;
        }

        private static BlockReport Measure(GameObject root, Transform details, MeshRenderer primary,
            string variant, string path, string guid, int disabled)
        {
            var report = new BlockReport {
                Variant = variant, PrefabPath = path, Guid = guid, PrimaryRendererPath = primary.name,
                ExistingTransformsPreserved = true, GameplayComponentsPreserved = true, PrimaryRendererPreserved = true,
                DisabledLegacyRenderers = disabled, DecorativeColliders = details.GetComponentsInChildren<Collider>(true).Length
            };
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeSelf) continue;
                report.EnabledRenderers++;
                Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                report.Vertices += mesh.vertexCount;
                report.Triangles += (int)mesh.GetIndexCount(0) / 3;
                Matrix4x4 matrix = root.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                foreach (Vector3 point in mesh.vertices) bounds.Encapsulate(matrix.MultiplyPoint3x4(point));
            }
            report.VisualBoundsMin = bounds.min; report.VisualBoundsMax = bounds.max;
            return report;
        }

        private static Texture2D PrepareWoodTexture()
        {
            TextureImporter importer = AssetImporter.GetAtPath(WOOD_TEXTURE) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("The reference wood grain texture must exist before applying 32.7.");
            importer.textureType = TextureImporterType.Default; importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None; importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat; importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4; importer.maxTextureSize = 1024; importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.ToNearest;
            foreach (string platform in new[] { "Android", "iPhone" })
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
                settings.name = platform; settings.overridden = true; settings.maxTextureSize = 1024;
                settings.format = TextureImporterFormat.ASTC_6x6; settings.compressionQuality = 70;
                importer.SetPlatformTextureSettings(settings);
            }
            TextureImporterPlatformSettings desktop = importer.GetPlatformTextureSettings("Standalone");
            desktop.name = "Standalone"; desktop.overridden = true; desktop.maxTextureSize = 1024;
            desktop.format = TextureImporterFormat.BC7; importer.SetPlatformTextureSettings(desktop);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(WOOD_TEXTURE);
        }

        private static Material ResolveWood(Material source, Texture2D texture)
        {
            string path = ART + "/Materials/M_Phase32BlockWood.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, path); }
            material.name = "M_Phase32BlockWood"; material.shader = source.shader;
            material.SetTexture("_BaseMap", texture); material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_BaseMap", Vector2.one); material.SetTextureOffset("_BaseMap", Vector2.zero);
            material.SetColor("_BaseColor", Color.white); material.SetColor("_Color", Color.white);
            material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", .34f);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static Material RequireMaterial(string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_" + name + ".mat");
            if (material == null) throw new InvalidOperationException("Missing existing material: " + name);
            return material;
        }

        private static string PrefabPath(string variant) => ART + "/Prefabs/PF_" + variant + ".prefab";
        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        private static Transform OwnRoot(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null) { child = new GameObject(name).transform; child.SetParent(parent, false); }
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity); child.localScale = Vector3.one;
            child.gameObject.layer = parent.gameObject.layer;
            return child;
        }

        private sealed class PreservationSnapshot
        {
            private readonly Dictionary<Transform, PoseState> _poses = new Dictionary<Transform, PoseState>();
            private readonly Dictionary<Component, string> _components = new Dictionary<Component, string>();
            public PreservationSnapshot(GameObject root)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    _poses.Add(transform, new PoseState(transform));
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    if (component != null && !(component is Transform) && !(component is Renderer) && !(component is MeshFilter))
                        _components.Add(component, EditorJsonUtility.ToJson(component));
            }
            public void Verify()
            {
                foreach (var entry in _poses)
                    if (entry.Key == null || !entry.Value.Matches(entry.Key))
                        throw new InvalidOperationException("A pre-existing block transform changed.");
                foreach (var entry in _components)
                    if (entry.Key == null || EditorJsonUtility.ToJson(entry.Key) != entry.Value)
                        throw new InvalidOperationException("A block gameplay component or collider changed.");
            }
            private sealed class PoseState
            {
                private readonly Vector3 _position, _scale;
                private readonly Quaternion _rotation;
                private readonly Transform _parent;
                private readonly string _name;
                private readonly int _layer, _sibling;
                private readonly bool _active;
                public PoseState(Transform t)
                {
                    _position = t.localPosition; _scale = t.localScale; _rotation = t.localRotation;
                    _parent = t.parent; _name = t.name; _layer = t.gameObject.layer;
                    _sibling = t.GetSiblingIndex(); _active = t.gameObject.activeSelf;
                }
                public bool Matches(Transform t) => t.localPosition == _position && t.localScale == _scale &&
                    t.localRotation == _rotation && t.parent == _parent && t.name == _name &&
                    t.gameObject.layer == _layer && t.GetSiblingIndex() == _sibling && t.gameObject.activeSelf == _active;
            }
        }

        private sealed class Builder : IDisposable
        {
            private readonly string _variant;
            private readonly Mesh _bevel, _gem, _bolt, _strip;
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            private readonly Dictionary<Material, string> _names = new Dictionary<Material, string>();
            public Builder(string variant, Mesh bevel, Material body, Material gold, Material steel, Material blue, Material cyan)
            {
                _variant = variant; _bevel = bevel; _gem = CreateGem(); _bolt = CreateBolt(); _strip = CreateStrip();
                Register(body, "Body"); Register(gold, "Gold"); Register(steel, "Steel");
                Register(blue, "Sapphire"); Register(cyan, "Gem");
            }
            private void Register(Material material, string name)
            { _parts.Add(material, new List<CombineInstance>()); _names.Add(material, name); }
            public void Box(Material m, Vector3 p, Vector3 s) => Add(_bevel, m, p, Quaternion.identity, s);
            public void FaceBox(Material m, Quaternion face, Vector3 p, Vector3 s, float angle = 0)
                => Add(_bevel, m, face * p, face * Quaternion.Euler(0, 0, angle), s);
            public void FaceStrip(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_strip, m, face * p, face, s);
            public void Gem(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_gem, m, face * p, face, s);
            public void Bolt(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_bolt, m, face * p, face, s);
            private void Add(Mesh mesh, Material m, Vector3 p, Quaternion q, Vector3 s)
                => _parts[m].Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(p, q, s) });

            public void Bake(Transform root, MeshRenderer primary, Transform details, Material body)
            {
                foreach (var group in _parts)
                {
                    if (group.Value.Count == 0) continue;
                    bool isBody = group.Key == body;
                    Transform target = isBody ? primary.transform : OwnRoot(details, _names[group.Key]);
                    var parts = group.Value.ToArray();
                    Matrix4x4 fromRoot = target.worldToLocalMatrix * root.localToWorldMatrix;
                    for (int i = 0; i < parts.Length; i++) parts[i].transform = fromRoot * parts[i].transform;
                    string meshName = "Phase32_Block_" + _variant + "_" + _names[group.Key];
                    string path = ART + "/Meshes/" + meshName + ".asset";
                    Mesh stored = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    bool isNew = stored == null;
                    if (isNew) stored = new Mesh { name = meshName, indexFormat = IndexFormat.UInt16 };
                    // Rebuild through Mesh APIs: CopySerialized can leave an existing renderer's
                    // graphics buffers stale even when the saved vertices already contain the revision.
                    else stored.Clear();
                    stored.CombineMeshes(parts, true, true); stored.RecalculateBounds(); stored.RecalculateTangents();
                    stored.UploadMeshData(false);
                    if (isNew) AssetDatabase.CreateAsset(stored, path);
                    else EditorUtility.SetDirty(stored);
                    AssetDatabase.SaveAssetIfDirty(stored);
                    MeshFilter filter = target.GetComponent<MeshFilter>();
                    if (filter == null) filter = target.gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = target.GetComponent<MeshRenderer>();
                    if (renderer == null) renderer = target.gameObject.AddComponent<MeshRenderer>();
                    filter.sharedMesh = stored; renderer.sharedMaterial = group.Key; renderer.enabled = true;
                    renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
                }
            }
            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(_gem); UnityEngine.Object.DestroyImmediate(_bolt);
                UnityEngine.Object.DestroyImmediate(_strip);
            }

            private static Mesh CreateStrip()
            {
                var data = new MeshData();
                foreach (Quaternion face in Faces)
                    data.Face(face * Vector3.back, face * V(-.5f, -.5f, -.5f), face * V(.5f, -.5f, -.5f),
                        face * V(.5f, .5f, -.5f), face * V(-.5f, .5f, -.5f));
                return data.Build();
            }

            private static Mesh CreateGem()
            {
                var data = new MeshData();
                Vector3[] rim = { V(0, .5f, 0), V(.5f, 0, 0), V(0, -.5f, 0), V(-.5f, 0, 0) };
                for (int i = 0; i < 4; i++)
                {
                    data.Face(Vector3.back, V(0, 0, -.5f), rim[i], rim[(i + 1) % 4]);
                    data.Face(Vector3.forward, V(0, 0, .2f), rim[i], rim[(i + 1) % 4]);
                }
                return data.Build();
            }
            private static Mesh CreateBolt()
            {
                var data = new MeshData();
                const int sides = 6;
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2 / sides, next = (i + 1) * Mathf.PI * 2 / sides;
                    Vector3 p = V(Mathf.Cos(a) * .5f, Mathf.Sin(a) * .5f, 0);
                    Vector3 q = V(Mathf.Cos(next) * .5f, Mathf.Sin(next) * .5f, 0);
                    data.Face((p + q).normalized, p + Vector3.back * .5f, q + Vector3.back * .5f,
                        q + Vector3.forward * .5f, p + Vector3.forward * .5f);
                    data.Face(Vector3.back, Vector3.back * .5f, p + Vector3.back * .5f, q + Vector3.back * .5f);
                    data.Face(Vector3.forward, Vector3.forward * .5f, p + Vector3.forward * .5f, q + Vector3.forward * .5f);
                }
                return data.Build();
            }
        }

        private sealed class MeshData
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<int> _triangles = new List<int>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            public void Face(Vector3 outward, params Vector3[] points)
            {
                if (Vector3.Dot(Vector3.Cross(points[1] - points[0], points[2] - points[0]), outward) < 0) Array.Reverse(points);
                int start = _vertices.Count;
                foreach (Vector3 point in points) { _vertices.Add(point); _uv.Add(new Vector2(point.x + .5f, point.y + .5f)); }
                for (int i = 1; i < points.Length - 1; i++) { _triangles.Add(start); _triangles.Add(start + i); _triangles.Add(start + i + 1); }
            }
            public Mesh Build()
            {
                Mesh mesh = new Mesh(); mesh.SetVertices(_vertices); mesh.SetTriangles(_triangles, 0); mesh.SetUVs(0, _uv);
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents(); return mesh;
            }
        }
    }
}
