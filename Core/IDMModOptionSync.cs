using System;
using System.Collections.Generic;
using ImbueDurationManager.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbueDurationManager.Core
{
    internal sealed class IDMModOptionSync
    {
        private const string OptionKeySeparator = "||";
        private const float UpdateIntervalSeconds = 0.15f;

        public static IDMModOptionSync Instance { get; } = new IDMModOptionSync();

        private readonly Dictionary<string, ModOption> modOptionsByKey = new Dictionary<string, ModOption>(StringComparer.Ordinal);

        private ModManager.ModData modData;
        private bool initialized;
        private float nextUpdateTime;
        private int lastPresetHash;

        private IDMModOptionSync()
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

            lastPresetHash = IDMModOptions.GetPresetSelectionHash();
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

                lastPresetHash = IDMModOptions.GetPresetSelectionHash();
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
            int presetHash = IDMModOptions.GetPresetSelectionHash();
            if (!force && presetHash == lastPresetHash)
            {
                return false;
            }

            string durationPreset = IDMModOptions.NormalizeDurationPreset(IDMModOptions.PresetDurationExperience);
            string contextPreset = IDMModOptions.NormalizeContextPreset(IDMModOptions.PresetContextProfile);

            bool valuesChanged = IDMModOptions.ApplySelectedPresets();
            bool uiChanged = SyncSourceOfTruthOptions();

            lastPresetHash = IDMModOptions.GetPresetSelectionHash();

            if (force || valuesChanged || uiChanged)
            {
                IDMLog.Info(
                    "Preset batch wrote source-of-truth collapsibles: duration=" + durationPreset +
                    " context=" + contextPreset +
                    " valuesChanged=" + valuesChanged +
                    " uiSynced=" + uiChanged +
                    " snapshot={" + IDMModOptions.GetSourceOfTruthSummary() + "}",
                    verboseOnly: !valuesChanged && !uiChanged);
            }

            return valuesChanged || uiChanged;
        }

        private bool SyncSourceOfTruthOptions()
        {
            bool changed = false;
            changed |= SyncFloatOption(IDMModOptions.CategoryGlobal, IDMModOptions.OptionGlobalDrainMultiplier, IDMModOptions.GlobalDrainMultiplier);
            changed |= SyncFloatOption(IDMModOptions.CategoryPlayerHeld, IDMModOptions.OptionPlayerHeldDrainMultiplier, IDMModOptions.PlayerHeldDrainMultiplier);
            changed |= SyncFloatOption(IDMModOptions.CategoryPlayerThrown, IDMModOptions.OptionPlayerThrownDrainMultiplier, IDMModOptions.PlayerThrownDrainMultiplier);
            changed |= SyncFloatOption(IDMModOptions.CategoryNpcHeld, IDMModOptions.OptionNpcHeldDrainMultiplier, IDMModOptions.NpcHeldDrainMultiplier);
            changed |= SyncFloatOption(IDMModOptions.CategoryNpcThrown, IDMModOptions.OptionNpcThrownDrainMultiplier, IDMModOptions.NpcThrownDrainMultiplier);
            changed |= SyncFloatOption(IDMModOptions.CategoryWorld, IDMModOptions.OptionWorldDrainMultiplier, IDMModOptions.WorldDrainMultiplier);
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
