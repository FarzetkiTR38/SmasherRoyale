# Implementation Plan — Phase 25 — Sky Citadel

**Status:** In progress; connection and read-only audit complete, scene implementation and acceptance checks pending.  
**Started:** 2026-09-20.  
**Scope:** Transform the existing `Assets/_Project/Scenes/MainGame.unity` into the user's cream stone, gold, royal blue and cyan sky-castle reference, with a real 3D floor, a visible stand, physics blocks, and matching mortar and HUD.  
**Checkpoint policy:** Save this document after every completed subphase and before stopping. Never mark an unchecked result as passed. The next session resumes from the first incomplete subphase.

Phase 24 already covers BootScene and records completion. Phase 25 is the next numbered phase; this document does not replace the earlier Visual Showcase plan.

## 1. Reference mapping and visual intent

- User request in the current Codex task takes precedence over historical visual references: replace the green kingdom/bottle showcase with the supplied sky-citadel direction. Keep a stand underneath the targets. The lower screen is a real 3D floor; the upper screen uses an appropriately positioned backdrop sprite/quad/canvas. Preserve playable 3D objects and projectile physics.
- `_FarzetkiDocs/master_build_prompt.md`: Sections 1 and 4–7 (world-coordinate targeting, real physics, limited ammunition and object behavior); Section 8 (static configurable stand); Sections 9–10 (level data and target/win tracking); Sections 24–25 (HUD, portrait camera and layered 3D world).
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`: Sections 127/130 and 257/259 (HUD and skills), 297–298 (camera and environment), 344 (readable gameplay and feedback).
- Workflow: `.agents/skills/phase_implementation_workflow/SKILL.md`; physics conventions: `.agents/skills/unity_3d_physics_game/SKILL.md`.

User reference files:

| File | Meaning |
| --- | --- |
| `C:/Users/Farzetki/Downloads/Codex Görseli 20 Eyl 2026 19_18_21.png` | Empty sky-castle environment and marble courtyard |
| `C:/Users/Farzetki/Downloads/Codex Görseli 20 Eyl 2026 19_18_12.png` | Stone, reinforced, energy, glass and wooden blocks; blue energy balls |
| `C:/Users/Farzetki/Downloads/Codex Görseli 20 Eyl 2026 19_18_36.png` | Overall palette/composition reference; floating target arrangement is not the intended support structure |
| `C:/Users/Farzetki/AppData/Local/Temp/codex-clipboard-73df2c25-069a-4bd7-9036-5ff9ab981ab0.png` | Existing scene to overhaul |

## 2. Architecture and implementation boundaries

- Unity `6000.4.0f1`; URP `17.4.0`. The project includes `com.coplaydev.unity-mcp`. Current PC quality uses `Assets/Settings/PC_RPAsset.asset`; mobile uses `Mobile_RPAsset.asset` at render scale 0.8. Current camera postprocessing is disabled.
- Preserve `RoyalSmash.Gameplay.MortarController`, `InputHandler`, `ProjectileController`, `PhysicsObject`, `StandController`, `RoyalSmash.Levels.LevelManager`, `LevelLoader`, and the existing UI event bindings.
- `PhysicsObject` uses Rigidbody plus a root Collider, registers itself with LevelManager and clears on ground contact/falling. Visual children should not add accidental colliders. Continue using physics layers 6 projectile, 7 object, 8 stand, 9 ground.
- Keep the mortar aiming root `Visuals` and its `Visuals/BarrelPivot/FirePoint`. Visual changes must preserve the muzzle position and aiming references.
- Add a scoped editor setup utility in namespace `RoyalSmash.Editor`, with assets in a dedicated SkyCitadel area and a deterministic entry point. Record exact final filenames in the deliverable manifest below once chosen. Do not run the old `VisualShowcaseSetup.SetupAllScenes()` because it also modifies PhysicsTestScene and recreates the earlier art direction.
- Prefer reusable asset materials/meshes/prefabs and Inspector configuration. The backdrop must not intercept input or collide with projectiles. The real ground must retain shadow reception and its gameplay collision layer.
- HUD remains attached to existing controllers: ball count changes after firing, skill inventory/unlocks remain authoritative, and settings/exit actions continue to work. Do not turn a remaining-ball counter into a static level label accidentally.

### Runtime hazards discovered by audit

1. `LevelLoader` is active in MainGame. `LevelManager.Awake()` loads the LevelDatabase through an editor fallback, and `Start()` locates the loader and spawns a data-driven level. The old scene also contains 30 static bottles. Resolve the static-showcase/dynamic-level ownership before validating target counts; otherwise targets and stands can duplicate.
2. `StandController.Awake()` reapplies `Assets/_Project/ScriptableObjects/Config/StandConfig.asset`, including tabletop height 1.35 and size `(5, 0.25, 3.2)`. A scene-only scale change can be overwritten on play.
3. `EnvironmentThemeManager`, if active, can rewrite sun, camera background and fog at runtime. Check its scene references/configuration during play verification.
4. Existing camera is `(0, 3.11, -5.43)`, pitch 8.16°, FOV 56°. Mortar is `(0, 1.07, -2.52)`. The old static targets are at z 7.5. These are baseline values, not locked design requirements.

## 3. Baseline and protected pre-existing edits

The following files were already modified before this phase. Preserve their unrelated edits; do not reset or overwrite them wholesale:

- `.plastic/plastic.wktree`
- `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`
- `Assets/_Project/Materials/M_KingdomBackdrop.mat` (new texture reference)
- `Assets/_Project/Materials/M_StylizedGrass.mat` (color and smoothness)
- `Assets/_Project/Scenes/MainGame.unity` (backdrop transform and serialized values)

Baseline screenshot exists at `_FarzetkiDocs/Art/SkyCitadel/sky-citadel-before.png`. Creating a durable baseline file backup and recording its location remain pending at the initial checkpoint. Avoid committing unrelated dirty files.

## 4. Resumable subphases

| ID | Work and completion gate | Status | Evidence / checkpoint |
| --- | --- | --- | --- |
| 25.1 | Verify live MCP, inspect current scene and dirty files; save a durable pre-change backup and baseline screenshot | In progress | MCP and audit done; baseline screenshot captured; durable backup location pending |
| 25.2 | Import/copy reference artwork or generate suitable texture assets; build the upper-screen backdrop with correct framing and noninteractive rendering | Pending | Record asset paths and an editor capture |
| 25.3 | Build cream marble 3D ground, blue/gold detail and an explicitly visible stand; match horizon and shadows; preserve support colliders | Pending | Inspect floor/backdrop seam and supported targets in portrait framing |
| 25.4 | Create reusable physics block visuals/prefabs and target arrangement; resolve LevelLoader ownership; verify collider alignment and supported idle stack | Pending | Record prefab mapping and expected target count |
| 25.5 | Restyle mortar/projectile in blue energy and gold; preserve aiming hierarchy, muzzle, ball physics and lifetime | Pending | Record changed prefab/material paths and a successful shot |
| 25.6 | Restyle HUD and skill buttons with readable gold/blue/cyan presentation and mobile spacing while preserving functional bindings | Pending | Capture readable portrait HUD with input unobstructed |
| 25.7 | Compile, perform focused play regression and console checks, verify idle/impact/reload behavior and capture final screenshots | Pending | Record each actual result, console errors, images and any failures |
| 25.8 | Review final diff; save final scene/assets; reconcile documentation and manifest; report remaining limits and provide the result | Pending | Final summary and optional scoped Git commit only after verification |

Subphases may have independent preparation in parallel, but a completion gate only passes after the integrated scene has the required evidence. If interrupted, retain partial work and record the exact next unverified action below.

## 5. Deliverable manifest

| Deliverable | Path / namespace | State |
| --- | --- | --- |
| Phase plan | `_FarzetkiDocs/Phases/Implementation Plan — Phase 25 — Sky Citadel.md` | Created |
| Main gameplay scene | `Assets/_Project/Scenes/MainGame.unity` | Visual update pending at initial checkpoint |
| Baseline capture | `_FarzetkiDocs/Art/SkyCitadel/sky-citadel-before.png` | File exists |
| Editor setup utility | `Assets/_Project/Scripts/Editor/` / `RoyalSmash.Editor`; exact filename to record | Pending |
| Backdrop/floor/block/UI textures and materials | Dedicated SkyCitadel asset paths to record | Pending |
| Reusable block, stand, projectile/mortar visual prefabs or prefab updates | Exact filenames to record after implementation | Pending |
| Focused play checks | `Assets/_Project/Scripts/Editor/Phase25PlayModeTest.cs` if a permanent test is needed; otherwise record executed MCP validation and output | Not run; test file not yet created |
| Final evidence | `_FarzetkiDocs/Art/SkyCitadel/`; exact capture filenames to record | Pending |

## 6. Acceptance criteria and actual verification

1. [ ] MainGame shows the supplied sky-castle visual direction with a 3D marble lower half and upper environment backdrop.
2. [ ] A visible stone/blue/gold stand supports every intended starting target; no unintended target overlap or duplicate dynamic spawn exists.
3. [ ] Camera frames the mortar, trajectories, stand and targets in portrait; background seam does not visibly break the environment.
4. [ ] Idle targets remain stable, and a world-targeted shot consumes exactly one ball and produces real physics impact/destruction feedback.
5. [ ] Shot lifetime, aim/tap behavior, target registration and win/lose tracking remain intact; a clean reload produces the expected target count.
6. [ ] HUD numbers and labels are readable, skill/settings buttons remain wired and gameplay input is not intercepted by background imagery.
7. [ ] Unity compiles with zero new errors, and the tested play session has no new missing-reference/runtime exceptions.
8. [ ] Scene/assets are saved, before/after images are recorded, unrelated baseline changes are preserved and unresolved issues are documented.

**Current verification:** audit completed; MCP connection reported working by the implementing agent; baseline screenshot file exists. No scene-art implementation result or play regression is marked as passed yet.

## 7. Restart instructions and live checkpoint

**Last checkpoint:** 2026-09-20, initial plan after connection/audit.  
**Current subphase:** 25.1.  
**Exact next action:** Verify the active Unity scene and play state through the existing MCP bridge, create/record a durable backup of the currently dirty MainGame scene and any assets about to be changed, then proceed with the scoped Sky Citadel builder and artwork integration in 25.2. If implementation has advanced since this checkpoint, inspect the current diff and builder before recreating anything.

1. Open this document first, then run `git status --short` and inspect only phase-related changes. Check whether the intended scene is already saved and whether a builder/asset folder exists.
2. Existing working bridge: `Temp/CodexSky/mcp.py`, local endpoint `http://127.0.0.1:8080/mcp`. It uses a session file and JSON argument files, with `list`, `resources`, `resource`, `schema` and `call` modes. The connection was confirmed in this session. `Temp` is disposable: if these files are absent after restart, reconnect using the installed Unity MCP package instead of assuming the server remains available.
3. `python Temp/CodexSky/mcp.py list` inventories tools. Read schemas before calls, e.g. `python Temp/CodexSky/mcp.py schema execute_code manage_scene manage_editor read_console`. Tool names must be confirmed from the live list. `Temp/CodexSky/inspect.json` and `screenshot.json` contain the initial inspection/capture arguments.
4. Confirm MainGame is active and whether Unity is playing before edits. Preserve unsaved changes; do not blindly reload the scene or run an older broad setup utility.
5. Resume the first incomplete gate in Section 4. After each gate, update its state, exact changed paths, verification evidence, and this checkpoint's next action.
6. Before quota interruption or stopping, save Unity assets/scene and record current compile/play state, last success or error, pending verification, and the next concrete action. Do not claim visual or runtime completion from script compilation alone.

## 8. Completion record

- Completion date: pending.
- Final changed asset/script paths: pending.
- Compilation and console results: pending.
- Play regression and target count: pending.
- Final portrait screenshot(s): pending.
- Outstanding defects/limits: unverified implementation; audit hazards listed above must be addressed.
- Commit: not created at initial checkpoint.
