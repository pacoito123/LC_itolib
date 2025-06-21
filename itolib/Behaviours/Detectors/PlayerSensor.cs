using System.Collections.Generic;
using GameNetcodeStuff;
using itolib.Extensions;
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
        public UnityEvent<PlayerControllerB> onPlayersAliveEach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onPlayersAliveAny = new();

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
            base.CheckObjectsInRegion(); // TODO: Do AABB checks on all players instead of overlap stuff.

            if (!IsHost)
            {
                return;
            }

            List<ulong> playersFound = new(StartOfRound.Instance.allPlayerScripts.Length);
            int playersFoundAlive = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out PlayerControllerB player))
                {
                    if (playersFound.Contains(player.actualClientId))
                    {
                        continue;
                    }

                    FoundPlayersEachClientRpc(player);

                    if (player.isActiveAndEnabled && !player.isPlayerDead)
                    {
                        FoundPlayersAliveEachClientRpc(player);
                        playersFoundAlive++;
                    }

                    playersFound.Add(player.actualClientId);
                }
            }

            if (playersFound.Count > 0)
            {
                FoundPlayersAnyClientRpc(playersFound.Count);

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
            if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                onRegionEntered.Invoke(player);

                RegionEnteredServerRpc(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                onRegionExited.Invoke(player);

                RegionEnteredServerRpc(player, exit: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void FoundPlayersEachClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                onObjectsEach.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void FoundPlayersAliveEachClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                onPlayersAliveEach.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [ClientRpc]
        public void FoundPlayersAnyClientRpc(int playersFound)
        {
            onObjectsAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [ClientRpc]
        public void FoundPlayersAliveAnyClientRpc(int playersFound)
        {
            onPlayersAliveAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        public void RegionEnteredServerRpc(NetworkBehaviourReference playerReference, bool exit = false)
        {
            RegionEnteredClientRpc(playerReference, exit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkBehaviourReference playerReference, bool exit = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                if (!exit)
                {
                    onRegionEntered.Invoke(player);
                }
                else
                {
                    onRegionExited.Invoke(player);
                }
            }
        }
    }
}