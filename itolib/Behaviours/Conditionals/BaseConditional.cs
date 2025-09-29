using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     Represents an override to perform based on a specified string.
    /// </summary>
    [Serializable]
    public struct ConditionalOverride
    {
        /// <summary>
        ///     Name to be matched in order to apply this override.
        /// </summary>
        [Header("Conditional Override")]
        [Tooltip("Name to be matched in order to apply this override.")]
        public string nameToSearch = string.Empty;

        /// <summary>
        ///     Additional names to apply this override for.
        /// </summary>
        [Tooltip("Additional names to apply this override for.")]
        public string[]? alsoAppliesTo = null;

        /// <summary>
        ///     Objects to enable when applying this override.
        /// </summary>
        /// <remarks><b>NOTE:</b> Objects become disabled when undoing the override.</remarks>
        [Tooltip("Objects to enable when applying this override. NOTE: Objects become disabled when undoing the override.")]
        public GameObject?[]? objectsToEnable = null;

        /// <summary>
        ///     Objects to disable when applying this override.
        /// </summary>
        /// <remarks><b>NOTE:</b> Objects become enabled when undoing the override.</remarks>
        [Tooltip("Objects to disable when applying this override. NOTE: Objects become enabled when undoing the override.")]
        public GameObject?[]? objectsToDisable = null;

        /// <summary>
        ///     Callback invoked when applying this override.
        /// </summary>
        [Tooltip("Callback invoked when applying this override.")]
        public UnityEvent onConditionalApply = new();

        /// <summary>
        ///     Callback invoked when undoing this override.
        /// </summary>
        [Tooltip("Callback invoked when undoing this override.")]
        public UnityEvent onConditionalUndo = new();

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public ConditionalOverride() { }

        /// <summary>
        ///     Apply (or undo) this conditional override.
        /// </summary>
        /// <param name="undo">Whether the conditional override should be undone or not.</param>
        public readonly void Apply(bool undo = false)
        {
            // Check if there are any objects set to be enabled.
            if (objectsToEnable != null)
            {
                for (int i = 0; i < objectsToEnable.Length; i++)
                {
                    GameObject? objectToEnable = objectsToEnable[i];

                    if (objectToEnable != null)
                    {
                        objectToEnable.SetActive(!undo);
                    }
                }
            }

            // Check if there are any objects set to be disabled.
            if (objectsToDisable != null)
            {
                for (int i = 0; i < objectsToDisable.Length; i++)
                {
                    GameObject? objectToDisable = objectsToDisable[i];

                    if (objectToDisable != null)
                    {
                        objectToDisable.SetActive(undo);
                    }
                }
            }

            // Check if override is being undone.
            if (!undo)
            {
                // Invoke apply event.
                onConditionalApply.Invoke();
            }
            else
            {
                // Invoke undo event.
                onConditionalUndo.Invoke();
            }
        }
    }

    /// <summary>
    ///     Represents a generic set of actions to perform (overrides) based on a condition.
    /// </summary>
    /// <typeparam name="T">Conditional type to perform the search for.</typeparam>
    public abstract class BaseConditional<T> : MonoBehaviour, IActivationScript
    {
        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>IActivationScript</c>, to avoid having to cast. 
        /// </summary>
        public IActivationScript ActivationSelf { get; set; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     List of <c>ConditionalOverrides</c> to perform.
        /// </summary>
        [Header("Base Conditional")]
        [Tooltip("List of conditional overrides to perform.")]
        [SerializeField] protected ConditionalOverride[]? conditionalOverrides;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the overrides to be applied.
        /// </summary>
        [field: Tooltip("Desired activation time for the overrides to be applied.")]
        [field: FormerlySerializedAs("activationTime")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the overrides to be applied.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the overrides to be applied. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        protected virtual void Awake()
        {
            ActivationSelf = this;

            if (activationTime is not ActivationTime.StartOfRound)
            {
                ActivationTime = activationTime;
            }
        }

        /// <summary>
        ///     Subscribe to events for automatic activation.
        /// </summary>
        protected virtual void Start()
        {
            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Unsubscribe from events for automatic activation.
        /// </summary>
        protected virtual void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     Apply conditional overrides.
        /// </summary>
        public void ApplyConditional()
        {
            ApplyConditional(undo: false);
        }

        /// <summary>
        ///    Apply (or undo) conditional overrides.
        /// </summary>
        /// <param name="undo">Whether the conditional override should be undone or not.</param>
        public abstract void ApplyConditional(bool undo);

        /// <summary>
        ///     Apply conditional overrides, with a given object to check.
        /// </summary>
        /// <param name="objectToCheck">Object of type <typeparamref name="T"/> to perform the conditional check with.</param>
        public void ApplyConditional(T objectToCheck)
        {
            ApplyConditional(objectToCheck, undo: false);
        }

        /// <summary>
        ///     Apply (or undo) conditional overrides, with a given object to check.
        /// </summary>
        /// <param name="objectToCheck">Object of type <typeparamref name="T"/> to perform the conditional check with.</param>
        /// <param name="undo">Whether the conditional override should be undone or not.</param>
        public abstract void ApplyConditional(T objectToCheck, bool undo);

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            ApplyConditional();
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