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
        /// <param name="currentVelocity">Current or initial velocity for the rotation, as a ref parameter.</param>
        /// <param name="smoothTime">Approximate time for the rotation to be fully complete, in seconds.</param>
        /// <returns>Current rotation towards the target.</returns>
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref Quaternion currentVelocity, float smoothTime)
        {
            // Return target directly if using a comically small smooth time.
            if (smoothTime < Quaternion.kEpsilon)
            {
                return target;
            }

            // Create vector for current rotation velocity.
            Vector4 velocity = new(currentVelocity.x, currentVelocity.y, currentVelocity.z, currentVelocity.w);

            // Invert rotation components, if double-covering.
            if (Quaternion.Dot(current, target) <= 0.0f)
            {
                target.Set(-target.x, -target.y, -target.z, -target.w);
            }

            // Perform smooth damp on every component towards the target rotation.
            Vector4 result = new(
                Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime),
                Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime),
                Mathf.SmoothDamp(current.z, target.z, ref velocity.z, smoothTime),
                Mathf.SmoothDamp(current.w, target.w, ref velocity.w, smoothTime));

            // Normalize result vector.
            result.Normalize();

            // Make current velocity vector tangent to result vector.
            velocity -= Vector4.Project(velocity, result);
            currentVelocity.Set(velocity.x, velocity.y, velocity.z, velocity.w);

            // Return resulting rotation.
            return new(result.x, result.y, result.z, result.w);
        }
    }
}