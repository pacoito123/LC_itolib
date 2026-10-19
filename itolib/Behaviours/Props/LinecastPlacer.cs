using itolib.Behaviours.Networking;
using itolib.Extensions;
using itolib.Structs;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     Represents an object that places itself onto an intersecting <c>Collider</c> by performing a <c>Linecast</c>.
    /// </summary>
    public class LinecastPlacer : NetworkedLinecast
    {
        /// <summary>
        ///     Whether to move this object to the point intersecting with the <c>Linecast</c> or not, if there is one.
        /// </summary>
        [Space(3.0f)]
        [Header("Linecast Placer")]
        [Space(5.0f)]
        [Header("Hit Position")]
        [Tooltip("Whether to move this object to the point intersecting with the Linecast or not, if there is one.")]
        [SerializeField] private bool moveToHit = true;

        /// <summary>
        ///     Approximate time until reaching the target position, in seconds.
        /// </summary>
        [Tooltip("Approximate time until reaching the target position, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float moveTime = 0.0f;

        /// <summary>
        ///     Offset to apply when moving to the point intersecting with the <c>Linecast</c>, if there is one.
        /// </summary>
        [Tooltip("Offset to apply when moving to the point intersecting with the Linecast, if there is one.")]
        [SerializeField] private Vector3 hitPositionOffset = Vector3.zero;

        /// <summary>
        ///     Whether to align this object's rotation with the point intersecting with the <c>Linecast</c> or not, if there is one.
        /// </summary>
        [Space(3.0f)]
        [Header("Hit Rotation")]
        [Tooltip("Whether to align this object's rotation with the point intersecting with the <c>Linecast</c> or not, if there is one.")]
        [SerializeField] private bool alignToHit = true;

        /// <summary>
        ///     Approximate time until reaching the target rotation, in seconds.
        /// </summary>
        [Tooltip("Approximate time until reaching the target rotation, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float alignTime = 0.0f;

        /// <summary>
        ///     Offset to apply when aligning with the point intersecting with the <c>Linecast</c>, if there is one.
        /// </summary>
        [Tooltip("Offset to apply when aligning with the point intersecting with the Linecast, if there is one.")]
        [SerializeField] private Vector3 hitRotationOffset = Vector3.up;

        /// <summary>
        ///     Position and rotation prior to performing the <c>Linecast</c>.
        /// </summary>
        private TransformInfo previousInfo;

        /// <summary>
        ///     Cached <c>Transform</c> for the placer.
        /// </summary>
        private Transform placerTransform = null!;

        /// <summary>
        ///     Position target to move towards.
        /// </summary>
        private Vector3 moveTarget = Vector3.zero;

        /// <summary>
        ///     Rotation target to align towards.
        /// </summary>
        private Quaternion alignTarget = Quaternion.identity;

        /// <summary>
        ///     Current or initial velocity for the movement.
        /// </summary>
        private Vector3 moveVelocityPos = Vector3.zero;

        /// <summary>
        ///     Current or initial velocity for the alignment.
        /// </summary>
        private Quaternion alignVelocityRot = Quaternion.identity;

        /// <summary>
        ///     Perform <c>Linecast</c> and set position and rotation accordingly.
        /// </summary>
        public override void PerformCast()
        {
            base.PerformCast();

            // Check if Linecast successfully hit a Collider.
            if (hit.collider == null)
            {
                return;
            }

            // Save previous position and rotation.
            previousInfo = new(placerTransform);

            // Obtain hit point position and hit normal rotation, with offsets applied.
            Vector3 hitPosition = moveToHit ? hit.point : previousInfo.position;
            Quaternion hitRotation = alignToHit ? previousInfo.rotation * Quaternion.FromToRotation(placerTransform.up, hit.normal) : previousInfo.rotation;

            // Apply position and rotation offsets.
            hitPosition += previousInfo.rotation * hitPositionOffset;
            hitRotation *= Quaternion.Euler(hitRotationOffset);

            // Place object for all clients, unless not spawned.
            PlaceObject(hitPosition, hitRotation, previousInfo);
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        private void Awake()
        {
            // Cache placer Transform.
            placerTransform = transform;
        }

        /// <summary>
        ///     Handle smooth placement.
        /// </summary>
        private void LateUpdate()
        {
            // Check if neither moving or aligning smoothly.
            if (moveTime <= 0.0f && alignTime <= 0.0f)
            {
                return;
            }

            // Obtain placer's position and rotation.
            placerTransform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

            // Check if both moving and aligning smoothly.
            if (moveTime > 0.0f && alignTime > 0.0f)
            {
                position = Vector3.SmoothDamp(placerTransform.position, moveTarget, ref moveVelocityPos, moveTime);
                rotation = rotation.SmoothDamp(alignTarget, ref alignVelocityRot, alignTime);

                // Apply smooth position and rotation with the specified time.
                placerTransform.SetPositionAndRotation(position, rotation);

                return;
            }

            // Check if moving smoothly.
            if (moveTime > 0.0f)
            {
                // Apply smooth position with the specified time.
                placerTransform.position = Vector3.SmoothDamp(position, moveTarget, ref moveVelocityPos, moveTime);
            }

            // Check if aligning smoothly.
            if (alignTime > 0.0f)
            {
                // Apply smooth rotation with the specified time.
                placerTransform.rotation = rotation.SmoothDamp(alignTarget, ref alignVelocityRot, alignTime);
            }
        }

        /// <summary>
        ///     Apply given position and rotation for all clients, unless not spawned.
        /// </summary>
        /// <param name="position">Position to apply.</param>
        /// <param name="rotation">Rotation to apply.</param>
        /// <param name="previousInfo">Previous position and rotation.</param>
        private void PlaceObject(Vector3 position, Quaternion rotation, TransformInfo previousInfo)
        {
            // Place object for the local client.
            PlaceObjectLocal(position, rotation, previousInfo);

            if (IsSpawned) // TODO: Separate local field?
            {
                // Place object for all other clients.
                PlaceObjectRpc(new(position, rotation), previousInfo);
            }
        }

        /// <summary>
        ///     Apply given position and rotation for all other clients.
        /// </summary>
        /// <param name="placementInfo">Position and rotation to apply.</param>
        /// <param name="previousInfo">Previous position and rotation.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlaceObjectRpc(TransformInfo placementInfo, TransformInfo previousInfo)
        {
            // Place object for the local client.
            PlaceObjectLocal(placementInfo.position, placementInfo.rotation, previousInfo);
        }

        /// <summary>
        ///     Apply given position and rotation for the local client.
        /// </summary>
        /// <param name="position">Position to apply.</param>
        /// <param name="rotation">Rotation to apply.</param>
        /// <param name="previousInfo">Previous position and rotation.</param>
        private void PlaceObjectLocal(Vector3 position, Quaternion rotation, TransformInfo previousInfo)
        {
            // Set previous position and rotation.
            this.previousInfo = previousInfo;

            // Check if neither moving or aligning smoothly.
            if (moveTime <= 0.0f && alignTime <= 0.0f)
            {
                // Apply position and rotation for the placer.
                placerTransform.SetPositionAndRotation(position, rotation);

                return;
            }

            // Check if moving smoothly.
            if (moveTime <= 0.0f)
            {
                // Apply position for the placer.
                placerTransform.position = position;
            }
            else
            {
                // Set target position for the placer.
                moveTarget = position;
            }

            // Check if aligning smoothly.
            if (alignTime <= 0.0f)
            {
                // Apply rotation for the placer.
                placerTransform.rotation = rotation;
            }
            else
            {
                // Set target rotation for the placer.
                alignTarget = rotation;
            }
        }

        /// <summary>
        ///     Reset original position and rotation.
        /// </summary>
        public void UndoCast()
        {
            PlaceObject(previousInfo.position, previousInfo.rotation, previousInfo);
        }
    }
}