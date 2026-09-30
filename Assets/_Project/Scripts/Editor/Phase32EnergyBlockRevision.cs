using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace RoyalSmash.Editor
{
    /// <summary>Revises crystal and core sockets without modifying the shared energy material or gameplay.</summary>
    public static class Phase32EnergyBlockRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel";
        private const string DETAILS = "Phase32EnergyBlockDetails";
        private static readonly Quaternion[] Faces = {
            Quaternion.identity, Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, 180, 0),
            Quaternion.Euler(0, 270, 0), Quaternion.Euler(90, 0, 0), Quaternion.Euler(-90, 0, 0)
        };
        public static RevisionReport LastReport { get; private set; }

        [Serializable]
        public sealed class RevisionReport
        {
            public bool SharedMaterialUnchanged;
            public string EnergyShader;
            public BlockReport[] Blocks;
        }

        [Serializable]
        public sealed class BlockReport
        {
            public string PrefabPath, Guid, PrimaryRendererPath;
            public bool ExistingTransformsPreserved, GameplayComponentsPreserved, PrimaryRendererPreserved;
            public bool EnergyRendererPreserved;
            public int Renderers, Vertices, Triangles, DecorativeColliders;
            public Vector3 VisualBoundsMin, VisualBoundsMax;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/08 - Apply Energy Block Geometry")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising energy blocks.");
            Material ivory = RequireMaterial("IvoryStone"), gold = RequireMaterial("AntiqueGold");
            Material steel = RequireMaterial("ObsidianSteel"), blue = RequireMaterial("RoyalSapphire");
            Material energy = RequireMaterial("ArcaneEnergy");
            Mesh bevel = AssetDatabase.LoadAssetAtPath<Mesh>(ART + "/Meshes/CitadelBeveledCube.asset");
            if (bevel == null || !energy.HasProperty("_BaseColor"))
                throw new InvalidOperationException("The beveled mesh and compatible existing energy material are required.");
            string energyBefore = EditorJsonUtility.ToJson(energy);
            foreach (string variant in new[] { "crystal", "core" })
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(variant)) == null)
                    throw new InvalidOperationException("The existing source block is missing: " + variant);

            var reports = new List<BlockReport>();
            reports.Add(Revise("crystal", bevel, ivory, gold, steel, blue, energy));
            reports.Add(Revise("core", bevel, ivory, gold, steel, blue, energy));
            bool materialUnchanged = EditorJsonUtility.ToJson(energy) == energyBefore;
            if (!materialUnchanged) throw new InvalidOperationException("The shared energy material changed during geometry revision.");
            AssetDatabase.SaveAssets();
            LastReport = new RevisionReport {
                SharedMaterialUnchanged = true, EnergyShader = energy.shader.name, Blocks = reports.ToArray()
            };
            Debug.Log("[Phase32EnergyBlockRevision] " + JsonUtility.ToJson(LastReport));
        }

        private static BlockReport Revise(string variant, Mesh bevel, Material ivory, Material gold,
            Material steel, Material blue, Material energy)
        {
            string path = PrefabPath(variant), guid = AssetDatabase.AssetPathToGUID(PrefabPath(variant));
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var snapshot = new PreservationSnapshot(root);
                Renderer primary = root.GetComponentInChildren<Renderer>();
                string expectedPrimary = variant == "crystal" ? "M_RoyalSapphire" : "M_IvoryStone";
                Transform energyTransform = root.transform.Find("M_ArcaneEnergy");
                MeshRenderer energyRenderer = energyTransform != null ? energyTransform.GetComponent<MeshRenderer>() : null;
                if (primary == null || primary.name != expectedPrimary || !primary.enabled || energyRenderer == null)
                    throw new InvalidOperationException("Unexpected source renderer structure: " + path);
                if (energyRenderer.sharedMaterial != energy)
                    throw new InvalidOperationException("Preserve the custom energy material override on " + path + " for review.");
                BoxCollider collider = root.GetComponent<BoxCollider>();
                if (collider == null || collider.size != Vector3.one || collider.center != Vector3.zero)
                    throw new InvalidOperationException("Expected the existing unit BoxCollider on " + path);

                using (var b = new Builder(variant, bevel, ivory, gold, steel, blue, energy))
                {
                    if (variant == "crystal") BuildCrystal(b, gold, steel, blue, energy);
                    else BuildCore(b, ivory, gold, steel, blue, energy);
                    Transform details = OwnRoot(root.transform, DETAILS);
                    b.Bake(root.transform, details);
                    snapshot.Verify();
                    if (root.GetComponentInChildren<Renderer>() != primary || !primary.enabled ||
                        !primary.sharedMaterial.HasProperty("_BaseColor") ||
                        root.transform.Find("M_ArcaneEnergy").GetComponent<MeshRenderer>() != energyRenderer)
                        throw new InvalidOperationException("A primary or energy renderer contract changed.");
                    BlockReport report = Measure(root, details, primary, path, guid);
                    if (report.Renderers > 5 || report.Triangles >= 5000 || report.DecorativeColliders != 0)
                        throw new InvalidOperationException("Energy block art exceeds its budget: " + JsonUtility.ToJson(report));
                    Vector3 min = report.VisualBoundsMin, max = report.VisualBoundsMax;
                    if (min.x < -.501f || min.y < -.501f || min.z < -.501f || max.x > .501f || max.y > .501f || max.z > .501f)
                        throw new InvalidOperationException("Energy block artwork exceeds the unit collider envelope.");
                    if (PrefabUtility.SaveAsPrefabAsset(root, path) == null)
                        throw new InvalidOperationException("Could not save the revised source prefab: " + path);
                    if (AssetDatabase.AssetPathToGUID(path) != guid)
                        throw new InvalidOperationException("Source prefab GUID changed.");
                    return report;
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BuildCrystal(Builder b, Material gold, Material steel, Material blue, Material energy)
        {
            b.Box(blue, Vector3.zero, Vector3.one * .89f);
            foreach (Quaternion face in Faces)
            {
                // The energy panel remains behind a substantial frame and separated center jewel.
                b.FaceBox(energy, face, V(0, 0, -.452f), V(.79f, .79f, .066f));
                foreach (int sign in new[] { -1, 1 })
                {
                    b.FaceBox(gold, face, V(sign * .442f, 0, -.465f), V(.052f, .884f, .065f));
                    b.FaceBox(gold, face, V(0, sign * .442f, -.465f), V(.884f, .052f, .065f));
                    b.FaceBox(blue, face, V(sign * .403f, 0, -.476f), V(.030f, .77f, .033f));
                    b.FaceBox(blue, face, V(0, sign * .403f, -.476f), V(.77f, .030f, .033f));
                }
                // Layer fronts: panel -.485, steel -.486, gold -.490, blue -.492,
                // then the complete jewel rim at -.493 and apex at -.499.
                b.FaceBox(steel, face, V(0, 0, -.477f), V(.254f, .254f, .018f), 45);
                b.FaceBox(gold, face, V(0, 0, -.483f), V(.217f, .217f, .014f), 45);
                b.FaceBox(blue, face, V(0, 0, -.487f), V(.157f, .157f, .010f), 45);
                b.Gem(energy, face, V(0, 0, -.493f), V(.158f, .226f, .012f));
                FaceBolts(b, face, steel, .414f, .036f);
            }
            CornerCaps(b, gold, .180f);
        }

        private static void BuildCore(Builder b, Material ivory, Material gold, Material steel, Material blue, Material energy)
        {
            // The original ivory body remains the first renderer, visible around the blue armor edges.
            b.Box(ivory, Vector3.zero, Vector3.one * .90f);
            foreach (Quaternion face in Faces)
            {
                b.FaceBox(blue, face, V(0, 0, -.456f), V(.874f, .874f, .027f));
                b.Ring(steel, face, .371f, .252f, -.477f, .014f, false);
                b.Ring(blue, face, .365f, .326f, -.487f, .012f, false);
                b.Ring(gold, face, .329f, .271f, -.489f, .018f, true);
                b.Lens(energy, face, V(0, 0, -.479f), V(.535f, .535f, .039f));
                for (int clamp = 0; clamp < 4; clamp++)
                {
                    Quaternion around = Quaternion.Euler(0, 0, clamp * 90);
                    b.FaceBox(gold, face, around * V(0, .382f, -.477f), V(.117f, .218f, .034f), clamp * 90);
                }
                FaceBolts(b, face, gold, .406f, .038f);
            }
            CornerCaps(b, steel, .182f);
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

        private static BlockReport Measure(GameObject root, Transform details, Renderer primary, string path, string guid)
        {
            var report = new BlockReport {
                PrefabPath = path, Guid = guid, PrimaryRendererPath = primary.name,
                ExistingTransformsPreserved = true, GameplayComponentsPreserved = true, PrimaryRendererPreserved = true,
                EnergyRendererPreserved = true, DecorativeColliders = details.GetComponentsInChildren<Collider>(true).Length
            };
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeSelf) continue;
                Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                report.Renderers++; report.Vertices += mesh.vertexCount;
                report.Triangles += (int)mesh.GetIndexCount(0) / 3;
                Matrix4x4 toRoot = root.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                foreach (Vector3 point in mesh.vertices) bounds.Encapsulate(toRoot.MultiplyPoint3x4(point));
            }
            report.VisualBoundsMin = bounds.min; report.VisualBoundsMax = bounds.max;
            return report;
        }

        private static string PrefabPath(string variant) => ART + "/Prefabs/PF_" + variant + ".prefab";
        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static Material RequireMaterial(string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ART + "/Materials/M_" + name + ".mat");
            if (material == null) throw new InvalidOperationException("Missing existing material: " + name);
            return material;
        }
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
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) _poses.Add(t, new PoseState(t));
                foreach (Component c in root.GetComponentsInChildren<Component>(true))
                    if (c != null && !(c is Transform) && !(c is Renderer) && !(c is MeshFilter))
                        _components.Add(c, EditorJsonUtility.ToJson(c));
            }
            public void Verify()
            {
                foreach (var entry in _poses)
                    if (entry.Key == null || !entry.Value.Matches(entry.Key))
                        throw new InvalidOperationException("A pre-existing energy block transform changed.");
                foreach (var entry in _components)
                    if (entry.Key == null || EditorJsonUtility.ToJson(entry.Key) != entry.Value)
                        throw new InvalidOperationException("An energy block gameplay component changed.");
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
            private readonly Mesh _bevel, _gem, _lens, _bolt;
            private readonly List<Mesh> _temporary = new List<Mesh>();
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();
            public Builder(string variant, Mesh bevel, params Material[] materials)
            {
                _variant = variant; _bevel = bevel; _gem = CreateGem(); _lens = CreateLens(); _bolt = CreateBolt();
                _temporary.Add(_gem); _temporary.Add(_lens); _temporary.Add(_bolt);
                foreach (Material material in materials) _parts.Add(material, new List<CombineInstance>());
            }
            public void Box(Material m, Vector3 p, Vector3 s) => Add(_bevel, m, p, Quaternion.identity, s);
            public void FaceBox(Material m, Quaternion face, Vector3 p, Vector3 s, float angle = 0)
                => Add(_bevel, m, face * p, face * Quaternion.Euler(0, 0, angle), s);
            public void Gem(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_gem, m, face * p, face, s);
            public void Lens(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_lens, m, face * p, face, s);
            public void Bolt(Material m, Quaternion face, Vector3 p, Vector3 s) => Add(_bolt, m, face * p, face, s);
            public void Ring(Material m, Quaternion face, float outer, float inner, float z, float depth, bool bevel)
            {
                Mesh mesh = CreateRing(outer, inner, depth, bevel);
                _temporary.Add(mesh); Add(mesh, m, face * V(0, 0, z), face, Vector3.one);
            }
            private void Add(Mesh mesh, Material m, Vector3 p, Quaternion q, Vector3 s)
                => _parts[m].Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(p, q, s) });

            public void Bake(Transform root, Transform details)
            {
                foreach (var group in _parts)
                {
                    if (group.Value.Count == 0) continue;
                    // Preserve original GameObjects and renderers; only missing material groups are new.
                    Transform target = root.Find(group.Key.name);
                    if (target == null) target = OwnRoot(details, group.Key.name);
                    Matrix4x4 toTarget = target.worldToLocalMatrix * root.localToWorldMatrix;
                    CombineInstance[] parts = group.Value.ToArray();
                    for (int i = 0; i < parts.Length; i++) parts[i].transform = toTarget * parts[i].transform;
                    string meshName = "Phase32_EnergyBlock_" + _variant + "_" + group.Key.name;
                    Mesh combined = new Mesh { name = meshName, indexFormat = IndexFormat.UInt16 };
                    combined.CombineMeshes(parts, true, true); combined.RecalculateBounds(); combined.RecalculateTangents();
                    string path = ART + "/Meshes/" + meshName + ".asset";
                    Mesh stored = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (stored == null) { stored = combined; AssetDatabase.CreateAsset(stored, path); }
                    else { EditorUtility.CopySerialized(combined, stored); UnityEngine.Object.DestroyImmediate(combined); EditorUtility.SetDirty(stored); }
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
                foreach (Mesh mesh in _temporary) UnityEngine.Object.DestroyImmediate(mesh);
                _temporary.Clear();
            }

            private static Mesh CreateRing(float outer, float inner, float depth, bool bevel)
            {
                float half = depth * .5f, lip = Mathf.Min(depth * .3f, .005f);
                Vector2[] profile = bevel ? new[] {
                    new Vector2(outer - lip, -half), new Vector2(outer, -half + lip), new Vector2(outer, half),
                    new Vector2(inner, half), new Vector2(inner, -half + lip), new Vector2(inner + lip, -half)
                } : new[] {
                    new Vector2(outer, -half), new Vector2(outer, half), new Vector2(inner, half), new Vector2(inner, -half)
                };
                var data = new MeshData();
                const int segments = 12;
                for (int arc = 0; arc < segments; arc++)
                {
                    float a = arc * Mathf.PI * 2 / segments, next = (arc + 1) * Mathf.PI * 2 / segments;
                    Vector3 radial = V(Mathf.Cos((a + next) * .5f), Mathf.Sin((a + next) * .5f), 0);
                    for (int edge = 0; edge < profile.Length; edge++)
                    {
                        Vector2 p = profile[edge], q = profile[(edge + 1) % profile.Length];
                        Vector3 normal = radial * (q.y - p.y) - Vector3.forward * (q.x - p.x);
                        data.Face(normal, RingPoint(p, a), RingPoint(p, next), RingPoint(q, next), RingPoint(q, a));
                    }
                }
                return data.Build();
            }
            private static Vector3 RingPoint(Vector2 p, float a) => V(Mathf.Cos(a) * p.x, Mathf.Sin(a) * p.x, p.y);

            private static Mesh CreateLens()
            {
                var data = new MeshData();
                float[] radius = { .17f, .36f, .5f }, z = { -.45f, -.28f, 0 };
                const int segments = 16;
                for (int i = 0; i < segments; i++)
                {
                    float a = i * Mathf.PI * 2 / segments, next = (i + 1) * Mathf.PI * 2 / segments;
                    data.Face(Vector3.back, V(0, 0, -.5f), LensPoint(radius[0], z[0], a), LensPoint(radius[0], z[0], next));
                    for (int ring = 0; ring < 2; ring++)
                        data.Face(Vector3.back, LensPoint(radius[ring], z[ring], a), LensPoint(radius[ring + 1], z[ring + 1], a),
                            LensPoint(radius[ring + 1], z[ring + 1], next), LensPoint(radius[ring], z[ring], next));
                    data.Face(Vector3.forward, V(0, 0, .2f), LensPoint(.5f, 0, a), LensPoint(.5f, 0, next));
                }
                return data.Build();
            }
            private static Vector3 LensPoint(float radius, float z, float a) => V(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, z);

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
                    Vector3 p = V(Mathf.Cos(a) * .5f, Mathf.Sin(a) * .5f, 0), q = V(Mathf.Cos(next) * .5f, Mathf.Sin(next) * .5f, 0);
                    data.Face((p + q).normalized, p + Vector3.back * .5f, q + Vector3.back * .5f, q + Vector3.forward * .5f, p + Vector3.forward * .5f);
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
                foreach (Vector3 p in points) { _vertices.Add(p); _uv.Add(new Vector2(p.x + .5f, p.y + .5f)); }
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
