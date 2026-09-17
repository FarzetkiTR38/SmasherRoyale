# Implementation Plan — Phase 15: Leaderboard & Cloud Sync

Implementation plan and technical specification for **Phase 15: Leaderboard & Cloud Sync** based on **Section 19 (Leaderboard)**, **Section 28 (Cloud Sync)**, and **Section 35 (Phase 15)** of `_FarzetkiDocs/master_build_prompt.md`.

---

## 1. Overview & Architecture

### Core Systems & Requirements

1. **Leaderboard Display & Ranking (Section 19)**:
   - Accessible via the **Leaderboard** tab in `BottomNavigationUI`.
   - Primary ranking criterion: **Current Level** (descending order).
   - Top 1000 player display support.
   - Each row displays: **Rank**, **Avatar** (matching the 8 royal archetypes), **Frame**, **Username**, and **Level**.
   - Special visual distinction for Top 3 ranks (Imperial Gold, Silver, Bronze).
   - **Sticky Player Card**: Dedicated bottom row showing the local player's real-time rank, avatar, name, and level regardless of scroll position.

2. **Online & Offline Mechanics (Section 19.3 & 19.4)**:
   - **Online**: Fetches live leaderboard data, calculates player rank, and displays "🟢 LIVE ONLINE" badge.
   - **Offline**: Displays the last successfully cached leaderboard from local JSON storage (`leaderboard_cache.json`).
   - Clearly flags cached data with "🟡 OFFLINE (CACHED)" indicator so players are never misled.
   - **Manual & Auto Refresh**: Includes refresh button and auto-sync on app startup / level completion.

3. **Cloud Synchronization ("Latest Local Progress Wins" — Section 28)**:
   - Push local progress (Level, Gold, Lives, Skills, Username, Avatar, Frame) to cloud storage.
   - Rule: If local level is higher than cloud (played offline), cloud is overwritten with latest local progress.
   - Non-blocking, idempotent, and resilient against network dropouts.
   - Graceful failure handling: if cloud service is unreachable, gameplay continues without crashes.

4. **Decoupled Cloud Architecture (`ICloudSyncProvider`)**:
   - Clean provider interface allowing seamless switching between **Simulated Cloud Provider** (for Editor/testing) and production backends (Firebase, PlayFab, Unity Gaming Services).

---

## 2. Component Deliverables

### Data Models & Configuration
- `[NEW]` `Assets/_Project/Scripts/Cloud/LeaderboardEntryData.cs`:
  - Serializable data model for leaderboard rows (Rank, PlayerId, Username, Level, AvatarIndex, FrameIndex, IsLocalPlayer).
- `[NEW]` `Assets/_Project/Scripts/Cloud/LeaderboardConfig.cs`:
  - ScriptableObject with default royal competitors, caching rules, and simulated latency.
- `[NEW]` `Assets/_Project/ScriptableObjects/Config/LeaderboardConfig.asset`:
  - Configured asset with pre-populated royal competitors.

### Cloud Synchronization & Providers
- `[NEW]` `Assets/_Project/Scripts/Cloud/ICloudSyncProvider.cs`:
  - Interface standardizing progress sync, leaderboard fetch, and connectivity status.
- `[NEW]` `Assets/_Project/Scripts/Cloud/SimulatedCloudProvider.cs`:
  - Mock cloud backend simulating cloud persistence, offline toggles, and network dropouts.
- `[NEW]` `Assets/_Project/Scripts/Cloud/CloudSyncManager.cs`:
  - Singleton managing background synchronization, local caching, and reconnect sync events.
- `[NEW]` `Assets/_Project/Scripts/Cloud/LeaderboardManager.cs`:
  - Singleton managing leaderboard fetching, level-based sorting, rank computation, and player positioning.

### UI Systems
- `[NEW]` `Assets/_Project/Scripts/UI/LeaderboardEntryRowUI.cs`:
  - Component managing row UI (Rank number/badge, Avatar sprite, Frame border, Username text, Level badge, local player highlight).
- `[NEW]` `Assets/_Project/Scripts/UI/LeaderboardUI.cs`:
  - Master page controller in `MainMenu.unity` managing the scrollable list, header status badge, refresh button, and sticky player footer.

### Editor & Test Automation
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase15Setup.cs`:
  - Editor script to construct `LeaderboardPage` UI in `MainMenu.unity`, instantiate `LeaderboardConfig.asset`, and wire references.
- `[NEW]` `Assets/_Project/Scripts/Editor/Phase15PlayModeTest.cs`:
  - Automated integration test suite validating all 10 Phase 15 acceptance criteria.

---

## 3. Acceptance Criteria & Automated Tests

1. **Test 1: Leaderboard Tab Navigation**:
   - Clicking Leaderboard tab in bottom navigation switches page and loads leaderboard data.
2. **Test 2: Descending Level Sort & Ranking**:
   - Entries are strictly sorted by Current Level in descending order with consecutive ranks (1, 2, 3...).
3. **Test 3: Top 3 Distinct Accents**:
   - Ranks 1, 2, and 3 are identified with distinct royal accents (Gold, Silver, Bronze).
4. **Test 4: Local Player Inclusion & Ranking**:
   - Player's active profile (Username, Avatar, Level) is present in the leaderboard and accurately placed according to level.
5. **Test 5: Sticky Player Card Update**:
   - Sticky player bar displays correct player rank, level, and username.
6. **Test 6: Offline Cache Fallback**:
   - When offline, leaderboard displays cached data from disk without showing an empty list.
7. **Test 7: Offline Status Banner**:
   - When offline, status indicates cached state; when online, indicates live connection.
8. **Test 8: Latest Local Progress Wins Rule**:
   - Advancing local level while offline syncs and updates the cloud state upon reconnection.
9. **Test 9: Service Failure Graceful Handling**:
   - Simulated network/server outage does not freeze UI or crash the game; falls back to cached data.
10. **Test 10: Zero Console Errors**:
    - All Phase 15 components compile cleanly with 0 errors and 0 warnings on Unity console.

---

## 4. Completion Record & Verification Status

**Date of Completion**: 2026-09-17  
**Execution Agent**: Antigravity Pair Programmer (Zero User Delegation Workflow)  
**Overall Status**: **COMPLETE (10/10 Acceptance Tests Passed - 100%)**

### Acceptance Test Results

| # | Test Name | Target Requirement | Status |
|:---:|:---|:---|:---:|
| 1 | Leaderboard Tab Navigation | Leaderboard page opens smoothly from `BottomNavigationUI.SwitchTab(Leaderboard)` | **PASSED** |
| 2 | Descending Level Sort & Ranks | All entries strictly sorted by level (descending) with consecutive ranks (1..N) | **PASSED** |
| 3 | Top 3 Distinct Accents | Ranks 1, 2, and 3 accurately identified with royal accents (Gold, Silver, Bronze) | **PASSED** |
| 4 | Local Player Inclusion | Player's username, avatar, and level seamlessly integrated into global ranking | **PASSED** |
| 5 | Sticky Player Card | Bottom pinned row displays current player rank and stats at all times | **PASSED** |
| 6 | Offline Cache Fallback | Gracefully loads 35+ cached entries from persistent disk storage without empty state | **PASSED** |
| 7 | Status Banner Differentiation | Status correctly toggles between "🟢 LIVE ONLINE" and "🟡 OFFLINE (CACHED)" | **PASSED** |
| 8 | Latest Local Progress Wins | Advancing level offline updates cloud progress to highest level on reconnect | **PASSED** |
| 9 | Outage Graceful Recovery | Simulated backend dropout caught gracefully, falling back to local cache without crash | **PASSED** |
| 10 | Console Health | 0 compilation errors, 0 runtime exceptions on Unity console | **PASSED** |

### Visual Proofs & Artifacts
- **Leaderboard Screen View**: `Assets/Screenshots/phase15_leaderboard_screen.png`

### Architecture & System Highlights
- **"Latest Local Progress Wins" Synchronization**: Adheres strictly to Section 28.1. When an offline player completes levels and reconnects, local progress safely overwrites stale cloud state and updates global leaderboard position.
- **Resilient Offline Cache**: Utilizes persistent JSON caching (`leaderboard_cache.json`) so players can always view their standing, with clear status indicators differentiating live from cached data.
- **Top 3 & Player Highlighting**: Top 3 ranks receive distinct imperial gold/silver/bronze badges, while the local player row features an accented royal navy background and gold highlight border.

