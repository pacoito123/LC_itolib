using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct ScanNodeInfo : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scan Node Info")]
        [Tooltip("")]
        public string headerText = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string subText = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int minRange = 5;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int maxRange = 7;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        public int creatureScanID = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        public int nodeType;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool requiresLineOfSight = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public ScanNodeInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref headerText);
            serializer.SerializeValue(ref subText);

            serializer.SerializeValue(ref minRange);
            serializer.SerializeValue(ref maxRange);

            serializer.SerializeValue(ref creatureScanID);
            serializer.SerializeValue(ref nodeType);

            serializer.SerializeValue(ref requiresLineOfSight);
        }
    }
}