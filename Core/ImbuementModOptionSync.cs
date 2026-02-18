using System;
using System.Collections.Generic;
using System.Reflection;
using ImbuementOverhaul.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    public sealed class ImbuementModOptionSync
    {
        private const string OptionKeySeparator = "||";
        private const float UpdateIntervalSeconds = 0.15f;

        private ModManager.ModData modData;
        private bool initialized;
        private float nextUpdateTime;
        private int lastPresetHash;
        private int lastChanceHash;
        private int lastOptionCatalogHash;
        private static string lastEligibilityHintSignature = string.Empty;

        private readonly Dictionary<string, ModOption> modOptionsByKey = new Dictionary<string, ModOption>(StringComparer.Ordinal);

        public static ImbuementModOptionSync Instance { get; } = new ImbuementModOptionSync();

        private ImbuementModOptionSync()
        {
        }

        public void Initialize()
        {
            initialized = false;
            nextUpdateTime = 0f;
            lastPresetHash = int.MinValue;
            lastChanceHash = int.MinValue;
            lastOptionCatalogHash = int.MinValue;
            lastEligibilityHintSignature = string.Empty;
            modData = null;
            modOptionsByKey.Clear();

            TryInitialize();
            if (!initialized)
            {
                return;
            }

            SeedTrackingState();
        }

        public void Shutdown()
        {
            initialized = false;
            modData = null;
            modOptionsByKey.Clear();
            lastPresetHash = int.MinValue;
            lastChanceHash = int.MinValue;
            lastOptionCatalogHash = int.MinValue;
            lastEligibilityHintSignature = string.Empty;
        }

        public void Update()
        {
            if (!initialized)
            {
                TryInitialize();
                if (!initialized)
                {
                    return;
                }

                SeedTrackingState();
                return;
            }

            float now = Time.unscaledTime;
            if (now < nextUpdateTime)
            {
                return;
            }

            nextUpdateTime = now + UpdateIntervalSeconds;
            bool changed = false;

            bool optionCatalogChanged = RefreshOptionCache();
            changed |= ApplyPresetsIfChanged(force: false);
            changed |= NormalizeChanceFields(force: false);
            if (optionCatalogChanged)
            {
                changed |= SyncAllFactionOptions();
            }

            if (changed)
            {
                ModManager.RefreshModOptionsUI();
            }
        }

        private void TryInitialize()
        {
            if (initialized)
            {
                return;
            }

            if (!ModManager.TryGetModData(Assembly.GetExecutingAssembly(), out modData))
            {
                return;
            }
            if (modData?.modOptions == null || modData.modOptions.Count == 0)
            {
                return;
            }

            RefreshOptionCache();
            initialized = true;
        }

        private void SeedTrackingState()
        {
            lastPresetHash = ImbuementModOptions.GetPresetSelectionHash();
            lastChanceHash = ImbuementModOptions.GetChanceStateHash();
        }

        private bool ApplyPresetsIfChanged(bool force)
        {
            int presetHash = ImbuementModOptions.GetPresetSelectionHash();
            if (!force && presetHash == lastPresetHash)
            {
                return false;
            }

            string factionProfilePreset = ImbuementModOptions.NormalizeFactionProfilePreset(ImbuementModOptions.PresetFactionProfile);
            string enemyTypeProfilePreset = ImbuementModOptions.NormalizeEnemyTypeProfilePreset(ImbuementModOptions.PresetEnemyTypeProfile);
            string imbuePreset = ImbuementModOptions.NormalizeImbuePreset(ImbuementModOptions.PresetImbue);
            string chancePreset = ImbuementModOptions.NormalizeChancePreset(ImbuementModOptions.PresetChance);
            string strengthPreset = ImbuementModOptions.NormalizeStrengthPreset(ImbuementModOptions.PresetStrength);

            bool valuesChanged = ImbuementModOptions.ApplySelectedPresets();
            bool uiChanged = SyncAllFactionOptions();

            lastPresetHash = ImbuementModOptions.GetPresetSelectionHash();
            lastChanceHash = ImbuementModOptions.GetChanceStateHash();

            if (force || valuesChanged || uiChanged)
            {
                LogPresetBatchWrite(factionProfilePreset, enemyTypeProfilePreset, imbuePreset, chancePreset, strengthPreset, valuesChanged, uiChanged, force);
            }
            return valuesChanged || uiChanged;
        }

        private bool NormalizeChanceFields(bool force)
        {
            int chanceHashBefore = ImbuementModOptions.GetChanceStateHash();
            if (!force && chanceHashBefore == lastChanceHash)
            {
                return false;
            }

            bool valuesChanged = ImbuementModOptions.NormalizeAllFactionChanceValues();
            int chanceHashAfter = ImbuementModOptions.GetChanceStateHash();
            lastChanceHash = chanceHashAfter;

            if (!valuesChanged)
            {
                return false;
            }

            bool uiChanged = SyncAllChanceOptions();
            return valuesChanged || uiChanged;
        }

        private bool SyncAllFactionOptions()
        {
            bool changed = false;

            for (int faction = 1; faction <= ImbuementModOptions.FactionCount; faction++)
            {
                string category = ImbuementModOptions.GetFactionCategory(faction);
                if (string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                changed |= SyncBoolOption(category, ImbuementModOptions.GetFactionEnabledOptionName(faction), ImbuementModOptions.GetFactionEnabled(faction));

                for (int slot = 1; slot <= ImbuementModOptions.ImbueSlotsPerFaction; slot++)
                {
                    changed |= SyncStringOption(category, ImbuementModOptions.GetFactionSlotSpellOptionName(faction, slot), ImbuementModOptions.GetFactionSlotSpell(faction, slot));
                    changed |= SyncFloatOption(category, ImbuementModOptions.GetFactionSlotChanceOptionName(faction, slot), ImbuementModOptions.GetFactionSlotChance(faction, slot));
                    changed |= SyncFloatOption(category, ImbuementModOptions.GetFactionSlotStrengthOptionName(faction, slot), ImbuementModOptions.GetFactionSlotStrength(faction, slot));
                    changed |= SyncFloatOption(category, ImbuementModOptions.GetFactionSlotDrainMultiplierOptionName(faction, slot), ImbuementModOptions.GetFactionSlotDrainMultiplier(faction, slot));
                }
            }

            string enemyTypeCategory = ImbuementModOptions.GetEnemyTypeCategory();
            if (!string.IsNullOrWhiteSpace(enemyTypeCategory))
            {
                for (int i = 0; i < ImbuementModOptions.EnemyTypeArchetypeCount(); i++)
                {
                    ImbuementModOptions.EnemyTypeArchetype archetype = (ImbuementModOptions.EnemyTypeArchetype)i;
                    changed |= SyncBoolOption(
                        enemyTypeCategory,
                        ImbuementModOptions.GetEnemyTypeOptionName(archetype),
                        ImbuementModOptions.GetEnemyTypeEligibility(archetype));
                }

                changed |= SyncStringOption(
                    enemyTypeCategory,
                    ImbuementModOptions.GetEnemyTypeFallbackOptionName(),
                    ImbuementModOptions.GetEnemyTypeFallbackMode());
            }

            return changed;
        }

        private bool SyncAllChanceOptions()
        {
            bool changed = false;

            for (int faction = 1; faction <= ImbuementModOptions.FactionCount; faction++)
            {
                string category = ImbuementModOptions.GetFactionCategory(faction);
                if (string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                for (int slot = 1; slot <= ImbuementModOptions.ImbueSlotsPerFaction; slot++)
                {
                    changed |= SyncFloatOption(category, ImbuementModOptions.GetFactionSlotChanceOptionName(faction, slot), ImbuementModOptions.GetFactionSlotChance(faction, slot));
                }
            }

            return changed;
        }

        private static string MakeKey(string category, string name)
        {
            return (category ?? string.Empty) + OptionKeySeparator + (name ?? string.Empty);
        }

        private static void LogPresetBatchWrite(
            string factionProfilePreset,
            string enemyTypeProfilePreset,
            string imbuePreset,
            string chancePreset,
            string strengthPreset,
            bool valuesChanged,
            bool uiChanged,
            bool force)
        {
            if (!force && !valuesChanged && !uiChanged)
            {
                return;
            }

            if (ImbuementLog.DiagnosticsEnabled)
            {
                ImbuementLog.Info(
                    "Preset batch wrote faction collapsible values: " +
                    "factionProfile=" + factionProfilePreset +
                    ", enemyTypeProfile=" + enemyTypeProfilePreset +
                    ", " +
                    "imbue=" + imbuePreset +
                    ", chance=" + chancePreset +
                    ", strength=" + strengthPreset +
                    ", enemyTypes={" + ImbuementModOptions.GetEnemyTypeEligibilitySummary() + "}" +
                    ", eligibilityMode=" + GetEligibilityModeLabel() +
                    ", valuesChanged=" + valuesChanged +
                    ", uiSynced=" + uiChanged +
                    ", force=" + force);
            }

            ImbuementLog.Info(
                "Preset explain: factionProfile=" + FriendlyFactionProfileLabel(factionProfilePreset) +
                ", enemyTypeProfile=" + FriendlyEnemyTypeProfileLabel(enemyTypeProfilePreset) +
                ", imbue=" + FriendlyImbueLabel(imbuePreset) +
                ", chance=" + FriendlyChanceLabel(chancePreset) +
                ", strength=" + FriendlyStrengthLabel(strengthPreset) +
                ", enemyTypes={" + ImbuementModOptions.GetEnemyTypeEligibilitySummary() + "}",
                verboseOnly: true);

            LogEnemyTypeEligibilityHint(enemyTypeProfilePreset);

            for (int faction = 1; faction <= ImbuementModOptions.FactionCount; faction++)
            {
                bool enabled = ImbuementModOptions.GetFactionEnabled(faction);
                string shortName = ImbuementModOptions.GetFactionShortName(faction);

                string s1Spell = ImbuementModOptions.GetFactionSlotSpell(faction, 1);
                float s1Chance = ImbuementModOptions.GetFactionSlotChance(faction, 1);
                float s1Strength = ImbuementModOptions.GetFactionSlotStrength(faction, 1);
                float s1Drain = ImbuementModOptions.GetFactionSlotDrainMultiplier(faction, 1);

                string s2Spell = ImbuementModOptions.GetFactionSlotSpell(faction, 2);
                float s2Chance = ImbuementModOptions.GetFactionSlotChance(faction, 2);
                float s2Strength = ImbuementModOptions.GetFactionSlotStrength(faction, 2);
                float s2Drain = ImbuementModOptions.GetFactionSlotDrainMultiplier(faction, 2);

                string s3Spell = ImbuementModOptions.GetFactionSlotSpell(faction, 3);
                float s3Chance = ImbuementModOptions.GetFactionSlotChance(faction, 3);
                float s3Strength = ImbuementModOptions.GetFactionSlotStrength(faction, 3);
                float s3Drain = ImbuementModOptions.GetFactionSlotDrainMultiplier(faction, 3);

                ImbuementLog.Info(
                    "Preset write " + shortName +
                    " enabled=" + enabled +
                    " | S1 " + s1Spell + " " + s1Chance.ToString("F1") + "%/" + s1Strength.ToString("F1") + "% drain=" + s1Drain.ToString("0.00") + "x" +
                    " | S2 " + s2Spell + " " + s2Chance.ToString("F1") + "%/" + s2Strength.ToString("F1") + "% drain=" + s2Drain.ToString("0.00") + "x" +
                    " | S3 " + s3Spell + " " + s3Chance.ToString("F1") + "%/" + s3Strength.ToString("F1") + "% drain=" + s3Drain.ToString("0.00") + "x",
                    verboseOnly: true);
            }
        }

        private static string GetEligibilityModeLabel()
        {
            return ImbuementModOptions.GetEnemyTypeEligibilityModeLabel();
        }

        private static void LogEnemyTypeEligibilityHint(string enemyTypeProfilePreset)
        {
            string mode = GetEligibilityModeLabel();
            if (!string.Equals(mode, "casters_only", StringComparison.Ordinal))
            {
                return;
            }

            string signature = (enemyTypeProfilePreset ?? string.Empty).Trim() + "|" + mode;
            if (string.Equals(signature, lastEligibilityHintSignature, StringComparison.Ordinal))
            {
                return;
            }

            lastEligibilityHintSignature = signature;

            if (ImbuementLog.DiagnosticsEnabled)
            {
                ImbuementLog.Info(
                    "diag evt=eligibility_hint enemyTypeProfile=" + enemyTypeProfilePreset +
                    " mode=" + mode +
                    " note=non_caster_enemy_types_will_be_skipped");
            }
        }

        private static string FriendlyFactionProfileLabel(string preset)
        {
            switch (ImbuementModOptions.NormalizeFactionProfilePreset(preset))
            {
                case ImbuementModOptions.PresetProfileFrontier: return "Core Factions";
                case ImbuementModOptions.PresetProfileWarfront: return "Most Factions";
                case ImbuementModOptions.PresetProfileHighMagic: return "All Factions";
                case ImbuementModOptions.PresetProfileRandom: return "Random";
                default: return "Default";
            }
        }

        private static string FriendlyEnemyTypeProfileLabel(string preset)
        {
            switch (ImbuementModOptions.NormalizeEnemyTypeProfilePreset(preset))
            {
                case ImbuementModOptions.PresetEnemyTypeRanged: return "Ranged";
                case ImbuementModOptions.PresetEnemyTypeAll: return "All";
                default: return "Casters";
            }
        }

        private static string FriendlyImbueLabel(string preset)
        {
            switch (ImbuementModOptions.NormalizeImbuePreset(preset))
            {
                case ImbuementModOptions.PresetImbueFactionIdentity: return "Two-Slot";
                case ImbuementModOptions.PresetImbueArcaneSurge: return "Tri-Slot";
                case ImbuementModOptions.PresetImbueElementalChaos: return "Tri-Slot+";
                case ImbuementModOptions.PresetImbueRandomized: return "Random";
                default: return "Default";
            }
        }

        private static string FriendlyChanceLabel(string preset)
        {
            switch (ImbuementModOptions.NormalizeChancePreset(preset))
            {
                case ImbuementModOptions.PresetChanceBalanced: return "Increased";
                case ImbuementModOptions.PresetChanceAggressive: return "High";
                case ImbuementModOptions.PresetChanceRelentless: return "Very High";
                case ImbuementModOptions.PresetChanceOverflow: return "Maximum";
                default: return "Default";
            }
        }

        private static string FriendlyStrengthLabel(string preset)
        {
            switch (ImbuementModOptions.NormalizeStrengthPreset(preset))
            {
                case ImbuementModOptions.PresetStrengthStandard: return "Increased";
                case ImbuementModOptions.PresetStrengthEmpowered: return "High";
                case ImbuementModOptions.PresetStrengthOvercharged: return "Very High";
                case ImbuementModOptions.PresetStrengthCataclysmic: return "Maximum";
                default: return "Default";
            }
        }

        private bool RefreshOptionCache()
        {
            if (modData?.modOptions == null || modData.modOptions.Count == 0)
            {
                return false;
            }

            var refreshed = new Dictionary<string, ModOption>(StringComparer.Ordinal);
            int catalogHash = 17;

            foreach (ModOption option in modData.modOptions)
            {
                if (option == null || string.IsNullOrEmpty(option.name))
                {
                    continue;
                }

                string key = MakeKey(option.category, option.name);
                refreshed[key] = option;

                unchecked
                {
                    catalogHash = (catalogHash * 397) ^ StringComparer.Ordinal.GetHashCode(key);
                }
            }

            bool changed = catalogHash != lastOptionCatalogHash || refreshed.Count != modOptionsByKey.Count;
            if (!changed)
            {
                foreach (var pair in refreshed)
                {
                    if (!modOptionsByKey.ContainsKey(pair.Key))
                    {
                        changed = true;
                        break;
                    }
                }
            }

            modOptionsByKey.Clear();
            foreach (var pair in refreshed)
            {
                modOptionsByKey[pair.Key] = pair.Value;
            }

            lastOptionCatalogHash = catalogHash;
            return changed;
        }

        private bool SyncBoolOption(string category, string optionName, bool value)
        {
            if (!TryGetOption(category, optionName, out ModOption option))
            {
                return false;
            }

            if (option.parameterValues == null || option.parameterValues.Length == 0)
            {
                option.LoadModOptionParameters();
            }

            int index = FindBoolIndex(option.parameterValues, value);
            if (index < 0 || option.currentValueIndex == index)
            {
                return false;
            }

            option.Apply(index);
            option.RefreshUI();
            return true;
        }

        private bool SyncFloatOption(string category, string optionName, float value)
        {
            if (!TryGetOption(category, optionName, out ModOption option))
            {
                return false;
            }

            if (option.parameterValues == null || option.parameterValues.Length == 0)
            {
                option.LoadModOptionParameters();
            }

            int index = FindFloatIndex(option.parameterValues, value);
            if (index < 0 || option.currentValueIndex == index)
            {
                return false;
            }

            option.Apply(index);
            option.RefreshUI();
            return true;
        }

        private bool SyncStringOption(string category, string optionName, string value)
        {
            if (!TryGetOption(category, optionName, out ModOption option))
            {
                return false;
            }

            if (option.parameterValues == null || option.parameterValues.Length == 0)
            {
                option.LoadModOptionParameters();
            }

            int index = FindStringIndex(option.parameterValues, value ?? string.Empty);
            if (index < 0 || option.currentValueIndex == index)
            {
                return false;
            }

            option.Apply(index);
            option.RefreshUI();
            return true;
        }

        private bool TryGetOption(string category, string optionName, out ModOption option)
        {
            option = null;
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(optionName))
            {
                return false;
            }

            string key = MakeKey(category, optionName);
            if (modOptionsByKey.TryGetValue(key, out option))
            {
                return true;
            }

            // Collapsible categories can populate options lazily, so refresh once and retry.
            if (!RefreshOptionCache())
            {
                return false;
            }

            return modOptionsByKey.TryGetValue(key, out option);
        }

        private static int FindBoolIndex(ModOptionParameter[] parameters, bool value)
        {
            if (parameters == null)
            {
                return -1;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                object parameterValue = parameters[i]?.value;
                if (parameterValue is bool b && b == value)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindFloatIndex(ModOptionParameter[] parameters, float value)
        {
            if (parameters == null)
            {
                return -1;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                object parameterValue = parameters[i]?.value;
                if (parameterValue is float f && Mathf.Abs(f - value) < 0.0001f)
                {
                    return i;
                }
                if (parameterValue is double d && Mathf.Abs((float)d - value) < 0.0001f)
                {
                    return i;
                }
                if (parameterValue is int n && Mathf.Abs(n - value) < 0.0001f)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindStringIndex(ModOptionParameter[] parameters, string value)
        {
            if (parameters == null)
            {
                return -1;
            }

            string expected = value ?? string.Empty;
            string expectedToken = NormalizeLookupToken(expected);

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!(parameters[i]?.value is string optionValue))
                {
                    continue;
                }

                if (string.Equals(optionValue, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                string optionToken = NormalizeLookupToken(optionValue);
                if (string.Equals(optionToken, expectedToken, StringComparison.Ordinal))
                {
                    return i;
                }

                if (string.IsNullOrEmpty(expectedToken) && string.Equals(optionToken, "NONE", StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string NormalizeLookupToken(string value)
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
    }
}


