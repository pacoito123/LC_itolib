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
    public class ItemThrowable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public PlayerControllerB? LastThrownBy { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public Ray ThrowRay { get; private set; }

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
        public float throwDistance = 12.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float fallDistance = 30.0f;

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
        [Space(10f)]
        [Header("Collision")]
        [Tooltip("")]
        public LayerMask collisionMask = 268437761;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Header("Events")]
        public UnityEvent<PlayerControllerB> onThrowStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onThrowFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB, int> onThrowFinishVariant = new();

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

            item.onActivate.AddListener(ItemActivate);
            item.onHitGround.AddListener(OnHitGround);
            item.onHitGroundVariant.AddListener(OnHitGroundVariant);
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
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        public void ItemActivate(bool used, bool buttonDown)
        {
            if (!item.IsOwner)
            {
                return;
            }

            item.FallWithCurveOverride = FallWithCurve;

            if (item.playerHeldBy != null)
            {
                LastThrownBy = item.playerHeldBy;
                onThrowStart.Invoke(LastThrownBy);

                SyncThrowerServerRpc(item.playerHeldBy);

                item.playerHeldBy.DiscardHeldObject(true, null, GetThrowDestination(), true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnHitGround()
        {
            if (LastThrownBy != null)
            {
                onThrowFinish.Invoke(LastThrownBy);
                LastThrownBy = null;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnHitGroundVariant(int variantIndex)
        {
            if (LastThrownBy != null)
            {
                onThrowFinishVariant.Invoke(LastThrownBy, item.VariantIndex);
                LastThrownBy = null;
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

            itemTransform.localPosition = magnitude > 5.0f
                ? Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                    new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z),
                    verticalFallCurveNoBounce?.Evaluate(item.fallTime) ?? item.fallTime)
                : Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                    new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z),
                    verticalFallCurve?.Evaluate(item.fallTime) ?? item.fallTime);

            item.fallTime += Mathf.Abs(fallSpeed * Time.deltaTime / magnitude);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetThrowDestination()
        {
            Transform cameraTransform = item.playerHeldBy.gameplayCamera.transform;
            ThrowRay = new(cameraTransform.position, cameraTransform.forward);

            Vector3 pos = Physics.Raycast(ThrowRay, out rayHit, throwDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? ThrowRay.GetPoint(rayHit.distance - 0.05f)
                : ThrowRay.GetPoint(throwDistance);

            ThrowRay = new(pos, Vector3.down);

            return Physics.Raycast(ThrowRay, out rayHit, fallDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? rayHit.point + (Vector3.up * item.itemProperties.verticalOffset) : ThrowRay.GetPoint(fallDistance);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SyncThrowerServerRpc(NetworkBehaviourReference playerReference)
        {
            SyncThrowerClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void SyncThrowerClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                LastThrownBy = player;
                onThrowStart.Invoke(LastThrownBy);
            }
        }
    }
}