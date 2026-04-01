using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents a force to be applied to the player, towards a specified direction.
    /// </summary>
    [Serializable]
    public struct LaunchParameters
    {
        /// <summary>
        ///     Magnitude of the force to apply to the player.
        /// </summary>
        [Tooltip("Magnitude of the force to apply to the player.")]
        public int forceToApply = 0;

        /// <summary>
        ///     Direction of the force to apply to the player.
        /// </summary>
        [Tooltip("Direction of the force to apply to the player.")]
        public Vector3 forceDirection = Vector3.zero;

        /// <summary>
        ///     Additional rotation source to consider when multiplying by the specified direction.
        /// </summary>
        [Tooltip("Additional rotation source to consider when multiplying by the specified direction.")]
        public RotationSource considerRotationFrom = RotationSource.Absolute;

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public LaunchParameters() { }
    }

    /// <summary>
    ///     Represents a launcher that can apply one or several forces to a player to move them around.
    /// </summary>
    /// <remarks>Intended for stuff like cannons, trampolines, or any obstacle that knocks the player back a certain distance.</remarks>
    public class PlayerLauncher : PlayerAttachable
    {
        /// <summary>
        ///     Hash of the bool parameter for a player jumping.
        /// </summary>
        private static readonly int jumpingBoolID = Animator.StringToHash("Jumping");

        /// <summary>
        ///     List of forces to apply to the attached player.
        /// </summary>
        /// <remarks>If multiple forces are specified, they are all combined and applied in unison.</remarks>
        [Space(10.0f)]
        [Header("Player Launcher")]
        [Tooltip("List of forces to apply to the attached player. If multiple forces are specified, they are all combined and applied in unison.")]
        [SerializeField] private LaunchParameters[]? forcesToApply;

        /// <summary>
        ///     Ramping speed for the forces applied to the attached player.
        /// </summary>
        /// <remarks>If left at <c>0</c>, the forces will be fully applied instantly without any smoothing.</remarks>
        [Tooltip("Ramping speed for the forces applied to the attached player. If left at '0', the forces will be fully applied instantly without any smoothing.")]
        [Min(0.0f)]
        [SerializeField] private float launchSpeed;

        /// <summary>
        ///     Whether the attached player should detach upon touching the ground or not.
        /// </summary>
        [Tooltip("Whether the attached player should detach upon touching the ground or not.")]
        [SerializeField] private bool detachOnLand = true;

        /// <summary>
        ///     Whether the attached player should detach as soon as the full force is applied or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Requires <c>launchSpeed</c> to be greater than <c>0</c>.</remarks>
        [Tooltip("Whether the attached player should detach as soon as the full force is applied or not. NOTE: Requires launch speed to be greater than '0'.")]
        [SerializeField] private bool detachOnPeak;

        /// <summary>
        ///     Whether the attached player should take fall damage or not, as long as they're attached.
        /// </summary>
        /// <remarks><b>NOTE:</b> Fall damage will apply if detached before landing.</remarks>
        [Tooltip("Whether the attached player should take fall damage or not, as long as they're attached. NOTE: Fall damage will apply if detached before landing.")]
        public bool negateFallDamage;

        /// <summary>
        ///     Whether the attached player should be allowed to move while being launched or not.
        /// </summary>
        [Tooltip("Whether the attached player should be allowed to move while being launched or not.")]
        public bool disableMovement;

        /// <summary>
        ///     Whether the attached player should be allowed to move their camera while being launched or not.
        /// </summary>
        [Tooltip("Whether the attached player should be allowed to move their camera while being launched or not.")]
        public bool lockCamera;

        /// <summary>
        ///     Whether crouching should prevent the player from attaching or not.
        /// </summary>
        [Tooltip("Whether crouching should prevent the player from attaching or not.")]
        public bool crouchingPreventsLaunch;

        /// <summary>
        ///     Whether the player should be able to perform a 'rocket jump', if jumping immediately before attaching.
        /// </summary>
        /// <remarks><b>NOTE:</b> Enabled by default as a joke, but should probably be disabled in most cases.</remarks>
        [Tooltip("Whether the player should be able to perform a 'rocket jump', if jumping immediately before attaching. NOTE: Enabled by default as a joke, but "
            + "should probably be disabled in most cases.")]
        public bool rocketJump = true;

        /// <summary>
        ///     Whether players should drop all their items immediately upon attaching or not.
        /// </summary>
        [Header("Item Dropping")]
        [Tooltip("Whether players should drop all their items immediately upon attaching or not.")]
        public bool dropPlayerItemsAtStart;

        /// <summary>
        ///     Whether players should drop all their items immediately upon detaching or not.
        /// </summary>
        [Tooltip("Whether players should drop all their items immediately upon detaching or not.")]
        public bool dropPlayerItemsAtEnd;

        /// <summary>
        ///     Whether players should drop their held item immediately upon attaching or not.
        /// </summary>
        [Tooltip("Whether players should drop their held item immediately upon attaching or not.")]
        public bool dropHeldItemAtStart;

        /// <summary>
        ///     Whether players should drop their held item immediately upon detaching or not.
        /// </summary>
        [Tooltip("Whether players should drop their held item immediately upon detaching or not.")]
        public bool dropHeldItemAtEnd;

        /// <summary>
        ///     Whether to rotate the player model towards the specified <c>targetAngle</c> or not.
        /// </summary>
        [Header("Player Rotation")]
        [Tooltip("Whether to rotate the player model towards the specified target angle or not.")]
        public bool rotateCamera;

        /// <summary>
        ///     Ramping speed for the player model rotation towards the specified <c>targetAngle</c>.
        /// </summary>
        [Tooltip("Ramping speed for the player model rotation towards the specified target angle.")]
        public float rotationSpeed = 1.0f;

        /// <summary>
        ///     Target rotation for the player model, in degrees.
        /// </summary>
        [Tooltip("Target rotation for the player model, in degrees.")]
        public Vector3 targetAngle;

        /// <summary>
        ///     Whether the shortest path towards the <c>targetAngle</c> should be prioritized or not.
        /// </summary>
        /// <remarks>Disabling clamping allows for <c>&gt;360°</c> rotation (full spins).</remarks>
        [Tooltip("Whether the shortest path towards the target angle should be prioritized or not. Disabling clamping allows for >360° rotation (full spins).")]
        public bool clampAngle = true;

        /// <summary>
        ///     Cached <c>Transform</c> of the currently attached player's model.
        /// </summary>
        private Transform playerModelTransform = null!;

        /// <summary>
        ///     Time passed since the player was launched.
        /// </summary>
        private float launchTimer;

        /// <summary>
        ///     Cumulative force between all the forces to apply, to smoothly ramp towards.
        /// </summary>
        private Vector3 targetForce;

        /// <summary>
        ///     Attach if the player is alive and not crouching (<c>crouchingPreventsLaunch</c>).
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected override bool AttachCondition(PlayerControllerB player)
        {
            return !player.isPlayerDead && !(crouchingPreventsLaunch && player.isCrouching);
        }

        /// <summary>
        ///     Detach if the player is dead, touches the ground (<c>detachOnLand</c>), or the full force has just been applied (<c>detachOnPeak</c>).
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected override bool DetachCondition(PlayerControllerB player)
        {
            return player.isPlayerDead || (detachOnLand && player.thisController != null && player.thisController.isGrounded)
                || (detachOnPeak && launchTimer * launchSpeed > 1.0f);
        }

        /// <summary>
        ///     Ramp up towards the target force, smoothly rotate player model transform, and/or continually set the player as no longer taking fall damage.
        /// </summary>
        protected override void Update()
        {
            if (attachedPlayer != null)
            {
                // Increment launch timer.
                launchTimer += Time.deltaTime;

                if (launchSpeed > 0.0f)
                {
                    // attachedPlayer.externalForceAutoFade = Vector3.Lerp(Vector3.zero, targetForce, launchTimer * launchSpeed);
                    attachedPlayer.externalForceAutoFade = Vector3.Lerp(attachedPlayer.externalForceAutoFade, targetForce, launchTimer * launchSpeed);
                }

                if (rotateCamera)
                {
                    if (clampAngle)
                    {
                        // Rotate player model towards the target angle, following the shortest path to get there (target angle values >360° wrap around).
                        playerModelTransform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(targetAngle), launchTimer * rotationSpeed);
                    }
                    else
                    {
                        // Rotate player model towards the target angle, following the full path to get there (target angle values >360° perform full spins).
                        playerModelTransform.localEulerAngles = Vector3.Lerp(Vector3.zero, targetAngle, launchTimer * rotationSpeed);
                    }
                }

                if (negateFallDamage)
                {
                    // Inform gravity that this player absolutely cannot, in fact, take damage from falling.
                    attachedPlayer.takingFallDamage = false;
                }
            }

            base.Update();
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            // Check if player was not attached successfully.
            if (attachedPlayer == null)
            {
                return;
            }

            // Cache reference to the attaching player's model's transform.
            playerModelTransform = player.meshContainer.transform;

            // Reset current player force, before all forces are applied. TODO: Add field for this.
            // player.externalForceAutoFade = Vector3.zero;

            // Reset target force.
            targetForce = Vector3.zero;

            // Add each specified force to apply to the cumulative target force.
            for (int i = 0; i < forcesToApply?.Length; i++)
            {
                LaunchParameters launch = forcesToApply[i];

                // Add the current force to the cumulative target force, multiplied by the rotation of its specified source.
                targetForce += launch.forceToApply * (launch.considerRotationFrom switch
                {
                    RotationSource.Player => playerModelTransform.rotation * launch.forceDirection,
                    RotationSource.Launcher => transform.rotation * launch.forceDirection,
                    RotationSource.Absolute or _ => launch.forceDirection
                });
            }

            if (launchSpeed == 0.0f)
            {
                // Immediately apply the full target force to the player.
                player.externalForceAutoFade += targetForce;
            }

            if (negateFallDamage)
            {
                // Reset player fall time to negate fall damage.
                player.ResetFallGravity();
            }

            if (!rocketJump && player.jumpCoroutine != null)
            {
                // Eat the player's jump, if they attach while jumping.
                player.StopCoroutine(player.jumpCoroutine);
                player.jumpCoroutine = null;

                // Set player as no longer falling.
                player.isJumping = false;
                player.isFallingFromJump = false;

                // Reset player jump animation.
                player.playerBodyAnimator.SetBool(jumpingBoolID, false);
            }

            // Check if the local player is not attached.
            if (!localPlayerAttached)
            {
                return;
            }

            // Disable player movement and camera (if enabled).
            player.disableMoveInput = disableMovement;
            player.disableLookInput = lockCamera;

            if (dropHeldItemAtStart && !player.throwingObject && player.isHoldingObject
                && player.currentlyHeldObjectServer != null)
            {
                // Force the player to drop their held item.
                player.DiscardHeldObject();
            }

            if (dropPlayerItemsAtStart)
            {
                // Get the player eye's position and rotation.
                player.playerEye.GetPositionAndRotation(out Vector3 eyePosition, out Quaternion eyeRotation);

                // Drop the player's full inventory.
                player.DropAllHeldItemsAndSync(attachedPlayerTransform.position, player.localItemHolder.position,
                    player.localItemHolder.localEulerAngles, eyePosition, eyeRotation.eulerAngles);
            }
        }

        /// <summary>
        ///    Detach player on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (attachedPlayer != null)
            {
                // Reset launch timer.
                launchTimer = 0.0f;

                if (playerModelTransform != null)
                {
                    // Reset player model rotation.
                    playerModelTransform.localRotation = Quaternion.identity;
                }

                // Enable player movement and camera.
                attachedPlayer.disableMoveInput = false;
                attachedPlayer.disableLookInput = false;

                if (dropHeldItemAtEnd && !attachedPlayer.throwingObject && attachedPlayer.isHoldingObject
                    && attachedPlayer.currentlyHeldObjectServer != null)
                {
                    // Force the player to drop their held item.
                    attachedPlayer.DiscardHeldObject();
                }

                if (dropPlayerItemsAtEnd)
                {
                    // Get the player eye's position and rotation.
                    attachedPlayer.playerEye.GetPositionAndRotation(out Vector3 eyePosition, out Quaternion eyeRotation);

                    // Drop the player's full inventory.
                    attachedPlayer.DropAllHeldItemsAndSync(attachedPlayerTransform.position, attachedPlayer.localItemHolder.position,
                        attachedPlayer.localItemHolder.localEulerAngles, eyePosition, eyeRotation.eulerAngles);
                }
            }

            base.DetachPlayerLocal();
        }
    }
}