using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Predicts and renders the parabolic ballistic trajectory of cannon balls in real-time.
    /// Uses the exact analytical kinematic physics equation matching ProjectileController.Launch.
    /// Detects obstacle/block impacts, clips the arc accurately, and renders a target hit marker.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MortarController _mortar;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private ProjectileConfig _projectileConfig;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private GameObject _hitMarker;

        [Header("Trajectory Settings")]
        [Range(15, 60)]
        [SerializeField] private int _resolution = 32;
        [SerializeField] private float _lineWidth = 0.08f;
        [SerializeField] private float _taperEndWidth = 0.04f;
        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private float _scrollSpeed = 2.5f;

        [Header("Colors & Aesthetics")]
        [SerializeField] private Color _startColor = new Color(0.18f, 0.84f, 1.0f, 0.95f); // Royal Cyan
        [SerializeField] private Color _endColor = new Color(1.0f, 0.82f, 0.22f, 0.65f);   // Warm Gold

        [Header("Display Settings")]
        [SerializeField] private bool _showTrajectoryLine = false;

        // Runtime state
        private bool _isAiming;
        private Vector3 _currentTarget;
        private Material _lineMaterial;
        private float _textureOffset;
        private Coroutine _fadeCoroutine;
        private float _currentAlpha = 0f;

        public bool ShowTrajectoryLine
        {
            get => _showTrajectoryLine;
            set
            {
                _showTrajectoryLine = value;
                if (!_showTrajectoryLine) SetTrajectoryVisible(false, instant: true);
            }
        }
        public bool IsAiming => _isAiming;
        public int PointCount => _lineRenderer != null ? _lineRenderer.positionCount : 0;
        public Vector3 CurrentTarget => _currentTarget;

        private void Awake()
        {
            if (_mortar == null) _mortar = GetComponentInParent<MortarController>();
            if (_mortar == null) _mortar = FindFirstObjectByType<MortarController>();

            if (_inputHandler == null) _inputHandler = FindFirstObjectByType<InputHandler>();

            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
            ConfigureLineRenderer();

            CreateDefaultHitMarkerIfNeeded();

            if (!_showTrajectoryLine)
            {
                if (_lineRenderer != null) _lineRenderer.enabled = false;
                if (_hitMarker != null) _hitMarker.SetActive(false);
            }
        }

        private void Start()
        {
            RegisterEvents();
            SetTrajectoryVisible(false, instant: true);
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
            SetTrajectoryVisible(false, instant: true);
        }

        private void ConfigureLineRenderer()
        {
            if (_lineRenderer == null) return;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _taperEndWidth;
            _lineRenderer.positionCount = 0;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;

            // Create smooth unlit shader material if needed
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader != null)
            {
                _lineMaterial = new Material(unlitShader);
                _lineMaterial.name = "M_Trajectory_Instance";
                _lineRenderer.material = _lineMaterial;
            }

            UpdateGradient(1.0f);
        }

        private void UpdateGradient(float alphaMultiplier)
        {
            if (_lineRenderer == null) return;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(_startColor, 0.0f),
                    new GradientColorKey(_endColor, 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(_startColor.a * alphaMultiplier, 0.0f),
                    new GradientAlphaKey(_endColor.a * alphaMultiplier, 1.0f)
                }
            );
            _lineRenderer.colorGradient = gradient;
        }

        private void CreateDefaultHitMarkerIfNeeded()
        {
            if (_hitMarker != null) return;

            // Generate clean circular indicator disk
            _hitMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _hitMarker.name = "Trajectory_HitMarker";
            _hitMarker.transform.SetParent(transform);
            _hitMarker.transform.localScale = new Vector3(0.45f, 0.015f, 0.45f);

            var col = _hitMarker.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var ren = _hitMarker.GetComponent<Renderer>();
            if (ren != null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    var mat = new Material(unlitShader);
                    mat.color = new Color(1.0f, 0.85f, 0.25f, 0.85f);
                    ren.material = mat;
                }
            }

            _hitMarker.SetActive(false);
        }

        private void RegisterEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnAimTargetChanged += HandleAimTargetChanged;
                _inputHandler.OnDragStarted -= HandleDragStarted;
                _inputHandler.OnDragStarted += HandleDragStarted;
                _inputHandler.OnDragEnded -= HandleDragEnded;
                _inputHandler.OnDragEnded += HandleDragEnded;
                _inputHandler.OnTapFired -= HandleTapFired;
                _inputHandler.OnTapFired += HandleTapFired;
            }
        }

        private void UnregisterEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnDragStarted -= HandleDragStarted;
                _inputHandler.OnDragEnded -= HandleDragEnded;
                _inputHandler.OnTapFired -= HandleTapFired;
            }
        }

        private void Update()
        {
            if (_isAiming)
            {
                // Animate trajectory material offset
                if (_lineMaterial != null)
                {
                    _textureOffset -= Time.deltaTime * _scrollSpeed;
                    _lineMaterial.mainTextureOffset = new Vector2(_textureOffset, 0f);
                }

                CalculateAndRenderArc(_currentTarget);
            }
        }

        private void HandleAimTargetChanged(Vector3 worldTarget)
        {
            _currentTarget = worldTarget;
            if (!_showTrajectoryLine) return;

            // Don't show if out of ammo or lives
            if (_mortar != null && _mortar.RemainingBalls <= 0) return;
            if (RoyalSmash.Lives.LivesManager.Instance != null && !RoyalSmash.Lives.LivesManager.Instance.HasLivesToPlay) return;

            SetTrajectoryVisible(true);
            CalculateAndRenderArc(_currentTarget);
        }

        private void HandleDragStarted()
        {
            if (!_showTrajectoryLine) return;
            if (_mortar != null && _mortar.RemainingBalls <= 0) return;
            SetTrajectoryVisible(true);
        }

        private void HandleDragEnded()
        {
            SetTrajectoryVisible(false);
        }

        private void HandleTapFired(Vector3 worldTarget)
        {
            SetTrajectoryVisible(false);
        }

        /// <summary>
        /// Analytical calculation and rendering of ballistic parabolic path.
        /// Matches ProjectileController.Launch physics kinematics 1:1.
        /// </summary>
        public void CalculateAndRenderArc(Vector3 targetPoint)
        {
            if (!_showTrajectoryLine) return;
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null) return;

            Transform originTransform = _firePoint;
            if (originTransform == null && _mortar != null) originTransform = _mortar.FirePoint;
            if (originTransform == null) originTransform = transform;

            Vector3 origin = originTransform.position;
            Vector3 displacement = targetPoint - origin;
            Vector3 displacementXZ = new Vector3(displacement.x, 0f, displacement.z);
            float distanceXZ = displacementXZ.magnitude;

            float launchSpeed = _projectileConfig != null ? _projectileConfig.LaunchSpeed : 22f;
            if (distanceXZ < 0.1f) distanceXZ = 0.1f;

            float timeToTarget = distanceXZ / launchSpeed;
            float gravity = Mathf.Abs(Physics.gravity.y);

            // Exact kinematic formula from ProjectileController
            float vy = (displacement.y + 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;
            Vector3 initialVelocity = (displacementXZ.normalized * launchSpeed) + (Vector3.up * vy);

            List<Vector3> points = new List<Vector3>(_resolution + 2);
            points.Add(origin);

            Vector3 prevPoint = origin;
            bool hitDetected = false;
            Vector3 hitPosition = targetPoint;
            Vector3 hitNormal = Vector3.up;

            float dt = timeToTarget / _resolution;
            // Extend prediction slightly past target to show drop if missed
            int totalSteps = Mathf.CeilToInt(_resolution * 1.15f);

            for (int i = 1; i <= totalSteps; i++)
            {
                float t = i * dt;
                Vector3 currentPoint = origin + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);

                // Raycast/Linecast along segment to detect obstacles
                Vector3 segment = currentPoint - prevPoint;
                float segLength = segment.magnitude;
                if (segLength > 0.001f)
                {
                    if (Physics.Linecast(prevPoint, currentPoint, out RaycastHit hit, _collisionMask, QueryTriggerInteraction.Ignore) ||
                        Physics.SphereCast(prevPoint, 0.08f, segment.normalized, out hit, segLength, _collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        // Ignore mortar itself or ball layer
                        if (hit.collider.gameObject != gameObject && hit.collider.gameObject.layer != 6)
                        {
                            hitDetected = true;
                            hitPosition = hit.point;
                            hitNormal = hit.normal;
                            points.Add(hitPosition);
                            break;
                        }
                    }
                }

                points.Add(currentPoint);
                prevPoint = currentPoint;

                // Stop if point dips below ground
                if (currentPoint.y < -1.0f) break;
            }

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());

            // Position Hit Marker
            if (_hitMarker != null)
            {
                if (hitDetected)
                {
                    _hitMarker.SetActive(true);
                    _hitMarker.transform.position = hitPosition + (hitNormal * 0.02f);
                    if (hitNormal != Vector3.zero)
                    {
                        _hitMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);
                    }
                }
                else
                {
                    _hitMarker.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Enables or smoothly fades out the trajectory line.
        /// </summary>
        public void SetTrajectoryVisible(bool visible, bool instant = false)
        {
            if (!_showTrajectoryLine) visible = false;
            _isAiming = visible;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (instant)
            {
                _currentAlpha = visible ? 1.0f : 0.0f;
                UpdateGradient(_currentAlpha);
                if (_lineRenderer != null) _lineRenderer.enabled = visible;
                if (_hitMarker != null && !visible) _hitMarker.SetActive(false);
            }
            else
            {
                _fadeCoroutine = StartCoroutine(FadeRoutine(visible ? 1.0f : 0.0f, 0.12f));
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            if (_lineRenderer != null && targetAlpha > 0f)
            {
                _lineRenderer.enabled = true;
            }

            float startAlpha = _currentAlpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                UpdateGradient(_currentAlpha);
                yield return null;
            }

            _currentAlpha = targetAlpha;
            UpdateGradient(_currentAlpha);

            if (_currentAlpha <= 0.01f)
            {
                if (_lineRenderer != null)
                {
                    _lineRenderer.positionCount = 0;
                    _lineRenderer.enabled = false;
                }
                if (_hitMarker != null)
                {
                    _hitMarker.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Directly binds references for testing or programmatic setup.
        /// </summary>
        public void BindReferences(MortarController mortar, InputHandler inputHandler, ProjectileConfig config, Transform firePoint, LineRenderer lineRenderer)
        {
            _mortar = mortar;
            _inputHandler = inputHandler;
            _projectileConfig = config;
            _firePoint = firePoint;
            _lineRenderer = lineRenderer != null ? lineRenderer : GetComponent<LineRenderer>();
            ConfigureLineRenderer();
            RegisterEvents();
        }
    }
}
