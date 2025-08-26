using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public struct ItemInfo : INetworkSerializable
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
        public ItemInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            transformInfo.NetworkSerialize(serializer);

            serializer.SerializeValue(ref scrapValue);
            serializer.SerializeValue(ref meshVariant);
            serializer.SerializeValue(ref materialVariant);
        }
    }
}