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
        [SerializeField] private float delayTimer = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool runsContinuously;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onDelayedEvent = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private float timer;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            enabled = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Update()
        {
            if (timer < delayTimer)
            {
                timer += Time.deltaTime;
                return;
            }

            onDelayedEvent.Invoke();
            ResetTimer();

            if (!runsContinuously)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            ResetTimer();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
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