using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Repairs five known compressed starting layouts without regenerating levels or changing gameplay data.</summary>
    public static class Phase32PlacementClearanceFix
    {
        private const string MAIN_SCENE = "Assets/_Project/Scenes/MainGame.unity";
        private const string REPORT_FOLDER = "_FarzetkiDocs/Art/Phase32";
        private const float STEP = .755f, BASE_Y = 1.845f, LAYER_Y = .746f, Z = 7.5f, EPSILON = .000001f;
        private static readonly Vector3 TARGET_SCALE = new Vector3(.74f, .74f, .74f);
        private static readonly int[] IDS = { 2, 10, 19, 22, 27 };
        private static readonly string[] PREFAB_FIELDS = { "_boxPrefab", "_stonePrefab", "_columnPrefab", "_conePrefab", "_paintBottlePrefab" };

        public static FixReport LastReport { get; private set; }

        [Serializable] public sealed class FixReport
        {
            public string startedUtc, finishedUtc, database, databaseGuid, backupDirectory, reportPath;
            public string scope = "Targeted authored X-spacing correction for L2/L10/L19/L22/L27 only. No level generation, SetData, scene save, player save, gameplay call, collider or prefab modification. Numeric box checks are not a Unity physics acceptance result.";
            public bool preflightPassed, success, alreadyCorrect, otherFieldsPreserved, unrelatedLevelsPreserved, databasePreserved, mainScenePreserved;
            public bool rollbackAttempted, rollbackVerified;
            public int changedAssets, changedPlacements;
            public List<LevelChange> levels = new List<LevelChange>();
            public List<ModelRow> predictedBoxChecks = new List<ModelRow>();
            public List<string> errors = new List<string>();
        }

        [Serializable] public sealed class LevelChange
        {
            public int level, objects, stands, changedPlacements;
            public string asset, guid, stateBefore, fileHashBefore, fileHashAfter, otherFieldsHashBefore, otherFieldsHashAfter;
            public float maximumAbsoluteDx;
            public List<PlacementChange> placements = new List<PlacementChange>();
        }

        [Serializable] public sealed class PlacementChange
        {
            public int index;
            public string type;
            public Vector3 before, after;
            public float dx;
        }

        [Serializable] public sealed class ModelRow
        {
            public int level, targetPairs, targetStandPairs, unsupportedProjectedFootprints;
            public float maximumDepth;
        }

        private sealed class ExpectedObject
        {
            public ObjectType type;
            public Vector3 before, after;
        }

        private sealed class Specification
        {
            public int id;
            public readonly List<ExpectedObject> objects = new List<ExpectedObject>();
            public StandPlacement[] stands;
            public void Add(ObjectType type, float oldX, int row, float newX)
            { objects.Add(new ExpectedObject { type = type, before = new Vector3(oldX, BASE_Y + row * LAYER_Y, Z), after = new Vector3(newX, BASE_Y + row * LAYER_Y, Z) }); }
            public void Add(ObjectType type, float x, int row) => Add(type, x, row, x);
        }

        private sealed class Snapshot
        {
            public LevelData data;
            public string path, guid, json, fullHash, filteredHash, fileHash, metaHash;
            public byte[] bytes, metaBytes;
            public bool dirty, needsFix, touched;
            public HashSet<string> allowedX;
            public Specification spec;
            public LevelChange report;
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/11 - Repair Known Placement Clearance")]
        public static void ApplyFromMenu() => Apply();

        public static FixReport Apply()
        {
            var report = new FixReport { startedUtc = DateTime.UtcNow.ToString("O") };
            LastReport = report;
            var snapshots = new List<Snapshot>();
            Scene main = SceneManager.GetActiveScene();
            string sceneHash = null, databaseHash = null, databaseFileHash = null, databaseMetaHash = null;
            bool sceneDirty = main.isDirty, databaseDirty = false;
            LevelDatabase database = null;
            try
            {
                Require(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && main.path == MAIN_SCENE,
                    "Run only in MainGame Edit Mode after compilation completes.");
                var manager = RequireSingle<LevelManager>(main);
                database = manager.Database;
                Require(database != null && database.TotalLevels == 30, "MainGame's configured database must contain exactly 30 levels.");
                report.database = AssetDatabase.GetAssetPath(database);
                report.databaseGuid = AssetDatabase.AssetPathToGUID(report.database);
                Require(!string.IsNullOrEmpty(report.databaseGuid), "Configured database is not a persistent asset.");
                databaseDirty = EditorUtility.IsDirty(database);
                databaseHash = Hash(EditorJsonUtility.ToJson(database));
                databaseFileHash = FileHash(report.database);
                databaseMetaHash = FileHash(report.database + ".meta");
                sceneHash = SceneHash(main);

                var managerSerialized = new SerializedObject(manager);
                var loader = managerSerialized.FindProperty("_levelLoader").objectReferenceValue as LevelLoader;
                if (loader == null) loader = RequireSingle<LevelLoader>(main);
                Require(loader.gameObject.scene == main, "Configured level loader belongs to another scene.");
                var loaderSerialized = new SerializedObject(loader);
                var targetBoxes = new BoxCollider[PREFAB_FIELDS.Length];
                for (int i = 0; i < targetBoxes.Length; i++)
                {
                    var prefab = RequirePrefab(loaderSerialized, PREFAB_FIELDS[i]);
                    targetBoxes[i] = RequireSingleSolidBox(prefab);
                    Require(targetBoxes[i].transform == prefab.transform && Near(targetBoxes[i].center, Vector3.zero) && Near(targetBoxes[i].size, Vector3.one),
                        "Expected a unit root BoxCollider for " + prefab.name + "; review actual collider shapes first.");
                }
                var standPrefab = RequirePrefab(loaderSerialized, "_standPrefab");
                var standBox = RequireSingleSolidBox(standPrefab);
                Matrix4x4 standLocal = standPrefab.transform.worldToLocalMatrix * standBox.transform.localToWorldMatrix;
                var levelRoot = loaderSerialized.FindProperty("_levelRoot").objectReferenceValue as Transform;
                if (levelRoot == null)
                    foreach (var root in main.GetRootGameObjects())
                        foreach (var t in root.GetComponentsInChildren<Transform>(true))
                            if (t.name == "Level_ObjectsRoot") { Require(levelRoot == null, "Multiple Level_ObjectsRoot objects."); levelRoot = t; }
                Require(levelRoot == null || Near(levelRoot.lossyScale, Vector3.one), "Non-unit loader parent scale needs a fresh physics review.");

                var paths = new HashSet<string>();
                for (int id = 1; id <= 30; id++)
                {
                    LevelData data = database.GetLevel(id);
                    Require(data != null && data.LevelId == id, "Missing or mismatched level " + id + ".");
                    var s = new Snapshot { data = data, path = AssetDatabase.GetAssetPath(data), dirty = EditorUtility.IsDirty(data) };
                    Require(!string.IsNullOrEmpty(s.path) && s.path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) && paths.Add(s.path), "Expected one persistent, unique asset per database level.");
                    s.guid = AssetDatabase.AssetPathToGUID(s.path);
                    s.json = EditorJsonUtility.ToJson(data);
                    s.fullHash = Hash(s.json);
                    s.fileHash = FileHash(s.path);
                    s.metaHash = FileHash(s.path + ".meta");
                    if (Array.IndexOf(IDS, id) >= 0)
                    {
                        Require(!s.dirty, "Level " + id + " has unsaved authored edits; preserve them and re-review before repair.");
                        s.spec = MakeSpec(id);
                        ValidateSignature(s);
                        s.allowedX = AllowedX(s.spec);
                        s.filteredHash = OtherFieldsHash(data, s.allowedX);
                        s.bytes = File.ReadAllBytes(s.path);
                        s.metaBytes = File.ReadAllBytes(s.path + ".meta");
                        s.report = new LevelChange { level = id, objects = data.Objects.Length, stands = data.Stands.Length, asset = s.path, guid = s.guid,
                            stateBefore = s.needsFix ? "original" : "already-repaired", fileHashBefore = s.fileHash, otherFieldsHashBefore = s.filteredHash };
                        report.levels.Add(s.report);
                    }
                    snapshots.Add(s);
                }

                // All signatures are accepted before the first asset mutation. This model uses actual bound
                // BoxCollider geometry and proposed X positions, not visual meshes or regenerated LevelData.
                CheckPredictedBoxes(snapshots, standBox, standLocal, report);
                report.preflightPassed = true;
                bool any = snapshots.Exists(s => s.needsFix);
                report.alreadyCorrect = !any;
                if (any)
                {
                    report.backupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", "local-backups",
                        "Phase32PlacementClearance-" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));
                    Directory.CreateDirectory(report.backupDirectory);
                    foreach (var s in snapshots)
                        if (s.spec != null)
                        {
                            string backup = Path.Combine(report.backupDirectory, "L" + s.spec.id + "-" + Path.GetFileName(s.path));
                            File.WriteAllBytes(backup, s.bytes);
                            File.WriteAllBytes(backup + ".meta", s.metaBytes);
                        }
                }

                foreach (var s in snapshots)
                {
                    if (s.spec == null) continue;
                    var serialized = new SerializedObject(s.data);
                    var objects = serialized.FindProperty("_objects");
                    for (int i = 0; i < s.spec.objects.Count; i++)
                    {
                        var e = s.spec.objects[i];
                        if (e.before.x == e.after.x) continue;
                        Vector3 before = s.data.Objects[i].Position;
                        Vector3 after = before;
                        if (s.needsFix) after.x = e.after.x;
                        s.report.placements.Add(new PlacementChange { index = i, type = e.type.ToString(), before = before, after = after, dx = after.x - before.x });
                        if (!s.needsFix) continue;
                        // Write only one serialized float, retaining Y/Z and every other placement field.
                        objects.GetArrayElementAtIndex(i).FindPropertyRelative("Position").FindPropertyRelative("x").floatValue = after.x;
                        s.report.maximumAbsoluteDx = Mathf.Max(s.report.maximumAbsoluteDx, Mathf.Abs(after.x - before.x));
                        s.report.changedPlacements++;
                    }
                    if (s.needsFix)
                    {
                        s.touched = true;
                        Require(serialized.ApplyModifiedPropertiesWithoutUndo(), "Serialized X repair did not apply for level " + s.spec.id + ".");
                        report.changedPlacements += s.report.changedPlacements;
                        report.changedAssets++;
                    }
                    s.report.otherFieldsHashAfter = OtherFieldsHash(s.data, s.allowedX);
                    Require(s.report.otherFieldsHashAfter == s.filteredHash, "A field outside allowed X components changed in level " + s.spec.id + ".");
                    ValidateRepaired(s);
                }
                CheckPreservation(snapshots, main, sceneHash, sceneDirty, database, databaseHash, databaseFileHash, databaseMetaHash, databaseDirty, report);

                foreach (var s in snapshots)
                    if (s.touched) AssetDatabase.SaveAssetIfDirty(s.data);
                foreach (var s in snapshots)
                    if (s.spec != null)
                    {
                        Require(AssetDatabase.AssetPathToGUID(s.path) == s.guid && FileHash(s.path + ".meta") == s.metaHash, "Asset identity changed for level " + s.spec.id + ".");
                        Require(!EditorUtility.IsDirty(s.data), "Repaired level was not fully saved.");
                        Require(OtherFieldsHash(s.data, s.allowedX) == s.filteredHash, "Other fields changed during save.");
                        s.report.fileHashAfter = FileHash(s.path);
                    }
                CheckPreservation(snapshots, main, sceneHash, sceneDirty, database, databaseHash, databaseFileHash, databaseMetaHash, databaseDirty, report);
                report.success = true;
                WriteReport(report);
            }
            catch (Exception error)
            {
                report.success = false;
                report.errors.Add(error.ToString());
                if (snapshots.Exists(s => s.touched))
                {
                    report.rollbackAttempted = true;
                    bool allRestored = true;
                    foreach (var s in snapshots)
                        if (s.touched)
                        {
                            try
                            {
                                EditorJsonUtility.FromJsonOverwrite(s.json, s.data);
                                EditorUtility.ClearDirty(s.data);
                                File.WriteAllBytes(s.path, s.bytes);
                                File.WriteAllBytes(s.path + ".meta", s.metaBytes);
                                AssetDatabase.ImportAsset(s.path, ImportAssetOptions.ForceUpdate);
                                Require(FileHash(s.path) == s.fileHash && FileHash(s.path + ".meta") == s.metaHash && Hash(EditorJsonUtility.ToJson(s.data)) == s.fullHash,
                                    "Rollback fingerprint mismatch for " + s.path + ".");
                            }
                            catch (Exception rollbackError)
                            {
                                allRestored = false;
                                report.errors.Add("ROLLBACK FAILURE for " + s.path + ": " + rollbackError);
                            }
                        }
                    report.rollbackVerified = allRestored;
                }
            }
            if (!report.success) WriteReport(report);
            return report;
        }

        private static void WriteReport(FixReport report)
        {
            report.finishedUtc = DateTime.UtcNow.ToString("O");
            Directory.CreateDirectory(REPORT_FOLDER);
            if (string.IsNullOrEmpty(report.reportPath))
                report.reportPath = REPORT_FOLDER + "/32-11-placement-clearance-fix-" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ") + ".json";
            File.WriteAllText(report.reportPath, JsonUtility.ToJson(report, true));
        }

        private static void ValidateSignature(Snapshot s)
        {
            Require(s.data.Objects != null && s.data.Objects.Length == s.spec.objects.Count && s.data.Stands != null && s.data.Stands.Length == s.spec.stands.Length,
                "Unexpected object/stand count in level " + s.spec.id + ".");
            bool original = true, repaired = true;
            for (int i = 0; i < s.spec.objects.Count; i++)
            {
                var a = s.data.Objects[i]; var e = s.spec.objects[i];
                Require(a != null && a.Type == e.type && Near(a.Rotation, Vector3.zero) && Near(a.Scale, TARGET_SCALE),
                    "Unexpected type/rotation/scale at L" + s.spec.id + " target " + i + ".");
                original &= Near(a.Position, e.before);
                repaired &= Near(a.Position, e.after);
            }
            for (int i = 0; i < s.spec.stands.Length; i++)
            {
                var a = s.data.Stands[i]; var e = s.spec.stands[i];
                Require(a != null && Near(a.Position, e.Position) && Near(a.Scale, e.Scale) && Near(a.Rotation, e.Rotation), "Unexpected stand layout at L" + s.spec.id + ".");
            }
            Require(original || repaired, "Unexpected or partially edited placement positions in L" + s.spec.id + "; no repair applied.");
            s.needsFix = original && !repaired;
        }

        private static void ValidateRepaired(Snapshot s)
        {
            for (int i = 0; i < s.spec.objects.Count; i++)
                Require(Near(s.data.Objects[i].Position, s.spec.objects[i].after), "Repaired position mismatch at L" + s.spec.id + " target " + i + ".");
        }

        private static void CheckPreservation(List<Snapshot> rows, Scene main, string sceneHash, bool sceneDirty, LevelDatabase db,
            string databaseHash, string databaseFileHash, string databaseMetaHash, bool databaseDirty, FixReport report)
        {
            foreach (var s in rows)
            {
                Require(AssetDatabase.AssetPathToGUID(s.path) == s.guid && FileHash(s.path + ".meta") == s.metaHash, "A level GUID/meta changed.");
                if (s.spec == null)
                    Require(Hash(EditorJsonUtility.ToJson(s.data)) == s.fullHash && FileHash(s.path) == s.fileHash && EditorUtility.IsDirty(s.data) == s.dirty,
                        "An unrelated level changed: " + s.path + ".");
                else Require(OtherFieldsHash(s.data, s.allowedX) == s.filteredHash, "Other target-level fields changed.");
            }
            report.unrelatedLevelsPreserved = true;
            report.otherFieldsPreserved = true;
            Require(Hash(EditorJsonUtility.ToJson(db)) == databaseHash && FileHash(report.database) == databaseFileHash && FileHash(report.database + ".meta") == databaseMetaHash && EditorUtility.IsDirty(db) == databaseDirty,
                "Database content or dirty state changed.");
            report.databasePreserved = true;
            Require(SceneManager.GetActiveScene() == main && main.isDirty == sceneDirty && SceneHash(main) == sceneHash, "MainGame content, active scene or dirty state changed.");
            report.mainScenePreserved = true;
        }

        private static HashSet<string> AllowedX(Specification spec)
        {
            var fields = new HashSet<string>();
            for (int i = 0; i < spec.objects.Count; i++)
                if (spec.objects[i].before.x != spec.objects[i].after.x) fields.Add("_objects.Array.data[" + i + "].Position.x");
            return fields;
        }

        private static string OtherFieldsHash(LevelData data, HashSet<string> excluded)
        {
            var serialized = new SerializedObject(data);
            var p = serialized.GetIterator();
            var text = new StringBuilder();
            while (p.Next(true))
            {
                // Composite property hashes include descendants; fingerprint leaves so only the explicitly
                // allowed X float is excluded. Array sizes, script reference and future serialized fields remain.
                if (p.hasChildren || excluded.Contains(p.propertyPath)) continue;
                text.Append(p.propertyPath).Append(':').Append(p.propertyType).Append(':').Append(p.contentHash).Append('\n');
            }
            return Hash(text.ToString());
        }

        private static void CheckPredictedBoxes(List<Snapshot> snapshots, BoxCollider standBox, Matrix4x4 standLocal, FixReport report)
        {
            foreach (var s in snapshots)
            {
                var row = new ModelRow { level = s.data.LevelId };
                Require(s.data.Objects != null && s.data.Stands != null && s.data.Stands.Length > 0, "Numeric check needs explicit objects and stands.");
                var targets = new List<Bounds>(); var stands = new List<Bounds>();
                for (int i = 0; i < s.data.Objects.Length; i++)
                {
                    var o = s.data.Objects[i];
                    Require(o != null && Near(o.Rotation, Vector3.zero), "Numeric axis-aligned model cannot classify a rotated/null target; use the actual collider audit.");
                    Vector3 pos = o.Position;
                    if (s.spec != null) pos.x = s.spec.objects[i].after.x;
                    targets.Add(new Bounds(pos, Abs(o.Scale)));
                }
                foreach (var stand in s.data.Stands)
                {
                    Require(stand != null && Near(stand.Rotation, Vector3.zero), "Numeric axis-aligned model cannot classify a rotated/null stand.");
                    var matrix = Matrix4x4.TRS(stand.Position, Quaternion.identity, stand.Scale) * standLocal;
                    Require(IsAxisAligned(matrix), "Stand collider is rotated/sheared; use the actual collider audit.");
                    stands.Add(BoxBounds(matrix, standBox.center, standBox.size));
                }
                for (int a = 0; a < targets.Count; a++)
                {
                    for (int b = a + 1; b < targets.Count; b++)
                    {
                        float depth = PenetrationDepth(targets[a], targets[b]);
                        if (depth > EPSILON) { row.targetPairs++; row.maximumDepth = Mathf.Max(row.maximumDepth, depth); }
                    }
                    bool supported = false;
                    foreach (var stand in stands)
                    {
                        float depth = PenetrationDepth(targets[a], stand);
                        if (depth > EPSILON) { row.targetStandPairs++; row.maximumDepth = Mathf.Max(row.maximumDepth, depth); }
                        var lo = targets[a].min; var hi = targets[a].max;
                        if (lo.x >= stand.min.x - EPSILON && hi.x <= stand.max.x + EPSILON && lo.z >= stand.min.z - EPSILON && hi.z <= stand.max.z + EPSILON) supported = true;
                    }
                    if (!supported) row.unsupportedProjectedFootprints++;
                }
                report.predictedBoxChecks.Add(row);
                Require(row.targetPairs == 0 && row.targetStandPairs == 0 && row.unsupportedProjectedFootprints == 0,
                    "Proposed numeric starting bounds still have a separation/stand-fit issue in L" + row.level + "; no assets changed.");
            }
        }

        private static bool IsAxisAligned(Matrix4x4 matrix)
        {
            for (int c = 0; c < 3; c++)
            {
                int nonzero = 0;
                for (int r = 0; r < 3; r++) if (Mathf.Abs(matrix[r, c]) > EPSILON) nonzero++;
                if (nonzero != 1) return false;
            }
            return true;
        }

        private static Bounds BoxBounds(Matrix4x4 matrix, Vector3 center, Vector3 size)
        {
            var bounds = new Bounds(matrix.MultiplyPoint3x4(center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                bounds.Encapsulate(matrix.MultiplyPoint3x4(center + Vector3.Scale(size * .5f, new Vector3(x, y, z))));
            return bounds;
        }

        private static float PenetrationDepth(Bounds a, Bounds b)
        {
            return Mathf.Min(Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x),
                Mathf.Min(Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y), Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z)));
        }

        private static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        private static bool Near(Vector3 a, Vector3 b) => Mathf.Abs(a.x - b.x) <= EPSILON && Mathf.Abs(a.y - b.y) <= EPSILON && Mathf.Abs(a.z - b.z) <= EPSILON;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

        private static T RequireSingle<T>(Scene scene) where T : Component
        {
            T found = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<T>(true))
                { Require(found == null, "Expected exactly one " + typeof(T).Name + " in MainGame."); found = candidate; }
            Require(found != null, "Missing " + typeof(T).Name + " in MainGame.");
            return found;
        }

        private static GameObject RequirePrefab(SerializedObject loader, string field)
        {
            var value = loader.FindProperty(field).objectReferenceValue as GameObject;
            Require(value != null && PrefabUtility.IsPartOfPrefabAsset(value), "Missing persistent loader prefab: " + field + ".");
            return value;
        }

        private static BoxCollider RequireSingleSolidBox(GameObject prefab)
        {
            BoxCollider found = null;
            foreach (var c in prefab.GetComponentsInChildren<Collider>(true))
            {
                if (!c.enabled || c.isTrigger || !ActiveToRoot(c.transform, prefab.transform)) continue;
                var box = c as BoxCollider;
                Require(box != null && found == null, "Expected exactly one active solid BoxCollider on " + prefab.name + ".");
                found = box;
            }
            Require(found != null, "No solid BoxCollider on " + prefab.name + ".");
            return found;
        }

        private static bool ActiveToRoot(Transform t, Transform root)
        {
            for (; t != null; t = t.parent) { if (!t.gameObject.activeSelf) return false; if (t == root) return true; }
            return false;
        }

        private static string SceneHash(Scene scene)
        {
            var text = new StringBuilder();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    text.Append(t.gameObject.GetEntityId().ToString()).Append(':').Append(EditorJsonUtility.ToJson(t.gameObject)).Append('\n');
                    foreach (var c in t.GetComponents<Component>())
                        text.Append(c == null ? "missing" : c.GetType().FullName + ":" + EditorJsonUtility.ToJson(c)).Append('\n');
                }
            return Hash(text.ToString());
        }

        private static string FileHash(string path) => Hash(File.ReadAllBytes(path));
        private static string Hash(string text) => Hash(Encoding.UTF8.GetBytes(text));
        private static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }

        private static StandPlacement[] SingleStand(float scaleX) => new[] { new StandPlacement { Position = new Vector3(0, 0, Z), Rotation = Vector3.zero, Scale = new Vector3(scaleX, 1, 1) } };
        private static StandPlacement[] DualStand(float center) => new[] {
            new StandPlacement { Position = new Vector3(-center, 0, Z), Rotation = Vector3.zero, Scale = new Vector3(.75f, 1, 1) },
            new StandPlacement { Position = new Vector3(center, 0, Z), Rotation = Vector3.zero, Scale = new Vector3(.75f, 1, 1) } };

        private static Specification MakeSpec(int id)
        {
            var s = new Specification { id = id };
            switch (id)
            {
                case 2:
                    s.stands = SingleStand(1.3f);
                    s.Add(ObjectType.Stone, -1.5f * STEP, 0, -2 * STEP); s.Add(ObjectType.Box, -.5f * STEP, 0, -STEP);
                    s.Add(ObjectType.Stone, -1.5f * STEP, 1, -2 * STEP); s.Add(ObjectType.Column, -.5f * STEP, 1, -STEP);
                    s.Add(ObjectType.Column, -1.5f * STEP, 2, -2 * STEP); s.Add(ObjectType.PaintBottle, -.5f * STEP, 2, -STEP);
                    s.Add(ObjectType.Box, .5f * STEP, 0, STEP); s.Add(ObjectType.Stone, 1.5f * STEP, 0, 2 * STEP);
                    s.Add(ObjectType.Column, .5f * STEP, 1, STEP); s.Add(ObjectType.Stone, 1.5f * STEP, 1, 2 * STEP);
                    s.Add(ObjectType.PaintBottle, .5f * STEP, 2, STEP); s.Add(ObjectType.Column, 1.5f * STEP, 2, 2 * STEP);
                    s.Add(ObjectType.Box, 0, 0); s.Add(ObjectType.Box, 0, 1); s.Add(ObjectType.Box, -.5f * STEP, 3); s.Add(ObjectType.Box, .5f * STEP, 3);
                    break;
                case 10:
                    s.stands = DualStand(2.2f);
                    for (int l = 0; l < 4; l++) { s.Add(ObjectType.Stone, -2.2f - .5f * STEP, l); s.Add(ObjectType.Box, -2.2f + .5f * STEP, l); }
                    s.Add(ObjectType.Cone, -2.2f - .5f * STEP, 4); s.Add(ObjectType.PaintBottle, -2.2f, 4, -2.2f + .5f * STEP);
                    for (int l = 0; l < 4; l++) { s.Add(ObjectType.Box, 2.2f - .5f * STEP, l); s.Add(ObjectType.Stone, 2.2f + .5f * STEP, l); }
                    s.Add(ObjectType.Cone, 2.2f + .5f * STEP, 4); s.Add(ObjectType.PaintBottle, 2.2f, 4, 2.2f - .5f * STEP);
                    s.Add(ObjectType.Column, -STEP, 3); s.Add(ObjectType.Column, STEP, 3);
                    break;
                case 19:
                    s.stands = SingleStand(1.35f);
                    for (int c = 0; c < 6; c++) s.Add(ObjectType.Stone, (-2.5f + c) * STEP, 0);
                    for (int c = 0; c < 6; c++) s.Add(c == 1 || c == 4 ? ObjectType.PaintBottle : ObjectType.Box, (-2.5f + c) * STEP, 1);
                    for (int c = 0; c < 5; c++) s.Add(c == 2 ? ObjectType.PaintBottle : ObjectType.Column, (-2f + c) * STEP, 2);
                    for (int c = 0; c < 4; c++) s.Add(ObjectType.Cone, (-1.5f + c) * STEP, 3);
                    s.Add(ObjectType.Column, -.75f * STEP, 4, -STEP); s.Add(ObjectType.PaintBottle, 0, 4); s.Add(ObjectType.Column, .75f * STEP, 4, STEP); s.Add(ObjectType.Column, 0, 5);
                    break;
                case 22:
                    s.stands = DualStand(2.3f);
                    foreach (float center in new[] { -2.3f, 2.3f })
                    {
                        for (int c = 0; c < 4; c++) s.Add(ObjectType.Stone, center + (-1.5f + c) * .7f * STEP, 0, center + (-1.5f + c) * STEP);
                        for (int c = 0; c < 4; c++) s.Add(ObjectType.Box, center + (-1.5f + c) * .7f * STEP, 1, center + (-1.5f + c) * STEP);
                        s.Add(ObjectType.Cone, center - .7f * STEP, 2, center - STEP); s.Add(ObjectType.PaintBottle, center, 2); s.Add(ObjectType.Cone, center + .7f * STEP, 2, center + STEP);
                        s.Add(ObjectType.Column, center - .35f * STEP, 3, center - .5f * STEP); s.Add(ObjectType.Column, center + .35f * STEP, 3, center + .5f * STEP);
                    }
                    break;
                case 27:
                    s.stands = DualStand(2.35f);
                    for (int l = 0; l < 4; l++) { s.Add(ObjectType.Stone, -2.35f - .7f * STEP, l, -2.35f - STEP); s.Add(ObjectType.Box, -2.35f, l); s.Add(ObjectType.Column, -2.35f + .7f * STEP, l, -2.35f + STEP); }
                    s.Add(ObjectType.Cone, -2.35f - .35f * STEP, 4, -2.35f - .5f * STEP); s.Add(ObjectType.PaintBottle, -2.35f + .35f * STEP, 4, -2.35f + .5f * STEP);
                    for (int l = 0; l < 4; l++) { s.Add(ObjectType.Column, 2.35f - .7f * STEP, l, 2.35f - STEP); s.Add(ObjectType.Box, 2.35f, l); s.Add(ObjectType.Stone, 2.35f + .7f * STEP, l, 2.35f + STEP); }
                    s.Add(ObjectType.PaintBottle, 2.35f - .35f * STEP, 4, 2.35f - .5f * STEP); s.Add(ObjectType.Cone, 2.35f + .35f * STEP, 4, 2.35f + .5f * STEP);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
            return s;
        }
    }
}
