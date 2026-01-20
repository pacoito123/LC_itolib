using itolib.Behaviours.Helpers;
using itolib.Behaviours.Networking;
using itolib.Interfaces;
using itolib.Structs;
using itolib.Util;
using LethalLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

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
        public string itemName = string.Empty;

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
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Replace with the desired item's 'itemName' field.")]
        public Item? itemToSpawn;

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
            itemName = (itemWithRarity.spawnableItem != null) ? itemWithRarity.spawnableItem.itemName : string.Empty;
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
        public NetworkList<ItemInfo> SyncedItems { get; private set; } = null!;

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
        public bool InitializedWeights { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Scrap Spawner")]
        [field: Tooltip("")]
        [field: SerializeField] public ScrapWeightEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [FormerlySerializedAs("useMoonScrapSpawns")]
        [SerializeField] private bool addMoonScrapSpawns;

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
        [Space(5.0f)]
        [Header("Other")]
        [Tooltip("")]
        [SerializeField] private bool muteSpawnedScrap;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool respectSingleItemDay;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Use 'addMoonScrapSpawns' field instead.")]
        [SerializeField] private bool useMoonScrapSpawns;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (respectSingleItemDay && TryGetNetworkObject(out NetworkObject itemNetworkObject, SimulateAnomaly.SingleItem))
            {
                return itemNetworkObject;
            }

            if (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out ScrapWeightEntry entry, isSeededRandom
                ? SeededSelf.GetSeededRandom() : null))
            {
                if (SearchContent.TryFindItem(out Item item, entry.itemName) && TryGetNetworkObject(out itemNetworkObject, item))
                {
                    return itemNetworkObject;
                }
                else if (entry.itemToSpawn != null && SearchContent.TryFindItem(out item, entry.itemToSpawn.name, checkObjectName: true)
                    && TryGetNetworkObject(out itemNetworkObject, item))
                {
                    return itemNetworkObject;
                }
            }

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemNetworkObject"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool TryGetNetworkObject(out NetworkObject itemNetworkObject, Item? item)
        {
            itemNetworkObject = null!;

            return item != null && item.spawnPrefab != null && item.spawnPrefab.TryGetComponent(out itemNetworkObject);
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

            if (IsSpawned)
            {
                SyncedItems.Add(serializedItem);
            }
            else
            {
                base.SpawnPerformed(item, spawnLocation);
            }
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

            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            if (useMoonScrapSpawns || addMoonScrapSpawns)
            {
                List<SpawnableItemWithRarity> spawnableScrap = LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap;
                ScrapWeightEntry[] scrapEntries = new ScrapWeightEntry[spawnableScrap.Count];

                for (int i = 0; i < spawnableScrap.Count; i++)
                {
                    scrapEntries[i] = new(spawnableScrap[i]);
                }

                WeightedSelf.AddWeights(scrapEntries);
            }

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedItems.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<ItemInfo>.EventType.Add
                    && changeEvent.Value.itemReference.TryGet(out GrabbableObject item))
                {
                    SyncItemValues(item, changeEvent.Value);
                }
            };
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        public void AddWeightEntry(ScrapWeightEntry entry)
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
        public void AddWeightEntries(ScrapWeightEntry[] entries)
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
        /// <param name="syncedItem"></param>
        /// <param name="itemInfo"></param>
        private void SyncItemValues(GrabbableObject syncedItem, ItemInfo itemInfo)
        {
            _ = StartCoroutine(WaitForItemFall(syncedItem, itemInfo.transformInfo));

            if (syncedItem.itemProperties != null)
            {
                if (itemInfo.meshVariant != -1 && syncedItem.TryGetComponent(out MeshFilter itemFilter))
                {
                    itemFilter.sharedMesh = syncedItem.itemProperties.meshVariants[itemInfo.meshVariant];
                }

                if (itemInfo.materialVariant != -1 && syncedItem.TryGetComponent(out MeshRenderer itemRenderer))
                {
                    itemRenderer.sharedMaterial = syncedItem.itemProperties.materialVariants[itemInfo.materialVariant];
                }
            }

            syncedItem.SetScrapValue(itemInfo.scrapValue);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.totalScrapValueInLevel += itemInfo.scrapValue;
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
            yield return Yielders.WaitForEndOfFrame;
            yield return Yielders.WaitForEndOfFrame;

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

            yield return Yielders.WaitForEndOfFrame;

            if (muteSpawnedScrap)
            {
                AudioSource[] sources = item.GetComponentsInChildren<AudioSource>();

                for (int i = 0; i < sources.Length; i++)
                {
                    sources[i].Stop();
                }
            }

            OnSpawnPerformed.Invoke(item);
        }
    }
}