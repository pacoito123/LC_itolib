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
using UnityEngine.Serialization;

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
    public class WeightedEvent : NetworkBehaviour, IActivationScript, ISeededScript<WeightedEvent>, IWeightedScript<WeightedEventEntry>
    {
        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>IActivationScript</c>, to avoid having to cast. 
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>WeightedEvent</c> as an <c>ISeededScript</c>, to avoid having to cast. 
        /// </summary>
        public ISeededScript<WeightedEvent> SeededSelf { get; }

        /// <summary>
        ///     Cached instance of <c>WeightedEvent</c> as an <c>IWeightedScript</c>, to avoid having to cast.
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
        ///     Desired <c>ActivationTime</c> for the overrides to be applied.
        /// </summary>
        [field: Tooltip("Desired activation time for the overrides to be applied.")]
        [field: FormerlySerializedAs("activationTime")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Manual;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the spawn to be performed.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the spawn to be performed. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.Manual;

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
        ///     Cache already-cast <c>ISeededScript</c> and <c>IWeightedScript</c> instances.
        /// </summary>
        private WeightedEvent()
        {
            ActivationSelf = this;
            SeededSelf = this;
            WeightedSelf = this;

            // Deprecated. Needed to stop an error being spammed, will be removed at some point.
            weightedSelf = this;
        }

        /// <summary>
        ///     Initialize weights for every <c>WeightedEventEntry</c>.
        /// </summary>
        private void Awake()
        {
            WeightedSelf.InitializeWeights();

            if (activationTime is not ActivationTime.Manual)
            {
                ActivationTime = activationTime;
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

            int rollsToPerform = seededRandom ? SeededSelf.GetSeededRandom().Next(minRolls, maxRolls + 1)
                : UnityEngine.Random.RandomRangeInt(minRolls, maxRolls + 1);

            for (int i = 0; i < rollsToPerform; i++)
            {
                if (CumulativeWeights == null || CumulativeWeights.Length == 0)
                {
                    break;
                }

                if (WeightedSelf.TryObtainRandomEntryIndex(out int weightIndex, seededRandom ? SeededSelf.GetSeededRandom() : null))
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