# DurationModOptionsTests Quirks
**Known Issues**:
- [Scope]: Tests validate normalization/default behavior and hash/preset signal paths, not live ThunderRoad runtime behavior.

**Edge Cases**:
- [Reference warnings]: Local test runs can emit missing Unity/ThunderRoad reference warnings but still execute assertions.
- [Preset asserts]: Preset tests now validate 5-step drain model (`Player Dominant` -> `Enemy Dominant`) and balanced equality behavior.
