using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using itolib.Enums;
using System;
using System.Collections.Generic;
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
        public int forceToApply;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 forceDirection;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public RotationSource considerRotationFrom;

        /// <summary>
        ///     TODO.
        /// </summary>
        public LaunchParameters()
        {
            forceToApply = 0;
            forceDirection = Vector3.zero;
            considerRotationFrom = RotationSource.Absolute;
        }
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
        public List<LaunchParameters> forcesToApply = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool landingDetaches = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool negateFallDamage = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableMovement = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool crouchingPreventsLaunch = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Dropping")]
        [Tooltip("")]
        public bool dropPlayerItemsAtStart = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropPlayerItemsAtEnd = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropHeldItemAtStart = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dropHeldItemAtEnd = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Camera")]
        [Tooltip("")]
        public bool lockCamera = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool rotateCamera = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float rotationSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 targetAngle = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool clampAngle = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public Transform playerModelTransform = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float fallTime = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            attachCondition = player => !player.isPlayerDead && !(crouchingPreventsLaunch && player.isCrouching);
            detachCondition = player => player.isPlayerDead || (landingDetaches && player.thisController != null && player.thisController.isGrounded);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            if (attachedPlayer != null)
            {
                fallTime += Time.deltaTime;

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

            /* player.externalForceAutoFade = (forceToApply * (considerRotationFrom switch
            {
                RotationSource.Player => playerModelTransform.rotation * forceDirection,
                RotationSource.Launcher => transform.rotation * forceDirection,
                RotationSource.Absolute or _ => forceDirection
            })) + additionalForce; */

            player.externalForceAutoFade = Vector3.zero;

            foreach (LaunchParameters launch in forcesToApply)
            {
                player.externalForceAutoFade += launch.forceToApply * (launch.considerRotationFrom switch
                {
                    RotationSource.Player => playerModelTransform.rotation * launch.forceDirection,
                    RotationSource.Launcher => transform.rotation * launch.forceDirection,
                    RotationSource.Absolute or _ => launch.forceDirection
                });
            }

            if (negateFallDamage)
            {
                player.ResetFallGravity();
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