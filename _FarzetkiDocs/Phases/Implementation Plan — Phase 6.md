# Implementation Plan — Phase 6: Skills Foundation (MegaBall & InfinityBall)

Implementation plan and completion record for Phase 6: Skills Foundation (MegaBall & InfinityBall).

## 1. Overview & Architecture

Per **Section 2 (Configurable Values)** and **Section 59–67 (Skill System)** of `RoyalSmashInformationFullVersion.md` and `master_build_prompt.md`:

### Core Systems
1. **Skill Data & Configuration**:
   - `SkillType.cs` enum: `None`, `MegaBall`, `InfinityBall`.
   - `SkillConfig.cs` (ScriptableObject):
     - MegaBall: `DiameterMultiplier = 2.0f`, `MassMultiplier = 2.5f`, `DefaultUses = 3`, `UnlockLevel = 5`, `UsePrice = 50`.
     - InfinityBall: `Duration = 5.0f` seconds, `RapidFireCooldown = 0.06f`, `DefaultUses = 3`, `UnlockLevel = 10`, `UsePrice = 100`.

2. **Skill Management**:
   - `SkillManager.cs`: Singleton service in `RoyalSmash.Skills`.
   - Manages unlock progression, remaining uses, active states, timer coroutines, and event dispatching.
   - Supports **Skill Stacking** (both MegaBall and InfinityBall active simultaneously).

3. **Gameplay Integration**:
   - `MortarController.cs`: Integrates rapid fire cooldown (InfinityBall) and projectile upgrade invocation (MegaBall).
   - `ProjectileController.cs`: Implements `ApplyMegaBall(float diameterMultiplier, float massMultiplier)` with visual gold tint and scaled collider/mass.

4. **HUD UI**:
   - `SkillButtonUI.cs`: TextMeshPro button displaying skill state, use badge counter, lock overlay, active glow, and timer countdown.
   - Symmetrical bottom HUD layout in `MainCanvas`.

5. **Acceptance Verification**:
   - `Phase6PlayModeTest.cs`: 5/5 Play Mode acceptance tests passed with zero errors and zero warnings.
