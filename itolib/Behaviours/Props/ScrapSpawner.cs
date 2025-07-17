using itolib.Behaviours.Networking;
using itolib.Enums;
using itolib.Extensions;
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
    public class ScrapSpawner : NetworkedSpawner<GrabbableObject>
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
        [Header("Item Spawner")]
        [Tooltip("")]
        [SerializeField] private Item[]? itemsToSpawn;

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
        [Min(-1)]
        [SerializeField] private int minItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int maxItems;

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
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (ItemPool.Count > 0)
            {
                int poolIndex = Random.Next(0, ItemPool.Count);
                Item randomItem = ItemPool[poolIndex];

                if (exhaustivePool)
                {
                    ItemPool.RemoveAt(poolIndex);
                }

                return randomItem.spawnPrefab.GetComponent<NetworkObject>();
            }

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PerformSpawn()
        {
            if (!IsHost)
            {
                return;
            }

            Random ??= (StartOfRound.Instance != null) ? new(StartOfRound.Instance.randomMapSeed + 44) : new();

            int itemsAmount = Random.Next(minItems, maxItems);
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
                    int locationIndex = Random.Next(0, spawnLocations.Count);
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
                    int areaIndex = Random.Next(0, spawnAreas.Count);
                    SpawnItem(spawnAreas[areaIndex]!);

                    if (exhaustiveAreas)
                    {
                        spawnAreas.RemoveAt(areaIndex);
                    }
                }
            }
            else
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
                Vector3 extents = spawnArea.size / 2.0f;
                Vector3 point = new(((float)Random.NextDouble() * extents.x * 2) - extents.x,
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
                int scrapValue = Random.Next(overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
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
                        ? Random.Next(0, item.itemProperties.meshVariants.Length) : -1,
                    materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                        ? Random.Next(0, item.itemProperties.materialVariants.Length) : -1,
                    scrapValue = scrapValue
                };

                PrefabInstances.Add(item);
                ItemsToSync.Add(serializedItem);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
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

            for (int i = 0; i < itemsToSpawn?.Length; i++)
            {
                Item? itemToSpawn = itemsToSpawn[i];

                if (itemToSpawn == null || ItemPool.Contains(itemToSpawn))
                {
                    continue;
                }

                if (itemToSpawn.spawnPrefab == null)
                {
                    ExtendedItem? extendedItem = PatchedContent.ExtendedItems.Find(extendedItem =>
                        extendedItem.Item.name.CompareOrdinal(itemToSpawn.name));

                    if (extendedItem != null && !ItemPool.Contains(extendedItem.Item))
                    {
                        ItemPool.Add(extendedItem.Item);
                    }
                }
                else
                {
                    ItemPool.Add(itemToSpawn);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost && activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncAllItemValuesServerRpc);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            Random = null!;

            if (activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncAllItemValuesServerRpc);
            }

            base.OnNetworkDespawn();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc]
        public void SyncAllItemValuesServerRpc()
        {
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
                itemFilter.mesh = item.itemProperties.meshVariants[syncedItem.meshVariant]; // TODO: Test with sharedMesh
            }
            if (syncedItem.materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
            {
                itemRenderer.sharedMaterial = item.itemProperties.materialVariants[syncedItem.materialVariant];
            }

            if (fallToGround)
            {
                item.FallToGround(randomizePosition, true);
            }
        }
    }
}