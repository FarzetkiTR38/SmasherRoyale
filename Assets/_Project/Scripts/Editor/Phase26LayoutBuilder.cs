using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Audits and repairs only the Citadel campaign, retaining target identity and campaign metadata.</summary>
    public static class Phase26LayoutBuilder
    {
        private const string LOCAL_ROOT = "Assets/_Project/Art/SkyCitadel/Levels";
        private const string DATABASE_PATH = LOCAL_ROOT + "/CitadelLevelDatabase.asset";
        private const string ORIGINAL_DATABASE_PATH = "Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset";
        private const string STAND_PATH = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_CitadelStand.prefab";
        private const string REPORT_ROOT = "_FarzetkiDocs/Art/Phase26";
        private const float MAX_TOP_Y = 5.95f;
        private const float PENETRATION_TOLERANCE = 0.001f;
        private const float CONTACT_TOLERANCE = 0.015f;
        private const float HORIZONTAL_GAP = 0.018f;

        [Serializable]
        public sealed class LayoutAudit
        {
            public int level;
            public string path;
            public int targetCount;
            public int[] typeCounts = new int[5];
            public List<string> intersections = new List<string>();
            public List<int> penetratesStand = new List<int>();
            public List<int> outsideStand = new List<int>();
            public List<int> unsupported = new List<int>();
            public List<int> aboveFrame = new List<int>();
            public List<int> invalidGeometry = new List<int>();
            public bool valid;
        }

        [Serializable]
        public sealed class AuditReport
        {
            public string timestampUtc;
            public Vector3 standCenter;
            public Vector3 standSize;
            public int originalValid;
            public int citadelValid;
            public List<LayoutAudit> original = new List<LayoutAudit>();
            public List<LayoutAudit> citadel = new List<LayoutAudit>();
        }

        [Serializable]
        public sealed class ApplyReport
        {
            public int levels;
            public int retainedLayouts;
            public int repairedLayouts;
            public int targets;
            public int validLayouts;
            public List<string> changes = new List<string>();
            public string beforeReport;
            public string afterReport;
        }

        private sealed class Revision
        {
            public LevelData source;
            public string path;
            public ObjectPlacement[] objects;
            public StandPlacement[] stands;
            public bool repaired;
        }

        /// <summary>Read-only audit: all original and currently bound Citadel levels, using the current stand collider.</summary>
        public static string AuditAll()
        {
            return JsonUtility.ToJson(CreateAudit(ReadStandBounds()), true);
        }

        /// <summary>For the play-mode verifier; this method only reads the supplied data and prefab.</summary>
        public static LayoutAudit Validate(LevelData level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            return Audit(level.LevelId, AssetDatabase.GetAssetPath(level), level.Objects, level.Stands, ReadStandBounds());
        }

        /// <summary>Idempotent entry point. Never writes an original campaign asset or changes the stand prefab.</summary>
        public static string Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Apply Citadel layout revisions outside Play mode.");
            var database = Required<LevelDatabase>(DATABASE_PATH);
            if (database.TotalLevels != 30) throw new InvalidOperationException("Expected the 30-level Citadel database.");
            Bounds stand = ReadStandBounds();
            var before = CreateAudit(stand);
            var revisions = new List<Revision>();
            var report = new ApplyReport { levels = database.TotalLevels };

            // Prepare and validate the entire revision before mutating any asset.
            for (int id = 1; id <= database.TotalLevels; id++)
            {
                var source = database.GetLevel(id);
                if (source == null || source.Objects == null || source.Objects.Length == 0)
                    throw new InvalidOperationException("Missing targets for Citadel level " + id);
                var existing = Audit(id, AssetDatabase.GetAssetPath(source), source.Objects, source.Stands, stand);
                var revision = new Revision
                {
                    source = source,
                    path = LOCAL_ROOT + "/LevelData_Citadel" + id.ToString("D2") + ".asset",
                    objects = CopyObjects(source.Objects),
                    stands = CopyStands(source.Stands),
                    repaired = !existing.valid
                };
                if (revision.repaired)
                {
                    revision.stands = new[] { new StandPlacement { Position = new Vector3(0f, 0f, 7.5f), Scale = Vector3.one } };
                    ArrangeSupportedColumns(revision.objects, id, stand);
                    report.repairedLayouts++;
                }
                else report.retainedLayouts++;

                var checkedLayout = Audit(id, revision.path, revision.objects, revision.stands, stand);
                if (!checkedLayout.valid)
                    throw new InvalidOperationException("Level " + id + " failed preflight: " + JsonUtility.ToJson(checkedLayout));
                AssertPreservedTargets(source.Objects, revision.objects);
                report.targets += source.Objects.Length;
                report.changes.Add("Level " + id + ": " + source.Objects.Length + " targets; " +
                    (revision.repaired ? "repacked into supported columns" : "retained valid placement") + "; " + revision.path);
                revisions.Add(revision);
            }

            Directory.CreateDirectory(REPORT_ROOT);
            report.beforeReport = REPORT_ROOT + "/layout-audit-before.json";
            report.afterReport = REPORT_ROOT + "/layout-audit-after.json";
            // Keep the original failing audit across repeated calls; latest report is always refreshed.
            if (!File.Exists(report.beforeReport)) File.WriteAllText(report.beforeReport, JsonUtility.ToJson(before, true));
            File.WriteAllText(REPORT_ROOT + "/layout-audit-pre-apply-latest.json", JsonUtility.ToJson(before, true));

            var levels = new LevelData[revisions.Count];
            for (int i = 0; i < revisions.Count; i++)
            {
                var revision = revisions[i];
                var destination = AssetDatabase.LoadAssetAtPath<LevelData>(revision.path);
                if (destination == null)
                {
                    destination = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(destination, revision.path);
                }
                if (destination != revision.source) EditorUtility.CopySerialized(revision.source, destination);
                destination.name = Path.GetFileNameWithoutExtension(revision.path);
                destination.SetData(revision.source.LevelId, revision.source.LevelName, revision.source.BallCount,
                    revision.source.BaseRewardGold, revision.stands, revision.objects, revision.source.SpecialEvents);
                EditorUtility.SetDirty(destination);
                levels[i] = destination;
            }
            database.SetLevels(levels);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            var after = CreateAudit(stand);
            report.validLayouts = after.citadelValid;
            File.WriteAllText(report.afterReport, JsonUtility.ToJson(after, true));
            File.WriteAllText(REPORT_ROOT + "/layout-revision-result.json", JsonUtility.ToJson(report, true));
            if (report.validLayouts != 30) throw new InvalidOperationException("Saved Citadel campaign did not pass its final audit.");
            return JsonUtility.ToJson(report, true);
        }

        private static void ArrangeSupportedColumns(ObjectPlacement[] objects, int level, Bounds stand)
        {
            // All upper cells are vertically aligned with a full cell below. Vary width and crown shape
            // between levels, without relying on precarious bridges or overlapping physics shapes.
            int[] widths = { 4, 3, 4, 3, 4, 5, 4, 5, 3, 5, 6, 4 };
            int columns = Mathf.Min(objects.Length, widths[(level - 1) % widths.Length]);
            int rows = Mathf.CeilToInt((float)objects.Length / columns);
            float width = Mathf.Min(5.45f, stand.size.x - 0.24f);
            float side = Mathf.Min(0.98f, (width - (columns - 1) * HORIZONTAL_GAP) / columns);
            side = Mathf.Min(side, (MAX_TOP_Y - stand.max.y - 0.04f) / rows, stand.size.z - 0.08f);
            if (side < 0.32f) throw new InvalidOperationException("Stand is too small for level " + level + ".");

            var order = new List<int>();
            for (int column = 0; column < columns; column++) order.Add(column);
            int mode = level % 4;
            order.Sort((a, b) =>
            {
                float center = (columns - 1) * 0.5f;
                if (mode == 0) return a.CompareTo(b);
                if (mode == 1) return b.CompareTo(a);
                int compare = Mathf.Abs(a - center).CompareTo(Mathf.Abs(b - center));
                if (mode == 3) compare = -compare;
                return compare != 0 ? compare : a.CompareTo(b);
            });

            float pitch = side + HORIZONTAL_GAP;
            for (int i = 0; i < objects.Length; i++)
            {
                int row = i / columns;
                int column = row == rows - 1 ? order[i % columns] : i % columns;
                objects[i].Position = new Vector3(stand.center.x + (column - (columns - 1) * 0.5f) * pitch,
                    stand.max.y + side * (row + 0.5f), 7.5f + stand.center.z);
                objects[i].Scale = Vector3.one * side;
                objects[i].Rotation = Vector3.zero;
            }
        }

        private static AuditReport CreateAudit(Bounds stand)
        {
            var report = new AuditReport { timestampUtc = DateTime.UtcNow.ToString("o"), standCenter = stand.center, standSize = stand.size };
            foreach (bool original in new[] { true, false })
            {
                var database = Required<LevelDatabase>(original ? ORIGINAL_DATABASE_PATH : DATABASE_PATH);
                for (int id = 1; id <= database.TotalLevels; id++)
                {
                    var data = database.GetLevel(id);
                    var result = Audit(id, AssetDatabase.GetAssetPath(data), data.Objects, data.Stands, stand);
                    if (original) { report.original.Add(result); if (result.valid) report.originalValid++; }
                    else { report.citadel.Add(result); if (result.valid) report.citadelValid++; }
                }
            }
            return report;
        }

        private static LayoutAudit Audit(int id, string path, ObjectPlacement[] objects, StandPlacement[] stands, Bounds stand)
        {
            var result = new LayoutAudit { level = id, path = path, targetCount = objects != null ? objects.Length : 0 };
            if (objects == null || stands == null || stands.Length == 0)
            {
                result.invalidGeometry.Add(-1);
                return result;
            }
            var boxes = new Bounds[objects.Length];
            var platforms = new Bounds[stands.Length];
            for (int i = 0; i < platforms.Length; i++)
                platforms[i] = TransformBounds(stand, Matrix4x4.TRS(stands[i].Position, Quaternion.Euler(stands[i].Rotation), stands[i].Scale));
            for (int i = 0; i < boxes.Length; i++)
            {
                var obj = objects[i];
                if ((int)obj.Type >= 0 && (int)obj.Type < result.typeCounts.Length) result.typeCounts[(int)obj.Type]++;
                bool positive = obj.Scale.x > 0f && obj.Scale.y > 0f && obj.Scale.z > 0f;
                if (!positive || !Finite(obj.Position) || !Finite(obj.Scale) || !Finite(obj.Rotation) || Quaternion.Angle(Quaternion.Euler(obj.Rotation), Quaternion.identity) > 0.01f)
                    result.invalidGeometry.Add(i);
                boxes[i] = TransformBounds(new Bounds(Vector3.zero, Vector3.one), Matrix4x4.TRS(obj.Position, Quaternion.Euler(obj.Rotation), obj.Scale));
            }
            for (int i = 0; i < boxes.Length; i++)
            {
                Bounds box = boxes[i];
                if (box.max.y > MAX_TOP_Y + PENETRATION_TOLERANCE) result.aboveFrame.Add(i);
                bool inside = false;
                bool standHit = false;
                float supportedArea = 0f;
                foreach (var platform in platforms)
                {
                    if (box.min.x >= platform.min.x - PENETRATION_TOLERANCE && box.max.x <= platform.max.x + PENETRATION_TOLERANCE &&
                        box.min.z >= platform.min.z - PENETRATION_TOLERANCE && box.max.z <= platform.max.z + PENETRATION_TOLERANCE) inside = true;
                    if (Penetrates(box, platform)) standHit = true;
                    if (Mathf.Abs(box.min.y - platform.max.y) <= CONTACT_TOLERANCE) supportedArea += FootprintOverlap(box, platform);
                }
                if (!inside) result.outsideStand.Add(i);
                if (standHit) result.penetratesStand.Add(i);
                for (int j = 0; j < boxes.Length; j++)
                {
                    if (j == i) continue;
                    if (j > i && Penetrates(box, boxes[j])) result.intersections.Add(i + ":" + j);
                    if (boxes[j].center.y < box.center.y && Mathf.Abs(box.min.y - boxes[j].max.y) <= CONTACT_TOLERANCE)
                        supportedArea += FootprintOverlap(box, boxes[j]);
                }
                if (supportedArea < box.size.x * box.size.z * 0.90f) result.unsupported.Add(i);
            }
            result.valid = result.targetCount > 0 && result.intersections.Count == 0 && result.penetratesStand.Count == 0 &&
                result.outsideStand.Count == 0 && result.unsupported.Count == 0 && result.aboveFrame.Count == 0 && result.invalidGeometry.Count == 0;
            return result;
        }

        private static Bounds ReadStandBounds()
        {
            var prefab = Required<GameObject>(STAND_PATH);
            BoxCollider tabletop = null;
            foreach (var collider in prefab.GetComponentsInChildren<BoxCollider>(true))
                if (!collider.isTrigger && collider.name == "MarbleTabletop") { tabletop = collider; break; }
            if (tabletop == null) throw new InvalidOperationException("Citadel stand has no MarbleTabletop BoxCollider.");
            Matrix4x4 relative = prefab.transform.worldToLocalMatrix * tabletop.transform.localToWorldMatrix;
            return TransformBounds(new Bounds(tabletop.center, tabletop.size), relative);
        }

        private static Bounds TransformBounds(Bounds bounds, Matrix4x4 transform)
        {
            Vector3 center = transform.MultiplyPoint3x4(bounds.center);
            var result = new Bounds(center, Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        result.Encapsulate(transform.MultiplyPoint3x4(bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z))));
            return result;
        }

        private static bool Penetrates(Bounds a, Bounds b)
        {
            return Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x) > PENETRATION_TOLERANCE &&
                Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y) > PENETRATION_TOLERANCE &&
                Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z) > PENETRATION_TOLERANCE;
        }

        private static float FootprintOverlap(Bounds a, Bounds b)
        {
            return Mathf.Max(0f, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x)) *
                Mathf.Max(0f, Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));
        }

        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) &&
                !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
        }

        private static ObjectPlacement[] CopyObjects(ObjectPlacement[] source)
        {
            var result = new ObjectPlacement[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var item = source[i];
                result[i] = new ObjectPlacement { Type = item.Type, Position = item.Position, Rotation = item.Rotation,
                    Scale = item.Scale, OverrideMass = item.OverrideMass, CustomMass = item.CustomMass, PaintColor = item.PaintColor };
            }
            return result;
        }

        private static StandPlacement[] CopyStands(StandPlacement[] source)
        {
            if (source == null) return Array.Empty<StandPlacement>();
            var result = new StandPlacement[source.Length];
            for (int i = 0; i < source.Length; i++)
                result[i] = new StandPlacement { Position = source[i].Position, Scale = source[i].Scale, Rotation = source[i].Rotation };
            return result;
        }

        private static void AssertPreservedTargets(ObjectPlacement[] before, ObjectPlacement[] after)
        {
            if (before.Length != after.Length) throw new InvalidOperationException("Target count changed.");
            for (int i = 0; i < before.Length; i++)
                if (before[i].Type != after[i].Type || before[i].OverrideMass != after[i].OverrideMass ||
                    before[i].CustomMass != after[i].CustomMass || before[i].PaintColor != after[i].PaintColor)
                    throw new InvalidOperationException("Target identity changed at index " + i);
        }

        private static T Required<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Missing asset: " + path);
            return asset;
        }
    }
}
