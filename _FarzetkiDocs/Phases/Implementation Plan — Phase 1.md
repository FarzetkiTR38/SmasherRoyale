# Implementation Plan — Phase 1: Project Setup & Core Scene

Implementation of **Phase 1** for **Royal Smash! — Physics Puzzle** based on the Master Build Prompt and Project Rules.

## Overview
Phase 1 establishes the rock-solid foundation for the 3D mobile physics puzzle game:
- Directory architecture under `Assets/_Project/`
- Mobile portrait orientation & URP settings
- Physics layers & tags
- Core gameplay scene (`MainGame.unity`) with lighting, ground plane, gameplay camera framing, and a stylized placeholder mortar
- Zero-delegation verification using Unity MCP (compilation check, console error check, game view screenshot verification)

---

## Proposed Changes

### 1. Project Configuration & Folder Structure

Set up the project folders and settings according to the specification.

#### [NEW] Directory Hierarchy
Create directories under `Assets/_Project/`:
- `Scripts/Core/`, `Scripts/Gameplay/`, `Scripts/Objects/`, `Scripts/Levels/`, `Scripts/Skills/`, `Scripts/Economy/`, `Scripts/Lives/`, `Scripts/UI/`, `Scripts/Audio/`, `Scripts/Ads/`, `Scripts/Cloud/`, `Scripts/Save/`, `Scripts/Tutorial/`, `Scripts/Profile/`, `Scripts/Utils/`
- `Prefabs/Gameplay/`, `Prefabs/UI/`, `Prefabs/VFX/`
- `ScriptableObjects/Config/`, `ScriptableObjects/Levels/`, `ScriptableObjects/Objects/`
- `Materials/`, `Textures/`, `Audio/Music/`, `Audio/SFX/`, `Scenes/`, `Animations/`, `UI/Fonts/`, `UI/Sprites/`

#### Settings Configuration (via `execute_code` & Unity MCP)
- Set PlayerSettings:
  - Default Interface Orientation: `UIOrientation.Portrait`
  - Allowed orientations: Portrait enabled, Landscape disabled
- Set Physics Layers in TagManager:
  - Layer 6: `Projectile`
  - Layer 7: `PhysicsObject`
  - Layer 8: `Stand`
  - Layer 9: `Ground`
  - Layer 10: `Boundary`

---

### 2. Materials & Visual Setup

Create clean, modern placeholder materials using URP Lit shaders:
- `M_Ground.mat`: Neutral floor with subtle contrast (`#2D3238` or soft stylized grid tone)
- `M_MortarBase.mat`: Metallic/regal dark tone (`#1E232A`)
- `M_MortarBarrel.mat`: Royal blue/gold accent (`#3A6EA5` / `#D4AF37`)
- `M_MortarStand.mat`: Base pedestal (`#414853`)

---

### 3. Core Gameplay Scene (`Assets/_Project/Scenes/MainGame.unity`)

#### Scene Hierarchy:
1. **Directional Light**:
   - Rotated for clean angled sunlight `(50, -30, 0)`
   - Soft shadows enabled, warm casual color (`#FFF8F0`)
2. **Main Camera**:
   - Positioned for portrait view: e.g. position `(0, 6, -10)`, rotation `(22, 0, 0)`, Field of View ~`55`
   - Tagged as `MainCamera`
   - Clear flags / background: Skybox or gradient background
3. **Environment**:
   - `Ground`: 3D Plane at `(0, 0, 0)`, scaled `(4, 1, 6)`, Layer 9 (`Ground`), with BoxCollider/MeshCollider
4. **Mortar Placeholder**:
   - Positioned at bottom-center of the gameplay view: position `(0, 0.5, -4.5)`
   - Composed of:
     - `Base` (Stylized cylindrical or beveled pedestal)
     - `Pivot` / `Barrel` (Angled cylinder pointed toward the target stand area at ~`25°` elevation)
   - Tagged as `Mortar` / Untagged with clean hierarchy
5. **Add Scene to Build Settings**:
   - Add `Assets/_Project/Scenes/MainGame.unity` as Scene 0.

---

## Verification Plan

### Automated & MCP-Driven Verification
1. **Console Check**:
   - Call `unityMCP.read_console(action="get", types=["error", "warning"])`
   - Ensure 0 compilation errors or runtime warnings.
2. **Hierarchy Verification**:
   - Call `unityMCP.manage_scene(action="get_hierarchy")`
   - Ensure all GameObjects, transforms, and layers exist properly.
3. **Visual Game View Capture**:
   - Call `unityMCP.manage_camera(action="screenshot", capture_source="game_view", include_image=true)`
   - Verify:
     - Aspect ratio / framing is portrait.
     - Mortar is clearly visible at the bottom-center.
     - Ground and lighting provide depth and clarity.
4. **Play Mode Smoke Test**:
   - Enter play mode via `unityMCP.manage_editor(action="play")`
   - Inspect console logs.
   - Stop play mode via `unityMCP.manage_editor(action="stop")`.

### User Approval Gate
- Present the verified results and screenshot in `walkthrough.md`.
- Request user approval.
- Upon approval, provide the commit message / commit changes, then request confirmation before advancing to Phase 2.
