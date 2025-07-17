using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct LaunchParameters
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int forceToApply = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 forceDirection = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public RotationSource considerRotationFrom = RotationSource.Absolute;

        /// <summary>
        ///     TODO.
        /// </summary>
        public LaunchParameters() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerLauncher : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Launcher")]
        [Tooltip("")]
        [SerializeField] private LaunchParameters[]? forcesToApply;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private float launchSpeed;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool detachOnLand = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool detachOnPeak;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool negateFallDamage;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableMovement;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool crouchingPreventsLaunch;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool rocketJump = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Dropping")]
        [Tooltip("")]
        public bool dropPlayerItemsAtStart;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropPlayerItemsAtEnd;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropHeldItemAtStart;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropHeldItemAtEnd;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Camera")]
        [Tooltip("")]
        public bool lockCamera;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool rotateCamera;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float rotationSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 targetAngle;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool clampAngle = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform playerModelTransform = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float fallTime;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Vector3 targetForce;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead && !(crouchingPreventsLaunch && player.isCrouching);
            detachCondition = player => player.isPlayerDead || (detachOnLand && player.thisController != null && player.thisController.isGrounded)
                || (detachOnPeak && fallTime * launchSpeed > 1.0f);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Update()
        {
            if (attachedPlayer != null)
            {
                fallTime += Time.deltaTime;

                if (launchSpeed > 0.0f)
                {
                    // attachedPlayer.externalForceAutoFade = Vector3.Lerp(Vector3.zero, targetForce, fallTime * launchSpeed);
                    attachedPlayer.externalForceAutoFade = Vector3.Lerp(attachedPlayer.externalForceAutoFade, targetForce, fallTime * launchSpeed);
                }

                if (rotateCamera)
                {
                    if (clampAngle)
                    {
                        playerModelTransform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(targetAngle), fallTime * rotationSpeed);
                    }
                    else
                    {
                        playerModelTransform.localEulerAngles = Vector3.Lerp(Vector3.zero, targetAngle, fallTime * rotationSpeed);
                    }
                }

                if (negateFallDamage)
                {
                    attachedPlayer.takingFallDamage = false;
                }
            }

            base.Update();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AttachPlayerLocal()
        {
            if (attachLocally)
            {
                AttachPlayerLocal(GameNetworkManager.Instance.localPlayerController);
            }
            else
            {
                AttachPlayerServerRpc(GameNetworkManager.Instance.localPlayerController);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            playerModelTransform = player.meshContainer.transform;
            // player.externalForceAutoFade = Vector3.zero;

            targetForce = Vector3.zero;

            for (int i = 0; i < forcesToApply?.Length; i++)
            {
                LaunchParameters launch = forcesToApply[i];

                targetForce += launch.forceToApply * (launch.considerRotationFrom switch
                {
                    RotationSource.Player => playerModelTransform.rotation * launch.forceDirection,
                    RotationSource.Launcher => transform.rotation * launch.forceDirection,
                    RotationSource.Absolute or _ => launch.forceDirection
                });
            }

            if (launchSpeed == 0.0f)
            {
                player.externalForceAutoFade += targetForce;
            }

            if (negateFallDamage)
            {
                player.ResetFallGravity();
            }

            if (!rocketJump && player.jumpCoroutine != null)
            {
                player.StopCoroutine(player.jumpCoroutine);
                player.jumpCoroutine = null;

                player.isJumping = false;
                player.isFallingFromJump = false;

                player.playerBodyAnimator.SetBool("Jumping", false);
            }

            if (!localPlayerAttached)
            {
                return;
            }

            player.disableMoveInput = disableMovement;
            player.disableLookInput = lockCamera;

            if (dropHeldItemAtStart && !player.throwingObject && player.isHoldingObject
                && player.currentlyHeldObjectServer != null)
            {
                player.DiscardHeldObject();
            }

            if (dropPlayerItemsAtStart)
            {
                player.DropAllHeldItemsAndSync();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (attachedPlayer != null)
            {
                playerModelTransform.localRotation = Quaternion.identity;
                fallTime = 0.0f;

                attachedPlayer.disableMoveInput = false;
                attachedPlayer.disableLookInput = false;

                if (dropHeldItemAtEnd && !attachedPlayer.throwingObject && attachedPlayer.isHoldingObject
                    && attachedPlayer.currentlyHeldObjectServer != null)
                {
                    attachedPlayer.DiscardHeldObject();
                }

                if (dropPlayerItemsAtEnd)
                {
                    attachedPlayer.DropAllHeldItemsAndSync();
                }
            }

            base.DetachPlayerLocal();
        }
    }
}