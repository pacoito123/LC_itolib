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
                    FoundEnemiesEachClientRpc(enemyCollision.mainScript.thisNetworkObject);
                    enemiesFound++;
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
                RegionEnteredClientRpc(enemyCollision.mainScript.thisNetworkObject);

                if (enemyFilters.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < enemyFilters.Count; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if ((filter.fuzzySearch && search.Contains(filter.enemyName, StringComparison.OrdinalIgnoreCase))
                        || string.CompareOrdinal(search, filter.enemyName) == 0)
                    {
                        if (++enemyAmounts[i] >= filter.amount)
                        {
                            onFilterAmountMet?.Invoke(filter);
                            enemyAmounts[i] = 0;
                        }

                        RegionEnteredClientRpc(enemyCollision.mainScript.thisNetworkObject);
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
                if (enemyFilters.Count == 0 || !filterExiting)
                {
                    RegionEnteredClientRpc(enemyCollision.mainScript.thisNetworkObject, exit: true);
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

                        RegionEnteredClientRpc(enemyCollision.mainScript.thisNetworkObject, exit: true);
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
        public void FoundEnemiesEachClientRpc(NetworkObjectReference enemyReference)
        {
            if (enemyReference.TryGet(out NetworkObject enemyNetworkObject)
                && enemyNetworkObject.TryGetComponent(out EnemyAI enemy))
            {
                onObjectsEach?.Invoke(enemy);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemiesFound"></param>
        [ClientRpc]
        public void FoundEnemiesAnyClientRpc(int enemiesFound)
        {
            onObjectsAny?.Invoke(enemiesFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkObjectReference enemyReference, bool exit = false)
        {
            if (enemyReference.TryGet(out NetworkObject enemyNetworkObject)
                && enemyNetworkObject.TryGetComponent(out EnemyAI enemy))
            {
                if (!exit)
                {
                    onRegionEntered?.Invoke(enemy);
                }
                else
                {
                    onRegionExited?.Invoke(enemy);
                }
            }
        }
    }
}