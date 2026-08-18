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
        [SerializeField] private LineRenderer lineRenderer;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Transform?[]? connectedPoints;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Other")]
        [Tooltip("")]
        [SerializeField] private bool disableWhenCulled = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Vector3[]? positions;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            positions = new Vector3[connectedPoints?.Length ?? 1];
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (lineRenderer == null || !TryGetComponent(out lineRenderer)
                || connectedPoints == null || connectedPoints.Length == 0 || connectedPoints.Length > lineRenderer.positionCount)
            {
                Plugin.Logger.LogWarning($"Could not find LineRenderer for ConnectedRope component in GameObject '{gameObject.name}'.");
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void LateUpdate()
        {
            for (int i = 0; i < positions?.Length; i++)
            {
                Transform? point = connectedPoints?[i];

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