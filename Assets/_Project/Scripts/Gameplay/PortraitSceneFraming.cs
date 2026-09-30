using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>Keeps the authored portrait width visible on taller phones.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class PortraitSceneFraming : MonoBehaviour
    {
        [SerializeField, Range(1f, 120f)] private float _referenceVerticalFov = 56f;
        [SerializeField] private float _referenceAspect = 9f / 16f;
        [SerializeField] private Transform _backdrop;

        private Camera _camera;
        private float _lastAspect = -1f;
        private float _originalFov;
        private Vector3 _backdropScale, _backdropPosition, _backdropHalfHeight;

        public float FieldOfViewForAspect(float aspect)
        {
            float ratio = Mathf.Max(1f, _referenceAspect / Mathf.Max(.1f, aspect));
            return 2f * Mathf.Atan(Mathf.Tan(_referenceVerticalFov * Mathf.Deg2Rad * .5f) * ratio) * Mathf.Rad2Deg;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            _camera = GetComponent<Camera>();
            _originalFov = _camera.fieldOfView;
            if (_backdrop != null)
            {
                _backdropScale = _backdrop.localScale;
                _backdropPosition = _backdrop.position;
                var filter = _backdrop.GetComponent<MeshFilter>();
                float halfHeight = filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds.extents.y : .5f;
                _backdropHalfHeight = _backdrop.TransformVector(Vector3.up * halfHeight);
            }
            _lastAspect = -1f;
            LateUpdate();
        }

        private void LateUpdate()
        {
            float aspect = _camera.aspect;
            if (Mathf.Approximately(aspect, _lastAspect)) return;
            _lastAspect = aspect;
            _camera.fieldOfView = FieldOfViewForAspect(aspect);
            if (_backdrop == null) return;
            float heightRatio = Mathf.Max(1f, _referenceAspect / Mathf.Max(.1f, aspect));
            float cover = Mathf.Max(heightRatio, aspect / _referenceAspect);
            _backdrop.localScale = _backdropScale * cover;
            // Keep the existing floor/background seam fixed while covering the extra sky.
            _backdrop.position = _backdropPosition + _backdropHalfHeight * (cover - 1f);
        }

        private void OnDisable()
        {
            if (_camera == null) return;
            _camera.fieldOfView = _originalFov;
            if (_backdrop == null) return;
            _backdrop.localScale = _backdropScale;
            _backdrop.position = _backdropPosition;
        }
    }
}
