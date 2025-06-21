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
        public Vector3 position;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        ///     TODO.
        /// </summary>
        public int meshVariant;

        /// <summary>
        ///     TODO.
        /// </summary>
        public int materialVariant;

        /// <summary>
        ///     TODO.
        /// </summary>
        public int scrapValue;

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
        public List<Item?> itemsToSpawn = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<ContentTag?> tagsToSpawn = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int minItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int maxItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustivePool;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustiveLocations;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Item Properties")]
        [Tooltip("")]
        [Min(-1)]
        public int overrideMinValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        public int overrideMaxValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool applyScrapMultiplier = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool allowMeshVariants = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool allowMaterialVariants = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Position")]
        [Tooltip("")]
        public bool fallToGround = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool randomizePosition;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Other")]
        [Tooltip("")]
        public bool skipInactive = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool seededRandom = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("DEPRECATED")]
        [Obsolete("Use 'itemsToSpawn' instead.")]
        public Item? itemToSpawn;

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

            if (StartOfRound.Instance != null)
            {
                Random ??= new(StartOfRound.Instance.randomMapSeed + 44);
            }
            else
            {
                Random ??= new();
            }

            if (spawnLocations.Count > 0)
            {
                if (skipInactive)
                {
                    _ = spawnLocations.RemoveAll(spawnLocation => spawnLocation == null || !spawnLocation.gameObject.activeInHierarchy);
                }

                if (spawnLocations.Count == 0)
                {
                    return;
                }

                int itemsAmount = minItems < maxItems ? Random.Next(minItems, maxItems) : minItems;

                if (itemsAmount > 0)
                {
                    for (int i = 0; i < itemsAmount; i++)
                    {
                        int locationIndex = Random.Next(0, spawnLocations.Count);
                        SpawnItem(spawnLocations[locationIndex]);

                        if (exhaustiveLocations)
                        {
                            spawnLocations.RemoveAt(locationIndex);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < spawnLocations.Count; i++)
                    {
                        SpawnItem(spawnLocations[i]);
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

            if (itemToSpawn == null || (skipInactive && !spawnLocation.gameObject.activeInHierarchy))
            {
                return;
            }

            GameObject itemPrefab = Instantiate(itemToSpawn.gameObject, spawnLocation.position, Quaternion.identity,
                RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null);

            if (itemPrefab.TryGetComponent(out GrabbableObject item) && item.itemProperties != null)
            {
                int scrapValue = Random.Next(overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                    overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue);
                if (applyScrapMultiplier && RoundManager.Instance != null)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                SyncedItem serializedItem = new()
                {
                    position = spawnLocation.position,
                    rotation = spawnLocation.rotation * Quaternion.Euler(item.itemProperties.restingRotation),
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
        public override void Start()
        {
            for (int i = 0; i < tagsToSpawn.Count; i++)
            {
                if (tagsToSpawn[i] != null)
                {
                    for (int j = 0; j < PatchedContent.ExtendedItems.Count; j++)
                    {
                        ExtendedItem extendedItem = PatchedContent.ExtendedItems[j];

                        if (extendedItem.ContentTags.Contains(tagsToSpawn[i])
                            && !ItemPool.Contains(extendedItem.Item))
                        {
                            ItemPool.Add(extendedItem.Item);
                        }
                    }
                }
            }

            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                Item? itemToSpawn = itemsToSpawn[i];

                if (itemToSpawn != null)
                {
                    if (itemToSpawn.spawnPrefab != null)
                    {
                        ItemPool.Add(itemToSpawn);
                    }
                    else
                    {
                        ExtendedItem? extendedItem = PatchedContent.ExtendedItems.Find(extendedItem =>
                            extendedItem.Item.name.CompareOrdinal(itemToSpawn.name));

                        if (extendedItem != null && !ItemPool.Contains(extendedItem.Item))
                        {
                            ItemPool.Add(extendedItem.Item);
                        }
                    }
                }
            }

            base.Start();

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncAllItemValuesServerRpc);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            Random = null!;

            if (activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncAllItemValuesServerRpc);
            }

            base.OnDestroy();
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