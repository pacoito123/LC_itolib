using itolib.Behaviours.Effects;
using itolib.Structs;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents an object that follows or sticks onto a player.
    /// </summary>
    public class PlayerTracker : PlayerAttachable
    {
        /// <summary>
        ///     Approximate time for tracker to reach the attached player, in seconds.
        /// </summary>
        [Space(5.0f)]
        [Header("Player Tracker")]
        [Tooltip("Approximate time for tracker to reach the attached player, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float trackingTime;

        /// <summary>
        ///     Offset to apply to the tracker's position while following the attached player.
        /// </summary>
        [Tooltip("Offset to apply to the tracker's position while following the attached player.")]
        [SerializeField] private Vector3 playerOffset = Vector3.zero;

        /// <summary>
        ///     List of pivots to rotate towards the tracker.
        /// </summary>
        [Space(5.0f)]
        [Tooltip("List of pivots to rotate towards the tracker.")]
        [SerializeField] private RotationPivot[]? pivotsToRotate;

        /// <summary>
        ///     Cached <c>Transform</c> for the tracker.
        /// </summary>
        private Transform trackerTransform = null!;

        /// <summary>
        ///     Current or initial velocity for the tracking.
        /// </summary>
        private Vector3 trackingVelocity = Vector3.zero;

        /// <summary>
        ///     Current or initial velocities for every pivot's rotation.
        /// </summary>
        private Vector3[] pivotVelocities;

        /// <summary>
        ///     Attach if the player is alive.
        ///     Detach if the player is dead.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead;
            detachCondition = player => player.isPlayerDead;
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Cache tracker transform.
            trackerTransform = transform;

            if (pivotsToRotate?.Length > 0)
            {
                pivotVelocities = new Vector3[pivotsToRotate.Length];
            }

            base.Start();
        }

        /// <summary>
        ///     Handle tracker's position following the attached player, and pivots' rotations looking towards the tracker.
        /// </summary>
        protected override void Update()
        {
            if (attachedPlayer != null)
            {
                // Obtain attached player's position, with the offset applied.
                Vector3 targetPosition = attachedPlayerTransform.position + playerOffset;

                // Move tracker towards the attached player.
                trackerTransform.position = (trackingTime == 0.0f) ? targetPosition
                    : Vector3.SmoothDamp(trackerTransform.position, targetPosition, ref trackingVelocity, trackingTime);

                // Apply rotations towards the tracker to each pivot.
                for (int i = 0; i < pivotsToRotate?.Length; i++)
                {
                    pivotsToRotate[i].Apply(trackerTransform.position, ref pivotVelocities[i]);
                }
            }

            base.Update();
        }
    }
}