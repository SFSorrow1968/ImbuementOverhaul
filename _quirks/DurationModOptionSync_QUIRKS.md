# DurationModOptionSync Quirks
**Known Issues**:
- [UI sync dependency]: Sync expects all source-of-truth drain options to be in `DurationModOptions.CategoryPresets`.
- [Preset loop]: Hash-based sync only refreshes UI when preset selection hash changes.

**Edge Cases**:
- [Shared category]: Multiple options in the same category rely on exact option names for key matching.
- [Missing option]: If an option is absent from mod options cache, sync safely no-ops for that field.
