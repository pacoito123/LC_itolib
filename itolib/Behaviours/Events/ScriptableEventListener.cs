using itolib.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ScriptableEventListener : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public ScriptableEvent? scriptableEvent;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent actions = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (scriptableEvent != null)
            {
                scriptableEvent.AddListener(InvokeEvent);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDisable()
        {
            if (scriptableEvent != null)
            {
                scriptableEvent.RemoveListener(InvokeEvent);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void InvokeEvent()
        {
            actions.Invoke();
        }
    }
}