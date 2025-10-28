using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComponentGroup<T> : NetworkBehaviour, IActivationScript where T : Component
    {
        /// <summary>
        ///     Cached instance of the current <c>ComponentGroup</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        private T[]? components;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Component Group")]
        [Tooltip("")]
        [SerializeField] private GameObject[]? objectsToSearch;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the <c><typeparamref name="T"/></c> search.
        /// </summary>
        [field: Tooltip($"Desired activation time for the {nameof(T)} search.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.DungeonComplete;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        protected ComponentGroup()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Awake()
        {
            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void FindComponentsInObjects()
        {
            for (int i = 0; i < objectsToSearch?.Length; i++)
            {
                if (objectsToSearch[i] != null)
                {
                    components = components?.Length > 0
                        ? [.. components, .. objectsToSearch[i].GetComponentsInChildren<T>()]
                        : [.. objectsToSearch[i].GetComponentsInChildren<T>()];
                }
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            FindComponentsInObjects();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="action"></param>
        protected virtual void PerformGroupAction(Action<T> action)
        {
            for (int i = 0; i < components?.Length; i++)
            {
                if (components[i] != null)
                {
                    action.Invoke(components[i]);
                }
            }
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}