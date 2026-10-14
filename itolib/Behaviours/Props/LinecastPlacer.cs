using itolib.Behaviours.Networking;
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
        ///     Offset to apply when aligning with the point intersecting with the <c>Linecast</c>, if there is one.
        /// </summary>
        [Tooltip("Offset to apply when aligning with the point intersecting with the Linecast, if there is one.")]
        [SerializeField] private Vector3 hitRotationOffset = Vector3.up;

        /// <summary>
        ///     Position and rotation prior to performing the <c>Linecast</c>.
        /// </summary>
        private TransformInfo previousInfo;

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
            previousInfo = new(transform);

            // Obtain hit point position and hit normal rotation, with offsets applied.
            Vector3 hitPosition = moveToHit ? hit.point + (previousInfo.rotation * hitPositionOffset) : previousInfo.position;
            Quaternion hitRotation = alignToHit ? previousInfo.rotation * Quaternion.FromToRotation(previousInfo.rotation * hitRotationOffset, hit.normal) : previousInfo.rotation;

            // Place object for all clients, unless not spawned.
            PlaceObject(hitPosition, hitRotation, previousInfo);
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

            // Apply position and rotation.
            transform.SetPositionAndRotation(position, rotation); // TODO: SmoothDamp!
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