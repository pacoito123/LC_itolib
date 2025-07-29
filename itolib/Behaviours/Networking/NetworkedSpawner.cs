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
    public abstract class NetworkedSpawner<T> : NetworkBehaviour, IDungeonCompleteReceiver where T : Behaviour
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
        [Min(-1)]
        [SerializeField] protected int minSpawns;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] protected int maxSpawns;

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
        [SerializeField] protected bool destroySpawner;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool destroyWithScene = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected bool performedActivation;

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
            if (!performedActivation)
            {
                UnsubscribeFromEvents();

                performedActivation = true;
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
        protected virtual void Awake()
        {
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