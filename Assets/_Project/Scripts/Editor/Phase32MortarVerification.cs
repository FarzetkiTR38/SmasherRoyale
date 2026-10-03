using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>Preservation checks and a real-frame aim check; never fires, loads levels or saves a scene.</summary>
    public static class Phase32MortarVerification
    {
        private const string SCENE = "Assets/_Project/Scenes/MainGame.unity";
        private const string PREFAB = "Assets/_Project/Prefabs/Gameplay/Mortar.prefab";
        private const string OUTPUT = "_FarzetkiDocs/Art/Phase32/";
        private const string BASELINE = OUTPUT + "32-04-mortar-baseline.json";
        private const string WORKING_ROOT = "[LevelDesigner_WorkingRoot]";

        [Serializable] public sealed class ComponentState
        {
            public string key, editorJson, canonicalJson;
        }
        [Serializable] public sealed class NodeState
        {
            public string path, tag;
            public int layer, siblingIndex;
            public bool active;
            public Vector3 position, scale;
            public Quaternion rotation;
            public List<ComponentState> components = new List<ComponentState>();
        }
        [Serializable] public sealed class HierarchyState
        {
            public int colliders, rigidbodies;
            public List<NodeState> nodes = new List<NodeState>();
        }
        [Serializable] public sealed class BaselineReport
        {
            public string capturedUtc, scenePath, prefabGuid, workingRootHash;
            public HierarchyState sceneMortar, prefabMortar, workingRoot;
            public List<ComponentState> configurations = new List<ComponentState>();
        }
        [Serializable] public sealed class PreservationReport
        {
            public string finishedUtc, baselineUtc, workingRootHash;
            public bool passed;
            public List<string> violations = new List<string>();
        }
        [Serializable] public sealed class AimSample
        {
            public float requestedYaw, expectedYaw, actualYaw, elapsedSeconds;
            public Vector3 firePointWorld;
        }
        [Serializable] public sealed class AimReport
        {
            public string status = "running", reason, startedUtc, finishedUtc;
            public string recoil = "Not tested: current mortar has no recoil implementation.";
            public bool passed;
            public int ballsBefore, ballsAfter, firesBefore, firesAfter, visualRenderers;
            public List<AimSample> samples = new List<AimSample>();
        }

        private sealed class VisualPose
        {
            public Transform transform;
            public Vector3 localToVisuals;
            public Quaternion rotationToVisuals;
        }

        private static readonly float[] AIM_YAWS = { -45f, 45f, 0f };
        private static readonly Regex INSTANCE_ID = new Regex("\"instanceID\"\\s*:\\s*(-?\\d+)");
        private static MortarController _mortar;
        private static Transform _visuals, _firePoint, _fireParent;
        private static Vector3 _firePosition, _fireScale;
        private static Quaternion _fireRotation;
        private static readonly List<VisualPose> _visualPoses = new List<VisualPose>();
        private static MethodInfo _aimHandler;
        private static int _aimStage;
        private static float _aimStarted;
        private static double _wallStarted;
        public static AimReport CurrentAim { get; private set; }
        public static string AimStatus => CurrentAim != null ? CurrentAim.status : "idle";

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/04 - Capture Mortar Baseline")]
        public static void CaptureBaseline()
        {
            GuardEditScene();
            var mortar = RequireSceneMortar();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
            if (prefab == null) throw new InvalidOperationException("Mortar prefab is missing.");
            var working = FindRoot(WORKING_ROOT);
            var baseline = new BaselineReport
            {
                capturedUtc = DateTime.UtcNow.ToString("O"), scenePath = SCENE,
                prefabGuid = AssetDatabase.AssetPathToGUID(PREFAB),
                sceneMortar = Capture(mortar.transform, false), prefabMortar = Capture(prefab.transform, false),
                workingRoot = working != null ? Capture(working.transform, true) : null
            };
            baseline.configurations = CaptureConfigurations(mortar);
            baseline.workingRootHash = Hash(baseline.workingRoot);
            Write(BASELINE, baseline);
            Debug.Log("[Phase32MortarVerification] Baseline captured; scene and prefab were not modified.");
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/04 - Verify Mortar Preservation")]
        public static void Finish()
        {
            GuardEditScene();
            if (!File.Exists(BASELINE)) throw new InvalidOperationException("Capture the baseline before applying the revision.");
            var baseline = JsonUtility.FromJson<BaselineReport>(File.ReadAllText(BASELINE));
            if (baseline == null || baseline.sceneMortar == null || baseline.prefabMortar == null)
                throw new InvalidOperationException("Mortar baseline is incomplete.");
            var report = new PreservationReport { baselineUtc = baseline.capturedUtc, finishedUtc = DateTime.UtcNow.ToString("O") };
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB);
            if (AssetDatabase.AssetPathToGUID(PREFAB) != baseline.prefabGuid) report.violations.Add("Mortar prefab GUID changed.");
            Compare("Scene mortar", baseline.sceneMortar, Capture(RequireSceneMortar().transform, false), report.violations);
            Compare("Prefab mortar", baseline.prefabMortar, prefab != null ? Capture(prefab.transform, false) : null, report.violations);
            var configurations = CaptureConfigurations(RequireSceneMortar());
            if (configurations.Count != baseline.configurations.Count) report.violations.Add("Mortar configuration references changed.");
            else for (int i = 0; i < configurations.Count; i++)
                if (configurations[i].key != baseline.configurations[i].key || configurations[i].canonicalJson != baseline.configurations[i].canonicalJson)
                    report.violations.Add("Mortar configuration asset changed: " + baseline.configurations[i].key);
            var working = FindRoot(WORKING_ROOT);
            report.workingRootHash = Hash(working != null ? Capture(working.transform, true) : null);
            if (report.workingRootHash != baseline.workingRootHash) report.violations.Add("Phase33 authoring hierarchy/components changed.");
            report.passed = report.violations.Count == 0;
            Write(OUTPUT + "32-04-mortar-preservation.json", report);
            if (!report.passed) throw new InvalidOperationException(string.Join("\n", report.violations));
            Debug.Log("[Phase32MortarVerification] Passed: original transforms, gameplay components, physics counts and Phase33 authoring tree preserved.");
        }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/04 - Run Mortar Aim Check")]
        public static void BeginAimTest()
        {
            if (!Application.isPlaying || SceneManager.GetActiveScene().path != SCENE || Time.timeScale <= 0f)
                throw new InvalidOperationException("Run in unpaused MainGame Play Mode.");
            if (AimStatus == "running") throw new InvalidOperationException("Aim check is already running.");
            _mortar = RequireSceneMortar().GetComponent<MortarController>();
            _visuals = _mortar.transform.Find("Visuals");
            _firePoint = _mortar.FirePoint;
            if (_visuals == null || _firePoint == null || _firePoint != _mortar.transform.Find("Visuals/BarrelPivot/FirePoint"))
                throw new InvalidOperationException("Expected mortar aim/fire hierarchy is missing.");
            _aimHandler = typeof(MortarController).GetMethod("HandleAimTargetChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_aimHandler == null) throw new InvalidOperationException("Mortar aim handler is missing.");
            _fireParent = _firePoint.parent; _firePosition = _firePoint.localPosition;
            _fireRotation = _firePoint.localRotation; _fireScale = _firePoint.localScale;
            _visualPoses.Clear();
            foreach (var renderer in _visuals.GetComponentsInChildren<Renderer>(true))
                _visualPoses.Add(new VisualPose { transform = renderer.transform,
                    localToVisuals = _visuals.InverseTransformPoint(renderer.transform.position),
                    rotationToVisuals = Quaternion.Inverse(_visuals.rotation) * renderer.transform.rotation });
            CurrentAim = new AimReport { startedUtc = DateTime.UtcNow.ToString("O"),
                ballsBefore = _mortar.RemainingBalls, firesBefore = _mortar.FireEventCount,
                visualRenderers = _visualPoses.Count };
            _aimStage = 0; _wallStarted = EditorApplication.timeSinceStartup;
            EditorApplication.update += AimTick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
            try { AimNext(); WriteAim(); }
            catch (Exception error) { FinishAim("failed", error.ToString()); throw; }
        }

        private static void AimNext()
        {
            Vector3 target = _mortar.transform.position + Quaternion.Euler(0f, AIM_YAWS[_aimStage], 0f) * Vector3.forward * 10f;
            _aimHandler.Invoke(_mortar, new object[] { target });
            _aimStarted = Time.time;
        }

        private static void AimTick()
        {
            try
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().path != SCENE || _mortar == null || _visuals == null || _firePoint == null)
                { FinishAim("interrupted", "Play Mode or mortar changed."); return; }
                if (EditorApplication.timeSinceStartup - _wallStarted > 20d)
                { FinishAim("failed", "Timed out waiting for gameplay frames."); return; }
                if (_mortar.RemainingBalls != CurrentAim.ballsBefore || _mortar.FireEventCount != CurrentAim.firesBefore)
                { FinishAim("failed", "A shot or ammunition change occurred during the aim-only check."); return; }
                if (_firePoint != _mortar.FirePoint || _firePoint.parent != _fireParent ||
                    !Same(_firePoint.localPosition, _firePosition) || !Same(_firePoint.localScale, _fireScale) ||
                    !Same(_firePoint.localRotation, _fireRotation))
                { FinishAim("failed", "FirePoint identity, parent or local pose changed while aiming."); return; }
                foreach (var pose in _visualPoses)
                    if (pose.transform == null || !pose.transform.IsChildOf(_visuals) ||
                        Vector3.Distance(pose.transform.position, _visuals.TransformPoint(pose.localToVisuals)) > .0001f ||
                        Quaternion.Angle(pose.transform.rotation, _visuals.rotation * pose.rotationToVisuals) > .05f)
                    { FinishAim("failed", "Mortar art did not follow its aim transform rigidly."); return; }
                float elapsed = Time.time - _aimStarted;
                if (elapsed < .6f) return;
                var serialized = new SerializedObject(_mortar);
                var config = serialized.FindProperty("_config").objectReferenceValue as MortarConfig;
                float expected = Mathf.Clamp(AIM_YAWS[_aimStage], config != null ? config.MinYaw : -45f, config != null ? config.MaxYaw : 45f);
                CurrentAim.samples.Add(new AimSample { requestedYaw = AIM_YAWS[_aimStage], expectedYaw = expected,
                    actualYaw = _mortar.CurrentYaw, elapsedSeconds = elapsed, firePointWorld = _firePoint.position });
                if (Mathf.Abs(Mathf.DeltaAngle(_mortar.CurrentYaw, expected)) > .25f ||
                    Quaternion.Angle(_visuals.localRotation, Quaternion.Euler(0f, expected, 0f)) > .25f)
                { FinishAim("failed", "Mortar did not reach the configured aim angle."); return; }
                _aimStage++;
                if (_aimStage == AIM_YAWS.Length) FinishAim("passed", "Left/right/center aiming preserved FirePoint and rigid art follow without spending ammunition.");
                else { AimNext(); WriteAim(); }
            }
            catch (Exception error) { FinishAim("failed", error.ToString()); }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                FinishAim("interrupted", "Play Mode ended before the aim check finished.");
        }
        private static void OnAssemblyReload() => FinishAim("interrupted", "Assembly reload interrupted the aim check.");
        private static void FinishAim(string status, string reason)
        {
            EditorApplication.update -= AimTick;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
            if (CurrentAim == null || CurrentAim.status != "running") return;
            CurrentAim.status = status; CurrentAim.reason = reason; CurrentAim.passed = status == "passed";
            CurrentAim.finishedUtc = DateTime.UtcNow.ToString("O");
            CurrentAim.ballsAfter = _mortar != null ? _mortar.RemainingBalls : -1;
            CurrentAim.firesAfter = _mortar != null ? _mortar.FireEventCount : -1;
            WriteAim();
            _visualPoses.Clear();
            Debug.Log("[Phase32MortarVerification] Aim " + status + ": " + reason);
        }
        private static void WriteAim() => Write(OUTPUT + "32-04-mortar-aim.json", CurrentAim);

        private static HierarchyState Capture(Transform root, bool includeVisualComponents)
        {
            var result = new HierarchyState { colliders = root.GetComponentsInChildren<Collider>(true).Length,
                rigidbodies = root.GetComponentsInChildren<Rigidbody>(true).Length };
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                var node = new NodeState { path = PathFrom(transform, root), position = transform.localPosition,
                    rotation = transform.localRotation, scale = transform.localScale, layer = transform.gameObject.layer,
                    tag = transform.gameObject.tag, active = transform.gameObject.activeSelf, siblingIndex = transform.GetSiblingIndex() };
                var counts = new Dictionary<string, int>();
                foreach (Component component in transform.GetComponents<Component>())
                {
                    if (component == null) throw new InvalidOperationException("Missing script on " + node.path);
                    if (component is Transform || (!includeVisualComponents && IsVisual(component))) continue;
                    string type = component.GetType().AssemblyQualifiedName;
                    int index; counts.TryGetValue(type, out index); counts[type] = index + 1;
                    string json = EditorJsonUtility.ToJson(component);
                    node.components.Add(new ComponentState { key = type + "#" + index, editorJson = json, canonicalJson = CanonicalJson(json) });
                }
                result.nodes.Add(node);
            }
            return result;
        }

        private static bool IsVisual(Component component) => component is Renderer || component is MeshFilter;
        private static List<ComponentState> CaptureConfigurations(GameObject mortar)
        {
            var assets = new SortedDictionary<string, ScriptableObject>(StringComparer.Ordinal);
            foreach (var behaviour in mortar.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;
                var iterator = new SerializedObject(behaviour).GetIterator();
                while (iterator.Next(true))
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is ScriptableObject)
                    {
                        var asset = (ScriptableObject)iterator.objectReferenceValue;
                        assets[StableReference(asset)] = asset;
                    }
            }
            var result = new List<ComponentState>();
            foreach (var item in assets)
            {
                string json = EditorJsonUtility.ToJson(item.Value);
                result.Add(new ComponentState { key = item.Key, editorJson = json, canonicalJson = CanonicalJson(json) });
            }
            return result;
        }
        private static string CanonicalJson(string json) => INSTANCE_ID.Replace(json, match => "\"instanceID\":\"" +
            StableReference(EditorUtility.InstanceIDToObject(int.Parse(match.Groups[1].Value))) + "\"");
        private static void Compare(string label, HierarchyState before, HierarchyState after, List<string> errors)
        {
            if (after == null) { errors.Add(label + " is missing."); return; }
            if (before.colliders != after.colliders || before.rigidbodies != after.rigidbodies)
                errors.Add(label + " collider/rigidbody counts changed.");
            var current = new Dictionary<string, NodeState>();
            foreach (var node in after.nodes) current.Add(node.path, node);
            foreach (var old in before.nodes)
            {
                NodeState node;
                if (!current.TryGetValue(old.path, out node)) { errors.Add(label + " missing original node " + old.path); continue; }
                if (!Same(old.position, node.position) || !Same(old.scale, node.scale) || !Same(old.rotation, node.rotation) ||
                    old.active != node.active || old.layer != node.layer || old.tag != node.tag)
                    errors.Add(label + " original transform/state changed: " + old.path);
                if (old.components.Count != node.components.Count) errors.Add(label + " nonvisual component count changed: " + old.path);
                else for (int i = 0; i < old.components.Count; i++)
                    if (old.components[i].key != node.components[i].key || old.components[i].canonicalJson != node.components[i].canonicalJson)
                        errors.Add(label + " component/reference changed: " + old.path + " / " + old.components[i].key);
                current.Remove(old.path);
            }
            foreach (var node in current.Values)
                if (node.components.Count != 0) errors.Add(label + " new art node contains a nonvisual component: " + node.path);
        }

        private static string StableReference(UnityEngine.Object value)
        {
            if (value == null) return "null";
            var go = value as GameObject;
            var component = value as Component;
            if (component != null) go = component.gameObject;
            if (go != null && go.scene.IsValid())
            {
                string reference = "scene:" + go.scene.path + ":" + PathFrom(go.transform, null);
                if (component != null)
                {
                    var siblings = go.GetComponents(component.GetType());
                    reference += ":" + component.GetType().FullName + "#" + Array.IndexOf(siblings, component);
                }
                return reference.Replace("\\", "\\\\").Replace("\"", "\\\"");
            }
            string guid; long localId;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out guid, out localId)) return "asset:" + guid + ":" + localId;
            return "object:" + value.GetType().FullName + ":" + Uri.EscapeDataString(value.name);
        }

        private static string PathFrom(Transform item, Transform root)
        {
            if (item == root) return ".";
            int ordinal = 0;
            if (item.parent != null)
            {
                for (int i = 0; i < item.GetSiblingIndex(); i++) if (item.parent.GetChild(i).name == item.name) ordinal++;
            }
            else if (item.gameObject.scene.IsValid())
            {
                foreach (var other in item.gameObject.scene.GetRootGameObjects())
                { if (other.transform == item) break; if (other.name == item.name) ordinal++; }
            }
            string segment = Uri.EscapeDataString(item.name) + "[" + ordinal + "]";
            return item.parent == null || item.parent == root ? segment : PathFrom(item.parent, root) + "/" + segment;
        }
        private static bool Same(Vector3 a, Vector3 b) => (a - b).sqrMagnitude <= 1e-12f;
        private static bool Same(Quaternion a, Quaternion b)
        {
            // Compare components to avoid Angle rounding non-unit serialized quaternions above zero.
            var av = new Vector4(a.x, a.y, a.z, a.w);
            var bv = new Vector4(b.x, b.y, b.z, b.w);
            return (av - bv).sqrMagnitude <= 1e-12f || (av + bv).sqrMagnitude <= 1e-12f;
        }
        private static string Hash(HierarchyState state)
        {
            if (state == null) return "absent";
            // Raw Editor JSON is retained as evidence, but instance IDs must not enter the stable hash.
            var stable = JsonUtility.FromJson<HierarchyState>(JsonUtility.ToJson(state));
            foreach (var node in stable.nodes) foreach (var component in node.components) component.editorJson = null;
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(stable)))).Replace("-", "").ToLowerInvariant();
        }
        private static GameObject FindRoot(string name)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) if (root.name == name) return root;
            return null;
        }
        private static GameObject RequireSceneMortar()
        {
            var mortar = FindRoot("Mortar");
            if (mortar == null || mortar.GetComponent<MortarController>() == null) throw new InvalidOperationException("MainGame Mortar is missing.");
            return mortar;
        }
        private static void GuardEditScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != SCENE)
                throw new InvalidOperationException("Open MainGame in Edit Mode.");
        }
        private static void Write(string path, object report)
        {
            Directory.CreateDirectory(OUTPUT);
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }
    }
}
