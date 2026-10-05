using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Generates high-fidelity 3D fractured Core debris pieces with physical rigidbodies.
    /// Creates 33 individual 3D shards (sapphire crystal, gold brackets, obsidian steel, ivory stone)
    /// that dynamically explode, bounce, tumble, and fall with realistic 3D physics upon ball impact.
    /// </summary>
    public static class CoreFractureGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/VFX/PF_Core_Fractured.prefab";
        private const string MESH_CONTAINER_PATH = "Assets/_Project/Art/SkyCitadel/Meshes/Core_Fractured_Meshes.asset";

        private const string MAT_SAPPHIRE = "Assets/_Project/Art/SkyCitadel/Materials/M_RoyalSapphire.mat";
        private const string MAT_ARCANE = "Assets/_Project/Art/SkyCitadel/Materials/M_ArcaneEnergy.mat";
        private const string MAT_GOLD = "Assets/_Project/Art/SkyCitadel/Materials/M_AntiqueGold.mat";
        private const string MAT_STEEL = "Assets/_Project/Art/SkyCitadel/Materials/M_ObsidianSteel.mat";
        private const string MAT_STONE = "Assets/_Project/Art/SkyCitadel/Materials/M_IvoryStone.mat";

        [MenuItem("Royal Smash/VFX/Generate 3D Fractured Core Prefab", false, 61)]
        public static void GenerateFracturedCorePrefab()
        {
            var matSapphire = AssetDatabase.LoadAssetAtPath<Material>(MAT_SAPPHIRE);
            var matArcane = AssetDatabase.LoadAssetAtPath<Material>(MAT_ARCANE);
            var matGold = AssetDatabase.LoadAssetAtPath<Material>(MAT_GOLD);
            var matSteel = AssetDatabase.LoadAssetAtPath<Material>(MAT_STEEL);
            var matStone = AssetDatabase.LoadAssetAtPath<Material>(MAT_STONE);

            if (matSapphire == null || matGold == null)
            {
                Debug.LogError("[CoreFractureGenerator] Citadel materials missing!");
                return;
            }

            // Ensure mesh container directory exists
            string dir = Path.GetDirectoryName(MESH_CONTAINER_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Re-create mesh asset container
            AssetDatabase.DeleteAsset(MESH_CONTAINER_PATH);
            var meshContainer = ScriptableObject.CreateInstance<MeshContainerSO>();
            AssetDatabase.CreateAsset(meshContainer, MESH_CONTAINER_PATH);

            // Root GameObject
            var root = new GameObject("PF_Core_Fractured");
            var fracturedCore = root.AddComponent<FracturedCoreObject>();

            Random.InitState(1337); // Deterministic seeds for consistent, beautiful shard facets

            float s = 1.0f / 3.0f; // 0.3333m nominal size per 3x3x3 grid cell
            int pieceIndex = 0;

            // 1. Generate 3x3x3 = 27 Primary Structural Shards
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        pieceIndex++;
                        bool isCorner = (x == 0 || x == 2) && (y == 0 || y == 2) && (z == 0 || z == 2);
                        bool isCenter = (x == 1 && y == 1 && z == 1);
                        bool isFaceCenter = !isCenter && !isCorner && ((x == 1 ? 1 : 0) + (y == 1 ? 1 : 0) + (z == 1 ? 1 : 0) == 2);

                        // Material distribution:
                        // Center & Face Centers -> Glowing Royal Sapphire / Arcane Energy Core!
                        // Corners -> Ornate Antique Gold & Obsidian Steel Brackets!
                        // Edges -> Ivory Stone Masonry & Antique Gold Trim!
                        Material pieceMat;
                        if (isCenter)
                        {
                            pieceMat = matArcane != null ? matArcane : matSapphire;
                        }
                        else if (isFaceCenter)
                        {
                            pieceMat = matSapphire;
                        }
                        else if (isCorner)
                        {
                            pieceMat = (pieceIndex % 2 == 0 && matSteel != null) ? matSteel : matGold;
                        }
                        else
                        {
                            pieceMat = (pieceIndex % 3 == 0) ? matGold : (matStone != null ? matStone : matSapphire);
                        }

                        // Jittered bounding coordinates
                        Vector3 minCorner = new Vector3(
                            -0.5f + x * s + (x > 0 ? Random.Range(-0.035f, 0.025f) : 0f),
                            -0.5f + y * s + (y > 0 ? Random.Range(-0.035f, 0.025f) : 0f),
                            -0.5f + z * s + (z > 0 ? Random.Range(-0.035f, 0.025f) : 0f)
                        );
                        Vector3 maxCorner = new Vector3(
                            -0.5f + (x + 1) * s + (x < 2 ? Random.Range(-0.025f, 0.035f) : 0f),
                            -0.5f + (y + 1) * s + (y < 2 ? Random.Range(-0.025f, 0.035f) : 0f),
                            -0.5f + (z + 1) * s + (z < 2 ? Random.Range(-0.025f, 0.035f) : 0f)
                        );

                        Vector3 cellCenter = (minCorner + maxCorner) * 0.5f;
                        Vector3 halfExtents = (maxCorner - minCorner) * 0.5f;

                        // Create Shard Mesh with faceted bevels
                        Mesh shardMesh = CreateFacetedShardMesh($"Mesh_Shard_{pieceIndex:D2}", halfExtents);
                        AssetDatabase.AddObjectToAsset(shardMesh, meshContainer);

                        // Create Piece GameObject
                        var shardGo = new GameObject($"Shard_{pieceIndex:D2}_{pieceMat.name}");
                        shardGo.transform.SetParent(root.transform, false);
                        shardGo.transform.localPosition = cellCenter;

                        var mf = shardGo.AddComponent<MeshFilter>();
                        mf.sharedMesh = shardMesh;

                        var mr = shardGo.AddComponent<MeshRenderer>();
                        mr.sharedMaterial = pieceMat;

                        var col = shardGo.AddComponent<BoxCollider>();
                        col.size = halfExtents * 1.35f; // snug collision box

                        var rb = shardGo.AddComponent<Rigidbody>();
                        rb.mass = isCorner ? 0.08f : (isCenter ? 0.04f : 0.06f);
                        rb.linearDamping = 0.10f;
                        rb.angularDamping = 0.3f;
                        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                        rb.interpolation = RigidbodyInterpolation.Interpolate;

                        shardGo.AddComponent<ShatteredDebrisPiece>();
                    }
                }
            }

            // 2. Generate 6 Inner Crystal Splinters (Sharp Diamond/Prism Fragments)
            Vector3[] splinterOffsets = new Vector3[]
            {
                new Vector3(0.08f, 0.06f, 0.05f),
                new Vector3(-0.07f, 0.08f, -0.06f),
                new Vector3(0.05f, -0.08f, 0.07f),
                new Vector3(-0.06f, -0.06f, -0.05f),
                new Vector3(0.02f, 0.09f, -0.04f),
                new Vector3(-0.03f, -0.04f, 0.08f)
            };

            for (int i = 0; i < splinterOffsets.Length; i++)
            {
                pieceIndex++;
                Vector3 size = new Vector3(Random.Range(0.07f, 0.12f), Random.Range(0.09f, 0.15f), Random.Range(0.07f, 0.12f));
                Mesh crystalMesh = CreateCrystalSplinterMesh($"Mesh_Crystal_{pieceIndex:D2}", size);
                AssetDatabase.AddObjectToAsset(crystalMesh, meshContainer);

                var crystalGo = new GameObject($"CrystalSplinter_{pieceIndex:D2}");
                crystalGo.transform.SetParent(root.transform, false);
                crystalGo.transform.localPosition = splinterOffsets[i];

                var mf = crystalGo.AddComponent<MeshFilter>();
                mf.sharedMesh = crystalMesh;

                var mr = crystalGo.AddComponent<MeshRenderer>();
                mr.sharedMaterial = (i % 2 == 0) ? matSapphire : (matArcane != null ? matArcane : matSapphire);

                var col = crystalGo.AddComponent<BoxCollider>();
                col.size = size * 0.75f;

                var rb = crystalGo.AddComponent<Rigidbody>();
                rb.mass = 0.03f;
                rb.linearDamping = 0.15f;
                rb.angularDamping = 0.4f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                crystalGo.AddComponent<ShatteredDebrisPiece>();
            }

            // Save Prefab
            string prefabDir = Path.GetDirectoryName(PREFAB_PATH);
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Link fractured prefab to Core prefabs & scene instances
            LinkFracturedPrefabToTargets(savedPrefab);

            Debug.Log($"<color=#00FF99><b>[CoreFractureGenerator] Successfully built 33-piece 3D Fractured Core Prefab!</b></color>");
        }

        public static void LinkFracturedPrefabToTargets(GameObject fracturedPrefab)
        {
            if (fracturedPrefab == null) return;

            string[] targetPrefabs = new string[]
            {
                "Assets/_Project/Art/SkyCitadel/Prefabs/PF_core.prefab",
                "Assets/_Project/Prefabs/SkyCitadel/Blocks/PF_Block_05_Core.prefab",
                "Assets/_Project/Prefabs/Gameplay/PaintBottle.prefab"
            };

            foreach (var path in targetPrefabs)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null)
                {
                    var pbo = p.GetComponent<PaintBottleObject>();
                    if (pbo != null)
                    {
                        var so = new SerializedObject(pbo);
                        var prop = so.FindProperty("_fracturedPrefab");
                        if (prop != null)
                        {
                            prop.objectReferenceValue = fracturedPrefab;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(p);
                        }
                    }
                }
            }

            // Update any PaintBottleObject in the active scene
            var sceneBottles = UnityEngine.Object.FindObjectsOfType<PaintBottleObject>();
            foreach (var b in sceneBottles)
            {
                b.FracturedPrefab = fracturedPrefab;
                EditorUtility.SetDirty(b);
            }

            AssetDatabase.SaveAssets();
        }

        private static Mesh CreateFacetedShardMesh(string name, Vector3 halfExtents)
        {
            var mesh = new Mesh { name = name };
            float hx = halfExtents.x;
            float hy = halfExtents.y;
            float hz = halfExtents.z;

            // 8 faceted vertices with jagged internal offsets
            Vector3[] v = new Vector3[]
            {
                new Vector3(-hx + Random.Range(-0.015f, 0.015f), -hy + Random.Range(-0.015f, 0.015f), -hz + Random.Range(-0.015f, 0.015f)), // 0: LBB
                new Vector3( hx + Random.Range(-0.015f, 0.015f), -hy + Random.Range(-0.015f, 0.015f), -hz + Random.Range(-0.015f, 0.015f)), // 1: RBB
                new Vector3( hx + Random.Range(-0.015f, 0.015f),  hy + Random.Range(-0.015f, 0.015f), -hz + Random.Range(-0.015f, 0.015f)), // 2: RTB
                new Vector3(-hx + Random.Range(-0.015f, 0.015f),  hy + Random.Range(-0.015f, 0.015f), -hz + Random.Range(-0.015f, 0.015f)), // 3: LTB
                new Vector3(-hx + Random.Range(-0.015f, 0.015f), -hy + Random.Range(-0.015f, 0.015f),  hz + Random.Range(-0.015f, 0.015f)), // 4: LBF
                new Vector3( hx + Random.Range(-0.015f, 0.015f), -hy + Random.Range(-0.015f, 0.015f),  hz + Random.Range(-0.015f, 0.015f)), // 5: RBF
                new Vector3( hx + Random.Range(-0.015f, 0.015f),  hy + Random.Range(-0.015f, 0.015f),  hz + Random.Range(-0.015f, 0.015f)), // 6: RTF
                new Vector3(-hx + Random.Range(-0.015f, 0.015f),  hy + Random.Range(-0.015f, 0.015f),  hz + Random.Range(-0.015f, 0.015f))  // 7: LTF
            };

            // 6 faces (4 vertices each = 24 vertices for sharp normals)
            Vector3[] vertices = new Vector3[]
            {
                // Front (+Z)
                v[4], v[5], v[6], v[7],
                // Back (-Z)
                v[1], v[0], v[3], v[2],
                // Top (+Y)
                v[7], v[6], v[2], v[3],
                // Bottom (-Y)
                v[0], v[1], v[5], v[4],
                // Right (+X)
                v[5], v[1], v[2], v[6],
                // Left (-X)
                v[0], v[4], v[7], v[3]
            };

            int[] triangles = new int[]
            {
                0, 1, 2,  0, 2, 3,       // Front
                4, 5, 6,  4, 6, 7,       // Back
                8, 9, 10, 8, 10, 11,     // Top
                12, 13, 14, 12, 14, 15,  // Bottom
                16, 17, 18, 16, 18, 19,  // Right
                20, 21, 22, 20, 22, 23   // Left
            };

            Vector2[] uvs = new Vector2[24];
            for (int i = 0; i < 6; i++)
            {
                uvs[i * 4 + 0] = new Vector2(0f, 0f);
                uvs[i * 4 + 1] = new Vector2(1f, 0f);
                uvs[i * 4 + 2] = new Vector2(1f, 1f);
                uvs[i * 4 + 3] = new Vector2(0f, 1f);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateCrystalSplinterMesh(string name, Vector3 size)
        {
            var mesh = new Mesh { name = name };
            float sx = size.x * 0.5f;
            float sy = size.y * 0.5f;
            float sz = size.z * 0.5f;

            // Bipyramidal diamond octahedron: 6 vertices (Top, Bottom, Front, Right, Back, Left)
            Vector3 top = new Vector3(0f, sy, 0f);
            Vector3 bottom = new Vector3(0f, -sy, 0f);
            Vector3 front = new Vector3(0f, 0f, sz);
            Vector3 right = new Vector3(sx, 0f, 0f);
            Vector3 back = new Vector3(0f, 0f, -sz);
            Vector3 left = new Vector3(-sx, 0f, 0f);

            // 8 triangular faces
            Vector3[] vertices = new Vector3[]
            {
                // Top pyramid
                top, front, right,
                top, right, back,
                top, back, left,
                top, left, front,
                // Bottom pyramid
                bottom, right, front,
                bottom, back, right,
                bottom, left, back,
                bottom, front, left
            };

            int[] triangles = new int[24];
            for (int i = 0; i < 24; i++) triangles[i] = i;

            Vector2[] uvs = new Vector2[24];
            for (int i = 0; i < 8; i++)
            {
                uvs[i * 3 + 0] = new Vector2(0.5f, 1f);
                uvs[i * 3 + 1] = new Vector2(0f, 0f);
                uvs[i * 3 + 2] = new Vector2(1f, 0f);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    /// <summary>
    /// ScriptableObject container to hold sub-asset meshes in a clean asset file.
    /// </summary>
    public class MeshContainerSO : ScriptableObject
    {
    }
}
