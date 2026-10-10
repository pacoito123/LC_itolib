using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct NestInfo() : INetworkSerializable, IEquatable<NestInfo>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Nest Info")]
        [Tooltip("")]
        public TransformInfo transformInfo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkObjectReference nestReference;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            transformInfo.NetworkSerialize(serializer);

            serializer.SerializeValue(ref nestReference);
        }

        /// <inheritdoc/>
        public readonly bool Equals(NestInfo other)
        {
            return transformInfo == other.transformInfo && nestReference.Equals(other.nestReference);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is NestInfo info && Equals(info);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(NestInfo left, NestInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(NestInfo left, NestInfo right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(transformInfo, nestReference);
        }
    }
}