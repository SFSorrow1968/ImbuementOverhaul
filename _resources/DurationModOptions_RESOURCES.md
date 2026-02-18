# DurationModOptions Resources

## Tools & Utilities
- **Mod options source**: `Configuration/DurationModOptions.cs` - Defines duration preset/category wiring and source-of-truth fields.
- **Runtime consumer**: `Core/DurationManager.cs` - Applies context multipliers to active imbues.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Mod option attributes and game integration.
- **Library**: `UnityEngine.dll` - Math/clamp helpers.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Context presets now target Player Held, NPC Held, and World only.
