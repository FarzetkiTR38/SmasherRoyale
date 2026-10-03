using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RoyalSmash.Editor
{
    /// <summary>Refreshes stale display geometry without rebuilding gameplay or the catalog layout.</summary>
    public static class Phase32PrefabCollectionRevision
    {
        private const string ART = "Assets/_Project/Art/SkyCitadel/Prefabs";
        private const string COLLECTION = "Assets/_Project/Prefabs/SkyCitadel";
        private const string MORTAR_SOURCE = "Assets/_Project/Prefabs/Gameplay/Mortar.prefab";
        private const string MORTAR_EXPORT = COLLECTION + "/PF_02_Mortar_Complete.prefab";
        private const string SHOWCASE = COLLECTION + "/PF_00_SkyCitadel_Showcase.prefab";
        private const string TOWER = COLLECTION + "/PF_01_Tower_26Blocks.prefab";
        private const string REPORT = "_FarzetkiDocs/Art/Phase32/32-10-prefab-collection.json";
        private static readonly string[] TYPES = { "wood", "stone", "crystal", "armored", "core" };
        private static readonly string[] NAMES = { "Wood", "Stone", "Crystal", "Armored", "Core" };

        public static RevisionReport LastReport { get; private set; }

        [Serializable] public sealed class RevisionReport
        {
            public string appliedUtc, showcasePath;
            public bool sourceAssetsUnchanged, activeScenePreserved, sceneDirtyStatePreserved;
            public bool sceneRootsPreserved, allExportGuidsPreserved;
            public int towerBlocks, towerStands, blockTypes;
            public string scope = "Refresh two independent art exports. Existing seven prefab variants and nested tower remain inherited and unmodified. No scene or gameplay builder calls.";
            public List<ExportReport> exports = new List<ExportReport>();
        }

        [Serializable] public sealed class ExportReport
        {
            public string path, sourcePath, mode, guidBefore, guidAfter;
            public bool guidPreserved, existingTransformsPreserved, gameplayComponentsPreserved, currentArtMatches;
            public int addedVisualObjects, updatedMeshFilters, updatedRenderers;
            public List<string> componentReferences = new List<string>();
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/10 - Refresh Existing Prefab Collection")]
        public static void Apply()
        {
            Scene main = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || main.path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before refreshing the collection.");
            bool wasDirty = main.isDirty;
            string rootsBefore = SceneRoots(main);
            var report = new RevisionReport { appliedUtc = DateTime.UtcNow.ToString("O"), showcasePath = SHOWCASE };
            var protectedFiles = new Dictionary<string, string>();

            // These are already variants. Re-exporting them would flatten or overwrite
            // authored inheritance; verify their current art and retain them unchanged.
            for (int i = 0; i < TYPES.Length; i++)
                AuditVariant(ART + "/PF_" + TYPES[i] + ".prefab", BlockPath(i), report, protectedFiles);
            AuditVariant(ART + "/PF_CitadelStand.prefab", COLLECTION + "/PF_03_Stand.prefab", report, protectedFiles);
            AuditVariant(ART + "/PF_CitadelBall.prefab", COLLECTION + "/PF_04_EnergyBall.prefab", report, protectedFiles);
            Protect(MORTAR_SOURCE, protectedFiles);
            Protect(TOWER, protectedFiles);
            Load(MORTAR_EXPORT); Load(SHOWCASE); Load(TOWER);
            var towerReport = Entry(TOWER, "nested block and stand exports", "retain nested inheritance");
            towerReport.currentArtMatches = AuditTower(report);
            towerReport.existingTransformsPreserved = true;
            towerReport.gameplayComponentsPreserved = true;
            CollectReferences(Load(TOWER), towerReport);
            report.exports.Add(towerReport);

            // Save only the two independent copies. Byte backups also roll back an earlier
            // successful save if a later export or its preservation check fails.
            var backups = new Dictionary<string, byte[]>();
            foreach (string path in new[] { MORTAR_EXPORT, SHOWCASE })
            { backups.Add(path, File.ReadAllBytes(path)); backups.Add(path + ".meta", File.ReadAllBytes(path + ".meta")); }
            try
            {
                report.exports.Add(RefreshMortar());
                report.exports.Add(RefreshShowcase());
                report.sourceAssetsUnchanged = true;
                foreach (var file in protectedFiles)
                    report.sourceAssetsUnchanged &= Hash(file.Key) == file.Value;
                report.activeScenePreserved = SceneManager.GetActiveScene().handle == main.handle;
                report.sceneDirtyStatePreserved = main.isDirty == wasDirty;
                report.sceneRootsPreserved = SceneRoots(main) == rootsBefore;
                report.allExportGuidsPreserved = true;
                foreach (ExportReport entry in report.exports)
                {
                    entry.guidAfter = AssetDatabase.AssetPathToGUID(entry.path);
                    entry.guidPreserved = entry.guidAfter == entry.guidBefore;
                    report.allExportGuidsPreserved &= entry.guidPreserved;
                }
                if (!report.sourceAssetsUnchanged || !report.activeScenePreserved ||
                    !report.sceneDirtyStatePreserved || !report.sceneRootsPreserved || !report.allExportGuidsPreserved)
                    throw new InvalidOperationException("Collection preservation check failed; restoring the two exports.");
                Directory.CreateDirectory(Path.GetDirectoryName(REPORT));
                File.WriteAllText(REPORT, JsonUtility.ToJson(report, true));
                LastReport = report;
            }
            catch (Exception revisionError)
            {
                try
                {
                    foreach (var file in backups) File.WriteAllBytes(file.Key, file.Value);
                    AssetDatabase.ImportAsset(MORTAR_EXPORT, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    AssetDatabase.ImportAsset(SHOWCASE, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                }
                catch (Exception rollbackError)
                { throw new AggregateException("Collection refresh failed and export restoration was incomplete.", revisionError, rollbackError); }
                throw;
            }
            Debug.Log("[Phase32PrefabCollectionRevision] " + JsonUtility.ToJson(report));
        }

        private static ExportReport RefreshMortar()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(MORTAR_EXPORT);
            try
            {
                var report = Entry(MORTAR_EXPORT, MORTAR_SOURCE, "update independent export artwork in place");
                var snapshot = new PreservationSnapshot(contents);
                SyncArt(Load(MORTAR_SOURCE).transform, contents.transform, report);
                snapshot.Verify();
                report.existingTransformsPreserved = report.gameplayComponentsPreserved = true;
                report.currentArtMatches = ArtState(contents.transform) == ArtState(Load(MORTAR_SOURCE).transform);
                if (!report.currentArtMatches) throw new InvalidOperationException("Mortar export art does not match its gameplay source.");
                CollectReferences(contents, report);
                Save(contents, MORTAR_EXPORT);
                return report;
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        private static ExportReport RefreshShowcase()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(SHOWCASE);
            try
            {
                var report = Entry(SHOWCASE, "current existing collection exports", "refresh existing eight visual displays; retain labels, shelves and layout");
                var snapshot = new PreservationSnapshot(contents);
                for (int i = 0; i < TYPES.Length; i++)
                    SyncDisplay(contents.transform, "0" + (i + 1) + "_" + NAMES[i], BlockPath(i), report);
                SyncDisplay(contents.transform, "Tower_26_Blocks_With_Stand", TOWER, report);
                SyncDisplay(contents.transform, "Complete_Mortar_All_Sides", MORTAR_EXPORT, report);
                SyncDisplay(contents.transform, "Energy_Ball", COLLECTION + "/PF_04_EnergyBall.prefab", report);
                snapshot.Verify();
                report.existingTransformsPreserved = report.gameplayComponentsPreserved = true;
                report.currentArtMatches = true;
                CollectReferences(contents, report);
                Save(contents, SHOWCASE);
                return report;
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        private static void SyncDisplay(Transform gallery, string name, string sourcePath, ExportReport report)
        {
            Transform target = gallery.Find(name);
            if (target == null) throw new InvalidOperationException("Expected existing display: " + name);
            Transform source = Load(sourcePath).transform;
            SyncArt(source, target, report);
            if (ArtState(source) != ArtState(target))
                throw new InvalidOperationException("Display art differs from its source: " + name);
        }

        private static void SyncArt(Transform source, Transform target, ExportReport report)
        {
            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            if (sourceFilter != null)
            {
                MeshFilter filter = target.GetComponent<MeshFilter>();
                if (filter == null) filter = target.gameObject.AddComponent<MeshFilter>();
                if (filter.sharedMesh != sourceFilter.sharedMesh)
                { filter.sharedMesh = sourceFilter.sharedMesh; report.updatedMeshFilters++; }
            }
            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            if (sourceRenderer != null)
            {
                MeshRenderer renderer = target.GetComponent<MeshRenderer>();
                if (renderer == null) renderer = target.gameObject.AddComponent<MeshRenderer>();
                if (RendererState(renderer) != RendererState(sourceRenderer))
                {
                    renderer.sharedMaterials = sourceRenderer.sharedMaterials;
                    renderer.enabled = sourceRenderer.enabled;
                    renderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                    renderer.receiveShadows = sourceRenderer.receiveShadows;
                    renderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
                    renderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;
                    renderer.renderingLayerMask = sourceRenderer.renderingLayerMask;
                    report.updatedRenderers++;
                }
            }
            var occurrences = new Dictionary<string, int>();
            foreach (Transform sourceChild in source)
            {
                int occurrence;
                occurrences.TryGetValue(sourceChild.name, out occurrence);
                occurrences[sourceChild.name] = occurrence + 1;
                Transform targetChild = FindOccurrence(target, sourceChild.name, occurrence);
                if (targetChild != null) { SyncArt(sourceChild, targetChild, report); continue; }
                if (sourceChild.GetComponentInChildren<MeshFilter>(true) == null) continue;
                // Newly introduced Phase32 art is renderer-only. Never clone gameplay,
                // colliders or ExecuteAlways scripts into the preview or export.
                foreach (Component component in sourceChild.GetComponentsInChildren<Component>(true))
                    if (!(component is Transform) && !(component is MeshFilter) && !(component is MeshRenderer))
                        throw new InvalidOperationException("Missing art subtree contains a nonvisual component: " + sourceChild.name);
                // Parent-aware cloning targets the loaded prefab's isolated scene directly;
                // no temporary GameObject is created in MainGame or its Phase33 hierarchy.
                GameObject clone = UnityEngine.Object.Instantiate(sourceChild.gameObject, target, false);
                clone.name = sourceChild.name;
                report.addedVisualObjects += clone.GetComponentsInChildren<Transform>(true).Length;
            }
        }

        private static Transform FindOccurrence(Transform parent, string name, int occurrence)
        {
            foreach (Transform child in parent)
                if (child.name == name && occurrence-- == 0) return child;
            return null;
        }

        private static void AuditVariant(string sourcePath, string exportPath, RevisionReport report,
            Dictionary<string, string> protectedFiles)
        {
            GameObject source = Load(sourcePath), export = Load(exportPath);
            GameObject inherited = PrefabUtility.GetCorrespondingObjectFromSource(export);
            if (PrefabUtility.GetPrefabAssetType(export) != PrefabAssetType.Variant ||
                AssetDatabase.GetAssetPath(inherited) != sourcePath)
                throw new InvalidOperationException("Expected existing source inheritance on " + exportPath);
            var entry = Entry(exportPath, sourcePath, "retain existing prefab variant");
            entry.currentArtMatches = ArtState(source.transform) == ArtState(export.transform);
            if (!entry.currentArtMatches)
                throw new InvalidOperationException("Variant contains authored art overrides; review before updating: " + exportPath);
            entry.existingTransformsPreserved = entry.gameplayComponentsPreserved = true;
            CollectReferences(export, entry);
            report.exports.Add(entry);
            Protect(sourcePath, protectedFiles); Protect(exportPath, protectedFiles);
        }

        private static bool AuditTower(RevisionReport report)
        {
            var types = new HashSet<string>();
            foreach (Transform child in Load(TOWER).transform)
            {
                GameObject inherited = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                string path = AssetDatabase.GetAssetPath(inherited);
                if (path == COLLECTION + "/PF_03_Stand.prefab") report.towerStands++;
                else
                {
                    bool known = false;
                    for (int i = 0; i < TYPES.Length; i++)
                        if (path == BlockPath(i)) { known = true; types.Add(TYPES[i]); break; }
                    if (!known) throw new InvalidOperationException("Tower contains a nonstandard placement; preserve it for review: " + child.name);
                    report.towerBlocks++;
                }
            }
            report.blockTypes = types.Count;
            if (report.towerBlocks != 26 || report.towerStands != 1 || report.blockTypes != 5)
                throw new InvalidOperationException("Existing 26-block showcase layout changed; do not rebuild it automatically.");
            return true;
        }

        private static ExportReport Entry(string path, string source, string mode)
            => new ExportReport { path = path, sourcePath = source, mode = mode, guidBefore = AssetDatabase.AssetPathToGUID(path) };
        private static string BlockPath(int index)
            => COLLECTION + "/Blocks/PF_Block_0" + (index + 1) + "_" + NAMES[index] + ".prefab";

        private static GameObject Load(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Missing existing collection dependency: " + path);
            return prefab;
        }
        private static void Save(GameObject root, string path)
        {
            if (PrefabUtility.SaveAsPrefabAsset(root, path) == null)
                throw new InvalidOperationException("Could not save collection export " + path);
        }

        private static string ArtState(Transform root)
        {
            var state = new List<string>();
            CollectArt(root, "", state);
            state.Sort(StringComparer.Ordinal);
            return string.Join("\n", state.ToArray());
        }
        private static void CollectArt(Transform transform, string path, List<string> state)
        {
            var filter = transform.GetComponent<MeshFilter>();
            var renderer = transform.GetComponent<MeshRenderer>();
            if (filter != null) state.Add(path + "|mesh|" + AssetId(filter.sharedMesh));
            if (renderer != null) state.Add(path + "|renderer|" + RendererState(renderer));
            var occurrences = new Dictionary<string, int>();
            foreach (Transform child in transform)
            {
                int occurrence; occurrences.TryGetValue(child.name, out occurrence);
                occurrences[child.name] = occurrence + 1;
                CollectArt(child, path + "/" + child.name + "[" + occurrence + "]", state);
            }
        }
        private static string RendererState(MeshRenderer renderer)
        {
            var text = new StringBuilder();
            text.Append(renderer.enabled).Append('|').Append(renderer.shadowCastingMode).Append('|')
                .Append(renderer.receiveShadows).Append('|').Append(renderer.lightProbeUsage).Append('|')
                .Append(renderer.reflectionProbeUsage).Append('|').Append(renderer.renderingLayerMask);
            foreach (Material material in renderer.sharedMaterials) text.Append('|').Append(AssetId(material));
            return text.ToString();
        }
        private static string AssetId(UnityEngine.Object value)
        {
            if (value == null) return "null";
            string guid; long fileId;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out guid, out fileId)) return guid + ":" + fileId;
            return value.GetType().Name + ":" + value.name;
        }

        private static void CollectReferences(GameObject root, ExportReport report)
        {
            foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) throw new InvalidOperationException("Missing script on existing collection export.");
                var serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference ||
                        property.propertyPath == "m_GameObject" || property.propertyPath == "m_Script") continue;
                    UnityEngine.Object value = property.objectReferenceValue;
                    string valuePath = value == null ? "null" : AssetDatabase.GetAssetPath(value);
                    if (value != null && string.IsNullOrEmpty(valuePath))
                    {
                        Component localComponent = value as Component;
                        GameObject localObject = value as GameObject;
                        Transform local = localComponent != null ? localComponent.transform : localObject != null ? localObject.transform : null;
                        valuePath = local == null ? value.name : LocalPath(local, root.transform);
                    }
                    report.componentReferences.Add(LocalPath(component.transform, root.transform) + ":" +
                        component.GetType().Name + "." + property.propertyPath + " -> " + valuePath);
                }
            }
        }
        private static string LocalPath(Transform transform, Transform root)
        {
            string path = transform.name;
            while (transform != root && transform.parent != null)
            { transform = transform.parent; path = transform.name + "/" + path; }
            return path;
        }
        private static void Protect(string path, Dictionary<string, string> files)
        {
            if (!files.ContainsKey(path)) files.Add(path, Hash(path));
            if (!files.ContainsKey(path + ".meta")) files.Add(path + ".meta", Hash(path + ".meta"));
        }
        private static string Hash(string path)
        {
            using (SHA256 hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(File.ReadAllBytes(path)));
        }
        private static string SceneRoots(Scene scene)
        {
            var roots = new List<string>();
            foreach (GameObject root in scene.GetRootGameObjects()) roots.Add(root.GetInstanceID() + ":" + root.name);
            return string.Join("|", roots.ToArray());
        }

        private sealed class PreservationSnapshot
        {
            private readonly Dictionary<Transform, PoseState> _poses = new Dictionary<Transform, PoseState>();
            private readonly Dictionary<Component, string> _components = new Dictionary<Component, string>();
            public PreservationSnapshot(GameObject root)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true)) _poses.Add(transform, new PoseState(transform));
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    if (component is MonoBehaviour || component is Collider || component is Rigidbody)
                        _components.Add(component, EditorJsonUtility.ToJson(component));
            }
            public void Verify()
            {
                foreach (var entry in _poses)
                    if (entry.Key == null || !entry.Value.Matches(entry.Key))
                        throw new InvalidOperationException("An existing collection transform or placement changed.");
                foreach (var entry in _components)
                    if (entry.Key == null || EditorJsonUtility.ToJson(entry.Key) != entry.Value)
                        throw new InvalidOperationException("An existing collection component or reference changed.");
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
                public bool Matches(Transform transform) => transform.localPosition == _position && transform.localScale == _scale &&
                    transform.localRotation == _rotation && transform.parent == _parent && transform.name == _name &&
                    transform.gameObject.layer == _layer && transform.gameObject.activeSelf == _active && transform.GetSiblingIndex() == _sibling;
            }
        }
    }
}
