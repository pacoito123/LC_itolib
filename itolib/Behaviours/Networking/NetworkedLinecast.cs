using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Represents an object that performs a <c>Linecast</c>, with event callbacks invoked if it failed to or successfully hit a <c>Collider</c>.
    /// </summary>
    public abstract class NetworkedLinecast : NetworkBehaviour, IActivationScript
    {
        /// <inheritdoc/>
        public IActivationScript ActivationSelf { get; }

        /// <inheritdoc/>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     Target end point for the <c>Linecast</c>. Can be considered both its maximum range and direction.
        /// </summary>
        [Header("Networked Linecast")]
        [Tooltip("Target end point for the Linecast. Can be considered both its maximum range and direction.")]
        [SerializeField] private Transform targetPoint = null!;

        /// <inheritdoc/>
        [field: Tooltip("Desired activation time for the Linecast to be performed.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.Immediate;

        /// <summary>
        ///     Position offset to apply to the start of the <c>Linecast</c>.
        /// </summary>
        [Space(3.0f)]
        [Tooltip("Position offset to apply to the start of the Linecast.")]
        [SerializeField] private Vector3 startOffset = Vector3.up;

        /// <summary>
        ///     <c>LayerMask</c> for all layers that can block the <c>Linecast</c>.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that can block the Linecast.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Callback invoked when the <c>Linecast</c> hits a <c>Collider</c>, with the distance between given as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when the Linecast hits a Collider, with the distance between given as parameter.")]
        [SerializeField] protected UnityEvent<float> onLinecastSuccess = new();

        /// <summary>
        ///     Callback invoked when the <c>Linecast</c> fails to hit a <c>Collider</c>.
        /// </summary>
        [Tooltip("Callback invoked when the Linecast fails to hit a Collider.")]
        [SerializeField] private UnityEvent onLinecastFail = new();

        /// <summary>
        ///     <c>RaycastHit</c> after performing the <c>Linecast</c>.
        /// </summary>
        protected RaycastHit hit;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        protected NetworkedLinecast()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     Set default <c>LayerMask</c> value for the <c>Linecast</c>.
        /// </summary>
        private void Reset()
        {
            // Set default line of sight layer mask.
            layerMask = LayerMask.GetMask("Default", "Room", "Foliage", "Colliders", "Terrain", "Vehicle");
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
        ///     Perform <c>Linecast</c> with the defined parameters and invoke corresponding event callback.
        /// </summary>
        public virtual void PerformCast()
        {
            if (targetPoint == null)
            {
                Plugin.Logger.LogError($"No valid target point defined for NetworkedLinecast '{name}'!");

                return;
            }

            // Perform Linecast between this object and the target end point to detect any intersecting Collider.
            if (!Physics.Linecast(transform.position + (transform.rotation * startOffset), targetPoint.position, out hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                // Invoke failed Linecast callback.
                onLinecastFail.Invoke();

                return;
            }

            // Invoke successful Linecast callback, with the distance between this object and the point hit as parameter.
            onLinecastSuccess.Invoke(hit.distance);
        }

        /// <inheritdoc/>
        public void PerformActivation(ActivationTime activationTime)
        {
            PerformCast();
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