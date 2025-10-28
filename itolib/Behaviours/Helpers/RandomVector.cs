using itolib.Extensions;
using itolib.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class RandomVector : MonoBehaviour, ISeededScript<RandomVector>
    {
        /// <summary>
        ///     Cached instance of the current <c>RandomVector</c> as an <c>ISeededScript</c>, to avoid having to cast.
        /// </summary>
        public ISeededScript<RandomVector> SeededSelf { get; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Random Vector")]
        [Tooltip("")]
        [SerializeField] private Vector3 minVector = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Vector3 maxVector = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool isSeededRandom = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<Vector3> onRollVector = new();

        /// <summary>
        ///     Cache already-cast <c>ISeededScript</c> instance.
        /// </summary>
        private RandomVector()
        {
            SeededSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void RollVector()
        {
            Vector3 randomVector = new()
            {
                x = (minVector.x >= maxVector.x) ? minVector.x : (isSeededRandom
                    ? SeededSelf.GetSeededRandom().Next(minVector.x, maxVector.x)
                    : Random.Range(minVector.x, maxVector.x)),
                y = (minVector.y >= maxVector.y) ? minVector.y : (isSeededRandom
                    ? SeededSelf.GetSeededRandom().Next(minVector.y, maxVector.y)
                    : Random.Range(minVector.y, maxVector.y)),
                z = (minVector.z >= maxVector.z) ? minVector.z : (isSeededRandom
                    ? SeededSelf.GetSeededRandom().Next(minVector.z, maxVector.z)
                    : Random.Range(minVector.z, maxVector.z))
            };

            onRollVector.Invoke(randomVector);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minX"></param>
        public void SetMinX(float minX)
        {
            minVector.x = minX;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minY"></param>
        public void SetMinY(float minY)
        {
            minVector.y = minY;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minZ"></param>
        public void SetMinZ(float minZ)
        {
            minVector.z = minZ;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxX"></param>
        public void SetMaxX(float maxX)
        {
            maxVector.x = maxX;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxY"></param>
        public void SetMaxY(float maxY)
        {
            maxVector.y = maxY;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxZ"></param>
        public void SetMaxZ(float maxZ)
        {
            maxVector.z = maxZ;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minX"></param>
        public void IncrementMinX(float minX)
        {
            minVector.x += minX;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minY"></param>
        public void IncrementMinY(float minY)
        {
            minVector.y += minY;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="minZ"></param>
        public void IncrementMinZ(float minZ)
        {
            minVector.z += minZ;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxX"></param>
        public void IncrementMaxX(float maxX)
        {
            maxVector.x += maxX;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxY"></param>
        public void IncrementMaxY(float maxY)
        {
            maxVector.y += maxY;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxZ"></param>
        public void IncrementMaxZ(float maxZ)
        {
            maxVector.z += maxZ;
        }
    }
}