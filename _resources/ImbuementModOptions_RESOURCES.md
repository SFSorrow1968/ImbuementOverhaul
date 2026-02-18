# ImbuementModOptions Resources

## Tools & Utilities
- **Options source**: `Configuration/ImbuementModOptions.cs` - Faction presets, profile resolution, batch-write logic, and per-slot faction drain multipliers.
- **Consumers**: `Core/FactionImbuementManager.cs`, `Core/DurationManager.cs`.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Mod option attributes and game data catalog access.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Unknown faction ids skip profile resolution; faction collapsibles now include 3 drain multiplier sliders per faction.
