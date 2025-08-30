using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ToggleEvent : NetworkBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public bool CurrentState { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Toggle Event")]
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.Manual;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> toggleOn = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> toggleOff = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool performedActivation;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost || performedActivation)
            {
                return;
            }

            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    ToggleFromServer();
                    break;
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(ToggleFromServer);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(ToggleFromServer);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(ToggleFromServer);
                    }
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ToggleFromServer()
        {
            if (!performedActivation)
            {
                UnsubscribeFromEvents();

                performedActivation = true;
            }

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            ToggleFromClient();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ToggleFromClient()
        {
            ToggleFromClient(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void ToggleFromClient(PlayerControllerB player)
        {
            if (!player.IsLocalClient())
            {
                return;
            }

            PerformToggleLocal(CurrentState);

            if (IsSpawned)
            {
                PerformToggleServerRpc(player, CurrentState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="state"></param>
        [ServerRpc(RequireOwnership = false)]
        private void PerformToggleServerRpc(NetworkBehaviourReference playerReference, bool state)
        {
            PerformToggleClientRpc(playerReference, state);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="state"></param>
        [ClientRpc]
        private void PerformToggleClientRpc(NetworkBehaviourReference playerReference, bool state)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                PerformToggleLocal(state);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="state"></param>
        private void PerformToggleLocal(bool state)
        {
            CurrentState = !state;

            if (CurrentState)
            {
                toggleOn.Invoke(true);
            }
            else
            {
                toggleOff.Invoke(false);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(ToggleFromServer);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(ToggleFromServer);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(ToggleFromServer);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (!performedActivation && activationTime is ActivationTime.DungeonComplete)
            {
                ToggleFromServer();

                performedActivation = true;
            }
        }
    }
}