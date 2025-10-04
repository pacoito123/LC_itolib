using itolib.Extensions;
using itolib.ScriptableObjects;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     Represents an event invoked whenever a specific <c>ScriptableEvent</c> is raised.
    /// </summary>
    public class ScriptableEventListener : MonoBehaviour
    {
        /// <summary>
        ///     Source of the <c>ScriptableEvent</c> (e.g. mod, moon, interior) to subscribe to.
        /// </summary>
        /// <remarks><b>NOTE:</b> Must match the <c>eventSource</c> field in the desired <c>ScriptableEvent</c>.</remarks>
        [Header("Scriptable Event Listener")]
        [Tooltip("Source of the ScriptableEvent (e.g. mod, moon, interior) to subscribe to. NOTE: Must match the 'eventSource' field in the "
            + "desired ScriptableEvent.")]
        [SerializeField] private string eventSource = string.Empty;

        /// <summary>
        ///     Name of the <c>ScriptableEvent</c> to subscribe to.
        /// </summary>
        /// <remarks><b>NOTE:</b> Must match the <c>eventName</c> field in the desired <c>ScriptableEvent</c>.</remarks>
        [Tooltip("Name of the ScriptableEvent to subscribe to. NOTE: Must match the 'eventName' field in the desired ScriptableEvent.")]
        [SerializeField] private string eventName = string.Empty;

        /// <summary>
        ///     Callback invoked when the targeted <c>ScriptableEvent</c> is raised.
        /// </summary>
        [Tooltip("Callback invoked when the targeted ScriptableEvent is raised.")]
        [SerializeField] private UnityEvent actions = new();

        /// <summary>
        ///     <c>ScriptableEvent</c> to target and subscribe to.
        /// </summary>
        /// <remarks>Deprecated. Should be left blank.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) ScriptableEvent to target and subscribe to. Should be left blank.")]
        [SerializeField] private ScriptableEvent? scriptableEvent;

        /// <summary>
        ///     <c>ScriptableEvent</c> to target and subscribe to.
        /// </summary>
        private ScriptableEvent? targetedEvent;

        /// <summary>
        ///     Obtain <c>ScriptableEvent</c> to target.
        /// </summary>
        private void Awake()
        {
            // Check if deprecated field has a value assigned.
            if (scriptableEvent != null)
            {
                eventSource = scriptableEvent.EventSource;
                eventName = scriptableEvent.EventName;
            }

            // Obtain event key to generate a GUID with.
            string key = $"{eventSource}:{eventName}";

            // Check if the specified event exists and can be targeted.
            if (!key.TryComputeGUID(out Guid eventGUID) || !ScriptableEvent.AllEvents.TryGetValue(eventGUID, out targetedEvent))
            {
                Plugin.StaticLogger.LogWarning($"Could not find event '{eventSource}:{eventName}' targeted by ScriptableEventListener component in '{name}'!");
            }
        }

        /// <summary>
        ///     Subscribe to the targeted <c>ScriptableEvent</c>.
        /// </summary>
        private void OnEnable()
        {
            if (targetedEvent != null)
            {
                targetedEvent.AddListener(InvokeEvent);
            }
        }

        /// <summary>
        ///     Unsubscribe from the targeted <c>ScriptableEvent</c>.
        /// </summary>
        private void OnDisable()
        {
            if (scriptableEvent != null)
            {
                scriptableEvent.RemoveListener(InvokeEvent);
            }
        }

        /// <summary>
        ///     Invoke event callback.
        /// </summary>
        private void InvokeEvent()
        {
            actions.Invoke();
        }
    }
}