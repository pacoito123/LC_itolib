using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Util;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Represents an effect or concept that continually affects a player (attach), and eventually stops (detach).
    /// </summary>
    public abstract class PlayerAttachable : NetworkBehaviour
    {
        /// <summary>
        ///     Callback invoked immediately after a player attaches, with the player in question as parameter.
        /// </summary>
        [Header("Attach")]
        [Tooltip("Callback invoked immediately after a player attaches, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onAttach = new();

        /// <summary>
        ///     Whether players can attach multiple times to the same object or not.
        /// </summary>
        [Tooltip("Whether players can attach multiple times to the same object or not.")]
        [SerializeField] private bool triggerOnce;

        /// <summary>
        ///     Whether players automatically attach upon entering the attach region or not.
        /// </summary>
        [Tooltip("Whether players automatically attach upon entering the attach region or not.")]
        [SerializeField] private bool attachOnEnter = true;

        /// <summary>
        ///     Callback invoked immediately before a player detaches, with the player in question as parameter.
        /// </summary>
        [Header("Detach")]
        [Tooltip("Callback invoked immediately before a player detaches, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDetach = new();

        /// <summary>
        ///     Delay in seconds until the player is forcibly detached. Can be left at <c>0</c> to attach to the player for an indefinite amount of time.
        /// </summary>
        [Tooltip("Delay in seconds until the player is forcibly detached. Can be left at '0' to attach to the player for an indefinite amount of time.")]
        [Min(0.0f)]
        [SerializeField] private float detachTimer;

        /// <summary>
        ///     Whether players automatically detach upon leaving the attach region or not.
        /// </summary>
        [Tooltip("Whether players automatically detach upon leaving the attach region or not.")]
        [SerializeField] protected bool detachOnExit;

        /// <summary>
        ///     Parent <c>NetworkObject</c> to attempt to despawn once detached, if set to despawn upon detaching.
        /// </summary>
        [Header("Despawn")]
        [Tooltip("Parent NetworkObject to attempt to despawn once detached, if set to despawn upon detaching.")]
        [SerializeField] private NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Callback invoked immediately before despawning.
        /// </summary>
        [Tooltip("Callback invoked immediately before despawning.")]
        [SerializeField] private UnityEvent onDespawn = new();

        /// <summary>
        ///     Whether the specified parent <c>NetworkObject</c> should despawn after the player detaches or not.
        /// </summary>
        [Tooltip("Whether the specified parent NetworkObject should despawn after the player detaches or not.")]
        [SerializeField] private bool despawnOnDetach;

        /// <summary>
        ///     Delay in seconds until despawning after detaching the player, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning after detaching the player, to allow effects to play.")]
        [Min(0.0f)]
        [SerializeField] private float despawnTimer;

        /// <summary>
        ///     Whether players attach locally or not, otherwise only one player can attach at a time.
        /// </summary>
        [Header("Other")]
        [Tooltip("Whether players attach locally or not, otherwise only one player can attach at a time.")]
        [SerializeField] protected bool attachLocally;

        /// <summary>
        ///     The player that's currently attached.
        /// </summary>
        protected PlayerControllerB? attachedPlayer;

        /// <summary>
        ///     Cached <c>Transform</c> of the currently attached player (if there is one).
        /// </summary>
        protected Transform attachedPlayerTransform = null!;

        /// <summary>
        ///     Cached <c>Transform</c> of the currently attached player's camera (if there is one).
        /// </summary>
        protected Transform attachedPlayerCameraTransform = null!;

        /// <summary>
        ///     Whether or not the local player is attached.
        /// </summary>
        protected bool localPlayerAttached;

        /// <summary>
        ///     Whether players should be able to attach or not, acting as a kill switch of sorts.
        /// </summary>
        protected bool attachDisabled;

        /// <summary>
        ///     Define any specific default values that should be applied for an inheriting script.
        /// </summary>
        protected virtual void Reset()
        {
            /* if (TryGetComponent(out Collider collider))
            {
                collider.isTrigger = true;
                collider.layerOverridePriority = 100;

                int excludeMask = ~LayerMask.GetMask("Player", "Enemies", "PlayerRagdoll");
                collider.excludeLayers = (collider.excludeLayers == 0) ? excludeMask : (collider.excludeLayers & excludeMask);
            } */
        }

        /// <summary>
        ///     Define the conditions needed for the player to be attached, which may vary depending on inheriting scripts.
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected abstract bool AttachCondition(PlayerControllerB player);

        /// <summary>
        ///     Define the conditions needed for the player to be detached, which may vary depending on inheriting scripts.
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected abstract bool DetachCondition(PlayerControllerB player);

        /// <summary>
        ///     Start with the script disabled, as the <c>Update()</c> loop should only run while a player is attached.
        /// </summary>
        protected virtual void Start()
        {
            // Start disabled.
            enabled = false;
        }

        /// <summary>
        ///     Attach upon coming into contact with a player.
        /// </summary>
        /// <param name="collider"><c>Collider</c> to attempt to attach.</param>
        protected virtual void OnTriggerEnter(Collider collider)
        {
            // Check if player should attach upon entering the attach region.
            if (!attachOnEnter || attachedPlayer != null)
            {
                return;
            }

            // Attach player that entered the attach region.
            if (collider.TryGetComponent(out PlayerControllerB player))
            {
                AttachPlayer(player);
            }
        }

        /// <summary>
        ///     Detach player upon exiting the attach region.
        /// </summary>
        /// <param name="collider"><c>Collider</c> to attempt to detach.</param>
        protected virtual void OnTriggerExit(Collider collider)
        {
            // Check if player should detach upon leaving the attach region.
            if (!detachOnExit || attachedPlayer == null || !localPlayerAttached)
            {
                return;
            }

            // Detach player that exited the attach region.
            if (collider.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                DetachPlayer();
            }
        }

        /// <summary>
        ///     Check if detach condition is met, in order to detach the player.
        /// </summary>
        protected virtual void Update()
        {
            if (attachedPlayer == null)
            {
                enabled = false;

                return;
            }

            // Detach attached player, if the detach condition is met for the local client.
            if (localPlayerAttached && (attachDisabled || DetachCondition(attachedPlayer)))
            {
                DetachPlayer();
            }
        }

        /// <summary>
        ///     Attach player for all clients, unless not spawned or attaching locally.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public virtual void AttachPlayer(PlayerControllerB player)
        {
            // Check if attaching is disabled.
            if (attachDisabled)
            {
                return;
            }

            // Check if attach condition is met.
            if (!player.IsLocalClient() || !AttachCondition(player))
            {
                return;
            }

            // Check if the player attaching should be sent to all clients.
            if (attachLocally || !IsSpawned)
            {
                // Attach player on the local client.
                AttachPlayerLocal(player);
            }
            else
            {
                // Attach player on all clients.
                AttachPlayerServerRpc(player);
            }
        }

        /// <summary>
        ///     Attach the given player on the server.
        /// </summary>
        /// <param name="playerReference">Network reference of the player to attach.</param>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void AttachPlayerServerRpc(NetworkBehaviourReference playerReference)
        {
            if (attachedPlayer == null)
            {
                // Attach the player on all clients.
                AttachPlayerClientRpc(playerReference);
            }
        }

        /// <summary>
        ///     Attach the given player on all clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player to attach.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void AttachPlayerClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Attach the player on the local client.
                AttachPlayerLocal(player);
            }
        }

        /// <summary>
        ///     Attach local player for all clients, unless not spawned or attaching locally.
        /// </summary>
        public virtual void AttachPlayerLocal()
        {
            AttachPlayer(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public virtual void AttachPlayerLocal(PlayerControllerB player)
        {
            // Check if attaching is disabled.
            if (attachDisabled)
            {
                return;
            }

            // Check if attach condition is met, when attaching locally.
            if (attachLocally && (!player.IsLocalClient() || !AttachCondition(player)))
            {
                return;
            }

            // Set whether or not the local player is attached.
            localPlayerAttached = player.IsLocalClient();

            // Attach given player.
            attachedPlayer = player;
            attachedPlayerTransform = player.transform;
            attachedPlayerCameraTransform = player.gameplayCamera.transform;

            // Invoke attach event.
            onAttach.Invoke(player);

            // Enable update loop.
            enabled = true;

            if (detachTimer > 0.0f)
            {
                // Start timer until the player is forcibly detached, if one is set.
                _ = StartCoroutine(DetachPlayerDelayed()); // TODO: Switch to handling it through a timer, instead of a separate Coroutine.
            }
        }

        /// <summary>
        ///     Detach player for all clients, unless not spawned or attached locally.
        /// </summary>
        public virtual void DetachPlayer()
        {
            // Check if attached player is the local client.
            if (attachedPlayer == null || !localPlayerAttached)
            {
                return;
            }

            // Detach player on the local client.
            DetachPlayerLocal();

            // Check if object is spawned.
            if (IsSpawned)
            {
                // Check if detaching should be sent to all other clients.
                if (!attachLocally)
                {
                    // Detach player on all other clients.
                    DetachPlayerRpc();
                }

                // Check if detaching should despawn the object.
                if (despawnOnDetach)
                {
                    // Despawn after the configured amount of time.
                    _ = StartCoroutine(DespawnNetworkObjectDelayed());
                }
            }
        }

        /// <summary>
        ///     Detach player on all other clients.
        /// </summary>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void DetachPlayerRpc()
        {
            // Detach player on the local client.
            DetachPlayerLocal();
        }

        /// <summary>
        ///     Detach player on the local client.
        /// </summary>
        public virtual void DetachPlayerLocal()
        {
            if (attachedPlayer != null)
            {
                // Invoke detach event.
                onDetach.Invoke(attachedPlayer);

                if (triggerOnce)
                {
                    // Disable attaching after triggering once, if set to do so.
                    EnableAttaching(false);
                }
            }

            // Remove attached player.
            attachedPlayer = null;
            attachedPlayerTransform = null!;
            attachedPlayerCameraTransform = null!;
            localPlayerAttached = false;

            // Disable update loop.
            enabled = false;
        }

        /// <summary>
        ///     Switch player to attach for all clients, unless already attached or no player is attached.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public void TransferPlayer(PlayerControllerB player)
        {
            // Check if attaching is disabled, or attaching locally.
            if (attachDisabled || attachLocally)
            {
                return;
            }

            // Check if attach condition is met.
            if (!player.IsLocalClient() || !AttachCondition(player))
            {
                return;
            }

            // Check if attached player is eligible to be transferred.
            if (!localPlayerAttached && attachedPlayer != null && IsSpawned)
            {
                // Transfer player on all clients.
                TransferPlayerServerRpc(player);
            }
        }

        /// <summary>
        ///     Switch player to attach on the server.
        /// </summary>
        /// <param name="playerReference">Network reference of the player to attach.</param>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TransferPlayerServerRpc(NetworkBehaviourReference playerReference)
        {
            if (attachedPlayer != null)
            {
                // Attach the player on all clients.
                TransferPlayerClientRpc(playerReference);
            }
        }

        /// <summary>
        ///     Switch player to attach on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player to attach.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void TransferPlayerClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Detach the player on the local client.
                DetachPlayerLocal();

                // Attach the player on the local client.
                AttachPlayerLocal(player);
            }
        }

        /// <summary>
        ///     Despawn parent <c>NetworkObject</c> on the server.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void DespawnNetworkObjectRpc()
        {
            // Invoke despawn event.
            onDespawn.Invoke();

            if (parentNetworkObject != null && parentNetworkObject.IsSpawned)
            {
                // Despawn and destroy parent NetworkObject.
                parentNetworkObject.Despawn(true);
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to detach the player after a specified amount of time passes without the detach condition being met.
        /// </summary>
        private IEnumerator DetachPlayerDelayed()
        {
            // Wait for detach timer.
            yield return Yielders.WaitForSeconds(detachTimer);

            // Detach player for all clients, unless not spawned or attached locally.
            DetachPlayer();
        }

        /// <summary>
        ///     <c>Coroutine</c> to despawn the parent <c>NetworkObject</c> after a specified amount of time.
        /// </summary>
        private IEnumerator DespawnNetworkObjectDelayed()
        {
            // Wait for despawn timer.
            yield return Yielders.WaitForSeconds(despawnTimer);

            // Despawn parent NetworkObject on the server.
            DespawnNetworkObjectRpc();
        }

        /// <summary>
        ///     Enable or disable players being able to attach.
        /// </summary>
        /// <param name="enabled">Whether attaching should be enabled or not.</param>
        public void EnableAttaching(bool enabled)
        {
            attachDisabled = !enabled;
        }

        /// <summary>
        ///     Toggle players being able to attach.
        /// </summary>
        public void ToggleAttaching()
        {
            attachDisabled = !attachDisabled;
        }
    }
}