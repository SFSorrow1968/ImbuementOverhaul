# Troubleshooting

## No imbues applied
- Confirm `Enable Mod` is on.
- Confirm the relevant faction profile is enabled.
- Confirm slot strengths and normalized slot chances are above `0`.
- Confirm spell ids exist in the current catalog.

## Wrong faction behavior
- Use `Dump Factions` in `Diagnostics`.
- Use `Dump Wave-Faction Map` to verify which faction ids sandbox waves actually spawn.
- Compare those ids to profile mapping in logs.

## Spell applies but looks weak
- Raise slot `Strength`.
- Use `Force Reapply` after changing options.
- Check verbose logs for both spell transfer and energy writes.

## Enemy type eligibility not behaving as expected
- Verify `Caster Enemies Eligible` / `Non-Caster Enemies Eligible` values.
- Use verbose logs and check `enemyType=Caster` or `enemyType=NonCaster` in track lines.
- Remember `Enemy Type Profile Preset` changes can overwrite enemy-type toggles.

## Lore-friendly caster mirror seems wrong
- In lore-friendly profile mode, caster enemies attempt to use their loaded/casting spell id for imbues.
- If that spell id is missing or not a valid `SpellCastCharge`, the system falls back to the selected slot spell.
- Use `Verbose` logging and `Dump State` to confirm the assigned slot spell and current faction values.
