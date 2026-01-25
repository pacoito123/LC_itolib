using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Structs;
using System;
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
        [Space(5.0f)]
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
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onShovelHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onShovelHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onKnifeHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onKnifeHitLocal = new();

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
                hitID = Enum.IsDefined(typeof(WeaponHitID), hitID) ? (WeaponHitID)hitID : WeaponHitID.None,
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
        public virtual void PerformHitLocal(HitInfo hitInfo)
        {
            onHit.Invoke();

            if (hitInfo.hitByPlayer && hitInfo.playerReference.TryGet(out PlayerControllerB player))
            {
                if (player.IsLocalClient())
                {
                    switch (hitInfo.hitID)
                    {
                        case WeaponHitID.Shovel:
                            onShovelHitLocal.Invoke(player);
                            break;
                        case WeaponHitID.Knife:
                            onKnifeHitLocal.Invoke(player);
                            break;
                        case WeaponHitID.None:
                        default:
                            break;
                    }

                    onPlayerHitLocal.Invoke(player);
                }

                switch (hitInfo.hitID)
                {
                    case WeaponHitID.Shovel:
                        onShovelHit.Invoke(player);
                        break;
                    case WeaponHitID.Knife:
                        onKnifeHit.Invoke(player);
                        break;
                    case WeaponHitID.None:
                    default:
                        break;
                }

                onPlayerHit.Invoke(player);
            }
        }
    }
}