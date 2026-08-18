using HarmonyLib;
using System;

namespace itolib.Patches
{
    internal static class RoundManagerPatches
    {
        internal static event Action? OnSpawnSyncedProps;
        internal static event Action? OnSpawnScrapInLevel;
        internal static event Action? OnSpawnMapObjects;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnSyncedProps))]
        [HarmonyPrefix]
        private static void RoundManagerSpawnSyncedPropsPre()
        {
            try
            {
                OnSpawnSyncedProps?.Invoke();
            }
            catch (Exception e)
            {
                Plugin.StaticLogger.LogError($"Exception during 'SyncedSpawn' ActivationTime: {e}");
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPrefix]
        private static void RoundManagerSpawnScrapInLevelPre()
        {
            try
            {
                OnSpawnScrapInLevel?.Invoke();
            }
            catch (Exception e)
            {
                Plugin.StaticLogger.LogError($"Exception during 'ScrapSpawn' ActivationTime: {e}");
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnMapObjects))]
        [HarmonyPrefix]
        private static void RoundManagerSpawnMapObjectsPre()
        {
            try
            {
                OnSpawnMapObjects?.Invoke();
            }
            catch (Exception e)
            {
                Plugin.StaticLogger.LogError($"Exception during 'HazardSpawn' ActivationTime: {e}");
            }
        }
    }
}