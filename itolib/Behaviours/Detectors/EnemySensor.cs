using itolib.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        [Header("Enemy Filter")]
        [Tooltip("")]
        public string enemyName = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string[]? alsoAppliesTo = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(1)]
        [FormerlySerializedAs("amount")]
        public int amountRequired = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onReachedAmount = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onBelowAmount = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onAboveAmount = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool fuzzySearch = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool objectSearch = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool triggerOnce = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool subtractOnExit = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool isBlacklist = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public EnemyFilter() { }
    }

    /// <summary>
    ///     Represents a <c>DetectRegion</c> specifically for <c>EnemyAI</c> objects, with some additional stuff for enemy filtering.
    /// </summary>
    public class EnemySensor : DetectRegion<EnemyAI>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Enemy Sensor")]
        [Tooltip("")]
        [SerializeField] private EnemyFilter[]? enemyFilters;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int[] enemyAmounts = null!;

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
            if (!IsHost)
            {
                if (regionCollider != null)
                {
                    regionCollider.enabled = false;
                }

                return;
            }

            if (enemyFilters != null && enemyFilters.Length > 0)
            {
                enemyAmounts = new int[enemyFilters.Length];
            }

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            int enemiesFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                    && enemyCollision.mainScript != null)
                {
                    if (enemyFilters == null || enemyFilters.Length == 0)
                    {
                        FoundEnemiesEachClientRpc(enemyCollision.mainScript);
                        enemiesFound++;

                        continue;
                    }

                    for (int j = 0; j < enemyFilters.Length; j++)
                    {
                        EnemyFilter filter = enemyFilters[j];
                        string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                        if (CheckFilter(filter, search, ref enemyAmounts[j], out bool matchedBlacklist))
                        {
                            FoundEnemiesEachClientRpc(enemyCollision.mainScript);
                            enemiesFound++;
                        }

                        if (matchedBlacklist)
                        {
                            break;
                        }
                    }
                }
            }

            if (enemiesFound > 0)
            {
                FoundEnemiesAnyClientRpc(enemiesFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (!IsHost)
            {
                return;
            }

            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                if (enemyFilters == null || enemyFilters.Length == 0)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript);

                    return;
                }

                for (int i = 0; i < enemyFilters.Length; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if (CheckFilter(filter, search, ref enemyAmounts[i], out bool matchedBlacklist))
                    {
                        RegionEnteredClientRpc(enemyCollision.mainScript);

                        break;
                    }

                    if (matchedBlacklist)
                    {
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
            if (!IsHost)
            {
                return;
            }

            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                if (enemyFilters == null || enemyFilters.Length == 0)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript, exit: true);

                    return;
                }

                for (int i = 0; i < enemyFilters.Length; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if (CheckFilter(filter, search, ref enemyAmounts[i], out bool matchedBlacklist, subtract: filter.subtractOnExit))
                    {
                        RegionEnteredClientRpc(enemyCollision.mainScript, exit: true);

                        break;
                    }

                    if (matchedBlacklist)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyReference"></param>
        [ClientRpc]
        public void FoundEnemiesEachClientRpc(NetworkBehaviourReference enemyReference)
        {
            if (enemyReference.TryGet(out EnemyAI enemy))
            {
                onObjectsEach.Invoke(enemy);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemiesFound"></param>
        [ClientRpc]
        public void FoundEnemiesAnyClientRpc(int enemiesFound)
        {
            onObjectsAny.Invoke(enemiesFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkBehaviourReference enemyReference, bool exit = false)
        {
            if (enemyReference.TryGet(out EnemyAI enemy))
            {
                if (!exit)
                {
                    onRegionEntered.Invoke(enemy);
                }
                else
                {
                    onRegionExited.Invoke(enemy);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="enemyName"></param>
        /// <param name="enemyAmount"></param>
        /// <param name="matchedBlacklist"></param>
        /// <param name="subtract"></param>
        /// <returns></returns>
        private static bool CheckFilter(EnemyFilter filter, string enemyName, ref int enemyAmount, out bool matchedBlacklist, bool subtract = false)
        {
            matchedBlacklist = false;

            if (enemyAmount < 0)
            {
                return false;
            }

            if (compareNames(filter.enemyName, ref enemyAmount, out matchedBlacklist))
            {
                return true;
            }
            else if (matchedBlacklist)
            {
                return false;
            }

            if (filter.alsoAppliesTo?.Length > 0)
            {
                for (int i = 0; i < filter.alsoAppliesTo.Length; i++)
                {
                    if (compareNames(filter.alsoAppliesTo[i], ref enemyAmount, out matchedBlacklist))
                    {
                        return true;
                    }
                    else if (matchedBlacklist)
                    {
                        return false;
                    }
                }
            }

            bool compareNames(string filterName, ref int enemyAmount, out bool matchedBlacklist)
            {
                matchedBlacklist = false;

                if (filterName.Length == 0 || (filter.fuzzySearch && enemyName.Contains(filterName, StringComparison.OrdinalIgnoreCase))
                    || enemyName.CompareOrdinal(filterName) || filterName.CompareOrdinal("Any"))
                {
                    if (filter.isBlacklist)
                    {
                        matchedBlacklist = true;

                        return false;
                    }

                    if (!subtract)
                    {
                        enemyAmount++;
                    }
                    else
                    {
                        enemyAmount--;
                    }

                    if (enemyAmount < filter.amountRequired)
                    {
                        filter.onBelowAmount.Invoke(enemyAmount);
                    }

                    if (enemyAmount == filter.amountRequired)
                    {
                        filter.onReachedAmount.Invoke(enemyAmount);

                        if (filter.triggerOnce)
                        {
                            enemyAmount = -1;
                        }
                    }

                    if (enemyAmount > filter.amountRequired)
                    {
                        filter.onAboveAmount.Invoke(enemyAmount);

                        if (filter.triggerOnce)
                        {
                            enemyAmount = -1;
                        }
                    }

                    return true;
                }

                return false;
            }

            return false;
        }
    }
}