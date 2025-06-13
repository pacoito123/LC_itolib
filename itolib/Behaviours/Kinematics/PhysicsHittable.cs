using itolib.Behaviours.Networking;
using itolib.Extensions;
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
                hittableBody.AddForce(hitForce * hitInfo.damage * hitInfo.direction, forceMode);
            }

            onHit.Invoke();

            if (hitInfo.playerWhoHit != null)
            {
                onPlayerHit.Invoke(hitInfo.playerWhoHit);

                if (hitInfo.playerWhoHit.IsLocalClient())
                {
                    onPlayerHitLocal.Invoke(hitInfo.playerWhoHit);
                }
            }
        }
    }
}