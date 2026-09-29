using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace RoyalSmash.Editor
{
    /// <summary>Bakes fixed pedestal and rotating armor around the existing mortar platform.</summary>
    public static class Phase32MortarPlatformRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        private const string PREFAB = "Assets/_Project/Prefabs/Gameplay/Mortar.prefab";
        private const string FIXED_DETAILS = "Phase32PlatformDetails";
        private const string SWIVEL_DETAILS = "Phase32SwivelDetails";
        private const string MESH_PREFIX = ART + "/Meshes/Phase32_MortarPlatform_";
        private static readonly string[] REPLACED_PATHS =
        {
            "CitadelMount/BaseGold", "CitadelMount/Pedestal", "CitadelMount/SwivelGold",
            "Visuals/SwivelPlatter/PlatterDisc_Purple", "Visuals/SwivelPlatter/PlatterRing_Gold"
        };

        public static RevisionReport LastReport { get; private set; }

        [Serializable]
        public sealed class RevisionReport
        {
            public string PrefabPath;
            public bool ExistingTransformsPreserved;
            public bool GameplayComponentsPreserved;
            public int OriginalTransforms;
            public int DisabledLegacyRenderers;
            public int ArtRenderers;
            public int ArtVertices;
            public int ArtTriangles;
            public int FixedTriangles;
            public int SwivelTriangles;
            public int ArtColliders;
            public MeshRecord[] Meshes;
        }

        [Serializable]
        public sealed class MeshRecord
        {
            public string Name;
            public int Vertices;
            public int Triangles;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/05 - Apply Mortar Platform Revision")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the mortar platform.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB) == null)
                throw new InvalidOperationException("The existing Mortar prefab is missing.");

            Material gold = RequireMaterial("AntiqueGold");
            Material steel = RequireMaterial("ObsidianSteel");
            Material ivory = RequireMaterial("Phase32MortarIvory");
            Material blue = RequireMaterial("RoyalSapphire");
            Material cyan = RequireMaterial("Phase32StandCyan");
            Mesh cube = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            if (cube == null) throw new InvalidOperationException("The existing beveled cube mesh is missing.");

            GameObject contents = PrefabUtility.LoadPrefabContents(PREFAB);
            try
            {
                Transform visuals = RequireChild(contents.transform, "Visuals");
                RequireChild(visuals, "BarrelPivot/FirePoint");
                RequireChild(visuals, "BarrelPivot/Phase32BarrelDetails");
                foreach (string path in REPLACED_PATHS)
                    if (RequireChild(contents.transform, path).GetComponent<Renderer>() == null)
                        throw new InvalidOperationException("Expected a legacy platform renderer: " + path);
                var snapshot = new PreservationSnapshot(contents);

                using (var fixedBase = new Builder("Fixed", cube))
                using (var swivel = new Builder("Swivel", cube))
                {
                    fixedBase.Register(steel, "Steel"); fixedBase.Register(gold, "Gold");
                    swivel.Register(steel, "Steel"); swivel.Register(gold, "Gold");
                    swivel.Register(ivory, "Ivory"); swivel.Register(blue, "Sapphire");
                    swivel.Register(cyan, "Energy");
                    BuildFixedBase(fixedBase, steel, gold);
                    BuildSwivel(swivel, steel, gold, ivory, blue, cyan);
                    Transform fixedRoot = OwnRoot(contents.transform, FIXED_DETAILS);
                    Transform swivelRoot = OwnRoot(visuals, SWIVEL_DETAILS);
                    fixedBase.Bake(fixedRoot);
                    swivel.Bake(swivelRoot);
                    foreach (string path in REPLACED_PATHS)
                        contents.transform.Find(path).GetComponent<Renderer>().enabled = false;

                    snapshot.Verify();
                    var records = new List<MeshRecord>();
                    var report = new RevisionReport
                    {
                        PrefabPath = PREFAB,
                        ExistingTransformsPreserved = true,
                        GameplayComponentsPreserved = true,
                        OriginalTransforms = snapshot.TransformCount,
                        DisabledLegacyRenderers = REPLACED_PATHS.Length
                    };
                    report.FixedTriangles = CountArt(fixedRoot, report, records);
                    report.SwivelTriangles = CountArt(swivelRoot, report, records);
                    report.Meshes = records.ToArray();
                    if (report.ArtColliders != 0 || report.ArtRenderers >= 8 || report.ArtTriangles >= 10000)
                        throw new InvalidOperationException("Mortar platform exceeded its decorative geometry budget.");
                    if (PrefabUtility.SaveAsPrefabAsset(contents, PREFAB) == null)
                        throw new InvalidOperationException("Could not save the revised mortar platform.");
                    LastReport = report;
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            Debug.Log("[Phase32MortarPlatformRevision] " + JsonUtility.ToJson(LastReport));
        }

        private static void BuildFixedBase(Builder b, Material steel, Material gold)
        {
            // Mortar-root space. The original 1.45 m footing and .9 m pedestal footprint
            // are retained; an octagonal silhouette replaces the unbroken gold cylinder.
            b.Prism(steel, Octagon(.725f, -.10f), -.996f, -.813f, .024f);
            b.Prism(gold, Octagon(.731f, -.10f), -.969f, -.937f, .009f);
            b.Prism(gold, Octagon(.657f, -.10f), -.844f, -.816f, .008f);
            b.Prism(steel, Octagon(.421f, -.10f), -.832f, -.157f, .021f);
            b.Prism(gold, Octagon(.481f, -.10f), -.796f, -.751f, .010f);
            b.Prism(steel, Octagon(.577f, -.10f), -.178f, -.062f, .014f);
            b.Prism(gold, Octagon(.586f, -.10f), -.130f, -.097f, .007f);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(gold, V(side * .585f, -.889f, -.415f), V(.060f, .116f, .083f), Quaternion.Euler(0, side * -24f, 0));
                b.Box(steel, V(side * .384f, -.883f, -.680f), V(.166f, .091f, .041f));
                b.Box(gold, V(side * .384f, -.883f, -.706f), V(.057f, .039f, .016f));
            }
        }

        private static void BuildSwivel(Builder b, Material steel, Material gold, Material ivory,
            Material blue, Material cyan)
        {
            // Visuals space (the existing scale is 1.28). These parts follow the existing
            // yaw only. The raised rails stay outside the sphere and below its center.
            b.Prism(ivory, Octagon(.350f, -.078f), -.660f, -.132f, .014f);
            b.Prism(gold, Octagon(.369f, -.078f), -.626f, -.599f, .006f);
            b.Prism(steel, Octagon(.395f, -.078f), -.161f, -.101f, .010f);
            for (int index = 0; index < 8; index++)
            {
                float angle = index * 45f;
                b.RadialBox(steel, angle, .339f, -.078f, -.401f, .064f, .367f, .018f);
                b.RadialBox(blue, angle, .350f, -.078f, -.400f, .042f, .304f, .012f);
                b.RadialBox(cyan, angle, .358f, -.078f, -.400f, .013f, .227f, .007f);
            }

            var deck = new[]
            {
                P(-.340f, .530f), P(.340f, .530f), P(.550f, .280f), P(.550f, -.330f),
                P(.320f, -.660f), P(-.320f, -.660f), P(-.550f, -.330f), P(-.550f, .280f)
            };
            b.Prism(steel, deck, -.106f, -.025f, .019f);
            b.Prism(gold, Scale(deck, .989f), -.029f, -.006f, .006f);
            b.Prism(blue, Scale(deck, .940f), -.006f, .026f, .010f);
            b.Prism(steel, Octagon(.318f, .055f), .018f, .038f, .005f);

            for (int side = -1; side <= 1; side += 2)
            {
                var rail = new[]
                {
                    P(side * .320f, .180f), P(side * .460f, .120f), P(side * .510f, -.340f),
                    P(side * .370f, -.530f), P(side * .280f, -.450f)
                };
                b.Prism(gold, rail, .024f, .055f, .008f);
                b.Prism(ivory, Scale(rail, .850f), .046f, .083f, .011f);
                b.Box(steel, V(side * .471f, .050f, -.091f), V(.035f, .049f, .282f), Quaternion.Euler(0, side * -6f, 0));
                b.Box(cyan, V(side * .488f, .078f, -.091f), V(.012f, .009f, .198f), Quaternion.Euler(0, side * -6f, 0));

                // Sockets and fasteners are inset into the broad upper armor shoulder.
                foreach (float z in new[] { -.245f, -.343f })
                {
                    b.Prism(steel, Polygon(8, .028f, side * .408f, z), .080f, .090f, .003f);
                    b.Prism(gold, Polygon(8, .021f, side * .408f, z), .088f, .094f, .002f);
                    b.Prism(cyan, Polygon(8, .014f, side * .408f, z), .093f, .097f, .001f);
                }
                foreach (float z in new[] { .019f, -.443f })
                    b.Prism(steel, Polygon(8, .012f, side * .365f, z), .081f, .088f, .002f);

                // Low longitudinal brass rails frame the armor without becoming walls.
                b.Beam(gold, V(side * .515f, .024f, .219f), V(side * .515f, .024f, -.276f), .028f, .042f);
                b.Beam(gold, V(side * .515f, .024f, -.276f), V(side * .286f, .024f, -.603f), .028f, .042f);
                b.Beam(cyan, V(side * .533f, -.025f, .153f), V(side * .533f, -.025f, -.226f), .009f, .011f);
            }

            var shield = new[]
            {
                P(-.126f, -.403f), P(.126f, -.403f), P(.203f, -.524f),
                P(.128f, -.652f), P(-.128f, -.652f), P(-.203f, -.524f)
            };
            b.Prism(gold, shield, .025f, .055f, .008f);
            b.Prism(steel, Scale(shield, .885f), .054f, .073f, .006f);
            b.Prism(blue, Scale(shield, .744f), .070f, .084f, .004f);
            b.Beam(cyan, V(-.058f, .090f, -.575f), V(0, .090f, -.476f), .011f, .006f);
            b.Beam(cyan, V(0, .090f, -.476f), V(.058f, .090f, -.575f), .011f, .006f);
            b.Beam(cyan, V(.058f, .090f, -.575f), V(-.058f, .090f, -.575f), .011f, .006f);
            b.Box(gold, V(0, .039f, -.382f), V(.245f, .036f, .021f));
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static Vector2 P(float x, float z) => new Vector2(x, z);
        private static Vector2[] Octagon(float radius, float z) => Polygon(8, radius, 0, z);

        private static Vector2[] Polygon(int sides, float radius, float x, float z)
        {
            var points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + .5f) * Mathf.PI * 2f / sides;
                points[i] = P(x + Mathf.Cos(angle) * radius, z + Mathf.Sin(angle) * radius);
            }
            return points;
        }

        private static Vector2[] Scale(Vector2[] outline, float scale)
        {
            Vector2 center = Vector2.zero;
            foreach (Vector2 point in outline) center += point;
            center /= outline.Length;
            var result = new Vector2[outline.Length];
            for (int i = 0; i < outline.Length; i++) result[i] = center + (outline[i] - center) * scale;
            return result;
        }

        private static Material RequireMaterial(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_" + name + ".mat");
            if (material == null) throw new InvalidOperationException("Missing platform material (complete Phase 32.4 first): " + name);
            return material;
        }

        private static Transform RequireChild(Transform parent, string path)
        {
            Transform child = parent.Find(path);
            if (child == null) throw new InvalidOperationException("Required mortar platform object is missing: " + path);
            return child;
        }

        private static Transform OwnRoot(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null) { child = new GameObject(name).transform; child.SetParent(parent, false); }
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.localScale = Vector3.one;
            child.gameObject.layer = parent.gameObject.layer;
            return child;
        }

        private static int CountArt(Transform root, RevisionReport report, List<MeshRecord> records)
        {
            int triangles = 0;
            report.ArtRenderers += root.GetComponentsInChildren<MeshRenderer>(true).Length;
            report.ArtColliders += root.GetComponentsInChildren<Collider>(true).Length;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                int meshTriangles = (int)mesh.GetIndexCount(0) / 3;
                report.ArtVertices += mesh.vertexCount;
                triangles += meshTriangles;
                records.Add(new MeshRecord { Name = mesh.name, Vertices = mesh.vertexCount, Triangles = meshTriangles });
            }
            report.ArtTriangles += triangles;
            return triangles;
        }

        private static Mesh StoreMesh(string name, Mesh mesh)
        {
            string path = MESH_PREFIX + name + ".asset";
            Mesh stored = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.name = "Phase32_MortarPlatform_" + name;
            if (stored == null) { AssetDatabase.CreateAsset(mesh, path); stored = mesh; }
            else
            {
                EditorUtility.CopySerialized(mesh, stored);
                UnityEngine.Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(stored);
            }
            AssetDatabase.SaveAssetIfDirty(stored);
            return stored;
        }

        private sealed class Builder : IDisposable
        {
            private readonly string _prefix;
            private readonly Mesh _cube;
            private readonly List<Mesh> _temporary = new List<Mesh>();
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            private readonly Dictionary<Material, string> _names = new Dictionary<Material, string>();
            public Builder(string prefix, Mesh cube) { _prefix = prefix; _cube = cube; }
            public void Register(Material material, string name)
            { _parts.Add(material, new List<CombineInstance>()); _names.Add(material, name); }
            public void Box(Material material, Vector3 position, Vector3 scale)
                => Box(material, position, scale, Quaternion.identity);
            public void Box(Material material, Vector3 position, Vector3 scale, Quaternion rotation)
                => Add(_cube, material, Matrix4x4.TRS(position, rotation, scale));

            public void Beam(Material material, Vector3 from, Vector3 to, float width, float height)
            {
                Vector3 direction = to - from;
                Box(material, (from + to) * .5f, V(width, height, direction.magnitude), Quaternion.LookRotation(direction, Vector3.up));
            }

            public void RadialBox(Material material, float angle, float radius, float centerZ,
                float y, float width, float height, float depth)
            {
                float a = angle * Mathf.Deg2Rad;
                Box(material, V(Mathf.Cos(a) * radius, y, centerZ + Mathf.Sin(a) * radius),
                    V(width, height, depth), Quaternion.Euler(0, 90f - angle, 0));
            }

            public void Prism(Material material, Vector2[] outline, float bottom, float top, float bevel)
            {
                Mesh mesh = BuildPrism(outline, bottom, top, bevel);
                _temporary.Add(mesh); Add(mesh, material, Matrix4x4.identity);
            }

            private void Add(Mesh mesh, Material material, Matrix4x4 matrix)
                => _parts[material].Add(new CombineInstance { mesh = mesh, transform = matrix });

            public void Bake(Transform root)
            {
                foreach (var group in _parts)
                {
                    int vertices = 0;
                    foreach (CombineInstance part in group.Value) vertices += part.mesh.vertexCount;
                    var mesh = new Mesh { indexFormat = vertices < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32 };
                    mesh.CombineMeshes(group.Value.ToArray(), true, true);
                    mesh.RecalculateBounds(); mesh.RecalculateTangents();
                    Mesh stored = StoreMesh(_prefix + "_" + _names[group.Key], mesh);
                    Transform child = OwnRoot(root, _names[group.Key]);
                    MeshFilter filter = child.GetComponent<MeshFilter>();
                    if (filter == null) filter = child.gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer == null) renderer = child.gameObject.AddComponent<MeshRenderer>();
                    filter.sharedMesh = stored; renderer.sharedMaterial = group.Key; renderer.enabled = true;
                    renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
                }
            }

            public void Dispose()
            {
                foreach (Mesh mesh in _temporary) UnityEngine.Object.DestroyImmediate(mesh);
                _temporary.Clear();
            }

            private static Mesh BuildPrism(Vector2[] outline, float bottom, float top, float bevel)
            {
                var data = new MeshData();
                Vector2 center = Vector2.zero;
                foreach (Vector2 point in outline) center += point;
                center /= outline.Length;
                float radius = float.MaxValue;
                foreach (Vector2 point in outline) radius = Mathf.Min(radius, Vector2.Distance(point, center));
                float inset = Mathf.Clamp01(1f - bevel / Mathf.Max(radius, .0001f));
                Vector2[] inner = Scale(outline, inset);
                float[] heights = { bottom, bottom + bevel, top - bevel, top };
                for (int edge = 0; edge < outline.Length; edge++)
                {
                    int next = (edge + 1) % outline.Length;
                    Vector2 middle = (outline[edge] + outline[next]) * .5f - center;
                    Vector3 outward = V(middle.x, 0, middle.y);
                    for (int level = 0; level < 3; level++)
                    {
                        Vector2[] low = level == 0 ? inner : outline;
                        Vector2[] high = level == 2 ? inner : outline;
                        data.Face(outward, At(low[edge], heights[level]), At(low[next], heights[level]),
                            At(high[next], heights[level + 1]), At(high[edge], heights[level + 1]));
                    }
                }
                var upper = new Vector3[outline.Length];
                var lower = new Vector3[outline.Length];
                for (int i = 0; i < outline.Length; i++)
                { upper[i] = At(inner[i], top); lower[i] = At(inner[i], bottom); }
                data.Face(Vector3.up, upper); data.Face(Vector3.down, lower);
                return data.Build();
            }

            private static Vector3 At(Vector2 point, float y) => V(point.x, y, point.y);
        }

        private sealed class MeshData
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<int> _triangles = new List<int>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            public void Face(Vector3 normal, params Vector3[] points)
            {
                if (Vector3.Dot(Vector3.Cross(points[1] - points[0], points[2] - points[0]), normal) < 0)
                    Array.Reverse(points);
                int first = _vertices.Count;
                foreach (Vector3 point in points)
                { _vertices.Add(point); _uv.Add(P(point.x, point.z)); }
                for (int i = 1; i < points.Length - 1; i++)
                { _triangles.Add(first); _triangles.Add(first + i); _triangles.Add(first + i + 1); }
            }
            public Mesh Build()
            {
                var mesh = new Mesh(); mesh.SetVertices(_vertices); mesh.SetTriangles(_triangles, 0); mesh.SetUVs(0, _uv);
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents(); return mesh;
            }
        }

        private sealed class PreservationSnapshot
        {
            private readonly Dictionary<Transform, PoseState> _poses = new Dictionary<Transform, PoseState>();
            private readonly Dictionary<Component, string> _components = new Dictionary<Component, string>();
            public int TransformCount => _poses.Count;
            public PreservationSnapshot(GameObject root)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    _poses.Add(transform, new PoseState(transform));
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    if (component is MonoBehaviour || component is Collider || component is Rigidbody)
                        _components.Add(component, EditorJsonUtility.ToJson(component));
            }
            public void Verify()
            {
                foreach (var entry in _poses)
                    if (entry.Key == null || !entry.Value.Matches(entry.Key))
                        throw new InvalidOperationException("A pre-existing mortar transform changed during platform revision.");
                foreach (var entry in _components)
                    if (entry.Key == null || EditorJsonUtility.ToJson(entry.Key) != entry.Value)
                        throw new InvalidOperationException("A mortar gameplay component changed during platform revision.");
            }
            private sealed class PoseState
            {
                private readonly Vector3 _position, _scale;
                private readonly Quaternion _rotation;
                private readonly Transform _parent;
                private readonly string _name;
                private readonly int _layer, _sibling;
                private readonly bool _active;
                public PoseState(Transform transform)
                {
                    _position = transform.localPosition; _scale = transform.localScale;
                    _rotation = transform.localRotation; _parent = transform.parent; _name = transform.name;
                    _layer = transform.gameObject.layer; _active = transform.gameObject.activeSelf;
                    _sibling = transform.GetSiblingIndex();
                }
                public bool Matches(Transform transform) => transform.localPosition == _position &&
                    transform.localScale == _scale && transform.localRotation == _rotation &&
                    transform.parent == _parent && transform.name == _name &&
                    transform.gameObject.layer == _layer && transform.gameObject.activeSelf == _active &&
                    transform.GetSiblingIndex() == _sibling;
            }
        }
    }
}
