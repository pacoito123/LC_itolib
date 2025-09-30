using itolib.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct EnemyWeightEntry : IWeightedEntry
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Enemy Weight Entry")]
        [Tooltip("")]
        public EnemyType? enemyToSpawn = null;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; } = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool SingleUse { get; set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public EnemyWeightEntry() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyWithRarity"></param>
        public EnemyWeightEntry(SpawnableEnemyWithRarity enemyWithRarity)
        {
            enemyToSpawn = enemyWithRarity.enemyType;
            Weight = enemyWithRarity.rarity;
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class EnemySpawner : EnemySpawnerBase<EnemyAI>, IWeightedScript<EnemyWeightEntry>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public int[]? CumulativeWeights { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int TotalWeight { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Header("Enemy Spawner")]
        [field: Tooltip("")]
        [field: SerializeField] public EnemyWeightEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     Cached instance of the current <c>EnemySpawner</c> as an <c>IWeightedScript</c>, to avoid having to cast. 
        /// </summary>
        private IWeightedScript<EnemyWeightEntry> weightedSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return (WeightedEntries?.Length > 0 && weightedSelf.TryObtainRandomEntry(out EnemyWeightEntry entry, isSeededRandom ?
                seededSelf.GetSeededRandom() : null)) ? GetEnemyToSpawn(entry.enemyToSpawn) : null;
        }

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        protected override void Awake()
        {
            weightedSelf = this;

            if (NetworkManager.Singleton.IsHost)
            {
                weightedSelf.InitializeWeights();
            }

            base.Awake();
        }
    }
}