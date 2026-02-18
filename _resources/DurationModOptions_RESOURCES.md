# DurationModOptions Resources

## Tools & Utilities
- **Mod options source**: `Configuration/DurationModOptions.cs` - Defines drain preset dropdown, drain sliders, and source-of-truth hash helpers.
- **Runtime consumer**: `Core/DurationManager.cs` - Applies context multipliers each update cycle.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Mod option attributes and game integration.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Preset dropdown now writes the three context multipliers directly (`playerHeld`, `npcHeld`, `world`).
