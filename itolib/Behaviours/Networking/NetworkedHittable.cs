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
        public int hitID = 1;

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
        [SerializeField] private HitInfo defaultHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5)]
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] protected UnityEvent onHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onPlayerHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onPlayerHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="force"></param>
        /// <param name="hitDirection"></param>
        /// <param name="playerWhoHit"></param>
        /// <param name="playHitSFX"></param>
        /// <param name="hitID"></param>
        /// <returns></returns>
        public virtual bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null!, bool playHitSFX = false, int hitID = -1)
        {
            HitInfo hitInfo = new()
            {
                damage = force,
                direction = hitDirection,
                hitID = hitID,
                hitByPlayer = playerWhoHit != null
            };

            if (hitInfo.hitByPlayer)
            {
                hitInfo.playerReference = playerWhoHit;
            }

            PerformHit(hitInfo);

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
        private void PerformHitServerRpc(NetworkBehaviourReference playerReference, HitInfo hitInfo)
        {
            PerformHitClientRpc(playerReference, hitInfo);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="hitInfo"></param>
        [ClientRpc]
        private void PerformHitClientRpc(NetworkBehaviourReference playerReference, HitInfo hitInfo)
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