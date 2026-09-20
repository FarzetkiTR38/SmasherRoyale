# Implementation Plan — Phase 17: VFX & Visual Polish

## 1. System Architecture & Objective

Implement a complete, high-performance visual effects (VFX) framework for **Royal Smash! — Physics Puzzle** adhering to **Section 22 (VFX System)**, **Section 1.1 (Rules 5, 7, 8: Anti-Popping & Depth Trajectory)**, and **Section 35 (Phase 17)** of `_FarzetkiDocs/master_build_prompt.md`.

### Core Requirements
1. **Muzzle Fire VFX**: Visual puff/flash at mortar muzzle tip when launching a projectile.
2. **Ball Flight Trail & Fade**: Visible particle/ribbon trail behind projectile in flight; balls smoothly dissolve/fade into world depth before auto-destroying (no popping).
3. **Impact VFX (Generic & Heavy)**: Collision point particle burst scaling with collision impulse (sparks & dust).
4. **Object Destruction VFX**: Crumbling fragment burst and smooth shrink/fade before deletion.
5. **Paint Bottle Break & Lingering Spill**: High-velocity glass shards accompanied by a colored paint splash that leaves a lingering puddle decal.
6. **Skill Activation VFX**: Glowing radial burst and icon pulse upon MegaBall / InfinityBall activation, with dynamic projectile trail color shifts.
7. **Win Celebration VFX**: Confetti and royal star burst upon level completion.
8. **UI Reward Coin Animation**: Flying gold coin fountain towards HUD gold display when claiming level rewards.
9. **Object Pooling & Performance**: Zero garbage allocation spikes, particle auto-recycling/pooling, mobile-optimized particle counts.

---

## 2. Deliverables & Component Changes

### VFX Core Framework
- `[NEW]` `Assets/_Project/Scripts/VFX/VFXConfig.cs`:
  - ScriptableObject schema holding prefabs for MuzzleFlash, BallTrail, ImpactGeneric, ImpactHeavy, ObjectDestruct, BottleBreakShards, PaintSpill, SkillActivate, ConfettiWin, and FlyingCoin.
- `[NEW]` `Assets/_Project/Scripts/VFX/VFXManager.cs`:
  - Singleton manager with pre-warmed particle pooling, safe auto-despawn coroutines, and centralized trigger methods:
    - `SpawnMuzzleFlash(Vector3 position, Quaternion rotation)`
    - `SpawnImpact(Vector3 position, Vector3 normal, float impulse)`
    - `SpawnDestruction(Vector3 position, Color baseColor)`
    - `SpawnBottleBreak(Vector3 position, Color paintColor)`
    - `SpawnSkillActivation(Vector3 screenPos, SkillType type)`
    - `SpawnWinCelebration(Vector3 position)`
    - `PlayRewardCoinAnimation(Vector3 startScreenPos, Vector3 targetScreenPos, int coinCount, Action onComplete)`
- `[NEW]` `Assets/_Project/Scripts/VFX/CoinFlyAnimation.cs`:
  - UI animation component driving procedural bezier curve flight of reward coins towards GoldDisplay widget.

### Gameplay & UI Integration
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/MortarController.cs`:
  - Trigger `VFXManager.Instance.SpawnMuzzleFlash()` at `_firePoint`.
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/ProjectileController.cs`:
  - Integrate `TrailRenderer` with dynamic gradient tinting based on active skills.
  - Smooth fade-out before auto-destruction.
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/PhysicsObject.cs`:
  - Spawn impact VFX at contact point.
  - Spawn destruction debris particles on shrink/dissolve.
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/PaintBottleObject.cs`:
  - Spawn specialized glass shard burst and lingering paint spill.
- `[MODIFY]` `Assets/_Project/Scripts/Skills/SkillManager.cs` / `HUDController.cs`:
  - Trigger skill activation glow on button press.
- `[MODIFY]` `Assets/_Project/Scripts/UI/WinPanel.cs`:
  - Trigger win confetti burst on popup show.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase17Setup.cs`:
  - Procedural generator creating particle system prefabs, materials, and `VFXConfig.asset`.
  - Scene configuration wiring `VFXManager` in `MainMenu.unity` and `MainGame.unity`.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase17PlayModeTest.cs`:
  - Automated integration test suite validating all 10 Phase 17 acceptance criteria.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: VFXConfig & Prefab Validation**:
   - `VFXConfig.asset` loaded with all 10 visual effect prefabs properly linked.
2. **Test 2: VFXManager Initialization & Pooling**:
   - `VFXManager.Instance` initialized with active pre-warmed pools.
3. **Test 3: Mortar Muzzle Flash**:
   - Firing ball spawns muzzle flash particle at fire point.
4. **Test 4: Ball In-Flight Trail**:
   - Projectile possesses an active `TrailRenderer` or flight particle trail.
5. **Test 5: Impact VFX Differentiation**:
   - Light impact spawns generic spark/dust; heavy impact spawns expanded burst.
6. **Test 6: Object Destruction Debris**:
   - Destruction triggers fragment particles and smooth scale shrink (anti-pop rule).
7. **Test 7: Paint Bottle Shards & Lingering Spill**:
   - Paint bottle break creates glass shards and a lingering colored paint splat.
8. **Test 8: Skill Activation Visual Feedback**:
   - Activating MegaBall or InfinityBall triggers visual flash/glow.
9. **Test 9: Win Celebration Confetti**:
   - Level victory triggers celebratory particle burst.
10. **Test 10: Zero Console Errors**:
    - Clean execution across all tests with 0 errors and 0 warnings.

---

## 4. Completion Record & Verification Status

- **Execution Date**: 2026-09-17
- **Test Suite**: `RoyalSmash.Editor.Phase17PlayModeTest.RunAllTests()`
- **Overall Result**: **10/10 PASSED (100%), 0 FAILED, 0 CONSOLE ERRORS**

### Detailed Test Results
| Test ID | Test Case | Target System | Result | Notes |
|---|---|---|---|---|
| **Test 1** | VFXConfigAndPrefabs | `VFXConfig.asset` | **PASS** | 10 Section 22 prefabs & materials linked and verified. |
| **Test 2** | VFXManagerInitialization | `VFXManager.cs` | **PASS** | Singleton initialized with pre-warmed pooling. |
| **Test 3** | MuzzleFlashVFX | `MortarController.cs` | **PASS** | Muzzle flash particle spawned at launch fire point. |
| **Test 4** | BallFlightTrail | `ProjectileController.cs` | **PASS** | `TrailRenderer` linked with material on `Ball.prefab`. |
| **Test 5** | ImpactDifferentiation | `PhysicsObject.cs` | **PASS** | Dynamic scaling for light vs heavy collision impulses. |
| **Test 6** | ObjectDestructionDebris | `PhysicsObject.cs` | **PASS** | Debris burst with object-tinted particle emission. |
| **Test 7** | PaintBottleBreakShardsAndSpill | `PaintBottleObject.cs` | **PASS** | Glass shard burst + lingering paint puddle decal spawned. |
| **Test 8** | SkillActivationGlow | `SkillManager.cs` | **PASS** | Fiery orange (Mega) and cyan (Infinity) visual flash. |
| **Test 9** | WinCelebrationConfetti | `WinPanel.cs` | **PASS** | Multi-colored celebration confetti burst spawned. |
| **Test 10** | RewardCoinFly | `CoinFlyAnimation.cs` | **PASS** | Quadratic Bezier flying coin animation executed cleanly. |

### Artifacts and Assets Produced
- `Assets/_Project/Scripts/VFX/VFXConfig.cs`: ScriptableObject schema for visual effects.
- `Assets/_Project/ScriptableObjects/Config/VFXConfig.asset`: Central configuration asset.
- `Assets/_Project/Scripts/VFX/VFXManager.cs`: Singleton pooling manager for all Section 22 effects.
- `Assets/_Project/Scripts/VFX/CoinFlyAnimation.cs`: Procedural Bezier curve animator for flying gold coins.
- `Assets/_Project/Prefabs/VFX/`:
  - `VFX_MuzzleFlash.prefab`
  - `VFX_ImpactGeneric.prefab`
  - `VFX_ImpactHeavy.prefab`
  - `VFX_ObjectDestruct.prefab`
  - `VFX_BottleShards.prefab`
  - `VFX_PaintSpill.prefab`
  - `VFX_SkillActivate.prefab`
  - `VFX_WinConfetti.prefab`
  - `UI_CoinFly.prefab`
- `Assets/_Project/Materials/VFX/`:
  - `Mat_Trail_Default.mat`, `Mat_Trail_Mega.mat`, `Mat_Trail_Infinity.mat`, `Mat_Particle_Additive.mat`
- `Assets/_Project/Prefabs/Gameplay/Ball.prefab`: Updated with `TrailRenderer`.
- `Assets/_Project/Scenes/MainMenu.unity` & `Assets/_Project/Scenes/MainGame.unity`: `VFXManager` configured.
