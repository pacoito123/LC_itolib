using itolib.Behaviours.Grabbables;
using System.Collections.Generic;
using System.Linq;
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

            int scrapFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                Collider scrapCollider = OverlapBuffer![i];

                if (scrapCollider.TryGetComponent(out GrabbableObject item)
                    && !scrapCollider.TryGetComponent(out EnemyAI _)) // Maneater check...
                {
                    onObjectsEach?.Invoke(item);
                    scrapFound++;
                }
            }

            if (scrapFound > 0)
            {
                onObjectsAny?.Invoke(scrapFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GrabbableObject item)
                && !item.TryGetComponent(out EnemyAI _)) // Maneater check...
            {
                onRegionEntered?.Invoke(item);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GrabbableObject item)
                && !item.TryGetComponent(out EnemyAI _)) // Maneater check...
            {
                onRegionExited?.Invoke(item);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DisableItemCollider(GrabbableObject item)
        {
            item.GetComponentsInChildren<Collider>().Where(collider => collider.enabled).ToList().ForEach(collider =>
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
    }
}