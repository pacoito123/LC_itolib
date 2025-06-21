using itolib.Behaviours.Networking;
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
        public override void PerformSpawn()
        {
            if (!NetworkManager.Singleton.IsHost || prefabToSpawn == null)
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
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void SpawnPrefab(Transform spawnLocation)
        {
            if (prefabToSpawn == null || !spawnLocation.gameObject.activeInHierarchy)
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