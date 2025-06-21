using itolib.Behaviours.Grabbables;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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
        public List<Renderer> DisabledRenderers { get; private set; } = [];

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
                    && !item.TryGetComponent(out NavMeshAgent _)) // Maneater check...
                {
                    FoundItemsEachClientRpc(item);
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
                && !item.TryGetComponent(out NavMeshAgent _)) // Maneater check...
            {
                onRegionEntered.Invoke(item);
                RegionEnteredServerRpc(item);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GrabbableObject item) && item.IsOwner
                && !item.TryGetComponent(out NavMeshAgent _)) // Maneater check...
            {
                onRegionExited.Invoke(item);
                RegionEnteredServerRpc(item, exit: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DisableItemCollider(GrabbableObject item)
        {
            Collider[] colliders = item.GetComponentsInChildren<Collider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider? collider = colliders[i];

                if (collider != null && collider.enabled && collider.gameObject.layer != LayerMask.NameToLayer("ScanNode"))
                {
                    collider.enabled = false;
                    DisabledColliders.Add(collider);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DisableItemRenderer(GrabbableObject item)
        {
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer? renderer = renderers[i];

                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    DisabledRenderers.Add(renderer);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemRenderers()
        {
            for (int i = 0; i < DisabledRenderers.Count; i++)
            {
                if (DisabledRenderers[i] != null)
                {
                    DisabledRenderers[i].enabled = true;
                }
            }

            DisabledRenderers.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemColliders()
        {
            for (int i = 0; i < DisabledColliders.Count; i++)
            {
                if (DisabledColliders[i] != null)
                {
                    DisabledColliders[i].enabled = true;
                }
            }

            DisabledColliders.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public static void DropItem(GrabbableObject item)
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
        /// <param name="itemReference"></param>
        [ClientRpc]
        public void FoundItemsEachClientRpc(NetworkBehaviourReference itemReference)
        {
            if (itemReference.TryGet(out GrabbableObject item))
            {
                onObjectsEach.Invoke(item);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemsFound"></param>
        [ClientRpc]
        public void FoundItemsAnyClientRpc(int itemsFound)
        {
            onObjectsAny.Invoke(itemsFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        public void RegionEnteredServerRpc(NetworkBehaviourReference itemReference, bool exit = false)
        {
            RegionEnteredClientRpc(itemReference, exit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        public void RegionEnteredClientRpc(NetworkBehaviourReference itemReference, bool exit = false)
        {
            if (itemReference.TryGet(out GrabbableObject item) && !item.IsOwner)
            {
                if (!exit)
                {
                    onRegionEntered.Invoke(item);
                }
                else
                {
                    onRegionExited.Invoke(item);
                }
            }
        }
    }
}