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
    ///     Represents a surface upon which attaching players may rest their rear, should they need a short break.
    /// </summary>
    public class PlayerSeater : PlayerAttachable
    {
        /// <summary>
        ///    Position and rotation to be used for sitting players.
        /// </summary>
        /// <remarks>If left empty, the <c>Transform</c> of the object this component is attached to will be used.</remarks>
        [Header("Player Seater")]
        [Tooltip("Position and rotation to be used for sitting players. If left empty, the Transform of the object this component is attached to will be used.")]
        [SerializeField] private Transform? seatPosition;

        /// <summary>
        ///    <c>PlayerAction</c> required to be held for the player to unseat. See <c>PlayerActions.MovementActions</c> for a list of valid player actions.
        /// </summary>
        /// <remarks>Leaving it blank prevents the player from leaving their seat on their own, so they must be detached through other means (e.g. <c>detachTimer</c>).</remarks>
        [Tooltip("PlayerAction required to be held for the player to unseat. See 'PlayerActions.MovementActions' for a list of valid player actions. Leaving "
            + "it blank prevents the player from leaving their seat on their own, so they must be detached through other means (e.g. detach timer).")]
        [SerializeField] private string actionToUnseat = "Jump";

        /// <summary>
        ///     Trigger parameter to activate in the attaching player's <c>Animator</c> in order to enter a sitting animation.
        /// </summary>
        [Tooltip("Trigger parameter to activate in the attaching player's Animator in order to enter a sitting animation.")]
        [SerializeField] private string sittingAnimation = "SA_Truck";

        /// <summary>
        ///     Whether to hide the sitting player's held item or not.
        /// </summary>
        [Tooltip("Whether to hide the sitting player's held item or not.")]
        [SerializeField] private bool hidePlayerItem = true;

        /// <summary>
        ///     Whether to toggle the sitting player's <c>inVehicleAnimation</c> field or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> NOT required for the player to sit down, and having it enabled disables player position syncing with other clients.</remarks>
        [Tooltip("Whether to toggle the sitting player's 'inVehicleAnimation' field or not. NOTE: NOT required for the player to sit down, and having "
            + "it enabled disables player position syncing with other clients.")]
        [SerializeField] private bool enterVehicleAnimation;

        /// <summary>
        ///     Ramping speed for the sitting player rotating towards the seat's rotation.
        /// </summary>
        /// <remarks>If left at <c>0</c>, the player will not be rotated when sitting down and will remain facing the direction they were looking towards.</remarks>
        [Header("Camera")]
        [Tooltip("Ramping speed for the sitting player rotating towards the seat's rotation. If left at '0', the player will not be rotated when sitting "
            + "down and will remain facing the direction they were looking towards.")]
        [Min(0.0f)]
        [SerializeField] private float cameraTurnSpeed = 20.0f;

        /// <summary>
        ///     Whether the sitting player's camera should be clamped between specified angles or not.
        /// </summary>
        [Tooltip("Whether the sitting player's camera should be clamped between specified angles or not.")]
        [SerializeField] private bool cameraClamping = true;

        /// <summary>
        ///     Minimum vertical value for the player's camera clamp, in degrees.
        /// </summary>
        /// <remarks>Defines how far down the sitting player will be able to look.</remarks>
        [Tooltip("Minimum vertical value for the player's camera clamp, in degrees. Defines how far down the sitting player will be able to look.")]
        [Range(-180.0f, 0.0f)]
        [SerializeField] private float minVerticalClamp = -50.0f;

        /// <summary>
        ///     Maximum vertical value for the player's camera clamp, in degrees.
        /// </summary>
        /// <remarks>Defines how far up the sitting player will be able to look.</remarks>
        [Tooltip("Maximum vertical value for the player's camera clamp, in degrees. Defines how far up the sitting player will be able to look.")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float maxVerticalClamp = 70.0f;

        /// <summary>
        ///     Horizontal value for the player's camera clamp, in degrees.
        /// </summary>
        /// <remarks>Defines how far to the sides the sitting player will be able to look.</remarks>
        [Tooltip("Horizontal value for the player's camera clamp, in degrees. Defines how far to the sides the sitting player will be able to look.")]
        [Range(0.0f, 360.0f)]
        [SerializeField] private float horizontalClamp = 120.0f;

        /// <summary>
        ///     Callback invoked when a player begins sitting down, with the player in question as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when a player begins sitting down, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onPlayerSit = new();

        /// <summary>
        ///     Callback invoked after a player unseats, with the player in question as parameter.
        /// </summary>
        [Tooltip("Callback invoked after a player unseats, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onPlayerUnsit = new();

        /// <summary>
        ///     <c>Transform</c> to be used by sitting players.
        /// </summary>
        private Transform targetTransform = null!;

        /// <summary>
        ///     Cached reference to the player action required to be pressed in order to unseat.
        /// </summary>
        private InputAction? playerAction;

        /* /// <summary>
        ///     Cached transform of the currently attached player's camera.
        /// </summary>
        private Transform playerCamera = null!; */

        /// <summary>
        ///     Hash of the trigger parameter to activate upon sitting down.
        /// </summary>
        private int sittingAnimationID = -1;

        /// <summary>
        ///     Hash of the trigger parameter to activate to reset the player's animations.
        /// </summary>
        private readonly int stopAnimationID = Animator.StringToHash("SA_stopAnimation");

        /// <summary>
        ///     Hash of the trigger parameter ..
        /// </summary>
        private readonly int notInSpecialAnimID = Animator.StringToHash("notInSpecialAnim");

        /// <summary>
        ///     Hash of the bool parameter to toggle the player's crouching animation.
        /// </summary>
        private readonly int crouchingID = Animator.StringToHash("crouching");

        /// <summary>
        ///     Attach if the player is alive.
        ///     Detach if the player is dead, or presses the set (<c>playerAction</c>).
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead;
            detachCondition = player => player.isPlayerDead || (actionToUnseat.Length > 0 && (playerAction == null || playerAction.IsPressed()));
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            targetTransform = (seatPosition != null) ? seatPosition : transform;

            // Try obtain player action required for the player to unseat.
            if (actionToUnseat.Length > 0 && !GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToUnseat))
            {
                Plugin.StaticLogger.LogWarning($"Could not find movement action '{actionToUnseat}' defined for PlayerSeater component in '{name}'!");
            }

            if (sittingAnimation.Length > 0)
            {
                // Get ID of the player sitting animation, if one is set.
                sittingAnimationID = Animator.StringToHash(sittingAnimation);
            }

            base.Start();
        }

        /// <summary>
        ///     Handle rotating player who is sitting down.
        /// </summary>
        protected override void Update()
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                if (cameraTurnSpeed > 0.0f)
                {
                    // Rotate sitting player towards the target rotation.
                    attachedPlayerTransform.rotation = Quaternion.Lerp(attachedPlayerTransform.rotation, targetTransform.rotation,
                        Time.deltaTime * cameraTurnSpeed);

                    /* attachedPlayer.syncFullRotation = attachedPlayerTransform.rotation.eulerAngles;

                    playerCamera.rotation = Quaternion.Lerp(playerCamera.localRotation, Quaternion.Euler(attachedPlayer.syncFullCameraRotation),
                        Time.deltaTime * cameraTurnSpeed); */
                }
            }

            base.Update();
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            // Check if player was attached successfully.
            if (attachedPlayer != null)
            {
                // Sit player down for all clients, unless not spawned or attaching locally.
                PlayerSit(attachedPlayer, unsit: false);
            }
        }

        /// <summary>
        ///     Detach player on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            // Check if player is attached.
            if (attachedPlayer != null)
            {
                // Unseat player for all clients, unless not spawned or attached locally.
                PlayerSit(attachedPlayer, unsit: true);
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     Sit given player down for all clients, unless not spawned or attaching locally.
        /// </summary>
        /// <param name="player">Player to sit down.</param>
        /// <param name="unsit">Whether the player is unseating or not.</param>
        private void PlayerSit(PlayerControllerB player, bool unsit)
        {
            // Check if player sitting down is the local client.
            if (!player.IsLocalClient())
            {
                return;
            }

            // Sit player down on the local client.
            PlayerSitLocal(player, unsit);

            if (IsSpawned) // TODO: Separate local field?
            {
                // Sit player down on all clients.
                PlayerSitServerRpc(player, unsit);
            }
        }

        /// <summary>
        ///     Sit given player down on the server.
        /// </summary>
        /// <param name="playerReference">Network reference of the player sitting down.</param>
        /// <param name="unsit">Whether the player is unseating or not.</param>
        [ServerRpc(RequireOwnership = false)]
        private void PlayerSitServerRpc(NetworkBehaviourReference playerReference, bool unsit)
        {
            // Sit player down on all clients.
            PlayerSitClientRpc(playerReference, unsit);
        }

        /// <summary>
        ///     Sit given player down on all clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player sitting down.</param>
        /// <param name="unsit">Whether the player is unseating or not.</param>
        [ClientRpc]
        private void PlayerSitClientRpc(NetworkBehaviourReference playerReference, bool unsit)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                // Sit player down on the local client.
                PlayerSitLocal(player, unsit);
            }
        }

        /// <summary>
        ///     Sit given player down on the local client.
        /// </summary>
        /// <param name="player">Player to sit down.</param>
        /// <param name="unsit">Whether the player is unseating or not.</param>
        private void PlayerSitLocal(PlayerControllerB player, bool unsit)
        {
            if (!unsit)
            {
                player.transform.position = targetTransform.position;
                // player.syncFullCameraRotation = player.gameplayCamera.transform.localEulerAngles;

                // Uncrouch player, should they happen to be crouching.
                player.isCrouching = false;
                player.playerBodyAnimator.SetBool(crouchingID, false);

                // Set sitting player as being in a special animation.
                player.inVehicleAnimation = enterVehicleAnimation;
                player.inSpecialInteractAnimation = true;

                if (player.IsLocalClient())
                {
                    // playerCamera = player.gameplayCamera.transform;

                    // Send special animation status to other clients.
                    player.UpdateSpecialAnimationValue(true, (short)targetTransform.eulerAngles.y, 0.0f, false);
                }

                // Enable player camera clamping.
                if (cameraClamping)
                {
                    player.minVerticalClamp = -minVerticalClamp; // Inverted sign to make it slightly more intuitive.
                    player.maxVerticalClamp = -maxVerticalClamp;
                    player.horizontalClamp = horizontalClamp;
                    player.clampLooking = true;
                }
                // ...

                if (hidePlayerItem && player.currentlyHeldObjectServer != null)
                {
                    // Hide the sitting player's held item (if enabled).
                    player.currentlyHeldObjectServer.EnableItemMeshes(false);

                    if (player.IsLocalClient())
                    {
                        // Hide the currently held item's tooltips.
                        HUDManager.Instance.ClearControlTips();
                    }
                }

                if (sittingAnimationID != -1)
                {
                    // Set the specified player sitting animation.
                    player.playerBodyAnimator.ResetTrigger(sittingAnimationID);
                    player.playerBodyAnimator.SetTrigger(sittingAnimationID);
                }

                // Invoke sitting down event.
                onPlayerSit.Invoke(player);
            }
            else
            {
                // Set unseating player as no longer in a special animation.
                player.inVehicleAnimation = false;
                player.inSpecialInteractAnimation = false;

                if (player.IsLocalClient())
                {
                    // Send special animation status to other clients.
                    player.UpdateSpecialAnimationValue(false, 0, 0.0f, false);
                }

                // Disable player camera clamping.
                player.gameplayCamera.transform.localEulerAngles = Vector3.zero;
                player.ladderCameraHorizontal = 0.0f;
                player.clampLooking = false;
                // ...

                if (hidePlayerItem && player.currentlyHeldObjectServer != null)
                {
                    // Show the unseating player's held item once again (if enabled).
                    player.currentlyHeldObjectServer.EnableItemMeshes(true);

                    if (player.IsLocalClient())
                    {
                        // Show the currently held item's tooltips once again.
                        player.currentlyHeldObjectServer.SetControlTipsForItem();
                    }
                }

                // Check if player is in a sitting animation.
                if (player.playerBodyAnimator.GetCurrentAnimatorStateInfo(5).tagHash == notInSpecialAnimID)
                {
                    // Reset the unseating player's animations.
                    player.playerBodyAnimator.SetTrigger(stopAnimationID);
                }

                // Invoke unseating event.
                onPlayerUnsit.Invoke(player);
            }
        }
    }
}