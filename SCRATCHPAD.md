# Imbuement Overhaul Scratchpad

## Current Status (2026-02-18)
- Duration menu refactor completed:
  - Removed old `Duration Experience Preset`, `Context Profile Preset`, and `Global Drain Multiplier` controls.
  - Added single 5-step `Drain Multiplier Preset` dropdown in `Imbuement Overhaul` category.
  - Moved `Player Held`, `NPC Held`, and `World / Dropped` drain sliders into a new `Drain Multiplier` collapsible.
- New drain preset behavior:
  - Preset #3 (`Balanced`) sets all three context multipliers to `1.00`.
  - Right-side presets increase player/world drain while decreasing NPC drain; left-side presets do the inverse.
- Faction collapsible refactor completed:
  - Added `Faction Weight` toggle (default ON) under presets.
  - Added 3 per-slot `[Faction] Drain Multiplier` sliders to every faction collapsible.
  - `Faction Weight` now batch-writes tiered defaults to all faction-slot drain sliders.
- Runtime integration completed:
  - `DurationManager` now multiplies NPC-held drain by per-faction/per-slot multipliers using tracked slot info from `FactionImbuementManager`.
  - Added `TryGetTrackedSlotForCreature` API in `FactionImbuementManager`.
- Version bump completed:
  - `manifest.json` `ModVersion` -> `0.2.1`
  - `Configuration/ImbuementModOptions.cs` `VERSION` -> `0.4.1`
  - `Configuration/DurationModOptions.cs` `VERSION` -> `0.1.1`
- Validation succeeded:
  - `dotnet build -c Release` (0 errors, 0 warnings)
  - `dotnet build -c Nomad` (0 errors, 0 warnings)
  - `dotnet test ImbuementOverhaul.Tests/ImbuementOverhaul.Tests.csproj` passed (12/12; expected missing local Unity/ThunderRoad reference warnings)
- Publish script note:
  - `_agent/publish.ps1` currently exits early with `Working tree is dirty` because unrelated local changes already exist.

## Next Steps
1. In-game UI sanity pass:
   - Confirm new `Drain Multiplier` collapsible placement/order.
   - Confirm `Drain Multiplier Preset` batch-writes all three context sliders.
2. In-game behavior pass:
   - Verify rightmost presets produce slower enemy imbue drain and faster player/world drain.
   - Verify leftmost presets produce the inverse.
3. Faction drain pass:
   - Toggle `Faction Weight` ON and verify per-faction slot sliders batch-write tiered values.
   - Confirm NPC-held drain scaling reflects faction slot multipliers at runtime.
4. When clean-tree policy allows, run `_agent/publish.ps1` and then create snapshot commit/tag branches per `_docs/GIT_WORKFLOW.md`.
