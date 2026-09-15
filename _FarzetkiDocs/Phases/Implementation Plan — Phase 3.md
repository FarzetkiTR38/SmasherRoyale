# Implementation Plan — Phase 3: Projectile System & World-Coordinate Targeting

Implementation of **Phase 3** for **Royal Smash! — Physics Puzzle** based on the Master Build Prompt (Sections 1.1, 4, 5, 24, 35) and the 3D Ball art specification.

---

## 1. Overview & Goals
Implement the core projectile launching system:
- Physics-driven candy-red Ball projectile with natural gravity and bounce physics.
- Ballistic trajectory calculation ensuring balls launched from the cannon muzzle fly with gravity and hit the exact tapped 3D world coordinate.
- Ball lifetime management (~5 seconds) with smooth shrink/fade-out before destruction (no instant popping).
- Fire cooldown management (~0.5 sec, configurable), with the first ball immediately ready.
- Level ball ammunition counter decrementing on each shot.
- Mobile portrait HUD (1080x1920 reference) displaying the remaining balls count with TextMeshPro.

---

## 2. Proposed Changes

### Configuration
#### [NEW] [ProjectileConfig.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/ProjectileConfig.cs)
- ScriptableObject asset at `Assets/_Project/ScriptableObjects/Config/ProjectileConfig.asset`:
  - `launchSpeed`: Horizontal launch velocity (~22 m/s).
  - `lifetime`: Total lifetime before destruction (~5.0 s).
  - `fadeDuration`: Time before end of life to begin shrinking/fading (~1.0 s).
  - `fireCooldown`: Delay between consecutive shots (~0.5 s).
  - `defaultBallCount`: Starting ball count for levels (~25).
  - `ballMass`: Rigidbody mass (~1.5 kg).
  - `bounciness`: Physics material bounciness (~0.45).
  - `friction`: Physics material friction (~0.3).

---

### Scripts

#### [NEW] [ProjectileController.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/ProjectileController.cs)
- Attached to the `Ball` prefab.
- Rigidbody on Layer 6 (`Projectile`), Continuous collision detection.
- Handles trajectory initialization via ballistic velocity calculation to reach target point.
- Tracks lifetime and runs coroutine at `(lifetime - fadeDuration)` to smoothly shrink scale and fade material alpha before recycling / destroying.

#### [MODIFY] [MortarController.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/MortarController.cs)
- Add cooldown tracking (`_cooldownTimer`): first ball has 0 cooldown, subsequent balls respect `fireCooldown`.
- Add ammunition tracking (`_remainingBalls`): decrements on each valid shot; prevents firing when empty.
- Subscribes to `InputHandler.OnTapFired(Vector3 worldTarget)`:
  - If cooldown ready and remaining balls > 0:
    - Spawns `Ball` prefab at `FirePoint.position`.
    - Initializes `ProjectileController.Launch(worldTarget)`.
    - Decrements remaining balls and resets cooldown timer.
    - Raises `OnBallCountChanged(int remaining)` and `OnBallFired()`.

#### [NEW] [HUDController.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/UI/HUDController.cs)
- Manages gameplay HUD elements.
- Listens to `MortarController.OnBallCountChanged`.
- Updates the TextMeshPro counter with a punchy micro-scale animation upon firing.

---

### Prefabs, Materials & UI

#### [NEW] Materials & Physics Materials
- `Assets/_Project/Materials/M_Ball.mat`: Glossy candy red (`#EF4444`, smoothness 0.85).
- `Assets/_Project/Materials/PM_Ball.physicMaterial`: 3D PhysicMaterial with bounciness 0.45, friction 0.3.

#### [NEW] `Assets/_Project/Prefabs/Gameplay/Ball.prefab`
- 3D Sphere primitive, Layer 6 (`Projectile`), SphereCollider with `PM_Ball`.
- Rigidbody (mass 1.5, Continuous collision detection, interpolate).
- `ProjectileController` component.

#### [NEW] UI Canvas & HUD
- `MainCanvas` in `MainGame.unity`:
  - Canvas Scaler: Scale with Screen Size, 1080 x 1920 (Portrait), Match 0.5.
  - Safe Area / Top-left HUD Card: Ball icon + TextMeshPro remaining count (`"25"`).
  - `HUDController` component wired to `MortarController`.

---

## 3. Verification Plan

### Automated & MCP-Driven Verification
1. **Compilation Check**:
   - `read_console(action="get", types=["error", "warning"])` -> Ensure 0 errors.
2. **Behavioral Play Mode Test**:
   - Enter Play mode via `manage_editor(action="play")`.
   - Run automated test script:
     - Test 1: First ball fires immediately without cooldown.
     - Test 2: Rapid successive taps within 0.1s are blocked by cooldown (~0.5s).
     - Test 3: Ball count decrements from 25 to 24, and HUD text updates to "24".
     - Test 4: Spawned ball has Rigidbody velocity toward world target, follows ballistic gravity arc.
     - Test 5: Ball fired into empty space travels into world depth, begins shrinking at ~4s, and auto-destroys at ~5s.
   - Capture screenshot with `manage_camera(action="screenshot", capture_source="game_view", include_image=true)`.
   - Stop Play mode.
3. **Console Log Check**: Ensure 0 runtime exceptions.

### Acceptance Criteria Checklist
- [ ] Tapping fires a ball toward the tapped world point
- [ ] Ball follows physics arc (gravity affects trajectory)
- [ ] Ball auto-destroys after ~5 seconds with fade/shrink effect
- [ ] Cooldown prevents rapid-fire
- [ ] Ball counter decrements correctly
- [ ] Firing into empty space: ball travels into depth without sticking to 2D plane
- [ ] First ball of level has NO cooldown
- [ ] Remaining Balls HUD counter displays correctly
