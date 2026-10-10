using itolib.Interfaces;
using itolib.Structs;
using itolib.Util;
using System;
using System.Collections;
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
        /// <summary>
        ///     List of spawned <c>EnemyAINestSpawnObject</c> instances to sync with clients.
        /// </summary>
        public NetworkList<NestInfo> SyncedNests { get; private set; }

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
            if (SearchContent.TryFindEnemy(out EnemyType enemy, enemyToSpawn)
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

            SyncedNests = null!;
        }

        /// <summary>
        ///     Initialize weights for every defined <c>EnemyWeightEntry</c>.
        /// </summary>
        protected override void Awake()
        {
            SyncedNests = new();

            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            base.Awake();
        }

        /// <summary>
        ///     Set relevant <c>EnemyAINestSpawnObject</c> values to be synced when spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedNests.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<NestInfo>.EventType.Add && changeEvent.Value.nestReference.TryGet(out NetworkObject nestNetworkObject)
                    && nestNetworkObject.TryGetComponent(out EnemyAINestSpawnObject enemyNest))
                {
                    SyncNestValues(enemyNest, changeEvent.Value);
                }
            };
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

        /// <inheritdoc/>
        protected override void PerformSpawn(NetworkObject prefabToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            // Check if enemy requires a nest, to delay spawning for a frame (after an appropriate nest is spawned).
            if (prefabToSpawn.TryGetComponent(out EnemyAI enemy) && enemy.enemyType != null && enemy.enemyType.nestSpawnPrefab != null
                && enemy.enemyType.requireNestObjectsToSpawn)
            {
                // Instantiate enemy nest at the specified position.
                GameObject nestContainer = Instantiate(enemy.enemyType.nestSpawnPrefab, spawnPosition, spawnRotation, GetParentOverride());

                if (nestContainer.TryGetComponent(out NetworkObject nestNetworkObject))
                {
                    // Spawn enemy nest for all clients.
                    nestNetworkObject.Spawn(destroyWithScene);

                    // Serialize relevant nest values to sync.
                    NestInfo serializedNest = new()
                    {
                        transformInfo = new()
                        {
                            position = spawnPosition,
                            rotation = spawnRotation
                        },
                        nestReference = nestNetworkObject
                    };

                    // Add spawned nest to NetworkList.
                    SyncedNests.Add(serializedNest);

                    return; // Delay enemy spawning until nest is actually spawned.
                }
            }

            base.PerformSpawn(prefabToSpawn, spawnPosition, spawnRotation);
        }

        /// <summary>
        ///     Apply synced nest values upon being spawned.
        /// </summary>
        /// <param name="enemyNest">Nest to be synced.</param>
        /// <param name="nestInfo">Relevant nest values to sync.</param>
        private void SyncNestValues(EnemyAINestSpawnObject enemyNest, NestInfo nestInfo)
        {
            TransformInfo transformInfo = nestInfo.transformInfo;
            enemyNest.transform.SetPositionAndRotation(transformInfo.position, transformInfo.rotation);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.enemyNestSpawnObjects ??= [];
                RoundManager.Instance.enemyNestSpawnObjects.Insert(0, enemyNest); // Insert at start to make sure the correct nest is selected.
            }

            if (enemyNest.enemyType == null || enemyNest.enemyType.enemyPrefab == null)
            {
                return;
            }

            enemyNest.enemyType.nestsSpawned++;

            if (NetworkManager.Singleton.IsHost)
            {
                _ = StartCoroutine(WaitForNestSpawn(enemyNest.enemyType.enemyPrefab, transformInfo, GetParentOverride()));
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to wait an extra frame before spawning an enemy that requires a nest.
        /// </summary>
        /// <param name="enemyPrefab">Enemy prefab to spawn.</param>
        /// <param name="transformInfo">Position and rotation to set for the enemy.</param>
        /// <param name="parent">Parent to instantiate the enemy to.</param>
        private IEnumerator WaitForNestSpawn(GameObject enemyPrefab, TransformInfo transformInfo, Transform? parent)
        {
            yield return null;

            GameObject enemyContainer = Instantiate(enemyPrefab, transformInfo.position, transformInfo.rotation, parent);

            if (enemyContainer.TryGetComponent(out EnemyAI enemy))
            {
                PrefabInstances.Add(enemy);

                if (enemyContainer.TryGetComponent(out NetworkObject enemyNetworkObject))
                {
                    enemyNetworkObject.Spawn(destroyWithScene);
                }

                SpawnPerformed(enemy, transformInfo);
            }
        }
    }
}