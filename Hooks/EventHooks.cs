using System;
using ImbuementOverhaul.Core;
using ThunderRoad;

namespace ImbuementOverhaul.Hooks
{
    public sealed class EventHooks
    {
        private static EventHooks instance;

        private bool subscribed;
        private EventManager.CreatureSpawnedEvent onCreatureSpawnHandler;
        private EventManager.CreatureDespawnedEvent onCreatureDespawnHandler;
        private EventManager.LevelLoadEvent onLevelUnloadHandler;

        public static void Subscribe()
        {
            if (instance == null)
            {
                instance = new EventHooks();
            }

            instance.SubscribeInternal();
        }

        public static void Unsubscribe()
        {
            instance?.UnsubscribeInternal();
        }

        private void SubscribeInternal()
        {
            if (subscribed)
            {
                return;
            }

            try
            {
                onCreatureSpawnHandler = OnCreatureSpawn;
                onCreatureDespawnHandler = OnCreatureDespawn;
                onLevelUnloadHandler = OnLevelUnload;

                EventManager.onCreatureSpawn += onCreatureSpawnHandler;
                EventManager.onCreatureDespawn += onCreatureDespawnHandler;
                EventManager.onLevelUnload += onLevelUnloadHandler;

                subscribed = true;
                ImbuementLog.Info("Event hooks subscribed.");
            }
            catch (Exception ex)
            {
                ImbuementLog.Error("Failed to subscribe hooks: " + ex.Message);
            }
        }

        private void UnsubscribeInternal()
        {
            if (!subscribed)
            {
                return;
            }

            try
            {
                if (onCreatureSpawnHandler != null)
                {
                    EventManager.onCreatureSpawn -= onCreatureSpawnHandler;
                }
                if (onCreatureDespawnHandler != null)
                {
                    EventManager.onCreatureDespawn -= onCreatureDespawnHandler;
                }
                if (onLevelUnloadHandler != null)
                {
                    EventManager.onLevelUnload -= onLevelUnloadHandler;
                }
            }
            catch (Exception ex)
            {
                ImbuementLog.Warn("Failed to fully unsubscribe hooks: " + ex.Message);
            }

            onCreatureSpawnHandler = null;
            onCreatureDespawnHandler = null;
            onLevelUnloadHandler = null;
            subscribed = false;
        }

        private void OnCreatureSpawn(Creature creature)
        {
            FactionImbuementManager.Instance.OnCreatureSpawn(creature);
        }

        private void OnCreatureDespawn(Creature creature, EventTime eventTime)
        {
            FactionImbuementManager.Instance.OnCreatureDespawn(creature, eventTime);
        }

        private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
        {
            FactionImbuementManager.Instance.OnLevelUnload(levelData, mode, eventTime);
        }
    }
}


