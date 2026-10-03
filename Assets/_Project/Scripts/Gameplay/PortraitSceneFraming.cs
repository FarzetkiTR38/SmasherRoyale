using UnityEngine;
using System.Collections.Generic;
using RoyalSmash.Levels;

namespace RoyalSmash.Gameplay
{
    /// <summary>Keeps the authored portrait width and spawned level scenery visible on taller phones.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class PortraitSceneFraming : MonoBehaviour
    {
        private const float HORIZONTAL_VIEWPORT_EXTENT = .92f;

        [SerializeField, Range(1f, 120f)] private float _referenceVerticalFov = 56f;
        [SerializeField] private float _referenceAspect = 9f / 16f;
        [SerializeField] private Transform _backdrop;

        private Camera _camera;
        private LevelLoader _levelLoader;
        private readonly List<Bounds> _spawnedBounds = new List<Bounds>();
        private readonly List<MeshRenderer> _renderers = new List<MeshRenderer>();
        private bool _referencesCached;
        private bool _originalStateCached;
        private bool _backdropStateCached;
        private float _lastAspect = -1f;
        private float _originalFov;
        private float _requiredHorizontalTangent;
        private int _standCount = -1, _targetCount = -1;
        private int _firstStandId, _firstTargetId;
        private Vector3 _backdropScale, _backdropPosition, _backdropHalfHeight;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (!_referencesCached)
            {
                _camera = GetComponent<Camera>();
                _levelLoader = FindFirstObjectByType<LevelLoader>();
                _referencesCached = true;
            }
            if (_camera == null) return;
            _originalFov = _camera.fieldOfView;
            _backdropStateCached = _backdrop != null;
            if (_backdropStateCached)
            {
                _backdropScale = _backdrop.localScale;
                _backdropPosition = _backdrop.position;
                var filter = _backdrop.GetComponent<MeshFilter>();
                float halfHeight = filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds.extents.y : .5f;
                _backdropHalfHeight = _backdrop.TransformVector(Vector3.up * halfHeight);
            }
            _originalStateCached = true;
            _standCount = _targetCount = -1;
            _lastAspect = -1f;
            LateUpdate();
        }

        private void LateUpdate()
        {
            if (!_originalStateCached || _camera == null) return;
            bool levelChanged = RefreshSpawnBoundsIfChanged();
            float aspect = _camera.aspect;
            if (!levelChanged && Mathf.Approximately(aspect, _lastAspect)) return;
            _lastAspect = aspect;
            _camera.fieldOfView = FieldOfViewForAspect(aspect);
            if (!_backdropStateCached || _backdrop == null) return;
            float heightRatio = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * .5f)
                / Mathf.Tan(_referenceVerticalFov * Mathf.Deg2Rad * .5f);
            float cover = heightRatio * Mathf.Max(1f, aspect / Mathf.Max(.1f, _referenceAspect));
            _backdrop.localScale = _backdropScale * cover;
            // Keep the existing floor/background seam fixed while covering the extra sky.
            _backdrop.position = _backdropPosition + _backdropHalfHeight * (cover - 1f);
        }

        private void OnDisable()
        {
            if (!_originalStateCached) return;
            _originalStateCached = false;
            if (_camera != null) _camera.fieldOfView = _originalFov;
            if (!_backdropStateCached || _backdrop == null) return;
            _backdrop.localScale = _backdropScale;
            _backdrop.position = _backdropPosition;
        }

        /// <summary>Combines the authored portrait framing with the cached or supplied level width.</summary>
        public float FieldOfViewForAspect(float aspect, float requiredHorizontalTangent = -1f)
        {
            float safeAspect = Mathf.Max(.1f, aspect);
            float referenceTangent = Mathf.Tan(_referenceVerticalFov * Mathf.Deg2Rad * .5f);
            float verticalTangent = referenceTangent * Mathf.Max(1f, _referenceAspect / safeAspect);
            if (requiredHorizontalTangent < 0f) requiredHorizontalTangent = _requiredHorizontalTangent;
            verticalTangent = Mathf.Max(verticalTangent, requiredHorizontalTangent / safeAspect);
            return 2f * Mathf.Atan(verticalTangent) * Mathf.Rad2Deg;
        }

        /// <summary>Finds the horizontal half-FOV tangent needed to keep front-facing bounds within x=.04..96.</summary>
        public static float CalculateRequiredHorizontalTangent(Camera camera, IEnumerable<Bounds> bounds)
        {
            if (camera == null || bounds == null) return 0f;
            Matrix4x4 worldToCamera = camera.worldToCameraMatrix;
            float requiredTangent = 0f;
            foreach (Bounds bound in bounds)
            {
                Vector3 min = bound.min;
                Vector3 max = bound.max;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 worldPoint = new Vector3(
                        (corner & 1) == 0 ? min.x : max.x,
                        (corner & 2) == 0 ? min.y : max.y,
                        (corner & 4) == 0 ? min.z : max.z);
                    Vector3 cameraPoint = worldToCamera.MultiplyPoint3x4(worldPoint);
                    float depth = -cameraPoint.z;
                    if (depth <= 0f) continue;
                    requiredTangent = Mathf.Max(requiredTangent, Mathf.Abs(cameraPoint.x) / depth);
                }
            }
            return requiredTangent / HORIZONTAL_VIEWPORT_EXTENT;
        }

        private bool RefreshSpawnBoundsIfChanged()
        {
            int standCount = _levelLoader != null ? _levelLoader.SpawnedStands.Count : 0;
            int targetCount = _levelLoader != null ? _levelLoader.SpawnedObjects.Count : 0;
            int firstStandId = standCount > 0 ? GetSpawnInstanceId(_levelLoader.SpawnedStands[0]) : 0;
            int firstTargetId = targetCount > 0 ? GetSpawnInstanceId(_levelLoader.SpawnedObjects[0]) : 0;
            if (standCount == _standCount && targetCount == _targetCount
                && firstStandId == _firstStandId && firstTargetId == _firstTargetId) return false;

            _standCount = standCount;
            _targetCount = targetCount;
            _firstStandId = firstStandId;
            _firstTargetId = firstTargetId;
            _spawnedBounds.Clear();
            for (int i = 0; i < standCount; i++)
                AddEnabledRendererBounds(_levelLoader.SpawnedStands[i]);
            for (int i = 0; i < targetCount; i++)
            {
                PhysicsObject target = _levelLoader.SpawnedObjects[i];
                if (target != null) AddEnabledRendererBounds(target.gameObject);
            }
            _requiredHorizontalTangent = CalculateRequiredHorizontalTangent(_camera, _spawnedBounds);
            return true;
        }

        private void AddEnabledRendererBounds(GameObject root)
        {
            if (root == null) return;
            _renderers.Clear();
            root.GetComponentsInChildren<MeshRenderer>(false, _renderers);
            for (int i = 0; i < _renderers.Count; i++)
            {
                MeshRenderer renderer = _renderers[i];
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                    _spawnedBounds.Add(renderer.bounds);
            }
        }

        private static int GetSpawnInstanceId(Object spawned)
        {
            // Loader lists retain destroyed targets until reload. Their managed identity stays
            // stable, so clearing the first target during a shot must not refit moving blocks.
            return ReferenceEquals(spawned, null) ? 0 : spawned.GetInstanceID();
        }
    }
}
