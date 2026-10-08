using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Gameplay;
using RoyalSmash.Skills;

namespace RoyalSmash.Editor
{
    /// <summary>
    /// Interactive Game Designer & Physics Balance Window for Royal Smash.
    /// Allows real-time editing and 1-click synchronized persistence for:
    /// 1. Projectile ballistics (Mass, Launch Speed, Impact Impulse / Itme Gücü, Bounciness, Cooldown).
    /// 2. All 5 Citadel block types (Wood, Stone, Armored, Crystal, Core) physics & breakability.
    /// 3. Skill parameters (MegaBall mass/diameter multipliers, InfinityBall rapid stream cooldown).
    /// Directly updates and synchronizes all ScriptableObjects and Prefab Rigidbodies across the project.
    /// </summary>
    public class GameDesignerWindow : EditorWindow
    {
        private const string PROJECTILE_CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/ProjectileConfig.asset";
        private const string SKILL_CONFIG_PATH = "Assets/_Project/ScriptableObjects/Config/SkillConfig.asset";
        private const string SKILL_CONFIG_RES_PATH = "Assets/_Project/Resources/Config/SkillConfig.asset";

        // Citadel block config paths
        private const string CFG_WOOD = "Assets/_Project/Art/SkyCitadel/Levels/Config_wood.asset";
        private const string CFG_STONE = "Assets/_Project/Art/SkyCitadel/Levels/Config_stone.asset";
        private const string CFG_ARMORED = "Assets/_Project/Art/SkyCitadel/Levels/Config_armored.asset";
        private const string CFG_CRYSTAL = "Assets/_Project/Art/SkyCitadel/Levels/Config_crystal.asset";
        private const string CFG_CORE = "Assets/_Project/Art/SkyCitadel/Levels/Config_core.asset";

        // Core fallback block config paths
        private const string CORE_CFG_WOOD = "Assets/_Project/ScriptableObjects/Objects/ObjectConfig_Box.asset";
        private const string CORE_CFG_STONE = "Assets/_Project/ScriptableObjects/Objects/ObjectConfig_Stone.asset";
        private const string CORE_CFG_ARMORED = "Assets/_Project/ScriptableObjects/Objects/ObjectConfig_Cone.asset";
        private const string CORE_CFG_CRYSTAL = "Assets/_Project/ScriptableObjects/Objects/ObjectConfig_Column.asset";
        private const string CORE_CFG_CORE = "Assets/_Project/ScriptableObjects/Objects/ObjectConfig_PaintBottle.asset";

        // Prefab paths
        private const string PREFAB_CITADEL_BALL = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_CitadelBall.prefab";
        private const string PREFAB_WOOD = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_wood.prefab";
        private const string PREFAB_STONE = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_stone.prefab";
        private const string PREFAB_ARMORED = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_armored.prefab";
        private const string PREFAB_CRYSTAL = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_crystal.prefab";
        private const string PREFAB_CORE = "Assets/_Project/Art/SkyCitadel/Prefabs/PF_core.prefab";

        private int _selectedTab = 0;
        private Vector2 _scrollPos;

        // Config references
        private ProjectileConfig _projectileConfig;
        private SkillConfig _skillConfig;
        private ObjectConfig _cfgWood;
        private ObjectConfig _cfgStone;
        private ObjectConfig _cfgArmored;
        private ObjectConfig _cfgCrystal;
        private ObjectConfig _cfgCore;

        [MenuItem("Tools/Royal Smash/Game Designer (Physics & Balance)", false, 11)]
        [MenuItem("Window/Royal Smash/Game Designer", false, 11)]
        public static void OpenWindow()
        {
            var window = GetWindow<GameDesignerWindow>("Game Designer");
            window.minSize = new Vector2(480, 680);
            window.Show();
        }

        private void OnEnable()
        {
            LoadAllConfigs();
        }

        private void LoadAllConfigs()
        {
            _projectileConfig = AssetDatabase.LoadAssetAtPath<ProjectileConfig>(PROJECTILE_CONFIG_PATH);
            _skillConfig = AssetDatabase.LoadAssetAtPath<SkillConfig>(SKILL_CONFIG_PATH);

            _cfgWood = AssetDatabase.LoadAssetAtPath<ObjectConfig>(CFG_WOOD);
            _cfgStone = AssetDatabase.LoadAssetAtPath<ObjectConfig>(CFG_STONE);
            _cfgArmored = AssetDatabase.LoadAssetAtPath<ObjectConfig>(CFG_ARMORED);
            _cfgCrystal = AssetDatabase.LoadAssetAtPath<ObjectConfig>(CFG_CRYSTAL);
            _cfgCore = AssetDatabase.LoadAssetAtPath<ObjectConfig>(CFG_CORE);
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(6);
            _selectedTab = GUILayout.Toolbar(_selectedTab, new[] { "💣 Ball & Mortar", "🧱 5 Block Types", "⚖ Presets & Sync" }, GUILayout.Height(30));
            EditorGUILayout.Space(8);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawBallSection();
                    break;
                case 1:
                    DrawBlocksSection();
                    break;
                case 2:
                    DrawPresetsSection();
                    break;
            }

            EditorGUILayout.EndScrollView();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🎮 Royal Smash Game Designer", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🔄 Reload Assets", GUILayout.Width(110)))
            {
                LoadAllConfigs();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Live Physics, Ballistics & Economy Control Panel", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        // ==================== TAB 0: BALL & MORTAR ====================

        private void DrawBallSection()
        {
            if (_projectileConfig == null)
            {
                EditorGUILayout.HelpBox("ProjectileConfig asset not found at " + PROJECTILE_CONFIG_PATH, MessageType.Error);
                return;
            }

            SerializedObject soProj = new SerializedObject(_projectileConfig);
            soProj.Update();

            var propMass = soProj.FindProperty("_mass");
            var propSpeed = soProj.FindProperty("_launchSpeed");
            var propImpulse = soProj.FindProperty("_impactImpulse");
            var propBounce = soProj.FindProperty("_bounciness");
            var propFriction = soProj.FindProperty("_friction");
            var propCooldown = soProj.FindProperty("_fireCooldown");
            var propBallCount = soProj.FindProperty("_defaultBallCount");
            var propLifetime = soProj.FindProperty("_lifetime");

            EditorGUILayout.LabelField("Primary Ballistics", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Mass
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Ball Mass (Weight)", "Rigidbody mass in kg."));
            propMass.floatValue = EditorGUILayout.Slider(propMass.floatValue, 0.5f, 5.0f);
            EditorGUILayout.EndHorizontal();

            // Launch Speed
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Launch Speed", "Straight line flight speed in m/s."));
            propSpeed.floatValue = EditorGUILayout.Slider(propSpeed.floatValue, 10f, 50f);
            EditorGUILayout.EndHorizontal();

            // Impact Impulse / Itme Gücü
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Impact Impulse (İtme Gücü)", "Direct physical punch impulse (N·s) transferred to hit blocks."));
            propImpulse.floatValue = EditorGUILayout.Slider(propImpulse.floatValue, 0f, 100f);
            EditorGUILayout.EndHorizontal();

            // Bounciness & Friction
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Bounciness / Friction");
            propBounce.floatValue = EditorGUILayout.Slider(propBounce.floatValue, 0f, 1f);
            propFriction.floatValue = EditorGUILayout.Slider(propFriction.floatValue, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Calculated Live Metrics
            float m = propMass.floatValue;
            float v = propSpeed.floatValue;
            float p = m * v;
            float ek = 0.5f * m * v * v;

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📊 Calculated Live Flight Metrics", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"• Natural Momentum (p = m·v):", $"{p:F1} kg·m/s (N·s)");
            EditorGUILayout.LabelField($"• Kinetic Impact Work (Ek = ½mv²):", $"{ek:F1} Joules");
            EditorGUILayout.LabelField($"• Added Physical Impact Punch:", $"{propImpulse.floatValue:F1} N·s");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Gameplay & Ammunition", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            propCooldown.floatValue = EditorGUILayout.Slider(new GUIContent("Fire Cooldown (s)", "Delay between shots."), propCooldown.floatValue, 0.05f, 1.0f);
            propBallCount.intValue = EditorGUILayout.IntSlider(new GUIContent("Default Level Balls", "Base ammunition if not overridden by LevelData."), propBallCount.intValue, 10, 50);
            propLifetime.floatValue = EditorGUILayout.Slider(new GUIContent("Ball Lifetime (s)", "Seconds before auto-destruction."), propLifetime.floatValue, 1.0f, 15.0f);
            EditorGUILayout.EndVertical();

            soProj.ApplyModifiedProperties();

            // Skill Multipliers
            if (_skillConfig != null)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Skill Multipliers", EditorStyles.boldLabel);
                SerializedObject soSkill = new SerializedObject(_skillConfig);
                soSkill.Update();

                EditorGUILayout.BeginVertical("box");
                var propMegaD = soSkill.FindProperty("_megaBallDiameterMultiplier");
                var propMegaM = soSkill.FindProperty("_megaBallMassMultiplier");
                var propInfCooldown = soSkill.FindProperty("_infinityBallRapidCooldown");
                var propInfDuration = soSkill.FindProperty("_infinityBallDuration");

                propMegaD.floatValue = EditorGUILayout.Slider("MegaBall Diameter Multiplier", propMegaD.floatValue, 1.2f, 3.5f);
                propMegaM.floatValue = EditorGUILayout.Slider("MegaBall Mass Multiplier", propMegaM.floatValue, 1.5f, 5.0f);
                propInfCooldown.floatValue = EditorGUILayout.Slider("InfinityBall Rapid Rate (s)", propInfCooldown.floatValue, 0.02f, 0.20f);
                propInfDuration.floatValue = EditorGUILayout.Slider("InfinityBall Duration (s)", propInfDuration.floatValue, 2.0f, 15.0f);
                EditorGUILayout.EndVertical();

                soSkill.ApplyModifiedProperties();
            }
        }

        // ==================== TAB 1: 5 BLOCK TYPES ====================

        private void DrawBlocksSection()
        {
            EditorGUILayout.LabelField("Citadel Architectural Blocks (5 Variants)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Modifications here are automatically synchronized to both Citadel and Core configs, as well as Prefab Rigidbodies.", MessageType.Info);

            DrawBlockCard("🪵 Wood Block (Ahşap / Kutu)", _cfgWood, new Color(0.95f, 0.75f, 0.40f));
            DrawBlockCard("🪨 Stone Block (Ağır Taş)", _cfgStone, new Color(0.85f, 0.85f, 0.85f));
            DrawBlockCard("🛡️ Armored Block (Zırhlı Kaplama)", _cfgArmored, new Color(0.95f, 0.80f, 0.30f));
            DrawBlockCard("💎 Crystal Block (Kristal Sütun)", _cfgCrystal, new Color(0.40f, 0.85f, 1.0f));
            DrawBlockCard("🧪 Core Block (Reaktif Çekirdek)", _cfgCore, new Color(1.0f, 0.45f, 0.85f));
        }

        private void DrawBlockCard(string title, ObjectConfig cfg, Color accent)
        {
            if (cfg == null) return;

            GUI.backgroundColor = accent;
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            SerializedObject so = new SerializedObject(cfg);
            so.Update();

            var pMass = so.FindProperty("_mass");
            var pBounce = so.FindProperty("_bounciness");
            var pFriction = so.FindProperty("_friction");
            var pBreakable = so.FindProperty("_isBreakable");
            var pBreakForce = so.FindProperty("_breakForceThreshold");
            var pFallOff = so.FindProperty("_fallOffThresholdY");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Weight (Mass in kg)", "Rigidbody mass"));
            pMass.floatValue = EditorGUILayout.Slider(pMass.floatValue, 0.2f, 10.0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Bounciness / Friction");
            pBounce.floatValue = EditorGUILayout.Slider(pBounce.floatValue, 0f, 1f);
            pFriction.floatValue = EditorGUILayout.Slider(pFriction.floatValue, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            pBreakable.boolValue = EditorGUILayout.Toggle("Is Breakable", pBreakable.boolValue);
            if (pBreakable.boolValue)
            {
                pBreakForce.floatValue = EditorGUILayout.FloatField("Break Threshold (N)", pBreakForce.floatValue);
            }
            EditorGUILayout.EndHorizontal();

            pFallOff.floatValue = EditorGUILayout.FloatField(new GUIContent("Fall-off Floor Y", "Height threshold below which block is counted as destroyed"), pFallOff.floatValue);

            so.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // ==================== TAB 2: PRESETS & SYNC ====================

        private void DrawPresetsSection()
        {
            EditorGUILayout.LabelField("Pre-configured Game Balance Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("⭐ Farzetki Balanced Citadel (Recommended)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Stone: 4.0kg | Crystal: 2.0kg | Armored: 1.5kg | Wood: 1.0kg | Core: 0.5kg");
            EditorGUILayout.LabelField("Ball: 1.5kg | Speed: 25.0 m/s | Impact Impulse: 35.0 N·s");
            if (GUILayout.Button("Apply Farzetki Citadel Balance", GUILayout.Height(30)))
            {
                ApplyFarzetkiCitadelBalance();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("💥 Super Smasher / Arcade (Easy Destruction)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Stone: 3.0kg | Crystal: 1.5kg | Armored: 1.0kg | Wood: 0.8kg | Core: 0.4kg");
            EditorGUILayout.LabelField("Ball: 2.0kg | Speed: 28.0 m/s | Impact Impulse: 50.0 N·s");
            if (GUILayout.Button("Apply Arcade Destruction Preset", GUILayout.Height(30)))
            {
                ApplyArcadeBalance();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("🏰 Heavy Fortress / Puzzle (Hard / High Resistance)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Stone: 5.5kg | Crystal: 3.5kg | Armored: 2.0kg | Wood: 1.5kg | Core: 0.6kg");
            EditorGUILayout.LabelField("Ball: 1.2kg | Speed: 22.0 m/s | Impact Impulse: 25.0 N·s");
            if (GUILayout.Button("Apply Heavy Fortress Preset", GUILayout.Height(30)))
            {
                ApplyHeavyBalance();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Full Project Synchronization", EditorStyles.boldLabel);
            if (GUILayout.Button("🔄 Full Synchronize: Configs -> Prefabs -> Live", GUILayout.Height(34)))
            {
                SyncAllConfigsAndPrefabs();
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.35f);
            if (GUILayout.Button("💾 Save & Synchronize All (Configs + Prefabs)", GUILayout.Height(38)))
            {
                SyncAllConfigsAndPrefabs();
                ShowNotification(new GUIContent("All Physics Configs & Prefabs Synchronized!"));
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Level Designer"))
            {
                LevelDesignerWindow.OpenWindow();
            }
            if (GUILayout.Button("Ping ProjectileConfig"))
            {
                if (_projectileConfig != null) Selection.activeObject = _projectileConfig;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ==================== BALANCING PRESET IMPLEMENTATIONS ====================

        public void ApplyFarzetkiCitadelBalance()
        {
            if (_projectileConfig != null)
            {
                _projectileConfig.SetBallistics(25.0f, 1.5f, 35.0f, 0.45f, 0.30f);
                EditorUtility.SetDirty(_projectileConfig);
            }

            if (_cfgStone != null) { _cfgStone.SetPhysicsProperties(4.0f, 0.05f, 0.70f); EditorUtility.SetDirty(_cfgStone); }
            if (_cfgCrystal != null) { _cfgCrystal.SetPhysicsProperties(2.0f, 0.10f, 0.50f); EditorUtility.SetDirty(_cfgCrystal); }
            if (_cfgArmored != null) { _cfgArmored.SetPhysicsProperties(1.5f, 0.25f, 0.35f); EditorUtility.SetDirty(_cfgArmored); }
            if (_cfgWood != null) { _cfgWood.SetPhysicsProperties(1.0f, 0.20f, 0.40f); EditorUtility.SetDirty(_cfgWood); }
            if (_cfgCore != null) { _cfgCore.SetPhysicsProperties(0.5f, 0.10f, 0.40f); EditorUtility.SetDirty(_cfgCore); }

            SyncAllConfigsAndPrefabs();
            Debug.Log("<color=#44FF44><b>[GameDesigner] Successfully applied Farzetki Citadel Balance! Stone=4kg, Crystal=2kg, Armored=1.5kg, Ball=1.5kg @ 25m/s (Impulse 35).</b></color>");
        }

        public void ApplyArcadeBalance()
        {
            if (_projectileConfig != null)
            {
                _projectileConfig.SetBallistics(28.0f, 2.0f, 50.0f, 0.50f, 0.25f);
                EditorUtility.SetDirty(_projectileConfig);
            }

            if (_cfgStone != null) { _cfgStone.SetPhysicsProperties(3.0f, 0.08f, 0.60f); EditorUtility.SetDirty(_cfgStone); }
            if (_cfgCrystal != null) { _cfgCrystal.SetPhysicsProperties(1.5f, 0.15f, 0.45f); EditorUtility.SetDirty(_cfgCrystal); }
            if (_cfgArmored != null) { _cfgArmored.SetPhysicsProperties(1.0f, 0.30f, 0.30f); EditorUtility.SetDirty(_cfgArmored); }
            if (_cfgWood != null) { _cfgWood.SetPhysicsProperties(0.8f, 0.25f, 0.35f); EditorUtility.SetDirty(_cfgWood); }
            if (_cfgCore != null) { _cfgCore.SetPhysicsProperties(0.4f, 0.15f, 0.35f); EditorUtility.SetDirty(_cfgCore); }

            SyncAllConfigsAndPrefabs();
        }

        public void ApplyHeavyBalance()
        {
            if (_projectileConfig != null)
            {
                _projectileConfig.SetBallistics(22.0f, 1.2f, 25.0f, 0.40f, 0.35f);
                EditorUtility.SetDirty(_projectileConfig);
            }

            if (_cfgStone != null) { _cfgStone.SetPhysicsProperties(5.5f, 0.03f, 0.75f); EditorUtility.SetDirty(_cfgStone); }
            if (_cfgCrystal != null) { _cfgCrystal.SetPhysicsProperties(3.5f, 0.08f, 0.55f); EditorUtility.SetDirty(_cfgCrystal); }
            if (_cfgArmored != null) { _cfgArmored.SetPhysicsProperties(2.0f, 0.20f, 0.40f); EditorUtility.SetDirty(_cfgArmored); }
            if (_cfgWood != null) { _cfgWood.SetPhysicsProperties(1.5f, 0.15f, 0.45f); EditorUtility.SetDirty(_cfgWood); }
            if (_cfgCore != null) { _cfgCore.SetPhysicsProperties(0.6f, 0.08f, 0.45f); EditorUtility.SetDirty(_cfgCore); }

            SyncAllConfigsAndPrefabs();
        }

        // ==================== SYNCHRONIZATION ENGINE ====================

        public void SyncAllConfigsAndPrefabs()
        {
            LoadAllConfigs();

            // 1. Mirror Citadel configs into core ScriptableObjects
            SyncBlockConfigPair(_cfgWood, CORE_CFG_WOOD);
            SyncBlockConfigPair(_cfgStone, CORE_CFG_STONE);
            SyncBlockConfigPair(_cfgArmored, CORE_CFG_ARMORED);
            SyncBlockConfigPair(_cfgCrystal, CORE_CFG_CRYSTAL);
            SyncBlockConfigPair(_cfgCore, CORE_CFG_CORE);

            // 2. Mirror Skill configs
            if (_skillConfig != null)
            {
                EditorUtility.SetDirty(_skillConfig);
                var resSkill = AssetDatabase.LoadAssetAtPath<SkillConfig>(SKILL_CONFIG_RES_PATH);
                if (resSkill != null && resSkill != _skillConfig)
                {
                    EditorUtility.CopySerialized(_skillConfig, resSkill);
                    EditorUtility.SetDirty(resSkill);
                }
            }

            // 3. Sync Prefabs Rigidbodies
            float ballMass = _projectileConfig != null ? _projectileConfig.Mass : 1.5f;
            SetPrefabRigidbodyMass(PREFAB_CITADEL_BALL, ballMass);

            if (_cfgWood != null) SetPrefabRigidbodyMass(PREFAB_WOOD, _cfgWood.Mass);
            if (_cfgStone != null) SetPrefabRigidbodyMass(PREFAB_STONE, _cfgStone.Mass);
            if (_cfgArmored != null) SetPrefabRigidbodyMass(PREFAB_ARMORED, _cfgArmored.Mass);
            if (_cfgCrystal != null) SetPrefabRigidbodyMass(PREFAB_CRYSTAL, _cfgCrystal.Mass);
            if (_cfgCore != null) SetPrefabRigidbodyMass(PREFAB_CORE, _cfgCore.Mass);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GameDesigner] All Configs and Prefabs successfully synchronized!");
        }

        private void SyncBlockConfigPair(ObjectConfig source, string targetPath)
        {
            if (source == null) return;
            EditorUtility.SetDirty(source);

            var target = AssetDatabase.LoadAssetAtPath<ObjectConfig>(targetPath);
            if (target != null && target != source)
            {
                target.SetPhysicsProperties(source.Mass, source.Bounciness, source.Friction);
                target.SetBreakable(source.IsBreakable, source.BreakForceThreshold);
                target.SetDestructionSettings(source.ShrinkDuration, source.FallOffThresholdY);
                EditorUtility.SetDirty(target);
            }
        }

        private void SetPrefabRigidbodyMass(string prefabPath, float mass)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            var rb = prefab.GetComponent<Rigidbody>();
            if (rb != null && Mathf.Abs(rb.mass - mass) > 0.001f)
            {
                rb.mass = mass;
                EditorUtility.SetDirty(prefab);
            }
        }
    }
}
