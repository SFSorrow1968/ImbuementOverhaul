# ImbuementModOptions Resources

## Tools & Utilities
- **Options source**: `Configuration/ImbuementModOptions.cs` - Faction presets, profile resolution, and batch-write logic.
- **Consumer**: `Core/FactionImbuementManager.cs` - Calls `TryResolveFactionProfile` and applies outcomes.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Mod option attributes and game data catalog access.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Faction fallback profile was removed; unknown faction ids now skip profile resolution.
