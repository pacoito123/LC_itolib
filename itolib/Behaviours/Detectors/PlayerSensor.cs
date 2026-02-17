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
            layerMask = LayerMask.GetMask("Player", "PlayerRagdoll");

            base.Reset();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            if (onlyAffectLocalPlayer)
            {
                maxObjects = 0;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (onlyAffectLocalPlayer)
            {
                base.CheckObjectsInRegion();

                PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

                if (!localPlayer.isPlayerDead && localPlayer.isActiveAndEnabled && regionCollider != null
                    && regionCollider.bounds.Contains(localPlayer.transform.position))
                {
                    onPlayersAliveEach.Invoke(localPlayer);
                    onPlayersAliveAny.Invoke(1);
                }

                return;
            }

            if (!IsSpawned || !IsHost) // TODO: Overlapping players could be desynced since it's host-sided...
            {
                return;
            }

            base.CheckObjectsInRegion(); // TODO: Do AABB checks on all players instead of overlap stuff.

            List<ulong> playersFound = new(StartOfRound.Instance.allPlayerScripts.Length);
            int playersFoundAlive = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? playerCollider = overlapBuffer?[i];

                if (playerCollider == null || !playerCollider.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                if (playerCollider.TryGetComponent(out PlayerControllerB player))
                {
                    if (playersFound.Contains(player.actualClientId))
                    {
                        continue;
                    }

                    FoundPlayersEachRpc(player);

                    if (player.isActiveAndEnabled && !player.isPlayerDead)
                    {
                        FoundPlayersAliveEachRpc(player);
                        playersFoundAlive++;
                    }

                    playersFound.Add(player.actualClientId);
                }
            }

            if (playersFound.Count > 0)
            {
                FoundPlayersAnyRpc(playersFound.Count);

                if (playersFoundAlive > 0)
                {
                    FoundPlayersAliveAnyRpc(playersFoundAlive);
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
                    RegionEnteredRpc(player);
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
                    RegionEnteredRpc(player, exit: true);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundPlayersEachRpc(NetworkBehaviourReference playerReference)
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
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundPlayersAliveEachRpc(NetworkBehaviourReference playerReference)
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
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundPlayersAnyRpc(int playersFound)
        {
            onObjectsAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playersFound"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundPlayersAliveAnyRpc(int playersFound)
        {
            onPlayersAliveAny.Invoke(playersFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="exit"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void RegionEnteredRpc(NetworkBehaviourReference playerReference, bool exit = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
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