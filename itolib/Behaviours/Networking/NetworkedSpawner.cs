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
    public abstract class NetworkedSpawner<T> : NetworkBehaviour, IActivationScript, ISeededScript<NetworkedSpawner<T>> where T : Behaviour
    {
        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>IActivationScript</c>, to avoid having to cast. 
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>NetworkedSpawner</c> as an <c>ISeededScript</c>, to avoid having to cast. 
        /// </summary>
        public ISeededScript<NetworkedSpawner<T>> SeededSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

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
        [SerializeField] protected bool includeChildren;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool includeInsideAINodes;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool includeOutsideAINodes;

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
        ///     Desired <c>ActivationTime</c> for the overrides to be applied.
        /// </summary>
        [field: Tooltip("Desired activation time for the overrides to be applied.")]
        [field: FormerlySerializedAs("activationTime")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the spawn to be performed.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the spawn to be performed. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

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
        /// <returns></returns>
        public abstract NetworkObject? GetPrefabToSpawn();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected virtual Transform? GetParentOverride()
        {
            return null;
        }

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
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            int spawnAmount = isSeededRandom ? SeededSelf.GetSeededRandom().Next(minSpawns, maxSpawns + 1)
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
                    int locationIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, spawnLocations.Count)
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
                    int areaIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, spawnAreas.Count)
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
                Vector3 point = spawnArea.GetPointWithin(isSeededRandom ? SeededSelf.GetSeededRandom() : null);

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
            GameObject prefabObj = Instantiate(prefabToSpawn.gameObject, spawnPosition, spawnRotation, GetParentOverride());

            if (prefabObj.TryGetComponent(out T prefab))
            {
                PrefabInstances.Add(prefab);
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            PerformSpawn();
        }

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> and <c>ISeededScript</c> instances.
        /// </summary>
        protected NetworkedSpawner()
        {
            ActivationSelf = this;
            SeededSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Awake()
        {
            if (activationTime is not ActivationTime.StartOfRound)
            {
                ActivationTime = activationTime;
            }

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (includeChildren && spawnLocations?.Count > 0)
            {
                HashSet<Transform> childLocations = [];

                for (int i = 0; i < spawnLocations.Count; i++)
                {
                    Transform? locationRoot = spawnLocations[i];

                    if (locationRoot == null || locationRoot.childCount == 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < locationRoot.childCount; j++)
                    {
                        Transform locationChild = locationRoot.GetChild(j);

                        if (!spawnLocations.Contains(locationChild))
                        {
                            _ = childLocations.Add(locationChild);
                        }
                    }
                }

                spawnLocations.AddRange(childLocations);
            }

            if (includeInsideAINodes || includeOutsideAINodes)
            {
                DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(AddAINodes);
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(AddAINodes);

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void AddAINodes()
        {
            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(AddAINodes);

            if (RoundManager.Instance != null)
            {
                if (includeInsideAINodes)
                {
                    GameObject[]? insideAINodes = RoundManager.Instance.insideAINodes;

                    for (int i = 0; i < insideAINodes?.Length; i++)
                    {
                        if (insideAINodes[i] != null)
                        {
                            spawnLocations ??= [];
                            spawnLocations.Add(insideAINodes[i].transform);
                        }
                    }
                }

                if (includeOutsideAINodes)
                {
                    GameObject[]? outsideAINodes = RoundManager.Instance.outsideAINodes;

                    for (int i = 0; i < outsideAINodes?.Length; i++)
                    {
                        if (outsideAINodes[i] != null)
                        {
                            spawnLocations ??= [];
                            spawnLocations.Add(outsideAINodes[i].transform);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}