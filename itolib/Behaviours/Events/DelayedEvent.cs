using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class DelayedEvent : MonoBehaviour // TODO: Networked?
    {
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

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float timer;

        private void Update()
        {
            if (timer < delayTimer)
            {
                timer += Time.deltaTime;
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
            timer = 0.0f;
        }
    }
}