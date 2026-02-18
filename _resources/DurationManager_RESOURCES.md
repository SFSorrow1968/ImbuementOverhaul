# DurationManager Resources

## Tools & Utilities
- **Runtime loop**: `Core/DurationManager.cs` - Tracks imbues and applies correction deltas.
- **Telemetry/logging**: `Core/DurationTelemetry.cs`, `Core/DurationLog.cs`.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - Item/Imbue/Creature APIs.
- **Library**: `UnityEngine.dll` - `Mathf` and frame-time utilities.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Context resolution now uses PlayerHeld/NpcHeld/WorldDropped only.
