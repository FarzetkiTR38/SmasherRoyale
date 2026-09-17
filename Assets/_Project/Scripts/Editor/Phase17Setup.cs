using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using RoyalSmash.VFX;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Autonomous setup script for Phase 17: VFX & Visual Polish.
    /// Procedurally creates all Section 22 particle prefabs, materials, and configures VFXManager.
    /// </summary>
    public static class Phase17Setup
    {
        private const string VFX_DIR = "Assets/_Project/Prefabs/VFX";
        private const string MAT_DIR = "Assets/_Project/Materials/VFX";
        private const string CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/VFXConfig.asset";
        private const string BALL_PREFAB_PATH = "Assets/_Project/Prefabs/Gameplay/Ball.prefab";

        [MenuItem("RoyalSmash/Phase 17/Run Full VFX Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[Phase17Setup] Starting Phase 17 VFX Setup...");

            EnsureDirectories();

            // 1. Build Materials
            var defaultTrailMat = CreateOrLoadMaterial($"{MAT_DIR}/Mat_Trail_Default.mat", new Color(1f, 0.85f, 0.2f, 0.8f));
            var megaTrailMat = CreateOrLoadMaterial($"{MAT_DIR}/Mat_Trail_Mega.mat", new Color(1f, 0.45f, 0.05f, 0.9f));
            var infTrailMat = CreateOrLoadMaterial($"{MAT_DIR}/Mat_Trail_Infinity.mat", new Color(0.1f, 0.9f, 1f, 0.9f));
            var particleMat = CreateOrLoadMaterial($"{MAT_DIR}/Mat_Particle_Additive.mat", Color.white);

            // 2. Build Prefabs
            var muzzlePrefab = CreateMuzzleFlashPrefab($"{VFX_DIR}/VFX_MuzzleFlash.prefab", particleMat);
            var impactGenericPrefab = CreateImpactPrefab($"{VFX_DIR}/VFX_ImpactGeneric.prefab", particleMat, 10, 1.2f, false);
            var impactHeavyPrefab = CreateImpactPrefab($"{VFX_DIR}/VFX_ImpactHeavy.prefab", particleMat, 25, 2.8f, true);
            var destructPrefab = CreateDestructionPrefab($"{VFX_DIR}/VFX_ObjectDestruct.prefab", particleMat);
            var shardsPrefab = CreateBottleShardsPrefab($"{VFX_DIR}/VFX_BottleShards.prefab", particleMat);
            var spillPrefab = CreatePaintSpillPrefab($"{VFX_DIR}/VFX_PaintSpill.prefab");
            var skillPrefab = CreateSkillActivatePrefab($"{VFX_DIR}/VFX_SkillActivate.prefab", particleMat);
            var confettiPrefab = CreateWinConfettiPrefab($"{VFX_DIR}/VFX_WinConfetti.prefab", particleMat);
            var coinFlyPrefab = CreateCoinFlyPrefab($"{VFX_DIR}/UI_CoinFly.prefab");

            // 3. Configure VFXConfig
            var config = SetupVFXConfig(
                muzzlePrefab, defaultTrailMat, megaTrailMat, infTrailMat,
                impactGenericPrefab, impactHeavyPrefab, destructPrefab,
                shardsPrefab, spillPrefab, skillPrefab, confettiPrefab, coinFlyPrefab);

            // 4. Update Ball.prefab with TrailRenderer
            UpdateBallPrefab(defaultTrailMat);

            // 5. Configure Scenes
            ConfigureScene("Assets/_Project/Scenes/MainMenu.unity", config);
            ConfigureScene("Assets/_Project/Scenes/MainGame.unity", config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase17Setup] Phase 17 VFX Setup successfully completed!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(VFX_DIR)) Directory.CreateDirectory(VFX_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
            string cfgDir = Path.GetDirectoryName(CONFIG_PATH);
            if (!Directory.Exists(cfgDir)) Directory.CreateDirectory(cfgDir);
        }

        private static Material CreateOrLoadMaterial(string path, Color tint)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Universal Render Pipeline/Unlit");

                mat = new Material(shader);
                mat.color = tint;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static GameObject CreateMuzzleFlashPrefab(string path, Material mat)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("VFX_MuzzleFlash");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 0.2f;
            main.startLifetime = 0.25f;
            main.startSpeed = 4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = new Color(1f, 0.8f, 0.3f, 1f);
            main.loop = false;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.15f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateImpactPrefab(string path, Material mat, int burstCount, float speed, bool heavy)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(heavy ? "VFX_ImpactHeavy" : "VFX_ImpactGeneric");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 0.3f;
            main.startLifetime = heavy ? 0.6f : 0.35f;
            main.startSpeed = speed;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, heavy ? 0.45f : 0.25f);
            main.startColor = heavy ? new Color(1f, 0.5f, 0.1f, 1f) : new Color(1f, 0.9f, 0.6f, 1f);
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 0.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateDestructionPrefab(string path, Material mat)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("VFX_ObjectDestruct");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.8f;
            main.startSpeed = 3.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = Color.white;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 1.0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.5f, 0.5f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBottleShardsPrefab(string path, Material mat)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("VFX_BottleShards");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 0.4f;
            main.startLifetime = 0.7f;
            main.startSpeed = 5.0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = new Color(0.8f, 0.95f, 1.0f, 0.85f);
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 1.2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePaintSpillPrefab(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            // Flat quad or cylinder for lingering decal puddle
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "VFX_PaintSpill";
            go.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f);

            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateSkillActivatePrefab(string path, Material mat)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("VFX_SkillActivate");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 0.35f;
            main.startLifetime = 0.6f;
            main.startSpeed = 2.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
            main.loop = false;
            main.playOnAwake = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateWinConfettiPrefab(string path, Material mat)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("VFX_WinConfetti");
            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;

            var main = ps.main;
            main.duration = 1.0f;
            main.startLifetime = 2.0f;
            main.startSpeed = 5.0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 0.4f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.magenta, 0.5f), new GradientColorKey(Color.cyan, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 60) });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateCoinFlyPrefab(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("UI_CoinFly");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40f, 40f);

            var img = go.AddComponent<Image>();
            img.color = new Color(1.0f, 0.85f, 0.15f, 1f);

            go.AddComponent<CoinFlyAnimation>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static VFXConfig SetupVFXConfig(
            GameObject muzzle, Material defaultTrail, Material megaTrail, Material infTrail,
            GameObject impactGen, GameObject impactHvy, GameObject destruct,
            GameObject shards, GameObject spill, GameObject skill, GameObject win, GameObject coin)
        {
            var config = AssetDatabase.LoadAssetAtPath<VFXConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<VFXConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }

            SerializedObject so = new SerializedObject(config);
            so.FindProperty("_muzzleFlashPrefab").objectReferenceValue = muzzle;
            so.FindProperty("_defaultTrailMaterial").objectReferenceValue = defaultTrail;
            so.FindProperty("_megaBallTrailMaterial").objectReferenceValue = megaTrail;
            so.FindProperty("_infinityBallTrailMaterial").objectReferenceValue = infTrail;

            so.FindProperty("_impactGenericPrefab").objectReferenceValue = impactGen;
            so.FindProperty("_impactHeavyPrefab").objectReferenceValue = impactHvy;
            so.FindProperty("_objectDestructionPrefab").objectReferenceValue = destruct;

            so.FindProperty("_bottleBreakShardsPrefab").objectReferenceValue = shards;
            so.FindProperty("_paintSpillPrefab").objectReferenceValue = spill;
            so.FindProperty("_skillActivatePrefab").objectReferenceValue = skill;
            so.FindProperty("_winConfettiPrefab").objectReferenceValue = win;
            so.FindProperty("_coinFlyPrefab").objectReferenceValue = coin;

            so.FindProperty("_heavyImpactThreshold").floatValue = 3.0f;
            so.FindProperty("_paintSpillLifetime").floatValue = 4.0f;
            so.FindProperty("_defaultPoolSize").intValue = 6;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Phase17Setup] VFXConfig created and saved at {CONFIG_PATH}");
            return config;
        }

        private static void UpdateBallPrefab(Material trailMat)
        {
            var ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BALL_PREFAB_PATH);
            if (ballPrefab == null) return;

            string path = AssetDatabase.GetAssetPath(ballPrefab);
            var root = PrefabUtility.LoadPrefabContents(path);

            var trail = root.GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = root.AddComponent<TrailRenderer>();
            }

            trail.time = 0.25f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0.02f;
            trail.sharedMaterial = trailMat;
            trail.autodestruct = false;
            trail.emitting = false;

            var proj = root.GetComponent<ProjectileController>();
            if (proj != null)
            {
                SerializedObject soProj = new SerializedObject(proj);
                soProj.FindProperty("_trailRenderer").objectReferenceValue = trail;
                soProj.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"[Phase17Setup] Ball.prefab updated with configured TrailRenderer.");
        }

        private static void ConfigureScene(string scenePath, VFXConfig config)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var vfxMgr = Object.FindFirstObjectByType<VFXManager>();
            if (vfxMgr == null)
            {
                var go = new GameObject("VFXManager");
                vfxMgr = go.AddComponent<VFXManager>();
                Debug.Log($"[Phase17Setup] Created VFXManager in {scenePath}");
            }

            SerializedObject so = new SerializedObject(vfxMgr);
            so.FindProperty("_config").objectReferenceValue = config;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vfxMgr);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
