using itolib.Behaviours.Helpers;
using itolib.Enums;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Items
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemSpawner : NetworkedSpawner
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random? Random { get; internal set; }

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
            if (PrefabToSpawn == null)
            {
                return;
            }

            GameObject itemPrefab = Instantiate(PrefabToSpawn.gameObject, spawnLocation.position, spawnLocation.rotation,
                RoundManager.Instance.spawnedScrapContainer);

            if (itemPrefab.TryGetComponent(out NetworkObject itemNetworkObject)
                && itemPrefab.TryGetComponent(out GrabbableObject item))
            {
                itemPrefab.transform.rotation *= Quaternion.Euler(item.itemProperties.restingRotation);
                item.fallTime = 0.0f;

                item.scrapValue = Random!.Next(overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                    overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue);

                if (applyScrapMultiplier)
                {
                    item.scrapValue = (int)(item.scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                PrefabInstances.Add(itemNetworkObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnEnable()
        {
            if (activationTime == ActivationTime.ScrapSpawn)
            {
                LethalLevelLoader.DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(PerformSpawn);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDisable()
        {
            if (activationTime == ActivationTime.ScrapSpawn)
            {
                LethalLevelLoader.DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(PerformSpawn);
                StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(SyncAllItemValuesServerRpc);
            }

            Random = null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc]
        public void SyncAllItemValuesServerRpc()
        {
            for (int i = 0; i < PrefabInstances.Count; i++)
            {
                NetworkObject? itemNetworkObject = PrefabInstances[i];
                if (itemNetworkObject != null && itemNetworkObject.TryGetComponent(out GrabbableObject item))
                {
                    if (allowMeshVariants || allowMaterialVariants)
                    {
                        int meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                            ? Random!.Next(0, item.itemProperties.meshVariants.Length) : -1;

                        int materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                            ? Random!.Next(0, item.itemProperties.materialVariants.Length) : -1;

                        SyncItemValuesClientRpc(itemNetworkObject, item.scrapValue, meshVariant, materialVariant);

                        continue;
                    }

                    SyncItemValuesClientRpc(itemNetworkObject, item.scrapValue);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void SyncItemValuesClientRpc(NetworkObjectReference itemReference, int scrapValue, int meshVariant = -1, int materialVariant = -1)
        {
            if (itemReference.TryGet(out NetworkObject itemNetworkObject)
                && itemNetworkObject.TryGetComponent(out GrabbableObject item))
            {
                if (meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
                {
                    itemFilter.mesh = item.itemProperties.meshVariants[meshVariant];
                }

                if (materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
                {
                    itemRenderer.sharedMaterial = item.itemProperties.materialVariants[materialVariant];
                }

                item.SetScrapValue(scrapValue);
            }
        }
    }
}