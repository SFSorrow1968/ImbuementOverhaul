# Usage

## Presets
- Set `Faction Profile Preset`, `Enemy Type Profile Preset`, `Imbue Experience Preset`, `Chance Experience Preset`, and `Strength Experience Preset` in `Enemy Imbue Presets`.
- Preset changes write directly into every faction collapsible.
- Faction-level manual edits remain in effect until a preset is changed again.
- `Faction Profile Preset` writes faction enabled/disabled values.
- `Enemy Type Profile Preset` writes `Enemy Type Eligibility` toggles.
- `Lore Friendly` enemy-type profile sets caster-only enemy-type eligibility.
- `Lore Friendly` also mirrors caster enemies' active/loaded spell id onto imbued weapons by default.

## XLSX Reference
- `_docs/EnemyImbuePresets_MenuMock.xlsx`:
- `Menu Layout` sheet shows full option order and defaults.
- `Preset Impact` sheet shows exactly which options are source-of-truth and which preset family writes them.
- `_docs/EnemyImbuePresets_PresetMatrix.xlsx`:
- `Overview` sheet explains write behavior at a glance.
- `Faction Profile` maps which factions each faction-profile preset enables/disables.
- `Enemy Type Profile` maps caster/non-caster toggle writes per enemy-type preset.
- Enemy-type eligibility writes are shown in `Menu Layout` and `Preset Impact`.
- `Imbue Writes`, `Chance Writes`, and `Strength Writes` map each preset to each collapsible option value.

## Faction Sections
- Each faction has an `Enabled` toggle and 3 slots.
- Each slot has:
- `Imbue` (spell id)
- `Chance` (0..100 slider)
- `Strength` (0..100 slider)
- If slot chances exceed `100%` for a faction, they are automatically normalized.
- `Fallback (Any Enemy)` applies when no specific faction id matches.

## Enemy Type Eligibility
- Configure runtime type gates in `Enemy Type Eligibility`.
- `Caster Enemies Eligible`: allow/disallow detected caster-type enemies.
- `Non-Caster Enemies Eligible`: allow/disallow detected non-caster enemies.
- `Enemy Type Profile Preset` batch-writes these toggles.
- In lore-friendly profile mode, caster spell mirroring falls back to the slot spell if no valid cast spell id is detected.

## Performance
- `Imbue Update Interval`: how often tracked enemy weapons are refreshed.
- `Enemy Rescan Interval`: how often all active enemies are rescanned for tracking.
- Recommended starting point: `0.25s` update and `2.0s` rescan.

## Diagnostics
- `Log Level`: `Off`, `Basic`, `Verbose`.
- `Dump Factions`: logs detected factions and resolved profile mapping.
- `Dump Wave-Faction Map`: logs sandbox wave titles grouped by faction id.
- `Dump State`: logs tracked creature assignment state.
- `Force Reapply`: immediately reapplies imbues on tracked enemies.
