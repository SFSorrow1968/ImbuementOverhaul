using EnemyImbuePresets.Configuration;
using UnityEngine;

namespace EnemyImbuePresets.Core
{
    internal enum EIPLogLevel
    {
        Off = 0,
        Basic = 1,
        Verbose = 2
    }

    internal static class EIPLog
    {
        private const string Prefix = "[EIP] ";

        public static bool VerboseEnabled => GetCurrentLevel() >= EIPLogLevel.Verbose;

        public static void Info(string message, bool verboseOnly = false)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            EIPLogLevel level = GetCurrentLevel();
            if (level == EIPLogLevel.Off)
            {
                return;
            }
            if (verboseOnly && level < EIPLogLevel.Verbose)
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

            EIPLogLevel level = GetCurrentLevel();
            if (level == EIPLogLevel.Off)
            {
                return;
            }
            if (verboseOnly && level < EIPLogLevel.Verbose)
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

        private static EIPLogLevel GetCurrentLevel()
        {
            string configured = EIPModOptions.LogLevel;
            if (string.Equals(configured, "Off", System.StringComparison.OrdinalIgnoreCase))
            {
                return EIPLogLevel.Off;
            }
            if (string.Equals(configured, "Verbose", System.StringComparison.OrdinalIgnoreCase))
            {
                return EIPLogLevel.Verbose;
            }
            return EIPLogLevel.Basic;
        }
    }
}
