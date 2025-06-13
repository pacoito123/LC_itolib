using itolib.Behaviours.Networking;
using itolib.Enums;
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
        [Tooltip("")]
        public bool fallToGround = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool randomizePosition = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool skipInactive = true;

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

            if (itemToSpawn == null)
            {
                return null;
            }

            if (itemToSpawn.spawnPrefab != null && itemToSpawn.spawnPrefab.TryGetComponent(out NetworkObject itemNetworkObject))
            {
                return itemNetworkObject;
            }
            else
            {
                Item? item = StartOfRound.Instance != null ? StartOfRound.Instance.allItemsList.itemsList.Find(item =>
                    string.CompareOrdinal(item.name, itemToSpawn.name) == 0) : null;

                return (item != null && item.spawnPrefab != null && item.spawnPrefab.TryGetComponent(out NetworkObject blankReferenceNetworkObject))
                    ? blankReferenceNetworkObject : null;
            }
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

            if (StartOfRound.Instance != null)
            {
                Random ??= new(StartOfRound.Instance.randomMapSeed + 44);
            }
            else
            {
                Random ??= new();
            }

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
            if (PrefabToSpawn == null || (skipInactive && !spawnLocation.gameObject.activeInHierarchy))
            {
                return;
            }

            GameObject itemPrefab = Instantiate(PrefabToSpawn.gameObject, spawnLocation.position, Quaternion.identity,
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
                yield return new WaitForSeconds(0.03f);
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