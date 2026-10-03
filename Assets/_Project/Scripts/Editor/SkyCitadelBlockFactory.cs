using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>Builds reusable, physics-ready citadel blocks with baked decorative meshes.</summary>
    public static class SkyCitadelBlockFactory
    {
        private const string MESH_FOLDER = "Assets/_Project/Art/SkyCitadel/Meshes";
        private static Mesh _bevelMesh;
        private static Mesh _sphereMesh;
        private static Mesh _ringMesh;

        public static GameObject CreatePrefab(string path, string variant, Material stone,
            Material gold, Material blue, Material dark, Material energy, Material wood)
        {
            if (variant != "stone" && variant != "wood" && variant != "crystal" &&
                variant != "core" && variant != "armored")
                throw new ArgumentException("Unknown citadel block variant: " + variant, nameof(variant));

            EnsureFolder(MESH_FOLDER);
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var root = new GameObject("SkyCitadel_" + variant) { layer = 7 };
            try
            {
                var builder = new BlockBuilder();
                Material body = variant == "wood" ? wood : variant == "crystal" ? blue : stone;
                builder.Box(body, Vector3.zero, Vector3.one * 0.98f);

                for (int side = 0; side < 4; side++)
                {
                    Quaternion face = Quaternion.Euler(0f, side * 90f, 0f);
                    if (variant == "wood") BuildWoodFace(builder, face, wood, dark, gold);
                    else if (variant == "core") BuildCoreFace(builder, face, dark, gold, blue, energy);
                    else if (variant == "crystal") BuildCrystalFace(builder, face, gold, blue, energy);
                    else BuildStoneFace(builder, face, gold, blue);
                }

                if (variant == "wood")
                {
                    for (int i = -1; i <= 1; i++)
                        builder.Box(dark, new Vector3(i * 0.26f, 0.492f, 0f), new Vector3(0.009f, 0.008f, 0.86f));
                    AddCornerCaps(builder, dark, gold);
                }
                else if (variant == "core")
                {
                    BuildCoreFace(builder, Quaternion.Euler(90f, 0f, 0f), dark, gold, blue, energy);
                    AddCornerCaps(builder, dark, gold, 0.15f);
                }
                else if (variant == "crystal")
                {
                    BuildCrystalFace(builder, Quaternion.Euler(90f, 0f, 0f), gold, blue, energy);
                    AddCornerCaps(builder, gold, blue, 0.15f);
                }
                else
                {
                    builder.FaceBox(gold, Quaternion.Euler(90f, 0f, 0f), new Vector3(0f, 0f, -0.493f),
                        new Vector3(0.23f, 0.23f, 0.014f), 45f);
                    builder.FaceBox(blue, Quaternion.Euler(90f, 0f, 0f), new Vector3(0f, 0f, -0.502f),
                        new Vector3(0.16f, 0.16f, 0.012f), 45f);
                    if (variant == "armored") AddCornerCaps(builder, dark, gold);
                }

                builder.Bake(root.transform, variant);
                root.AddComponent<BoxCollider>().size = Vector3.one;
                var rigidbody = root.AddComponent<Rigidbody>();
                rigidbody.mass = variant == "armored" ? 2.5f : 1f;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                root.AddComponent<PhysicsObject>();
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void BuildStoneFace(BlockBuilder b, Quaternion face, Material gold, Material blue)
        {
            b.FaceBox(gold, face, new Vector3(0f, -0.29f, -0.492f), new Vector3(0.96f, 0.15f, 0.02f));
            b.FaceBox(blue, face, new Vector3(0f, -0.29f, -0.505f), new Vector3(0.92f, 0.10f, 0.012f));
            for (int i = -1; i <= 1; i++)
            {
                b.FaceBox(gold, face, new Vector3(i * 0.31f, -0.29f, -0.514f),
                    new Vector3(0.135f, 0.135f, 0.014f), 45f);
                b.FaceBox(blue, face, new Vector3(i * 0.31f, -0.29f, -0.522f),
                    new Vector3(0.082f, 0.082f, 0.012f), 45f);
            }
        }

        private static void BuildWoodFace(BlockBuilder b, Quaternion face, Material wood, Material dark, Material gold)
        {
            // Thin dark joints keep the planks legible without additional textures.
            for (int i = -1; i <= 1; i++)
                b.FaceBox(dark, face, new Vector3(i * 0.24f, 0f, -0.492f), new Vector3(0.011f, 0.88f, 0.01f));
            b.FaceBox(wood, face, new Vector3(0f, 0.375f, -0.505f), new Vector3(0.87f, 0.11f, 0.028f));
            b.FaceBox(wood, face, new Vector3(0f, -0.375f, -0.505f), new Vector3(0.87f, 0.11f, 0.028f));
            b.FaceBox(dark, face, new Vector3(0f, 0f, -0.497f), new Vector3(0.13f, 1.04f, 0.015f), 45f);
            b.FaceBox(dark, face, new Vector3(0f, 0f, -0.497f), new Vector3(0.13f, 1.04f, 0.015f), -45f);
            b.FaceBox(wood, face, new Vector3(0f, 0f, -0.508f), new Vector3(0.10f, 1.01f, 0.021f), 45f);
            b.FaceBox(wood, face, new Vector3(0f, 0f, -0.518f), new Vector3(0.10f, 1.01f, 0.021f), -45f);
            b.Add(GetSphere(), gold, face * new Vector3(0f, 0f, -0.538f), face,
                new Vector3(0.055f, 0.055f, 0.025f));
        }

        private static void BuildCoreFace(BlockBuilder b, Quaternion face, Material dark,
            Material gold, Material blue, Material energy)
        {
            // The socket surrounds a recessed lens; the outer brackets sit over the stone face.
            b.Add(GetSphere(), dark, face * new Vector3(0f, 0f, -0.475f), face,
                new Vector3(0.79f, 0.79f, 0.10f));
            b.Add(GetRing(), blue, face * new Vector3(0f, 0f, -0.502f), face, Vector3.one * 0.88f);
            b.Add(GetSphere(), energy, face * new Vector3(0f, 0f, -0.514f), face,
                new Vector3(0.53f, 0.53f, 0.028f));
            b.Add(GetRing(), gold, face * new Vector3(0f, 0f, -0.520f), face, Vector3.one * 0.72f);
            for (int i = 0; i < 4; i++)
            {
                Quaternion around = Quaternion.Euler(0f, 0f, i * 90f);
                b.Add(GetBevel(), gold, face * (around * new Vector3(0f, 0.367f, -0.52f)), face * around,
                    new Vector3(0.115f, 0.255f, 0.048f));
            }
            b.FaceBox(energy, face, new Vector3(0f, 0f, -0.533f), new Vector3(0.11f, 0.11f, 0.012f), 45f);
        }

        private static void BuildCrystalFace(BlockBuilder b, Quaternion face, Material gold, Material blue, Material energy)
        {
            b.FaceBox(energy, face, new Vector3(0f, 0f, -0.492f), new Vector3(0.81f, 0.81f, 0.022f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.FaceBox(gold, face, new Vector3(side * 0.433f, 0f, -0.497f), new Vector3(0.042f, 0.89f, 0.025f));
                b.FaceBox(gold, face, new Vector3(0f, side * 0.433f, -0.497f), new Vector3(0.89f, 0.042f, 0.025f));
            }
            b.FaceBox(blue, face, new Vector3(0f, 0f, -0.509f), new Vector3(0.255f, 0.36f, 0.012f), 45f);
            b.FaceBox(gold, face, new Vector3(0f, 0f, -0.518f), new Vector3(0.13f, 0.20f, 0.012f), 45f);
            b.FaceBox(energy, face, new Vector3(0f, 0f, -0.526f), new Vector3(0.075f, 0.13f, 0.012f), 45f);
        }

        private static void AddCornerCaps(BlockBuilder b, Material cap, Material bolt, float size = 0.21f)
        {
            float center = 0.5f - size * 0.5f;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        b.Box(cap, new Vector3(x, y, z) * center, Vector3.one * (size + 0.016f));
                        b.Add(GetSphere(), bolt, new Vector3(x * center, y * center, z * 0.513f),
                            Quaternion.identity, new Vector3(0.065f, 0.065f, 0.029f));
                        b.Add(GetSphere(), bolt, new Vector3(x * 0.513f, y * center, z * center),
                            Quaternion.identity, new Vector3(0.029f, 0.065f, 0.065f));
                    }
        }

        private static Mesh GetSphere()
        {
            if (_sphereMesh != null) return _sphereMesh;
            var temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphereMesh = temporary.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(temporary);
            return _sphereMesh;
        }

        private static Mesh GetBevel()
        {
            if (_bevelMesh != null) return _bevelMesh;
            string path = MESH_FOLDER + "/CitadelBeveledCube.asset";
            _bevelMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (_bevelMesh != null) return _bevelMesh;
            const float inner = 0.44f;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            for (int axis = 0; axis < 3; axis++)
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    var face = new Vector3[4];
                    for (int i = 0; i < 4; i++)
                    {
                        face[i][axis] = sign * 0.5f;
                        face[i][(axis + 1) % 3] = (i == 0 || i == 3 ? -1f : 1f) * inner;
                        face[i][(axis + 2) % 3] = (i < 2 ? -1f : 1f) * inner;
                    }
                    AddPolygon(face, vertices, triangles, uvs);
                }
            for (int axis = 0; axis < 3; axis++)
                for (int a = -1; a <= 1; a += 2)
                    for (int c = -1; c <= 1; c += 2)
                    {
                        var edge = new Vector3[4];
                        for (int i = 0; i < 4; i++)
                        {
                            edge[i][axis] = (i == 0 || i == 3 ? -1f : 1f) * inner;
                            edge[i][(axis + 1) % 3] = a * (i < 2 ? 0.5f : inner);
                            edge[i][(axis + 2) % 3] = c * (i < 2 ? inner : 0.5f);
                        }
                        AddPolygon(edge, vertices, triangles, uvs);
                    }
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        AddPolygon(new[] { new Vector3(x * 0.5f, y * inner, z * inner),
                            new Vector3(x * inner, y * 0.5f, z * inner),
                            new Vector3(x * inner, y * inner, z * 0.5f) }, vertices, triangles, uvs);
            _bevelMesh = new Mesh { name = "CitadelBeveledCube" };
            _bevelMesh.SetVertices(vertices);
            _bevelMesh.SetTriangles(triangles, 0);
            _bevelMesh.SetUVs(0, uvs);
            _bevelMesh.RecalculateNormals();
            _bevelMesh.RecalculateBounds();
            AssetDatabase.CreateAsset(_bevelMesh, path);
            return _bevelMesh;
        }

        private static void AddPolygon(Vector3[] points, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 point in points) center += point;
            Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
            if (Vector3.Dot(normal, center) < 0f) Array.Reverse(points);
            int start = vertices.Count;
            foreach (Vector3 point in points)
            {
                vertices.Add(point);
                Vector3 n = normal.normalized;
                if (Mathf.Abs(n.x) > 0.6f) uvs.Add(new Vector2(point.z + 0.5f, point.y + 0.5f));
                else if (Mathf.Abs(n.y) > 0.6f) uvs.Add(new Vector2(point.x + 0.5f, point.z + 0.5f));
                else uvs.Add(new Vector2(point.x + 0.5f, point.y + 0.5f));
            }
            for (int i = 1; i < points.Length - 1; i++)
            {
                triangles.Add(start);
                triangles.Add(start + i);
                triangles.Add(start + i + 1);
            }
        }

        private static Mesh GetRing()
        {
            if (_ringMesh != null) return _ringMesh;
            string path = MESH_FOLDER + "/CitadelSocketRing.asset";
            _ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (_ringMesh != null) return _ringMesh;
            const int segments = 24;
            const int tubeSides = 6;
            var vertices = new Vector3[segments * tubeSides];
            var triangles = new int[segments * tubeSides * 6];
            var uvs = new Vector2[vertices.Length];
            int index = 0;
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < tubeSides; j++)
                {
                    float a = i * Mathf.PI * 2f / segments;
                    float t = j * Mathf.PI * 2f / tubeSides;
                    float radius = 0.37f + Mathf.Cos(t) * 0.065f;
                    int v = i * tubeSides + j;
                    vertices[v] = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, Mathf.Sin(t) * 0.025f);
                    uvs[v] = new Vector2((float)i / segments, (float)j / tubeSides);
                    int next = ((i + 1) % segments) * tubeSides + j;
                    int up = i * tubeSides + (j + 1) % tubeSides;
                    int diagonal = ((i + 1) % segments) * tubeSides + (j + 1) % tubeSides;
                    triangles[index++] = v; triangles[index++] = next; triangles[index++] = up;
                    triangles[index++] = up; triangles[index++] = next; triangles[index++] = diagonal;
                }
            _ringMesh = new Mesh { name = "CitadelSocketRing", vertices = vertices, triangles = triangles, uv = uvs };
            _ringMesh.RecalculateNormals();
            _ringMesh.RecalculateBounds();
            AssetDatabase.CreateAsset(_ringMesh, path);
            return _ringMesh;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private sealed class BlockBuilder
        {
            private readonly Dictionary<Material, List<CombineInstance>> _parts = new Dictionary<Material, List<CombineInstance>>();

            public void Box(Material material, Vector3 position, Vector3 scale)
            {
                Add(GetBevel(), material, position, Quaternion.identity, scale);
            }

            public void FaceBox(Material material, Quaternion face, Vector3 position, Vector3 scale, float angle = 0f)
            {
                Add(GetBevel(), material, face * position, face * Quaternion.Euler(0f, 0f, angle), scale);
            }

            public void Add(Mesh mesh, Material material, Vector3 position, Quaternion rotation, Vector3 scale)
            {
                if (material == null) throw new ArgumentNullException(nameof(material), "Citadel block materials must be assigned.");
                if (!_parts.TryGetValue(material, out List<CombineInstance> group))
                {
                    group = new List<CombineInstance>();
                    _parts.Add(material, group);
                }
                group.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(position, rotation, scale) });
            }

            public void Bake(Transform root, string variant)
            {
                int index = 0;
                foreach (KeyValuePair<Material, List<CombineInstance>> group in _parts)
                {
                    string meshName = "Citadel_" + variant + "_" + index++;
                    var combined = new Mesh { name = meshName, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                    combined.CombineMeshes(group.Value.ToArray(), true, true);
                    combined.RecalculateBounds();
                    string path = MESH_FOLDER + "/" + meshName + ".asset";
                    Mesh stored = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (stored == null)
                    {
                        stored = combined;
                        AssetDatabase.CreateAsset(stored, path);
                    }
                    else
                    {
                        EditorUtility.CopySerialized(combined, stored);
                        UnityEngine.Object.DestroyImmediate(combined);
                        EditorUtility.SetDirty(stored);
                    }
                    var visual = new GameObject(group.Key.name) { layer = 7 };
                    visual.transform.SetParent(root, false);
                    visual.AddComponent<MeshFilter>().sharedMesh = stored;
                    visual.AddComponent<MeshRenderer>().sharedMaterial = group.Key;
                }
                AssetDatabase.SaveAssets();
            }
        }
    }
}
