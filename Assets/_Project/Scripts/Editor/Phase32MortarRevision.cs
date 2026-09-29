using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace RoyalSmash.Editor
{
    /// <summary>Bakes reference-inspired mortar armor without moving any gameplay transform.</summary>
    public static class Phase32MortarRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        private const string PREFAB = "Assets/_Project/Prefabs/Gameplay/Mortar.prefab";
        private const string BARREL_DETAILS = "Phase32BarrelDetails";
        private const string CRADLE_DETAILS = "Phase32CradleDetails";
        private const string MESH_PREFIX = ART + "/Meshes/Phase32_Mortar_";

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
            public int ArtColliders;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/04 - Apply Mortar Body Revision")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the mortar.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB) == null)
                throw new InvalidOperationException("The existing Mortar prefab is missing.");

            Material gold = RequireMaterial("AntiqueGold");
            Material steel = RequireMaterial("ObsidianSteel");
            Material sapphire = RequireMaterial("RoyalSapphire");
            Material cyan = RequireMaterial("Phase32StandCyan");
            Material stone = RequireMaterial("IvoryStone");
            Mesh cube = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            if (cube == null) throw new InvalidOperationException("The Citadel beveled cube mesh is missing.");

            GameObject contents = PrefabUtility.LoadPrefabContents(PREFAB);
            try
            {
                Transform visuals = RequireChild(contents.transform, "Visuals");
                Transform pivot = RequireChild(visuals, "BarrelPivot");
                RequireChild(pivot, "FirePoint");
                RequireChild(pivot, "BreechSphere_Red");
                RequireChild(contents.transform, "CitadelMount");
                var preserved = new PreservationSnapshot(contents);

                Material ivory = ResolveIvory(stone);
                using (var barrel = new Builder("Barrel", cube, ivory, gold, steel, sapphire, cyan))
                using (var cradle = new Builder("Cradle", cube, ivory, gold, steel, sapphire, cyan))
                {
                    BuildBarrel(barrel, ivory, gold, steel, sapphire, cyan);
                    BuildCradle(cradle, ivory, gold, steel, sapphire, cyan);
                    Transform barrelRoot = OwnRoot(pivot, BARREL_DETAILS);
                    Transform cradleRoot = OwnRoot(visuals, CRADLE_DETAILS);
                    barrel.Bake(barrelRoot);
                    cradle.Bake(cradleRoot);

                    int disabled = DisableReplacedRenderers(visuals, pivot);
                    preserved.Verify();
                    var report = new RevisionReport
                    {
                        PrefabPath = PREFAB,
                        ExistingTransformsPreserved = true,
                        GameplayComponentsPreserved = true,
                        OriginalTransforms = preserved.TransformCount,
                        DisabledLegacyRenderers = disabled
                    };
                    CountArt(barrelRoot, report);
                    CountArt(cradleRoot, report);
                    if (report.ArtColliders != 0)
                        throw new InvalidOperationException("Decorative mortar geometry must not have colliders.");
                    if (PrefabUtility.SaveAsPrefabAsset(contents, PREFAB) == null)
                        throw new InvalidOperationException("Could not save the revised Mortar prefab.");
                    LastReport = report;
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            Debug.Log("[Phase32MortarRevision] " + JsonUtility.ToJson(LastReport));
        }

        private static void BuildBarrel(Builder b, Material ivory, Material gold, Material steel,
            Material blue, Material cyan)
        {
            // BarrelPivot local coordinates. The inherited FirePoint remains at Z=.74;
            // all muzzle decoration ends at .704 and never changes the projectile path.
            b.Shell(steel, .047f, .648f, .183f, .154f, .027f, 0, 360, 48);
            for (int panel = 0; panel < 8; panel++)
            {
                float start = panel * 45f + 2f;
                b.Shell(blue, .064f, .326f, .199f, .187f, .017f, start, 41f, 6);
                b.Shell(blue, .363f, .628f, .185f, .167f, .017f, start, 41f, 6);
            }

            // Curved ivory armor occupies the shoulders, leaving a sapphire dorsal spine.
            // The gold perimeter is a slightly larger curved plate, not a flat decal.
            foreach (float start in new[] { 23f, 113f, 203f, 293f })
            {
                b.Shell(gold, .378f, .612f, .190f, .175f, .018f, start, 44f, 7);
                b.Shell(ivory, .389f, .600f, .193f, .179f, .013f, start + 3.2f, 37.6f, 6);
            }

            b.Ring(steel, .035f, .078f, .211f, .174f, .007f);
            b.Ring(gold, .086f, .128f, .212f, .177f, .008f);
            b.Ring(steel, .318f, .367f, .209f, .174f, .008f);
            b.Ring(gold, .329f, .355f, .219f, .179f, .006f);
            b.Ring(cyan, .359f, .366f, .200f, .185f, .002f);

            // A hollow, beveled lip replaces the primitive torus. The dark inner tube
            // starts behind the muzzle, so its edge reads as actual barrel thickness.
            b.Shell(steel, .586f, .697f, .151f, .151f, .025f, 0, 360, 48);
            b.Ring(steel, .620f, .661f, .191f, .126f, .006f);
            b.Ring(gold, .644f, .699f, .218f, .126f, .012f);
            b.Ring(blue, .660f, .689f, .2195f, .212f, .0025f);
            b.Ring(cyan, .693f, .704f, .145f, .128f, .003f);

            // Gold rails and recessed cyan channels are aligned to the shell tangents.
            foreach (float angle in new[] { 0f, 90f, 180f, 270f })
            {
                b.TangentBox(steel, angle, .191f, .238f, .043f, .014f, .137f);
                b.TangentBox(cyan, angle, .200f, .238f, .016f, .007f, .102f);
                b.TangentBox(gold, angle, .205f, .107f, .053f, .030f, .052f);
                b.TangentBox(gold, angle, .213f, .342f, .052f, .022f, .051f);
                b.TangentBox(gold, angle, .211f, .671f, .059f, .023f, .063f);
                b.TangentBox(steel, angle, .199f, .395f, .029f, .011f, .023f);
                b.TangentBox(gold, angle, .207f, .395f, .013f, .009f, .013f);
            }

            // Four narrow curved braces surround the existing energy sphere. Its mesh,
            // material, transform and central gold cascabel are deliberately preserved.
            var braceProfile = new[]
            {
                P(.124f, -.239f), P(.177f, -.199f), P(.215f, -.128f),
                P(.230f, -.040f), P(.215f, .051f), P(.184f, .112f),
                P(.170f, .109f), P(.202f, .047f), P(.217f, -.040f),
                P(.202f, -.121f), P(.165f, -.189f), P(.117f, -.224f)
            };
            foreach (float angle in new[] { 27f, 143f, 217f, 323f })
            {
                b.Profile(gold, braceProfile, angle - 4f, 8f, 3);
                b.TangentBox(steel, angle, .224f, -.064f, .047f, .015f, .056f);
                b.TangentBox(cyan, angle, .235f, -.064f, .019f, .008f, .030f);
            }
            b.Ring(steel, -.185f, -.158f, .195f, .171f, .005f);
            b.Ring(gold, -.183f, -.166f, .204f, .177f, .005f);
            b.Ring(cyan, -.1845f, -.180f, .190f, .181f, .0015f);

            // Small raised diamond on the top barrel panel, legible from the game camera.
            b.Box(gold, V(0, .187f, .488f), V(.077f, .014f, .077f), Quaternion.Euler(0, 45, 0));
            b.Box(blue, V(0, .196f, .488f), V(.053f, .007f, .053f), Quaternion.Euler(0, 45, 0));
            b.Box(cyan, V(0, .201f, .488f), V(.025f, .005f, .025f), Quaternion.Euler(0, 45, 0));
        }

        private static void BuildCradle(Builder b, Material ivory, Material gold, Material steel,
            Material blue, Material cyan)
        {
            // Visuals coordinates: these brackets support the animated BarrelPivot but
            // do not follow its elevation. The existing platter and CitadelMount remain.
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .265f;
                b.Box(steel, V(x, .158f, -.015f), V(.092f, .262f, .340f));
                b.Box(blue, V(side * .282f, .194f, -.021f), V(.085f, .233f, .300f));
                b.Box(ivory, V(side * .325f, .168f, -.022f), V(.023f, .179f, .254f));
                b.Box(gold, V(side * .328f, .265f, -.019f), V(.031f, .031f, .244f));
                b.Box(gold, V(side * .328f, .068f, -.019f), V(.033f, .025f, .264f));
                b.Box(gold, V(side * .305f, .143f, -.183f), V(.076f, .205f, .027f));
                b.Box(steel, V(side * .305f, .167f, -.199f), V(.048f, .113f, .016f));
                b.Box(cyan, V(side * .305f, .174f, -.210f), V(.016f, .076f, .007f));

                // Machined concentric axle caps run along X and have an inset blue hub.
                Quaternion axleRotation = Quaternion.Euler(0, side * 90f, 0);
                Vector3 hub = V(side * .319f, .225f, -.010f);
                b.Ring(steel, -.012f, .018f, .095f, .022f, .006f, hub, axleRotation, 24);
                b.Ring(gold, .016f, .033f, .088f, .057f, .004f, hub, axleRotation, 24);
                b.Ring(blue, .027f, .037f, .060f, .008f, .003f, hub, axleRotation, 24);
                b.Ring(cyan, .036f, .040f, .040f, .020f, .001f, hub, axleRotation, 24);
                b.Ring(gold, .039f, .045f, .024f, .002f, .0015f, hub, axleRotation, 16);
                foreach (float z in new[] { -.124f, .090f })
                {
                    b.Box(steel, V(side * .342f, .109f, z), V(.012f, .025f, .025f));
                    b.Box(gold, V(side * .350f, .109f, z), V(.006f, .010f, .010f));
                }
            }
        }

        private static int DisableReplacedRenderers(Transform visuals, Transform pivot)
        {
            int count = 0;
            string[] barrelPaths =
            {
                "MidBody_Red", "FrontBarrel_Blue", "MidRing_Gold", "FrontBand_Gold",
                "MuzzleBore_Dark", "MuzzleLip_TorusGold", "CrownInsignia/CrownBase",
                "CrownInsignia/CrownPoint_-1", "CrownInsignia/CrownPoint_0", "CrownInsignia/CrownPoint_1",
                "Stud_0", "Stud_1", "Stud_2", "Stud_3", "Stud_4"
            };
            foreach (string path in barrelPaths) count += DisableAt(pivot, path);
            Transform bands = pivot.Find("CitadelBands");
            if (bands != null)
                foreach (Transform child in bands)
                    if (child.name == "GoldEnergyBrace") count += DisableRenderer(child);
            count += DisableAt(visuals, "CradleMount/CradleCheek_Left");
            count += DisableAt(visuals, "CradleMount/CradleCheek_Right");
            count += DisableAt(visuals, "CradleMount/CradleCheek_Left/BearingCap_Left");
            count += DisableAt(visuals, "CradleMount/CradleCheek_Right/BearingCap_Right");
            return count;
        }

        private static int DisableAt(Transform root, string path)
        {
            Transform child = root.Find(path);
            return child == null ? 0 : DisableRenderer(child);
        }

        private static int DisableRenderer(Transform child)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null) return 0;
            renderer.enabled = false;
            return 1;
        }

        private static Transform RequireChild(Transform parent, string path)
        {
            Transform child = parent.Find(path);
            if (child == null) throw new InvalidOperationException("Required mortar object is missing: " + path);
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

        private static Material RequireMaterial(string name)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_" + name + ".mat");
            if (material == null) throw new InvalidOperationException("Missing existing mortar material: " + name);
            return material;
        }

        private static Material ResolveIvory(Material source)
        {
            const string path = ART + "/Materials/M_Phase32MortarIvory.mat";
            var ivory = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (ivory != null) return ivory;
            ivory = new Material(source) { name = "M_Phase32MortarIvory" };
            ivory.SetTexture("_BaseMap", null);
            if (ivory.HasProperty("_MainTex")) ivory.SetTexture("_MainTex", null);
            ivory.SetColor("_BaseColor", new Color(.94f, .958f, 1f));
            ivory.SetFloat("_Metallic", .28f);
            ivory.SetFloat("_Smoothness", .68f);
            AssetDatabase.CreateAsset(ivory, path);
            AssetDatabase.SaveAssetIfDirty(ivory);
            return ivory;
        }

        private static void CountArt(Transform root, RevisionReport report)
        {
            report.ArtRenderers += root.GetComponentsInChildren<MeshRenderer>(true).Length;
            report.ArtColliders += root.GetComponentsInChildren<Collider>(true).Length;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                report.ArtVertices += filter.sharedMesh.vertexCount;
                report.ArtTriangles += (int)filter.sharedMesh.GetIndexCount(0) / 3;
            }
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static Vector2 P(float radius, float z) => new Vector2(radius, z);

        private static Mesh StoreMesh(string name, Mesh mesh)
        {
            string path = MESH_PREFIX + name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.name = "Phase32_Mortar_" + name;
            if (existing == null) { AssetDatabase.CreateAsset(mesh, path); existing = mesh; }
            else
            {
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(existing);
            }
            AssetDatabase.SaveAssetIfDirty(existing);
            return existing;
        }

        /// <summary>Checks every pre-existing transform and gameplay component in memory before saving.</summary>
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
                foreach (var item in _poses)
                    if (item.Key == null || !item.Value.Matches(item.Key))
                        throw new InvalidOperationException("A pre-existing mortar transform changed during art revision.");
                foreach (var item in _components)
                    if (item.Key == null || EditorJsonUtility.ToJson(item.Key) != item.Value)
                        throw new InvalidOperationException("A mortar script, collider or rigidbody changed during art revision.");
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
                    _rotation = transform.localRotation; _parent = transform.parent;
                    _name = transform.name; _layer = transform.gameObject.layer;
                    _active = transform.gameObject.activeSelf; _sibling = transform.GetSiblingIndex();
                }
                public bool Matches(Transform transform) => transform.localPosition == _position &&
                    transform.localScale == _scale && transform.localRotation == _rotation &&
                    transform.parent == _parent && transform.name == _name &&
                    transform.gameObject.layer == _layer && transform.gameObject.activeSelf == _active &&
                    transform.GetSiblingIndex() == _sibling;
            }
        }

        private sealed class Builder : IDisposable
        {
            private readonly string _name;
            private readonly Mesh _cube;
            private readonly List<Mesh> _temporary = new List<Mesh>();
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            private readonly Dictionary<Material, string> _names = new Dictionary<Material, string>();

            public Builder(string name, Mesh cube, Material ivory, Material gold, Material steel, Material blue, Material cyan)
            {
                _name = name; _cube = cube;
                Register(ivory, "Ivory"); Register(gold, "Gold"); Register(steel, "Steel");
                Register(blue, "Sapphire"); Register(cyan, "Energy");
            }

            private void Register(Material material, string name)
            {
                _parts.Add(material, new List<CombineInstance>()); _names.Add(material, name);
            }

            public void Box(Material material, Vector3 position, Vector3 scale)
                => Box(material, position, scale, Quaternion.identity);
            public void Box(Material material, Vector3 position, Vector3 scale, Quaternion rotation)
                => Add(_cube, material, position, rotation, scale);

            public void TangentBox(Material material, float angle, float radius, float z,
                float width, float height, float length)
            {
                float a = angle * Mathf.Deg2Rad;
                Box(material, V(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, z),
                    V(width, height, length), Quaternion.Euler(0, 0, angle - 90f));
            }

            public void Shell(Material material, float z0, float z1, float r0, float r1,
                float thickness, float start, float sweep, int segments)
            {
                float bevel = Mathf.Min(.004f, (z1 - z0) * .12f);
                Profile(material, new[]
                {
                    P(r0 - bevel, z0), P(r0, z0 + bevel), P(r1, z1 - bevel), P(r1 - bevel, z1),
                    P(r1 - thickness, z1), P(r0 - thickness, z0)
                }, start, sweep, segments);
            }

            public void Ring(Material material, float z0, float z1, float outer, float inner, float bevel)
                => Ring(material, z0, z1, outer, inner, bevel, Vector3.zero, Quaternion.identity, 48);

            public void Ring(Material material, float z0, float z1, float outer, float inner, float bevel,
                Vector3 position, Quaternion rotation, int segments)
            {
                // A short circular fillet gives the metal a curved highlight instead of
                // one broad 45-degree chamfer. Endpoints keep the original ring bounds.
                var profile = new List<Vector2>();
                var normals = new List<Vector2>();
                AddFillet(profile, normals, P(outer - bevel, z0 + bevel), bevel, -90f);
                AddFillet(profile, normals, P(outer - bevel, z1 - bevel), bevel, 0f);
                AddFillet(profile, normals, P(inner + bevel, z1 - bevel), bevel, 90f);
                AddFillet(profile, normals, P(inner + bevel, z0 + bevel), bevel, 180f);
                // Analytic circumferential normals allow a slightly lower radial count:
                // 48 -> 40 and 24 -> 20 keep the extra fillet geometry modest.
                int angularSegments = segments >= 24 ? Mathf.RoundToInt(segments * 5f / 6f) : segments;
                Mesh mesh = Lathe(profile.ToArray(), 0, 360, angularSegments, normals.ToArray());
                _temporary.Add(mesh); Add(mesh, material, position, rotation, Vector3.one);
            }

            private static void AddFillet(List<Vector2> profile, List<Vector2> normals,
                Vector2 center, float radius, float startAngle)
            {
                for (int point = 0; point < 3; point++)
                {
                    float angle = (startAngle + point * 45f) * Mathf.Deg2Rad;
                    Vector2 normal = P(Mathf.Cos(angle), Mathf.Sin(angle));
                    profile.Add(center + normal * radius);
                    normals.Add(normal);
                }
            }

            public void Profile(Material material, Vector2[] profile, float start, float sweep, int segments)
            {
                Mesh mesh = Lathe(profile, start, sweep, segments);
                _temporary.Add(mesh); Add(mesh, material, Vector3.zero, Quaternion.identity, Vector3.one);
            }

            private void Add(Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
                => _parts[material].Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });

            public void Bake(Transform root)
            {
                foreach (var group in _parts)
                {
                    if (group.Value.Count == 0) continue;
                    int vertexCount = 0;
                    foreach (CombineInstance part in group.Value) vertexCount += part.mesh.vertexCount;
                    var combined = new Mesh { indexFormat = vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32 };
                    combined.CombineMeshes(group.Value.ToArray(), true, true);
                    combined.RecalculateBounds(); combined.RecalculateTangents();
                    Mesh stored = StoreMesh(_name + "_" + _names[group.Key], combined);
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

            private static Mesh Lathe(Vector2[] profile, float start, float sweep, int segments,
                Vector2[] profileNormals = null)
            {
                var data = new MeshData();
                for (int arc = 0; arc < segments; arc++)
                {
                    float a = (start + sweep * arc / segments) * Mathf.Deg2Rad;
                    float next = (start + sweep * (arc + 1) / segments) * Mathf.Deg2Rad;
                    for (int edge = 0; edge < profile.Length; edge++)
                    {
                        int nextEdge = (edge + 1) % profile.Length;
                        Vector2 p = profile[edge], q = profile[nextEdge];
                        Vector2 edgeNormal = P(q.y - p.y, p.x - q.x).normalized;
                        // Shell contours keep hard profile corners; rounded ring fillets
                        // supply exact curve normals. Neither path averages across caps.
                        Vector2 pNormal = profileNormals == null ? edgeNormal : profileNormals[edge];
                        Vector2 qNormal = profileNormals == null ? edgeNormal : profileNormals[nextEdge];
                        data.Surface(new[] { Point(p, a), Point(p, next), Point(q, next), Point(q, a) },
                            new[] { Normal(pNormal, a), Normal(pNormal, next), Normal(qNormal, next), Normal(qNormal, a) });
                    }
                }
                if (sweep < 359.9f)
                {
                    // Profile is a thin, potentially concave cross-section. A fan would
                    // bridge the hollow interior, so triangulate both cuts by ear clipping.
                    int[] cap = Triangulate(profile);
                    for (int end = 0; end < 2; end++)
                    {
                        float angle = (start + sweep * end) * Mathf.Deg2Rad;
                        Vector3 normal = V(-Mathf.Sin(angle), Mathf.Cos(angle), 0) * (end == 0 ? -1 : 1);
                        for (int t = 0; t < cap.Length; t += 3)
                            data.Face(normal, Point(profile[cap[t]], angle), Point(profile[cap[t + 1]], angle), Point(profile[cap[t + 2]], angle));
                    }
                }
                return data.Build();
            }

            private static Vector3 Point(Vector2 p, float angle) => V(p.x * Mathf.Cos(angle), p.x * Mathf.Sin(angle), p.y);
            private static Vector3 Normal(Vector2 p, float angle) => V(p.x * Mathf.Cos(angle), p.x * Mathf.Sin(angle), p.y).normalized;

            private static int[] Triangulate(Vector2[] points)
            {
                var remaining = new List<int>();
                var output = new List<int>();
                float area = 0;
                for (int i = 0; i < points.Length; i++)
                {
                    remaining.Add(i);
                    Vector2 a = points[i], b = points[(i + 1) % points.Length];
                    area += a.x * b.y - b.x * a.y;
                }
                if (area < 0) remaining.Reverse();
                int guard = points.Length * points.Length;
                while (remaining.Count > 2 && guard-- > 0)
                {
                    bool removed = false;
                    for (int i = 0; i < remaining.Count; i++)
                    {
                        int a = remaining[(i + remaining.Count - 1) % remaining.Count];
                        int b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                        if (Cross(points[b] - points[a], points[c] - points[b]) <= 1e-9f) continue;
                        bool contains = false;
                        foreach (int other in remaining)
                        {
                            if (other == a || other == b || other == c) continue;
                            Vector2 p = points[other];
                            if (Cross(points[b] - points[a], p - points[a]) > -1e-9f &&
                                Cross(points[c] - points[b], p - points[b]) > -1e-9f &&
                                Cross(points[a] - points[c], p - points[c]) > -1e-9f)
                            { contains = true; break; }
                        }
                        if (contains) continue;
                        output.Add(a); output.Add(b); output.Add(c);
                        remaining.RemoveAt(i); removed = true; break;
                    }
                    if (!removed) throw new InvalidOperationException("Mortar shell cap could not be triangulated.");
                }
                return output.ToArray();
            }

            private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        }

        private sealed class MeshData
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<Vector3> _normals = new List<Vector3>();
            private readonly List<int> _triangles = new List<int>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            public void Face(Vector3 normal, params Vector3[] points)
            {
                var normals = new Vector3[points.Length];
                for (int i = 0; i < normals.Length; i++) normals[i] = normal.normalized;
                Surface(points, normals);
            }
            public void Surface(Vector3[] points, Vector3[] normals)
            {
                Vector3 hint = Vector3.zero;
                foreach (Vector3 normal in normals) hint += normal;
                if (Vector3.Dot(Vector3.Cross(points[1] - points[0], points[2] - points[0]), hint) < 0)
                { Array.Reverse(points); Array.Reverse(normals); }
                int first = _vertices.Count;
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 point = points[i];
                    _vertices.Add(point);
                    _normals.Add(normals[i]);
                    _uv.Add(new Vector2(Mathf.Atan2(point.y, point.x) / (Mathf.PI * 2f), point.z));
                }
                for (int i = 1; i < points.Length - 1; i++)
                { _triangles.Add(first); _triangles.Add(first + i); _triangles.Add(first + i + 1); }
            }
            public Mesh Build()
            {
                var mesh = new Mesh();
                mesh.SetVertices(_vertices); mesh.SetNormals(_normals); mesh.SetTriangles(_triangles, 0); mesh.SetUVs(0, _uv);
                mesh.RecalculateBounds(); mesh.RecalculateTangents();
                return mesh;
            }
        }
    }
}
