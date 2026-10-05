#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    /// <summary>Explicit import of reviewed text-free source PNGs. Source pixels never change.</summary>
    public static class Phase34ShopArtImporter
    {
        private const string Folder = "Assets/_Project/Art/UI/ShopDynamic/";
        [Serializable] public sealed class ArtRecord
        {
            public string path;
            public int width, height, transparentPixels, partialAlphaPixels;
            public Rect spriteRect;
            public Vector4 border;
            public float pixelsPerUnit;
        }

        public static string ImportAndBind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Import artwork only in Edit Mode.");
            var records = new List<ArtRecord>();
            var sprites = new Dictionary<string, Sprite>();
            foreach (string path in Directory.GetFiles(Folder, "*.png").OrderBy(p => p))
            {
                string normalized = path.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(path);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    ImageConversion.LoadImage(texture, File.ReadAllBytes(path));
                    Color32[] pixels = texture.GetPixels32();
                    int x0 = texture.width, y0 = texture.height, x1 = -1, y1 = -1, zero = 0, semi = 0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i].a == 0) zero++;
                        else if (pixels[i].a < 255) semi++;
                        if (pixels[i].a <= 16) continue;
                        int x = i % texture.width, y = i / texture.width;
                        x0 = Math.Min(x0, x); y0 = Math.Min(y0, y);
                        x1 = Math.Max(x1, x); y1 = Math.Max(y1, y);
                    }
                    if (zero == 0 || x1 < x0) throw new InvalidOperationException("Expected genuine exterior alpha: " + path);
                    x0 = Math.Max(0, x0 - 3); y0 = Math.Max(0, y0 - 3);
                    x1 = Math.Min(texture.width - 1, x1 + 3); y1 = Math.Min(texture.height - 1, y1 + 3);
                    var rect = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
                    bool skin = name.StartsWith("Skin_", StringComparison.Ordinal) || name.StartsWith("Card_", StringComparison.Ordinal);
                    Vector4 border = Vector4.zero;
                    float ppu = skin ? 300f : 100f;
                    if (skin)
                    {
                        border = new Vector4(rect.width * .10f, rect.height * .17f, rect.width * .10f, rect.height * .17f);
                        if (name == "Skin_Frame")
                        {
                            ppu = 200f;
                            border = new Vector4(100f, 100f, 100f, 230f);
                        }
                        if (name == "Skin_Buy") border = new Vector4(180f, 100f, 180f, 100f);
                    }
                    var importer = (TextureImporter)AssetImporter.GetAtPath(normalized);
                    if (importer == null) { AssetDatabase.ImportAsset(normalized); importer = (TextureImporter)AssetImporter.GetAtPath(normalized); }
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.spritePixelsPerUnit = ppu;
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.isReadable = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.maxTextureSize = 2048;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    var factories = new SpriteDataProviderFactories();
                    factories.Init();
                    var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
                    provider.InitSpriteEditorDataProvider();
                    var old = provider.GetSpriteRects().FirstOrDefault(s => s.name == name);
                    var spriteId = old != null ? old.spriteID : GUID.Generate();
                    provider.SetSpriteRects(new[] { new SpriteRect { name = name, rect = rect, border = border,
                        pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center, spriteID = spriteId } });
                    var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                    names.SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, spriteId) });
                    provider.Apply();
                    importer.SaveAndReimport();
                    var sprite = AssetDatabase.LoadAllAssetsAtPath(normalized).OfType<Sprite>().Single();
                    sprites.Add(name, sprite);
                    records.Add(new ArtRecord { path = normalized, width = texture.width, height = texture.height,
                        transparentPixels = zero, partialAlphaPixels = semi, spriteRect = sprite.rect,
                        border = sprite.border, pixelsPerUnit = sprite.pixelsPerUnit });
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            var presentation = AssetDatabase.LoadAssetAtPath<ShopPresentationConfig>("Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset");
            if (presentation == null) throw new InvalidOperationException("Presentation asset missing.");
            Undo.RecordObject(presentation, "Phase 34: assign reviewed modular artwork");
            presentation.Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font_Settings.asset");
            presentation.ShopFrame = sprites["Skin_Frame"];
            presentation.HeaderBackground = sprites["Skin_Header"];
            presentation.SectionBackground = sprites["Skin_Section"];
            presentation.FeaturedBackground = sprites["Card_Gold"];
            presentation.CompactBackground = sprites["Card_Blue"];
            presentation.NoAdsBackground = sprites["Card_Purple"];
            presentation.BundleBackground = sprites["Card_Purple"];
            presentation.BuyBackground = sprites["Skin_Buy"];
            presentation.BadgeBackground = sprites["Skin_Badge"];
            presentation.GoldRewardIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/LeaderboardSliced/icon_coin_gold.png");
            presentation.MegaRewardIcon = sprites["Art_Mega"];
            presentation.InfinityRewardIcon = sprites["Art_Infinity"];
            presentation.NoAdsRewardIcon = sprites["Art_NoAds"];
            string[] ids = { "deal_kings_royal", "no_ads_pass", "gold_pouch_800", "gold_chest_1600", "skill_mega_5", "skill_infinity_5", "bundle_warrior", "bundle_champion" };
            string[] namesOfArt = { "Art_RoyalDeal", "Art_NoAds", "Art_GoldBag", "Art_GoldChest", "Art_Mega", "Art_Infinity", "Art_Warrior", "Art_Champion" };
            for (int i = 0; i < ids.Length; i++)
            {
                var art = presentation.FindArtwork(ids[i]);
                if (art == null) { art = new ShopProductArtwork { ItemId = ids[i] }; presentation.Products.Add(art); }
                art.Artwork = sprites[namesOfArt[i]];
            }
            EditorUtility.SetDirty(presentation);
            AssetDatabase.SaveAssetIfDirty(presentation);
            string json = JsonUtility.ToJson(new Wrapper { capturedUtc = DateTime.UtcNow.ToString("o"), records = records }, true);
            File.WriteAllText("_FarzetkiDocs/Art/Phase34/34-03-art-import.json", json);
            return json;
        }
        [Serializable] private sealed class Wrapper { public string capturedUtc; public List<ArtRecord> records; }
    }
}
#endif
