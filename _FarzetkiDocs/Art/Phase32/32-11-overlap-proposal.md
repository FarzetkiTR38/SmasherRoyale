# Phase 32 — existing level overlap correction proposal

Read-only proposal, 2 October 2026. No level assets, builder code, prefab, scene or Unity state was changed while preparing this document.

`32-11-penetration-audit.json` establishes 44 actual BoxCollider overlaps: L2=4, L10=2, L19=2, L22=18, L27=18. All affected blocks have zero authored rotation and a 1 m collider scaled to 0.74 m. The builder's normal `STEP_X = 0.755f` (line 18) gives 0.015 m clearance. Recommendations below use that existing spacing, preserve object order/type/count, rows, Y/Z, rotation, scale, stand placement, required-target flags, ammo, rewards and events. They are minimal corrections on this spacing convention with the stated alignment constraints; exact-touch 0.740 m spacing could move blocks slightly less, but would omit the project's normal clearance.

## Recommended X changes

Source: `Assets/_Project/Scripts/Editor/Citadel30LevelBuilder.cs`. Line numbers refer to the file read on 2 October.

| Level | Source expressions to revise | Nominal largest absolute X change | Changed placements | Intended alignment |
| --- | --- | ---: | ---: | --- |
| 2 | Lines 183–188: tower `-1.5*STEP_X` → `-2*STEP_X`, `-0.5*STEP_X` → `-STEP_X`. Lines 190–195: `0.5*STEP_X` → `STEP_X`, `1.5*STEP_X` → `2*STEP_X`. | 0.3775 m = 0.5×STEP_X | 12 | Each complete tower moves outward by half a cell; keep central bridge and cap, lines 197–200, unchanged. |
| 10 | Line 400 core X `-2.2f` → `-2.2f + 0.5f*STEP_X`; line 409 core X `2.2f` → `2.2f - 0.5f*STEP_X`. | 0.3775 m | 2 | Crown blocks align directly with the two gatehouse columns below; crown mean becomes the stand/tower center ±2.2. |
| 19 | Lines 599/601 eye side offsets `±0.75f*STEP_X` → `±STEP_X`. | 0.18875 m = 0.25×STEP_X | 2 | Central eye/core remains X=0; symmetrical row and spire remain centered. |
| 22 | Lines 651–655, 660–664: `0.7f*STEP_X` → `STEP_X`; lines 656–657, 665–666: `0.35f*STEP_X` → `0.5f*STEP_X`. | 0.33975 m = 1.5×0.3×STEP_X | 24 | Preserve every row center and both bastion centers ±2.3. Four-block rows use offsets ±1.5/±0.5 cells; three-block row ±1/0 cells; spires ±0.5 cells. |
| 27 | Lines 750/752, 760/762: `0.7f*STEP_X` → `STEP_X`; lines 754–755, 764–765: `0.35f*STEP_X` → `0.5f*STEP_X`. | 0.2265 m = 0.3×STEP_X | 20 | Preserve every row center and both tower centers ±2.35. Three-block rows ±1/0 cells; two-block crowns ±0.5 cells. |

These are nominal decimal changes. Existing YAML float serialization has approximately 1–2×10^-7 m rounding differences; compare exact expected positions with an appropriate float tolerance rather than requiring identical decimal deltas.

### L2 bridge tradeoff

The current two-block towers are centered at ±0.755 m. Their inner columns at ±0.3775 m occupy the same rows as the central bridge blocks at X=0, causing four overlaps. Keeping the bridge centered and preserving ordered, internally spaced two-block towers cannot retain those old tower centers: the inner column must be at least one cell away from X=0, forcing the tower centers outward to ±1.1325 m. Moving whole towers by ±0.3775 m preserves their vertical columns and the symmetric watchtower design. Moving only the eight blocks in rows 0/1 would clear the four pairs but would introduce a sideways step in the towers; moving bridge blocks elsewhere would lose the central connection. The upper bridge/cap at X=±0.3775 stays supported in projection by 0.3625 m overlap with the corresponding inner top tower block. Actual settling after release still requires Unity testing.

### L10 lower-displacement alternative

The current top pairs have means ±2.38875 m, although their gatehouses/stands are centered at ±2.2 m. If preserving those current top means is preferred, instead change all four top placements:

- Left armored (line 399): `-2.2f - 0.5f*STEP_X` → `-2.2f - 0.75f*STEP_X` = -2.76625 m; left core (line 400): `-2.2f` → `-2.2f + 0.25f*STEP_X` = -2.01125 m.
- Right armored (line 408): `2.2f + 0.5f*STEP_X` → `2.2f + 0.75f*STEP_X` = 2.76625 m; right core (line 409): `2.2f` → `2.2f - 0.25f*STEP_X` = 2.01125 m.

That alternative minimizes the largest per-object movement to 0.18875 m while preserving the displaced crown centers. The recommended two-core change is easier to read visually: it restores the existing two-column support grid and avoids moving armored blocks. Both options retain all authored rows, object types/count and economy.

## Numeric check against current asset positions

Parsed the five current `LevelData_CitadelNN.asset` files without loading Unity. A temporary in-memory axis-aligned 0.74 m box model, using the recommended changes, predicts **zero residual block pairs** in all five. The stand's existing tabletop collider is 5.7×0.25×1.9 m before stand placement scale. All target XZ projections remain inside their nearest existing tabletop; no stand resize is needed:

| Level | Minimum horizontal tabletop margin after recommendation |
| --- | ---: |
| 2 | 1.825 m |
| 10 | 0.3225 m (unchanged arch connectors determine this margin) |
| 19 | 1.590 m |
| 22 | 0.635 m |
| 27 | 1.0125 m |

Every target's Z margin remains 0.58 m. This check establishes proposed starting-pose separation and projected stand fit only; it does not establish dynamic stability, support at each layer, physics response, or a Unity acceptance pass.

Apply only to the exact five checked assets and exact expected X values, after snapshotting their current bytes. Update the corresponding builder expressions so future generation preserves the repair; **do not invoke GenerateAllLevels**, which would rewrite unrelated authored/Phase 33 data. If any expected placement, type, count, Y/Z, rotation or scale has changed since this proposal, stop that asset's correction and re-review instead of replacing the user's layout. After a targeted repair, rerun the actual collider audit and held/miss/contact checks, then record genuine results in the Phase 32 plan.
