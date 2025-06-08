using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using itolib.Enums;
using System;
using System.Collections.Generic;
using Unity.Netcode;
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
        [Space(3.0f)]
        [Header("DEPRECATED")]
        [Obsolete("Add to 'forcesToApply' instead.")]
        [Tooltip("")]
        public int forceToApply = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Obsolete("Add to 'forcesToApply' instead.")]
        [Tooltip("")]
        public Vector3 forceDirection = Vector3.forward;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Obsolete("Add to 'forcesToApply' instead.")]
        [Tooltip("")]
        public RotationSource considerRotationFrom = RotationSource.Absolute;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Obsolete("Add to 'forcesToApply' instead.")]
        [Tooltip("")]
        public Vector3 additionalForce = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead && !(crouchingPreventsLaunch && player.isCrouching);
            DetachCondition = player => player.isPlayerDead || (landingDetaches && player.thisController?.isGrounded == true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            if (AttachedPlayer != null)
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
                    AttachedPlayer.takingFallDamage = false;
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
                AttachPlayerServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>());
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

            if (!LocalPlayerAttached)
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
            if (AttachedPlayer != null)
            {
                playerModelTransform.localRotation = Quaternion.identity;
                fallTime = 0.0f;

                AttachedPlayer.disableMoveInput = false;
                AttachedPlayer.disableLookInput = false;

                if (dropHeldItemAtEnd && !AttachedPlayer.throwingObject && AttachedPlayer.isHoldingObject
                    && AttachedPlayer.currentlyHeldObjectServer != null)
                {
                    AttachedPlayer.DiscardHeldObject();
                }

                if (dropPlayerItemsAtEnd)
                {
                    AttachedPlayer.DropAllHeldItemsAndSync();
                }
            }

            base.DetachPlayerLocal();
        }
    }
}