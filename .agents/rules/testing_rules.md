# Testing & Verification Rules

## After Every Code Change

1. **Compilation Check**: Ensure zero errors in Console. Warnings are acceptable but should be addressed.
2. **Scene Integrity**: Verify the active scene still loads and runs.
3. **Regression Check**: Previously working features must still work.

## Phase Gate Testing

This project follows sequential development phases (see Master Build Prompt Section 35).
After completing each phase:

1. Run ALL acceptance tests for that phase.
2. Run regression tests for ALL previous phases.
3. Do NOT proceed to the next phase until all tests pass.
4. Document any known issues or limitations.

## Critical Test Scenarios

### Physics Tests
- Ball fires toward tapped world coordinate (not random direction)
- Ball affected by gravity (arcs downward)
- Ball bounces off heavy objects
- Light objects (Box) pushed easily by ball impact
- Heavy objects (Stone) resist movement
- Stacked objects react physically when bottom is hit
- Objects fall off stand when pushed to edge
- Ball in empty space travels into world depth (not stuck at 2D boundary)
- Ball auto-destroys after lifetime with fade (no instant pop)
- No physics explosions or infinite flying objects

### Save/Load Tests
- Progress persists across app restart
- Gold amount correct after restart
- Lives correct after restart
- Skill uses correct after restart
- Settings persist after restart
- Same level completion does NOT give reward twice
- Same loss does NOT deduct life twice

### Economy Tests
- Starting Gold matches configured value
- Level reward: Base + (Remaining Balls × 2)
- Gold cannot go negative
- Skill uses cannot go negative
- Lives cannot go below 0

### Lifecycle Tests
- App close during active level → -1 life on reopen
- App close NOT during level → no life loss
- Exit active level (confirmed) → -1 life
- Win → no life loss
- Lose → exactly -1 life

## What Constitutes a Bug

- Any locked gameplay rule being violated
- Physics objects flying infinitely
- Duplicate reward/life deduction
- UI text using legacy Text instead of TextMeshPro
- Any 2D physics component (Rigidbody2D, Collider2D) being used
- Hardcoded values that should be configurable
- Objects popping instantly instead of fading
- Drag accidentally firing a ball
- Ball not targeting world coordinate
