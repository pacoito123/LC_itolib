using System;
using UnityEngine;

namespace itolib.ScriptableObjects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "itolib/Events/ScriptableEvent")]
    public class ScriptableEvent : ScriptableObject
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private event Action? OnEventRaise;

        /// <summary>
        /// 
        /// </summary>
        public void RaiseEvent()
        {
            OnEventRaise?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AddListener(Action listener)
        {
            OnEventRaise += listener;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void RemoveListener(Action listener)
        {
            OnEventRaise -= listener;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ClearListeners()
        {
            OnEventRaise = null;
        }
    }
}