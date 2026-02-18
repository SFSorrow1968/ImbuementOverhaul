# ImbuementModOptionSync Quirks
**Known Issues**:
- [UI sync dependency]: Sync keys depend on exact category + option names for each faction collapsible.

**Edge Cases**:
- [Preset writes]: Preset-batch sync now includes per-slot faction drain multipliers in addition to spell/chance/strength fields.
- [Lazy categories]: Collapsible option lookup refreshes cache once if a key is initially missing.
