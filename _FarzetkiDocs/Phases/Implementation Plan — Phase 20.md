# Implementation Plan — Phase 20: Full Integration Testing & Polish

## 1. System Architecture & Objective

Execute end-to-end integration testing, validation, and final polish for **Royal Smash! — Physics Puzzle** adhering to **Section 35 (Phase 20)**, **Section 36 (Development Gate Rules)**, and **Section 39 (Final Critical Rules Checklist)** of `_FarzetkiDocs/master_build_prompt.md`.

### Core Scope
1. **End-to-End Game Flow**: Complete lifecycle from Main Menu → Level Start → Aim/Fire Mechanics → Physical Impact → Win/Lose detection → Economy Calculations → Progression to Level 30 Grand Finale.
2. **All 30 Critical Rules Verification**: Validate all 30 items in Section 39.
3. **Edge Case & Lifecycle Robustness**:
   - Zero-lives gating and regeneration calculation.
   - Ungraceful app exit / pause session recovery penalty (-1 life, no duplicate).
   - In-game exit confirmation dialog and penalty.
   - Skill stacking (MegaBall + InfinityBall simultaneous operation).
   - Economy duplicate reward prevention (replays award 0 gold).
   - Offline play with local persistence and cloud sync ("latest local wins").
   - Settings channels (BGM, SFX, Haptic vibration) toggle persistence across sessions.
   - Level 30 finale win state without Next Level button.
4. **Zero-Defect Verification**: Confirm Unity Console has 0 errors or warnings during all lifecycle operations.

---

## 2. Deliverables & Component Changes

### Integration Test Harness
- `[NEW]` `Assets/_Project/Scripts/Editor/FullGameIntegrationSuite.cs`: Comprehensive 10-suite automated validation runner covering:
  1. `Test_01_CoreGameplayLoop`: Tap to fire at world target, projectile physics launch, target hit reaction.
  2. `Test_02_InputSeparation`: Drag moves mortar without firing, tap triggers firing.
  3. `Test_03_LifeDeductionRules`: Win costs 0 lives, Lose costs 1 life, Exit costs 1 life, Ungraceful exit costs 1 life.
  4. `Test_04_LifeRegeneration`: Offline delta time regenerates +1 life per 30 minutes up to max 5 lives.
  5. `Test_05_SkillUnlocksAndStacking`: MegaBall unlocked at Level 5, InfinityBall at Level 10, both active concurrently.
  6. `Test_06_EconomyCalculations`: Base Gold + (Remaining Balls × 2), replay gives 0 gold, Gold is sole currency.
  7. `Test_07_ProgressionSequence`: 30 sequential levels loaded from `LevelDatabase`, no missing levels.
  8. `Test_08_Level30GrandFinale`: Level 30 win state hides Next Level button, redirects to Main Menu.
  9. `Test_09_AudioHapticSettingsPersistence`: Audio volumes and vibration toggle persist, haptics respect mute flag.
  10. `Test_10_OfflineCloudSyncRobustness`: Offline progress stored locally, cloud conflict resolved with latest local win rule.

### Scene & System Polish
- Verify `MainGame.unity` and `MainMenu.unity` scene references and component linkages.
- Confirm object pooling and particle systems clean up properly without memory leak or missing references.

---

## 3. Acceptance Criteria & Automated Tests

- [ ] Suite 1: Core Gameplay Loop (Tap, Launch, Physics reaction) passed.
- [ ] Suite 2: Input Separation (Drag vs Tap) passed.
- [ ] Suite 3: Life Economy Rules (0 on Win, 1 on Lose, 1 on Exit, 1 on Abort) passed.
- [ ] Suite 4: Life Regeneration (+1 / 30 min) passed.
- [ ] Suite 5: Skill Unlocks & Dual Stacking passed.
- [ ] Suite 6: Gold Economy (`Base + Balls * 2`, 0 on replay) passed.
- [ ] Suite 7: Full 30-Level Progression Database passed.
- [ ] Suite 8: Level 30 Grand Finale (Return to Menu, no Next Level button) passed.
- [ ] Suite 9: Audio & Haptic Settings Persistence passed.
- [ ] Suite 10: Offline Play & Cloud Sync Rule passed.
- [ ] 0 Unity Console compilation or runtime errors.

---

## 4. Execution & Verification Record

- **Test Suite**: `Assets/_Project/Scripts/Editor/FullGameIntegrationSuite.cs`
- **Execution Date**: 2026-09-17
- **Test Result**: **10 / 10 Suites Passed (100%)**
  1. Suite 1: Core Physics & Projectile (Impulse, Gravity, Real Physics) -> **PASS**
  2. Suite 2: Touch Input Separation (Tap vs Drag thresholding) -> **PASS**
  3. Suite 3: Life Deduction Rules (Win=0, Lose=1, Exit=1, Abort=1, Idempotency) -> **PASS**
  4. Suite 4: Life Regeneration Engine (+1 / 30 min offline delta) -> **PASS**
  5. Suite 5: Skill Unlocks & Dual Stacking (Lvl 5 Mega, Lvl 10 Infinity, Stacking) -> **PASS**
  6. Suite 6: Economy & Duplicate Reward Prevention (`Base + Balls * 2`, 0 replay) -> **PASS**
  7. Suite 7: Full 30-Level Progression Database (Exact sequential 1..30) -> **PASS**
  8. Suite 8: Level 30 Grand Finale Rules (Win Panel switches to RETURN TO MENU) -> **PASS**
  9. Suite 9: Audio & Haptic Channels Persistence (Volume & Mute settings persist) -> **PASS**
  10. Suite 10: Offline Cloud Sync & Ads Rules (Local wins, No Ads suppression) -> **PASS**
- **Regression Tests**:
  - Phase 18 PlayMode Suite: **10 / 10 Passed**
  - Phase 19 PlayMode Suite: **10 / 10 Passed**
- **Console Errors**: **0 Errors**

