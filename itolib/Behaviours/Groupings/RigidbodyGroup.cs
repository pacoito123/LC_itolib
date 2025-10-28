using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class RigidbodyGroup : ComponentGroup<Rigidbody>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="acceleration"></param>
        public void ApplyContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(rigidbody => rigidbody.AddForce(acceleration, ForceMode.Acceleration));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyContinuousForce(Vector3 force)
        {
            PerformGroupAction(rigidbody => rigidbody.AddForce(force, ForceMode.Force));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(rigidbody => rigidbody.AddForce(impulse, ForceMode.Impulse));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ApplyInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(rigidbody => rigidbody.AddForce(velocityChange, ForceMode.VelocityChange));
        }
    }
}