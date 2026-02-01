using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    /// 	Represents an event invoked after a specific <c>Renderer</c> becomes visible or invisible to all cameras being displayed to the local client.
    /// </summary>
    /// <remarks><b>NOTE:</b> Doesn't work quite right for a <c>LODGroup</c>.</remarks>
    [RequireComponent(typeof(Renderer))]
    public class RenderEvent : MonoBehaviour
    {
        /// <summary>
        ///     Callback invoked when the <c>Renderer</c> changes its visibility, with the parameter being whether it became visible or not.
        /// </summary>
        [Header("Render Event")]
        [Tooltip("Callback invoked when the Renderer changes its visibility, with the parameter being whether it became visible or not.")]
        [SerializeField] private UnityEvent<bool> onVisibilityChanged = new();

        /// <summary>
        ///     Callback invoked when the <c>Renderer</c> becomes visible.
        /// </summary>
        [Tooltip("Callback invoked when a Renderer becomes visible.")]
        [SerializeField] private UnityEvent onBecameVisible = new();

        /// <summary>
        ///     Callback invoked when the <c>Renderer</c> becomes invisible.
        /// </summary>
        [Tooltip("Callback invoked when a Renderer becomes invisible.")]
        [SerializeField] private UnityEvent onBecameInvisible = new();

        /// <summary>
        ///     Handle invoking event upon the <c>Renderer</c> becoming visible.
        /// </summary>
        private void OnBecameVisible()
        {
            // Check if called from Editor, or disabled.
            if (Application.isEditor || !enabled)
            {
                return;
            }

            // Invoke events for becoming visible.
            onBecameVisible.Invoke();
            onVisibilityChanged.Invoke(true);
        }

        /// <summary>
        ///     Handle invoking event upon the <c>Renderer</c> becoming invisible.
        /// </summary>
        private void OnBecameInvisible()
        {
            // Check if called from Editor, or disabled.
            if (Application.isEditor || !enabled)
            {
                return;
            }

            // Invoke events for becoming invisible.
            onBecameInvisible.Invoke();
            onVisibilityChanged.Invoke(false);
        }
    }
}