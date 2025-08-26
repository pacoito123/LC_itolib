using itolib.Behaviours.Helpers;
using itolib.Behaviours.Networking;
using itolib.Enums;
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
        public Item? itemToSpawn = null;

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
        public List<Item> ItemPool { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<ItemInfo> ItemsToSync { get; private set; } = [];

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
        [SerializeField] private ContentTag[]? tagsToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private bool requireAllTags;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool useMoonScrapSpawns;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool exhaustivePool;

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
        [SerializeField] private bool applyScrapMultiplier = true;

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
        ///     Cached instance of the current <c>ScrapSpawner</c> as an <c>IWeightedScript</c>, to avoid having to cast. 
        /// </summary>
        private IWeightedScript<ScrapWeightEntry> weightedSelf;

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

            if (WeightedEntries?.Length > 0 && weightedSelf.TryObtainRandomEntry(out ScrapWeightEntry entry, isSeededRandom
                ? seededSelf.GetSeededRandom() : null))
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
            }

            /* if (ItemPool.Count > 0)
            {
                int poolIndex = seededRandom ? Random.Next(0, ItemPool.Count)
                    : UnityEngine.Random.RandomRangeInt(0, ItemPool.Count);
                Item randomItem = ItemPool[poolIndex];

                if (exhaustivePool)
                {
                    ItemPool.RemoveAt(poolIndex);
                }

                return randomItem.spawnPrefab.GetComponent<NetworkObject>();
            } */

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PerformSpawn()
        {
            base.PerformSpawn();

            if (activationTime is not ActivationTime.ScrapSpawn or ActivationTime.HazardSpawn)
            {
                SyncAllItemValuesServerRpc();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        protected override bool AdditionalProcessing(GrabbableObject item, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (item.itemProperties != null)
            {
                int minValue = overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                    maxValue = overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue;

                int scrapValue = isSeededRandom ? seededSelf.GetSeededRandom().Next(minValue, maxValue)
                    : UnityEngine.Random.RandomRangeInt(minValue, maxValue);

                if (RoundManager.Instance != null)
                {
                    item.transform.SetParent(RoundManager.Instance.spawnedScrapContainer);

                    if (applyScrapMultiplier)
                    {
                        scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                    }
                }

                if (applyRestingRotation)
                {
                    spawnRotation *= Quaternion.Euler(item.itemProperties.restingRotation);
                }

                ItemInfo serializedItem = new()
                {
                    transformInfo = new()
                    {
                        position = spawnPosition,
                        rotation = spawnRotation
                    },
                    scrapValue = scrapValue,
                    meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                        ? (isSeededRandom ? seededSelf.GetSeededRandom().Next(0, item.itemProperties.meshVariants.Length)
                            : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.meshVariants.Length)) : -1,
                    materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                        ? (isSeededRandom ? seededSelf.GetSeededRandom().Next(0, item.itemProperties.materialVariants.Length)
                            : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.materialVariants.Length)) : -1
                };

                ItemsToSync.Add(serializedItem);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            weightedSelf = this;

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

                weightedSelf.Initialize();
            }

            base.Awake();
        }

        /// <summary>
        ///     TOOD.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost && activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncAllItemValuesServerRpc);
            }
        }

        /* /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            for (int i = 0; i < PatchedContent.ExtendedItems.Count; i++)
            {
                if (tagsToSpawn == null || tagsToSpawn.Length == 0)
                {
                    break;
                }

                ExtendedItem extendedItem = PatchedContent.ExtendedItems[i];

                int tagsFound = 0;

                for (int j = 0; j < tagsToSpawn.Length; j++)
                {
                    ContentTag? tagToSpawn = tagsToSpawn[j];

                    if (tagToSpawn == null || tagToSpawn.contentTagName.IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (extendedItem.ContentTags.Find(tagToSpawn.CompareTag) != null)
                    {
                        tagsFound++;

                        if (!requireAllTags)
                        {
                            break;
                        }
                    }
                }

                if ((requireAllTags && tagsFound == tagsToSpawn.Length) || (!requireAllTags && tagsFound > 0))
                {
                    ItemPool.Add(extendedItem.Item);
                }
            }
        } */

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc]
        public void SyncAllItemValuesServerRpc()
        {
            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncAllItemValuesServerRpc);
            }

            for (int i = 0; i < PrefabInstances.Count; i++)
            {
                if (PrefabInstances[i] != null)
                {
                    SyncItemValuesClientRpc(PrefabInstances[i], ItemsToSync[i]);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="syncedItem"></param>
        [ClientRpc]
        public void SyncItemValuesClientRpc(NetworkBehaviourReference itemReference, ItemInfo syncedItem)
        {
            _ = StartCoroutine(SyncItemValuesOnSpawn(itemReference, syncedItem));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="syncedItem"></param>
        /// <returns></returns>
        private IEnumerator SyncItemValuesOnSpawn(NetworkBehaviourReference itemReference, ItemInfo syncedItem)
        {
            GrabbableObject item;

            float startTime = Time.realtimeSinceStartup;
            while (!itemReference.TryGet(out item) && Time.realtimeSinceStartup - startTime < 8f)
            {
                yield return new WaitForSeconds(0.03f); // TODO: Replace with better method.
            }

            yield return new WaitForEndOfFrame();

            if (item == null)
            {
                yield break;
            }

            item.fallTime = 1.0f;
            item.hasHitGround = true;
            item.reachedFloorTarget = true;

            item.transform.SetPositionAndRotation(syncedItem.transformInfo.position, syncedItem.transformInfo.rotation);

            item.startFallingPosition = syncedItem.transformInfo.position;
            item.targetFloorPosition = syncedItem.transformInfo.position;

            item.SetScrapValue(syncedItem.scrapValue);

            if (syncedItem.meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
            {
                itemFilter.sharedMesh = item.itemProperties.meshVariants[syncedItem.meshVariant];
            }
            if (syncedItem.materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
            {
                itemRenderer.sharedMaterial = item.itemProperties.materialVariants[syncedItem.materialVariant];
            }

            if (fallToGround)
            {
                item.FallToGround(randomizePosition, true);
            }

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.totalScrapValueInLevel += syncedItem.scrapValue;
            }
        }
    }
}