using DunGen;
using itolib.Enums;
using LethalLevelLoader;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public abstract class NetworkedSpawner<T> : NetworkBehaviour, IDungeonCompleteReceiver where T : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<T?> PrefabInstances { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Network Spawner")]
        [Tooltip("")]
        public List<Transform?>? spawnLocations;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustiveLocations;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<BoxCollider?>? spawnAreas = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustiveAreas;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.DungeonComplete;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool destroySpawner = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool destroyWithScene = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public abstract NetworkObject? GetPrefabToSpawn();

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void PerformSpawn()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            for (int i = 0; i < PrefabInstances.Count; i++)
            {
                T? prefabInstance = PrefabInstances[i];
                if (prefabInstance != null && prefabInstance.TryGetComponent(out NetworkObject prefabNetworkObject)
                    && !prefabNetworkObject.IsSpawned)
                {
                    prefabNetworkObject.Spawn(destroyWithScene);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
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
        public override void OnNetworkDespawn()
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

            base.OnNetworkDespawn();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                PerformSpawn();
            }
        }
    }
}