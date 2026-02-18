# DurationManager Resources

## Tools & Utilities
- **Runtime loop**: `Core/DurationManager.cs` - Tracks imbues and applies correction deltas.
- **Faction slot lookup**: `Core/FactionImbuementManager.cs` - Exposes tracked slot index per creature.
- **Telemetry/logging**: `Core/DurationTelemetry.cs`, `Core/DurationLog.cs`.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Item/Imbue/Creature APIs.
- **Library**: `UnityEngine.dll` - `Mathf` and frame-time utilities.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: NPC-held drain can now be further scaled by per-faction, per-slot drain multipliers.
