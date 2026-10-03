using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;
using RoyalSmash.Skills;

namespace RoyalSmash.Editor
{
    /// <summary>Bounded, separate final checks. Play checks use real APIs and require an external save backup.</summary>
    public static class Phase32FinalVerification
    {
        private const string OUTPUT = "_FarzetkiDocs/Art/Phase32/";
        private const string SCENE = "Assets/_Project/Scenes/MainGame.unity";
        private static readonly string[] PREFAB_FIELDS = { "_boxPrefab", "_stonePrefab", "_columnPrefab", "_conePrefab", "_paintBottlePrefab" };

        [Serializable] public class Result
        {
            public string status = "running", reason, startedUtc, finishedUtc;
            public bool passed;
        }
        [Serializable] public sealed class GeometryLevel
        {
            public int level, targets, stands, unsupportedFootprints, overlapCandidates;
            public Rect viewport9x16, viewport9x20;
            public bool targetsInside9x16, targetsInside9x20;
            public List<string> notes = new List<string>();
        }
        [Serializable] public sealed class GeometryReport
        {
            public string capturedUtc;
            public string scope = "Read-only collider AABB survey. Footprint and AABB overlaps are review flags, not exact support/penetration tests or a gameplay pass.";
            public List<GeometryLevel> levels = new List<GeometryLevel>();
            public List<string> errors = new List<string>();
        }
        [Serializable] public sealed class HudItem
        {
            public string path, text;
            public Rect screenRect;
        }
        [Serializable] public sealed class FrameMetrics
        {
            public int level, width, height, targetCount, standCount;
            public Rect safeArea, targetViewport, standViewport;
            public string screenshot;
            public bool screenshotExists;
            public List<HudItem> hud = new List<HudItem>();
        }
        [Serializable] public sealed class IdleLevel
        {
            public int level, objects, expectedObjects, heldBodies, balls;
            public float seconds, maximumDrift;
            public bool passed, gateHeld, allKinematic;
            public FrameMetrics frame;
        }
        [Serializable] public sealed class IdleReport : Result
        {
            public int originalLevel;
            public string captureRun;
            public string scope = "Two real physics seconds per level, no shots. Actual shot/impact behavior requires the separate Phase32VisualVerification smoke test.";
            public List<IdleLevel> levels = new List<IdleLevel>();
        }
        [Serializable] public sealed class ShotEvidence
        {
            public string kind, primaryRenderer, shader;
            public bool mega, primaryEnabled;
            public Vector3 scale;
            public float mass, cooldown, time;
            public Color baseColor;
        }
        [Serializable] public sealed class SkillReport : Result
        {
            public int ballsBefore, ballsAfter, firesBefore, firesAfter, megaUsesBefore, megaUsesAfter, infinityUsesBefore, infinityUsesAfter;
            public float configuredNormalCooldown, configuredRapidCooldown, configuredInfinityDuration, infinityElapsed;
            public bool megaConsumed, infinityExpired, missGateHeld = true;
            public string scope = "Five real miss shots: normal, Mega, three Infinity shots. Does not replace contact/impact or UI pointer tests. Consumes one saved use of each skill.";
            public List<ShotEvidence> shots = new List<ShotEvidence>();
        }
        [Serializable] public sealed class DestructionReport : Result
        {
            public string target, primaryRenderer, primaryShader;
            public bool primaryEnabled, colliderDisabled, kinematic, scaleDecreased, alphaDecreased, removed;
            public Vector3 initialScale, minimumScale;
            public float initialAlpha, minimumAlpha;
            public string scope = "Explicit TriggerDestroyCondition lifecycle check on one real target; this does not assert a projectile caused the destruction.";
        }

        public static IdleReport CurrentIdle { get; private set; }
        public static SkillReport CurrentSkills { get; private set; }
        public static DestructionReport CurrentDestruction { get; private set; }
        public static string Status { get; private set; } = "idle";
        private static string _job;
        private static double _wallStart;
        private static double _captureWaitStarted;
        private static float _phaseStart;
        private static int _stage, _captureFrame;
        private static LevelManager _manager;
        private static LevelLoader _loader;
        private static MortarController _mortar;
        private static SkillManager _skills;
        private static PhysicsObject[] _objects;
        private static Vector3[] _positions;
        private static IdleLevel _idle;
        private static int _idleFires;
        private static PhysicsObject _destroyTarget;
        private static Renderer _destroyRenderer;
        private static LevelStartPhysicsGate _skillGate;

        public static GeometryReport SurveyGeometry()
        {
            GuardScene(false);
            var database = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
            var loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            if (database == null || loader == null || Camera.main == null) throw new InvalidOperationException("MainGame database, loader and camera are required.");
            var serialized = new SerializedObject(loader);
            var stand = serialized.FindProperty("_standPrefab").objectReferenceValue as GameObject;
            var prefabs = new GameObject[PREFAB_FIELDS.Length];
            for (int i = 0; i < prefabs.Length; i++) prefabs[i] = serialized.FindProperty(PREFAB_FIELDS[i]).objectReferenceValue as GameObject;
            var report = new GeometryReport { capturedUtc = DateTime.UtcNow.ToString("O") };
            for (int level = 1; level <= 30; level++)
            {
                try
                {
                    var data = database.GetLevel(level);
                    if (data == null || data.Objects == null || data.Stands == null || stand == null) throw new InvalidOperationException("Level, placements or stand prefab missing.");
                    var row = new GeometryLevel { level = level, targets = data.Objects.Length, stands = data.Stands.Length };
                    var platforms = new List<Bounds>();
                    foreach (var placement in data.Stands) platforms.Add(ColliderBounds(stand, Matrix4x4.TRS(placement.Position, Quaternion.Euler(placement.Rotation), placement.Scale)));
                    var targets = new List<Bounds>();
                    foreach (var placement in data.Objects)
                    {
                        int type = (int)placement.Type;
                        if (type < 0 || type >= prefabs.Length || prefabs[type] == null) throw new InvalidOperationException("Missing target prefab for " + placement.Type);
                        Bounds bounds = ColliderBounds(prefabs[type], Matrix4x4.TRS(placement.Position, Quaternion.Euler(placement.Rotation), placement.Scale));
                        targets.Add(bounds);
                        bool fits = false;
                        foreach (Bounds platform in platforms)
                            fits |= bounds.min.x >= platform.min.x - .003f && bounds.max.x <= platform.max.x + .003f && bounds.min.z >= platform.min.z - .003f && bounds.max.z <= platform.max.z + .003f;
                        if (!fits) row.unsupportedFootprints++;
                    }
                    for (int a = 0; a < targets.Count; a++) for (int b = a + 1; b < targets.Count; b++)
                    {
                        Vector3 overlap = Vector3.Min(targets[a].max, targets[b].max) - Vector3.Max(targets[a].min, targets[b].min);
                        if (Mathf.Min(overlap.x, Mathf.Min(overlap.y, overlap.z)) > .003f) row.overlapCandidates++;
                    }
                    row.viewport9x16 = Project(Camera.main, targets, 9f / 16f);
                    row.viewport9x20 = Project(Camera.main, targets, 9f / 20f);
                    row.targetsInside9x16 = Inside(row.viewport9x16); row.targetsInside9x20 = Inside(row.viewport9x20);
                    if (row.unsupportedFootprints > 0) row.notes.Add("Some target collider AABBs exceed every individual stand footprint.");
                    if (row.overlapCandidates > 0) row.notes.Add("Review overlapping world AABBs; rotated shapes can produce false positives.");
                    report.levels.Add(row);
                }
                catch (Exception error) { report.errors.Add("Level " + level + ": " + error.Message); }
            }
            Write("32-11-geometry-survey.json", report);
            return report;
        }

        public static void BeginIdleSurvey()
        {
            Prepare("idle");
            RequireNoProjectilesOrSkills();
            CurrentIdle = new IdleReport { startedUtc = DateTime.UtcNow.ToString("O"), originalLevel = _manager.CurrentLevelIndex,
                captureRun = DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") };
            _idle = null;
            _stage = 1; _captureFrame = -1;
            StartJob();
            try { LoadIdleLevel(); } catch (Exception error) { Finish("failed", error.ToString()); }
        }
        private static void LoadIdleLevel()
        {
            _manager.LoadLevelDirect(_stage);
            _objects = _loader.SpawnedObjects.ToArray();
            _positions = new Vector3[_objects.Length];
            for (int i = 0; i < _objects.Length; i++) _positions[i] = _objects[i].transform.position;
            var gate = _loader.GetComponent<LevelStartPhysicsGate>();
            _idle = new IdleLevel { level = _stage, objects = _objects.Length, expectedObjects = _manager.Database.GetLevel(_stage).Objects.Length,
                heldBodies = gate != null ? gate.HeldBodyCount : 0, balls = _mortar.RemainingBalls, gateHeld = true, allKinematic = true };
            _idleFires = _mortar.FireEventCount; _phaseStart = Time.fixedTime; _captureFrame = -1;
        }
        private static void IdleTick()
        {
            if (_captureFrame >= 0)
            {
                if (Time.frameCount < _captureFrame + 2) return;
                _idle.frame.screenshotExists = File.Exists(_idle.frame.screenshot) && new FileInfo(_idle.frame.screenshot).Length > 0;
                if (!_idle.frame.screenshotExists)
                {
                    if (EditorApplication.timeSinceStartup - _captureWaitStarted < 8d) return;
                    Finish("failed", "Requested reference screenshot was not written within eight seconds."); return;
                }
                NextIdle(); return;
            }
            var gate = _loader.GetComponent<LevelStartPhysicsGate>();
            _idle.gateHeld &= gate != null && gate.IsWaitingForFirstShot && gate.HeldBodyCount == _idle.heldBodies;
            for (int i = 0; i < _objects.Length; i++)
            {
                var obj = _objects[i];
                if (obj == null || !obj.gameObject.activeInHierarchy || (int)obj.CurrentState >= (int)ObjectState.DestroyCondition)
                { Finish("failed", "An idle target was removed or entered destruction in level " + _stage); return; }
                _idle.maximumDrift = Mathf.Max(_idle.maximumDrift, Vector3.Distance(_positions[i], obj.transform.position));
                _idle.allKinematic &= obj.Rigidbody != null && obj.Rigidbody.isKinematic;
            }
            _idle.seconds = Time.fixedTime - _phaseStart;
            if (!_idle.gateHeld || !_idle.allKinematic || _idle.maximumDrift > .001f || _mortar.FireEventCount != _idleFires || _mortar.RemainingBalls != _idle.balls)
            { Finish("failed", "Idle movement, gate, shot or ammunition invariant failed in level " + _stage); return; }
            if (_idle.seconds < 2f) return;
            _idle.passed = _idle.objects == _idle.expectedObjects && _idle.objects > 0;
            if (!_idle.passed) { Finish("failed", "Spawn count mismatch in level " + _stage); return; }
            _idle.frame = CaptureFrameMetrics();
            CurrentIdle.levels.Add(_idle);
            if (_stage == 1 || _stage == 15 || _stage == 22 || _stage == 30)
            {
                _idle.frame.screenshot = Path.GetFullPath(OUTPUT + "32-11-idle-level" + _stage.ToString("00") + "-" + Screen.width + "x" + Screen.height + "-" + CurrentIdle.captureRun + ".png");
                Directory.CreateDirectory(OUTPUT);
                ScreenCapture.CaptureScreenshot(_idle.frame.screenshot);
                _captureFrame = Time.frameCount;
                _captureWaitStarted = EditorApplication.timeSinceStartup;
            }
            else NextIdle();
            Write("32-11-idle-survey.json", CurrentIdle);
        }
        private static void NextIdle()
        {
            _stage++;
            if (_stage > 30) Finish("passed", "All 30 layouts remained held and motionless for two real physics seconds each.");
            else LoadIdleLevel();
        }

        public static FrameMetrics CaptureFrameMetrics()
        {
            GuardScene(true);
            var loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            var camera = Camera.main;
            if (loader == null || camera == null || LevelManager.Instance == null) throw new InvalidOperationException("Runtime level/camera missing.");
            var frame = new FrameMetrics { level = LevelManager.Instance.CurrentLevelIndex, width = Screen.width, height = Screen.height,
                safeArea = Screen.safeArea, targetCount = loader.SpawnedObjects.Count, standCount = loader.SpawnedStands.Count };
            var targetBounds = new List<Bounds>(); var standBounds = new List<Bounds>();
            foreach (var target in loader.SpawnedObjects) if (target != null) AddRendererBounds(target.gameObject, targetBounds);
            foreach (var stand in loader.SpawnedStands) if (stand != null) AddRendererBounds(stand, standBounds);
            frame.targetViewport = Project(camera, targetBounds, camera.aspect);
            frame.standViewport = Project(camera, standBounds, camera.aspect);
            foreach (var text in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                if (!text.isActiveAndEnabled) continue;
                var canvas = text.canvas; var corners = new Vector3[4]; text.rectTransform.GetWorldCorners(corners);
                Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity), max = -min;
                foreach (var corner in corners)
                {
                    Vector2 p = RectTransformUtility.WorldToScreenPoint(canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null, corner);
                    min = Vector2.Min(min, p); max = Vector2.Max(max, p);
                }
                frame.hud.Add(new HudItem { path = HierarchyPath(text.transform), text = text.text, screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y) });
            }
            return frame;
        }

        public static void BeginSkillSmoke()
        {
            Prepare("skills"); RequireNoProjectilesOrSkills();
            if (_skills == null || !_skills.IsSkillUnlocked(SkillType.MegaBall) || !_skills.IsSkillUnlocked(SkillType.InfinityBall) ||
                _skills.GetRemainingUses(SkillType.MegaBall) < 1 || _skills.GetRemainingUses(SkillType.InfinityBall) < 1 || _mortar.RemainingBalls < 6)
                throw new InvalidOperationException("Requires unlocked skills, one saved use of each and at least six balls; no currency or uses are granted by this helper.");
            var config = new SerializedObject(_mortar).FindProperty("_projectileConfig").objectReferenceValue as ProjectileConfig;
            CurrentSkills = new SkillReport { startedUtc = DateTime.UtcNow.ToString("O"), ballsBefore = _mortar.RemainingBalls,
                firesBefore = _mortar.FireEventCount, megaUsesBefore = _skills.GetRemainingUses(SkillType.MegaBall), infinityUsesBefore = _skills.GetRemainingUses(SkillType.InfinityBall),
                configuredNormalCooldown = config != null ? config.FireCooldown : .5f,
                configuredRapidCooldown = _skills.Config != null ? _skills.Config.InfinityBallRapidCooldown : .06f,
                configuredInfinityDuration = _skills.Config != null ? _skills.Config.InfinityBallDuration : 5f };
            _skillGate = _loader.GetComponent<LevelStartPhysicsGate>();
            if (_skillGate == null || !_skillGate.IsWaitingForFirstShot) throw new InvalidOperationException("Skill miss check requires a fresh held puzzle.");
            _stage = 0; StartJob();
        }
        private static void SkillTick()
        {
            CurrentSkills.missGateHeld &= _skillGate != null && _skillGate.IsWaitingForFirstShot;
            if (!CurrentSkills.missGateHeld) throw new InvalidOperationException("The deliberate skill miss released the puzzle gate or the puzzle changed.");
            if (_stage == 5)
            {
                CurrentSkills.infinityElapsed = Time.time - _phaseStart;
                if (_skills.IsSkillActive(SkillType.InfinityBall)) return;
                if (CurrentSkills.infinityElapsed < CurrentSkills.configuredInfinityDuration - .08f)
                    throw new InvalidOperationException("Infinity deactivated before its configured duration elapsed.");
                CurrentSkills.infinityExpired = true;
                bool uses = _skills.GetRemainingUses(SkillType.MegaBall) == CurrentSkills.megaUsesBefore - 1 &&
                    _skills.GetRemainingUses(SkillType.InfinityBall) == CurrentSkills.infinityUsesBefore - 1;
                Finish(uses ? "passed" : "failed", uses ? "Normal/Mega/three rapid shots verified; Infinity expired and each skill consumed exactly one use." : "Skill use accounting mismatch.");
                return;
            }
            if (_mortar.CooldownTimer > 0f) return;
            if (_stage == 1 && !_skills.ActivateSkill(SkillType.MegaBall)) throw new InvalidOperationException("Mega activation failed.");
            if (_stage == 2)
            {
                if (!_skills.ActivateSkill(SkillType.InfinityBall)) throw new InvalidOperationException("Infinity activation failed.");
                _phaseStart = Time.time;
            }
            var known = new HashSet<int>();
            foreach (var projectile in UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None)) known.Add(projectile.GetInstanceID());
            int fires = _mortar.FireEventCount, balls = _mortar.RemainingBalls;
            _mortar.Fire(new Vector3(12f, 8f, 8f));
            ProjectileController created = null;
            foreach (var projectile in UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None))
                if (!known.Contains(projectile.GetInstanceID())) { if (created != null) throw new InvalidOperationException("One shot created multiple projectiles."); created = projectile; }
            if (created == null || _mortar.FireEventCount != fires + 1 || _mortar.RemainingBalls != balls - 1) throw new InvalidOperationException("Projectile/ammunition event mismatch.");
            var renderer = new SerializedObject(created).FindProperty("_renderer").objectReferenceValue as Renderer;
            var material = renderer != null ? renderer.sharedMaterial : null;
            if (material == null || !material.HasProperty("_BaseColor")) throw new InvalidOperationException("Projectile primary renderer/BaseColor is missing.");
            var shot = new ShotEvidence { kind = _stage == 0 ? "Normal" : _stage == 1 ? "Mega" : "Infinity",
                mega = created.IsMegaBall, scale = created.transform.localScale, mass = created.Rigidbody.mass, cooldown = _mortar.CooldownTimer,
                time = Time.time, primaryRenderer = renderer.name, primaryEnabled = renderer.enabled, shader = material.shader.name, baseColor = material.GetColor("_BaseColor") };
            CurrentSkills.shots.Add(shot);
            if (_stage == 1)
            {
                var normal = CurrentSkills.shots[0]; float diameter = _skills.Config != null ? _skills.Config.MegaBallDiameterMultiplier : 2f;
                float mass = _skills.Config != null ? _skills.Config.MegaBallMassMultiplier : 2.5f;
                if (!shot.mega || Vector3.Distance(shot.scale, normal.scale * diameter) > .001f || Mathf.Abs(shot.mass - normal.mass * mass) > .001f ||
                    Vector4.Distance(shot.baseColor, new Color(1f, .65f, .1f, 1f)) > .01f) throw new InvalidOperationException("Mega scale/mass/runtime gold tint mismatch.");
                CurrentSkills.megaConsumed = !_skills.IsSkillActive(SkillType.MegaBall);
                if (!CurrentSkills.megaConsumed) throw new InvalidOperationException("Mega buff was not consumed by its shot.");
            }
            else if (shot.mega) throw new InvalidOperationException("Unexpected Mega flag on a normal/Infinity projectile.");
            float expectedCooldown = _stage >= 2 ? CurrentSkills.configuredRapidCooldown : CurrentSkills.configuredNormalCooldown;
            if (Mathf.Abs(shot.cooldown - expectedCooldown) > .002f) throw new InvalidOperationException("Shot cooldown does not match the active skill/configuration.");
            _stage++;
            Write("32-11-skills.json", CurrentSkills);
        }

        public static void BeginDestructionCheck()
        {
            Prepare("destruction"); RequireNoProjectilesOrSkills();
            if (_manager.RemainingTargets < 2 || _loader.SpawnedObjects.Count < 2) throw new InvalidOperationException("Requires at least two targets to avoid deliberately completing the level.");
            var gate = _loader.GetComponent<LevelStartPhysicsGate>();
            if (gate != null && gate.IsWaitingForFirstShot)
                throw new InvalidOperationException("Run destruction verification after the separate real-impact smoke has released the gate and its projectiles are gone.");
            _destroyTarget = null;
            foreach (var candidate in _loader.SpawnedObjects)
                if (candidate != null && candidate.gameObject.activeInHierarchy && (int)candidate.CurrentState < (int)ObjectState.DestroyCondition &&
                    candidate.Rigidbody != null && !candidate.Rigidbody.isKinematic && !(candidate is PaintBottleObject))
                { _destroyTarget = candidate; break; }
            if (_destroyTarget == null) throw new InvalidOperationException("No surviving dynamic plain target is available for the lifecycle check.");
            // PhysicsObject._renderer is protected and not serialized; inspect the actual
            // runtime cache rather than choosing a possibly different decorative renderer.
            var primaryField = typeof(PhysicsObject).GetField("_renderer", BindingFlags.Instance | BindingFlags.NonPublic);
            _destroyRenderer = primaryField != null ? primaryField.GetValue(_destroyTarget) as Renderer : null;
            if (_destroyRenderer == null || _destroyRenderer.sharedMaterial == null || !_destroyRenderer.sharedMaterial.HasProperty("_BaseColor"))
                throw new InvalidOperationException("Target's actual primary renderer/BaseColor is unavailable.");
            float alpha = _destroyRenderer.sharedMaterial.GetColor("_BaseColor").a;
            CurrentDestruction = new DestructionReport { startedUtc = DateTime.UtcNow.ToString("O"), target = _destroyTarget.name,
                primaryRenderer = _destroyRenderer.name, primaryEnabled = _destroyRenderer.enabled, primaryShader = _destroyRenderer.sharedMaterial.shader.name,
                initialScale = _destroyTarget.transform.localScale, minimumScale = _destroyTarget.transform.localScale, initialAlpha = alpha, minimumAlpha = alpha };
            StartJob();
            try { _destroyTarget.TriggerDestroyCondition("Phase32 explicit lifecycle verification"); }
            catch (Exception error) { Finish("failed", error.ToString()); }
        }
        private static void DestructionTick()
        {
            if (_destroyTarget == null)
            {
                CurrentDestruction.removed = true;
                bool passed = CurrentDestruction.scaleDecreased && CurrentDestruction.alphaDecreased && CurrentDestruction.colliderDisabled && CurrentDestruction.kinematic;
                Finish(passed ? "passed" : "failed", passed ? "Target shrank, primary BaseColor alpha decreased, physics disabled and object was removed." : "Object removal occurred without all sampled lifecycle conditions.");
                return;
            }
            Vector3 scale = _destroyTarget.transform.localScale;
            CurrentDestruction.minimumScale = Vector3.Min(CurrentDestruction.minimumScale, scale);
            CurrentDestruction.scaleDecreased |= scale.magnitude < CurrentDestruction.initialScale.magnitude * .95f;
            CurrentDestruction.colliderDisabled |= _destroyTarget.Collider != null && !_destroyTarget.Collider.enabled;
            CurrentDestruction.kinematic |= _destroyTarget.Rigidbody != null && _destroyTarget.Rigidbody.isKinematic;
            if (_destroyRenderer != null && _destroyRenderer.sharedMaterial != null)
            {
                float alpha = _destroyRenderer.sharedMaterial.GetColor("_BaseColor").a;
                CurrentDestruction.minimumAlpha = Mathf.Min(CurrentDestruction.minimumAlpha, alpha);
                CurrentDestruction.alphaDecreased |= alpha < CurrentDestruction.initialAlpha - .05f;
            }
        }

        private static void Prepare(string job)
        {
            GuardScene(true);
            if (Status == "running") throw new InvalidOperationException("A final verification job is already running.");
            if (Time.timeScale <= 0f || EditorApplication.isPaused) throw new InvalidOperationException("Unpause gameplay before testing.");
            _manager = LevelManager.Instance; _loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            _mortar = UnityEngine.Object.FindFirstObjectByType<MortarController>();
            _skills = SkillManager.HasInstance ? SkillManager.Instance : null;
            if (_manager == null || _loader == null || _mortar == null) throw new InvalidOperationException("Runtime level/loader/mortar missing.");
            if (_manager.CurrentState != LevelState.Playing) throw new InvalidOperationException("Requires a fresh playing level, not a victory/defeat state.");
            _job = job;
        }
        private static void RequireNoProjectilesOrSkills()
        {
            if (UnityEngine.Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None).Length != 0 ||
                (_skills != null && (_skills.IsSkillActive(SkillType.MegaBall) || _skills.IsSkillActive(SkillType.InfinityBall))))
                throw new InvalidOperationException("Use a fresh level with no projectiles or active skills.");
        }
        private static void StartJob()
        {
            Status = "running"; _wallStart = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayState;
            AssemblyReloadEvents.beforeAssemblyReload += OnReload;
        }
        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().path != SCENE || _manager == null || _loader == null || _mortar == null)
                { Finish("interrupted", "Play Mode or required scene state changed."); return; }
                if (EditorApplication.timeSinceStartup - _wallStart > (_job == "idle" ? 180d : 30d))
                { Finish("failed", "Timed out waiting for real gameplay frames/state."); return; }
                if (_job == "idle") IdleTick(); else if (_job == "skills") SkillTick(); else DestructionTick();
            }
            catch (Exception error) { Finish("failed", error.ToString()); }
        }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) Finish("interrupted", "Play Mode ended.");
        }
        private static void OnReload() => Finish("interrupted", "Assembly reload interrupted verification.");
        public static void Cancel() => Finish("interrupted", "Cancelled explicitly.");
        private static void Finish(string status, string reason)
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayState; AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
            if (Status != "running") return;
            Status = status;
            Result result = _job == "idle" ? (Result)CurrentIdle : _job == "skills" ? (Result)CurrentSkills : CurrentDestruction;
            if (_job == "idle" && _idle != null && !CurrentIdle.levels.Contains(_idle)) CurrentIdle.levels.Add(_idle);
            if (_job == "idle" && Application.isPlaying && SceneManager.GetActiveScene().path == SCENE && _manager != null && status != "interrupted")
            {
                try { _manager.LoadLevelDirect(CurrentIdle.originalLevel); }
                catch (Exception error) { status = Status = "failed"; reason += " Original level restoration failed: " + error.Message; }
            }
            if (_job == "skills")
            {
                CurrentSkills.ballsAfter = _mortar != null ? _mortar.RemainingBalls : -1; CurrentSkills.firesAfter = _mortar != null ? _mortar.FireEventCount : -1;
                CurrentSkills.megaUsesAfter = _skills != null ? _skills.GetRemainingUses(SkillType.MegaBall) : -1;
                CurrentSkills.infinityUsesAfter = _skills != null ? _skills.GetRemainingUses(SkillType.InfinityBall) : -1;
                if (status == "passed" && (CurrentSkills.ballsAfter != CurrentSkills.ballsBefore - 5 || CurrentSkills.firesAfter != CurrentSkills.firesBefore + 5))
                { status = Status = "failed"; reason = "Total shot/ammunition count mismatch."; }
            }
            result.status = status; result.reason = reason; result.passed = status == "passed"; result.finishedUtc = DateTime.UtcNow.ToString("O");
            Write(_job == "idle" ? "32-11-idle-survey.json" : _job == "skills" ? "32-11-skills.json" : "32-11-destruction.json", result);
            Debug.Log("[Phase32FinalVerification] " + _job + " " + status + ": " + reason);
        }

        private static Bounds ColliderBounds(GameObject prefab, Matrix4x4 placement)
        {
            bool found = false; Bounds result = default;
            foreach (var collider in prefab.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.enabled) continue;
                Bounds local;
                if (collider is BoxCollider) { var box = (BoxCollider)collider; local = new Bounds(box.center, box.size); }
                else if (collider is SphereCollider) { var sphere = (SphereCollider)collider; local = new Bounds(sphere.center, Vector3.one * sphere.radius * 2f); }
                else if (collider is CapsuleCollider)
                {
                    var capsule = (CapsuleCollider)collider; Vector3 size = Vector3.one * capsule.radius * 2f; size[capsule.direction] = Mathf.Max(capsule.height, size[capsule.direction]); local = new Bounds(capsule.center, size);
                }
                else if (collider is MeshCollider && ((MeshCollider)collider).sharedMesh != null) local = ((MeshCollider)collider).sharedMesh.bounds;
                else throw new InvalidOperationException("Unsupported collider " + collider.GetType().Name);
                Matrix4x4 matrix = placement * prefab.transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;
                foreach (var corner in Corners(local)) { Vector3 p = matrix.MultiplyPoint3x4(corner); if (!found) { result = new Bounds(p, Vector3.zero); found = true; } else result.Encapsulate(p); }
            }
            if (!found) throw new InvalidOperationException("No supported enabled collider on " + prefab.name);
            return result;
        }
        private static Rect Project(Camera camera, List<Bounds> bounds, float aspect)
        {
            if (bounds.Count == 0) return new Rect();
            var framing = camera.GetComponent<PortraitSceneFraming>();
            float fov = framing != null && framing.enabled ? framing.FieldOfViewForAspect(aspect) : camera.fieldOfView;
            Matrix4x4 projection = camera.orthographic ? Matrix4x4.Ortho(-camera.orthographicSize * aspect, camera.orthographicSize * aspect,
                -camera.orthographicSize, camera.orthographicSize, camera.nearClipPlane, camera.farClipPlane) : Matrix4x4.Perspective(fov, aspect, camera.nearClipPlane, camera.farClipPlane);
            Matrix4x4 matrix = projection * camera.worldToCameraMatrix;
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity), max = -min;
            foreach (var item in bounds) foreach (var corner in Corners(item))
            {
                Vector4 clip = matrix * new Vector4(corner.x, corner.y, corner.z, 1f);
                if (clip.w <= 0) return new Rect(-100, -100, 200, 200);
                var p = new Vector2(clip.x / clip.w * .5f + .5f, clip.y / clip.w * .5f + .5f);
                min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private static IEnumerable<Vector3> Corners(Bounds bounds)
        {
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                yield return bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
        }
        private static void AddRendererBounds(GameObject root, List<Bounds> output)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>()) if (renderer.enabled && !(renderer is TrailRenderer) && !(renderer is ParticleSystemRenderer)) output.Add(renderer.bounds);
        }
        private static bool Inside(Rect rect) => rect.xMin >= 0f && rect.xMax <= 1f && rect.yMin >= 0f && rect.yMax <= 1f;
        private static string HierarchyPath(Transform transform) => transform.parent == null ? transform.name : HierarchyPath(transform.parent) + "/" + transform.name;
        private static void GuardScene(bool play)
        {
            if (SceneManager.GetActiveScene().path != SCENE || Application.isPlaying != play) throw new InvalidOperationException("Requires MainGame " + (play ? "Play Mode" : "Edit Mode") + ".");
        }
        private static void Write(string name, object report) { Directory.CreateDirectory(OUTPUT); File.WriteAllText(OUTPUT + name, JsonUtility.ToJson(report, true)); }
    }
}
