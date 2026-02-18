using System;
using ImbuementOverhaul.Configuration;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    public class ImbuementOverhaulModule : ThunderScript
    {
        public static ImbuementOverhaulModule Instance { get; private set; }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            Instance = this;

            try
            {
                ImbuementLog.Info(
                    "Imbuement Overhaul enabled. " +
                    "factionEngine=" + ImbuementModOptions.VERSION +
                    " durationEngine=" + DurationModOptions.VERSION + ".");

                ImbuementTelemetry.Initialize();
                FactionImbuementManager.Instance.Initialize();
                ImbuementModOptionSync.Instance.Initialize();
                Hooks.EventHooks.Subscribe();

                DurationTelemetry.Initialize();
                DurationModOptionSync.Instance.Initialize();
                DurationManager.Instance.Initialize();
            }
            catch (Exception ex)
            {
                ImbuementLog.Error("ScriptEnable failed: " + ex.Message);
            }
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();

            try
            {
                float now = Time.unscaledTime;

                ImbuementModOptionSync.Instance.Update();
                FactionImbuementManager.Instance.Update();

                DurationModOptionSync.Instance.Update();
                DurationManager.Instance.Update();

                ImbuementTelemetry.Update(now);
                DurationTelemetry.Update(now);
            }
            catch (Exception ex)
            {
                ImbuementLog.Error("ScriptUpdate error: " + ex.Message);
            }
        }

        public override void ScriptDisable()
        {
            try
            {
                Hooks.EventHooks.Unsubscribe();

                DurationManager.Instance.Shutdown();
                DurationModOptionSync.Instance.Shutdown();
                DurationTelemetry.Shutdown();

                ImbuementModOptionSync.Instance.Shutdown();
                FactionImbuementManager.Instance.Shutdown();
                ImbuementTelemetry.Shutdown();

                ImbuementLog.Info("Imbuement Overhaul disabled.");
            }
            catch (Exception ex)
            {
                ImbuementLog.Error("ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}


