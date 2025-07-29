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
        [SerializeField] private ScriptableEvent? scriptableEvent;

        /// <summary>
        ///     TODO.
        /// </summary>
        [SerializeField] private UnityEvent actions = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (scriptableEvent != null)
            {
                scriptableEvent.AddListener(InvokeEvent);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDisable()
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