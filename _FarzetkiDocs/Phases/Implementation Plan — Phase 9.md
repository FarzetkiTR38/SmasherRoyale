# Implementation Plan — Phase 9: Skill System (Mega Ball & Infinity Ball)

Implementation plan and technical specification for **Phase 9: Skill System** based on **Section 12 (Skill System)** and **Section 35 (Phase 9)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Mechanics & Features

1. **Mega Ball (Skill 1)**:
   - **Unlock Level**: Level 5 (Locked before Level 5).
   - **Starting Uses**: 3 uses on new game.
   - **Physical Effect**:
     - 2.0x diameter multiplier (larger projectile geometry and collider).
     - 2.5x mass multiplier (significantly greater momentum, kinetic energy, and impact force against physics structures).
     - Distinct visual styling (glowing gold/amber shader/material tint).
   - **Duration & Consumption**: One-shot buff; applied to the next projectile fired from the mortar, then consumed.

2. **Infinity Ball (Skill 2)**:
   - **Unlock Level**: Level 10 (Locked before Level 10).
   - **Starting Uses**: 3 uses on new game.
   - **Physical Effect**:
     - Overrides normal mortar reload cooldown (0.5s) to rapid stream rate (0.06s).
     - Allows continuous rapid fire during active duration.
   - **Duration & Consumption**: Active for 5.0 seconds from tap; displays live countdown timer on the HUD skill button.

3. **Skill Stacking**:
   - Both Mega Ball and Infinity Ball can be activated simultaneously:
     `Mega Ball (Large & Heavy) + Infinity Ball (Rapid Stream) = Rapid Large Heavy Fire`.
   - Modifiers stack without conflicts or race conditions.

4. **Skill UI States & Tactile Feedback**:
   - **Locked**: Level condition not met (< Level 5 or < Level 10). Shows 🔒 lock badge, non-interactive.
   - **Available**: Unlocked with uses $> 0$. Displays remaining count badge.
   - **Active**: Currently engaged. Displays active glow animation and live countdown timer (`X.Xs`).
   - **Depleted**: 0 uses remaining. Displays "0" badge and prevents accidental activation.

5. **Persistence & Anti-Cheat Guards**:
   - Starting uses, unlock flags, and inventory synced directly with `PlayerSaveData.cs` and `SaveManager.cs`.
   - Atomic decrement upon activation (-1 use); prevents double consumption or negative inventory ($uses \ge 0$).

---

## 2. Deliverables & Files

- `[MAINTAIN]` `Assets/_Project/Scripts/Skills/SkillType.cs` (MegaBall = 1, InfinityBall = 2)
- `[MAINTAIN]` `Assets/_Project/Scripts/Skills/SkillConfig.cs` (Exposes all multipliers, timers, unlock thresholds via ScriptableObject)
- `[MODIFY]` `Assets/_Project/Scripts/Skills/SkillManager.cs` (Integrate SaveManager synchronization, level unlock checks, persistence, stacking)
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/MortarController.cs` (Ensure rapid Infinity fire and Mega Ball projectile instantiation)
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/ProjectileController.cs` (Verify scaling, mass multiplication, and material tint)
- `[MODIFY]` `Assets/_Project/Scripts/UI/SkillButtonUI.cs` (Refine visual states: Locked, Available, Active, Depleted)
- `[MODIFY]` `Assets/_Project/Scripts/Levels/LevelManager.cs` (Hook up skill unlock refresh per level and active skill reset on completion)
- `[NEW]` `Assets/_Project/Scripts/Utils/Phase9PlayModeTest.cs` (Comprehensive automated Play Mode test suite)

---

## 3. Acceptance Tests

- [x] **Test 1: Default Inventory & Initialization**: Starting player has 3 Mega Ball uses and 3 Infinity Ball uses.
- [x] **Test 2: Level 1 Locked State**: On Level 1, both Mega Ball and Infinity Ball are locked (🔒). Tap attempt does not activate or consume.
- [x] **Test 3: Level 5 Mega Ball Unlock**: Advancing to Level 5 unlocks Mega Ball into Available state; Infinity Ball remains locked.
- [x] **Test 4: Mega Ball Activation & Projectile Scaling**: Activating Mega Ball consumes exactly 1 use (3 -> 2); next fired projectile has 2.0x scale and 2.5x mass.
- [x] **Test 5: Level 10 Infinity Ball Unlock**: Advancing to Level 10 unlocks Infinity Ball into Available state.
- [x] **Test 6: Infinity Ball Activation & Rapid Fire**: Activating Infinity Ball sets rapid cooldown (0.06s) for 5 seconds with countdown timer on HUD.
- [x] **Test 7: Skill Stacking**: Activating both skills simultaneously produces rapid mega projectiles.
- [x] **Test 8: Zero Uses Depletion Guard**: When uses reach 0, skill cannot be activated; inventory never goes negative.
- [x] **Test 9: Persistence Across Sessions**: Used skill counts persist across save reloads via SaveManager.

---

## 4. Completion Record & Verification Results

### Automated Play Mode Test Execution
The automated test runner (`Phase9PlayModeTest.cs`) executed against `MainGame.unity` with 100% pass rate:
- **Test 1 (Default Starting Inventory)**: MegaBall: 3, InfinityBall: 3 -> **PASSED**
- **Test 2 (Level 1 Locked State)**: Skills locked on Level 1, activation attempts rejected -> **PASSED**
- **Test 3 (Level 5 Mega Ball Unlock)**: MegaBall unlocked, InfinityBall locked -> **PASSED**
- **Test 4 (Mega Ball Activation & Consumption)**: 1 use consumed (3 -> 2), active buff applied -> **PASSED**
- **Test 5 (Mega Ball Projectile Fired)**: Instantiated with 2.0x scale, 2.5x mass, buff consumed upon launch -> **PASSED**
- **Test 6 (Level 10 Infinity Ball Unlock)**: InfinityBall unlocked at Level 10 -> **PASSED**
- **Test 7 (Infinity Ball Rapid Stream)**: 5.0s countdown active, rapid fire cooldown (0.06s) enabled -> **PASSED**
- **Test 8 (Skill Stacking)**: Both MegaBall and InfinityBall active simultaneously without conflicts -> **PASSED**
- **Test 9 (Zero Uses Depleted Guard)**: At 0 uses, activation blocked, count remains $\ge 0$ -> **PASSED**
- **Test 10 (Save Persistence)**: Skill inventory and unlock flags persist to `SaveManager.Data` -> **PASSED**

### Visual Proof
- Active Skills in Flight (Mega Ball + Rapid Stream Infinity Ball + Active HUD Glows & Timers):
  `C:/Users/Farzetki/.gemini/antigravity-ide/brain/097e239d-ba65-4d8a-b4de-41b788ffcbf3/phase9_skills_active.png`

### Console Health
- Zero compilation errors (`read_console` returned 0 error logs).
- Both `MainGame.unity` and `PhysicsTestScene.unity` saved with complete Settings & Skill hierarchies.
