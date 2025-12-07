using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
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
        public UnityEvent onEvent = new();

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

        /// <summary>
        ///     TODO.
        /// </summary>
        public WeightedEventEntry() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class WeightedEvent : NetworkBehaviour, IActivationScript, ISeededScript<WeightedEvent>, IWeightedScript<WeightedEventEntry>
    {
        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>ISeededScript</c>, to avoid having to cast.
        /// </summary>
        public ISeededScript<WeightedEvent> SeededSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        public IWeightedScript<WeightedEventEntry> WeightedSelf { get; }

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
        public bool InitializedWeights { get; set; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Header("Weighted Event")]
        [field: Tooltip("")]
        [field: SerializeField] public WeightedEventEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the initial weighted roll.
        /// </summary>
        [field: Tooltip("Desired activation time for the initial weighted roll.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Manual;

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
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [SuppressMessage("Style", "IDE0052:Remove unread private members", Justification = "Deprecated.")]
        private readonly IWeightedScript<WeightedEventEntry> weightedSelf;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c>, <c>ISeededScript</c>, and <c>IWeightedScript</c> instances.
        /// </summary>
        private WeightedEvent()
        {
            ActivationSelf = this;
            SeededSelf = this;
            WeightedSelf = this;

            // Deprecated. Needed to stop a (harmless) error from being spammed, will be removed at some point.
            weightedSelf = this;
        }

        /// <summary>
        ///     Initialize weights for every <c>WeightedEventEntry</c>.
        /// </summary>
        private void Awake()
        {
            WeightedSelf.InitializeWeights();
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

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        public void AddWeightEntry(WeightedEventEntry entry)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entries"></param>
        public void AddWeightEntries(WeightedEventEntry[] entries)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveWeightEntry(int index)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     TODO.
        /// </summary> 
        public void RollFromServer()
        {
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

            if (WeightedEntries == null || WeightedEntries.Length == 0)
            {
                return;
            }

            int rollsToPerform = seededRandom ? SeededSelf.GetSeededRandom().Next(minRolls, maxRolls + 1)
                : UnityEngine.Random.RandomRangeInt(minRolls, maxRolls + 1);

            for (int i = 0; i < rollsToPerform; i++)
            {
                if (WeightedSelf.TryObtainRandomEntryIndex(out int weightIndex, seededRandom
                    ? SeededSelf.GetSeededRandom() : null))
                {
                    InvokeEventLocal(weightIndex);

                    if (IsSpawned)
                    {
                        InvokeEventRpc(weightIndex);
                    }
                }
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            RollFromServer();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="weightIndex"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        public void InvokeEventRpc(int weightIndex)
        {
            InvokeEventLocal(weightIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="weightIndex"></param>
        private void InvokeEventLocal(int weightIndex)
        {
            if (WeightedSelf.TryObtainEntry(out WeightedEventEntry entry, weightIndex))
            {
                entry.onEvent.Invoke();
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