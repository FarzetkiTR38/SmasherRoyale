using System.IO;
using UnityEngine;
using UnityEditor;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Generates high-resolution procedural VFX textures for particles, trails, paint splatters, and glows.
    /// Creates crisp, mobile-optimized alpha textures directly in Assets/_Project/Textures/VFX/.
    /// </summary>
    public static class VFXTextureGenerator
    {
        private const string VFX_TEX_DIR = "Assets/_Project/Textures/VFX";

        [MenuItem("RoyalSmash/VFX/Generate All Procedural VFX Textures")]
        public static void GenerateAllVFXTextures()
        {
            if (!Directory.Exists(VFX_TEX_DIR)) Directory.CreateDirectory(VFX_TEX_DIR);

            CreateSoftGlowTexture($"{VFX_TEX_DIR}/tex_vfx_soft_glow.png", 256);
            CreateTrailStreakTexture($"{VFX_TEX_DIR}/tex_vfx_trail_streak.png", 512, 128);
            CreateSparkleStarTexture($"{VFX_TEX_DIR}/tex_vfx_sparkle_star.png", 256);
            CreateShockwaveRingTexture($"{VFX_TEX_DIR}/tex_vfx_shockwave_ring.png", 256);
            CreateSmokePuffTexture($"{VFX_TEX_DIR}/tex_vfx_smoke_puff.png", 256);
            CreatePaintSplatterTexture($"{VFX_TEX_DIR}/tex_vfx_paint_splat.png", 512);
            CreateGlassShardTexture($"{VFX_TEX_DIR}/tex_vfx_glass_shard.png", 256);

            AssetDatabase.Refresh();

            // Set importer settings for all generated textures
            string[] files = Directory.GetFiles(VFX_TEX_DIR, "*.png");
            foreach (var f in files)
            {
                var importer = AssetImporter.GetAtPath(f) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = true;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log("[VFXTextureGenerator] Successfully generated and imported all 7 VFX textures!");
        }

        private static void CreateSoftGlowTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    if (dist >= 1f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        // Gaussian smooth exponential falloff
                        float alpha = Mathf.Exp(-dist * dist * 4.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreateTrailStreakTexture(string path, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float ny = ((float)y / height) * 2f - 1f; // -1 to +1 vertical
                float yFalloff = Mathf.Cos(ny * Mathf.PI * 0.5f);
                if (yFalloff < 0f) yFalloff = 0f;
                yFalloff = Mathf.Pow(yFalloff, 2.2f); // Sharp core, soft edge

                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width; // 0 (tail) to 1 (head)
                    // Head is bright white, tail fades out smoothly
                    float xFalloff = Mathf.Pow(nx, 0.85f);
                    float alpha = yFalloff * xFalloff;

                    // Core is intense white, edges blend cleanly
                    float core = Mathf.Pow(yFalloff, 4.0f) * xFalloff;
                    Color col = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreateSparkleStarTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;

                    // 4-point star cross flares
                    float flareX = Mathf.Exp(-dx * 1.5f) / (dy * 18f + 1f);
                    float flareY = Mathf.Exp(-dy * 1.5f) / (dx * 18f + 1f);
                    // Diagonal secondary flare
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = Mathf.Exp(-dist * 8f);

                    float val = Mathf.Clamp01(flareX * 0.7f + flareY * 0.7f + core * 1.2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, val));
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreateShockwaveRingTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float ringRadius = 0.70f;
            float ringThickness = 0.16f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    float diff = Mathf.Abs(dist - ringRadius);
                    if (diff < ringThickness)
                    {
                        float norm = diff / ringThickness;
                        float alpha = Mathf.Cos(norm * Mathf.PI * 0.5f);
                        alpha = Mathf.Pow(alpha, 1.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreateSmokePuffTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;

            // 5 overlapping lobes to create a puffy cloud
            Vector2[] lobes = new Vector2[] {
                new Vector2(0f, 0f),
                new Vector2(0.24f, 0.18f),
                new Vector2(-0.22f, 0.14f),
                new Vector2(0.20f, -0.20f),
                new Vector2(-0.18f, -0.16f)
            };
            float[] radii = new float[] { 0.42f, 0.32f, 0.30f, 0.31f, 0.28f };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 uv = new Vector2((x - center) / center, (y - center) / center);
                    float maxDensity = 0f;

                    for (int i = 0; i < lobes.Length; i++)
                    {
                        float d = Vector2.Distance(uv, lobes[i]) / radii[i];
                        if (d < 1f)
                        {
                            float density = Mathf.Cos(d * Mathf.PI * 0.5f);
                            if (density > maxDensity) maxDensity = density;
                        }
                    }

                    maxDensity = Mathf.Pow(maxDensity, 1.4f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(maxDensity)));
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreatePaintSplatterTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            var rng = new System.Random(4242);

            // Generate main central splash body + 12 satellite splatter drops
            int satelliteCount = 14;
            Vector2[] splatCenters = new Vector2[satelliteCount + 1];
            float[] splatRadii = new float[satelliteCount + 1];

            splatCenters[0] = new Vector2(0f, 0f);
            splatRadii[0] = 0.38f;

            for (int s = 1; s <= satelliteCount; s++)
            {
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float dist = 0.35f + (float)rng.NextDouble() * 0.45f;
                splatCenters[s] = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                splatRadii[s] = 0.05f + (float)rng.NextDouble() * 0.12f;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 uv = new Vector2((x - center) / center, (y - center) / center);
                    float density = 0f;

                    // Angular organic jitter for main blob
                    float angle = Mathf.Atan2(uv.y, uv.x);
                    float noise = Mathf.Sin(angle * 7f) * 0.06f + Mathf.Cos(angle * 13f) * 0.04f;

                    for (int i = 0; i <= satelliteCount; i++)
                    {
                        float d = Vector2.Distance(uv, splatCenters[i]) / (splatRadii[i] + (i == 0 ? noise : 0f));
                        if (d < 1f)
                        {
                            float localD = 1f - d;
                            // Sharp ink drop edge with soft antialiasing
                            float edge = Mathf.Clamp01(localD * 6f);
                            if (edge > density) density = edge;
                        }
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, density));
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void CreateGlassShardTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;

            // Sharp angular diamond/triangle fragment with bright specular edge
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)(x - center) / center;
                    float ny = (float)(y - center) / center;

                    // Diamond shard polygon
                    float d = Mathf.Abs(nx * 1.6f) + Mathf.Abs(ny * 0.85f);
                    if (d < 0.85f)
                    {
                        // Specular gradient along one edge
                        float specular = Mathf.Clamp01(1f - Mathf.Abs(nx - ny * 0.5f) * 2f);
                        float alpha = Mathf.Lerp(0.65f, 1.0f, specular);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTexture(tex, path);
        }

        private static void SaveTexture(Texture2D tex, string relativePath)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), relativePath);
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(tex);
        }
    }
}
