# Implementation Plan — Phase 5: Stand & Level Completion Logic

Implement the complete Stand platform system, Win/Lose condition evaluator with physics settling checks, victory delay timing, locked Gold calculation formula, and polished TextMeshPro Win/Lose UI panels.

## 1. Overview & Architecture

Based on **Section 8 (Stand / Platform)** and **Section 10 (Win / Lose System)** of `_FarzetkiDocs/master_build_prompt.md`:

### Core Systems
1. **Stand Architecture**:
   - `StandConfig.cs` (ScriptableObject): Defines platform dimensions (width, depth, height), pillar style, and dual-platform layout configuration.
   - `StandController.cs`: Builds and scales the platform dynamically based on `StandConfig`. Supports up to **2 independent platforms** per level (e.g. front/back or left/right).
   - Visual Style: Royal pedestal theme (golden fluted pillar, round base, purple tabletop with rounded beveled edge) matching the cartoon reference screenshot.
   - Physics Layer: `Stand` (Layer 8). Static, non-moving, high friction.

2. **Level Completion & Win/Lose Evaluator**:
   - `LevelManager.cs` (Singleton / Service in Gameplay):
     - Tracks registered `PhysicsObject`s that count as required targets.
     - Subscribes to `OnStateChanged` / `OnObjectCleared` events from `PhysicsObject`.
     - Tracks active `ProjectileController` instances in the scene.
     - **Physics Settling Check**: When last target enters destruction, verifies physics motion has settled before confirming victory (prevents premature win).
     - **Victory Delay**: Configurable timer (default $2.0\text{s}$) between clearing and showing the Win Panel.
     - **Lose Condition**: Fired when `RemainingBalls == 0` AND all in-flight projectiles have destroyed/settled AND remaining targets $> 0$.
     - **Reward Formula (Locked)**:
       $$\text{Total Gold} = \text{Base Level Reward} + (\text{Remaining Balls} \times 2)$$
       (Default base reward: 50 Gold, fixed multiplier: 2).

3. **Win & Lose UI Panels**:
   - Built on Canvas with TextMeshPro (no legacy UI Text).
   - `WinPanel.cs`:
     - Level Complete title ("LEVEL COMPLETED!").
     - Base Gold reward text.
     - Remaining Balls bonus text ($N \times 2\text{ Gold}$).
     - Total Gold earned counter.
     - "Next Level" / "Continue" button.
   - `LosePanel.cs`:
     - Level Failed title ("LEVEL FAILED").
     - "Out of balls!" indicator.
     - "Retry" button (reloads the current level / scene).
     - "Main Menu" button (stubbed for future menu navigation).
   - `HUDController.cs`: Decoupled panel management with target count badge.

---

## 2. Acceptance Tests
- [ ] All objects cleared → ~2 sec delay → Win Panel appears
- [ ] Balls run out with objects remaining → Lose Panel appears
- [ ] Objects falling off stand count as "cleared"
- [ ] Physics settles before win is confirmed (no premature win)
- [ ] Win Panel shows correct remaining ball count
- [ ] Retry button reloads the level
- [ ] Win reward calculation: Base + (Remaining × 2)
