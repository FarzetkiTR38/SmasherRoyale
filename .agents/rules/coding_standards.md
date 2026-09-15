# Coding Standards & Best Practices

## C# Style Guide

### Naming Conventions
- **PascalCase**: Classes, methods, properties, public fields, events, enums, namespaces
- **camelCase with underscore prefix**: Private fields (`_myField`)
- **camelCase**: Local variables, parameters
- **UPPER_SNAKE_CASE**: Constants (`MAX_LIFETIME`)
- **I prefix**: Interfaces (`IDestructible`, `ISaveable`)

### Namespace Structure
All game code must use the `RoyalSmash` namespace:
```
RoyalSmash.Core
RoyalSmash.Gameplay
RoyalSmash.Objects
RoyalSmash.Levels
RoyalSmash.Skills
RoyalSmash.Economy
RoyalSmash.Lives
RoyalSmash.UI
RoyalSmash.Audio
RoyalSmash.Ads
RoyalSmash.Cloud
RoyalSmash.Save
RoyalSmash.Tutorial
RoyalSmash.Profile
RoyalSmash.Utils
```

### Class Suffixes
- `Manager` — Singleton global systems (GameManager, AudioManager)
- `Controller` — Per-object behavior scripts (MortarController, ProjectileController)
- `Data` — ScriptableObject data containers (LevelData, ObjectData)
- `Config` — Configuration ScriptableObjects (GameConfig, PhysicsConfig)
- `System` — Stateless logic systems (RewardSystem, ScoreSystem)
- `Handler` — Event/callback responders (InputHandler, CollisionHandler)
- `Panel` — UI panels (WinPanel, LosePanel, ShopPanel)
- `Button` — Custom button components (SkillButton, SafeButton)

### Access Modifiers
- Prefer `private` by default. Only make `public` what is genuinely needed outside.
- Use `[SerializeField] private` instead of `public` for inspector-visible fields.
- Use properties with `{ get; private set; }` for read-only external access.

### Comments
- Use `///` XML doc comments on all public methods and classes.
- Use `//` inline comments only for non-obvious logic.
- Do NOT comment obvious code (`i++ // increment i`).
- Use `// TODO:` for planned work and `// HACK:` for known workarounds.

## Architecture Principles

1. **Single Responsibility** — One class does one thing. No "do everything" scripts.
2. **Composition over Inheritance** — Use components and interfaces, not deep inheritance trees.
3. **Dependency Injection via Inspector / Serialized References** — Wire references directly via Unity MCP (`manage_components` or `execute_code`), not with runtime Find() and NEVER ask the user to wire them manually.
4. **ScriptableObject for Data** — All tunable values in ScriptableObjects, not hardcoded.
5. **Events for Decoupling** — Systems communicate through events, not direct references to each other.
6. **Immutable Game Rules** — Never change locked gameplay rules from the Master Build Prompt.
7. **Autonomous In-Engine Execution** — All Unity Editor operations (components, materials, UI, references) must be performed by AI via MCP tools, never delegated to the user.

## Error Handling

- Always null-check references before use.
- Use `Debug.LogWarning()` for recoverable issues.
- Use `Debug.LogError()` for serious problems.
- Never silently swallow exceptions — log them.
- Gameplay must never crash — handle edge cases gracefully.

## Performance Rules

- No allocations in Update/FixedUpdate (no `new`, no LINQ, no string concat).
- Cache component references in Awake().
- Use object pooling for frequently created/destroyed objects.
- Limit active Rigidbodies to ~50 per scene.
- Profile before optimizing — don't premature optimize.
