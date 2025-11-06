using GameNetcodeStuff;
using itolib.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemKickable : ItemTargetable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Item Kickable")]
        [Tooltip("")]
        [SerializeField] private float kickUpwardAmount = 0.4f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AnimationCurve? verticalOffsetCurve;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onPlayerKick = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<EnemyAI> onEnemyKick = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private PlayerControllerB? lastKickedBy;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float kickTimer;

        /// <summary>
        ///     TODO.
        /// </summary> 
        protected override void Reset()
        {
            maxDistance = 12.0f;
            fallDistance = 65.0f;
            fallSpeed = 12.0f;
            rotationSpeed = 14.0f;

            collisionMask = LayerMask.GetMask("Default", "Room", "Colliders", "Terrain", "PlaceableShipObjects", "Railing");

            // Soccer ball fall curves.
            Keyframe[] soccerFallCurveKeyframes = [new(0.0f, 0.0f, 2.0f, 2.0f, 0.0f, 0.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];
            Keyframe[] soccerVerticalFallCurveKeyframes = [new(0.0f, 0.0f, 0.1169f, 0.1169f, 0.0f, 0.2723f),
                new(0.4908f, 1.0f, 4.1147f, 0.0512f, 0.0723f, 0.5374f),
                new(0.9394f, 1.0f, 0.086f, -0.029f, 0.1912f, 1.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];
            Keyframe[] soccerVerticalFallCurveNoBounceKeyframes = [new(0.0f, 0.0f, 0.1169f, 0.1169f, 0.0f, 0.2723f),
                new(0.4908f, 1.0f, 4.1147f, 0.061f, 0.0723f, 0.2077f),
                new(0.9394f, 1.0f, 0.0639f, 0.029f, 0.1981f, 1.0f),
                new(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)];
            Keyframe[] soccerVerticalOffsetKeyframes = [new(0.0f, 0.0f, 7.1366f, 7.1366f, 0.0f, 0.0494f),
                new(0.4666f, 1.4733f, 0.0665f, 0.0665f, 0.2469f, 0.3333f),
                new(0.7487f, 0.0f, -11.1508f, 5.0087f, 0.076f, 0.1733f),
                new(1.0f, 0.0f, -7.1532f, -7.1532f, 0.0616f, 0.0f)];

            SetKeyframeModes(soccerFallCurveKeyframes, WeightedMode.None);
            SetKeyframeModes(soccerVerticalFallCurveKeyframes, WeightedMode.None);
            SetKeyframeModes(soccerVerticalFallCurveNoBounceKeyframes, WeightedMode.None);
            SetKeyframeModes(soccerVerticalOffsetKeyframes, WeightedMode.None);

            fallCurve = new(soccerFallCurveKeyframes);
            verticalFallCurve = new(soccerVerticalFallCurveKeyframes);
            verticalFallCurveNoBounce = new(soccerVerticalFallCurveNoBounceKeyframes);
            verticalOffsetCurve = new(soccerVerticalOffsetKeyframes);
            // ...
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            eventfulSelf?.OnActivatePhysicsTrigger.AddListener(ActivatePhysicsTrigger);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        private void ActivatePhysicsTrigger(Collider other)
        {
            if (Physics.Linecast(other.gameObject.transform.position + Vector3.up, itemTransform.position + (Vector3.up * 0.5f),
                StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (item.parentObject != null || (itemTransform.GetParent() != RoundManager.Instance.spawnedScrapContainer
                && itemTransform.GetParent() != StartOfRound.Instance.propsContainer
                && itemTransform.GetParent() != StartOfRound.Instance.elevatorTransform))
            {
                return;
            }

            if (other.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient()
                && (lastKickedBy == null || !lastKickedBy.IsLocalClient() || Time.realtimeSinceStartup - kickTimer >= 0.35f)
                && TryGetDestination(out DestinationInfo playerKickInfo, player.transform, player: player))
            {
                kickTimer = Time.realtimeSinceStartup;

                BeginTrajectoryLocal(playerKickInfo);

                if (IsSpawned)
                {
                    BeginTrajectoryRpc(playerKickInfo);
                }
            }
            else if (IsHost && other.TryGetComponent(out EnemyAICollisionDetect enemyCollision) && enemyCollision.mainScript != null
                && TryGetDestination(out DestinationInfo enemyKickInfo, enemyCollision.mainScript.transform, enemy: enemyCollision.mainScript))
            {
                BeginTrajectoryLocal(enemyKickInfo);

                if (IsSpawned)
                {
                    BeginTrajectoryRpc(enemyKickInfo);
                }
            }
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

            if (magnitude < 3.0f)
            {
                itemTransform.localPosition = Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                    new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z),
                    verticalFallCurveNoBounce?.Evaluate(item.fallTime) ?? item.fallTime);
            }
            else
            {
                itemTransform.localPosition = Vector3.Lerp(new(itemTransform.localPosition.x, item.startFallingPosition.y, itemTransform.localPosition.z),
                    new(itemTransform.localPosition.x, item.targetFloorPosition.y, itemTransform.localPosition.z),
                    verticalFallCurve?.Evaluate(item.fallTime) ?? item.fallTime);
                itemTransform.localPosition = new(itemTransform.localPosition.x, itemTransform.localPosition.y
                    + ((verticalOffsetCurve?.Evaluate(item.fallTime) ?? 0.0f) * kickUpwardAmount), itemTransform.localPosition.z);
            }

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
            Vector3 pos = itemTransform.position;

            Vector3 direction = (pos - origin.position) * 1000.0f;
            direction = Vector3.Normalize(direction);
            direction.y = 0.15f;

            trajectoryRay = new(pos + (Vector3.up * 0.22f), direction);

            pos = Physics.Raycast(trajectoryRay, out rayHit, maxDistance, collisionMask, QueryTriggerInteraction.Ignore)
                ? (rayHit.distance < 2f
                    ? trajectoryRay.GetPoint(rayHit.distance - 0.05f) + (rayHit.normal * (rayHit.distance * 2.0f))
                    : trajectoryRay.GetPoint(rayHit.distance - 0.05f))
                : trajectoryRay.GetPoint(maxDistance);

            trajectoryRay = new(pos, Vector3.down);

            if (Physics.Raycast(trajectoryRay, out rayHit, maxDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                destination = rayHit.point + (Vector3.up * item.itemProperties.verticalOffset);

                return true;
            }

            destination = Vector3.zero;

            return false;
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
                lastKickedBy = player;
                onPlayerKick.Invoke(player);
            }
            else if (destinationInfo.enemyInvolved && destinationInfo.enemyReference.TryGet(out EnemyAI enemy))
            {
                onEnemyKick.Invoke(enemy);
            }
        }
    }
}