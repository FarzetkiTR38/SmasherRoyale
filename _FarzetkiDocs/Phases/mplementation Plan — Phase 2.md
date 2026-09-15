# Implementation Plan — Phase 2: Input System & Mortar Movement

Implementation of **Phase 2** for **Royal Smash! — Physics Puzzle** based on the Master Build Prompt, Section 4 (Input & Controls), and Section 35 (Phase 2).

---

## 1. Overview & Goal
Build a mobile-first touch input system that cleanly differentiates between **Drag** (moving/aiming the cannon horizontally) and **Tap** (firing at the tapped 3D world coordinates). Ensure that dragging never triggers accidental fire, the cannon movement is smoothly clamped to gameplay boundaries, and tapping calculates the true 3D world target point with a visual indicator.

---

## 2. Proposed Changes

### Configuration
#### [NEW] [MortarConfig.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/MortarConfig.cs)
- ScriptableObject at `Assets/_Project/ScriptableObjects/Config/MortarConfig.asset`:
  - `moveSpeed`: Horizontal tracking speed multiplier.
  - `smoothTime`: Smoothing duration for horizontal interpolation.
  - `minX`, `maxX`: Boundary limits (e.g. `-2.6f` to `+2.6f`).
  - `dragThresholdPixels`: Threshold (e.g. `12f` px) to differentiate Tap from Drag.
  - `targetPlaneDistance`: Default depth plane distance for world-coordinate raycasting.

---

### Scripts

#### [NEW] [InputHandler.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/InputHandler.cs)
- Uses `UnityEngine.InputSystem.Pointer.current` (seamless across mobile Touchscreen and Editor Mouse).
- Tracks touch start position and accumulates movement delta.
- If distance exceeds `dragThresholdPixels` -> enters `Dragging` state, invokes `OnDragDelta(float screenDeltaX)`.
- On release:
  - If was dragging: ends drag gracefully, **never fires**.
  - If not dragging (total movement <= threshold): calculates 3D world coordinate using `Camera.ScreenPointToRay` intersecting the gameplay target plane / colliders, invokes `OnTapFired(Vector3 worldTargetPoint)`.

#### [NEW] [MortarController.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/MortarController.cs)
- Attached to the `Mortar` prefab.
- Subscribes to `InputHandler` drag and tap events.
- Smoothly moves the cannon along the X axis using `Mathf.SmoothDamp` or lerp, strictly clamped between `[minX, maxX]`.
- Receives fire event with world target coordinates (prepares for ball spawning in Phase 3).
- Logs fire events with exact world coordinates.

#### [NEW] [TapIndicator.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/TapIndicator.cs)
- Visual debug indicator showing the tapped 3D world coordinate.
- Instantiates / moves a glowing ring marker at the target position, animates scale/alpha, and fades out.

---

### Prefabs & Scene Setup

#### [NEW] `Assets/_Project/Prefabs/Gameplay/TapIndicator.prefab`
- Subtle gold/blue ring marker with additive/unlit material that flashes at the tapped world point.

#### [MODIFY] `Assets/_Project/Prefabs/Gameplay/Mortar.prefab`
- Attach `MortarController` component and wire serialized references.

#### [MODIFY] `Assets/_Project/Scenes/MainGame.unity`
- Add `InputManager` GameObject with `InputHandler`.
- Wire `MortarController` and `MortarConfig`.

---

## 3. Verification Plan

### Automated & MCP-Driven Verification
1. **Console Compilation & Error Check**:
   - `read_console(action="get", types=["error", "warning"])` -> Ensure 0 errors.
2. **Behavioral Play Mode Test**:
   - Enter Play mode via `manage_editor(action="play")`.
   - Run in-editor test script via `execute_code` that simulates:
     - Drag motion: asserts cannon position changes within `[minX, maxX]`, asserts no fire event triggered.
     - Tap motion: asserts fire event triggered, asserts world coordinate is correct, asserts visual indicator is spawned.
   - Capture screenshot with `manage_camera(action="screenshot", capture_source="game_view", include_image=true)`.
   - Exit Play mode via `manage_editor(action="stop")`.
3. **Console Log Check**:
   - Ensure clean execution with zero runtime exceptions.

### Acceptance Criteria Checklist
- [ ] Dragging finger moves mortar left/right
- [ ] Mortar cannot leave screen bounds (clamped)
- [ ] Drag does NOT trigger fire
- [ ] Tap triggers fire event with debug log
- [ ] Tap point converts to correct world coordinate
- [ ] Visual indicator shows the tapped 3D point
