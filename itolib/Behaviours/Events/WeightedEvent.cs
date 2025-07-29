using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct WeightedEventEntry : IWeightedEntry
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Weighted Event Entry")]
        [Tooltip("")]
        public UnityEvent onEvent;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool SingleUse { get; set; }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class WeightedEvent : NetworkBehaviour, IWeightedScript<WeightedEventEntry>, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; internal set; } = null!;

        /// <summary>
        ///    TODO.
        /// </summary>
        public int[]? CumulativeWeights { get; set; }

        /// <summary>
        ///    TODO.
        /// </summary>
        public int TotalWeight { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Header("Weighted Event")]
        [field: Tooltip("")]
        [field: SerializeField] public WeightedEventEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.Manual;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int minRolls = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int maxRolls = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool seededRandom;

        /// <summary>
        ///     TODO.
        /// </summary>
        [SerializeField] private bool performedActivation;

        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>IWeightedScript</c>, to avoid having to cast. 
        /// </summary>
        public IWeightedScript<WeightedEventEntry> weightedSelf;

        /// <summary>
        ///     OBSOLETE
        /// </summary>
        [Space(5.0f)]
        [Header("== OBSOLETE ==")]
        [Obsolete("Use WeightedEventEntry list instead")]
        public List<EventWithWeight> eventEntries = [];

        /// <summary>
        ///     OBSOLETE
        /// </summary>
        [Obsolete("Use SingleUse fields instead")]
        public bool exhaustiveRolls;

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        private void Awake()
        {
            weightedSelf = this;

            if (seededRandom)
            {
                Random ??= (StartOfRound.Instance != null) ? new(StartOfRound.Instance.randomMapSeed + 66) : new();
            }

            weightedSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost || performedActivation)
            {
                return;
            }

            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    RollFromServer();
                    break;
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(RollFromServer);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(RollFromServer);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(RollFromServer);
                    }
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
            if (seededRandom)
            {
                Random = null!; // TODO: Handle some other way.
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(RollFromServer);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(RollFromServer);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(RollFromServer);
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
        public void RollFromServer()
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

            RollFromClient(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     TODO.
        /// </summary> 
        public void RollFromClient(PlayerControllerB player)
        {
            if (!player.IsLocalClient())
            {
                return;
            }

            int rollsToPerform = seededRandom ? Random.Next(minRolls, maxRolls + 1)
                : UnityEngine.Random.RandomRangeInt(minRolls, maxRolls + 1);

            for (int i = 0; i < rollsToPerform; i++)
            {
                if (CumulativeWeights == null || CumulativeWeights.Length == 0)
                {
                    break;
                }

                if (weightedSelf.TryObtainRandomEntryIndex(out int weightIndex, seededRandom ? Random : null))
                {
                    InvokeEventLocal(weightIndex);

                    if (IsSpawned)
                    {
                        InvokeEventServerRpc(player, weightIndex);
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveWeight(int index)
        {
            weightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="weightIndex"></param>
        [ServerRpc(RequireOwnership = false)]
        public void InvokeEventServerRpc(NetworkBehaviourReference playerReference, int weightIndex)
        {
            InvokeEventClientRpc(playerReference, weightIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="weightIndex"></param>
        [ClientRpc]
        public void InvokeEventClientRpc(NetworkBehaviourReference playerReference, int weightIndex)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                InvokeEventLocal(weightIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="weightIndex"></param>
        private void InvokeEventLocal(int weightIndex)
        {
            if (weightedSelf.TryObtainEntry(out WeightedEventEntry entry, weightIndex))
            {
                entry.onEvent.Invoke();
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
                RollFromServer();

                performedActivation = true;
            }
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    [Obsolete("Switch to WeightedEventEntry")]
    [Serializable]
    public struct EventWithWeight
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onEvent;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int weight;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool onlyOnce;
    }
}