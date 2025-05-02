using System;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Networking
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
        public int damage;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Vector3 direction;

        /// <summary>
        ///     TODO.
        /// </summary>
        public int hitID;

        /// <summary>
        ///     TODO.
        /// </summary>
        public HitInfo()
        {
            damage = 1;
            direction = Vector3.zero;
            hitID = 1;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref damage);
            serializer.SerializeValue(ref direction);
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public abstract class NetworkedHittable : NetworkBehaviour, IHittable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Networked Hittable")]
        [Tooltip("")]
        public HitInfo defaultHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="hitDirection"></param>
        /// <param name="playerWhoHit"></param>
        /// <param name="playHitSFX"></param>
        /// <param name="hitID"></param>
        /// <returns></returns>
        public bool Hit(int force, Vector3 hitDirection, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            PerformHit(new()
            {
                damage = force,
                direction = hitDirection,
                hitID = hitID
            });

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PerformHit()
        {
            PerformHit(defaultHit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public void PerformHit(HitInfo hitInfo)
        {
            PerformHitLocal(hitInfo);
            PerformHitServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), hitInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="hitInfo"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PerformHitServerRpc(NetworkObjectReference playerReference, HitInfo hitInfo)
        {
            PerformHitClientRpc(playerReference, hitInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="hitInfo"></param>
        [ClientRpc]
        public void PerformHitClientRpc(NetworkObjectReference playerReference, HitInfo hitInfo)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                PerformHitLocal(hitInfo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public abstract void PerformHitLocal(HitInfo hitInfo);
    }
}