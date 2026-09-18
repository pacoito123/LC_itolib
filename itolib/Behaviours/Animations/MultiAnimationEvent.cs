using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     Represents a single event to be called, with a given name for identification purposes.
    /// </summary>
    [Serializable]
    public struct EventEntry()
    {
        /// <summary>
        ///     Name of this event entry, for identification purposes.
        /// </summary>
        [Header("Event Entry")]
        [Tooltip("Name of this event entry, for identification purposes.")]
        public string eventName = string.Empty;

        /// <summary>
        ///     Event callback to be invoked.
        /// </summary>
        [Tooltip("Event callback to be invoked.")]
        public UnityEvent onEventCalled = new();
    }

    /// <summary>
    ///     Represents one or several events invoked from an <c>AnimationEvent</c> by an <c>Animator</c>.
    /// </summary>
    public class MultiAnimationEvent : MonoBehaviour
    {
        /// <summary>
        ///     List of event entries that can be invoked.
        /// </summary>
        [Header("Multi Animation Event")]
        [Tooltip("List of event entries that can be invoked.")]
        [SerializeField] private EventEntry[]? eventEntries;

        /// <summary>
        ///     Call an event entry at the given index.
        /// </summary>
        /// <param name="eventIndex">Index of the entry to invoke.</param>
        public void CallEvent(int eventIndex)
        {
            if (eventIndex < eventEntries?.Length)
            {
                eventEntries[eventIndex].onEventCalled.Invoke();
            }
        }

        /// <summary>
        ///     Call an event entry with the given name.
        /// </summary>
        /// <param name="eventName">Name of the entry to invoke.</param>
        public void CallEvent(string eventName)
        {
            for (int i = 0; i < eventEntries?.Length; i++)
            {
                if (string.Equals(eventEntries[i].eventName, eventName, StringComparison.OrdinalIgnoreCase))
                {
                    CallEvent(i);

                    return;
                }
            }
        }
    }
}