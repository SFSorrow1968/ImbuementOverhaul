using System;
using System.Collections.Generic;
using ImbueDurationManager.Configuration;
using UnityEngine;

namespace ImbueDurationManager.Core
{
    internal static class IDMTelemetry
    {
        private const float CorrectionLogIntervalSeconds = 0.8f;
        private const float SummaryIntervalSeconds = 30f;

        private static readonly Dictionary<int, float> correctionLogGate = new Dictionary<int, float>();

        private static string runId = "none";
        private static bool initialized;
        private static float nextSummaryTime;
        private static float sessionStartTime;
        private static int summaryCount;

        private static int cycles;
        private static int itemsScanned;
        private static int imbuesScanned;
        private static int adjustedUp;
        private static int adjustedDown;
        private static int unchanged;
        private static int nativeInfiniteCycles;
        private static int peakTracked;

        private static int totalCycles;
        private static int totalItemsScanned;
        private static int totalImbuesScanned;
        private static int totalAdjustedUp;
        private static int totalAdjustedDown;
        private static int totalUnchanged;
        private static int totalNativeInfiniteCycles;
        private static int totalPeakTracked;

        public static void Initialize()
        {
            correctionLogGate.Clear();
            runId = Guid.NewGuid().ToString("N").Substring(0, 8);
            initialized = true;
            sessionStartTime = Time.unscaledTime;
            summaryCount = 0;
            nextSummaryTime = sessionStartTime + SummaryIntervalSeconds;
            ResetIntervalCounters();
            ResetTotals();

            IDMLog.Diag(
                "diag evt=session_start run=" + runId +
                " presetHash=" + IDMModOptions.GetPresetSelectionHash() +
                " sourceHash=" + IDMModOptions.GetSourceOfTruthHash());
        }

        public static void Shutdown()
        {
            if (!initialized)
            {
                correctionLogGate.Clear();
                return;
            }

            EmitSummary(force: true);
            EmitSessionTotals();
            IDMLog.Diag(
                "diag evt=session_end run=" + runId +
                " uptimeSec=" + Mathf.Max(0f, Time.unscaledTime - sessionStartTime).ToString("F1") +
                " summaryCount=" + summaryCount);

            correctionLogGate.Clear();
            nextSummaryTime = 0f;
            initialized = false;
        }

        public static void ResetTrackingCounters()
        {
            ResetIntervalCounters();
            ResetTotals();
            summaryCount = 0;
            sessionStartTime = Time.unscaledTime;
            nextSummaryTime = sessionStartTime + SummaryIntervalSeconds;
        }

        public static void RecordCycle(int cycleItems, int cycleImbues, int cycleAdjustedUp, int cycleAdjustedDown, int cycleUnchanged, int trackedCount, bool nativeInfinite)
        {
            if (!initialized)
            {
                return;
            }

            cycles++;
            itemsScanned += cycleItems;
            imbuesScanned += cycleImbues;
            adjustedUp += cycleAdjustedUp;
            adjustedDown += cycleAdjustedDown;
            unchanged += cycleUnchanged;
            if (nativeInfinite)
            {
                nativeInfiniteCycles++;
            }
            if (trackedCount > peakTracked)
            {
                peakTracked = trackedCount;
            }

            totalCycles++;
            totalItemsScanned += cycleItems;
            totalImbuesScanned += cycleImbues;
            totalAdjustedUp += cycleAdjustedUp;
            totalAdjustedDown += cycleAdjustedDown;
            totalUnchanged += cycleUnchanged;
            if (nativeInfinite)
            {
                totalNativeInfiniteCycles++;
            }
            if (trackedCount > totalPeakTracked)
            {
                totalPeakTracked = trackedCount;
            }

            if (IDMLog.VerboseEnabled)
            {
                IDMLog.Info(
                    "cycle scannedItems=" + cycleItems +
                    " scannedImbues=" + cycleImbues +
                    " adjustedUp=" + cycleAdjustedUp +
                    " adjustedDown=" + cycleAdjustedDown +
                    " unchanged=" + cycleUnchanged +
                    " tracked=" + trackedCount +
                    " nativeInfinite=" + nativeInfinite,
                    verboseOnly: true);
            }
        }

        public static void Update(float now)
        {
            if (!initialized)
            {
                return;
            }

            if (now < nextSummaryTime)
            {
                return;
            }

            EmitSummary(force: false);
            nextSummaryTime = now + SummaryIntervalSeconds;
        }

        public static bool ShouldLogCorrection(int imbueId, float now)
        {
            if (correctionLogGate.TryGetValue(imbueId, out float nextAllowed) && now < nextAllowed)
            {
                return false;
            }

            correctionLogGate[imbueId] = now + CorrectionLogIntervalSeconds;
            return true;
        }

        private static void EmitSummary(bool force)
        {
            if (!force && cycles == 0)
            {
                return;
            }

            summaryCount++;
            int totalAdjustments = adjustedUp + adjustedDown;
            float adjustmentRate = imbuesScanned > 0
                ? (totalAdjustments * 100f) / imbuesScanned
                : 0f;
            float stableRate = imbuesScanned > 0
                ? (unchanged * 100f) / imbuesScanned
                : 0f;

            IDMLog.Diag(
                "diag evt=summary run=" + runId +
                " intervalSec=" + SummaryIntervalSeconds.ToString("F0") +
                " cycles=" + cycles +
                " scannedItems=" + itemsScanned +
                " scannedImbues=" + imbuesScanned +
                " adjustedUp=" + adjustedUp +
                " adjustedDown=" + adjustedDown +
                " unchanged=" + unchanged +
                " adjustmentRate=" + adjustmentRate.ToString("F1") + "%" +
                " stableRate=" + stableRate.ToString("F1") + "%" +
                " nativeInfiniteCycles=" + nativeInfiniteCycles +
                " peakTracked=" + peakTracked);

            ResetIntervalCounters();
        }

        private static void EmitSessionTotals()
        {
            float uptime = Mathf.Max(0f, Time.unscaledTime - sessionStartTime);
            int totalAdjustments = totalAdjustedUp + totalAdjustedDown;
            float adjustmentRate = totalImbuesScanned > 0
                ? (totalAdjustments * 100f) / totalImbuesScanned
                : 0f;
            float stableRate = totalImbuesScanned > 0
                ? (totalUnchanged * 100f) / totalImbuesScanned
                : 0f;
            float upShare = totalAdjustments > 0
                ? (totalAdjustedUp * 100f) / totalAdjustments
                : 0f;
            float avgImbuesPerCycle = totalCycles > 0
                ? totalImbuesScanned / (float)totalCycles
                : 0f;

            IDMLog.Diag(
                "diag evt=session_totals run=" + runId +
                " uptimeSec=" + uptime.ToString("F1") +
                " summaryCount=" + summaryCount +
                " cycles=" + totalCycles +
                " scannedItems=" + totalItemsScanned +
                " scannedImbues=" + totalImbuesScanned +
                " adjustedUp=" + totalAdjustedUp +
                " adjustedDown=" + totalAdjustedDown +
                " unchanged=" + totalUnchanged +
                " adjustmentRate=" + adjustmentRate.ToString("F1") + "%" +
                " stableRate=" + stableRate.ToString("F1") + "%" +
                " nativeInfiniteCycles=" + totalNativeInfiniteCycles +
                " peakTracked=" + totalPeakTracked);

            IDMLog.Diag(
                "diag evt=session_kpi run=" + runId +
                " adjustmentRate=" + adjustmentRate.ToString("F1") + "%" +
                " upShare=" + upShare.ToString("F1") + "%" +
                " avgImbuesPerCycle=" + avgImbuesPerCycle.ToString("F2") +
                " peakTracked=" + totalPeakTracked +
                " nativeInfiniteCycles=" + totalNativeInfiniteCycles);
        }

        private static void ResetIntervalCounters()
        {
            cycles = 0;
            itemsScanned = 0;
            imbuesScanned = 0;
            adjustedUp = 0;
            adjustedDown = 0;
            unchanged = 0;
            nativeInfiniteCycles = 0;
            peakTracked = 0;
        }

        private static void ResetTotals()
        {
            totalCycles = 0;
            totalItemsScanned = 0;
            totalImbuesScanned = 0;
            totalAdjustedUp = 0;
            totalAdjustedDown = 0;
            totalUnchanged = 0;
            totalNativeInfiniteCycles = 0;
            totalPeakTracked = 0;
        }

    }
}
