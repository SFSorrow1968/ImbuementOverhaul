using System;
using System.Collections.Generic;
using System.Reflection;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Configuration
{
    public static class EIPModOptions
    {
        public const string VERSION = "0.4.0";
        public const int FactionCount = 8;
        public const int ImbueSlotsPerFaction = 3;

        private const string CategoryPresets = "Imbuement Overhaul";
        private const string CategoryEnemyTypes = "Enemy Type Eligibility";
        private const string CategoryDiagnostics = "Advanced";

        private const string CategoryF01 = "Combat Practice (Mixed Enemies)";
        private const string CategoryF02 = "Outlaws (T0)";
        private const string CategoryF03 = "Wildfolk (T1)";
        private const string CategoryF04 = "Eraden Kingdom (T2)";
        private const string CategoryF05 = "The Eye (T3)";
        private const string CategoryF06 = "Rakta";
        private const string CategoryF07 = "Special / Rogue";
        private const string CategoryF08 = "Fallback (Any Enemy)";

        private const string OptionEnableMod = "Enable Mod";
        private const string OptionPresetFactionProfile = "Faction Profile Preset";
        private const string OptionPresetEnemyTypeProfile = "Enemy Type Profile Preset";
        private const string OptionPresetImbue = "Imbue Experience Preset";
        private const string OptionPresetChance = "Chance Experience Preset";
        private const string OptionPresetStrength = "Strength Experience Preset";

        private const string OptionEnemyTypeMageEligible = "Mage Eligible";
        private const string OptionEnemyTypeBowEligible = "Bow Eligible";
        private const string OptionEnemyTypeMeleeEligible = "Melee Eligible";
        private const string OptionEnemyTypeUncertainFallback = "Uncertain Enemy Type Fallback";

        private const string OptionEnableBasicLogging = "Basic Logs";
        private const string OptionEnableDiagnosticsLogging = "Diagnostics Logs";
        private const string OptionEnableVerboseLogging = "Verbose Logs";
        private const string OptionUpdateInterval = "Imbue Update Interval";
        private const string OptionRescanInterval = "Enemy Rescan Interval";
        private const string OptionForceReapply = "Force Reapply";

        private const string OptionF01Enabled = "Combat Enabled";
        private const string OptionF01S1Spell = "Combat Imbue 1";
        private const string OptionF01S1Chance = "Combat Chance 1";
        private const string OptionF01S1Strength = "Combat Strength 1";
        private const string OptionF01S2Spell = "Combat Imbue 2";
        private const string OptionF01S2Chance = "Combat Chance 2";
        private const string OptionF01S2Strength = "Combat Strength 2";
        private const string OptionF01S3Spell = "Combat Imbue 3";
        private const string OptionF01S3Chance = "Combat Chance 3";
        private const string OptionF01S3Strength = "Combat Strength 3";

        private const string OptionF02Enabled = "Outlaws Enabled";
        private const string OptionF02S1Spell = "Outlaws Imbue 1";
        private const string OptionF02S1Chance = "Outlaws Chance 1";
        private const string OptionF02S1Strength = "Outlaws Strength 1";
        private const string OptionF02S2Spell = "Outlaws Imbue 2";
        private const string OptionF02S2Chance = "Outlaws Chance 2";
        private const string OptionF02S2Strength = "Outlaws Strength 2";
        private const string OptionF02S3Spell = "Outlaws Imbue 3";
        private const string OptionF02S3Chance = "Outlaws Chance 3";
        private const string OptionF02S3Strength = "Outlaws Strength 3";

        private const string OptionF03Enabled = "Wildfolk Enabled";
        private const string OptionF03S1Spell = "Wildfolk Imbue 1";
        private const string OptionF03S1Chance = "Wildfolk Chance 1";
        private const string OptionF03S1Strength = "Wildfolk Strength 1";
        private const string OptionF03S2Spell = "Wildfolk Imbue 2";
        private const string OptionF03S2Chance = "Wildfolk Chance 2";
        private const string OptionF03S2Strength = "Wildfolk Strength 2";
        private const string OptionF03S3Spell = "Wildfolk Imbue 3";
        private const string OptionF03S3Chance = "Wildfolk Chance 3";
        private const string OptionF03S3Strength = "Wildfolk Strength 3";

        private const string OptionF04Enabled = "Eraden Enabled";
        private const string OptionF04S1Spell = "Eraden Imbue 1";
        private const string OptionF04S1Chance = "Eraden Chance 1";
        private const string OptionF04S1Strength = "Eraden Strength 1";
        private const string OptionF04S2Spell = "Eraden Imbue 2";
        private const string OptionF04S2Chance = "Eraden Chance 2";
        private const string OptionF04S2Strength = "Eraden Strength 2";
        private const string OptionF04S3Spell = "Eraden Imbue 3";
        private const string OptionF04S3Chance = "Eraden Chance 3";
        private const string OptionF04S3Strength = "Eraden Strength 3";

        private const string OptionF05Enabled = "Eye Enabled";
        private const string OptionF05S1Spell = "Eye Imbue 1";
        private const string OptionF05S1Chance = "Eye Chance 1";
        private const string OptionF05S1Strength = "Eye Strength 1";
        private const string OptionF05S2Spell = "Eye Imbue 2";
        private const string OptionF05S2Chance = "Eye Chance 2";
        private const string OptionF05S2Strength = "Eye Strength 2";
        private const string OptionF05S3Spell = "Eye Imbue 3";
        private const string OptionF05S3Chance = "Eye Chance 3";
        private const string OptionF05S3Strength = "Eye Strength 3";

        private const string OptionF06Enabled = "Rakta Enabled";
        private const string OptionF06S1Spell = "Rakta Imbue 1";
        private const string OptionF06S1Chance = "Rakta Chance 1";
        private const string OptionF06S1Strength = "Rakta Strength 1";
        private const string OptionF06S2Spell = "Rakta Imbue 2";
        private const string OptionF06S2Chance = "Rakta Chance 2";
        private const string OptionF06S2Strength = "Rakta Strength 2";
        private const string OptionF06S3Spell = "Rakta Imbue 3";
        private const string OptionF06S3Chance = "Rakta Chance 3";
        private const string OptionF06S3Strength = "Rakta Strength 3";

        private const string OptionF07Enabled = "Special Enabled";
        private const string OptionF07S1Spell = "Special Imbue 1";
        private const string OptionF07S1Chance = "Special Chance 1";
        private const string OptionF07S1Strength = "Special Strength 1";
        private const string OptionF07S2Spell = "Special Imbue 2";
        private const string OptionF07S2Chance = "Special Chance 2";
        private const string OptionF07S2Strength = "Special Strength 2";
        private const string OptionF07S3Spell = "Special Imbue 3";
        private const string OptionF07S3Chance = "Special Chance 3";
        private const string OptionF07S3Strength = "Special Strength 3";

        private const string OptionF08Enabled = "Fallback Enabled";
        private const string OptionF08S1Spell = "Fallback Imbue 1";
        private const string OptionF08S1Chance = "Fallback Chance 1";
        private const string OptionF08S1Strength = "Fallback Strength 1";
        private const string OptionF08S2Spell = "Fallback Imbue 2";
        private const string OptionF08S2Chance = "Fallback Chance 2";
        private const string OptionF08S2Strength = "Fallback Strength 2";
        private const string OptionF08S3Spell = "Fallback Imbue 3";
        private const string OptionF08S3Chance = "Fallback Chance 3";
        private const string OptionF08S3Strength = "Fallback Strength 3";

        public const string PresetProfileLore = "LoreFriendly";
        public const string PresetProfileFrontier = "FrontierPressure";
        public const string PresetProfileWarfront = "WarfrontArcana";
        public const string PresetProfileHighMagic = "HighMagicConflict";
        public const string PresetProfileRandom = "RandomizedEligibility";
        public const string PresetEnemyTypeMageOnly = "MageOnly";
        public const string PresetEnemyTypeRanged = "Ranged";
        public const string PresetEnemyTypeAll = "All";

        public const string EnemyTypeFallbackMelee = "Melee";
        public const string EnemyTypeFallbackSkip = "Skip";

        public const string PresetImbueLore = "LoreAccurate";
        public const string PresetImbueFactionIdentity = "FactionIdentity";
        public const string PresetImbueArcaneSurge = "ArcaneSurge";
        public const string PresetImbueElementalChaos = "ElementalChaos";
        public const string PresetImbueRandomized = "Randomized";

        public const string PresetChanceLow = "LowIntensity";
        public const string PresetChanceBalanced = "BalancedBattles";
        public const string PresetChanceAggressive = "AggressiveWaves";
        public const string PresetChanceRelentless = "RelentlessThreat";
        public const string PresetChanceOverflow = "OverflowNormalized";

        public const string PresetStrengthFaint = "FaintCharge";
        public const string PresetStrengthStandard = "BattleReady";
        public const string PresetStrengthEmpowered = "Empowered";
        public const string PresetStrengthOvercharged = "Overcharged";
        public const string PresetStrengthCataclysmic = "Cataclysmic";

        public const string SpellNone = "None";

        private static readonly string[] FactionCategories =
        {
            CategoryF01,
            CategoryF02,
            CategoryF03,
            CategoryF04,
            CategoryF05,
            CategoryF06,
            CategoryF07,
            CategoryF08,
        };

        private static readonly int[] DefaultFactionIds =
        {
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            -1,
        };

        private static readonly string[] FactionDisplayNames =
        {
            "Mixed Enemies",
            "Outlaws",
            "Wildfolk",
            "Eraden Kingdom",
            "The Eye",
            "Rakta",
            "Special / Rogue",
            "Any Enemy",
        };

        private static readonly string[] FactionShortNames =
        {
            "Combat",
            "Outlaws",
            "Wildfolk",
            "Eraden",
            "Eye",
            "Rakta",
            "Special",
            "Fallback",
        };

        private static readonly string[] EnemyTypeDisplayNames =
        {
            "Mage",
            "Bow",
            "Melee",
        };

        private static readonly string[] EnemyTypeOptionNames =
        {
            OptionEnemyTypeMageEligible,
            OptionEnemyTypeBowEligible,
            OptionEnemyTypeMeleeEligible,
        };

        private static readonly string[] EnemyTypeTokens =
        {
            "mage",
            "bow",
            "melee",
        };

        private static readonly string[][] FactionKeywords =
        {
            new[] { "mixed", "combat" },
            new[] { "outlaw", "bandit", "t0" },
            new[] { "wildfolk", "wild", "t1" },
            new[] { "eraden", "kingdom", "t2" },
            new[] { "eye", "t3" },
            new[] { "rakta" },
            new[] { "special", "rogue" },
            new string[0],
        };

        private static readonly System.Random presetRandom = new System.Random();
        private static readonly string[] BaseSpellPool = { "Fire", "Lightning", "Gravity" };

        private static int[] resolvedFactionIds;
        private static bool resolvedFromCatalog;
        private static int resolvedFactionCount = -1;

        public enum WeaponFilterBucket
        {
            All = 0,
            Arrow = 1,
            Dagger = 2,
            Sword = 3,
            Axe = 4,
            Mace = 5,
            Spear = 6,
            Staff = 7,
            Bow = 8,
            Shield = 9,
            Throwing = 10,
            Other = 11
        }

        public enum EnemyTypeArchetype
        {
            Mage = 0,
            Bow = 1,
            Melee = 2
        }

        public struct ImbueSlotConfig
        {
            public int SlotIndex;
            public string SpellId;
            public float ChancePercent;
            public float StrengthPercent;
        }

        public struct FactionProfile
        {
            public int FactionIndex;
            public int FactionId;
            public bool Enabled;
            public ImbueSlotConfig Slot1;
            public ImbueSlotConfig Slot2;
            public ImbueSlotConfig Slot3;
            public float TotalNormalizedChancePercent;
            public int ProfileHash;
        }

        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = "Master switch for Imbuement Overhaul.")]
        public static bool EnableMod = true;

        [ModOption(name = OptionPresetFactionProfile, category = CategoryPresets, categoryOrder = 0, order = 5, defaultValueIndex = 0, valueSourceName = nameof(FactionProfilePresetProvider), tooltip = "Controls which faction collapsibles are eligible for imbues.")]
        public static string PresetFactionProfile = PresetProfileLore;

        [ModOption(name = OptionPresetEnemyTypeProfile, category = CategoryPresets, categoryOrder = 0, order = 7, defaultValueIndex = 0, valueSourceName = nameof(EnemyTypeProfilePresetProvider), tooltip = "Controls runtime enemy-type eligibility by archetype.")]
        public static string PresetEnemyTypeProfile = PresetEnemyTypeMageOnly;

        [ModOption(name = OptionPresetImbue, category = CategoryPresets, categoryOrder = 0, order = 10, defaultValueIndex = 0, valueSourceName = nameof(ImbuePresetProvider), tooltip = "Controls which imbue types each faction slot can receive.")]
        public static string PresetImbue = PresetImbueLore;

        [ModOption(name = OptionPresetChance, category = CategoryPresets, categoryOrder = 0, order = 20, defaultValueIndex = 0, valueSourceName = nameof(ChancePresetProvider), tooltip = "Controls per-slot imbue chance for each faction.")]
        public static string PresetChance = PresetChanceLow;

        [ModOption(name = OptionPresetStrength, category = CategoryPresets, categoryOrder = 0, order = 30, defaultValueIndex = 0, valueSourceName = nameof(StrengthPresetProvider), tooltip = "Controls per-slot imbue strength for each faction.")]
        public static string PresetStrength = PresetStrengthFaint;

        [ModOption(name = OptionEnemyTypeMageEligible, category = CategoryEnemyTypes, categoryOrder = 20, order = 0, defaultValueIndex = 1, tooltip = "If disabled, pure mage enemies cannot receive imbues.")]
        public static bool EnemyTypeMageEligible = true;
        [ModOption(name = OptionEnemyTypeBowEligible, category = CategoryEnemyTypes, categoryOrder = 20, order = 10, defaultValueIndex = 0, tooltip = "If disabled, non-caster bow enemies cannot receive imbues.")]
        public static bool EnemyTypeBowEligible = false;
        [ModOption(name = OptionEnemyTypeMeleeEligible, category = CategoryEnemyTypes, categoryOrder = 20, order = 20, defaultValueIndex = 0, tooltip = "If disabled, non-caster melee enemies cannot receive imbues.")]
        public static bool EnemyTypeMeleeEligible = false;
        [ModOption(name = OptionEnemyTypeUncertainFallback, category = CategoryEnemyTypes, categoryOrder = 20, order = 30, defaultValueIndex = 0, valueSourceName = nameof(EnemyTypeFallbackProvider), tooltip = "How to handle enemies where runtime archetype signals are uncertain.")]
        public static string EnemyTypeUncertainFallbackMode = EnemyTypeFallbackMelee;

        [ModOption(name = OptionF01Enabled, category = CategoryF01, categoryOrder = 100, order = 0, defaultValueIndex = 1)]
        public static bool F01Enabled = true;

        [ModOption(name = OptionF01S1Spell, category = CategoryF01, categoryOrder = 100, order = 10, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F01S1Spell = "Fire";
        [ModOption(name = OptionF01S1Chance, category = CategoryF01, categoryOrder = 100, order = 20, defaultValueIndex = 5, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F01S1Chance = 25f;
        [ModOption(name = OptionF01S1Strength, category = CategoryF01, categoryOrder = 100, order = 30, defaultValueIndex = 13, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F01S1Strength = 65f;

        [ModOption(name = OptionF01S2Spell, category = CategoryF01, categoryOrder = 100, order = 40, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F01S2Spell = "Lightning";
        [ModOption(name = OptionF01S2Chance, category = CategoryF01, categoryOrder = 100, order = 50, defaultValueIndex = 2, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F01S2Chance = 10f;
        [ModOption(name = OptionF01S2Strength, category = CategoryF01, categoryOrder = 100, order = 60, defaultValueIndex = 10, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F01S2Strength = 50f;

        [ModOption(name = OptionF01S3Spell, category = CategoryF01, categoryOrder = 100, order = 70, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F01S3Spell = "Gravity";
        [ModOption(name = OptionF01S3Chance, category = CategoryF01, categoryOrder = 100, order = 80, defaultValueIndex = 1, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F01S3Chance = 5f;
        [ModOption(name = OptionF01S3Strength, category = CategoryF01, categoryOrder = 100, order = 90, defaultValueIndex = 8, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F01S3Strength = 40f;

        [ModOption(name = OptionF02Enabled, category = CategoryF02, categoryOrder = 110, order = 0, defaultValueIndex = 1)]
        public static bool F02Enabled = true;

        [ModOption(name = OptionF02S1Spell, category = CategoryF02, categoryOrder = 110, order = 10, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F02S1Spell = "Fire";
        [ModOption(name = OptionF02S1Chance, category = CategoryF02, categoryOrder = 110, order = 20, defaultValueIndex = 9, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F02S1Chance = 45f;
        [ModOption(name = OptionF02S1Strength, category = CategoryF02, categoryOrder = 110, order = 30, defaultValueIndex = 15, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F02S1Strength = 75f;

        [ModOption(name = OptionF02S2Spell, category = CategoryF02, categoryOrder = 110, order = 40, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F02S2Spell = "Fire";
        [ModOption(name = OptionF02S2Chance, category = CategoryF02, categoryOrder = 110, order = 50, defaultValueIndex = 4, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F02S2Chance = 20f;
        [ModOption(name = OptionF02S2Strength, category = CategoryF02, categoryOrder = 110, order = 60, defaultValueIndex = 12, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F02S2Strength = 60f;

        [ModOption(name = OptionF02S3Spell, category = CategoryF02, categoryOrder = 110, order = 70, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F02S3Spell = "Lightning";
        [ModOption(name = OptionF02S3Chance, category = CategoryF02, categoryOrder = 110, order = 80, defaultValueIndex = 2, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F02S3Chance = 10f;
        [ModOption(name = OptionF02S3Strength, category = CategoryF02, categoryOrder = 110, order = 90, defaultValueIndex = 10, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F02S3Strength = 50f;

        [ModOption(name = OptionF03Enabled, category = CategoryF03, categoryOrder = 120, order = 0, defaultValueIndex = 1)]
        public static bool F03Enabled = true;

        [ModOption(name = OptionF03S1Spell, category = CategoryF03, categoryOrder = 120, order = 10, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F03S1Spell = "Gravity";
        [ModOption(name = OptionF03S1Chance, category = CategoryF03, categoryOrder = 120, order = 20, defaultValueIndex = 6, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F03S1Chance = 30f;
        [ModOption(name = OptionF03S1Strength, category = CategoryF03, categoryOrder = 120, order = 30, defaultValueIndex = 14, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F03S1Strength = 70f;

        [ModOption(name = OptionF03S2Spell, category = CategoryF03, categoryOrder = 120, order = 40, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F03S2Spell = "Lightning";
        [ModOption(name = OptionF03S2Chance, category = CategoryF03, categoryOrder = 120, order = 50, defaultValueIndex = 5, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F03S2Chance = 25f;
        [ModOption(name = OptionF03S2Strength, category = CategoryF03, categoryOrder = 120, order = 60, defaultValueIndex = 12, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F03S2Strength = 60f;

        [ModOption(name = OptionF03S3Spell, category = CategoryF03, categoryOrder = 120, order = 70, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F03S3Spell = "Fire";
        [ModOption(name = OptionF03S3Chance, category = CategoryF03, categoryOrder = 120, order = 80, defaultValueIndex = 3, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F03S3Chance = 15f;
        [ModOption(name = OptionF03S3Strength, category = CategoryF03, categoryOrder = 120, order = 90, defaultValueIndex = 9, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F03S3Strength = 45f;

        [ModOption(name = OptionF04Enabled, category = CategoryF04, categoryOrder = 130, order = 0, defaultValueIndex = 1)]
        public static bool F04Enabled = true;

        [ModOption(name = OptionF04S1Spell, category = CategoryF04, categoryOrder = 130, order = 10, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F04S1Spell = "Lightning";
        [ModOption(name = OptionF04S1Chance, category = CategoryF04, categoryOrder = 130, order = 20, defaultValueIndex = 7, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F04S1Chance = 35f;
        [ModOption(name = OptionF04S1Strength, category = CategoryF04, categoryOrder = 130, order = 30, defaultValueIndex = 16, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F04S1Strength = 80f;

        [ModOption(name = OptionF04S2Spell, category = CategoryF04, categoryOrder = 130, order = 40, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F04S2Spell = "Fire";
        [ModOption(name = OptionF04S2Chance, category = CategoryF04, categoryOrder = 130, order = 50, defaultValueIndex = 6, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F04S2Chance = 30f;
        [ModOption(name = OptionF04S2Strength, category = CategoryF04, categoryOrder = 130, order = 60, defaultValueIndex = 13, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F04S2Strength = 65f;

        [ModOption(name = OptionF04S3Spell, category = CategoryF04, categoryOrder = 130, order = 70, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F04S3Spell = "Gravity";
        [ModOption(name = OptionF04S3Chance, category = CategoryF04, categoryOrder = 130, order = 80, defaultValueIndex = 4, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F04S3Chance = 20f;
        [ModOption(name = OptionF04S3Strength, category = CategoryF04, categoryOrder = 130, order = 90, defaultValueIndex = 11, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F04S3Strength = 55f;

        [ModOption(name = OptionF05Enabled, category = CategoryF05, categoryOrder = 140, order = 0, defaultValueIndex = 1)]
        public static bool F05Enabled = true;

        [ModOption(name = OptionF05S1Spell, category = CategoryF05, categoryOrder = 140, order = 10, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F05S1Spell = "Gravity";
        [ModOption(name = OptionF05S1Chance, category = CategoryF05, categoryOrder = 140, order = 20, defaultValueIndex = 8, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F05S1Chance = 40f;
        [ModOption(name = OptionF05S1Strength, category = CategoryF05, categoryOrder = 140, order = 30, defaultValueIndex = 17, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F05S1Strength = 85f;

        [ModOption(name = OptionF05S2Spell, category = CategoryF05, categoryOrder = 140, order = 40, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F05S2Spell = "Lightning";
        [ModOption(name = OptionF05S2Chance, category = CategoryF05, categoryOrder = 140, order = 50, defaultValueIndex = 7, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F05S2Chance = 35f;
        [ModOption(name = OptionF05S2Strength, category = CategoryF05, categoryOrder = 140, order = 60, defaultValueIndex = 14, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F05S2Strength = 70f;

        [ModOption(name = OptionF05S3Spell, category = CategoryF05, categoryOrder = 140, order = 70, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F05S3Spell = "Fire";
        [ModOption(name = OptionF05S3Chance, category = CategoryF05, categoryOrder = 140, order = 80, defaultValueIndex = 3, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F05S3Chance = 15f;
        [ModOption(name = OptionF05S3Strength, category = CategoryF05, categoryOrder = 140, order = 90, defaultValueIndex = 12, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F05S3Strength = 60f;

        [ModOption(name = OptionF06Enabled, category = CategoryF06, categoryOrder = 150, order = 0, defaultValueIndex = 1)]
        public static bool F06Enabled = true;

        [ModOption(name = OptionF06S1Spell, category = CategoryF06, categoryOrder = 150, order = 10, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F06S1Spell = "Fire";
        [ModOption(name = OptionF06S1Chance, category = CategoryF06, categoryOrder = 150, order = 20, defaultValueIndex = 10, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F06S1Chance = 50f;
        [ModOption(name = OptionF06S1Strength, category = CategoryF06, categoryOrder = 150, order = 30, defaultValueIndex = 16, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F06S1Strength = 80f;

        [ModOption(name = OptionF06S2Spell, category = CategoryF06, categoryOrder = 150, order = 40, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F06S2Spell = "Gravity";
        [ModOption(name = OptionF06S2Chance, category = CategoryF06, categoryOrder = 150, order = 50, defaultValueIndex = 5, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F06S2Chance = 25f;
        [ModOption(name = OptionF06S2Strength, category = CategoryF06, categoryOrder = 150, order = 60, defaultValueIndex = 13, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F06S2Strength = 65f;

        [ModOption(name = OptionF06S3Spell, category = CategoryF06, categoryOrder = 150, order = 70, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F06S3Spell = "Lightning";
        [ModOption(name = OptionF06S3Chance, category = CategoryF06, categoryOrder = 150, order = 80, defaultValueIndex = 3, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F06S3Chance = 15f;
        [ModOption(name = OptionF06S3Strength, category = CategoryF06, categoryOrder = 150, order = 90, defaultValueIndex = 11, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F06S3Strength = 55f;

        [ModOption(name = OptionF07Enabled, category = CategoryF07, categoryOrder = 160, order = 0, defaultValueIndex = 1)]
        public static bool F07Enabled = true;

        [ModOption(name = OptionF07S1Spell, category = CategoryF07, categoryOrder = 160, order = 10, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F07S1Spell = "Lightning";
        [ModOption(name = OptionF07S1Chance, category = CategoryF07, categoryOrder = 160, order = 20, defaultValueIndex = 9, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F07S1Chance = 45f;
        [ModOption(name = OptionF07S1Strength, category = CategoryF07, categoryOrder = 160, order = 30, defaultValueIndex = 17, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F07S1Strength = 85f;

        [ModOption(name = OptionF07S2Spell, category = CategoryF07, categoryOrder = 160, order = 40, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F07S2Spell = "Gravity";
        [ModOption(name = OptionF07S2Chance, category = CategoryF07, categoryOrder = 160, order = 50, defaultValueIndex = 6, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F07S2Chance = 30f;
        [ModOption(name = OptionF07S2Strength, category = CategoryF07, categoryOrder = 160, order = 60, defaultValueIndex = 14, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F07S2Strength = 70f;

        [ModOption(name = OptionF07S3Spell, category = CategoryF07, categoryOrder = 160, order = 70, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F07S3Spell = "Fire";
        [ModOption(name = OptionF07S3Chance, category = CategoryF07, categoryOrder = 160, order = 80, defaultValueIndex = 4, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F07S3Chance = 20f;
        [ModOption(name = OptionF07S3Strength, category = CategoryF07, categoryOrder = 160, order = 90, defaultValueIndex = 12, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F07S3Strength = 60f;

        [ModOption(name = OptionF08Enabled, category = CategoryF08, categoryOrder = 170, order = 0, defaultValueIndex = 1)]
        public static bool F08Enabled = true;

        [ModOption(name = OptionF08S1Spell, category = CategoryF08, categoryOrder = 170, order = 10, defaultValueIndex = 0, valueSourceName = nameof(SpellProvider))]
        public static string F08S1Spell = "Fire";
        [ModOption(name = OptionF08S1Chance, category = CategoryF08, categoryOrder = 170, order = 20, defaultValueIndex = 4, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F08S1Chance = 20f;
        [ModOption(name = OptionF08S1Strength, category = CategoryF08, categoryOrder = 170, order = 30, defaultValueIndex = 12, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F08S1Strength = 60f;

        [ModOption(name = OptionF08S2Spell, category = CategoryF08, categoryOrder = 170, order = 40, defaultValueIndex = 1, valueSourceName = nameof(SpellProvider))]
        public static string F08S2Spell = "Lightning";
        [ModOption(name = OptionF08S2Chance, category = CategoryF08, categoryOrder = 170, order = 50, defaultValueIndex = 3, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F08S2Chance = 15f;
        [ModOption(name = OptionF08S2Strength, category = CategoryF08, categoryOrder = 170, order = 60, defaultValueIndex = 10, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F08S2Strength = 50f;

        [ModOption(name = OptionF08S3Spell, category = CategoryF08, categoryOrder = 170, order = 70, defaultValueIndex = 2, valueSourceName = nameof(SpellProvider))]
        public static string F08S3Spell = "Gravity";
        [ModOption(name = OptionF08S3Chance, category = CategoryF08, categoryOrder = 170, order = 80, defaultValueIndex = 2, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2, tooltip = "If total slot chances exceed 100, they are normalized.")]
        public static float F08S3Chance = 10f;
        [ModOption(name = OptionF08S3Strength, category = CategoryF08, categoryOrder = 170, order = 90, defaultValueIndex = 8, valueSourceName = nameof(PercentProvider), interactionType = (ModOption.InteractionType)2)]
        public static float F08S3Strength = 40f;

        [ModOption(name = OptionEnableBasicLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 0, defaultValueIndex = 1, tooltip = "Enable general informational logs")]
        public static bool EnableBasicLogging = true;
        [ModOption(name = OptionEnableDiagnosticsLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 5, defaultValueIndex = 0, tooltip = "Enable deeper troubleshooting logs")]
        public static bool EnableDiagnosticsLogging = false;
        [ModOption(name = OptionEnableVerboseLogging, category = CategoryDiagnostics, categoryOrder = 999, order = 7, defaultValueIndex = 0, tooltip = "Enable high-volume per-entity and per-state logs")]
        public static bool EnableVerboseLogging = false;
        [ModOption(name = OptionUpdateInterval, category = CategoryDiagnostics, categoryOrder = 999, order = 10, defaultValueIndex = 4, valueSourceName = nameof(UpdateIntervalProvider), interactionType = (ModOption.InteractionType)2)]
        public static float UpdateInterval = 0.25f;
        [ModOption(name = OptionRescanInterval, category = CategoryDiagnostics, categoryOrder = 999, order = 20, defaultValueIndex = 3, valueSourceName = nameof(RescanIntervalProvider), interactionType = (ModOption.InteractionType)2)]
        public static float RescanInterval = 2.0f;

        [ModOption(name = OptionForceReapply, category = CategoryDiagnostics, categoryOrder = 999, order = 30, defaultValueIndex = 0)]
        [ModOptionDontSave]
        public static bool ForceReapply = false;

        private static readonly string[] FactionCodes =
        {
            "F01",
            "F02",
            "F03",
            "F04",
            "F05",
            "F06",
            "F07",
            "F08"
        };

        private static readonly FieldInfo[] FactionEnabledFields = new FieldInfo[FactionCount];
        private static readonly FieldInfo[,] FactionSpellFields = new FieldInfo[FactionCount, ImbueSlotsPerFaction];
        private static readonly FieldInfo[,] FactionChanceFields = new FieldInfo[FactionCount, ImbueSlotsPerFaction];
        private static readonly FieldInfo[,] FactionStrengthFields = new FieldInfo[FactionCount, ImbueSlotsPerFaction];

        private static readonly string[] FactionEnabledOptionNames = new string[FactionCount];
        private static readonly string[,] FactionSpellOptionNames = new string[FactionCount, ImbueSlotsPerFaction];
        private static readonly string[,] FactionChanceOptionNames = new string[FactionCount, ImbueSlotsPerFaction];
        private static readonly string[,] FactionStrengthOptionNames = new string[FactionCount, ImbueSlotsPerFaction];

        private static readonly Dictionary<int, int> FactionIdToIndex = new Dictionary<int, int>();

        // Preset profile determines faction eligibility toggles.
        // Factions: Combat, Outlaws, Wildfolk, Eraden, Eye, Rakta, Special, Fallback.
        private static readonly bool[][] ProfileEnabledValues =
        {
            new[] { false, false, false, true, true, true, true, true }, // Lore Friendly
            new[] { false, false, true,  true, true, true, true, true }, // Frontier Pressure
            new[] { false, true,  true,  true, true, true, true, true }, // Warfront Arcana
            new[] { true,  true,  true,  true, true, true, true, true }  // High Magic Conflict
        };

        // Enemy-type eligibility profile writes:
        // [0] Mage, [1] Bow, [2] Melee.
        private static readonly bool[][] ProfileEnemyTypeValues =
        {
            new[] { true,  false, false }, // Mage Only
            new[] { true,  true,  false }, // Ranged
            new[] { true,  true,  true  }  // All
        };

        // Imbue preset determines which spells can appear in each slot (None = no imbue in that slot).
        private static readonly string[][] ImbueLoreValues =
        {
            new[] { SpellNone, SpellNone, SpellNone },
            new[] { SpellNone, SpellNone, SpellNone },
            new[] { SpellNone, SpellNone, SpellNone },
            new[] { "Fire",    SpellNone, SpellNone },
            new[] { "Gravity", SpellNone, SpellNone },
            new[] { "Lightning", SpellNone, SpellNone },
            new[] { "Gravity", SpellNone, SpellNone },
            new[] { "Fire",    SpellNone, SpellNone }
        };

        private static readonly string[][] ImbueFrontierValues =
        {
            new[] { SpellNone, SpellNone, SpellNone },
            new[] { SpellNone, SpellNone, SpellNone },
            new[] { "Lightning", SpellNone, SpellNone },
            new[] { "Fire",    "Lightning", SpellNone },
            new[] { "Gravity", "Lightning", SpellNone },
            new[] { "Fire",    "Lightning", SpellNone },
            new[] { "Gravity", "Fire",     SpellNone },
            new[] { "Fire",    "Lightning", SpellNone }
        };

        private static readonly string[][] ImbueWarfrontValues =
        {
            new[] { "Fire",    SpellNone, SpellNone },
            new[] { "Fire",    SpellNone, SpellNone },
            new[] { "Lightning", "Fire",   SpellNone },
            new[] { "Fire",    "Lightning", "Gravity" },
            new[] { "Gravity", "Lightning", "Fire" },
            new[] { "Lightning", "Fire",    "Gravity" },
            new[] { "Gravity", "Fire",      "Lightning" },
            new[] { "Fire",    "Lightning", "Gravity" }
        };

        private static readonly string[][] ImbueHighMagicValues =
        {
            new[] { "Fire",    "Lightning", SpellNone },
            new[] { "Fire",    "Lightning", SpellNone },
            new[] { "Lightning", "Fire",    SpellNone },
            new[] { "Fire",    "Lightning", "Gravity" },
            new[] { "Gravity", "Lightning", "Fire" },
            new[] { "Lightning", "Fire",    "Gravity" },
            new[] { "Gravity", "Fire",      "Lightning" },
            new[] { "Fire",    "Lightning", "Gravity" }
        };

        // Chance presets are world-tiered: lower tiers get lower chance in lore presets.
        private static readonly float[][] ChanceLoreValues =
        {
            new[] { 0f,  0f,  0f },
            new[] { 0f,  0f,  0f },
            new[] { 6f,  0f,  0f },
            new[] { 24f, 0f,  0f },
            new[] { 28f, 0f,  0f },
            new[] { 30f, 0f,  0f },
            new[] { 16f, 0f,  0f },
            new[] { 12f, 0f,  0f }
        };

        private static readonly float[][] ChanceFrontierValues =
        {
            new[] { 2f,  0f,  0f },
            new[] { 4f,  0f,  0f },
            new[] { 10f, 2f,  0f },
            new[] { 30f, 14f, 0f },
            new[] { 34f, 16f, 0f },
            new[] { 36f, 18f, 0f },
            new[] { 20f, 8f,  0f },
            new[] { 16f, 6f,  0f }
        };

        private static readonly float[][] ChanceWarfrontValues =
        {
            new[] { 6f,  2f,  0f },
            new[] { 8f,  4f,  0f },
            new[] { 14f, 6f,  0f },
            new[] { 34f, 18f, 8f },
            new[] { 38f, 20f, 10f },
            new[] { 40f, 22f, 10f },
            new[] { 24f, 12f, 6f },
            new[] { 20f, 10f, 4f }
        };

        private static readonly float[][] ChanceHighMagicValues =
        {
            new[] { 10f, 4f,  0f },
            new[] { 12f, 6f,  0f },
            new[] { 18f, 8f,  2f },
            new[] { 38f, 22f, 10f },
            new[] { 42f, 24f, 12f },
            new[] { 44f, 26f, 12f },
            new[] { 28f, 14f, 8f },
            new[] { 24f, 12f, 6f }
        };

        private static readonly float[][] ChanceRandomValues =
        {
            new[] { 14f, 8f,  2f },
            new[] { 18f, 10f, 4f },
            new[] { 24f, 14f, 6f },
            new[] { 46f, 28f, 14f },
            new[] { 50f, 30f, 16f },
            new[] { 52f, 32f, 16f },
            new[] { 34f, 18f, 10f },
            new[] { 30f, 16f, 8f }
        };

        // Strength presets are world-tiered: lower tiers stay weaker in lore presets.
        private static readonly float[][] StrengthLoreValues =
        {
            new[] { 0f,  0f,  0f },
            new[] { 0f,  0f,  0f },
            new[] { 22f, 0f,  0f },
            new[] { 56f, 0f,  0f },
            new[] { 60f, 0f,  0f },
            new[] { 62f, 0f,  0f },
            new[] { 40f, 0f,  0f },
            new[] { 34f, 0f,  0f }
        };

        private static readonly float[][] StrengthFrontierValues =
        {
            new[] { 14f, 0f,  0f },
            new[] { 18f, 0f,  0f },
            new[] { 28f, 12f, 0f },
            new[] { 62f, 36f, 0f },
            new[] { 66f, 40f, 0f },
            new[] { 68f, 42f, 0f },
            new[] { 46f, 22f, 0f },
            new[] { 40f, 18f, 0f }
        };

        private static readonly float[][] StrengthWarfrontValues =
        {
            new[] { 20f, 10f, 0f },
            new[] { 24f, 14f, 0f },
            new[] { 34f, 18f, 6f },
            new[] { 68f, 44f, 22f },
            new[] { 72f, 48f, 24f },
            new[] { 74f, 50f, 24f },
            new[] { 52f, 28f, 14f },
            new[] { 46f, 24f, 10f }
        };

        private static readonly float[][] StrengthHighMagicValues =
        {
            new[] { 26f, 14f, 0f },
            new[] { 30f, 18f, 0f },
            new[] { 40f, 22f, 8f },
            new[] { 74f, 50f, 28f },
            new[] { 78f, 54f, 30f },
            new[] { 80f, 56f, 30f },
            new[] { 58f, 34f, 18f },
            new[] { 52f, 30f, 14f }
        };

        private static readonly float[][] StrengthRandomValues =
        {
            new[] { 34f, 20f, 8f },
            new[] { 40f, 24f, 10f },
            new[] { 48f, 28f, 12f },
            new[] { 82f, 60f, 36f },
            new[] { 86f, 64f, 38f },
            new[] { 88f, 66f, 38f },
            new[] { 66f, 40f, 24f },
            new[] { 58f, 34f, 18f }
        };

        static EIPModOptions()
        {
            for (int i = 0; i < FactionCount; i++)
            {
                string code = FactionCodes[i];
                FactionEnabledFields[i] = FindField(code + "Enabled");
                FactionEnabledOptionNames[i] = ReadConstString("Option" + code + "Enabled", FactionShortNames[i] + " Enabled");

                for (int slot = 0; slot < ImbueSlotsPerFaction; slot++)
                {
                    string slotSuffix = "S" + (slot + 1).ToString();
                    FactionSpellFields[i, slot] = FindField(code + slotSuffix + "Spell");
                    FactionChanceFields[i, slot] = FindField(code + slotSuffix + "Chance");
                    FactionStrengthFields[i, slot] = FindField(code + slotSuffix + "Strength");

                    FactionSpellOptionNames[i, slot] = ReadConstString("Option" + code + slotSuffix + "Spell", FactionShortNames[i] + " Imbue " + (slot + 1).ToString());
                    FactionChanceOptionNames[i, slot] = ReadConstString("Option" + code + slotSuffix + "Chance", FactionShortNames[i] + " Chance " + (slot + 1).ToString());
                    FactionStrengthOptionNames[i, slot] = ReadConstString("Option" + code + slotSuffix + "Strength", FactionShortNames[i] + " Strength " + (slot + 1).ToString());
                }
            }

            resolvedFactionIds = (int[])DefaultFactionIds.Clone();
            RebuildFactionIndexMap();
        }

        public static float ClampPercent(float value)
        {
            return Mathf.Clamp(value, 0f, 100f);
        }

        public static string CanonicalSpellId(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return string.Empty;
            }

            string value = spellId.Trim();
            if (value.Equals(SpellNone, StringComparison.OrdinalIgnoreCase) ||
                value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("disabled", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no imbue", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            if (value.Equals("fire", StringComparison.OrdinalIgnoreCase))
            {
                return "Fire";
            }
            if (value.Equals("lightning", StringComparison.OrdinalIgnoreCase))
            {
                return "Lightning";
            }
            if (value.Equals("gravity", StringComparison.OrdinalIgnoreCase))
            {
                return "Gravity";
            }

            return value;
        }

        public static string GetFactionCategory(int factionIndex)
        {
            int i = ToFactionIndex(factionIndex);
            return i >= 0 ? FactionCategories[i] : string.Empty;
        }

        public static string GetEnemyTypeCategory()
        {
            return CategoryEnemyTypes;
        }

        public static string GetEnemyTypeOptionName(EnemyTypeArchetype archetype)
        {
            int index = (int)archetype;
            return index >= 0 && index < EnemyTypeOptionNames.Length
                ? EnemyTypeOptionNames[index]
                : string.Empty;
        }

        public static string GetEnemyTypeDisplayName(EnemyTypeArchetype archetype)
        {
            int index = (int)archetype;
            return index >= 0 && index < EnemyTypeDisplayNames.Length
                ? EnemyTypeDisplayNames[index]
                : "Unknown";
        }

        public static string GetEnemyTypeToken(EnemyTypeArchetype archetype)
        {
            int index = (int)archetype;
            return index >= 0 && index < EnemyTypeTokens.Length
                ? EnemyTypeTokens[index]
                : "unknown";
        }

        public static bool IsCasterArchetype(EnemyTypeArchetype archetype)
        {
            return archetype == EnemyTypeArchetype.Mage;
        }

        public static int EnemyTypeArchetypeCount()
        {
            return EnemyTypeDisplayNames.Length;
        }

        public static string GetEnemyTypeFallbackOptionName()
        {
            return OptionEnemyTypeUncertainFallback;
        }

        public static string GetEnemyTypeFallbackMode()
        {
            return NormalizeEnemyTypeFallbackMode(EnemyTypeUncertainFallbackMode);
        }

        public static bool ShouldSkipUncertainEnemyTypes()
        {
            return string.Equals(GetEnemyTypeFallbackMode(), EnemyTypeFallbackSkip, StringComparison.Ordinal);
        }

        public static string GetEnemyTypeFallbackDisplayLabel()
        {
            return ShouldSkipUncertainEnemyTypes() ? "Skip Enemy" : "Treat As Melee";
        }

        // Backward-compatible aliases used by older logging/sync code paths.
        public static string GetEnemyTypeCasterOptionName()
        {
            return GetEnemyTypeOptionName(EnemyTypeArchetype.Mage);
        }

        public static string GetEnemyTypeNonCasterOptionName()
        {
            return GetEnemyTypeOptionName(EnemyTypeArchetype.Melee);
        }

        public static string GetFactionShortName(int factionIndex)
        {
            int i = ToFactionIndex(factionIndex);
            return i >= 0 ? FactionShortNames[i] : "Unknown";
        }

        public static int GetResolvedFactionId(int factionIndex)
        {
            int i = ToFactionIndex(factionIndex);
            if (i < 0)
            {
                return -1;
            }

            EnsureResolvedFactionIds();
            return resolvedFactionIds[i];
        }

        public static string GetFactionEnabledOptionName(int factionIndex)
        {
            int i = ToFactionIndex(factionIndex);
            return i >= 0 ? FactionEnabledOptionNames[i] : string.Empty;
        }

        public static string GetFactionSlotSpellOptionName(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            return (i >= 0 && s >= 0) ? FactionSpellOptionNames[i, s] : string.Empty;
        }

        public static string GetFactionSlotChanceOptionName(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            return (i >= 0 && s >= 0) ? FactionChanceOptionNames[i, s] : string.Empty;
        }

        public static string GetFactionSlotStrengthOptionName(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            return (i >= 0 && s >= 0) ? FactionStrengthOptionNames[i, s] : string.Empty;
        }

        public static bool GetFactionEnabled(int factionIndex)
        {
            int i = ToFactionIndex(factionIndex);
            return i >= 0 && ReadBoolField(FactionEnabledFields[i], true);
        }

        public static bool SetFactionEnabled(int factionIndex, bool value)
        {
            int i = ToFactionIndex(factionIndex);
            return i >= 0 && SetBoolField(FactionEnabledFields[i], value);
        }

        public static string GetFactionSlotSpell(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return string.Empty;
            }

            return CanonicalSpellId(ReadStringField(FactionSpellFields[i, s], string.Empty));
        }

        public static bool SetFactionSlotSpell(int factionIndex, int slotIndex, string spellId)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return false;
            }

            return SetStringField(FactionSpellFields[i, s], CanonicalSpellId(spellId));
        }

        public static float GetFactionSlotChance(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return 0f;
            }

            return ClampPercent(ReadFloatField(FactionChanceFields[i, s], 0f));
        }

        public static bool SetFactionSlotChance(int factionIndex, int slotIndex, float chancePercent)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return false;
            }

            return SetFloatField(FactionChanceFields[i, s], ClampPercent(chancePercent));
        }

        public static float GetFactionSlotStrength(int factionIndex, int slotIndex)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return 0f;
            }

            return ClampPercent(ReadFloatField(FactionStrengthFields[i, s], 0f));
        }

        public static bool SetFactionSlotStrength(int factionIndex, int slotIndex, float strengthPercent)
        {
            int i = ToFactionIndex(factionIndex);
            int s = ToSlotIndex(slotIndex);
            if (i < 0 || s < 0)
            {
                return false;
            }

            return SetFloatField(FactionStrengthFields[i, s], ClampPercent(strengthPercent));
        }

        public static bool NormalizeAllFactionChanceValues()
        {
            bool changed = false;
            for (int faction = 1; faction <= FactionCount; faction++)
            {
                changed |= NormalizeFactionChanceValues(faction);
            }

            return changed;
        }

        public static bool NormalizeFactionChanceValues(int factionIndex)
        {
            float c1 = GetFactionSlotChance(factionIndex, 1);
            float c2 = GetFactionSlotChance(factionIndex, 2);
            float c3 = GetFactionSlotChance(factionIndex, 3);

            NormalizeChanceTriplet(ref c1, ref c2, ref c3);

            bool changed = false;
            changed |= SetFactionSlotChance(factionIndex, 1, c1);
            changed |= SetFactionSlotChance(factionIndex, 2, c2);
            changed |= SetFactionSlotChance(factionIndex, 3, c3);
            return changed;
        }

        public static int GetPresetSelectionHash()
        {
            int hash = 17;
            hash = CombineHash(hash, StringHash(NormalizeFactionProfilePreset(PresetFactionProfile)));
            hash = CombineHash(hash, StringHash(NormalizeEnemyTypeProfilePreset(PresetEnemyTypeProfile)));
            hash = CombineHash(hash, StringHash(NormalizeImbuePreset(PresetImbue)));
            hash = CombineHash(hash, StringHash(NormalizeChancePreset(PresetChance)));
            hash = CombineHash(hash, StringHash(NormalizeStrengthPreset(PresetStrength)));
            return hash;
        }

        public static int GetChanceStateHash()
        {
            int hash = 19;
            for (int faction = 1; faction <= FactionCount; faction++)
            {
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    hash = CombineHash(hash, PercentHash(GetFactionSlotChance(faction, slot)));
                }
            }
            return hash;
        }

        public static int GetOptionsStateHash()
        {
            int hash = 23;
            hash = CombineHash(hash, EnableMod ? 1 : 0);
            hash = CombineHash(hash, EnableBasicLogging ? 1 : 0);
            hash = CombineHash(hash, EnableDiagnosticsLogging ? 1 : 0);
            hash = CombineHash(hash, EnableVerboseLogging ? 1 : 0);
            hash = CombineHash(hash, PercentHash(UpdateInterval * 100f));
            hash = CombineHash(hash, PercentHash(RescanInterval * 100f));

            for (int i = 0; i < EnemyTypeArchetypeCount(); i++)
            {
                hash = CombineHash(hash, GetEnemyTypeEligibility((EnemyTypeArchetype)i) ? 1 : 0);
            }
            hash = CombineHash(hash, StringHash(GetEnemyTypeFallbackMode()));

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                hash = CombineHash(hash, GetFactionEnabled(faction) ? 1 : 0);
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    hash = CombineHash(hash, StringHash(GetFactionSlotSpell(faction, slot)));
                    hash = CombineHash(hash, PercentHash(GetFactionSlotChance(faction, slot)));
                    hash = CombineHash(hash, PercentHash(GetFactionSlotStrength(faction, slot)));
                }
            }

            return hash;
        }

        /// <summary>
        /// Hash of runtime assignment inputs only (values that affect roll/apply behavior).
        /// Excludes diagnostics/UI-only options to avoid unnecessary rerolls.
        /// </summary>
        public static int GetAssignmentStateHash()
        {
            int hash = 29;
            hash = CombineHash(hash, EnableMod ? 1 : 0);
            for (int i = 0; i < EnemyTypeArchetypeCount(); i++)
            {
                hash = CombineHash(hash, GetEnemyTypeEligibility((EnemyTypeArchetype)i) ? 1 : 0);
            }
            hash = CombineHash(hash, StringHash(GetEnemyTypeFallbackMode()));

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                hash = CombineHash(hash, GetFactionEnabled(faction) ? 1 : 0);
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    hash = CombineHash(hash, StringHash(GetFactionSlotSpell(faction, slot)));
                    hash = CombineHash(hash, PercentHash(GetFactionSlotChance(faction, slot)));
                    hash = CombineHash(hash, PercentHash(GetFactionSlotStrength(faction, slot)));
                }
            }

            return hash;
        }

        public static bool ApplySelectedPresets()
        {
            return ApplyPresets(
                NormalizeFactionProfilePreset(PresetFactionProfile),
                NormalizeEnemyTypeProfilePreset(PresetEnemyTypeProfile),
                NormalizeImbuePreset(PresetImbue),
                NormalizeChancePreset(PresetChance),
                NormalizeStrengthPreset(PresetStrength));
        }

        public static bool ApplyPresets(string imbuePreset, string chancePreset, string strengthPreset)
        {
            return ApplyPresets(PresetFactionProfile, PresetEnemyTypeProfile, imbuePreset, chancePreset, strengthPreset);
        }

        public static bool ApplyPresets(string profilePreset, string imbuePreset, string chancePreset, string strengthPreset)
        {
            // Backward compatibility: treat single profile preset as both faction + enemy-type profile.
            return ApplyPresets(profilePreset, profilePreset, imbuePreset, chancePreset, strengthPreset);
        }

        public static bool ApplyPresets(string factionProfilePreset, string enemyTypeProfilePreset, string imbuePreset, string chancePreset, string strengthPreset)
        {
            string normalizedFactionProfilePreset = NormalizeFactionProfilePreset(factionProfilePreset);
            string normalizedEnemyTypeProfilePreset = NormalizeEnemyTypeProfilePreset(enemyTypeProfilePreset);
            string normalizedImbuePreset = NormalizeImbuePreset(imbuePreset);
            string normalizedChancePreset = NormalizeChancePreset(chancePreset);
            string normalizedStrengthPreset = NormalizeStrengthPreset(strengthPreset);

            bool changed = false;
            changed |= ApplyFactionProfilePreset(normalizedFactionProfilePreset);
            changed |= ApplyEnemyTypeProfilePreset(normalizedEnemyTypeProfilePreset);
            changed |= ApplyImbuePreset(normalizedImbuePreset);
            changed |= ApplyChancePreset(normalizedChancePreset);
            changed |= ApplyStrengthPreset(normalizedStrengthPreset);
            return changed;
        }

        public static bool ApplyFactionProfilePreset(string profilePreset)
        {
            bool[] enabled = BuildFactionProfilePresetEnabledArray(profilePreset);
            bool changed = false;

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                changed |= SetFactionEnabled(faction, enabled[faction - 1]);
            }

            return changed;
        }

        public static bool ApplyEnemyTypeProfilePreset(string enemyTypeProfilePreset)
        {
            bool[] values = BuildEnemyTypeProfileEnabledArray(enemyTypeProfilePreset);
            bool changed = false;
            changed |= SetEnemyTypeEligibility(
                mageEligible: values[(int)EnemyTypeArchetype.Mage],
                bowEligible: values[(int)EnemyTypeArchetype.Bow],
                meleeEligible: values[(int)EnemyTypeArchetype.Melee]);
            return changed;
        }

        public static bool ApplyImbuePreset(string imbuePreset)
        {
            string[][] matrix = BuildImbuePresetMatrix(NormalizeImbuePreset(imbuePreset));
            bool changed = false;

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    changed |= SetFactionSlotSpell(faction, slot, matrix[faction - 1][slot - 1]);
                }
            }

            return changed;
        }

        public static bool ApplyChancePreset(string chancePreset)
        {
            float[][] matrix = BuildChancePresetMatrix(NormalizeChancePreset(chancePreset));
            bool changed = false;

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    changed |= SetFactionSlotChance(faction, slot, matrix[faction - 1][slot - 1]);
                }
            }

            changed |= NormalizeAllFactionChanceValues();
            return changed;
        }

        public static bool ApplyStrengthPreset(string strengthPreset)
        {
            float[][] matrix = BuildStrengthPresetMatrix(NormalizeStrengthPreset(strengthPreset));
            bool changed = false;

            for (int faction = 1; faction <= FactionCount; faction++)
            {
                for (int slot = 1; slot <= ImbueSlotsPerFaction; slot++)
                {
                    changed |= SetFactionSlotStrength(faction, slot, matrix[faction - 1][slot - 1]);
                }
            }

            return changed;
        }

        public static IEnumerable<FactionProfile> GetAllFactionProfiles()
        {
            for (int i = 1; i <= FactionCount; i++)
            {
                yield return GetFactionProfileByIndex(i);
            }
        }

        public static FactionProfile GetFactionProfileByIndex(int factionIndex)
        {
            int index = ToFactionIndex(factionIndex);
            if (index < 0)
            {
                return default(FactionProfile);
            }

            EnsureResolvedFactionIds();

            bool enabled = GetFactionEnabled(factionIndex);
            string spell1 = GetFactionSlotSpell(factionIndex, 1);
            string spell2 = GetFactionSlotSpell(factionIndex, 2);
            string spell3 = GetFactionSlotSpell(factionIndex, 3);
            float chance1 = GetFactionSlotChance(factionIndex, 1);
            float chance2 = GetFactionSlotChance(factionIndex, 2);
            float chance3 = GetFactionSlotChance(factionIndex, 3);
            float strength1 = GetFactionSlotStrength(factionIndex, 1);
            float strength2 = GetFactionSlotStrength(factionIndex, 2);
            float strength3 = GetFactionSlotStrength(factionIndex, 3);

            NormalizeChanceTriplet(ref chance1, ref chance2, ref chance3);

            ImbueSlotConfig slot1 = BuildSlotConfig(1, spell1, chance1, strength1);
            ImbueSlotConfig slot2 = BuildSlotConfig(2, spell2, chance2, strength2);
            ImbueSlotConfig slot3 = BuildSlotConfig(3, spell3, chance3, strength3);

            float totalChance = slot1.ChancePercent + slot2.ChancePercent + slot3.ChancePercent;

            int hash = 31;
            hash = CombineHash(hash, factionIndex);
            hash = CombineHash(hash, resolvedFactionIds[index]);
            hash = CombineHash(hash, enabled ? 1 : 0);
            hash = CombineHash(hash, SlotHash(slot1));
            hash = CombineHash(hash, SlotHash(slot2));
            hash = CombineHash(hash, SlotHash(slot3));

            return new FactionProfile
            {
                FactionIndex = factionIndex,
                FactionId = resolvedFactionIds[index],
                Enabled = enabled,
                Slot1 = slot1,
                Slot2 = slot2,
                Slot3 = slot3,
                TotalNormalizedChancePercent = totalChance,
                ProfileHash = hash
            };
        }

        public static bool TryResolveFactionProfile(int factionId, out FactionProfile profile)
        {
            EnsureResolvedFactionIds();

            int exact = FindFactionIndexForId(factionId);
            if (exact >= 0)
            {
                profile = GetFactionProfileByIndex(exact + 1);
                return true;
            }

            profile = GetFactionProfileByIndex(FactionCount);
            return true;
        }

        public static int FindFactionIndexForId(int factionId)
        {
            EnsureResolvedFactionIds();
            return FactionIdToIndex.TryGetValue(factionId, out int index) ? index : -1;
        }

        public static string GetFactionName(int factionId, string fallbackName = null)
        {
            if (factionId < 0)
            {
                return "Any Enemy";
            }

            List<GameData.Faction> factions = Catalog.gameData?.factions;
            if (factions != null)
            {
                for (int i = 0; i < factions.Count; i++)
                {
                    GameData.Faction faction = factions[i];
                    if (faction != null && faction.id == factionId && !string.IsNullOrWhiteSpace(faction.name))
                    {
                        return faction.name;
                    }
                }
            }

            int index = FindFactionIndexForId(factionId);
            if (index >= 0)
            {
                return FactionDisplayNames[index];
            }

            return string.IsNullOrWhiteSpace(fallbackName) ? "Faction " + factionId : fallbackName;
        }

        public static bool SetEnemyTypeEligibility(bool mageEligible, bool bowEligible, bool meleeEligible)
        {
            bool changed = false;
            if (EnemyTypeMageEligible != mageEligible)
            {
                EnemyTypeMageEligible = mageEligible;
                changed = true;
            }
            if (EnemyTypeBowEligible != bowEligible)
            {
                EnemyTypeBowEligible = bowEligible;
                changed = true;
            }
            if (EnemyTypeMeleeEligible != meleeEligible)
            {
                EnemyTypeMeleeEligible = meleeEligible;
                changed = true;
            }
            return changed;
        }

        public static bool GetEnemyTypeEligibility(EnemyTypeArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyTypeArchetype.Mage:
                    return EnemyTypeMageEligible;
                case EnemyTypeArchetype.Bow:
                    return EnemyTypeBowEligible;
                case EnemyTypeArchetype.Melee:
                    return EnemyTypeMeleeEligible;
                default:
                    return false;
            }
        }

        public static bool IsEnemyTypeEligible(EnemyTypeArchetype archetype)
        {
            return GetEnemyTypeEligibility(archetype);
        }

        public static bool IsAnyCasterEnemyTypeEligible()
        {
            return EnemyTypeMageEligible;
        }

        public static bool IsAnyNonCasterEnemyTypeEligible()
        {
            return EnemyTypeBowEligible || EnemyTypeMeleeEligible;
        }

        public static string GetEnemyTypeEligibilityModeLabel()
        {
            if (EnemyTypeMageEligible && !EnemyTypeBowEligible && !EnemyTypeMeleeEligible)
            {
                return "casters_only";
            }

            if (EnemyTypeMageEligible && EnemyTypeBowEligible && !EnemyTypeMeleeEligible)
            {
                return "ranged";
            }

            if (EnemyTypeMageEligible && EnemyTypeBowEligible && EnemyTypeMeleeEligible)
            {
                return "all";
            }

            if (!EnemyTypeMageEligible && IsAnyNonCasterEnemyTypeEligible())
            {
                return "noncasters_only";
            }

            if (EnemyTypeMageEligible || IsAnyNonCasterEnemyTypeEligible())
            {
                return "mixed";
            }

            return "none";
        }

        public static string GetEnemyTypeEligibilitySummary()
        {
            return
                "mage=" + EnemyTypeMageEligible +
                ", bow=" + EnemyTypeBowEligible +
                ", melee=" + EnemyTypeMeleeEligible +
                ", uncertainFallback=" + GetEnemyTypeFallbackMode();
        }

        public static ModOptionString[] EnemyTypeFallbackProvider()
        {
            return new[]
            {
                new ModOptionString("Treat As Melee", EnemyTypeFallbackMelee),
                new ModOptionString("Skip Enemy", EnemyTypeFallbackSkip)
            };
        }

        public static ModOptionFloat[] PercentProvider()
        {
            ModOptionFloat[] values = new ModOptionFloat[21];
            for (int i = 0; i < values.Length; i++)
            {
                float percent = i * 5f;
                values[i] = new ModOptionFloat(percent.ToString("F0") + "%", percent);
            }
            return values;
        }

        public static ModOptionFloat[] UpdateIntervalProvider()
        {
            return new[]
            {
                new ModOptionFloat("0.05s", 0.05f),
                new ModOptionFloat("0.10s", 0.10f),
                new ModOptionFloat("0.15s", 0.15f),
                new ModOptionFloat("0.20s", 0.20f),
                new ModOptionFloat("0.25s", 0.25f),
                new ModOptionFloat("0.33s", 0.33f),
                new ModOptionFloat("0.50s", 0.50f),
                new ModOptionFloat("0.75s", 0.75f),
                new ModOptionFloat("1.00s", 1.00f)
            };
        }

        public static ModOptionFloat[] RescanIntervalProvider()
        {
            return new[]
            {
                new ModOptionFloat("0.50s", 0.50f),
                new ModOptionFloat("1.00s", 1.00f),
                new ModOptionFloat("1.50s", 1.50f),
                new ModOptionFloat("2.00s", 2.00f),
                new ModOptionFloat("3.00s", 3.00f),
                new ModOptionFloat("5.00s", 5.00f)
            };
        }

        public static ModOptionString[] SpellProvider()
        {
            List<ModOptionString> values = new List<ModOptionString>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            values.Add(new ModOptionString("None (No Imbue)", SpellNone));

            AddSpellOption(values, seen, "Fire");
            AddSpellOption(values, seen, "Lightning");
            AddSpellOption(values, seen, "Gravity");

            List<string> discovered = new List<string>();
            try
            {
                List<SpellCastCharge> spells = Catalog.GetDataList<SpellCastCharge>();
                if (spells != null)
                {
                    for (int i = 0; i < spells.Count; i++)
                    {
                        string id = spells[i]?.id;
                        if (string.IsNullOrWhiteSpace(id))
                        {
                            continue;
                        }

                        string canonical = CanonicalSpellId(id);
                        if (!seen.Contains(canonical))
                        {
                            discovered.Add(canonical);
                        }
                    }
                }
            }
            catch
            {
                // Catalog may not be fully initialized while options are built.
            }

            discovered.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < discovered.Count; i++)
            {
                AddSpellOption(values, seen, discovered[i]);
            }

            if (values.Count == 0)
            {
                values.Add(new ModOptionString("Fire", "Fire"));
            }

            return values.ToArray();
        }

        public static ModOptionString[] FactionProfilePresetProvider()
        {
            return new[]
            {
                new ModOptionString("Default", PresetProfileLore),
                new ModOptionString("Core Factions", PresetProfileFrontier),
                new ModOptionString("Most Factions", PresetProfileWarfront),
                new ModOptionString("All Factions", PresetProfileHighMagic),
                new ModOptionString("Random", PresetProfileRandom)
            };
        }

        public static ModOptionString[] EnemyTypeProfilePresetProvider()
        {
            return new[]
            {
                new ModOptionString("Casters", PresetEnemyTypeMageOnly),
                new ModOptionString("Ranged", PresetEnemyTypeRanged),
                new ModOptionString("All", PresetEnemyTypeAll)
            };
        }

        public static ModOptionString[] ProfilePresetProvider()
        {
            // Backward-compatible alias for older references.
            return FactionProfilePresetProvider();
        }

        public static ModOptionString[] ImbuePresetProvider()
        {
            return new[]
            {
                new ModOptionString("Default", PresetImbueLore),
                new ModOptionString("Two-Slot", PresetImbueFactionIdentity),
                new ModOptionString("Tri-Slot", PresetImbueArcaneSurge),
                new ModOptionString("Tri-Slot+", PresetImbueElementalChaos),
                new ModOptionString("Random", PresetImbueRandomized)
            };
        }

        public static ModOptionString[] ChancePresetProvider()
        {
            return new[]
            {
                new ModOptionString("Default", PresetChanceLow),
                new ModOptionString("Increased", PresetChanceBalanced),
                new ModOptionString("High", PresetChanceAggressive),
                new ModOptionString("Very High", PresetChanceRelentless),
                new ModOptionString("Maximum", PresetChanceOverflow)
            };
        }

        public static ModOptionString[] StrengthPresetProvider()
        {
            return new[]
            {
                new ModOptionString("Default", PresetStrengthFaint),
                new ModOptionString("Increased", PresetStrengthStandard),
                new ModOptionString("High", PresetStrengthEmpowered),
                new ModOptionString("Very High", PresetStrengthOvercharged),
                new ModOptionString("Maximum", PresetStrengthCataclysmic)
            };
        }

        public static string NormalizeFactionProfilePreset(string preset)
        {
            if (string.Equals(preset, PresetProfileLore, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Lore Friendly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return PresetProfileLore;
            }

            if (string.Equals(preset, PresetProfileFrontier, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Frontier Pressure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Core Factions", StringComparison.OrdinalIgnoreCase))
            {
                return PresetProfileFrontier;
            }

            if (string.Equals(preset, PresetProfileWarfront, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Warfront Arcana", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Most Factions", StringComparison.OrdinalIgnoreCase))
            {
                return PresetProfileWarfront;
            }

            if (string.Equals(preset, PresetProfileHighMagic, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High Magic Conflict", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "All Factions", StringComparison.OrdinalIgnoreCase))
            {
                return PresetProfileHighMagic;
            }

            if (string.Equals(preset, PresetProfileRandom, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Randomized Eligibility", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Randomized", StringComparison.OrdinalIgnoreCase))
            {
                return PresetProfileRandom;
            }

            return PresetProfileLore;
        }

        public static string NormalizeEnemyTypeProfilePreset(string preset)
        {
            if (string.IsNullOrWhiteSpace(preset))
            {
                return PresetEnemyTypeMageOnly;
            }

            string trimmed = preset.Trim();

            if (string.Equals(trimmed, PresetEnemyTypeMageOnly, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, PresetProfileLore, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Lore Friendly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Default", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Mage", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Mage Only", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Casters", StringComparison.OrdinalIgnoreCase))
            {
                return PresetEnemyTypeMageOnly;
            }
            if (string.Equals(trimmed, PresetEnemyTypeRanged, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, PresetProfileFrontier, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Frontier Pressure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Mage Bow", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Open", StringComparison.OrdinalIgnoreCase))
            {
                return PresetEnemyTypeRanged;
            }
            if (string.Equals(trimmed, PresetEnemyTypeAll, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, PresetProfileWarfront, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Warfront Arcana", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Mage Melee", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Open+", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, PresetProfileHighMagic, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("High Magic Conflict", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Mage Bow Melee", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Open++", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "All", StringComparison.OrdinalIgnoreCase))
            {
                return PresetEnemyTypeAll;
            }
            if (string.Equals(trimmed, PresetProfileRandom, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Randomized", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Random", StringComparison.OrdinalIgnoreCase))
            {
                return PresetEnemyTypeAll;
            }

            return PresetEnemyTypeMageOnly;
        }

        public static string NormalizeEnemyTypeFallbackMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return EnemyTypeFallbackMelee;
            }

            string trimmed = mode.Trim();
            if (string.Equals(trimmed, EnemyTypeFallbackSkip, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Skip Enemy", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Skip", StringComparison.OrdinalIgnoreCase))
            {
                return EnemyTypeFallbackSkip;
            }

            if (string.Equals(trimmed, EnemyTypeFallbackMelee, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Treat As Melee", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Fallback Melee", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return EnemyTypeFallbackMelee;
            }

            return EnemyTypeFallbackMelee;
        }

        public static string NormalizeProfilePreset(string preset)
        {
            // Backward-compatible alias for older callers.
            return NormalizeFactionProfilePreset(preset);
        }

        public static bool IsLoreFriendlyProfileSelected()
        {
            return string.Equals(
                NormalizeEnemyTypeProfilePreset(PresetEnemyTypeProfile),
                PresetEnemyTypeMageOnly,
                StringComparison.Ordinal);
        }

        public static string NormalizeImbuePreset(string preset)
        {
            if (string.Equals(preset, PresetImbueLore, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Lore Accurate", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Lore Friendly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return PresetImbueLore;
            }

            if (string.Equals(preset, PresetImbueFactionIdentity, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Faction Identity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Frontier Doctrines", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Less Lore", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Two-Slot", StringComparison.OrdinalIgnoreCase))
            {
                return PresetImbueFactionIdentity;
            }

            if (string.Equals(preset, PresetImbueArcaneSurge, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Arcane Surge", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Warfront Expansion", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Tri-Slot", StringComparison.OrdinalIgnoreCase))
            {
                return PresetImbueArcaneSurge;
            }

            if (string.Equals(preset, PresetImbueElementalChaos, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Elemental Chaos", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High Magic Arsenal", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Tri-Slot+", StringComparison.OrdinalIgnoreCase))
            {
                return PresetImbueElementalChaos;
            }

            if (string.Equals(preset, PresetImbueRandomized, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Randomized", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Random", StringComparison.OrdinalIgnoreCase))
            {
                return PresetImbueRandomized;
            }

            return PresetImbueLore;
        }

        public static string NormalizeChancePreset(string preset)
        {
            if (string.Equals(preset, PresetChanceLow, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Low Intensity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Lore Friendly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return PresetChanceLow;
            }

            if (string.Equals(preset, PresetChanceBalanced, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Balanced Battles", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Frontier Pressure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Increased", StringComparison.OrdinalIgnoreCase))
            {
                return PresetChanceBalanced;
            }

            if (string.Equals(preset, PresetChanceAggressive, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Aggressive Waves", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Warfront Arcana", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High", StringComparison.OrdinalIgnoreCase))
            {
                return PresetChanceAggressive;
            }

            if (string.Equals(preset, PresetChanceRelentless, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Relentless Threat", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High Magic Conflict", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Very High", StringComparison.OrdinalIgnoreCase))
            {
                return PresetChanceRelentless;
            }

            if (string.Equals(preset, PresetChanceOverflow, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Overflow Normalized", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Chaotic Storm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Maximum", StringComparison.OrdinalIgnoreCase))
            {
                return PresetChanceOverflow;
            }

            return PresetChanceBalanced;
        }

        public static string NormalizeStrengthPreset(string preset)
        {
            if (string.Equals(preset, PresetStrengthFaint, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Faint Charge", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Lore Friendly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return PresetStrengthFaint;
            }

            if (string.Equals(preset, PresetStrengthStandard, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Battle Ready", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Frontier Pressure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Increased", StringComparison.OrdinalIgnoreCase))
            {
                return PresetStrengthStandard;
            }

            if (string.Equals(preset, PresetStrengthEmpowered, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Empowered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Warfront Arcana", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High", StringComparison.OrdinalIgnoreCase))
            {
                return PresetStrengthEmpowered;
            }

            if (string.Equals(preset, PresetStrengthOvercharged, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Overcharged", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "High Magic Conflict", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Very High", StringComparison.OrdinalIgnoreCase))
            {
                return PresetStrengthOvercharged;
            }

            if (string.Equals(preset, PresetStrengthCataclysmic, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Cataclysmic", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Chaotic Storm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset, "Maximum", StringComparison.OrdinalIgnoreCase))
            {
                return PresetStrengthCataclysmic;
            }

            return PresetStrengthStandard;
        }

        private static void NormalizeChanceTriplet(ref float c1, ref float c2, ref float c3)
        {
            c1 = ClampPercent(c1);
            c2 = ClampPercent(c2);
            c3 = ClampPercent(c3);

            float sum = c1 + c2 + c3;
            if (sum > 100f && sum > 0.0001f)
            {
                float scale = 100f / sum;
                c1 *= scale;
                c2 *= scale;
                c3 *= scale;
            }
        }

        private static bool[] BuildFactionProfilePresetEnabledArray(string preset)
        {
            string normalized = NormalizeFactionProfilePreset(preset);

            if (string.Equals(normalized, PresetProfileFrontier, StringComparison.Ordinal))
            {
                return CopyBoolArray(ProfileEnabledValues[1]);
            }

            if (string.Equals(normalized, PresetProfileWarfront, StringComparison.Ordinal))
            {
                return CopyBoolArray(ProfileEnabledValues[2]);
            }

            if (string.Equals(normalized, PresetProfileHighMagic, StringComparison.Ordinal))
            {
                return CopyBoolArray(ProfileEnabledValues[3]);
            }

            if (string.Equals(normalized, PresetProfileRandom, StringComparison.Ordinal))
            {
                return BuildRandomProfileEnabledArray();
            }

            return CopyBoolArray(ProfileEnabledValues[0]);
        }

        private static bool[] BuildEnemyTypeProfileEnabledArray(string preset)
        {
            string normalized = NormalizeEnemyTypeProfilePreset(preset);

            if (string.Equals(normalized, PresetEnemyTypeRanged, StringComparison.Ordinal))
            {
                return CopyEnemyTypeArray(ProfileEnemyTypeValues[1]);
            }

            if (string.Equals(normalized, PresetEnemyTypeAll, StringComparison.Ordinal))
            {
                return CopyEnemyTypeArray(ProfileEnemyTypeValues[2]);
            }

            return CopyEnemyTypeArray(ProfileEnemyTypeValues[0]);
        }

        private static bool[] CopyEnemyTypeArray(bool[] source)
        {
            bool[] copy = new bool[EnemyTypeArchetypeCount()];
            if (source == null)
            {
                return copy;
            }

            int length = Mathf.Min(copy.Length, source.Length);
            for (int i = 0; i < length; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static string[][] BuildImbuePresetMatrix(string preset)
        {
            string normalized = NormalizeImbuePreset(preset);

            if (string.Equals(normalized, PresetImbueFactionIdentity, StringComparison.Ordinal))
            {
                return CopySpellMatrix(ImbueFrontierValues);
            }

            if (string.Equals(normalized, PresetImbueArcaneSurge, StringComparison.Ordinal))
            {
                return CopySpellMatrix(ImbueWarfrontValues);
            }

            if (string.Equals(normalized, PresetImbueElementalChaos, StringComparison.Ordinal))
            {
                return CopySpellMatrix(ImbueHighMagicValues);
            }

            if (string.Equals(normalized, PresetImbueRandomized, StringComparison.Ordinal))
            {
                return BuildRandomImbueMatrix();
            }

            return CopySpellMatrix(ImbueLoreValues);
        }

        private static float[][] BuildChancePresetMatrix(string preset)
        {
            string normalized = NormalizeChancePreset(preset);
            float[][] matrix;

            if (string.Equals(normalized, PresetChanceBalanced, StringComparison.Ordinal))
            {
                matrix = CopyFloatMatrix(ChanceFrontierValues);
            }
            else if (string.Equals(normalized, PresetChanceAggressive, StringComparison.Ordinal))
            {
                matrix = CopyFloatMatrix(ChanceWarfrontValues);
            }
            else if (string.Equals(normalized, PresetChanceRelentless, StringComparison.Ordinal))
            {
                matrix = CopyFloatMatrix(ChanceHighMagicValues);
            }
            else if (string.Equals(normalized, PresetChanceOverflow, StringComparison.Ordinal))
            {
                matrix = CopyFloatMatrix(ChanceRandomValues);
            }
            else
            {
                matrix = CopyFloatMatrix(ChanceLoreValues);
            }

            // Keep per-faction chance sum capped at 100%.
            for (int faction = 0; faction < FactionCount; faction++)
            {
                float c1 = matrix[faction][0];
                float c2 = matrix[faction][1];
                float c3 = matrix[faction][2];
                NormalizeChanceTriplet(ref c1, ref c2, ref c3);
                matrix[faction][0] = c1;
                matrix[faction][1] = c2;
                matrix[faction][2] = c3;
            }

            return matrix;
        }

        private static float[][] BuildStrengthPresetMatrix(string preset)
        {
            string normalized = NormalizeStrengthPreset(preset);
            if (string.Equals(normalized, PresetStrengthStandard, StringComparison.Ordinal))
            {
                return CopyFloatMatrix(StrengthFrontierValues);
            }
            if (string.Equals(normalized, PresetStrengthEmpowered, StringComparison.Ordinal))
            {
                return CopyFloatMatrix(StrengthWarfrontValues);
            }
            if (string.Equals(normalized, PresetStrengthOvercharged, StringComparison.Ordinal))
            {
                return CopyFloatMatrix(StrengthHighMagicValues);
            }
            if (string.Equals(normalized, PresetStrengthCataclysmic, StringComparison.Ordinal))
            {
                return CopyFloatMatrix(StrengthRandomValues);
            }

            return CopyFloatMatrix(StrengthLoreValues);
        }

        private static ImbueSlotConfig BuildSlotConfig(int slotIndex, string spellId, float chancePercent, float strengthPercent)
        {
            return new ImbueSlotConfig
            {
                SlotIndex = slotIndex,
                SpellId = CanonicalSpellId(spellId),
                ChancePercent = ClampPercent(chancePercent),
                StrengthPercent = ClampPercent(strengthPercent)
            };
        }

        private static int SlotHash(ImbueSlotConfig slot)
        {
            int hash = 37;
            hash = CombineHash(hash, slot.SlotIndex);
            hash = CombineHash(hash, StringHash(slot.SpellId));
            hash = CombineHash(hash, PercentHash(slot.ChancePercent));
            hash = CombineHash(hash, PercentHash(slot.StrengthPercent));
            return hash;
        }

        private static bool[] CopyBoolArray(bool[] source)
        {
            bool[] copy = new bool[FactionCount];
            if (source == null)
            {
                return copy;
            }

            int length = Mathf.Min(FactionCount, source.Length);
            for (int i = 0; i < length; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static string[][] CopySpellMatrix(string[][] source)
        {
            string[][] matrix = CreateSpellMatrix();
            if (source == null)
            {
                return matrix;
            }

            int factionCount = Mathf.Min(FactionCount, source.Length);
            for (int faction = 0; faction < factionCount; faction++)
            {
                if (source[faction] == null)
                {
                    continue;
                }

                int slotCount = Mathf.Min(ImbueSlotsPerFaction, source[faction].Length);
                for (int slot = 0; slot < slotCount; slot++)
                {
                    matrix[faction][slot] = CanonicalSpellId(source[faction][slot]);
                }
            }

            return matrix;
        }

        private static float[][] CopyFloatMatrix(float[][] source)
        {
            float[][] matrix = CreateFloatMatrix();
            if (source == null)
            {
                return matrix;
            }

            int factionCount = Mathf.Min(FactionCount, source.Length);
            for (int faction = 0; faction < factionCount; faction++)
            {
                if (source[faction] == null)
                {
                    continue;
                }

                int slotCount = Mathf.Min(ImbueSlotsPerFaction, source[faction].Length);
                for (int slot = 0; slot < slotCount; slot++)
                {
                    matrix[faction][slot] = ClampPercent(source[faction][slot]);
                }
            }

            return matrix;
        }

        private static string[][] BuildRandomImbueMatrix()
        {
            string[][] matrix = CreateSpellMatrix();
            for (int faction = 0; faction < FactionCount; faction++)
            {
                for (int slot = 0; slot < ImbueSlotsPerFaction; slot++)
                {
                    matrix[faction][slot] = BaseSpellPool[presetRandom.Next(0, BaseSpellPool.Length)];
                }
            }

            return matrix;
        }

        private static bool[] BuildRandomProfileEnabledArray()
        {
            bool[] enabled = new bool[FactionCount];
            int enabledCount = 0;

            for (int i = 0; i < FactionCount; i++)
            {
                bool allow = presetRandom.NextDouble() >= 0.35;
                enabled[i] = allow;
                if (allow)
                {
                    enabledCount++;
                }
            }

            if (enabledCount == 0)
            {
                enabled[FactionCount - 1] = true;
            }

            return enabled;
        }

        private static bool[] BuildRandomEnemyTypeEnabledArray()
        {
            bool[] values = new bool[EnemyTypeArchetypeCount()];
            int enabledCount = 0;

            for (int i = 0; i < values.Length; i++)
            {
                bool enabled = presetRandom.NextDouble() >= 0.35;
                values[i] = enabled;
                if (enabled)
                {
                    enabledCount++;
                }
            }

            if (enabledCount == 0)
            {
                values[(int)EnemyTypeArchetype.Mage] = true;
            }

            return values;
        }

        private static string[][] CreateSpellMatrix()
        {
            string[][] matrix = new string[FactionCount][];
            for (int i = 0; i < FactionCount; i++)
            {
                matrix[i] = new string[ImbueSlotsPerFaction];
            }
            return matrix;
        }

        private static float[][] CreateFloatMatrix()
        {
            float[][] matrix = new float[FactionCount][];
            for (int i = 0; i < FactionCount; i++)
            {
                matrix[i] = new float[ImbueSlotsPerFaction];
            }
            return matrix;
        }

        private static string NextSpell(string spell, int offset)
        {
            string canonical = CanonicalSpellId(spell);
            int index = 0;

            for (int i = 0; i < BaseSpellPool.Length; i++)
            {
                if (BaseSpellPool[i].Equals(canonical, StringComparison.Ordinal))
                {
                    index = i;
                    break;
                }
            }

            int next = (index + offset) % BaseSpellPool.Length;
            return BaseSpellPool[next];
        }

        private static void EnsureResolvedFactionIds()
        {
            List<GameData.Faction> factions = Catalog.gameData?.factions;
            if (factions == null || factions.Count == 0)
            {
                if (resolvedFactionIds == null || resolvedFactionIds.Length != FactionCount)
                {
                    resolvedFactionIds = (int[])DefaultFactionIds.Clone();
                    RebuildFactionIndexMap();
                }
                resolvedFromCatalog = false;
                resolvedFactionCount = -1;
                return;
            }

            if (resolvedFromCatalog &&
                resolvedFactionIds != null &&
                resolvedFactionIds.Length == FactionCount &&
                resolvedFactionCount == factions.Count)
            {
                return;
            }

            int[] resolved = (int[])DefaultFactionIds.Clone();
            for (int i = 0; i < FactionCount - 1; i++)
            {
                resolved[i] = ResolveFactionId(factions, DefaultFactionIds[i], FactionDisplayNames[i], FactionKeywords[i]);
            }
            resolved[FactionCount - 1] = -1;

            resolvedFactionIds = resolved;
            resolvedFromCatalog = true;
            resolvedFactionCount = factions.Count;
            RebuildFactionIndexMap();
        }

        private static int ResolveFactionId(List<GameData.Faction> factions, int fallbackId, string displayName, string[] keywords)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                GameData.Faction faction = factions[i];
                if (faction != null && faction.id == fallbackId)
                {
                    return faction.id;
                }
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                for (int i = 0; i < factions.Count; i++)
                {
                    GameData.Faction faction = factions[i];
                    if (faction == null || string.IsNullOrWhiteSpace(faction.name))
                    {
                        continue;
                    }

                    if (faction.name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return faction.id;
                    }
                }
            }

            if (keywords != null)
            {
                for (int i = 0; i < factions.Count; i++)
                {
                    GameData.Faction faction = factions[i];
                    if (faction == null || string.IsNullOrWhiteSpace(faction.name))
                    {
                        continue;
                    }

                    string name = faction.name.ToLowerInvariant();
                    for (int k = 0; k < keywords.Length; k++)
                    {
                        string keyword = keywords[k];
                        if (!string.IsNullOrWhiteSpace(keyword) && name.Contains(keyword))
                        {
                            return faction.id;
                        }
                    }
                }
            }

            return fallbackId;
        }

        private static void RebuildFactionIndexMap()
        {
            FactionIdToIndex.Clear();
            if (resolvedFactionIds == null)
            {
                return;
            }

            for (int i = 0; i < resolvedFactionIds.Length; i++)
            {
                int factionId = resolvedFactionIds[i];
                if (factionId >= 0 && !FactionIdToIndex.ContainsKey(factionId))
                {
                    FactionIdToIndex.Add(factionId, i);
                }
            }
        }

        private static FieldInfo FindField(string fieldName)
        {
            return typeof(EIPModOptions).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static string ReadConstString(string fieldName, string fallback)
        {
            FieldInfo field = typeof(EIPModOptions).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(null);
            return value as string ?? fallback;
        }

        private static bool ReadBoolField(FieldInfo field, bool fallback)
        {
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(null);
            return value is bool b ? b : fallback;
        }

        private static float ReadFloatField(FieldInfo field, float fallback)
        {
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(null);
            if (value is float f)
            {
                return f;
            }
            if (value is double d)
            {
                return (float)d;
            }
            if (value is int n)
            {
                return n;
            }

            return fallback;
        }

        private static string ReadStringField(FieldInfo field, string fallback)
        {
            if (field == null)
            {
                return fallback;
            }

            object value = field.GetValue(null);
            return value as string ?? fallback;
        }

        private static bool SetBoolField(FieldInfo field, bool value)
        {
            if (field == null)
            {
                return false;
            }

            bool current = ReadBoolField(field, value);
            if (current == value)
            {
                return false;
            }

            field.SetValue(null, value);
            return true;
        }

        private static bool SetFloatField(FieldInfo field, float value)
        {
            if (field == null)
            {
                return false;
            }

            float current = ReadFloatField(field, value);
            if (Mathf.Abs(current - value) < 0.0001f)
            {
                return false;
            }

            field.SetValue(null, value);
            return true;
        }

        private static bool SetStringField(FieldInfo field, string value)
        {
            if (field == null)
            {
                return false;
            }

            string next = value ?? string.Empty;
            string current = ReadStringField(field, string.Empty);
            if (string.Equals(current, next, StringComparison.Ordinal))
            {
                return false;
            }

            field.SetValue(null, next);
            return true;
        }

        private static int ToFactionIndex(int factionIndex)
        {
            int index = factionIndex - 1;
            return index >= 0 && index < FactionCount ? index : -1;
        }

        private static int ToSlotIndex(int slotIndex)
        {
            int index = slotIndex - 1;
            return index >= 0 && index < ImbueSlotsPerFaction ? index : -1;
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
            return Mathf.RoundToInt(value * 100f);
        }

        private static void AddSpellOption(List<ModOptionString> values, HashSet<string> seen, string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return;
            }

            string canonical = CanonicalSpellId(spellId);
            if (string.IsNullOrWhiteSpace(canonical))
            {
                return;
            }

            if (seen.Add(canonical))
            {
                values.Add(new ModOptionString(canonical, canonical));
            }
        }
    }
}


