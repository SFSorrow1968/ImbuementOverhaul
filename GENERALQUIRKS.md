# General Quirks

Document cross-cutting quirks that do not fit a specific theme.

## Format

Each entry should include:
- **Issue**: Brief description
- **Context**: When/why this matters
- **Solution/Workaround**: How to handle it

---

## Entry 1

- **Issue**: Preset dropdown selections can use UI labels while runtime logic expects compact preset IDs.
- **Context**: If parsing only checks IDs (for example `BalancedBattles`) and ignores labels (for example `Balanced Battles`), preset application silently falls back and faction sections do not batch update as expected.
- **Solution/Workaround**: Normalize preset values to canonical IDs before hashing/applying, and keep UI sync tolerant to label/value formatting differences.

## Entry 2

- **Issue**: Collapsible category options may not all be available in the initial mod-option cache.
- **Context**: If the cache is only built once, preset changes can miss faction options that appear later, leaving section UI out of sync.
- **Solution/Workaround**: Refresh the option cache during updates and retry lookups when keys are missing, then sync all faction options when new entries appear.

## Entry 3

- **Issue**: Preset intent is split across profile, imbue, chance, and strength families.
- **Context**: `Faction Profile Preset` controls faction `Enabled` toggles, while `Enemy Type Profile Preset` controls caster/non-caster eligibility; neither directly changes slot spell/chance/strength values.
- **Solution/Workaround**: Treat faction profile + enemy-type profile as eligibility-only layers, and use the three slot preset families to define what eligible factions/types can roll.
