using itolib.Extensions;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct EventEntry()
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Event Entry")]
        [Tooltip("")]
        public string eventName = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onEventCalled = new();
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class MultiAnimationEvent : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Multi Animation Event")]
        [Tooltip("")]
        [SerializeField] private EventEntry[]? eventEntries;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="eventIndex"></param>
        public void CallEvent(int eventIndex)
        {
            if (eventIndex < eventEntries?.Length)
            {
                eventEntries[eventIndex].onEventCalled.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="eventName"></param>
        public void CallEvent(string eventName)
        {
            for (int i = 0; i < eventEntries?.Length; i++)
            {
                if (eventEntries[i].eventName.CompareOrdinal(eventName))
                {
                    CallEvent(i);

                    return;
                }
            }
        }
    }
}