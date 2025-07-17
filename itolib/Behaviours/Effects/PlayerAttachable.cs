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
        public UnityEvent<PlayerControllerB> onAttach = new();

        /// <summary>
        ///     Whether players can attach multiple times to the same object or not.
        /// </summary>
        [Tooltip("Whether players can attach multiple times to the same object or not.")]
        public bool triggerOnce = false;

        /// <summary>
        ///     Whether players automatically attach upon entering the attach region or not.
        /// </summary>
        [Tooltip("Whether players automatically attach upon entering the attach region or not.")]
        public bool attachOnEnter = true;

        /// <summary>
        ///     Callback invoked immediately before a player detaches, with the player in question as parameter.
        /// </summary>
        [Header("Detach")]
        [Tooltip("Callback invoked immediately before a player detaches, with the player in question as parameter.")]
        public UnityEvent<PlayerControllerB> onDetach = new();

        /// <summary>
        ///     Delay in seconds until the player is forcibly detached. Can be left at '0' to attach to the player for an indefinite amount of time.
        /// </summary>
        [Tooltip("Delay in seconds until the player is forcibly detached. Can be left at '0' to attach to the player for an indefinite amount of time.")]
        public float detachTimer = 0.0f;

        /// <summary>
        ///     Whether players automatically detach upon leaving the attach region or not.
        /// </summary>
        [Tooltip("Whether players automatically detach upon leaving the attach region or not.")]
        public bool detachOnExit = false;

        /// <summary>
        ///     Parent NetworkObject to despawn once detached, if set to despawn upon detaching.
        /// </summary>
        [Header("Despawn")]
        [Tooltip("Parent NetworkObject to despawn once detached, if set to despawn upon detaching.")]
        public NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Callback invoked immediately before despawning.
        /// </summary>
        [Tooltip("Callback invoked when despawning.")]
        public UnityEvent onDespawn = new();

        /// <summary>
        ///     Destroy and despawn after the player detaches.
        /// </summary>
        [Tooltip("Destroy and despawn after the player detaches.")]
        public bool despawnOnDetach = false;

        /// <summary>
        ///     Delay in seconds until despawning after detaching the player, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning after detaching the player, to allow effects to play.")]
        public float despawnTimer = 0.0f;

        /// <summary>
        ///     Whether players attach locally, otherwise only one player can attach at a time.
        /// </summary>
        [Header("Other")]
        [Tooltip("Whether players attach locally, otherwise only one player can attach at a time.")]
        [FormerlySerializedAs("isLocalEffect")]
        public bool attachLocally = false;

        /// <summary>
        ///     The player that's currently attached.
        /// </summary>
        [HideInInspector]
        protected PlayerControllerB? attachedPlayer;

        /// <summary>
        ///     Cached transform of the currently attached player (if there is one).
        /// </summary>
        [HideInInspector]
        protected Transform attachedPlayerTransform = null!;

        /// <summary>
        ///     Cached transform of the currently attached player's gameplay camera (if there is one).
        /// </summary>
        [HideInInspector]
        protected Transform attachedPlayerGameplayCamera = null!;

        /// <summary>
        ///     Whether or not the local player is attached.
        /// </summary>
        [HideInInspector]
        protected bool localPlayerAttached;

        /// <summary>
        ///     Whether or not a player has attached once already.
        /// </summary>
        [HideInInspector]
        protected bool hasTriggered;

        /// <summary>
        ///     Condition needed for the player to be attached.
        /// </summary>
        [HideInInspector]
        protected Predicate<PlayerControllerB> attachCondition = _ => true;

        /// <summary>
        ///     Condition needed for the player to be detached.
        /// </summary>
        [HideInInspector]
        protected Predicate<PlayerControllerB> detachCondition = _ => false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public abstract void Awake();

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void Start()
        {
            // Start disabled.
            enabled = false;
        }

        /// <summary>
        ///     Attach upon coming into contact with a player.
        /// </summary>
        /// <param name="collider">Collider to attempt to attach to.</param>
        public virtual void OnTriggerEnter(Collider collider)
        {
            // Check if player should attach upon entering the attach region.
            if (!attachOnEnter)
            {
                return;
            }

            // Check if a player has attached once already.
            if (triggerOnce && hasTriggered)
            {
                return;
            }

            // Check if a player is already attached, or the local player is not the one who will be attached.
            if (attachedPlayer != null || !collider.CompareTag("Player")
                || !collider.TryGetComponent(out PlayerControllerB player) || !player.IsLocalClient())
            {
                return;
            }

            // Check if attach condition is met.
            if (attachCondition(player))
            {
                if (attachLocally)
                {
                    // Attach player locally.
                    AttachPlayerLocal(player);
                }
                else
                {
                    // Attach player on all clients.
                    AttachPlayerServerRpc(player);
                }

                // Start timer until the player is forcibly detached, if one is set.
                if (detachTimer > 0.0f)
                {
                    _ = StartCoroutine(DetachPlayerDelayed());
                }
            }
        }

        /// <summary>
        ///     Detach player upon exiting the attach region.
        /// </summary>
        /// <param name="collider">Collider to attempt to detach.</param>
        public virtual void OnTriggerExit(Collider collider)
        {
            // Check if player should detach upon leaving the attach region.
            if (!detachOnExit)
            {
                return;
            }

            // Check if the local player is not attached, or something else exited the attach region.
            if (!localPlayerAttached || !collider.CompareTag("Player")
                || !collider.TryGetComponent(out PlayerControllerB player) || !player.IsLocalClient())
            {
                return;
            }

            // Detach attached player locally.
            DetachPlayerLocal();

            if (!attachLocally)
            {
                // Detach attached player on all clients.
                DetachPlayerServerRpc();
            }
        }

        /// <summary>
        ///     Check if detach condition is met, in order to detach the player.
        /// </summary>
        public virtual void Update()
        {
            if (attachedPlayer == null)
            {
                enabled = false;

                return;
            }

            if (localPlayerAttached && detachCondition(attachedPlayer))
            {
                // Detach player locally, if the detach condition is met.
                DetachPlayerLocal();

                if (!attachLocally)
                {
                    // Detach player on all clients.
                    DetachPlayerServerRpc();
                }
            }
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public virtual void AttachPlayerLocal(PlayerControllerB player)
        {
            // Check if the player attaching is the local client.
            localPlayerAttached = player.IsLocalClient();

            // Attach given player.
            attachedPlayer = player;
            attachedPlayerTransform = player.transform;
            attachedPlayerGameplayCamera = player.gameplayCamera.transform;

            if (triggerOnce)
            {
                // Set as already having been attached to.
                hasTriggered = true;
            }

            // Invoke attach event.
            onAttach.Invoke(player);

            // Enable update loop.
            enabled = true;
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
            attachedPlayerGameplayCamera = null!;
            localPlayerAttached = false;

            // Disable update loop.
            enabled = false;
        }

        /// <summary>
        ///     Attach the given player on the server.
        /// </summary>
        /// <param name="playerReference">NetworkObject reference of the player to attach.</param>
        [ServerRpc(RequireOwnership = false)]
        public void AttachPlayerServerRpc(NetworkBehaviourReference playerReference)
        {
            if (attachedPlayer == null)
            {
                // Attach the player on all clients.
                AttachPlayerClientRpc(playerReference);
            }
        }

        /// <summary>
        ///     Attach the given player on clients.
        /// </summary>
        /// <param name="playerReference">NetworkObject reference of the player to attach.</param>
        [ClientRpc]
        public void AttachPlayerClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                AttachPlayerLocal(player);
            }
        }

        /// <summary>
        ///     Detach player on the server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void DetachPlayerServerRpc()
        {
            // Detach the player on all clients.
            DetachPlayerClientRpc();

            if (despawnOnDetach && IsSpawned && parentNetworkObject != null)
            {
                // Despawn after the configured amount of time.
                _ = StartCoroutine(DespawnDelayed());
            }
        }

        /// <summary>
        ///     Detach player on clients.
        /// </summary>
        [ClientRpc]
        public void DetachPlayerClientRpc()
        {
            if (attachedPlayer != null)
            {
                DetachPlayerLocal();
            }
        }

        /// <summary>
        ///     Coroutine to detach the player after a specified amount of time passes without the detach condition being met.
        /// </summary>
        public virtual IEnumerator DetachPlayerDelayed()
        {
            if (!localPlayerAttached)
            {
                // Exit early if not attached to the local player.
                yield break;
            }

            yield return new WaitForSeconds(detachTimer);

            // Detach attached player locally.
            DetachPlayerLocal();

            if (!attachLocally)
            {
                // Detach attached player on all clients.
                DetachPlayerServerRpc();
            }
        }

        /// <summary>
        ///     Coroutine to despawn after a specified amount of time passes after detaching.
        /// </summary>
        public virtual IEnumerator DespawnDelayed()
        {
            yield return new WaitForSeconds(despawnTimer);

            // Invoke despawn event.
            onDespawn.Invoke();

            // Despawn and destroy.
            if (parentNetworkObject != null)
            {
                parentNetworkObject.Despawn(true);
            }
        }
    }
}