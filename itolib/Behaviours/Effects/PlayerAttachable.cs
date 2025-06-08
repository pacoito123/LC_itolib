using GameNetcodeStuff;
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
        ///     The player that's currently attached.
        /// </summary>
        public PlayerControllerB? AttachedPlayer { get; protected set; }

        /// <summary>
        ///     Cached transform of the currently attached player (if there is one).
        /// </summary>
        public Transform AttachedPlayerTransform { get; protected set; } = null!;

        /// <summary>
        ///     Cached transform of the currently attached player's gameplay camera (if there is one).
        /// </summary>
        public Transform AttachedPlayerGameplayCamera { get; protected set; } = null!;

        /// <summary>
        ///     Whether or not the local player is attached.
        /// </summary>
        public bool LocalPlayerAttached { get; protected set; } = false;

        /// <summary>
        ///     Whether or not a player has attached once already.
        /// </summary>
        public bool HasTriggered { get; protected set; } = false;

        /// <summary>
        ///     Condition needed for the player to be attached.
        /// </summary>
        public Predicate<PlayerControllerB> AttachCondition { get; protected set; } = _ => true;

        /// <summary>
        ///     Condition needed for the player to be detached.
        /// </summary>
        public Predicate<PlayerControllerB> DetachCondition { get; protected set; } = _ => false;

        /// <summary>
        ///     Callback invoked immediately after a player attaches, with the player in question as parameter.
        /// </summary>
        [Header("Attach")]
        [Tooltip("Callback invoked immediately after a player attaches, with the player in question as parameter.")]
        public UnityEvent<PlayerControllerB>? onAttach;

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
        public UnityEvent<PlayerControllerB>? onDetach;

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
        public UnityEvent? onDespawn;

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
            if (triggerOnce && HasTriggered)
            {
                return;
            }

            // Check if a player is already attached, or the local player is not the one who will be attached.
            if (AttachedPlayer != null || !collider.CompareTag("Player")
                || !collider.TryGetComponent(out PlayerControllerB player)
                || player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                return;
            }

            // Check if attach condition is met.
            if (AttachCondition(player))
            {
                if (attachLocally)
                {
                    // Attach player locally.
                    AttachPlayerLocal(player);
                }
                else
                {
                    // Attach player on all clients.
                    AttachPlayerServerRpc(player.GetComponent<NetworkObject>());
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
            if (!LocalPlayerAttached || !collider.CompareTag("Player")
                || !collider.TryGetComponent(out PlayerControllerB player)
                || player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
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
            if (LocalPlayerAttached && AttachedPlayer != null && DetachCondition(AttachedPlayer))
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
            if (player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                LocalPlayerAttached = true;
            }

            // Attach given player.
            AttachedPlayer = player;
            AttachedPlayerTransform = player.transform;
            AttachedPlayerGameplayCamera = player.gameplayCamera.transform;

            if (triggerOnce)
            {
                // Set as already having been attached to.
                HasTriggered = true;
            }

            // Invoke attach event.
            onAttach?.Invoke(player);
        }

        /// <summary>
        ///     Detach player on the local client.
        /// </summary>
        public virtual void DetachPlayerLocal()
        {
            if (AttachedPlayer != null)
            {
                // Invoke detach event.
                onDetach?.Invoke(AttachedPlayer);
            }

            // Remove attached player.
            AttachedPlayer = null;
            AttachedPlayerTransform = null!;
            AttachedPlayerGameplayCamera = null!;
            LocalPlayerAttached = false;
        }

        /// <summary>
        ///     Attach the given player on the server.
        /// </summary>
        /// <param name="playerReference">NetworkObject reference of the player to attach.</param>
        [ServerRpc(RequireOwnership = false)]
        public void AttachPlayerServerRpc(NetworkObjectReference playerReference)
        {
            if (AttachedPlayer == null)
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
        public void AttachPlayerClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
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
            if (AttachedPlayer != null)
            {
                DetachPlayerLocal();
            }
        }

        /// <summary>
        ///     Coroutine to detach the player after a specified amount of time passes without the detach condition being met.
        /// </summary>
        public virtual IEnumerator DetachPlayerDelayed()
        {
            if (!LocalPlayerAttached)
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
            onDespawn?.Invoke();

            // Despawn and destroy.
            parentNetworkObject?.Despawn(true);
        }
    }
}