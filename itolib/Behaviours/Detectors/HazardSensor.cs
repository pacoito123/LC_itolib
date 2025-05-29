using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class HazardSensor : DetectRegion<GameObject>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            maxObjects = 8;
            layerMask = 1 << LayerMask.NameToLayer("MapHazards");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            base.CheckObjectsInRegion();

            if (!IsHost)
            {
                return;
            }

            int hazardsFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out NetworkObject hazardNetworkObject)
                    && hazardNetworkObject.IsSpawned)
                {
                    FoundHazardsEachClientRpc(hazardNetworkObject);
                }
            }

            if (hazardsFound > 0)
            {
                FoundHazardsAnyClientRpc(hazardsFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardReference"></param>
        [ClientRpc]
        public void FoundHazardsEachClientRpc(NetworkObjectReference hazardReference)
        {
            if (hazardReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out GameObject hazard))
            {
                onObjectsEach?.Invoke(hazard);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardsFound"></param>
        [ClientRpc]
        public void FoundHazardsAnyClientRpc(int hazardsFound)
        {
            onObjectsAny?.Invoke(hazardsFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDespawn"></param>
        public void DespawnHazard(GameObject hazardToDespawn)
        {
            if (IsHost && hazardToDespawn.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                hazardNetworkObject.Despawn(true);
            }
        }
    }
}