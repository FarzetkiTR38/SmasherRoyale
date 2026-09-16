# Implementation Plan — Phase 4: Physics Objects & Interactions

Placeable, interactive 3D physics objects that react realistically to ball impacts, topple, stack, and transition through life states toward graceful destruction.

## 1. Overview & Architecture

### Core Objectives
1. **Configurable Physics Architecture**:
   - `ObjectConfig.cs` (ScriptableObject): Data-driven parameters per object type (`mass`, `bounciness`, `friction`, `durability`, `isBreakable`, `breakForceThreshold`, `shrinkDuration`, `objectType`, `impactSoundType`).
   - `ObjectState` state machine: `Idle → Hit → Moving → DestroyCondition → Destroying → Destroyed`.
   - `PhysicsObject.cs`: Base component attached to all target objects. Handles Rigidbody caching, impulse reaction, state transitions, fall-off-stand detection, and smooth shrink/fade auto-destruction.
   - `PaintBottleObject.cs`: Specialized breakable object. Shatters on sufficient impact force with glass fracture and paint splash effects instead of merely tumbling.

2. **The 5 Object Types & Weights**:
   | Object | Shape & Collider | Visual Style | Mass (kg) | Bounciness / Friction | Destruction Behavior |
   |---|---|---|---|---|---|
   | **Box** | BoxCollider ($1 \times 1 \times 1$) | Royal Toy Block (Royal blue / warm gold trim) | $1.0\text{ kg}$ | $0.2 / 0.4$ | Movable, stackable; shrinks/melts when falling off stand |
   | **Stone** | BoxCollider / Rounded ($1 \times 1 \times 1$) | Heavy Granite/Slate Gray | $5.0\text{ kg}$ | $0.05 / 0.7$ | High inertia, absorbs impacts; topples only under sustained hits |
   | **Column** | Capsule/Cylinder ($0.8 \times 2.4 \times 0.8$) | Tall Candy Pillar (Warm orange) | $3.0\text{ kg}$ | $0.1 / 0.5$ | High center of mass; designed to topple and trigger chain reactions |
   | **Cone** | MeshCollider (convex) / Capsule | Candy Cone (Vivid yellow) | $0.8\text{ kg}$ | $0.25 / 0.35$ | Light, rolls and tumbles easily across the platform |
   | **PaintBottle** | Capsule / Cylinder ($0.6 \times 1.2 \times 0.6$) | Glossy Enamel Bottle (Magenta/Cyan) | $0.5\text{ kg}$ | $0.1 / 0.4$ | **Breakable**: Shatters on impact $> 8\text{ Ns}$ with glass & paint VFX |

3. **Physics Interaction & Destruction Rules**:
   - Layer 7: `PhysicsObject`. Collides with Layer 6 (`Projectile`), Layer 8 (`Stand`), Layer 9 (`Ground`), and Layer 7 (`PhysicsObject`).
   - Ball collision transfers impulse ($J = m \Delta v$) based on projectile velocity and mass ratio.
   - Fall-off-stand detection: When $Y < 0.2\text{ m}$ (falls off the stand platform toward the ground) or collides with Layer 9 (`Ground`), trigger destruction flow.
   - Objects never pop instantly: smooth shrink scale interpolation over $0.5\text{s}$ before GameObject destruction.

---

## 2. Proposed File Changes

### Configurations & ScriptableObjects
#### [NEW] [ObjectConfig.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/ObjectConfig.cs)
- Exposes `ObjectType`, `mass`, `bounciness`, `friction`, `isBreakable`, `breakForceThreshold`, `shrinkDuration`, `impactSoundType`.
- ScriptableObject asset instances for each type:
  - `Config/ObjectConfig_Box.asset`
  - `Config/ObjectConfig_Stone.asset`
  - `Config/ObjectConfig_Column.asset`
  - `Config/ObjectConfig_Cone.asset`
  - `Config/ObjectConfig_PaintBottle.asset`

### Gameplay & Object Scripts
#### [NEW] [ObjectState.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/ObjectState.cs)
- Enum: `Idle`, `Hit`, `Moving`, `DestroyCondition`, `Destroying`, `Destroyed`.

#### [NEW] [PhysicsObject.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/PhysicsObject.cs)
- Core base MonoBehaviour for all puzzle objects:
  - Rigidbody & Collider setup from `ObjectConfig`.
  - Collision callback `OnCollisionEnter`: detects projectiles, computes impact impulse, transitions `Idle → Hit → Moving`.
  - Fall-off detection in `FixedUpdate`: checks height relative to stand or ground layer.
  - Smooth shrink & fade coroutine (`DestroyRoutine`), unregistering on destruction.
  - Events: `OnStateChanged`, `OnObjectHit`, `OnObjectCleared`.

#### [NEW] [PaintBottleObject.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Gameplay/PaintBottleObject.cs)
- Inherits from `PhysicsObject`.
- Checks collision impact impulse: if force exceeds `breakForceThreshold`, triggers `Break()`:
  - Spawns glass shards and paint splash particle placeholders.
  - Hides bottle mesh instantly, leaves paint stain briefly, removes object.

### Materials & Prefabs
#### [NEW] Materials in `Assets/_Project/Materials/`:
- `M_ObjectBox.mat` (Royal Blue `#2563EB`, smooth toy plastic)
- `M_ObjectStone.mat` (Granite Slate `#475569`, rough matte)
- `M_ObjectColumn.mat` (Candy Orange `#EA580C`, smooth enamel)
- `M_ObjectCone.mat` (Vivid Yellow `#FACC15`, gloss)
- `M_ObjectPaintBottle.mat` (Magenta `#D946EF`, glass/enamel shine)
- `M_PaintSplat.mat` (Magenta paint puddle decal/quad)

#### [NEW] PhysicMaterials in `Assets/_Project/Materials/`:
- `PM_Box.physicsMaterial` ($B=0.2, F=0.4$)
- `PM_Stone.physicsMaterial` ($B=0.05, F=0.7$)
- `PM_Column.physicsMaterial` ($B=0.1, F=0.5$)
- `PM_Cone.physicsMaterial` ($B=0.25, F=0.35$)
- `PM_PaintBottle.physicsMaterial` ($B=0.1, F=0.4$)

#### [NEW] Prefabs in `Assets/_Project/Prefabs/Gameplay/`:
- `Box.prefab`
- `Stone.prefab`
- `Column.prefab`
- `Cone.prefab`
- `PaintBottle.prefab`

---

## 3. Verification Plan

### Autonomous In-Editor Verification (Unity MCP)
1. **Asset & Hierarchy Integrity**:
   - Inspect all 5 prefabs: Rigidbody, Collider, PhysicsMaterial, Layer 7 (`PhysicsObject`), and `PhysicsObject` components properly wired.
   - Check ScriptableObject assets exist with correct masses ($1.0, 5.0, 3.0, 0.8, 0.5\text{ kg}$).

2. **Automated Play Mode Suite ([Phase4PlayModeTest.cs](file:///c:/Users/Farzetki/wkspaces/SmasherRoyale-SmasherRoyale/Assets/_Project/Scripts/Utils/Phase4PlayModeTest.cs))**:
   - **Test 1 (Box Push)**: Spawn Box on stand; fire ball; verify Box receives impulse and moves significantly ($> 1.0\text{ m}$).
   - **Test 2 (Stone Heavy Resistance)**: Spawn Stone on stand; fire ball with same force; verify Stone barely displaces ($< 0.3\text{ m}$).
   - **Test 3 (Column Topple)**: Spawn Column upright; hit upper section; verify Column tilts and topples over.
   - **Test 4 (PaintBottle Shatter)**: Spawn PaintBottle; hit with projectile; verify bottle shatters and is removed immediately via glass/paint effect.
   - **Test 5 (Fall-Off-Stand Destruction)**: Push Box off platform; verify that upon falling ($Y < 0.2\text{ m}$), it begins shrink/melt and gets destroyed (does not pop instantly).
   - **Test 6 (Stacked Physics)**: Stack 2 boxes vertically; hit lower box; verify upper box falls physically under gravity.
   - **Test 7 (Visual Capture)**: Capture Game View screenshot showing objects placed on the stand, reactions to hits, and shatter effects.

3. **Console Verification**:
   - Ensure 0 errors and 0 warnings in Unity Console.
