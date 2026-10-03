using System;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.VFX;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Overhauls the Core (PaintBottle) shatter VFX into a stunning, juicy Glass Shatter + Royal Navy Paint Splatter effect.
    /// Completely eliminates all magenta/pink missing-shader artifacts.
    /// </summary>
    public static class CoreVFXOverhaul
    {
        public static readonly Color CoreNavyColor = new Color(0.04f, 0.22f, 0.82f, 1.0f);   // Vibrant Lacivert / Royal Sapphire
        public static readonly Color CoreCyanColor = new Color(0.10f, 0.65f, 1.0f, 1.0f);   // Arcane Glow Cyan
        public static readonly Color CoreGoldColor = new Color(1.0f, 0.82f, 0.28f, 1.0f);   // Citadel Antique Gold Sparkle

        private const string PREFAB_PATH = "Assets/_Project/Prefabs/VFX/VFX_BottleBreak_Opt1_FluidSplash.prefab";
        private const string SPILL_PATH = "Assets/_Project/Prefabs/VFX/VFX_PaintSpill.prefab";
        private const string SPILL_MAT_PATH = "Assets/_Project/Materials/VFX/Mat_PaintSpill_Sapphire.mat";

        [MenuItem("Royal Smash/VFX/Overhaul Core Shatter Effect", false, 60)]
        public static void ApplyOverhaul()
        {
            var glassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_GlassShard.mat");
            var paintMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_PaintSplat.mat");
            var shockwaveMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_Shockwave.mat");
            var smokeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_Smoke.mat");
            var glowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_Glow_Additive.mat");
            var sparkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/VFX/Mat_Particle_Sparkle_Additive.mat");

            if (glassMat == null || paintMat == null || shockwaveMat == null || smokeMat == null || glowMat == null || sparkMat == null)
            {
                Debug.LogError("[CoreVFXOverhaul] Missing particle materials!");
                return;
            }

            // 1. Create/Update Sapphire Paint Spill Material
            var spillMat = AssetDatabase.LoadAssetAtPath<Material>(SPILL_MAT_PATH);
            if (spillMat == null)
            {
                var litShader = Shader.Find("Universal Render Pipeline/Lit");
                spillMat = new Material(litShader) { name = "Mat_PaintSpill_Sapphire" };
                AssetDatabase.CreateAsset(spillMat, SPILL_MAT_PATH);
            }
            spillMat.SetColor("_BaseColor", new Color(0.03f, 0.14f, 0.55f, 0.92f));
            if (spillMat.HasProperty("_Color")) spillMat.SetColor("_Color", new Color(0.03f, 0.14f, 0.55f, 0.92f));
            spillMat.SetFloat("_Smoothness", 0.85f);
            spillMat.SetFloat("_Metallic", 0.1f);
            EditorUtility.SetDirty(spillMat);

            // 2. Overhaul Paint Spill Prefab
            var spillPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SPILL_PATH);
            if (spillPrefab != null)
            {
                var spillRenderer = spillPrefab.GetComponentInChildren<Renderer>();
                if (spillRenderer != null)
                {
                    spillRenderer.sharedMaterial = spillMat;
                    EditorUtility.SetDirty(spillPrefab);
                }
            }

            // 3. Build Root GameObject for VFX_BottleBreak_Opt1_FluidSplash
            var root = new GameObject("VFX_BottleBreak_Opt1_FluidSplash");

            // Disable root emission and rendering completely so no default pink particle is ever emitted
            var rootPs = root.AddComponent<ParticleSystem>();
            var rootMain = rootPs.main;
            rootMain.duration = 1.5f;
            rootMain.loop = false;
            rootMain.playOnAwake = false;
            var rootEmission = rootPs.emission;
            rootEmission.enabled = false;
            var rootRenderer = root.GetComponent<ParticleSystemRenderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;

            // --- Child 1: Shockwave Ring ---
            CreateShockwave(root.transform, shockwaveMat);

            // --- Child 2: Arcane Center Glow Flash ---
            CreateArcaneFlash(root.transform, glowMat);

            // --- Child 3: Sharp Glass & Crystal Shards ---
            CreateGlassShards(root.transform, glassMat);

            // --- Child 4: Royal Navy / Sapphire Paint Droplets (Liquid Splatter) ---
            CreatePaintDroplets(root.transform, paintMat);

            // --- Child 5: Golden & Arcane Sparkles ---
            CreateGoldenSparks(root.transform, sparkMat);

            // --- Child 6: Arcane Mist Puff ---
            CreateArcaneMist(root.transform, smokeMat);

            // 4. Save as Prefab
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(root);

            // 5. Update PF_core and PF_Block_05_Core prefabs
            UpdateCorePrefabColor("Assets/_Project/Art/SkyCitadel/Prefabs/PF_core.prefab");
            UpdateCorePrefabColor("Assets/_Project/Prefabs/SkyCitadel/Blocks/PF_Block_05_Core.prefab");
            UpdateCorePrefabColor("Assets/_Project/Prefabs/Gameplay/PaintBottle.prefab");

            // 6. Ensure VFXConfig links are solid
            var vfxConfig = AssetDatabase.LoadAssetAtPath<VFXConfig>("Assets/_Project/ScriptableObjects/Config/VFXConfig.asset");
            if (vfxConfig != null)
            {
                var mainShatter = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                vfxConfig.BottleBreakShardsPrefab = mainShatter;
                EditorUtility.SetDirty(vfxConfig);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00D4FF><b>[CoreVFXOverhaul] Successfully overhauled Core Glass Shatter & Navy Paint Splatter VFX!</b></color>");
        }

        private static void CreateShockwave(Transform parent, Material mat)
        {
            var go = new GameObject("ShockwaveRing");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.20f;
            main.startSpeed = 0f;
            main.startSize = 0.30f;
            main.startColor = Color.white;
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.01f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.55f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(CoreCyanColor, 0.35f), new GradientColorKey(CoreNavyColor, 0.8f) },
                new[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.85f, 0.25f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void CreateArcaneFlash(Transform parent, Material mat)
        {
            var go = new GameObject("ArcaneFlash");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.15f;
            main.loop = false;
            main.startLifetime = 0.12f;
            main.startSpeed = 0f;
            main.startSize = 1.35f;
            main.startColor = Color.white;
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(CoreCyanColor, 0.5f), new GradientColorKey(CoreNavyColor, 1f) },
                new[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.7f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.EaseInOut(0f, 0.85f, 1f, 1.2f));

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void CreateGlassShards(Transform parent, Material mat)
        {
            var go = new GameObject("GlassShards");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.40f, 0.70f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startColor = new Color(0.92f, 0.97f, 1.0f, 0.95f); // Crystalline clean glass
            main.gravityModifier = 2.8f; // Tumble down fast like heavy glass
            main.startRotation = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.dampen = 0.35f;
            limitVel.limit = new ParticleSystem.MinMaxCurve(2.0f);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.Linear(0f, 1.0f, 1f, 0.3f));

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-540f * Mathf.Deg2Rad, 540f * Mathf.Deg2Rad);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.85f, 0.95f, 1.0f), 0.5f), new GradientColorKey(CoreCyanColor, 1f) },
                new[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.95f, 0.6f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void CreatePaintDroplets(Transform parent, Material mat)
        {
            var go = new GameObject("PaintDroplets");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.38f);
            main.startColor = CoreNavyColor;
            main.gravityModifier = 3.2f; // Liquid splashes down fast
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.dampen = 0.38f;
            limitVel.limit = new ParticleSystem.MinMaxCurve(1.8f);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.Linear(0f, 1.0f, 1f, 0.35f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.08f, 0.35f, 0.95f), 0f), new GradientColorKey(CoreNavyColor, 0.5f), new GradientColorKey(new Color(0.02f, 0.10f, 0.45f), 1f) },
                new[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.95f, 0.75f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void CreateGoldenSparks(Transform parent, Material mat)
        {
            var go = new GameObject("GoldenSparks");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.18f);
            main.startColor = CoreGoldColor;
            main.gravityModifier = 0.40f;
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.dampen = 0.40f;
            limitVel.limit = new ParticleSystem.MinMaxCurve(1.5f);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.Linear(0f, 1.0f, 1f, 0f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.98f, 0.7f), 0f), new GradientColorKey(CoreGoldColor, 0.5f), new GradientColorKey(CoreCyanColor, 1f) },
                new[] { new GradientAlphaKey(1.0f, 0f), new GradientAlphaKey(0.90f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void CreateArcaneMist(Transform parent, Material mat)
        {
            var go = new GameObject("ArcaneMist");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.48f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            main.startColor = new Color(0.04f, 0.16f, 0.65f, 0.32f);
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 5) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1.0f, AnimationCurve.Linear(0f, 0.8f, 1f, 1.4f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(CoreNavyColor, 0f), new GradientColorKey(CoreCyanColor, 1f) },
                new[] { new GradientAlphaKey(0.32f, 0f), new GradientAlphaKey(0.20f, 0.35f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;
            rend.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static void UpdateCorePrefabColor(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var pbo = prefab.GetComponent<PaintBottleObject>();
            if (pbo != null)
            {
                var so = new SerializedObject(pbo);
                var prop = so.FindProperty("_paintColor");
                if (prop != null)
                {
                    prop.colorValue = CoreNavyColor;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(prefab);
                }
            }
        }
    }
}
