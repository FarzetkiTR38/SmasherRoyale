# Implementation Plan — Phase 12: Profile & Settings

Implementation plan and technical specification for **Phase 12: Profile & Settings** based on **Section 18 (Profile)**, **Section 20 (Settings)**, and **Section 35 (Phase 12)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Requirements

1. **Profile System (Section 18)**:
   - **8 Default Avatars**: All unlocked from the start (no progression or paywall gating). Colorful stylized fantasy/royal archetypes (Knight, Archer, King, Queen, Wizard, Princess, Rogue, Blacksmith).
   - **4 Avatar Frames**: All unlocked from the start (Bronze, Silver, Gold, Amethyst).
   - **Username**:
     - Unlimited, completely free changes.
     - Level 10 prompt: When the player reaches Level 10 for the first time (`CurrentLevel >= 10 && !UsernamePrompted`), a dedicated `UsernamePromptDialog` is triggered in the Main Menu.
   - **Stats Display**:
     - Current Level.
     - Optional basic stats: Total Shots Fired, Total Wins, Completed Levels.
   - **Local Persistence**:
     - Saved in `PlayerSaveData.json` through `SaveManager`.

2. **Settings System (Section 20)**:
   - **Three Toggles**:
     - **Music Toggle (ON/OFF)**: Controls background music audio playback.
     - **Sound (SFX) Toggle (ON/OFF)**: Controls all gameplay sound effects.
     - **Vibration Toggle (ON/OFF)**: Controls haptic feedback.
   - **Dual Accessibility**:
     - Accessible from the **Main Menu** (top-right gear icon).
     - Accessible during **Gameplay** (top-right gear icon in `MainGame`).
   - **Gameplay Non-Destructive Overlay**:
     - Opening Settings during gameplay safely freezes or blocks mortar aiming/firing without breaking physics state, and cleanly restores state upon closing.

3. **Audio & Haptic Infrastructure**:
   - `AudioManager.cs` in `Assets/_Project/Scripts/Audio/`:
     - Central singleton managing BGM and SFX audio sources.
     - Immediately applies mute/volume states based on `SaveManager` settings.
   - `VibrationManager.cs` in `Assets/_Project/Scripts/Utils/`:
     - Centralized haptic triggering respecting `VibrationEnabled`.

---

## 2. Component Deliverables

### Scripts & Data Models
- `[MODIFY]` `Assets/_Project/Scripts/Save/PlayerSaveData.cs`:
  - Add `Username` (string, default `"Player"`), `AvatarId` (int, 0..7), `FrameId` (int, 0..3), `UsernamePrompted` (bool), `TotalShotsFired` (int), `TotalWins` (int), `CompletedLevelsCount` (int).
- `[NEW]` `Assets/_Project/Scripts/Profile/ProfileConfig.cs`:
  - ScriptableObject defining avatar definitions (ID, name, color, icon) and frame definitions (ID, name, border color, border style).
- `[NEW]` `Assets/_Project/ScriptableObjects/Config/ProfileConfig.asset`:
  - Populated configuration with 8 vibrant avatars and 4 distinct frames.
- `[NEW]` `Assets/_Project/Scripts/Audio/AudioManager.cs`:
  - Singleton managing background music and SFX channels with persistent audio listener and settings synchronizer.
- `[NEW]` `Assets/_Project/Scripts/Utils/VibrationManager.cs`:
  - Haptic feedback utility honoring vibration toggle.
- `[NEW]` `Assets/_Project/Scripts/Profile/ProfileManager.cs`:
  - Central manager for player profile state, avatar/frame selection, username validation, stats tracking, and Level 10 prompt gating.
- `[NEW]` `Assets/_Project/Scripts/UI/ProfileDialog.cs`:
  - Full-featured profile modal with current avatar/frame display, username input/edit, 8-avatar selector grid, 4-frame selector grid, and stats breakdown.
- `[NEW]` `Assets/_Project/Scripts/UI/UsernamePromptDialog.cs`:
  - Dedicated prompt dialog that triggers at Level 10 prompting the player to enter their Royal Username.
- `[MODIFY]` `Assets/_Project/Scripts/UI/InGameSettingsDialog.cs`:
  - Add Vibration toggle button and label.
  - Update toggle labels to clear English `ON` / `OFF` states with vibrant green / neutral gray color coding.
  - Connect audio and vibration toggles to `AudioManager` and `VibrationManager`.
  - Ensure safe gameplay state pause/resume.
- `[MODIFY]` `Assets/_Project/Scripts/UI/MainMenuController.cs`:
  - Wire `_profileButton` to open `ProfileDialog`.
  - Add automatic check for Level 10 username prompt on menu start.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase12Setup.cs`:
  - Editor automation to build procedural avatar/frame textures, instantiate `ProfileConfig.asset`, inject `ProfileDialog` and `UsernamePromptDialog` into `MainMenu.unity`, and update `InGameSettingsDialog` across both scenes.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase12PlayModeTest.cs`:
  - 10 automated acceptance tests verifying all Phase 12 deliverables.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: Avatar Selection (8 Options)**:
   - All 8 avatars can be selected freely without restrictions.
   - Selected avatar updates `PlayerSaveData.AvatarId` and persists across saves.
2. **Test 2: Frame Selection (4 Options)**:
   - All 4 frames can be selected freely without restrictions.
   - Selected frame updates `PlayerSaveData.FrameId` and persists across saves.
3. **Test 3: Username Input at Level 10**:
   - When player reaches Level 10 and `UsernamePrompted == false`, `UsernamePromptDialog` is triggered.
   - Submitting username marks `UsernamePrompted = true` and updates save data.
4. **Test 4: Username Change Unlimited & Free**:
   - Player can change username repeatedly through `ProfileDialog` at no cost.
5. **Test 5: Music Toggle Mutes/Unmutes BGM**:
   - Toggling Music updates `PlayerSaveData.MusicEnabled` and synchronizes with `AudioManager`.
6. **Test 6: Sound (SFX) Toggle Mutes/Unmutes SFX**:
   - Toggling Sound updates `PlayerSaveData.SfxEnabled` and synchronizes with `AudioManager`.
7. **Test 7: Vibration Toggle Enables/Disables Haptics**:
   - Toggling Vibration updates `PlayerSaveData.VibrationEnabled` and controls `VibrationManager.CanVibrate`.
8. **Test 8: Settings Persistence**:
   - Saving settings, re-initializing `SaveManager`, and verifying settings survive reload intact.
9. **Test 9: Profile Stats Accurate Display**:
   - Level, shots fired, and wins stats correctly render in `ProfileDialog`.
10. **Test 10: In-Game Settings Non-Destructive Overlay**:
    - Opening settings during gameplay in `MainGame` safely pauses/restores game state without corrupting gameplay.

---

## 4. Completion Record & Verification Status

### Automated Test Execution Results (10/10 Passed)
Executed via `Phase12PlayModeTest.cs`:
- [x] **Test 1: Avatar Selection (8 Options)** — All 8 avatars selectable and persistent in `PlayerSaveData` -> **PASSED**
- [x] **Test 2: Frame Selection (4 Options)** — All 4 frames selectable and persistent in `PlayerSaveData` -> **PASSED**
- [x] **Test 3: Username Input at Level 10** — Triggered at Level 10, confirmed and saved with `UsernamePrompted = true` -> **PASSED**
- [x] **Test 4: Username Unlimited Free Changes** — Changed multiple times with zero gold cost -> **PASSED**
- [x] **Test 5: Music Toggle Mutes/Unmutes BGM** — Synchronized between SaveManager and `AudioManager` -> **PASSED**
- [x] **Test 6: Sound (SFX) Toggle Mutes/Unmutes SFX** — Synchronized between SaveManager and `AudioManager` -> **PASSED**
- [x] **Test 7: Vibration Toggle Enables/Disables Haptics** — Synchronized between SaveManager and `VibrationManager` -> **PASSED**
- [x] **Test 8: Settings Persistence** — Settings combination survived save and reload -> **PASSED**
- [x] **Test 9: Profile Stats Accurate Display** — Level, shots, wins, and clears accurately mapped and rendered -> **PASSED**
- [x] **Test 10: In-Game Settings Non-Destructive Overlay** — Non-destructive pause/resume verified in `MainGame.unity` -> **PASSED**

### Visual Artifacts & Proofs
- `Assets/Screenshots/phase12_profile_dialog.png` — Player Profile dialog displaying avatar selector, frame selector, username edit, and live stats.
- `Assets/Screenshots/phase12_username_prompt.png` — Level 10 modal prompt dialog for Royal Name entry.
- `Assets/Screenshots/phase12_settings_dialog.png` — Settings modal displaying single-line SETTINGS title, 3 toggles (Music, Sound SFX, Vibration), and Back button.
- `Assets/Screenshots/phase12_main_menu.png` — Clean Main Menu view with dynamic top-left Profile button reflecting active avatar & frame.

### Console Verification
- Unity Console retrieved: **0 errors, 0 warnings**.

