using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class EnemyAnnihilator : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private readonly List<EnemyAI?> enemiesToAnnihilate = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Enemy Annihilator")]
        [Tooltip("")]
        [SerializeField] private bool destroyKillable = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool destroyUnkillable = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AnnihilateEnemies()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            for (int i = 0; i < enemiesToAnnihilate.Count; i++)
            {
                EnemyAI? enemy = enemiesToAnnihilate[i];

                if (enemy == null || !enemy.IsSpawned)
                {
                    continue;
                }

                if (enemy.enemyType != null && enemy.enemyType.canDie)
                {
                    if (!enemy.isEnemyDead)
                    {
                        enemy.KillEnemy(destroyKillable);
                    }
                }
                else if (destroyUnkillable)
                {
                    if (RoundManager.Instance != null && RoundManager.Instance.SpawnedEnemies != null)
                    {
                        _ = RoundManager.Instance.SpawnedEnemies.Remove(enemy);
                    }

                    if (!enemy.removedPowerLevel)
                    {
                        enemy.SubtractFromPowerLevel();
                    }

                    enemy.GetComponent<NetworkObject>().Despawn(true);
                }
            }

            enemiesToAnnihilate.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void PrepareForAnnihilation(EnemyAI enemy)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (enemy.IsSpawned && !enemiesToAnnihilate.Contains(enemy))
            {
                enemiesToAnnihilate.Add(enemy);
            }
        }
    }
}