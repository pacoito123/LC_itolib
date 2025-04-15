using itolib.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Items
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ItemRegion : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public Collider[]? OverlapBuffer { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Region")]
        [Tooltip("")]
        public Collider? regionCollider;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxItems = 16;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onItemsObtained;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<GrabbableObject>? onItemsEach;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("")]
        public LayerMask layerMask = 4160;

        private void Start()
        {
            OverlapBuffer = new Collider[maxItems];

            if (activationTime == ActivationTime.StartOfRound)
            {
                StartOfRound.Instance?.StartNewRoundEvent.AddListener(ObtainItemsInRegion);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ObtainItemsInRegion()
        {
            if (regionCollider == null || OverlapBuffer == null)
            {
                return;
            }

            if (regionCollider is BoxCollider)
            {
                int items = Physics.OverlapBoxNonAlloc(transform.position, regionCollider.bounds.extents, OverlapBuffer,
                    Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore);

                for (int i = 0; i < items; i++)
                {
                    if (OverlapBuffer[i].TryGetComponent(out GrabbableObject item))
                    {
                        onItemsEach?.Invoke(item);
                    }
                }
            }

            onItemsObtained?.Invoke();
        }
    }
}