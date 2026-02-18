# DurationModOptions Quirks
**Known Issues**:
- [Preset migration]: Legacy saved values from old duration/context presets now normalize into the new 5-step `Drain Multiplier Preset` model.
- [No global multiplier]: Runtime scaling now uses only context sliders (`Player Held`, `NPC Held`, `World`) and no longer has a separate global drain multiplier.

**Edge Cases**:
- [Balanced default]: Preset index 3 (`Balanced`) sets all three context multipliers to `1.00`.
- [Native infinite]: `Use Native Infinite Flag` now activates only when all three context multipliers are `0.00`.
