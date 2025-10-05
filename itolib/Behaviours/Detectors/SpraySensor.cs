using GameNetcodeStuff;
using itolib.Extensions;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Represents a threshold for a certain number of sprays required to trigger something.
    /// </summary>
    [Serializable]
    public struct SprayThreshold
    {
        /// <summary>
        ///     Number of sprays required to trigger this threshold.
        /// </summary>
        [Header("Spray Threshold")]
        [Tooltip("Number of sprays required to trigger this threshold.")]
        [Min(1)]
        public int spraysRequired = 1;

        /// <summary>
        ///     Callback invoked when the spray threshold is reached, with the number of sprays as parameter.
        /// </summary>
        [Tooltip("Callback invoked when the spray threshold is reached, with the number of sprays as parameter.")]
        public UnityEvent<int> onReachedThreshold = new();

        /// <summary>
        ///     Callback invoked when a spray is detected below the spray threshold, with the number of sprays as parameter.
        /// </summary>
        [Tooltip("Callback invoked when a spray is detected below the spray threshold, with the number of sprays as parameter.")]
        public UnityEvent<int> onBelowThreshold = new();

        /// <summary>
        ///     Callback invoked when a spray is detected above the spray threshold, with the number of sprays as parameter.
        /// </summary>
        [Tooltip("Callback invoked when a spray is detected above the spray threshold, with the number of sprays as parameter.")]
        public UnityEvent<int> onAboveThreshold = new();

        /// <summary>
        ///     Whether the threshold should only trigger once or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Makes <c>onAboveThreshold</c> event not be called.</remarks>
        [Tooltip("Whether the threshold should only trigger once or not. NOTE: Makes 'onAboveThreshold' event not be called.")]
        public bool triggerOnce = false;

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public SprayThreshold() { }
    }

    /// <summary>
    ///     Detects an attached player's successful <c>SprayPaintItem</c> activation.
    /// </summary>
    public class SpraySensor : MovementSensor
    {
        /// <summary>
        ///     List of thresholds for things to occur upon spraying a certain amount of times.
        /// </summary>
        [Header("Spray Sensor")]
        [Tooltip("List of thresholds for things to occur upon spraying a certain amount of times.")]
        [SerializeField] private List<SprayThreshold> sprayThresholds = [];

        /// <summary>
        ///     Whether spray paint spraying should be detected or not.
        /// </summary>
        [Tooltip("Whether spray paint spraying should be detected or not.")]
        [SerializeField] private bool allowSprayPaint = true;

        /// <summary>
        ///     Whether weed killer spraying should be detected or not.
        /// </summary>
        [Tooltip("Whether weed killer spraying should be detected or not.")]
        [SerializeField] private bool allowWeedKiller = true;

        /// <summary>
        ///     Minimum angle required for the spray to be considered within line of sight, in degrees.
        /// </summary>
        [Tooltip("Minimum angle required for the spray to be considered within line of sight, in degrees.")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float sprayAngle = 45.0f;

        /// <summary>
        ///     Maximum range for a spray to be detected.
        /// </summary>
        [Tooltip("Maximum range for a spray to be detected.")]
        [Min(0.0f)]
        [SerializeField] private float sprayRange = 2.0f;

        /// <summary>
        ///     Maximum proximity range for a spray to be detected, regardless of where the player is looking at. Can be set to <c>-1</c> to disable.
        /// </summary>
        [Tooltip("Maximum proximity range for a spray to be detected, regardless of where the player is looking at. Can be set to '-1' to disable.")]
        [Min(0.0f)]
        [SerializeField] private float proximityRange = -1.0f;

        /// <summary>
        ///     Callback invoked when a spray is successfully detected, with the player in question as parameter.
        /// </summary>
        [Tooltip("Callback invoked when a spray is successfully detected, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onSprayPerformed = new();

        /// <summary>
        ///     <c>LayerMask</c> for all layers that should block spray detection.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block spray detection.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Total number of sprays that have been detected.
        /// </summary>
        private int timesSprayed;

        /// <summary>
        ///     Set some default values for spraying purposes.
        /// </summary>
        protected override void Reset()
        {
            // Attaching locally is recommended for multiple players to be able to spray.
            attachLocally = true;
            detachOnExit = true;

            // Set player action to check for spraying.
            actionToTrigger = "ActivateItem";

            // Hold action should be off for weed killer detection, but it could be useful for some spray paint shenanigans.
            holdAction = false;

            // Set default weed killer spray layer mask.
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");

            base.Reset();
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Sort thresholds by lowest to greatest number of sprays required.
            sprayThresholds.Sort((thresholdA, thresholdB) => thresholdB.spraysRequired - thresholdA.spraysRequired);

            // Add spray performed function to the movement detected callback.
            onMovementDetected.AddListener(PerformSpray);

            base.Start();
        }

        /// <summary>
        ///     Trigger spray detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose spraying was detected.</param>
        private void PerformSpray(PlayerControllerB player)
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

            // Check if the item being activated is neither spray paint nor weed killer, or the bottle is empty.
            if (player.currentlyHeldObjectServer is not SprayPaintItem spray || (!allowSprayPaint && !spray.isWeedKillerSprayBottle)
                || (!allowWeedKiller && spray.isWeedKillerSprayBottle) || spray.sprayCanTank <= 0.0f || spray.sprayCanShakeMeter <= 0.0f)
            {
                return;
            }

            // Check if the player is within range and has unobstructed line of sight with the sensor.
            if (player.HasLineOfSightToPosition(transform.position, sprayAngle, sprayRange, proximityRange, layerMask))
            {
                // Trigger spray detection on the local client.
                PerformedSprayLocal(player);

                if (IsSpawned) // TODO: Separate local field?
                {
                    // Send spray detection to all other clients.
                    PerformedSprayRpc(player);
                }
            }
        }

        /// <summary>
        ///     Trigger spray detection on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the detected player.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PerformedSprayRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Trigger spray detection on the local client.
                PerformedSprayLocal(player);
            }
        }

        /// <summary>
        ///     Trigger spray detection on the local client.
        /// </summary>
        /// <param name="player">Player whose spraying was detected.</param>
        private void PerformedSprayLocal(PlayerControllerB player)
        {
            // Increment amount of times sprayed.
            timesSprayed++;

            // Invoke spray performed callback, with the detected player as parameter.
            onSprayPerformed.Invoke(player);

            // Check all spray thresholds.
            foreach (SprayThreshold sprayThreshold in sprayThresholds)
            {
                if (timesSprayed < sprayThreshold.spraysRequired)
                {
                    // Invoke below threshold callback, with the number of spray as parameter.
                    sprayThreshold.onBelowThreshold.Invoke(timesSprayed);
                }

                if (timesSprayed == sprayThreshold.spraysRequired)
                {
                    // Invoke reached threshold callback, with the number of spray as parameter.
                    sprayThreshold.onReachedThreshold.Invoke(timesSprayed);
                }

                if (timesSprayed > sprayThreshold.spraysRequired)
                {
                    // Invoke above threshold callback, with the number of spray as parameter.
                    sprayThreshold.onAboveThreshold.Invoke(timesSprayed);
                }
            }

            // Remove any triggered thresholds set to trigger once.
            _ = sprayThresholds.RemoveAll(sprayThreshold => sprayThreshold.triggerOnce && timesSprayed >= sprayThreshold.spraysRequired);
        }
    }
}