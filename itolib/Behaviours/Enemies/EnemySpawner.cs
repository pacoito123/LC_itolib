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
        ///     Cached instance of <c>EnemySpawner</c> as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        public IWeightedScript<EnemyWeightEntry> WeightedSelf { get; }

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
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out EnemyWeightEntry entry, isSeededRandom ?
                SeededSelf.GetSeededRandom() : null)) ? GetEnemyToSpawn(entry.enemyToSpawn) : null;
        }

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        protected EnemySpawner() : base()
        {
            WeightedSelf = this;
        }

        /// <summary>
        ///     Initialize weights for every <c>EnemyWeightEntry</c>.
        /// </summary>
        protected override void Awake()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                WeightedSelf.InitializeWeights();
            }

            base.Awake();
        }
    }
}