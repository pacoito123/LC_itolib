using itolib.Behaviours.Detectors;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class WallBreaker : DetectRegion<WallBreaker>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public int BlockersFound { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Wall Breaker")]
        [Tooltip("")]
        public float tolerance = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool disableSelf = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            maxObjects = 8;
            layerMask = 1 << LayerMask.NameToLayer("Room");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            if (disableSelf)
            {
                onObjectsAny?.AddListener(_ => gameObject.SetActive(false));
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

            BlockersFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out WallBreaker blocker))
                {
                    if (blocker == this)
                    {
                        continue;
                    }

                    float magnitude = (transform.position - blocker.transform.position).magnitude;

                    if (magnitude < tolerance)
                    {
                        onObjectsEach?.Invoke(blocker);
                        BlockersFound++;
                    }
                }
            }

            if (BlockersFound > 0)
            {
                onObjectsAny?.Invoke(BlockersFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="blocker"></param>
        public void DisableWall(WallBreaker blocker)
        {
            blocker.gameObject.SetActive(false);
        }
    }
}