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

            Place(balls, new Vector2(0.5f, 1), new Vector2(0, -112), new Vector2(230, 110));
            Card(balls, false);
            Rounded(balls, "Sky_NumberWell", new Vector2(40, -8), new Vector2(103, 74), 19,
                Color.white, new Color32(238, 245, 255, 255), new Color32(169, 192, 224, 255), .12f);
            Image ballIcon = balls.Find("Icon").GetComponent<Image>();
            Orb(ballIcon.rectTransform, 82, new Vector2(-63, -1));
            TextMeshProUGUI countText = Text(Require(balls, "CountText"), null, 54, Navy, new Vector2(40, -12), new Vector2(98, 72));
            countText.transform.SetAsLastSibling();
            ballIcon.transform.SetAsLastSibling();
            Label(balls, "Sky_CounterTitle", "TOP", 19, new Vector2(37, 37), new Vector2(110, 26), Ivory).transform.SetAsLastSibling();

            Place(settings, new Vector2(1, 1), new Vector2(-105, -112), new Vector2(140, 140));
            Card(settings, true);
            var oldSettingsIcon = settings.Find("Icon");
            if (oldSettingsIcon != null) oldSettingsIcon.gameObject.SetActive(false);
            Gear(settings);
            ButtonColors(settings);

            Place(exit, new Vector2(0, 1), new Vector2(80, -281), new Vector2(96, 96));
            Card(exit, true);
            var exitText = exit.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            if (exitText != null) exitText.text = string.Empty;
            Shape(exit, "Sky_BackShaft", new Vector2(4, 0), new Vector2(35, 8), Ivory);
            Shape(exit, "Sky_BackUpper", new Vector2(-12, 10), new Vector2(29, 8), Ivory, false, 43);
            Shape(exit, "Sky_BackLower", new Vector2(-12, -10), new Vector2(29, 8), Ivory, false, -43);
            ButtonColors(exit);

            Place(gold, new Vector2(0, 1), new Vector2(239, -274), new Vector2(190, 76));
            Card(gold, false);
            var oldCoinText = gold.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            if (oldCoinText != null) oldCoinText.text = string.Empty;
            Rounded(gold, "Sky_Coin", new Vector2(-65, 0), new Vector2(49, 49), 25,
                new Color32(255, 249, 177, 255), Gold, new Color32(163, 89, 12, 255), .6f);
            Rounded(gold, "Sky_CoinInlay", new Vector2(-65, 0), new Vector2(36, 36), 18,
                new Color32(172, 102, 14, 255), new Color32(255, 218, 81, 255), new Color32(255, 238, 146, 255), .1f);
            Rounded(gold, "Sky_CoinMark", new Vector2(-65, 0), new Vector2(11, 24), 3,
                Ivory, new Color32(255, 218, 91, 255), new Color32(183, 111, 15, 255), .25f);
            Text(Require(gold, "GoldText"), null, 31, Ivory, new Vector2(18, -1), new Vector2(126, 53));

            // Keep the existing paths and all SkillButtonUI references; only their presentation changes.
            Place(skillBar, new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            skillBar.anchorMin = new Vector2(0, 0);
            skillBar.anchorMax = new Vector2(1, 0);
            StyleSkill(mega, false);
            StyleSkill(infinity, true);

            RectTransform stage = Child(canvas.transform, "HUD_SkyCitadelStage");
            stage.SetSiblingIndex(0); // Existing dialogs and tutorial remain above the HUD.
            Place(stage, new Vector2(0, 1), new Vector2(150, -132), new Vector2(220, 188));
            Card(stage, false);
            Rounded(stage, "Sky_IvoryEdge", new Vector2(0, -12), new Vector2(188, 133), 32,
                new Color32(153, 198, 251, 255), new Color32(245, 250, 255, 255), new Color32(105, 142, 199, 255));
            Rounded(stage, "Sky_IvoryPlate", new Vector2(0, -9), new Vector2(174, 119), 26,
                Color.white, new Color32(248, 246, 236, 255), new Color32(208, 222, 241, 255), .22f);
            RectTransform titleCap = Child(stage, "Sky_TitleCap");
            Place(titleCap, new Vector2(.5f, .5f), new Vector2(0, 72), new Vector2(208, 64));
            Card(titleCap, false);
            titleCap.SetAsLastSibling();
            string stageTitle = _font != null && _font.HasCharacter('\u015e', true, true) ? "A\u015eAMA" : "STAGE";
            Label(stage, "Sky_Title", stageTitle, 29, new Vector2(0, 73), new Vector2(170, 40), Ivory).transform.SetAsLastSibling();
            TextMeshProUGUI stageText = Label(stage, "Sky_Level", "1", 88, new Vector2(0, -18), new Vector2(158, 110), Blue);
            stageText.enableVertexGradient = true;
            stageText.colorGradient = new VertexGradient(new Color32(120, 204, 255, 255), new Color32(120, 204, 255, 255),
                new Color32(10, 66, 157, 255), new Color32(10, 66, 157, 255));
            stageText.color = Color.white;
            // TMP's own SDF outline is crisp without duplicating the live number into decorative labels.
            stageText.outlineColor = new Color32(6, 27, 76, 255);
            stageText.outlineWidth = .18f;
            stageText.transform.SetAsLastSibling();
            Rounded(stage, "Sky_LeftClasp", new Vector2(-108, -10), new Vector2(15, 60), 6,
                Ivory, Gold, new Color32(180, 93, 12, 255), .4f);
            Rounded(stage, "Sky_RightClasp", new Vector2(108, -10), new Vector2(15, 60), 6,
                Ivory, Gold, new Color32(180, 93, 12, 255), .4f);
            var level = UnityEngine.Object.FindFirstObjectByType<LevelManager>();
            if (level != null) stageText.SetText("{0}", level.CurrentLevelIndex);
            var stageController = stage.GetComponent<SkyCitadelStageLabel>();
            if (stageController == null) stageController = Undo.AddComponent<SkyCitadelStageLabel>(stage.gameObject);
            stageController.Configure(stageText, new[] { stage, balls, settings, exit, gold }, new[] { mega, infinity });
            EditorUtility.SetDirty(stageController);
            StyleSettingsDialog(canvas.transform);
            StyleExitDialog(canvas.transform);
            PutModalsAboveHUD(canvas.transform);
            EditorSceneManager.MarkSceneDirty(canvasObject.scene);
            Debug.Log("[SkyCitadelHUDBuilder] HUD and settings/exit visuals updated; live bindings preserved; modals draw above the HUD.");
        }

        private static void StyleSettingsDialog(Transform canvas)
        {
            RectTransform dialog = Require(canvas, "InGameSettingsDialog");
            RectTransform card = PrepareModal(dialog, new Vector2(660, 860));
            Text(Require(card, "Title"), "AYARLAR", 43, Ivory, new Vector2(-30, 331), new Vector2(380, 66));
            Rounded(card, "Sky_HeaderLine", new Vector2(0, 271), new Vector2(522, 3), 1,
                Gold, Gold, Gold);

            RectTransform close = Require(card, "CloseBtn");
            Place(close, new Vector2(.5f, .5f), new Vector2(259, 350), new Vector2(96, 96));
            Card(close, true);
            ButtonColors(close);
            Text(Require(close, "Text"), string.Empty, 32, Ivory, Vector2.zero, new Vector2(72, 72));
            Shape(close, "Sky_CloseSlashA", Vector2.zero, new Vector2(37, 8), Ivory, false, 45);
            Shape(close, "Sky_CloseSlashB", Vector2.zero, new Vector2(37, 8), Ivory, false, -45);

            StyleSettingRow(Require(card, "MusicToggle"), 180, Supported("M\u00dcZ\u0130K", "MUZIK"));
            StyleSettingRow(Require(card, "SfxToggle"), 58, Supported("SES EFEKTLER\u0130", "SES EFEKTLERI"));

            // Vibration already has a separate row/label and a nested bound button. Keep those exact paths.
            RectTransform vibrationRow = Require(card, "VibrationRow");
            Place(vibrationRow, new Vector2(.5f, .5f), new Vector2(0, -64), new Vector2(560, 104));
            RectTransform vibrationButton = Require(vibrationRow, "VibrationToggleBtn");
            Place(vibrationButton, new Vector2(.5f, .5f), Vector2.zero, new Vector2(560, 104));
            Card(vibrationButton, false);
            ButtonColors(vibrationButton);
            SettingStatus(vibrationButton, Require(vibrationButton, "Text"));
            vibrationButton.SetAsFirstSibling();
            TextMeshProUGUI vibrationLabel = Text(Require(vibrationRow, "Label"), Supported("T\u0130TRE\u015e\u0130M", "TITRESIM"),
                31, Ivory, new Vector2(-78, 0), new Vector2(292, 68));
            vibrationLabel.alignment = TextAlignmentOptions.MidlineLeft;
            vibrationLabel.transform.SetAsLastSibling();

            StyleModalButton(Require(card, "ResumeBtn"), "DEVAM ET", new Vector2(0, -235), new Vector2(560, 102), false);
            StyleModalButton(Require(card, "ExitLevelBtn"), Supported("SEV\u0130YEDEN \u00c7IK", "SEVIYEDEN CIK"),
                new Vector2(0, -352), new Vector2(560, 96), true);
            close.SetAsLastSibling();
        }

        private static void StyleSettingRow(RectTransform row, float y, string label)
        {
            Place(row, new Vector2(.5f, .5f), new Vector2(0, y), new Vector2(560, 104));
            Card(row, false);
            ButtonColors(row);
            TextMeshProUGUI title = Text(Require(row, "Label"), label, 31, Ivory,
                new Vector2(-78, 0), new Vector2(292, 68));
            title.alignment = TextAlignmentOptions.MidlineLeft;
            SettingStatus(row, Require(row, "Status"));
        }

        private static void SettingStatus(RectTransform button, RectTransform status)
        {
            Rounded(button, "Sky_StatusWell", new Vector2(181, 1), new Vector2(127, 62), 22,
                new Color32(2, 22, 58, 255), new Color32(5, 35, 77, 255), new Color32(20, 72, 121, 255));
            Text(status, null, 31, Cyan, new Vector2(181, 1), new Vector2(116, 57));
            status.SetAsLastSibling();
        }

        private static void StyleExitDialog(Transform canvas)
        {
            RectTransform dialog = Require(canvas, "ExitConfirmDialog");
            RectTransform card = PrepareModal(dialog, new Vector2(680, 520));
            Text(Require(card, "Title"), Supported("SEV\u0130YEDEN \u00c7IKILSIN MI?", "SEVIYEDEN CIKILSIN MI?"),
                36, Ivory, new Vector2(0, 164), new Vector2(588, 68));
            Rounded(card, "Sky_HeaderLine", new Vector2(0, 113), new Vector2(544, 3), 1,
                Gold, Gold, Gold);
            TextMeshProUGUI warning = Text(Require(card, "Warning"),
                Supported("Bu seviyeden \u00e7\u0131karsan\n1 CAN kaybedeceksin.", "Bu seviyeden cikarsan\n1 CAN kaybedeceksin."),
                33, Ivory, new Vector2(0, 21), new Vector2(550, 116));
            warning.textWrappingMode = TextWrappingModes.Normal;
            StyleModalButton(Require(card, "CancelBtn"), Supported("VAZGE\u00c7", "VAZGEC"),
                new Vector2(-151, -163), new Vector2(272, 104), false);
            StyleModalButton(Require(card, "ConfirmBtn"), Supported("\u00c7IK (-1 CAN)", "CIK (-1 CAN)"),
                new Vector2(151, -163), new Vector2(272, 104), true);
        }

        private static RectTransform PrepareModal(RectTransform dialog, Vector2 size)
        {
            Stretch(dialog);
            RectTransform content = Require(dialog, "Content");
            Stretch(content);
            Image backdrop = content.GetComponent<Image>();
            if (backdrop == null) backdrop = Undo.AddComponent<Image>(content.gameObject);
            backdrop.sprite = null;
            backdrop.type = Image.Type.Simple;
            backdrop.color = new Color32(3, 13, 34, 212);
            backdrop.raycastTarget = true;
            RectTransform card = Require(content, "Card");
            Place(card, new Vector2(.5f, .5f), Vector2.zero, size);
            Card(card, false);
            // The backdrop blocks gameplay input even on the rounded card's decorative areas.
            card.GetComponent<Image>().raycastTarget = true;
            return card;
        }

        private static void StyleModalButton(RectTransform button, string value, Vector2 position, Vector2 size, bool danger)
        {
            Place(button, new Vector2(.5f, .5f), position, size);
            Card(button, false);
            ButtonColors(button);
            if (danger)
            {
                Image face = Require(Require(button, "Sky_Chrome"), "BlueFace").GetComponent<Image>();
                Gradient(face, new Color32(204, 78, 75, 255), new Color32(128, 34, 54, 255),
                    new Color32(69, 18, 44, 255), 23, .18f);
            }
            Text(Require(button, "Text"), value, size.x > 400 ? 34 : 29, Ivory,
                Vector2.zero, new Vector2(size.x - 38, size.y - 20));
        }

        private static string Supported(string text, string fallback)
        {
            if (_font == null) return fallback;
            foreach (char character in text)
                if (character > 127 && !_font.HasCharacter(character, true, true)) return fallback;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void PutModalsAboveHUD(Transform canvas)
        {
            // Stable partition: preserve the HUD order and the relative order of all non-HUD overlays.
            // Moving only HUD roots also catches existing or future dialogs without a hardcoded modal list.
            var original = new Transform[canvas.childCount];
            for (int i = 0; i < original.Length; i++) original[i] = canvas.GetChild(i);
            int hudIndex = 0;
            foreach (Transform child in original)
                if (child.name.StartsWith("HUD_", StringComparison.Ordinal)) child.SetSiblingIndex(hudIndex++);

            Transform settings = canvas.Find("InGameSettingsDialog");
            Transform exit = canvas.Find("ExitConfirmDialog");
            if (settings != null && exit != null && exit.GetSiblingIndex() < settings.GetSiblingIndex())
                exit.SetSiblingIndex(settings.GetSiblingIndex());
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
            Gradient(badge.GetComponent<Image>(), new Color32(255, 116, 105, 255),
                new Color32(211, 36, 52, 255), new Color32(119, 6, 28, 255), 30, .25f);
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
            Image body = Shape(gear, "Body", Vector2.zero, new Vector2(60, 60), Ivory, true);
            Gradient(body, Color.white, Ivory, new Color32(158, 184, 221, 255), 30, .2f);
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
            var gradient=image.GetComponent<SkyCitadelRoundedGradient>();
            if(gradient!=null)UnityEngine.Object.DestroyImmediate(gradient);
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
            float radius = round ? Mathf.Min(root.sizeDelta.x, root.sizeDelta.y) * .5f : Mathf.Min(38f, root.sizeDelta.y * .33f);
            Gradient(background, new Color32(25, 75, 129, 255), Navy, new Color32(1, 13, 38, 255), radius);
            background.raycastTarget = root.GetComponent<Button>() != null;
            Outline(root, new Color32(3, 22, 48, 200), 2);
            Transform oldFace = root.Find("Sky_Face");
            if (oldFace != null) oldFace.gameObject.SetActive(false);
            RectTransform chrome = Child(root, "Sky_Chrome");
            Place(chrome, new Vector2(.5f, .5f), Vector2.zero, root.sizeDelta);
            chrome.SetAsFirstSibling();
            Rounded(chrome, "OuterGold", new Vector2(0, 3), root.sizeDelta - Vector2.one * 5, radius - 2,
                new Color32(255, 251, 194, 255), new Color32(233, 163, 34, 255), new Color32(155, 76, 6, 255), .58f);
            Rounded(chrome, "PolishedGold", new Vector2(0, 4), root.sizeDelta - Vector2.one * 11, radius - 5,
                new Color32(255, 213, 84, 255), new Color32(169, 85, 11, 255), new Color32(255, 235, 147, 255), .34f);
            Rounded(chrome, "BlueBevel", new Vector2(0, 4), root.sizeDelta - Vector2.one * 17, radius - 8,
                new Color32(2, 25, 72, 255), new Color32(4, 69, 144, 255), new Color32(61, 172, 235, 255));
            Rounded(chrome, "BlueFace", new Vector2(0, 5), root.sizeDelta - Vector2.one * 25, radius - 12,
                new Color32(23, 125, 199, 255), new Color32(4, 64, 133, 255), new Color32(3, 27, 81, 255), .2f);
        }

        private static Image Rounded(Transform parent, string name, Vector2 position, Vector2 size, float radius,
            Color top, Color middle, Color bottom, float gloss = 0f)
        {
            RectTransform rect = Child(parent, name);
            Place(rect, new Vector2(.5f, .5f), position, size);
            Image image = rect.GetComponent<Image>();
            if (image == null) image = Undo.AddComponent<Image>(rect.gameObject);
            Gradient(image, top, middle, bottom, Mathf.Max(0, radius), gloss);
            image.raycastTarget = false;
            rect.SetAsLastSibling();
            return image;
        }

        private static void Gradient(Image image, Color top, Color middle, Color bottom, float radius, float gloss = 0f)
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            var oldCircle = image.GetComponent<SkyCitadelCircleGraphic>();
            if (oldCircle != null) oldCircle.enabled = false;
            var gradient = image.GetComponent<SkyCitadelRoundedGradient>();
            if (gradient == null) gradient = Undo.AddComponent<SkyCitadelRoundedGradient>(image.gameObject);
            gradient.enabled = true;
            gradient.Configure(top, middle, bottom, radius, gloss);
            // Outline must receive the finished rounded mesh, never the source Image quad.
            var components = image.GetComponents<Component>();
            int gradientIndex = Array.IndexOf(components, gradient);
            int outlineIndex = Array.IndexOf(components, image.GetComponent<Outline>());
            while (outlineIndex >= 0 && gradientIndex > outlineIndex)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(gradient);
                gradientIndex--;
            }
            EditorUtility.SetDirty(gradient);
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
            var gradient = image.GetComponent<SkyCitadelRoundedGradient>();
            if (gradient != null) gradient.enabled = false;
            if(round)
            {
                var circle=image.GetComponent<SkyCitadelCircleGraphic>();
                if(circle==null)circle=Undo.AddComponent<SkyCitadelCircleGraphic>(image.gameObject);
                circle.enabled = true;
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
