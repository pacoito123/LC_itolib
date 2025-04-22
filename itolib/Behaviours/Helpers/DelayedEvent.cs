using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class DelayedEvent : MonoBehaviour // TODO: Networked?
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public float Timer { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Delayed Event")]
        [Tooltip("")]
        public float delayTimer = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool runsContinuously;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDelayedEvent;

        private void Update()
        {
            if (Timer < delayTimer)
            {
                Timer += Time.deltaTime;
                return;
            }

            onDelayedEvent?.Invoke();
            ResetTimer();

            if (!runsContinuously)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            ResetTimer();
        }

        private void OnDisable()
        {
            ResetTimer();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ResetTimer()
        {
            Timer = 0.0f;
        }
    }
}