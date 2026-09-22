using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Economy;
using RoyalSmash.Lives;
using RoyalSmash.Skills;
using RoyalSmash.Profile;
using RoyalSmash.Levels;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Debugging
{
    /// <summary>
    /// Developer Admin & Cheat Manager.
    /// Provides live player stats editing, instant level map spawning/testing,
    /// and quick cheats both in Editor Inspector and via an in-game debug overlay.
    /// </summary>
    public class AdminManager : MonoBehaviour
    {
        public static AdminManager Instance { get; private set; }

        [Header("--- Level Testing & Map Spawner ---")]
        [Tooltip("Level index to spawn (1-30)")]
        [Range(1, 30)]
        public int targetLevel = 1;

        [Header("--- Player Stats & Economy ---")]
        public string playerUsername = "AdminPlayer";
        public int gold = 1000;
        [Range(0, 5)]
        public int lives = 5;
        public bool infiniteLives = false;

        [Header("--- Skills Inventory ---")]
        public bool megaBallUnlocked = true;
        public int megaBallUses = 10;
        public bool infinityBallUnlocked = true;
        public int infinityBallUses = 10;

        [Header("--- In-Game GUI Overlay ---")]
        public bool showInGameGUI = true;
        public KeyCode toggleKey = KeyCode.F1;
        public KeyCode backquoteKey = KeyCode.BackQuote;

        private bool _guiExpanded = false;
        private Rect _windowRect = new Rect(20, 80, 320, 480);
        private Vector2 _guiScrollPos;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            SyncFieldsFromRuntime();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.f1Key.wasPressedThisFrame || kb.backquoteKey.wasPressedThisFrame)
                {
                    _guiExpanded = !_guiExpanded;
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(backquoteKey))
            {
                _guiExpanded = !_guiExpanded;
            }
#endif

            if (infiniteLives && !LivesManager.InfiniteLives)
            {
                LivesManager.InfiniteLives = true;
            }
            else if (!infiniteLives && LivesManager.InfiniteLives)
            {
                LivesManager.InfiniteLives = false;
            }
        }

        /// <summary>
        /// Reads active runtime managers and save data into AdminManager fields.
        /// </summary>
        [ContextMenu("Sync From Runtime")]
        public void SyncFieldsFromRuntime()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (save != null)
            {
                playerUsername = save.Username;
                gold = save.Gold;
                lives = save.Lives;
                megaBallUses = save.MegaBallUses;
                megaBallUnlocked = save.MegaBallUnlocked;
                infinityBallUses = save.InfinityBallUses;
                infinityBallUnlocked = save.InfinityBallUnlocked;
            }

            var lvl = LevelManager.Instance;
            if (lvl != null)
            {
                targetLevel = Mathf.Clamp(lvl.CurrentLevelIndex, 1, 30);
            }

            infiniteLives = LivesManager.InfiniteLives;
        }

        // =========================================================================
        // LEVEL SPAWNER & GAMEPLAY CHEATS
        // =========================================================================

        /// <summary>
        /// Instantly loads and spawns the targetLevel in the current scene.
        /// </summary>
        [ContextMenu("Spawn Target Level")]
        public void SpawnTargetLevel()
        {
            SpawnLevel(targetLevel);
        }

        public void SpawnLevel(int levelIndex)
        {
            targetLevel = Mathf.Clamp(levelIndex, 1, 30);
            var lvl = LevelManager.Instance ?? FindFirstObjectByType<LevelManager>();
            if (lvl != null)
            {
                lvl.LoadLevelDirect(targetLevel);
                Debug.Log($"[AdminManager] Successfully spawned Level {targetLevel}!");
            }
            else
            {
                Debug.LogWarning("[AdminManager] LevelManager not found in scene!");
            }
        }

        [ContextMenu("Reload Current Level")]
        public void ReloadCurrentLevel()
        {
            var lvl = LevelManager.Instance ?? FindFirstObjectByType<LevelManager>();
            if (lvl != null)
            {
                lvl.LoadLevelDirect(lvl.CurrentLevelIndex);
            }
        }

        [ContextMenu("Add +10 Balls to Mortar")]
        public void AddTenBalls()
        {
            var mortar = FindFirstObjectByType<MortarController>();
            if (mortar != null)
            {
                mortar.SetBallCount(mortar.RemainingBalls + 10);
                Debug.Log($"[AdminManager] Mortar ammo increased to {mortar.RemainingBalls}.");
            }
        }

        [ContextMenu("Win Level Now")]
        public void WinLevelNow()
        {
            var objects = FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None);
            foreach (var target in objects)
            {
                if (target != null && target.CurrentState != ObjectState.Destroyed && target.CurrentState != ObjectState.Destroying && target.CurrentState != ObjectState.DestroyCondition)
                {
                    target.TriggerDestroyCondition("Admin_InstantWin");
                }
            }
        }

        [ContextMenu("Fail Level Now")]
        public void FailLevelNow()
        {
            var mortar = FindFirstObjectByType<MortarController>();
            if (mortar != null)
            {
                mortar.SetBallCount(0);
            }
        }

        // =========================================================================
        // PLAYER STATS & ECONOMY
        // =========================================================================

        public void ApplyUsername()
        {
            if (ProfileManager.Instance != null)
            {
                ProfileManager.Instance.SetUsername(playerUsername);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Username = playerUsername;
                SaveManager.Instance.Save();
            }
        }

        public void SetGoldAmount(int amount)
        {
            gold = Mathf.Max(0, amount);
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.SetGold(gold);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Gold = gold;
                SaveManager.Instance.Save();
            }
        }

        public void AddGoldAmount(int amount)
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(amount, "AdminManager");
                gold = GoldManager.Instance.CurrentGold;
            }
            else
            {
                SetGoldAmount(gold + amount);
            }
        }

        public void SetLivesCount(int count)
        {
            lives = Mathf.Clamp(count, 0, 5);
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.SetLives(lives);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.Lives = lives;
                SaveManager.Instance.Save();
            }
        }

        public void RefillLives()
        {
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.RefillLives();
                lives = LivesManager.Instance.CurrentLives;
            }
            else
            {
                SetLivesCount(5);
            }
        }

        public void ToggleInfiniteLives()
        {
            infiniteLives = !infiniteLives;
            LivesManager.InfiniteLives = infiniteLives;
            Debug.Log($"[AdminManager] Infinite Lives: {infiniteLives}");
        }

        // =========================================================================
        // SKILLS
        // =========================================================================

        public void SetMegaUses(int count)
        {
            megaBallUses = Mathf.Max(0, count);
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.SetRemainingUses(SkillType.MegaBall, megaBallUses);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.MegaBallUses = megaBallUses;
                SaveManager.Instance.Save();
            }
        }

        public void AddMegaUses(int count)
        {
            SetMegaUses(megaBallUses + count);
        }

        public void SetMegaUnlocked(bool unlocked)
        {
            megaBallUnlocked = unlocked;
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.SetSkillUnlocked(SkillType.MegaBall, unlocked);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.MegaBallUnlocked = unlocked;
                SaveManager.Instance.Save();
            }
        }

        public void SetInfinityUses(int count)
        {
            infinityBallUses = Mathf.Max(0, count);
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.SetRemainingUses(SkillType.InfinityBall, infinityBallUses);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.InfinityBallUses = infinityBallUses;
                SaveManager.Instance.Save();
            }
        }

        public void AddInfinityUses(int count)
        {
            SetInfinityUses(infinityBallUses + count);
        }

        public void SetInfinityUnlocked(bool unlocked)
        {
            infinityBallUnlocked = unlocked;
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.SetSkillUnlocked(SkillType.InfinityBall, unlocked);
            }
            else if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.InfinityBallUnlocked = unlocked;
                SaveManager.Instance.Save();
            }
        }

        // =========================================================================
        // SAVE PERSISTENCE
        // =========================================================================

        public void SaveStatsToDisk()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (save != null)
            {
                save.Username = playerUsername;
                save.Gold = gold;
                save.Lives = lives;
                save.MegaBallUses = megaBallUses;
                save.MegaBallUnlocked = megaBallUnlocked;
                save.InfinityBallUses = infinityBallUses;
                save.InfinityBallUnlocked = infinityBallUnlocked;
                save.CurrentLevel = targetLevel;
                SaveManager.Instance.Save();
                Debug.Log("[AdminManager] All stats saved to disk!");
            }
        }

        public void ReloadStatsFromDisk()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Load();
                SyncFieldsFromRuntime();
                Debug.Log("[AdminManager] Stats reloaded from disk.");
            }
        }

        public void ResetSaveToDefaults()
        {
            if (SaveManager.Instance != null)
            {
                var newSave = new PlayerSaveData();
                typeof(SaveManager).GetField("_data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(SaveManager.Instance, newSave);
                SaveManager.Instance.Save();
                SyncFieldsFromRuntime();
                Debug.Log("[AdminManager] Save data reset to brand new defaults!");
            }
        }

        // =========================================================================
        // IN-GAME DEBUG GUI
        // =========================================================================

        private void OnGUI()
        {
            if (!showInGameGUI) return;

            // Small button in top-left to toggle panel on device or editor
            if (!_guiExpanded)
            {
                if (GUI.Button(new Rect(10, 10, 80, 32), "ADMIN"))
                {
                    _guiExpanded = true;
                    SyncFieldsFromRuntime();
                }
                return;
            }

            _windowRect = GUI.Window(99991, _windowRect, DrawAdminWindow, "👑 Royal Smash Admin Panel");
        }

        private void DrawAdminWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Close button
            if (GUILayout.Button("✕ Close Panel [F1 / ~]"))
            {
                _guiExpanded = false;
            }

            _guiScrollPos = GUILayout.BeginScrollView(_guiScrollPos);

            // --- LEVEL SPAWNER ---
            GUILayout.Label("<b>--- MAP & LEVEL TESTING ---</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Prev"))
            {
                targetLevel = Mathf.Max(1, targetLevel - 1);
                SpawnTargetLevel();
            }
            GUILayout.Label($"<b>Level: {targetLevel}</b>", GUILayout.Width(70));
            if (GUILayout.Button("Next ▶"))
            {
                targetLevel = Mathf.Min(30, targetLevel + 1);
                SpawnTargetLevel();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lvl 1")) SpawnLevel(1);
            if (GUILayout.Button("Lvl 5")) SpawnLevel(5);
            if (GUILayout.Button("Lvl 10")) SpawnLevel(10);
            if (GUILayout.Button("Lvl 15")) SpawnLevel(15);
            if (GUILayout.Button("Lvl 20")) SpawnLevel(20);
            if (GUILayout.Button("Lvl 30")) SpawnLevel(30);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("🔄 Reload Current Level")) ReloadCurrentLevel();

            // --- QUICK CHEATS ---
            GUILayout.Space(6);
            GUILayout.Label("<b>--- QUICK CHEATS ---</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("💣 +10 Balls")) AddTenBalls();
            if (GUILayout.Button("🏆 WIN LEVEL")) WinLevelNow();
            GUILayout.EndHorizontal();

            // --- ECONOMY & LIVES ---
            GUILayout.Space(6);
            GUILayout.Label("<b>--- ECONOMY & LIVES ---</b>");
            GUILayout.Label($"Gold: {gold}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+500 Gold")) AddGoldAmount(500);
            if (GUILayout.Button("+5000 Gold")) AddGoldAmount(5000);
            if (GUILayout.Button("10K Gold")) SetGoldAmount(10000);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Lives: {lives}/5");
            if (GUILayout.Button("Refill Lives")) RefillLives();
            if (GUILayout.Button(infiniteLives ? "Inf: ON" : "Inf: OFF")) ToggleInfiniteLives();
            GUILayout.EndHorizontal();

            // --- SKILLS ---
            GUILayout.Space(6);
            GUILayout.Label("<b>--- SKILLS ---</b>");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Mega: {megaBallUses}");
            if (GUILayout.Button("+5 Mega")) AddMegaUses(5);
            if (GUILayout.Button(megaBallUnlocked ? "Lock" : "Unlock")) SetMegaUnlocked(!megaBallUnlocked);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Infinity: {infinityBallUses}");
            if (GUILayout.Button("+5 Inf")) AddInfinityUses(5);
            if (GUILayout.Button(infinityBallUnlocked ? "Lock" : "Unlock")) SetInfinityUnlocked(!infinityBallUnlocked);
            GUILayout.EndHorizontal();

            // --- SAVE ---
            GUILayout.Space(6);
            GUILayout.Label("<b>--- SAVE CONTROLS ---</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("💾 Save Disk")) SaveStatsToDisk();
            if (GUILayout.Button("🔄 Reload")) ReloadStatsFromDisk();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("⚠️ Reset Save to Defaults")) ResetSaveToDefaults();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 30));
        }
    }
}
