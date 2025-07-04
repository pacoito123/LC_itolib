using itolib.Extensions;
using System;
using System.Collections.Generic;
using Unity.Netcode;
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
    ///     Represents a <c>DetectRegion</c> specifically for <c>EnemyAI</c> objects, with some additional stuff.
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
        public UnityEvent<EnemyFilter> onFilterAmountMet = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool filterIsBlacklist = false;

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
            if (!IsHost)
            {
                if (regionCollider != null)
                {
                    regionCollider.enabled = false;
                }

                return;
            }

            enemyAmounts = new int[enemyFilters.Count];

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
                    if (enemyFilters.Count == 0)
                    {
                        FoundEnemiesEachClientRpc(enemyCollision.mainScript);
                        enemiesFound++;
                        continue;
                    }

                    bool blacklistedEnemy = false;
                    for (int j = 0; j < enemyFilters.Count; j++)
                    {
                        EnemyFilter filter = enemyFilters[j];
                        string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                        if ((filter.fuzzySearch && search.Contains(filter.enemyName, StringComparison.OrdinalIgnoreCase))
                            || search.CompareOrdinal(filter.enemyName))
                        {
                            if (filterIsBlacklist)
                            {
                                blacklistedEnemy = true;
                                break;
                            }

                            if (++enemyAmounts[j] >= filter.amount)
                            {
                                onFilterAmountMet.Invoke(filter);
                                enemyAmounts[j] = 0;
                            }

                            FoundEnemiesEachClientRpc(enemyCollision.mainScript);
                            enemiesFound++;
                            break;
                        }
                    }

                    if (filterIsBlacklist && !blacklistedEnemy)
                    {
                        FoundEnemiesEachClientRpc(enemyCollision.mainScript);
                        enemiesFound++;
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
                if (enemyFilters.Count == 0)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript);
                    return;
                }

                for (int i = 0; i < enemyFilters.Count; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if ((filter.fuzzySearch && search.Contains(filter.enemyName, StringComparison.OrdinalIgnoreCase))
                        || search.CompareOrdinal(filter.enemyName))
                    {
                        if (filterIsBlacklist)
                        {
                            return;
                        }

                        if (++enemyAmounts[i] >= filter.amount)
                        {
                            onFilterAmountMet.Invoke(filter);
                            enemyAmounts[i] = 0;
                        }

                        RegionEnteredClientRpc(enemyCollision.mainScript);
                        break;
                    }
                }

                if (filterIsBlacklist)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript);
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
                if (enemyFilters.Count == 0 || !filterExiting)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript, exit: true);
                    return;
                }

                for (int i = 0; i < enemyFilters.Count; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if ((filter.fuzzySearch && search.Contains(filter.enemyName)) || search.CompareOrdinal(filter.enemyName))
                    {
                        if (subtractOnExit)
                        {
                            enemyAmounts[i]--;
                        }

                        RegionEnteredClientRpc(enemyCollision.mainScript, exit: true);
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
    }
}