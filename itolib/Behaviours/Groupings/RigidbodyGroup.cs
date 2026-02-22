using System;
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
        private enum RigidbodyActions
        {
            ApplyAcceleration,
            ApplyAccelerationRelative,
            ApplyForce,
            ApplyForceRelative,
            ApplyImpulse,
            ApplyImpulseRelative,
            ApplyVelocityChange,
            ApplyVelocityChangeRelative,
            Sleep,
            WakeUp
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Rigidbody Group")]
        [Tooltip("")]
        [SerializeField] private bool respectSleep;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
        protected override void PerformSingleAction(Rigidbody rigidbody, Enum actionID, object? parameter = null)
        {
            if (actionID is not RigidbodyActions rigidbodyActionID)
            {
                return;
            }

            if (respectSleep && (int)rigidbodyActionID < 8 && rigidbody.IsSleeping())
            {
                return;
            }

            Vector3 force = Vector3.zero;
            if ((int)rigidbodyActionID < 8)
            {
                if (parameter is not Vector3 vector)
                {
                    return;
                }
                force = vector;
            }

            switch (rigidbodyActionID)
            {
                case RigidbodyActions.ApplyAcceleration:
                    rigidbody.AddForce(force, ForceMode.Acceleration);
                    break;
                case RigidbodyActions.ApplyAccelerationRelative:
                    rigidbody.AddRelativeForce(force, ForceMode.Acceleration);
                    break;
                case RigidbodyActions.ApplyForce:
                    rigidbody.AddForce(force, ForceMode.Force);
                    break;
                case RigidbodyActions.ApplyForceRelative:
                    rigidbody.AddRelativeForce(force, ForceMode.Force);
                    break;
                case RigidbodyActions.ApplyImpulse:
                    rigidbody.AddForce(force, ForceMode.Impulse);
                    break;
                case RigidbodyActions.ApplyImpulseRelative:
                    rigidbody.AddRelativeForce(force, ForceMode.Impulse);
                    break;
                case RigidbodyActions.ApplyVelocityChange:
                    rigidbody.AddForce(force, ForceMode.VelocityChange);
                    break;
                case RigidbodyActions.ApplyVelocityChangeRelative:
                    rigidbody.AddRelativeForce(force, ForceMode.VelocityChange);
                    break;
                case RigidbodyActions.Sleep:
                    if (!rigidbody.IsSleeping())
                    {
                        rigidbody.Sleep();
                    }
                    break;
                case RigidbodyActions.WakeUp:
                    if (rigidbody.IsSleeping())
                    {
                        rigidbody.WakeUp();
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="enabled"></param>
        protected override void EnableSingleComponent(Rigidbody rigidbody, bool enabled)
        {
            rigidbody.isKinematic = !enabled;
            rigidbody.detectCollisions = enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="rigidbody"></param>
        protected override void ToggleSingleComponent(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = !rigidbody.isKinematic;
            rigidbody.detectCollisions = !rigidbody.isKinematic; // Detect collisions only when not kinematic.
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="acceleration"></param>
        public void ApplyContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(RigidbodyActions.ApplyAcceleration, acceleration);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="acceleration"></param>
        public void ApplyRelativeContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(RigidbodyActions.ApplyAccelerationRelative, acceleration);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyContinuousForce(Vector3 force)
        {
            PerformGroupAction(RigidbodyActions.ApplyForce, force);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyRelativeContinuousForce(Vector3 force)
        {
            PerformGroupAction(RigidbodyActions.ApplyForceRelative, force);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(RigidbodyActions.ApplyImpulse, impulse);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyRelativeInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(RigidbodyActions.ApplyImpulseRelative, impulse);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ApplyInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(RigidbodyActions.ApplyVelocityChange, velocityChange);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="velocityChange"></param>
        public void ApplyRelativeInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(RigidbodyActions.ApplyVelocityChangeRelative, velocityChange);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SleepAll()
        {
            PerformGroupAction(RigidbodyActions.Sleep);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void WakeUpAll()
        {
            PerformGroupAction(RigidbodyActions.WakeUp);
        }
    }
}