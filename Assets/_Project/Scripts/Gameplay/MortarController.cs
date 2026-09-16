using System;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Controls the mortar / cannon aiming, cooldown timing, ammunition tracking, and projectile launching.
    /// First ball is immediately ready (no initial cooldown).
    /// </summary>
    public class MortarController : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private MortarConfig _config;
        [SerializeField] private ProjectileConfig _projectileConfig;

        [Header("Projectile References")]
        [SerializeField] private GameObject _ballPrefab;
        [Tooltip("Transform that swivels horizontally to aim (Visuals or Turret).")]
        [SerializeField] private Transform _aimTransform;

        [Tooltip("Transform where balls spawn and launch.")]
        [SerializeField] private Transform _firePoint;

        [SerializeField] private InputHandler _inputHandler;

        // Aiming state
        private float _targetYaw;
        private float _currentYaw;
        private float _yawVelocity;
        private Vector3 _lastTargetPoint;

        // Ammunition and Cooldown state
        private int _remainingBalls;
        private float _cooldownTimer;
        private int _fireEventCount;

        // Events
        public event Action<int> OnBallCountChanged;
        public event Action OnBallFired;

        public float CurrentYaw => _currentYaw;
        public float TargetYaw => _targetYaw;
        public Transform FirePoint => _firePoint;
        public Vector3 LastTargetPoint => _lastTargetPoint;
        public int RemainingBalls => _remainingBalls;
        public float CooldownTimer => _cooldownTimer;
        public int FireEventCount => _fireEventCount;

        private void Awake()
        {
            if (_aimTransform == null)
            {
                var visuals = transform.Find("Visuals");
                if (visuals != null) _aimTransform = visuals;
            }

            if (_firePoint == null)
            {
                var foundFirePoint = transform.Find("Visuals/BarrelPivot/FirePoint");
                if (foundFirePoint != null) _firePoint = foundFirePoint;
            }

            // Initialize ammunition and cooldown
            _remainingBalls = _projectileConfig != null ? _projectileConfig.DefaultBallCount : 25;
            _cooldownTimer = 0f; // First ball of level has NO cooldown
        }

        private void Start()
        {
            if (_inputHandler == null)
            {
                _inputHandler = FindFirstObjectByType<InputHandler>();
            }

            RegisterInputEvents();
            OnBallCountChanged?.Invoke(_remainingBalls);
        }

        private void OnEnable()
        {
            RegisterInputEvents();
        }

        private void OnDisable()
        {
            UnregisterInputEvents();
        }

        private void RegisterInputEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnAimTargetChanged += HandleAimTargetChanged;
                _inputHandler.OnTapFired -= HandleTapFired;
                _inputHandler.OnTapFired += HandleTapFired;
            }
        }

        private void UnregisterInputEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnAimTargetChanged -= HandleAimTargetChanged;
                _inputHandler.OnTapFired -= HandleTapFired;
            }
        }

        private void Update()
        {
            UpdateAimRotation();
            UpdateCooldown();
        }

        private void UpdateAimRotation()
        {
            if (_aimTransform == null) return;

            float smoothTime = _config != null ? _config.SmoothTime : 0.05f;
            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, smoothTime);
            _aimTransform.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        private void UpdateCooldown()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer < 0f) _cooldownTimer = 0f;
            }
        }

        private void HandleAimTargetChanged(Vector3 worldTarget)
        {
            Vector3 direction = (worldTarget - transform.position).normalized;
            if (direction.sqrMagnitude < Mathf.Epsilon) return;

            float calculatedYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float minYaw = _config != null ? _config.MinYaw : -45f;
            float maxYaw = _config != null ? _config.MaxYaw : 45f;

            _targetYaw = Mathf.Clamp(calculatedYaw, minYaw, maxYaw);
        }

        private void HandleTapFired(Vector3 worldTarget)
        {
            // Check ammunition
            if (_remainingBalls <= 0)
            {
                Debug.Log("[MortarController] Cannot fire: No balls remaining!");
                return;
            }

            // Check cooldown
            if (_cooldownTimer > 0f)
            {
                Debug.Log($"[MortarController] Cooldown active ({_cooldownTimer:F2}s remaining), fire blocked.");
                return;
            }

            // Aim directly at target point
            HandleAimTargetChanged(worldTarget);
            _lastTargetPoint = worldTarget;

            // Fire Ball
            LaunchProjectile(worldTarget);

            // Reset cooldown (InfinityBall overrides standard cooldown with rapid stream)
            float cooldown = _projectileConfig != null ? _projectileConfig.FireCooldown : 0.5f;
            if (RoyalSmash.Skills.SkillManager.Instance != null && RoyalSmash.Skills.SkillManager.Instance.IsSkillActive(RoyalSmash.Skills.SkillType.InfinityBall))
            {
                float rapid = RoyalSmash.Skills.SkillManager.Instance.Config != null
                    ? RoyalSmash.Skills.SkillManager.Instance.Config.InfinityBallRapidCooldown
                    : 0.06f;
                cooldown = rapid;
            }
            _cooldownTimer = cooldown;

            // Decrement remaining ammunition
            _remainingBalls--;
            _fireEventCount++;

            OnBallCountChanged?.Invoke(_remainingBalls);
            OnBallFired?.Invoke();

            Debug.Log($"[MortarController] Ball fired toward {worldTarget}! Remaining: {_remainingBalls}");
        }

        /// <summary>
        /// Fires a ball directly toward the target world position.
        /// </summary>
        public void Fire(Vector3 worldTarget)
        {
            HandleTapFired(worldTarget);
        }

        private void LaunchProjectile(Vector3 targetPoint)
        {
            if (_ballPrefab == null)
            {
                Debug.LogWarning("[MortarController] Ball prefab is not assigned!");
                return;
            }

            Vector3 spawnPosition = _firePoint != null ? _firePoint.position : transform.position + Vector3.up;
            Quaternion spawnRotation = _firePoint != null ? _firePoint.rotation : Quaternion.identity;

            GameObject ballGo = Instantiate(_ballPrefab, spawnPosition, spawnRotation);
            var projectile = ballGo.GetComponent<ProjectileController>();

            if (projectile != null)
            {
                projectile.Launch(targetPoint, _projectileConfig);

                // Check MegaBall skill buff
                if (RoyalSmash.Skills.SkillManager.Instance != null && RoyalSmash.Skills.SkillManager.Instance.IsSkillActive(RoyalSmash.Skills.SkillType.MegaBall))
                {
                    float dMult = RoyalSmash.Skills.SkillManager.Instance.Config != null
                        ? RoyalSmash.Skills.SkillManager.Instance.Config.MegaBallDiameterMultiplier
                        : 2.0f;
                    float mMult = RoyalSmash.Skills.SkillManager.Instance.Config != null
                        ? RoyalSmash.Skills.SkillManager.Instance.Config.MegaBallMassMultiplier
                        : 2.5f;

                    projectile.ApplyMegaBall(dMult, mMult);
                    RoyalSmash.Skills.SkillManager.Instance.ConsumeMegaBall();
                }
            }
        }

        /// <summary>
        /// Set remaining ball count directly (for level loading or testing).
        /// </summary>
        public void SetBallCount(int count)
        {
            _remainingBalls = Mathf.Max(0, count);
            OnBallCountChanged?.Invoke(_remainingBalls);
        }

        /// <summary>
        /// Reset cooldown timer to 0 (for testing).
        /// </summary>
        public void ResetCooldown()
        {
            _cooldownTimer = 0f;
        }

        /// <summary>
        /// Manually set target yaw angle.
        /// </summary>
        public void SetTargetYaw(float yawAngle)
        {
            float minYaw = _config != null ? _config.MinYaw : -45f;
            float maxYaw = _config != null ? _config.MaxYaw : 45f;
            _targetYaw = Mathf.Clamp(yawAngle, minYaw, maxYaw);
        }
    }
}
