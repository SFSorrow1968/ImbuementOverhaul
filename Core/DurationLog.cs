using ImbuementOverhaul.Configuration;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    internal static class DurationLog
    {
        private const string Prefix = "[IOD] ";

        public static bool DiagnosticsEnabled => DurationModOptions.EnableDiagnosticsLogging || VerboseEnabled;
        public static bool StructuredDiagnosticsEnabled => DiagnosticsEnabled;
        public static bool VerboseEnabled => DurationModOptions.EnableVerboseLogging;
        public static bool BasicEnabled => DurationModOptions.EnableBasicLogging || DiagnosticsEnabled;

        public static void Info(string message, bool verboseOnly = false)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!BasicEnabled)
            {
                return;
            }

            if (verboseOnly && !VerboseEnabled)
            {
                return;
            }

            Debug.Log(Prefix + message);
        }

        public static void Warn(string message, bool verboseOnly = false)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!BasicEnabled)
            {
                return;
            }

            if (verboseOnly && !VerboseEnabled)
            {
                return;
            }

            Debug.LogWarning(Prefix + message);
        }

        public static void Error(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Debug.LogError(Prefix + message);
        }

        public static void Diag(string message, bool verboseOnly = false)
        {
            if (string.IsNullOrWhiteSpace(message) || !StructuredDiagnosticsEnabled)
            {
                return;
            }

            if (verboseOnly && !VerboseEnabled)
            {
                return;
            }

            Debug.Log(Prefix + message);
        }
    }
}

