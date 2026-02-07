using System;
using EnemyImbuePresets.Configuration;
using ThunderRoad;

namespace EnemyImbuePresets.Core
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
                EIPLog.Info("Enemy Imbue Presets v" + EIPModOptions.VERSION + " enabled.");
                EnemyImbueManager.Instance.Initialize();
                EIPModOptionSync.Instance.Initialize();
                Hooks.EventHooks.Subscribe();
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
                EIPModOptionSync.Instance.Update();
                EnemyImbueManager.Instance.Update();
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
                EIPModOptionSync.Instance.Shutdown();
                EnemyImbueManager.Instance.Shutdown();
                EIPLog.Info("Enemy Imbue Presets disabled.");
            }
            catch (Exception ex)
            {
                EIPLog.Error("ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}
