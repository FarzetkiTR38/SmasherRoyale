using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Editor
{
    /// <summary>Editor-only rounded boxes whose fillet radius uses actual dimensions.</summary>
    internal static class Phase32DimensionalBevel
    {
        public static Mesh Create(Vector3 size)
        {
            for (int axis = 0; axis < 3; axis++)
                if (float.IsNaN(size[axis]) || float.IsInfinity(size[axis]) || size[axis] <= 0)
                    throw new ArgumentOutOfRangeException(nameof(size), "Box dimensions must be positive and finite.");
            float minimum = Mathf.Min(size.x, Mathf.Min(size.y, size.z));

            float radius = minimum * .22f;
            Vector3 inner = size * .5f - Vector3.one * radius;
            // Large cheeks get a third arc segment; small rails and fasteners stay cheap.
            int segments = minimum >= .075f ? 3 : 2;
            var data = new Surface();
            for (int axis = 0; axis < 3; axis++)
            {
                int u = (axis + 1) % 3, v = (axis + 2) % 3;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 normal = Axis(axis, sign);
                    Vector3 center = Axis(axis, sign * size[axis] * .5f);
                    Vector3 du = Axis(u, inner[u]), dv = Axis(v, inner[v]);
                    int first = data.Add(center - du - dv, normal, new Vector2(0, 0));
                    data.Add(center + du - dv, normal, new Vector2(1, 0));
                    data.Add(center + du + dv, normal, new Vector2(1, 1));
                    data.Add(center - du + dv, normal, new Vector2(0, 1));
                    data.Quad(first, first + 1, first + 2, first + 3);
                }

                // Four cylindrical quarter-fillets parallel to each principal axis.
                for (int su = -1; su <= 1; su += 2)
                    for (int sv = -1; sv <= 1; sv += 2)
                    {
                        Vector3 center = Axis(u, su * inner[u]) + Axis(v, sv * inner[v]);
                        int first = data.Count;
                        for (int step = 0; step <= segments; step++)
                        {
                            float t = (float)step / segments;
                            // Normalized barycentric sampling also matches the corner seams.
                            Vector3 normal = (Axis(u, su * (1 - t)) + Axis(v, sv * t)).normalized;
                            Vector3 radial = center + normal * radius;
                            data.Add(radial - Axis(axis, inner[axis]), normal, new Vector2(0, t));
                            data.Add(radial + Axis(axis, inner[axis]), normal, new Vector2(1, t));
                        }
                        for (int step = 0; step < segments; step++)
                        {
                            int a = first + step * 2;
                            data.Quad(a, a + 1, a + 3, a + 2);
                        }
                    }
            }

            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 center = new Vector3(sx * inner.x, sy * inner.y, sz * inner.z);
                        var indices = new int[segments + 1, segments + 1];
                        for (int i = 0; i <= segments; i++)
                            for (int j = 0; j <= segments - i; j++)
                            {
                                Vector3 normal = new Vector3(sx * i, sy * j, sz * (segments - i - j)).normalized;
                                indices[i, j] = data.Add(center + normal * radius, normal,
                                    new Vector2((float)i / segments, (float)j / segments));
                            }
                        for (int i = 0; i < segments; i++)
                            for (int j = 0; j < segments - i; j++)
                            {
                                data.Triangle(indices[i, j], indices[i + 1, j], indices[i, j + 1]);
                                if (j < segments - i - 1)
                                    data.Triangle(indices[i + 1, j], indices[i + 1, j + 1], indices[i, j + 1]);
                            }
                    }
            return data.Build();
        }

        private static Vector3 Axis(int axis, float value)
        {
            Vector3 result = Vector3.zero;
            result[axis] = value;
            return result;
        }

        private sealed class Surface
        {
            private readonly List<Vector3> _positions = new List<Vector3>();
            private readonly List<Vector3> _normals = new List<Vector3>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            private readonly List<int> _triangles = new List<int>();
            public int Count => _positions.Count;

            public int Add(Vector3 position, Vector3 normal, Vector2 uv)
            {
                int index = _positions.Count;
                _positions.Add(position); _normals.Add(normal); _uv.Add(uv);
                return index;
            }

            public void Quad(int a, int b, int c, int d)
            { Triangle(a, b, c); Triangle(a, c, d); }

            public void Triangle(int a, int b, int c)
            {
                Vector3 face = Vector3.Cross(_positions[b] - _positions[a], _positions[c] - _positions[a]);
                Vector3 outward = _normals[a] + _normals[b] + _normals[c];
                _triangles.Add(a);
                _triangles.Add(Vector3.Dot(face, outward) >= 0 ? b : c);
                _triangles.Add(Vector3.Dot(face, outward) >= 0 ? c : b);
            }

            public Mesh Build()
            {
                var mesh = new Mesh { name = "Phase32_DimensionalBevel_Temporary" };
                mesh.SetVertices(_positions); mesh.SetNormals(_normals);
                mesh.SetUVs(0, _uv); mesh.SetTriangles(_triangles, 0);
                // Never recalculate normals: exact face/fillet normals keep broad faces flat
                // and give the rolled edges smooth highlights without smoothing armor faces.
                mesh.RecalculateBounds(); mesh.RecalculateTangents();
                return mesh;
            }
        }
    }
}
