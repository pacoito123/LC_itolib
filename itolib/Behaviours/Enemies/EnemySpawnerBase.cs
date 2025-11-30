using itolib.Behaviours.Networking;
using itolib.Structs;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EnemySpawnerBase<T> : NetworkedSpawner<T> where T : EnemyAI
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Enemy Spawner Base")]
        [Tooltip("")]
        [SerializeField] private bool influencePowerLevels = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnedPrefab"></param>
        /// <param name="spawnLocation"></param>
        protected override void SpawnPerformed(T? spawnedPrefab, TransformInfo spawnLocation)
        {
            if (spawnedPrefab == null || !spawnedPrefab.IsSpawned)
            {
                return;
            }

            if (RoundManager.Instance != null)
            {
                if (influencePowerLevels)
                {
                    EnemyType? enemyType = spawnedPrefab.enemyType;

                    if (enemyType != null)
                    {
                        if (enemyType.isDaytimeEnemy)
                        {
                            RoundManager.Instance.currentDaytimeEnemyPower += enemyType.PowerLevel;
                        }
                        else if (enemyType.isOutsideEnemy)
                        {
                            RoundManager.Instance.currentOutsideEnemyPower += enemyType.PowerLevel;
                        }
                        else
                        {
                            RoundManager.Instance.currentEnemyPower += enemyType.PowerLevel;
                        }
                    }
                }
                else
                {
                    spawnedPrefab.removedPowerLevel = true;
                }

                RoundManager.Instance.SpawnedEnemies.Add(spawnedPrefab);
            }

            base.SpawnPerformed(spawnedPrefab, spawnLocation);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyNetworkObject"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        protected static bool TryGetNetworkObject(out NetworkObject enemyNetworkObject, EnemyType? enemy)
        {
            enemyNetworkObject = null!;

            return enemy != null && enemy.enemyPrefab != null && enemy.enemyPrefab.TryGetComponent(out enemyNetworkObject);
        }
    }
}