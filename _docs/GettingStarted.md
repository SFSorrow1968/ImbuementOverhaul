# Getting Started

1. Build the mod:
   - `dotnet build EnemyImbuePresets.csproj -c Release`
   - `dotnet build EnemyImbuePresets.csproj -c Nomad`
2. Install the output folder contents into your Blade & Sorcery mod directory.
3. Open Mod Options and keep `Enable Mod` on.
4. Pick your five presets in `Factioned Imbuement`.
5. Adjust faction slot values and enemy-type eligibility toggles as needed.
6. Set `Uncertain Enemy Type Fallback` to `Treat As Melee` (recommended default) unless you want strict skipping.
7. For low-noise validation, enable only `Session Diagnostics`.
8. For deeper troubleshooting, enable `Diagnostics Logs`; add `Verbose Logs` only when needed.
