# Phase Workflow & Git Commit Rules

## Development Workflow (MANDATORY)

This project follows a strict **phase-based development workflow** as defined in the Master Build Prompt (`_FarzetkiDocs/master_build_prompt.md`, Section 35).

### Phase Completion Flow

Every phase MUST follow this exact sequence:

```
1. Complete all deliverables for Phase N
2. Run acceptance tests (verify in console / gameplay)
3. Run regression tests (previous phases still work)
4. STOP and ask user for approval:
   "Phase N tamamlandı. Kontrol edip onaylar mısın?"
5. WAIT for user approval
6. On approval → Execute git commit + push (see format below)
7. STOP and ask: "Phase N commit edildi ve push edildi. Sonraki phase'e geçeyim mi?"
8. WAIT for user approval
9. On approval → Begin Phase N+1
```

### Git Commit Format

When the user approves a completed phase, execute these commands in order:

```bash
git add .
git commit -m "<commit message>"
git push origin dev
```

### Commit Message Format

```
Feat(Phase X): <Brief phase title>

- Added <feature 1>
- Added <feature 2>
- Implemented <feature 3>
- Configured <setting 1>
- Created <file/prefab/SO>
...
```

**Example:**
```
Feat(Phase 1): Project Setup & Core Scene

- Created Unity 3D project structure under Assets/_Project/
- Configured portrait orientation (1080x1920)
- Set up basic 3D scene with ground plane and camera
- Added mortar placeholder at bottom-center
- Configured basic lighting
- Created folder structure: Scripts/, Prefabs/, ScriptableObjects/, Scenes/
```

**Example:**
```
Feat(Phase 3): Projectile System & World-Coordinate Targeting

- Implemented world-coordinate targeting (Screen → Ray → World Position)
- Added ball prefab with Rigidbody and physics force launch
- Implemented fire cooldown system (~0.5s configurable)
- Added ball auto-destroy after lifetime with fade effect
- Created ball count system with HUD counter
- Configured continuous collision detection for projectiles
```

### Rules

1. **NEVER skip the approval step.** Always ask the user before committing.
2. **NEVER proceed to the next phase without explicit approval.**
3. **Commit messages MUST be in English.**
4. **Each commit covers exactly ONE phase** — no multi-phase commits.
5. **Always push to `dev` branch** — never push directly to `main`.
6. **List every meaningful change** in the commit body (bullet points).
7. **Use conventional commit prefix**: `Feat(Phase X):` for new phases.
8. **If a phase requires fixes after approval**, use: `Fix(Phase X): <description>`
9. **If assets/docs are updated separately**, use: `Chore: <description>`

### Branch Strategy

```
main   — stable, production-ready (never push directly)
dev    — active development (all phase commits go here)
```

### What To Do If Tests Fail

If acceptance tests fail after completing a phase:
1. Fix the issues
2. Re-run tests
3. Do NOT ask for approval until all tests pass
4. Mention what was fixed in the approval request

### What To Do If User Requests Changes

If the user requests changes after reviewing but before approving:
1. Make the requested changes
2. Re-run tests
3. Ask for approval again
4. Only then commit + push
