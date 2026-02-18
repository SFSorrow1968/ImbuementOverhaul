# Imbues Quirks

## Entry 1

- **Issue**: Imbue behavior drifts after game/library updates when assumptions come from stale decompilation.
- **Context**: This shows up during debugging when logic appears correct but runtime behavior disagrees.
- **Solution/Workaround**: Re-check decompiled API assumptions against the current libraries in `../libs/` before changing logic.

## Entry 2

- **Issue**: An imbue can appear to apply while VFX intensity is still partial.
- **Context**: This can look like a rendering issue, but it can also come from an incomplete spell or energy path.
- **Solution/Workaround**: Verify both spell load and energy write paths in logs before concluding the effect is fully applied.

## Entry 3

- **Issue**: Slot spell values can now be intentionally empty (`None`), and empty slots should never roll.
- **Context**: If a slot uses `None` but chance/strength are non-zero, it still must be treated as unavailable.
- **Solution/Workaround**: Canonicalize `None`/blank spell ids to empty and ensure roll logic skips any slot whose spell id is empty.

## Entry 4

- **Issue**: Preset matrix docs drift if C# constants change without updating the XLSX generator dictionaries.
- **Context**: The primary configuration options class drives runtime behavior, while `_agent/generate-design-xlsx.py` drives readable design sheets.
- **Solution/Workaround**: When changing any profile/imbue/chance/strength matrix in C#, mirror the exact values in Python and regenerate both `_docs/*.xlsx` files in the same change.

## Entry 5

- **Issue**: Runtime caster detection is heuristic and can under-detect before AI/components fully initialize.
- **Context**: Enemy-type eligibility evaluates identity strings, held caster-focus items, and spell/mana component signals at runtime.
- **Solution/Workaround**: Cache positive caster detections, periodically retry negatives, and rely on per-faction slot chances/strength as the source of truth after type gating.

## Entry 6

- **Issue**: Lore-friendly caster spell mirroring can fail if the detected spell is non-charge or temporarily unavailable.
- **Context**: Runtime spell detection reads active/loaded spell ids from left/right caster state and prioritizes currently firing hands.
- **Solution/Workaround**: Keep a short-lived per-creature spell cache, prefer active-cast spell ids, and fallback to slot spell ids when no valid `SpellCastCharge` can be resolved.

## Entry 7

- **Issue**: Filtering on `creature.isPlayer` alone is not always enough to exclude the possessed player creature.
- **Context**: During long sessions, player tracking can reappear under fallback faction logic and receive periodic rerolls.
- **Solution/Workaround**: Treat player exclusion as a composite check (`isPlayer`, `Player.currentCreature` reference, faction id `2`, and `data.id` containing `player`) before any tracking/roll/apply logic.

## Entry 8

- **Issue**: Using a broad options hash for runtime refresh can cause unnecessary assignment rerolls.
- **Context**: Diagnostics/UI option changes and preset-sync normalization can trigger "configuration changed" refreshes even when assignment inputs are unchanged.
- **Solution/Workaround**: Use a dedicated assignment-state hash (enemy-type eligibility + per-faction enabled/spell/chance/strength values) for runtime refresh decisions.

## Entry 9

- **Issue**: One-off per-creature logs are hard to compare across long playthroughs when tuning presets.
- **Context**: Manual inspection misses aggregate patterns (for example, why most enemies are skipped or why transfer failures spike).
- **Solution/Workaround**: Use periodic `diag evt=summary` logs with `Diagnostics Logs` enabled; add `Verbose Logs` only when deeper traces are required.

## Entry 10

- **Issue**: Enemy-type profile behavior can become confusing if runtime classification and UI toggles use different mental models.
- **Context**: The current model uses three archetypes (`Mage`, `Bow`, `Melee`) and presets batch-write those toggles (`Casters`, `Ranged`, `All`); lore/default caster behavior depends on caster archetypes, not a legacy boolean.
- **Solution/Workaround**: Keep classification, preset matrices, sync logging, and docs/XLSX generator aligned to the same five-archetype vocabulary whenever enemy-type logic changes.

## Entry 11

- **Issue**: Runtime enemy archetypes can oscillate during weapon swaps or spawn initialization, causing noisy rerolls.
- **Context**: Archer/melee hints can appear a frame or two before/after caster signals, especially during hand-item transitions.
- **Solution/Workaround**: Keep a short archetype stabilization cache window, expose uncertain fallback mode (`Treat As Melee` vs `Skip Enemy`), and use targeted verbose session logs before changing profile defaults.
