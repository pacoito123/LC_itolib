using GameNetcodeStuff;
using itolib.Extensions;
using System;
using System.Collections.Generic;
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
        ///     Distance-based angle required for the spray to be considered within line of sight, in degrees (<c>0°</c> to <c>180°</c>).
        /// </summary>
        /// <remarks><c>0</c> is as close to the sensor as possible, <c>1</c> is at maximum range from the sensor.</remarks>
        [Space(5.0f)]
        [Tooltip("Distance-based angle required for the spray to be considered within line of sight, in degrees ('0°' to '180°'). '0' is as close to the sensor "
            + "as possible, '1' is at maximum range from the sensor.")]
        [SerializeField] private AnimationCurve angleCurve = AnimationCurve.Linear(0.0f, 45.0f, 1.0f, 5.0f);

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
        ///     <c>LayerMask</c> for all layers that should block spray detection.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block spray detection.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Minimum angle required for the spray to be considered within line of sight, in degrees.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Minimum angle required for the spray to be considered within line of sight, in degrees.")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float sprayAngle = -1.0f;

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

            base.Start();
        }

        /// <summary>
        ///     Trigger spray detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose spraying was detected.</param>
        protected override void PlayerMoved(PlayerControllerB player)
        {
            // Check if spray range is valid.
            if (sprayRange <= 0.0f)
            {
                return;
            }

            // Check if player is not holding an item or is unable to use it for any reason.
            if (player.throwingObject || !player.CanUseItem())
            {
                return;
            }

            // Check if the item being activated is neither spray paint nor weed killer, or the bottle is empty.
            if (player.currentlyHeldObjectServer is not SprayPaintItem spray || (!allowSprayPaint && !spray.isWeedKillerSprayBottle)
                || (!allowWeedKiller && spray.isWeedKillerSprayBottle) || spray.sprayCanTank <= 0.0f || spray.sprayCanShakeMeter <= 0.0f)
            {
                return;
            }

            Vector3 pos = transform.position;
            float sqrRange = sprayRange * sprayRange,
                sqrDistance = (attachedPlayerTransform.position - pos).sqrMagnitude,
                sqrProximityRange = (proximityRange > 0.0f) ? proximityRange * proximityRange : -1.0f,
                sprayAngle = (this.sprayAngle >= 0.0f) ? Mathf.Clamp(angleCurve.Evaluate(Mathf.Sqrt(sqrDistance / sqrRange)), 0.0f, 180.0f) : this.sprayAngle;

            // Check if the player is within range and has unobstructed line of sight with the sensor.
            if (!player.HasLineOfSightToPosition(transform.position, sprayAngle, sprayRange, sqrProximityRange, layerMask))
            {
                return;
            }

            base.PlayerMoved(player);
        }

        /// <summary>
        ///     Trigger spray detection from an attached player on the local client.
        /// </summary>
        /// <param name="player">Player whose spraying was detected.</param>
        protected override void PlayerMovedLocal(PlayerControllerB player)
        {
            base.PlayerMovedLocal(player);

            // Increment amount of times sprayed.
            timesSprayed++;

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