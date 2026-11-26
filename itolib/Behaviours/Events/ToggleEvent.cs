using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ToggleEvent : NetworkBehaviour, IActivationScript
    {
        /// <inheritdoc/>
        public IActivationScript ActivationSelf { get; }

        /// <inheritdoc/>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool CurrentState { get; private set; } // TODO: Replace with a NetworkVariable.

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the initial toggle to occur.
        /// </summary>
        [field: Header("Toggle Event")]
        [field: Tooltip("Desired activation time for the initial toggle to occur.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Manual;

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
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        private ToggleEvent()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     Perform event immediately or subscribe to a specific event upon being enabled, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void OnEnable()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Perform event immediately or subscribe to a specific event upon being spawned, depending on the set <c>ActivationTime</c>.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                return;
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Unsubscribe from any events that may have been subscribed to.
        /// </summary>
        private void OnDisable()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ToggleFromServer()
        {
            if (!IsHost)
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
            // Check if local player exists.
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }

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

            // TODO: Replace with a NetworkVariable.
            CurrentState = !CurrentState;

            PerformToggleLocal(CurrentState);

            if (IsSpawned)
            {
                PerformToggleRpc(CurrentState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="state"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PerformToggleRpc(bool state)
        {
            PerformToggleLocal(state);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="state"></param>
        private void PerformToggleLocal(bool state)
        {
            CurrentState = state;

            if (CurrentState)
            {
                toggleOn.Invoke(true);
            }
            else
            {
                toggleOff.Invoke(false);
            }
        }

        /// <inheritdoc/>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            ToggleFromClient();
        }

        /// <inheritdoc/>
        public void ResetActivation(bool instant)
        {
            ActivationSelf.Reinitialize(instant);
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}