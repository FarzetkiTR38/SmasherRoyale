# Implementation Plan — Phase 14: Ads Framework & Integration

Implementation plan and technical specification for **Phase 14: Ads Integration & Safety Framework** based on **Section 15 (Ads System)**, **Section 1.6 (Ads Rules)**, and **Section 35 (Phase 14)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Requirements

1. **Mandatory / Interstitial Ads (Section 15.1)**:
   - Shown at progression milestones: every **5 levels** (e.g. after completing Level 5, 10, 15, 20, 25).
   - Shown upon **Lose** (when defeat is evaluated).
   - **Bypassed completely** if `PlayerSaveData.NoAdsPurchased == true`.

2. **Rewarded Ads (Section 15.2)**:
   - Player-initiated voluntarily (e.g., from `OutOfLivesDialog` to recover +1 life).
   - Grants reward **ONLY** after successful ad completion.
   - **Remains fully active and available** even if the player purchased No Ads.

3. **Ad Safety & Idempotency (Section 15.3)**:
   - Single reward per ad view: prevents duplicate reward delivery.
   - Prevents reward on cancelled or failed ads.
   - Double-tap and concurrent ad call protection.
   - Graceful fallback: if ad fails to load or provider fails, gameplay continues smoothly without freeze or crash.

4. **Extensible Provider Architecture (`IAdProvider`)**:
   - Decoupled interface supporting both **Simulated / Mock Provider** (for Editor and offline dev) and real Ad SDKs (Google Mobile Ads / AdMob, Unity Ads, AppLovin).
   - Configurable via `AdConfig.asset` ScriptableObject.

---

## 2. Component Deliverables

### Scripts & Data Models
- `[NEW]` `Assets/_Project/Scripts/Ads/AdConfig.cs`:
  - ScriptableObject exposing level progression interval (default 5), lose trigger toggle, ad unit IDs (Android/iOS), simulation mode flag, and mock timings.
- `[NEW]` `Assets/_Project/ScriptableObjects/Config/AdConfig.asset`:
  - Configured instance of `AdConfig`.
- `[NEW]` `Assets/_Project/Scripts/Ads/IAdProvider.cs`:
  - Interface standardizing `Initialize`, `IsInterstitialReady`, `IsRewardedReady`, `ShowInterstitial`, and `ShowRewarded`.
- `[NEW]` `Assets/_Project/Scripts/Ads/SimulatedAdProvider.cs`:
  - In-editor and mock provider implementing `IAdProvider` with visual simulation UI.
- `[NEW]` `Assets/_Project/Scripts/Ads/AdManager.cs`:
  - Central singleton managing ad state, cooldowns, No Ads verification, safe callback dispatch, and idempotent reward granting.
- `[NEW]` `Assets/_Project/Scripts/UI/SimulatedAdDialog.cs`:
  - Visual modal overlay simulating real ad playback with countdown, close button, and simulated failure testing hook.
- `[MODIFY]` `Assets/_Project/Scripts/UI/OutOfLivesDialog.cs`:
  - Connect `HandleWatchAd` to `AdManager.Instance.ShowRewarded` with verified reward callback.
- `[MODIFY]` `Assets/_Project/Scripts/UI/LosePanel.cs`:
  - Trigger `AdManager.Instance.ShowLoseInterstitial` on defeat.
- `[MODIFY]` `Assets/_Project/Scripts/Levels/LevelManager.cs`:
  - Trigger `AdManager.Instance.CheckAndShowProgressionInterstitial` upon level victory.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase14Setup.cs`:
  - Editor tool to instantiate `AdConfig.asset`, ensure `AdManager` exists, and wire UI components across scenes.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase14PlayModeTest.cs`:
  - Automated test suite validating all Phase 14 acceptance criteria.
- `[NEW]` `_FarzetkiDocs/Ads_Integration_Guide.md`:
  - Comprehensive step-by-step guide for importing Google Mobile Ads Unity plugin, setting App IDs, Ad Unit IDs, and going live.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: Interstitial after Progression Point**:
   - Interstitial triggers after every 5th level (Level 5, 10, etc.) on victory.
2. **Test 2: Interstitial after Lose**:
   - Interstitial triggers when player runs out of balls and loses.
3. **Test 3: No Ads Disables Interstitials**:
   - When `NoAdsPurchased = true`, progression and lose interstitials are completely skipped.
4. **Test 4: Rewarded Ad Grants +1 Life**:
   - Successful completion of rewarded ad via `OutOfLivesDialog` grants exactly +1 life.
5. **Test 5: Rewarded Ad Active with No Ads**:
   - Rewarded ad remains fully functional and accessible even when `NoAdsPurchased = true`.
6. **Test 6: Ad Failure Handling**:
   - If an ad fails to load/show, no exception is thrown, gameplay continues smoothly without freeze.
7. **Test 7: Cancelled Ad Grants No Reward**:
   - If player closes or cancels a rewarded ad before completion, no life or reward is granted.
8. **Test 8: Double-Reward Prevention (Idempotency)**:
   - Multiple reward callbacks from the same ad session are blocked; exactly one reward is granted.
9. **Test 9: Concurrent Ad Call Lock**:
   - Calling `ShowInterstitial` or `ShowRewarded` while an ad is already active is safely rejected.
10. **Test 10: Zero Console Errors**:
    - All Phase 14 scripts compile cleanly with 0 errors and 0 warnings.

---

## 4. Completion Record & Verification Status

**Date of Completion**: 2026-09-17  
**Execution Agent**: Antigravity Pair Programmer (Zero User Delegation Workflow)  
**Overall Status**: **COMPLETE (10/10 Acceptance Tests Passed - 100%)**

### Acceptance Test Results

| # | Test Name | Target Requirement | Status |
|:---:|:---|:---|:---:|
| 1 | Progression Interstitial | Triggers after every 5th level (Level 5, 10, etc.) on victory; skipped on non-milestones | **PASSED** |
| 2 | Lose Interstitial | Triggers upon level defeat when remaining balls reach zero | **PASSED** |
| 3 | No Ads Bypasses Interstitials | `NoAdsPurchased = true` permanently suppresses mandatory/interstitial ads | **PASSED** |
| 4 | Rewarded Ad Grants +1 Life | Completing rewarded ad in `OutOfLivesDialog` reliably adds +1 life | **PASSED** |
| 5 | Rewarded Active with No Ads | Rewarded ads remain 100% active and available even when No Ads is purchased | **PASSED** |
| 6 | Ad Failure Handling | Load or show network errors are caught gracefully without freezing gameplay | **PASSED** |
| 7 | Cancelled Ad Safety | Cancelling or closing an ad before completion yields zero rewards | **PASSED** |
| 8 | Double-Reward Idempotency | Duplicate reward callbacks are blocked; exactly one reward granted per session | **PASSED** |
| 9 | Concurrent Ad Call Lock | Attempting to trigger concurrent ads while one is active is locked and rejected | **PASSED** |
| 10 | Console Health | 0 compilation errors, 0 runtime exceptions on Unity console | **PASSED** |

### Visual Proofs & Deliverables
- **Simulated Ad Dialog**: `Assets/Screenshots/phase14_simulated_ad.png`
- **External SDK Setup Guide**: `_FarzetkiDocs/Ads_Integration_Guide.md`

### Architecture & Handover Notes
- **Provider Decoupling**: Built around `IAdProvider` interface so switching from simulation mode to live Google Mobile Ads or Unity Ads SDK requires only swapping the provider or unchecking `EnableSimulationMode` in `AdConfig.asset`.
- **Zero-Interruption Safety**: All ad calls use non-blocking asynchronous callbacks with safe error fallbacks, ensuring the player is never trapped in a frozen state.
- **External Integration Readiness**: Complete documentation for linking AdMob App ID and Ad Unit IDs is documented in `_FarzetkiDocs/Ads_Integration_Guide.md` for when the user is ready to connect live store credentials.

