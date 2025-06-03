using itolib.Behaviours.Networking;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class PrefabSpawner : NetworkedSpawner
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
            if (!NetworkManager.Singleton.IsHost || PrefabToSpawn == null)
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
            if (PrefabToSpawn == null || !spawnLocation.gameObject.activeInHierarchy)
            {
                return;
            }

            PrefabInstances.Add(Instantiate(PrefabToSpawn.gameObject, spawnLocation.position, spawnLocation.rotation,
                parentTransform ?? RoundManager.Instance?.mapPropsContainer.transform).GetComponent<NetworkObject>());
        }
    }
}