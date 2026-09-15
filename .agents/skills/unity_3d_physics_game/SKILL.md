---
name: Unity 3D Physics Game Development
description: >
  Comprehensive Unity 3D game development skill for physics-based puzzle games.
  Covers C# coding conventions, physics system best practices, 3D projectile mechanics,
  object interaction patterns, mobile optimization, ScriptableObject-based data architecture,
  and Unity-specific gotchas for 3D mobile games.
---

# Unity 3D Physics Game Development Skill

## Applies To
All Unity 3D C# code in this project. This game is a **3D physics-based puzzle** with projectile mechanics, object destruction, and mobile touch input.

---

## 1. C# Coding Conventions

### Naming
```csharp
// PascalCase for: classes, methods, properties, public fields, events
public class ProjectileController : MonoBehaviour { }
public float LaunchSpeed { get; private set; }
public event Action<int> OnBallFired;

// camelCase for: private fields (with underscore prefix), local variables, parameters
private float _fireCooldown;
private Rigidbody _rigidbody;
private int _remainingBalls;

// UPPER_CASE for: constants
private const float MAX_BALL_LIFETIME = 10f;
private const int DEFAULT_BALL_COUNT = 25;

// Suffix conventions
// Manager — global singleton systems (GameManager, AudioManager)
// Controller — per-object behavior (ProjectileController, MortarController)
// Data — ScriptableObject data containers (LevelData, ObjectData)
// Config — configuration ScriptableObjects (GameConfig, PhysicsConfig)
// System — stateless utility systems (RewardSystem, SaveSystem)
// Handler — event/callback responders (InputHandler, CollisionHandler)
```

### File Structure
```csharp
// Every script file follows this order:
using UnityEngine;           // Unity namespaces first
using UnityEngine.UI;
using TMPro;                 // Third-party
using System;                // System namespaces
using System.Collections;
using System.Collections.Generic;
using RoyalSmash.Core;       // Project namespaces last

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Brief class description.
    /// </summary>
    public class ExampleClass : MonoBehaviour
    {
        // 1. Constants
        // 2. Serialized fields (inspector-visible)
        // 3. Private fields
        // 4. Properties
        // 5. Events
        // 6. Unity lifecycle (Awake, Start, OnEnable, OnDisable, Update, etc.)
        // 7. Public methods
        // 8. Private methods
        // 9. Coroutines
    }
}
```

### Serialized Fields Pattern
```csharp
[Header("Projectile Settings")]
[SerializeField] private float _launchSpeed = 20f;
[SerializeField] private float _lifetime = 5f;

[Header("References")]
[SerializeField] private Rigidbody _rigidbody;
[SerializeField] private ParticleSystem _trailEffect;

[Header("Configuration")]
[SerializeField] private ProjectileConfig _config;
```

---

## 2. Physics System Best Practices

### Rigidbody Usage (3D ONLY)
```csharp
// CORRECT — Use 3D physics components
private Rigidbody _rb;
private SphereCollider _collider;

// WRONG — NEVER use 2D physics in this project
// private Rigidbody2D _rb2d;    ❌
// private CircleCollider2D _col; ❌
```

### Physics Layers
```
Layer 6: "Projectile"    — Balls
Layer 7: "PhysicsObject"  — Box, Stone, Column, Cone, PaintBottle
Layer 8: "Stand"          — Static platforms
Layer 9: "Ground"         — Ground/floor plane
Layer 10: "Boundary"      — Kill zones / world boundaries
```

Configure Layer Collision Matrix to disable unnecessary collision pairs.

### Force Application
```csharp
// For projectile launch — use velocity directly for predictable results
_rb.velocity = launchDirection * launchSpeed;

// For impact impulse — use ForceMode.Impulse
targetRb.AddForce(impactForce, ForceMode.Impulse);

// For continuous push — use ForceMode.Force (in FixedUpdate)
_rb.AddForce(Vector3.down * gravity, ForceMode.Force);

// NEVER apply forces in Update() — always use FixedUpdate()
```

### Physics Materials
```csharp
// Create PhysicMaterial assets for different object types:
// Ball:         Bounciness=0.4, Friction=0.3
// Box:          Bounciness=0.1, Friction=0.5
// Stone:        Bounciness=0.05, Friction=0.8
// Stand:        Bounciness=0.0, Friction=0.9
// These are EXAMPLE values — expose via config
```

### Collision Detection
```csharp
// For fast-moving projectiles, use Continuous collision detection
_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

// For static objects (Stand), use Discrete (default)
// For physics objects (Box, Stone), use ContinuousDynamic if needed
```

### Physics Settling Detection
```csharp
/// <summary>
/// Check if all physics objects have settled (stopped moving).
/// Used for win condition detection.
/// </summary>
private bool AreAllObjectsSettled()
{
    foreach (var obj in _activeObjects)
    {
        if (obj.Rigidbody.velocity.sqrMagnitude > SETTLE_THRESHOLD)
            return false;
        if (obj.Rigidbody.angularVelocity.sqrMagnitude > SETTLE_THRESHOLD)
            return false;
    }
    return true;
}
```

---

## 3. World-Coordinate Targeting (CRITICAL)

This is the most important mechanic in the game. Implement it correctly:

```csharp
/// <summary>
/// Convert screen touch position to world-space target point.
/// Uses camera ray + plane intersection or raycast.
/// </summary>
private Vector3 GetWorldTargetFromScreenPoint(Vector2 screenPosition)
{
    Ray ray = Camera.main.ScreenPointToRay(screenPosition);
    
    // Option A: Raycast against a target plane at gameplay depth
    Plane targetPlane = new Plane(Vector3.forward, _targetPlaneDistance);
    if (targetPlane.Raycast(ray, out float distance))
    {
        return ray.GetPoint(distance);
    }
    
    // Option B: Raycast against physics objects / colliders
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _targetLayerMask))
    {
        return hit.point;
    }
    
    return ray.GetPoint(_defaultTargetDistance);
}

/// <summary>
/// Calculate launch velocity to hit target point from mortar position.
/// Accounts for gravity and desired trajectory arc.
/// </summary>
private Vector3 CalculateLaunchVelocity(Vector3 origin, Vector3 target, float speed)
{
    Vector3 direction = (target - origin).normalized;
    return direction * speed;
    
    // For more advanced arc trajectory, use kinematic equations:
    // Consider gravity, launch angle, and distance
}
```

**NEVER** simplify this to angle-based aiming. The ball MUST go toward where the player tapped in world space.

---

## 4. Input System — Touch Handling

```csharp
/// <summary>
/// Distinguish between Tap (fire) and Drag (move mortar).
/// Key: Track total finger movement. If below threshold = Tap, else = Drag.
/// </summary>
private const float DRAG_THRESHOLD = 10f; // pixels

private Vector2 _touchStartPosition;
private bool _isDragging;

private void HandleTouchBegan(Vector2 position)
{
    _touchStartPosition = position;
    _isDragging = false;
}

private void HandleTouchMoved(Vector2 position)
{
    float distance = Vector2.Distance(_touchStartPosition, position);
    if (distance > DRAG_THRESHOLD)
    {
        _isDragging = true;
        MoveMortar(position); // Drag = move mortar
    }
}

private void HandleTouchEnded(Vector2 position)
{
    if (!_isDragging)
    {
        FireBall(position); // Tap = fire at world coordinate
    }
    _isDragging = false;
}
```

**Critical Rule**: Drag must NEVER accidentally fire. Use a pixel threshold to distinguish.

---

## 5. Object System — Base Class Pattern

```csharp
namespace RoyalSmash.Objects
{
    /// <summary>
    /// Base class for all destructible physics objects on the stand.
    /// </summary>
    public abstract class PhysicsObject : MonoBehaviour
    {
        [Header("Object Configuration")]
        [SerializeField] protected ObjectData _objectData;
        
        protected Rigidbody _rigidbody;
        protected ObjectState _currentState = ObjectState.Idle;
        
        public ObjectState CurrentState => _currentState;
        public event Action<PhysicsObject> OnDestroyed;
        
        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.mass = _objectData.Mass;
        }
        
        public virtual void OnImpact(Vector3 force, float magnitude)
        {
            _currentState = ObjectState.Hit;
            // Override in subclasses for specific behavior
        }
        
        protected virtual void BeginDestruction()
        {
            _currentState = ObjectState.Destroying;
            StartCoroutine(DestroySequence());
        }
        
        /// <summary>
        /// Override this to define object-specific destruction animation.
        /// Objects must NOT pop instantly — they should shrink/melt/fade.
        /// </summary>
        protected abstract IEnumerator DestroySequence();
        
        protected void FinalizeDestruction()
        {
            _currentState = ObjectState.Destroyed;
            OnDestroyed?.Invoke(this);
            gameObject.SetActive(false); // Return to pool or destroy
        }
    }
    
    public enum ObjectState
    {
        Idle,
        Hit,
        Moving,
        Falling,
        Breaking,     // PaintBottle specific
        Destroying,
        Destroyed
    }
}
```

### Paint Bottle Special Case
```csharp
public class PaintBottle : PhysicsObject
{
    [SerializeField] private ParticleSystem _glassBreakVFX;
    [SerializeField] private ParticleSystem _paintSpillVFX;
    [SerializeField] private Color _paintColor = Color.red;
    
    public override void OnImpact(Vector3 force, float magnitude)
    {
        base.OnImpact(force, magnitude);
        if (magnitude >= _objectData.BreakThreshold)
        {
            _currentState = ObjectState.Breaking;
            StartCoroutine(BreakSequence());
        }
    }
    
    private IEnumerator BreakSequence()
    {
        // 1. Glass break VFX
        _glassBreakVFX.Play();
        AudioManager.Instance.PlaySFX("glass_break");
        
        // 2. Paint spill VFX  
        var main = _paintSpillVFX.main;
        main.startColor = _paintColor;
        _paintSpillVFX.Play();
        
        yield return new WaitForSeconds(0.5f);
        
        // 3. Fade and destroy
        BeginDestruction();
    }
}
```

---

## 6. ScriptableObject Architecture

### Configuration Pattern
```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "RoyalSmash/Config/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Projectile")]
    public float FireCooldown = 0.5f;
    public float BallLifetime = 5f;
    public float BallSpeed = 20f;
    public float MegaBallDiameterMultiplier = 2f;
    public float InfinityBallDuration = 5f;
    
    [Header("Economy")]
    public int StartingGold = 1000;
    public int BaseLevelReward = 50;
    public int RemainingBallMultiplier = 2;
    public int LifeGoldPrice = 100;
    
    [Header("Lives")]
    public int MaxLives = 5;
    public int StartingLives = 5;
    public float LifeRegenMinutes = 30f;
    
    [Header("Timing")]
    public float VictoryDelay = 2f;
}
```

### Level Data Pattern
```csharp
[CreateAssetMenu(fileName = "LevelData_XX", menuName = "RoyalSmash/Levels/Level Data")]
public class LevelData : ScriptableObject
{
    public int LevelId;
    public int BallCount = 25;
    public StandConfig StandConfiguration;
    public ObjectPlacement[] Objects;
    public BackgroundType Background;
    public string[] UnlockedFeatures;
    public bool HasTutorial;
    
    [System.Serializable]
    public class ObjectPlacement
    {
        public ObjectType Type;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }
    
    [System.Serializable]
    public class StandConfig
    {
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public int PlatformCount = 1;
    }
}

public enum ObjectType
{
    Box,
    Stone,
    Column,
    Cone,
    PaintBottle
}
```

---

## 7. Singleton Manager Pattern

```csharp
namespace RoyalSmash.Core
{
    /// <summary>
    /// Generic persistent singleton. Use sparingly — only for truly global managers.
    /// </summary>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            Debug.LogError($"[{typeof(T).Name}] No instance found in scene!");
                        }
                    }
                    return _instance;
                }
            }
        }
        
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

Use for: `GameManager`, `AudioManager`, `SaveManager`, `AdManager`.
Do NOT use for: per-level controllers, UI panels, gameplay objects.

---

## 8. Event System Pattern

```csharp
namespace RoyalSmash.Core
{
    /// <summary>
    /// Simple ScriptableObject-based event for decoupled communication.
    /// </summary>
    [CreateAssetMenu(menuName = "RoyalSmash/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> _listeners = new();
        
        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised();
        }
        
        public void RegisterListener(GameEventListener listener) => _listeners.Add(listener);
        public void UnregisterListener(GameEventListener listener) => _listeners.Remove(listener);
    }
}

// Usage examples:
// Create SO events: OnLevelWin, OnLevelLose, OnBallFired, OnObjectDestroyed, OnGoldChanged
// Wire them in Inspector — no hardcoded cross-references
```

---

## 9. Save System Pattern

```csharp
/// <summary>
/// Player save data — serialized to JSON, stored locally.
/// NEVER use PlayerPrefs for complex data.
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public int CurrentLevel = 1;
    public int Gold;
    public int Lives;
    public long LifeTimerTicks;     // DateTime.Ticks for regeneration
    public int MegaBallUses;
    public int InfinityBallUses;
    public bool MegaBallUnlocked;
    public bool InfinityBallUnlocked;
    public string Username;
    public int SelectedAvatarId;
    public int SelectedFrameId;
    public bool TutorialCompleted;
    public bool NoAdsPurchased;
    public bool MusicEnabled = true;
    public bool SfxEnabled = true;
    public bool VibrationEnabled = true;
    public bool IsInActiveLevel;     // For app-close life deduction
    public string LastSessionLevelState; // Track active level on close
}

// Save to: Application.persistentDataPath + "/save.json"
// Use JsonUtility.ToJson() / FromJson()
// Save at critical moments: level complete, level fail, purchases, profile changes
```

---

## 10. Object Pooling Pattern

```csharp
namespace RoyalSmash.Core
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();
        
        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            
            for (int i = 0; i < initialSize; i++)
                _pool.Enqueue(CreateInstance());
        }
        
        public T Get()
        {
            T instance = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
            instance.gameObject.SetActive(true);
            return instance;
        }
        
        public void Return(T instance)
        {
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
        
        private T CreateInstance()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
```

Use for: Balls/Projectiles, Impact VFX, Destruction VFX.

---

## 11. Mobile Optimization Guidelines

- **Target 60 FPS** on mid-range devices.
- **Limit active Rigidbodies**: Max ~50 active physics objects per scene.
- **Object Pooling**: Pool projectiles, VFX particles, temporary objects.
- **Avoid GC Allocation in Update**: No `new`, no LINQ, no string concatenation in Update/FixedUpdate.
- **Physics Settings**: Set `Fixed Timestep = 0.02` (50 Hz). Use appropriate `Solver Iterations`.
- **Texture Compression**: Use ASTC (Android) and PVRTC/ASTC (iOS).
- **UI Optimization**: Use `Canvas.ForceUpdateCanvases()` sparingly. Separate dynamic and static UI into different canvases.
- **Audio**: Use compressed audio for music, uncompressed for short SFX.
- **Particle Systems**: Limit max particles. Use `Stop Action = Disable` for pooled particles.

---

## 12. Common Gotchas & Pitfalls

### Physics
- ⚠️ Forces in `Update()` give inconsistent results — use `FixedUpdate()`.
- ⚠️ Don't modify `Rigidbody.position` directly — use `MovePosition()` or forces.
- ⚠️ Set fast-moving balls to `Continuous` collision detection to prevent tunneling.
- ⚠️ Don't destroy Rigidbody objects with `Destroy()` mid-physics-step — defer with `Destroy(obj, delay)` or use pooling.

### Touch Input
- ⚠️ Always check `Touch.phase` to distinguish Began/Moved/Ended.
- ⚠️ Differentiate tap from drag using a movement threshold (10-15 pixels).
- ⚠️ On mobile, `Input.mousePosition` works but `Input.GetTouch()` is preferred.
- ⚠️ Use the new Input System if configured, otherwise legacy `Input` is acceptable.

### UI
- ⚠️ Always use TextMeshPro (`TextMeshProUGUI`), never `UnityEngine.UI.Text`.
- ⚠️ Button click handlers should prevent double-click with a brief cooldown.
- ⚠️ UI raycasts block touch input — ensure gameplay area is NOT covered by invisible UI elements.

### Save/Load
- ⚠️ `PlayerPrefs` is for simple settings only (music on/off). Use JSON serialization for game state.
- ⚠️ Save BEFORE scene transitions, not after.
- ⚠️ Handle corrupted save files gracefully — fallback to defaults.

### Memory
- ⚠️ Unload unused assets between scenes: `Resources.UnloadUnusedAssets()`.
- ⚠️ Avoid loading all 30 levels' data simultaneously.
- ⚠️ Cache component references in `Awake()` — don't call `GetComponent<>()` every frame.

---

## 13. Project-Specific Patterns

### Level Completion Check
```csharp
// Win: All required objects destroyed/cleared
// Lose: Balls = 0 AND Objects remain
// Exit: -1 Life
// App Close: -1 Life (check flag on next launch)

// IMPORTANT: Wait for physics to settle before declaring win
// IMPORTANT: Add victory delay (~2 sec) before showing Win Panel
// IMPORTANT: Never deduct life twice for same event
```

### Reward Idempotency
```csharp
// All rewards must be idempotent — same event cannot grant reward twice
private bool _rewardGranted = false;

public void GrantLevelReward(int remainingBalls)
{
    if (_rewardGranted) return; // Prevent duplicate
    _rewardGranted = true;
    
    int baseReward = _config.BaseLevelReward;
    int ballBonus = remainingBalls * _config.RemainingBallMultiplier;
    int totalReward = baseReward + ballBonus;
    
    GoldManager.Instance.AddGold(totalReward);
    SaveManager.Instance.Save();
}
```

### Skill System
```csharp
// Skills can STACK — both Mega Ball + Infinity Ball active simultaneously
// Each activation: -1 use (never go negative)
// Depleted: show purchase option, not error

public bool CanActivateSkill(SkillType type)
{
    return IsUnlocked(type) && GetRemainingUses(type) > 0;
}
```
