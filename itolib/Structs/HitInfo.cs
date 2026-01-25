using System;
using itolib.Enums;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct HitInfo : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Hit Info")]
        [Tooltip("")]
        public int damage = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 direction = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public WeaponHitID hitID = WeaponHitID.Shovel;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public bool hitByPlayer = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public NetworkBehaviourReference playerReference = default;

        /// <summary>
        ///     TODO.
        /// </summary>
        public HitInfo() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref damage);
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref hitID);
            serializer.SerializeValue(ref hitByPlayer);

            if (hitByPlayer)
            {
                serializer.SerializeValue(ref playerReference);
            }
        }
    }
}