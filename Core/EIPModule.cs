using System;
using ImbuementOverhaul.Configuration;
using ImbueDurationManager.Configuration;
using ImbueDurationManager.Core;
using ThunderRoad;
using UnityEngine;

namespace ImbuementOverhaul.Core
{
    public class EIPModule : ThunderScript
    {
        public static EIPModule Instance { get; private set; }

        public override void ScriptEnable()
        {
            base.ScriptEnable();
            Instance = this;

            try
            {
                EIPLog.Info(
                    "Imbuement Overhaul enabled. " +
                    "factionEngine=" + EIPModOptions.VERSION +
                    " durationEngine=" + IDMModOptions.VERSION + ".");

                EIPTelemetry.Initialize();
                EnemyImbueManager.Instance.Initialize();
                EIPModOptionSync.Instance.Initialize();
                Hooks.EventHooks.Subscribe();

                IDMTelemetry.Initialize();
                IDMModOptionSync.Instance.Initialize();
                IDMManager.Instance.Initialize();
            }
            catch (Exception ex)
            {
                EIPLog.Error("ScriptEnable failed: " + ex.Message);
            }
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();

            try
            {
                float now = Time.unscaledTime;

                EIPModOptionSync.Instance.Update();
                EnemyImbueManager.Instance.Update();

                IDMModOptionSync.Instance.Update();
                IDMManager.Instance.Update();

                EIPTelemetry.Update(now);
                IDMTelemetry.Update(now);
            }
            catch (Exception ex)
            {
                EIPLog.Error("ScriptUpdate error: " + ex.Message);
            }
        }

        public override void ScriptDisable()
        {
            try
            {
                Hooks.EventHooks.Unsubscribe();

                IDMManager.Instance.Shutdown();
                IDMModOptionSync.Instance.Shutdown();
                IDMTelemetry.Shutdown();

                EIPModOptionSync.Instance.Shutdown();
                EnemyImbueManager.Instance.Shutdown();
                EIPTelemetry.Shutdown();

                EIPLog.Info("Imbuement Overhaul disabled.");
            }
            catch (Exception ex)
            {
                EIPLog.Error("ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}

