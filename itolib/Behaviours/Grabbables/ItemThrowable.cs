using GameNetcodeStuff;
using itolib.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemThrowable : ItemTargetable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Item Throwable")]
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onThrowStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onThrowFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private PlayerControllerB? lastThrownBy;

        /// <summary>
        ///     TODO.
        /// </summary> 
        protected override void Reset()
        {
            maxDistance = 12.0f;
            fallDistance = 30.0f;
            fallSpeed = 12.0f;
            rotationSpeed = 14.0f;

            collisionMask = LayerMask.GetMask("Default", "Room", "Colliders", "Railing");

            // Stun grenade fall curves.
            Keyframe[] grenadeFallCurveKeyframes = [new(0.0f, 0.0f, 2.0f, 2.0f, 0.0f, 0.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];
            Keyframe[] grenadeVerticalFallCurveKeyframes = [new(0.0f, 0.0f, 0.1169f, 0.1169f, 0.0f, 0.2723f),
                new(0.4908f, 1.0f, 4.1147f, -1.8138f, 0.0723f, 0.2832f),
                new(0.7588f, 1.0f, 1.4123f, -1.3679f, 0.32f, 0.5692f),
                new(0.9394f, 1.0f, 0.8265f, -0.029f, 0.5375f, 1.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];
            Keyframe[] grenadeVerticalFallCurveNoBounceKeyframes = [new(0.0f, 0.0f, 0.1169f, 0.1169f, 0.0f, 0.2723f),
                new(0.4908f, 1.0f, 4.1147f, 0.061f, 0.0723f, 0.2077f),
                new(0.9394f, 1.0f, 0.0639f, 0.029f, 0.1981f, 1.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];

            SetKeyframeModes(grenadeFallCurveKeyframes, WeightedMode.None);
            SetKeyframeModes(grenadeVerticalFallCurveKeyframes, WeightedMode.None);
            SetKeyframeModes(grenadeVerticalFallCurveNoBounceKeyframes, WeightedMode.None);

            fallCurve = new(grenadeFallCurveKeyframes);
            verticalFallCurve = new(grenadeVerticalFallCurveKeyframes);
            verticalFallCurveNoBounce = new(grenadeVerticalFallCurveNoBounceKeyframes);
            // ...
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            eventfulSelf?.OnActivate.AddListener(ItemActivate);
            eventfulSelf?.OnGroundReached.AddListener(OnHitGround);
            eventfulSelf?.OnGroundReachedVariant.AddListener(OnHitGround);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        private void ItemActivate(bool used, bool buttonDown)
        {
            if (item.playerHeldBy == null || !item.playerHeldBy.IsLocalClient())
            {
                return;
            }

            if (TryGetDestination(out DestinationInfo playerThrowInfo, item.transform, player: item.playerHeldBy))
            {
                item.playerHeldBy.DiscardHeldObject(true, null, playerThrowInfo.targetPosition, true);

                BeginTrajectoryLocal(playerThrowInfo);

                if (IsSpawned)
                {
                    BeginTrajectoryServerRpc(playerThrowInfo);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnHitGround()
        {
            if (lastThrownBy != null)
            {
                onThrowFinish.Invoke(lastThrownBy);
                lastThrownBy = null;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnHitGround(int _)
        {
            OnHitGround();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void FallWithCurve()
        {
            float magnitude = (item.startFallingPosition - item.targetFloorPosition).magnitude;

            itemTransform.rotation = Quaternion.Lerp(itemTransform.rotation, Quaternion.Euler(item.itemProperties.restingRotation.x, itemTransform.eulerAngles.y,
                item.itemProperties.restingRotation.z), rotationSpeed * Time.deltaTime / magnitude);
            itemTransform.localPosition = Vector3.Lerp(item.startFallingPosition, item.targetFloorPosition,
                fallCurve?.Evaluate(item.fallTime) ?? item.fallTime);

            itemTransform.localPosition = Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z), magnitude > 5.0f
                    ? (verticalFallCurveNoBounce?.Evaluate(item.fallTime) ?? item.fallTime)
                    : (verticalFallCurve?.Evaluate(item.fallTime) ?? item.fallTime));

            item.fallTime += Mathf.Abs(fallSpeed * Time.deltaTime / magnitude);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        protected override bool TryGetDestination(out Vector3 destination, Transform origin)
        {
            trajectoryRay = new(item.playerHeldBy.gameplayCamera.transform.position, item.playerHeldBy.gameplayCamera.transform.forward);

            destination = Physics.Raycast(trajectoryRay, out rayHit, maxDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? trajectoryRay.GetPoint(rayHit.distance - 0.05f)
                : trajectoryRay.GetPoint(maxDistance);

            trajectoryRay = new(destination, Vector3.down);

            destination = Physics.Raycast(trajectoryRay, out rayHit, fallDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? rayHit.point + (Vector3.up * item.itemProperties.verticalOffset) : trajectoryRay.GetPoint(fallDistance);

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="destinationInfo"></param>
        protected override void BeginTrajectoryLocal(DestinationInfo destinationInfo)
        {
            base.BeginTrajectoryLocal(destinationInfo);

            if (destinationInfo.playerInvolved && destinationInfo.playerReference.TryGet(out PlayerControllerB player))
            {
                lastThrownBy = player;
                onThrowStart.Invoke(player);
            }
        }
    }
}