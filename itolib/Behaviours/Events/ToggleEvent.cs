using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("onEnable")]
        public UnityEvent toggleOn = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [FormerlySerializedAs("onDisable")]
        public UnityEvent toggleOff = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            enabled = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            toggleOn.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDisable()
        {
            toggleOff.Invoke();
        }
    }
}