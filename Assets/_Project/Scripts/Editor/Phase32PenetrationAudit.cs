using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Read-only placement audit using collider-only replicas; never runs gameplay scripts.</summary>
    public static class Phase32PenetrationAudit
    {
        private const string SCENE = "Assets/_Project/Scenes/MainGame.unity";
        private const string OUTPUT = "_FarzetkiDocs/Art/Phase32/32-11-penetration-audit.json";
        private const float REVIEW_DEPTH = .003f;
        private static readonly string[] FIELDS = { "_boxPrefab", "_stonePrefab", "_columnPrefab", "_conePrefab", "_paintBottlePrefab" };

        [Serializable] public sealed class AuditReport
        {
            public string capturedUtc, finishedUtc, database, sourceScene;
            public string scope = "Authored starting poses only: collider-only copies, no scripts, Rigidbody, simulation, gameplay calls, scene saves or asset changes. Pair vectors move collider A out of B. Contact offsets, settling, stability, visual mesh intersections and collision response are not tested. Mesh results follow ComputePenetration's one-sided triangle behavior; unsupported shape combinations fail explicitly.";
            public float reviewDepthMetres = REVIEW_DEPTH;
            public int expectedLevels = 30, completedLevels, targetAabbCandidates, falsePositiveTargetAabbCandidates;
            public int penetratingTargetPairs, targetReviewFlags, standReviewFlags;
            public float maximumTargetDepth, maximumStandDepth;
            public bool complete, noReviewFlags, mainSceneUnchanged, temporarySceneClosed;
            public string mainSceneHashBefore, mainSceneHashAfter;
            public bool mainSceneDirtyBefore, mainSceneDirtyAfter;
            public List<PrefabCost> prefabs = new List<PrefabCost>();
            public List<LevelReport> levels = new List<LevelReport>();
            public List<string> errors = new List<string>();
        }

        [Serializable] public sealed class PrefabCost
        {
            public string role, path;
            public int colliderCount, enabledSolidColliders, excludedColliders, renderers, renderedMeshInstances;
            public long vertices, triangles, nonTriangleIndices;
            public List<string> colliderTypes = new List<string>();
        }

        [Serializable] public sealed class LevelReport
        {
            public int level, targets, stands, testedPlacementPairs, testedColliderPairs, targetAabbCandidates;
            public int falsePositiveTargetAabbCandidates, penetratingTargetPairs, targetReviewFlags, standReviewFlags;
            public string name, asset, error;
            public bool complete;
            public float maximumTargetDepth, maximumStandDepth;
            public long renderedVertices, renderedTriangles;
            public List<PairReport> pairs = new List<PairReport>();
        }

        [Serializable] public sealed class PairReport
        {
            public string category, aName, bName, aType, bType;
            public bool aabbCandidate, penetrating, reviewFlag;
            public Vector3 aabbOverlap, maximumSeparationVector;
            public float maximumDepth;
            public int colliderPairsTested;
            public List<ColliderPairReport> penetrations = new List<ColliderPairReport>();
        }

        [Serializable] public sealed class ColliderPairReport
        {
            public string aCollider, bCollider, aColliderType, bColliderType;
            public bool reviewFlag, layerCollisionIgnored;
            public float depth;
            public Vector3 direction, separationVector;
        }

        private sealed class PlacedShape
        {
            public string name, type;
            public bool stand;
            public GameObject root;
            public readonly List<Collider> colliders = new List<Collider>();
            public readonly List<string> colliderPaths = new List<string>();
            public Bounds bounds;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/11 - Audit Exact Collider Penetration")]
        public static void RunFromMenu() => Run();

        /// <summary>Run synchronously in MainGame Edit Mode. Writes evidence outside Assets and returns it.</summary>
        public static AuditReport Run()
        {
            var main = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || main.path != SCENE)
                throw new InvalidOperationException("Run the penetration audit in MainGame Edit Mode.");
            var manager = RequireSingle<LevelManager>(main);
            var managerData = new SerializedObject(manager);
            var database = manager.Database;
            var loader = managerData.FindProperty("_levelLoader").objectReferenceValue as LevelLoader;
            if (loader == null) loader = RequireSingle<LevelLoader>(main);
            if (loader.gameObject.scene != main || database == null || database.TotalLevels != 30)
                throw new InvalidOperationException("MainGame must have its configured loader and an exact 30-level database.");
            var settings = new SerializedObject(loader);
            var stand = RequirePrefab(settings, "_standPrefab");
            var targets = new GameObject[FIELDS.Length];
            for (int i = 0; i < targets.Length; i++) targets[i] = RequirePrefab(settings, FIELDS[i]);
            var sourceParent = settings.FindProperty("_levelRoot").objectReferenceValue as Transform;
            if (sourceParent == null) sourceParent = FindNamedTransform(main, "Level_ObjectsRoot");

            var report = new AuditReport { capturedUtc = DateTime.UtcNow.ToString("O"),
                database = AssetDatabase.GetAssetPath(database), sourceScene = main.path,
                mainSceneDirtyBefore = main.isDirty, mainSceneHashBefore = SceneHash(main) };
            report.prefabs.Add(Cost(stand, "Stand"));
            for (int i = 0; i < targets.Length; i++) report.prefabs.Add(Cost(targets[i], ((ObjectType)i).ToString()));
            Scene temporary = default;
            try
            {
                // NewScene is the Edit Mode API. Make it active before creating anything so no temporary
                // GameObject is ever created in MainGame. Only Transform and Collider components are added.
                temporary = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                // NewScene can already select its new scene. Verify the resulting active scene instead of
                // treating SetActiveScene's return value as proof that an already-selected scene failed.
                if (SceneManager.GetActiveScene() != temporary) SceneManager.SetActiveScene(temporary);
                if (!temporary.IsValid() || !temporary.isLoaded || SceneManager.GetActiveScene() != temporary)
                    throw new InvalidOperationException("Cannot activate temporary audit scene (valid=" + temporary.IsValid() +
                        ", loaded=" + temporary.isLoaded + ", temporary=" + temporary.handle +
                        ", active=" + SceneManager.GetActiveScene().handle + ").");
                for (int level = 1; level <= 30; level++)
                {
                    var row = new LevelReport { level = level };
                    report.levels.Add(row);
                    GameObject workspace = null;
                    try
                    {
                        var data = database.GetLevel(level);
                        if (data == null || data.LevelId != level || data.Objects == null || data.Stands == null || data.Stands.Length == 0)
                            throw new InvalidOperationException("Missing/mismatched level data or explicit stand placements. Scene-stand fallback is deliberately not guessed.");
                        row.name = data.LevelName; row.asset = AssetDatabase.GetAssetPath(data);
                        row.targets = data.Objects.Length; row.stands = data.Stands.Length;
                        workspace = new GameObject("Phase32PenetrationAudit_Level" + level);
                        var placementParent = CopyParentChain(sourceParent, workspace.transform);
                        var placed = new List<PlacedShape>();
                        for (int i = 0; i < data.Objects.Length; i++)
                        {
                            var item = data.Objects[i];
                            if (item == null) throw new InvalidOperationException("Null object placement " + i);
                            int type = (int)item.Type;
                            if (type < 0 || type >= targets.Length) type = 0; // Same default-to-Box behavior as LevelLoader.
                            placed.Add(Place(targets[type], placementParent, item.Position, item.Rotation, item.Scale,
                                "Target[" + i + "]_" + targets[type].name, item.Type.ToString(), false));
                            row.renderedVertices += report.prefabs[type + 1].vertices;
                            row.renderedTriangles += report.prefabs[type + 1].triangles;
                        }
                        for (int i = 0; i < data.Stands.Length; i++)
                        {
                            var item = data.Stands[i];
                            if (item == null) throw new InvalidOperationException("Null stand placement " + i);
                            placed.Add(Place(stand, placementParent, item.Position, item.Rotation, item.Scale,
                                "Stand[" + i + "]_" + stand.name, "Stand", true));
                            row.renderedVertices += report.prefabs[0].vertices;
                            row.renderedTriangles += report.prefabs[0].triangles;
                        }
                        // Deliberately test every cross-placement collider pair. No AABB broadphase, SyncTransforms,
                        // FixedUpdate, or simulation is necessary for ComputePenetration with explicit poses.
                        for (int a = 0; a < placed.Count; a++) for (int b = a + 1; b < placed.Count; b++)
                            InspectPair(placed[a], placed[b], row);
                        row.complete = true;
                    }
                    catch (Exception error)
                    {
                        row.error = error.ToString();
                        report.errors.Add("Level " + level + ": " + error.Message);
                    }
                    finally
                    {
                        if (workspace != null) UnityEngine.Object.DestroyImmediate(workspace);
                    }
                    if (row.complete) report.completedLevels++;
                    report.targetAabbCandidates += row.targetAabbCandidates;
                    report.falsePositiveTargetAabbCandidates += row.falsePositiveTargetAabbCandidates;
                    report.penetratingTargetPairs += row.penetratingTargetPairs;
                    report.targetReviewFlags += row.targetReviewFlags; report.standReviewFlags += row.standReviewFlags;
                    report.maximumTargetDepth = Mathf.Max(report.maximumTargetDepth, row.maximumTargetDepth);
                    report.maximumStandDepth = Mathf.Max(report.maximumStandDepth, row.maximumStandDepth);
                }
            }
            catch (Exception error) { report.errors.Add(error.ToString()); }
            finally
            {
                try
                {
                    if (main.IsValid() && main.isLoaded && SceneManager.GetActiveScene() != main)
                        SceneManager.SetActiveScene(main);
                    if (SceneManager.GetActiveScene() != main)
                        report.errors.Add("Could not restore the original active scene.");
                    if (temporary.IsValid() && temporary.isLoaded)
                        report.temporarySceneClosed = EditorSceneManager.CloseScene(temporary, true);
                    else report.temporarySceneClosed = true;
                }
                catch (Exception error) { report.errors.Add("Temporary-scene cleanup: " + error.Message); }
                report.mainSceneDirtyAfter = main.isDirty;
                report.mainSceneHashAfter = SceneHash(main);
                report.mainSceneUnchanged = report.mainSceneHashBefore == report.mainSceneHashAfter &&
                    report.mainSceneDirtyBefore == report.mainSceneDirtyAfter && SceneManager.GetActiveScene() == main;
                if (!report.mainSceneUnchanged) report.errors.Add("MainGame serialized state, dirty state, or active-scene state changed during audit.");
                if (!report.temporarySceneClosed) report.errors.Add("Temporary scene was not closed.");
                report.complete = report.completedLevels == 30 && report.errors.Count == 0;
                report.noReviewFlags = report.complete && report.targetReviewFlags == 0 && report.standReviewFlags == 0;
                report.finishedUtc = DateTime.UtcNow.ToString("O");
                Directory.CreateDirectory(Path.GetDirectoryName(OUTPUT));
                File.WriteAllText(OUTPUT, JsonUtility.ToJson(report, true));
            }
            Debug.Log("[Phase32PenetrationAudit] Completed " + report.completedLevels + "/30 levels; target AABB candidates=" +
                report.targetAabbCandidates + ", false positives=" + report.falsePositiveTargetAabbCandidates +
                ", target review flags=" + report.targetReviewFlags + ", stand review flags=" + report.standReviewFlags +
                ", max target depth=" + report.maximumTargetDepth.ToString("F6") + " m. Evidence: " + OUTPUT);
            return report;
        }

        private static void InspectPair(PlacedShape a, PlacedShape b, LevelReport row)
        {
            Vector3 overlap = Vector3.Min(a.bounds.max, b.bounds.max) - Vector3.Max(a.bounds.min, b.bounds.min);
            var pair = new PairReport { category = a.stand && b.stand ? "stand-stand" : a.stand || b.stand ? "target-stand" : "target-target",
                aName = a.name, bName = b.name, aType = a.type, bType = b.type, aabbOverlap = overlap,
                aabbCandidate = Mathf.Min(overlap.x, Mathf.Min(overlap.y, overlap.z)) > REVIEW_DEPTH };
            bool targetPair = !a.stand && !b.stand;
            row.testedPlacementPairs++;
            if (targetPair && pair.aabbCandidate) row.targetAabbCandidates++;
            for (int i = 0; i < a.colliders.Count; i++) for (int j = 0; j < b.colliders.Count; j++)
            {
                var ca = a.colliders[i]; var cb = b.colliders[j];
                bool aSupported = IsPenetrationShape(ca), bSupported = IsPenetrationShape(cb);
                if (!aSupported && !bSupported)
                    throw new InvalidOperationException("ComputePenetration requires a primitive or convex mesh on at least one side: " + a.name + " / " + b.name);
                Vector3 direction; float depth;
                // Put a supported convex shape first. Preserve A-out-of-B vector semantics when swapped.
                bool hit = aSupported ? Physics.ComputePenetration(ca, ca.transform.position, ca.transform.rotation,
                    cb, cb.transform.position, cb.transform.rotation, out direction, out depth) :
                    Physics.ComputePenetration(cb, cb.transform.position, cb.transform.rotation,
                    ca, ca.transform.position, ca.transform.rotation, out direction, out depth);
                if (!aSupported) direction = -direction;
                pair.colliderPairsTested++; row.testedColliderPairs++;
                if (!hit || depth <= 0f) continue;
                if (float.IsNaN(depth) || float.IsInfinity(depth)) throw new InvalidOperationException("Invalid penetration depth.");
                pair.penetrating = true;
                var evidence = new ColliderPairReport { aCollider = a.colliderPaths[i], bCollider = b.colliderPaths[j],
                    aColliderType = ca.GetType().Name, bColliderType = cb.GetType().Name,
                    depth = depth, direction = direction, separationVector = direction * depth,
                    reviewFlag = depth > REVIEW_DEPTH,
                    layerCollisionIgnored = Physics.GetIgnoreLayerCollision(ca.gameObject.layer, cb.gameObject.layer) };
                pair.penetrations.Add(evidence);
                if (depth > pair.maximumDepth) { pair.maximumDepth = depth; pair.maximumSeparationVector = evidence.separationVector; }
            }
            pair.reviewFlag = pair.maximumDepth > REVIEW_DEPTH;
            if (targetPair)
            {
                if (pair.aabbCandidate && !pair.penetrating) row.falsePositiveTargetAabbCandidates++;
                if (pair.penetrating) row.penetratingTargetPairs++;
                if (pair.reviewFlag) row.targetReviewFlags++;
                row.maximumTargetDepth = Mathf.Max(row.maximumTargetDepth, pair.maximumDepth);
            }
            else
            {
                if (pair.reviewFlag) row.standReviewFlags++;
                row.maximumStandDepth = Mathf.Max(row.maximumStandDepth, pair.maximumDepth);
            }
            if (pair.aabbCandidate || pair.penetrating) row.pairs.Add(pair);
        }

        private static PlacedShape Place(GameObject prefab, Transform parent, Vector3 position, Vector3 euler, Vector3 scale,
            string name, string type, bool stand)
        {
            var shape = new PlacedShape { name = name, type = type, stand = stand };
            shape.root = new GameObject(name);
            shape.root.transform.SetParent(parent, false);
            // Instantiate(prefab, worldPosition, worldRotation, parent), followed by localScale = placement.Scale.
            shape.root.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            shape.root.transform.localScale = scale;
            var map = new Dictionary<Transform, Transform> { { prefab.transform, shape.root.transform } };
            foreach (var source in prefab.GetComponentsInChildren<Collider>(true))
            {
                if (!source.enabled || source.isTrigger || !ActiveInPrefab(source.transform, prefab.transform)) continue;
                var target = CopyCollider(source, CopyColliderTransform(source.transform, map));
                shape.colliders.Add(target);
                shape.colliderPaths.Add(ColliderPath(source, prefab.transform));
                Bounds bounds = WorldBounds(target);
                if (shape.colliders.Count == 1) shape.bounds = bounds; else shape.bounds.Encapsulate(bounds);
            }
            if (shape.colliders.Count == 0) throw new InvalidOperationException("No enabled, active, solid colliders on " + prefab.name);
            return shape;
        }

        private static Transform CopyParentChain(Transform source, Transform workspace)
        {
            if (source == null) return workspace;
            Transform parent = source.parent == null ? workspace : CopyParentChain(source.parent, workspace);
            return CopyTransform(source, parent);
        }

        private static Transform CopyColliderTransform(Transform source, Dictionary<Transform, Transform> map)
        {
            Transform result;
            if (map.TryGetValue(source, out result)) return result;
            result = CopyTransform(source, CopyColliderTransform(source.parent, map));
            map.Add(source, result);
            return result;
        }

        private static Transform CopyTransform(Transform source, Transform parent)
        {
            var result = new GameObject(source.name).transform;
            result.SetParent(parent, false);
            result.SetLocalPositionAndRotation(source.localPosition, source.localRotation);
            result.localScale = source.localScale;
            return result;
        }

        private static Collider CopyCollider(Collider source, Transform transform)
        {
            Collider result;
            if (source is BoxCollider box)
            {
                var copy = transform.gameObject.AddComponent<BoxCollider>(); copy.center = box.center; copy.size = box.size; result = copy;
            }
            else if (source is SphereCollider sphere)
            {
                var copy = transform.gameObject.AddComponent<SphereCollider>(); copy.center = sphere.center; copy.radius = sphere.radius; result = copy;
            }
            else if (source is CapsuleCollider capsule)
            {
                var copy = transform.gameObject.AddComponent<CapsuleCollider>(); copy.center = capsule.center;
                copy.direction = capsule.direction; copy.radius = capsule.radius; copy.height = capsule.height; result = copy;
            }
            else if (source is MeshCollider mesh)
            {
                if (mesh.sharedMesh == null) throw new InvalidOperationException("MeshCollider has no mesh.");
                var copy = transform.gameObject.AddComponent<MeshCollider>();
                copy.cookingOptions = mesh.cookingOptions; copy.convex = mesh.convex; copy.sharedMesh = mesh.sharedMesh; result = copy;
            }
            else throw new NotSupportedException("Collider shape is not supported: " + source.GetType().FullName);
            result.gameObject.layer = source.gameObject.layer;
            result.isTrigger = false; result.enabled = true;
            return result;
        }

        private static bool IsPenetrationShape(Collider collider) => collider is BoxCollider || collider is SphereCollider ||
            collider is CapsuleCollider || collider is MeshCollider mesh && mesh.convex;

        private static Bounds WorldBounds(Collider collider)
        {
            // Primitive bounds mirror PhysX scaling rules and do not depend on broadphase synchronization.
            var transform = collider.transform;
            Vector3 scale = transform.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            if (collider is SphereCollider sphere)
                return new Bounds(transform.TransformPoint(sphere.center), Vector3.one * (2f * sphere.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z))));
            if (collider is CapsuleCollider capsule)
            {
                int axis = capsule.direction, other = (axis + 1) % 3, last = (axis + 2) % 3;
                float radius = capsule.radius * Mathf.Max(scale[other], scale[last]);
                float segment = Mathf.Max(0f, capsule.height * scale[axis] * .5f - radius);
                Vector3 localAxis = Vector3.zero; localAxis[axis] = segment;
                Vector3 worldAxis = transform.rotation * localAxis;
                return new Bounds(transform.TransformPoint(capsule.center),
                    2f * (new Vector3(Mathf.Abs(worldAxis.x), Mathf.Abs(worldAxis.y), Mathf.Abs(worldAxis.z)) + Vector3.one * radius));
            }
            Bounds local;
            Matrix4x4 matrix;
            if (collider is BoxCollider box)
            {
                local = new Bounds(Vector3.zero, Vector3.Scale(box.size, scale));
                matrix = Matrix4x4.TRS(transform.TransformPoint(box.center), transform.rotation, Vector3.one);
            }
            else
            {
                local = ((MeshCollider)collider).sharedMesh.bounds;
                matrix = transform.localToWorldMatrix;
            }
            Bounds bounds = default; bool first = true;
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
            {
                Vector3 point = matrix.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, new Vector3(x, y, z)));
                if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
            }
            return bounds;
        }

        private static PrefabCost Cost(GameObject prefab, string role)
        {
            var cost = new PrefabCost { role = role, path = AssetDatabase.GetAssetPath(prefab) };
            foreach (var collider in prefab.GetComponentsInChildren<Collider>(true))
            {
                cost.colliderCount++;
                if (!cost.colliderTypes.Contains(collider.GetType().Name)) cost.colliderTypes.Add(collider.GetType().Name);
                if (collider.enabled && !collider.isTrigger && ActiveInPrefab(collider.transform, prefab.transform)) cost.enabledSolidColliders++;
                else cost.excludedColliders++;
            }
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.enabled || !ActiveInPrefab(renderer.transform, prefab.transform)) continue;
                cost.renderers++;
                var filter = renderer.GetComponent<MeshFilter>();
                Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : filter != null ? filter.sharedMesh : null;
                if (mesh == null) continue;
                cost.renderedMeshInstances++; cost.vertices += mesh.vertexCount;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    if (mesh.GetTopology(sub) == MeshTopology.Triangles) cost.triangles += mesh.GetIndexCount(sub) / 3;
                    else cost.nonTriangleIndices += mesh.GetIndexCount(sub);
                }
            }
            return cost;
        }

        private static bool ActiveInPrefab(Transform item, Transform root)
        {
            while (item != null) { if (!item.gameObject.activeSelf) return false; if (item == root) return true; item = item.parent; }
            return false;
        }

        private static string ColliderPath(Collider collider, Transform root)
        {
            string path = collider.transform == root ? "." : AnimationUtility.CalculateTransformPath(collider.transform, root);
            return path + "/" + collider.GetType().Name + "[" + Array.IndexOf(collider.GetComponents<Collider>(), collider) + "]";
        }

        private static GameObject RequirePrefab(SerializedObject settings, string field)
        {
            var prefab = settings.FindProperty(field).objectReferenceValue as GameObject;
            if (prefab == null || !EditorUtility.IsPersistent(prefab)) throw new InvalidOperationException("Missing prefab asset binding: " + field);
            return prefab;
        }

        private static T RequireSingle<T>(Scene scene) where T : Component
        {
            T result = null;
            foreach (var root in scene.GetRootGameObjects()) foreach (var candidate in root.GetComponentsInChildren<T>(true))
            { if (result != null) throw new InvalidOperationException("Multiple " + typeof(T).Name + " components in MainGame."); result = candidate; }
            if (result == null) throw new InvalidOperationException("Missing " + typeof(T).Name + " in MainGame.");
            return result;
        }

        private static Transform FindNamedTransform(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects()) foreach (var item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == name && item.gameObject.activeInHierarchy) return item;
            return null;
        }

        private static string SceneHash(Scene scene)
        {
            var text = new StringBuilder();
            foreach (var root in scene.GetRootGameObjects()) foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                text.Append(transform.gameObject.GetEntityId().ToString()).Append(':').Append(EditorJsonUtility.ToJson(transform.gameObject)).Append('\n');
                foreach (var component in transform.GetComponents<Component>())
                    text.Append(component == null ? "missing" : component.GetType().FullName + ":" + EditorJsonUtility.ToJson(component)).Append('\n');
            }
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "").ToLowerInvariant();
        }
    }
}
