---
name: Level Design & Data-Driven Architecture
description: >
  Patterns for data-driven level creation using ScriptableObjects, level loading,
  progressive difficulty design, object placement strategies, and the 30-level
  MVP content structure with milestone events (skill unlocks, new content).
---

# Level Design & Data-Driven Architecture Skill

## Applies To
All level creation, loading, progression, and content management in this project.

---

## 1. Core Principle

Levels MUST be **data-driven**, not hardcoded. Adding a new level should require:
1. Creating a LevelData asset
2. Placing objects and configuring parameters
3. NO new code

---

## 2. LevelData ScriptableObject

```csharp
namespace RoyalSmash.Levels
{
    [CreateAssetMenu(fileName = "LevelData_XX", menuName = "RoyalSmash/Levels/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelId;
        public string LevelName;
        
        [Header("Gameplay")]
        public int BallCount = 25;
        
        [Header("Stand")]
        public StandConfig[] Stands;
        
        [Header("Objects")]
        public ObjectPlacement[] Objects;
        
        [Header("Environment")]
        public BackgroundType Background = BackgroundType.Default;
        
        [Header("Progression Events")]
        public LevelEvent[] SpecialEvents;
        
        [Header("Tutorial")]
        public bool IsTutorialLevel = false;
        public Vector3 TutorialTargetPosition;
        public float TutorialHighlightRadius = 1f;
    }
    
    [System.Serializable]
    public class StandConfig
    {
        public Vector3 Position = new Vector3(0, 2, 5);
        public Vector3 Scale = new Vector3(4, 0.3f, 2);
        public Vector3 Rotation = Vector3.zero;
    }
    
    [System.Serializable]
    public class ObjectPlacement
    {
        public ObjectType Type;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
        
        [Header("Optional Overrides")]
        public bool OverrideMass = false;
        public float CustomMass = 1f;
    }
    
    public enum ObjectType
    {
        Box,
        Stone,
        Column,
        Cone,
        PaintBottle
    }
    
    public enum BackgroundType
    {
        Default,
        Sunset,
        Night,
        Garden,
        Industrial
    }
    
    [System.Serializable]
    public class LevelEvent
    {
        public LevelEventType Type;
        public string Data; // Flexible string data for event
    }
    
    public enum LevelEventType
    {
        None,
        UnlockMegaBall,         // Level 5
        UnlockInfinityBall,     // Level 10
        PromptUsername,          // Level 10
        NewContentIntroduction, // Every 5 levels
        FinalLevel              // Level 30
    }
}
```

---

## 3. Level Database

```csharp
/// <summary>
/// Central registry of all level data. Used by LevelManager to load levels.
/// </summary>
[CreateAssetMenu(fileName = "LevelDatabase", menuName = "RoyalSmash/Levels/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private LevelData[] _levels;
    
    public int TotalLevels => _levels.Length;
    
    public LevelData GetLevel(int levelId)
    {
        // LevelId is 1-based
        int index = levelId - 1;
        if (index >= 0 && index < _levels.Length)
            return _levels[index];
        
        Debug.LogError($"[LevelDatabase] Level {levelId} not found!");
        return null;
    }
    
    public bool IsLastLevel(int levelId)
    {
        return levelId >= _levels.Length; // Level 30 = last
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Auto-assign levels from folder")]
    private void AutoAssignLevels()
    {
        // Editor utility to bulk-assign LevelData assets
        _levels = new LevelData[30];
        for (int i = 0; i < 30; i++)
        {
            string path = $"Assets/_Project/ScriptableObjects/Levels/LevelData_{(i+1):D2}";
            _levels[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(path + ".asset");
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    #endif
}
```

---

## 4. Level Loader

```csharp
/// <summary>
/// Instantiates a level from LevelData. Spawns stand, objects, sets ball count.
/// </summary>
public class LevelLoader : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject _standPrefab;
    [SerializeField] private GameObject _boxPrefab;
    [SerializeField] private GameObject _stonePrefab;
    [SerializeField] private GameObject _columnPrefab;
    [SerializeField] private GameObject _conePrefab;
    [SerializeField] private GameObject _paintBottlePrefab;
    
    [SerializeField] private Transform _levelRoot; // Parent for spawned objects
    
    private List<PhysicsObject> _spawnedObjects = new();
    
    public List<PhysicsObject> SpawnedObjects => _spawnedObjects;
    
    public void LoadLevel(LevelData data)
    {
        ClearLevel();
        
        // Spawn stands
        foreach (var standConfig in data.Stands)
        {
            var stand = Instantiate(_standPrefab, _levelRoot);
            stand.transform.position = standConfig.Position;
            stand.transform.eulerAngles = standConfig.Rotation;
            stand.transform.localScale = standConfig.Scale;
        }
        
        // Spawn objects
        foreach (var placement in data.Objects)
        {
            GameObject prefab = GetPrefabForType(placement.Type);
            var obj = Instantiate(prefab, _levelRoot);
            obj.transform.position = placement.Position;
            obj.transform.eulerAngles = placement.Rotation;
            obj.transform.localScale = placement.Scale;
            
            var physObj = obj.GetComponent<PhysicsObject>();
            if (placement.OverrideMass)
                physObj.OverrideMass(placement.CustomMass);
            
            _spawnedObjects.Add(physObj);
        }
    }
    
    public void ClearLevel()
    {
        foreach (Transform child in _levelRoot)
            Destroy(child.gameObject);
        _spawnedObjects.Clear();
    }
    
    private GameObject GetPrefabForType(ObjectType type)
    {
        return type switch
        {
            ObjectType.Box => _boxPrefab,
            ObjectType.Stone => _stonePrefab,
            ObjectType.Column => _columnPrefab,
            ObjectType.Cone => _conePrefab,
            ObjectType.PaintBottle => _paintBottlePrefab,
            _ => _boxPrefab
        };
    }
}
```

---

## 5. 30-Level Progression Map

### Level Content Structure (LOCKED)

```
Level 1:   Tutorial (first launch only). 3-4 Boxes, easy layout. 25 balls.
Level 2:   5-6 Boxes, simple stacking. 25 balls.
Level 3:   6-8 Boxes, first multi-layer hint. 25 balls.
Level 4:   8-10 Boxes, chain reaction introduction. 22 balls.
Level 5:   ★ NEW CONTENT + MEGA BALL UNLOCK. First Stone objects. 22 balls.
Level 6:   Mix of Boxes + Stones. 22 balls.
Level 7:   Stones blocking boxes. 20 balls.
Level 8:   First Column introduction. 20 balls.
Level 9:   Column topple puzzle. 20 balls.
Level 10:  ★ NEW CONTENT + INFINITY BALL UNLOCK + USERNAME PROMPT. First Cone. 20 balls.
Level 11:  Mix of all objects so far. 20 balls.
Level 12:  Two-stand layout begins. 18 balls.
Level 13:  Complex stacking with Stones. 18 balls.
Level 14:  Chain reaction focus. 18 balls.
Level 15:  ★ NEW CONTENT. First PaintBottle. 18 balls.
Level 16:  PaintBottle mixed with other objects. 18 balls.
Level 17:  Two-layer (front + back row). 16 balls.
Level 18:  Heavy stone fortress. 16 balls.
Level 19:  Precision targeting challenge. 16 balls.
Level 20:  ★ NEW CONTENT. Complex multi-type combination. 16 balls.
Level 21:  Large structure with chain reactions. 15 balls.
Level 22:  Two-stand with mixed objects. 15 balls.
Level 23:  Column domino puzzle. 15 balls.
Level 24:  Skill-dependent challenge. 14 balls.
Level 25:  ★ NEW CONTENT. Maximum object variety. 14 balls.
Level 26:  Demanding precision + chain reactions. 14 balls.
Level 27:  Two-layer complex fortress. 12 balls.
Level 28:  All object types combined. 12 balls.
Level 29:  Pre-final challenge. 12 balls.
Level 30:  ★ FINAL LEVEL. Epic combination puzzle. Win Panel: NO "Next Level". 12 balls.
```

### Difficulty Levers
- More objects per level
- Heavier objects (Stone, Column)
- More complex stacking/layering
- Two-row/layer layouts
- Fewer available balls
- Harder angles / obstructed targets
- Objects requiring chain reactions to reach

---

## 6. Level Progression Events Handler

```csharp
/// <summary>
/// Handles special events triggered at milestone levels.
/// </summary>
public class LevelProgressionHandler : MonoBehaviour
{
    public void ProcessLevelEvents(LevelData levelData, int completedLevelId)
    {
        if (levelData.SpecialEvents == null) return;
        
        foreach (var evt in levelData.SpecialEvents)
        {
            switch (evt.Type)
            {
                case LevelEventType.UnlockMegaBall:
                    UnlockMegaBall();
                    break;
                case LevelEventType.UnlockInfinityBall:
                    UnlockInfinityBall();
                    break;
                case LevelEventType.PromptUsername:
                    ShowUsernamePrompt();
                    break;
                case LevelEventType.FinalLevel:
                    HandleFinalLevel();
                    break;
            }
        }
    }
    
    private void UnlockMegaBall()
    {
        var data = SaveManager.Instance.Data;
        if (!data.MegaBallUnlocked)
        {
            data.MegaBallUnlocked = true;
            data.MegaBallUses = 3; // Starting uses
            SaveManager.Instance.Save();
            // Show unlock UI notification
        }
    }
    
    private void UnlockInfinityBall()
    {
        var data = SaveManager.Instance.Data;
        if (!data.InfinityBallUnlocked)
        {
            data.InfinityBallUnlocked = true;
            data.InfinityBallUses = 3;
            SaveManager.Instance.Save();
        }
    }
    
    private void ShowUsernamePrompt()
    {
        if (string.IsNullOrEmpty(SaveManager.Instance.Data.Username))
        {
            // Show username input panel
        }
    }
    
    private void HandleFinalLevel()
    {
        // Win Panel shows but NO "Next Level" button
        // Player returns to Main Menu
    }
}
```

---

## 7. Level Validation (Editor Tool)

```csharp
#if UNITY_EDITOR
/// <summary>
/// Editor utility to validate all 30 levels have required data.
/// </summary>
public static class LevelValidator
{
    [UnityEditor.MenuItem("RoyalSmash/Validate All Levels")]
    public static void ValidateAllLevels()
    {
        var database = Resources.Load<LevelDatabase>("LevelDatabase");
        if (database == null)
        {
            Debug.LogError("LevelDatabase not found!");
            return;
        }
        
        for (int i = 1; i <= 30; i++)
        {
            var level = database.GetLevel(i);
            if (level == null)
            {
                Debug.LogError($"Level {i}: MISSING!");
                continue;
            }
            
            if (level.BallCount <= 0)
                Debug.LogWarning($"Level {i}: BallCount is {level.BallCount}");
            
            if (level.Objects == null || level.Objects.Length == 0)
                Debug.LogWarning($"Level {i}: No objects placed!");
            
            if (level.Stands == null || level.Stands.Length == 0)
                Debug.LogWarning($"Level {i}: No stand configured!");
        }
        
        Debug.Log("Level validation complete.");
    }
}
#endif
```

---

## 8. Level Completion Detection

```csharp
/// <summary>
/// Monitors active physics objects to detect win/lose conditions.
/// </summary>
public class LevelCompletionDetector : MonoBehaviour
{
    private List<PhysicsObject> _trackedObjects;
    private int _remainingBalls;
    private bool _levelEnded;
    
    // Object is "cleared" when:
    // - Destroyed (state = Destroyed)
    // - Fallen below kill zone (y < threshold)
    // - Off-stand (physics check)
    
    private bool AreAllObjectsCleared()
    {
        foreach (var obj in _trackedObjects)
        {
            if (obj.CurrentState != ObjectState.Destroyed 
                && obj.transform.position.y > _killZoneY
                && obj.gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }
    
    private void CheckCompletion()
    {
        if (_levelEnded) return;
        
        if (AreAllObjectsCleared())
        {
            _levelEnded = true;
            StartCoroutine(VictorySequence());
        }
        else if (_remainingBalls <= 0 && AreAllPhysicsSettled())
        {
            _levelEnded = true;
            StartCoroutine(LoseSequence());
        }
    }
}
```
