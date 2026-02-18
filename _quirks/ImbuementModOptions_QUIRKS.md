# ImbuementModOptions Quirks
**Known Issues**:
- [Fallback removal]: F08 fallback faction category and option fields were removed; no fallback profile is auto-selected.

**Edge Cases**:
- [Unknown factions]: `TryResolveFactionProfile` now returns `false` when no exact faction id match exists.
- [Resolved ids]: Catalog resolution now resolves all configured factions directly without reserving a synthetic fallback id.
