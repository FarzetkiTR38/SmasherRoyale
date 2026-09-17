# Implementation Plan — Phase 16: Audio & SFX System

Implementation plan and technical specification for **Phase 16: Audio & SFX System** based on **Section 21 (Audio System)** and **Section 35 (Phase 16)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Requirements

1. **Section 21 SFX Catalog & Object-Specific Audio**:
   - **Ball Launch**: Mortar firing burst sound.
   - **Box Impact**: Snappy, light wooden impact sound.
   - **Stone Impact**: Heavy, solid bass thud.
   - **Column Impact**: Deep, resonant stone impact.
   - **Cone Impact**: Light, hollow hit.
   - **Paint Bottle Break**: Distinct glass shatter + liquid splash sound.
   - **Object Destruction**: Crumble/dissolve sound.
   - **UI Button Click**: Crisp UI click sound.
   - **Win**: Ascending royal victory fanfare.
   - **Lose**: Somber defeat tone.
   - **Reward / Gold**: Bright coin collect chime.

2. **Background Music (BGM)**:
   - Dedicated looping background music tracks for **Main Menu** and **Gameplay**.
   - Smooth track transitions and independent volume control.

3. **Channel Separation & Settings Enforcement**:
   - Dedicated `_musicSource` and multi-voice `_sfxSources` pool.
   - Mute synchronization with `PlayerSaveData.MusicEnabled` and `PlayerSaveData.SfxEnabled`.
   - Voice limiting (concurrency clamping) to prevent audio clipping during dense object cascades.
   - Subtle pitch variation (0.94x - 1.06x) for realistic, non-repetitive physical impacts.

4. **Procedural Audio Synthesis**:
   - Synthesizes clean 44.1kHz 16-bit PCM `.wav` audio assets into `Assets/_Project/Audio/SFX/` and `Assets/_Project/Audio/Music/`.

---

## 2. Component Deliverables

### Data Models & Configuration
- `[NEW]` `Assets/_Project/Scripts/Audio/AudioConfig.cs`:
  - ScriptableObject referencing all SFX clips, BGM tracks, volume multipliers, pitch randomization range, and voice concurrency limits.
- `[NEW]` `Assets/_Project/ScriptableObjects/Config/AudioConfig.asset`:
  - Configured instance holding all synthesized audio clips.

### Audio Generation & Core Managers
- `[NEW]` `Assets/_Project/Scripts/Editor/ProceduralAudioGenerator.cs`:
  - Synthesis tool generating 44.1kHz 16-bit PCM WAV assets for all 11 SFX and BGM tracks.
- `[MODIFY]` `Assets/_Project/Scripts/Audio/AudioManager.cs`:
  - Upgraded with object-specific playback methods (`PlayImpact`, `PlayBottleBreak`, `PlayLaunch`, `PlayWin`, `PlayLose`, `PlayReward`), multi-source voice pooling, pitch randomization, and concurrency throttling.

### Gameplay & UI Integration
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/Mortar.cs`:
  - Trigger `AudioManager.Instance.PlayLaunchSound()` on ball launch.
- `[MODIFY]` `Assets/_Project/Scripts/Gameplay/PhysicsObject.cs`:
  - Trigger object-specific sound (`PlayImpact`) based on object type and collision impulse.
- `[MODIFY]` `Assets/_Project/Scripts/Objects/PaintBottle.cs`:
  - Trigger `PlayBottleBreak()` on fracture/shatter.
- `[MODIFY]` `Assets/_Project/Scripts/UI/WinPanel.cs` & `Assets/_Project/Scripts/UI/LosePanel.cs`:
  - Trigger `PlayWinSound()` and `PlayLoseSound()`.
- `[MODIFY]` `Assets/_Project/Scripts/UI/MainMenuController.cs` & `Assets/_Project/Scripts/Levels/LevelManager.cs`:
  - Start respective BGM tracks.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase16Setup.cs`:
  - Automation script to synthesize all WAV files, construct `AudioConfig.asset`, wire `AudioManager` in both scenes.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase16PlayModeTest.cs`:
  - Automated integration test suite validating all 10 Phase 16 acceptance criteria.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: BGM Playback in Main Menu & Gameplay**:
   - Main Menu and Gameplay scenes trigger appropriate looping background music.
2. **Test 2: Distinct Impact Audio by Object Type**:
   - Box, Stone, Column, and Cone play distinct audio clips with different acoustic profiles.
3. **Test 3: Paint Bottle Break Audio**:
   - Paint bottle impact/break triggers glass shatter + liquid splash clip.
4. **Test 4: Ball Launch Mortar Audio**:
   - Firing mortar triggers the ball launch sound effect.
5. **Test 5: Win & Lose Fanfares**:
   - Level victory plays victory fanfare; defeat plays lose sound.
6. **Test 6: Coin & Gold Reward Chime**:
   - Gold acquisition triggers coin collect chime.
7. **Test 7: Music Toggle Mutes Music**:
   - Disabling `MusicEnabled` in settings immediately mutes the music channel.
8. **Test 8: SFX Toggle Mutes Sound Effects**:
   - Disabling `SfxEnabled` in settings immediately blocks all sound effects.
9. **Test 9: Voice Clamping & Anti-Stacking**:
   - Concurrency throttling prevents speaker clipping during simultaneous object cascades.
10. **Test 10: Zero Console Errors**:
    - All audio systems compile and execute cleanly with 0 console errors and 0 warnings.

---

## 4. Completion Record & Verification Status

- **Execution Date**: 2026-09-17
- **Test Suite**: `RoyalSmash.Editor.Phase16PlayModeTest.RunAllTests()`
- **Overall Result**: **10/10 PASSED (100%), 0 FAILED, 0 CONSOLE ERRORS**

### Detailed Test Results
| Test ID | Test Case | Target System | Result | Notes |
|---|---|---|---|---|
| **Test 1** | AudioConfigLoaded | `AudioConfig.asset` | **PASS** | 13 Section 21 clips assigned and loaded into memory. |
| **Test 2** | AudioManagerInitialization | `AudioManager.cs` | **PASS** | Multi-voice pool (8 voices) and BGM source initialized. |
| **Test 3** | BallLaunchSFX | `MortarController.cs` | **PASS** | `PlayBallLaunch()` executes with pitch randomization (+-6%). |
| **Test 4** | ObjectImpactAcousticDifferentiation | `PhysicsObject.cs` | **PASS** | Box, Stone, Column, and Cone play distinct acoustic signatures. |
| **Test 5** | PaintBottleBreakSFX | `PaintBottleObject.cs` | **PASS** | `PlayBottleBreak()` glass shatter + paint splash executed. |
| **Test 6** | ObjectDestructionSFX | `PhysicsObject.cs` | **PASS** | `PlayObjectDestruction()` crumbling collapse executed. |
| **Test 7** | UIFeedbackSFX | `UIButtonSound.cs` / `GoldManager.cs` | **PASS** | Button click tick and coin reward chime verified. |
| **Test 8** | WinAndLoseAudio | `WinPanel.cs` / `LosePanel.cs` | **PASS** | Victory fanfare and defeat cadence verified. |
| **Test 9** | BGMPlaybackAndLooping | `MainMenuController.cs` / `LevelManager.cs` | **PASS** | Looping Main Menu & Gameplay background music verified. |
| **Test 10** | SettingsMuteToggles | `AudioManager.cs` / `InGameSettingsDialog.cs` | **PASS** | Music & SFX mute flags strictly enforce silent channels. |

### Artifacts and Assets Produced
- `Assets/_Project/Scripts/Audio/AudioConfig.cs`: ScriptableObject schema for audio configuration.
- `Assets/_Project/ScriptableObjects/Config/AudioConfig.asset`: Master audio asset containing all 13 Section 21 clips and voice pool configurations.
- `Assets/_Project/Scripts/Audio/AudioManager.cs`: Singleton audio manager with voice pooling, impact throttling (40ms), pitch variance (0.06), and settings sync.
- `Assets/_Project/Scripts/UI/UIButtonSound.cs`: Auto-attached component giving immediate tactile acoustic feedback to all UI buttons.
- `Assets/_Project/Scripts/Editor/ProceduralAudioGenerator.cs`: Synthesizer generating 13 44.1kHz 16-bit PCM WAV audio clips.
- `Assets/_Project/Audio/SFX/` (11 clips): `ball_launch.wav`, `box_impact.wav`, `stone_impact.wav`, `column_impact.wav`, `cone_impact.wav`, `bottle_break.wav`, `object_destruction.wav`, `ui_click.wav`, `win_fanfare.wav`, `lose_sound.wav`, `reward_coin.wav`.
- `Assets/_Project/Audio/Music/` (2 clips): `bgm_mainmenu.wav`, `bgm_gameplay.wav`.
- `Assets/_Project/Scenes/MainMenu.unity`: `AudioManager` linked with `AudioConfig`, 52 buttons equipped with `UIButtonSound`.
- `Assets/_Project/Scenes/MainGame.unity`: `AudioManager` linked with `AudioConfig`, 21 buttons equipped with `UIButtonSound`.
