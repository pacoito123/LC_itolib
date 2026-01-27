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
        /// <param name="acceleration"></param>
        public void ApplyRelativeContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(rigidbody => rigidbody.AddRelativeForce(acceleration, ForceMode.Acceleration));
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
        /// <param name="force"></param>
        public void ApplyRelativeContinuousForce(Vector3 force)
        {
            PerformGroupAction(rigidbody => rigidbody.AddRelativeForce(force, ForceMode.Force));
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
        /// <param name="impulse"></param>
        public void ApplyRelativeInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(rigidbody => rigidbody.AddRelativeForce(impulse, ForceMode.Impulse));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ApplyInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(rigidbody => rigidbody.AddForce(velocityChange, ForceMode.VelocityChange));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ApplyRelativeInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(rigidbody => rigidbody.AddRelativeForce(velocityChange, ForceMode.VelocityChange));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SleepAll()
        {
            PerformGroupAction(rigidbody =>
            {
                if (!rigidbody.IsSleeping())
                {
                    rigidbody.Sleep();
                }
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void WakeUpAll()
        {
            PerformGroupAction(rigidbody =>
            {
                if (rigidbody.IsSleeping())
                {
                    rigidbody.WakeUp();
                }
            });
        }
    }
}