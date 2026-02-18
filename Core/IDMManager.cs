using System.Collections.Generic;
using ImbueDurationManager.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbueDurationManager.Core
{
    internal sealed class IDMManager
    {
        private struct TrackedImbueState
        {
            public float PreviousEnergy;
            public float PreviousTime;
            public float LastSeenTime;
            public float LastAppliedMultiplier;
        }

        public static IDMManager Instance { get; } = new IDMManager();

        private readonly Dictionary<int, TrackedImbueState> trackedStates = new Dictionary<int, TrackedImbueState>();
        private float nextUpdateTime;
        private bool nativeInfiniteApplied;
        private int stableCyclesWithoutCorrections;
        private float adaptiveIntervalMultiplier = 1f;

        private const int StableCyclesPerBackoffStep = 6;
        private const float AdaptiveIntervalStep = 0.5f;
        private const float MaxAdaptiveIntervalMultiplier = 4f;

        private IDMManager()
        {
        }

        public void Initialize()
        {
            trackedStates.Clear();
            nextUpdateTime = 0f;
            stableCyclesWithoutCorrections = 0;
            adaptiveIntervalMultiplier = 1f;
            SetNativeInfinite(false);
        }

        public void Shutdown()
        {
            trackedStates.Clear();
            nextUpdateTime = 0f;
            stableCyclesWithoutCorrections = 0;
            adaptiveIntervalMultiplier = 1f;
            SetNativeInfinite(false);
        }

        public void Update()
        {
            HandleDiagnosticsToggles();

            if (!IDMModOptions.EnableMod)
            {
                if (nativeInfiniteApplied)
                {
                    SetNativeInfinite(false);
                }

                if (trackedStates.Count > 0)
                {
                    trackedStates.Clear();
                }
                stableCyclesWithoutCorrections = 0;
                adaptiveIntervalMultiplier = 1f;
                return;
            }

            float now = Time.unscaledTime;
            if (now < nextUpdateTime)
            {
                return;
            }

            float interval = IDMModOptions.GetUpdateIntervalSeconds() * adaptiveIntervalMultiplier;
            nextUpdateTime = now + interval;

            bool shouldNativeInfinite = IDMModOptions.ShouldUseNativeInfinite();
            if (shouldNativeInfinite != nativeInfiniteApplied)
            {
                SetNativeInfinite(shouldNativeInfinite);
            }

            ProcessAllActiveImbues(now);
        }

        private void ProcessAllActiveImbues(float now)
        {
            List<Item> activeItems = Item.allActive;
            if (activeItems == null || activeItems.Count == 0)
            {
                trackedStates.Clear();
                UpdateAdaptiveInterval(scannedImbues: 0, corrections: 0);
                return;
            }

            int scannedItems = 0;
            int scannedImbues = 0;
            int adjustedUp = 0;
            int adjustedDown = 0;
            int unchanged = 0;

            for (int i = 0; i < activeItems.Count; i++)
            {
                Item item = activeItems[i];
                if (item == null || item.imbues == null || item.imbues.Count == 0)
                {
                    continue;
                }

                scannedItems++;
                IDMModOptions.DrainContext context = ResolveContext(item);
                float multiplier = IDMModOptions.GetEffectiveDrainMultiplier(context);

                for (int j = 0; j < item.imbues.Count; j++)
                {
                    Imbue imbue = item.imbues[j];
                    if (imbue == null)
                    {
                        continue;
                    }

                    scannedImbues++;
                    int key = imbue.GetInstanceID();
                    float currentEnergy = imbue.energy;

                    if (imbue.maxEnergy <= 0f || imbue.spellCastBase == null || currentEnergy <= 0f)
                    {
                        trackedStates.Remove(key);
                        continue;
                    }

                    if (!trackedStates.TryGetValue(key, out TrackedImbueState state))
                    {
                        trackedStates[key] = new TrackedImbueState
                        {
                            PreviousEnergy = currentEnergy,
                            PreviousTime = now,
                            LastSeenTime = now,
                            LastAppliedMultiplier = multiplier,
                        };
                        unchanged++;
                        continue;
                    }

                    float delta = currentEnergy - state.PreviousEnergy;
                    if (delta < -0.0001f)
                    {
                        float naturalDrain = -delta;
                        float desiredDrain = naturalDrain * multiplier;
                        float targetEnergy = state.PreviousEnergy - desiredDrain;

                        if (multiplier <= 1f)
                        {
                            float floor = imbue.maxEnergy * IDMModOptions.GetMinimumEnergyFloorRatio();
                            targetEnergy = Mathf.Max(targetEnergy, floor);
                        }

                        targetEnergy = Mathf.Clamp(targetEnergy, 0f, imbue.maxEnergy);

                        float maxCorrection = imbue.maxEnergy * IDMModOptions.GetMaxCorrectionRatio();
                        targetEnergy = Mathf.Clamp(targetEnergy, currentEnergy - maxCorrection, currentEnergy + maxCorrection);

                        float correction = targetEnergy - currentEnergy;
                        if (Mathf.Abs(correction) >= 0.01f)
                        {
                            imbue.SetEnergyInstant(targetEnergy);
                            currentEnergy = imbue.energy;

                            if (correction > 0f)
                            {
                                adjustedUp++;
                            }
                            else
                            {
                                adjustedDown++;
                            }

                            if (IDMTelemetry.ShouldLogCorrection(key, now) && IDMLog.VerboseEnabled)
                            {
                                IDMLog.Info(
                                    "imbue_correction context=" + context +
                                    " multiplier=" + multiplier.ToString("0.00") +
                                    " naturalDrain=" + naturalDrain.ToString("0.000") +
                                    " target=" + targetEnergy.ToString("0.00") +
                                    " correction=" + correction.ToString("0.00"),
                                    verboseOnly: true);
                            }
                        }
                        else
                        {
                            unchanged++;
                        }
                    }
                    else
                    {
                        unchanged++;
                    }

                    state.PreviousEnergy = currentEnergy;
                    state.PreviousTime = now;
                    state.LastSeenTime = now;
                    state.LastAppliedMultiplier = multiplier;
                    trackedStates[key] = state;
                }
            }

            CleanupStaleStates(now);
            UpdateAdaptiveInterval(scannedImbues, adjustedUp + adjustedDown);
            IDMTelemetry.RecordCycle(scannedItems, scannedImbues, adjustedUp, adjustedDown, unchanged, trackedStates.Count, nativeInfiniteApplied);
        }

        private void UpdateAdaptiveInterval(int scannedImbues, int corrections)
        {
            float previousMultiplier = adaptiveIntervalMultiplier;

            if (scannedImbues <= 0 || corrections > 0)
            {
                stableCyclesWithoutCorrections = 0;
                adaptiveIntervalMultiplier = 1f;
            }
            else
            {
                stableCyclesWithoutCorrections++;
                if (stableCyclesWithoutCorrections % StableCyclesPerBackoffStep == 0)
                {
                    adaptiveIntervalMultiplier = Mathf.Min(
                        MaxAdaptiveIntervalMultiplier,
                        adaptiveIntervalMultiplier + AdaptiveIntervalStep);
                }
            }

            if (!Mathf.Approximately(previousMultiplier, adaptiveIntervalMultiplier) && IDMLog.DiagnosticsEnabled)
            {
                IDMLog.Info(
                    "adaptive_scan multiplier=" + adaptiveIntervalMultiplier.ToString("0.0") +
                    " stableCycles=" + stableCyclesWithoutCorrections +
                    " scannedImbues=" + scannedImbues +
                    " corrections=" + corrections);
            }
        }

        private void CleanupStaleStates(float now)
        {
            if (trackedStates.Count == 0)
            {
                return;
            }

            List<int> staleKeys = null;
            foreach (KeyValuePair<int, TrackedImbueState> pair in trackedStates)
            {
                if (now - pair.Value.LastSeenTime <= 15f)
                {
                    continue;
                }

                if (staleKeys == null)
                {
                    staleKeys = new List<int>();
                }
                staleKeys.Add(pair.Key);
            }

            if (staleKeys == null)
            {
                return;
            }

            for (int i = 0; i < staleKeys.Count; i++)
            {
                trackedStates.Remove(staleKeys[i]);
            }
        }

        private static IDMModOptions.DrainContext ResolveContext(Item item)
        {
            bool thrown = item.isFlying || item.isThrowed;
            Creature creature = item.mainHandler != null && item.mainHandler.creature != null
                ? item.mainHandler.creature
                : (item.lastHandler != null ? item.lastHandler.creature : null);

            if (creature != null)
            {
                if (creature.isPlayer)
                {
                    return thrown ? IDMModOptions.DrainContext.PlayerThrown : IDMModOptions.DrainContext.PlayerHeld;
                }
                return thrown ? IDMModOptions.DrainContext.NpcThrown : IDMModOptions.DrainContext.NpcHeld;
            }

            return IDMModOptions.DrainContext.WorldDropped;
        }

        private void HandleDiagnosticsToggles()
        {
            bool refreshed = false;

            if (IDMModOptions.ResetTracking)
            {
                IDMModOptions.ResetTracking = false;
                trackedStates.Clear();
                IDMTelemetry.ResetTrackingCounters();
                IDMLog.Info("Tracking reset from diagnostics menu.", true);
                refreshed = true;
            }

            if (refreshed)
            {
                ModManager.RefreshModOptionsUI();
            }
        }

        private void SetNativeInfinite(bool enabled)
        {
            nativeInfiniteApplied = enabled;
            Imbue.infiniteImbue = enabled;
            IDMLog.Info("native_infinite=" + enabled + " source=global_drain", true);
        }
    }
}
