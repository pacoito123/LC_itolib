using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class InteractSeatable : InteractTrigger
    {
        /// <summary>
        ///     The player currently sitting on this object.
        /// </summary>
        public PlayerControllerB? SittingPlayer { get; private set; }

        /// <summary>
        ///     Whether or not the local player is sitting on this object.
        /// </summary>
        public bool LocalPlayerSeated { get; private set; } = false;

        /// <summary>
        ///     
        /// </summary>
        public Vector3 PlayerExitPoint { get; private set; } = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        public InputAction? ActionToExit { get; private set; }

        /// <summary>
        ///     Key required to be held for the player to hang on to the platform. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows players to
        ///     remain attached without holding anything, until being detached through other means (e.g. 'detachTimer').
        /// </summary>
        /// <remarks>Probably worth looking into adding controller support for this.</remarks>
        [Header("Interact Seatable")]
        [Tooltip("Key required to be held for the player to hang on to the platform. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows "
            + "players to remain attached without holding anything, until being detached through other means (e.g. 'detachTimer').")]
        public string actionToExit = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioSource? seatableSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPlayerSit;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPlayerStand;

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

        private void Awake()
        {
            onInteractEarlyOtherClients.AddListener(player =>
            {
                if (player?.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId
                    && player.TryGetComponent(out NetworkObject playerReference))
                {
                    SetPlayerOnSeatServerRpc(playerReference);
                }
            });

            ActionToExit = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToExit);
        }

        private new void Update()
        {
            base.Update();

            if (SittingPlayer == null || ActionToExit == null)
            {
                return;
            }

            if (LocalPlayerSeated && ActionToExit.IsPressed())
            {
                ExitChairLocal(true);
                ExitChairServerRpc();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerSitting"></param>
        public void SetPlayerOnSeatLocal(PlayerControllerB playerSitting)
        {
            if (GameNetworkManager.Instance.localPlayerController.actualClientId == playerSitting.actualClientId)
            {
                PlayerExitPoint = playerSitting.visorCamera.transform.position;
                LocalPlayerSeated = true;
            }

            SittingPlayer = playerSitting;
            interactable = false;

            onPlayerSit?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ExitChairLocal(bool teleport = false)
        {
            if (SittingPlayer == null)
            {
                return;
            }

            if (teleport)
            {
                SittingPlayer.TeleportPlayer(PlayerExitPoint);
            }

            onPlayerStand?.Invoke();

            PlayerExitPoint = Vector3.zero;
            LocalPlayerSeated = false;
            SittingPlayer = null;

            interactable = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerOnSeatServerRpc(NetworkObjectReference playerReference)
        {
            if (LocalPlayerSeated || SittingPlayer != null)
            {
                return;
            }

            SetPlayerOnSeatClientRpc(playerReference);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void SetPlayerOnSeatClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
            {
                SetPlayerOnSeatLocal(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ExitChairServerRpc()
        {
            ExitChairClientRpc();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void ExitChairClientRpc()
        {
            ExitChairLocal();
        }
    }
}