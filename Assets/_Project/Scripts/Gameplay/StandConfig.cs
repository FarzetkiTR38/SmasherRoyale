using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Configurable parameters for stand platform dimensions, pillar styling, and multi-platform layouts.
    /// Supports up to 2 platforms per level as required by the MVP specification.
    /// </summary>
    [CreateAssetMenu(fileName = "StandConfig", menuName = "RoyalSmash/Config/StandConfig")]
    public class StandConfig : ScriptableObject
    {
        [Header("Primary Platform Dimensions")]
        [Tooltip("Width (X), Tabletop thickness (Y), and Depth (Z) of the primary platform.")]
        [SerializeField] private Vector3 _platformSize = new Vector3(5.0f, 0.25f, 3.2f);

        [Tooltip("Height of the stand tabletop surface above ground.")]
        [SerializeField] private float _tabletopHeight = 1.35f;

        [Header("Secondary Platform (Optional - up to 2 per level)")]
        [Tooltip("Whether this level layout features a secondary platform.")]
        [SerializeField] private bool _hasSecondaryPlatform = false;

        [Tooltip("World position offset for the secondary platform relative to the primary stand.")]
        [SerializeField] private Vector3 _secondaryPlatformOffset = new Vector3(0f, 0.8f, 2.5f);

        [Tooltip("Dimensions of the secondary platform.")]
        [SerializeField] private Vector3 _secondaryPlatformSize = new Vector3(3.5f, 0.25f, 2.0f);

        public Vector3 PlatformSize => _platformSize;
        public float TabletopHeight => _tabletopHeight;
        public bool HasSecondaryPlatform => _hasSecondaryPlatform;
        public Vector3 SecondaryPlatformOffset => _secondaryPlatformOffset;
        public Vector3 SecondaryPlatformSize => _secondaryPlatformSize;
    }
}
