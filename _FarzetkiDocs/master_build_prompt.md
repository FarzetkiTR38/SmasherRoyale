# ROYAL SMASH! — PHYSICS PUZZLE
## MASTER BUILD PROMPT — COMPLETE MVP DEVELOPMENT SPECIFICATION

---

> **ROLE**: You are a senior Unity 3D game developer AI. You will build a complete, playable mobile physics puzzle game called **"Royal Smash! — Physics Puzzle"** from scratch in Unity. This prompt is the **absolute source of truth**. Do NOT invent systems not described here. Do NOT skip documented requirements. Do NOT replace confirmed rules with assumptions.

> **TARGET**: Unity 3D | C# | Mobile (Android + iOS) | Portrait Orientation | 30 Playable Levels

> **CRITICAL INSTRUCTION**: Break this entire build into **sequential, testable phases**. Each phase must be self-contained and testable before proceeding to the next. At the end of each phase, list specific acceptance tests. Do NOT proceed to Phase N+1 until Phase N passes all tests.

---

# ═══════════════════════════════════════════
# SECTION 1 — NON-NEGOTIABLE LOCKED REQUIREMENTS
# ═══════════════════════════════════════════

The following rules are **LOCKED / CONFIRMED** by the game designer. They MUST NOT be altered, simplified, or replaced under any circumstances:

## 1.1 Core Gameplay Rules

1. **World-Coordinate Targeting**: When the player taps the screen, the ball MUST target the actual **world-space coordinate** of the tap point, NOT just calculate a 2D angle. Implementation: `Screen Tap → Camera Ray → World Position → Target Point → Projectile Trajectory`.
2. **Physics-Based Gameplay**: Objects MUST react to ball impacts using real physics (forces, impulses, collisions). Do NOT use fake animations or scripted destruction.
3. **Limited Ammunition**: Each level has a finite, configurable number of balls. The player cannot fire infinitely.
4. **Natural Bounce**: Balls can bounce off objects and potentially come back toward the player. This is natural physics bounce, NOT an arcade pinball system.
5. **Empty-Space Trajectory**: Balls fired into empty space MUST travel into the 3D world depth and fade/disappear naturally. They must NOT pop/vanish at a 2D boundary.
6. **Drag = Move Mortar / Aim | Tap = Fire**: Dragging moves the mortar horizontally. Tapping fires. Dragging MUST NEVER accidentally fire.
7. **Ball Lifetime**: Balls auto-destroy after approximately ~5 seconds (configurable). They should fade/dissolve before disappearing, NOT pop instantly.
8. **Objects Must Not Pop**: Objects should shrink/melt/fade before disappearing. Instant pop = unacceptable.

## 1.2 Progression Rules (LOCKED)

9. Exactly **30 MVP levels**, played sequentially. No level select. No replay system.
10. Every **5 levels**, new block/object/content is introduced (Levels 5, 10, 15, 20, 25).
11. **Mega Ball** unlocks at **Level 5** (3 initial uses).
12. **Infinity Ball** unlocks at **Level 10** (3 initial uses).
13. At **Level 10**, the player is prompted for a **username/nickname**.
14. **Level 30**: Normal Win Panel opens, but there is **NO "Next Level" button**. Player returns to Main Menu.

## 1.3 Lives Rules (LOCKED)

15. Player starts with **5 lives**. Maximum capacity is **5**.
16. **Win** = 0 life loss. **Lose** = -1 life. **Exit active level** = -1 life. **App close during active level** = -1 life (treated as exit).
17. Life regeneration: **+1 life every 30 minutes** (configurable).
18. At **0 lives**: Player cannot start levels. Options: buy life with Gold, or watch Rewarded Ad for +1 life.

## 1.4 Economy Rules (LOCKED)

19. Single currency: **Gold**.
20. Level completion reward formula: `Total Gold = Base Level Reward + (Remaining Balls × 2)`. The "Remaining Balls × 2" part is **confirmed and fixed**.
21. Base level reward (e.g., 50 Gold) is configurable.
22. Starting Gold is configurable (e.g., 1000).

## 1.5 Skill Rules (LOCKED)

23. **Mega Ball**: Larger projectile, stronger physical impact. Unlock: Level 5. Starting uses: 3.
24. **Infinity Ball**: Temporarily removes fire cooldown. Duration ~5 sec (configurable). Unlock: Level 10. Starting uses: 3.
25. Skills **can stack** simultaneously (both active at once).
26. Each activation consumes 1 use. Extra uses purchasable from Shop.

## 1.6 Ads Rules (LOCKED)

27. **No Ads** purchase removes mandatory/interstitial ads ONLY.
28. **Rewarded Ads** remain available even with No Ads purchased.
29. Rewarded Ads: Player voluntarily watches → receives reward (e.g., +1 life). Reward granted ONLY after successful completion.

## 1.7 Offline & Sync Rules (LOCKED)

30. Core gameplay MUST work fully **offline**. No internet required for playing levels.
31. Cloud sync rule: **"Latest local progress wins"** (no complex merge).
32. Offline leaderboard shows last successfully cached data.

---

# ═══════════════════════════════════════════
# SECTION 2 — CONFIGURABLE VALUES
# ═══════════════════════════════════════════

All values below MUST be exposed as tunable parameters (e.g., ScriptableObject fields, config files, or serialized inspector values). Do NOT hardcode them.

```
// Projectile
float fireCooldown = 0.5f;              // seconds between shots
float ballLifetime = 5.0f;              // seconds before auto-destroy
float ballSpeed = configurable;          // projectile launch speed
float megaBallDiameterMultiplier = 2.0f; // relative to normal ball
float infinityBallDuration = 5.0f;       // seconds

// Objects
float boxMass = configurable;
float stoneMass = configurable;          // heavier than box
float columnMass = configurable;         // heavy, tall
float coneMass = configurable;           // lighter
float paintBottleMass = configurable;    // breakable

// Economy
int startingGold = 1000;
int baseLevelReward = 50;
int remainingBallMultiplier = 2;
int lifePrice = configurable;            // Gold cost for +1 life
int megaBallUsePrice = configurable;
int infinityBallUsePrice = configurable;

// Lives
int maxLives = 5;
int startingLives = 5;
float lifeRegenMinutes = 30.0f;

// Gameplay
float victoryDelay = 2.0f;              // seconds after last object cleared
int ballsPerLevel = configurable;        // per-level, typically 20-30

// Ads
int interstitialFrequency = 5;           // every N levels

// Limited Offer
float offerDurationHours = 24.0f;
```

---

# ═══════════════════════════════════════════
# SECTION 3 — GAME OVERVIEW
# ═══════════════════════════════════════════

## 3.1 Product Definition

> A casual mobile 3D physics puzzle game where the player controls a mortar/cannon, fires a limited number of balls at world-coordinate targets, and uses physics-based collisions to topple, break, and clear all objects from a stand/platform.

## 3.2 Core Loop

```
Open Game → Main Menu → Start Level → Aim (Drag) → Fire (Tap)
→ Watch Physics → Objects Move/Break/Fall → Keep Firing
→ Result: All Objects Cleared (WIN) OR Balls Depleted (LOSE)
→ Win/Lose → Reward/Life Loss → Continue/Return → Next Level
```

## 3.3 Target Player Emotion

```
"I know where I want to hit."
→ "I fired."
→ "Great, it pushed the box forward."
→ "That hit the stone."
→ "Everything is toppling!"
→ "I solved the puzzle!"
```

## 3.4 What This Game Is NOT

- NOT a precision shooter / artillery simulator
- NOT a match-3 game
- NOT a clicker / idle game
- NOT an endless runner
- NOT a multiplayer shooter

It IS a **Physics Puzzle Game**.

---

# ═══════════════════════════════════════════
# SECTION 4 — INPUT & CONTROLS
# ═══════════════════════════════════════════

## 4.1 Control Scheme

| Input | Action | Notes |
|---|---|---|
| **Tap** | Fire ball | Ball fires toward the **world-coordinate** of the tap point |
| **Hold + Drag** | Move/Aim mortar | Mortar follows finger horizontally. NO fire on release |
| **Release after drag** | Nothing | Drag release does NOT fire |

## 4.2 Mortar Behavior

- Positioned at the **bottom-center** of the screen, below the stand.
- Moves primarily on the **X-axis** (horizontal).
- Slight Y movement allowed for visual polish.
- CANNOT move outside camera/map boundaries.
- Roughly moves between the left and right edges of the stand area.

## 4.3 World-Coordinate Targeting (CRITICAL)

```
Player taps screen
    ↓
Screen point converted via Camera.ScreenPointToRay()
    ↓
Raycast into 3D world (or plane intersection)
    ↓
World position = target point
    ↓
Calculate launch velocity/direction from mortar to target
    ↓
Apply physics force to ball
```

This is the **most important control mechanic**. Do NOT simplify to angle-based aiming.

## 4.4 Fire Cooldown

- First ball of each level: **immediately ready** (no cooldown).
- After each fire: **~0.5 sec** cooldown (configurable).
- During Infinity Ball: cooldown is **temporarily removed**.

## 4.5 Targeting Restrictions

- Player cannot target below the stand.
- Player can target above/behind the stand.
- The target must be a physically reachable world point.

---

# ═══════════════════════════════════════════
# SECTION 5 — PROJECTILE / BALL SYSTEM
# ═══════════════════════════════════════════

## 5.1 Ball Physics

- Ball is a **physics-driven projectile** (Rigidbody with AddForce/velocity).
- Carries momentum, creates impulse on collision.
- Can bounce off objects (natural physics bounce).
- Affected by gravity.
- Can push/knock objects.

## 5.2 Ball Lifetime

- Approximate lifetime: **~5 seconds** (configurable).
- At ~4 seconds: begin fade/dissolve/destroy effect.
- At ~5 seconds: fully destroyed.
- Ball must NOT pop instantly.

## 5.3 Ball Count

- Per-level, configurable (typically 20-30).
- Displayed in HUD as "Remaining Balls".
- Decrements by 1 on each normal fire.
- Stored in LevelData.

## 5.4 Mega Ball Variant

- Larger diameter (e.g., 2× normal, configurable).
- Stronger physical impact.
- Larger collision area.
- Visual distinction from normal ball.
- Uses Mega Ball uses inventory.

## 5.5 Ball Destruction Boundary

Ball is destroyed when ANY of:
- Lifetime exceeded
- Out of gameplay bounds
- Explicit cleanup condition met

---

# ═══════════════════════════════════════════
# SECTION 6 — PHYSICS SYSTEM
# ═══════════════════════════════════════════

## 6.1 Core Physics Requirements

- Ball impacts transfer impulse to objects.
- Light objects (Box) move easily.
- Heavy objects (Stone, Column) resist movement.
- Multiple objects influence each other (stacking, chain reactions).
- Objects can fall/topple from the stand.
- Physics results must feel **natural, not arcade**.
- Ball can bounce back toward the player.

## 6.2 Physics Acceptance Criteria

- ✅ Ball uses real physics impulse for movement
- ✅ Ball hitting an object moves/pushes it
- ✅ Light objects pushed easier than heavy ones
- ✅ Stacked objects behave physically
- ✅ Objects can topple/fall
- ✅ Ball bounces naturally
- ✅ Ball in empty space travels into world depth
- ✅ Ball auto-destroys after lifetime
- ✅ Physics feels natural, not scripted

## 6.3 Physics Stability

AVOID:
- Objects flying infinitely
- Explosive/unpredictable impulses
- Tunneling (objects passing through each other)
- Perpetual jitter/vibration
- Physics explosions
- Excessive CPU usage
- Balls stuck permanently

## 6.4 Configurable Physics Parameters (Per Object Type)

```
Mass, Scale, Health/Durability, Breakable (bool)
PhysicsMaterial (Friction, Bounciness)
Destroy Behaviour, Destroy Effect, Impact Sound
```

---

# ═══════════════════════════════════════════
# SECTION 7 — OBJECT SYSTEM
# ═══════════════════════════════════════════

## 7.1 MVP Object Types (5 types)

### Box
- Light/medium weight, movable, pushable
- Can stack, topple, fall from stand
- DestroyAnimation: shrink/melt
- Sound: light/wood impact

### Stone
- Heavy weight, harder to move
- Stronger impact absorption
- Sound: heavy/solid impact
- Can topple but requires more force

### Column
- Tall, heavy, vertical orientation
- Primary gameplay purpose: **topple onto other objects for chain reactions**
- Can be knocked over with sufficient force

### Cone
- Light weight, easily moved
- Different collision profile (conical shape)
- Used for level design variety

### Paint Bottle
- **Breakable** — unique destruction behavior
- Impact → Glass Break VFX → Paint Spill VFX → Object Disappear
- Does NOT simply fall; it **shatters** on sufficient impact
- Sound: glass break + liquid splash
- Spilled paint remains briefly visible in scene

## 7.2 Object State Machine

```
IDLE → HIT → MOVING/FALLING → DESTROY_CONDITION → DESTROYING → DESTROYED
```

For Paint Bottle: includes `BREAK` state with glass/paint VFX.

## 7.3 Object Destruction

- Objects should NOT pop instantly.
- Preferred flow: physical movement → fall/topple → destroy condition → shrink/melt/fade → removed.
- Paint Bottle: impact → glass break particle → paint spill → disappear.

---

# ═══════════════════════════════════════════
# SECTION 8 — STAND / PLATFORM
# ═══════════════════════════════════════════

- **Static** platform(s) where objects are placed.
- Does not move or break.
- MVP: Maximum **2 platforms** per level.
- Stand width/length: **configurable per level**.
- Variety: wide horizontal, narrow tall, high stand, two separate stands, connected platforms, long horizontal columns.

---

# ═══════════════════════════════════════════
# SECTION 9 — LEVEL SYSTEM
# ═══════════════════════════════════════════

## 9.1 Level Structure

- **30 levels**, sequential progression.
- No level select screen. No replay system.
- Each level is **data-driven** (not hardcoded).

## 9.2 Level Data Schema

```
LevelData:
    levelId: int
    ballCount: int
    objectList: ObjectPlacement[]
    standConfiguration: StandConfig
    background: BackgroundType
    unlockedFeatures: string[]
    tutorialState: TutorialConfig
    rewardConfig: RewardConfig
    difficultyTier: int
    
ObjectPlacement:
    objectType: enum (Box, Stone, Column, Cone, PaintBottle)
    position: Vector3
    rotation: Quaternion
    scale: Vector3
    customParameters: ObjectOverrides (optional)
```

## 9.3 Level Progression Map

| Level | New Content | Special Events |
|---|---|---|
| 1-4 | Base objects (Box, basic layouts) | Tutorial (Level 1 first launch only) |
| 5 | New object/block type | **Mega Ball unlocks** |
| 6-9 | Uses new content from L5 | — |
| 10 | New object/block type | **Infinity Ball unlocks** + **Username prompt** |
| 11-14 | Uses new content from L10 | — |
| 15 | New object/block type | — |
| 16-19 | Progressive difficulty | — |
| 20 | New object/block type | — |
| 21-24 | Complex multi-object puzzles | — |
| 25 | New object/block type | — |
| 26-29 | Final difficulty ramp | Multiple combos, layers, limited balls |
| 30 | Final MVP level | Win Panel with NO "Next Level" button |

## 9.4 Level Design Philosophy

- NOT just "shoot all objects directly".
- Encourage chain reactions: shoot one object → hits another → cascade → multiple objects fall.
- Difficulty variables: more objects, heavier objects, complex stacking, second row/layer, harder angles, fewer balls, new object types.
- Some levels have **two layers** (front row + back row of objects).

## 9.5 Level Difficulty Progression

- Levels 1-4: Learn aim, fire, basic physics, win condition.
- Levels 5-9: New content + Mega Ball introduction.
- Levels 10-14: New content + Infinity Ball + more complex puzzles.
- Levels 15-19: Advanced physics combinations.
- Levels 20-24: Multi-layer layouts, limited resources.
- Levels 25-29: Maximum complexity, requires skill usage.
- Level 30: Final challenge, satisfying conclusion.

## 9.6 Data-Driven Level Creation

- System must allow adding new levels by creating LevelData + placing objects WITHOUT writing new code.
- Use ScriptableObjects, JSON, or equivalent data format.
- AI may generate reasonable level layouts following the difficulty curve above.

---

# ═══════════════════════════════════════════
# SECTION 10 — WIN / LOSE SYSTEM
# ═══════════════════════════════════════════

## 10.1 Victory Condition

```
All required objects on the stand are destroyed/cleared/fallen off
    ↓
Physics settles (brief check)
    ↓
~2 sec delay (configurable)
    ↓
Win Panel opens
```

"Cleared" = object destroyed, fallen off stand, or otherwise removed from play.

## 10.2 Win Panel

- Shows: completed level, reward, remaining ball bonus, Continue/Next button.
- **NO** star rating. **NO** efficiency rating. **NO** score rating.
- Reward calculation:
  ```
  Total Gold = Base Level Reward + (Remaining Balls × 2)
  ```
- Level 30 Win Panel: NO "Next Level" button. Only "Main Menu" or appropriate completion action.

## 10.3 Lose Condition

```
Remaining Balls = 0 AND Remaining Objects > 0
    ↓
Lose detected
    ↓
-1 Life
    ↓
Lose Panel (Level failed message, remaining lives, Retry, Main Menu)
```

## 10.4 Level Exit

```
Player taps exit during active level
    ↓
Confirmation Popup: "Are you sure you want to leave?"
    ↓
Confirm → -1 Life → Main Menu
Cancel → Continue playing
```

## 10.5 App Close During Active Level

```
Active level + App killed/closed
    ↓
Treated as EXIT
    ↓
-1 Life (on next app open, deduct if not already deducted)
```

This prevents the exploit: "Close app → reopen → no life lost."

---

# ═══════════════════════════════════════════
# SECTION 11 — LIVES SYSTEM
# ═══════════════════════════════════════════

```
Starting Lives: 5
Max Lives: 5
Win: 0 life loss
Lose: -1 life
Exit: -1 life
App Close (active level): -1 life
Regeneration: +1 every 30 minutes (configurable)
Zero Lives: Cannot start levels. Options: Gold purchase or Rewarded Ad.
```

## Life Purchase

- Gold cost: configurable.
- UI: "+1 Life [XX Gold]"

## Rewarded Ad Life

- Available even with No Ads purchased.
- Player voluntarily starts ad → completes → +1 Life.
- Reward ONLY after successful ad completion.

---

# ═══════════════════════════════════════════
# SECTION 12 — SKILL SYSTEM
# ═══════════════════════════════════════════

## 12.1 Mega Ball

- **Effect**: Larger projectile + stronger physics impact + larger collision area.
- **Unlock**: Level 5.
- **Starting uses**: 3.
- **Consumption**: -1 use per activation.
- **Size**: configurable (e.g., 2× normal diameter).
- **Extra uses**: purchasable from Shop.

## 12.2 Infinity Ball

- **Effect**: Temporarily removes normal fire cooldown. Player can fire rapidly.
- **Unlock**: Level 10.
- **Starting uses**: 3.
- **Duration**: ~5 seconds (configurable).
- **Consumption**: -1 use per activation.
- **Activation**: Instant.
- **Extra uses**: purchasable from Shop.

## 12.3 Skill Stacking

Both skills can be active simultaneously:
```
Mega Ball = Active → Larger projectile
+ Infinity Ball = Active → No cooldown
= Large balls firing rapidly
```

Implementation must ensure both modifiers work without conflicts.

## 12.4 Skill UI States

| State | Display |
|---|---|
| Locked | 🔒 icon |
| Available | Skill icon + remaining uses count |
| Active (timer-based) | Skill icon + countdown timer |
| Depleted (0 uses) | Price / Shop shortcut |

## 12.5 Skill Consumption Safety

- Double-tap or repeated callbacks MUST NOT consume 2 uses unless the player genuinely activated twice.
- Skill inventory must NEVER go negative.

---

# ═══════════════════════════════════════════
# SECTION 13 — GOLD ECONOMY
# ═══════════════════════════════════════════

## 13.1 Gold Sources

- Starting Gold (configurable, e.g., 1000)
- Base Level Completion Reward (configurable, e.g., 50)
- Remaining Ball Bonus: `Remaining Balls × 2` (LOCKED formula)
- IAP Gold Packs

## 13.2 Gold Sinks

- Skill uses purchase
- Life purchase
- Shop bundles

## 13.3 Gold Display

- Always visible in Main Menu top area.
- Icon + formatted number (e.g., 🪙 1,250).

---

# ═══════════════════════════════════════════
# SECTION 14 — SHOP
# ═══════════════════════════════════════════

Accessible from bottom navigation in Main Menu.

## 14.1 Shop Contents

| Category | Description |
|---|---|
| Gold Packs | Small, Medium, Large, Premium (IAP, configurable amounts/prices) |
| Skill Packs | +X Mega Ball uses, +X Infinity Ball uses (Gold or IAP) |
| Gold + Skill Bundles | Combined value packs |
| Limited-Time Offers | Countdown timer (e.g., 24h), can repeat/cycle. MVP: same offer recycling is fine |
| No Ads | One-time IAP. Removes mandatory/interstitial ads. Rewarded ads stay |

## 14.2 Shop Design

- Clean, easy to understand.
- All prices: configurable.
- IAP product IDs: configurable.

---

# ═══════════════════════════════════════════
# SECTION 15 — ADS SYSTEM
# ═══════════════════════════════════════════

## 15.1 Mandatory / Interstitial Ads

- Shown at progression points (approximately every 5 levels).
- Shown after Lose.
- Removed by No Ads purchase.

## 15.2 Rewarded Ads

- Player-initiated voluntarily.
- Reward: +1 Life (or other configurable reward).
- Reward granted ONLY after successful ad completion.
- Available even with No Ads purchased.

## 15.3 Ad Safety

PREVENT:
- Double reward granting
- Duplicate life regeneration
- Duplicate Gold rewards
- Reward for cancelled/failed ads
- Same ad completion rewarded multiple times

Reward operations must be **idempotent**.

---

# ═══════════════════════════════════════════
# SECTION 16 — TUTORIAL
# ═══════════════════════════════════════════

- Shown **only on first launch** (tracked via `TutorialCompleted` flag).
- Uses the **real gameplay map** with real mortar/physics systems.
- Highlights a specific target area (circle, arrow, glow, etc.).
- Player MUST tap the highlighted area. Taps outside the area are **disabled/ignored**.
- After the guided first shot, control is released to the player.
- Tutorial flow:
  ```
  Level/map opens → Mortar ready → Target area highlighted
  → Player taps target → First ball fires → Object reacts
  → Tutorial releases control → Player finishes level normally
  → Tutorial complete → Normal progression begins
  ```

---

# ═══════════════════════════════════════════
# SECTION 17 — MAIN MENU
# ═══════════════════════════════════════════

## 17.1 Layout

```
┌────────────────────────────────┐
│ [Profile]        [Settings]    │
│                                │
│     [Gold]    [Lives]          │
│                                │
│                                │
│       CURRENT LEVEL            │
│         LEVEL XX               │
│          [PLAY]                │
│                                │
│ [Special]          [Special]   │
│  Offer               Offer    │
│                                │
│  Shop | Main Menu | Leaderboard│
└────────────────────────────────┘
```

## 17.2 Bottom Navigation

```
┌──────────┬──────────────┬─────────────┐
│   Shop   │  Main Menu   │ Leaderboard │
└──────────┴──────────────┴─────────────┘
```

Active tab visually distinguished.

## 17.3 Top Bar

- Left: Profile button
- Right: Settings button
- Between: Gold display + Life display (with regen timer if applicable)

## 17.4 Center

- Current level number prominently displayed.
- Large PLAY button.

## 17.5 Special Offers

- Left/right of center area.
- Limited-time bundles, Gold offers, Skill offers, No Ads.
- Must NOT obstruct the Play button.

---

# ═══════════════════════════════════════════
# SECTION 18 — PROFILE
# ═══════════════════════════════════════════

## 18.1 Profile Screen

- Avatar (from 8 defaults, all available from start, no unlock system)
- Username (requested at Level 10, unlimited free changes)
- Current Level
- Optional basic stats (Total Shots, Total Wins, Completed Levels)

## 18.2 Edit Profile

- Change: Username, Avatar, Frame.
- **8 default avatars**, all available. No gallery upload.
- **4 frames** (avatar border), all available. No unlock system.
- Changes saved locally.

---

# ═══════════════════════════════════════════
# SECTION 19 — LEADERBOARD
# ═══════════════════════════════════════════

## 19.1 Ranking

- Primary criterion: **Current Level** (higher = higher rank).
- Top 1000 players displayed.

## 19.2 Leaderboard Entry

Each row: Rank, Avatar, Username, Level.

## 19.3 Online Behavior

- Fetches current leaderboard data when internet available.
- Updates player's position after sync.

## 19.4 Offline Behavior

- Shows **last successfully synced/cached** leaderboard.
- Does NOT show stale data as guaranteed live data.
- On reconnect: syncs local progress → updates leaderboard.

---

# ═══════════════════════════════════════════
# SECTION 20 — SETTINGS
# ═══════════════════════════════════════════

Three toggles:

| Setting | Control | Affects |
|---|---|---|
| Music | ON/OFF toggle | Background music |
| Sound (SFX) | ON/OFF toggle | All gameplay sound effects |
| Vibration | ON/OFF toggle | All haptic feedback |

Settings accessible from:
- Main Menu (gear icon, top right)
- During Gameplay (gear icon, top right)

Gameplay state must be safely preserved when settings panel is opened/closed.

---

# ═══════════════════════════════════════════
# SECTION 21 — AUDIO SYSTEM
# ═══════════════════════════════════════════

## 21.1 SFX Categories

| Event | Sound Character |
|---|---|
| Ball Fire | Mortar/launch sound |
| Box Impact | Light, wood/cardboard hit |
| Stone Impact | Heavy, solid thud |
| Paint Bottle Break | Glass shatter + liquid splash |
| Column Impact | Heavy, deep impact |
| Cone Impact | Light, hollow hit |
| Object Destruction | Shrink/crumble |
| UI Button Click | Clean click |
| Win | Victory fanfare |
| Lose | Failure sound |
| Reward | Coin/collect sound |

## 21.2 Object-Specific Audio

Different objects MUST sound different. The player should identify object weight/type partially through audio.

---

# ═══════════════════════════════════════════
# SECTION 22 — VFX SYSTEM
# ═══════════════════════════════════════════

| Event | VFX |
|---|---|
| Ball Fire | Muzzle/fire effect at mortar |
| Ball Trail | Movement trail/particle while in flight |
| Impact (generic) | Brief impact burst at collision point |
| Impact (heavy) | Stronger, more visible burst |
| Object Destruction | Shrink/melt/dissolve particles |
| Paint Bottle Break | Glass shards + paint spill that lingers briefly |
| Skill Activation | Glow/flash on skill icon + optional projectile effect |
| Win | Celebratory particle burst |
| Lose | Subtle failure effect |
| UI Reward | Gold coin animation |

---

# ═══════════════════════════════════════════
# SECTION 23 — HAPTIC FEEDBACK
# ═══════════════════════════════════════════

| Event | Intensity |
|---|---|
| Ball Fire | Medium |
| Light Object Impact | Light |
| Heavy Object Impact | Strong |
| Paint Bottle Break | Medium-Strong |
| Skill Activation | Medium |
| Win | Strong |
| Lose | Light |

Intensity scales with object weight where applicable. All haptics disabled when Vibration = OFF.

---

# ═══════════════════════════════════════════
# SECTION 24 — GAMEPLAY HUD
# ═══════════════════════════════════════════

```
┌───────────────────────────────┐
│ 🏀 24              ⚙️        │
│                               │
│                               │
│          GAMEPLAY AREA        │
│                               │
│          ─────────            │
│           STAND               │
│                               │
│ [Mega]     MORTAR     [∞Ball] │
│    3                    🔒    │
└───────────────────────────────┘
```

- **Top-left**: Remaining Balls count (updates on each fire).
- **Top-right**: Settings button.
- **Bottom-left**: Mega Ball skill button.
- **Bottom-right**: Infinity Ball skill button.
- HUD must NOT obstruct objects or trajectories.

---

# ═══════════════════════════════════════════
# SECTION 25 — CAMERA & 3D WORLD
# ═══════════════════════════════════════════

## 25.1 3D World Feel

The game must convey a **3D scene** feel:
- **Foreground**: Mortar / player position
- **Midground**: Ball trajectory path
- **Background**: Stand + objects + environment

Objects behind other objects must convey depth.

## 25.2 Camera Configuration

- Camera must frame: stand, mortar, projectile trajectory, objects — all in a clear gameplay view.
- Exact FOV, position, orthographic size, perspective: implementation decision.
- Portrait orientation.

## 25.3 Environment

- MVP: Single main gameplay environment is fine.
- Visual background may vary by level or level group.
- Complex world map system NOT required.

---

# ═══════════════════════════════════════════
# SECTION 26 — SAVE SYSTEM
# ═══════════════════════════════════════════

## 26.1 Local Save Data

```
Current Level
Gold
Lives
Life Timer (regen countdown)
Skill Uses (Mega Ball, Infinity Ball)
Unlocked Skills
Username
Selected Avatar
Selected Frame
Settings (Music, Sound, Vibration)
No Ads Purchase State
Tutorial Completed Flag
Active Level State (for app-close detection)
```

## 26.2 Save Points

Save at:
- Level completion (win)
- Level failure (lose)
- Life deduction
- Gold gain/spend
- Skill purchase/consumption
- Username change
- Profile change
- Settings change

## 26.3 Save Safety

- Minimize risk of progress loss.
- Duplicate reward prevention: same event must NOT grant reward twice (across scene reloads, app restarts, internet reconnects, double-clicks).
- Life deduction safety: same loss event must NOT deduct life twice.
- Skill consumption safety: same activation must NOT consume twice.

---

# ═══════════════════════════════════════════
# SECTION 27 — OFFLINE MODE
# ═══════════════════════════════════════════

Core gameplay works **fully offline**:
- Start levels ✅
- Aim ✅
- Fire ✅
- Complete levels ✅
- Lose levels ✅
- Spend local resources ✅
- Progress through levels ✅

Network-dependent features wait for connection:
- Cloud sync
- Current leaderboard data
- IAP processing
- Ad loading

---

# ═══════════════════════════════════════════
# SECTION 28 — CLOUD SYNC
# ═══════════════════════════════════════════

## 28.1 Sync Rule

**"Latest local progress wins."** (MVP confirmed)

```
Cloud: Level 10
Player goes offline, plays to Level 12
Player reconnects
→ Cloud updated to Level 12
→ Leaderboard updated
```

## 28.2 Sync Data

- Current Level
- Gold
- Lives/state
- Skill inventory/uses
- Username
- Avatar
- Frame
- Relevant progression

## 28.3 Sync Philosophy

- Non-blocking where possible.
- Retryable.
- Protected against duplicate writes/rewards.
- Consistent with "latest local wins" rule.

## 28.4 No Login/Account

- No email login, password, social login, account registration.
- No cross-device syncing.
- Goal: single-device local + cloud persistence.

---

# ═══════════════════════════════════════════
# SECTION 29 — APP FLOW
# ═══════════════════════════════════════════

## 29.1 First Launch

```
App Start → Splash (dev/publisher logos) → Loading → Tutorial → Level 1
```

## 29.2 Returning Player

```
App Start → Splash → Loading → Main Menu → Current Level
```

## 29.3 Splash Screen

- Developer logo, publisher logo (placeholder templates fine for MVP).
- Not gameplay-critical.

## 29.4 Loading Screen

- Loading indicator (progress bar or spinner).
- Gameplay tip text.
- Simple background design.
- Single reusable loading design is sufficient for MVP.

---

# ═══════════════════════════════════════════
# SECTION 30 — ERROR HANDLING
# ═══════════════════════════════════════════

Handle gracefully:
- Internet disconnection → continue with local state
- Leaderboard request failure → show cached data
- Cloud sync error → retry later, don't block gameplay
- Ad load failure → skip ad, don't block gameplay
- IAP error → don't credit, show error message, allow retry
- App backgrounding → preserve state
- Scene loading issues → fallback gracefully

**Core gameplay must ALWAYS continue working** regardless of external service failures.

---

# ═══════════════════════════════════════════
# SECTION 31 — PERFORMANCE
# ═══════════════════════════════════════════

## 31.1 Requirements

1. Stable frame rate on reasonable mobile hardware.
2. Stable physics calculations.
3. Fast scene loading.
4. Avoid garbage allocation spikes.
5. Controlled VFX usage.
6. Efficient object management.

## 31.2 Object Pooling

Consider pooling for:
- Projectiles
- Impact VFX
- Destruction VFX
- Paint effects

Pooling is an optimization, not a gameplay requirement.

## 31.3 Level Scope

- Typically 20-30 projectiles + limited objects per level.
- Avoid: excessive Instantiate/Destroy spam, heavy particles, unnecessary physics calculations.

---

# ═══════════════════════════════════════════
# SECTION 32 — PLACEHOLDER ASSETS
# ═══════════════════════════════════════════

If original assets are unavailable, use **clearly identifiable placeholders**:

- Balls: colored sphere primitives
- Mortar: simple 3D model or primitive composition
- Box: cube with distinct color/material
- Stone: darker, rougher cube/sphere
- Column: elongated cylinder
- Cone: Unity cone primitive
- Paint Bottle: cylinder with cap
- Stand: flat platform primitive
- UI icons: simple symbolic icons
- Avatars: colored circles with initials
- Frames: colored rings
- Backgrounds: gradient skyboxes
- Logos: text-based placeholders

Placeholder system must allow **easy replacement** with real assets later.

---

# ═══════════════════════════════════════════
# SECTION 33 — VISUAL STYLE
# ═══════════════════════════════════════════

- Casual mobile puzzle feel.
- Bright, clear, readable gameplay.
- Clean 3D presentation.
- High object readability.
- Clear physics interactions.
- Satisfying destruction feel.
- Simple UI.
- Do NOT over-complicate visually.

---

# ═══════════════════════════════════════════
# SECTION 34 — OUT OF SCOPE (DO NOT BUILD)
# ═══════════════════════════════════════════

The following are **explicitly excluded** from MVP:

- Login / registration / email account
- Social login
- Cross-device progression
- Level selection screen
- Level replay
- Star rating / efficiency rating
- Daily chest / daily reward
- Complex achievement system
- Multiple currencies
- Multiple maps with independent progression
- Clan / guild system
- Multiplayer / PvP
- Complex wall-bounce mechanics
- Large skill system (only 2 skills)
- Complex inventory system
- Battle pass
- Complex quest system
- Character skill trees
- Equipment / crafting
- Energy currencies (beyond lives)
- Gallery photo upload for avatar

---

# ═══════════════════════════════════════════
# SECTION 35 — DEVELOPMENT PHASES
# ═══════════════════════════════════════════

> **INSTRUCTION**: Divide this entire project into sequential, testable phases as described below. Each phase MUST include:
> 1. Clear deliverables
> 2. Specific acceptance tests
> 3. Regression check (previous phases still work)
> 
> Do NOT proceed to the next phase until the current phase passes ALL tests.

## PHASE 1 — Project Setup & Core Scene
**Goal**: Empty Unity project with proper structure, basic scene, camera, mortar, and ground.

Deliverables:
- Unity 3D project (choose appropriate render pipeline)
- Portrait orientation configured
- Basic 3D scene: ground plane, camera positioned for gameplay view
- Mortar placeholder object at bottom-center
- Basic lighting
- Project folder structure: Scripts/, Prefabs/, ScriptableObjects/, Scenes/, Materials/, Audio/, VFX/, UI/

Acceptance Tests:
- [ ] Project opens and runs without errors
- [ ] Camera shows portrait gameplay view
- [ ] Mortar is visible at bottom-center
- [ ] Scene renders with basic lighting

---

## PHASE 2 — Input System & Mortar Movement
**Goal**: Touch input with Drag-to-move-mortar and Tap-to-fire differentiation.

Deliverables:
- Input manager distinguishing Tap vs Drag
- Mortar moves horizontally on drag
- Mortar clamped to gameplay bounds
- Tap detected (fire event raised but no ball yet)
- Visual debug indicator showing tap world-coordinate

Acceptance Tests:
- [ ] Dragging finger moves mortar left/right
- [ ] Mortar cannot leave screen bounds
- [ ] Drag does NOT trigger fire
- [ ] Tap triggers fire event (debug log)
- [ ] Tap point converts to correct world coordinate

---

## PHASE 3 — Projectile System & World-Coordinate Targeting
**Goal**: Firing balls at world-coordinate targets with physics.

Deliverables:
- Ball prefab with Rigidbody
- World-coordinate targeting: Screen Tap → Camera Ray → World Position → Launch Direction
- Ball launched toward target with physics force
- Ball affected by gravity
- Ball auto-destroys after lifetime (~5 sec)
- Ball fade/dissolve effect before destruction
- Fire cooldown system (~0.5 sec, configurable)
- Ball count system (configurable per level)
- Remaining Balls HUD counter

Acceptance Tests:
- [ ] Tapping fires a ball toward the tapped world point
- [ ] Ball follows physics arc (gravity affects trajectory)
- [ ] Ball auto-destroys after ~5 seconds with fade effect
- [ ] Cooldown prevents rapid-fire (except Infinity Ball later)
- [ ] Ball counter decrements correctly
- [ ] Firing into empty space: ball travels into world depth, not stuck at 2D boundary
- [ ] First ball of level has NO cooldown

---

## PHASE 4 — Physics Objects & Interactions
**Goal**: Placeable physics objects that react to ball impacts.

Deliverables:
- 5 object prefabs: Box, Stone, Column, Cone, PaintBottle
- Each with Rigidbody, Collider, configurable mass
- Ball collision → impulse transfer → objects move/topple
- Different weights feel different (Box light, Stone heavy)
- Object destruction flow: hit → move → destroy condition → shrink/melt → removed
- PaintBottle special destruction: impact → glass break → paint spill → disappear
- Object state machine (Idle → Hit → Moving → Destroying → Destroyed)
- Object-specific impact sounds (placeholder)

Acceptance Tests:
- [ ] Ball hitting a Box pushes it easily
- [ ] Ball hitting a Stone barely moves it
- [ ] Column can be toppled with sufficient force
- [ ] PaintBottle shatters with glass + paint VFX
- [ ] Stacked boxes react physically when bottom box is hit
- [ ] Objects fall off stand when pushed to edge
- [ ] Ball bounces off heavy objects naturally
- [ ] Objects don't pop instantly — they fade/shrink

---

## PHASE 5 — Stand & Level Completion Logic
**Goal**: Stand platform + Win/Lose detection.

Deliverables:
- Stand prefab (static platform, configurable width/length)
- Support for up to 2 platforms per level
- Level completion detection: all required objects cleared
- Physics settling check before declaring win
- Victory delay (~2 sec, configurable) before Win Panel
- Lose detection: Balls = 0 AND Objects remain
- Basic Win Panel (level complete, reward, next button)
- Basic Lose Panel (failed, retry, main menu)

Acceptance Tests:
- [ ] All objects cleared → ~2 sec delay → Win Panel appears
- [ ] Balls run out with objects remaining → Lose Panel appears
- [ ] Objects falling off stand count as "cleared"
- [ ] Physics settles before win is confirmed (no premature win)
- [ ] Win Panel shows correct remaining ball count
- [ ] Retry button reloads the level
- [ ] Win reward calculation: Base + (Remaining × 2)

---

## PHASE 6 — Level Progression & Data-Driven Levels
**Goal**: Sequential 30-level progression with data-driven level loading.

Deliverables:
- LevelData ScriptableObject (or equivalent) with full schema
- At least 5 handcrafted levels (Level 1-5) as starting content
- Remaining 25 levels with reasonable auto-generated or hand-placed layouts
- Sequential progression: complete Level N → unlock Level N+1
- Current level tracking in player save
- Level loading from LevelData
- Level 30: Win Panel without "Next Level" button

Acceptance Tests:
- [ ] Completing Level 1 unlocks Level 2
- [ ] Progression is purely sequential
- [ ] All 30 levels load from LevelData without errors
- [ ] Level 30 Win Panel has no "Next Level" button
- [ ] Current level persists across app restarts
- [ ] Each level has appropriate ball count and object placement

---

## PHASE 7 — Tutorial System
**Goal**: First-launch tutorial with guided input.

Deliverables:
- Tutorial flag (TutorialCompleted in save data)
- Tutorial overlay on Level 1 (first launch only)
- Highlighted target area (circle/arrow/glow)
- Input restriction: only highlighted area accepts taps
- After first guided shot: control released to player
- Tutorial does NOT replay on subsequent launches

Acceptance Tests:
- [ ] First launch shows tutorial on Level 1
- [ ] Tapping outside highlighted area does NOT fire
- [ ] Tapping highlighted area fires and hits target
- [ ] After first shot, full control restored
- [ ] Second app launch skips tutorial
- [ ] Tutorial uses real physics/mortar system

---

## PHASE 8 — Lives System & Exit Handling
**Goal**: Complete lives system with all loss/regen rules.

Deliverables:
- Lives system: start 5, max 5
- Life loss on: Lose (-1), Exit (-1), App close during level (-1)
- Win: no life loss
- Life regeneration timer: +1 every 30 min
- Zero lives: block level start, show options
- Life purchase with Gold (configurable price)
- Confirmation popup on level exit
- App close detection (OnApplicationPause/OnApplicationQuit)
- Life display in Main Menu (❤️ X/5 + timer)
- Duplicate life deduction prevention

Acceptance Tests:
- [ ] Starting lives = 5
- [ ] Losing a level deducts exactly 1 life
- [ ] Winning a level deducts 0 lives
- [ ] Exiting active level (confirmed) deducts 1 life
- [ ] App close during active level deducts 1 life on reopen
- [ ] Same event does NOT deduct twice
- [ ] Timer regenerates +1 life after 30 min
- [ ] At 0 lives, cannot start level
- [ ] Gold purchase grants +1 life

---

## PHASE 9 — Skill System
**Goal**: Mega Ball and Infinity Ball with full behavior.

Deliverables:
- Mega Ball: larger projectile, stronger impact, 3 starting uses, unlock at Level 5
- Infinity Ball: removes cooldown for ~5 sec, 3 starting uses, unlock at Level 10
- Skill activation UI buttons in gameplay HUD
- Skill states: Locked → Available → Active → Depleted
- Skill stacking (both active simultaneously)
- Skill consumption (-1 per activation, no negative inventory)
- Skill use persistence in save data
- Depleted state: show purchase option

Acceptance Tests:
- [ ] Before Level 5: Mega Ball shows as locked
- [ ] At Level 5: Mega Ball unlocks with 3 uses
- [ ] Mega Ball activation: larger ball, -1 use
- [ ] Before Level 10: Infinity Ball shows as locked
- [ ] At Level 10: Infinity Ball unlocks with 3 uses
- [ ] Infinity Ball: no cooldown for ~5 seconds, timer visible
- [ ] Both skills active simultaneously: large ball + rapid fire
- [ ] 0 uses: skill cannot be activated, shows purchase option
- [ ] Skill uses persist across app restarts

---

## PHASE 10 — Gold Economy & Rewards
**Goal**: Full Gold economy with earning, spending, and persistence.

Deliverables:
- Gold system: starting amount (configurable)
- Level completion reward: Base + (Remaining Balls × 2)
- Gold display in Main Menu
- Gold spending: lives, skill uses
- Gold persistence in save data
- Reward animation in Win Panel
- Duplicate reward prevention

Acceptance Tests:
- [ ] Starting Gold correct (configurable value)
- [ ] Completing a level with 15 remaining balls: Base + 30 Gold
- [ ] Gold updates immediately after reward
- [ ] Gold persists across app restarts
- [ ] Spending Gold for life: correct deduction
- [ ] Same level completion does NOT give reward twice
- [ ] Gold display updates in real-time

---

## PHASE 11 — Main Menu & UI System
**Goal**: Complete Main Menu with all navigation and UI panels.

Deliverables:
- Main Menu layout (as specified in Section 17)
- Bottom navigation: Shop | Main Menu | Leaderboard
- Current level display + Play button
- Gold + Lives display in top bar
- Profile button (top-left)
- Settings button (top-right)
- Special offer placeholders
- Scene transitions between Menu ↔ Gameplay
- Win Panel UI (polished)
- Lose Panel UI (polished)
- Confirmation popup (level exit)

Acceptance Tests:
- [ ] Main Menu shows correct current level
- [ ] Play button starts the current level
- [ ] Bottom navigation switches between sections
- [ ] Gold and Lives display correct values
- [ ] Settings accessible from Main Menu
- [ ] Scene transitions work smoothly
- [ ] Win/Lose panels display correctly
- [ ] All buttons have click feedback

---

## PHASE 12 — Profile & Settings
**Goal**: Profile editing and settings panel.

Deliverables:
- Profile screen: Avatar, Username, Current Level, basic stats
- 8 default avatars (placeholder images, all available)
- 4 frames (placeholder designs, all available)
- Username input field (prompted at Level 10)
- Username change: unlimited, free
- Settings panel: Music, Sound, Vibration toggles
- Settings persistence in save data
- Settings accessible during gameplay (without breaking state)

Acceptance Tests:
- [ ] Avatar selection works (8 options)
- [ ] Frame selection works (4 options)
- [ ] Username input at Level 10
- [ ] Username changeable unlimited times
- [ ] Music toggle mutes/unmutes background music
- [ ] Sound toggle mutes/unmutes SFX
- [ ] Vibration toggle enables/disables haptics
- [ ] Settings persist across app restarts

---

## PHASE 13 — Shop & IAP Framework
**Goal**: Shop UI with purchase flows.

Deliverables:
- Shop screen accessible from bottom navigation
- Gold Packs section (placeholder prices)
- Skill Packs section (Mega Ball uses, Infinity Ball uses)
- Gold + Skill Bundle section
- Limited-Time Offer with countdown timer
- No Ads purchase option
- IAP framework setup (Unity IAP or equivalent, configurable product IDs)
- Purchase confirmation and error handling

Acceptance Tests:
- [ ] Shop opens from bottom navigation
- [ ] All shop sections visible and navigable
- [ ] Gold pack purchase: correct Gold amount added
- [ ] Skill pack purchase: correct uses added
- [ ] Limited offer countdown timer counts down
- [ ] No Ads purchase: flags interstitial ads as disabled
- [ ] Purchase error: no items credited, error shown
- [ ] Double-click prevention on purchase buttons

---

## PHASE 14 — Ads Integration
**Goal**: Interstitial and Rewarded ad flows.

Deliverables:
- Ad SDK integration (configurable provider)
- Interstitial ads at progression points (every ~5 levels) and after Lose
- Rewarded ads for life recovery
- No Ads purchase disables interstitials
- Rewarded ads remain active regardless of No Ads
- Ad failure handling: skip gracefully, don't block gameplay
- Reward only after successful ad completion

Acceptance Tests:
- [ ] Interstitial shown after every 5th level completion
- [ ] Interstitial shown after Lose
- [ ] No Ads purchased: interstitials don't show
- [ ] Rewarded ad for life: +1 life after completion
- [ ] Rewarded ad available even with No Ads
- [ ] Ad fails to load: gameplay continues normally
- [ ] Cancelled ad: no reward given

---

## PHASE 15 — Leaderboard & Cloud Sync
**Goal**: Leaderboard display and cloud data sync.

Deliverables:
- Leaderboard screen (bottom navigation)
- Top 1000 display: Rank, Avatar, Username, Level
- Sorted by Current Level (descending)
- Online: fetch and display current data
- Offline: show last cached leaderboard
- Cloud sync: push local progress on reconnect
- "Latest local progress wins" rule
- Leaderboard service failure: graceful handling, no crash

Acceptance Tests:
- [ ] Leaderboard shows ranked players
- [ ] Online: fresh data displayed
- [ ] Offline: cached data displayed (not empty)
- [ ] Reconnect after offline play: local progress synced to cloud
- [ ] Cloud updated to latest local level
- [ ] Leaderboard service down: graceful error, no crash

---

## PHASE 16 — Audio & SFX
**Goal**: Object-specific sound effects and background music.

Deliverables:
- Background music (placeholder track, loops)
- All SFX from Section 21 (placeholder sounds acceptable)
- Object-specific sounds (Box=light, Stone=heavy, Bottle=glass, etc.)
- SFX plays on: fire, impact, destruction, UI interaction, win, lose
- Music/SFX respect Settings toggles
- Audio manager with proper channel separation

Acceptance Tests:
- [ ] Background music plays in Main Menu and Gameplay
- [ ] Each object type has a distinct impact sound
- [ ] Paint Bottle produces glass break + splash sound
- [ ] Win/Lose play appropriate sounds
- [ ] Music OFF → no music
- [ ] Sound OFF → no SFX
- [ ] Sounds don't stack/overlap excessively

---

## PHASE 17 — VFX & Visual Polish
**Goal**: All visual effects from Section 22.

Deliverables:
- Ball fire muzzle effect
- Ball trail/particle while in flight
- Impact VFX (generic + heavy)
- Object destruction particles (shrink/melt/dissolve)
- Paint Bottle: glass shards + paint spill (spill lingers)
- Skill activation effect (glow/flash)
- Win celebration particles
- UI reward animation (Gold coins)
- Ball fade/dissolve before destruction

Acceptance Tests:
- [ ] Ball fire shows muzzle effect
- [ ] Ball has visible trail during flight
- [ ] Impact at collision point visible
- [ ] Objects dissolve/shrink, NOT pop
- [ ] Paint Bottle: glass + paint VFX plays
- [ ] Skill activation: visual feedback on icon
- [ ] Win: celebration particles
- [ ] All VFX don't tank framerate

---

## PHASE 18 — Haptic Feedback
**Goal**: Tactile feedback for key events.

Deliverables:
- Haptic on: fire, impact (light/heavy), bottle break, skill activation, win, lose
- Intensity scales with object weight
- Disabled when Vibration = OFF
- Platform-appropriate haptic API (iOS Taptic, Android Vibration)

Acceptance Tests:
- [ ] Firing produces medium haptic
- [ ] Heavy object impact: strong haptic
- [ ] Light object impact: light haptic
- [ ] Vibration OFF: no haptics at all
- [ ] Haptics don't fire in menus (only gameplay events)

---

## PHASE 19 — Level Content & Balancing
**Goal**: All 30 levels fully designed, balanced, and tested.

Deliverables:
- 30 complete LevelData assets
- Difficulty curve following progression map (Section 9.3)
- New content introductions at Levels 5, 10, 15, 20, 25
- Two-layer (front+back row) levels in later stages
- Each level clearable with available ball count
- Varied stand configurations across levels
- Level 30: satisfying final challenge

Acceptance Tests:
- [ ] All 30 levels loadable and playable
- [ ] Each level is completable (not impossible)
- [ ] Difficulty ramps appropriately
- [ ] New content appears at milestone levels
- [ ] Later levels use complex multi-object combinations
- [ ] Level 30 feels like a satisfying finale
- [ ] Two-layer levels work correctly with physics

---

## PHASE 20 — Full Integration Testing & Polish
**Goal**: Complete game tested end-to-end with all systems verified.

Deliverables:
- Full playthrough test: Level 1 → Level 30
- All acceptance tests from Phases 1-19 re-verified
- Edge case testing (zero lives, zero Gold, offline play, app close, etc.)
- Performance optimization pass
- UI polish pass (responsive touch targets, animations)
- Bug fixes from integration testing
- Build verification (Android + iOS)

### Core Gameplay Acceptance Flow:
```
Open Game → Main Menu → Start Level → Mortar Visible → Ball Ready
→ Tap Target → Ball Fires → Ball Reaches World Target
→ Object Reacts Physically → Cooldown → Player Fires Again
→ Objects Topple/Break → All Objects Cleared → Win Panel
→ Reward Calculated → Next Level Startable
```

### Lose Flow:
```
Start Level → Fire Until Balls = 0 → Objects Remain
→ Lose Detected → -1 Life → Lose Panel → Retry/Menu
```

### Exit Flow:
```
Start Level → Exit → Confirmation → Confirm → -1 Life → Main Menu
```

### App Close Flow:
```
Active Level → App Killed → Reopen → -1 Life (no duplicate)
```

### Skill Flow:
```
Level 5+: Mega Ball available → Activate → Larger ball → -1 use
Level 10+: Infinity Ball available → Activate → No cooldown → Timer → Normal cooldown returns
Both active: Stack works correctly
```

### Economy Flow:
```
Starting Gold + Level Rewards + Remaining Ball Bonus - Purchases = Correct persistent balance
```

### Offline Flow:
```
Online Level 10 → Go offline → Play to Level 12 → Reconnect → Cloud = Level 12
```

### No Ads Flow:
```
No Ads OFF: Interstitials show
No Ads ON: Interstitials don't show, Rewarded ads still work
```

Acceptance Tests:
- [ ] Full game playable from Level 1 to Level 30
- [ ] All systems interact correctly without conflicts
- [ ] No duplicate rewards/life deductions
- [ ] Offline play works completely
- [ ] Cloud sync works on reconnect
- [ ] Performance stable throughout all 30 levels
- [ ] No physics explosions or instabilities
- [ ] All UI responsive and readable
- [ ] Save data persists correctly across all scenarios
- [ ] Level 30 completion shows proper ending (no Next Level button)

---

# ═══════════════════════════════════════════
# SECTION 36 — DEVELOPMENT GATE RULES
# ═══════════════════════════════════════════

After each phase:
1. **Verify** all acceptance tests pass.
2. **Regression check**: Previous phase functionality must NOT be broken.
3. **Do NOT add** systems not described in this prompt.
4. **Do NOT skip** documented requirements.
5. **Do NOT replace** confirmed rules with assumptions.
6. If a value is unknown, use a reasonable configurable default and label it "CONFIGURABLE".
7. If an asset is missing, use a **placeholder** and label it "PLACEHOLDER".

---

# ═══════════════════════════════════════════
# SECTION 37 — AI BEHAVIOR RULES
# ═══════════════════════════════════════════

1. **Follow locked requirements exactly.** No exceptions.
2. **Do NOT invent** systems not described here (no daily quests, no PvP, no battle pass, etc.).
3. **Configurable values** must be exposed as parameters, NOT hardcoded.
4. **State assumptions explicitly.** If you choose a technical approach, document it.
5. **Build data-driven.** Levels, objects, and economy values loaded from data, not code.
6. **Build modular.** Each system should be independent and replaceable.
7. **Stay in scope.** The out-of-scope list (Section 34) is absolute.
8. **Test critically.** Run acceptance tests at each phase gate.
9. **Preserve mobile ergonomics.** Touch-first, fast response, clear visual feedback.
10. **No hallucination.** Do NOT convert "Unknown" into "Confirmed." If unsure, use configurable defaults.
11. **Gameplay rules are IMMUTABLE.** Technical implementation can vary; player-facing gameplay rules CANNOT be changed.

---

# ═══════════════════════════════════════════
# SECTION 38 — FUTURE EXTENSIBILITY
# ═══════════════════════════════════════════

Architecture should make it easy to add (WITHOUT rewriting core systems):
- Level 31+ (just add LevelData)
- New object types (implement same object interface)
- New skills (add to skill system)
- New backgrounds/environments
- New shop offers
- New cosmetics (avatars, frames)

---

# ═══════════════════════════════════════════
# SECTION 39 — FINAL CRITICAL RULES CHECKLIST
# ═══════════════════════════════════════════

Before considering the project complete, verify ALL of these:

1. ✅ Tap fires at world-coordinate target
2. ✅ Drag moves mortar, does NOT fire
3. ✅ Ball uses real physics
4. ✅ Objects react physically to impacts
5. ✅ Balls are limited per level
6. ✅ Balls can bounce back toward player
7. ✅ Balls auto-destroy after lifetime with fade effect
8. ✅ Empty-space balls travel into 3D world depth
9. ✅ All objects cleared = level won
10. ✅ Balls depleted + objects remain = level lost
11. ✅ Win costs 0 lives
12. ✅ Lose costs 1 life
13. ✅ Exit costs 1 life
14. ✅ App close during level costs 1 life
15. ✅ Lives regenerate (+1 per 30 min)
16. ✅ Mega Ball unlocks at Level 5
17. ✅ Infinity Ball unlocks at Level 10
18. ✅ Skills can stack
19. ✅ Remaining Balls × 2 = Gold bonus (LOCKED)
20. ✅ Gold is the only currency
21. ✅ Offline gameplay works fully
22. ✅ Cloud sync: latest local wins
23. ✅ Offline leaderboard shows cached data
24. ✅ No Ads removes only mandatory/interstitial ads
25. ✅ Rewarded ads always available
26. ✅ 30 MVP levels, sequential, no level select
27. ✅ Level 30: Win Panel, NO Next Level button
28. ✅ Username prompted at Level 10
29. ✅ New content every 5 levels
30. ✅ Objects don't pop — they fade/shrink/melt

---

**END OF MASTER BUILD PROMPT**

*This prompt was generated from the Royal Smash! Physics Puzzle comprehensive game documentation (373 documented items, 7271 lines). It serves as the absolute source of truth for MVP development.*
