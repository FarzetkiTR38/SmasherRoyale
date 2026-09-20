using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    /// <summary>Rebuildable Sky Citadel art template scoped to MainGame only.</summary>
    public static class SkyCitadelSceneBuilder
    {
        private const string ROOT = "Assets/_Project/Art/SkyCitadel";
        private static Material _stone, _gold, _blue, _dark, _energy, _wood;

        [MenuItem("Royal Smash/Sky Citadel/Build MainGame")]
        public static void BuildAll()
        {
            GuardScene();
            BuildEnvironment();
            BuildGameplay();
            BuildMortar();
            SkyCitadelHUDBuilder.Apply();
            Save();
        }

        private static void GuardScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before building Sky Citadel.");
            string backup = "_FarzetkiDocs/Backups/MainGame-BeforeSkyCitadel.unity";
            Directory.CreateDirectory("_FarzetkiDocs/Backups");
            if (!File.Exists(backup)) EditorSceneManager.SaveScene(scene, backup, true);
        }

        public static void BuildEnvironment()
        {
            GuardScene();
            Materials();
            foreach (string name in new[] { "KingdomBackdrop", "DirtPatch", "GroundDecorations", "PaintBottleGrid_6x5", "Stand" })
            {
                var legacy = GameObject.Find(name);
                if (legacy != null) legacy.SetActive(false);
            }
            RemoveOwned("SkyCitadelEnvironment");
            var root = new GameObject("SkyCitadelEnvironment");
            var cam = Camera.main;
            cam.transform.SetPositionAndRotation(new Vector3(0, 3.11f, -5.43f), Quaternion.Euler(8.16f, 0, 0));
            cam.fieldOfView = 56;
            cam.nearClipPlane = .08f;
            cam.farClipPlane = 110;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.43f, .72f, .98f);
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            cam.allowHDR = true;

            // The backdrop contains no targets/cannon. Only its upper architecture is sampled.
            var backdropMat = Mat("Backdrop", "Universal Render Pipeline/Unlit", Color.white);
            backdropMat.mainTexture = Texture("T_CitadelBackdrop.png", false);
            backdropMat.mainTextureScale = new Vector2(1, .515f);
            backdropMat.mainTextureOffset = new Vector2(0, .485f);
            var backdrop = Primitive(root.transform, "CastleAndWaterfalls", PrimitiveType.Quad, Vector3.zero, Vector3.one, backdropMat);
            const float distance = 60;
            float height = 2 * distance * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * .5f);
            backdrop.transform.position = cam.ViewportToWorldPoint(new Vector3(.5f, .744f, distance));
            backdrop.transform.rotation = cam.transform.rotation;
            backdrop.transform.localScale = new Vector3(height * 9f / 16f * 1.06f, height * .535f, 1);
            backdrop.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            backdrop.GetComponent<Renderer>().receiveShadows = false;

            var ground = GameObject.Find("Ground");
            ground.transform.position = new Vector3(0, 0, .45f);
            ground.transform.localScale = new Vector3(3.2f, 1, 2.61f);
            ground.layer = 9;
            var marble = Mat("MarbleFloor", "Universal Render Pipeline/Lit", Color.white, .03f, .27f);
            marble.mainTexture = Texture("T_CitadelMarble.png", true);
            marble.mainTextureScale = new Vector2(7.5f, 6.1f);
            ground.GetComponent<Renderer>().sharedMaterial = marble;

            // A slim real balustrade ties the painted skyline to the physical courtyard.
            Primitive(root.transform, "HorizonPlinth", PrimitiveType.Cube, new Vector3(0,.1f,13.36f), new Vector3(28,.2f,.28f), _stone);
            Primitive(root.transform, "HorizonGoldCap", PrimitiveType.Cube, new Vector3(0,.215f,13.36f), new Vector3(28,.045f,.34f), _gold);
            for (int side = -1; side <= 1; side += 2)
            {
                var p = new Vector3(side * 4.2f, 0, 11.8f);
                Primitive(root.transform, "BalustradeFoot", PrimitiveType.Cube, p + Vector3.up*.14f, new Vector3(.9f,.28f,.9f), _stone);
                Primitive(root.transform, "BalustradePillar", PrimitiveType.Cube, p + Vector3.up*.65f, new Vector3(.62f,.8f,.62f), _stone);
                Primitive(root.transform, "PillarBlueInset", PrimitiveType.Cube, p + new Vector3(0,.65f,-.321f), new Vector3(.34f,.57f,.025f), _blue);
                Primitive(root.transform, "PillarCornice", PrimitiveType.Cube, p + Vector3.up*1.12f, new Vector3(.88f,.14f,.88f), _gold);
            }

            var sun = GameObject.Find("Directional Light").GetComponent<Light>();
            sun.transform.rotation = Quaternion.Euler(48,-32,0);
            sun.color = new Color(1f,.94f,.84f); sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft; sun.shadowStrength = .55f; sun.shadowBias = .035f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.64f,.77f,1f);
            RenderSettings.ambientEquatorColor = new Color(.61f,.63f,.65f);
            RenderSettings.ambientGroundColor = new Color(.36f,.32f,.25f);
            RenderSettings.fog = false;

            string profilePath = ROOT + "/CitadelVolume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile,profilePath); }
            Bloom bloom;
            if (!profile.TryGet(out bloom)) { bloom = profile.Add<Bloom>(); AssetDatabase.AddObjectToAsset(bloom, profile); }
            bloom.intensity.Override(.22f); bloom.threshold.Override(1.1f); bloom.scatter.Override(.55f);
            var volume = root.AddComponent<Volume>(); volume.isGlobal = true; volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);
            Save();
        }

        public static void BuildGameplay()
        {
            GuardScene(); Materials();
            Directory.CreateDirectory(ROOT + "/Prefabs");
            Directory.CreateDirectory(ROOT + "/Levels");
            var variants = new[] { "wood", "stone", "crystal", "armored", "core" };
            var prefabs = new GameObject[variants.Length];
            for (int i=0;i<variants.Length;i++)
            {
                prefabs[i] = SkyCitadelBlockFactory.CreatePrefab(ROOT+"/Prefabs/PF_"+variants[i]+".prefab", variants[i], _stone,_gold,_blue,_dark,_energy,_wood);
                var configPath = ROOT+"/Levels/Config_"+variants[i]+".asset";
                var config = AssetDatabase.LoadAssetAtPath<ObjectConfig>(configPath);
                if (config == null) { config=ScriptableObject.CreateInstance<ObjectConfig>(); AssetDatabase.CreateAsset(config,configPath); }
                var so = new SerializedObject(config);
                so.FindProperty("_objectType").enumValueIndex = i;
                so.FindProperty("_displayName").stringValue = "Citadel "+variants[i];
                so.FindProperty("_mass").floatValue = i==1 ? 1.3f : 1f;
                so.ApplyModifiedPropertiesWithoutUndo();
                var prefabRoot = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefabs[i]));
                SetObject(prefabRoot.GetComponent<PhysicsObject>(),"_config",config);
                var rb = prefabRoot.GetComponent<Rigidbody>(); rb.solverIterations=10; rb.solverVelocityIterations=4;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot,AssetDatabase.GetAssetPath(prefabs[i]));
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
            var stand = CreateStandPrefab();
            var loader = UnityEngine.Object.FindFirstObjectByType<LevelLoader>();
            loader.BindPrefabs(stand,prefabs[0],prefabs[1],prefabs[2],prefabs[3],prefabs[4]);
            EditorUtility.SetDirty(loader);

            // The original 30-level database remains intact; this scene uses a local art-template variant.
            string dataPath = ROOT+"/Levels/LevelData_Citadel01.asset";
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(dataPath);
            if (data==null) { data=ScriptableObject.CreateInstance<LevelData>(); AssetDatabase.CreateAsset(data,dataPath); }
            var placements = new List<ObjectPlacement>();
            // Wide foundation, twin shoulders and a central tower. Every block has support.
            int[] widths = { 7, 7, 5, 3, 3, 1 };
            for(int row=0;row<widths.Length;row++)
            for(int col=0;col<widths[row];col++)
            {
                int type = (row+col)%5;
                if(row==5 || (row==2&&col==2)) type=4;
                placements.Add(new ObjectPlacement { Type=(ObjectType)type,
                    Position=new Vector3((col-(widths[row]-1)*.5f)*.745f,1.35f+.125f+.37f+row*.746f,7.5f),
                    Scale=Vector3.one*.74f,Rotation=Vector3.zero });
            }
            data.SetData(1,"Sky Citadel",25,50,new[]{new StandPlacement {Position=new Vector3(0,1.35f,7.5f),Scale=new Vector3(5.7f,.25f,1.6f)}},placements.ToArray(),Array.Empty<LevelEvent>());
            EditorUtility.SetDirty(data);
            string databasePath=ROOT+"/Levels/CitadelLevelDatabase.asset";
            var database=AssetDatabase.LoadAssetAtPath<LevelDatabase>(databasePath);
            if(database==null) { database=ScriptableObject.CreateInstance<LevelDatabase>(); AssetDatabase.CreateAsset(database,databasePath); }
            var original=AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
            var levels = new LevelData[original.TotalLevels];
            for(int i=0;i<levels.Length;i++) levels[i]= i==0?data:original.GetLevel(i+1);
            database.SetLevels(levels); EditorUtility.SetDirty(database);
            var manager=UnityEngine.Object.FindFirstObjectByType<LevelManager>();
            SetObject(manager,"_levelDatabase",database); SetObject(manager,"_levelLoader",loader);

            RemoveOwned("SkyCitadelPreview");
            var preview=new GameObject("SkyCitadelPreview"); preview.AddComponent<SkyCitadelPreview>();
            foreach(var s in data.Stands) Place(stand,preview.transform,s.Position,s.Scale,s.Rotation);
            foreach(var p in data.Objects) Place(prefabs[(int)p.Type],preview.transform,p.Position,p.Scale,p.Rotation);
            Save();
        }

        private static GameObject CreateStandPrefab()
        {
            var stand=new GameObject("CitadelStand"); stand.layer=8;
            var slab=Primitive(stand.transform,"MarbleTabletop",PrimitiveType.Cube,Vector3.zero,Vector3.one,_stone);
            slab.AddComponent<BoxCollider>(); slab.layer=8;
            Primitive(stand.transform,"GoldCornice",PrimitiveType.Cube,new Vector3(0,-.43f,0),new Vector3(1.015f,.15f,1.03f),_gold);
            Primitive(stand.transform,"SapphireFrieze",PrimitiveType.Cube,new Vector3(0,-.64f,0),new Vector3(.98f,.28f,.96f),_blue);
            Primitive(stand.transform,"LowerGoldEdge",PrimitiveType.Cube,new Vector3(0,-.83f,0),new Vector3(1.01f,.1f,1),_gold);
            for(int x=-1;x<=1;x+=2)
            {
                Primitive(stand.transform,"IvoryPedestal",PrimitiveType.Cylinder,new Vector3(x*.25f,-2.8f,0),new Vector3(.10f,2.1f,.38f),_stone);
                Primitive(stand.transform,"PedestalCapital",PrimitiveType.Cylinder,new Vector3(x*.25f,-.95f,0),new Vector3(.16f,.3f,.58f),_gold);
                Primitive(stand.transform,"PedestalFoot",PrimitiveType.Cylinder,new Vector3(x*.25f,-4.9f,0),new Vector3(.2f,.5f,.72f),_gold);
                Primitive(stand.transform,"PedestalInset",PrimitiveType.Cube,new Vector3(x*.25f,-2.85f,-.191f),new Vector3(.035f,2.4f,.025f),_blue);
            }
            for(int x=-3;x<=3;x++)
            {
                var gem=Primitive(stand.transform,"FriezeDiamond",PrimitiveType.Cube,new Vector3(x*.13f,-.64f,-.5f),new Vector3(.025f,.34f,.025f),_gold);
                // Scale-aware front ornament avoids stretching the world diamond into a line.
                gem.transform.localRotation=Quaternion.Euler(0,0,12);
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(stand,ROOT+"/Prefabs/PF_CitadelStand.prefab");
            UnityEngine.Object.DestroyImmediate(stand); return prefab;
        }

        public static void BuildMortar()
        {
            GuardScene(); Materials();
            var mortar=GameObject.Find("Mortar");
            foreach(var r in mortar.GetComponentsInChildren<Renderer>(true))
            {
                string n=r.name.ToLowerInvariant();
                r.sharedMaterial=n.Contains("gold")||n.Contains("stud")||n.Contains("crown")||n.Contains("bearing")?_gold:
                    n.Contains("red")?_energy:n.Contains("lavender")?_stone:n.Contains("dark")||n.Contains("bore")?_dark:_blue;
            }
            var dais=mortar.transform.Find("DaisBase"); if(dais!=null) dais.gameObject.SetActive(false);
            var visuals=mortar.transform.Find("Visuals"); visuals.localScale=Vector3.one*1.28f;
            var old=mortar.transform.Find("CitadelMount"); if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var mount=new GameObject("CitadelMount"); mount.transform.SetParent(mortar.transform,false);
            Primitive(mount.transform,"BaseGold",PrimitiveType.Cylinder,new Vector3(0,-.9f,-.1f),new Vector3(1.45f,.10f,1.45f),_gold);
            Primitive(mount.transform,"Pedestal",PrimitiveType.Cylinder,new Vector3(0,-.48f,-.1f),new Vector3(.9f,.35f,.9f),_stone);
            Primitive(mount.transform,"SwivelGold",PrimitiveType.Cylinder,new Vector3(0,-.1f,-.1f),new Vector3(1.16f,.07f,1.16f),_gold);
            var pivot=mortar.transform.Find("Visuals/BarrelPivot");
            var oldBands=pivot.Find("CitadelBands"); if(oldBands!=null) UnityEngine.Object.DestroyImmediate(oldBands.gameObject);
            var bands=new GameObject("CitadelBands"); bands.transform.SetParent(pivot,false);
            for(int i=0;i<3;i++)
            {
                var rail=Primitive(bands.transform,"GoldEnergyBrace",PrimitiveType.Cube,new Vector3((i-1)*.12f,.195f,-.03f),new Vector3(.025f,.026f,.32f),_gold);
                rail.transform.localRotation=Quaternion.Euler(0,(i-1)*-12,0);
            }
            // Clone the existing gameplay projectile, preserving configuration and collision behavior.
            var mc=mortar.GetComponent<MortarController>(); var ms=new SerializedObject(mc);
            var source=(GameObject)ms.FindProperty("_ballPrefab").objectReferenceValue;
            var ball=UnityEngine.Object.Instantiate(source); ball.name="CitadelEnergyBall";
            foreach(var r in ball.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial=_energy;
            for(int i=0;i<3;i++)
            {
                var ring=new GameObject("GoldOrbit"); ring.transform.SetParent(ball.transform,false);
                var mf=ring.AddComponent<MeshFilter>(); mf.sharedMesh=RingMesh();
                ring.AddComponent<MeshRenderer>().sharedMaterial=_gold;
                ring.transform.localRotation=Quaternion.Euler(i==0?0:90,i==2?90:0,0);
            }
            var ballPrefab=PrefabUtility.SaveAsPrefabAsset(ball,ROOT+"/Prefabs/PF_CitadelBall.prefab");
            UnityEngine.Object.DestroyImmediate(ball); SetObject(mc,"_ballPrefab",ballPrefab);
            Save();
        }

        private static Mesh RingMesh()
        {
            string path=ROOT+"/Meshes/BallOrbit.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path); if(mesh!=null)return mesh;
            Directory.CreateDirectory(ROOT+"/Meshes");
            var v=new List<Vector3>(); var t=new List<int>();
            for(int i=0;i<=48;i++)for(int j=0;j<=6;j++)
            {
                float a=i*Mathf.PI*2/48,b=j*Mathf.PI*2/6;
                float r=.49f+.025f*Mathf.Cos(b); v.Add(new Vector3(r*Mathf.Cos(a),.025f*Mathf.Sin(b),r*Mathf.Sin(a)));
                if(i<48&&j<6){int n=i*7+j;t.AddRange(new[]{n,n+7,n+1,n+1,n+7,n+8});}
            }
            mesh=new Mesh {name="GoldOrbitalBand"}; mesh.SetVertices(v); mesh.SetTriangles(t,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }

        private static void Materials()
        {
            Directory.CreateDirectory(ROOT+"/Materials"); Directory.CreateDirectory(ROOT+"/Prefabs");
            _stone=Mat("IvoryStone","Universal Render Pipeline/Lit",new Color(.93f,.84f,.66f),.02f,.3f);
            _stone.mainTexture=Texture("T_CitadelMarble.png",true); _stone.mainTextureScale=Vector2.one*.17f; _stone.mainTextureOffset=new Vector2(.2f,.24f);
            _gold=Mat("AntiqueGold","Universal Render Pipeline/Lit",new Color(1,.63f,.17f),.48f,.64f);
            _blue=Mat("RoyalSapphire","Universal Render Pipeline/Lit",new Color(.035f,.15f,.48f),.25f,.6f);
            _dark=Mat("ObsidianSteel","Universal Render Pipeline/Lit",new Color(.095f,.14f,.21f),.35f,.46f);
            _energy=Mat("ArcaneEnergy","RoyalSmash/Citadel Energy",new Color(.015f,.22f,.95f));
            _wood=Mat("WarmWood","Universal Render Pipeline/Lit",new Color(.55f,.25f,.08f),0,.22f);
        }

        private static Material Mat(string name,string shader,Color color,float metallic=0,float smoothness=.4f)
        {
            var path=ROOT+"/Materials/M_"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            var sh=Shader.Find(shader);if(sh==null)throw new InvalidOperationException("Missing shader "+shader);
            if(mat==null){mat=new Material(sh);AssetDatabase.CreateAsset(mat,path);}else mat.shader=sh;
            mat.SetColor("_BaseColor",color);
            if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",metallic);
            if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",smoothness);
            EditorUtility.SetDirty(mat);return mat;
        }

        private static Texture2D Texture(string name,bool repeat)
        {
            string path=ROOT+"/Textures/"+name;
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer!=null){var wrap=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
                if(importer.wrapMode!=wrap||importer.maxTextureSize!=2048){importer.wrapMode=wrap;importer.maxTextureSize=2048;importer.mipmapEnabled=repeat;importer.SaveAndReimport();}}
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static GameObject Primitive(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);
            g.transform.localPosition=pos;g.transform.localScale=scale;
            var collider=g.GetComponent<Collider>();if(collider!=null)UnityEngine.Object.DestroyImmediate(collider);
            g.GetComponent<Renderer>().sharedMaterial=material;return g;
        }

        private static void Place(GameObject prefab,Transform parent,Vector3 position,Vector3 scale,Vector3 rotation)
        {
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);g.transform.position=position;
            g.transform.localScale=scale;g.transform.rotation=Quaternion.Euler(rotation);
        }

        private static void SetObject(UnityEngine.Object target,string property,UnityEngine.Object value)
        {
            var so=new SerializedObject(target);so.FindProperty(property).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);
        }

        private static void RemoveOwned(string name)
        {
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if(root.name==name)UnityEngine.Object.DestroyImmediate(root);
        }

        public static void Save()
        {
            AssetDatabase.SaveAssets();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}
