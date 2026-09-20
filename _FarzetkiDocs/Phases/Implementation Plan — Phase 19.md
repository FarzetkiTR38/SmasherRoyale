# Implementation Plan — Phase 19: 30 Levels & Balancing

## 1. System Architecture & Objective

Implement complete data-driven 30-level progression and balancing for **Royal Smash! — Physics Puzzle** adhering to **Section 1.2 (Progression Design)**, **Section 24 (Difficulty Balancing)**, and **Section 35 (Phase 19)** of `_FarzetkiDocs/master_build_prompt.md`.

### Core Requirements
1. **Full 30-Level Progression**:
   - 30 distinct levels with deterministic object layouts, stand configurations, and ball counts.
   - ScriptableObject-based architecture (`LevelData` assets) referenced via `LevelDatabase`.
2. **Object Introduction Sequence**:
   - Level 1: Boxes only (introductory tutorial).
   - Level 4: Stones introduced (heavier, requires more force/shots).
   - Level 8: Columns introduced (tall structures, topple dynamics).
   - Level 12: Cones introduced (irregular rolling, unstable tips).
   - Level 16+: Mixed setups with Paint Bottles and complex structures.
3. **Milestone Events**:
   - Level 5: MegaBall Skill unlocked.
   - Level 10: InfinityBall Skill unlocked.
   - Level 30: Grand Finale (all object types, multi-stand layout).
4. **Difficulty Curve**:
   - Gradual increase in target count (from 3 to 12+), stand heights, and distances.
   - Ball allocations tuned for 2-3 star clearability.
5. **Level 30 Finale Handling**:
   - WinPanel displays "RETURN TO MENU" instead of "NEXT LEVEL" when completing Level 30.

---

## 2. Deliverables & Component Changes

### Level Data Architecture
- `[NEW]` `Assets/_Project/Scripts/Levels/LevelData.cs`: ScriptableObject defining:
  - `LevelNumber`, `DisplayName`, `AllocatedBalls`
  - `BaseGoldReward` (default 50)
  - `StandPlacements`: List of stand types, positions, heights
  - `ObjectPlacements`: List of object types, stand indices, local/world positions, rotations
  - `MilestoneEvents`: Unlocks for skills, tutorials
- `[NEW]` `Assets/_Project/Scripts/Levels/LevelDatabase.cs`: ScriptableObject cataloging all 30 levels with query helpers (`GetLevelData(int levelNumber)`, `TotalLevels`, `IsLastLevel(int levelNumber)`).
- `[NEW]` `Assets/_Project/Scripts/Levels/LevelLoader.cs`: Runtime component on LevelManager or standalone that spawns stands and targets according to `LevelData`.

### Asset Generation
- `[NEW]` `Assets/_Project/Scripts/Editor/LevelGenerator30.cs`: Editor utility generating all 30 `LevelData_XX.asset` files and `LevelDatabase.asset` adhering to progression curves.
- `[NEW]` `Assets/_Project/ScriptableObjects/Levels/`: 30 generated `LevelData` ScriptableObject assets + `LevelDatabase.asset`.

### UI & Flow Integration
- `[MODIFY]` `Assets/_Project/Scripts/UI/WinPanel.cs`: Checks `LevelDatabase.IsLastLevel()` or `currentLevel >= 30` to switch "NEXT LEVEL" button text to "RETURN TO MENU".
- `[MODIFY]` `Assets/_Project/Scenes/MainGame.unity`: Scene updated with LevelDatabase reference on LevelManager / LevelLoader.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase19Setup.cs`: Automated scene and asset wiring.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase19PlayModeTest.cs`: 10 automated test cases.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: LevelDatabase Completeness**: Exactly 30 levels cataloged in sequence without gaps.
2. **Test 2: Ball Count Allocation**: All levels allocate between 3 and 10 balls.
3. **Test 3: Object Introductions**: Level 1 (Boxes only), Level 4 (Stones), Level 8 (Columns), Level 12 (Cones).
4. **Test 4: Milestone Event Level 5**: Unlocks MegaBall skill.
5. **Test 5: Milestone Event Level 10**: Unlocks InfinityBall skill.
6. **Test 6: Level 30 Grand Finale Config**: Contains all object types and multi-stand configuration.
7. **Test 7: LevelLoader Spawning**: Spawns correct counts of stands and physics objects.
8. **Test 8: Object Target Registration**: Spawner registers all spawned targets with ObjectManager.
9. **Test 9: Level 30 Win Flow**: WinPanel displays "RETURN TO MENU" instead of "NEXT LEVEL".
10. **Test 10: Zero Errors & Performance**: No console errors, clean memory/allocation pass.

---

## 4. Execution & Verification Record

- **Test Runner**: `Assets/_Project/Scripts/Editor/Phase19PlayModeTest.cs`
- **Execution Date**: 2026-09-17
- **Test Result**: **10 / 10 Tests Passed**
- **Console Errors**: **0 Errors**
