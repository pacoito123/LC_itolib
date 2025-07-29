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
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; private set; } = null!;

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
        [SerializeField] private Transform? parentTransform;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool useLocalRotation;

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
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            int spawnAmount = seededRandom ? Random.Next(minSpawns, maxSpawns + 1)
                : UnityEngine.Random.RandomRangeInt(minSpawns, maxSpawns + 1);

            if (spawnAmount == 0)
            {
                return;
            }

            _ = spawnLocations?.RemoveAll(spawnLocation => spawnLocation == null || (skipInactive && !spawnLocation.gameObject.activeInHierarchy));
            _ = spawnAreas?.RemoveAll(spawnArea => spawnArea == null || (skipInactive && !spawnArea.gameObject.activeInHierarchy));

            if (spawnLocations?.Count > 0)
            {
                if (spawnAmount == -1)
                {
                    spawnAmount = spawnLocations.Count;
                }

                for (int i = 0; i < spawnAmount && spawnLocations.Count > 0; i++)
                {
                    int locationIndex = seededRandom ? Random.Next(0, spawnLocations.Count)
                        : UnityEngine.Random.RandomRangeInt(0, spawnLocations.Count);

                    SpawnPrefab(spawnLocations[locationIndex]!);

                    if (exhaustiveLocations)
                    {
                        spawnLocations.RemoveAt(locationIndex);
                    }
                }
            }
            else if (spawnAreas?.Count > 0)
            {
                if (spawnAmount == -1)
                {
                    spawnAmount = spawnAreas.Count;
                }

                for (int i = 0; i < spawnAmount && spawnAreas.Count > 0; i++)
                {
                    int areaIndex = seededRandom ? Random.Next(0, spawnAreas.Count)
                        : UnityEngine.Random.RandomRangeInt(0, spawnAreas.Count);

                    SpawnPrefab(spawnAreas[areaIndex]!);

                    if (exhaustiveAreas)
                    {
                        spawnAreas.RemoveAt(areaIndex);
                    }
                }
            }
            else if (!skipInactive)
            {
                SpawnPrefab(transform);
            }

            base.PerformSpawn();

            /* if (destroySpawner && TryGetComponent(out NetworkObject networkObject) && networkObject.IsSpawned)
            {
                networkObject.Despawn(destroy: true);
            } */
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void SpawnPrefab(Transform spawnLocation)
        {
            NetworkObject? prefabToSpawn = GetPrefabToSpawn();

            if (prefabToSpawn != null)
            {
                SpawnPrefab(prefabToSpawn, spawnLocation.position, !useLocalRotation ?
                    spawnLocation.rotation : spawnLocation.localRotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnArea"></param>
        private void SpawnPrefab(BoxCollider spawnArea)
        {
            NetworkObject? prefabToSpawn = GetPrefabToSpawn();

            if (prefabToSpawn != null)
            {
                Vector3 extents = spawnArea.size * 0.5f;
                Vector3 point = seededRandom ?
                    new(((float)Random.NextDouble() * extents.x * 2) - extents.x,
                        ((float)Random.NextDouble() * extents.y * 2) - extents.y,
                        ((float)Random.NextDouble() * extents.z * 2) - extents.z) :
                    new((UnityEngine.Random.Range(0.0f, 1.0f) * extents.x * 2) - extents.x,
                        (UnityEngine.Random.Range(0.0f, 1.0f) * extents.y * 2) - extents.y,
                        (UnityEngine.Random.Range(0.0f, 1.0f) * extents.z * 2) - extents.z);

                Transform spawnTransform = spawnArea.transform;
                Vector3 spawnPosition = spawnTransform.TransformPoint(point + spawnArea.center); // TODO: Maybe find point in NavMesh instead?

                SpawnPrefab(prefabToSpawn, spawnPosition, !useLocalRotation ? spawnTransform.rotation : spawnTransform.localRotation);
            }
        }

        private void SpawnPrefab(NetworkObject prefabToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject prefab = Instantiate(prefabToSpawn.gameObject, spawnPosition, spawnRotation,
                (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null) ?
                    RoundManager.Instance.mapPropsContainer.transform : null);

            if (parentTransform != null)
            {
                prefab.transform.SetParent(parentTransform);
            }

            PrefabInstances.Add(prefab.GetComponent<NetworkObject>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (seededRandom)
            {
                Random ??= (StartOfRound.Instance != null) ? new(StartOfRound.Instance.randomMapSeed + 44) : new();
            }

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            if (seededRandom)
            {
                Random = null!; // TODO: Handle some other way.
            }

            base.OnDestroy();
        }
    }
}