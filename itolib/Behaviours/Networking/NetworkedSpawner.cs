using DunGen;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Structs;
using LethalLevelLoader;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public abstract class NetworkedSpawner<T> : NetworkBehaviour, ISeededScript<NetworkedSpawner<T>>, IDungeonCompleteReceiver where T : Behaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<T?> PrefabInstances { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Networked Spawner")]
        [Tooltip("")]
        [SerializeField] protected List<Transform?>? spawnLocations;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool exhaustiveLocations;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected List<BoxCollider?>? spawnAreas;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool exhaustiveAreas;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool skipInactive = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool useLocalRotation;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] protected int minSpawns = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] protected int maxSpawns = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Tooltip("")]
        [SerializeField] protected ActivationTime activationTime = ActivationTime.DungeonComplete;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool destroyWithScene = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [FormerlySerializedAs("seededRandom")]
        [SerializeField] protected bool isSeededRandom = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onSpawnPerformed")]
        [field: SerializeField] public UnityEvent<T> OnSpawnPerformed { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<int> OnSpawningFinish { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        protected bool performedActivation;

        /// <summary>
        ///     Cached instance of the current <c>NetworkedSpawner</c> as an <c>ISeededScript</c>, to avoid having to cast. 
        /// </summary>
        protected ISeededScript<NetworkedSpawner<T>> seededSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public abstract NetworkObject? GetPrefabToSpawn();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnedPrefab"></param>
        /// <param name="spawnLocation"></param>
        protected virtual void SpawnPerformed(T? spawnedPrefab, TransformInfo spawnLocation)
        {
            if (spawnedPrefab != null)
            {
                OnSpawnPerformed.Invoke(spawnedPrefab);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void PerformSpawn()
        {
            if (!performedActivation)
            {
                UnsubscribeFromEvents();

                performedActivation = true;
            }

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            int spawnAmount = isSeededRandom ? seededSelf.GetSeededRandom().Next(minSpawns, maxSpawns + 1)
                : Random.RandomRangeInt(minSpawns, maxSpawns + 1);

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
                    int locationIndex = isSeededRandom ? seededSelf.GetSeededRandom().Next(0, spawnLocations.Count)
                        : Random.RandomRangeInt(0, spawnLocations.Count);

                    PerformSpawn(spawnLocations[locationIndex]!);

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
                    int areaIndex = isSeededRandom ? seededSelf.GetSeededRandom().Next(0, spawnAreas.Count)
                        : Random.RandomRangeInt(0, spawnAreas.Count);

                    PerformSpawn(spawnAreas[areaIndex]!);

                    if (exhaustiveAreas)
                    {
                        spawnAreas.RemoveAt(areaIndex);
                    }
                }
            }
            else if (!skipInactive)
            {
                PerformSpawn(transform);
            }

            for (int i = 0; i < PrefabInstances.Count; i++)
            {
                T? prefabInstance = PrefabInstances[i];
                if (prefabInstance != null && prefabInstance.TryGetComponent(out NetworkObject prefabNetworkObject)
                    && !prefabNetworkObject.IsSpawned)
                {
                    prefabNetworkObject.Spawn(destroyWithScene);

                    SpawnPerformed(prefabInstance, new(prefabInstance.transform));
                }
            }

            OnSpawningFinish.Invoke(PrefabInstances.Count);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnLocation"></param>
        private void PerformSpawn(Transform spawnLocation)
        {
            NetworkObject? prefabToSpawn = GetPrefabToSpawn();

            if (prefabToSpawn != null)
            {
                PerformSpawn(prefabToSpawn, spawnLocation.position, !useLocalRotation
                    ? spawnLocation.rotation : spawnLocation.localRotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnArea"></param>
        private void PerformSpawn(BoxCollider spawnArea)
        {
            NetworkObject? prefabToSpawn = GetPrefabToSpawn();

            if (prefabToSpawn != null)
            {
                // TODO: Maybe find point in NavMesh instead?
                Vector3 point = spawnArea.GetPointWithin(isSeededRandom ? seededSelf.GetSeededRandom() : null);

                Transform spawnTransform = spawnArea.transform;
                Vector3 spawnPosition = spawnTransform.TransformPoint(point + spawnArea.center);

                PerformSpawn(prefabToSpawn, spawnPosition, !useLocalRotation ? spawnTransform.rotation : spawnTransform.localRotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="prefabToSpawn"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        protected virtual void PerformSpawn(NetworkObject prefabToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject prefabObj = Instantiate(prefabToSpawn.gameObject, spawnPosition, spawnRotation);

            if (prefabObj.TryGetComponent(out T prefab))
            {
                PrefabInstances.Add(prefab);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Awake()
        {
            seededSelf = this;

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    PerformSpawn();
                    break;
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
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     Unsubscribe to the event that may have been subscribed to, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void UnsubscribeFromEvents()
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
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (!performedActivation && activationTime is ActivationTime.DungeonComplete)
            {
                PerformSpawn();
            }
        }
    }
}