using GameNetcodeStuff;
using itolib.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Obsolete("Could use a regular InteractTrigger along with PlayerSeater instead.")]
    public class InteractSeatable : InteractTrigger
    {
        /// <summary>
        ///     Action required for the player to stop sitting down. See 'UnityEngine.InputSystem.Key' for number values.
        /// </summary>
        [Space(5.0f)]
        [Header("Interact Seatable")]
        [Tooltip("Action required for the player to stop sitting down. See 'UnityEngine.InputSystem.Key' for number values.")]
        [SerializeField] private string actionToExit = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioSource? seatableSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onPlayerSit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onPlayerStand = new();

        /// <summary>
        ///     The player currently sitting on this object.
        /// </summary>
        private PlayerControllerB? sittingPlayer;

        /// <summary>
        ///     Whether or not the local player is sitting on this object.
        /// </summary>
        private bool localPlayerSeated;

        /// <summary>
        ///     
        /// </summary>
        private Vector3 playerExitPoint = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        private InputAction? playerAction;

        /// <summary>
        ///     Set default seat properties.
        /// </summary>
        /// <remarks>NOTE: Still need to set the Transform points for the InteractTrigger, but the settings here are the vanilla Cruiser seats.</remarks>
        private void Reset()
        {
            hoverTip = "Sit down : [LMB]";
            oneHandedItemAllowed = true;
            holdInteraction = true;
            timeToHold = 0.2f;
            cooldownTime = 0.3f;
            specialCharacterAnimation = true;
            stopAnimationManually = true;
            stopAnimationString = "SA_stopAnimation";
            hidePlayerItem = true;
            animationWaitTime = 2f;
            animationString = "SA_Truck";
            lockPlayerPosition = true;
            clampLooking = true;
            setVehicleAnimation = true;
            minVerticalClamp = 50f;
            maxVerticalClamp = -70f;
            horizontalClamp = 120;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            onInteractEarlyOtherClients.AddListener(player =>
            {
                if (IsSpawned && sittingPlayer == null && player.IsLocalClient())
                {
                    SetPlayerOnSeatRpc(player);
                }
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Start()
        {
            base.Start();

            // Try obtain player action required for the player to get back up.
            if (!GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToExit))
            {
                Plugin.StaticLogger.LogWarning($"Could not find movement action '{actionToExit}' defined for PlayerSeater component in '{name}'!");
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Update()
        {
            base.Update();

            if (!localPlayerSeated || sittingPlayer == null)
            {
                return;
            }

            if (playerAction == null || playerAction.WasPressedThisFrame())
            {
                ExitChairLocal(true);

                if (IsSpawned)
                {
                    ExitChairRpc(teleport: true);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerSitting"></param>
        private void SetPlayerOnSeatLocal(PlayerControllerB playerSitting)
        {
            if (playerSitting.IsLocalClient())
            {
                playerExitPoint = playerSitting.visorCamera.transform.position;
                localPlayerSeated = true;
            }

            sittingPlayer = playerSitting;
            interactable = false;

            onPlayerSit.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void ExitChairLocal(bool teleport = false)
        {
            if (sittingPlayer == null)
            {
                return;
            }

            if (teleport)
            {
                sittingPlayer.TeleportPlayer(playerExitPoint);
            }

            onPlayerStand.Invoke();

            playerExitPoint = Vector3.zero;
            localPlayerSeated = false;
            sittingPlayer = null;

            interactable = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void SetPlayerOnSeatRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                SetPlayerOnSeatLocal(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="teleport"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void ExitChairRpc(bool teleport = false)
        {
            ExitChairLocal(teleport);
        }
    }
}