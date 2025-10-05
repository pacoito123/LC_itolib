using GameNetcodeStuff;
using itolib.Structs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Networking
{
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

            if (IsSpawned)
            {
                PerformHitRpc(hitInfo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PerformHitRpc(HitInfo hitInfo)
        {
            PerformHitLocal(hitInfo);
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