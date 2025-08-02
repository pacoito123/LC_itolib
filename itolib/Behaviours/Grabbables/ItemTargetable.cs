using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct DestinationInfo : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Destination Info")]
        [Tooltip("")]
        public Vector3 startPosition = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 targetPosition = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool setInElevator;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool setInShipRoom;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool playerInvolved;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool enemyInvolved;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference playerReference = default;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference enemyReference = default;

        /// <summary>
        ///     TODO.
        /// </summary>
        public DestinationInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref startPosition);
            serializer.SerializeValue(ref targetPosition);
            serializer.SerializeValue(ref setInElevator);
            serializer.SerializeValue(ref setInShipRoom);

            serializer.SerializeValue(ref playerInvolved);
            serializer.SerializeValue(ref enemyInvolved);

            if (playerInvolved)
            {
                serializer.SerializeValue(ref playerReference);
            }

            if (enemyInvolved)
            {
                serializer.SerializeValue(ref enemyReference);
            }
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public abstract class ItemTargetable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Targetable")]
        [Tooltip("")]
        [SerializeField] protected GrabbableObject item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected Transform itemTransform = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Trajectory")]
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] protected float maxDistance;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] protected float fallDistance;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] protected float fallSpeed;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] protected float rotationSpeed;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Curves")]
        [Tooltip("")]
        [SerializeField] protected AnimationCurve? fallCurve;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected AnimationCurve? verticalFallCurve;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected AnimationCurve? verticalFallCurveNoBounce;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Collision")]
        [Tooltip("")]
        [SerializeField] protected LayerMask collisionMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected Ray trajectoryRay;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected RaycastHit rayHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected IEventfulItem? eventfulSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected abstract void Reset();

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Awake()
        {
            if (item == null || !TryGetComponent(out item) || item is not IEventfulItem eventfulItem)
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            eventfulSelf = eventfulItem;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            if (eventfulSelf != null)
            {
                eventfulSelf.FallWithCurveOverride = FallWithCurve;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected abstract void FallWithCurve();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        protected abstract bool TryGetDestination(out Vector3 destination, Transform origin);

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destinationInfo"></param>
        /// <param name="origin"></param>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        protected bool TryGetDestination(out DestinationInfo destinationInfo, Transform origin, PlayerControllerB? player = null, EnemyAI? enemy = null)
        {
            destinationInfo = default;

            if (!TryGetDestination(out Vector3 targetPosition, origin))
            {
                return false;
            }

            bool setInElevator = false, setInShipRoom = false;

            if (StartOfRound.Instance.shipBounds != null && StartOfRound.Instance.shipBounds.bounds.Contains(targetPosition))
            {
                setInElevator = true;
                setInShipRoom = StartOfRound.Instance.shipInnerRoomBounds != null
                    && StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(targetPosition);

                targetPosition = StartOfRound.Instance.elevatorTransform != null ?
                    StartOfRound.Instance.elevatorTransform.InverseTransformPoint(targetPosition) : targetPosition;
            }
            else
            {
                targetPosition = StartOfRound.Instance.propsContainer != null ?
                    StartOfRound.Instance.propsContainer.InverseTransformPoint(targetPosition) : targetPosition;
            }

            if (player != null)
            {
                destinationInfo = new()
                {
                    startPosition = origin.position,
                    targetPosition = targetPosition,
                    setInElevator = setInElevator,
                    setInShipRoom = setInShipRoom,
                    playerInvolved = true,
                    playerReference = player
                };
            }
            else if (enemy != null)
            {
                destinationInfo = new()
                {
                    startPosition = origin.position,
                    targetPosition = targetPosition,
                    setInElevator = setInElevator,
                    setInShipRoom = setInShipRoom,
                    enemyInvolved = true,
                    enemyReference = enemy
                };
            }

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destinationInfo"></param>
        [ServerRpc(RequireOwnership = false)]
        protected void BeginTrajectoryServerRpc(DestinationInfo destinationInfo)
        {
            BeginTrajectoryClientRpc(destinationInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destinationInfo"></param>
        [ClientRpc]
        protected void BeginTrajectoryClientRpc(DestinationInfo destinationInfo)
        {
            if (destinationInfo.playerInvolved && destinationInfo.playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                BeginTrajectoryLocal(destinationInfo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destinationInfo"></param>
        protected virtual void BeginTrajectoryLocal(DestinationInfo destinationInfo)
        {
            if (eventfulSelf != null)
            {
                eventfulSelf.FallWithCurveOverride = FallWithCurve;
            }

            item.fallTime = 0.0f;
            item.hasHitGround = false;

            if (destinationInfo.playerInvolved && destinationInfo.playerReference.TryGet(out PlayerControllerB player))
            {
                // TODO: Collect on ship from host if enemy involved?
                player.SetItemInElevator(destinationInfo.setInShipRoom, destinationInfo.setInElevator, item);
            }

            itemTransform.SetParent(destinationInfo.setInElevator ? StartOfRound.Instance.elevatorTransform
                : StartOfRound.Instance.propsContainer, true);

            item.startFallingPosition = itemTransform.GetParent().InverseTransformPoint(destinationInfo.startPosition);
            item.targetFloorPosition = destinationInfo.targetPosition;
        }
    }
}