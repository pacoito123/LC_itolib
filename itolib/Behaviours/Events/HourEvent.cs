using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class HourEvent : MonoBehaviour
    {
        /// <summary>
        ///     100 =  7:40 AM (Day starts)
        ///     120 =  8:00 AM
        ///     360 = 12:00 PM
        ///    1020 = 11:00 PM
        ///    1080 = 12:00 AM (Day ends)
        /// </summary>
        [Header("Hour Event")]
        [Tooltip("")]
        [Range(100, 1080)]
        [SerializeField] private int playAtTime = 100;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onHourEvent = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool hasRun;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            CheckHour();

            if (!hasRun && TimeOfDay.Instance != null)
            {
                TimeOfDay.Instance.onHourChanged.AddListener(CheckHour);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDisable()
        {
            if (TimeOfDay.Instance != null)
            {
                TimeOfDay.Instance.onHourChanged.RemoveListener(CheckHour);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void CheckHour()
        {
            if (!hasRun && TimeOfDay.Instance != null && TimeOfDay.Instance.globalTime >= playAtTime)
            {
                onHourEvent.Invoke();
                hasRun = true;

                enabled = false;
            }
        }
    }
}