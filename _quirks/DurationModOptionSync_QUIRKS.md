# DurationModOptionSync Quirks
**Known Issues**:
- [Category binding]: Source-of-truth slider sync now depends on `DurationModOptions.CategoryDrainMultipliers`.
- [Preset loop]: Hash-based sync only refreshes UI when the drain preset token changes.

**Edge Cases**:
- [Missing option]: If an option is absent from mod options cache, sync safely no-ops for that field.
- [Preset write]: Sync applies drain preset first, then pushes slider values to UI.
