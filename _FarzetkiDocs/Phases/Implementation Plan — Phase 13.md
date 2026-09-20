# Implementation Plan — Phase 13: Shop & IAP Framework

Implementation plan and technical specification for **Phase 13: Shop & IAP Framework** based on **Section 14 (Shop)**, **Section 15 (Ads Safety/No Ads)**, and **Section 35 (Phase 13)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Requirements

1. **Shop Navigation (Section 14 & Section 17.2)**:
   - Accessible via the **Shop** tab in `BottomNavigationUI`.
   - Smooth page transition between Shop, Battle, and Leaderboard.
   - Dynamic TopBar integration (reflecting live Gold and Lives).

2. **Shop Catalog & Sections (Section 14.1)**:
   - **Limited-Time Offer**:
     - 24h recurring countdown timer.
     - Premium value pack (Gold + Mega Balls + Infinity Balls).
   - **No Ads (One-Time IAP)**:
     - Permanently disables interstitial/mandatory ads (`NoAdsPurchased = true`).
     - Rewarded ads remain active and accessible.
     - Displays "PURCHASED" once acquired.
   - **Gold Packs**:
     - 4 tiers: Pouch (500), Chest (1,500), Vault (5,000), Royal Treasury (15,000).
   - **Skill Packs**:
     - Mega Ball packs (+5 uses, +15 uses).
     - Infinity Ball packs (+5 uses, +15 uses).
   - **Gold + Skill Bundles**:
     - Combined value packs: Warrior Bundle and Royal Champion Bundle.

3. **IAP & Transaction Engine (Section 14.2 & Section 35)**:
   - Extensible `IAPManager` singleton supporting simulated/sandbox transactions in Editor & offline, ready for store backend SDKs.
   - **Double-Click Prevention**: Prevents rapid multi-taps from triggering duplicate transactions; purchase buttons lock during processing.
   - **Error Handling**: On transaction failure, no items or gold are credited, and an error dialog/banner is displayed.
   - **Idempotent Rewards**: Ensures reliable, transactional delivery of purchased items.

---

## 2. Component Deliverables

### Scripts & Data Models
- `[MODIFY]` `Assets/_Project/Scripts/Save/PlayerSaveData.cs`:
  - Add `public bool NoAdsPurchased = false;`
- `[NEW]` `Assets/_Project/Scripts/Economy/ShopConfig.cs`:
  - ScriptableObject defining all shop items, product IDs, currency types (IAP real money or in-game Gold), rewards, and pricing.
- `[NEW]` `Assets/_Project/ScriptableObjects/Config/ShopConfig.asset`:
  - Configured asset with Gold Packs, Skill Packs, Bundles, Limited Offer, and No Ads.
- `[NEW]` `Assets/_Project/Scripts/Economy/IAPManager.cs`:
  - Central purchase coordinator managing transaction states, double-click locks, product fulfillment, and error handling.
- `[NEW]` `Assets/_Project/Scripts/UI/ShopUI.cs`:
  - Master UI controller for the Shop page inside `MainMenu.unity`, managing section layouts, 24h countdown timer, purchase feedback, and live refresh.
- `[NEW]` `Assets/_Project/Scripts/UI/ShopItemCardUI.cs`:
  - Component attached to each shop item card managing icon, title, description, badge, and purchase button clicks with double-tap lock.
- `[NEW]` `Assets/_Project/Scripts/UI/PurchaseFeedbackDialog.cs`:
  - Modal dialog displaying purchase success or error details with dismissal.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase13Setup.cs`:
  - Editor automation to generate shop UI cards, assemble the scrollable shop catalog inside `MainMenu.unity`, wire `ShopUI`, and update scene references.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase13PlayModeTest.cs`:
  - Automated integration test suite validating all Phase 13 acceptance criteria.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: Shop Navigation from Bottom Tab**:
   - Clicking Shop tab in bottom navigation opens the Shop page and highlights the tab.
2. **Test 2: All Shop Sections Visible & Navigable**:
   - Limited Offer, No Ads, Gold Packs, Skill Packs, and Bundles are instantiated and active.
3. **Test 3: Gold Pack Purchase Fulfillment**:
   - Purchasing a gold pack adds the exact configured Gold amount to `GoldManager` and `PlayerSaveData`.
4. **Test 4: Skill Pack Purchase Fulfillment**:
   - Purchasing a skill pack adds the exact configured Mega Ball and Infinity Ball uses to `SkillManager` and `PlayerSaveData`.
5. **Test 5: Limited Offer Countdown**:
   - 24-hour countdown timer decrements over time and formats correctly (`HH:mm:ss`).
6. **Test 6: No Ads Purchase**:
   - Purchasing No Ads sets `NoAdsPurchased = true`, updates UI to "PURCHASED", and persists across save.
7. **Test 7: Purchase Error Handling**:
   - Simulated purchase failure does not credit any items, keeps balances unchanged, and displays error feedback.
8. **Test 8: Double-Click Prevention**:
   - Rapidly tapping purchase button while a transaction is pending ignores subsequent taps.
9. **Test 9: Value Bundle Purchase Fulfillment**:
   - Purchasing a combined bundle correctly credits both Gold and Skill uses in a single transaction.
10. **Test 10: Zero Console Errors**:
    - All Phase 13 scripts compile cleanly with 0 errors and 0 warnings.

---

## 4. Completion Record & Verification Status

**Date of Completion**: 2026-09-17  
**Execution Agent**: Antigravity Pair Programmer (Zero User Delegation Workflow)  
**Overall Status**: **COMPLETE (10/10 Acceptance Tests Passed - 100%)**

### Acceptance Test Results

| # | Test Name | Target Requirement | Status |
|:---:|:---|:---|:---:|
| 1 | Shop Navigation | Opens smoothly via `BottomNavigationUI.SwitchTab(Shop)` and highlights tab | **PASSED** |
| 2 | Shop Catalog & Sections | All 5 sections (Limited Offer, No Ads, Gold Packs, Skill Packs, Bundles) active with 10 configured items | **PASSED** |
| 3 | Gold Pack Fulfillment | Credited exact 1,500 Gold balance upon purchase without corruption | **PASSED** |
| 4 | Skill Pack Fulfillment | Purchased with Gold, debited gold balance and credited +5 uses | **PASSED** |
| 5 | Limited Offer Timer | Real-time 24-hour countdown loop active with formatted `HH:mm:ss` display | **PASSED** |
| 6 | No Ads One-Time IAP | Purchased, permanently flagged in `PlayerSaveData`, and displays "PURCHASED" | **PASSED** |
| 7 | Error Handling & Safety | Simulated transaction failure triggers error dialog, no items or gold credited | **PASSED** |
| 8 | Double-Click Prevention | Rapid subsequent clicks during active transactions are locked out | **PASSED** |
| 9 | Value Bundle Fulfillment | Combined transaction safely credits Gold (+2,000) and both Skills (+5 Mega, +5 Infinity) | **PASSED** |
| 10 | Console Health | 0 compilation errors, 0 runtime exceptions on Unity console | **PASSED** |

### Visual Proofs & Artifacts
- **Shop Catalog View**: `Assets/Screenshots/phase13_shop_screen.png`
- **Purchase Feedback Dialog**: `Assets/Screenshots/phase13_purchase_feedback.png`
- **Main Menu Default State**: `Assets/Screenshots/phase13_main_menu.png`

### Architecture Highlights & Technical Notes
- **Dynamic UI Grid / Layout Fix**: Wrapped all section grids inside `LayoutElement` with calculated heights so Unity's `VerticalLayoutGroup` correctly scrolls the entire catalog without overlapping section headers.
- **Idempotent Purchase Lock**: Implemented `IsPurchaseInProgress` within `IAPManager` preventing race conditions from simultaneous tap inputs.
- **Save System Integration**: Added `NoAdsPurchased` to `PlayerSaveData.cs` ensuring permanent ad-free status across sessions.
- **Offline / Sandbox Ready**: Fully simulated transactions with fallback hooks ready for Google Play Billing / Apple StoreKit integration.

