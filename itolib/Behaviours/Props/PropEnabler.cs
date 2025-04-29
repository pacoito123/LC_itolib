using DunGen;
using itolib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct PropWithWeight
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public GameObject? prop;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int weight;
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class PropEnabler : NetworkBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<int>? PropWeightsCumulative { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int TotalWeight { get; private set; } = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool HasRun { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Delayed Local Props")]
        [Tooltip("")]
        public List<PropWithWeight> propsToEnable = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int minProps;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxProps;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<GameObject>? onEnableProp;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPropsFinish;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                Random ??= new(StartOfRound.Instance.randomMapSeed + 55);

                List<int> propWeights = [.. propsToEnable.Select(prop => prop.weight)];
                PropWeightsCumulative = new(propWeights.Count);

                for (int i = 0; i < propWeights.Count; i++)
                {
                    TotalWeight += propWeights[i];
                    PropWeightsCumulative.Add(TotalWeight);
                }

                switch (activationTime)
                {
                    case ActivationTime.Immediate:
                        EnablePropsServerRpc();
                        break;
                    case ActivationTime.ScrapSpawn:
                        LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(EnablePropsServerRpc);
                        break;
                    case ActivationTime.HazardSpawn:
                        LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(EnablePropsServerRpc);
                        break;
                    case ActivationTime.StartOfRound:
                        StartOfRound.Instance?.StartNewRoundEvent.AddListener(EnablePropsServerRpc);
                        break;
                    case ActivationTime.DungeonComplete:
                    case ActivationTime.Manual:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            Random = null!;

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.RemoveListener(EnablePropsServerRpc);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.RemoveListener(EnablePropsServerRpc);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(EnablePropsServerRpc);
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
        [ServerRpc(RequireOwnership = false)]
        public void EnablePropsServerRpc()
        {
            if (HasRun || PropWeightsCumulative == null || PropWeightsCumulative.Count == 0)
            {
                return;
            }

            int numProps = (minProps < maxProps) ? Random.Next(minProps, maxProps + 1) : minProps;

            for (int i = 0; i < numProps; i++)
            {
                int randomWeight = Random.Next(1, TotalWeight + 1);
                int propIndex = PropWeightsCumulative.FindIndex(weight => randomWeight <= weight);

                EnablePropClientRpc(propIndex);

                int weightRemoved = propsToEnable[propIndex].weight;
                TotalWeight -= weightRemoved;

                for (int j = propIndex + 1; j < PropWeightsCumulative.Count; j++)
                {
                    if (PropWeightsCumulative[j] > 0)
                    {
                        PropWeightsCumulative[j] -= weightRemoved;
                    }
                }

                PropWeightsCumulative[propIndex] = 0;
            }

            HasRun = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void EnablePropClientRpc(int index)
        {
            propsToEnable[index].prop?.SetActive(true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime == ActivationTime.DungeonComplete)
            {
                EnablePropsServerRpc();
            }
        }
    }
}