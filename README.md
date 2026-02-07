# Enemy Imbue Presets

Enemy Imbue Presets is a framework-style mod for assigning weapon imbues to enemies by faction.

## Core Features
- Root-level `Enable Mod` toggle (not inside a collapsible).
- Unified `Enemy Imbue Presets` preset section:
- `Faction Profile Preset`, `Enemy Type Profile Preset`, `Imbue Experience Preset`, `Chance Experience Preset`, and `Strength Experience Preset`.
- 8 fixed faction collapsibles:
- `Combat Practice`, `Outlaws (T0)`, `Wildfolk (T1)`, `Eraden Kingdom (T2)`, `The Eye (T3)`, `Rakta`, `Special / Rogue`, `Fallback (Any Enemy)`.
- Each faction has 3 imbue slots:
- `Imbue`, `Chance`, and `Strength` per slot.
- `Imbue` supports `None (No Imbue)` to intentionally leave a slot empty.
- Chance values normalize per faction if total slot chance exceeds `100%`.
- `Enemy Type Eligibility` collapsible:
- `Caster Enemies Eligible` and `Non-Caster Enemies Eligible` toggles.
- `Enemy Type Profile Preset` batch-writes these enemy-type toggles; `Lore Friendly` is caster-only.
- In `Lore Friendly`, caster-type enemies mirror their loaded/casting spell id for weapon imbues; if no valid cast spell is detected, slot spell values remain the fallback.
- Preset sync behavior:
- Changing a preset writes values directly into all faction sections (last edited wins afterward).
- Diagnostics/logging grouped under final `Diagnostics` collapsible.
- Spell aliases removed from the menu; use canonical spell ids (`Fire`, `Lightning`, `Gravity`, or valid `SpellCastCharge` ids).

## Build
- `dotnet build EnemyImbuePresets.csproj -c Release`
- `dotnet build EnemyImbuePresets.csproj -c Nomad`

Output folders:
- `bin/Release/PCVR/EnemyImbuePresets/`
- `bin/Release/Nomad/EnemyImbuePresets/`

## Design Artifacts
- `_docs/EnemyImbuePresets_MenuMock.xlsx`
- `_docs/EnemyImbuePresets_PresetMatrix.xlsx`
