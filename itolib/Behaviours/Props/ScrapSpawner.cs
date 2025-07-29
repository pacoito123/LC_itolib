using itolib.Behaviours.Networking;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
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
    public struct SyncedItem : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Synced Item")]
        [Tooltip("")]
        public Vector3 position = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Quaternion rotation = Quaternion.identity;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int scrapValue = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int meshVariant = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int materialVariant = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        public SyncedItem() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref scrapValue);
            serializer.SerializeValue(ref meshVariant);
            serializer.SerializeValue(ref materialVariant);
        }
    }

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
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<Item> ItemPool { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<SyncedItem> ItemsToSync { get; private set; } = [];

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
        [Header("Position & Rotation")]
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
        [SerializeField] private bool skipInactive = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool seededRandom = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool respectSingleItemDay;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("== OBSOLETE ==")]
        [Tooltip("")]
        [Min(-1)]
        [Obsolete("Use minSpawns instead.")]
        [SerializeField] private int minItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [Obsolete("Use maxSpawns instead.")]
        [SerializeField] private int maxItems;

        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>IWeightedScript</c>, to avoid having to cast. 
        /// </summary>
        private IWeightedScript<ScrapWeightEntry> weightedSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            /* if (respectSingleItemDay)
            {
                // TODO: Spawn da item
            } */

            if (WeightedEntries?.Length > 0 && weightedSelf.TryObtainRandomEntry(out ScrapWeightEntry entry, seededRandom ? Random : null))
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
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            int itemsAmount = seededRandom ? Random.Next(minSpawns, maxSpawns + 1)
                : UnityEngine.Random.RandomRangeInt(minSpawns, maxSpawns + 1);

            if (itemsAmount == 0)
            {
                return;
            }

            _ = spawnLocations?.RemoveAll(spawnLocation => spawnLocation == null || (skipInactive && !spawnLocation.gameObject.activeInHierarchy));
            _ = spawnAreas?.RemoveAll(spawnArea => spawnArea == null || (skipInactive && !spawnArea.gameObject.activeInHierarchy));

            if (spawnLocations?.Count > 0)
            {
                if (itemsAmount == -1)
                {
                    itemsAmount = spawnLocations.Count;
                }

                for (int i = 0; i < itemsAmount && spawnLocations.Count > 0; i++)
                {
                    int locationIndex = seededRandom ? Random.Next(0, spawnLocations.Count)
                        : UnityEngine.Random.RandomRangeInt(0, spawnLocations.Count);

                    SpawnItem(spawnLocations[locationIndex]!);

                    if (exhaustiveLocations)
                    {
                        spawnLocations.RemoveAt(locationIndex);
                    }
                }
            }
            else if (spawnAreas?.Count > 0)
            {
                if (itemsAmount == -1)
                {
                    itemsAmount = spawnAreas.Count;
                }

                for (int i = 0; i < itemsAmount && spawnAreas.Count > 0; i++)
                {
                    int areaIndex = seededRandom ? Random.Next(0, spawnAreas.Count)
                        : UnityEngine.Random.RandomRangeInt(0, spawnAreas.Count);

                    SpawnItem(spawnAreas[areaIndex]!);

                    if (exhaustiveAreas)
                    {
                        spawnAreas.RemoveAt(areaIndex);
                    }
                }
            }
            else if (!skipInactive)
            {
                SpawnItem(transform);
            }

            base.PerformSpawn();

            if (activationTime is not ActivationTime.ScrapSpawn or ActivationTime.HazardSpawn)
            {
                SyncAllItemValuesServerRpc();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void SpawnItem(Transform spawnLocation)
        {
            NetworkObject? itemToSpawn = GetPrefabToSpawn();

            if (itemToSpawn != null)
            {
                SpawnItem(itemToSpawn, spawnLocation.position, spawnLocation.rotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnArea"></param>
        private void SpawnItem(BoxCollider spawnArea)
        {
            NetworkObject? itemToSpawn = GetPrefabToSpawn();

            if (itemToSpawn != null)
            {
                Vector3 extents = spawnArea.size * 0.5f;
                Vector3 point = new(((float)Random.NextDouble() * extents.x * 2) - extents.x, // TODO: Seeded check
                    ((float)Random.NextDouble() * extents.y * 2) - extents.y,
                    ((float)Random.NextDouble() * extents.z * 2) - extents.z);

                Vector3 spawnPosition = spawnArea.transform.TransformPoint(point + spawnArea.center); // TODO: Maybe find point in NavMesh instead?

                SpawnItem(itemToSpawn, spawnPosition, Quaternion.identity);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemToSpawn"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        private void SpawnItem(NetworkObject itemToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject itemObj = Instantiate(itemToSpawn.gameObject, spawnPosition, Quaternion.identity,
                RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null);

            if (itemObj.TryGetComponent(out GrabbableObject item) && item.itemProperties != null)
            {
                int scrapValue = Random.Next(overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue, // TODO: Seeded check.
                    overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue);

                if (applyScrapMultiplier && RoundManager.Instance != null)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                if (applyRestingRotation)
                {
                    spawnRotation *= Quaternion.Euler(item.itemProperties.restingRotation);
                }

                SyncedItem serializedItem = new()
                {
                    position = spawnPosition,
                    rotation = spawnRotation,
                    meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                        ? Random.Next(0, item.itemProperties.meshVariants.Length) : -1, // TODO: Seeded check.
                    materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                        ? Random.Next(0, item.itemProperties.materialVariants.Length) : -1, // TODO: Seeded check.
                    scrapValue = scrapValue
                };

                PrefabInstances.Add(item);
                ItemsToSync.Add(serializedItem);
            }
        }

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        protected override void Awake()
        {
            weightedSelf = this;

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            /* if (seededRandom)
            {
                Random ??= (StartOfRound.Instance != null) ? new(StartOfRound.Instance.randomMapSeed + 44) : new();
            } */
            Random ??= (StartOfRound.Instance != null) ? new(StartOfRound.Instance.randomMapSeed + 44) : new();

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
        public override void OnDestroy()
        {
            if (seededRandom)
            {
                Random = null!; // TODO: Handle differently.
            }

            base.OnDestroy();
        }

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
        public void SyncItemValuesClientRpc(NetworkBehaviourReference itemReference, SyncedItem syncedItem)
        {
            _ = StartCoroutine(SyncItemValuesOnSpawn(itemReference, syncedItem));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="syncedItem"></param>
        /// <returns></returns>
        private IEnumerator SyncItemValuesOnSpawn(NetworkBehaviourReference itemReference, SyncedItem syncedItem)
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

            item.transform.SetPositionAndRotation(syncedItem.position, syncedItem.rotation);

            item.startFallingPosition = syncedItem.position;
            item.targetFloorPosition = syncedItem.position;

            item.SetScrapValue(syncedItem.scrapValue);

            if (syncedItem.meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
            {
                itemFilter.sharedMesh = item.itemProperties.meshVariants[syncedItem.meshVariant]; // TODO: Test with sharedMesh
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