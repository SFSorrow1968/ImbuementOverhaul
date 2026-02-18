# FactionImbuementManager Quirks
**Known Issues**:
- [Tracking dependency]: Slot lookups are only available for creatures currently tracked by the manager.

**Edge Cases**:
- [Slot lookup API]: `TryGetTrackedSlotForCreature` returns `false` when a creature has no passed roll or no selected slot yet.
- [Lifecycle]: Tracked slot data is cleared on despawn/level unload just like other tracked assignment state.
