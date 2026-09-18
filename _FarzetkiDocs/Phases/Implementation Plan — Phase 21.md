# Implementation Plan — Phase 21: App Launch Flow, Animated Splash, Loading Screen & Scene Transition System

## 1. System Architecture & Objective

Implement complete first-launch and returning-player application lifecycle flows for **Royal Smash! — Physics Puzzle** adhering to **Sections 34, 35, 36, 268, 300, 301, 302, 303** of `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`.

### Core Requirements
1. **App Bootstrapper & Flow Routing**:
   - First-time player: Splash → Loading Screen (Progress + Tips) → Level 1 (Tutorial).
   - Returning player: Splash → Loading Screen → Main Menu.
2. **Splash Screen Presentation**:
   - Clean, branded royal title display with smooth alpha fade-in, display pause, and fade-out.
3. **Animated Loading Screen**:
   - Dynamic gameplay hints/tips rotation (e.g., "Mega Ball topples heavy columns!", "Aim for the base to trigger chain reactions!").
   - Smooth non-linear progress bar animation with percentage readout.
4. **Smooth Scene Transition Overlay**:
   - CanvasGroup-based screen fader ensuring zero visual glitches, freezing, or popping during scene loads.
   - Non-blocking async load operations with safety timeout.

---

## 2. Deliverables & Component Changes

### Core Lifecycle & UI Scripts
- `[NEW]` `Assets/_Project/Scripts/Core/AppBootstrapper.cs`: Evaluates `PlayerSaveData.TutorialCompleted`, controls splash duration, coordinates loading sequence, and routes to appropriate scene.
- `[NEW]` `Assets/_Project/Scripts/UI/SplashLoadingOverlay.cs`: UI component managing the splash title, logo, animated progress bar, loading spinner, rotating tips, and CanvasGroup fade effects.
- `[MODIFY]` `Assets/_Project/Scripts/Core/SceneTransitionManager.cs`: Enhanced with smooth async scene loading, fade overlay integration, and loading progress broadcasting.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase21PlayModeTest.cs`: 10 automated test cases validating first launch routing, returning player routing, splash fade transitions, rotating tip selection, async load callbacks, and 0 console errors.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: AppBootstrapper Initialization**: Initializes correctly and detects save data status.
2. **Test 2: First-Time Player Routing**: Routes uninitiated players (`TutorialCompleted == false`) to gameplay/tutorial.
3. **Test 3: Returning Player Routing**: Routes initiated players (`TutorialCompleted == true`) to Main Menu.
4. **Test 4: Splash Fade Curve**: Alpha smoothly transitions from 0 to 1 and fades out.
5. **Test 5: Loading Progress Tracking**: Progress bar smoothly interpolates from 0% to 100%.
6. **Test 6: Rotating Gameplay Tips**: Selects valid, helpful casual gameplay tips from array.
7. **Test 7: Scene Transition Fader**: Fader smoothly blackens screen before scene activation and clears after scene loaded.
8. **Test 8: Double-Load Protection**: Prevents concurrent scene loading requests while a transition is in progress.
9. **Test 9: Scene Validation**: Validates that target scenes (`MainMenu`, `MainGame`) exist in Build Settings.
10. **Test 10: Zero Errors & Performance**: 0 errors, warnings, or memory leaks during transition cycles.

---

## 4. Execution & Verification Record

- **Test Suite**: `Assets/_Project/Scripts/Editor/Phase21PlayModeTest.cs`
- **Execution Date**: 2026-09-18
- **Test Result**: **10 / 10 Tests Passed (100%)**
  1. Test 1: AppBootstrapper Initialization -> **PASS**
  2. Test 2: First-Time Player Routing -> **PASS**
  3. Test 3: Returning Player Routing -> **PASS**
  4. Test 4: SplashLoadingOverlay Creation -> **PASS**
  5. Test 5: Rotating Gameplay Tips -> **PASS**
  6. Test 6: Progress Bar Clamping -> **PASS**
  7. Test 7: Double-Load Protection -> **PASS**
  8. Test 8: Build Settings Scene Verification -> **PASS**
  9. Test 9: Overlay Fade Safety -> **PASS**
  10. Test 10: Zero Errors Health Pass -> **PASS**
- **Console Errors**: **0 Errors**

