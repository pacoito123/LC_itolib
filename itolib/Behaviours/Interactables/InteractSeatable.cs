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
        public AudioSource? seatableSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent? onPlayerSit;

        /// <summary>
        ///     TODO.
        /// </summary>
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

            /* holdingInteractEvent.AddListener(amount =>
            {
                if (LocalPlayerSeated && amount >= 0.9f)
                {
                    ExitChairLocal(teleport: true);
                    ExitChairServerRpc();
                }
            }); */
        }

        private new void Update()
        {
            base.Update();

            if (SittingPlayer == null)
            {
                return;
            }

            // if (LocalPlayerSeated && Keyboard.current[(Key)keyToHold].isPressed)
            if (LocalPlayerSeated && Keyboard.current[Key.Space].isPressed)
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