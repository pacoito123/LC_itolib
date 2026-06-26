using DunGen;
using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     Represents a single entry with weights to be used for weighted event selection.
    /// </summary>
    [Serializable]
    public struct WeightedEventEntry() : IWeightedEntry
    {
        /// <summary>
        ///     Event invoked whenever this specific entry is used.
        /// </summary>
        [Header("Weighted Event Entry")]
        [Tooltip("Event invoked whenever this specific entry is used.")]
        public UnityEvent onEvent = new();

        /// <inheritdoc/>
        [field: Tooltip("Weight value for this specific entry.")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; }

        /// <inheritdoc/>
        [field: Tooltip("Weight modifiers to apply whenever this specific entry is used.")]
        [field: SerializeField] public WeightedModifier[]? WeightedModifiers { get; set; }

        /// <inheritdoc/>
        [field: Tooltip("Whether this specific entry can be used more than once or not.")]
        [field: SerializeField] public bool SingleUse { get; set; }
    }

    /// <summary>
    ///     Represents an event with weighted selection capabilities.
    /// </summary>
    public class WeightedEvent : NetworkBehaviour, IActivationScript, ISeededScript<WeightedEvent>, IWeightedScript<WeightedEventEntry>
    {
        /// <inheritdoc/>
        public IActivationScript ActivationSelf { get; }

        /// <inheritdoc/>
        public ISeededScript<WeightedEvent> SeededSelf { get; }

        /// <inheritdoc/>
        public IWeightedScript<WeightedEventEntry> WeightedSelf { get; }

        /// <inheritdoc/>
        public int[]? CurrentWeights { get; set; }

        /// <inheritdoc/>
        public int TotalWeight { get; set; }

        /// <inheritdoc/>
        public bool InitializedWeights { get; set; }

        /// <inheritdoc/>
        public bool PerformedActivation { get; set; }

        /// <inheritdoc/>
        [field: Header("Weighted Event")]
        [field: Tooltip("List of weighted entries of type WeightedEventEntry.")]
        [field: SerializeField] public WeightedEventEntry[]? WeightedEntries { get; set; }

        /// <inheritdoc/>
        [field: Tooltip("Desired activation time for the initial weighted roll.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Manual;

        /// <summary>
        ///     Minimum number of rolls to perform per call.
        /// </summary>
        [Tooltip("Minimum number of rolls to perform per call.")]
        [Min(0)]
        [SerializeField] private int minRolls = 1;

        /// <summary>
        ///     Maximum number of rolls to perform per call.
        /// </summary>
        [Tooltip("Maximum number of rolls to perform per call.")]
        [Min(0)]
        [SerializeField] private int maxRolls = 1;

        /// <summary>
        ///     Whether the random weighted event rolling should be seeded or not.
        /// </summary>
        [Tooltip("Whether the random weighted event rolling should be seeded or not.")]
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
        ///     Initialize weights for every defined <c>WeightedEventEntry</c>.
        /// </summary>
        private void Awake()
        {
            WeightedSelf.InitializeWeights();
        }

        /// <summary>
        ///     Subscribe to events for automatic activation.
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
        ///     Unsubscribe from any events that may have been subscribed to.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            base.OnDestroy();
        }

        /// <summary>
        ///     Add a single weighted entry of type <c>WeightedEventEntry</c>.
        /// </summary>
        /// <param name="entry">Entry of type <c>WeightedEventEntry</c> to add.</param>
        public void AddWeightEntry(WeightedEventEntry entry)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     Add multiple weighted entries of type <c>WeightedEventEntry></c>.
        /// </summary>
        /// <param name="entries">Entries of type <c>WeightedEventEntry</c> to add.</param>
        public void AddWeightEntries(WeightedEventEntry[] entries)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     Remove weights for the weighted entry of type <c>WeightedEventEntry</c> at the specified index.
        /// </summary>
        /// <remarks>Sets weights to <c>0</c> instead of actually removing them.</remarks>
        /// <param name="index">Index of the entry of type <c>WeightedEventEntry</c> to remove.</param>
        public void RemoveWeightEntry(int index)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     Perform weighted roll on the server.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can only be successfully called from the host.</remarks>
        public void RollFromServer()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            RollFromClient(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     Perform weighted roll on a specific client.
        /// </summary>
        /// <param name="player">Player calling the weighted roll.</param>
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

            int rollsToPerform = (minRolls >= maxRolls) ? minRolls
                : (seededRandom ? SeededSelf.GetSeededRandom().Next(minRolls, maxRolls + 1)
                : UnityEngine.Random.RandomRangeInt(minRolls, maxRolls + 1));

            for (int i = 0; i < rollsToPerform; i++)
            {
                if (WeightedSelf.TryObtainRandomEntry(out WeightedEventEntry entry, out int weightIndex, seededRandom
                    ? SeededSelf.GetSeededRandom() : null))
                {
                    entry.onEvent.Invoke();

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
        ///     Invoke weighted entry of type <c>WeightedEventEntry</c> at the specified index for all other clients.
        /// </summary>
        /// <param name="weightIndex">Index of the entry of type <c>WeightedEventEntry</c> to invoke.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        public void InvokeEventRpc(int weightIndex)
        {
            InvokeEventLocal(weightIndex);
        }

        /// <summary>
        ///     Invoke weighted entry of type <c>WeightedEventEntry</c> at the specified index for the local client.
        /// </summary>
        /// <param name="weightIndex">Index of the entry of type <c>WeightedEventEntry</c> to invoke.</param>
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