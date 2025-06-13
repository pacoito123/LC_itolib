using itolib.Enums;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
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
        public string nameToSearch;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<string> alsoAppliesTo;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<GameObject?> objectsToEnable;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<GameObject?> objectsToDisable;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent onConditionalApply;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent onConditionalUndo;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public readonly void Apply(bool undo = false)
        {
            objectsToEnable?.ForEach(weatherEffect =>
            {
                if (weatherEffect != null)
                {
                    weatherEffect.SetActive(!undo);
                }
            });
            objectsToDisable?.ForEach(weatherEffect =>
            {
                if (weatherEffect != null)
                {
                    weatherEffect.SetActive(undo);
                }
            });

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
    public abstract class BaseConditional<T> : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Base Conditional")]
        [Tooltip("")]
        public List<ConditionalOverride> conditionalOverrides = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void Start()
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
        public virtual void OnDestroy()
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
        public virtual void ApplyConditional()
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
        public virtual void ApplyConditional(T objectToCheck)
        {
            ApplyConditional(objectToCheck, undo: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public abstract void ApplyConditional(T objectToCheck, bool undo);
    }
}