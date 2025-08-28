using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct HiveInfo : INetworkSerializable, IEquatable<HiveInfo>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Hive Info")]
        [Tooltip("")]
        public ItemInfo itemInfo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference beesReference;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Hive Override")]
        [Tooltip("")]
        public bool overrideHive;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference hiveReference;

        /// <summary>
        ///     TODO.
        /// </summary>
        public HiveInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            itemInfo.NetworkSerialize(serializer);

            serializer.SerializeValue(ref beesReference);
            serializer.SerializeValue(ref overrideHive);

            if (overrideHive)
            {
                serializer.SerializeValue(ref hiveReference);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool Equals(HiveInfo other)
        {
            return itemInfo == other.itemInfo && beesReference.Equals(other.beesReference)
                && overrideHive == other.overrideHive && hiveReference.Equals(other.hiveReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override readonly bool Equals(object obj)
        {
            return obj is HiveInfo info && Equals(info);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(HiveInfo left, HiveInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(HiveInfo left, HiveInfo right)
        {
            return !(left == right);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(itemInfo, beesReference, overrideHive, hiveReference);
        }
    }
}