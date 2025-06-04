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
                if (OverlapBuffer![i]?.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject) == true
                    && hazardNetworkObject.IsSpawned)
                {
                    FoundHazardsEachClientRpc(hazardNetworkObject);
                    hazardsFound++;
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
            if (hazardReference.TryGet(out NetworkObject hazardNetworkObject))
            {
                onObjectsEach?.Invoke(hazardNetworkObject.gameObject);
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
            if (IsHost && hazardToDespawn?.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject) == true)
            {
                hazardNetworkObject.Despawn(true);
            }
        }
    }
}