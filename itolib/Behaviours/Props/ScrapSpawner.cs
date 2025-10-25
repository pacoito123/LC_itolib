using itolib.Behaviours.Helpers;
using itolib.Behaviours.Networking;
using itolib.Compatibility;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Structs;
using LethalLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct ScrapWeightEntry : IWeightedEntry
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scrap Weight Entry")]
        [Tooltip("")]
        public Item? itemToSpawn;

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
        [field: SerializeField] public bool SingleUse { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public ScrapWeightEntry() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemWithRarity"></param>
        public ScrapWeightEntry(SpawnableItemWithRarity itemWithRarity)
        {
            itemToSpawn = itemWithRarity.spawnableItem;
            Weight = itemWithRarity.rarity;
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class ScrapSpawner : NetworkedSpawner<GrabbableObject>, IWeightedScript<ScrapWeightEntry>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public NetworkList<ItemInfo> SyncedItems { get; private set; }

        /// <summary>
        ///     Cached instance of <c>ScrapSpawner</c> as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        public IWeightedScript<ScrapWeightEntry> WeightedSelf { get; }

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
        [field: Header("Item Spawner")]
        [field: Tooltip("")]
        [field: SerializeField] public ScrapWeightEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool useMoonScrapSpawns;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Item Properties")]
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int overrideMinValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int overrideMaxValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool applyScrapMultiplier = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool allowMeshVariants = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool allowMaterialVariants = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Position and Rotation")]
        [Tooltip("")]
        [SerializeField] private bool fallToGround = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool randomizePosition;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool applyRestingRotation;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool respectSingleItemDay;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (respectSingleItemDay && SimulateAnomaly.SingleItem != null)
            {
                return SimulateAnomaly.SingleItem.spawnPrefab.GetComponent<NetworkObject>();
            }

            if (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out ScrapWeightEntry entry, isSeededRandom
                ? SeededSelf.GetSeededRandom() : null))
            {
                Item? itemToSpawn = entry.itemToSpawn;

                if (itemToSpawn == null)
                {
                    // TODO: Log warning.
                    return null;
                }

                if (itemToSpawn.spawnPrefab != null)
                {
                    return itemToSpawn.spawnPrefab.GetComponent<NetworkObject>();
                }

                ExtendedItem? extendedItem = PatchedContent.ExtendedItems.Find(extendedItem =>
                    extendedItem.Item.name.CompareOrdinal(itemToSpawn.name));

                if (extendedItem != null)
                {
                    return extendedItem.Item.spawnPrefab.GetComponent<NetworkObject>();
                }

                if (DawnLibCompatibility.Enabled)
                {
                    Item? dawnItem = DawnLibCompatibility.GetDawnItem(itemToSpawn.name);

                    return (dawnItem != null) ? dawnItem.spawnPrefab.GetComponent<NetworkObject>() : null;
                }
            }

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected override Transform? GetParentOverride()
        {
            return RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spawnLocation"></param>
        protected override void SpawnPerformed(GrabbableObject? item, TransformInfo spawnLocation)
        {
            if (item == null || !item.IsSpawned || item.itemProperties == null)
            {
                return;
            }

            int minValue = overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                maxValue = overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue;

            /* if (SimulateAnomaly.SingleItem != null)
            {
                // TODO: Apply single item day values.
            } */

            int scrapValue = isSeededRandom ? SeededSelf.GetSeededRandom().Next(minValue, maxValue)
                : UnityEngine.Random.RandomRangeInt(minValue, maxValue);

            if (RoundManager.Instance != null)
            {
                if (applyScrapMultiplier)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }
            }

            if (applyRestingRotation)
            {
                spawnLocation.rotation *= Quaternion.Euler(item.itemProperties.restingRotation);
            }

            ItemInfo serializedItem = new()
            {
                transformInfo = spawnLocation,
                itemReference = item,
                scrapValue = scrapValue,
                meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                    ? (isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, item.itemProperties.meshVariants.Length)
                        : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.meshVariants.Length)) : -1,
                materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                    ? (isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, item.itemProperties.materialVariants.Length)
                        : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.materialVariants.Length)) : -1
            };

            SyncedItems.Add(serializedItem);

            base.SpawnPerformed(item, spawnLocation);
        }

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        protected ScrapSpawner() : base()
        {
            WeightedSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            SyncedItems = new();

            if (NetworkManager.Singleton.IsHost)
            {
                if (useMoonScrapSpawns)
                {
                    List<SpawnableItemWithRarity> spawnableScrap = LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap;
                    WeightedEntries = new ScrapWeightEntry[spawnableScrap.Count];

                    for (int i = 0; i < spawnableScrap.Count; i++)
                    {
                        WeightedEntries[i] = new(spawnableScrap[i]);
                    }
                }

                WeightedSelf.InitializeWeights();
            }

            base.Awake();
        }

        /// <summary>
        ///     TOOD.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedItems.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<ItemInfo>.EventType.Add)
                {
                    SyncItemValues(changeEvent.Value);
                }
            };
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        public void AddWeightEntry(ScrapWeightEntry entry)
        {
            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entries"></param>
        public void AddWeightEntries(ScrapWeightEntry[] entries)
        {
            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveWeightEntry(int index)
        {
            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="syncedItem"></param>
        private void SyncItemValues(ItemInfo syncedItem)
        {
            if (!syncedItem.itemReference.TryGet(out GrabbableObject item))
            {
                return;
            }

            _ = StartCoroutine(WaitForItemFall(item, syncedItem.transformInfo));

            if (item.itemProperties != null)
            {
                if (syncedItem.meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
                {
                    itemFilter.sharedMesh = item.itemProperties.meshVariants[syncedItem.meshVariant];
                }

                if (syncedItem.materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
                {
                    itemRenderer.sharedMaterial = item.itemProperties.materialVariants[syncedItem.materialVariant];
                }
            }

            item.SetScrapValue(syncedItem.scrapValue);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.totalScrapValueInLevel += syncedItem.scrapValue;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spawnLocation"></param>
        /// <returns></returns>
        private IEnumerator WaitForItemFall(GrabbableObject item, TransformInfo spawnLocation)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            item.fallTime = 1.0f;
            item.hasHitGround = true;
            item.reachedFloorTarget = true;

            item.transform.SetPositionAndRotation(spawnLocation.position, spawnLocation.rotation);

            item.startFallingPosition = spawnLocation.position;
            item.targetFloorPosition = spawnLocation.position;

            if (fallToGround)
            {
                item.FallToGround(randomizePosition, true);
            }
        }
    }
}