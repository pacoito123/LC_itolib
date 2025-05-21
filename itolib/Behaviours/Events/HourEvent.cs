using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class HourEvent : MonoBehaviour // TODO: Improve with LLL events
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public bool HasRun { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(100, 1080)]
        public int playAtTime = 100;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onTimedEvent;

        private void Awake()
        {
            enabled = false;
        }

        private void OnEnable()
        {
            if (!HasRun && TimeOfDay.Instance.globalTime >= playAtTime)
            {
                onTimedEvent?.Invoke();
                HasRun = true;
            }
        }
    }
}