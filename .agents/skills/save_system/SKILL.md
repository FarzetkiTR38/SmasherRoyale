---
name: Save System & Data Persistence
description: >
  Complete save system patterns for mobile Unity games. Covers JSON serialization,
  local persistence, cloud sync preparation, anti-cheat basics, idempotent operations,
  life regeneration timer handling, and app lifecycle management (pause/quit detection).
---

# Save System & Data Persistence Skill

## Applies To
All data persistence, player progress, economy transactions, life management, and app lifecycle handling.

---

## 1. Save Data Structure

```csharp
namespace RoyalSmash.Save
{
    /// <summary>
    /// Complete player save data. Serialized to JSON.
    /// Add new fields at the END to maintain backward compatibility.
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        // --- Version ---
        public int SaveVersion = 1;
        
        // --- Progression ---
        public int CurrentLevel = 1;
        public bool TutorialCompleted = false;
        
        // --- Economy ---
        public int Gold = 1000; // Overridden by GameConfig.StartingGold
        
        // --- Lives ---
        public int Lives = 5;
        public long LifeTimerStartTicks; // DateTime.UtcNow.Ticks when regen started
        
        // --- Skills ---
        public bool MegaBallUnlocked = false;
        public bool InfinityBallUnlocked = false;
        public int MegaBallUses = 0;
        public int InfinityBallUses = 0;
        
        // --- Profile ---
        public string Username = "";
        public int SelectedAvatarId = 0;
        public int SelectedFrameId = 0;
        
        // --- Settings ---
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;
        public bool VibrationEnabled = true;
        
        // --- Purchases ---
        public bool NoAdsPurchased = false;
        
        // --- App Lifecycle ---
        public bool WasInActiveLevel = false; // For app-close life deduction
        public int ActiveLevelId = -1;        // Which level was active
        
        // --- Cloud ---
        public long LastCloudSyncTicks = 0;
    }
}
```

---

## 2. Save Manager Implementation

```csharp
namespace RoyalSmash.Save
{
    /// <summary>
    /// Handles all local save/load operations.
    /// Uses JSON serialization to Application.persistentDataPath.
    /// </summary>
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        private const string SAVE_FILE_NAME = "player_save.json";
        private const string BACKUP_FILE_NAME = "player_save_backup.json";
        
        private PlayerSaveData _data;
        public PlayerSaveData Data => _data;
        
        private string SaveFilePath => 
            Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private string BackupFilePath => 
            Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);
        
        protected override void Awake()
        {
            base.Awake();
            Load();
        }
        
        /// <summary>
        /// Save current data to disk. Call at critical state changes.
        /// </summary>
        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                
                // Write backup first (in case write fails mid-way)
                if (File.Exists(SaveFilePath))
                    File.Copy(SaveFilePath, BackupFilePath, true);
                
                File.WriteAllText(SaveFilePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load from disk. Falls back to backup, then defaults.
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _data = JsonUtility.FromJson<PlayerSaveData>(json);
                }
                else if (File.Exists(BackupFilePath))
                {
                    string json = File.ReadAllText(BackupFilePath);
                    _data = JsonUtility.FromJson<PlayerSaveData>(json);
                    Debug.LogWarning("[SaveManager] Loaded from backup.");
                }
                else
                {
                    _data = CreateDefaultSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}");
                _data = CreateDefaultSave();
            }
            
            // Handle app-close life deduction from previous session
            HandlePreviousSessionClose();
        }
        
        private PlayerSaveData CreateDefaultSave()
        {
            return new PlayerSaveData(); // Uses default field values
        }
        
        /// <summary>
        /// If the previous session ended while in an active level,
        /// deduct 1 life (app-close = exit = -1 life).
        /// </summary>
        private void HandlePreviousSessionClose()
        {
            if (_data.WasInActiveLevel)
            {
                _data.Lives = Mathf.Max(0, _data.Lives - 1);
                _data.WasInActiveLevel = false;
                _data.ActiveLevelId = -1;
                Save();
                Debug.Log("[SaveManager] Deducted 1 life for previous session app-close.");
            }
        }
    }
}
```

---

## 3. Critical Save Points

ALWAYS save at these moments:
```csharp
// Level completed (win)
SaveManager.Instance.Data.CurrentLevel++;
SaveManager.Instance.Data.Gold += totalReward;
SaveManager.Instance.Save();

// Level failed (lose)
SaveManager.Instance.Data.Lives--;
SaveManager.Instance.Save();

// Level exit (confirmed)
SaveManager.Instance.Data.Lives--;
SaveManager.Instance.Data.WasInActiveLevel = false;
SaveManager.Instance.Save();

// Entering active level
SaveManager.Instance.Data.WasInActiveLevel = true;
SaveManager.Instance.Data.ActiveLevelId = levelId;
SaveManager.Instance.Save();

// Leaving active level (normal completion)
SaveManager.Instance.Data.WasInActiveLevel = false;
SaveManager.Instance.Save();

// Gold spent (purchase)
SaveManager.Instance.Data.Gold -= cost;
SaveManager.Instance.Save();

// Skill purchased
SaveManager.Instance.Data.MegaBallUses += amount;
SaveManager.Instance.Save();

// Profile changed
SaveManager.Instance.Data.Username = newName;
SaveManager.Instance.Save();

// Settings changed
SaveManager.Instance.Data.MusicEnabled = value;
SaveManager.Instance.Save();
```

---

## 4. App Lifecycle Handling

```csharp
/// <summary>
/// Handles app pause/quit to detect active level app-close.
/// Attach to a persistent GameObject.
/// </summary>
public class AppLifecycleHandler : MonoBehaviour
{
    /// <summary>
    /// Called when app goes to background (mobile) or loses focus.
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // App going to background — save immediately
            SaveManager.Instance.Save();
        }
        else
        {
            // App returning to foreground — process life regen
            LivesManager.Instance?.ProcessRegeneration();
        }
    }
    
    /// <summary>
    /// Called when app is about to quit.
    /// On mobile, this is NOT always called — rely on OnApplicationPause too.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveManager.Instance.Save();
    }
}
```

**CRITICAL**: On mobile, `OnApplicationQuit` is NOT reliably called. The `WasInActiveLevel` flag is the safety net — set it true when entering a level, false when leaving normally.

---

## 5. Life Regeneration Timer

```csharp
/// <summary>
/// Handles offline life regeneration.
/// Uses DateTime.UtcNow.Ticks for time tracking (survives app close).
/// </summary>
public class LivesManager : PersistentSingleton<LivesManager>
{
    private GameConfig _config; // Inject or find
    
    /// <summary>
    /// Call on app launch and when returning from background.
    /// Calculates how many lives should have regenerated while away.
    /// </summary>
    public void ProcessRegeneration()
    {
        var data = SaveManager.Instance.Data;
        
        if (data.Lives >= _config.MaxLives)
        {
            data.LifeTimerStartTicks = 0; // No regen needed
            return;
        }
        
        if (data.LifeTimerStartTicks == 0)
        {
            // Start timer now
            data.LifeTimerStartTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            return;
        }
        
        // Calculate elapsed time
        long elapsedTicks = DateTime.UtcNow.Ticks - data.LifeTimerStartTicks;
        TimeSpan elapsed = new TimeSpan(elapsedTicks);
        double elapsedMinutes = elapsed.TotalMinutes;
        
        int livesRegained = (int)(elapsedMinutes / _config.LifeRegenMinutes);
        
        if (livesRegained > 0)
        {
            data.Lives = Mathf.Min(data.Lives + livesRegained, _config.MaxLives);
            
            // Keep remainder time for next regen
            double remainderMinutes = elapsedMinutes % _config.LifeRegenMinutes;
            data.LifeTimerStartTicks = DateTime.UtcNow.Ticks 
                - (long)(remainderMinutes * TimeSpan.TicksPerMinute);
            
            if (data.Lives >= _config.MaxLives)
                data.LifeTimerStartTicks = 0; // Stop timer at max
            
            SaveManager.Instance.Save();
        }
    }
    
    /// <summary>
    /// Get remaining time until next life regeneration.
    /// Returns TimeSpan.Zero if lives are full.
    /// </summary>
    public TimeSpan GetTimeUntilNextLife()
    {
        var data = SaveManager.Instance.Data;
        
        if (data.Lives >= _config.MaxLives || data.LifeTimerStartTicks == 0)
            return TimeSpan.Zero;
        
        long elapsedTicks = DateTime.UtcNow.Ticks - data.LifeTimerStartTicks;
        TimeSpan elapsed = new TimeSpan(elapsedTicks);
        double elapsedMinutes = elapsed.TotalMinutes;
        double remainderMinutes = elapsedMinutes % _config.LifeRegenMinutes;
        double minutesUntilNext = _config.LifeRegenMinutes - remainderMinutes;
        
        return TimeSpan.FromMinutes(minutesUntilNext);
    }
}
```

---

## 6. Idempotent Operations (Duplicate Prevention)

```csharp
/// <summary>
/// Ensures a one-time operation (like level reward) happens exactly once.
/// Uses a transaction ID to prevent duplicates across scene reloads, 
/// app restarts, and double-clicks.
/// </summary>
public class TransactionGuard
{
    private HashSet<string> _completedTransactions = new();
    
    /// <summary>
    /// Try to execute a one-time transaction. Returns true if allowed.
    /// </summary>
    public bool TryExecute(string transactionId)
    {
        if (_completedTransactions.Contains(transactionId))
        {
            Debug.LogWarning($"[TransactionGuard] Duplicate blocked: {transactionId}");
            return false;
        }
        _completedTransactions.Add(transactionId);
        return true;
    }
    
    public void Reset()
    {
        _completedTransactions.Clear();
    }
}

// Usage:
// string txId = $"level_reward_{levelId}_{sessionId}";
// if (_guard.TryExecute(txId))
//     GrantReward();
```

---

## 7. Cloud Sync Preparation

```csharp
/// <summary>
/// Cloud sync follows "latest local progress wins" rule.
/// This is a framework — actual backend implementation is configurable.
/// </summary>
public interface ICloudSyncProvider
{
    Task<bool> PushProgress(PlayerSaveData localData);
    Task<PlayerSaveData> PullProgress();
    Task<bool> IsOnline();
}

/// <summary>
/// Sync flow:
/// 1. Check if online
/// 2. Push local progress to cloud
/// 3. Pull latest leaderboard data (separate from progress)
/// 4. Cache leaderboard locally
/// </summary>
public class CloudSyncManager : PersistentSingleton<CloudSyncManager>
{
    private ICloudSyncProvider _provider;
    
    public async Task TrySyncProgress()
    {
        if (_provider == null || !await _provider.IsOnline())
            return;
        
        var localData = SaveManager.Instance.Data;
        bool success = await _provider.PushProgress(localData);
        
        if (success)
        {
            localData.LastCloudSyncTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
        }
    }
}
```

---

## 8. Anti-Exploit Patterns

```csharp
// 1. App close during level = life loss (handled by WasInActiveLevel flag)
// 2. Same reward not given twice (TransactionGuard)
// 3. Same life not deducted twice (check state before deduction)
// 4. Gold cannot go negative
// 5. Skill uses cannot go negative
// 6. Lives capped at max

public static class SafeEconomy
{
    public static bool TrySpendGold(int amount)
    {
        var data = SaveManager.Instance.Data;
        if (data.Gold < amount) return false;
        data.Gold -= amount;
        SaveManager.Instance.Save();
        return true;
    }
    
    public static void AddGold(int amount)
    {
        var data = SaveManager.Instance.Data;
        data.Gold += amount;
        SaveManager.Instance.Save();
    }
    
    public static bool TryDeductLife()
    {
        var data = SaveManager.Instance.Data;
        if (data.Lives <= 0) return false;
        data.Lives--;
        SaveManager.Instance.Save();
        return true;
    }
    
    public static bool TryUseSkill(SkillType type)
    {
        var data = SaveManager.Instance.Data;
        ref int uses = ref (type == SkillType.MegaBall 
            ? ref data.MegaBallUses 
            : ref data.InfinityBallUses);
        
        if (uses <= 0) return false;
        uses--;
        SaveManager.Instance.Save();
        return true;
    }
}
```

---

## 9. Save File Migration

```csharp
/// <summary>
/// Handle save file version upgrades when adding new fields.
/// </summary>
public static class SaveMigration
{
    private const int CURRENT_VERSION = 1;
    
    public static PlayerSaveData Migrate(PlayerSaveData data)
    {
        if (data.SaveVersion < CURRENT_VERSION)
        {
            // Add migration steps as version increases
            // if (data.SaveVersion < 2) MigrateV1ToV2(data);
            // if (data.SaveVersion < 3) MigrateV2ToV3(data);
            
            data.SaveVersion = CURRENT_VERSION;
        }
        return data;
    }
}
```
