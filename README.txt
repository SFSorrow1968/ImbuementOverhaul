# Imbuement Overhaul

Imbuement Overhaul combines Imbuement Overhaul presets with runtime imbuement duration management in one mod.

## Core Features
- Faction-based imbuement assignment with preset-driven and per-faction slot overrides.
- Enemy type eligibility controls (`Mage`, `Bow`, `Melee`) plus uncertain-type fallback behavior.
- Runtime caster spell mirroring support in caster-focused profiles.
- Duration scaling presets from `Way Less` to `Infinite`.
- Per-context duration controls:
- `Player Held`
- `Player Thrown`
- `NPC Held`
- `NPC Thrown`
- `World / Dropped`
- Presets remain batch writers while collapsible values stay runtime source of truth.

## Build
- `dotnet build ImbuementOverhaul.csproj -c Release`
- `dotnet build ImbuementOverhaul.csproj -c Nomad`

Output folders:
- `bin/PCVR/ImbuementOverhaul/`
- `bin/Nomad/ImbuementOverhaul/`

