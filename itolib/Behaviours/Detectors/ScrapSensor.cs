using itolib.Interfaces;
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
        private readonly List<Collider> disabledColliders = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        private readonly List<Renderer> disabledRenderers = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
        {
            layerMask = (1 << LayerMask.NameToLayer("Props")) | (1 << LayerMask.NameToLayer("PhysicsObject"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (!NetworkManager.Singleton.IsHost && regionCollider != null)
            {
                regionCollider.enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            int itemsFound = 0;

            for (int i = 0; i < overlapBuffer?.Length; i++)
            {
                Collider? itemCollider = overlapBuffer[i];

                if (itemCollider == null || !itemCollider.enabled) // Skip disabled colliders.
                {
                    continue;
                }

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
        protected override void OnTriggerEnter(Collider other)
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
        protected override void OnTriggerExit(Collider other)
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
                    disabledColliders.Add(collider);
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
                    disabledRenderers.Add(renderer);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemRenderers()
        {
            for (int i = 0; i < disabledRenderers.Count; i++)
            {
                if (disabledRenderers[i] != null)
                {
                    disabledRenderers[i].enabled = true;
                }
            }

            disabledRenderers.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EnableItemColliders()
        {
            for (int i = 0; i < disabledColliders.Count; i++)
            {
                if (disabledColliders[i] != null)
                {
                    disabledColliders[i].enabled = true;
                }
            }

            disabledColliders.Clear();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public static void DropItem(GrabbableObject item)
        {
            if (item is IEventfulItem grabbable)
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
        private void FoundItemsEachClientRpc(NetworkBehaviourReference itemReference)
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
        private void FoundItemsAnyClientRpc(int itemsFound)
        {
            onObjectsAny.Invoke(itemsFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ServerRpc(RequireOwnership = false)]
        private void RegionEnteredServerRpc(NetworkBehaviourReference itemReference, bool exit = false)
        {
            RegionEnteredClientRpc(itemReference, exit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="exit"></param>
        [ClientRpc]
        private void RegionEnteredClientRpc(NetworkBehaviourReference itemReference, bool exit = false)
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