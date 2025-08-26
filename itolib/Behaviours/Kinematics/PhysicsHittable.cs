using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using itolib.Extensions;
using itolib.Structs;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PhysicsHittable : NetworkedHittable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Physics Hittable")]
        [Tooltip("")]
        public Rigidbody? hittableBody;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int hitForce = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ForceMode forceMode = ForceMode.Impulse;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public override void PerformHitLocal(HitInfo hitInfo)
        {
            if (hittableBody != null)
            {
                Vector3 forceToApply = hitInfo.hitByPlayer
                    ? hitForce * hitInfo.damage * hitInfo.direction
                    : hitForce * hitInfo.damage * (transform.rotation * hitInfo.direction);

                hittableBody.AddForce(forceToApply, forceMode);
            }

            onHit.Invoke();

            if (hitInfo.hitByPlayer && hitInfo.playerReference.TryGet(out PlayerControllerB player))
            {
                if (player.IsLocalClient())
                {
                    onPlayerHitLocal.Invoke(player);
                }

                onPlayerHit.Invoke(player);
            }
        }
    }
}