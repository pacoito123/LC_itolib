using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class VehicleSensor : DetectRegion<VehicleController>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
        {
            maxObjects = 16;
            layerMask = LayerMask.GetMask("Vehicle");

            base.Reset();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            // TODO: Initialize something...
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out VehicleCollisionTrigger vehicle) && vehicle.mainScript != null
                && (vehicle.mainScript.localPlayerInControl || (vehicle.mainScript.currentDriver == null && NetworkManager.Singleton.IsHost)))
            {
                onRegionEntered.Invoke(vehicle.mainScript);

                if (IsSpawned)
                {
                    RegionEnteredRpc(vehicle.mainScript);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out VehicleCollisionTrigger vehicle) && vehicle.mainScript != null
                && (vehicle.mainScript.localPlayerInControl || (vehicle.mainScript.currentDriver == null && NetworkManager.Singleton.IsHost)))
            {
                onRegionExited.Invoke(vehicle.mainScript);

                if (IsSpawned)
                {
                    RegionEnteredRpc(vehicle.mainScript, exit: true);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="vehicleReference"></param>
        /// <param name="exit"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void RegionEnteredRpc(NetworkBehaviourReference vehicleReference, bool exit = false)
        {
            if (vehicleReference.TryGet(out VehicleController vehicle))
            {
                if (!exit)
                {
                    onRegionEntered.Invoke(vehicle);
                }
                else
                {
                    onRegionExited.Invoke(vehicle);
                }
            }
        }
    }
}