using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct EnemyFilter
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string enemyName;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int amount;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool fuzzySearch;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool objectSearch;

        /// <summary>
        ///     TODO.
        /// </summary>
        public EnemyFilter()
        {
            enemyName = "";
            amount = 1;
            fuzzySearch = false;
            objectSearch = false;
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class EnemySensor : DetectRegion<EnemyAI>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Enemy Sensor")]
        [Tooltip("")]
        public List<EnemyFilter> enemyFilters = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<EnemyFilter>? onFilterAmountMet;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool filterExiting = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool subtractOnExit = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public int[] enemyAmounts = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            layerMask = 1 << LayerMask.NameToLayer("Enemies");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            enemyAmounts = new int[enemyFilters.Count];

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            base.CheckObjectsInRegion();

            int enemiesFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                    && enemyCollision.mainScript != null)
                {
                    onObjectsEach?.Invoke(enemyCollision.mainScript);
                    enemiesFound++;
                }
            }

            if (enemiesFound > 0)
            {
                onObjectsAny?.Invoke(enemiesFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                onRegionEntered?.Invoke(enemyCollision.mainScript);

                if (enemyFilters.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < enemyFilters.Count; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if ((filter.fuzzySearch && search.Contains(filter.enemyName)) || string.CompareOrdinal(search, filter.enemyName) == 0)
                    {
                        if (++enemyAmounts[i] >= filter.amount)
                        {
                            onFilterAmountMet?.Invoke(filter);
                            enemyAmounts[i] = 0;
                        }

                        onRegionEntered?.Invoke(enemyCollision.mainScript);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                if (enemyFilters.Count == 0 || !filterExiting)
                {
                    onRegionExited?.Invoke(enemyCollision.mainScript);
                    return;
                }

                for (int i = 0; i < enemyFilters.Count; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if ((filter.fuzzySearch && search.Contains(filter.enemyName)) || string.CompareOrdinal(search, filter.enemyName) == 0)
                    {
                        if (subtractOnExit)
                        {
                            enemyAmounts[i]--;
                        }

                        onRegionExited?.Invoke(enemyCollision.mainScript);
                        break;
                    }
                }
            }
        }
    }
}