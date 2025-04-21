using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class EnemyRegion : DetectRegion<EnemyAI>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            layerMask = 1 << LayerMask.NameToLayer("Enemies");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyAI enemy))
            {
                onRegionEntered?.Invoke(enemy);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out EnemyAI enemy))
            {
                onRegionExited?.Invoke(enemy);
            }
        }
    }
}