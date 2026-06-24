using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct ItemInfo() : INetworkSerializable, IEquatable<ItemInfo>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Info")]
        [Tooltip("")]
        public TransformInfo transformInfo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference itemReference;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int scrapValue;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        public int meshVariant = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        public int materialVariant = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            transformInfo.NetworkSerialize(serializer);

            serializer.SerializeValue(ref itemReference);
            serializer.SerializeValue(ref scrapValue);
            serializer.SerializeValue(ref meshVariant);
            serializer.SerializeValue(ref materialVariant);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool Equals(ItemInfo other)
        {
            return transformInfo == other.transformInfo && itemReference.Equals(other.itemReference)
                && scrapValue == other.scrapValue && meshVariant == other.meshVariant && materialVariant == other.materialVariant;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override readonly bool Equals(object obj)
        {
            return obj is ItemInfo info && Equals(info);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ItemInfo left, ItemInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ItemInfo left, ItemInfo right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(transformInfo, itemReference, scrapValue, meshVariant, materialVariant);
        }
    }
}