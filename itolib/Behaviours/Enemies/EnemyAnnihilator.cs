using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

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
        private readonly HashSet<EnemyAI?> enemiesToAnnihilate = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        private EnemyAI? targetedEnemy;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int enemiesKilled;

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
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<EnemyAI> onEnemyHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<EnemyAI> onEnemyKill = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onEnemiesKilled = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AnnihilateEnemies()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            enemiesKilled = 0;

            foreach (EnemyAI? enemy in enemiesToAnnihilate)
            {
                AnnihilateEnemy(enemy!);
            }

            if (enemiesKilled > 0)
            {
                onEnemiesKilled.Invoke(enemiesKilled);
            }

            enemiesToAnnihilate.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void PrepareForAnnihilation(EnemyAI enemy)
        {
            if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned || !enemy.IsHost)
            {
                return;
            }

            _ = enemiesToAnnihilate.Add(enemy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AnnihilateEnemy()
        {
            AnnihilateEnemy(targetedEnemy!);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void AnnihilateEnemy(EnemyAI enemy)
        {
            if (enemy == null || enemy.isEnemyDead || !enemy.IsSpawned || !enemy.IsHost)
            {
                return;
            }

            if (enemy.enemyType != null && enemy.enemyType.canDie)
            {
                onEnemyKill.Invoke(enemy);
                enemy.KillEnemy(destroyKillable);

                enemiesKilled++;
            }
            else if (destroyUnkillable)
            {
                onEnemyKill.Invoke(enemy);

                if (RoundManager.Instance != null && RoundManager.Instance.SpawnedEnemies != null)
                {
                    _ = RoundManager.Instance.SpawnedEnemies.Remove(enemy);
                }

                enemy.SubtractFromPowerLevel();
                enemy.GetComponent<NetworkObject>().Despawn(destroy: true);

                enemiesKilled++;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="damage"></param>
        public void DamageEnemy(int damage)
        {
            if (targetedEnemy == null || targetedEnemy.isEnemyDead || !targetedEnemy.IsSpawned)
            {
                return;
            }

            targetedEnemy.HitEnemyOnLocalClient(damage);
            onEnemyHit.Invoke(targetedEnemy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetedEnemy"></param>
        public void SwitchTarget(EnemyAI targetedEnemy)
        {
            this.targetedEnemy = targetedEnemy;
        }
    }
}