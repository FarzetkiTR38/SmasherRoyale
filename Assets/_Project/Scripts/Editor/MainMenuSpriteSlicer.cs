using UnityEngine;
using UnityEditor;
using System.IO;

namespace RoyalSmash.EditorTools
{
    public static class MainMenuSpriteSlicer
    {
        [MenuItem("Tools/RoyalSmash/Slice Main Menu Sprites")]
        public static void SliceSprites()
        {
            string targetDir = "Assets/_Project/Art/Sprites/MainMenuSliced";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Helper to crop and save
            System.Action<string, int, int, int, int, string> saveSub = (srcPath, x, y, w, h, outName) =>
            {
                var importer = AssetImporter.GetAtPath(srcPath) as TextureImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                var src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
                if (src == null)
                {
                    Debug.LogError($"[SpriteSlicer] Source texture not found: {srcPath}");
                    return;
                }

                var dest = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var pixels = src.GetPixels(x, y, w, h);
                dest.SetPixels(pixels);
                dest.Apply();
                byte[] bytes = dest.EncodeToPNG();
                Object.DestroyImmediate(dest);

                string fullPath = Path.Combine(targetDir, outName);
                File.WriteAllBytes(fullPath, bytes);
            };

            // Tex9 (ChatGPT Image 20 Eyl 2026 00_07_46 (9).png)
            string path9 = "Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 20 Eyl 2026 00_07_46 (9).png";
            // Gold Play Button (x=14, y=1140, w=657, h=216)
            saveSub(path9, 14, 1140, 657, 216, "btn_play_gold.png");
            // Stage / Card Container (x=16, y=672, w=608, h=442)
            saveSub(path9, 16, 672, 608, 442, "panel_stage_card.png");
            // Blue Bar (x=15, y=492, w=609, h=152)
            saveSub(path9, 15, 492, 609, 152, "bar_stage_header.png");
            // Stage info badge (x=678, y=1158, w=426, h=180)
            saveSub(path9, 678, 1158, 426, 180, "badge_stage_info.png");

            // Tex8 (ChatGPT Image 20 Eyl 2026 00_07_46 (8).png)
            string path8 = "Assets/_Project/Art/_FarzetkiArts/mainmenu/ChatGPT Image 20 Eyl 2026 00_07_46 (8).png";
            // Gold pill (x=24, y=1147, w=551, h=185)
            saveSub(path8, 24, 1147, 551, 185, "widget_gold_pill.png");
            // Energy / Lives pill (x=30, y=918, w=546, h=184)
            saveSub(path8, 30, 918, 546, 184, "widget_energy_pill.png");
            // Settings gear (x=636, y=885, w=228, h=219)
            saveSub(path8, 636, 885, 228, 219, "btn_settings_gear.png");
            // Avatar Rex (x=24, y=528, w=354, h=351)
            saveSub(path8, 24, 528, 354, 351, "avatar_rex.png");
            // Empty blue pill (x=413, y=594, w=679, h=180)
            saveSub(path8, 413, 594, 679, 180, "widget_pill_empty.png");
            // Coin Icon (x=42, y=234, w=222, h=220)
            saveSub(path8, 42, 234, 222, 220, "icon_coin.png");
            // Lightning Bolt Icon (x=543, y=225, w=168, h=241)
            saveSub(path8, 543, 225, 168, 241, "icon_lightning.png");
            // Green Plus Icon (x=912, y=900, w=180, h=174)
            saveSub(path8, 912, 900, 180, 174, "icon_plus_green.png");
            // Empty Avatar Frame (x=768, y=204, w=335, h=342)
            saveSub(path8, 768, 204, 335, 342, "frame_avatar_empty.png");

            AssetDatabase.Refresh();

            // Set all outputs as 2D Sprites with clamp & transparency
            string[] files = Directory.GetFiles(targetDir, "*.png");
            foreach (var file in files)
            {
                string assetPath = file.Replace("\\", "/");
                var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.alphaIsTransparency = true;
                    imp.alphaSource = TextureImporterAlphaSource.FromInput;
                    imp.mipmapEnabled = false;
                    imp.wrapMode = TextureWrapMode.Clamp;
                    imp.SaveAndReimport();
                }
            }

            Debug.Log($"[MainMenuSpriteSlicer] Successfully sliced {files.Length} sprites to {targetDir}");
        }
    }
}
