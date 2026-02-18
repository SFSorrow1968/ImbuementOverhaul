using System;
using ThunderRoad;

namespace ImbuementOverhaul.Configuration
{
    public static class DurationModOptions
    {
        public const string VERSION = "0.1.1";

        public const string CategoryPresets = "Imbuement Overhaul";
        public const string CategoryDrainMultipliers = "Drain Multiplier";
        public const string CategoryDiagnostics = "Duration Advanced";

        public const string OptionEnableMod = "Enable Duration Scaling";
        public const string OptionPresetDrainProfile = "Drain Multiplier Preset";
        public const string OptionPlayerHeldDrainMultiplier = "Player Held Drain Multiplier";
        public const string OptionNpcHeldDrainMultiplier = "NPC Held Drain Multiplier";
        public const string OptionWorldDrainMultiplier = "World / Dropped Drain Multiplier";

        public const string OptionUpdateInterval = "Update Interval";
        public const string OptionMaxCorrectionPerTick = "Max Correction Per Tick";
        public const string OptionMinimumEnergyFloor = "Minimum Energy Floor";
        public const string OptionUseNativeInfinite = "Use Native Infinite Flag";

        public const string OptionEnableBasicLogging = "Duration Basic Logs";
        public const string OptionEnableDiagnosticsLogging = "Duration Diagnostics Logs";
        public const string OptionEnableVerboseLogging = "Duration Verbose Logs";
        public const string OptionResetTracking = "Reset Tracking";

        public const string PresetDrainPlayerDominant = "PlayerDominant";
        public const string PresetDrainPlayerFavored = "PlayerFavored";
        public const string PresetDrainBalanced = "Balanced";
        public const string PresetDrainEnemyFavored = "EnemyFavored";
        public const string PresetDrainEnemyDominant = "EnemyDominant";

        public enum DrainContext
        {
            PlayerHeld = 0,
            NpcHeld = 1,
            WorldDropped = 2,
        }

        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = "Master switch for imbue duration scaling")]
        public static bool EnableMod = true;

        [ModOption(name = OptionPresetDrainProfile, category = CategoryPresets, categoryOrder = 0, order = 10, defaultValueIndex = 2, valueSourceName = nameof(DrainPresetProvider), tooltip = "Batch writes player/NPC/world drain multipliers")]
        public static string PresetDrainProfile = PresetDrainBalanced;

        [ModOption(name = OptionUpdateInterval, category = CategoryDiagnostics, categoryOrder = 200, order = 30, defaultValueIndex = 1, valueSourceName = nameof(UpdateIntervalProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How often runtime scaling updates active imbues")]
        public static float UpdateIntervalSeconds = 0.10f;

        [ModOption(name = OptionMaxCorrectionPerTick, category = CategoryDiagnostics, categoryOrder = 200, order = 40, defaultValueIndex = 3, valueSourceName = nameof(MaxCorrectionProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Caps max energy correction each tick as % of max energy")]
        public static float MaxCorrectionPercentPerTick = 15f;

        [ModOption(name = OptionMinimumEnergyFloor, category = CategoryDiagnostics, categoryOrder = 200, order = 50, defaultValueIndex = 1, valueSourceName = nameof(MinimumEnergyFloorProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Optional floor used when slowing drain to avoid accidental unload bursts")]
        public static float MinimumEnergyFloorPercent = 1f;

        [ModOption(name = OptionUseNativeInfinite, category = CategoryDiagnostics, categoryOrder = 200, order = 60, defaultValueIndex = 1, tooltip = "When all context drains are Infinite, also toggle native Imbue.infiniteImbue")]
        public static bool UseNativeInfiniteFlag = true;

        [ModOption(name = OptionPlayerHeldDrainMultiplier, category = CategoryDrainMultipliers, categoryOrder = 20, order = 10, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for held player weapons")]
        public static float PlayerHeldDrainMultiplier = 1f;

        [ModOption(name = OptionNpcHeldDrainMultiplier, category = CategoryDrainMultipliers, categoryOrder = 20, order = 20, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for held NPC weapons")]
        public static float NpcHeldDrainMultiplier = 1f;

        [ModOption(name = OptionWorldDrainMultiplier, category = CategoryDrainMultipliers, categoryOrder = 20, order = 30, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for dropped/world weapons")]
        public static float WorldDrainMultiplier = 1f;

        [ModOption(name = OptionEnableBasicLogging, category = CategoryDiagnostics, categoryOrder = 200, order = 0, defaultValueIndex = 1, tooltip = "Enable general informational logs")]
        public static bool EnableBasicLogging = true;

        [ModOption(name = OptionEnableDiagnosticsLogging, category = CategoryDiagnostics, categoryOrder = 200, order = 2, defaultValueIndex = 0, tooltip = "Enable deeper troubleshooting logs")]
        public static bool EnableDiagnosticsLogging = false;

        [ModOption(name = OptionEnableVerboseLogging, category = CategoryDiagnostics, categoryOrder = 200, order = 4, defaultValueIndex = 0, tooltip = "Enable high-volume per-item and per-imbue logs")]
        public static bool EnableVerboseLogging = false;

        [ModOption(name = OptionResetTracking, category = CategoryDiagnostics, categoryOrder = 200, order = 20, defaultValueIndex = 0, tooltip = "Clear tracked imbue baseline data")]
        [ModOptionDontSave]
        public static bool ResetTracking;

        public static ModOptionString[] DrainPresetProvider()
        {
            return new[]
            {
                new ModOptionString("Player Dominant", PresetDrainPlayerDominant),
                new ModOptionString("Player Favored", PresetDrainPlayerFavored),
                new ModOptionString("Balanced", PresetDrainBalanced),
                new ModOptionString("Enemy Favored", PresetDrainEnemyFavored),
                new ModOptionString("Enemy Dominant", PresetDrainEnemyDominant),
            };
        }

        public static ModOptionFloat[] DrainMultiplierProvider()
        {
            return new[]
            {
                new ModOptionFloat("Infinite", 0.00f),
                new ModOptionFloat("0.10x", 0.10f),
                new ModOptionFloat("0.25x", 0.25f),
                new ModOptionFloat("0.50x", 0.50f),
                new ModOptionFloat("0.65x", 0.65f),
                new ModOptionFloat("0.75x", 0.75f),
                new ModOptionFloat("0.85x", 0.85f),
                new ModOptionFloat("1.00x", 1.00f),
                new ModOptionFloat("1.15x", 1.15f),
                new ModOptionFloat("1.35x", 1.35f),
                new ModOptionFloat("1.60x", 1.60f),
                new ModOptionFloat("2.00x", 2.00f),
                new ModOptionFloat("2.50x", 2.50f),
                new ModOptionFloat("3.00x", 3.00f),
            };
        }

        public static ModOptionFloat[] UpdateIntervalProvider()
        {
            return new[]
            {
                new ModOptionFloat("0.05s", 0.05f),
                new ModOptionFloat("0.10s", 0.10f),
                new ModOptionFloat("0.15s", 0.15f),
                new ModOptionFloat("0.20s", 0.20f),
                new ModOptionFloat("0.33s", 0.33f),
                new ModOptionFloat("0.50s", 0.50f),
            };
        }

        public static ModOptionFloat[] MaxCorrectionProvider()
        {
            return new[]
            {
                new ModOptionFloat("2%", 2f),
                new ModOptionFloat("5%", 5f),
                new ModOptionFloat("10%", 10f),
                new ModOptionFloat("15%", 15f),
                new ModOptionFloat("25%", 25f),
                new ModOptionFloat("40%", 40f),
                new ModOptionFloat("60%", 60f),
                new ModOptionFloat("100%", 100f),
            };
        }

        public static ModOptionFloat[] MinimumEnergyFloorProvider()
        {
            return new[]
            {
                new ModOptionFloat("0%", 0f),
                new ModOptionFloat("1%", 1f),
                new ModOptionFloat("2%", 2f),
                new ModOptionFloat("5%", 5f),
                new ModOptionFloat("10%", 10f),
                new ModOptionFloat("15%", 15f),
            };
        }

        public static bool ApplySelectedPresets()
        {
            string drainPreset = NormalizeDrainPreset(PresetDrainProfile);
            ResolveDrainPresetMultipliers(drainPreset, out float playerHeld, out float npcHeld, out float world);

            bool changed = false;
            changed |= SetFloat(ref PlayerHeldDrainMultiplier, playerHeld);
            changed |= SetFloat(ref NpcHeldDrainMultiplier, npcHeld);
            changed |= SetFloat(ref WorldDrainMultiplier, world);
            return changed;
        }

        public static int GetPresetSelectionHash()
        {
            int hash = 17;
            hash = CombineHash(hash, StringHash(NormalizeDrainPreset(PresetDrainProfile)));
            return hash;
        }

        public static int GetSourceOfTruthHash()
        {
            int hash = 17;
            hash = CombineHash(hash, PercentHash(PlayerHeldDrainMultiplier));
            hash = CombineHash(hash, PercentHash(NpcHeldDrainMultiplier));
            hash = CombineHash(hash, PercentHash(WorldDrainMultiplier));
            hash = CombineHash(hash, PercentHash(UpdateIntervalSeconds));
            hash = CombineHash(hash, PercentHash(MaxCorrectionPercentPerTick));
            hash = CombineHash(hash, PercentHash(MinimumEnergyFloorPercent));
            hash = CombineHash(hash, UseNativeInfiniteFlag ? 1 : 0);
            return hash;
        }

        public static float GetEffectiveDrainMultiplier(DrainContext context)
        {
            float contextMultiplier;
            switch (context)
            {
                case DrainContext.PlayerHeld:
                    contextMultiplier = PlayerHeldDrainMultiplier;
                    break;
                case DrainContext.NpcHeld:
                    contextMultiplier = NpcHeldDrainMultiplier;
                    break;
                default:
                    contextMultiplier = WorldDrainMultiplier;
                    break;
            }

            float multiplier = Math.Max(0f, contextMultiplier);
            return Clamp(multiplier, 0f, 6f);
        }

        public static bool ShouldUseNativeInfinite()
        {
            return EnableMod &&
                   UseNativeInfiniteFlag &&
                   PlayerHeldDrainMultiplier <= 0.0001f &&
                   NpcHeldDrainMultiplier <= 0.0001f &&
                   WorldDrainMultiplier <= 0.0001f;
        }

        public static float GetUpdateIntervalSeconds()
        {
            return Clamp(UpdateIntervalSeconds, 0.03f, 1.0f);
        }

        public static float GetMaxCorrectionRatio()
        {
            return Clamp(MaxCorrectionPercentPerTick / 100f, 0.01f, 1f);
        }

        public static float GetMinimumEnergyFloorRatio()
        {
            return Clamp01(MinimumEnergyFloorPercent / 100f);
        }

        public static string NormalizeDrainPreset(string preset)
        {
            string token = NormalizeToken(preset);

            if (token.Contains("PLAYERDOMINANT")) return PresetDrainPlayerDominant;
            if (token.Contains("PLAYERFAVORED")) return PresetDrainPlayerFavored;
            if (token.Contains("ENEMYDOMINANT") || token.Contains("NPCDOMINANT")) return PresetDrainEnemyDominant;
            if (token.Contains("ENEMYFAVORED") || token.Contains("NPCFAVORED") || token.Contains("WORLDDECAY")) return PresetDrainEnemyFavored;
            if (token.Contains("PLAYER")) return PresetDrainPlayerFavored;
            if (token.Contains("NPC")) return PresetDrainEnemyFavored;
            if (token.Contains("BALANCED") || token.Contains("DEFAULT") || token.Contains("BASE") || token.Contains("UNIFORM")) return PresetDrainBalanced;

            return PresetDrainBalanced;
        }

        public static string GetSourceOfTruthSummary()
        {
            return "playerHeld=" + PlayerHeldDrainMultiplier.ToString("0.00") +
                   " npcHeld=" + NpcHeldDrainMultiplier.ToString("0.00") +
                   " world=" + WorldDrainMultiplier.ToString("0.00") +
                   " update=" + GetUpdateIntervalSeconds().ToString("0.00") + "s" +
                   " maxCorrection=" + MaxCorrectionPercentPerTick.ToString("0") + "%" +
                   " floor=" + MinimumEnergyFloorPercent.ToString("0") + "%";
        }

        private static void ResolveDrainPresetMultipliers(string preset, out float playerHeld, out float npcHeld, out float world)
        {
            switch (preset)
            {
                case PresetDrainPlayerDominant:
                    playerHeld = 0.65f;
                    npcHeld = 1.35f;
                    world = 0.65f;
                    break;
                case PresetDrainPlayerFavored:
                    playerHeld = 0.85f;
                    npcHeld = 1.15f;
                    world = 0.85f;
                    break;
                case PresetDrainEnemyFavored:
                    playerHeld = 1.15f;
                    npcHeld = 0.85f;
                    world = 1.15f;
                    break;
                case PresetDrainEnemyDominant:
                    playerHeld = 1.60f;
                    npcHeld = 0.50f;
                    world = 1.60f;
                    break;
                default:
                    playerHeld = 1.00f;
                    npcHeld = 1.00f;
                    world = 1.00f;
                    break;
            }

            playerHeld = Clamp(playerHeld, 0f, 6f);
            npcHeld = Clamp(npcHeld, 0f, 6f);
            world = Clamp(world, 0f, 6f);
        }

        private static bool SetFloat(ref float field, float value)
        {
            float clamped = Clamp(value, 0f, 6f);
            if (Math.Abs(field - clamped) < 0.0001f)
            {
                return false;
            }

            field = clamped;
            return true;
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            char[] buffer = new char[trimmed.Length];
            int length = 0;

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (char.IsLetterOrDigit(c))
                {
                    buffer[length++] = char.ToUpperInvariant(c);
                }
            }

            return new string(buffer, 0, length);
        }

        private static int CombineHash(int seed, int value)
        {
            unchecked
            {
                return (seed * 397) ^ value;
            }
        }

        private static int StringHash(string value)
        {
            return string.IsNullOrEmpty(value) ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);
        }

        private static int PercentHash(float value)
        {
            return (int)Math.Round(value * 1000f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }
    }
}

