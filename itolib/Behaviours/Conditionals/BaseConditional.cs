using DunGen;
using itolib.Enums;
using LethalLevelLoader;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct ConditionalOverride
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Conditional Override")]
        [Tooltip("")]
        public string nameToSearch = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string[]? alsoAppliesTo = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public GameObject?[]? objectsToEnable = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public GameObject?[]? objectsToDisable = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onConditionalApply = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onConditionalUndo = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public ConditionalOverride() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public readonly void Apply(bool undo = false)
        {
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

            if (!undo)
            {
                onConditionalApply.Invoke();
            }
            else
            {
                onConditionalUndo.Invoke();
            }
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseConditional<T> : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Base Conditional")]
        [Tooltip("")]
        [SerializeField] protected ConditionalOverride[]? conditionalOverrides;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Start()
        {
            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    ApplyConditional();
                    break;
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(ApplyConditional);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(ApplyConditional);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(ApplyConditional);
                    }
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void OnDestroy()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(ApplyConditional);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(ApplyConditional);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(ApplyConditional);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ApplyConditional()
        {
            ApplyConditional(undo: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public abstract void ApplyConditional(bool undo);

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        public void ApplyConditional(T objectToCheck)
        {
            ApplyConditional(objectToCheck, undo: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public abstract void ApplyConditional(T objectToCheck, bool undo);

        /// <summary>
        ///     <c>DunGen</c> listener called when generation finishes, but before blockers and connectors are placed.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                ApplyConditional();
            }
        }
    }
}