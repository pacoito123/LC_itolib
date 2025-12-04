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
                if (field.IsNullOrEmpty())
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
        ///     Add event to the <c>AllEvents</c> dictionary, if not already present.
        /// </summary>
        private void Awake()
        {
            string key = EventSource + ':' + EventName;

            // Check if the GUID is already present in the dictionary.
            if (!key.TryComputeGUID(out Guid guid) || !AllEvents.TryAdd(guid, this))
            {
#if !UNITY_EDITOR
                // This needs not exist if it's a duplicate GUID.
                Destroy(this);
#endif
            }
        }

        /// <summary>
        ///     Raise the event and perform its added listeners.
        /// </summary>
        public void RaiseEvent()
        {
            OnEventRaise?.Invoke();
        }

        /// <summary>
        ///     Add a listener to the event.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        public void AddListener(Action listener)
        {
            OnEventRaise += listener;
        }

        /// <summary>
        ///     Remove a listener from the event.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        public void RemoveListener(Action listener)
        {
            OnEventRaise -= listener;
        }

        /// <summary>
        ///     Clear all listeners from the event.
        /// </summary>
        public void ClearListeners()
        {
            OnEventRaise = null;
        }
    }
}