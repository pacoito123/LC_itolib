using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerSeater : PlayerAttachable
    {
        /// <summary>
        ///     Key required to be pressed for the player to unsit. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows players to
        ///     remain attached until being detached through other means (e.g. 'detachTimer').
        /// </summary>
        [Header("Player Seater")]
        [Tooltip("Key required to be pressed for the player to unsit. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows "
            + "players to remain attached until being detached through other means (e.g. 'detachTimer').")]
        public string actionToUnseat = "Jump";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool hidePlayerItem = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool enterVehicleAnimation;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float cameraTurnSpeed = 20.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onPlayerSit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onPlayerUnsit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private InputAction? playerAction;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform playerCamera = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead && !player.inAnimationWithEnemy && !player.inSpecialInteractAnimation;
            detachCondition = player => player.isPlayerDead || (playerAction != null && playerAction.IsPressed());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            if (actionToUnseat.Length > 0)
            {
                // Get action (key) that must be pressed, if one is set.
                playerAction = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToUnseat);
            }

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Update()
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                if (cameraTurnSpeed > 0.0f && playerCamera != null)
                {
                    attachedPlayerTransform.rotation = Quaternion.Lerp(attachedPlayerTransform.rotation, transform.rotation, Time.deltaTime * cameraTurnSpeed);

                    playerCamera.localEulerAngles = new(Mathf.LerpAngle(playerCamera.localEulerAngles.x, attachedPlayer.syncFullCameraRotation.x,
                        cameraTurnSpeed * Time.deltaTime), Mathf.LerpAngle(playerCamera.localEulerAngles.y, attachedPlayer.syncFullCameraRotation.y,
                        cameraTurnSpeed * Time.deltaTime), Mathf.LerpAngle(playerCamera.localEulerAngles.z, attachedPlayer.syncFullCameraRotation.z,
                        cameraTurnSpeed * Time.deltaTime));
                }
            }

            base.Update();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            if (!player.IsLocalClient())
            {
                return;
            }

            base.AttachPlayerLocal(player);

            PlayerSitLocal(player);

            if (IsSpawned)
            {
                PlayerSitServerRpc(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (attachedPlayer == null || (!attachLocally && !attachedPlayer.IsLocalClient()))
            {
                return;
            }

            PlayerSitLocal(attachedPlayer, true);

            if (IsSpawned)
            {
                PlayerSitServerRpc(attachedPlayer, unsit: true);
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="unsit"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PlayerSitServerRpc(NetworkBehaviourReference playerReference, bool unsit = false)
        {
            PlayerSitClientRpc(playerReference, unsit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="unsit"></param>
        [ClientRpc]
        public void PlayerSitClientRpc(NetworkBehaviourReference playerReference, bool unsit = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                PlayerSitLocal(player, unsit);
            }
        }

        private void PlayerSitLocal(PlayerControllerB player, bool unsit = false)
        {
            if (!unsit)
            {
                if (enterVehicleAnimation)
                {
                    player.inVehicleAnimation = true;
                }

                player.syncFullCameraRotation = player.gameplayCamera.transform.localEulerAngles;

                player.Crouch(false);
                player.inSpecialInteractAnimation = true;

                if (player.IsOwner)
                {
                    playerCamera = player.gameplayCamera.transform;
                    player.UpdateSpecialAnimationValue(true, (short)transform.eulerAngles.y, 0.0f, false);
                }

                player.minVerticalClamp = 50.0f;
                player.maxVerticalClamp = -70.0f;
                player.horizontalClamp = 120.0f;
                player.clampLooking = true;

                if (hidePlayerItem && player.currentlyHeldObjectServer != null)
                {
                    player.currentlyHeldObjectServer.EnableItemMeshes(false);
                }

                player.playerBodyAnimator.ResetTrigger("SA_Truck");
                player.playerBodyAnimator.SetTrigger("SA_Truck");

                onPlayerSit.Invoke(player);
            }
            else
            {
                player.inVehicleAnimation = false;
                player.inSpecialInteractAnimation = false;

                if (player.IsOwner)
                {
                    player.UpdateSpecialAnimationValue(true, 0, 0.0f, false);
                }

                player.gameplayCamera.transform.localEulerAngles = Vector3.zero;
                player.ladderCameraHorizontal = 0.0f;
                player.clampLooking = false;

                if (hidePlayerItem && player.currentlyHeldObjectServer != null)
                {
                    player.currentlyHeldObjectServer.EnableItemMeshes(true);
                }

                player.playerBodyAnimator.SetTrigger("SA_stopAnimation");

                onPlayerUnsit.Invoke(player);
            }
        }
    }
}