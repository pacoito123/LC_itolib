using UnityEngine;

namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class ColliderExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="seededRandom"></param>
        /// <returns></returns>
        public static Vector3 GetPointWithin(this BoxCollider collider, System.Random? seededRandom = null)
        {
            Vector3 extents = collider.size * 0.5f;
            Vector3 point = seededRandom != null
                ? new(seededRandom.Next(-extents.x, extents.x),
                    seededRandom.Next(-extents.y, extents.y),
                    seededRandom.Next(-extents.z, extents.z))
                : new(UnityEngine.Random.Range(-extents.x, extents.x),
                    UnityEngine.Random.Range(-extents.y, extents.y),
                    UnityEngine.Random.Range(-extents.z, extents.z));

            return point;
        }
    }
}