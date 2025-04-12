using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using itolib.Enums;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerLauncher : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary> 
        public Transform PlayerModelTransform { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float FallTime { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Launcher")]
        [Tooltip("")]
        public int forceToApply = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 forceDirection = Vector3.forward;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public RotationSource considerRotationFrom = RotationSource.Absolute;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 additionalForce = Vector3.zero;

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
                FallTime += Time.deltaTime;

                if (rotateCamera)
                {
                    PlayerModelTransform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(targetAngle), FallTime * rotationSpeed);
                }

                if (negateFallDamage)
                {
                    // AttachedPlayer.ResetFallGravity();
                    AttachedPlayer.takingFallDamage = false;
                }
            }

            base.Update();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            PlayerModelTransform = player.meshContainer.transform;

            player.externalForceAutoFade = (forceToApply * (considerRotationFrom switch
            {
                RotationSource.Player => PlayerModelTransform.rotation * forceDirection,
                RotationSource.Launcher => transform.rotation * forceDirection,
                RotationSource.Absolute or _ => forceDirection
            })) + additionalForce;

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
                PlayerModelTransform.localRotation = Quaternion.identity;
                FallTime = 0.0f;

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