using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComponentGroup<T> : MonoBehaviour, IActivationScript where T : Component
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
        private T?[]? components;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Component Group")]
        [Tooltip("")]
        [SerializeField] private GameObject[]? objectsToSearch;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string[]? tagsToIgnore;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the <c><typeparamref name="T"/></c> search.
        /// </summary>
        [field: Tooltip($"Desired activation time for the {nameof(T)} search.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Immediate;

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
        private void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void FindComponentsInObjects()
        {
            if (PerformedActivation)
            {
                return;
            }

            HashSet<T> uniqueComponents = [];

            for (int i = 0; i < objectsToSearch?.Length; i++)
            {
                if (objectsToSearch[i] != null)
                {
                    uniqueComponents.UnionWith(objectsToSearch[i].GetComponentsInChildren<T>());
                }
            }

            for (int i = 0; i < tagsToIgnore?.Length; i++)
            {
                _ = uniqueComponents.RemoveWhere(component => component.CompareTag(tagsToIgnore[i]));
            }

            components = [.. uniqueComponents];
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
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
        protected void PerformGroupAction(Enum actionID, object? parameter = null)
        {
            for (int i = 0; i < components?.Length; i++)
            {
                T? component = components[i];

                if (component != null)
                {
                    PerformSingleAction(component, actionID, parameter);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
        protected abstract void PerformSingleAction(T component, Enum actionID, object? parameter = null);

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableComponents(bool enabled)
        {
            EnableGroupComponents(enabled);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enabled"></param>
        protected void EnableGroupComponents(bool enabled)
        {
            for (int i = 0; i < components?.Length; i++)
            {
                T? component = components[i];

                if (component != null)
                {
                    EnableSingleComponent(component, enabled);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="enabled"></param>
        protected abstract void EnableSingleComponent(T component, bool enabled);

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ToggleComponents()
        {
            for (int i = 0; i < components?.Length; i++)
            {
                T? component = components[i];

                if (component != null)
                {
                    ToggleSingleComponent(component);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected void ToggleGroupComponents()
        {
            for (int i = 0; i < components?.Length; i++)
            {
                T? component = components[i];

                if (component != null)
                {
                    ToggleSingleComponent(component);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="component"></param>
        protected abstract void ToggleSingleComponent(T component);

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