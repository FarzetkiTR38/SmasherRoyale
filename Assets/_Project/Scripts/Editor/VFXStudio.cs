#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using RoyalSmash.VFX;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Professional VFX Studio generator.
    /// Builds 3 high-quality mobile-optimized visual variants for:
    /// - Projectile Launch Trails (Royal Comet, Fiery Ribbon, Stardust)
    /// - Paint Bottle Shatter & Fluid Splash (Fluid Splash, Ink Burst, Crystal Shatter)
    /// - Block & Obstacle Impacts (Radial Shockwave, Dust Blast, Comic Burst)
    /// - Mortar Muzzle Flash (Smoke Core, Fire Cone, Royal Spark)
    /// - Victory Celebration (Gold Ribbon, Color Streamer, Star Fountain)
    /// Also upgrades Ball.prefab and configures VFXConfig ScriptableObject.
    /// </summary>
    public static class VFXStudio
    {
        private const string MaterialsDir = "Assets/_Project/Materials/VFX";
        private const string TexturesDir = "Assets/_Project/Textures/VFX";
        private const string PrefabsDir = "Assets/_Project/Prefabs/VFX";
        private const string ConfigPath = "Assets/_Project/ScriptableObjects/Config/VFXConfig.asset";
        private const string BallPrefabPath = "Assets/_Project/Prefabs/Gameplay/Ball.prefab";

        [MenuItem("RoyalSmash/VFX Studio/Build All VFX Ecosystem")]
        public static void BuildAll()
        {
            EnsureDirectories();
            BuildMaterials();
            BuildPrefabs();
            UpgradeBallPrefab();
            UpdateVFXConfig();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>[VFXStudio] Successfully built complete VFX Ecosystem with 3 variants each!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(MaterialsDir)) Directory.CreateDirectory(MaterialsDir);
            if (!Directory.Exists(PrefabsDir)) Directory.CreateDirectory(PrefabsDir);
        }

        public static void BuildMaterials()
        {
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null)
            {
                particleShader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Mobile/Particles/Additive");
            }

            Texture2D texTrail = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_trail_streak.png");
            Texture2D texGlow = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_soft_glow.png");
            Texture2D texStar = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_sparkle_star.png");
            Texture2D texRing = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_shockwave_ring.png");
            Texture2D texSmoke = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_smoke_puff.png");
            Texture2D texSplat = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_paint_splat.png");
            Texture2D texShard = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesDir}/tex_vfx_glass_shard.png");

            // 1. Trail Materials (3 Variants + Default)
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_RoyalComet.mat", particleShader, texTrail, 
                new Color(1.0f, 0.85f, 0.4f, 1.0f), isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_FieryRibbon.mat", particleShader, texTrail, 
                new Color(1.0f, 0.35f, 0.05f, 1.0f), isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_Stardust.mat", particleShader, texTrail, 
                new Color(0.35f, 0.9f, 1.0f, 1.0f), isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_Default.mat", particleShader, texTrail, 
                new Color(1.0f, 0.85f, 0.4f, 1.0f), isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_Mega.mat", particleShader, texTrail, 
                new Color(1.0f, 0.2f, 0.2f, 1.0f), isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Trail_Infinity.mat", particleShader, texTrail, 
                new Color(0.3f, 0.8f, 1.0f, 1.0f), isAdditive: true);

            // 2. Particle Materials
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_Glow_Additive.mat", particleShader, texGlow, Color.white, isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_Sparkle_Additive.mat", particleShader, texStar, Color.white, isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_Shockwave.mat", particleShader, texRing, Color.white, isAdditive: true);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_Smoke.mat", particleShader, texSmoke, Color.white, isAdditive: false);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_PaintSplat.mat", particleShader, texSplat, Color.white, isAdditive: false);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_GlassShard.mat", particleShader, texShard, new Color(0.9f, 0.95f, 1.0f, 0.85f), isAdditive: false);
            CreateOrUpdateMaterial($"{MaterialsDir}/Mat_Particle_Additive.mat", particleShader, texGlow, Color.white, isAdditive: true);

            AssetDatabase.SaveAssets();
        }

        private static Material CreateOrUpdateMaterial(string path, Shader shader, Texture2D texture, Color color, bool isAdditive)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // URP Particles/Unlit property bindings
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1.0f); // Transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", isAdditive ? 2.0f : 0.0f); // 2 = Additive, 0 = Alpha
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", isAdditive ? 1.0f : 5.0f); // One or SrcAlpha
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", isAdditive ? 1.0f : 10.0f); // One or OneMinusSrcAlpha
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0.0f);

            if (mat.HasProperty("_BaseMap") && texture != null) mat.SetTexture("_BaseMap", texture);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void BuildPrefabs()
        {
            Material matGlow = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_Glow_Additive.mat");
            Material matSparkle = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_Sparkle_Additive.mat");
            Material matShockwave = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_Shockwave.mat");
            Material matSmoke = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_Smoke.mat");
            Material matSplat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_PaintSplat.mat");
            Material matShard = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_GlassShard.mat");

            // A. Bottle Break Variants (3 variants)
            CreateBottleBreakOpt1($"{PrefabsDir}/VFX_BottleBreak_Opt1_FluidSplash.prefab", matSplat, matShard, matShockwave, matGlow);
            CreateBottleBreakOpt2($"{PrefabsDir}/VFX_BottleBreak_Opt2_InkBurst.prefab", matSmoke, matSplat, matShard);
            CreateBottleBreakOpt3($"{PrefabsDir}/VFX_BottleBreak_Opt3_CrystalShatter.prefab", matShard, matSparkle, matGlow);
            CreateBottleBreakOpt1($"{PrefabsDir}/VFX_BottleShards.prefab", matSplat, matShard, matShockwave, matGlow);

            // B. Impact Variants (3 variants)
            CreateImpactOpt1($"{PrefabsDir}/VFX_Impact_Opt1_RadialShockwave.prefab", matShockwave, matSparkle, matGlow);
            CreateImpactOpt2($"{PrefabsDir}/VFX_Impact_Opt2_DustBlast.prefab", matSmoke, matShard);
            CreateImpactOpt3($"{PrefabsDir}/VFX_Impact_Opt3_ComicBurst.prefab", matSparkle, matShockwave);
            CreateImpactOpt1($"{PrefabsDir}/VFX_ImpactGeneric.prefab", matShockwave, matSparkle, matGlow);
            CreateImpactOpt2($"{PrefabsDir}/VFX_ImpactHeavy.prefab", matSmoke, matShard);

            // C. Muzzle Flash Variants (3 variants)
            CreateMuzzleFlashOpt1($"{PrefabsDir}/VFX_MuzzleFlash_Opt1_SmokeCore.prefab", matGlow, matSmoke, matSparkle);
            CreateMuzzleFlashOpt2($"{PrefabsDir}/VFX_MuzzleFlash_Opt2_FireCone.prefab", matGlow, matSparkle);
            CreateMuzzleFlashOpt3($"{PrefabsDir}/VFX_MuzzleFlash_Opt3_RoyalSpark.prefab", matSparkle, matShockwave);
            CreateMuzzleFlashOpt1($"{PrefabsDir}/VFX_MuzzleFlash.prefab", matGlow, matSmoke, matSparkle);

            // D. Win Confetti Variants (3 variants)
            CreateWinConfettiOpt1($"{PrefabsDir}/VFX_Win_Opt1_GoldRibbon.prefab", matSparkle, matGlow);
            CreateWinConfettiOpt2($"{PrefabsDir}/VFX_Win_Opt2_ColorStreamer.prefab", matSparkle);
            CreateWinConfettiOpt3($"{PrefabsDir}/VFX_Win_Opt3_StarFountain.prefab", matSparkle, matGlow);
            CreateWinConfettiOpt1($"{PrefabsDir}/VFX_WinConfetti.prefab", matSparkle, matGlow);

            // E. Object Destruction
            CreateDestructionPrefab($"{PrefabsDir}/VFX_ObjectDestruct.prefab", matSmoke, matShard);
        }

        // =========================================================================
        // PREFAB BUILDERS
        // =========================================================================

        private static void CreateBottleBreakOpt1(string path, Material matSplat, Material matShard, Material matRing, Material matGlow)
        {
            GameObject root = new GameObject("VFX_BottleBreak_Opt1_FluidSplash");

            // Sub-emitter 1: Liquid Paint Droplets
            GameObject goDroplets = new GameObject("PaintDroplets");
            goDroplets.transform.SetParent(root.transform);
            var psDroplets = goDroplets.AddComponent<ParticleSystem>();
            var renDroplets = goDroplets.GetComponent<ParticleSystemRenderer>();
            renDroplets.material = matSplat;

            var mainD = psDroplets.main;
            mainD.duration = 1.0f;
            mainD.loop = false;
            mainD.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            mainD.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 9.0f);
            mainD.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.4f);
            mainD.gravityModifier = 1.6f;
            mainD.playOnAwake = false;

            var emitD = psDroplets.emission;
            emitD.rateOverTime = 0;
            emitD.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 32) });

            var shapeD = psDroplets.shape;
            shapeD.shapeType = ParticleSystemShapeType.Hemisphere;
            shapeD.radius = 0.2f;

            var sizeD = psDroplets.sizeOverLifetime;
            sizeD.enabled = true;
            AnimationCurve curveD = new AnimationCurve();
            curveD.AddKey(0f, 0.4f);
            curveD.AddKey(0.25f, 1.0f);
            curveD.AddKey(1f, 0.0f);
            sizeD.size = new ParticleSystem.MinMaxCurve(1f, curveD);

            // Sub-emitter 2: 3D Glass Shards
            GameObject goShards = new GameObject("GlassShards");
            goShards.transform.SetParent(root.transform);
            var psShards = goShards.AddComponent<ParticleSystem>();
            var renShards = goShards.GetComponent<ParticleSystemRenderer>();
            renShards.material = matShard;

            var mainS = psShards.main;
            mainS.duration = 1.0f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(5.0f, 10.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            mainS.gravityModifier = 1.8f;
            mainS.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            mainS.startColor = new Color(0.9f, 0.95f, 1.0f, 0.85f);
            mainS.playOnAwake = false;

            var emitS = psShards.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shapeS = psShards.shape;
            shapeS.shapeType = ParticleSystemShapeType.Sphere;
            shapeS.radius = 0.15f;

            var rotS = psShards.rotationOverLifetime;
            rotS.enabled = true;
            rotS.x = new ParticleSystem.MinMaxCurve(180f * Mathf.Deg2Rad, 540f * Mathf.Deg2Rad);
            rotS.z = new ParticleSystem.MinMaxCurve(180f * Mathf.Deg2Rad, 540f * Mathf.Deg2Rad);

            // Sub-emitter 3: Radial Paint Shockwave Ring
            GameObject goRing = new GameObject("PaintRing");
            goRing.transform.SetParent(root.transform);
            goRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var psRing = goRing.AddComponent<ParticleSystem>();
            var renRing = goRing.GetComponent<ParticleSystemRenderer>();
            renRing.material = matRing;

            var mainR = psRing.main;
            mainR.duration = 0.5f;
            mainR.loop = false;
            mainR.startLifetime = 0.4f;
            mainR.startSpeed = 0f;
            mainR.startSize = 0.3f;
            mainR.playOnAwake = false;

            var emitR = psRing.emission;
            emitR.rateOverTime = 0;
            emitR.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var sizeR = psRing.sizeOverLifetime;
            sizeR.enabled = true;
            AnimationCurve curveR = new AnimationCurve();
            curveR.AddKey(0f, 0.2f);
            curveR.AddKey(1f, 2.2f);
            sizeR.size = new ParticleSystem.MinMaxCurve(1f, curveR);

            var colorR = psRing.colorOverLifetime;
            colorR.enabled = true;
            Gradient gradR = new Gradient();
            gradR.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.0f, 1f) }
            );
            colorR.color = gradR;

            // Root particle system orchestrator
            var rootPS = root.AddComponent<ParticleSystem>();
            var rootRen = root.GetComponent<ParticleSystemRenderer>();
            rootRen.enabled = false;
            var rootMain = rootPS.main;
            rootMain.duration = 1.2f;
            rootMain.loop = false;
            rootMain.playOnAwake = false;
            var rootEmit = rootPS.emission;
            rootEmit.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateBottleBreakOpt2(string path, Material matSmoke, Material matSplat, Material matShard)
        {
            GameObject root = new GameObject("VFX_BottleBreak_Opt2_InkBurst");

            // Dense cartoon ink puffs
            GameObject goSmoke = new GameObject("InkPuffs");
            goSmoke.transform.SetParent(root.transform);
            var psSmoke = goSmoke.AddComponent<ParticleSystem>();
            var renSmoke = goSmoke.GetComponent<ParticleSystemRenderer>();
            renSmoke.material = matSmoke;

            var mainSmoke = psSmoke.main;
            mainSmoke.duration = 1.0f;
            mainSmoke.loop = false;
            mainSmoke.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.2f);
            mainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 5.0f);
            mainSmoke.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            mainSmoke.gravityModifier = 0.2f;
            mainSmoke.playOnAwake = false;

            var emitSmoke = psSmoke.emission;
            emitSmoke.rateOverTime = 0;
            emitSmoke.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });

            var shapeSmoke = psSmoke.shape;
            shapeSmoke.shapeType = ParticleSystemShapeType.Sphere;
            shapeSmoke.radius = 0.25f;

            // Splat drops
            GameObject goSplat = new GameObject("InkDroplets");
            goSplat.transform.SetParent(root.transform);
            var psSplat = goSplat.AddComponent<ParticleSystem>();
            var renSplat = goSplat.GetComponent<ParticleSystemRenderer>();
            renSplat.material = matSplat;

            var mainSplat = psSplat.main;
            mainSplat.duration = 1.0f;
            mainSplat.loop = false;
            mainSplat.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            mainSplat.startSpeed = new ParticleSystem.MinMaxCurve(5.0f, 10.0f);
            mainSplat.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            mainSplat.gravityModifier = 1.6f;
            mainSplat.playOnAwake = false;

            var emitSplat = psSplat.emission;
            emitSplat.rateOverTime = 0;
            emitSplat.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });

            // Root
            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 1.2f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateBottleBreakOpt3(string path, Material matShard, Material matSparkle, Material matGlow)
        {
            GameObject root = new GameObject("VFX_BottleBreak_Opt3_CrystalShatter");

            // High-density crystal shards
            GameObject goShards = new GameObject("CrystalSplinters");
            goShards.transform.SetParent(root.transform);
            var psShards = goShards.AddComponent<ParticleSystem>();
            var renShards = goShards.GetComponent<ParticleSystemRenderer>();
            renShards.material = matShard;

            var mainS = psShards.main;
            mainS.duration = 1.0f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(6.0f, 12.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.28f);
            mainS.gravityModifier = 1.9f;
            mainS.startColor = new Color(0.8f, 0.95f, 1.0f, 0.9f);
            mainS.playOnAwake = false;

            var emitS = psShards.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 36) });

            // Prismatic sparkles
            GameObject goSpark = new GameObject("CrystalSparkles");
            goSpark.transform.SetParent(root.transform);
            var psSpark = goSpark.AddComponent<ParticleSystem>();
            var renSpark = goSpark.GetComponent<ParticleSystemRenderer>();
            renSpark.material = matSparkle;

            var mainSpark = psSpark.main;
            mainSpark.duration = 0.8f;
            mainSpark.loop = false;
            mainSpark.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            mainSpark.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 8.0f);
            mainSpark.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            mainSpark.startColor = new Color(1.0f, 0.95f, 0.6f, 1.0f);
            mainSpark.playOnAwake = false;

            var emitSpark = psSpark.emission;
            emitSpark.rateOverTime = 0;
            emitSpark.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            // Root
            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 1.2f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateImpactOpt1(string path, Material matRing, Material matSparkle, Material matGlow)
        {
            GameObject root = new GameObject("VFX_Impact_Opt1_RadialShockwave");

            // Expanding Shockwave ring
            GameObject goRing = new GameObject("Shockwave");
            goRing.transform.SetParent(root.transform);
            goRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var psRing = goRing.AddComponent<ParticleSystem>();
            goRing.GetComponent<ParticleSystemRenderer>().material = matRing;

            var mainR = psRing.main;
            mainR.duration = 0.35f;
            mainR.loop = false;
            mainR.startLifetime = 0.3f;
            mainR.startSpeed = 0f;
            mainR.startSize = 0.2f;
            mainR.startColor = new Color(1.0f, 0.9f, 0.6f, 0.9f);
            mainR.playOnAwake = false;

            var emitR = psRing.emission;
            emitR.rateOverTime = 0;
            emitR.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var sizeR = psRing.sizeOverLifetime;
            sizeR.enabled = true;
            AnimationCurve cR = new AnimationCurve();
            cR.AddKey(0f, 0.1f);
            cR.AddKey(1f, 1.6f);
            sizeR.size = new ParticleSystem.MinMaxCurve(1f, cR);

            // Bouncing sparks
            GameObject goSparks = new GameObject("Sparks");
            goSparks.transform.SetParent(root.transform);
            var psSparks = goSparks.AddComponent<ParticleSystem>();
            psSparks.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var mainS = psSparks.main;
            mainS.duration = 0.5f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 9.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            mainS.gravityModifier = 1.5f;
            mainS.startColor = new Color(1.0f, 0.85f, 0.3f, 1.0f);
            mainS.playOnAwake = false;

            var emitS = psSparks.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 22) });

            var shapeS = psSparks.shape;
            shapeS.shapeType = ParticleSystemShapeType.Cone;
            shapeS.angle = 45f;
            shapeS.radius = 0.1f;

            // Core flash
            GameObject goFlash = new GameObject("Flash");
            goFlash.transform.SetParent(root.transform);
            var psFlash = goFlash.AddComponent<ParticleSystem>();
            psFlash.GetComponent<ParticleSystemRenderer>().material = matGlow;

            var mainF = psFlash.main;
            mainF.duration = 0.15f;
            mainF.loop = false;
            mainF.startLifetime = 0.1f;
            mainF.startSpeed = 0f;
            mainF.startSize = 0.8f;
            mainF.startColor = new Color(1.0f, 0.95f, 0.8f, 1.0f);
            mainF.playOnAwake = false;
            var emitF = psFlash.emission;
            emitF.rateOverTime = 0;
            emitF.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            // Root
            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.6f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateImpactOpt2(string path, Material matSmoke, Material matShard)
        {
            GameObject root = new GameObject("VFX_Impact_Opt2_DustBlast");

            GameObject goSmoke = new GameObject("DustCloud");
            goSmoke.transform.SetParent(root.transform);
            var psSmoke = goSmoke.AddComponent<ParticleSystem>();
            psSmoke.GetComponent<ParticleSystemRenderer>().material = matSmoke;

            var mainSmoke = psSmoke.main;
            mainSmoke.duration = 0.6f;
            mainSmoke.loop = false;
            mainSmoke.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            mainSmoke.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 5.0f);
            mainSmoke.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            mainSmoke.startColor = new Color(0.85f, 0.82f, 0.75f, 0.8f);
            mainSmoke.gravityModifier = 0.3f;
            mainSmoke.playOnAwake = false;

            var emitSmoke = psSmoke.emission;
            emitSmoke.rateOverTime = 0;
            emitSmoke.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 14) });

            // Debris
            GameObject goDebris = new GameObject("Debris");
            goDebris.transform.SetParent(root.transform);
            var psDebris = goDebris.AddComponent<ParticleSystem>();
            psDebris.GetComponent<ParticleSystemRenderer>().material = matShard;

            var mainD = psDebris.main;
            mainD.duration = 0.6f;
            mainD.loop = false;
            mainD.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            mainD.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);
            mainD.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            mainD.gravityModifier = 1.8f;
            mainD.startColor = new Color(0.6f, 0.55f, 0.5f, 1.0f);
            mainD.playOnAwake = false;

            var emitD = psDebris.emission;
            emitD.rateOverTime = 0;
            emitD.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.8f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateImpactOpt3(string path, Material matSparkle, Material matRing)
        {
            GameObject root = new GameObject("VFX_Impact_Opt3_ComicBurst");

            GameObject goStar = new GameObject("ComicStar");
            goStar.transform.SetParent(root.transform);
            var psStar = goStar.AddComponent<ParticleSystem>();
            psStar.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var mainS = psStar.main;
            mainS.duration = 0.25f;
            mainS.loop = false;
            mainS.startLifetime = 0.2f;
            mainS.startSpeed = 0f;
            mainS.startSize = 1.2f;
            mainS.startColor = new Color(1.0f, 0.9f, 0.2f, 1.0f);
            mainS.playOnAwake = false;

            var emitS = psStar.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            // Pop Ring
            GameObject goRing = new GameObject("PopRing");
            goRing.transform.SetParent(root.transform);
            var psRing = goRing.AddComponent<ParticleSystem>();
            psRing.GetComponent<ParticleSystemRenderer>().material = matRing;

            var mainR = psRing.main;
            mainR.duration = 0.3f;
            mainR.loop = false;
            mainR.startLifetime = 0.25f;
            mainR.startSpeed = 0f;
            mainR.startSize = 0.2f;
            mainR.startColor = new Color(1.0f, 0.4f, 0.1f, 1.0f);
            mainR.playOnAwake = false;

            var emitR = psRing.emission;
            emitR.rateOverTime = 0;
            emitR.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var sizeR = psRing.sizeOverLifetime;
            sizeR.enabled = true;
            AnimationCurve c = new AnimationCurve();
            c.AddKey(0f, 0.1f);
            c.AddKey(1f, 1.4f);
            sizeR.size = new ParticleSystem.MinMaxCurve(1f, c);

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.4f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateMuzzleFlashOpt1(string path, Material matGlow, Material matSmoke, Material matSparkle)
        {
            GameObject root = new GameObject("VFX_MuzzleFlash_Opt1_SmokeCore");

            // Core flash
            GameObject goFlash = new GameObject("CoreFlash");
            goFlash.transform.SetParent(root.transform);
            var psFlash = goFlash.AddComponent<ParticleSystem>();
            psFlash.GetComponent<ParticleSystemRenderer>().material = matGlow;

            var mainF = psFlash.main;
            mainF.duration = 0.1f;
            mainF.loop = false;
            mainF.startLifetime = 0.08f;
            mainF.startSpeed = 0f;
            mainF.startSize = 1.0f;
            mainF.startColor = new Color(1.0f, 0.9f, 0.6f, 1.0f);
            mainF.playOnAwake = false;

            var emitF = psFlash.emission;
            emitF.rateOverTime = 0;
            emitF.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            // Rolling smoke out of muzzle
            GameObject goSmoke = new GameObject("RollingSmoke");
            goSmoke.transform.SetParent(root.transform);
            var psSmoke = goSmoke.AddComponent<ParticleSystem>();
            psSmoke.GetComponent<ParticleSystemRenderer>().material = matSmoke;

            var mainS = psSmoke.main;
            mainS.duration = 0.5f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 6.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
            mainS.startColor = new Color(0.9f, 0.9f, 0.92f, 0.75f);
            mainS.gravityModifier = -0.2f;
            mainS.playOnAwake = false;

            var emitS = psSmoke.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

            var shapeS = psSmoke.shape;
            shapeS.shapeType = ParticleSystemShapeType.Cone;
            shapeS.angle = 25f;
            shapeS.radius = 0.15f;

            // Sparks
            GameObject goSparks = new GameObject("Sparks");
            goSparks.transform.SetParent(root.transform);
            var psSparks = goSparks.AddComponent<ParticleSystem>();
            psSparks.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var mainSparks = psSparks.main;
            mainSparks.duration = 0.3f;
            mainSparks.loop = false;
            mainSparks.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            mainSparks.startSpeed = new ParticleSystem.MinMaxCurve(6.0f, 12.0f);
            mainSparks.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            mainSparks.startColor = new Color(1.0f, 0.75f, 0.2f, 1.0f);
            mainSparks.playOnAwake = false;

            var emitSparks = psSparks.emission;
            emitSparks.rateOverTime = 0;
            emitSparks.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16) });

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.7f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateMuzzleFlashOpt2(string path, Material matGlow, Material matSparkle)
        {
            GameObject root = new GameObject("VFX_MuzzleFlash_Opt2_FireCone");

            GameObject goCone = new GameObject("FireCone");
            goCone.transform.SetParent(root.transform);
            var psCone = goCone.AddComponent<ParticleSystem>();
            psCone.GetComponent<ParticleSystemRenderer>().material = matGlow;

            var mainC = psCone.main;
            mainC.duration = 0.2f;
            mainC.loop = false;
            mainC.startLifetime = 0.12f;
            mainC.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);
            mainC.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            mainC.startColor = new Color(1.0f, 0.35f, 0.05f, 1.0f);
            mainC.playOnAwake = false;

            var emitC = psCone.emission;
            emitC.rateOverTime = 0;
            emitC.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.4f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateMuzzleFlashOpt3(string path, Material matSparkle, Material matRing)
        {
            GameObject root = new GameObject("VFX_MuzzleFlash_Opt3_RoyalSpark");

            GameObject goStars = new GameObject("RoyalStars");
            goStars.transform.SetParent(root.transform);
            var psStars = goStars.AddComponent<ParticleSystem>();
            psStars.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var mainS = psStars.main;
            mainS.duration = 0.4f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(5.0f, 11.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            mainS.startColor = new Color(1.0f, 0.85f, 0.2f, 1.0f);
            mainS.playOnAwake = false;

            var emitS = psStars.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 0.5f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateWinConfettiOpt1(string path, Material matSparkle, Material matGlow)
        {
            GameObject root = new GameObject("VFX_Win_Opt1_GoldRibbon");

            GameObject goConfetti = new GameObject("GoldConfetti");
            goConfetti.transform.SetParent(root.transform);
            var ps = goConfetti.AddComponent<ParticleSystem>();
            ps.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var main = ps.main;
            main.duration = 3.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 6.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.gravityModifier = 0.4f;
            main.startColor = new Color(1.0f, 0.85f, 0.25f, 1.0f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.playOnAwake = false;

            var emit = ps.emission;
            emit.rateOverTime = 40;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 1f, 4f);

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(120f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 4.0f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateWinConfettiOpt2(string path, Material matSparkle)
        {
            GameObject root = new GameObject("VFX_Win_Opt2_ColorStreamer");

            GameObject go = new GameObject("Streamers");
            go.transform.SetParent(root.transform);
            var ps = go.AddComponent<ParticleSystem>();
            ps.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var main = ps.main;
            main.duration = 3.0f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.0f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5.0f, 12.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
            main.gravityModifier = 0.6f;
            main.playOnAwake = false;

            var emit = ps.emission;
            emit.rateOverTime = 50;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.5f;

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 3.5f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateWinConfettiOpt3(string path, Material matSparkle, Material matGlow)
        {
            GameObject root = new GameObject("VFX_Win_Opt3_StarFountain");

            GameObject goStars = new GameObject("StarFountain");
            goStars.transform.SetParent(root.transform);
            var ps = goStars.AddComponent<ParticleSystem>();
            ps.GetComponent<ParticleSystemRenderer>().material = matSparkle;

            var main = ps.main;
            main.duration = 3.0f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6.0f, 14.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.gravityModifier = 0.8f;
            main.startColor = new Color(1.0f, 0.9f, 0.3f, 1.0f);
            main.playOnAwake = false;

            var emit = ps.emission;
            emit.rateOverTime = 35;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.3f;

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 3.5f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void CreateDestructionPrefab(string path, Material matSmoke, Material matShard)
        {
            GameObject root = new GameObject("VFX_ObjectDestruct");

            // Smoke puff
            GameObject goSmoke = new GameObject("SmokePuff");
            goSmoke.transform.SetParent(root.transform);
            var psSmoke = goSmoke.AddComponent<ParticleSystem>();
            psSmoke.GetComponent<ParticleSystemRenderer>().material = matSmoke;

            var mainS = psSmoke.main;
            mainS.duration = 0.8f;
            mainS.loop = false;
            mainS.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            mainS.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.0f);
            mainS.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            mainS.gravityModifier = 0.2f;
            mainS.playOnAwake = false;

            var emitS = psSmoke.emission;
            emitS.rateOverTime = 0;
            emitS.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16) });

            // Debris rubble
            GameObject goDebris = new GameObject("RubbleShards");
            goDebris.transform.SetParent(root.transform);
            var psDebris = goDebris.AddComponent<ParticleSystem>();
            psDebris.GetComponent<ParticleSystemRenderer>().material = matShard;

            var mainD = psDebris.main;
            mainD.duration = 0.8f;
            mainD.loop = false;
            mainD.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            mainD.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 7.0f);
            mainD.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
            mainD.gravityModifier = 1.7f;
            mainD.playOnAwake = false;

            var emitD = psDebris.emission;
            emitD.rateOverTime = 0;
            emitD.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16) });

            var rootPS = root.AddComponent<ParticleSystem>();
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            var rm = rootPS.main;
            rm.duration = 1.0f;
            rm.loop = false;
            rm.playOnAwake = false;
            var rem = rootPS.emission;
            rem.rateOverTime = 0;

            SaveAndDestroyPrefab(root, path);
        }

        private static void SaveAndDestroyPrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // =========================================================================
        // BALL PREFAB UPGRADE
        // =========================================================================

        public static void UpgradeBallPrefab()
        {
            GameObject ballPrefab = PrefabUtility.LoadPrefabContents(BallPrefabPath);
            if (ballPrefab == null)
            {
                Debug.LogWarning($"[VFXStudio] Could not load Ball prefab at {BallPrefabPath}");
                return;
            }

            Material matComet = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Trail_RoyalComet.mat");
            TrailRenderer trail = ballPrefab.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.material = matComet;
                trail.time = 0.35f;
                trail.minVertexDistance = 0.05f;
                trail.numCapVertices = 4;
                trail.numCornerVertices = 4;
                trail.alignment = LineAlignment.View;
                trail.textureMode = LineTextureMode.DistributePerSegment;

                // Smooth width curve (0.45 -> 0.0)
                AnimationCurve widthCurve = new AnimationCurve();
                widthCurve.AddKey(new Keyframe(0f, 0.45f, 0f, -0.6f));
                widthCurve.AddKey(new Keyframe(0.5f, 0.22f, -0.4f, -0.4f));
                widthCurve.AddKey(new Keyframe(1f, 0.0f, -0.3f, 0f));
                trail.widthCurve = widthCurve;

                // Glowing additive gradient: bright gold core -> warm amber -> alpha fade
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f),
                        new GradientColorKey(new Color(1f, 0.65f, 0.15f), 0.4f),
                        new GradientColorKey(new Color(0.9f, 0.3f, 0.05f), 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(1.0f, 0f),
                        new GradientAlphaKey(0.85f, 0.3f),
                        new GradientAlphaKey(0.0f, 1f)
                    }
                );
                trail.colorGradient = grad;
            }

            // Trailing sparkles child emitter
            Transform sparkChild = ballPrefab.transform.Find("VFX_TrailingSparkles");
            if (sparkChild == null)
            {
                GameObject sparkGo = new GameObject("VFX_TrailingSparkles");
                sparkGo.transform.SetParent(ballPrefab.transform);
                sparkGo.transform.localPosition = Vector3.zero;
                sparkChild = sparkGo.transform;
            }

            Material matStar = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Particle_Sparkle_Additive.mat");
            ParticleSystem ps = sparkChild.GetComponent<ParticleSystem>();
            if (ps == null) ps = sparkChild.gameObject.AddComponent<ParticleSystem>();
            ParticleSystemRenderer ren = sparkChild.GetComponent<ParticleSystemRenderer>();
            ren.material = matStar;

            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new Color(1.0f, 0.85f, 0.3f, 0.9f);
            main.loop = true;
            main.playOnAwake = true;

            var emit = ps.emission;
            emit.rateOverTime = 25;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            PrefabUtility.SaveAsPrefabAsset(ballPrefab, BallPrefabPath);
            PrefabUtility.UnloadPrefabContents(ballPrefab);
            Debug.Log("[VFXStudio] Ball.prefab successfully upgraded with Royal Comet trail and sparkle emitter!");
        }

        // =========================================================================
        // CONFIG BINDINGS
        // =========================================================================

        public static void UpdateVFXConfig()
        {
            VFXConfig config = AssetDatabase.LoadAssetAtPath<VFXConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[VFXStudio] Could not find VFXConfig at {ConfigPath}");
                return;
            }

            // Trails
            Material opt1Trail = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Trail_RoyalComet.mat");
            Material opt2Trail = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Trail_FieryRibbon.mat");
            Material opt3Trail = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsDir}/Mat_Trail_Stardust.mat");
            config.SetTrailVariants(opt1Trail, opt2Trail, opt3Trail);

            // Muzzle Flash
            GameObject opt1Flash = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_MuzzleFlash_Opt1_SmokeCore.prefab");
            GameObject opt2Flash = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_MuzzleFlash_Opt2_FireCone.prefab");
            GameObject opt3Flash = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_MuzzleFlash_Opt3_RoyalSpark.prefab");
            config.SetMuzzleFlashVariants(opt1Flash, opt2Flash, opt3Flash);

            // Bottle Break
            GameObject opt1Bottle = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_BottleBreak_Opt1_FluidSplash.prefab");
            GameObject opt2Bottle = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_BottleBreak_Opt2_InkBurst.prefab");
            GameObject opt3Bottle = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_BottleBreak_Opt3_CrystalShatter.prefab");
            config.SetBottleBreakVariants(opt1Bottle, opt2Bottle, opt3Bottle);

            // Impacts
            GameObject opt1Impact = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Impact_Opt1_RadialShockwave.prefab");
            GameObject opt2Impact = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Impact_Opt2_DustBlast.prefab");
            GameObject opt3Impact = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Impact_Opt3_ComicBurst.prefab");
            config.SetImpactGenericVariants(opt1Impact, opt2Impact, opt3Impact);
            config.SetImpactHeavyVariants(opt2Impact, opt1Impact, opt3Impact);

            // Destruction
            GameObject destruct = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_ObjectDestruct.prefab");
            config.SetDestructionVariants(destruct, opt2Impact, opt3Impact);

            // Win Confetti
            GameObject opt1Win = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Win_Opt1_GoldRibbon.prefab");
            GameObject opt2Win = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Win_Opt2_ColorStreamer.prefab");
            GameObject opt3Win = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsDir}/VFX_Win_Opt3_StarFountain.prefab");
            config.SetWinConfettiVariants(opt1Win, opt2Win, opt3Win);

            EditorUtility.SetDirty(config);
            Debug.Log("[VFXStudio] VFXConfig.asset successfully updated with all 3 variants per effect!");
        }
    }
}
#endif
