using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.Skills;

namespace RoyalSmash.Editor
{
    /// <summary>Real-frame smoke test of the current puzzle; never loads levels or edits scene assets.</summary>
    public static class Phase32VisualVerification
    {
        [Serializable]
        public sealed class Report
        {
            public string status = "running", reason, startedUtc, finishedUtc, hitTargetName;
            public bool passed, idleHeld, missHeld, releasedOnImpact;
            public int level, objects, heldBodies, ballsBefore, ballsAfter, firesBefore, firesAfter;
            public int projectileHits, removedObjects;
            public float idleSeconds, missSeconds, hitSeconds, totalSeconds;
            public float maxIdleDisplacement, maxMissDisplacement, maxHitDisplacement;
            public Vector3 missTarget = new Vector3(12f, 8f, 8f), hitTarget;
        }

        private const string ReportPath = "_FarzetkiDocs/Art/Phase32/32-02-playmode-smoke.json";
        public static Report Current { get; private set; }
        public static string FinishStatus => Current != null ? Current.status : "idle";
        private static LevelLoader _loader;
        private static MortarController _mortar;
        private static LevelStartPhysicsGate _gate;
        private static PhysicsObject[] _objects;
        private static Vector3[] _positions;
        private static int _phase;
        private static float _phaseStart, _testStart;
        private static double _wallStart;

        [MenuItem("Royal Smash/Phase 32/Run Current Level Visual Smoke Test")]
        public static void Begin()
        {
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainGame")
                throw new InvalidOperationException("Run in MainGame Play Mode.");
            if (FinishStatus == "running") throw new InvalidOperationException("Smoke test already running.");
            _loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            _mortar = UnityEngine.Object.FindFirstObjectByType<MortarController>();
            _gate = _loader != null ? _loader.GetComponent<LevelStartPhysicsGate>() : null;
            if (_loader == null || _mortar == null || _gate == null || !_gate.IsWaitingForFirstShot ||
                _gate.HeldBodyCount == 0 || _loader.SpawnedObjects.Count == 0 || _mortar.RemainingBalls < 2 || Time.timeScale <= 0)
                throw new InvalidOperationException("Requires a fresh, unpaused puzzle with a held gate and at least two balls.");
            if (UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None).Length != 0)
                throw new InvalidOperationException("Wait until existing projectiles are gone before testing.");
            if (SkillManager.HasInstance && (SkillManager.Instance.IsSkillActive(SkillType.MegaBall) ||
                SkillManager.Instance.IsSkillActive(SkillType.InfinityBall)))
                throw new InvalidOperationException("Run the normal-ball smoke test without active skills.");

            _objects = _loader.SpawnedObjects.ToArray();
            _positions = new Vector3[_objects.Length];
            for (int i = 0; i < _objects.Length; i++)
            {
                if (_objects[i] == null) throw new InvalidOperationException("Puzzle contains a missing target.");
                _positions[i] = _objects[i].transform.position;
            }
            Current = new Report { startedUtc = DateTime.UtcNow.ToString("O"), objects = _objects.Length,
                level = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelIndex : 0,
                heldBodies = _gate.HeldBodyCount, ballsBefore = _mortar.RemainingBalls,
                firesBefore = _mortar.FireEventCount, idleHeld = true, missHeld = true };
            _phase = 0;
            _testStart = _phaseStart = Time.fixedTime;
            _wallStart = EditorApplication.timeSinceStartup;
            WriteReport();
            foreach (var item in _objects) item.OnObjectHit += ObserveImpact;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
        }

        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainGame" ||
                    _loader == null || _mortar == null || _gate == null)
                { Finish("interrupted", "Play Mode or puzzle changed."); return; }
                if (EditorApplication.timeSinceStartup - _wallStart > 45)
                { Finish("failed", "Timed out waiting for real physics frames."); return; }
                float elapsed = Time.fixedTime - _phaseStart;
                float drift = 0f;
                int removed = 0;
                foreach (var item in _objects)
                    if (item == null || !item.gameObject.activeInHierarchy || (int)item.CurrentState >= (int)ObjectState.DestroyCondition) removed++;
                for (int i = 0; i < _objects.Length; i++)
                    if (_objects[i] != null) drift = Mathf.Max(drift, Vector3.Distance(_positions[i], _objects[i].transform.position));
                Current.removedObjects = Mathf.Max(Current.removedObjects, removed);
                bool held = _gate.IsWaitingForFirstShot && _gate.HeldBodyCount == Current.heldBodies && removed == 0;
                int expectedShots = _phase == 0 ? 0 : _phase;
                if (_mortar.FireEventCount != Current.firesBefore + expectedShots || _mortar.RemainingBalls != Current.ballsBefore - expectedShots)
                { Finish("failed", "Unexpected shot or ammunition change."); return; }
                if (_phase < 2)
                {
                    if (_phase == 0) { Current.idleHeld &= held; Current.idleSeconds = elapsed; Current.maxIdleDisplacement = Mathf.Max(Current.maxIdleDisplacement, drift); }
                    else { Current.missHeld &= held; Current.missSeconds = elapsed; Current.maxMissDisplacement = Mathf.Max(Current.maxMissDisplacement, drift); }
                    if (!held || drift > 0.001f || Current.projectileHits != 0)
                    { Finish("failed", "Puzzle moved or gate opened before the deliberate impact."); return; }
                    if (elapsed < 3f) return;
                    Vector3 target = Current.missTarget;
                    if (_phase == 1)
                    {
                        Collider front = null;
                        float best = float.PositiveInfinity;
                        foreach (var item in _objects)
                        {
                            var collider = item.Collider;
                            if (collider == null || !collider.enabled) continue;
                            Vector3 center = collider.bounds.center;
                            float score = center.z * 1000f + center.y * 10f + Mathf.Abs(center.x);
                            if (score < best) { best = score; front = collider; }
                        }
                        if (front == null) { Finish("failed", "No active target collider."); return; }
                        Current.hitTargetName = front.gameObject.name;
                        target = Current.hitTarget = front.bounds.center;
                    }
                    _mortar.ResetCooldown();
                    _mortar.Fire(target);
                    _phase++;
                    _phaseStart = Time.fixedTime;
                    WriteReport();
                }
                else
                {
                    Current.hitSeconds = elapsed;
                    Current.maxHitDisplacement = Mathf.Max(Current.maxHitDisplacement, drift);
                    if (Current.projectileHits > 0 && Current.releasedOnImpact && !_gate.IsWaitingForFirstShot &&
                        (Current.maxHitDisplacement > 0.005f || removed > 0))
                    { Finish("passed", "Idle and missed shot held; real projectile impact released the gate and changed the puzzle."); return; }
                    if (elapsed >= 8f) Finish("failed", "No verified projectile impact, gate release and physical/destruction response within eight seconds.");
                }
            }
            catch (Exception error) { Finish("failed", error.ToString()); }
        }

        private static void ObserveImpact(PhysicsObject item, Collision collision)
        {
            if (collision == null || collision.gameObject.GetComponent<ProjectileController>() == null) return;
            Current.projectileHits++;
            if (_phase == 2 && _gate != null && !_gate.IsWaitingForFirstShot) Current.releasedOnImpact = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                Finish("interrupted", "Play Mode ended before completion.");
        }
        private static void OnAssemblyReload() => Finish("interrupted", "Assembly reload interrupted the test.");

        private static void Finish(string status, string reason)
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnAssemblyReload;
            if (_objects != null) foreach (var item in _objects) if (item != null) item.OnObjectHit -= ObserveImpact;
            if (Current == null || Current.status != "running") return;
            Current.status = status; Current.passed = status == "passed"; Current.reason = reason;
            Current.finishedUtc = DateTime.UtcNow.ToString("O");
            Current.totalSeconds = Mathf.Max(0f, Time.fixedTime - _testStart);
            Current.ballsAfter = _mortar != null ? _mortar.RemainingBalls : -1;
            Current.firesAfter = _mortar != null ? _mortar.FireEventCount : -1;
            try { WriteReport(); }
            catch (Exception error) { Current.status = "failed"; Current.passed = false; Debug.LogException(error); }
            Debug.Log($"[Phase32VisualVerification] {Current.status}: {reason}");
        }

        private static void WriteReport()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, JsonUtility.ToJson(Current, true));
        }
    }
}
