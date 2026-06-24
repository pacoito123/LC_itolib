using itolib.Extensions;
using System;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     Represents a rotation to apply to a specific <c>Transform</c> towards a target over time.
    /// </summary>
    [Serializable]
    public struct RotationPivot()
    {
        /// <summary>
        ///     Pivot <c>Transform</c> to apply the rotation to.
        /// </summary>
        [Header("Rotation Pivot")]
        [Tooltip("Pivot Transform to apply the rotation to.")]
        public Transform? pivot;

        /// <summary>
        ///     Additional offset to apply to the final rotation.
        /// </summary>
        [Tooltip("Additional offset to apply to the final rotation.")]
        public Vector3 offset;

        /// <summary>
        ///     Direction to consider upwards when applying the rotation. Default is <c>Vector3.up</c>, or <c>(0,1,0)</c>.
        /// </summary>
        [Tooltip("Direction to consider upwards when applying the rotation. Default is 'Vector3.up', or '(0,1,0)'.")]
        public Vector3 worldUp = Vector3.up;

        /// <summary>
        ///     Approximate time for the rotation to be fully complete, in seconds.
        /// </summary>
        [Tooltip("Approximate time for the rotation to be fully complete, in seconds.")]
        [Min(0.0f)]
        public float rotationTime;

        /// <summary>
        ///     Whether to freeze rotation for the <c>X-axis</c> or not.
        /// </summary>
        [Space(5.0f)]
        [Tooltip("Whether to freeze rotation for the X-axis or not.")]
        public bool freezeX;

        /// <summary>
        ///     Whether to freeze rotation for the <c>Y-axis</c> or not.
        /// </summary>
        [Tooltip("Whether to freeze rotation for the Y-axis or not.")]
        public bool freezeY;

        /// <summary>
        ///     Whether to freeze rotation for the <c>Z-axis</c> or not.
        /// </summary>
        [Tooltip("Whether to freeze rotation for the Z-axis or not.")]
        public bool freezeZ;

        /// <summary>
        ///     Apply rotation to the current pivot.
        /// </summary>
        /// <param name="targetPosition">Position for the pivot to turn and look towards.</param>
        /// <param name="pivotVelocity">Current velocity for this pivot's rotation.</param>
        public readonly void Apply(Vector3 targetPosition, ref Vector3 pivotVelocity)
        {
            if (pivot != null)
            {
                // Obtain actual target for the pivot, taking frozen axes into account.
                float targetPitch = freezeX ? pivot.position.x : targetPosition.x,
                    targetYaw = freezeY ? pivot.position.y : targetPosition.y,
                    targetRoll = freezeZ ? pivot.position.z : targetPosition.z;

                // Obtain pivot rotation towards the target, with the additional offset applied.
                Vector3 lookTarget = new(targetPitch, targetYaw, targetRoll);
                Quaternion targetRotation = Quaternion.LookRotation(lookTarget - pivot.position, worldUp) * Quaternion.Euler(offset);

                // Apply rotation to the pivot over time, or immediately if set to a value of zero.
                pivot.rotation = (rotationTime == 0.0f) ? targetRotation
                    : pivot.rotation.SmoothDamp(targetRotation, ref pivotVelocity, rotationTime);
            }
        }
    }
}