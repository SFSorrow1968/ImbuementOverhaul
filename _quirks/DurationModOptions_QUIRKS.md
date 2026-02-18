# DurationModOptions Quirks
**Known Issues**:
- [Shared category]: Duration preset controls now live in `Imbuement Overhaul` category; option sync must resolve by category+name to avoid collisions.
- [Legacy preset tokens]: Saved context preset values containing `THROWN` now normalize to `Uniform` because thrown-only profiles were removed.

**Edge Cases**:
- [Thrown items]: Runtime thrown contexts are intentionally folded into held contexts (`PlayerHeld` or `NpcHeld`).
- [Preset writes]: `ApplySelectedPresets` writes only `global`, `playerHeld`, `npcHeld`, and `world` source-of-truth fields.
