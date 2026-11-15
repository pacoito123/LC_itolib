using itolib.Behaviours.Networking;
using itolib.Structs;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     Represents an abstract enemy spawner.
    /// </summary>
    /// <typeparam name="T">Enemy of type <c>EnemyAI</c> to be spawned.</typeparam>
    public abstract class EnemySpawnerBase<T> : NetworkedSpawner<T> where T : Behaviour
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
            if (spawnedPrefab == null)
            {
                return;
            }

            if (RoundManager.Instance != null && spawnedPrefab is EnemyAI spawnedEnemy)
            {
                // Apply enemy power level changes.
                if (influencePowerLevels)
                {
                    EnemyType? enemyType = spawnedEnemy.enemyType;

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
                    spawnedEnemy.removedPowerLevel = true;
                }

                RoundManager.Instance.SpawnedEnemies.Add(spawnedEnemy);
            }

            base.SpawnPerformed(spawnedPrefab, spawnLocation);
        }
    }
}