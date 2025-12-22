using itolib.Extensions;
using itolib.Util;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
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
        protected override void Reset()
        {
            layerMask = LayerMask.NameToLayer("Enemies");

            /* if (TryGetComponent(out Collider collider))
            {
                collider.isTrigger = true;
                collider.layerOverridePriority = 100;
                collider.excludeLayers = (collider.excludeLayers == 0) ? ~layerMask : (collider.excludeLayers & ~layerMask);
            } */
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            /* if (!NetworkManager.Singleton.IsHost && regionCollider != null)
            {
                regionCollider.enabled = false;
            } */

            if (NetworkManager.Singleton.IsHost && enemyFilters != null && enemyFilters.Length > 0)
            {
                enemyAmounts = new int[enemyFilters.Length];
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            int enemiesFound = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? enemyCollider = overlapBuffer?[i];

                if (enemyCollider == null || !enemyCollider.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                if (enemyCollider.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                    && enemyCollision.mainScript != null)
                {
                    if (enemyFilters == null || enemyFilters.Length == 0)
                    {
                        FoundEnemiesEachRpc(enemyCollision.mainScript);
                        enemiesFound++;

                        continue;
                    }

                    for (int j = 0; j < enemyFilters.Length; j++)
                    {
                        EnemyFilter filter = enemyFilters[j];
                        string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                        if (CheckFilter(filter, search, ref enemyAmounts[j], out bool matchedBlacklist))
                        {
                            FoundEnemiesEachRpc(enemyCollision.mainScript);
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
                FoundEnemiesAnyRpc(enemiesFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                if (enemyFilters == null || enemyFilters.Length == 0)
                {
                    RegionEnteredRpc(enemyCollision.mainScript);

                    return;
                }

                for (int i = 0; i < enemyFilters.Length; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if (CheckFilter(filter, search, ref enemyAmounts[i], out bool matchedBlacklist))
                    {
                        RegionEnteredRpc(enemyCollision.mainScript);

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
        protected override void OnTriggerExit(Collider other)
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            if (other.TryGetComponent(out EnemyAICollisionDetect enemyCollision)
                && enemyCollision.mainScript != null)
            {
                if (enemyFilters == null || enemyFilters.Length == 0)
                {
                    RegionEnteredRpc(enemyCollision.mainScript, exit: true);

                    return;
                }

                for (int i = 0; i < enemyFilters.Length; i++)
                {
                    EnemyFilter filter = enemyFilters[i];
                    string search = !filter.objectSearch ? enemyCollision.mainScript.enemyType.enemyName : enemyCollision.mainScript.gameObject.name;

                    if (CheckFilter(filter, search, ref enemyAmounts[i], out bool matchedBlacklist, subtract: filter.subtractOnExit))
                    {
                        RegionEnteredRpc(enemyCollision.mainScript, exit: true);

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
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundEnemiesEachRpc(NetworkBehaviourReference enemyReference)
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
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundEnemiesAnyRpc(int enemiesFound)
        {
            onObjectsAny.Invoke(enemiesFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyReference"></param>
        /// <param name="exit"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void RegionEnteredRpc(NetworkBehaviourReference enemyReference, bool exit = false)
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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public static void PurgeTulipSnake(EnemyAI enemy)
        {
            if (enemy is FlowerSnakeEnemy tulipSnake)
            {
                // Check if Tulip Snake is clinging to a player, and get them off if they are.
                if (tulipSnake.clingingToPlayer != null && tulipSnake.clingingToPlayer.IsLocalClient() && tulipSnake.clingPosition == 4)
                {
                    tulipSnake.StopClingingOnLocalClient(true);
                    tulipSnake.StopClingingServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                }

                if (tulipSnake.IsHost)
                {
                    // Swiftly end the Tulip Snake before it can react.
                    tulipSnake.KillEnemyOnOwnerClient(true);
                }

                // Disable Tulip Snake so it simply, abruptly vanishes.
                tulipSnake.gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void FreezeEnemy(EnemyAI enemy)
        {
            if (!IsSpawned || !IsHost || enemy.agent == null)
            {
                return;
            }

            NavMeshAgent agent = enemy.agent;
            _ = enemy.StartCoroutine(FreezeAgentDelayed(agent, agent.speed));

            if (agent.isOnOffMeshLink)
            {
                OffMeshLinkData offMeshLinkData = agent.currentOffMeshLinkData;
                agent.CompleteOffMeshLink();

                if (offMeshLinkData.valid && offMeshLinkData.startPos != Vector3.zero)
                {
                    _ = agent.Warp(offMeshLinkData.startPos);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="originalSpeed"></param>
        /// <returns></returns>
        private static IEnumerator FreezeAgentDelayed(NavMeshAgent agent, float originalSpeed)
        {
            agent.speed = 0.0f;
            yield return Yielders.WaitForSeconds(1.0f);
            agent.speed = originalSpeed;
        }
    }
}