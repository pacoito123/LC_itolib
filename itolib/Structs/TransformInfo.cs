using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     Represents a serialized position and rotation.
    /// </summary>
    [Serializable]
    public struct TransformInfo : INetworkSerializable, IEquatable<TransformInfo>
    {
        /// <summary>
        ///     Serialized position value.
        /// </summary>
        [Header("Transform Info")]
        [Tooltip("Serialized position value.")]
        public Vector3 position = Vector3.zero;

        /// <summary>
        ///     Serialized rotation value.
        /// </summary>
        [Tooltip("Serialized rotation value.")]
        public Quaternion rotation = Quaternion.identity;

        /// <summary>
        ///     Create <c>TransformInfo</c> with a given <c>Transform</c>.
        /// </summary>
        /// <param name="transform"><c>Transform</c> to copy the position and rotation of.</param>
        public TransformInfo(Transform transform)
        {
            if (transform != null)
            {
                transform.GetPositionAndRotation(out position, out rotation);
            }
        }

        /// <summary>
        ///     Create <c>TransformInfo</c> with a given position and rotation.
        /// </summary>
        /// <param name="position">Position to use.</param>
        /// <param name="rotation">Rotation to use.</param>
        public TransformInfo(Vector3 position = default, Quaternion rotation = default)
        {
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        ///     Provides bi-directional serialization to read and write the desired data to serialize this type.
        /// </summary>
        /// <typeparam name="T">Either <c>BufferSerializerReader</c> or <c>BufferSerializerWriter</c>, depending whether the serializer is in read mode or write mode.</typeparam>
        /// <param name="serializer">The serializer to use to read and write the data.</param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }

        /// <inheritdoc/>
        public readonly bool Equals(TransformInfo other)
        {
            return position == other.position && rotation == other.rotation;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is TransformInfo info && Equals(info);
        }

        /// <summary>
        ///     Overload equality operator (<c>==</c>).
        /// </summary>
        /// <param name="left">Left side <c>TransformInfo</c> to compare.</param>
        /// <param name="right">Right side <c>TransformInfo</c> to compare.</param>
        /// <returns>Whether the two sides are equal or not.</returns>
        public static bool operator ==(TransformInfo left, TransformInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Overload inequality operator (<c>!=</c>).
        /// </summary>
        /// <param name="left">Left side <c>TransformInfo</c> to compare.</param>
        /// <param name="right">Right side <c>TransformInfo</c> to compare.</param>
        /// <returns>Whether the two sides are <i>not</i> equal or not.</returns>
        public static bool operator !=(TransformInfo left, TransformInfo right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(position, rotation);
        }
    }
}