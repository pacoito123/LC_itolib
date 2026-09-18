using GameNetcodeStuff;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     Represents a sequence of points connected by a <c>LineRenderer</c>.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class ConnectedRope : MonoBehaviour
    {
        /// <summary>
        ///     <c>LineRenderer</c> to update the positions of.
        /// </summary>
        [Header("Connected Rope")]
        [Tooltip("LineRenderer to update the positions of.")]
        [SerializeField] private LineRenderer lineRenderer;

        /// <summary>
        ///     List of <c>Transform</c> points to connect together.
        /// </summary>
        /// <remarks><b>NOTE:</b> Number of <c>Transform</c> points must match the number of positions in the <c>LineRenderer</c>.</remarks>
        [Tooltip("List of Transform points to connect together. NOTE: Number of Transform points must match the number of positions in the LineRenderer.")]
        [SerializeField] private Transform?[]? connectedPoints;

        /// <summary>
        ///     Whether positions should update only when the <c>LineRenderer</c> is visible or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Other")]
        [Tooltip("Whether positions should update only when the LineRenderer is visible or not.")]
        [SerializeField] private bool disableWhenCulled = true;

        /// <summary>
        ///     Current positions for each connected <c>LineRenderer</c> point.
        /// </summary>
        private Vector3[]? positions;

        /// <summary>
        ///     Assign <c>LineRenderer</c> field when creating or resetting the component.
        /// </summary>
        private void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        ///     Initialize array with current positions.
        /// </summary>
        private void Awake()
        {
            positions = new Vector3[connectedPoints?.Length ?? 1];
        }

        /// <summary>
        ///     Make sure there is a <c>LineRenderer</c> component to target, with a valid number of points.
        /// </summary>
        private void OnEnable()
        {
            if (lineRenderer == null || !TryGetComponent(out lineRenderer)
                || connectedPoints == null || connectedPoints.Length == 0 || connectedPoints.Length > lineRenderer.positionCount)
            {
                Plugin.Logger.LogWarning($"Could not find a LineRenderer with a valid amount of points for ConnectedRope component in GameObject '{gameObject.name}'.");
                enabled = false;
            }
        }

        /// <summary>
        ///     Update positions for all connected <c>Transform</c> points.
        /// </summary>
        private void LateUpdate()
        {
            for (int i = 0; i < positions?.Length; i++)
            {
                Transform? point = connectedPoints?[i];

                // Use local position if LineRenderer is not set to use world space.
                positions[i] = (point != null) ? (lineRenderer.useWorldSpace ? point.position : point.localPosition) : lineRenderer.GetPosition(i);
            }

            lineRenderer.SetPositions(positions);
        }

        /// <summary>
        ///     Handle invoking event upon the <c>LineRenderer</c> becoming visible.
        /// </summary>
        private void OnBecameVisible()
        {
            // Check if called from Editor.
            if (Application.isEditor || !disableWhenCulled)
            {
                return;
            }

            enabled = true;
        }

        /// <summary>
        ///     Handle invoking event upon the <c>LineRenderer</c> becoming invisible.
        /// </summary>
        private void OnBecameInvisible()
        {
            // Check if called from Editor.
            if (Application.isEditor || !disableWhenCulled)
            {
                return;
            }

            enabled = false;
        }

        /// <summary>
        ///     Attach a given <c>Transform</c> to the start of the <c>ConnectedRope</c>.
        /// </summary>
        /// <param name="point"><c>Transform</c> point to connect.</param>
        public void AttachStart(Transform point)
        {
            if (connectedPoints?.Length > 0)
            {
                connectedPoints[0] = point;
            }
        }

        /// <summary>
        ///     Attach a given <c>Transform</c> to the end of the <c>ConnectedRope</c>.
        /// </summary>
        /// <param name="point"><c>Transform</c> point to connect.</param>
        public void AttachEnd(Transform point)
        {
            if (connectedPoints?.Length > 0)
            {
                connectedPoints[^1] = point;
            }
        }

        /// <summary>
        ///     Attach a given player to the start of the <c>ConnectedRope</c>.
        /// </summary>
        /// <param name="player">Player to connect.</param>
        public void AttachPlayerStart(PlayerControllerB player)
        {
            AttachStart(player.transform);
        }

        /// <summary>
        ///     Attach a given player to the end of the <c>ConnectedRope</c>.
        /// </summary>
        /// <param name="player">Player to connect.</param>
        public void AttachPlayerEnd(PlayerControllerB player)
        {
            AttachEnd(player.transform);
        }
    }
}