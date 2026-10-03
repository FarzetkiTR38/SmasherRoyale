using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Generates high quality 44.1kHz 16-bit PCM WAV audio assets for all Section 21 sound effects and music.
    /// Follows Master Build Prompt Section 21 & Section 32 (Placeholder Assets).
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        private const int SAMPLE_RATE = 44100;
        public const string SFX_PATH = "Assets/_Project/Audio/SFX";
        public const string MUSIC_PATH = "Assets/_Project/Audio/Music";

        public static void GenerateAllClips()
        {
            if (!Directory.Exists(SFX_PATH)) Directory.CreateDirectory(SFX_PATH);
            if (!Directory.Exists(MUSIC_PATH)) Directory.CreateDirectory(MUSIC_PATH);

            // 1. Ball Launch (Mortar)
            WriteWavFile($"{SFX_PATH}/ball_launch.wav", GenerateBallLaunch());

            // 2. Object Impacts
            WriteWavFile($"{SFX_PATH}/box_impact.wav", GenerateBoxImpact());
            WriteWavFile($"{SFX_PATH}/stone_impact.wav", GenerateStoneImpact());
            WriteWavFile($"{SFX_PATH}/column_impact.wav", GenerateColumnImpact());
            WriteWavFile($"{SFX_PATH}/cone_impact.wav", GenerateConeImpact());
            WriteWavFile($"{SFX_PATH}/bottle_break.wav", GenerateBottleBreak());
            WriteWavFile($"{SFX_PATH}/object_destruction.wav", GenerateObjectDestruction());

            // 3. UI & States
            WriteWavFile($"{SFX_PATH}/ui_click.wav", GenerateUiClick());
            WriteWavFile($"{SFX_PATH}/win_fanfare.wav", GenerateWinFanfare());
            WriteWavFile($"{SFX_PATH}/lose_sound.wav", GenerateLoseSound());
            WriteWavFile($"{SFX_PATH}/reward_coin.wav", GenerateRewardCoin());

            // 4. Music Tracks
            WriteWavFile($"{MUSIC_PATH}/bgm_mainmenu.wav", GenerateMenuMusic());
            WriteWavFile($"{MUSIC_PATH}/bgm_gameplay.wav", GenerateGameplayMusic());

            AssetDatabase.Refresh();
            Debug.Log("[ProceduralAudioGenerator] All 13 audio clips generated successfully.");
        }

        private static float[] GenerateBallLaunch()
        {
            float duration = 0.35f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];
            var rng = new System.Random(101);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                // Frequency drops rapidly from 160Hz to 40Hz
                float freq = Mathf.Lerp(160f, 40f, Mathf.Pow(progress, 0.4f));
                float sin = Mathf.Sin(2f * Mathf.PI * freq * t);

                // Whoosh noise burst
                float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-progress * 8f);

                // Exponential decay envelope
                float env = Mathf.Exp(-progress * 6f);
                samples[i] = Mathf.Clamp((sin * 0.75f + noise * 0.35f) * env, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateBoxImpact()
        {
            // Light, snappy wooden hit (Section 21.1)
            float duration = 0.18f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];
            var rng = new System.Random(102);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                float fundamental = Mathf.Sin(2f * Mathf.PI * 290f * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * 580f * t) * 0.4f;
                float click = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-progress * 40f) * 0.4f;

                float env = Mathf.Exp(-progress * 18f);
                samples[i] = Mathf.Clamp((fundamental + harmonic + click) * env * 0.8f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateStoneImpact()
        {
            // Heavy, solid bass thud (Section 21.1)
            float duration = 0.32f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];
            var rng = new System.Random(103);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                float sub = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.8f;
                float bass = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.5f;
                float punch = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-progress * 25f) * 0.5f;

                float env = Mathf.Exp(-progress * 9f);
                samples[i] = Mathf.Clamp((sub + bass + punch) * env * 0.9f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateColumnImpact()
        {
            // Heavy, deep stone resonance
            float duration = 0.42f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                float f1 = Mathf.Sin(2f * Mathf.PI * 95f * t) * 0.7f;
                float f2 = Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.3f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 380f * t) * 0.15f;

                float env = Mathf.Exp(-progress * 7f);
                samples[i] = Mathf.Clamp((f1 + f2 + f3) * env * 0.85f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateConeImpact()
        {
            // Light, hollow tone
            float duration = 0.16f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                // FM synthesis for hollow bell quality
                float mod = Mathf.Sin(2f * Mathf.PI * 180f * t) * 120f;
                float carrier = Mathf.Sin(2f * Mathf.PI * (440f + mod) * t);

                float env = Mathf.Exp(-progress * 22f);
                samples[i] = Mathf.Clamp(carrier * env * 0.7f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateBottleBreak()
        {
            // Glass shatter + liquid splash (Section 21.1)
            float duration = 0.55f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];
            var rng = new System.Random(104);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                // Glass crack frequencies
                float g1 = Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.3f;
                float g2 = Mathf.Sin(2f * Mathf.PI * 3400f * t) * 0.25f;
                float g3 = Mathf.Sin(2f * Mathf.PI * 5100f * t) * 0.2f;
                float shatterNoise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-progress * 12f) * 0.5f;

                // Liquid splash tail
                float splashNoise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Sin(progress * Mathf.PI) * 0.35f;

                float glassEnv = Mathf.Exp(-progress * 15f);
                samples[i] = Mathf.Clamp((g1 + g2 + g3 + shatterNoise) * glassEnv + splashNoise * 0.3f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateObjectDestruction()
        {
            // Crumble / shrink sound
            float duration = 0.38f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];
            var rng = new System.Random(105);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                float rumble = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 40f, progress) * t) * 0.4f;
                float crumble = ((float)rng.NextDouble() * 2f - 1f) * 0.35f;

                float env = Mathf.Sin(progress * Mathf.PI) * (1f - progress * 0.4f);
                samples[i] = Mathf.Clamp((rumble + crumble) * env * 0.7f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateUiClick()
        {
            // Crisp UI click
            float duration = 0.06f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = t / duration;

                float tone = Mathf.Sin(2f * Mathf.PI * 1250f * t);
                float env = Mathf.Exp(-progress * 45f);
                samples[i] = Mathf.Clamp(tone * env * 0.7f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateWinFanfare()
        {
            // Ascending royal victory arpeggio: C4, E4, G4, C5, E5
            float duration = 1.6f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            float[] notes = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f };
            float noteDuration = 0.28f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int noteIndex = Mathf.Min((int)(t / noteDuration), notes.Length - 1);
                float noteFreq = notes[noteIndex];
                float noteT = t - (noteIndex * noteDuration);

                float s1 = Mathf.Sin(2f * Mathf.PI * noteFreq * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * (noteFreq * 2f) * t) * 0.4f;

                float env = (noteIndex == notes.Length - 1)
                    ? Mathf.Exp(-(t - (notes.Length - 1) * noteDuration) * 2.5f)
                    : Mathf.Exp(-noteT * 3.5f);

                samples[i] = Mathf.Clamp((s1 + s2) * env * 0.65f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateLoseSound()
        {
            // Descending minor cadence
            float duration = 1.2f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            float[] notes = new float[] { 220.0f, 196.0f, 174.61f, 146.83f };
            float noteDuration = 0.28f;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int noteIndex = Mathf.Min((int)(t / noteDuration), notes.Length - 1);
                float noteFreq = notes[noteIndex];

                float s1 = Mathf.Sin(2f * Mathf.PI * noteFreq * t);
                float env = Mathf.Exp(-t * 2.2f);

                samples[i] = Mathf.Clamp(s1 * env * 0.6f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateRewardCoin()
        {
            // Bright coin ding (dual bell chime)
            float duration = 0.45f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                float c1 = Mathf.Sin(2f * Mathf.PI * 1480f * t) * Mathf.Exp(-t * 12f);
                float c2 = (t > 0.08f) ? Mathf.Sin(2f * Mathf.PI * 2200f * (t - 0.08f)) * Mathf.Exp(-(t - 0.08f) * 10f) : 0f;

                samples[i] = Mathf.Clamp((c1 + c2 * 0.8f) * 0.65f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateMenuMusic()
        {
            // 4.0s seamless ambient royal harp chord progression
            float duration = 4.0f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            float[] chordRoots = new float[] { 261.63f, 329.63f, 392.00f, 440.00f }; // C, E, G, A
            float step = duration / chordRoots.Length;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                int stepIdx = (int)(t / step) % chordRoots.Length;
                float root = chordRoots[stepIdx];

                float wave = Mathf.Sin(2f * Mathf.PI * root * t) * 0.25f +
                             Mathf.Sin(2f * Mathf.PI * (root * 1.5f) * t) * 0.18f +
                             Mathf.Sin(2f * Mathf.PI * (root * 2.0f) * t) * 0.10f;

                // Seamless loop window
                float loopWindow = Mathf.Sin((t / duration) * Mathf.PI);
                samples[i] = Mathf.Clamp(wave * (0.6f + 0.4f * loopWindow) * 0.45f, -1f, 1f);
            }
            return samples;
        }

        private static float[] GenerateGameplayMusic()
        {
            // 4.0s light percussion + medieval fantasy groove
            float duration = 4.0f;
            int count = (int)(SAMPLE_RATE * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SAMPLE_RATE;

                // Gentle pulse at 120 BPM (2Hz)
                float pulse = Mathf.Sin(2f * Mathf.PI * 2f * t);
                float bass = Mathf.Sin(2f * Mathf.PI * 130.81f * t) * (pulse > 0 ? 0.35f : 0.15f);

                // Melody line
                float melody = Mathf.Sin(2f * Mathf.PI * 392.0f * t) * 0.2f;

                samples[i] = Mathf.Clamp((bass + melody) * 0.4f, -1f, 1f);
            }
            return samples;
        }

        private static void WriteWavFile(string filePath, float[] samples)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                int sampleCount = samples.Length;
                int byteCount = sampleCount * 2; // 16-bit = 2 bytes per sample

                // RIFF chunk
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + byteCount);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // Subchunk1Size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 = PCM)
                writer.Write((short)1); // NumChannels (1 = Mono)
                writer.Write(SAMPLE_RATE); // SampleRate
                writer.Write(SAMPLE_RATE * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
                writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
                writer.Write((short)16); // BitsPerSample

                // data chunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(byteCount);

                // Write PCM samples
                for (int i = 0; i < sampleCount; i++)
                {
                    short val = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                    writer.Write(val);
                }
            }
        }
    }
}
