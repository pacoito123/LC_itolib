using GameNetcodeStuff;
using itolib.Extensions;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Effects
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
        [SerializeField] protected bool triggerOnce;

        /// <summary>
        ///     Whether players automatically attach upon entering the attach region or not.
        /// </summary>
        [Tooltip("Whether players automatically attach upon entering the attach region or not.")]
        [SerializeField] protected bool attachOnEnter = true;

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
        [SerializeField] protected float detachTimer;

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
        [SerializeField] protected NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Callback invoked immediately before despawning.
        /// </summary>
        [Tooltip("Callback invoked immediately before despawning.")]
        [SerializeField] private UnityEvent onDespawn = new();

        /// <summary>
        ///     Whether the specified parent <c>NetworkObject</c> should despawn after the player detaches or not.
        /// </summary>
        [Tooltip("Whether the specified parent NetworkObject should despawn after the player detaches or not.")]
        [SerializeField] protected bool despawnOnDetach;

        /// <summary>
        ///     Delay in seconds until despawning after detaching the player, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning after detaching the player, to allow effects to play.")]
        [Min(0.0f)]
        [SerializeField] protected float despawnTimer;

        /// <summary>
        ///     Whether players attach locally or not, otherwise only one player can attach at a time.
        /// </summary>
        [Header("Other")]
        [Tooltip("Whether players attach locally or not, otherwise only one player can attach at a time.")]
        [FormerlySerializedAs("isLocalEffect")]
        [SerializeField] protected bool attachLocally;

        /// <summary>
        ///     The player that's currently attached.
        /// </summary>
        protected PlayerControllerB? attachedPlayer;

        /// <summary>
        ///     Cached transform of the currently attached player (if there is one).
        /// </summary>
        protected Transform attachedPlayerTransform = null!;

        /// <summary>
        ///     Whether or not the local player is attached.
        /// </summary>
        protected bool localPlayerAttached;

        /// <summary>
        ///     Whether or not a player has attached once already.
        /// </summary>
        protected bool hasTriggered;

        /// <summary>
        ///     Condition needed for the player to be attached.
        /// </summary>
        protected Predicate<PlayerControllerB> attachCondition = _ => true;

        /// <summary>
        ///     Condition needed for the player to be detached.
        /// </summary>
        protected Predicate<PlayerControllerB> detachCondition = _ => false;

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
        ///     Define the conditions for attaching and detaching, which may vary depending on inheriting scripts.
        /// </summary>
        protected abstract void Awake();

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
            if (collider.CompareTag("Player") && collider.TryGetComponent(out PlayerControllerB player))
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
            if (collider.CompareTag("Player") && collider.TryGetComponent(out PlayerControllerB _))
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
            if (localPlayerAttached && detachCondition(attachedPlayer))
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
            // Check if attach condition is met.
            if (!player.IsLocalClient() || !attachCondition(player))
            {
                return;
            }

            // Check if the player attaching should be sent to all clients.
            if (!IsSpawned || attachLocally)
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
        [Rpc(SendTo.ClientsAndHost)]
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
            // Check if a player has attached once already.
            if (triggerOnce)
            {
                if (!hasTriggered)
                {
                    // Set as already having been attached to.
                    hasTriggered = true;
                }
                else
                {
                    return;
                }
            }

            // Set whether or not the local player is attached.
            localPlayerAttached = player.IsLocalClient();

            // Attach given player.
            attachedPlayer = player;
            attachedPlayerTransform = player.transform;

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
        [Rpc(SendTo.NotMe)]
        private void DetachPlayerRpc()
        {
            // Invoke despawn event.
            onDespawn.Invoke();

            // Check if parent NetworkObject is spawned.
            if (parentNetworkObject != null && parentNetworkObject.IsSpawned)
            {
                // Despawn and destroy parent NetworkObject.
                parentNetworkObject.Despawn(true);
            }
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
            }

            // Remove attached player.
            attachedPlayer = null;
            attachedPlayerTransform = null!;
            localPlayerAttached = false;

            // Disable update loop.
            enabled = false;
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
            yield return new WaitForSeconds(detachTimer);

            // Detach player for all clients, unless not spawned or attached locally.
            DetachPlayer();
        }

        /// <summary>
        ///     <c>Coroutine</c> to despawn the parent <c>NetworkObject</c> after a specified amount of time.
        /// </summary>
        private IEnumerator DespawnNetworkObjectDelayed()
        {
            // Wait for despawn timer.
            yield return new WaitForSeconds(despawnTimer);

            // Despawn parent NetworkObject on the server.
            DespawnNetworkObjectRpc();
        }
    }
}