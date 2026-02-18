using System;
using System.Collections.Generic;
using ImbuementOverhaul.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    internal sealed class DurationModOptionSync
    {
        private const string OptionKeySeparator = "||";
        private const float UpdateIntervalSeconds = 0.15f;

        public static DurationModOptionSync Instance { get; } = new DurationModOptionSync();

        private readonly Dictionary<string, ModOption> modOptionsByKey = new Dictionary<string, ModOption>(StringComparer.Ordinal);

        private ModManager.ModData modData;
        private bool initialized;
        private float nextUpdateTime;
        private int lastPresetHash;

        private DurationModOptionSync()
        {
        }

        public void Initialize()
        {
            initialized = false;
            nextUpdateTime = 0f;
            lastPresetHash = int.MinValue;
            modData = null;
            modOptionsByKey.Clear();

            TryInitialize();
            if (!initialized)
            {
                return;
            }

            lastPresetHash = DurationModOptions.GetPresetSelectionHash();
        }

        public void Shutdown()
        {
            initialized = false;
            modData = null;
            modOptionsByKey.Clear();
            lastPresetHash = int.MinValue;
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

                lastPresetHash = DurationModOptions.GetPresetSelectionHash();
                return;
            }

            float now = Time.unscaledTime;
            if (now < nextUpdateTime)
            {
                return;
            }

            nextUpdateTime = now + UpdateIntervalSeconds;
            bool changed = false;

            changed |= ApplyPresetsIfChanged(force: false);

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

            if (!ModManager.TryGetModData(System.Reflection.Assembly.GetExecutingAssembly(), out modData))
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
            int presetHash = DurationModOptions.GetPresetSelectionHash();
            if (!force && presetHash == lastPresetHash)
            {
                return false;
            }

            string durationPreset = DurationModOptions.NormalizeDurationPreset(DurationModOptions.PresetDurationExperience);
            string contextPreset = DurationModOptions.NormalizeContextPreset(DurationModOptions.PresetContextProfile);

            bool valuesChanged = DurationModOptions.ApplySelectedPresets();
            bool uiChanged = SyncSourceOfTruthOptions();

            lastPresetHash = DurationModOptions.GetPresetSelectionHash();

            if (force || valuesChanged || uiChanged)
            {
                DurationLog.Info(
                    "Preset batch wrote source-of-truth collapsibles: duration=" + durationPreset +
                    " context=" + contextPreset +
                    " valuesChanged=" + valuesChanged +
                    " uiSynced=" + uiChanged +
                    " snapshot={" + DurationModOptions.GetSourceOfTruthSummary() + "}",
                    verboseOnly: !valuesChanged && !uiChanged);
            }

            return valuesChanged || uiChanged;
        }

        private bool SyncSourceOfTruthOptions()
        {
            bool changed = false;
            changed |= SyncFloatOption(DurationModOptions.CategoryGlobal, DurationModOptions.OptionGlobalDrainMultiplier, DurationModOptions.GlobalDrainMultiplier);
            changed |= SyncFloatOption(DurationModOptions.CategoryPlayerHeld, DurationModOptions.OptionPlayerHeldDrainMultiplier, DurationModOptions.PlayerHeldDrainMultiplier);
            changed |= SyncFloatOption(DurationModOptions.CategoryPlayerThrown, DurationModOptions.OptionPlayerThrownDrainMultiplier, DurationModOptions.PlayerThrownDrainMultiplier);
            changed |= SyncFloatOption(DurationModOptions.CategoryNpcHeld, DurationModOptions.OptionNpcHeldDrainMultiplier, DurationModOptions.NpcHeldDrainMultiplier);
            changed |= SyncFloatOption(DurationModOptions.CategoryNpcThrown, DurationModOptions.OptionNpcThrownDrainMultiplier, DurationModOptions.NpcThrownDrainMultiplier);
            changed |= SyncFloatOption(DurationModOptions.CategoryWorld, DurationModOptions.OptionWorldDrainMultiplier, DurationModOptions.WorldDrainMultiplier);
            return changed;
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

            RefreshOptionCache();
            return modOptionsByKey.TryGetValue(key, out option);
        }

        private void RefreshOptionCache()
        {
            modOptionsByKey.Clear();
            if (modData?.modOptions == null)
            {
                return;
            }

            foreach (ModOption option in modData.modOptions)
            {
                if (option == null || string.IsNullOrWhiteSpace(option.name))
                {
                    continue;
                }

                modOptionsByKey[MakeKey(option.category, option.name)] = option;
            }
        }

        private static string MakeKey(string category, string name)
        {
            return (category ?? string.Empty) + OptionKeySeparator + (name ?? string.Empty);
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
    }
}

