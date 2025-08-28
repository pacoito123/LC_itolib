using itolib.Behaviours.Networking;
using itolib.Structs;
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
        public NetworkList<NetworkObjectReference> SyncedPrefabs { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Prefab Spawner")]
        [Tooltip("")]
        [SerializeField] private NetworkObject? prefabToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Transform? parentTransform;

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
        /// <param name="spawnedPrefab"></param>
        /// <param name="spawnLocation"></param>
        protected override void SpawnPerformed(NetworkObject? spawnedPrefab, TransformInfo spawnLocation)
        {
            if (spawnedPrefab == null || !spawnedPrefab.IsSpawned)
            {
                return;
            }

            if (IsSpawned)
            {
                SyncedPrefabs.Add(spawnedPrefab);
            }
            else
            {
                base.SpawnPerformed(spawnedPrefab, spawnLocation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            SyncedPrefabs = new();

            base.Awake();
        }

        /// <summary>
        ///     TOOD.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedPrefabs.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type == NetworkListEvent<NetworkObjectReference>.EventType.Add)
                {
                    if (changeEvent.Value.TryGet(out NetworkObject spawnedPrefab))
                    {
                        if (parentTransform != null)
                        {
                            spawnedPrefab.transform.SetParent(parentTransform);
                        }
                        else if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null)
                        {
                            spawnedPrefab.transform.SetParent(RoundManager.Instance.mapPropsContainer.transform);
                        }

                        OnSpawnPerformed.Invoke(spawnedPrefab);
                    }
                }
            };
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="prefab"></param>
        public void SwitchPrefabToSpawn(NetworkObject? prefab)
        {
            if (prefab != null)
            {
                prefabToSpawn = prefab;
            }
        }
    }
}