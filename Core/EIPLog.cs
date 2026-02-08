using EnemyImbuePresets.Configuration;
using UnityEngine;

namespace EnemyImbuePresets.Core
{
    internal static class EIPLog
    {
        private const string Prefix = "[EIP] ";

        public static bool DiagnosticsEnabled => EIPModOptions.EnableDiagnosticsLogging || VerboseEnabled;
        public static bool StructuredDiagnosticsEnabled => DiagnosticsEnabled || EIPModOptions.SessionDiagnostics;
        public static bool VerboseEnabled => EIPModOptions.EnableVerboseLogging;
        public static bool BasicEnabled => EIPModOptions.EnableBasicLogging || DiagnosticsEnabled;

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
