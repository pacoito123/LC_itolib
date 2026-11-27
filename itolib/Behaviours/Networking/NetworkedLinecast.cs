using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using itolib.Structs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Represents an object that performs a <c>Linecast</c>, with event callbacks invoked if it failed to or successfully hit a <c>Collider</c>.
    /// </summary>
    /// <typeparam name="T">Type of object hit after a successful <c>Linecast</c>.</typeparam>
    public abstract class NetworkedLinecast<T> : NetworkBehaviour, IActivationScript where T : Object
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
        ///     Layers able to be hit by the <c>Linecast</c>.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("Layers able to be hit by the Linecast.")]
        [SerializeField] private LayerMask layerMask;

        /// <summary>
        ///     Callback invoked when the <c>Linecast</c> hits a <c>Collider</c>, with the distance between given as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when the Linecast hits a Collider, with the distance between given as parameter.")]
        [SerializeField] private UnityEvent<float> onLinecastSuccess = new();

        /// <summary>
        ///     Callback invoked when the <c>Linecast</c> fails to hit a <c>Collider</c>.
        /// </summary>
        [Tooltip("Callback invoked when the Linecast fails to hit a Collider.")]
        [SerializeField] private UnityEvent onLinecastFail = new();

        /// <summary>
        ///     Callback invoked when the <c>Linecast</c> hits an object of type <c><typeparamref name="T"/></c>, with the object itself given as parameter.
        /// </summary>
        [Tooltip("Callback invoked when the Linecast hits an object of the defined type, with the object itself given as parameter.")]
        [SerializeField] private UnityEvent<T> onObjectHit = new();

        /// <summary>
        ///     <c>RaycastHit</c> result after performing the <c>Linecast</c>.
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
        ///     Perform <c>Linecast</c> immediately or subscribe to a specific event upon being enabled, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void OnEnable()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Perform <c>Linecast</c> immediately or subscribe to a specific event upon being spawned, depending on the set <c>ActivationTime</c>.
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
        private void OnDisable()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     Obtain object of type <c><typeparamref name="T"/></c> hit by the <c>Linecast</c>.
        /// </summary>
        /// <param name="objectHit">Object of type <c><typeparamref name="T"/></c>, if one was obtained.</param>
        /// <returns>Whether an object of type <c><typeparamref name="T"/></c> was successfully obtained or not.</returns>
        protected abstract bool TryGetObject(out T objectHit);

        /// <summary>
        ///     Perform <c>Linecast</c> on all clients, unless not spawned or unsuccessful.
        /// </summary>
        public virtual void PerformCast()
        {
            if (targetPoint == null)
            {
                Plugin.Logger.LogError($"No valid target point defined for NetworkedLinecast '{name}'!");

                return;
            }

            // Obtain start and end positions for the Linecast.
            Vector3 startPosition = transform.position + (transform.rotation * startOffset),
                endPosition = targetPoint.position;

            // Perform Linecast on the local client.
            if (PerformCastLocal(startPosition, endPosition) && IsSpawned)
            {
                // Perform Linecast on all other clients with the successful start and end positions for this client.
                PerformCastRpc(new(startPosition), new(endPosition));
            }
        }

        /// <summary>
        ///     Perform <c>Linecast</c> on all other clients, with the same start and end positions.
        /// </summary>
        /// <param name="startInfo">Start position for the <c>Linecast</c>.</param>
        /// <param name="endInfo">End position for the <c>Linecast</c>.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PerformCastRpc(TransformInfo startInfo, TransformInfo endInfo)
        {
            if (!PerformCastLocal(startInfo.position, endInfo.position, receivedCall: true))
            {
                Plugin.Logger.LogWarning("Linecast that was successful on another client failed for this client, there could potentially be a desync!");
            }
        }

        /// <summary>
        ///     Perform <c>Linecast</c> on the local client.
        /// </summary>
        public virtual void PerformCastLocal()
        {
            if (targetPoint == null)
            {
                Plugin.Logger.LogError($"No valid target point defined for NetworkedLinecast '{name}' on the local client!");

                return;
            }

            _ = PerformCastLocal(transform.position + (transform.rotation * startOffset), targetPoint.position);
        }

        /// <summary>
        ///     Perform <c>Linecast</c> on the local client.
        /// </summary>
        /// <param name="startPosition">Start position for the <c>Linecast</c>.</param>
        /// <param name="endPosition">End position for the <c>Linecast</c>.</param>
        /// <param name="receivedCall">Whether the local client is receiving another client's successful <c>Linecast</c> or not.</param>
        /// <returns>Whether the <c>Linecast</c> performed was successful or not.</returns>
        protected virtual bool PerformCastLocal(Vector3 startPosition, Vector3 endPosition, bool receivedCall = false)
        {
            // Perform Linecast between this object and the target end point to detect any intersecting Collider.
            if (!Physics.Linecast(startPosition, endPosition, out hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                // Check if client received this Linecast call.
                if (receivedCall)
                {
                    return false;
                }

                if (IsSpawned) // TODO: Separate local field?
                {
                    // Invoke failed Linecast callback on all clients.
                    FailedCastRpc();
                }
                else
                {
                    // Invoke failed Linecast callback.
                    onLinecastFail.Invoke();
                }

                return false;
            }

            // Invoke successful Linecast callback, with the distance between this object and the point hit given as parameter.
            onLinecastSuccess.Invoke(hit.distance);

            // Attempt to get defined type from the object hit by the Linecast.
            if (TryGetObject(out T objectHit))
            {
                // Invoke object hit callback, with the object in question as parameter.
                onObjectHit.Invoke(objectHit);
            }

            return true;
        }

        /// <summary>
        ///     Send <c>Linecast</c> fail callback to all clients.
        /// </summary>
        [Rpc(SendTo.ClientsAndHost, DeferLocal = true, RequireOwnership = false)]
        private void FailedCastRpc()
        {
            onLinecastFail.Invoke();
        }

        /// <inheritdoc/>
        public void PerformActivation(ActivationTime activationTime)
        {
            PerformCast();
        }

        /// <inheritdoc/>
        public void ResetActivation(bool instant)
        {
            ActivationSelf.Reinitialize(instant);
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