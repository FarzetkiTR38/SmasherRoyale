# Royal Smash! — Physics Puzzle
## Agent Instructions

> This is a Unity 3D mobile physics puzzle game. All AI agents MUST follow these rules.

## Source of Truth

- The **Master Build Prompt** is located at: `_FarzetkiDocs/master_build_prompt.md`
- The **Full Game Documentation** is at: `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`
- When in doubt, refer to the Master Build Prompt first. It is the absolute authority.

## Critical Project Rules

1. **Unity 3D Project** — This is a 3D game, NOT 2D. Use Rigidbody (not Rigidbody2D), Collider (not Collider2D), Physics (not Physics2D).
2. **Portrait Orientation** — The game runs in portrait mode on mobile (Android + iOS).
3. **C# Only** — All scripts must be written in C#. Follow C# naming conventions.
4. **Assets Folder Only** — NEVER modify files outside the `Assets/` folder unless explicitly asked.
5. **TextMeshPro** — Always use TextMeshPro (TMP) for UI text. NEVER use legacy `UnityEngine.UI.Text`.
6. **No Hardcoded Values** — All configurable values (physics, economy, timing, etc.) MUST be exposed via ScriptableObjects, serialized fields, or config files. NEVER embed magic numbers in code.
7. **Data-Driven Levels** — Level layouts MUST be defined in data (ScriptableObject or JSON), NOT hardcoded in scenes or scripts.
8. **Placeholder Assets** — If an asset doesn't exist, create a clear placeholder (primitive mesh, solid color material, simple icon). Label it as "PLACEHOLDER". Make replacement easy.
9. **Zero User Delegation via Unity MCP** — NEVER tell the user to manually perform actions inside the Unity Editor (attaching components, creating materials, setting inspector values, creating canvases, checking console). Use Unity MCP tools (`manage_gameobject`, `manage_components`, `manage_scene`, `manage_prefabs`, `manage_material`, `execute_code`, etc.) to execute them autonomously. Verify with `read_console` and screenshots (`manage_camera`), iterate until flawless, and only escalate external tasks (ad dashboards, backend credentials, custom art assets). See `.agents/skills/unity_mcp_automation/SKILL.md`.
10. **Phase Implementation Plans** — For every development phase, you MUST create, maintain, and archive the implementation plan directly in `_FarzetkiDocs/Phases/Implementation Plan — Phase X.md`. Document system architecture, deliverables, acceptance criteria, and completion records. See `.agents/skills/phase_implementation_workflow/SKILL.md`.

## Architecture Rules

- **Single Responsibility Principle** — One class, one job. No god scripts.
- **Composition over Inheritance** — Prefer component composition. Use interfaces and ScriptableObjects.
- **Manager Pattern** — Use singleton managers only for truly global systems (GameManager, AudioManager, SaveManager). Avoid excessive singletons.
- **Event System** — Use C# events, UnityEvents, or ScriptableObject-based events for decoupling. Avoid direct cross-references between unrelated systems.
- **Namespace Usage** — All game scripts should use the `RoyalSmash` namespace (e.g., `RoyalSmash.Gameplay`, `RoyalSmash.UI`, `RoyalSmash.Economy`).

## Code Quality

- Write clean, self-documenting code with descriptive names.
- Add XML doc comments (`///`) on public methods and classes.
- Always handle null checks and edge cases.
- Prefer `async/await` or Coroutines over Update-based timers.
- Avoid `Find()`, `FindObjectOfType()` in runtime code. Use dependency injection or inspector references.
- Avoid `Destroy()` spam — use object pooling where appropriate.

## File Organization

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, SaveManager, EventBus
│   │   ├── Gameplay/       # Mortar, Projectile, ObjectSystem, PhysicsInteraction
│   │   ├── Objects/        # Box, Stone, Column, Cone, PaintBottle base classes
│   │   ├── Levels/         # LevelManager, LevelData, LevelLoader
│   │   ├── Skills/         # MegaBall, InfinityBall, SkillManager
│   │   ├── Economy/        # GoldManager, RewardSystem
│   │   ├── Lives/          # LivesManager, LifeRegeneration
│   │   ├── UI/             # UIManager, panels, HUD
│   │   ├── Audio/          # AudioManager, SFXController
│   │   ├── Ads/            # AdManager, InterstitialAd, RewardedAd
│   │   ├── Cloud/          # CloudSyncManager, LeaderboardManager
│   │   ├── Save/           # SaveSystem, PlayerData
│   │   ├── Tutorial/       # TutorialManager, TutorialStep
│   │   ├── Profile/        # ProfileManager, AvatarSystem
│   │   └── Utils/          # Helpers, Extensions, Constants
│   ├── Prefabs/
│   │   ├── Gameplay/       # Ball, Mortar, Objects, Stand
│   │   ├── UI/             # Panels, Buttons, HUD elements
│   │   └── VFX/            # Particle effects
│   ├── ScriptableObjects/
│   │   ├── Config/         # GameConfig, PhysicsConfig, EconomyConfig
│   │   ├── Levels/         # LevelData_01 through LevelData_30
│   │   └── Objects/        # ObjectConfig per type
│   ├── Materials/
│   ├── Textures/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   ├── Scenes/
│   ├── Animations/
│   └── UI/
│       ├── Fonts/
│       └── Sprites/
└── Plugins/                # Third-party SDKs (Ads, IAP, etc.)
```

## Testing After Changes

- After ANY script change, verify compilation succeeds (no errors in Console).
- After physics changes, test that balls still fire correctly and objects react properly.
- After UI changes, verify all panels open/close correctly.
- After save system changes, verify data persists across play sessions.
- NEVER break existing working systems when adding new features.

## What NOT To Do

- Do NOT add systems not in the Master Build Prompt (no PvP, no daily quests, no battle pass, etc.)
- Do NOT use legacy UI Text — always use TextMeshPro
- Do NOT use Physics2D components — this is a 3D game
- Do NOT hardcode level layouts in code
- Do NOT create files outside Assets/ folder
- Do NOT use `PlayerPrefs` for complex save data — use a proper serialization system
- Do NOT ignore the locked requirements in Section 1 of the Master Build Prompt
- Do NOT instruct the user to perform tasks in the Unity Editor that can be done via Unity MCP
