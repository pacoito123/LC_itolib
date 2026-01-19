using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    /// 	Represents an event invoked after a specified amount of time, either once or repeatedly.
    /// </summary>
    public class DelayedEvent : MonoBehaviour
    {
        /// <summary>
        ///     Amount of time to wait before invoking the event, in seconds.
        /// </summary>
        [Header("Delayed Event")]
        [Tooltip("Amount of time to wait before invoking the event, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float delayTimer = 1.0f;

        /// <summary>
        ///     Whether the timer should be reset back to <c>0</c> upon pausing the timer or not.
        /// </summary>
        [Tooltip("Whether the timer should be reset back to '0' upon pausing the timer or not.")]
        [SerializeField] private bool resetOnToggle = true;

        /// <summary>
        ///     Whether the timer should disable itself upon triggering once or not.
        /// </summary>
        [Tooltip("Whether the timer should disable itself upon triggering once or not.")]
        [SerializeField] private bool runsContinuously;

        /// <summary>
        ///     Whether the timer should disable itself at the start or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Only really here as a reminder that enabling/disabling the <c>DelayedEvent</c> is what pauses/unpauses it.</remarks>
        [Tooltip("Whether the timer should disable itself at the start or not. NOTE: Only really here as a reminder that enabling/disabling "
            + "the delayed event itself is what pauses/unpauses it.")]
        [SerializeField] private bool disableOnAwake = true;

        /// <summary>
        ///     Callback invoked after the specified amount of time passes.
        /// </summary>
        [Tooltip("Callback invoked after the specified amount of time passes.")]
        [SerializeField] private UnityEvent onDelayedEvent = new();

        /// <summary>
        ///     Time passed since enabling the event (or invoking, if set to run continuously).
        /// </summary>
        private float timer;

        /// <summary>
        ///     Start disabled, if set to do so.
        /// </summary>
        private void Awake()
        {
            if (disableOnAwake)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     Handle updating the timer before the event is invoked.
        /// </summary>
        private void Update()
        {
            if (timer < delayTimer)
            {
                timer += Time.deltaTime;
                return;
            }

            // Invoke event and reset timer.
            onDelayedEvent.Invoke();
            ResetTimer();

            if (!runsContinuously)
            {
                // Disable after triggering once.
                enabled = false;
            }
        }

        /// <summary>
        ///     Reset the timer back to <c>0</c> upon enabling the event, if set to do so.
        /// </summary>
        private void OnEnable()
        {
            if (resetOnToggle)
            {
                ResetTimer();
            }
        }

        /// <summary>
        ///     Manually reset the timer back to <c>0</c>.
        /// </summary>
        public void ResetTimer()
        {
            timer = 0.0f;
        }
    }
}