using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Represents a <c>DetectRegion</c> specifically for <c>PlayerControllerB</c> objects, with some additional stuff.
    /// </summary>
    public class PlayerSensor : DetectRegion<PlayerControllerB>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Sensor")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onPlayersAliveEach;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int>? onPlayersAliveAny;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            maxObjects = 4;
            layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("PlayerRagdoll"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            base.CheckObjectsInRegion();

            if (!IsHost)
            {
                return;
            }

            int playersFound = 0,
                playersFoundAlive = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out PlayerControllerB player))
                {
                    FoundPlayersEachClientRpc(player.GetComponent<NetworkObject>());
                    playersFound++;

                    if (player.isActiveAndEnabled && !player.isPlayerDead)
                    {
                        FoundPlayersAliveEachClientRpc(player.GetComponent<NetworkObject>());
                        playersFoundAlive++;
                    }
                }
            }

            if (playersFound > 0)
            {
                FoundPlayersAnyClientRpc(playersFound);

                if (playersFoundAlive > 0)
                {
                    FoundPlayersAliveAnyClientRpc(playersFoundAlive);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                onRegionEntered?.Invoke(player);

                RegionEnteredServerRpc(player.GetComponent<NetworkObject>());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                onRegionExited?.Invoke(player);

                RegionEnteredServerRpc(player.GetComponent<NetworkObject>(), exit: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void FoundPlayersEachClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
            {
                onObjectsEach?.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void FoundPlayersAliveEachClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
            {
                onPlayersAliveEach?.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [ClientRpc]
        public void FoundPlayersAnyClientRpc(int playersFound)
        {
            onObjectsAny?.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [ClientRpc]
        public void FoundPlayersAliveAnyClientRpc(int playersFound)
        {
            onPlayersAliveAny?.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        public void RegionEnteredServerRpc(NetworkObjectReference playerReference, bool exit = false)
        {
            RegionEnteredClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkObjectReference playerReference, bool exit = false)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                if (exit)
                {
                    onRegionEntered?.Invoke(player);
                }
                else
                {
                    onRegionExited?.Invoke(player);
                }
            }
        }
    }
}