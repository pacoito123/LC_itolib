using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace itolib.Behaviours.Items
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemDisabler : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<MeshRenderer> DisabledRenderers { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<Collider> DisabledColliders { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
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
    }
}