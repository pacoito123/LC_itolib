using GameNetcodeStuff;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlatformGravity : PlayerPhysicsRegion
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Gravity Platform")]
        [Tooltip("")]
        public bool applyReducedMotion = false;

        private void Awake()
        {
            if (applyReducedMotion)
            {
                disablePhysicsRegion = true;
            }
        }

        /* private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.useGravity = false;
                rigidbody.constraints = (RigidbodyConstraints)4;

                Plugin.StaticLogger.LogInfo("Enter");
            }
        } */

        /* private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.useGravity = true;
                rigidbody.constraints = 0;

                Plugin.StaticLogger.LogInfo("Exit");
            }
        } */

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public new bool IsPhysicsRegionActive()
        {
            return !disablePhysicsRegion;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public static void CancelPlayerMomentum(PlayerControllerB player)
        {
            if (player.IsOwner)
            {
                player.transform.SetParent(null);
            }
        }
    }
}