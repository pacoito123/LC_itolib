using itolib.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.ScriptableObjects
{
    /// <summary>
    ///     Represents an arbitrary global event that can be called from any event callback.
    /// </summary>
    /// <remarks>Intended to be used along with a <c>ScriptableEventListener</c> component.</remarks>
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "itolib/Events/ScriptableEvent")]
    public sealed class ScriptableEvent : ScriptableObject
    {
        /// <summary>
        ///     Dictionary of all registered <c>ScriptableEvent</c> instances.
        /// </summary>
        internal static Dictionary<Guid, ScriptableEvent> AllEvents { get; } = [];

        /// <summary>
        ///     Source of the event (e.g. mod, moon, interior), to avoid conflicting event names.
        /// </summary>
        [field: Header("Scriptable Event")]
        [field: Tooltip("Source of the event (e.g. mod, moon, interior), to avoid conflicting event names.")]
        [field: SerializeField] public string EventSource { get; private set; } = string.Empty;

        /// <summary>
        ///     Name of the event. Object name will be used as fallback if the field is left blank.
        /// </summary>
        [field: Tooltip("Name of the event. Object name will be used as fallback if the field is left blank.")]
        [field: SerializeField]
        public string EventName
        {
            get
            {
                // Use object name as fallback, if field is empty.
                if (string.IsNullOrEmpty(field))
                {
                    field = name;
                }

                return field;
            }
        } = string.Empty;

        /// <summary>
        ///     Action to perform upon raising the event.
        /// </summary>
        private event Action? OnEventRaise;

        /// <summary>
        ///     <c>GUID</c> of this specific <c>ScriptableEvent</c>.
        /// </summary>
        private Guid guid = Guid.Empty;

        /// <summary>
        ///     Add event to the <c>AllEvents</c> dictionary, if not already present.
        /// </summary>
        private void Awake()
        {
            string key = EventSource + ':' + EventName;

            // Check if the GUID is already present in the dictionary.
            if (!key.TryComputeGUID(out guid) || !AllEvents.TryAdd(guid, this))
            {
                // TODO: Log message?
            }
        }

        /// <summary>
        ///     Raise the event and perform its added listeners.
        /// </summary>
        public void RaiseEvent()
        {
            if (guid != Guid.Empty && AllEvents.TryGetValue(guid, out ScriptableEvent scriptableEvent)
                && scriptableEvent != null)
            {
                scriptableEvent.OnEventRaise?.Invoke();
            }
        }

        /// <summary>
        ///     Add a listener to the event.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        public void AddListener(Action listener)
        {
            if (guid != Guid.Empty && AllEvents.TryGetValue(guid, out ScriptableEvent scriptableEvent)
                && scriptableEvent != null)
            {
                scriptableEvent.OnEventRaise += listener;
            }
        }

        /// <summary>
        ///     Remove a listener from the event.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        public void RemoveListener(Action listener)
        {
            if (guid != Guid.Empty && AllEvents.TryGetValue(guid, out ScriptableEvent scriptableEvent)
                && scriptableEvent != null)
            {
                scriptableEvent.OnEventRaise -= listener;
            }
        }

        /// <summary>
        ///     Clear all listeners from the event.
        /// </summary>
        public void ClearListeners()
        {
            if (guid != Guid.Empty && AllEvents.TryGetValue(guid, out ScriptableEvent scriptableEvent)
                && scriptableEvent != null)
            {
                scriptableEvent.OnEventRaise = null;
            }
        }
    }
}