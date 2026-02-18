# DurationModOptionSync Resources

## Tools & Utilities
- **Sync implementation**: `Core/DurationModOptionSync.cs` - Hash-based preset apply and UI refresh.
- **Option definitions**: `Configuration/DurationModOptions.cs` - Category/name constants consumed by sync.

## Libraries & Dependencies
- **Library**: `ThunderRoad.dll` - `ModOption`, `ModManager` APIs.

## Documentation
- **Spec**: `_docs/DESIGN.md`
- **Relevant Info**: Sync keys are built from `category + || + optionName`; drain sliders now resolve under `Drain Multiplier` category.
