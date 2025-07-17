using GameNetcodeStuff;
using itolib.Extensions;
using System.Collections.Generic;
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
        [SerializeField] private bool onlyAffectLocalPlayer;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5)]
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onPlayersAliveEach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onPlayersAliveAny = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
        {
            maxObjects = 12;
            layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("PlayerRagdoll"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!IsHost) // TODO: Overlapping players could be desynced since it's host only...
            {
                return;
            }

            base.CheckObjectsInRegion(); // TODO: Do AABB checks on all players instead of overlap stuff.

            List<ulong> playersFound = new(StartOfRound.Instance.allPlayerScripts.Length);
            int playersFoundAlive = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                if (overlapBuffer![i].TryGetComponent(out PlayerControllerB player))
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
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                onRegionEntered.Invoke(player);

                if (!onlyAffectLocalPlayer && IsSpawned)
                {
                    RegionEnteredServerRpc(player);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                onRegionExited.Invoke(player);

                if (!onlyAffectLocalPlayer && IsSpawned)
                {
                    RegionEnteredServerRpc(player, exit: true);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void FoundPlayersEachClientRpc(NetworkBehaviourReference playerReference)
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
        private void FoundPlayersAliveEachClientRpc(NetworkBehaviourReference playerReference)
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
        private void FoundPlayersAnyClientRpc(int playersFound)
        {
            onObjectsAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [ClientRpc]
        private void FoundPlayersAliveAnyClientRpc(int playersFound)
        {
            onPlayersAliveAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        private void RegionEnteredServerRpc(NetworkBehaviourReference playerReference, bool exit = false)
        {
            RegionEnteredClientRpc(playerReference, exit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        private void RegionEnteredClientRpc(NetworkBehaviourReference playerReference, bool exit = false)
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