using itolib.Behaviours.Networking;
using itolib.Enums;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class PrefabSpawner : NetworkedSpawner<NetworkObject>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Prefab Spawner")]
        [Tooltip("")]
        public NetworkObject? prefabToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Transform? parentTransform;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return prefabToSpawn;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            if (!NetworkManager.Singleton.IsHost || IsSpawned)
            {
                return;
            }

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(PerformSpawn);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(PerformSpawn);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(PerformSpawn);
                    }
                    break;
                case ActivationTime.Immediate:
                    PerformSpawn();
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(PerformSpawn);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(PerformSpawn);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(PerformSpawn);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PerformSpawn()
        {
            if (!NetworkManager.Singleton.IsHost || prefabToSpawn == null || spawnLocations == null)
            {
                return;
            }

            if (spawnLocations.Count == 0)
            {
                SpawnPrefab(transform);
            }
            else
            {
                for (int i = 0; i < spawnLocations.Count; i++)
                {
                    SpawnPrefab(spawnLocations[i]);
                }
            }

            base.PerformSpawn();

            if (destroySpawner && TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Despawn(destroy: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void SpawnPrefab(Transform? spawnLocation)
        {
            if (prefabToSpawn == null || spawnLocation == null || !spawnLocation.gameObject.activeInHierarchy)
            {
                return;
            }

            GameObject prefab = Instantiate(prefabToSpawn.gameObject, spawnLocation.position, spawnLocation.rotation,
                (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null) ?
                    RoundManager.Instance.mapPropsContainer.transform : null);

            if (parentTransform != null)
            {
                prefab.transform.SetParent(parentTransform);
            }

            PrefabInstances.Add(prefab.GetComponent<NetworkObject>());
        }
    }
}