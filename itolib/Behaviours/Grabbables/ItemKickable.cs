using GameNetcodeStuff;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(ItemGrabbable))]
    public class ItemKickable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public PlayerControllerB? LastKickedBy { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public float KickTimer { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public Ray KickRay { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public RaycastHit rayHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ItemGrabbable item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Transform itemTransform = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float kickDistance = 12.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float kickUpwardAmount = 0.4f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float fallDistance = 65.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float fallSpeed = 12.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float rotationSpeed = 14.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve? fallCurve;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve? verticalFallCurve;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve? verticalFallCurveNoBounce;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent onEnemyKick = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onPlayerKick = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Collision")]
        [Tooltip("")]
        public LayerMask collisionMask = 369101057;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            if (item == null && !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            item.onActivatePhysicsTrigger.AddListener(ActivatePhysicsTrigger);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            item.FallWithCurveOverride = FallWithCurve;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ActivatePhysicsTrigger(Collider other)
        {
            if ((!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
                || Physics.Linecast(other.gameObject.transform.position + Vector3.up, itemTransform.position + (Vector3.up * 0.5f),
                    StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (other.gameObject.CompareTag("Enemy"))
            {
                item.FallWithCurveOverride = FallWithCurve;

                if (IsHost)
                {
                    BeginKick(other.gameObject.transform.position + Vector3.up, hitByEnemy: true);
                }
            }
            else if (other.gameObject.CompareTag("Player"))
            {
                item.FallWithCurveOverride = FallWithCurve;

                if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
                {
                    BeginKick(other.gameObject.transform.position + Vector3.up, hitByEnemy: false);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void FallWithCurve()
        {
            float magnitude = (item.startFallingPosition - item.targetFloorPosition).magnitude;

            itemTransform.rotation = Quaternion.Lerp(itemTransform.rotation, Quaternion.Euler(item.itemProperties.restingRotation.x, itemTransform.eulerAngles.y,
                item.itemProperties.restingRotation.z), rotationSpeed * Time.deltaTime / magnitude);
            itemTransform.localPosition = Vector3.Lerp(item.startFallingPosition, item.targetFloorPosition,
                fallCurve?.Evaluate(item.fallTime) ?? item.fallTime);

            itemTransform.localPosition = magnitude < 3.0f
                ? Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                    new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z),
                    verticalFallCurveNoBounce?.Evaluate(item.fallTime) ?? item.fallTime)
                : new(itemTransform.localPosition.x, itemTransform.localPosition.y + ((verticalFallCurve?.Evaluate(item.fallTime)
                    ?? item.fallTime) * kickUpwardAmount), itemTransform.localPosition.z);

            item.fallTime += Mathf.Abs(fallSpeed * Time.deltaTime / magnitude);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitFromPosition"></param>
        /// <returns></returns>
        public Vector3 GetKickDestination(Vector3 hitFromPosition)
        {
            Vector3 pos = itemTransform.position;

            Vector3 direction = (pos - hitFromPosition) * 1000.0f;
            direction = Vector3.Normalize(direction);
            direction.y = 0.15f;

            KickRay = new(pos + (Vector3.up * 0.22f), direction);

            pos = Physics.Raycast(KickRay, out rayHit, kickDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? (rayHit.distance < 2f
                    ? KickRay.GetPoint(rayHit.distance - 0.05f) + (rayHit.normal * (rayHit.distance * 2.0f))
                    : KickRay.GetPoint(rayHit.distance - 0.05f))
                : KickRay.GetPoint(kickDistance);

            KickRay = new(pos, Vector3.down);

            return Physics.Raycast(KickRay, out rayHit, kickDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? rayHit.point + (Vector3.up * item.itemProperties.verticalOffset) : Vector3.zero;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitFromPosition"></param>
        /// <param name="hitByEnemy"></param> 
        public void BeginKick(Vector3 hitFromPosition, bool hitByEnemy)
        {
            if (hitByEnemy)
            {
                onEnemyKick.Invoke();

                return;
            }

            if (item.isHeld || item.parentObject != null || (itemTransform.GetParent() != RoundManager.Instance.spawnedScrapContainer
                && itemTransform.GetParent() != StartOfRound.Instance.propsContainer
                && itemTransform.GetParent() != StartOfRound.Instance.elevatorTransform))
            {
                return;
            }

            if (LastKickedBy != null && LastKickedBy.IsLocalClient() && Time.realtimeSinceStartup - KickTimer < 0.35f)
            {
                return;
            }

            KickTimer = Time.realtimeSinceStartup;
            LastKickedBy = GameNetworkManager.Instance.localPlayerController;

            Vector3 destination = GetKickDestination(hitFromPosition);
            if (destination == Vector3.zero)
            {
                return;
            }

            bool setInElevator = false;
            bool setInShipRoom = false;

            if (StartOfRound.Instance.shipBounds.bounds.Contains(destination))
            {
                setInElevator = true;
                setInShipRoom = StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(destination);

                destination = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(destination);
            }
            else
            {
                destination = StartOfRound.Instance.propsContainer.InverseTransformPoint(destination);
            }

            if (!hitByEnemy)
            {
                onPlayerKick.Invoke(LastKickedBy);
            }

            KickLocalClient(destination, setInElevator, setInShipRoom);
            KickServerRpc(destination, LastKickedBy, setInElevator, setInShipRoom);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="setInElevator"></param>
        /// <param name="setInShipRoom"></param>
        public void KickLocalClient(Vector3 destination, bool setInElevator, bool setInShipRoom)
        {
            item.fallTime = 0.0f;
            item.hasHitGround = false;

            itemTransform.SetParent(setInElevator ? StartOfRound.Instance.elevatorTransform : StartOfRound.Instance.propsContainer, true);

            if (LastKickedBy != null)
            {
                LastKickedBy.SetItemInElevator(setInElevator, setInShipRoom, item);
            }

            item.startFallingPosition = itemTransform.GetParent().InverseTransformPoint(itemTransform.position + (Vector3.up * 0.07f));
            item.targetFloorPosition = destination;

            // TODO: Player kick animation?
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="playerWhoKicked"></param>
        /// <param name="setInElevator"></param>
        /// <param name="setInShipRoom"></param>
        [ServerRpc(RequireOwnership = false)]
        public void KickServerRpc(Vector3 destination, NetworkBehaviourReference playerWhoKicked, bool setInElevator, bool setInShipRoom)
        {
            if (playerWhoKicked.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                LastKickedBy = player;
                KickLocalClient(destination, setInElevator, setInShipRoom);
            }

            KickClientRpc(destination, playerWhoKicked, setInElevator, setInShipRoom);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="playerWhoKicked"></param>
        /// <param name="setInElevator"></param>
        /// <param name="setInShipRoom"></param>
        [ClientRpc]
        public void KickClientRpc(Vector3 destination, NetworkBehaviourReference playerWhoKicked, bool setInElevator, bool setInShipRoom)
        {
            if (IsHost)
            {
                return;
            }

            if (playerWhoKicked.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                LastKickedBy = player;
                KickLocalClient(destination, setInElevator, setInShipRoom);
            }
        }
    }
}