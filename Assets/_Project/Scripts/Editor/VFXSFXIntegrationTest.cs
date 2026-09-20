#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Audio;
using RoyalSmash.VFX;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automated integration tests for the VFX and SFX ecosystems.
    /// Validates all 3 variants per effect and sound event per user specifications.
    /// </summary>
    public static class VFXSFXIntegrationTest
    {
        [MenuItem("RoyalSmash/Tests/Run VFX & SFX Verification Suite")]
        public static void RunAllTests()
        {
            Debug.Log("<color=cyan>[VFXSFXTest] === Starting VFX & SFX Verification Suite ===</color>");

            TestAudioConfigAndClips();
            TestVFXConfigAndPrefabs();
            TestRuntimeParticleInstantiation();

            Debug.Log("<color=green>[VFXSFXTest] === ALL VFX & SFX INTEGRATION TESTS PASSED! ===</color>");
        }

        public static void TestAudioConfigAndClips()
        {
            var config = AssetDatabase.LoadAssetAtPath<AudioConfig>("Assets/_Project/ScriptableObjects/Config/AudioConfig.asset");
            Assert.IsNotNull(config, "AudioConfig.asset must exist");

            // Verify Primary Clips
            Assert.IsNotNull(config.BgmMainMenu, "BgmMainMenu must be assigned");
            Assert.IsNotNull(config.BgmGameplay, "BgmGameplay must be assigned");
            Assert.IsNotNull(config.BallLaunch, "BallLaunch must be assigned");
            Assert.IsNotNull(config.BoxImpact, "BoxImpact must be assigned");
            Assert.IsNotNull(config.StoneImpact, "StoneImpact must be assigned");
            Assert.IsNotNull(config.ColumnImpact, "ColumnImpact must be assigned");
            Assert.IsNotNull(config.ConeImpact, "ConeImpact must be assigned");
            Assert.IsNotNull(config.PaintBottleBreak, "PaintBottleBreak must be assigned");
            Assert.IsNotNull(config.ObjectDestruction, "ObjectDestruction must be assigned");
            Assert.IsNotNull(config.ButtonClick, "ButtonClick must be assigned");
            Assert.IsNotNull(config.PanelOpen, "PanelOpen must be assigned");
            Assert.IsNotNull(config.GameStart, "GameStart must be assigned");
            Assert.IsNotNull(config.WinFanfare, "WinFanfare must be assigned");
            Assert.IsNotNull(config.LoseSound, "LoseSound must be assigned");
            Assert.IsNotNull(config.RewardSound, "RewardSound must be assigned");

            // Verify Alternative Clips
            Assert.IsNotNull(config.BgmMainMenuAlt1, "BgmMainMenuAlt1 must be assigned");
            Assert.IsNotNull(config.BgmMainMenuAlt2, "BgmMainMenuAlt2 must be assigned");
            Assert.IsNotNull(config.BgmGameplayAlt1, "BgmGameplayAlt1 must be assigned");
            Assert.IsNotNull(config.BgmGameplayAlt2, "BgmGameplayAlt2 must be assigned");

            Assert.IsNotNull(config.BallLaunchAlt1, "BallLaunchAlt1 must be assigned");
            Assert.IsNotNull(config.BallLaunchAlt2, "BallLaunchAlt2 must be assigned");

            Assert.IsNotNull(config.BoxImpactAlt1, "BoxImpactAlt1 must be assigned");
            Assert.IsNotNull(config.BoxImpactAlt2, "BoxImpactAlt2 must be assigned");
            Assert.IsNotNull(config.StoneImpactAlt1, "StoneImpactAlt1 must be assigned");
            Assert.IsNotNull(config.StoneImpactAlt2, "StoneImpactAlt2 must be assigned");
            Assert.IsNotNull(config.ColumnImpactAlt1, "ColumnImpactAlt1 must be assigned");
            Assert.IsNotNull(config.ColumnImpactAlt2, "ColumnImpactAlt2 must be assigned");
            Assert.IsNotNull(config.ConeImpactAlt1, "ConeImpactAlt1 must be assigned");
            Assert.IsNotNull(config.ConeImpactAlt2, "ConeImpactAlt2 must be assigned");

            Assert.IsNotNull(config.PaintBottleBreakAlt1, "PaintBottleBreakAlt1 must be assigned");
            Assert.IsNotNull(config.PaintBottleBreakAlt2, "PaintBottleBreakAlt2 must be assigned");

            Assert.IsNotNull(config.ObjectDestructionAlt1, "ObjectDestructionAlt1 must be assigned");
            Assert.IsNotNull(config.ObjectDestructionAlt2, "ObjectDestructionAlt2 must be assigned");

            Assert.IsNotNull(config.ButtonClickAlt1, "ButtonClickAlt1 must be assigned");
            Assert.IsNotNull(config.ButtonClickAlt2, "ButtonClickAlt2 must be assigned");
            Assert.IsNotNull(config.PanelOpenAlt1, "PanelOpenAlt1 must be assigned");
            Assert.IsNotNull(config.PanelOpenAlt2, "PanelOpenAlt2 must be assigned");
            Assert.IsNotNull(config.GameStartAlt1, "GameStartAlt1 must be assigned");
            Assert.IsNotNull(config.GameStartAlt2, "GameStartAlt2 must be assigned");

            Assert.IsNotNull(config.WinFanfareAlt1, "WinFanfareAlt1 must be assigned");
            Assert.IsNotNull(config.WinFanfareAlt2, "WinFanfareAlt2 must be assigned");
            Assert.IsNotNull(config.LoseSoundAlt1, "LoseSoundAlt1 must be assigned");
            Assert.IsNotNull(config.LoseSoundAlt2, "LoseSoundAlt2 must be assigned");
            Assert.IsNotNull(config.RewardSoundAlt1, "RewardSoundAlt1 must be assigned");
            Assert.IsNotNull(config.RewardSoundAlt2, "RewardSoundAlt2 must be assigned");

            // Verify Audio lengths are valid (> 0s)
            Assert.Greater(config.BallLaunch.length, 0.1f);
            Assert.Greater(config.PaintBottleBreak.length, 0.1f);
            Assert.Greater(config.BgmGameplay.length, 1.0f);
            Assert.Greater(config.BgmMainMenu.length, 1.0f);

            Debug.Log("<color=green>[VFXSFXTest] AudioConfig and all 3x SFX / Music variants verified!</color>");
        }

        public static void TestVFXConfigAndPrefabs()
        {
            var config = AssetDatabase.LoadAssetAtPath<VFXConfig>("Assets/_Project/ScriptableObjects/Config/VFXConfig.asset");
            Assert.IsNotNull(config, "VFXConfig.asset must exist");

            // Trails
            Assert.IsNotNull(config.DefaultTrailMaterial, "DefaultTrailMaterial must be assigned");
            Assert.IsNotNull(config.DefaultTrailAlt1Material, "DefaultTrailAlt1Material must be assigned");
            Assert.IsNotNull(config.DefaultTrailAlt2Material, "DefaultTrailAlt2Material must be assigned");

            // Muzzle Flash
            Assert.IsNotNull(config.MuzzleFlashPrefab, "MuzzleFlashPrefab must be assigned");
            Assert.IsNotNull(config.MuzzleFlashAlt1Prefab, "MuzzleFlashAlt1Prefab must be assigned");
            Assert.IsNotNull(config.MuzzleFlashAlt2Prefab, "MuzzleFlashAlt2Prefab must be assigned");

            // Bottle Break
            Assert.IsNotNull(config.BottleBreakShardsPrefab, "BottleBreakShardsPrefab must be assigned");
            Assert.IsNotNull(config.BottleBreakShardsAlt1Prefab, "BottleBreakShardsAlt1Prefab must be assigned");
            Assert.IsNotNull(config.BottleBreakShardsAlt2Prefab, "BottleBreakShardsAlt2Prefab must be assigned");

            // Impacts
            Assert.IsNotNull(config.ImpactGenericPrefab, "ImpactGenericPrefab must be assigned");
            Assert.IsNotNull(config.ImpactGenericAlt1Prefab, "ImpactGenericAlt1Prefab must be assigned");
            Assert.IsNotNull(config.ImpactGenericAlt2Prefab, "ImpactGenericAlt2Prefab must be assigned");

            // Win Confetti
            Assert.IsNotNull(config.WinConfettiPrefab, "WinConfettiPrefab must be assigned");
            Assert.IsNotNull(config.WinConfettiAlt1Prefab, "WinConfettiAlt1Prefab must be assigned");
            Assert.IsNotNull(config.WinConfettiAlt2Prefab, "WinConfettiAlt2Prefab must be assigned");

            Debug.Log("<color=green>[VFXSFXTest] VFXConfig and all 3x VFX variants verified!</color>");
        }

        public static void TestRuntimeParticleInstantiation()
        {
            var config = AssetDatabase.LoadAssetAtPath<VFXConfig>("Assets/_Project/ScriptableObjects/Config/VFXConfig.asset");

            GameObject[] prefabsToTest = new GameObject[]
            {
                config.BottleBreakShardsPrefab,
                config.BottleBreakShardsAlt1Prefab,
                config.BottleBreakShardsAlt2Prefab,
                config.ImpactGenericPrefab,
                config.ImpactGenericAlt1Prefab,
                config.ImpactGenericAlt2Prefab,
                config.MuzzleFlashPrefab,
                config.MuzzleFlashAlt1Prefab,
                config.MuzzleFlashAlt2Prefab,
                config.WinConfettiPrefab,
                config.WinConfettiAlt1Prefab,
                config.WinConfettiAlt2Prefab
            };

            foreach (var prefab in prefabsToTest)
            {
                Assert.IsNotNull(prefab, "Prefab to test must not be null");
                GameObject instance = Object.Instantiate(prefab);
                Assert.IsNotNull(instance);

                var particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
                Assert.Greater(particleSystems.Length, 0, $"Prefab {prefab.name} must have at least one ParticleSystem");

                foreach (var ps in particleSystems)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    Assert.IsNotNull(renderer, $"ParticleSystem on {ps.gameObject.name} must have a ParticleSystemRenderer");
                    if (renderer.enabled)
                    {
                        Assert.IsNotNull(renderer.sharedMaterial, $"ParticleSystemRenderer on {ps.gameObject.name} must have a valid sharedMaterial");
                    }
                }

                Object.DestroyImmediate(instance);
            }

            Debug.Log("<color=green>[VFXSFXTest] Runtime Particle Instantiation and Component Integrity verified!</color>");
        }
    }
}
#endif
