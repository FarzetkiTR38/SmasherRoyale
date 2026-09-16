# Implementation Plan — Phase 7: Tutorial System & Save Persistence

Implementation plan and completion record for **Phase 7: First-Launch Tutorial & Save System** based on **Section 8 (Tutorial System)** and **Section 1.1 / 18 / 35** of `_FarzetkiDocs/master_build_prompt.md` and `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`.

---

## 1. Overview & Architecture

### Core Systems

1. **Persistent Save System**:
   - `PlayerSaveData.cs`: Serializable data structure storing progression and settings:
     - `SaveVersion = 1`
     - `CurrentLevel = 1`
     - `TutorialCompleted = false` (tracks first-launch completion)
     - `Gold = 1000`
     - `Lives = 5`, `LifeTimerStartTicks`
     - `MegaBallUnlocked`, `InfinityBallUnlocked`, remaining skill uses
     - `MusicEnabled`, `SfxEnabled`, `VibrationEnabled`
   - `SaveManager.cs`: Persistent singleton manager (`RoyalSmash.Save`).
     - JSON serialization directly to `Application.persistentDataPath/player_save.json`.
     - Atomic write with automatic fallback backup file (`player_save_backup.json`) to prevent corruption.

2. **Tutorial Engine & Lifecycle**:
   - `TutorialConfig.cs` (ScriptableObject) & `TutorialConfig.asset`: Exposes tuning parameters without hardcoded numbers:
     - `TargetRadiusPixels = 160f`
     - `RingPulseSpeed = 2.5f`, `FingerBobSpeed = 3.0f`, `FingerBobDistance = 25f`, `FadeDuration = 0.35f`
     - `InstructionText = "HEDEFE DOKUN VE FIRLAT!"`
     - `InvalidTapHint = "LÜTFEN İŞARETLİ ALANA DOKUNUN!"`
   - `TutorialManager.cs`: Central tutorial singleton (`RoyalSmash.Tutorial`).
     - Evaluates whether Level 1 is active and `SaveManager.Data.TutorialCompleted == false`.
     - Automatically targets the front-center physics object (front golden Cone at $Z \approx 6.4\text{m}$).
     - Validates screen and world proximity on each tap.
     - Automatically triggers attention pulses on unauthorized taps.
     - On first valid hit, executes standard mortar shot, unlocks full control immediately, updates `TutorialCompleted = true`, and persists to disk.
     - **Master Toggle (`EnableTutorial`)**: Serialized checkbox and Context Menu actions in Inspector (`Toggle Tutorial`, `Enable Tutorial`, `Disable Tutorial`, `Reset Tutorial Save`) allowing instant ON/OFF control for testing.

3. **Tutorial Visual Overlay**:
   - `TutorialOverlayUI.cs`: Renders and animates UI widgets under `MainCanvas`:
     - **Target Ring**: Dynamic circular reticle with 4 orientation ticks (`ReticleRing.png`), tracking target world-to-screen coordinate with continuous subtle pulse.
     - **Pointer Arrow**: Gold upward-pointing arrow (`PointerArrow.png`) bobbing below the target.
     - **Instruction Banner**: Top-centered purple card with gold border and TextMeshPro text.
     - **CanvasGroup Non-Blocking Architecture**: `blocksRaycasts = false` and `interactable = false` so touch events pass through to `InputHandler`.

4. **Input Gating**:
   - `InputHandler.cs`:
     - Suppresses dragging while tutorial is active.
     - Validates tap screen/world position against `TutorialManager.Instance.IsTapAllowed`.
     - Rejects off-target taps and triggers the visual attention pulse without expending balls.
     - Immediately releases full control upon first successful shot.

---

## 2. Acceptance Criteria & Test Suite

The automated test suite `Phase7PlayModeTest.cs` validates all requirements in Play Mode:

- [x] **Test 1: First-Launch Initialization**: On fresh save, tutorial activates on Level 1 with target properly bound.
- [x] **Test 2: Input Restriction**: Taps outside the target area are rejected; mortar does not fire; ammunition is preserved; attention pulse triggers.
- [x] **Test 3: First Guided Shot**: Tapping within the target area fires a real physics projectile; ammunition decrements; tutorial completes; save data records `TutorialCompleted = true`.
- [x] **Test 4: Full Control Restored**: Player possesses complete aiming and firing freedom across the stand immediately after the first shot.
- [x] **Test 5: Subsequent Launch Skip**: Rechecking tutorial state when `TutorialCompleted == true` skips the tutorial completely.
- [x] **Test 6: Master Toggle Verification**: Inspector checkbox `EnableTutorial` can disable the system entirely for rapid level testing.

---

## 3. Visual Reference & Scene Integration

- Configured across both `MainGame.unity` and `PhysicsTestScene.unity` via `Phase7TutorialSetup.cs`.
- Visual captures verified:
  - `phase7_tutorial_overlay.png` (Reticle locked to front cone, pointing arrow, top instruction card).
  - `phase7_first_shot_fired.png` (First projectile mid-flight, UI fading out, control active).
