#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoyalSmash.EditorTools
{
    /// <summary>
    /// Procedurally synthesizes clean 16-bit 44.1kHz PCM WAV audio assets for Royal Smash.
    /// Provides 3 distinct variations (opt1, opt2, opt3) for all game SFX categories.
    /// </summary>
    public static class AudioSynthesizerTool
    {
        private const int SampleRate = 44100;
        private const string SfxBaseDir = "Assets/_Project/Audio/SFX/";

        public static void GenerateAllMissingSfx()
        {
            EnsureDirectory(SfxBaseDir + "UI");
            EnsureDirectory(SfxBaseDir + "Destruction");
            EnsureDirectory(SfxBaseDir + "Skills");
            EnsureDirectory(SfxBaseDir + "GameState");
            EnsureDirectory(SfxBaseDir + "Combat");

            // 1. Panel Close
            WriteWav(SfxBaseDir + "UI/sfx_panel_close_opt1_smooth_zip.wav", GenWhoosh(0.22f, 800f, 150f, true));
            WriteWav(SfxBaseDir + "UI/sfx_panel_close_opt2_gentle_pop.wav", GenPop(0.15f, 400f, 180f));
            WriteWav(SfxBaseDir + "UI/sfx_panel_close_opt3_wooden_latch.wav", GenClick(0.18f, 650f, 220f));

            // 2. Shop Purchase Confirmed
            WriteWav(SfxBaseDir + "UI/sfx_purchase_opt1_royal_chime.wav", GenChimeChord(new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.65f));
            WriteWav(SfxBaseDir + "UI/sfx_purchase_opt2_gold_chest.wav", GenCoinPayout(0.70f));
            WriteWav(SfxBaseDir + "UI/sfx_purchase_opt3_sparkle_burst.wav", GenSparkleSweep(0.60f, 600f, 2400f));

            // 3. Next Level Transition
            WriteWav(SfxBaseDir + "GameState/sfx_transition_opt1_wind_whoosh.wav", GenWhoosh(0.55f, 200f, 1200f, false));
            WriteWav(SfxBaseDir + "GameState/sfx_transition_opt2_celestial_glide.wav", GenGlide(0.60f, 440f, 880f));
            WriteWav(SfxBaseDir + "GameState/sfx_transition_opt3_speed_dash.wav", GenWhoosh(0.35f, 400f, 1600f, false));

            // 4. Box Destroy (Wood)
            WriteWav(SfxBaseDir + "Destruction/sfx_box_destroy_opt1_wood_shatter.wav", GenWoodDestruction(0.35f, 220f, 1.2f));
            WriteWav(SfxBaseDir + "Destruction/sfx_box_destroy_opt2_heavy_crate.wav", GenWoodDestruction(0.45f, 160f, 1.8f));
            WriteWav(SfxBaseDir + "Destruction/sfx_box_destroy_opt3_hollow_bust.wav", GenWoodDestruction(0.28f, 280f, 0.9f));

            // 5. Stone Destroy
            WriteWav(SfxBaseDir + "Destruction/sfx_stone_destroy_opt1_rock_crush.wav", GenStoneDestruction(0.42f, 130f));
            WriteWav(SfxBaseDir + "Destruction/sfx_stone_destroy_opt2_granite_shatter.wav", GenStoneDestruction(0.50f, 110f));
            WriteWav(SfxBaseDir + "Destruction/sfx_stone_destroy_opt3_slate_collapse.wav", GenStoneDestruction(0.38f, 170f));

            // 6. Column Destroy (Heavy Pillar)
            WriteWav(SfxBaseDir + "Destruction/sfx_column_destroy_opt1_pillar_collapse.wav", GenPillarDestruction(0.55f, 95f));
            WriteWav(SfxBaseDir + "Destruction/sfx_column_destroy_opt2_marble_break.wav", GenPillarDestruction(0.48f, 120f));
            WriteWav(SfxBaseDir + "Destruction/sfx_column_destroy_opt3_ancient_crash.wav", GenPillarDestruction(0.62f, 80f));

            // 7. Cone Destroy (Metal)
            WriteWav(SfxBaseDir + "Destruction/sfx_cone_destroy_opt1_metal_crumple.wav", GenMetalDestruction(0.38f, 850f));
            WriteWav(SfxBaseDir + "Destruction/sfx_cone_destroy_opt2_tin_burst.wav", GenMetalDestruction(0.30f, 1200f));
            WriteWav(SfxBaseDir + "Destruction/sfx_cone_destroy_opt3_iron_clatter.wav", GenMetalDestruction(0.45f, 650f));

            // 8. Skill Mega Ball
            WriteWav(SfxBaseDir + "Skills/sfx_mega_launch_opt1_thunder_blast.wav", GenHeavyBlast(0.65f, 65f, 2.5f));
            WriteWav(SfxBaseDir + "Skills/sfx_mega_launch_opt2_titan_roar.wav", GenHeavyBlast(0.75f, 50f, 3.0f));
            WriteWav(SfxBaseDir + "Skills/sfx_mega_launch_opt3_shockwave.wav", GenHeavyBlast(0.55f, 80f, 2.0f));

            // 9. Skill Infinity Ball
            WriteWav(SfxBaseDir + "Skills/sfx_infinity_launch_opt1_cosmic_beam.wav", GenCosmicPulse(0.70f, 523.25f));
            WriteWav(SfxBaseDir + "Skills/sfx_infinity_launch_opt2_portal_warp.wav", GenCosmicPulse(0.80f, 440f));
            WriteWav(SfxBaseDir + "Skills/sfx_infinity_launch_opt3_celestial_sweep.wav", GenCosmicPulse(0.65f, 659.25f));

            // 10. Star Award (Win stars)
            WriteWav(SfxBaseDir + "GameState/sfx_star_opt1_crystal_ping.wav", GenPing(0.40f, 1318.51f)); // E6
            WriteWav(SfxBaseDir + "GameState/sfx_star_opt2_golden_sparkle.wav", GenPing(0.45f, 1567.98f)); // G6
            WriteWav(SfxBaseDir + "GameState/sfx_star_opt3_arcade_star.wav", GenPing(0.35f, 1760.00f)); // A6

            // 11. Combo Destroy
            WriteWav(SfxBaseDir + "Destruction/sfx_combo_opt1_triad_chime.wav", GenCombo(0.50f, 440f, 554.37f, 659.25f));
            WriteWav(SfxBaseDir + "Destruction/sfx_combo_opt2_fever_bells.wav", GenCombo(0.55f, 523.25f, 659.25f, 783.99f));
            WriteWav(SfxBaseDir + "Destruction/sfx_combo_opt3_power_surge.wav", GenCombo(0.60f, 392f, 493.88f, 587.33f));

            // 12. Ball Bounce
            WriteWav(SfxBaseDir + "Combat/sfx_bounce_opt1_rubber_thud.wav", GenBounce(0.12f, 220f, 80f));
            WriteWav(SfxBaseDir + "Combat/sfx_bounce_opt2_solid_clack.wav", GenBounce(0.10f, 340f, 120f));
            WriteWav(SfxBaseDir + "Combat/sfx_bounce_opt3_cushion_tap.wav", GenBounce(0.14f, 180f, 60f));

            // 13. Cannon Aim Tension
            WriteWav(SfxBaseDir + "Combat/sfx_aim_opt1_tension_creak.wav", GenTension(0.30f, 250f, 450f));
            WriteWav(SfxBaseDir + "Combat/sfx_aim_opt2_spring_stretch.wav", GenTension(0.25f, 320f, 600f));
            WriteWav(SfxBaseDir + "Combat/sfx_aim_opt3_gear_ratchet.wav", GenRatchet(0.28f, 8));

            AssetDatabase.Refresh();
            AssignAllClipsToConfig();
        }

        public static void AssignAllClipsToConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<RoyalSmash.Audio.AudioConfig>(
                "Assets/_Project/ScriptableObjects/Config/AudioConfig.asset");
            if (config == null) return;

            // BGM
            config.BgmMainMenu = LoadClip("Assets/_Project/Audio/Music/bgm_menu_opt1_royal_courtyard.mp3");
            config.BgmMainMenuAlt1 = LoadClip("Assets/_Project/Audio/Music/bgm_menu_opt2_ballroom_waltz.mp3");
            config.BgmMainMenuAlt2 = LoadClip("Assets/_Project/Audio/Music/bgm_menu_opt3_cozy_tavern.mp3");
            config.BgmGameplay = LoadClip("Assets/_Project/Audio/Music/bgm_game_opt1_puzzle_march.mp3");
            config.BgmGameplayAlt1 = LoadClip("Assets/_Project/Audio/Music/bgm_game_opt2_run_amok.mp3");
            config.BgmGameplayAlt2 = LoadClip("Assets/_Project/Audio/Music/bgm_game_opt3_wagon_wheel.mp3");

            // Combat & Launch
            config.BallLaunch = LoadClip(SfxBaseDir + "BallLaunch/sfx_launch_opt1_heavy_cannon.wav");
            config.BallLaunchAlt1 = LoadClip(SfxBaseDir + "BallLaunch/sfx_launch_opt2_catapult_boom.wav");
            config.BallLaunchAlt2 = LoadClip(SfxBaseDir + "BallLaunch/sfx_launch_opt3_arcade_pow.wav");

            config.CannonAim = LoadClip(SfxBaseDir + "Combat/sfx_aim_opt1_tension_creak.wav");
            config.CannonAimAlt1 = LoadClip(SfxBaseDir + "Combat/sfx_aim_opt2_spring_stretch.wav");
            config.CannonAimAlt2 = LoadClip(SfxBaseDir + "Combat/sfx_aim_opt3_gear_ratchet.wav");

            config.BallBounce = LoadClip(SfxBaseDir + "Combat/sfx_bounce_opt1_rubber_thud.wav");
            config.BallBounceAlt1 = LoadClip(SfxBaseDir + "Combat/sfx_bounce_opt2_solid_clack.wav");
            config.BallBounceAlt2 = LoadClip(SfxBaseDir + "Combat/sfx_bounce_opt3_cushion_tap.wav");

            // Skills
            config.SkillMegaBall = LoadClip(SfxBaseDir + "Skills/sfx_mega_launch_opt1_thunder_blast.wav");
            config.SkillMegaBallAlt1 = LoadClip(SfxBaseDir + "Skills/sfx_mega_launch_opt2_titan_roar.wav");
            config.SkillMegaBallAlt2 = LoadClip(SfxBaseDir + "Skills/sfx_mega_launch_opt3_shockwave.wav");

            config.SkillInfinityBall = LoadClip(SfxBaseDir + "Skills/sfx_infinity_launch_opt1_cosmic_beam.wav");
            config.SkillInfinityBallAlt1 = LoadClip(SfxBaseDir + "Skills/sfx_infinity_launch_opt2_portal_warp.wav");
            config.SkillInfinityBallAlt2 = LoadClip(SfxBaseDir + "Skills/sfx_infinity_launch_opt3_celestial_sweep.wav");

            // Impacts
            config.BoxImpact = LoadClip(SfxBaseDir + "Impacts/sfx_impact_box_opt1.wav");
            config.BoxImpactAlt1 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_box_opt2.wav");
            config.BoxImpactAlt2 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_box_opt3.wav");

            config.StoneImpact = LoadClip(SfxBaseDir + "Impacts/sfx_impact_stone_opt1.wav");
            config.StoneImpactAlt1 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_stone_opt2.wav");
            config.StoneImpactAlt2 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_stone_opt3.wav");

            config.ColumnImpact = LoadClip(SfxBaseDir + "Impacts/sfx_impact_column_opt1.wav");
            config.ColumnImpactAlt1 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_column_opt2.wav");
            config.ColumnImpactAlt2 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_column_opt3.wav");

            config.ConeImpact = LoadClip(SfxBaseDir + "Impacts/sfx_impact_cone_opt1.wav");
            config.ConeImpactAlt1 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_cone_opt2.wav");
            config.ConeImpactAlt2 = LoadClip(SfxBaseDir + "Impacts/sfx_impact_cone_opt3.wav");

            // Destruction
            config.BoxDestroy = LoadClip(SfxBaseDir + "Destruction/sfx_box_destroy_opt1_wood_shatter.wav");
            config.BoxDestroyAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_box_destroy_opt2_heavy_crate.wav");
            config.BoxDestroyAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_box_destroy_opt3_hollow_bust.wav");

            config.StoneDestroy = LoadClip(SfxBaseDir + "Destruction/sfx_stone_destroy_opt1_rock_crush.wav");
            config.StoneDestroyAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_stone_destroy_opt2_granite_shatter.wav");
            config.StoneDestroyAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_stone_destroy_opt3_slate_collapse.wav");

            config.ColumnDestroy = LoadClip(SfxBaseDir + "Destruction/sfx_column_destroy_opt1_pillar_collapse.wav");
            config.ColumnDestroyAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_column_destroy_opt2_marble_break.wav");
            config.ColumnDestroyAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_column_destroy_opt3_ancient_crash.wav");

            config.ConeDestroy = LoadClip(SfxBaseDir + "Destruction/sfx_cone_destroy_opt1_metal_crumple.wav");
            config.ConeDestroyAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_cone_destroy_opt2_tin_burst.wav");
            config.ConeDestroyAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_cone_destroy_opt3_iron_clatter.wav");

            config.PaintBottleBreak = LoadClip(SfxBaseDir + "BottleBreak/sfx_bottle_opt1_glass_splash.wav");
            config.PaintBottleBreakAlt1 = LoadClip(SfxBaseDir + "BottleBreak/sfx_bottle_opt2_ceramic_squish.wav");
            config.PaintBottleBreakAlt2 = LoadClip(SfxBaseDir + "BottleBreak/sfx_bottle_opt3_crystal_burst.wav");

            config.ObjectDestruction = LoadClip(SfxBaseDir + "Destruction/sfx_destruct_opt1_debris_crumble.wav");
            config.ObjectDestructionAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_destruct_opt2_explosive_burst.wav");
            config.ObjectDestructionAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_destruct_opt3_arcade_crunch.wav");

            config.ComboDestroy = LoadClip(SfxBaseDir + "Destruction/sfx_combo_opt1_triad_chime.wav");
            config.ComboDestroyAlt1 = LoadClip(SfxBaseDir + "Destruction/sfx_combo_opt2_fever_bells.wav");
            config.ComboDestroyAlt2 = LoadClip(SfxBaseDir + "Destruction/sfx_combo_opt3_power_surge.wav");

            // UI
            config.ButtonClick = LoadClip(SfxBaseDir + "UI/sfx_click_opt1_casual_pop.wav");
            config.ButtonClickAlt1 = LoadClip(SfxBaseDir + "UI/sfx_click_opt2_royal_click.wav");
            config.ButtonClickAlt2 = LoadClip(SfxBaseDir + "UI/sfx_click_opt3_bubble_snap.wav");

            config.PanelOpen = LoadClip(SfxBaseDir + "UI/sfx_panel_open_opt1_airy_whoosh.wav");
            config.PanelOpenAlt1 = LoadClip(SfxBaseDir + "UI/sfx_panel_open_opt2_royal_swoosh.wav");
            config.PanelOpenAlt2 = LoadClip(SfxBaseDir + "UI/sfx_panel_open_opt3_slide_pop.wav");

            config.PanelClose = LoadClip(SfxBaseDir + "UI/sfx_panel_close_opt1_smooth_zip.wav");
            config.PanelCloseAlt1 = LoadClip(SfxBaseDir + "UI/sfx_panel_close_opt2_gentle_pop.wav");
            config.PanelCloseAlt2 = LoadClip(SfxBaseDir + "UI/sfx_panel_close_opt3_wooden_latch.wav");

            config.ShopPurchase = LoadClip(SfxBaseDir + "UI/sfx_purchase_opt1_royal_chime.wav");
            config.ShopPurchaseAlt1 = LoadClip(SfxBaseDir + "UI/sfx_purchase_opt2_gold_chest.wav");
            config.ShopPurchaseAlt2 = LoadClip(SfxBaseDir + "UI/sfx_purchase_opt3_sparkle_burst.wav");

            // Game States
            config.GameStart = LoadClip(SfxBaseDir + "GameState/sfx_start_opt1_royal_horn.wav");
            config.GameStartAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_start_opt2_ascending_harp.wav");
            config.GameStartAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_start_opt3_drum_roll.wav");

            config.WinFanfare = LoadClip(SfxBaseDir + "GameState/sfx_win_opt1_royal_brass.wav");
            config.WinFanfareAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_win_opt2_orchestral_jingle.wav");
            config.WinFanfareAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_win_opt3_celebratory_bells.wav");

            config.LoseSound = LoadClip(SfxBaseDir + "GameState/sfx_lose_opt1_melodic_groan.wav");
            config.LoseSoundAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_lose_opt2_wah_wah.wav");
            config.LoseSoundAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_lose_opt3_soft_chimes.wav");

            config.RewardSound = LoadClip(SfxBaseDir + "GameState/sfx_coin_opt1_sparkle_cascade.wav");
            config.RewardSoundAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_coin_opt2_heavy_gold_clink.wav");
            config.RewardSoundAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_coin_opt3_arcade_ding.wav");

            config.StarAward = LoadClip(SfxBaseDir + "GameState/sfx_star_opt1_crystal_ping.wav");
            config.StarAwardAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_star_opt2_golden_sparkle.wav");
            config.StarAwardAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_star_opt3_arcade_star.wav");

            config.NextLevelTransition = LoadClip(SfxBaseDir + "GameState/sfx_transition_opt1_wind_whoosh.wav");
            config.NextLevelTransitionAlt1 = LoadClip(SfxBaseDir + "GameState/sfx_transition_opt2_celestial_glide.wav");
            config.NextLevelTransitionAlt2 = LoadClip(SfxBaseDir + "GameState/sfx_transition_opt3_speed_dash.wav");

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
        }

        private static AudioClip LoadClip(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning($"[AudioSynthesizerTool] Missing audio clip: {path}");
            return clip;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        #region Sound Generators

        private static float[] GenWhoosh(float duration, float startFreq, float endFreq, bool reverse)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Sin(t * Mathf.PI); // Smooth arc

                float whiteNoise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float sine = Mathf.Sin(2f * Mathf.PI * currentFreq * (float)i / SampleRate);
                samples[i] = (sine * 0.4f + whiteNoise * 0.6f) * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenPop(float duration, float startFreq, float endFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float freq = Mathf.Lerp(startFreq, endFreq, t * t);
                float env = Mathf.Pow(1f - t, 2.5f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate) * env * 0.75f;
            }
            return samples;
        }

        private static float[] GenClick(float duration, float freq1, float freq2)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 24f);
                float s1 = Mathf.Sin(2f * Mathf.PI * freq1 * (float)i / SampleRate);
                float s2 = Mathf.Sin(2f * Mathf.PI * freq2 * (float)i / SampleRate);
                samples[i] = (s1 * 0.6f + s2 * 0.4f) * env * 0.8f;
            }
            return samples;
        }

        private static float[] GenChimeChord(float[] frequencies, float duration)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int f = 0; f < frequencies.Length; f++)
            {
                float freq = frequencies[f];
                float delay = f * 0.045f;
                int startSample = (int)(delay * SampleRate);

                for (int i = startSample; i < numSamples; i++)
                {
                    float localT = (float)(i - startSample) / (numSamples - startSample);
                    float env = Mathf.Exp(-localT * 6.5f);
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * (float)(i - startSample) / SampleRate) +
                                 0.25f * Mathf.Sin(4f * Mathf.PI * freq * (float)(i - startSample) / SampleRate);
                    samples[i] += wave * env * (0.5f / frequencies.Length);
                }
            }
            return samples;
        }

        private static float[] GenCoinPayout(float duration)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            float[] notes = { 987.77f, 1318.51f, 1567.98f, 1975.53f, 2637.02f };
            System.Random rand = new System.Random(101);

            for (int n = 0; n < notes.Length; n++)
            {
                float note = notes[n];
                int startSample = (int)(n * 0.08f * SampleRate);

                for (int i = startSample; i < numSamples; i++)
                {
                    float t = (float)(i - startSample) / (SampleRate * 0.35f);
                    if (t > 1f) break;
                    float env = Mathf.Exp(-t * 7f);
                    float bell = Mathf.Sin(2f * Mathf.PI * note * (float)(i - startSample) / SampleRate);
                    samples[i] += bell * env * 0.3f;
                }
            }
            return samples;
        }

        private static float[] GenSparkleSweep(float duration, float startFreq, float endFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(77);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, Mathf.Sqrt(t));
                float env = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * 2f);
                float sparkle = Mathf.Sin(2f * Mathf.PI * currentFreq * (float)i / SampleRate);
                float overtone = Mathf.Sin(4f * Mathf.PI * currentFreq * (float)i / SampleRate);
                samples[i] = (sparkle * 0.7f + overtone * 0.3f) * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenGlide(float duration, float startFreq, float endFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Sin(t * Mathf.PI);
                float sine = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate);
                samples[i] = sine * env * 0.6f;
            }
            return samples;
        }

        private static float[] GenWoodDestruction(float duration, float baseFreq, float punch)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(88);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 12f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 16f);
                float tone = Mathf.Sin(2f * Mathf.PI * baseFreq * (1f - t * 0.5f) * (float)i / SampleRate);
                samples[i] = (tone * 0.5f * punch + noise * 0.6f) * env * 0.8f;
            }
            return samples;
        }

        private static float[] GenStoneDestruction(float duration, float rumbleFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(99);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 9f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 8f);
                float sub = Mathf.Sin(2f * Mathf.PI * rumbleFreq * (1f - t * 0.4f) * (float)i / SampleRate);
                samples[i] = (sub * 0.7f + noise * 0.5f) * env * 0.85f;
            }
            return samples;
        }

        private static float[] GenPillarDestruction(float duration, float deepFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(111);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 6f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 7f);
                float rumble = Mathf.Sin(2f * Mathf.PI * deepFreq * (1f - t * 0.3f) * (float)i / SampleRate);
                samples[i] = (rumble * 0.8f + noise * 0.4f) * env * 0.9f;
            }
            return samples;
        }

        private static float[] GenMetalDestruction(float duration, float metalFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(222);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 11f);
                float clang1 = Mathf.Sin(2f * Mathf.PI * metalFreq * (float)i / SampleRate);
                float clang2 = Mathf.Sin(2f * Mathf.PI * (metalFreq * 1.58f) * (float)i / SampleRate);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 18f);
                samples[i] = (clang1 * 0.4f + clang2 * 0.3f + noise * 0.4f) * env * 0.8f;
            }
            return samples;
        }

        private static float[] GenHeavyBlast(float duration, float subFreq, float impactMult)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            System.Random rand = new System.Random(333);

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 7f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 14f);
                float sub = Mathf.Sin(2f * Mathf.PI * (subFreq * (1f - t * 0.5f)) * (float)i / SampleRate);
                samples[i] = (sub * 0.75f * impactMult + noise * 0.5f) * env * 0.85f;
            }
            return samples;
        }

        private static float[] GenCosmicPulse(float duration, float rootFreq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * 3f);
                float mod = Mathf.Sin(2f * Mathf.PI * 14f * (float)i / SampleRate);
                float carrier = Mathf.Sin(2f * Mathf.PI * (rootFreq + mod * 80f) * (float)i / SampleRate);
                float sparkle = 0.25f * Mathf.Sin(2f * Mathf.PI * (rootFreq * 3f) * (float)i / SampleRate);
                samples[i] = (carrier * 0.75f + sparkle) * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenPing(float duration, float freq)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 8.5f);
                float sine = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate);
                float harmonic = 0.2f * Mathf.Sin(4f * Mathf.PI * freq * (float)i / SampleRate);
                samples[i] = (sine + harmonic) * env * 0.75f;
            }
            return samples;
        }

        private static float[] GenCombo(float duration, float f1, float f2, float f3)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float env = Mathf.Exp(-t * 7f);
                float s1 = Mathf.Sin(2f * Mathf.PI * f1 * (float)i / SampleRate);
                float s2 = Mathf.Sin(2f * Mathf.PI * f2 * (float)i / SampleRate);
                float s3 = Mathf.Sin(2f * Mathf.PI * f3 * (float)i / SampleRate);
                samples[i] = (s1 + s2 + s3) * 0.33f * env * 0.8f;
            }
            return samples;
        }

        private static float[] GenBounce(float duration, float fStart, float fEnd)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float freq = Mathf.Lerp(fStart, fEnd, t * t);
                float env = Mathf.Exp(-t * 22f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SampleRate) * env * 0.8f;
            }
            return samples;
        }

        private static float[] GenTension(float duration, float fStart, float fEnd)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / numSamples;
                float freq = Mathf.Lerp(fStart, fEnd, t);
                float env = Mathf.Sin(t * Mathf.PI * 0.5f);
                float triangle = Mathf.Abs(((freq * (float)i / SampleRate) % 1f) * 4f - 2f) - 1f;
                samples[i] = triangle * env * 0.45f;
            }
            return samples;
        }

        private static float[] GenRatchet(float duration, int clicks)
        {
            int numSamples = (int)(SampleRate * duration);
            float[] samples = new float[numSamples];
            for (int c = 0; c < clicks; c++)
            {
                int startSample = (int)((float)c / clicks * numSamples);
                for (int i = startSample; i < numSamples; i++)
                {
                    float t = (float)(i - startSample) / (SampleRate * 0.02f);
                    if (t > 1f) break;
                    float env = Mathf.Exp(-t * 25f);
                    float click = Mathf.Sin(2f * Mathf.PI * 800f * (float)(i - startSample) / SampleRate);
                    samples[i] += click * env * 0.5f;
                }
            }
            return samples;
        }

        #endregion

        #region WAV File Writer

        private static void WriteWav(string path, float[] samples)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                int numChannels = 1;
                int bitsPerSample = 16;
                int byteRate = SampleRate * numChannels * (bitsPerSample / 8);
                short blockAlign = (short)(numChannels * (bitsPerSample / 8));
                int subChunk2Size = samples.Length * numChannels * (bitsPerSample / 8);
                int chunkSize = 36 + subChunk2Size;

                // RIFF header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(chunkSize);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // subchunk1 size (16 for PCM)
                writer.Write((short)1); // audio format (1 = PCM)
                writer.Write((short)numChannels);
                writer.Write(SampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write((short)bitsPerSample);

                // data chunk
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(subChunk2Size);

                // Audio data
                for (int i = 0; i < samples.Length; i++)
                {
                    float clamped = Mathf.Clamp(samples[i], -1f, 1f);
                    short pcmSample = (short)(clamped * 32767f);
                    writer.Write(pcmSample);
                }
            }
        }

        #endregion
    }
}
#endif
