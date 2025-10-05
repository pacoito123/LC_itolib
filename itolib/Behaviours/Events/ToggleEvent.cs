using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ToggleEvent : NetworkBehaviour, IActivationScript
    {
        /// <summary>
        ///     Cached instance of the current <c>ToggleEvent</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
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
        [field: FormerlySerializedAs("activationTime")]
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
        ///     Desired <c>ActivationTime</c> for the initial toggle to occur.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the initial toggle to occur. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.Manual;


        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        private ToggleEvent()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (activationTime is not ActivationTime.Manual)
            {
                ActivationTime = activationTime;
            }
        }

        /// <summary>
        ///     TODO.
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
        ///     TODO.
        /// </summary>
        public void ToggleFromServer()
        {
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

            // TODO: Replace with a NetworkVariable.
            CurrentState = !CurrentState;

            PerformToggleLocal(CurrentState);

            if (IsSpawned)
            {
                PerformToggleRpc(CurrentState);
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            ToggleFromServer();
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