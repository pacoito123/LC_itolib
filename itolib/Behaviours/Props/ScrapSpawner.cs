using itolib.Behaviours.Networking;
using itolib.Enums;
using System;
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
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref meshVariant);
            serializer.SerializeValue(ref materialVariant);
            serializer.SerializeValue(ref scrapValue);
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class ScrapSpawner : NetworkedSpawner
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<SyncedItem> ItemsToSync { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Spawner")]
        [Tooltip("")]
        public Item? itemToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int overrideMinValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
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
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (PrefabToSpawn != null)
            {
                return PrefabToSpawn;
            }

            if (itemToSpawn?.spawnPrefab?.TryGetComponent(out NetworkObject itemNetworkObject) == true)
            {
                return itemNetworkObject;
            }
            else if (StartOfRound.Instance?.allItemsList.itemsList.Find(item => string.CompareOrdinal(item.name, itemToSpawn?.name) == 0)?.spawnPrefab
                .TryGetComponent(out NetworkObject blankReferenceNetworkObject) == true)
            {
                return blankReferenceNetworkObject;
            }

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PerformSpawn()
        {
            if (!IsHost || PrefabToSpawn == null)
            {
                return;
            }

            Random ??= new(StartOfRound.Instance.randomMapSeed + 44);

            if (spawnLocations.Count == 0)
            {
                SpawnItem(transform);
            }
            else
            {
                for (int i = 0; i < spawnLocations.Count; i++)
                {
                    SpawnItem(spawnLocations[i]);
                }
            }

            base.PerformSpawn();

            if (activationTime == ActivationTime.ScrapSpawn) // TODO: Start Coroutine if spawned manually.
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncAllItemValuesServerRpc);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        public void SpawnItem(Transform spawnLocation)
        {
            if (PrefabToSpawn == null || !spawnLocation.gameObject.activeInHierarchy)
            {
                return;
            }

            GameObject itemPrefab = Instantiate(PrefabToSpawn.gameObject, spawnLocation.position, Quaternion.identity,
                RoundManager.Instance.spawnedScrapContainer);

            if (itemPrefab.TryGetComponent(out NetworkObject itemNetworkObject)
                && itemPrefab.TryGetComponent(out GrabbableObject item))
            {
                item.fallTime = 0.0f;

                int scrapValue = Random.Next(overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                    overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue);
                if (applyScrapMultiplier)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                SyncedItem serializedItem = new()
                {
                    rotation = spawnLocation.rotation * Quaternion.Euler(item.itemProperties.restingRotation),
                    meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                        ? Random.Next(0, item.itemProperties.meshVariants.Length) : -1,
                    materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                        ? Random.Next(0, item.itemProperties.materialVariants.Length) : -1,
                    scrapValue = scrapValue
                };

                PrefabInstances.Add(itemNetworkObject);
                ItemsToSync.Add(serializedItem);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnEnable()
        {
            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    break;
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(PerformSpawn);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(PerformSpawn);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(PerformSpawn);
                    break;
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDisable()
        {
            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    break;
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.RemoveListener(PerformSpawn);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.RemoveListener(PerformSpawn);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(PerformSpawn);
                    break;
                case ActivationTime.Manual:
                default:
                    break;
            }

            Random = null!;
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
        public void SyncItemValuesClientRpc(NetworkObjectReference itemReference, SyncedItem syncedItem)
        {
            if (itemReference.TryGet(out NetworkObject itemNetworkObject)
                && itemNetworkObject.TryGetComponent(out GrabbableObject item))
            {
                item.transform.rotation = syncedItem.rotation;

                if (syncedItem.meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
                {
                    itemFilter.mesh = item.itemProperties.meshVariants[syncedItem.meshVariant]; // TODO: Test sharedMesh
                }
                if (syncedItem.materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
                {
                    itemRenderer.sharedMaterial = item.itemProperties.materialVariants[syncedItem.materialVariant];
                }

                item.SetScrapValue(syncedItem.scrapValue);
            }
        }
    }
}