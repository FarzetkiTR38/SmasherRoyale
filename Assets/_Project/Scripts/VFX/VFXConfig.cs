using UnityEngine;

namespace RoyalSmash.VFX
{
    /// <summary>
    /// Configuration data for visual effects (VFX) in Royal Smash.
    /// Exposes primary (Option 1) and alternative (Options 2 & 3) particle prefabs and materials.
    /// Per Master Build Prompt Section 22 and user overhaul request.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXConfig", menuName = "RoyalSmash/Config/VFXConfig")]
    public class VFXConfig : ScriptableObject
    {
        [Header("Projectile & Mortar Effects - Option 1 (Active)")]
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private Material _defaultTrailMaterial;
        [SerializeField] private Material _megaBallTrailMaterial;
        [SerializeField] private Material _infinityBallTrailMaterial;

        [Header("Projectile & Mortar Effects - Alternatives (Options 2 & 3)")]
        [SerializeField] private GameObject _muzzleFlashAlt1Prefab;
        [SerializeField] private GameObject _muzzleFlashAlt2Prefab;
        [SerializeField] private Material _defaultTrailAlt1Material;
        [SerializeField] private Material _defaultTrailAlt2Material;

        [Header("Impact & Physics Effects - Option 1 (Active)")]
        [SerializeField] private GameObject _impactGenericPrefab;
        [SerializeField] private GameObject _impactHeavyPrefab;
        [SerializeField] private GameObject _objectDestructionPrefab;
        [SerializeField] private float _heavyImpactThreshold = 3.0f;

        [Header("Impact & Physics Effects - Alternatives (Options 2 & 3)")]
        [SerializeField] private GameObject _impactGenericAlt1Prefab;
        [SerializeField] private GameObject _impactGenericAlt2Prefab;
        [SerializeField] private GameObject _impactHeavyAlt1Prefab;
        [SerializeField] private GameObject _impactHeavyAlt2Prefab;
        [SerializeField] private GameObject _objectDestructionAlt1Prefab;
        [SerializeField] private GameObject _objectDestructionAlt2Prefab;

        [Header("Special Object Effects (Paint Bottle) - Option 1 (Active)")]
        [SerializeField] private GameObject _bottleBreakShardsPrefab;
        [SerializeField] private GameObject _paintSpillPrefab;
        [SerializeField] private float _paintSpillLifetime = 4.0f;

        [Header("Special Object Effects (Paint Bottle) - Alternatives (Options 2 & 3)")]
        [SerializeField] private GameObject _bottleBreakShardsAlt1Prefab;
        [SerializeField] private GameObject _bottleBreakShardsAlt2Prefab;

        [Header("UI & State Celebrations - Option 1 (Active)")]
        [SerializeField] private GameObject _skillActivatePrefab;
        [SerializeField] private GameObject _winConfettiPrefab;
        [SerializeField] private GameObject _coinFlyPrefab;

        [Header("UI & State Celebrations - Alternatives (Options 2 & 3)")]
        [SerializeField] private GameObject _winConfettiAlt1Prefab;
        [SerializeField] private GameObject _winConfettiAlt2Prefab;

        [Header("Optimization & Pooling")]
        [SerializeField] private int _defaultPoolSize = 6;

        // Primary Accessors (Backwards Compatible)
        public GameObject MuzzleFlashPrefab { get => _muzzleFlashPrefab; set => _muzzleFlashPrefab = value; }
        public Material DefaultTrailMaterial { get => _defaultTrailMaterial; set => _defaultTrailMaterial = value; }
        public Material MegaBallTrailMaterial { get => _megaBallTrailMaterial; set => _megaBallTrailMaterial = value; }
        public Material InfinityBallTrailMaterial { get => _infinityBallTrailMaterial; set => _infinityBallTrailMaterial = value; }

        public GameObject ImpactGenericPrefab { get => _impactGenericPrefab; set => _impactGenericPrefab = value; }
        public GameObject ImpactHeavyPrefab { get => _impactHeavyPrefab; set => _impactHeavyPrefab = value; }
        public GameObject ObjectDestructionPrefab { get => _objectDestructionPrefab; set => _objectDestructionPrefab = value; }
        public float HeavyImpactThreshold => _heavyImpactThreshold;

        public GameObject BottleBreakShardsPrefab { get => _bottleBreakShardsPrefab; set => _bottleBreakShardsPrefab = value; }
        public GameObject PaintSpillPrefab { get => _paintSpillPrefab; set => _paintSpillPrefab = value; }
        public float PaintSpillLifetime => _paintSpillLifetime;

        public GameObject SkillActivatePrefab { get => _skillActivatePrefab; set => _skillActivatePrefab = value; }
        public GameObject WinConfettiPrefab { get => _winConfettiPrefab; set => _winConfettiPrefab = value; }
        public GameObject CoinFlyPrefab { get => _coinFlyPrefab; set => _coinFlyPrefab = value; }

        public int DefaultPoolSize => _defaultPoolSize;

        // Alternative Accessors
        public GameObject MuzzleFlashAlt1Prefab { get => _muzzleFlashAlt1Prefab; set => _muzzleFlashAlt1Prefab = value; }
        public GameObject MuzzleFlashAlt2Prefab { get => _muzzleFlashAlt2Prefab; set => _muzzleFlashAlt2Prefab = value; }
        public Material DefaultTrailAlt1Material { get => _defaultTrailAlt1Material; set => _defaultTrailAlt1Material = value; }
        public Material DefaultTrailAlt2Material { get => _defaultTrailAlt2Material; set => _defaultTrailAlt2Material = value; }

        public GameObject ImpactGenericAlt1Prefab { get => _impactGenericAlt1Prefab; set => _impactGenericAlt1Prefab = value; }
        public GameObject ImpactGenericAlt2Prefab { get => _impactGenericAlt2Prefab; set => _impactGenericAlt2Prefab = value; }
        public GameObject ImpactHeavyAlt1Prefab { get => _impactHeavyAlt1Prefab; set => _impactHeavyAlt1Prefab = value; }
        public GameObject ImpactHeavyAlt2Prefab { get => _impactHeavyAlt2Prefab; set => _impactHeavyAlt2Prefab = value; }
        public GameObject ObjectDestructionAlt1Prefab { get => _objectDestructionAlt1Prefab; set => _objectDestructionAlt1Prefab = value; }
        public GameObject ObjectDestructionAlt2Prefab { get => _objectDestructionAlt2Prefab; set => _objectDestructionAlt2Prefab = value; }

        public GameObject BottleBreakShardsAlt1Prefab { get => _bottleBreakShardsAlt1Prefab; set => _bottleBreakShardsAlt1Prefab = value; }
        public GameObject BottleBreakShardsAlt2Prefab { get => _bottleBreakShardsAlt2Prefab; set => _bottleBreakShardsAlt2Prefab = value; }

        public GameObject WinConfettiAlt1Prefab { get => _winConfettiAlt1Prefab; set => _winConfettiAlt1Prefab = value; }
        public GameObject WinConfettiAlt2Prefab { get => _winConfettiAlt2Prefab; set => _winConfettiAlt2Prefab = value; }

        public void SetMuzzleFlashVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _muzzleFlashPrefab = opt1;
            _muzzleFlashAlt1Prefab = opt2;
            _muzzleFlashAlt2Prefab = opt3;
        }

        public void SetTrailVariants(Material opt1, Material opt2, Material opt3)
        {
            _defaultTrailMaterial = opt1;
            _defaultTrailAlt1Material = opt2;
            _defaultTrailAlt2Material = opt3;
        }

        public void SetImpactGenericVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _impactGenericPrefab = opt1;
            _impactGenericAlt1Prefab = opt2;
            _impactGenericAlt2Prefab = opt3;
        }

        public void SetImpactHeavyVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _impactHeavyPrefab = opt1;
            _impactHeavyAlt1Prefab = opt2;
            _impactHeavyAlt2Prefab = opt3;
        }

        public void SetDestructionVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _objectDestructionPrefab = opt1;
            _objectDestructionAlt1Prefab = opt2;
            _objectDestructionAlt2Prefab = opt3;
        }

        public void SetBottleBreakVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _bottleBreakShardsPrefab = opt1;
            _bottleBreakShardsAlt1Prefab = opt2;
            _bottleBreakShardsAlt2Prefab = opt3;
        }

        public void SetWinConfettiVariants(GameObject opt1, GameObject opt2, GameObject opt3)
        {
            _winConfettiPrefab = opt1;
            _winConfettiAlt1Prefab = opt2;
            _winConfettiAlt2Prefab = opt3;
        }
    }
}
