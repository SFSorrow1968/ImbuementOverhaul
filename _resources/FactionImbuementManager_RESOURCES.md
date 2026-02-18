# FactionImbuementManager Resources

## Tools & Utilities
- **Runtime manager**: `Core/FactionImbuementManager.cs` - Enemy imbue assignment and tracked slot state.
- **Drain consumer**: `Core/DurationManager.cs` - Reads tracked slot/faction info to apply faction-slot drain multipliers.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Creature/item runtime APIs.
- **Library**: `UnityEngine.dll` - Time/math helpers.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Public slot lookup API enables cross-system runtime scaling without exposing internal tracking dictionary.
