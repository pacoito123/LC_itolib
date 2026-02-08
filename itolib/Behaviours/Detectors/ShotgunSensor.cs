using GameNetcodeStuff;
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
        ///     Minimum angle required for the shot to be considered within line of sight, in degrees.
        /// </summary>
        [Header("Shotgun Sensor")]
        [Tooltip("Minimum angle required for the shot to be considered within line of sight, in degrees.")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float shootAngle = 45.0f;

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
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block shot detection.")]
        [SerializeField] private LayerMask layerMask;

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
            // Check if player is not holding an item or is unable to use it for any reason.
            if (player.throwingObject || !player.CanUseItem())
            {
                return;
            }

            // Check if the item being activated is not a shotgun, or the shotgun is reloading, has no shells, has its safety enabled, or is on cooldown.
            if (player.currentlyHeldObjectServer is not ShotgunItem shotgun || shotgun.isReloading || shotgun.shellsLoaded == 0 || shotgun.safetyOn || shotgun.currentUseCooldown > 0.0f)
            {
                return;
            }

            // Check if the player is within range and has unobstructed line of sight with the sensor.
            if (!player.HasLineOfSightToPosition(transform.position, shootAngle, shootRange, proximityRange, layerMask))
            {
                return;
            }

            base.PlayerMoved(player);
        }
    }
}