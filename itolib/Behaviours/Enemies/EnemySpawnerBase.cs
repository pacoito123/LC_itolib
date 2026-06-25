using itolib.Behaviours.Networking;
using itolib.Structs;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     Represents an abstract enemy spawner.
    /// </summary>
    /// <typeparam name="T">Enemy of type <c>EnemyAI</c> to be spawned.</typeparam>
    public abstract class EnemySpawnerBase<T> : NetworkedSpawner<T> where T : EnemyAI
    {
        /// <summary>
        ///     Whether spawned enemies should increase or decrease enemy power levels for the current round or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Affected power levels may not correspond to where enemies are actually spawned.</remarks>
        [Space(5.0f)]
        [Header("Enemy Spawner Base")]
        [Tooltip("Whether spawned enemies should increase or decrease enemy power levels for the current round or not. NOTE: Affected power levels "
            + "may not correspond to where enemies are actually spawned.")]
        [SerializeField] private bool influencePowerLevels = true;

        /// <inheritdoc/>
        protected override void SpawnPerformed(T? spawnedPrefab, TransformInfo spawnLocation)
        {
            if (spawnedPrefab == null || !spawnedPrefab.IsSpawned)
            {
                return;
            }

            if (RoundManager.Instance != null)
            {
                // Apply enemy power level changes.
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
                    // Set power level as removed to not decrease it when the enemy dies.
                    spawnedPrefab.removedPowerLevel = true;
                }

                RoundManager.Instance.SpawnedEnemies.Add(spawnedPrefab);
            }

            base.SpawnPerformed(spawnedPrefab, spawnLocation);
        }

        /// <summary>
        ///     Attempt to obtain the <c>NetworkObject</c> to spawn from a given <c>EnemyType</c>.
        /// </summary>
        /// <param name="enemyNetworkObject"><c>NetworkObject</c> of the enemy to spawn, as an out parameter.</param>
        /// <param name="enemy">Enemy to obtain the <c>NetworkObject</c> off of.</param>
        /// <returns>Whether a <c>NetworkObject</c> was successfully obtained or not.</returns>
        protected static bool TryGetNetworkObject(out NetworkObject enemyNetworkObject, EnemyType? enemy)
        {
            enemyNetworkObject = null!;

            return enemy != null && enemy.enemyPrefab != null && enemy.enemyPrefab.TryGetComponent(out enemyNetworkObject);
        }
    }
}