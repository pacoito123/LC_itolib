using System;
using itolib.Behaviours.Detectors;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Obsolete("Will likely be merged into ConnectorMerger, heh.")]
    public class WallBreaker : DetectRegion<WallBreaker>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Wall Breaker")]
        [Tooltip("")]
        [SerializeField] private float tolerance = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool disableSelf = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int blockersFound;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
        {
            maxObjects = 8;
            layerMask = 1 << LayerMask.NameToLayer("Room");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            if (disableSelf)
            {
                onObjectsAny.AddListener(_ => gameObject.SetActive(false));
            }

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            base.CheckObjectsInRegion();

            blockersFound = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                if (overlapBuffer![i].TryGetComponent(out WallBreaker blocker))
                {
                    if (blocker == this)
                    {
                        continue;
                    }

                    float magnitude = (transform.position - blocker.transform.position).magnitude;

                    if (magnitude < tolerance)
                    {
                        onObjectsEach.Invoke(blocker);
                        blockersFound++;
                    }
                }
            }

            if (blockersFound > 0)
            {
                onObjectsAny.Invoke(blockersFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="blocker"></param>
        public static void DisableWall(WallBreaker blocker)
        {
            blocker.gameObject.SetActive(false);
        }
    }
}