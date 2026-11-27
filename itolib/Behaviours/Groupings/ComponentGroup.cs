using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     Represents a group of components of type <c><typeparamref name="T"/></c> to perform bulk operations on.
    /// </summary>
    /// <typeparam name="T">Type of components within this group.</typeparam>
    public abstract class ComponentGroup<T> : MonoBehaviour, IActivationScript where T : Component
    {
        /// <inheritdoc/>
        public IActivationScript ActivationSelf { get; }

        /// <inheritdoc/>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     All unique components of type <c><typeparamref name="T"/></c> found by the component search.
        /// </summary>
        private readonly HashSet<T?> components = [];

        /// <summary>
        ///     List of components of type <c><typeparamref name="T"/></c> obtained from the last searched object.
        /// </summary>
        private readonly List<T> tempComponents = [];

        /// <summary>
        ///     List of objects to search within for components of type <c><typeparamref name="T"/></c>.
        /// </summary>
        [Header("Component Group")]
        [Tooltip("List of objects to search within for components of the defined type.")]
        [SerializeField] private List<GameObject?>? objectsToSearch;

        /// <summary>
        ///     List of object tags to ignore when searching for components of type <c><typeparamref name="T"/></c>.
        /// </summary>
        [Tooltip("List of object tags to ignore when searching for components of the defined type.")]
        [SerializeField] private string[]? tagsToIgnore;

        /// <summary>
        ///     Whether to search recursively for components of type <c><typeparamref name="T"/></c> or not.
        /// </summary>
        [Tooltip("Whether to search recursively for components of the defined type or not.")]
        [SerializeField] private bool recursively = true;

        /// <inheritdoc/>
        [field: Tooltip($"Desired activation time for the component search.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Immediate;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        protected ComponentGroup()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     Perform component search immediately or subscribe to a specific event upon being enabled, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void OnEnable()
        {
            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Unsubscribe from any events that may have been subscribed to.
        /// </summary>
        private void OnDisable()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     Perform search for components of type <c><typeparamref name="T"/></c> within the specified objects.
        /// </summary>
        /// <param name="recursively">Whether to search for components of type <c><typeparamref name="T"/></c> recursively or not.</param>
        public virtual void FindComponentsInObjects(bool recursively)
        {
            // Check if there are no objects to search.
            if (objectsToSearch == null || objectsToSearch.Count == 0)
            {
                return;
            }

            // Clear components in the group.
            components.Clear();

            // Search every object for components of the defined type.
            foreach (GameObject? gameObject in objectsToSearch)
            {
                if (gameObject == null)
                {
                    continue;
                }

                if (recursively)
                {
                    // Obtain all components of the defined type within the object's hierarchy.
                    gameObject.GetComponentsInChildren(includeInactive: true, tempComponents);
                }
                else
                {
                    // Obtain all components of the defined type at the root of the object.
                    gameObject.GetComponents(tempComponents);
                }

                // Add found components to group.
                components.UnionWith(tempComponents);
            }

            // Remove any components with ignored tags from the group.
            for (int i = 0; i < tagsToIgnore?.Length; i++)
            {
                _ = components.RemoveWhere(component => component == null || component.CompareTag(tagsToIgnore[i]));
            }
        }

        /// <summary>
        ///     Perform bulk action for all components of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="actionID">ID of the action to perform.</param>
        /// <param name="parameter">Optional parameter given to the action to perform.</param>
        protected virtual void PerformGroupAction(Enum actionID, object? parameter = null)
        {
            // Perform action for every found component of the defined type in the group.
            foreach (T? component in components)
            {
                if (component != null)
                {
                    PerformSingleAction(component, actionID, parameter);
                }
            }
        }

        /// <summary>
        ///     Perform action for a single component of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="component">Single component of type <c><typeparamref name="T"/></c> in the group.</param>
        /// <param name="actionID">ID of the action to perform.</param>
        /// <param name="parameter">Optional parameter given to the action to perform.</param>
        protected abstract void PerformSingleAction(T component, Enum actionID, object? parameter = null);

        /// <summary>
        ///     Enable or disable all components of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable.</param>
        public void EnableComponents(bool enabled)
        {
            EnableGroupComponents(enabled);
        }

        /// <summary>
        ///     Enable or disable all components of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable.</param>
        protected virtual void EnableGroupComponents(bool enabled)
        {
            // Enable or disable every found component of the defined type in the group.
            foreach (T? component in components)
            {
                if (component != null)
                {
                    EnableSingleComponent(component, enabled);
                }
            }
        }

        /// <summary>
        ///     Enable a single component of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="component">Single component of type <c><typeparamref name="T"/></c> in the group.</param>
        /// <param name="enabled">Whether to enable or disable.</param>
        protected abstract void EnableSingleComponent(T component, bool enabled);

        /// <summary>
        ///     Toggle all components of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        public void ToggleComponents()
        {
            ToggleGroupComponents();
        }

        /// <summary>
        ///     Toggle all components of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        protected virtual void ToggleGroupComponents()
        {
            // Toggle every found component of the defined type in the group.
            foreach (T? component in components)
            {
                if (component != null)
                {
                    ToggleSingleComponent(component);
                }
            }
        }

        /// <summary>
        ///     Toggle a single component of type <c><typeparamref name="T"/></c> in the group.
        /// </summary>
        /// <param name="component"></param>
        protected abstract void ToggleSingleComponent(T component);

        /// <summary>
        ///     Manually add an object to be searched for components of type <c><typeparamref name="T"/></c>. 
        /// </summary>
        /// <remarks>Does not automatically call <c>FindComponentsInObjects()</c> again, it must be done manually.</remarks>
        /// <param name="gameObject">Object to add to the search for components of type <c><typeparamref name="T"/></c>.</param>
        public virtual void AddObjectToSearch(GameObject gameObject)
        {
            // Initialize list of objects to search, if empty.
            objectsToSearch ??= [];

            if (gameObject != null && !objectsToSearch.Contains(gameObject))
            {
                // Add object to the list of objects to search, if not already present.
                objectsToSearch.Add(gameObject);
            }
        }

        /// <summary>
        ///     Manually add a single component of type <c><typeparamref name="T"/></c> to the group.
        /// </summary>
        /// <param name="component"></param>
        public virtual void AddComponentToGroup(T component)
        {
            if (component != null)
            {
                // Add component to the group, if not already present.
                _ = components.Add(component);
            }
        }

        /// <inheritdoc/>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            FindComponentsInObjects(recursively);
        }

        /// <inheritdoc/>
        public void ResetActivation(bool instant)
        {
            ActivationSelf.Reinitialize(instant);
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