# Project-Specific Constraints

## Game Identity
This is **Royal Smash! — Physics Puzzle**, a casual mobile 3D physics puzzle game.

## Absolute Constraints

### Technology
- **Unity 3D** (NOT 2D) — Use `Rigidbody`, `Collider`, `Physics`, NOT 2D variants
- **Portrait orientation** for mobile
- **TextMeshPro** for ALL text — NEVER use legacy `UnityEngine.UI.Text`
- **C# only** — No Visual Scripting, no Bolt, no Lua

### File System
- **ALL game code/assets inside `Assets/` folder** — Never modify outside Assets/
- **Organized under `Assets/_Project/`** — Keep project files separate from packages
- **No modifications to Packages/ or ProjectSettings/** unless explicitly asked

### Physics
- Use **3D Rigidbody** and **3D Colliders** exclusively
- **World-Coordinate Targeting** — Ball targets the tapped world position, NOT a 2D angle
- **Real physics impulse** — Objects react with actual forces, NOT scripted animations
- **Continuous collision detection** on fast projectiles to prevent tunneling

### Data
- **ScriptableObjects** for all configuration (GameConfig, LevelData, ObjectData)
- **JSON serialization** for save data — NOT PlayerPrefs for complex state
- **Data-driven levels** — Level layouts in ScriptableObject data, NOT hardcoded in scripts
- **All tunable values exposed as configurable parameters** — No magic numbers in code

### Save System
- Save at every critical state change (win, lose, purchase, profile change)
- **Idempotent operations** — Same event must NOT grant rewards or deduct lives twice
- **App close = exit** — Track active level state to deduct life on next launch
- **Offline-first** — Core gameplay works without internet

## Scope Boundaries

### DO build (MVP scope):
30 sequential levels, physics gameplay, mortar aiming, world-coordinate targeting,
5 object types (Box/Stone/Column/Cone/PaintBottle), 2 skills (Mega Ball/Infinity Ball),
Gold economy, lives system, shop, ads (interstitial + rewarded), No Ads IAP,
main menu, profile (8 avatars + 4 frames), leaderboard, settings, tutorial,
local save, offline gameplay, cloud sync framework, VFX, SFX, haptics.

### DO NOT build (out of scope):
PvP, multiplayer, clans, daily quests, battle pass, star ratings, level select,
level replay, multiple currencies, complex achievements, social login, email accounts,
cross-device sync, chest/gacha systems, character skill trees, equipment/crafting,
complex inventory, daily rewards, complex wall-bounce mechanics, 2D physics.

## Locked Gameplay Rules (IMMUTABLE)

These come from the Master Build Prompt Section 1 and MUST NEVER be changed:

1. Tap fires at world-coordinate target
2. Drag moves mortar, NEVER fires
3. Balls use real physics, NOT scripted movement
4. Objects react physically to impacts
5. Balls are limited per level (configurable)
6. Balls can bounce back toward player
7. Balls auto-destroy after ~5s with fade effect
8. Empty-space balls travel into 3D depth
9. All objects cleared = win
10. Balls depleted + objects remain = lose
11. Win = 0 life loss
12. Lose = -1 life
13. Exit = -1 life
14. App close during level = -1 life
15. Life regen: +1 per 30 min
16. Mega Ball unlocks at Level 5
17. Infinity Ball unlocks at Level 10
18. Skills can stack
19. Remaining Balls × 2 = Gold bonus (FIXED)
20. Gold is the only currency
21. Offline gameplay works fully
22. Cloud sync: latest local wins
23. No Ads removes only mandatory/interstitial ads
24. Rewarded ads always available
25. 30 levels, sequential, no level select
26. Level 30: Win Panel, NO Next Level button
27. Username prompted at Level 10
28. New content every 5 levels
29. Objects fade/shrink/melt — NEVER pop instantly
30. No star rating, no efficiency scoring
