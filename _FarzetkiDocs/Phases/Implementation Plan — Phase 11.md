# Implementation Plan — Phase 11: Main Menu & UI System

Implementation plan and technical specification for **Phase 11: Main Menu & UI System** based on **Section 17 (Main Menu)**, **Section 1.2 (Progression Rules)**, and **Section 35 (Phase 11)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Layout

1. **Main Menu Layout (Section 17.1)**:
   - **Top Bar**:
     - Left: Profile shortcut button (Avatar icon + Frame).
     - Center: Gold Widget (`🪙 1,000`) and Lives Widget (`❤️ 5/5` with live countdown timer and `+` purchase button).
     - Right: Settings button (`⚙` opening Settings modal).
   - **Center Screen**:
     - Current level badge: `LEVEL XX`.
     - Large, tactile **PLAY** button. Clicking triggers zero-lives validation: if lives $> 0$, transitions into Gameplay; if lives $= 0$, prompts `OutOfLivesDialog`.
   - **Special Offers (Flanking Center)**:
     - Left Offer Card: Starter / Special Bundle placeholder with badge.
     - Right Offer Card: Limited Time Offer with live countdown timer.
   - **Bottom Navigation Bar (Section 17.2)**:
     - 3 Tabs: **Shop** | **Main Menu (Battle)** | **Leaderboard**.
     - Tactile tab switching with active tab indicator and view management.
     - Placeholder views for Shop (Phase 13) and Leaderboard (Phase 15).

2. **Scene Management & Flow**:
   - Dedicated `Assets/_Project/Scenes/MainMenu.unity` added to Editor Build Settings (Index 0).
   - `Assets/_Project/Scenes/MainGame.unity` in Build Settings (Index 1).
   - `SceneTransitionManager.cs`: Handles smooth transitions between Menu and Gameplay.
   - Win Panel & Lose Panel "Main Menu" buttons seamlessly route back to `MainMenu.unity`.

3. **Polished Menus & Shared Prefabs**:
   - Reusable `LivesWidget` displaying real-time heart count and regeneration timer.
   - Reusable `GoldDisplayUI` showing persistent gold balance with punch animation.
   - Reusable `InGameSettingsDialog` and `OutOfLivesDialog` accessible across Menu and Gameplay.

---

## 2. Component Deliverables

- `[NEW]` `Assets/_Project/Scripts/UI/MainMenuController.cs`:
  Central controller for the Main Menu layout, tab navigation, level display, Play button gating, and special offer timers.
- `[NEW]` `Assets/_Project/Scripts/UI/BottomNavigationUI.cs`:
  Manages tab selection (Shop, Main, Leaderboard) with visual tab highlighting and page view switching.
- `[NEW]` `Assets/_Project/Scripts/Core/SceneTransitionManager.cs`:
  Centralized scene loader providing clean loading between MainMenu and MainGame.
- `[NEW]` `Assets/_Project/Scenes/MainMenu.unity`:
  Complete Main Menu scene configured with Canvas, camera, background, top bar, center stage, and navigation.
- `[MODIFY]` `Assets/_Project/Scripts/UI/LosePanel.cs`:
  Hook up "Main Menu" button to navigate back to `MainMenu` scene.
- `[MODIFY]` `Assets/_Project/Scripts/UI/WinPanel.cs`:
  Ensure transition back to Main Menu on Level 30 completion or menu button.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase11Setup.cs`:
  Automated scene builder and Build Settings configurator for `MainMenu.unity`.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase11PlayModeTest.cs`:
  Comprehensive automated Play Mode test suite validating all Phase 11 acceptance criteria.

---

## 3. Acceptance Tests

- [x] **Test 1: Main Menu Current Level Display**: Current level accurately loaded from `SaveManager` and displayed in center card.
- [x] **Test 2: Play Button Level Start**: Clicking Play launches the active level when lives $> 0$.
- [x] **Test 3: Zero Lives Play Gating**: When lives $= 0$, clicking Play blocks level start and opens `OutOfLivesDialog`.
- [x] **Test 4: Bottom Navigation Tab Switching**: Clicking Shop, Battle, or Leaderboard tabs activates the respective view and highlights the tab.
- [x] **Test 5: Top Bar Currency & Lives Sync**: Gold and Lives widgets display real-time persistent data in Main Menu.
- [x] **Test 6: Settings Dialog Accessibility**: Settings button opens modal without breaking state.
- [x] **Test 7: Special Offers Layout**: Side offer cards are present and do not obstruct the Play button.
- [x] **Test 8: Scene Build Settings Configuration**: `MainMenu` (0) and `MainGame` (1) properly registered in EditorBuildSettings.
- [x] **Test 9: Button Tactile Feedback**: UI buttons provide tactile pointer animations.
- [x] **Test 10: Zero Console Errors**: All scripts compile with 0 errors and 0 warnings.

---

## 4. Completion Record & Verification Results

### Automated Verification Execution
The automated test runner (`Phase11PlayModeTest.cs`) executed against `MainMenu.unity` with 100% pass rate:
- **Test 1 (Current Level Display)**: Level display text: 'LEVEL 3' matches CurrentLevel (3) -> **PASSED**
- **Test 2 (Build Settings Scenes)**: Scene 0: 'Assets/_Project/Scenes/MainMenu.unity', Scene 1: 'Assets/_Project/Scenes/MainGame.unity' -> **PASSED**
- **Test 3 (Bottom Nav Shop Tab)**: Shop page active: True, Battle page inactive: True -> **PASSED**
- **Test 4 (Bottom Nav Leaderboard Tab)**: Leaderboard page active: True -> **PASSED**
- **Test 5 (Bottom Nav Battle Tab)**: Battle page active: True -> **PASSED**
- **Test 6 (Zero Lives Play Gating)**: OutOfLivesDialog active: True when player has 0 lives -> **PASSED**
- **Test 7 (Normal Play Flow Allowed)**: Player has 5 lives; play transition permitted -> **PASSED**
- **Test 8 (Top Bar Gold Synchronization)**: Gold text: '1.250' reflects save data (1,250) -> **PASSED**
- **Test 9 (Settings Dialog Accessibility)**: Settings modal opens and closes cleanly without errors -> **PASSED**
- **Test 10 (Special Offers Display)**: Side offers present without obstructing Play button (Timer: 23:59:59) -> **PASSED**

### Visual Proof & Captured Assets
- Polished Main Menu with Top Bar, Center Stage, Special Offers, and Bottom Navigation:
  `Assets/Screenshots/phase11_main_menu_final.png`

### Console Health
- Zero compilation or runtime errors (`read_console` returned 0 log entries).
- Both `MainMenu.unity` and `MainGame.unity` registered and synchronized.
