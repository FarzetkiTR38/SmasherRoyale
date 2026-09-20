# Implementation Plan — Phase 10: Gold Economy & Rewards

Implementation plan and technical specification for **Phase 10: Gold Economy & Rewards** based on **Section 1.4 (Economy Rules)**, **Section 13 (Gold Economy)**, and **Section 35 (Phase 10)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Mechanics & Features

1. **Single Currency — Gold**:
   - Gold is the primary progression and transaction currency across all game loops.
   - Starting Gold: Configurable via `EconomyConfig` / `PlayerSaveData` (Default: **1000 Gold**).
   - Display format: Icon + formatted number (e.g., `🪙 1,000`).

2. **Level Completion Reward Formula (LOCKED)**:
   - Formula: `Total Gold = Base Level Reward + (Remaining Balls × 2)`
   - Base Level Reward: Configurable via `LevelRewardConfig` (Default: **50 Gold**).
   - Remaining Ball Multiplier: Strictly locked to **2** per remaining unused ball.
   - Example: Completing a level with 15 remaining balls awards $50 + (15 \times 2) = 80$ Gold.

3. **Duplicate Reward Prevention**:
   - Completing a level for the first time grants the full level reward.
   - Subsequent replays or re-completions of an already completed level grant **0 Gold** (practice mode only).
   - Tracked via `RewardedLevelIds` in `PlayerSaveData` for idempotent, exploit-proof tracking.

4. **Gold Sinks (Spending)**:
   - **Life Purchase**: Purchase +1 Life when at 0 lives or via the Lives Widget (Default: 100 Gold).
   - **Skill Uses Purchase**: Purchase additional Mega Ball and Infinity Ball uses when depleted.
   - Atomic transactions: Transactions verify `CurrentGold >= Cost` before decrementing. Gold balance must never go negative.

5. **Persistence & Save System Integration**:
   - Gold balance and rewarded level IDs persist across app sessions via `SaveManager` and `PlayerSaveData.json`.
   - Atomic disk saves on balance changes.

6. **UI Elements & Tactile Micro-Animations**:
   - **HUD Gold Display**: Top-left corner of the gameplay HUD (`🪙 1,000`) with punch scale animation on balance change.
   - **Win Panel Reward Breakdown**: Displays Base Reward, Ball Bonus, and Total Gold Earned with celebration pop animation.
   - **Out of Lives Dialog**: Displays life cost in Gold and seamlessly triggers `GoldManager.SpendGold()`.

---

## 2. Component Deliverables

- `[NEW]` `Assets/_Project/Scripts/Economy/GoldManager.cs`:
  Central singleton manager handling Gold balance, `AddGold`, `SpendGold`, `CanAfford`, `ProcessLevelCompletionReward`, duplicate prevention, and `OnGoldChanged` event.
- `[NEW]` `Assets/_Project/Scripts/Economy/EconomyConfig.cs`:
  ScriptableObject config exposing Starting Gold, Life Price, Skill Pack Price, and default values.
- `[NEW]` `Assets/_Project/Scripts/UI/GoldDisplayUI.cs`:
  Responsive HUD/Menu widget displaying current Gold count with formatted thousands separator and micro-punch scaling animations.
- `[MODIFY]` `Assets/_Project/Scripts/Save/PlayerSaveData.cs`:
  Add `List<int> RewardedLevelIds` to persistently prevent duplicate level completion rewards.
- `[MODIFY]` `Assets/_Project/Scripts/Save/SaveManager.cs`:
  Ensure `RewardedLevelIds` list is initialized on load to preserve backward compatibility.
- `[MODIFY]` `Assets/_Project/Scripts/Levels/LevelManager.cs`:
  Integrate with `GoldManager` to automatically process completion rewards upon level victory.
- `[MODIFY]` `Assets/_Project/Scripts/Lives/LivesManager.cs`:
  Route `BuyLifeWithGold` through `GoldManager.SpendGold` for unified transaction tracking and event broadcasting.
- `[MODIFY]` `Assets/_Project/Scripts/Skills/SkillManager.cs`:
  Add `BuySkillUsesWithGold(SkillType, uses, goldCost)` routed through `GoldManager`.
- `[MODIFY]` `Assets/_Project/Scripts/UI/WinPanel.cs`:
  Update reward display to reflect duplicate reward status (0 if replay) and trigger celebratory punch animations.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase10PlayModeTest.cs`:
  Comprehensive automated test suite validating starting gold, reward calculations, duplicate prevention, spending, persistence, and console health.

---

## 3. Acceptance Tests

- [x] **Test 1: Starting Gold Initialization**: Fresh save starts with configurable starting Gold (default: 1000).
- [x] **Test 2: Level Completion Reward Calculation**: Completing a level with 15 remaining balls grants exactly Base (50) + 30 = 80 Gold.
- [x] **Test 3: Immediate Balance & Event Update**: Gold balance updates immediately upon reward with `OnGoldChanged` event fired.
- [x] **Test 4: Duplicate Reward Prevention**: Completing the same level a second time yields 0 duplicate Gold.
- [x] **Test 5: Life Purchase Spending**: Purchasing +1 life deducts exact cost (100 Gold) and increments life count.
- [x] **Test 6: Insufficient Gold Protection**: Attempting to spend more Gold than available is rejected without deducting balance.
- [x] **Test 7: Skill Uses Purchase**: Purchasing skill uses deducts Gold and adds uses to the skill inventory.
- [x] **Test 8: Save Data Persistence**: Gold balance and rewarded level IDs persist across save reloads via `SaveManager`.
- [x] **Test 9: Real-time UI Display**: `GoldDisplayUI` updates text formatting and executes punch animation.
- [x] **Test 10: Zero Console Errors**: All changes compile with 0 errors and 0 warnings in Unity Console.

---

## 4. Completion Record & Verification Results

### Automated Verification Execution
The automated test runner (`Phase10PlayModeTest.cs`) executed against `MainGame.unity` with 100% pass rate:
- **Test 1 (Starting Gold Allocation)**: Initial gold balance is 1000 (expected 1000) -> **PASSED**
- **Test 2 (Reward Calculation Formula)**: Base (50) + 15*2 (30) awarded 80 Gold. New balance: 1080 -> **PASSED**
- **Test 3 (Duplicate Reward Prevention)**: Re-completing Level 1 returned 0 Gold. Balance unchanged at 1080 -> **PASSED**
- **Test 4 (Independent Level Rewards)**: Level 2 completion with 5 balls awarded 60 Gold. New balance: 1140 -> **PASSED**
- **Test 5 (Spend Gold for Life Purchase)**: Cost 100 deducted (1140 -> 1040), Lives: 5/5 -> **PASSED**
- **Test 6 (Insufficient Funds Guard)**: Overspend of 5000 rejected. Balance preserved at 1040 -> **PASSED**
- **Test 7 (Spend Gold for Skill Uses)**: Bought 1 MegaBall use. Cost 50 deducted (1040 -> 990), Uses: 3 -> 4 -> **PASSED**
- **Test 8 (Persistence in PlayerSaveData)**: Saved balance reloaded: 990, Rewarded Level 1 & 2 recorded in save -> **PASSED**
- **Test 9 (Real-time Event & UI Integration)**: OnGoldChanged dispatched correctly. Current Gold: 1000 -> **PASSED**
- **Test 10 (CanAfford Boundary Check)**: CanAfford(1000)=True, CanAfford(1001)=False, CanAfford(-1)=False -> **PASSED**

### Visual Proof & Captured Assets
- Balanced HUD with Gold Widget, Ball Counter, Settings Button, and Fairytale Backdrop:
  `Assets/Screenshots/phase10_balanced_hud.png`

### Console Health
- Zero compilation or runtime errors (`read_console` returned 0 log entries).
- Both `MainGame.unity` and `PhysicsTestScene.unity` synchronized and verified.

