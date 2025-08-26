using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct TransformInfo : INetworkSerializable
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
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }
    }
}