using System;
using System.Collections.Generic;
using System.Reflection;
using EnemyImbuePresets.Configuration;
using ThunderRoad;
using UnityEngine;

namespace EnemyImbuePresets.Core
{
    public sealed class EIPModOptionSync
    {
        private const string OptionKeySeparator = "||";
        private const float UpdateIntervalSeconds = 0.15f;
        private const float StartupOverwriteWatchSeconds = 8f;

        private ModManager.ModData modData;
        private bool initialized;
        private float nextUpdateTime;
        private int lastPresetHash;
        private int lastChanceHash;
        private int lastOptionCatalogHash;
        private bool startupOverwriteWatchActive;
        private float startupOverwriteWatchEndTime;
        private int startupOptionsStateHash;

        private readonly Dictionary<string, ModOption> modOptionsByKey = new Dictionary<string, ModOption>(StringComparer.Ordinal);

        public static EIPModOptionSync Instance { get; } = new EIPModOptionSync();

        private EIPModOptionSync()
        {
        }

        public void Initialize()
        {
            initialized = false;
            nextUpdateTime = 0f;
            lastPresetHash = int.MinValue;
            lastChanceHash = int.MinValue;
            lastOptionCatalogHash = int.MinValue;
            startupOverwriteWatchActive = false;
            startupOverwriteWatchEndTime = 0f;
            startupOptionsStateHash = int.MinValue;
            modData = null;
            modOptionsByKey.Clear();

            TryInitialize();
            if (!initialized)
            {
                return;
            }

            bool changed = false;
            changed |= ApplyPresetsIfChanged(force: true);
            changed |= NormalizeChanceFields(force: true);

            if (changed)
            {
                ModManager.RefreshModOptionsUI();
            }

            // Mod options can apply after ScriptEnable and clobber faction collapsibles.
            // Watch startup for one overwrite and re-apply current presets once if detected.
            startupOptionsStateHash = EIPModOptions.GetOptionsStateHash();
            startupOverwriteWatchActive = true;
            startupOverwriteWatchEndTime = Time.unscaledTime + StartupOverwriteWatchSeconds;
        }

        public void Shutdown()
        {
            initialized = false;
            modData = null;
            modOptionsByKey.Clear();
            lastPresetHash = int.MinValue;
            lastChanceHash = int.MinValue;
            lastOptionCatalogHash = int.MinValue;
            startupOverwriteWatchActive = false;
            startupOverwriteWatchEndTime = 0f;
            startupOptionsStateHash = int.MinValue;
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

                bool initChanged = false;
                initChanged |= ApplyPresetsIfChanged(force: true);
                initChanged |= NormalizeChanceFields(force: true);
                if (initChanged)
                {
                    ModManager.RefreshModOptionsUI();
                }
                return;
            }

            float now = Time.unscaledTime;
            if (now < nextUpdateTime)
            {
                return;
            }

            nextUpdateTime = now + UpdateIntervalSeconds;
            bool changed = false;
            changed |= ReapplyPresetsIfStartupOverwriteDetected(now);

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

        private bool ReapplyPresetsIfStartupOverwriteDetected(float now)
        {
            if (!startupOverwriteWatchActive)
            {
                return false;
            }

            if (now > startupOverwriteWatchEndTime)
            {
                startupOverwriteWatchActive = false;
                return false;
            }

            int currentOptionsHash = EIPModOptions.GetOptionsStateHash();
            if (currentOptionsHash == startupOptionsStateHash)
            {
                return false;
            }

            bool changed = false;
            changed |= ApplyPresetsIfChanged(force: true);
            changed |= NormalizeChanceFields(force: true);

            startupOptionsStateHash = EIPModOptions.GetOptionsStateHash();
            startupOverwriteWatchActive = false;

            EIPLog.Info("Detected post-load mod-option overwrite; re-applied selected presets once.");
            return changed;
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

        private bool ApplyPresetsIfChanged(bool force)
        {
            int presetHash = EIPModOptions.GetPresetSelectionHash();
            if (!force && presetHash == lastPresetHash)
            {
                return false;
            }

            string factionProfilePreset = EIPModOptions.NormalizeFactionProfilePreset(EIPModOptions.PresetFactionProfile);
            string enemyTypeProfilePreset = EIPModOptions.NormalizeEnemyTypeProfilePreset(EIPModOptions.PresetEnemyTypeProfile);
            string imbuePreset = EIPModOptions.NormalizeImbuePreset(EIPModOptions.PresetImbue);
            string chancePreset = EIPModOptions.NormalizeChancePreset(EIPModOptions.PresetChance);
            string strengthPreset = EIPModOptions.NormalizeStrengthPreset(EIPModOptions.PresetStrength);

            bool valuesChanged = EIPModOptions.ApplySelectedPresets();
            bool uiChanged = SyncAllFactionOptions();

            lastPresetHash = EIPModOptions.GetPresetSelectionHash();
            lastChanceHash = EIPModOptions.GetChanceStateHash();

            LogPresetBatchWrite(factionProfilePreset, enemyTypeProfilePreset, imbuePreset, chancePreset, strengthPreset, valuesChanged, uiChanged, force);
            return valuesChanged || uiChanged;
        }

        private bool NormalizeChanceFields(bool force)
        {
            int chanceHashBefore = EIPModOptions.GetChanceStateHash();
            if (!force && chanceHashBefore == lastChanceHash)
            {
                return false;
            }

            bool valuesChanged = EIPModOptions.NormalizeAllFactionChanceValues();
            int chanceHashAfter = EIPModOptions.GetChanceStateHash();
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

            for (int faction = 1; faction <= EIPModOptions.FactionCount; faction++)
            {
                string category = EIPModOptions.GetFactionCategory(faction);
                if (string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                changed |= SyncBoolOption(category, EIPModOptions.GetFactionEnabledOptionName(faction), EIPModOptions.GetFactionEnabled(faction));

                for (int slot = 1; slot <= EIPModOptions.ImbueSlotsPerFaction; slot++)
                {
                    changed |= SyncStringOption(category, EIPModOptions.GetFactionSlotSpellOptionName(faction, slot), EIPModOptions.GetFactionSlotSpell(faction, slot));
                    changed |= SyncFloatOption(category, EIPModOptions.GetFactionSlotChanceOptionName(faction, slot), EIPModOptions.GetFactionSlotChance(faction, slot));
                    changed |= SyncFloatOption(category, EIPModOptions.GetFactionSlotStrengthOptionName(faction, slot), EIPModOptions.GetFactionSlotStrength(faction, slot));
                }
            }

            string enemyTypeCategory = EIPModOptions.GetEnemyTypeCategory();
            if (!string.IsNullOrWhiteSpace(enemyTypeCategory))
            {
                changed |= SyncBoolOption(enemyTypeCategory, EIPModOptions.GetEnemyTypeCasterOptionName(), EIPModOptions.EnemyTypeCasterEligible);
                changed |= SyncBoolOption(enemyTypeCategory, EIPModOptions.GetEnemyTypeNonCasterOptionName(), EIPModOptions.EnemyTypeNonCasterEligible);
            }

            return changed;
        }

        private bool SyncAllChanceOptions()
        {
            bool changed = false;

            for (int faction = 1; faction <= EIPModOptions.FactionCount; faction++)
            {
                string category = EIPModOptions.GetFactionCategory(faction);
                if (string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                for (int slot = 1; slot <= EIPModOptions.ImbueSlotsPerFaction; slot++)
                {
                    changed |= SyncFloatOption(category, EIPModOptions.GetFactionSlotChanceOptionName(faction, slot), EIPModOptions.GetFactionSlotChance(faction, slot));
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
            EIPLog.Info(
                "Preset batch wrote faction collapsible values: " +
                "factionProfile=" + factionProfilePreset +
                ", enemyTypeProfile=" + enemyTypeProfilePreset +
                ", " +
                "imbue=" + imbuePreset +
                ", chance=" + chancePreset +
                ", strength=" + strengthPreset +
                ", casterEligible=" + EIPModOptions.EnemyTypeCasterEligible +
                ", nonCasterEligible=" + EIPModOptions.EnemyTypeNonCasterEligible +
                ", valuesChanged=" + valuesChanged +
                ", uiSynced=" + uiChanged +
                ", force=" + force);

            for (int faction = 1; faction <= EIPModOptions.FactionCount; faction++)
            {
                bool enabled = EIPModOptions.GetFactionEnabled(faction);
                string shortName = EIPModOptions.GetFactionShortName(faction);

                string s1Spell = EIPModOptions.GetFactionSlotSpell(faction, 1);
                float s1Chance = EIPModOptions.GetFactionSlotChance(faction, 1);
                float s1Strength = EIPModOptions.GetFactionSlotStrength(faction, 1);

                string s2Spell = EIPModOptions.GetFactionSlotSpell(faction, 2);
                float s2Chance = EIPModOptions.GetFactionSlotChance(faction, 2);
                float s2Strength = EIPModOptions.GetFactionSlotStrength(faction, 2);

                string s3Spell = EIPModOptions.GetFactionSlotSpell(faction, 3);
                float s3Chance = EIPModOptions.GetFactionSlotChance(faction, 3);
                float s3Strength = EIPModOptions.GetFactionSlotStrength(faction, 3);

                EIPLog.Info(
                    "Preset write " + shortName +
                    " enabled=" + enabled +
                    " | S1 " + s1Spell + " " + s1Chance.ToString("F1") + "%/" + s1Strength.ToString("F1") + "%" +
                    " | S2 " + s2Spell + " " + s2Chance.ToString("F1") + "%/" + s2Strength.ToString("F1") + "%" +
                    " | S3 " + s3Spell + " " + s3Chance.ToString("F1") + "%/" + s3Strength.ToString("F1") + "%",
                    verboseOnly: true);
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
