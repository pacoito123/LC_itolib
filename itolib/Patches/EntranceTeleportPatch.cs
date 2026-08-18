using GameNetcodeStuff;
using HarmonyLib;
using itolib.Extensions;
using System;
using Unity.Netcode;

namespace itolib.Patches
{
    internal static class EntranceTeleportPatch
    {
        internal static event Action<PlayerControllerB, bool>? OnEntranceTeleportUsed;

        [HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.TeleportPlayerServerRpc))]
        [HarmonyPrefix]
        private static void EntranceTeleportTeleportPlayerServerRpcPre(EntranceTeleport __instance, int playerObj)
        {
            // Only run on the player calling the ServerRpc.
            if (__instance.__rpc_exec_stage is not NetworkBehaviour.__RpcExecStage.Send)
            {
                return;
            }

            if (StartOfRound.Instance != null)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerObj];

                if (player == null || !player.IsLocalClient())
                {
                    return;
                }

                OnEntranceTeleportUsed?.Invoke(player, !__instance.isEntranceToBuilding);
            }
        }
    }
}