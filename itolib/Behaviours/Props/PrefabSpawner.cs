using itolib.Behaviours.Networking;
using itolib.Extensions;
using itolib.ScriptableObjects;
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
        public NetworkList<NetworkObjectReference>? SyncedPrefabs { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Prefab Spawner")]
        [Tooltip("")]
        [SerializeField] private string networkPrefabName;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Transform? parentTransform;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Replace with the desired (registered) network prefab's file name.")]
        [SerializeField] private NetworkObject? prefabToSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            if (prefabToSpawn != null)
            {
                return prefabToSpawn;
            }

            if (ScriptableNetworkPrefab.TryGetPrefab(out NetworkObject registeredPrefab, networkPrefabName))
            {
                return registeredPrefab;
            }

            foreach (NetworkPrefab networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
            {
                if (networkPrefab.Prefab.name.CompareOrdinal(networkPrefabName))
                {
                    return networkPrefab.Prefab.GetComponent<NetworkObject>();
                }
            }

            return null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected override Transform? GetParentOverride()
        {
            return parentTransform != null ? parentTransform
                : (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null
                    ? RoundManager.Instance.mapPropsContainer.transform : null);
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
                SyncedPrefabs?.Add(spawnedPrefab);
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
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedPrefabs?.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<NetworkObjectReference>.EventType.Add)
                {
                    if (changeEvent.Value.TryGet(out NetworkObject spawnedPrefab))
                    {
                        OnSpawnPerformed.Invoke(spawnedPrefab);
                    }
                }
            };
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="prefabName"></param>
        public void SwitchPrefabToSpawn(string prefabName)
        {
            networkPrefabName = prefabName;
        }
    }
}