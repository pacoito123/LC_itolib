using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerSensor : DetectRegion<PlayerControllerB>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Sensor")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onPlayersAliveEach;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            maxObjects = 4;
            layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("PlayerRagdoll"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            base.CheckObjectsInRegion();

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out PlayerControllerB player))
                {
                    onObjectsEach?.Invoke(player);

                    if (player.isActiveAndEnabled && !player.isPlayerDead)
                    {
                        onPlayersAliveEach?.Invoke(player);
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerControllerB player))
            {
                onRegionExited?.Invoke(player);
            }
        }
    }
}