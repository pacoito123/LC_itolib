using itolib.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class FearInducer : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Fear Inducer")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<float> onPlayerSpook = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AnimationCurve fearDistanceCurve = AnimationCurve.Constant(0.0f, 1.0f, 0.5f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float lookAngle = 45.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int lookRange = 60;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float proximityRange = -1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform? localPlayerCamera;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                enabled = false;

                return;
            }

            localPlayerCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Update()
        {
            if (localPlayerCamera == null)
            {
                enabled = false;

                return;
            }

            float distance = Vector3.Distance(localPlayerCamera.position, transform.position);
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(transform.position,
                lookAngle, lookRange, proximityRange, layerMask))
            {
                float fearLevel = fearDistanceCurve.Evaluate(1 - (distance / lookRange));
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(fearLevel, true);

                onPlayerSpook.Invoke(fearLevel);

                enabled = false;
            }
        }
    }
}