# Implementation Plan — Visual & Environment Showcase (Option B)

> **Document Type**: Phase Specification & Implementation Plan  
> **Target**: Visual Showcase — 6-Color Paint Bottle Shelf & Fairytale Kingdom Scenery  
> **Source of Truth**: `_FarzetkiDocs/master_build_prompt.md`, `_FarzetkiDocs/Phases/Prompts_for_3D_Models.md`  

---

## 1. Reference & Objectives
- **Reference**: Reference Image 3 (6x5 Grid of colorful paint bottles on royal stand) and `Prompts_for_3D_Models.md` (Items 2, 3, 18).
- **Core Goal**: Transform the gameplay environment into a high-fidelity visual experience:
  1. Backdrop: Stylized royal fairytale kingdom with blue sky, puffy clouds, fairytale castle, rolling green hills, and flower lawn.
  2. Stand: Royal purple platform slab with gold trim mounted on a tapered gold/purple pedestal base.
  3. Objects: 30 stylized glossy paint bottles arranged in 6 color columns (Red, Orange, Yellow, Blue, Purple, Pink) × 5 rows.
  4. Mechanics: Full physics stability on idle, explosive shatter and color-burst effects on impact.

---

## 2. Deliverables
1. **Backdrop Texture & Material**:
   - `Assets/_Project/Textures/Environment/KingdomBackdrop.jpg`
   - `Assets/_Project/Materials/M_KingdomBackdrop.mat`
2. **Bottle Materials**:
   - `Assets/_Project/Materials/Bottles/M_Bottle_Red.mat`
   - `Assets/_Project/Materials/Bottles/M_Bottle_Orange.mat`
   - `Assets/_Project/Materials/Bottles/M_Bottle_Yellow.mat`
   - `Assets/_Project/Materials/Bottles/M_Bottle_Blue.mat`
   - `Assets/_Project/Materials/Bottles/M_Bottle_Purple.mat`
   - `Assets/_Project/Materials/Bottles/M_Bottle_Pink.mat`
   - `Assets/_Project/Materials/Bottles/M_Cap_*.mat` (6 individual color-matched candy-striped collar materials)
3. **Stand Overhaul**:
   - Upgraded Stand with gold tapered pillar, dual-tier base, and purple beveled platform with gold accent rim.
4. **Scenes Updated**:
   - `Assets/_Project/Scenes/MainGame.unity`
   - `Assets/_Project/Scenes/PhysicsTestScene.unity`
5. **Scripts**:
   - `Assets/_Project/Scripts/Editor/VisualShowcaseSetup.cs` (scene automation & generator)
   - `Assets/_Project/Scripts/Editor/VisualShowcasePlayModeTest.cs` (verification suite)
   - `Assets/_Project/Scripts/Gameplay/PaintBottleObject.cs` (sleep-on-start stability)

---

## 3. Acceptance Criteria
- [x] Backdrop quad covers camera view with seamless kingdom scenery.
- [x] Stand pedestal base aligns with the flower lawn in front of the bridge in the backdrop.
- [x] 30 bottles spawned in 6 distinct vertical color columns (Red, Orange, Yellow, Blue, Purple, Pink) × 5 rows.
- [x] Bottles remain stationary on level start without toppling or trembling.
- [x] Direct cannon shot shatters the targeted bottle and triggers particle spray matching the bottle's color.
- [x] Zero Unity compilation or runtime console errors.

---

## 4. Verification Results & Visual Evidence
- **Automated Test Results**:
  - `PASS: KingdomBackdrop quad exists with material`
  - `PASS: Royal Stand exists with BoxCollider`
  - `PASS: Exactly 30 Paint Bottles found in scene`
  - `PASS: All 6 distinct paint colors represented (5 bottles per color)`
  - `PASS: Direct shot shatters target bottle, ball count decrements (25 -> 24 -> 23)`
  - `PASS: Upper bottles realistically drop into cleared space`
- **Console Log**: 0 errors, 0 warnings.
- **Visual Captures**:
  - `Assets/Screenshots/screenshot-20260916-232422.png` (Idle state, all 30 bottles rock-solid on stand)
  - `Assets/Screenshots/screenshot-20260916-232529.png` (Action: Yellow bottle shattered on direct hit, paint spray)
  - `Assets/Screenshots/screenshot-20260916-232600.png` (Action: Pink column wiped off and shattered)
  - `Assets/Screenshots/screenshot-20260916-232706.png` (Clean reset state)
