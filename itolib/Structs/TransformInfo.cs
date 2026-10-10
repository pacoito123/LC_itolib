using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <param name="transform"></param>
    [Serializable]
    public struct TransformInfo(Transform transform) : INetworkSerializable, IEquatable<TransformInfo>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Transform Info")]
        [Tooltip("")]
        public Vector3 position = transform.position;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Quaternion rotation = transform.rotation;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
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
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(TransformInfo left, TransformInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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