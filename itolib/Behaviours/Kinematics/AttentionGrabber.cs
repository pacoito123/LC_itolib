using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents an object that a player's camera is forced to stare at.
    /// </summary>
    public class AttentionGrabber : PlayerAttachable
    {
        /// <summary>
        ///     Hash of the bool parameter for a player's camera being moved to the left.
        /// </summary>
        private static readonly int pullingCameraLeftID = Animator.StringToHash("PullingCameraLeft");

        /// <summary>
        ///     Hash of the bool parameter for a player's camera being moved to the right.
        /// </summary>
        private static readonly int pullingCameraRightID = Animator.StringToHash("PullingCameraRight");

        /// <summary>
        ///     Approximate time for horizontal camera rotation to be fully complete, in seconds. Can be set to <c>-1</c> to disable.
        /// </summary>
        [Header("Attention Grabber")]
        [Tooltip("Approximate time for horizontal camera rotation to be fully complete, in seconds. Can be set to '-1' to disable.")]
        [Min(-1.0f)]
        [SerializeField] private float grabTimeHorizontal;

        /// <summary>
        ///     Approximate time for vertical camera rotation to be fully complete, in seconds. Can be set to <c>-1</c> to disable.
        /// </summary>
        [Tooltip("Approximate time for vertical camera rotation to be fully complete, in seconds. Can be set to '-1' to disable.")]
        [Min(-1.0f)]
        [SerializeField] private float grabTimeVertical;

        /// <summary>
        ///     Offset to apply to the target position.
        /// </summary>
        [Tooltip("Offset to apply to the target position.")]
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        /// <summary>
        ///     Whether to detach the player if the player enters a special animation or not.
        /// </summary>
        [Tooltip("Whether to detach the player if the player enters a special animation or not.")]
        [SerializeField] private bool detachOnSpecialAnimation;

        /// <summary>
        ///     Whether the attached player should be allowed to move their camera to fight the grabber or not.
        /// </summary>
        [Tooltip("Whether the attached player should be allowed to move their camera to fight the grabber or not.")]
        [SerializeField] private bool disableLookInput;

        /// <summary>
        ///     Cached <c>Transform</c> for the grabber.
        /// </summary>
        private Transform grabberTransform = null!;

        /// <summary>
        ///     Current or initial velocity for horizontal camera rotation.
        /// </summary>
        private float cameraVelocityHorizontal;

        /// <summary>
        ///     Current or initial velocity for vertical camera rotation.
        /// </summary>
        private float cameraVelocityVertical;

        /// <summary>
        ///     Attach if the player is alive and enabled.
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected override bool AttachCondition(PlayerControllerB player)
        {
            return !player.isPlayerDead && player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Detach if the player is dead or disabled, or enters a special animation (<c>detachOnSpecialAnimation</c>).
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected override bool DetachCondition(PlayerControllerB player)
        {
            return player.isPlayerDead || !player.isActiveAndEnabled || (detachOnSpecialAnimation && player.inSpecialInteractAnimation);
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        private void Awake()
        {
            // Cache grabber Transform.
            grabberTransform = transform;
        }

        /// <summary>
        ///     Handle camera movement for the attached player.
        /// </summary>
        protected override void Update()
        {
            if (attachedPlayer != null)
            {
                // Check if horizontal camera movement should be applied.
                if (grabTimeHorizontal >= 0.0f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(grabberTransform.position - attachedPlayerTransform.position
                        + (grabberTransform.rotation * targetOffset), Vector3.up);

                    if (attachedPlayer.physicsParent != null)
                    {
                        // Inverse rotation from physics parent.
                        targetRotation *= Quaternion.Inverse(attachedPlayer.physicsParent.rotation);
                    }

                    // Obtain angle on the y-axis to rotate to.
                    float lookAngle = Mathf.SmoothDampAngle(attachedPlayerTransform.localEulerAngles.y, targetRotation.eulerAngles.y,
                        ref cameraVelocityHorizontal, grabTimeHorizontal);

                    // Rotate player on the y-axis towards the target rotation.
                    attachedPlayerTransform.localRotation = Quaternion.Euler(Vector3.up * lookAngle);

                    // Set left or right camera pulling bool parameter, depending on velocity direction.
                    attachedPlayer.playerBodyAnimator.SetBool(pullingCameraLeftID, cameraVelocityHorizontal < -5.0f);
                    attachedPlayer.playerBodyAnimator.SetBool(pullingCameraRightID, cameraVelocityHorizontal > 5.0f);
                }

                // Check if vertical camera movement should be applied.
                if (grabTimeVertical >= 0.0f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(grabberTransform.position - attachedPlayerCameraTransform.position
                        + (grabberTransform.rotation * targetOffset), Vector3.right);

                    // Obtain angle on the x-axis to rotate to.
                    float lookAngle = Mathf.SmoothDampAngle(attachedPlayerCameraTransform.localEulerAngles.x, targetRotation.eulerAngles.x,
                        ref cameraVelocityVertical, grabTimeVertical);

                    // Rotate camera on the x-axis towards the target rotation and set 'cameraUp' value accordingly.
                    attachedPlayerCameraTransform.localRotation = Quaternion.Euler(Vector3.right * lookAngle);
                    attachedPlayer.cameraUp = ((lookAngle + 180.0f) % 360.0f) - 180.0f;
                }
            }

            base.Update();
        }

        /// <inheritdoc/>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            // Disable player camera input (if enabled).
            player.disableLookInput = disableLookInput;

            // Reset previous horizontal and vertical camera velocity.
            cameraVelocityHorizontal = 0.0f;
            cameraVelocityVertical = 0.0f;
        }

        /// <inheritdoc/>
        public override void DetachPlayerLocal()
        {
            if (attachedPlayer != null)
            {
                // Enable player camera input.
                attachedPlayer.disableLookInput = false;
            }

            base.DetachPlayerLocal();
        }
    }
}