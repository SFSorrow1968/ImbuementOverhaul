using System;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Configuration
{
    public static class DurationModOptions
    {
        public const string VERSION = "0.1.0";

        public const string CategoryPresets = "Imbuement Overhaul";
        public const string CategoryDiagnostics = "Duration Advanced";

        public const string OptionEnableMod = "Enable Duration Scaling";
        public const string OptionPresetDuration = "Duration Experience Preset";
        public const string OptionPresetContext = "Context Profile Preset";

        public const string OptionGlobalDrainMultiplier = "Global Drain Multiplier";
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

        public const string PresetWayLess = "WayLess";
        public const string PresetLess = "Less";
        public const string PresetSlightlyLess = "SlightlyLess";
        public const string PresetBaseGame = "BaseGame";
        public const string PresetDefaultPlus = "DefaultPlus";
        public const string PresetLong = "Long";
        public const string PresetVeryLong = "VeryLong";
        public const string PresetExtremeLong = "ExtremeLong";
        public const string PresetInfinite = "Infinite";

        public const string PresetContextUniform = "Uniform";
        public const string PresetContextPlayerFavored = "PlayerFavored";
        public const string PresetContextNpcFavored = "NpcFavored";
        public const string PresetContextWorldDecay = "WorldDecay";

        public enum DrainContext
        {
            PlayerHeld = 0,
            NpcHeld = 1,
            WorldDropped = 2,
        }

        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = "Master switch for imbue duration scaling")]
        public static bool EnableMod = true;

        [ModOption(name = OptionPresetDuration, category = CategoryPresets, categoryOrder = 0, order = 10, defaultValueIndex = 4, valueSourceName = nameof(DurationPresetProvider), tooltip = "Batch writes global/collapsible drain multipliers")]
        public static string PresetDurationExperience = PresetDefaultPlus;

        [ModOption(name = OptionPresetContext, category = CategoryPresets, categoryOrder = 0, order = 20, defaultValueIndex = 0, valueSourceName = nameof(ContextPresetProvider), tooltip = "Batch writes context collapsibles without changing runtime rules")]
        public static string PresetContextProfile = PresetContextUniform;

        [ModOption(name = OptionGlobalDrainMultiplier, category = CategoryPresets, categoryOrder = 0, order = 30, defaultValueIndex = 6, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base drain multiplier. 1.00 = base game, lower = longer, 0 = infinite")]
        public static float GlobalDrainMultiplier = 0.85f;

        [ModOption(name = OptionUpdateInterval, category = CategoryDiagnostics, categoryOrder = 200, order = 30, defaultValueIndex = 1, valueSourceName = nameof(UpdateIntervalProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How often runtime scaling updates active imbues")]
        public static float UpdateIntervalSeconds = 0.10f;

        [ModOption(name = OptionMaxCorrectionPerTick, category = CategoryDiagnostics, categoryOrder = 200, order = 40, defaultValueIndex = 3, valueSourceName = nameof(MaxCorrectionProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Caps max energy correction each tick as % of max energy")]
        public static float MaxCorrectionPercentPerTick = 15f;

        [ModOption(name = OptionMinimumEnergyFloor, category = CategoryDiagnostics, categoryOrder = 200, order = 50, defaultValueIndex = 1, valueSourceName = nameof(MinimumEnergyFloorProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Optional floor used when slowing drain to avoid accidental unload bursts")]
        public static float MinimumEnergyFloorPercent = 1f;

        [ModOption(name = OptionUseNativeInfinite, category = CategoryDiagnostics, categoryOrder = 200, order = 60, defaultValueIndex = 1, tooltip = "When global drain is Infinite, also toggle native Imbue.infiniteImbue")]
        public static bool UseNativeInfiniteFlag = true;

        [ModOption(name = OptionPlayerHeldDrainMultiplier, category = CategoryPresets, categoryOrder = 0, order = 40, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for held player weapons")]
        public static float PlayerHeldDrainMultiplier = 1f;

        [ModOption(name = OptionNpcHeldDrainMultiplier, category = CategoryPresets, categoryOrder = 0, order = 50, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for held NPC weapons")]
        public static float NpcHeldDrainMultiplier = 1f;

        [ModOption(name = OptionWorldDrainMultiplier, category = CategoryPresets, categoryOrder = 0, order = 60, defaultValueIndex = 7, valueSourceName = nameof(DrainMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Context multiplier for dropped/world weapons")]
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

        public static ModOptionString[] DurationPresetProvider()
        {
            return new[]
            {
                new ModOptionString("Way Less", PresetWayLess),
                new ModOptionString("Less", PresetLess),
                new ModOptionString("Slightly Less", PresetSlightlyLess),
                new ModOptionString("Base Game", PresetBaseGame),
                new ModOptionString("Default Plus", PresetDefaultPlus),
                new ModOptionString("Long", PresetLong),
                new ModOptionString("Very Long", PresetVeryLong),
                new ModOptionString("Extreme Long", PresetExtremeLong),
                new ModOptionString("Infinite", PresetInfinite),
            };
        }

        public static ModOptionString[] ContextPresetProvider()
        {
            return new[]
            {
                new ModOptionString("Uniform", PresetContextUniform),
                new ModOptionString("Player Favored", PresetContextPlayerFavored),
                new ModOptionString("NPC Favored", PresetContextNpcFavored),
                new ModOptionString("World Decay", PresetContextWorldDecay),
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
            string durationPreset = NormalizeDurationPreset(PresetDurationExperience);
            string contextPreset = NormalizeContextPreset(PresetContextProfile);

            float global = ResolveDurationPresetMultiplier(durationPreset);
            ResolveContextFactors(contextPreset, out float playerHeld, out float npcHeld, out float world);

            bool changed = false;
            changed |= SetFloat(ref GlobalDrainMultiplier, global);
            changed |= SetFloat(ref PlayerHeldDrainMultiplier, playerHeld);
            changed |= SetFloat(ref NpcHeldDrainMultiplier, npcHeld);
            changed |= SetFloat(ref WorldDrainMultiplier, world);
            return changed;
        }

        public static int GetPresetSelectionHash()
        {
            int hash = 17;
            hash = CombineHash(hash, StringHash(NormalizeDurationPreset(PresetDurationExperience)));
            hash = CombineHash(hash, StringHash(NormalizeContextPreset(PresetContextProfile)));
            return hash;
        }

        public static int GetSourceOfTruthHash()
        {
            int hash = 17;
            hash = CombineHash(hash, PercentHash(GlobalDrainMultiplier));
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

            float multiplier = Mathf.Max(0f, GlobalDrainMultiplier) * Mathf.Max(0f, contextMultiplier);
            return Mathf.Clamp(multiplier, 0f, 6f);
        }

        public static bool ShouldUseNativeInfinite()
        {
            return EnableMod && UseNativeInfiniteFlag && GlobalDrainMultiplier <= 0.0001f;
        }

        public static float GetUpdateIntervalSeconds()
        {
            return Mathf.Clamp(UpdateIntervalSeconds, 0.03f, 1.0f);
        }

        public static float GetMaxCorrectionRatio()
        {
            return Mathf.Clamp(MaxCorrectionPercentPerTick / 100f, 0.01f, 1f);
        }

        public static float GetMinimumEnergyFloorRatio()
        {
            return Mathf.Clamp01(MinimumEnergyFloorPercent / 100f);
        }

        public static string NormalizeDurationPreset(string preset)
        {
            string token = NormalizeToken(preset);

            if (token.Contains("INFINITE")) return PresetInfinite;
            if (token.Contains("EXTREME")) return PresetExtremeLong;
            if (token.Contains("VERYLONG")) return PresetVeryLong;
            if (token.Contains("LONG")) return PresetLong;
            if (token.Contains("DEFAULT") || token.Contains("PLUS")) return PresetDefaultPlus;
            if (token.Contains("BASE") || token.Contains("GAME")) return PresetBaseGame;
            if (token.Contains("SLIGHT")) return PresetSlightlyLess;
            if (token.Contains("WAY") || token.Contains("MUCH")) return PresetWayLess;
            if (token.Contains("LESS")) return PresetLess;

            return PresetDefaultPlus;
        }

        public static string NormalizeContextPreset(string preset)
        {
            string token = NormalizeToken(preset);

            if (token.Contains("PLAYER")) return PresetContextPlayerFavored;
            if (token.Contains("NPC")) return PresetContextNpcFavored;
            if (token.Contains("WORLD") || token.Contains("DROPPED")) return PresetContextWorldDecay;
            return PresetContextUniform;
        }

        public static string GetSourceOfTruthSummary()
        {
            return "global=" + GlobalDrainMultiplier.ToString("0.00") +
                   " playerHeld=" + PlayerHeldDrainMultiplier.ToString("0.00") +
                   " npcHeld=" + NpcHeldDrainMultiplier.ToString("0.00") +
                   " world=" + WorldDrainMultiplier.ToString("0.00") +
                   " update=" + GetUpdateIntervalSeconds().ToString("0.00") + "s" +
                   " maxCorrection=" + MaxCorrectionPercentPerTick.ToString("0") + "%" +
                   " floor=" + MinimumEnergyFloorPercent.ToString("0") + "%";
        }

        private static float ResolveDurationPresetMultiplier(string preset)
        {
            switch (preset)
            {
                case PresetWayLess:
                    return 2.50f;
                case PresetLess:
                    return 1.60f;
                case PresetSlightlyLess:
                    return 1.15f;
                case PresetBaseGame:
                    return 1.00f;
                case PresetLong:
                    return 0.65f;
                case PresetVeryLong:
                    return 0.40f;
                case PresetExtremeLong:
                    return 0.20f;
                case PresetInfinite:
                    return 0.00f;
                default:
                    return 0.85f;
            }
        }

        private static void ResolveContextFactors(string contextPreset, out float playerHeld, out float npcHeld, out float world)
        {
            playerHeld = 1f;
            npcHeld = 1f;
            world = 1f;

            switch (contextPreset)
            {
                case PresetContextPlayerFavored:
                    playerHeld = 0.75f;
                    npcHeld = 1.25f;
                    world = 1.00f;
                    break;
                case PresetContextNpcFavored:
                    playerHeld = 1.25f;
                    npcHeld = 0.75f;
                    world = 1.00f;
                    break;
                case PresetContextWorldDecay:
                    playerHeld = 0.95f;
                    npcHeld = 0.95f;
                    world = 1.80f;
                    break;
            }

            playerHeld = Mathf.Clamp(playerHeld, 0f, 6f);
            npcHeld = Mathf.Clamp(npcHeld, 0f, 6f);
            world = Mathf.Clamp(world, 0f, 6f);
        }

        private static bool SetFloat(ref float field, float value)
        {
            float clamped = Mathf.Clamp(value, 0f, 6f);
            if (Mathf.Abs(field - clamped) < 0.0001f)
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
            return Mathf.RoundToInt(value * 1000f);
        }
    }
}

