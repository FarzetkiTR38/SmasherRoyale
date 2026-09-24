using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoyalSmash.EditorTools
{
    public static class LeaderboardSpriteImporter
    {
        [MenuItem("Tools/RoyalSmash/Import Leaderboard Sprites")]
        public static void ConfigureSprites()
        {
            string targetDir = "Assets/_Project/Art/Sprites/LeaderboardSliced";
            if (!Directory.Exists(targetDir))
            {
                Debug.LogError($"[LeaderboardSpriteImporter] Target directory not found: {targetDir}");
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string[] files = Directory.GetFiles(targetDir, "*.png");
            int configured = 0;

            foreach (var file in files)
            {
                string assetPath = file.Replace("\\", "/");
                var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (imp != null)
                {
                    bool changed = false;

                    if (imp.textureType != TextureImporterType.Sprite)
                    {
                        imp.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }

                    if (imp.spriteImportMode != SpriteImportMode.Single)
                    {
                        imp.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }

                    if (!imp.alphaIsTransparency)
                    {
                        imp.alphaIsTransparency = true;
                        changed = true;
                    }

                    if (imp.mipmapEnabled)
                    {
                        imp.mipmapEnabled = false;
                        changed = true;
                    }

                    if (imp.wrapMode != TextureWrapMode.Clamp)
                    {
                        imp.wrapMode = TextureWrapMode.Clamp;
                        changed = true;
                    }

                    var settings = imp.GetDefaultPlatformTextureSettings();
                    if (settings.maxTextureSize < 2048)
                    {
                        settings.maxTextureSize = 2048;
                        imp.SetPlatformTextureSettings(settings);
                        changed = true;
                    }

                    if (changed)
                    {
                        imp.SaveAndReimport();
                        configured++;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[LeaderboardSpriteImporter] Configured {configured}/{files.Length} sprites in {targetDir}");
        }
    }
}
