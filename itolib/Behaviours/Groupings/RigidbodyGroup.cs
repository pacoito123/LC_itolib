using System;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     Represents a group of <c>Rigidbody</c> components to perform bulk operations on.
    /// </summary>
    public class RigidbodyGroup : ComponentGroup<Rigidbody>
    {
        /// <summary>
        ///     All actions able to be performed on every <c>Rigidbody</c> component in the group.
        /// </summary>
        private enum RigidbodyActions : byte
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
        ///     Whether to wake up sleeping <c>Rigidbody</c> components when applying forces or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Rigidbody Group")]
        [Tooltip("Whether to wake up sleeping Rigidbody components when applying forces or not.")]
        [SerializeField] private bool respectSleep;

        /// <inheritdoc/>
        protected override void PerformSingleAction(Rigidbody rigidbody, Enum actionID, object? parameter = null)
        {
            // Check if action ID is invalid.
            if (actionID is not RigidbodyActions rigidbodyActionID)
            {
                return;
            }

            // Check if sleeping Rigidbodies should be respected.
            if (respectSleep && (int)rigidbodyActionID < 8 && rigidbody.IsSleeping())
            {
                return;
            }

            // Obtain force parameter, if one is provided.
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

        /// <inheritdoc/>
        protected override void EnableSingleComponent(Rigidbody rigidbody, bool enabled)
        {
            rigidbody.isKinematic = !enabled;
            rigidbody.detectCollisions = enabled;
        }

        /// <inheritdoc/>
        protected override void ToggleSingleComponent(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = !rigidbody.isKinematic;
            rigidbody.detectCollisions = !rigidbody.isKinematic; // Detect collisions only when not kinematic.
        }

        /// <summary>
        ///     Apply a continuous acceleration force in world space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="acceleration">Acceleration force to apply, in world space.</param>
        public void ApplyContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(RigidbodyActions.ApplyAcceleration, acceleration);
        }

        /// <summary>
        ///     Apply a continuous acceleration force in object space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="acceleration">Acceleration force to apply, in object space.</param>
        public void ApplyRelativeContinuousAcceleration(Vector3 acceleration)
        {
            PerformGroupAction(RigidbodyActions.ApplyAccelerationRelative, acceleration);
        }

        /// <summary>
        ///     Apply a continuous force in world space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="force">Force to apply, in world space.</param>
        public void ApplyContinuousForce(Vector3 force)
        {
            PerformGroupAction(RigidbodyActions.ApplyForce, force);
        }

        /// <summary>
        ///     Apply a continuous force in object space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="force">Force to apply, in object space.</param>
        public void ApplyRelativeContinuousForce(Vector3 force)
        {
            PerformGroupAction(RigidbodyActions.ApplyForceRelative, force);
        }

        /// <summary>
        ///     Apply an impulse force in world space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="impulse">Impulse force to apply, in world space.</param>
        public void ApplyInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(RigidbodyActions.ApplyImpulse, impulse);
        }

        /// <summary>
        ///     Apply an impulse force in object space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="impulse">Impulse force to apply, in object space.</param>
        public void ApplyRelativeInstantImpulse(Vector3 impulse)
        {
            PerformGroupAction(RigidbodyActions.ApplyImpulseRelative, impulse);
        }

        /// <summary>
        ///     Apply a velocity force in world space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="velocityChange">Velocity force to apply, in world space.</param>
        public void ApplyInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(RigidbodyActions.ApplyVelocityChange, velocityChange);
        }

        /// <summary>
        ///     Apply a velocity force in object space to every <c>Rigidbody</c> component in the group.
        /// </summary>
        /// <param name="velocityChange">Velocity force to apply, in object space.</param>
        public void ApplyRelativeInstantVelocityChange(Vector3 velocityChange)
        {
            PerformGroupAction(RigidbodyActions.ApplyVelocityChangeRelative, velocityChange);
        }

        /// <summary>
        ///     Force sleep upon every <c>Rigidbody</c> component in the group.
        /// </summary>
        public void SleepAll()
        {
            PerformGroupAction(RigidbodyActions.Sleep);
        }

        /// <summary>
        ///     Awaken every <c>Rigidbody</c> component in the group.
        /// </summary>
        public void WakeUpAll()
        {
            PerformGroupAction(RigidbodyActions.WakeUp);
        }
    }
}