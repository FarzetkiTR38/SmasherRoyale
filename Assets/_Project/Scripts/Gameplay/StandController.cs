using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Controls the physical stand platform where puzzle objects are placed.
    /// Supports dynamic sizing, primary and optional secondary platforms (up to 2 per level),
    /// and ensures proper physics layer (Layer 8: Stand) and PhysicsMaterial assignments.
    /// </summary>
    public class StandController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private StandConfig _config;

        [Header("Primary Platform References")]
        [SerializeField] private Transform _primaryTabletop;
        [SerializeField] private Transform _primaryPillar;
        [SerializeField] private Transform _primaryBase;

        [Header("Secondary Platform References (Optional)")]
        [SerializeField] private GameObject _secondaryPlatformHolder;
        [SerializeField] private Transform _secondaryTabletop;
        [SerializeField] private Transform _secondaryPillar;

        public StandConfig Config => _config;

        public float TabletopSurfaceY
        {
            get
            {
                if (_primaryTabletop != null)
                {
                    return _primaryTabletop.position.y + (_primaryTabletop.localScale.y * 0.5f);
                }
                return transform.position.y + 1.35f;
            }
        }

        private void Awake()
        {
            ApplyConfiguration();
        }

        /// <summary>
        /// Applies dimensions and secondary platform setup from the assigned StandConfig.
        /// </summary>
        public void ApplyConfiguration(StandConfig configOverride = null)
        {
            if (configOverride != null)
            {
                _config = configOverride;
            }

            if (_config == null) return;

            // 1. Primary Platform Sizing
            if (_primaryTabletop != null)
            {
                _primaryTabletop.localScale = _config.PlatformSize;
                _primaryTabletop.localPosition = new Vector3(0f, _config.TabletopHeight, 0f);
            }

            if (_primaryPillar != null)
            {
                float pillarHeight = Mathf.Max(0.1f, _config.TabletopHeight);
                _primaryPillar.localScale = new Vector3(_primaryPillar.localScale.x, pillarHeight * 0.5f, _primaryPillar.localScale.z);
                _primaryPillar.localPosition = new Vector3(0f, pillarHeight * 0.5f, 0f);
            }

            // 2. Secondary Platform Setup (Optional up to 2 per level)
            if (_secondaryPlatformHolder != null)
            {
                _secondaryPlatformHolder.SetActive(_config.HasSecondaryPlatform);

                if (_config.HasSecondaryPlatform && _secondaryTabletop != null)
                {
                    _secondaryPlatformHolder.transform.localPosition = _config.SecondaryPlatformOffset;
                    _secondaryTabletop.localScale = _config.SecondaryPlatformSize;
                }
            }

            // Ensure proper layer
            int standLayer = LayerMask.NameToLayer("Stand");
            if (standLayer != -1)
            {
                SetLayerRecursively(gameObject, standLayer);
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child != null) SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<ProjectileController>() != null || collision.gameObject.CompareTag("Projectile"))
            {
                LevelStartPhysicsGate.ReleaseActiveGate();
            }
        }
    }
}
