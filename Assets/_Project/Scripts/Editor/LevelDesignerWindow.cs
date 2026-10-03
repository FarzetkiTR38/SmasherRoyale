using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Levels;

namespace RoyalSmash.Editor
{
    public enum PlacementToolMode
    {
        Place,
        Erase,
        Select
    }

    /// <summary>
    /// Interactive In-Editor Level Designer Tool for Royal Smash.
    /// Supports:
    /// - Selecting and loading any of the 30 levels live into the SceneView.
    /// - Dynamic Stand scaling, positioning, and multi-stand setups (dual platforms, tiered stands).
    /// - Minecraft-style block placement with face snapping, grid snapping (0.755m / 0.745m), and half-step staggering.
    /// - Palette selection for all 5 Citadel block types (Wood, Stone, Armored, Crystal, Core).
    /// - Live SceneView phantom preview, click-to-place, shift-click-to-erase.
    /// - Symmetry mirror tool (Left <-> Right) for fast fortress construction.
    /// - Direct 1-click serialization to LevelData ScriptableObjects.
    /// </summary>
    public class LevelDesignerWindow : EditorWindow
    {
        private const string ROOT_NAME = "[LevelDesigner_WorkingRoot]";
        private const string CITADEL_DB_PATH = "Assets/_Project/Art/SkyCitadel/Levels/CitadelLevelDatabase.asset";
        private const string LEVELS_FOLDER = "Assets/_Project/Art/SkyCitadel/Levels";

        private const float BASE_STAND_TOP_Y = 1.475f;
        private const float BLOCK_SIZE = 0.78f;
        private const float STEP_X = 0.78f;
        private const float STEP_Y = 0.78f;
        private const float STEP_Z = 0.78f;
        private const float STAND_Z = 9.00f;

        // Editor State
        [SerializeField] private int _selectedLevelId = 1;
        [SerializeField] private LevelDatabase _database;
        [SerializeField] private LevelData _currentLevelData;

        // Tool Mode
        [SerializeField] private PlacementToolMode _toolMode = PlacementToolMode.Place;
        [SerializeField] private ObjectType _selectedBlockType = ObjectType.Box; // Box=Wood, Stone=Stone, Cone=Armored, Column=Crystal, PaintBottle=Core
        [SerializeField] private bool _enableGridSnapping = true;
        [SerializeField] private bool _enableHalfStep = false;
        [SerializeField] private bool _drawGridGuides = true;

        // Multi-Stand Configuration
        [SerializeField] private List<StandPlacement> _standsWorkingList = new List<StandPlacement>();

        // Prefab Cache
        private GameObject _woodPrefab;
        private GameObject _stonePrefab;
        private GameObject _armoredPrefab;
        private GameObject _crystalPrefab;
        private GameObject _corePrefab;
        private GameObject _standPrefab;

        // Scene Interaction
        private Vector3 _ghostPosition = Vector3.zero;
        private bool _ghostValid = false;
        private Vector2 _scrollPos;

        [MenuItem("Tools/Royal Smash/Level Designer", false, 10)]
        [MenuItem("Window/Royal Smash/Level Designer", false, 10)]
        public static void OpenWindow()
        {
            var window = GetWindow<LevelDesignerWindow>("Level Designer");
            window.minSize = new Vector2(380, 650);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPrefabs();
            LoadDatabase();
            SceneView.duringSceneGui += OnSceneGUI;
            LoadLevelToWorkingRoot(_selectedLevelId);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void LoadPrefabs()
        {
            _woodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_wood.prefab");
            _stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_stone.prefab");
            _armoredPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_armored.prefab");
            _crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_crystal.prefab");
            _corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_core.prefab");
            _standPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/SkyCitadel/Prefabs/PF_CitadelStand.prefab");
        }

        private void LoadDatabase()
        {
            if (_database == null)
            {
                _database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(CITADEL_DB_PATH);
                if (_database == null)
                {
                    _database = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
                }
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            EditorGUILayout.Space(6);

            DrawLevelSelector();
            EditorGUILayout.Space(6);

            DrawStandConfiguration();
            EditorGUILayout.Space(6);

            DrawBlockPalette();
            EditorGUILayout.Space(6);

            DrawToolsAndActions();
            EditorGUILayout.Space(6);

            DrawLevelStatistics();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.7f, 1.0f) }
            };
            EditorGUILayout.LabelField("🏰 ROYAL SMASH LEVEL DESIGNER", titleStyle);
            EditorGUILayout.LabelField("Visual Multi-Stand & Minecraft-Style Block Builder", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("⚙ Open Game Designer (Physics & Balance)", GUILayout.Height(24)))
            {
                GameDesignerWindow.OpenWindow();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLevelSelector()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("LEVEL SELECTION", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Prev", GUILayout.Width(65)))
            {
                if (_selectedLevelId > 1)
                {
                    _selectedLevelId--;
                    LoadLevelToWorkingRoot(_selectedLevelId);
                }
            }

            int prevLevel = _selectedLevelId;
            _selectedLevelId = EditorGUILayout.IntSlider(_selectedLevelId, 1, 30);
            if (_selectedLevelId != prevLevel)
            {
                LoadLevelToWorkingRoot(_selectedLevelId);
            }

            if (GUILayout.Button("Next ▶", GUILayout.Width(65)))
            {
                if (_selectedLevelId < 30)
                {
                    _selectedLevelId++;
                    LoadLevelToWorkingRoot(_selectedLevelId);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_currentLevelData != null)
            {
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField("Level Name", _currentLevelData.LevelName);
                int balls = EditorGUILayout.IntSlider("Ball Count", _currentLevelData.BallCount, 5, 50);
                int reward = EditorGUILayout.IntField("Reward Gold", _currentLevelData.BaseRewardGold);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_currentLevelData, "Edit Level Parameters");
                    SetPrivateField(_currentLevelData, "_levelName", newName);
                    SetPrivateField(_currentLevelData, "_ballCount", balls);
                    SetPrivateField(_currentLevelData, "_baseRewardGold", reward);
                    EditorUtility.SetDirty(_currentLevelData);
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("💾 SAVE LEVEL TO DATA", GUILayout.Height(30)))
            {
                SaveWorkingRootToLevelData();
            }
            GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f);
            if (GUILayout.Button("🔄 RELOAD LEVEL", GUILayout.Height(30)))
            {
                LoadLevelToWorkingRoot(_selectedLevelId);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStandConfiguration()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("STAND CONFIGURATION", EditorStyles.boldLabel);

            // Stand Presets
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Standard (1x)"))
            {
                ApplyStandPreset(new[] { new StandPlacement { Position = new Vector3(0, 0, STAND_Z), Scale = Vector3.one } });
            }
            if (GUILayout.Button("Wide (1.4x)"))
            {
                ApplyStandPreset(new[] { new StandPlacement { Position = new Vector3(0, 0, STAND_Z), Scale = new Vector3(1.4f, 1f, 1f) } });
            }
            if (GUILayout.Button("Dual Platforms"))
            {
                ApplyStandPreset(new[]
                {
                    new StandPlacement { Position = new Vector3(-2.2f, 0, STAND_Z), Scale = new Vector3(0.75f, 1f, 1f) },
                    new StandPlacement { Position = new Vector3( 2.2f, 0, STAND_Z), Scale = new Vector3(0.75f, 1f, 1f) }
                });
            }
            if (GUILayout.Button("Tiered (F/B)"))
            {
                ApplyStandPreset(new[]
                {
                    new StandPlacement { Position = new Vector3(0, 0, 6.8f), Scale = new Vector3(1f, 0.85f, 1f) },
                    new StandPlacement { Position = new Vector3(0, 0.9f, 8.2f), Scale = new Vector3(1f, 1f, 1f) }
                });
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Active Stands: {_standsWorkingList.Count}", EditorStyles.boldLabel);

            for (int i = 0; i < _standsWorkingList.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Stand #{i + 1}", EditorStyles.boldLabel);
                if (_standsWorkingList.Count > 1 && GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _standsWorkingList.RemoveAt(i);
                    RefreshStandsInScene();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                var s = _standsWorkingList[i];
                s.Position = EditorGUILayout.Vector3Field("Position", s.Position);
                s.Scale = EditorGUILayout.Vector3Field("Scale", s.Scale);
                s.Rotation = EditorGUILayout.Vector3Field("Rotation", s.Rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshStandsInScene();
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Stand Platform"))
            {
                _standsWorkingList.Add(new StandPlacement
                {
                    Position = new Vector3(0, 0, STAND_Z),
                    Scale = Vector3.one,
                    Rotation = Vector3.zero
                });
                RefreshStandsInScene();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBlockPalette()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("BLOCK PALETTE & BRUSH", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawBlockButton(ObjectType.Box, "Wood", new Color(0.9f, 0.6f, 0.2f), "1");
            DrawBlockButton(ObjectType.Stone, "Stone", new Color(0.85f, 0.85f, 0.85f), "2");
            DrawBlockButton(ObjectType.Cone, "Armored", new Color(0.7f, 0.7f, 0.5f), "3");
            DrawBlockButton(ObjectType.Column, "Crystal", new Color(0.2f, 0.6f, 1.0f), "4");
            DrawBlockButton(ObjectType.PaintBottle, "Core", new Color(0.9f, 0.3f, 0.9f), "5");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = _toolMode == PlacementToolMode.Place ? Color.cyan : Color.white;
            if (GUILayout.Button("🖌 PLACE (Stamp)", GUILayout.Height(26))) _toolMode = PlacementToolMode.Place;

            GUI.backgroundColor = _toolMode == PlacementToolMode.Erase ? Color.red : Color.white;
            if (GUILayout.Button("🧹 ERASE (Del)", GUILayout.Height(26))) _toolMode = PlacementToolMode.Erase;

            GUI.backgroundColor = _toolMode == PlacementToolMode.Select ? Color.yellow : Color.white;
            if (GUILayout.Button("🔍 SELECT", GUILayout.Height(26))) _toolMode = PlacementToolMode.Select;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            _enableGridSnapping = EditorGUILayout.ToggleLeft("Grid Snap (0.755m)", _enableGridSnapping, GUILayout.Width(140));
            _enableHalfStep = EditorGUILayout.ToggleLeft("Half-Step (0.378m) [G]", _enableHalfStep, GUILayout.Width(150));
            _drawGridGuides = EditorGUILayout.ToggleLeft("Draw Guides", _drawGridGuides);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawBlockButton(ObjectType type, string label, Color color, string hotkey)
        {
            bool isSelected = _selectedBlockType == type;
            GUI.backgroundColor = isSelected ? color : color * 0.6f;
            if (GUILayout.Button($"{label}\n[{hotkey}]", GUILayout.Height(40)))
            {
                _selectedBlockType = type;
                _toolMode = PlacementToolMode.Place;
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawToolsAndActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("ARCHITECTURAL ACTIONS", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("↔ Mirror Symmetry (Left ➔ Right)"))
            {
                MirrorSymmetry();
            }
            if (GUILayout.Button("⇈ Duplicate Layer Up"))
            {
                DuplicateTopLayerUp();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⌖ Re-center Blocks to Stand"))
            {
                RecenterBlocks();
            }
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑 Clear All Blocks"))
            {
                if (EditorUtility.DisplayDialog("Clear Blocks", "Are you sure you want to remove all blocks from this level?", "Yes, Clear", "Cancel"))
                {
                    ClearBlocksInScene();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelStatistics()
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            int blockCount = blocksRoot != null ? blocksRoot.childCount : 0;

            int wood = 0, stone = 0, armored = 0, crystal = 0, core = 0;
            if (blocksRoot != null)
            {
                for (int i = 0; i < blocksRoot.childCount; i++)
                {
                    var b = blocksRoot.GetChild(i);
                    if (b.name.Contains("wood") || b.name.Contains("Wood")) wood++;
                    else if (b.name.Contains("stone") || b.name.Contains("Stone")) stone++;
                    else if (b.name.Contains("armored") || b.name.Contains("Armored")) armored++;
                    else if (b.name.Contains("crystal") || b.name.Contains("Crystal")) crystal++;
                    else if (b.name.Contains("core") || b.name.Contains("Core")) core++;
                }
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("LEVEL STATISTICS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Blocks: {blockCount} | Stands: {_standsWorkingList.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Wood: {wood} | Stone: {stone} | Armored: {armored} | Crystal: {crystal} | Core: {core}", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox("Controls in SceneView:\n- Left Click: Place block on Stand or adjacent to clicked block face\n- Shift + Left Click: Delete clicked block\n- Keys 1-5: Switch block type | Key G: Toggle half-step", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void ApplyStandPreset(StandPlacement[] presets)
        {
            _standsWorkingList.Clear();
            _standsWorkingList.AddRange(presets);
            RefreshStandsInScene();
        }

        // ==================== SCENEVIEW INTERACTION ====================

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            // Handle Hotkeys
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Alpha1) { _selectedBlockType = ObjectType.Box; _toolMode = PlacementToolMode.Place; Repaint(); }
                else if (e.keyCode == KeyCode.Alpha2) { _selectedBlockType = ObjectType.Stone; _toolMode = PlacementToolMode.Place; Repaint(); }
                else if (e.keyCode == KeyCode.Alpha3) { _selectedBlockType = ObjectType.Cone; _toolMode = PlacementToolMode.Place; Repaint(); }
                else if (e.keyCode == KeyCode.Alpha4) { _selectedBlockType = ObjectType.Column; _toolMode = PlacementToolMode.Place; Repaint(); }
                else if (e.keyCode == KeyCode.Alpha5) { _selectedBlockType = ObjectType.PaintBottle; _toolMode = PlacementToolMode.Place; Repaint(); }
                else if (e.keyCode == KeyCode.G) { _enableHalfStep = !_enableHalfStep; Repaint(); }
            }

            // Raycast into scene
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            _ghostValid = false;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var hitGo = hit.collider.gameObject;

                // Check if hit a block
                bool hitBlock = hitGo.transform.IsChildOf(GetOrCreateWorkingRoot().transform.Find("Blocks"));
                bool hitStand = hitGo.name.Contains("Tabletop") || hitGo.name.Contains("Stand");

                if (hitBlock || hitStand)
                {
                    _ghostValid = true;
                    if (hitStand)
                    {
                        // Place on Stand tabletop surface
                        float snappedX = SnapX(hit.point.x);
                        float tableTopY = hit.collider.bounds.max.y;
                        float snappedY = tableTopY + (BLOCK_SIZE * 0.5f);
                        float snappedZ = SnapZ(STAND_Z);
                        _ghostPosition = new Vector3(snappedX, snappedY, snappedZ);
                    }
                    else if (hitBlock)
                    {
                        // Place on adjacent face of block
                        Vector3 normal = hit.normal;
                        Vector3 blockCenter = hitGo.transform.position;

                        // Snap normal to dominant axis
                        if (Mathf.Abs(normal.y) > 0.5f)
                        {
                            float targetY = normal.y > 0 ? blockCenter.y + STEP_Y : blockCenter.y - STEP_Y;
                            _ghostPosition = new Vector3(blockCenter.x, targetY, blockCenter.z);
                        }
                        else if (Mathf.Abs(normal.x) > 0.5f)
                        {
                            float step = _enableHalfStep ? (STEP_X * 0.5f) : STEP_X;
                            float targetX = normal.x > 0 ? blockCenter.x + step : blockCenter.x - step;
                            _ghostPosition = new Vector3(targetX, blockCenter.y, blockCenter.z);
                        }
                        else
                        {
                            float targetZ = normal.z > 0 ? blockCenter.z + STEP_Z : blockCenter.z - STEP_Z;
                            _ghostPosition = new Vector3(blockCenter.x, blockCenter.y, targetZ);
                        }
                    }

                    // Render Phantom/Ghost Cube
                    if (_toolMode == PlacementToolMode.Place && _ghostValid)
                    {
                        Color ghostCol = GetColorForType(_selectedBlockType);
                        ghostCol.a = 0.5f;
                        Handles.color = ghostCol;
                        Handles.DrawWireCube(_ghostPosition, Vector3.one * BLOCK_SIZE);
                        ghostCol.a = 0.2f;
                        Handles.color = ghostCol;
                        Handles.CubeHandleCap(0, _ghostPosition, Quaternion.identity, BLOCK_SIZE, EventType.Repaint);
                    }

                    // Handle Clicks
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        if (e.shift || _toolMode == PlacementToolMode.Erase)
                        {
                            if (hitBlock)
                            {
                                Undo.DestroyObjectImmediate(hitGo);
                                e.Use();
                                Repaint();
                            }
                        }
                        else if (_toolMode == PlacementToolMode.Place && _ghostValid)
                        {
                            PlaceBlockAt(_ghostPosition, _selectedBlockType);
                            e.Use();
                            Repaint();
                        }
                    }
                }
            }

            // Draw Stand Guides
            if (_drawGridGuides)
            {
                Handles.color = new Color(0.2f, 0.7f, 1f, 0.3f);
                foreach (var stand in _standsWorkingList)
                {
                    float w = 5.70f * stand.Scale.x;
                    float y = stand.Position.y + (BASE_STAND_TOP_Y * stand.Scale.y);
                    Handles.DrawWireCube(new Vector3(stand.Position.x, y, stand.Position.z), new Vector3(w, 0.05f, 0.95f * stand.Scale.z));
                }
            }

            HandleUtility.Repaint();
        }

        private float SnapX(float val)
        {
            if (!_enableGridSnapping) return val;
            float step = _enableHalfStep ? (STEP_X * 0.5f) : STEP_X;
            return Mathf.Round(val / step) * step;
        }

        private float SnapZ(float val)
        {
            if (!_enableGridSnapping) return val;
            return Mathf.Round(val / STEP_Z) * STEP_Z;
        }

        // ==================== BLOCK PLACEMENT ====================

        private void PlaceBlockAt(Vector3 pos, ObjectType type)
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot == null)
            {
                var bGo = new GameObject("Blocks");
                bGo.transform.SetParent(root.transform);
                blocksRoot = bGo.transform;
            }

            // Check if block already exists near this spot
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                if (Vector3.Distance(blocksRoot.GetChild(i).position, pos) < 0.2f)
                {
                    return; // already occupied
                }
            }

            GameObject prefab = GetPrefabForType(type);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, blocksRoot);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * BLOCK_SIZE;
            instance.name = $"{type}_{blocksRoot.childCount + 1}";

            var rb = instance.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            var physObj = instance.GetComponent<PhysicsObject>();
            if (physObj != null) physObj.SetRequiredTarget(true);

            Undo.RegisterCreatedObjectUndo(instance, "Place Citadel Block");
            EditorUtility.SetDirty(instance);
        }

        private Color GetColorForType(ObjectType type)
        {
            return type switch
            {
                ObjectType.Box => new Color(0.9f, 0.6f, 0.2f),
                ObjectType.Stone => new Color(0.9f, 0.9f, 0.9f),
                ObjectType.Cone => new Color(0.75f, 0.75f, 0.55f),
                ObjectType.Column => new Color(0.2f, 0.6f, 1.0f),
                ObjectType.PaintBottle => new Color(0.95f, 0.3f, 0.95f),
                _ => Color.white
            };
        }

        private GameObject GetPrefabForType(ObjectType type)
        {
            return type switch
            {
                ObjectType.Box => _woodPrefab,
                ObjectType.Stone => _stonePrefab,
                ObjectType.Cone => _armoredPrefab,
                ObjectType.Column => _crystalPrefab,
                ObjectType.PaintBottle => _corePrefab,
                _ => _woodPrefab
            };
        }

        private ObjectType GetTypeFromInstance(GameObject go)
        {
            string n = go.name.ToLower();
            if (n.Contains("core") || n.Contains("bottle")) return ObjectType.PaintBottle;
            if (n.Contains("crystal") || n.Contains("column")) return ObjectType.Column;
            if (n.Contains("armored") || n.Contains("cone")) return ObjectType.Cone;
            if (n.Contains("stone")) return ObjectType.Stone;
            return ObjectType.Box;
        }

        // ==================== SCENE & DATA SYNCHRONIZATION ====================

        public static GameObject GetOrCreateWorkingRoot()
        {
            var root = GameObject.Find(ROOT_NAME);
            if (root == null)
            {
                root = new GameObject(ROOT_NAME);
                root.tag = "EditorOnly";
                root.AddComponent<LevelDesignerWorkingRoot>();

                var stands = new GameObject("Stands");
                stands.transform.SetParent(root.transform);

                var blocks = new GameObject("Blocks");
                blocks.transform.SetParent(root.transform);
            }
            else if (root.GetComponent<LevelDesignerWorkingRoot>() == null)
            {
                root.AddComponent<LevelDesignerWorkingRoot>();
            }
            return root;
        }

        public void LoadLevelToWorkingRoot(int levelId)
        {
            LoadDatabase();
            if (_database == null) return;

            _currentLevelData = _database.GetLevel(levelId);
            if (_currentLevelData == null)
            {
                string path = $"{LEVELS_FOLDER}/LevelData_Citadel{levelId:D2}.asset";
                _currentLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            }
            if (_currentLevelData == null) return;

            // Load stands into working list
            _standsWorkingList.Clear();
            if (_currentLevelData.Stands != null && _currentLevelData.Stands.Length > 0)
            {
                foreach (var s in _currentLevelData.Stands)
                {
                    _standsWorkingList.Add(new StandPlacement
                    {
                        Position = s.Position,
                        Scale = s.Scale,
                        Rotation = s.Rotation
                    });
                }
            }
            else
            {
                _standsWorkingList.Add(new StandPlacement
                {
                    Position = new Vector3(0, 0, STAND_Z),
                    Scale = Vector3.one,
                    Rotation = Vector3.zero
                });
            }

            // Populate Scene
            var root = GetOrCreateWorkingRoot();
            ClearChildren(root.transform.Find("Stands"));
            ClearChildren(root.transform.Find("Blocks"));

            // Hide default scene stand while level designer is active
            var sceneStand = GameObject.Find("Stand");
            if (sceneStand != null) sceneStand.SetActive(false);

            // Hide old SkyCitadelPreview if present
            var preview = GameObject.Find("SkyCitadelPreview");
            if (preview != null) preview.SetActive(false);

            // Spawn Stands
            RefreshStandsInScene();

            // Spawn Blocks
            var blocksRoot = root.transform.Find("Blocks");
            if (_currentLevelData.Objects != null)
            {
                foreach (var obj in _currentLevelData.Objects)
                {
                    GameObject prefab = GetPrefabForType(obj.Type);
                    if (prefab == null) continue;

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, blocksRoot);
                    instance.transform.position = obj.Position;
                    instance.transform.rotation = Quaternion.Euler(obj.Rotation);
                    instance.transform.localScale = obj.Scale;
                    instance.name = $"{obj.Type}_{blocksRoot.childCount}";

                    var rb = instance.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;

                    var phys = instance.GetComponent<PhysicsObject>();
                    if (phys != null) phys.SetRequiredTarget(obj.IsRequiredTarget);
                }
            }

            Debug.Log($"[LevelDesigner] Loaded Level {levelId} ('{_currentLevelData.LevelName}') with {_standsWorkingList.Count} stand(s) and {blocksRoot.childCount} block(s).");
            Repaint();
        }

        private void RefreshStandsInScene()
        {
            var root = GetOrCreateWorkingRoot();
            var standsRoot = root.transform.Find("Stands");
            if (standsRoot == null)
            {
                var sGo = new GameObject("Stands");
                sGo.transform.SetParent(root.transform);
                standsRoot = sGo.transform;
            }

            ClearChildren(standsRoot);

            if (_standPrefab == null) LoadPrefabs();

            foreach (var s in _standsWorkingList)
            {
                var stand = (GameObject)PrefabUtility.InstantiatePrefab(_standPrefab, standsRoot);
                stand.transform.position = s.Position;
                stand.transform.localScale = s.Scale;
                stand.transform.rotation = Quaternion.Euler(s.Rotation);
                stand.name = $"Stand_{standsRoot.childCount}";
            }
        }

        public void SaveWorkingRootToLevelData()
        {
            if (_currentLevelData == null)
            {
                Debug.LogError("[LevelDesigner] Cannot save: CurrentLevelData is null!");
                return;
            }

            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot == null) return;

            // Extract blocks
            var objectsList = new List<ObjectPlacement>();
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                var child = blocksRoot.GetChild(i).gameObject;
                ObjectType type = GetTypeFromInstance(child);
                var phys = child.GetComponent<PhysicsObject>();
                bool isReq = phys != null ? phys.IsRequiredTarget : true;

                objectsList.Add(new ObjectPlacement
                {
                    Type = type,
                    Position = child.transform.position,
                    Rotation = child.transform.rotation.eulerAngles,
                    Scale = child.transform.localScale,
                    IsRequiredTarget = isReq,
                    OverrideMass = false,
                    CustomMass = 1.0f
                });
            }

            // Save Stands & Objects
            Undo.RecordObject(_currentLevelData, $"Save Level {_selectedLevelId} Architecture");
            SetPrivateField(_currentLevelData, "_stands", _standsWorkingList.ToArray());
            SetPrivateField(_currentLevelData, "_objects", objectsList.ToArray());

            EditorUtility.SetDirty(_currentLevelData);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=#44FF44><b>[LevelDesigner] Successfully saved Level {_selectedLevelId} ('{_currentLevelData.LevelName}') with {_standsWorkingList.Count} stands and {objectsList.Count} blocks!</b></color>");
            ShowNotification(new GUIContent($"Level {_selectedLevelId} Saved! ({objectsList.Count} blocks)"));
        }

        // ==================== ARCHITECTURAL TOOLS ====================

        private void MirrorSymmetry()
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot == null) return;

            var toAdd = new List<(Vector3 pos, ObjectType type)>();

            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                var child = blocksRoot.GetChild(i);
                if (child.position.x < -0.1f)
                {
                    Vector3 mirroredPos = new Vector3(-child.position.x, child.position.y, child.position.z);
                    ObjectType type = GetTypeFromInstance(child.gameObject);

                    // Check if already exists on other side
                    bool exists = false;
                    for (int j = 0; j < blocksRoot.childCount; j++)
                    {
                        if (Vector3.Distance(blocksRoot.GetChild(j).position, mirroredPos) < 0.2f)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        toAdd.Add((mirroredPos, type));
                    }
                }
            }

            foreach (var item in toAdd)
            {
                PlaceBlockAt(item.pos, item.type);
            }

            Debug.Log($"[LevelDesigner] Mirrored {toAdd.Count} blocks across X = 0.");
        }

        private void DuplicateTopLayerUp()
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot == null || blocksRoot.childCount == 0) return;

            float maxY = float.MinValue;
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                float y = blocksRoot.GetChild(i).position.y;
                if (y > maxY) maxY = y;
            }

            var toAdd = new List<(Vector3 pos, ObjectType type)>();
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                var c = blocksRoot.GetChild(i);
                if (Mathf.Abs(c.position.y - maxY) < 0.2f)
                {
                    toAdd.Add((new Vector3(c.position.x, c.position.y + STEP_Y, c.position.z), GetTypeFromInstance(c.gameObject)));
                }
            }

            foreach (var item in toAdd)
            {
                PlaceBlockAt(item.pos, item.type);
            }
        }

        private void RecenterBlocks()
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot == null || blocksRoot.childCount == 0) return;

            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                float x = blocksRoot.GetChild(i).position.x;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
            }

            float centerOffset = (minX + maxX) * 0.5f;
            for (int i = 0; i < blocksRoot.childCount; i++)
            {
                var c = blocksRoot.GetChild(i);
                c.position = new Vector3(c.position.x - centerOffset, c.position.y, c.position.z);
            }
        }

        private void ClearBlocksInScene()
        {
            var root = GetOrCreateWorkingRoot();
            var blocksRoot = root.transform.Find("Blocks");
            if (blocksRoot != null)
            {
                ClearChildren(blocksRoot);
            }
        }

        private static void ClearChildren(Transform t)
        {
            if (t == null) return;
            if (Selection.activeTransform != null && Selection.activeTransform.IsChildOf(t))
            {
                Selection.activeObject = null;
            }
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(t.GetChild(i).gameObject);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
