using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Frame-driven 30-level regression using real Play Mode physics and projectiles.</summary>
    public static class Phase26PlayModeTest
    {
        [Serializable] public class Result
        {
            public int level, expected, idleRemaining, projectileHits;
            public float maxPositionDrift, maxRotationDrift;
            public bool held, rejectedShotStayedHeld, released, consumedOneBall, passed;
        }
        [Serializable] public class Report
        {
            public string status, updatedUtc;
            public int completed, passed;
            public List<Result> levels = new List<Result>();
        }
        public static Report Current { get; private set; }
        private static LevelManager _manager;
        private static LevelLoader _loader;
        private static MortarController _mortar;
        private static InputHandler _input;
        private static bool _inputEnabled;
        private static int _index, _originalLevel, _state;
        private static float _started;
        private static Vector3[] _positions;
        private static Quaternion[] _rotations;
        private static Result _result;
        private static int _hits;
        private const string PATH = "_FarzetkiDocs/Art/Phase26/playmode-30-levels.json";

        [MenuItem("Royal Smash/Phase 26/Run 30 Level Play Tests")]
        public static void Begin()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Enter MainGame Play Mode first.");
            if (Current != null && Current.status == "running") throw new InvalidOperationException("Tests already running.");
            _manager = LevelManager.Instance;
            _loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            _mortar = UnityEngine.Object.FindFirstObjectByType<MortarController>();
            if (_manager == null || _loader == null || _mortar == null) throw new InvalidOperationException("MainGame components missing.");
            _input = UnityEngine.Object.FindFirstObjectByType<InputHandler>();
            _inputEnabled = _input != null && _input.enabled;
            if (_input != null) _input.enabled = false;
            _originalLevel = _manager.CurrentLevelIndex;
            Application.runInBackground = true;
            Time.timeScale = 1;
            Current = new Report { status = "running" };
            _index = 1;
            LoadNext();
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        private static void LoadNext()
        {
            foreach (var projectile in UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None))
            {
                projectile.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(projectile.gameObject);
            }
            _manager.ResetState();
            _manager.SetLevelIndex(_index);
            var data = _manager.Database.GetLevel(_index);
            _loader.LoadLevel(data);
            _mortar.SetBallCount(data.BallCount);
            _mortar.ResetCooldown();
            _result = new Result { level = _index, expected = data.Objects.Length, held = true };
            _positions = new Vector3[data.Objects.Length];
            _rotations = new Quaternion[data.Objects.Length];
            for (int i = 0; i < _loader.SpawnedObjects.Count; i++)
            {
                _positions[i] = _loader.SpawnedObjects[i].transform.position;
                _rotations[i] = _loader.SpawnedObjects[i].transform.rotation;
            }
            _started = Time.fixedTime;
            _state = 0;
            SaveReport();
        }

        private static void Tick()
        {
            if (!Application.isPlaying || _manager == null) { End("interrupted"); return; }
            try
            {
                if (_state == 0)
                {
                    if (Time.fixedTime <= _started) return;
                    _manager.ScanAndRegisterSceneTargets();
                    _state = 1;
                }
                if (_state == 1)
                {
                    var objects = _loader.SpawnedObjects;
                    var gate = _loader.GetComponent<LevelStartPhysicsGate>();
                    _result.held &= gate != null && gate.IsWaitingForFirstShot && gate.HeldBodyCount == _result.expected;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        var item = objects[i];
                        if (item == null) { _result.held = false; continue; }
                        _result.maxPositionDrift = Mathf.Max(_result.maxPositionDrift, Vector3.Distance(_positions[i], item.transform.position));
                        _result.maxRotationDrift = Mathf.Max(_result.maxRotationDrift, Quaternion.Angle(_rotations[i], item.transform.rotation));
                        _result.held &= item.Rigidbody.isKinematic;
                    }
                    if (Time.fixedTime - _started < 3f) return;
                    _result.idleRemaining = _manager.RemainingTargets;
                    var target = objects[0].transform.position;
                    int ammo = _mortar.RemainingBalls;
                    _mortar.SetBallCount(0);
                    _mortar.Fire(target);
                    bool zeroAmmoHeld = gate.IsWaitingForFirstShot && _mortar.RemainingBalls == 0;
                    _mortar.SetBallCount(ammo);
                    // The zero-ammo event schedules a loss evaluation. Cancel that test-only evaluation.
                    _manager.ResetState();
                    typeof(MortarController).GetField("_cooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_mortar, 1f);
                    _mortar.Fire(target);
                    _result.rejectedShotStayedHeld = zeroAmmoHeld && gate.IsWaitingForFirstShot && _mortar.RemainingBalls == ammo;
                    _mortar.ResetCooldown();
                    _hits = 0;
                    float nearest = float.MaxValue;
                    foreach (var item in objects)
                    {
                        item.OnObjectHit += CountHit;
                        float score = Mathf.Abs(item.transform.position.x) + item.transform.position.y * .4f;
                        if (score < nearest) { nearest = score; target = item.transform.position; }
                    }
                    _mortar.Fire(target);
                    _result.consumedOneBall = _mortar.RemainingBalls == ammo - 1;
                    _result.released = !gate.IsWaitingForFirstShot;
                    foreach (var item in objects) _result.released &= !item.Rigidbody.isKinematic;
                    _started = Time.fixedTime;
                    _state = 2;
                }
                else if (_state == 2 && Time.fixedTime - _started >= 2f)
                {
                    _result.projectileHits = _hits;
                    _result.passed = _result.held && _result.idleRemaining == _result.expected &&
                        _result.maxPositionDrift < .0001f && _result.maxRotationDrift < .001f &&
                        _result.rejectedShotStayedHeld && _result.released && _result.consumedOneBall && _hits > 0;
                    Current.levels.Add(_result);
                    Current.completed++;
                    if (_result.passed) Current.passed++;
                    SaveReport();
                    if (++_index > 30) End(Current.passed == 30 ? "passed" : "failed");
                    else LoadNext();
                }
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                End("error");
            }
        }

        private static void CountHit(PhysicsObject target, Collision collision) => _hits++;

        private static void End(string status)
        {
            EditorApplication.update -= Tick;
            if (Current == null) return;
            Current.status = status;
            if (Application.isPlaying && _manager != null && _loader != null)
            {
                _index = _originalLevel;
                LoadNext();
                _manager.ScanAndRegisterSceneTargets();
            }
            if (_input != null) _input.enabled = _inputEnabled;
            SaveReport();
            Debug.Log("[Phase26] " + status + ": " + Current.passed + "/" + Current.completed + " levels.");
        }

        private static void SaveReport()
        {
            Current.updatedUtc = DateTime.UtcNow.ToString("o");
            Directory.CreateDirectory(Path.GetDirectoryName(PATH));
            File.WriteAllText(PATH, JsonUtility.ToJson(Current, true));
        }
    }
}
