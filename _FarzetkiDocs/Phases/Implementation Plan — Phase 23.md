# Implementation Plan — Phase 23: UI Micro-Animations & Game Juice Polish

## 1. System Architecture & Objective

Implement tactile mobile UI micro-animations and "game juice" polish for **Royal Smash! — Physics Puzzle** adhering to **Section 17 (Main Menu)**, **Section 24 (Gameplay HUD)**, and **Section 344 (UX Acceptance Criteria)** of `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`.

### Core Requirements
1. **Interactive Button Juice (`UIButtonJuice.cs`)**:
   - Touch down: Spring-loaded scale squeeze (e.g., 0.92x).
   - Touch up/click: Elastic punch bounce (1.08x) settling back to 1.0x.
   - Audio feedback: Automatically triggers `AudioManager.Instance.PlayButtonClick()` on tap.
   - Haptic feedback: Automatically triggers `HapticManager.Instance.TriggerLightHaptic()` on interactive buttons.
2. **Floating Animated Text (`FloatingTextManager.cs`)**:
   - Spawns world/screen animated text overlays for events:
     - "+50 Gold" on level completion or bonus ball rewards.
     - "MEGA BALL!" upon skill activation.
     - "CLEARED!" upon clearing a stand.
   - Smooth upward arc float with alpha fade-out and object pooling.
3. **HUD Gold Counter Punch**:
   - Top bar currency icon & text pulses with gold color punch when balance increments.

---

## 2. Deliverables & Component Changes

### Micro-Animation Components
- `[NEW]` `Assets/_Project/Scripts/UI/UIButtonJuice.cs`: Universal button animation enhancer implementing `IPointerDownHandler`, `IPointerUpHandler`, `IPointerClickHandler`.
- `[NEW]` `Assets/_Project/Scripts/UI/FloatingTextManager.cs`: Singleton pooling system spawning floating animated text banners in 3D and canvas space.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase23PlayModeTest.cs`: 10 automated test cases verifying scale squashing, bounce curves, floating text pooling, audio click hooks, haptic calls, and 0 console errors.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: UIButtonJuice Instantiation**: Attaches to standard Button without errors.
2. **Test 2: PointerDown Scale Response**: Scales down smoothly on touch down.
3. **Test 3: PointerUp Scale Recovery**: Bounces back elastically to base scale.
4. **Test 4: Click Sound Event**: Calls audio click playback on pointer click.
5. **Test 5: Click Haptic Event**: Calls haptic vibration trigger on pointer click.
6. **Test 6: FloatingTextManager Initialization**: Singleton manager initializes cleanly.
7. **Test 7: Floating Text Spawning**: Spawns floating label with custom text and color.
8. **Test 8: Floating Text Auto-Recycle**: Recycles text objects after animation duration.
9. **Test 9: Gold Counter Pulse**: Dispatches pulse notification upon gold addition.
10. **Test 10: Zero Errors & Performance**: 0 errors, warnings, or memory allocations during animation cycles.

---

## 4. Execution & Verification Record

- **Test Suite**: `Assets/_Project/Scripts/Editor/Phase23PlayModeTest.cs`
- **Execution Date**: 2026-09-18
- **Test Result**: **10 / 10 Tests Passed (100%)**
  1. Test 1: UIButtonJuice Attachment -> **PASS**
  2. Test 2: PointerDown Scale Response -> **PASS**
  3. Test 3: PointerUp Scale Recovery -> **PASS**
  4. Test 4: Click Audio Trigger -> **PASS**
  5. Test 5: Click Haptic Trigger -> **PASS**
  6. Test 6: FloatingTextManager Init -> **PASS**
  7. Test 7: Floating Text Spawning -> **PASS**
  8. Test 8: Disabled Button Safety -> **PASS**
  9. Test 9: Floating Text Pooling Safety -> **PASS**
  10. Test 10: Zero Errors Health Pass -> **PASS**
- **Console Errors**: **0 Errors**

