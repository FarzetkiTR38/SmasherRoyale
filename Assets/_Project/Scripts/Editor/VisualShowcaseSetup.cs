using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Automates the creation and setup of the Visual & Environment Showcase (Option B):
    /// - Stylized Kingdom Scenery Backdrop Quad
    /// - Royal Pedestal Stand with Golden Column and Purple Beveled Platform
    /// - 6-Color Paint Bottle Shelf (30 bottles: Red, Orange, Yellow, Blue, Purple, Pink)
    /// </summary>
    public static class VisualShowcaseSetup
    {
        private static readonly string[] ColorNames = new string[] { "Red", "Orange", "Yellow", "Blue", "Purple", "Pink" };

        private static readonly Color[] BottleColors = new Color[]
        {
            new Color(0.92f, 0.12f, 0.16f), // Red
            new Color(1.00f, 0.50f, 0.05f), // Orange
            new Color(1.00f, 0.82f, 0.08f), // Yellow
            new Color(0.10f, 0.55f, 0.95f), // Blue
            new Color(0.55f, 0.16f, 0.85f), // Purple
            new Color(0.96f, 0.20f, 0.58f)  // Pink
        };

        private static readonly float[] ColXPositions = new float[] { -1.80f, -1.08f, -0.36f, 0.36f, 1.08f, 1.80f };

        public static string SetupAllScenes()
        {
            EnsureMaterialsAndTextures();

            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/MainGame.unity",
                "Assets/_Project/Scenes/PhysicsTestScene.unity"
            };

            var results = new List<string>();
            foreach (string scenePath in scenes)
            {
                if (!File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath);
                SetupActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                results.Add("Configured " + Path.GetFileName(scenePath));
            }

            return string.Join("; ", results);
        }

        public static void EnsureMaterialsAndTextures()
        {
            string texDir = "Assets/_Project/Textures/Environment";
            string matDir = "Assets/_Project/Materials/Bottles";

            if (!AssetDatabase.IsValidFolder(texDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Textures"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Textures");
                AssetDatabase.CreateFolder("Assets/_Project/Textures", "Environment");
            }

            if (!AssetDatabase.IsValidFolder(matDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Materials");
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "Bottles");
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? litShader;

            // Backdrop Material
            string bdMatPath = "Assets/_Project/Materials/M_KingdomBackdrop.mat";
            Material bdMat = AssetDatabase.LoadAssetAtPath<Material>(bdMatPath);
            if (bdMat == null)
            {
                bdMat = new Material(unlitShader);
                AssetDatabase.CreateAsset(bdMat, bdMatPath);
            }
            Texture2D bdTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texDir + "/KingdomBackdrop.jpg");
            if (bdTex != null)
            {
                if (bdMat.HasProperty("_BaseMap")) bdMat.SetTexture("_BaseMap", bdTex);
                if (bdMat.HasProperty("_MainTex")) bdMat.SetTexture("_MainTex", bdTex);
            }

            // Cap Striped Textures and Materials
            Color whiteCol = new Color(0.98f, 0.98f, 0.98f, 1f);
            int texWidth = 128;
            int texHeight = 32;
            int numStripes = 16;
            int stripeWidth = texWidth / numStripes;

            for (int i = 0; i < 6; i++)
            {
                string colName = ColorNames[i];
                Color colVal = BottleColors[i];

                string texPath = texDir + "/CandyStripe_" + colName + ".png";
                if (!File.Exists(texPath))
                {
                    Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                    for (int y = 0; y < texHeight; y++)
                    {
                        for (int x = 0; x < texWidth; x++)
                        {
                            bool isWhite = ((x / stripeWidth) % 2) == 0;
                            tex.SetPixel(x, y, isWhite ? whiteCol : colVal);
                        }
                    }
                    tex.Apply();
                    File.WriteAllBytes(texPath, tex.EncodeToPNG());
                    AssetDatabase.ImportAsset(texPath);

                    var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.wrapMode = TextureWrapMode.Repeat;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.SaveAndReimport();
                    }
                }

                // Striped Cap Material
                string capMatPath = matDir + "/M_Cap_" + colName + ".mat";
                Material capMat = AssetDatabase.LoadAssetAtPath<Material>(capMatPath);
                if (capMat == null)
                {
                    capMat = new Material(litShader);
                    AssetDatabase.CreateAsset(capMat, capMatPath);
                }
                var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (capMat.HasProperty("_BaseMap")) capMat.SetTexture("_BaseMap", loadedTex);
                if (capMat.HasProperty("_MainTex")) capMat.SetTexture("_MainTex", loadedTex);
                if (capMat.HasProperty("_Smoothness")) capMat.SetFloat("_Smoothness", 0.75f);
                EditorUtility.SetDirty(capMat);

                // Body Material
                string bodyMatPath = matDir + "/M_Bottle_" + colName + ".mat";
                Material bodyMat = AssetDatabase.LoadAssetAtPath<Material>(bodyMatPath);
                if (bodyMat == null)
                {
                    bodyMat = new Material(litShader);
                    AssetDatabase.CreateAsset(bodyMat, bodyMatPath);
                }
                bodyMat.color = colVal;
                if (bodyMat.HasProperty("_BaseColor")) bodyMat.SetColor("_BaseColor", colVal);
                if (bodyMat.HasProperty("_Smoothness")) bodyMat.SetFloat("_Smoothness", 0.88f);
                if (bodyMat.HasProperty("_Metallic")) bodyMat.SetFloat("_Metallic", 0.05f);
                EditorUtility.SetDirty(bodyMat);
            }

            AssetDatabase.SaveAssets();
        }

        public static void SetupActiveScene()
        {
            // 0. Camera & Mortar (Preserving user's forward placement)
            var cam = GameObject.Find("Main Camera");
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 3.11f, -5.43f);
                cam.transform.rotation = Quaternion.Euler(8.16f, 0f, 0f);
            }

            var mortar = GameObject.Find("Mortar");
            if (mortar != null)
            {
                mortar.transform.position = new Vector3(0f, 1.07f, -2.52f);
            }

            // 1. Lighting
            var dirLightGo = GameObject.Find("Directional Light");
            if (dirLightGo != null)
            {
                dirLightGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
                var l = dirLightGo.GetComponent<Light>();
                if (l != null)
                {
                    l.color = new Color(1.0f, 0.97f, 0.92f);
                    l.intensity = 1.20f;
                    l.shadows = LightShadows.Soft;
                }
            }

            // 2. Backdrop
            var existingBackdrop = GameObject.Find("KingdomBackdrop");
            if (existingBackdrop != null)
            {
                UnityEngine.Object.DestroyImmediate(existingBackdrop);
            }
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "KingdomBackdrop";
            var bdCol = backdrop.GetComponent<Collider>();
            if (bdCol != null) UnityEngine.Object.DestroyImmediate(bdCol);

            var bdMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_KingdomBackdrop.mat");
            backdrop.GetComponent<Renderer>().sharedMaterial = bdMat;
            backdrop.transform.position = new Vector3(0f, 6.6f, 32.0f);
            backdrop.transform.rotation = Quaternion.Euler(7.5f, 0f, 0f);
            backdrop.transform.localScale = new Vector3(26.0f, 40.0f, 1f);

            // 3. 3D Ground & Stylized Grass
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_StylizedGrass.mat");
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = new Vector3(0f, 0f, 13.5f);
                ground.transform.localScale = new Vector3(45f, 1f, 37f);
                var mr = ground.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = true;
                    mr.sharedMaterial = grassMat;
                    mr.receiveShadows = true;
                }
            }

            // 4. DirtPatch under Stand
            var existingDirt = GameObject.Find("DirtPatch");
            if (existingDirt != null) UnityEngine.Object.DestroyImmediate(existingDirt);

            var dirtMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_DirtPatch.mat");
            var dirtGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dirtGo.name = "DirtPatch";
            dirtGo.transform.position = new Vector3(0f, 0.015f, 7.50f);
            dirtGo.transform.localScale = new Vector3(4.8f, 0.01f, 4.8f);
            dirtGo.GetComponent<Renderer>().sharedMaterial = dirtMat;
            dirtGo.GetComponent<Renderer>().receiveShadows = true;
            var dirtCol = dirtGo.GetComponent<Collider>();
            if (dirtCol != null) UnityEngine.Object.DestroyImmediate(dirtCol);

            // 5. Decorative Pebbles and Flowers around DirtPatch
            var existingDecor = GameObject.Find("GroundDecorations");
            if (existingDecor != null) UnityEngine.Object.DestroyImmediate(existingDecor);

            GameObject decorContainer = new GameObject("GroundDecorations");
            decorContainer.transform.position = new Vector3(0f, 0f, 7.50f);

            Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_ObjectStone.mat") ?? grassMat;
            Material flowerPinkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Bottles/M_Bottle_Pink.mat");
            Material flowerYellowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Bottles/M_Bottle_Yellow.mat");
            Material flowerBlueMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Bottles/M_Bottle_Blue.mat");

            float radius = 2.45f;
            int numPoints = 14;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * (Mathf.PI * 2f / numPoints) + 0.15f;
                float r = radius + (i % 2 == 0 ? 0.15f : -0.10f);
                float px = Mathf.Cos(angle) * r;
                float pz = Mathf.Sin(angle) * r;

                if (i % 3 == 0)
                {
                    var pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pebble.name = "Pebble_" + i;
                    pebble.transform.SetParent(decorContainer.transform, false);
                    pebble.transform.localPosition = new Vector3(px, 0.03f, pz);
                    pebble.transform.localScale = new Vector3(0.22f, 0.08f, 0.18f);
                    pebble.GetComponent<Renderer>().sharedMaterial = stoneMat;
                    UnityEngine.Object.DestroyImmediate(pebble.GetComponent<Collider>());
                }
                else
                {
                    var flower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    flower.name = "Flower_" + i;
                    flower.transform.SetParent(decorContainer.transform, false);
                    flower.transform.localPosition = new Vector3(px, 0.02f, pz);
                    flower.transform.localScale = new Vector3(0.16f, 0.04f, 0.16f);
                    Material fMat = (i % 3 == 1) ? flowerPinkMat : (i % 2 == 0 ? flowerYellowMat : flowerBlueMat);
                    flower.GetComponent<Renderer>().sharedMaterial = fMat;
                    UnityEngine.Object.DestroyImmediate(flower.GetComponent<Collider>());
                }
            }

            // 6. Stand Structure
            int standLayer = LayerMask.NameToLayer("Stand");
            if (standLayer == -1) standLayer = 8;

            var pmStand = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PM_Stand.physicsMaterial");
            var matStandPurple = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_StandPurple.mat");
            var matGold = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_MortarGold.mat");
            var matBase = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/M_DaisPurple.mat");

            var stand = GameObject.Find("Stand");
            if (stand != null)
            {
                Transform primaryPlatform = stand.transform.Find("PrimaryPlatform");
                if (primaryPlatform == null)
                {
                    var pGo = new GameObject("PrimaryPlatform");
                    pGo.transform.SetParent(stand.transform, false);
                    primaryPlatform = pGo.transform;
                }

                // Tabletop
                Transform tabletop = primaryPlatform.Find("Tabletop");
                if (tabletop == null)
                {
                    var ttGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ttGo.name = "Tabletop";
                    ttGo.transform.SetParent(primaryPlatform, false);
                    tabletop = ttGo.transform;
                }
                tabletop.localPosition = new Vector3(0f, 1.35f, 0f);
                tabletop.localScale = new Vector3(5.5f, 0.20f, 1.4f);
                tabletop.gameObject.layer = standLayer;
                tabletop.GetComponent<Renderer>().sharedMaterial = matStandPurple;
                var tableCol = tabletop.GetComponent<BoxCollider>();
                if (tableCol != null && pmStand != null) tableCol.sharedMaterial = pmStand;

                // Gold Rim
                Transform goldRim = primaryPlatform.Find("Tabletop_GoldRim");
                if (goldRim == null)
                {
                    var grGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    grGo.name = "Tabletop_GoldRim";
                    grGo.transform.SetParent(primaryPlatform, false);
                    goldRim = grGo.transform;
                }
                goldRim.localPosition = new Vector3(0f, 1.23f, 0f);
                goldRim.localScale = new Vector3(5.54f, 0.04f, 1.44f);
                goldRim.gameObject.layer = standLayer;
                goldRim.GetComponent<Renderer>().sharedMaterial = matGold;
                var rimCol = goldRim.GetComponent<Collider>();
                if (rimCol != null) UnityEngine.Object.DestroyImmediate(rimCol);

                // Pillar
                Transform pillar = primaryPlatform.Find("CenterPillar");
                if (pillar == null)
                {
                    var pilGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pilGo.name = "CenterPillar";
                    pilGo.transform.SetParent(primaryPlatform, false);
                    pillar = pilGo.transform;
                }
                pillar.localPosition = new Vector3(0f, 0.73f, 0f);
                pillar.localScale = new Vector3(0.50f, 0.49f, 0.50f);
                pillar.gameObject.layer = standLayer;
                pillar.GetComponent<Renderer>().sharedMaterial = matGold;

                // Pillar collar
                Transform pCollar = primaryPlatform.Find("PillarCollar");
                if (pCollar == null)
                {
                    var colGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    colGo.name = "PillarCollar";
                    colGo.transform.SetParent(primaryPlatform, false);
                    pCollar = colGo.transform;
                }
                pCollar.localPosition = new Vector3(0f, 1.21f, 0f);
                pCollar.localScale = new Vector3(0.85f, 0.04f, 0.85f);
                pCollar.gameObject.layer = standLayer;
                pCollar.GetComponent<Renderer>().sharedMaterial = matStandPurple;
                var pCollarCol = pCollar.GetComponent<Collider>();
                if (pCollarCol != null) UnityEngine.Object.DestroyImmediate(pCollarCol);

                // Pedestal Base
                Transform baseLower = primaryPlatform.Find("BaseRing_Lower");
                if (baseLower == null)
                {
                    var blGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    blGo.name = "BaseRing_Lower";
                    blGo.transform.SetParent(primaryPlatform, false);
                    baseLower = blGo.transform;
                }
                baseLower.localPosition = new Vector3(0f, 0.05f, 0f);
                baseLower.localScale = new Vector3(2.4f, 0.10f, 2.4f);
                baseLower.gameObject.layer = standLayer;
                baseLower.GetComponent<Renderer>().sharedMaterial = matGold;

                Transform baseMid = primaryPlatform.Find("Base_MidPurple");
                if (baseMid == null)
                {
                    var bmGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    bmGo.name = "Base_MidPurple";
                    bmGo.transform.SetParent(primaryPlatform, false);
                    baseMid = bmGo.transform;
                }
                baseMid.localPosition = new Vector3(0f, 0.14f, 0f);
                baseMid.localScale = new Vector3(1.9f, 0.10f, 1.9f);
                baseMid.gameObject.layer = standLayer;
                baseMid.GetComponent<Renderer>().sharedMaterial = matBase;

                Transform baseUpper = primaryPlatform.Find("BaseRing_Upper");
                if (baseUpper == null)
                {
                    var buGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    buGo.name = "BaseRing_Upper";
                    buGo.transform.SetParent(primaryPlatform, false);
                    baseUpper = buGo.transform;
                }
                baseUpper.localPosition = new Vector3(0f, 0.21f, 0f);
                baseUpper.localScale = new Vector3(1.4f, 0.06f, 1.4f);
                baseUpper.gameObject.layer = standLayer;
                baseUpper.GetComponent<Renderer>().sharedMaterial = matGold;
            }

            // 4. Paint Bottle Grid (6 columns x 5 rows = 30 bottles)
            var gridContainer = GameObject.Find("PaintBottleGrid_6x5");
            if (gridContainer != null) UnityEngine.Object.DestroyImmediate(gridContainer);

            // Also clean up any loose placeholder objects
            var looseObjects = GameObject.FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None);
            foreach (var lo in looseObjects)
            {
                if (lo.name.StartsWith("HUD") || lo.name.StartsWith("Ball") || lo.name.StartsWith("Mortar")) continue;
                UnityEngine.Object.DestroyImmediate(lo.gameObject);
            }

            gridContainer = new GameObject("PaintBottleGrid_6x5");
            int objectLayer = LayerMask.NameToLayer("Object");
            if (objectLayer == -1) objectLayer = 7;

            var pmBottle = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/_Project/Materials/PM_PaintBottle.physicsMaterial");
            var config = AssetDatabase.LoadAssetAtPath<ObjectConfig>("Assets/_Project/ScriptableObjects/Objects/ObjectConfig_PaintBottle.asset");

            Material[] bodyMats = new Material[6];
            Material[] capMats = new Material[6];
            for (int i = 0; i < 6; i++)
            {
                bodyMats[i] = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Bottles/M_Bottle_" + ColorNames[i] + ".mat");
                capMats[i] = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Bottles/M_Cap_" + ColorNames[i] + ".mat");
            }

            float startY = 1.450f;
            float rowHeight = 0.760f;
            float zPos = 7.50f;

            for (int col = 0; col < 6; col++)
            {
                float x = ColXPositions[col];
                Color paintCol = BottleColors[col];
                Material bMat = bodyMats[col];
                Material cMat = capMats[col];

                for (int row = 0; row < 5; row++)
                {
                    float y = startY + row * rowHeight;

                    GameObject bottleGo = new GameObject(string.Format("Bottle_{0}_R{1}", ColorNames[col], row));
                    bottleGo.layer = objectLayer;
                    bottleGo.transform.SetParent(gridContainer.transform, false);
                    bottleGo.transform.position = new Vector3(x, y, zPos);

                    var rb = bottleGo.AddComponent<Rigidbody>();
                    rb.mass = 1.2f;
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 1.0f;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;

                    var boxCol = bottleGo.AddComponent<BoxCollider>();
                    boxCol.size = new Vector3(0.72f, 0.76f, 0.72f);
                    boxCol.center = new Vector3(0f, 0.38f, 0f);
                    boxCol.sharedMaterial = pmBottle;

                    var bottleObj = bottleGo.AddComponent<PaintBottleObject>();
                    var so = new SerializedObject(bottleObj);
                    so.FindProperty("_paintColor").colorValue = paintCol;
                    so.FindProperty("_config").objectReferenceValue = config;
                    so.ApplyModifiedProperties();

                    // Visual components
                    var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    body.name = "Body";
                    body.transform.SetParent(bottleGo.transform, false);
                    body.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                    body.transform.localScale = new Vector3(0.68f, 0.28f, 0.68f);
                    body.layer = objectLayer;
                    body.GetComponent<Renderer>().sharedMaterial = bMat;
                    UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

                    var bCollar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    bCollar.name = "StripedCollar";
                    bCollar.transform.SetParent(bottleGo.transform, false);
                    bCollar.transform.localPosition = new Vector3(0f, 0.60f, 0f);
                    bCollar.transform.localScale = new Vector3(0.70f, 0.05f, 0.70f);
                    bCollar.layer = objectLayer;
                    bCollar.GetComponent<Renderer>().sharedMaterial = cMat;
                    UnityEngine.Object.DestroyImmediate(bCollar.GetComponent<Collider>());

                    var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cap.name = "CapLid";
                    cap.transform.SetParent(bottleGo.transform, false);
                    cap.transform.localPosition = new Vector3(0f, 0.68f, 0f);
                    cap.transform.localScale = new Vector3(0.64f, 0.04f, 0.64f);
                    cap.layer = objectLayer;
                    cap.GetComponent<Renderer>().sharedMaterial = bMat;
                    UnityEngine.Object.DestroyImmediate(cap.GetComponent<Collider>());
                }
            }
        }
    }
}
