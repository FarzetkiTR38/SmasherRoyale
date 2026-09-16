---
name: Phase Implementation & Documentation Workflow
description: >
  Mandatory workflow for every phase in Royal Smash. Enforces creating, maintaining,
  and archiving phase implementation plans directly into _FarzetkiDocs/Phases/Implementation Plan — Phase X.md
  with deliverables, architecture, acceptance tests, and completion records.
---

# Phase Implementation & Documentation Workflow Skill

## Applies To
All development phases (Phases 1 through 30+), system additions, and milestones in the Royal Smash project.

---

## 1. The Core Rule: Phase Documentation in `_FarzetkiDocs/Phases/`

Whenever a Phase is planned, initiated, or updated:
> **MANDATORY**:
> You MUST create and maintain the implementation plan document inside the workspace directory:
> `_FarzetkiDocs/Phases/Implementation Plan — Phase X.md`
> (where `X` is the current phase number, e.g., `Phase 1`, `Phase 2`, `Phase 7`, `Phase 8`, etc.).

Do NOT only keep it in temporary brain memory or scratch folders. It must exist as a persistent project documentation file in `_FarzetkiDocs/Phases/`.

---

## 2. Document Structure

Every `Implementation Plan — Phase X.md` must contain:

1. **Title & Reference Mapping**:
   - Explicit reference to the relevant sections in `_FarzetkiDocs/master_build_prompt.md` and `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`.
2. **Overview & Architecture**:
   - Core systems, classes, ScriptableObjects, and event flow.
   - Enums, data structures, and configuration parameters.
3. **Component Deliverables**:
   - Exact scripts created or modified with namespaces (`RoyalSmash.*`).
   - Prefab, asset, or scene hierarchy changes.
4. **Acceptance Criteria & Testing**:
   - Numbered checklist of verification requirements.
   - Reference to the automated test script (`PhaseXPlayModeTest.cs`).
5. **Completion Record & Verification Status**:
   - Pass/Fail verification results.
   - Captured screenshots and visual proof paths.

---

## 3. Workflow Steps for Every Phase

1. **Phase Start**:
   - Read the Master Build Prompt requirements for the phase.
   - Create `_FarzetkiDocs/Phases/Implementation Plan — Phase X.md` outlining the architecture, deliverables, and acceptance tests.
2. **Implementation**:
   - Write clean, decoupled C# scripts adhering to Single Responsibility and Master Build Prompt guidelines.
   - Zero-delegation editor execution via Unity MCP (`execute_code`, `manage_scene`, etc.).
3. **Verification**:
   - Verify zero errors in Unity Console (`read_console`).
   - Run automated Play Mode test suite and capture screenshots.
4. **Phase Finalization**:
   - Update `_FarzetkiDocs/Phases/Implementation Plan — Phase X.md` with the final acceptance test results.
   - Commit all changes to Git with a conventional commit message.
