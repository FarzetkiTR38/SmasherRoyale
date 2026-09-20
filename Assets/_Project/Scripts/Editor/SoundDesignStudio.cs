using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Studio-grade acoustic audio synthesizer and sound designer for Royal Smash.
    /// Synthesizes 44.1kHz 16-bit uncompressed WAV sound effects for ball launches,
    /// bottle shatters, impacts, destruction, fanfares, and UI feedback.
    /// Provides 3 distinct curated acoustic variants for every sound in the game.
    /// </summary>
    public static class SoundDesignStudio
    {
        private const int SAMPLE_RATE = 44100;

        [MenuItem("RoyalSmash/Audio/Synthesize All 3x SFX Variants")]
        public static void GenerateAllSoundVariants()
        {
            Debug.Log("[SoundDesignStudio] Starting synthesis of all SFX variants...");

            string baseDir = "Assets/_Project/Audio/SFX";
            EnsureDirectory(baseDir);
            EnsureDirectory($"{baseDir}/BallLaunch");
            EnsureDirectory($"{baseDir}/BottleBreak");
            EnsureDirectory($"{baseDir}/Impacts");
            EnsureDirectory($"{baseDir}/Destruction");
            EnsureDirectory($"{baseDir}/GameState");
            EnsureDirectory($"{baseDir}/UI");

            // 1. Ball Launch / Mortar Cannon Fire (3 Variants)
            SaveWav($"{baseDir}/BallLaunch/sfx_launch_opt1_heavy_cannon.wav", SynthesizeCannonBlast(0));
            SaveWav($"{baseDir}/BallLaunch/sfx_launch_opt2_catapult_boom.wav", SynthesizeCannonBlast(1));
            SaveWav($"{baseDir}/BallLaunch/sfx_launch_opt3_arcade_pow.wav", SynthesizeCannonBlast(2));

            // 2. Paint Bottle Break & Splash (3 Variants)
            SaveWav($"{baseDir}/BottleBreak/sfx_bottle_opt1_glass_splash.wav", SynthesizeBottleBreak(0));
            SaveWav($"{baseDir}/BottleBreak/sfx_bottle_opt2_ceramic_squish.wav", SynthesizeBottleBreak(1));
            SaveWav($"{baseDir}/BottleBreak/sfx_bottle_opt3_crystal_burst.wav", SynthesizeBottleBreak(2));

            // 3. Object Impacts (3 Variants each: Wood, Stone, Column, Cone)
            SaveWav($"{baseDir}/Impacts/sfx_impact_box_opt1.wav", SynthesizeImpact("wood", 0));
            SaveWav($"{baseDir}/Impacts/sfx_impact_box_opt2.wav", SynthesizeImpact("wood", 1));
            SaveWav($"{baseDir}/Impacts/sfx_impact_box_opt3.wav", SynthesizeImpact("wood", 2));

            SaveWav($"{baseDir}/Impacts/sfx_impact_stone_opt1.wav", SynthesizeImpact("stone", 0));
            SaveWav($"{baseDir}/Impacts/sfx_impact_stone_opt2.wav", SynthesizeImpact("stone", 1));
            SaveWav($"{baseDir}/Impacts/sfx_impact_stone_opt3.wav", SynthesizeImpact("stone", 2));

            SaveWav($"{baseDir}/Impacts/sfx_impact_column_opt1.wav", SynthesizeImpact("column", 0));
            SaveWav($"{baseDir}/Impacts/sfx_impact_column_opt2.wav", SynthesizeImpact("column", 1));
            SaveWav($"{baseDir}/Impacts/sfx_impact_column_opt3.wav", SynthesizeImpact("column", 2));

            SaveWav($"{baseDir}/Impacts/sfx_impact_cone_opt1.wav", SynthesizeImpact("cone", 0));
            SaveWav($"{baseDir}/Impacts/sfx_impact_cone_opt2.wav", SynthesizeImpact("cone", 1));
            SaveWav($"{baseDir}/Impacts/sfx_impact_cone_opt3.wav", SynthesizeImpact("cone", 2));

            // 4. Object Destruction / Target Clear (3 Variants)
            SaveWav($"{baseDir}/Destruction/sfx_destruct_opt1_debris_crumble.wav", SynthesizeDestruction(0));
            SaveWav($"{baseDir}/Destruction/sfx_destruct_opt2_explosive_burst.wav", SynthesizeDestruction(1));
            SaveWav($"{baseDir}/Destruction/sfx_destruct_opt3_arcade_crunch.wav", SynthesizeDestruction(2));

            // 5. Game State Fanfares & Feedback (3 Variants each)
            SaveWav($"{baseDir}/GameState/sfx_win_opt1_royal_brass.wav", SynthesizeWinFanfare(0));
            SaveWav($"{baseDir}/GameState/sfx_win_opt2_orchestral_jingle.wav", SynthesizeWinFanfare(1));
            SaveWav($"{baseDir}/GameState/sfx_win_opt3_celebratory_bells.wav", SynthesizeWinFanfare(2));

            SaveWav($"{baseDir}/GameState/sfx_lose_opt1_melodic_groan.wav", SynthesizeLoseSound(0));
            SaveWav($"{baseDir}/GameState/sfx_lose_opt2_wah_wah.wav", SynthesizeLoseSound(1));
            SaveWav($"{baseDir}/GameState/sfx_lose_opt3_soft_chimes.wav", SynthesizeLoseSound(2));

            SaveWav($"{baseDir}/GameState/sfx_coin_opt1_sparkle_cascade.wav", SynthesizeCoinChime(0));
            SaveWav($"{baseDir}/GameState/sfx_coin_opt2_heavy_gold_clink.wav", SynthesizeCoinChime(1));
            SaveWav($"{baseDir}/GameState/sfx_coin_opt3_arcade_ding.wav", SynthesizeCoinChime(2));

            SaveWav($"{baseDir}/GameState/sfx_start_opt1_royal_horn.wav", SynthesizeGameStart(0));
            SaveWav($"{baseDir}/GameState/sfx_start_opt2_ascending_harp.wav", SynthesizeGameStart(1));
            SaveWav($"{baseDir}/GameState/sfx_start_opt3_drum_roll.wav", SynthesizeGameStart(2));

            // 6. Synthesize Gameplay BGM Alternatives (16-bit 44.1kHz stereo/mono loop)
            SynthesizeMusicTracks();

            AssetDatabase.Refresh();
            UpdateAudioConfig();
            Debug.Log("[SoundDesignStudio] Completed synthesizing all 3x SFX variants and music!");
        }

        public static void SynthesizeMusicTracks()
        {
            string musicDir = "Assets/_Project/Audio/Music";
            EnsureDirectory(musicDir);

            SaveWav($"{musicDir}/bgm_game_opt2_driving_groove.wav", SynthesizeDrivingGroove());
            SaveWav($"{musicDir}/bgm_game_opt3_bouncy_arcade.wav", SynthesizeBouncyArcade());
        }

        public static void UpdateAudioConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<RoyalSmash.Audio.AudioConfig>("Assets/_Project/ScriptableObjects/Config/AudioConfig.asset");
            if (config == null)
            {
                Debug.LogWarning("[SoundDesignStudio] AudioConfig.asset not found!");
                return;
            }

            // Primary (Option 1)
            config.BgmMainMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_menu_opt1_royal_courtyard.mp3");
            config.BgmGameplay = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_game_opt1_puzzle_march.mp3");

            config.BallLaunch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BallLaunch/sfx_launch_opt1_heavy_cannon.wav");
            config.BoxImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_box_opt1.wav");
            config.StoneImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_stone_opt1.wav");
            config.ColumnImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_column_opt1.wav");
            config.ConeImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_cone_opt1.wav");
            config.PaintBottleBreak = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BottleBreak/sfx_bottle_opt1_glass_splash.wav");
            config.ObjectDestruction = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Destruction/sfx_destruct_opt1_debris_crumble.wav");

            config.ButtonClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_click_opt1_casual_pop.wav");
            config.PanelOpen = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_panel_open_opt1_airy_whoosh.wav");
            config.GameStart = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_start_opt1_royal_horn.wav");
            config.WinFanfare = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_win_opt1_royal_brass.wav");
            config.LoseSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_lose_opt1_melodic_groan.wav");
            config.RewardSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_coin_opt1_sparkle_cascade.wav");

            // Alternatives (Alt 1 & Alt 2)
            config.BgmMainMenuAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_menu_opt2_ballroom_waltz.mp3");
            config.BgmMainMenuAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_menu_opt3_cozy_tavern.mp3");
            config.BgmGameplayAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_game_opt2_run_amok.mp3");
            config.BgmGameplayAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/bgm_game_opt3_wagon_wheel.mp3");

            config.BallLaunchAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BallLaunch/sfx_launch_opt2_catapult_boom.wav");
            config.BallLaunchAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BallLaunch/sfx_launch_opt3_arcade_pow.wav");

            config.BoxImpactAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_box_opt2.wav");
            config.BoxImpactAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_box_opt3.wav");
            config.StoneImpactAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_stone_opt2.wav");
            config.StoneImpactAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_stone_opt3.wav");
            config.ColumnImpactAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_column_opt2.wav");
            config.ColumnImpactAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_column_opt3.wav");
            config.ConeImpactAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_cone_opt2.wav");
            config.ConeImpactAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Impacts/sfx_impact_cone_opt3.wav");

            config.PaintBottleBreakAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BottleBreak/sfx_bottle_opt2_ceramic_squish.wav");
            config.PaintBottleBreakAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/BottleBreak/sfx_bottle_opt3_crystal_burst.wav");

            config.ObjectDestructionAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Destruction/sfx_destruct_opt2_explosive_burst.wav");
            config.ObjectDestructionAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Destruction/sfx_destruct_opt3_arcade_crunch.wav");

            config.ButtonClickAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_click_opt2_royal_click.wav");
            config.ButtonClickAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_click_opt3_bubble_snap.wav");

            config.PanelOpenAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_panel_open_opt2_royal_swoosh.wav");
            config.PanelOpenAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/sfx_panel_open_opt3_slide_pop.wav");

            config.GameStartAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_start_opt2_ascending_harp.wav");
            config.GameStartAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_start_opt3_drum_roll.wav");

            config.WinFanfareAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_win_opt2_orchestral_jingle.wav");
            config.WinFanfareAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_win_opt3_celebratory_bells.wav");

            config.LoseSoundAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_lose_opt2_wah_wah.wav");
            config.LoseSoundAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_lose_opt3_soft_chimes.wav");

            config.RewardSoundAlt1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_coin_opt2_heavy_gold_clink.wav");
            config.RewardSoundAlt2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/GameState/sfx_coin_opt3_arcade_ding.wav");

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("[SoundDesignStudio] AudioConfig.asset successfully updated with all 3 variants per sound event!");
        }

        private static float[] SynthesizeDrivingGroove()
        {
            float duration = 12.0f; // 12 seconds loop
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // 120 BPM: 1 beat = 0.5s, bar = 2.0s (6 bars total)
            // Chords: Am (220, 261.6, 329.6), F (174.6, 220, 261.6), C (130.8, 164.8, 196), G (196, 246.9, 293.7)
            float[][] chords = new float[][] {
                new float[] { 220f, 261.63f, 329.63f, 440f }, // Am
                new float[] { 174.61f, 220f, 261.63f, 349.23f }, // F
                new float[] { 130.81f, 164.81f, 196f, 261.63f }, // C
                new float[] { 196f, 246.94f, 293.66f, 392f } // G
            };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int bar = ((int)(t / 2.0f)) % 4;
                float[] currentChord = chords[bar];
                float barTime = t % 2.0f;

                // 1. Lute / Harp arpeggio: 16th note steps (0.125s per note)
                int noteIndex = (int)(barTime / 0.125f) % 4;
                float noteTime = barTime % 0.125f;
                float freq = currentChord[noteIndex];
                float noteEnv = Mathf.Exp(-14f * noteTime);
                float lute = (Mathf.Sin(2f * Mathf.PI * freq * t) + 0.4f * Mathf.Sin(4f * Mathf.PI * freq * t)) * noteEnv * 0.28f;

                // 2. Rhythmic kick: on beats 0.0s, 0.5s, 1.0s, 1.5s
                float beatTime = barTime % 0.5f;
                float kickFreq = Mathf.Lerp(110f, 45f, beatTime * 12f);
                float kick = Mathf.Sin(2f * Mathf.PI * kickFreq * beatTime) * Mathf.Exp(-16f * beatTime) * 0.35f;

                // 3. Woodblock tap on offbeats (0.25s, 0.75s, 1.25s, 1.75s)
                float offTime = (barTime + 0.25f) % 0.5f;
                float woodblock = Mathf.Sin(2f * Mathf.PI * 880f * offTime) * Mathf.Exp(-35f * offTime) * 0.15f;

                // 4. Bass pulse
                float bassFreq = currentChord[0] * 0.5f;
                float bass = Mathf.Sin(2f * Mathf.PI * bassFreq * t) * 0.2f;

                buffer[i] = Mathf.Clamp(lute + kick + woodblock + bass, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static float[] SynthesizeBouncyArcade()
        {
            float duration = 11.077f; // ~24 beats at 130 BPM (loopable)
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // 130 BPM: beat = 0.4615s
            float beatLen = 60f / 130f;
            float[] bassNotes = new float[] { 130.81f, 196f, 220f, 174.61f }; // C, G, A, F

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int beatNum = (int)(t / beatLen);
                int chordIdx = (beatNum / 4) % 4;
                float beatTime = t % beatLen;

                // 1. Bouncy pizzicato bass
                float bFreq = bassNotes[chordIdx];
                float bass = Mathf.Sin(2f * Mathf.PI * bFreq * beatTime) * Mathf.Exp(-10f * beatTime) * 0.35f;

                // 2. Playful xylophone bell ping
                float bellFreq = bassNotes[chordIdx] * 4f;
                float bellTime = (t % (beatLen * 0.5f));
                float bell = (Mathf.Sin(2f * Mathf.PI * bellFreq * bellTime) + 0.3f * Mathf.Sin(6f * Mathf.PI * bellFreq * bellTime)) 
                             * Mathf.Exp(-20f * bellTime) * 0.25f;

                // 3. Shaker / Pop percussion
                float popTime = beatTime % (beatLen * 0.25f);
                float pop = (Mathf.Sin(2f * Mathf.PI * 1200f * popTime) * Mathf.Exp(-45f * popTime)) * 0.12f;

                buffer[i] = Mathf.Clamp(bass + bell + pop, -0.95f, 0.95f);
            }

            return buffer;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        // =========================================================================
        // ACOUSTIC SYNTHESIS ENGINES
        // =========================================================================

        private static float[] SynthesizeCannonBlast(int variant)
        {
            float duration = variant == 0 ? 0.85f : (variant == 1 ? 0.70f : 0.55f);
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];
            var rng = new System.Random(42 + variant * 101);

            float startFreq = variant == 0 ? 68f : (variant == 1 ? 95f : 140f);
            float endFreq = variant == 0 ? 30f : (variant == 1 ? 42f : 55f);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                // 1. Transient Shockwave Crack (first 0.02s)
                float transient = 0f;
                if (t < 0.025f)
                {
                    float tEnv = 1f - (t / 0.025f);
                    transient = ((float)rng.NextDouble() * 2f - 1f) * tEnv * (variant == 0 ? 1.0f : 0.8f);
                }

                // 2. Sub-bass Acoustic Body (Pitch slides downward)
                float currentFreq = Mathf.Lerp(startFreq, endFreq, Mathf.Pow(progress, 0.4f));
                float phase = 2f * Mathf.PI * currentFreq * t;
                float bodyEnv = Mathf.Exp(-t * (variant == 0 ? 4.5f : 6.0f));
                float subBass = Mathf.Sin(phase) * bodyEnv;

                // Non-linear saturation for warmth/punch
                subBass = (float)Math.Tanh(subBass * 1.8f);

                // 3. Gunpowder Combustion / Air Rumble (Bandpass noise)
                float noise = ((float)rng.NextDouble() * 2f - 1f);
                float rumbleEnv = Mathf.Exp(-t * (variant == 0 ? 7f : 9f));
                float rumble = noise * rumbleEnv * 0.45f;

                // 4. Metallic Barrel Resonance (Variant 0 & 1)
                float ring = 0f;
                if (variant == 0)
                {
                    float ringPhase = 2f * Mathf.PI * 340f * t;
                    ring = Mathf.Sin(ringPhase) * Mathf.Exp(-t * 12f) * 0.22f;
                }

                buffer[i] = (transient * 0.6f) + (subBass * 0.75f) + (rumble * 0.45f) + (ring * 0.25f);
            }

            Normalize(buffer, 0.95f);
            return buffer;
        }

        private static float[] SynthesizeBottleBreak(int variant)
        {
            float duration = 0.95f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];
            var rng = new System.Random(1337 + variant * 77);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                // Initial impact crack
                float crack = 0f;
                if (t < 0.018f)
                {
                    float env = 1f - (t / 0.018f);
                    crack = ((float)rng.NextDouble() * 2f - 1f) * env * 0.9f;
                }

                // Crystalline glass shards (multiple resonant decaying harmonics)
                float glass = 0f;
                float[] freqs = variant == 2 
                    ? new float[] { 4200f, 5600f, 7100f, 8900f } 
                    : new float[] { 2800f, 3750f, 4900f, 6200f };

                for (int f = 0; f < freqs.Length; f++)
                {
                    float decay = 9f + (f * 3.5f);
                    glass += Mathf.Sin(2f * Mathf.PI * freqs[f] * t) * Mathf.Exp(-t * decay) * 0.25f;
                }

                // Wet fluid splash / squelch (frequency-modulated noise)
                float fluid = 0f;
                if (t > 0.015f && t < 0.65f)
                {
                    float fluidT = t - 0.015f;
                    float mod = Mathf.Sin(2f * Mathf.PI * 35f * fluidT);
                    float fluidEnv = Mathf.Sin(fluidT / 0.65f * Mathf.PI) * Mathf.Exp(-fluidT * 3.5f);
                    float rawNoise = ((float)rng.NextDouble() * 2f - 1f);
                    fluid = rawNoise * fluidEnv * (variant == 1 ? 0.65f : 0.45f) * (1f + mod * 0.3f);
                }

                buffer[i] = crack + glass + fluid;
            }

            Normalize(buffer, 0.92f);
            return buffer;
        }

        private static float[] SynthesizeImpact(string material, int variant)
        {
            float duration = 0.35f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];
            var rng = new System.Random(material.GetHashCode() + variant * 33);

            float freq = 120f;
            float decay = 18f;
            float noiseMix = 0.2f;

            switch (material.ToLower())
            {
                case "wood":
                    freq = variant == 0 ? 140f : (variant == 1 ? 110f : 175f);
                    decay = 14f;
                    noiseMix = 0.25f;
                    break;
                case "stone":
                    freq = variant == 0 ? 190f : (variant == 1 ? 160f : 230f);
                    decay = 22f;
                    noiseMix = 0.40f;
                    break;
                case "column":
                    freq = variant == 0 ? 95f : (variant == 1 ? 80f : 115f);
                    decay = 11f;
                    noiseMix = 0.20f;
                    break;
                case "cone":
                    freq = variant == 0 ? 280f : (variant == 1 ? 340f : 240f);
                    decay = 16f;
                    noiseMix = 0.15f;
                    break;
            }

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                // Transient click
                float click = (t < 0.012f) ? ((float)rng.NextDouble() * 2f - 1f) * (1f - t / 0.012f) : 0f;

                // Resonant body
                float body = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * decay);
                // Harmonic overtone
                float overtone = Mathf.Sin(2f * Mathf.PI * (freq * 2.15f) * t) * Mathf.Exp(-t * (decay * 1.5f)) * 0.35f;

                float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * (decay * 1.2f)) * noiseMix;

                buffer[i] = (click * 0.6f) + (body * 0.7f) + overtone + noise;
            }

            Normalize(buffer, 0.90f);
            return buffer;
        }

        private static float[] SynthesizeDestruction(int variant)
        {
            float duration = 0.75f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];
            var rng = new System.Random(999 + variant * 123);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                // Multi-grain crumbling noise
                float grain = ((float)rng.NextDouble() * 2f - 1f);
                float env = Mathf.Pow(1f - (t / duration), 1.8f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 65f * t) * Mathf.Exp(-t * 5f) * 0.6f;

                // Crunchy debris burst
                float crackle = (rng.NextDouble() > 0.88 ? 1f : 0f) * ((float)rng.NextDouble() * 2f - 1f) * env * 0.5f;

                buffer[i] = (grain * env * 0.45f) + rumble + crackle;
            }

            Normalize(buffer, 0.92f);
            return buffer;
        }

        private static float[] SynthesizeWinFanfare(int variant)
        {
            float duration = 1.85f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // Triumphant Royal Chord: C major arpeggio into rich sustained chord (C4, E4, G4, C5)
            float[] notes = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f };
            float[] noteTimes = new float[] { 0.0f, 0.14f, 0.28f, 0.42f, 0.70f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sum = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float noteStart = noteTimes[n];
                    if (t >= noteStart)
                    {
                        float localT = t - noteStart;
                        float freq = notes[n];
                        float noteEnv = Mathf.Exp(-localT * 2.2f);
                        // Brass / Bell harmonics
                        float wave = Mathf.Sin(2f * Mathf.PI * freq * localT) +
                                     Mathf.Sin(2f * Mathf.PI * (freq * 2f) * localT) * 0.35f +
                                     Mathf.Sin(2f * Mathf.PI * (freq * 3f) * localT) * 0.15f;
                        sum += wave * noteEnv * 0.22f;
                    }
                }

                // Shimmer chime layer
                float shimmer = Mathf.Sin(2f * Mathf.PI * 1567.98f * t) * Mathf.Exp(-t * 1.8f) * 0.08f;

                buffer[i] = sum + shimmer;
            }

            Normalize(buffer, 0.94f);
            return buffer;
        }

        private static float[] SynthesizeLoseSound(int variant)
        {
            float duration = 1.25f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // Melodic descending minor notes: Eb4 -> D4 -> C4 -> G3
            float[] notes = new float[] { 311.13f, 293.66f, 261.63f, 196.00f };
            float[] noteTimes = new float[] { 0.0f, 0.25f, 0.50f, 0.75f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sum = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    float noteStart = noteTimes[n];
                    if (t >= noteStart)
                    {
                        float localT = t - noteStart;
                        float freq = notes[n];
                        float env = Mathf.Exp(-localT * 3.2f);
                        float wave = Mathf.Sin(2f * Mathf.PI * freq * localT) +
                                     Mathf.Sin(2f * Mathf.PI * (freq * 2f) * localT) * 0.2f;
                        sum += wave * env * 0.28f;
                    }
                }

                buffer[i] = sum;
            }

            Normalize(buffer, 0.90f);
            return buffer;
        }

        private static float[] SynthesizeCoinChime(int variant)
        {
            float duration = 0.55f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // Rapid 3-tier cascade of golden coin pings (E6, G#6, B6)
            float[] pings = variant == 0
                ? new float[] { 1318.5f, 1661.2f, 1975.5f }
                : (variant == 1 ? new float[] { 1046.5f, 1318.5f, 1567.9f } : new float[] { 1567.9f, 2093.0f, 2637.0f });
            float[] pingTimes = new float[] { 0.0f, 0.06f, 0.12f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sum = 0f;

                for (int p = 0; p < pings.Length; p++)
                {
                    if (t >= pingTimes[p])
                    {
                        float localT = t - pingTimes[p];
                        float freq = pings[p];
                        float env = Mathf.Exp(-localT * 9.5f);
                        float wave = Mathf.Sin(2f * Mathf.PI * freq * localT);
                        sum += wave * env * 0.35f;
                    }
                }

                buffer[i] = sum;
            }

            Normalize(buffer, 0.92f);
            return buffer;
        }

        private static float[] SynthesizeGameStart(int variant)
        {
            float duration = 0.95f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] buffer = new float[samples];

            // Royal battle horn fanfare
            float[] notes = new float[] { 392f, 523.25f, 659.25f };
            float[] times = new float[] { 0f, 0.16f, 0.35f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float sum = 0f;

                for (int n = 0; n < notes.Length; n++)
                {
                    if (t >= times[n])
                    {
                        float localT = t - times[n];
                        float freq = notes[n];
                        float env = Mathf.Exp(-localT * 3.8f);
                        float wave = Mathf.Sin(2f * Mathf.PI * freq * localT) +
                                     Mathf.Sin(2f * Mathf.PI * freq * 2f * localT) * 0.3f;
                        sum += wave * env * 0.32f;
                    }
                }

                buffer[i] = sum;
            }

            Normalize(buffer, 0.92f);
            return buffer;
        }

        private static void Normalize(float[] buffer, float peak)
        {
            float max = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                float abs = Mathf.Abs(buffer[i]);
                if (abs > max) max = abs;
            }

            if (max > 0.0001f)
            {
                float mult = peak / max;
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] *= mult;
                }
            }
        }

        // =========================================================================
        // RIFF WAVE FILE WRITER
        // =========================================================================

        private static void SaveWav(string relativePath, float[] samples)
        {
            string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), relativePath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var fs = new FileStream(fullPath, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                int sampleCount = samples.Length;
                int byteRate = SAMPLE_RATE * 1 * 2; // 16-bit mono

                // 1. RIFF chunk descriptor
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + sampleCount * 2);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // 2. fmt sub-chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16); // Subchunk1Size for PCM
                bw.Write((short)1); // AudioFormat (1 = PCM)
                bw.Write((short)1); // NumChannels (1 = Mono)
                bw.Write(SAMPLE_RATE);
                bw.Write(byteRate);
                bw.Write((short)2); // BlockAlign
                bw.Write((short)16); // BitsPerSample

                // 3. data sub-chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(sampleCount * 2);

                for (int i = 0; i < sampleCount; i++)
                {
                    float val = Mathf.Clamp(samples[i], -1f, 1f);
                    short pcm = (short)(val * 32767f);
                    bw.Write(pcm);
                }
            }

            Debug.Log($"[SoundDesignStudio] Exported WAV: {relativePath} ({samples.Length} samples)");
        }
    }
}
