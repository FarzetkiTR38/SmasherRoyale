using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.Save;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>Waits for rendered frames between modal events, then verifies raycasts and shrink scale.</summary>
    public static class Phase26PresentationTest
    {
        [Serializable] public class Report
        {
            public string status = "running";
            public int width, height;
            public List<string> passed = new List<string>(), failed = new List<string>();
            public float maximumShrinkScale;
        }
        public static Report Current { get; private set; }
        private static InGameSettingsDialog _settings;
        private static ExitConfirmDialog _exit;
        private static MortarController _mortar;
        private static LevelLoader _loader;
        private static InputHandler _input;
        private static bool _inputEnabled;
        private static int[] _before;
        private static int _step;
        private static double _next, _started;
        private static PhysicsObject _shrink;
        private static float _lastScale;
        private static bool _monotonic;

        public static void Begin()
        {
            if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainGame")
                throw new InvalidOperationException("Run in MainGame Play Mode.");
            if (Current != null && Current.status == "running") throw new InvalidOperationException("Test already running.");
            _settings = UnityEngine.Object.FindFirstObjectByType<InGameSettingsDialog>();
            _exit = UnityEngine.Object.FindFirstObjectByType<ExitConfirmDialog>();
            _mortar = UnityEngine.Object.FindFirstObjectByType<MortarController>();
            _loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            _input = UnityEngine.Object.FindFirstObjectByType<InputHandler>();
            _inputEnabled = _input.enabled;
            _input.enabled = false;
            _settings.Hide(); _exit.Hide();
            _before = State();
            Current = new Report { width = Screen.width, height = Screen.height };
            _step = 0;
            _next = EditorApplication.timeSinceStartup + .25;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (!Application.isPlaying) { Finish("interrupted"); return; }
            if (EditorApplication.timeSinceStartup < _next) return;
            try
            {
                switch (_step++)
                {
                    case 0:
                        Check("HUD in screen bounds", UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                            .Where(b => b.name.StartsWith("HUD_") || b.name.StartsWith("SkillBtn_")).All(b =>
                            { var c = new Vector3[4]; b.GetComponent<RectTransform>().GetWorldCorners(c);
                                return c.All(p => p.x >= 0 && p.x <= Screen.width && p.y >= 0 && p.y <= Screen.height); }));
                        Check("Settings button raycast and click", Click(GameObject.Find("HUD_SettingsButton")));
                        break;
                    case 1:
                        Check("Settings visible and paused", _settings.IsVisible && Time.timeScale == 0);
                        Check("Settings blocks world and back HUD", Blocked() && Rays(Center(GameObject.Find("HUD_ExitButton"))).First().gameObject.transform.IsChildOf(_settings.transform));
                        Check("Resume button raycast and click", Click(Reference<Button>(_settings, "_resumeButton").gameObject));
                        break;
                    case 2:
                        Check("Resume hides and restores time", !_settings.IsVisible && Time.timeScale == 1);
                        Check("Back button raycast and click", Click(GameObject.Find("HUD_ExitButton")));
                        break;
                    case 3:
                        Check("Exit confirmation visible", Reference<GameObject>(_exit, "_contentRoot").activeInHierarchy);
                        Check("Exit blocks world and settings HUD", Blocked() && Rays(Center(GameObject.Find("HUD_SettingsButton"))).First().gameObject.transform.IsChildOf(_exit.transform));
                        Check("Cancel button raycast and click", Click(Reference<Button>(_exit, "_cancelButton").gameObject));
                        break;
                    case 4:
                        Check("Cancel hides confirmation", !Reference<GameObject>(_exit, "_contentRoot").activeInHierarchy);
                        Check("Gameplay raycasts clear after modal", !Blocked() && Rays(Camera.main.WorldToScreenPoint(_loader.SpawnedObjects[0].transform.position)).Length == 0);
                        Check("UI preserves ammo, fire count, lives and start lock", _before.SequenceEqual(State()) && _loader.GetComponent<LevelStartPhysicsGate>().IsWaitingForFirstShot);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_wood.prefab");
                        _shrink = UnityEngine.Object.Instantiate(prefab, new Vector3(-50, 3, 7), Quaternion.identity).GetComponent<PhysicsObject>();
                        _shrink.transform.localScale = Vector3.one * .74f;
                        _lastScale = .74f; _monotonic = true;
                        _shrink.TriggerDestroyCondition("Phase26 shrink verification");
                        // StartCoroutine advances to its first yield immediately; inspect that first rendered size too.
                        float firstSize = _shrink.transform.localScale.x;
                        Current.maximumShrinkScale = firstSize;
                        _monotonic = firstSize <= .7401f;
                        _lastScale = firstSize;
                        _started = EditorApplication.timeSinceStartup;
                        _step = 5;
                        _next = 0;
                        return;
                    case 5:
                        if (_shrink != null)
                        {
                            float size = _shrink.transform.localScale.x;
                            Current.maximumShrinkScale = Mathf.Max(Current.maximumShrinkScale, size);
                            _monotonic &= size <= _lastScale + .0001f && size <= .7401f;
                            _lastScale = size;
                            if (EditorApplication.timeSinceStartup - _started > 5) { Check("Shrink finished", false); Finish("failed"); return; }
                            _step = 5; _next = 0; return;
                        }
                        Check("Smaller block shrinks without enlarging", _monotonic);
                        Finish(Current.failed.Count == 0 ? "passed" : "failed");
                        return;
                }
                _next = EditorApplication.timeSinceStartup + .2;
            }
            catch (Exception e) { Current.failed.Add(e.ToString()); Finish("failed"); }
        }

        private static int[] State() => new[] { _mortar.RemainingBalls, _mortar.FireEventCount, SaveManager.Instance.Data.Lives };
        private static T Reference<T>(UnityEngine.Object owner, string field) where T : UnityEngine.Object =>
            (T)new SerializedObject(owner).FindProperty(field).objectReferenceValue;
        private static Vector2 Center(GameObject go)
        { var rt = go.GetComponent<RectTransform>(); return RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center)); }
        private static RaycastResult[] Rays(Vector2 point)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, results);
            return results.ToArray();
        }
        private static bool Blocked() => Rays(new Vector2(Screen.width * .5f, Screen.height * .33f)).Length > 0;
        private static bool Click(GameObject button)
        {
            Vector2 point = Center(button);
            var hits = Rays(point);
            return hits.Length > 0 && ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,
                new PointerEventData(EventSystem.current) { position = point, button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler) == button;
        }
        private static void Check(string name, bool passed) { (passed ? Current.passed : Current.failed).Add(name); }
        private static void Finish(string status)
        {
            EditorApplication.update -= Tick;
            Current.status = status;
            if (Application.isPlaying)
            {
                _settings.Hide(); _exit.Hide();
                if (_shrink != null) UnityEngine.Object.Destroy(_shrink.gameObject);
                var manager = LevelManager.Instance;
                manager.ResetState();
                var level = manager.Database.GetLevel(manager.CurrentLevelIndex);
                _loader.LoadLevel(level);
                _mortar.SetBallCount(level.BallCount); _mortar.ResetCooldown();
                _input.enabled = _inputEnabled;
            }
            Directory.CreateDirectory("_FarzetkiDocs/Art/Phase26");
            File.WriteAllText("_FarzetkiDocs/Art/Phase26/presentation-" + Current.width + "x" + Current.height + ".json", JsonUtility.ToJson(Current, true));
            Debug.Log("[Phase26Presentation] " + status + " (" + Current.passed.Count + " passed, " + Current.failed.Count + " failed)");
        }
    }
}
