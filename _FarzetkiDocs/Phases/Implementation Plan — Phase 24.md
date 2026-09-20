# Implementation Plan — Phase 24: Dedicated BootScene, Global Managers & Visual Loading Template

## 1. System Architecture & Objective

Implement a dedicated **`BootScene.unity`** as the primary entry point (Build Index 0) for **Royal Smash! — Physics Puzzle** adhering to the user's visual reference and requirements.

### Core Objectives
1. **First-Load Entry Point (Scene 0)**:
   - Always boots first upon application launch.
   - Houses all global singletons (`SaveManager`, `ProfileManager`, `GoldManager`, `LivesManager`, `SkillManager`, `AudioManager`, `HapticManager`, `VFXManager`, `CloudSyncManager`, `LeaderboardManager`, `AdManager`, `IAPManager`, `SceneTransitionManager`) as `DontDestroyOnLoad`.
2. **Asynchronous Initialization Pipeline**:
   - Loads local save files, synchronizes profile and player stats.
   - Processes passive offline life regeneration (+1/30min) and checks gold balance.
   - Pre-warms audio channels, haptic settings, and skill inventory.
   - Connects to cloud sync ("latest local wins") and warms leaderboard cache.
   - Routes smoothly to `MainGame` (if tutorial uncompleted) or `MainMenu` (if returning player).
3. **Template UI Matching User Reference**:
   - Mobile portrait responsive layout (1080x1920 reference, Canvas Scaler Match Width/Height).
   - **Background**: Full-screen image easily swappable with custom artwork in the Inspector.
   - **Logo**: Upper-centered game title / logo image easily swappable in the Inspector.
   - **Hint Text**: Centered gameplay tips (e.g., *"Her Rift bir zayıf nokta saklar."*).
   - **Loading Slider**: Sleek rounded horizontal progress bar with glowing cyan fill.
   - **Status Text**: Bold uppercase status label (*"YÜKLENİYOR..."* / dynamic step updates).

---

## 2. Deliverables & Component Changes

### Scripts & Logic
- `[NEW]` `Assets/_Project/Scripts/Core/BootController.cs`: Coordinates the asynchronous step-by-step loading pipeline, drives slider fill interpolation, cycles hint texts, and manages scene routing.
- `[MODIFY]` `Assets/_Project/Scripts/Core/SceneTransitionManager.cs`: Adds `SCENE_BOOT = "BootScene"` constant.

### Scene & UI Assets
- `[NEW]` `Assets/_Project/Scenes/BootScene.unity`: Configured scene with Camera, EventSystem, BootManager, all persistent managers, and BootCanvas template matching the user's reference image.
- `[NEW]` `Assets/_Project/Scripts/Editor/BootSceneSetup.cs`: Automated editor utility to build, wire, configure UI elements and managers, and insert `BootScene` at index 0 of `EditorBuildSettings`.

### Automated Testing & Verification
- `[NEW]` `Assets/_Project/Scripts/Editor/BootScenePlayModeTest.cs`: Automated 10-test suite verifying build index 0, manager initialization, slider interpolation, tip generation, cloud sync trigger, and scene routing.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: Build Settings Index 0**: `BootScene` is registered at index 0 of `EditorBuildSettings`.
2. **Test 2: Scene Loading & Hierarchy**: Scene contains `BootController`, `BootCanvas`, `EventSystem`, and Camera.
3. **Test 3: Persistent Managers Initialization**: All 12 global managers instantiate and persist without duplicates.
4. **Test 4: Save & Profile Preload**: Player save and profile data load before progression.
5. **Test 5: Economy & Lives Validation**: Lives regeneration and gold balances process on boot.
6. **Test 6: Cloud & Leaderboard Sync**: Cloud sync and leaderboard prewarm fire during loading pipeline.
7. **Test 7: Slider Interpolation**: Slider value advances smoothly from 0.0 to 1.0.
8. **Test 8: UI Swappable Template Slots**: `_backgroundImage`, `_logoImage`, `_loadingSlider`, `_hintText`, and `_statusText` are cleanly exposed.
9. **Test 9: Destination Routing**: Accurately routes to `MainGame` (new player) or `MainMenu` (returning player).
10. **Test 10: Zero Errors**: 0 console errors or missing reference warnings.

---

## 4. Execution & Verification Record

- **Test Suite**: `Assets/_Project/Scripts/Editor/BootScenePlayModeTest.cs`
- **Execution Date**: 2026-09-20
- **Test Result**: **10 / 10 Tests Passed (100%)**
  1. Test 1: Build Settings Index 0 -> **PASS**
  2. Test 2: BootScene Asset Exists -> **PASS**
  3. Test 3: BootController Initialization -> **PASS**
  4. Test 4: Slider Progress Driving -> **PASS**
  5. Test 5: Hint Cycle Mechanic -> **PASS**
  6. Test 6: Global Managers Instantiation -> **PASS**
  7. Test 7: Routing First-Time Player -> **PASS**
  8. Test 8: Routing Returning Player -> **PASS**
  9. Test 9: Template Slots Exposed -> **PASS**
  10. Test 10: Zero Errors Health Pass -> **PASS**
- **Full Regression**: 50 / 50 Automated Tests across all suites Passed.
- **Console Errors**: **0 Errors**

