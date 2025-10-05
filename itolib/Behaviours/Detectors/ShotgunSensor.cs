using GameNetcodeStuff;
using itolib.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

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
        ///     Callback invoked when a shot is successfully detected, with the player in question as parameter.
        /// </summary>
        [Tooltip("Callback invoked when a shot is successfully detected, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onShotPerformed = new();

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

            // Set default shotgun layer mask. TODO: Update
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");

            base.Reset();
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Add shot performed function to the movement detected callback.
            onMovementDetected.AddListener(PerformShot);

            base.Start();
        }

        /// <summary>
        ///     Trigger shot detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose shooting was detected.</param>
        private void PerformShot(PlayerControllerB player)
        {
            // Check if player is not the local client.
            if (player == null || !player.IsLocalClient())
            {
                return;
            }

            // Check if player is not holding an item.
            if (player.throwingObject || !player.isHoldingObject || player.currentlyHeldObjectServer == null)
            {
                return;
            }

            // Check if the item being activated is not a shotgun, or the shotgun is reloading, has no shells, or has its safety enabled.
            if (player.currentlyHeldObjectServer is not ShotgunItem shotgun || shotgun.isReloading || shotgun.shellsLoaded == 0 || shotgun.safetyOn)
            {
                return;
            }

            // Check if the player is within range and has unobstructed line of sight with the sensor.
            if (player.HasLineOfSightToPosition(transform.position, shootAngle, shootRange, proximityRange, layerMask))
            {
                // Trigger shot detection on the local client.
                PerformedShotLocal(player);

                if (IsSpawned) // TODO: Separate local field?
                {
                    // Send shot detection to all other clients.
                    PerformedShotRpc(player);
                }
            }
        }

        /// <summary>
        ///     Trigger shot detection on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the detected player.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PerformedShotRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Trigger shot detection on the local client.
                PerformedShotLocal(player);
            }
        }

        /// <summary>
        ///     Trigger shot detection on the local client.
        /// </summary>
        /// <param name="player">Player whose shooting was detected.</param>
        private void PerformedShotLocal(PlayerControllerB player)
        {
            // Invoke shot performed callback, with the detected player as parameter.
            onShotPerformed.Invoke(player);
        }
    }
}