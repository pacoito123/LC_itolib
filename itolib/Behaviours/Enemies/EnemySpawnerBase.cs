using itolib.Behaviours.Networking;
using itolib.Extensions;
using LethalLevelLoader;
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
        [Header("Enemy Spawner Base")]
        [Tooltip("")]
        [SerializeField] private bool influencePowerLevels = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyToSpawn"></param>
        /// <returns></returns>
        protected NetworkObject? GetEnemyToSpawn(EnemyType? enemyToSpawn)
        {
            if (enemyToSpawn == null)
            {
                // TODO: Log warning.
                return null;
            }

            if (enemyToSpawn.enemyPrefab != null)
            {
                return enemyToSpawn.enemyPrefab.GetComponent<NetworkObject>();
            }

            ExtendedEnemyType? extendedEnemy = PatchedContent.ExtendedEnemyTypes.Find(extendedEnemy =>
                extendedEnemy.EnemyType.name.CompareOrdinal(enemyToSpawn.name));

            return extendedEnemy != null ? extendedEnemy.EnemyType.enemyPrefab.GetComponent<NetworkObject>() : null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        /// <returns></returns>
        protected override bool AdditionalProcessing(T enemy, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (RoundManager.Instance != null)
            {
                if (influencePowerLevels)
                {
                    EnemyType? enemyType = enemy.enemyType;

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
                    enemy.removedPowerLevel = true;
                }

                RoundManager.Instance.SpawnedEnemies.Add(enemy);
            }

            return true;
        }
    }
}