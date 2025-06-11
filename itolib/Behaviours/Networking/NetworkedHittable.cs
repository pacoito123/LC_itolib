using GameNetcodeStuff;
using itolib.Extensions;
using System;
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
        public PlayerControllerB? playerWhoHit;

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
            playerWhoHit = null;
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
        public UnityEvent onHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onPlayerHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onPlayerHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="hitDirection"></param>
        /// <param name="playerWhoHit"></param>
        /// <param name="playHitSFX"></param>
        /// <param name="hitID"></param>
        /// <returns></returns>
        public virtual bool Hit(int force, Vector3 hitDirection, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            PerformHit(new()
            {
                damage = force,
                direction = hitDirection,
                playerWhoHit = playerWhoHit,
                hitID = hitID
            });

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void PerformHit()
        {
            PerformHit(defaultHit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public virtual void PerformHit(HitInfo hitInfo)
        {
            PerformHitLocal(hitInfo);
            PerformHitServerRpc(GameNetworkManager.Instance.localPlayerController, hitInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="hitInfo"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PerformHitServerRpc(NetworkBehaviourReference playerReference, HitInfo hitInfo)
        {
            PerformHitClientRpc(playerReference, hitInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="hitInfo"></param>
        [ClientRpc]
        public void PerformHitClientRpc(NetworkBehaviourReference playerReference, HitInfo hitInfo)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                PerformHitLocal(hitInfo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void PerformHitLocal()
        {
            PerformHitLocal(defaultHit);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public abstract void PerformHitLocal(HitInfo hitInfo);
    }
}