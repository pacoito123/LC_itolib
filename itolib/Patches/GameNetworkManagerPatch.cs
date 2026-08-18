using HarmonyLib;
using itolib.ScriptableObjects;
using Unity.Netcode;

namespace itolib.Patches
{
    internal static class GameNetworkManagerPatch
    {
        internal static bool networkStarted;

        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        [HarmonyPrefix]
        private static void GameNetworkManagerStartPre(GameNetworkManager __instance)
        {
            if (networkStarted)
            {
                return;
            }
            networkStarted = true;

            if (!__instance.TryGetComponent(out NetworkManager networkManager))
            {
                return;
            }

            foreach (NetworkObject prefab in ScriptableNetworkPrefab.RegisteredPrefabs.Values)
            {
                if (prefab != null)
                {
                    networkManager.AddNetworkPrefab(prefab.gameObject);
                }
            }
        }
    }
}