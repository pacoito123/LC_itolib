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
    public abstract class NetworkedSpawner : NetworkBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<NetworkObject?> PrefabInstances { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public NetworkObject? PrefabToSpawn { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Network Spawner")]
        [Tooltip("")]
        public List<Transform> spawnLocations = [];

        /// <summary>
        ///     TODO.
        /// </summary>
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
                NetworkObject? prefabToSpawn = PrefabInstances[i];
                if (prefabToSpawn != null && !prefabToSpawn.IsSpawned)
                {
                    prefabToSpawn.Spawn(destroyWithScene);
                }
            }

            if (destroySpawner) // TODO: Move elsewhere?
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void Start()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (PrefabToSpawn == null)
            {
                PrefabToSpawn = GetPrefabToSpawn();
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
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(PerformSpawn);
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
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(PerformSpawn);
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