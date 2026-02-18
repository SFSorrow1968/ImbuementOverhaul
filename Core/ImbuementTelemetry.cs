using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    internal static class ImbuementTelemetry
    {
        private const float SummaryIntervalSeconds = 30f;

        private static string runId = "none";
        private static bool initialized;
        private static float nextSummaryTime;
        private static float sessionStartTime;
        private static int summaryCount;

        private static int trackEvaluations;
        private static int trackRollApplied;
        private static int trackRollSkipped;
        private static int trackReused;
        private static int spawnEvaluations;
        private static int updateEvaluations;
        private static int configRefreshes;
        private static int itemWrites;
        private static int itemClears;
        private static int transferFailures;
        private static int casterSpellOverrides;
        private static int applyAttempts;
        private static int applyChanged;
        private static int applyNoChange;
        private static int applyNoHeldItems;

        private static int totalTrackEvaluations;
        private static int totalTrackRollApplied;
        private static int totalTrackRollSkipped;
        private static int totalTrackReused;
        private static int totalSpawnEvaluations;
        private static int totalUpdateEvaluations;
        private static int totalConfigRefreshes;
        private static int totalItemWrites;
        private static int totalItemClears;
        private static int totalTransferFailures;
        private static int totalCasterSpellOverrides;
        private static int totalApplyAttempts;
        private static int totalApplyChanged;
        private static int totalApplyNoChange;
        private static int totalApplyNoHeldItems;

        private static readonly Dictionary<string, int> skipReasonsInterval = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> skipReasonsTotal = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            runId = Guid.NewGuid().ToString("N").Substring(0, 8);
            initialized = true;
            sessionStartTime = Time.unscaledTime;
            nextSummaryTime = sessionStartTime + SummaryIntervalSeconds;
            summaryCount = 0;
            ResetInterval();
            ResetTotals();

            ImbuementLog.Diag(
                "diag evt=session_start run=" + runId +
                " assignmentHash=" + Configuration.ImbuementModOptions.GetAssignmentStateHash() +
                " presetHash=" + Configuration.ImbuementModOptions.GetPresetSelectionHash());
        }

        public static void Shutdown()
        {
            if (!initialized)
            {
                return;
            }

            EmitSummary(force: true);
            EmitSessionTotals();
            ImbuementLog.Diag(
                "diag evt=session_end run=" + runId +
                " uptimeSec=" + Mathf.Max(0f, Time.unscaledTime - sessionStartTime).ToString("F1") +
                " summaryCount=" + summaryCount);
            initialized = false;
        }

        public static void Update(float now)
        {
            if (!initialized || now < nextSummaryTime)
            {
                return;
            }

            EmitSummary(force: false);
            nextSummaryTime = now + SummaryIntervalSeconds;
        }

        public static void RecordConfigRefresh(int previousHash, int currentHash)
        {
            if (!initialized)
            {
                return;
            }

            configRefreshes++;
            totalConfigRefreshes++;

            ImbuementLog.Diag(
                "diag evt=config_refresh run=" + runId +
                " prevHash=" + previousHash +
                " newHash=" + currentHash,
                verboseOnly: true);
        }

        public static void RecordTrackSkip(string reason, bool fromSpawnEvent)
        {
            if (!initialized)
            {
                return;
            }

            trackEvaluations++;
            totalTrackEvaluations++;
            trackRollSkipped++;
            totalTrackRollSkipped++;

            if (fromSpawnEvent)
            {
                spawnEvaluations++;
                totalSpawnEvaluations++;
            }
            else
            {
                updateEvaluations++;
                totalUpdateEvaluations++;
            }

            Increment(skipReasonsInterval, reason);
            Increment(skipReasonsTotal, reason);
        }

        public static void RecordTrackReuse(bool fromSpawnEvent)
        {
            if (!initialized)
            {
                return;
            }

            trackReused++;
            totalTrackReused++;

            if (fromSpawnEvent)
            {
                spawnEvaluations++;
                totalSpawnEvaluations++;
            }
            else
            {
                updateEvaluations++;
                totalUpdateEvaluations++;
            }
        }

        public static void RecordTrackRollResult(bool fromSpawnEvent, bool applied)
        {
            if (!initialized)
            {
                return;
            }

            trackEvaluations++;
            totalTrackEvaluations++;
            if (applied)
            {
                trackRollApplied++;
                totalTrackRollApplied++;
            }
            else
            {
                trackRollSkipped++;
                totalTrackRollSkipped++;
                Increment(skipReasonsInterval, "roll_miss");
                Increment(skipReasonsTotal, "roll_miss");
            }

            if (fromSpawnEvent)
            {
                spawnEvaluations++;
                totalSpawnEvaluations++;
            }
            else
            {
                updateEvaluations++;
                totalUpdateEvaluations++;
            }
        }

        public static void RecordItemWrite()
        {
            if (!initialized)
            {
                return;
            }

            itemWrites++;
            totalItemWrites++;
        }

        public static void RecordItemClear()
        {
            if (!initialized)
            {
                return;
            }

            itemClears++;
            totalItemClears++;
        }

        public static void RecordTransferFailure()
        {
            if (!initialized)
            {
                return;
            }

            transferFailures++;
            totalTransferFailures++;
        }

        public static void RecordApplyAttempt()
        {
            if (!initialized)
            {
                return;
            }

            applyAttempts++;
            totalApplyAttempts++;
        }

        public static void RecordApplyOutcome(bool changed)
        {
            if (!initialized)
            {
                return;
            }

            if (changed)
            {
                applyChanged++;
                totalApplyChanged++;
            }
            else
            {
                applyNoChange++;
                totalApplyNoChange++;
            }
        }

        public static void RecordApplyNoHeldItems()
        {
            if (!initialized)
            {
                return;
            }

            applyNoHeldItems++;
            totalApplyNoHeldItems++;
            Increment(skipReasonsInterval, "roll_pass_no_held_items");
            Increment(skipReasonsTotal, "roll_pass_no_held_items");
        }

        public static void RecordApplySkipReason(string reason)
        {
            if (!initialized)
            {
                return;
            }

            Increment(skipReasonsInterval, reason);
            Increment(skipReasonsTotal, reason);
        }

        public static void RecordCasterSpellOverride()
        {
            if (!initialized)
            {
                return;
            }

            casterSpellOverrides++;
            totalCasterSpellOverrides++;
        }

        private static void EmitSummary(bool force)
        {
            if (!force &&
                trackEvaluations == 0 &&
                trackReused == 0 &&
                configRefreshes == 0 &&
                itemWrites == 0 &&
                itemClears == 0 &&
                transferFailures == 0 &&
                casterSpellOverrides == 0 &&
                applyAttempts == 0)
            {
                return;
            }

            summaryCount++;
            float applyRate = trackEvaluations > 0
                ? (trackRollApplied * 100f) / trackEvaluations
                : 0f;

            ImbuementLog.Diag(
                "diag evt=summary run=" + runId +
                " intervalSec=" + SummaryIntervalSeconds.ToString("F0") +
                " trackEval=" + trackEvaluations +
                " trackReuse=" + trackReused +
                " apply=" + trackRollApplied +
                " skip=" + trackRollSkipped +
                " applyRate=" + applyRate.ToString("F1") + "%" +
                " spawnEval=" + spawnEvaluations +
                " updateEval=" + updateEvaluations +
                " cfgRefresh=" + configRefreshes +
                " itemWrites=" + itemWrites +
                " itemClears=" + itemClears +
                " transferFail=" + transferFailures +
                " applyAttempt=" + applyAttempts +
                " applyChanged=" + applyChanged +
                " applyNoChange=" + applyNoChange +
                " applyNoItems=" + applyNoHeldItems +
                " casterOverride=" + casterSpellOverrides +
                " topSkipReasons=" + FormatTop(skipReasonsInterval));

            ResetInterval();
        }

        private static void EmitSessionTotals()
        {
            float uptime = Mathf.Max(0f, Time.unscaledTime - sessionStartTime);
            float applyRate = totalTrackEvaluations > 0
                ? (totalTrackRollApplied * 100f) / totalTrackEvaluations
                : 0f;

            float skipRate = totalTrackEvaluations > 0
                ? (totalTrackRollSkipped * 100f) / totalTrackEvaluations
                : 0f;

            ImbuementLog.Diag(
                "diag evt=session_totals run=" + runId +
                " uptimeSec=" + uptime.ToString("F1") +
                " summaryCount=" + summaryCount +
                " trackEval=" + totalTrackEvaluations +
                " trackReuse=" + totalTrackReused +
                " apply=" + totalTrackRollApplied +
                " skip=" + totalTrackRollSkipped +
                " applyRate=" + applyRate.ToString("F1") + "%" +
                " spawnEval=" + totalSpawnEvaluations +
                " updateEval=" + totalUpdateEvaluations +
                " cfgRefresh=" + totalConfigRefreshes +
                " itemWrites=" + totalItemWrites +
                " itemClears=" + totalItemClears +
                " transferFail=" + totalTransferFailures +
                " applyAttempt=" + totalApplyAttempts +
                " applyChanged=" + totalApplyChanged +
                " applyNoChange=" + totalApplyNoChange +
                " applyNoItems=" + totalApplyNoHeldItems +
                " casterOverride=" + totalCasterSpellOverrides +
                " topSkipReasons=" + FormatTop(skipReasonsTotal));

            ImbuementLog.Diag(
                "diag evt=session_kpi run=" + runId +
                " applyRate=" + applyRate.ToString("F1") + "%" +
                " skipRate=" + skipRate.ToString("F1") + "%" +
                " transferFail=" + totalTransferFailures +
                " applyNoItems=" + totalApplyNoHeldItems);
        }

        private static void ResetInterval()
        {
            trackEvaluations = 0;
            trackRollApplied = 0;
            trackRollSkipped = 0;
            trackReused = 0;
            spawnEvaluations = 0;
            updateEvaluations = 0;
            configRefreshes = 0;
            itemWrites = 0;
            itemClears = 0;
            transferFailures = 0;
            casterSpellOverrides = 0;
            applyAttempts = 0;
            applyChanged = 0;
            applyNoChange = 0;
            applyNoHeldItems = 0;
            skipReasonsInterval.Clear();
        }

        private static void ResetTotals()
        {
            totalTrackEvaluations = 0;
            totalTrackRollApplied = 0;
            totalTrackRollSkipped = 0;
            totalTrackReused = 0;
            totalSpawnEvaluations = 0;
            totalUpdateEvaluations = 0;
            totalConfigRefreshes = 0;
            totalItemWrites = 0;
            totalItemClears = 0;
            totalTransferFailures = 0;
            totalCasterSpellOverrides = 0;
            totalApplyAttempts = 0;
            totalApplyChanged = 0;
            totalApplyNoChange = 0;
            totalApplyNoHeldItems = 0;
            skipReasonsTotal.Clear();
        }

        private static void Increment(Dictionary<string, int> map, string reason)
        {
            if (map == null)
            {
                return;
            }

            string key = string.IsNullOrWhiteSpace(reason) ? "unknown" : reason.Trim().ToLowerInvariant();
            if (map.TryGetValue(key, out int count))
            {
                map[key] = count + 1;
            }
            else
            {
                map[key] = 1;
            }
        }

        private static string FormatTop(Dictionary<string, int> map)
        {
            if (map == null || map.Count == 0)
            {
                return "none";
            }

            List<KeyValuePair<string, int>> pairs = new List<KeyValuePair<string, int>>(map);
            pairs.Sort((a, b) => b.Value.CompareTo(a.Value));

            int take = Mathf.Min(6, pairs.Count);
            string result = string.Empty;
            for (int i = 0; i < take; i++)
            {
                if (i > 0)
                {
                    result += "|";
                }

                result += pairs[i].Key + ":" + pairs[i].Value;
            }

            return result;
        }
    }
}


