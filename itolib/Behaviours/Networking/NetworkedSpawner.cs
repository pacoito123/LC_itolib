using DunGen;
using itolib.Enums;
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
            if (!IsHost)
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

            if (destroySpawner) // TODO: Move elsewhere.
            {
                Destroy(gameObject);
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
                enabled = false;
                return;
            }

            PrefabToSpawn ??= GetPrefabToSpawn();

            if (activationTime == ActivationTime.Immediate)
            {
                PerformSpawn();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void OnEnable()
        {
            if (activationTime == ActivationTime.StartOfRound)
            {
                StartOfRound.Instance?.StartNewRoundEvent.AddListener(PerformSpawn);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void OnDisable()
        {
            if (activationTime == ActivationTime.StartOfRound)
            {
                StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(PerformSpawn);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime == ActivationTime.DungeonComplete)
            {
                PerformSpawn();
            }
        }
    }
}