using UnityEngine;

namespace itolib.Extensions
{
    /// <summary>
    ///     Extensions for the <c>Quaternion</c> class.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        ///     <c>Vector3.SmoothDamp</c> but for a <c>Quaternion</c>'s rotation.
        /// </summary>
        /// <param name="current">Current or initial rotation.</param>
        /// <param name="target">Rotation to move towards.</param>
        /// <param name="currentVelocity">Current or initial velocity for the rotation, as a reference.</param>
        /// <param name="smoothTime">Approximate time for the rotation to be fully complete, in seconds.</param>
        /// <returns>Current rotation towards the target.</returns>
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            // Convert current and target rotations to Euler angles.
            Vector3 currentEuler = current.eulerAngles, targetEuler = target.eulerAngles;

            // Applying rotation towards the target to every axis.
            return Quaternion.Euler(Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref currentVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref currentVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref currentVelocity.z, smoothTime));
        }
    }
}