# Imbuement Overhaul Scratchpad

## Current Status (2026-02-18)
- Removed fallback faction collapsible from `ImbuementModOptions` (F08 category/options removed).
- Removed thrown duration contexts/options from `DurationModOptions` and runtime context resolution.
- Consolidated duration source-of-truth controls into one presets category (`Imbuement Overhaul`) so duration no longer appears as separate held/thrown/world collapsibles.
- Duration presets now target `Global`, `Player Held`, `NPC Held`, and `World` only.
- Build validation succeeded:
  - `dotnet build -c Release` (0 errors, 0 warnings)
  - `dotnet build -c Nomad` (0 errors, 0 warnings)
- Test validation succeeded:
  - `dotnet test ImbuementOverhaul.Tests/ImbuementOverhaul.Tests.csproj` passed (11/11)
  - Note: test project emits missing Unity/ThunderRoad reference warnings in local CLI runs.

## Next Steps
1. In-game sanity pass:
   - Confirm mod options list no longer shows `Player Thrown`, `NPC Thrown`, or `Fallback (Any Enemy)`.
   - Confirm duration controls are grouped under one presets category and write correctly.
2. Validate unknown/uncatalogued faction behavior:
   - Ensure expected skip behavior when no explicit faction profile exists.
3. If UI ordering needs tuning, adjust `order` values in `Configuration/DurationModOptions.cs`.
4. Create snapshot commit after in-game verification.
