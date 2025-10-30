using GameNetcodeStuff;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class ConnectedRope : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Connected Rope")]
        [Tooltip("")]
        [SerializeField] private LineRenderer lineRenderer = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Transform?[]? connectedPoints;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (connectedPoints == null || connectedPoints.Length == 0 || connectedPoints.Length > lineRenderer.positionCount)
            {
                // TODO: Log warning.
                enabled = false;

                return;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void LateUpdate()
        {
            Vector3[] positions = new Vector3[connectedPoints?.Length ?? 0];

            for (int i = 0; i < connectedPoints?.Length; i++)
            {
                Transform? point = connectedPoints[i];

                positions[i] = (point != null) ? (lineRenderer.useWorldSpace ? point.position : point.localPosition) : lineRenderer.GetPosition(i);
            }

            lineRenderer.SetPositions(positions);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="attachTo"></param>
        public void AttachStart(Transform attachTo)
        {
            if (connectedPoints?.Length > 0)
            {
                connectedPoints[0] = attachTo;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="attachTo"></param>
        public void AttachEnd(Transform attachTo)
        {
            if (connectedPoints?.Length > 0)
            {
                connectedPoints[^1] = attachTo;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void AttachPlayerStart(PlayerControllerB player)
        {
            AttachStart(player.transform);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void AttachPlayerEnd(PlayerControllerB player)
        {
            AttachEnd(player.transform);
        }
    }
}