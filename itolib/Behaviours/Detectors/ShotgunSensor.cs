using GameNetcodeStuff;
using itolib.Compatibility;
using itolib.Extensions;
using System;
using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Detects an attached player's successful <c>ShotgunItem</c> activation.
    /// </summary>
    public class ShotgunSensor : MovementSensor
    {
        /// <summary>
        ///     Distance-based angle required for the shot to be considered within line of sight, in degrees (<c>0°</c> to <c>180°</c>).
        /// </summary>
        /// <remarks><c>0</c> is as close to the sensor as possible, <c>1</c> is at maximum range from the sensor.</remarks>
        [Space(5.0f)]
        [Header("Shotgun Sensor")]
        [Tooltip("Distance-based angle required for the shot to be considered within line of sight, in degrees (0° to 180°). '0' is as close to the sensor "
            + "as possible, '1' is at maximum range from the sensor.")]
        [SerializeField] private AnimationCurve angleCurve = AnimationCurve.Linear(0.0f, 45.0f, 1.0f, 5.0f);

        /// <summary>
        ///     Maximum range for a shot to be detected.
        /// </summary>
        [Tooltip("Maximum range for a shot to be detected.")]
        [Min(0.0f)]
        [SerializeField] private float shootRange = 30.0f;

        /// <summary>
        ///     Maximum proximity range for a shot to be detected, regardless of where the player is looking at. Can be set to <c>-1</c> to disable.
        /// </summary>
        [Tooltip("Maximum proximity range for a shot to be detected, regardless of where the player is looking at. Can be set to '-1' to disable.")]
        [Min(0.0f)]
        [SerializeField] private float proximityRange = -1.0f;

        /// <summary>
        ///     <c>LayerMask</c> for all layers that should block shot detection.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block shot detection.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Minimum angle required for the shot to be considered within line of sight, in degrees.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Minimum angle required for the shot to be considered within line of sight, in degrees.")]
        [SerializeField] private float shootAngle = -1.0f;

        /// <summary>
        ///     Set some default values for shooting purposes.
        /// </summary>
        protected override void Reset()
        {
            // Attaching locally is recommended for multiple players to be able to shoot.
            attachLocally = true;
            detachOnExit = true;

            // Set player action to check for shooting.
            actionToTrigger = "ActivateItem";

            // Hold action should be off for shotgun purposes.
            holdAction = false;

            // Set default shotgun layer mask.
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");

            base.Reset();
        }

        /// <summary>
        ///     Trigger shot detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose shooting was detected.</param>
        protected override void PlayerMoved(PlayerControllerB player)
        {
            // Check if shoot range is valid.
            if (shootRange <= 0.0f)
            {
                return;
            }

            // Check if player is not holding an item or is unable to use it for any reason.
            if (player.throwingObject || !player.CanUseItem())
            {
                return;
            }

            // Check if player has successfully fired a shotgun shell.
            GrabbableObject heldItem = player.currentlyHeldObjectServer;
            if (!CheckShotgunFire(heldItem) && (!BeanieLibCompatibility.Enabled || !BeanieLibCompatibility.CheckBeanieShotgunFire(heldItem)))
            {
                return;
            }

            Vector3 pos = transform.position;
            float sqrRange = shootRange * shootRange,
                sqrDistance = (attachedPlayerTransform.position - pos).sqrMagnitude,
                sqrProximityRange = (proximityRange > 0.0f) ? (proximityRange * proximityRange) : -1.0f,
                shootAngle = (this.shootAngle < 0.0f) ? Math.Clamp(angleCurve.Evaluate(Mathf.Sqrt(sqrDistance / sqrRange)), 0.0f, 180.0f) : this.shootAngle;

            // Check if the player is within range and has unobstructed line of sight with the sensor.
            if (!player.HasLineOfSightToPosition(pos, sqrDistance, shootAngle, sqrRange, sqrProximityRange, layerMask))
            {
                return;
            }

            base.PlayerMoved(player);
        }

        /// <summary>
        ///     Check if the item being activated is a <c>ShotgunItem</c> that is not reloading, has shells, has its safety disabled, and is not on cooldown.
        /// </summary>
        /// <param name="heldItem"></param>
        /// <returns>Whether the <c>ShotgunItem</c> was fired or not.</returns>
        private static bool CheckShotgunFire(GrabbableObject? heldItem)
        {
            // TODO: Shells loaded count is inaccurate at this point, this check happens after firing reduces ammo count.
            return heldItem is ShotgunItem shotgun && !shotgun.isReloading && shotgun.shellsLoaded != 0 && !shotgun.safetyOn
                && (!shotgun.RequireCooldown() || shotgun.currentUseCooldown == shotgun.useCooldown);
        }
    }
}