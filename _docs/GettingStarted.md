# Getting Started

1. Build the mod:
   - `dotnet build ImbuementOverhaul.csproj -c Release`
   - `dotnet build ImbuementOverhaul.csproj -c Nomad`
2. Install the output folder contents into your Blade & Sorcery mod directory.
3. Open Mod Options and keep `Enable Mod` on.
4. Pick your five presets in `Imbuement Overhaul`.
5. Adjust faction slot values and enemy-type eligibility toggles as needed.
6. Set `Uncertain Enemy Type Fallback` to `Treat As Melee` (recommended default) unless you want strict skipping.
7. For low-noise validation, set `Basic Logs=On`, `Diagnostics Logs=Off`, `Verbose Logs=Off`.
8. For deeper troubleshooting, enable `Diagnostics Logs`; add `Verbose Logs` only when needed.

