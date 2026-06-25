using itolib.Extensions;
using itolib.Interfaces;
using itolib.Structs;
using itolib.Util;
using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     Represents a single entry with weights to be used for weighted enemy selection.
    /// </summary>
    /// <param name="enemyWithRarity">Enemy to copy name and weights from.</param>
    [Serializable]
    public struct EnemyWeightEntry(SpawnableEnemyWithRarity enemyWithRarity) : IWeightedEntry
    {
        /// <summary>
        ///     Enemy name corresponding to this specific entry.
        /// </summary>
        [Header("Enemy Weight Entry")]
        [Tooltip("Enemy name corresponding to this specific entry.")]
        public string enemyName = (enemyWithRarity.enemyType != null) ? enemyWithRarity.enemyType.enemyName : string.Empty;

        /// <inheritdoc/>
        [field: Tooltip("Weight value for this specific entry.")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; } = enemyWithRarity.rarity;

        /// <inheritdoc/>
        [field: Tooltip("Weight modifiers to apply whenever this specific entry is used.")]
        [field: SerializeField] public WeightedModifier[]? WeightedModifiers { get; set; }

        /// <inheritdoc/>
        [field: Tooltip("Whether this specific entry can be used more than once or not.")]
        [field: SerializeField] public bool SingleUse { get; set; } = false;

        /// <summary>
        ///     Enemy corresponding to this specific entry.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Replace with the desired enemy's 'enemyName' field.")]
        [Obsolete("Replace with the desired enemy's 'enemyName' field.")]
        public EnemyType? enemyToSpawn = enemyWithRarity.enemyType;
    }

    /// <summary>
    ///     Represents an enemy spawner with weighted selection capabilities.
    /// </summary>
    public class EnemySpawner : EnemySpawnerBase<EnemyAI>, IWeightedScript<EnemyWeightEntry>
    {
        /// <inheritdoc/>
        public IWeightedScript<EnemyWeightEntry> WeightedSelf { get; }

        /// <inheritdoc/>
        public int[]? CurrentWeights { get; set; }

        /// <inheritdoc/>
        public int TotalWeight { get; set; }

        /// <inheritdoc/>
        public bool InitializedWeights { get; set; }

        /// <summary>
        ///     Name of the enemy to be spawned.
        /// </summary>
        /// <remarks><b>NOTE:</b> Should be left empty if intending to use weighted selection.</remarks>
        [Space(5.0f)]
        [Header("Enemy Spawner")]
        [Tooltip("Name of the enemy to be spawned. NOTE: Should be left empty if intending to use weighted selection.")]
        [SerializeField] private string enemyToSpawn = string.Empty;

        /// <inheritdoc/>
        [field: Tooltip("List of weighted entries of type EnemyWeightEntry.")]
        [field: SerializeField] public EnemyWeightEntry[]? WeightedEntries { get; set; }

        /// <inheritdoc/>
        public override NetworkObject? GetPrefabToSpawn()
        {
            // Spawn enemy specified in the 'enemyToSpawn' field, if one is set.
            if (!enemyToSpawn.IsNullOrEmpty() && SearchContent.TryFindEnemy(out EnemyType enemy, enemyToSpawn)
                && TryGetNetworkObject(out NetworkObject enemyNetworkObject, enemy))
            {
                return enemyNetworkObject;
            }

            // Spawn enemy using weighted selection.
            if (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out EnemyWeightEntry entry, out int _, isSeededRandom
                ? SeededSelf.GetSeededRandom() : null))
            {
                if (SearchContent.TryFindEnemy(out enemy, entry.enemyName) && TryGetNetworkObject(out enemyNetworkObject, enemy))
                {
                    return enemyNetworkObject;
                }
#pragma warning disable CS0618 // Type or member is obsolete.
                else if (entry.enemyToSpawn != null && (TryGetNetworkObject(out enemyNetworkObject, entry.enemyToSpawn) || (SearchContent.TryFindEnemy(out enemy,
                    entry.enemyToSpawn.name, checkObjectName: true) && TryGetNetworkObject(out enemyNetworkObject, enemy))))
#pragma warning restore CS0618 // Type or member is obsolete.
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
        ///     Initialize weights for every defined <c>EnemyWeightEntry</c>.
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
        ///     Add a single weighted entry of type <c>EnemyWeightEntry</c>.
        /// </summary>
        /// <param name="entry">Entry of type <c>EnemyWeightEntry</c> to add.</param>
        public void AddWeightEntry(EnemyWeightEntry entry)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     Add multiple weighted entries of type <c>EnemyWeightEntry></c>.
        /// </summary>
        /// <param name="entries">Entries of type <c>EnemyWeightEntry</c> to add.</param>
        public void AddWeightEntries(EnemyWeightEntry[] entries)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     Remove weights for the weighted entry of type <c>EnemyWeightEntry</c> at the specified index.
        /// </summary>
        /// <remarks>Sets weights to <c>0</c> instead of actually removing them.</remarks>
        /// <param name="index">Index of the entry of type <c>EnemyWeightEntry</c> to remove.</param>
        public void RemoveWeightEntry(int index)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     Switch target enemy to be spawned.
        /// </summary>
        /// <remarks><b>NOTE:</b> Should not be used if intending to use weighted selection.</remarks>
        /// <param name="enemyName">Name of the enemy to be spawned.</param>
        public void SwitchEnemyToSpawn(string enemyName)
        {
            enemyToSpawn = enemyName;
        }
    }
}