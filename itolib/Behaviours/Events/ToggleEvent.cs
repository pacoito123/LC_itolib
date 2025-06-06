using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ToggleEvent : MonoBehaviour // TODO: Networked?
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Toggle Event")]
        [Tooltip("")]
        public UnityEvent? onEnable;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDisable;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            onEnable?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDisable()
        {
            onDisable?.Invoke();
        }
    }
}