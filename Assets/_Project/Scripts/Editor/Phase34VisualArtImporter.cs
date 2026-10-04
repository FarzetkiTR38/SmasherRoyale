#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    /// <summary>Explicit import of reviewed revision PNGs; preserves source bytes and existing shop header.</summary>
    public static class Phase34VisualArtImporter
    {
        public const string Folder = "Assets/_Project/Art/UI/ShopDynamic/";
        private static readonly string[] Names = { "Skin_BuyStraight_v2", "Skin_SectionSlim_v2", "Skin_RibbonRed_v2", "Art_GoldBag_v2", "Art_GoldChest_v2", "Art_NoAdsHero_v2" };

        public static string ImportAndBind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Import only in Edit Mode.");
            var sprites = new Dictionary<string, Sprite>();
            var records = new List<object>();
            foreach (string name in Names)
            {
                string path = Folder + name + ".png";
                if (!File.Exists(path)) throw new FileNotFoundException("Reviewed sprite missing", path);
                byte[] source = File.ReadAllBytes(path);
                var raw = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    ImageConversion.LoadImage(raw, source);
                    int x0 = raw.width, y0 = raw.height, x1 = -1, y1 = -1, zero = 0;
                    Color32[] pixels = raw.GetPixels32();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i].a == 0) zero++;
                        if (pixels[i].a <= 16) continue;
                        int x = i % raw.width, y = i / raw.width;
                        x0 = Math.Min(x0, x); y0 = Math.Min(y0, y);
                        x1 = Math.Max(x1, x); y1 = Math.Max(y1, y);
                    }
                    if (zero == 0 || x1 < x0) throw new InvalidOperationException("Missing exterior alpha: " + name);
                    x0 = Math.Max(0, x0 - 3); y0 = Math.Max(0, y0 - 3);
                    x1 = Math.Min(raw.width - 1, x1 + 3); y1 = Math.Min(raw.height - 1, y1 + 3);
                    var rect = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
                    Vector4 border = name == "Skin_BuyStraight_v2" ? new Vector4(rect.width * .07f, rect.height * .16f, rect.width * .07f, rect.height * .16f) : Vector4.zero;
                    AssetDatabase.ImportAsset(path);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.spritePixelsPerUnit = name.StartsWith("Skin_") ? 300f : 100f;
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.isReadable = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.maxTextureSize = 4096;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    var factories = new SpriteDataProviderFactories(); factories.Init();
                    var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
                    SpriteRect previous = provider.GetSpriteRects().FirstOrDefault(s => s.name == name);
                    GUID id = previous != null ? previous.spriteID : GUID.Generate();
                    provider.SetSpriteRects(new[] { new SpriteRect { name = name, rect = rect, border = border, pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center, spriteID = id } });
                    provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
                    provider.Apply(); importer.SaveAndReimport();
                    Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
                    sprites.Add(name, sprite);
                    records.Add(new { path, width = raw.width, height = raw.height, transparentPixels = zero, rect = new[] { sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height }, border = new[] { sprite.border.x, sprite.border.y, sprite.border.z, sprite.border.w } });
                    if (!source.SequenceEqual(File.ReadAllBytes(path))) throw new InvalidOperationException("Source pixels changed: " + name);
                }
                finally { UnityEngine.Object.DestroyImmediate(raw); }
            }
            var config = AssetDatabase.LoadAssetAtPath<ShopPresentationConfig>("Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset");
            if (config == null) throw new InvalidOperationException("Presentation missing.");
            Undo.RecordObject(config, "Phase34 reference visual revision");
            config.BuyBackground = sprites["Skin_BuyStraight_v2"];
            config.SectionBackground = sprites["Skin_SectionSlim_v2"];
            config.BadgeBackground = sprites["Skin_RibbonRed_v2"];
            config.FindArtwork("gold_pouch_800").Artwork = sprites["Art_GoldBag_v2"];
            config.FindArtwork("gold_chest_1600").Artwork = sprites["Art_GoldChest_v2"];
            EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { capturedUtc = DateTime.UtcNow.ToString("o"), sourcePixelsUnchanged = true, records }, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("_FarzetkiDocs/Art/Phase34/Revision/art-import.json", json);
            return json;
        }
    }
}
#endif
