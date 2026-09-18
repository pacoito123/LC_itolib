using itolib.Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     Represents an object that increases the local player's fear level when looked at (e.g. a player body), or manually (e.g. Forest Giant chase).
    /// </summary>
    public class FearInducer : MonoBehaviour
    {
        /// <summary>
        ///     Callback invoked when the local player's fear level is increased.
        /// </summary>
        [Header("Fear Inducer")]
        [Tooltip("Callback invoked when the local player's fear level is increased.")]
        [SerializeField] private UnityEvent<float> onPlayerSpook = new();

        /// <summary>
        ///     Distance-based fear level change to apply to the local player.
        /// </summary>
        /// <remarks><c>0</c> is as close to the fear source as possible, <c>1</c> is at maximum range from it.</remarks>
        [Tooltip("Distance-based fear level change to apply to the local player. '0' is as close to the fear source as possible, '1' is at maximum range from it.")]
        [SerializeField] private AnimationCurve fearDistanceCurve = AnimationCurve.Constant(0.0f, 1.0f, 0.5f);

        /// <summary>
        ///     Distance-based angle required to be considered within line of sight of the fear source, in degrees (<c>0°</c> to <c>180°</c>).
        /// </summary>
        /// <remarks><c>0</c> is as close to the fear source as possible, <c>1</c> is at maximum range from it.</remarks>
        [Space(5.0f)]
        [Tooltip("Distance-based angle required to be considered within line of sight of the fear source, in degrees ('0°' to '180°'). '0' is as close to the fear "
            + "source as possible, '1' is at maximum range from it.")]
        [SerializeField] private AnimationCurve angleCurve = AnimationCurve.Linear(0.0f, 45.0f, 1.0f, 5.0f);

        /// <summary>
        ///     Maximum range for line of sight check to scale fear distance with. Can be set to <c>0</c> to disable automatic line of sight checking.
        /// </summary>
        [Tooltip("Maximum range for line of sight check to scale fear distance with. Can be set to '0' to disable automatic line of sight checking.")]
        [Min(0.0f)]
        [SerializeField] private float lookRange = 60.0f;

        /// <summary>
        ///     Maximum proximity range for line of sight check, regardless of where the player is looking at. Can be set to <c>0</c> to disable.
        /// </summary>
        [Tooltip("Maximum proximity range for line of sight check, regardless of where the player is looking at. Can be set to '0' to disable.")]
        [Min(0.0f)]
        [SerializeField] private float proximityRange = 0.0f;

        /// <summary>
        ///     Offset to apply to the fear source for the line of sight check.
        /// </summary>
        [Tooltip("Offset to apply to the fear source for the line of sight check.")]
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        /// <summary>
        ///     <c>LayerMask</c> for all layers that should block line of sight with the fear source.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block line of sight with the fear source.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Minimum angle required to be considered within line of sight of the fear source, in degrees (<c>0°</c> to <c>180°</c>).
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Minimum angle required to be considered within line of sight of the fear source, in degrees ('0°' to '180°').")]
        [SerializeField] private float lookAngle = -1.0f;

        /// <summary>
        ///     Cached <c>Transform</c> of the fear source.
        /// </summary>
        private Transform inducerTransform = null!;

        /// <summary>
        ///     Cached <c>Transform</c> of the local player.
        /// </summary>
        private Transform localPlayerTransform = null!;

        /// <summary>
        ///     Cached <c>Transform</c> of the local player's camera.
        /// </summary>
        private Transform localPlayerCameraTransform = null!;

        /// <summary>
        ///     Set some default values for spook purposes.
        /// </summary>
        private void Reset()
        {
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        private void Awake()
        {
            // Cache fear source Transform.
            inducerTransform = transform;

            // Check if local player exists.
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null
                || GameNetworkManager.Instance.localPlayerController.gameplayCamera == null)
            {
                enabled = false;

                return;
            }

            // Cache local player Transforms.
            localPlayerTransform = GameNetworkManager.Instance.localPlayerController.transform;
            localPlayerCameraTransform = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
        }

        /// <summary>
        ///     Handle automatic line of sight checking.
        /// </summary>
        private void Update()
        {
            // Check if automatic line of sight checking is enabled.
            if (lookRange <= 0.0f)
            {
                enabled = false;

                return;
            }

            // Increase local player's fear level, if within line of sight.
            IncreaseFearLevel(requireLineOfSight: true);
        }

        /// <summary>
        ///     Increase fear level for the local player relative to their distance from the fear source.
        /// </summary>
        /// <param name="requireLineOfSight">Whether to require line of sight or not.</param>
        public void IncreaseFearLevel(bool requireLineOfSight = false)
        {
            // Check if local player exists.
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null
                || localPlayerTransform == null || localPlayerCameraTransform == null)
            {
                enabled = false;

                return;
            }

            // Obtain target position, with the additional offset applied.
            Vector3 targetPosition = inducerTransform.position + (inducerTransform.rotation * targetOffset);

            // Calculate squared parameters to check line of sight with the fear source.
            float sqrRange = lookRange * lookRange,
                sqrDistance = (localPlayerTransform.position - targetPosition).sqrMagnitude,
                sqrProximityRange = (proximityRange > 0.0f) ? (proximityRange * proximityRange) : -1.0f;

            // Obtain fear level to apply from defined curve, at the current distance.
            float fearLevel = Math.Clamp(fearDistanceCurve.Evaluate(Mathf.Sqrt(sqrDistance / sqrRange)), 0.0f, 1.0f);

            // Check if line of sight is required.
            if (requireLineOfSight)
            {
                // Obtain fear angle from defined curve, at the current distance.
                float lookAngle = (this.lookAngle < 0.0f) ? Math.Clamp(angleCurve.Evaluate(Mathf.Sqrt(sqrDistance / sqrRange)), 0.0f, 180.0f) : this.lookAngle;

                // Check if target position is within line of sight.
                if (!GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(targetPosition, localPlayerCameraTransform.position,
                    sqrDistance, lookAngle, sqrRange, sqrProximityRange, layerMask))
                {
                    return;
                }
            }

            // Apply fear level to the local player.
            ApplyFearLevel(fearLevel);
        }

        /// <summary>
        ///     Manually increase fear level for the local player by the specified amount.
        /// </summary>
        /// <param name="fearLevel">Amount to increase fear level by.</param>
        public void IncreaseFearLevel(float fearLevel)
        {
            ApplyFearLevel(fearLevel, onlyGoUp: true);
        }

        /// <summary>
        ///     Manually set fear level for the local player to the specified amount.
        /// </summary>
        /// <param name="fearLevel">Amount to set fear level to.</param>
        public void ApplyFearLevel(float fearLevel)
        {
            ApplyFearLevel(fearLevel, onlyGoUp: false);
        }

        /// <summary>
        ///     Manually increase or set fear level for the local player.
        /// </summary>
        /// <param name="fearLevel">Amount to increase or set fear level.</param>
        /// <param name="onlyGoUp">Whether fear level value should be increased or set.</param>
        private void ApplyFearLevel(float fearLevel, bool onlyGoUp = true)
        {
            // Check if local player exists.
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }

            // Jump to specified fear level and invoke player spook event.
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(fearLevel, onlyGoUp);
            onPlayerSpook.Invoke(fearLevel);

            // Disable after triggering once.
            enabled = false;
        }
    }
}