using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using itolib.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Detects a specific player's line of sight towards the sensor.
    /// </summary>
    public class SightSensor : PlayerAttachable
    {
        /// <summary>
        ///     Cooldown interval between each line of sight detection, in seconds.
        /// </summary>
        [Header("Sight Sensor")]
        [Tooltip("Cooldown interval between each line of sight detection, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float triggerInterval = 1.0f;

        /// <summary>
        ///     Detach immediately after each line of sight detection.
        /// </summary>
        [Tooltip("Detach immediately after each line of sight detection.")]
        [SerializeField] private bool detachOnTrigger;

        /// <summary>
        ///     Offset to apply to the target position.
        /// </summary>
        [Tooltip("Offset to apply to the target position.")]
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        /// <summary>
        ///     Distance-based angle required to be considered within line of sight, in degrees (<c>0°</c> to <c>180°</c>).
        /// </summary>
        /// <remarks><c>0</c> is as close to the sensor as possible, <c>1</c> is at maximum range from the sensor.</remarks>
        [Tooltip("Distance-based angle required to be considered within line of sight, in degrees (0° to 180°). '0' is as close to the sensor "
            + "as possible, '1' is at maximum range from the sensor.")]
        [SerializeField] private AnimationCurve angleCurve = AnimationCurve.Linear(0.0f, 45.0f, 1.0f, 5.0f);

        /// <summary>
        ///     Maximum range for line of sight to be detected.
        /// </summary>
        [Tooltip("Maximum range for line of sight to be detected.")]
        [Min(0.0f)]
        [SerializeField] private float lookRange = 60.0f;

        /// <summary>
        ///     Maximum proximity range for line of sight to be detected, regardless of where the player is looking at. Can be set to <c>0</c> to disable.
        /// </summary>
        [Tooltip("Maximum proximity range for line of sight to be detected, regardless of where the player is looking at. Can be set to '0' to disable.")]
        [Min(0.0f)]
        [SerializeField] private float proximityRange = 0.0f;

        /// <summary>
        ///     <c>LayerMask</c> for all layers that should block line of sight detection.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block line of sight detection.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Callback invoked when line of sight is successfully detected, with the player in question as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when line of sight is successfully detected, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onSightDetected = new();

        /// <summary>
        ///     Time passed since the last line of sight detection.
        /// </summary>
        private float timer;

        /// <summary>
        ///     Cached <c>Transform</c> for this sensor.
        /// </summary>
        private Transform sensorTransform = null!;

        /// <summary>
        ///     Attach if the player is alive and enabled.
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected override bool AttachCondition(PlayerControllerB player)
        {
            return !player.isPlayerDead && player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Detach if the player is dead or disabled.
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected override bool DetachCondition(PlayerControllerB player)
        {
            return player.isPlayerDead || !player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Set some default values for sight detection purposes.
        /// </summary>
        protected override void Reset()
        {
            // Attaching locally is recommended for multiple players to be able to be detected.
            attachLocally = true;
            detachOnExit = true;

            // Set default line of sight layer mask.
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");

            base.Reset();
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Cache sensor Transform.
            sensorTransform = transform;

            base.Start();
        }

        /// <summary>
        ///     Set cooldown timer to detect line of sight immediately after being enabled.
        /// </summary>
        private void OnEnable()
        {
            timer = triggerInterval; // TODO: Maybe add field for this?
        }

        /// <summary>
        ///     Handle line of sight detection and its cooldown interval.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (localPlayerAttached && attachedPlayer != null)
            {
                // Check if detection is currently on cooldown.
                if (timer < triggerInterval)
                {
                    // Increment timer.
                    timer += Time.deltaTime;

                    return;
                }

                // Obtain target position, with the additional offset applied.
                Vector3 targetPosition = sensorTransform.position + (sensorTransform.rotation * targetOffset);

                // Calculate squared parameters to check line of sight with the sensor.
                float sqrRange = lookRange * lookRange,
                    sqrDistance = (attachedPlayerTransform.position - targetPosition).sqrMagnitude,
                    sqrProximityRange = (proximityRange > 0.0f) ? (proximityRange * proximityRange) : -1.0f;

                // Obtain line of sight detection angle from defined curve, at the current distance.
                float lookAngle = Math.Clamp(angleCurve.Evaluate(Mathf.Sqrt(sqrDistance / sqrRange)), 0.0f, 180.0f);

                // Check if player has line of sight with the sensor.
                if (!attachedPlayer.HasLineOfSightToPosition(targetPosition, attachedPlayerCameraTransform.position,
                    sqrDistance, lookAngle, sqrRange, sqrProximityRange, layerMask))
                {
                    return;
                }

                // Trigger line of sight detection.
                SightDetected(attachedPlayer);
            }
        }

        /// <summary>
        ///     Trigger line of sight detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose line of sight was detected.</param>
        protected virtual void SightDetected(PlayerControllerB player)
        {
            // Trigger line of sight detection on the local client.
            SightDetectedLocal(player);

            if (IsSpawned) // TODO: Separate local effect field?
            {
                // Send line of sight detection to all other clients.
                SightDetectedRpc(player);
            }
        }

        /// <summary>
        ///     Trigger line of sight detection on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the detected player.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SightDetectedRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Trigger line of sight detection on the local client.
                SightDetectedLocal(player);
            }
        }

        /// <summary>
        ///     Trigger line of sight detection on the local client.
        /// </summary>
        /// <param name="player">Player whose line of sight was detected.</param>
        protected virtual void SightDetectedLocal(PlayerControllerB player)
        {
            // Invoke line of sight detected callback, with the detected player as parameter.
            onSightDetected.Invoke(player);

            if (detachOnTrigger)
            {
                // Detach player after triggering, if set to do so.
                DetachPlayer();
            }

            if (localPlayerAttached || !attachLocally)
            {
                // Reset cooldown timer after triggering.
                timer = 0.0f;
            }
        }
    }
}