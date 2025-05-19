using itolib.Behaviours.Grabbables;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Represents a <c>DetectRegion</c> specifically for <c>GrabbableObject</c> objects, with some additional stuff.
    /// </summary>
    public class ScrapSensor : DetectRegion<GrabbableObject>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<Collider> DisabledColliders { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<MeshRenderer> DisabledRenderers { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            layerMask = (1 << LayerMask.NameToLayer("Props")) | (1 << LayerMask.NameToLayer("PhysicsObject"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            base.CheckObjectsInRegion();

            if (!IsHost)
            {
                if (regionCollider != null)
                {
                    regionCollider.enabled = false;
                }

                return;
            }

            int itemsFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                Collider itemCollider = OverlapBuffer![i];

                if (itemCollider.TryGetComponent(out GrabbableObject item)
                    && !itemCollider.TryGetComponent(out EnemyAI _)) // Maneater check...
                {
                    FoundItemsEachClientRpc(item.GetComponent<NetworkObject>());
                    itemsFound++;
                }
            }

            if (itemsFound > 0)
            {
                FoundItemsAnyClientRpc(itemsFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GrabbableObject item) && item.IsOwner
                && !item.TryGetComponent(out EnemyAI _)) // Maneater check...
            {
                onRegionEntered?.Invoke(item);
                RegionEnteredServerRpc(item.GetComponent<NetworkObject>());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GrabbableObject item) && item.IsOwner
                && !item.TryGetComponent(out EnemyAI _)) // Maneater check...
            {
                onRegionExited?.Invoke(item);
                RegionEnteredServerRpc(item.GetComponent<NetworkObject>(), exit: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DisableItemCollider(GrabbableObject item)
        {
            item.GetComponentsInChildren<Collider>().Where(collider =>
                collider.enabled && collider.gameObject.layer != LayerMask.NameToLayer("ScanNode")).ToList().ForEach(collider =>
            {
                collider.enabled = false;
                DisabledColliders.Add(collider);
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DisableItemRenderer(GrabbableObject item)
        {
            item.GetComponentsInChildren<MeshRenderer>().Where(renderer => renderer.enabled).ToList().ForEach(renderer =>
            {
                renderer.enabled = false;
                DisabledRenderers.Add(renderer);
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemRenderers()
        {
            DisabledRenderers.ForEach(renderer => renderer.enabled = true);
            DisabledRenderers.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemColliders()
        {
            DisabledColliders.ForEach(collider => collider.enabled = true);
            DisabledColliders.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DropItem(GrabbableObject item)
        {
            if (item is ItemGrabbable grabbable)
            {
                grabbable.FallWithCurveOverride = null;
            }

            item.startFallingPosition = item.transform.position;

            if (item.transform.GetParent() != null)
            {
                item.startFallingPosition = item.transform.GetParent().InverseTransformPoint(item.startFallingPosition);
            }

            item.FallToGround();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="scrapReference"></param>
        [ClientRpc]
        public void FoundItemsEachClientRpc(NetworkObjectReference scrapReference)
        {
            if (scrapReference.TryGet(out NetworkObject scrapNetworkObject)
                && scrapNetworkObject.TryGetComponent(out GrabbableObject scrap))
            {
                onObjectsEach?.Invoke(scrap);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="scrapFound"></param>
        [ClientRpc]
        public void FoundItemsAnyClientRpc(int scrapFound)
        {
            onObjectsAny?.Invoke(scrapFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        public void RegionEnteredServerRpc(NetworkObjectReference itemReference, bool exit = false)
        {
            RegionEnteredClientRpc(itemReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkObjectReference itemReference, bool exit = false)
        {
            if (itemReference.TryGet(out NetworkObject itemNetworkObject)
                && itemNetworkObject.TryGetComponent(out GrabbableObject item) && !item.IsOwner)
            {
                if (!exit)
                {
                    onRegionEntered?.Invoke(item);
                }
                else
                {
                    onRegionExited?.Invoke(item);
                }
            }
        }
    }
}