# DurationManager Quirks
**Known Issues**:
- [Context simplification]: Thrown-state detection is no longer used for context selection.

**Edge Cases**:
- [Owner resolution]: Held context selection uses `mainHandler` then `lastHandler`; no creature owner falls back to `WorldDropped`.
- [Scaling]: Effective multiplier is now `ContextMultiplier * FactionSlotDrainMultiplier` for NPC-held imbues (capped to `0..6`).
