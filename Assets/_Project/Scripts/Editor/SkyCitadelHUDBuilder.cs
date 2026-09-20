using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using RoyalSmash.Levels;
using RoyalSmash.UI;

namespace RoyalSmash.Editor
{
    /// <summary>Idempotent visual pass over MainGame's existing HUD; gameplay bindings remain intact.</summary>
    public static class SkyCitadelHUDBuilder
    {
        private static readonly Color Navy = new Color32(12, 35, 79, 255);
        private static readonly Color Blue = new Color32(20, 72, 143, 255);
        private static readonly Color Gold = new Color32(231, 180, 67, 255);
        private static readonly Color Ivory = new Color32(255, 248, 222, 255);
        private static readonly Color Cyan = new Color32(61, 226, 255, 255);
        private static Sprite _circle;
        private static Sprite _card;
        private static TMP_FontAsset _font;

        public static void Apply()
        {
            var canvasObject = GameObject.Find("MainCanvas");
            if (canvasObject == null || canvasObject.scene.name != "MainGame")
                throw new InvalidOperationException("Open MainGame with its existing MainCanvas before styling the HUD.");
            if (Application.isPlaying) throw new InvalidOperationException("Apply the HUD theme outside Play mode.");

            var canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null) throw new InvalidOperationException("MainCanvas has no Canvas component.");
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            Undo.RegisterFullObjectHierarchyUndo(canvasObject, "Apply Sky Citadel HUD");
            _circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            _card = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (_circle == null || _card == null) throw new InvalidOperationException("Unity built-in UI sprites were not found.");
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
            if (_font == null) _font = TMP_Settings.defaultFontAsset;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = Undo.AddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform balls = Require(canvas.transform, "HUD_BallCounter");
            RectTransform settings = Require(canvas.transform, "HUD_SettingsButton");
            RectTransform exit = Require(canvas.transform, "HUD_ExitButton");
            RectTransform gold = Require(canvas.transform, "HUD_GoldDisplay");
            RectTransform skillBar = Require(canvas.transform, "HUD_SkillBar");
            RectTransform mega = Require(skillBar, "SkillBtn_MegaBall");
            RectTransform infinity = Require(skillBar, "SkillBtn_InfinityBall");

            Place(balls, new Vector2(0.5f, 1), new Vector2(0, -104), new Vector2(220, 106));
            Card(balls, false);
            Image ballIcon = balls.Find("Icon").GetComponent<Image>();
            Orb(ballIcon.rectTransform, 58, new Vector2(-62, -4));
            Text(Require(balls, "CountText"), null, 57, Ivory, new Vector2(37, -7), new Vector2(105, 72));
            Label(balls, "Sky_CounterTitle", "BALLS", 20, new Vector2(23, 32), new Vector2(130, 26), Gold);

            Place(settings, new Vector2(1, 1), new Vector2(-100, -106), new Vector2(128, 128));
            Card(settings, true);
            var oldSettingsIcon = settings.Find("Icon");
            if (oldSettingsIcon != null) oldSettingsIcon.gameObject.SetActive(false);
            Gear(settings);
            ButtonColors(settings);

            Place(exit, new Vector2(0, 1), new Vector2(76, -245), new Vector2(96, 96));
            Card(exit, true);
            var exitText = exit.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            if (exitText != null) exitText.text = string.Empty;
            Shape(exit, "Sky_BackShaft", new Vector2(4, 0), new Vector2(35, 8), Ivory);
            Shape(exit, "Sky_BackUpper", new Vector2(-12, 10), new Vector2(29, 8), Ivory, false, 43);
            Shape(exit, "Sky_BackLower", new Vector2(-12, -10), new Vector2(29, 8), Ivory, false, -43);
            ButtonColors(exit);

            Place(gold, new Vector2(0, 1), new Vector2(225, -239), new Vector2(165, 66));
            Card(gold, false);
            var oldCoinText = gold.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            if (oldCoinText != null) oldCoinText.text = string.Empty;
            Shape(gold, "Sky_Coin", new Vector2(-54, 0), new Vector2(35, 35), Gold, true);
            Shape(gold, "Sky_CoinInlay", new Vector2(-54, 0), new Vector2(22, 22), Ivory, true);
            Shape(gold, "Sky_CoinMark", new Vector2(-54, 0), new Vector2(8, 16), Gold);
            Text(Require(gold, "GoldText"), null, 28, Ivory, new Vector2(19, 0), new Vector2(109, 50));

            // Keep the existing paths and all SkillButtonUI references; only their presentation changes.
            Place(skillBar, new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            skillBar.anchorMin = new Vector2(0, 0);
            skillBar.anchorMax = new Vector2(1, 0);
            StyleSkill(mega, false);
            StyleSkill(infinity, true);

            RectTransform stage = Child(canvas.transform, "HUD_SkyCitadelStage");
            stage.SetSiblingIndex(0); // Existing dialogs and tutorial remain above the HUD.
            Place(stage, new Vector2(0, 1), new Vector2(132, -112), new Vector2(188, 160));
            Card(stage, false);
            Label(stage, "Sky_Title", "STAGE", 25, new Vector2(0, 46), new Vector2(155, 36), Gold);
            TextMeshProUGUI stageText = Label(stage, "Sky_Level", "1", 78, new Vector2(0, -17), new Vector2(156, 95), Ivory);
            var level = UnityEngine.Object.FindFirstObjectByType<LevelManager>();
            if (level != null) stageText.SetText("{0}", level.CurrentLevelIndex);
            var stageController = stage.GetComponent<SkyCitadelStageLabel>();
            if (stageController == null) stageController = Undo.AddComponent<SkyCitadelStageLabel>(stage.gameObject);
            stageController.Configure(stageText, new[] { stage, balls, settings, exit, gold }, new[] { mega, infinity });
            EditorUtility.SetDirty(stageController);
            EditorSceneManager.MarkSceneDirty(canvasObject.scene);
            Debug.Log("[SkyCitadelHUDBuilder] Existing HUD restyled; stage bound to LevelManager; dialogs preserved.");
        }

        private static void StyleSkill(RectTransform button, bool infinity)
        {
            Place(button, new Vector2(infinity ? 1 : 0, 0), new Vector2(infinity ? -147 : 147, 354), new Vector2(206, 206));
            Card(button, true);
            ButtonColors(button);
            Image icon = Shape(button, "Sky_Orb", new Vector2(0, 20), new Vector2(106, 106), Gold, true);
            Place(icon.rectTransform,new Vector2(.5f,.5f),new Vector2(0,18),new Vector2(158,158));
            ReferenceIcon(icon, infinity ? "Infinity" : "Mega",
                infinity ? new Rect(787,48,335,420) : new Rect(284,26,505,525));
            foreach(var name in new[]{"Sky_OrbLeft","Sky_OrbRight"})
            { var old=button.Find(name); if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Text(Require(button, "Label"), infinity ? "INFINITY" : "MEGA", infinity ? 24 : 30, Ivory,
                new Vector2(0, -60), new Vector2(176, 43));

            RectTransform glow = Require(button, "ActiveGlow");
            Place(glow, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(225, 225));
            SetImage(glow.GetComponent<Image>(), new Color(0.24f, 0.88f, 1f, 0.55f), true);
            glow.SetAsFirstSibling();
            RectTransform timer = Require(button, "TimerText");
            Text(timer, null, 40, Ivory, new Vector2(0, -15), new Vector2(170, 55));
            timer.SetAsLastSibling();
            RectTransform locked = Require(button, "LockBadge");
            Place(locked, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(190, 190));
            SetImage(locked.GetComponent<Image>(), new Color(0.025f, 0.05f, 0.12f, 0.84f), true);
            Text(Require(locked, "LockText"), "LOCKED", 28, Ivory, Vector2.zero, new Vector2(170, 52));
            locked.SetAsLastSibling();
            RectTransform badge = Require(button, "UsesBadge");
            Place(badge, new Vector2(0.5f, 0.5f), new Vector2(75, -70), new Vector2(60, 60));
            SetImage(badge.GetComponent<Image>(), new Color32(187, 42, 62, 255), true);
            Outline(badge, Ivory, 2);
            Text(Require(badge, "UsesText"), null, 36, Ivory, Vector2.zero, new Vector2(55, 55));
            badge.SetAsLastSibling();
        }

        private static void Gear(RectTransform parent)
        {
            RectTransform gear = Child(parent, "Sky_Gear");
            Place(gear, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(66, 66));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                float radians = angle * Mathf.Deg2Rad;
                Shape(gear, "Tooth" + i, new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * 28,
                    new Vector2(17, 23), Ivory, false, -angle);
            }
            Shape(gear, "Body", Vector2.zero, new Vector2(60, 60), Ivory, true);
            Shape(gear, "Hub", Vector2.zero, new Vector2(30, 30), Navy, true);
            Shape(gear, "HubLight", new Vector2(-3, 3), new Vector2(19, 19), Blue, true);
        }

        private static void Orb(RectTransform root, float diameter, Vector2 position)
        {
            Place(root, new Vector2(0.5f, 0.5f), position, new Vector2(diameter, diameter));
            ReferenceIcon(root.GetComponent<Image>(),"Ball",new Rect(0,108,302,312));
        }

        private static void ReferenceIcon(Image image,string name,Rect rect)
        {
            for(int i=image.transform.childCount-1;i>=0;i--)
                if(image.transform.GetChild(i).name.StartsWith("Sky_"))
                    UnityEngine.Object.DestroyImmediate(image.transform.GetChild(i).gameObject);
            var effect=image.GetComponent<SkyCitadelCircleGraphic>();
            if(effect!=null)UnityEngine.Object.DestroyImmediate(effect);
            string path="Assets/_Project/Art/SkyCitadel/Textures/UI_"+name+".asset";
            string texturePath="Assets/_Project/Art/SkyCitadel/Textures/T_CitadelReferenceIcons.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
            if(importer.npotScale!=TextureImporterNPOTScale.None || importer.maxTextureSize!=2048)
            {
                importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)
            {
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                sprite.name="Citadel"+name;AssetDatabase.CreateAsset(sprite,path);
            }
            image.sprite=sprite;image.type=Image.Type.Simple;image.preserveAspect=true;
            image.color=Color.white;image.raycastTarget=false;
        }

        private static void Card(RectTransform root, bool round)
        {
            var background = root.GetComponent<Image>();
            if (background == null) background = Undo.AddComponent<Image>(root.gameObject);
            SetImage(background, Gold, round);
            background.raycastTarget = root.GetComponent<Button>() != null;
            Outline(root, Navy, round ? 3 : 2);
            Image inner = Shape(root, "Sky_Face", Vector2.zero, root.sizeDelta - Vector2.one * 12, Navy, round);
            inner.transform.SetAsFirstSibling();
            Image highlight = Shape(inner.transform, "Sky_Blue", new Vector2(0, 2), root.sizeDelta - Vector2.one * 22, Blue, round);
            highlight.color = Blue;
        }

        private static void ButtonColors(RectTransform root)
        {
            var button = root.GetComponent<Button>();
            if (button == null) return;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.86f);
            colors.pressedColor = new Color(0.72f, 0.87f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.63f, 0.68f, 0.75f, 1f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
        }

        private static void Outline(RectTransform root, Color color, float width)
        {
            var outline = root.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null) outline = Undo.AddComponent<UnityEngine.UI.Outline>(root.gameObject);
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
            outline.useGraphicAlpha = true;
        }

        private static Image Shape(Transform parent, string name, Vector2 position, Vector2 size, Color color,
            bool round = false, float rotation = 0)
        {
            RectTransform rect = Child(parent, name);
            Place(rect, new Vector2(0.5f, 0.5f), position, size);
            rect.localRotation = Quaternion.Euler(0, 0, rotation);
            Image image = rect.GetComponent<Image>();
            if (image == null) image = Undo.AddComponent<Image>(rect.gameObject);
            SetImage(image, color, round);
            return image;
        }

        private static void SetImage(Image image, Color color, bool round)
        {
            if (image == null) return;
            image.sprite = round ? _circle : _card;
            image.type = round ? Image.Type.Simple : Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = round;
            if(round)
            {
                var circle=image.GetComponent<SkyCitadelCircleGraphic>();
                if(circle==null)circle=Undo.AddComponent<SkyCitadelCircleGraphic>(image.gameObject);
                // Mesh modifiers run in component order; build the vector circle before outlining it.
                var components=image.GetComponents<Component>();
                int circleIndex=Array.IndexOf(components,circle);
                int outlineIndex=Array.IndexOf(components,image.GetComponent<Outline>());
                while(outlineIndex>=0 && circleIndex>outlineIndex)
                { UnityEditorInternal.ComponentUtility.MoveComponentUp(circle);circleIndex--; }
            }
        }

        private static TextMeshProUGUI Label(Transform parent, string name, string value, float size, Vector2 position, Vector2 box, Color color)
        {
            RectTransform rect = Child(parent, name);
            return Text(rect, value, size, color, position, box);
        }

        private static TextMeshProUGUI Text(RectTransform rect, string value, float size, Color color, Vector2 position, Vector2 box)
        {
            Place(rect, new Vector2(0.5f, 0.5f), position, box);
            var text = rect.GetComponent<TextMeshProUGUI>();
            if (text == null) text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
            if (_font != null) text.font = _font;
            if (value != null) text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * 0.7f;
            text.fontSizeMax = size;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.margin = Vector4.zero;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform Child(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create Sky Citadel HUD detail");
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform Require(Transform parent, string name)
        {
            var rect = parent.Find(name) as RectTransform;
            if (rect == null) throw new InvalidOperationException("Missing existing HUD object: " + parent.name + "/" + name);
            return rect;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
    }
}
