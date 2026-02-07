# Development Notes

## Runtime Design
- Event hooks: `EventManager.onCreatureSpawn`, `EventManager.onCreatureDespawn`, `EventManager.onLevelUnload`.
- Creature filtering: non-player only, faction profile lookup by faction id.
- Item source: left/right hand grabbed handles (`RagdollHand.grabbedHandle?.item`).
- Imbue application: `Imbue.Transfer(...)` + `Imbue.SetEnergyInstant(...)`.

## ModOption Design
- Global preset values are first-class options.
- `Faction Profile Preset` batch-writes faction `Enabled` toggles.
- `Enemy Type Profile Preset` batch-writes `Enemy Type Eligibility` toggles (`Lore Friendly` writes caster-only).
- Faction sections mirror preset fields.
- A sync manager detects preset changes and writes values to faction options.
- Runtime uses faction collapsible values as the source of truth.
- Presets are batch-write helpers only; they do not bypass or override faction collapsible values at runtime.
- Slot spells support `None`, which means that slot cannot roll an imbue.

## Decompile References
- Primary local reference source: `References/`.
- If deeper verification is needed, use local decompile tooling from `.tools/` against `../libs/*.dll`.
