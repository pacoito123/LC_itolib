using itolib.Behaviours.Networking;
using itolib.Compatibility;
using itolib.Extensions;
using itolib.Structs;
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

            if (extendedEnemy != null)
            {
                return extendedEnemy.EnemyType.enemyPrefab.GetComponent<NetworkObject>();
            }

            if (DawnLibCompatibility.Enabled)
            {
                EnemyType? dawnEnemy = DawnLibCompatibility.GetDawnEnemyType(enemyToSpawn.name);

                return (dawnEnemy != null) ? dawnEnemy.enemyPrefab.GetComponent<NetworkObject>() : null;
            }

            return null;
        }
    }
}