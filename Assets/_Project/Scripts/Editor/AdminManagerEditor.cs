using UnityEditor;
using UnityEngine;
using RoyalSmash.Debugging;

namespace RoyalSmash.Editor
{
    [CustomEditor(typeof(AdminManager))]
    public class AdminManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var admin = (AdminManager)target;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("👑 ROYAL SMASH ADMIN & CHEAT MANAGER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Live player stats editor & instant map test controller.\nWorks both during Play Mode and Edit Mode.", MessageType.Info);
            EditorGUILayout.Space(4);

            if (GUILayout.Button("🔄 Sync From Runtime / Save Data", GUILayout.Height(28)))
            {
                admin.SyncFieldsFromRuntime();
            }

            EditorGUILayout.Space(10);

            // =========================================================================
            // 1. MAP & LEVEL TESTING
            // =========================================================================
            EditorGUILayout.LabelField("🗺️ MAP & LEVEL TESTING", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            admin.targetLevel = EditorGUILayout.IntSlider("Target Level (1-30)", admin.targetLevel, 1, 30);

            GUI.backgroundColor = new Color(0.2f, 0.7f, 1f);
            if (GUILayout.Button($"🚀 SPAWN LEVEL {admin.targetLevel}", GUILayout.Height(32)))
            {
                admin.SpawnTargetLevel();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Lvl 1")) admin.SpawnLevel(1);
            if (GUILayout.Button("Lvl 5")) admin.SpawnLevel(5);
            if (GUILayout.Button("Lvl 10")) admin.SpawnLevel(10);
            if (GUILayout.Button("Lvl 15")) admin.SpawnLevel(15);
            if (GUILayout.Button("Lvl 20")) admin.SpawnLevel(20);
            if (GUILayout.Button("Lvl 30")) admin.SpawnLevel(30);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("🔄 Reload Current Stage"))
            {
                admin.ReloadCurrentLevel();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =========================================================================
            // 2. QUICK GAMEPLAY CHEATS
            // =========================================================================
            EditorGUILayout.LabelField("⚡ GAMEPLAY CHEATS", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
            if (GUILayout.Button("🏆 WIN LEVEL NOW", GUILayout.Height(26)))
            {
                admin.WinLevelNow();
            }

            GUI.backgroundColor = new Color(0.9f, 0.35f, 0.35f);
            if (GUILayout.Button("💀 FAIL LEVEL", GUILayout.Height(26)))
            {
                admin.FailLevelNow();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("💣 +10 Balls Ammo to Mortar"))
            {
                admin.AddTenBalls();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =========================================================================
            // 3. ECONOMY & LIVES
            // =========================================================================
            EditorGUILayout.LabelField("💰 ECONOMY & LIVES", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            admin.gold = EditorGUILayout.IntField("Gold Balance", admin.gold);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+500 Gold")) admin.AddGoldAmount(500);
            if (GUILayout.Button("+5,000 Gold")) admin.AddGoldAmount(5000);
            if (GUILayout.Button("Set 10,000")) admin.SetGoldAmount(10000);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            admin.lives = EditorGUILayout.IntSlider("Lives (0-5)", admin.lives, 0, 5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("❤️ Refill Lives (5/5)")) admin.RefillLives();

            GUI.backgroundColor = admin.infiniteLives ? new Color(0.3f, 0.9f, 0.4f) : Color.white;
            if (GUILayout.Button(admin.infiniteLives ? "♾️ Infinite Lives: ON" : "♾️ Infinite Lives: OFF"))
            {
                admin.ToggleInfiniteLives();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =========================================================================
            // 4. SKILLS INVENTORY
            // =========================================================================
            EditorGUILayout.LabelField("🔮 SKILLS INVENTORY", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // MegaBall
            EditorGUILayout.BeginHorizontal();
            admin.megaBallUses = EditorGUILayout.IntField("Mega Ball Uses", admin.megaBallUses);
            if (GUILayout.Button("+5 Uses")) admin.AddMegaUses(5);
            if (GUILayout.Button(admin.megaBallUnlocked ? "Lock" : "Unlock")) admin.SetMegaUnlocked(!admin.megaBallUnlocked);
            EditorGUILayout.EndHorizontal();

            // InfinityBall
            EditorGUILayout.BeginHorizontal();
            admin.infinityBallUses = EditorGUILayout.IntField("Infinity Ball Uses", admin.infinityBallUses);
            if (GUILayout.Button("+5 Uses")) admin.AddInfinityUses(5);
            if (GUILayout.Button(admin.infinityBallUnlocked ? "Lock" : "Unlock")) admin.SetInfinityUnlocked(!admin.infinityBallUnlocked);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("✨ Max All Skills (+20 Each & Unlock)"))
            {
                admin.SetMegaUnlocked(true);
                admin.SetInfinityUnlocked(true);
                admin.AddMegaUses(20);
                admin.AddInfinityUses(20);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =========================================================================
            // 5. PROFILE & USERNAME
            // =========================================================================
            EditorGUILayout.LabelField("👤 PLAYER PROFILE", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            admin.playerUsername = EditorGUILayout.TextField("Username", admin.playerUsername);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                admin.ApplyUsername();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // =========================================================================
            // 6. SAVE & CONFIG
            // =========================================================================
            EditorGUILayout.LabelField("💾 SAVE DATA MANAGEMENT", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("💾 Save Stats to Disk")) admin.SaveStatsToDisk();
            if (GUILayout.Button("🔄 Reload from Disk")) admin.ReloadStatsFromDisk();
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("⚠️ Reset Save Data to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Save Data", "Are you sure you want to reset all player progress and stats to factory defaults?", "Reset", "Cancel"))
                {
                    admin.ResetSaveToDefaults();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // In-Game GUI options
            admin.showInGameGUI = EditorGUILayout.Toggle("Show In-Game GUI Overlay", admin.showInGameGUI);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(admin);
            }
        }
    }
}
