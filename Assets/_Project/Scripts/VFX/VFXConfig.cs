using UnityEngine;

namespace RoyalSmash.VFX
{
    /// <summary>
    /// Configuration data for visual effects (VFX) in Royal Smash.
    /// Exposes particle prefabs, pools, and thresholds per Master Build Prompt Section 22.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXConfig", menuName = "RoyalSmash/Config/VFXConfig")]
    public class VFXConfig : ScriptableObject
    {
        [Header("Projectile & Mortar Effects")]
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private Material _defaultTrailMaterial;
        [SerializeField] private Material _megaBallTrailMaterial;
        [SerializeField] private Material _infinityBallTrailMaterial;

        [Header("Impact & Physics Effects")]
        [SerializeField] private GameObject _impactGenericPrefab;
        [SerializeField] private GameObject _impactHeavyPrefab;
        [SerializeField] private GameObject _objectDestructionPrefab;
        [SerializeField] private float _heavyImpactThreshold = 3.0f;

        [Header("Special Object Effects (Paint Bottle)")]
        [SerializeField] private GameObject _bottleBreakShardsPrefab;
        [SerializeField] private GameObject _paintSpillPrefab;
        [SerializeField] private float _paintSpillLifetime = 4.0f;

        [Header("UI & State Celebrations")]
        [SerializeField] private GameObject _skillActivatePrefab;
        [SerializeField] private GameObject _winConfettiPrefab;
        [SerializeField] private GameObject _coinFlyPrefab;

        [Header("Optimization & Pooling")]
        [SerializeField] private int _defaultPoolSize = 6;

        public GameObject MuzzleFlashPrefab => _muzzleFlashPrefab;
        public Material DefaultTrailMaterial => _defaultTrailMaterial;
        public Material MegaBallTrailMaterial => _megaBallTrailMaterial;
        public Material InfinityBallTrailMaterial => _infinityBallTrailMaterial;

        public GameObject ImpactGenericPrefab => _impactGenericPrefab;
        public GameObject ImpactHeavyPrefab => _impactHeavyPrefab;
        public GameObject ObjectDestructionPrefab => _objectDestructionPrefab;
        public float HeavyImpactThreshold => _heavyImpactThreshold;

        public GameObject BottleBreakShardsPrefab => _bottleBreakShardsPrefab;
        public GameObject PaintSpillPrefab => _paintSpillPrefab;
        public float PaintSpillLifetime => _paintSpillLifetime;

        public GameObject SkillActivatePrefab => _skillActivatePrefab;
        public GameObject WinConfettiPrefab => _winConfettiPrefab;
        public GameObject CoinFlyPrefab => _coinFlyPrefab;

        public int DefaultPoolSize => _defaultPoolSize;
    }
}
