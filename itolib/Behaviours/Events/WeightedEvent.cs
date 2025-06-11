using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     TODO.
    /// </summary>
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

    /// <summary>
    ///     TODO.
    /// </summary>
    public class WeightedEvent : NetworkBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random SeededRandom { get; internal set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<int>? AllWeightsCumulative { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int TotalWeight { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Weighted Event")]
        [Tooltip("")]
        public List<EventWithWeight> eventEntries = [];

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
        public int minRolls = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int maxRolls = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustiveRolls;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool seededRandom;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (seededRandom)
            {
                SeededRandom ??= new(StartOfRound.Instance.randomMapSeed + 66);
            }

            List<int> propWeights = [.. eventEntries.Select(prop => prop.weight)];
            AllWeightsCumulative = new(propWeights.Count);

            for (int i = 0; i < propWeights.Count; i++)
            {
                TotalWeight += propWeights[i];
                AllWeightsCumulative.Add(TotalWeight);
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
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(RollFromServer);
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
                SeededRandom = null!;
            }

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(RollFromServer);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(RollFromServer);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(RollFromServer);
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
        public void RollFromServer()
        {
            if (!IsHost)
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
            if (AllWeightsCumulative == null || AllWeightsCumulative.Count == 0 || !player.IsLocalClient())
            {
                return;
            }

            int rollsToPerform = (minRolls < maxRolls) ? (seededRandom ? SeededRandom.Next(minRolls, maxRolls + 1)
                : UnityEngine.Random.RandomRangeInt(minRolls, maxRolls + 1)) : minRolls;

            for (int i = 0; i < rollsToPerform; i++)
            {
                if (AllWeightsCumulative.Count == 0)
                {
                    break;
                }

                int randomWeight = seededRandom ? SeededRandom.Next(1, TotalWeight + 1) : UnityEngine.Random.RandomRangeInt(1,
                    TotalWeight + 1), weightIndex = AllWeightsCumulative.FindIndex(weight => randomWeight <= weight);

                if (weightIndex < 0 || weightIndex >= eventEntries.Count)
                {
                    return;
                }

                InvokeEventLocal(weightIndex);

                if (IsSpawned)
                {
                    InvokeEventServerRpc(player, weightIndex);
                }
            }
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
            eventEntries[weightIndex].onEvent.Invoke();

            if (!exhaustiveRolls || AllWeightsCumulative == null)
            {
                return;
            }

            int weightRemoved = eventEntries[weightIndex].weight;
            TotalWeight -= weightRemoved;

            for (int i = weightIndex + 1; i < AllWeightsCumulative.Count; i++)
            {
                if (AllWeightsCumulative[i] > 0)
                {
                    AllWeightsCumulative[i] -= weightRemoved;
                }
            }

            AllWeightsCumulative[weightIndex] = 0;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDungeonComplete(Dungeon _)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                RollFromServer();
            }
        }
    }
}