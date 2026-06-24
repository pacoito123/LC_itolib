using itolib.Extensions;
using itolib.Interfaces;
using itolib.Util;
using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <param name="enemyWithRarity"></param>
    [Serializable]
    public struct EnemyWeightEntry(SpawnableEnemyWithRarity enemyWithRarity) : IWeightedEntry
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Enemy Weight Entry")]
        [Tooltip("")]
        public string enemyName = (enemyWithRarity.enemyType != null) ? enemyWithRarity.enemyType.enemyName : string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; } = enemyWithRarity.rarity;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool SingleUse { get; set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Replace with the desired enemy's 'enemyName' field.")]
        public EnemyType? enemyToSpawn = enemyWithRarity.enemyType;
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
        public bool InitializedWeights { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Enemy Spawner")]
        [Tooltip("")]
        [SerializeField] private string enemyToSpawn = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public EnemyWeightEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (!enemyToSpawn.IsNullOrEmpty() && SearchContent.TryFindEnemy(out EnemyType enemy, enemyToSpawn)
                && TryGetNetworkObject(out NetworkObject enemyNetworkObject, enemy))
            {
                return enemyNetworkObject;
            }

            if (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out EnemyWeightEntry entry, isSeededRandom
                ? SeededSelf.GetSeededRandom() : null))
            {
                if (SearchContent.TryFindEnemy(out enemy, entry.enemyName) && TryGetNetworkObject(out enemyNetworkObject, enemy))
                {
                    return enemyNetworkObject;
                }
                else if (entry.enemyToSpawn != null && (TryGetNetworkObject(out enemyNetworkObject, entry.enemyToSpawn) || (SearchContent.TryFindEnemy(out enemy,
                    entry.enemyToSpawn.name, checkObjectName: true) && TryGetNetworkObject(out enemyNetworkObject, enemy))))
                {
                    return enemyNetworkObject;
                }
            }

            return null;
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
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        public void AddWeightEntry(EnemyWeightEntry entry)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entries"></param>
        public void AddWeightEntries(EnemyWeightEntry[] entries)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveWeightEntry(int index)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyName"></param>
        public void SwitchEnemyToSpawn(string enemyName)
        {
            enemyToSpawn = enemyName;
        }
    }
}