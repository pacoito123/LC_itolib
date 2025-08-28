using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct TransformInfo : INetworkSerializable, IEquatable<TransformInfo>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Transform Info")]
        [Tooltip("")]
        public Vector3 position = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Quaternion rotation = Quaternion.identity;

        /// <summary>
        ///     TODO.
        /// </summary>
        public TransformInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="transform"></param>
        public TransformInfo(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
        }

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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool Equals(TransformInfo other)
        {
            return position == other.position && rotation == other.rotation;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(position, rotation);
        }
    }
}