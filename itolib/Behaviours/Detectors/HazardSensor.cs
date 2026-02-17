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
        protected override void Reset()
        {
            maxObjects = 8;
            layerMask = LayerMask.NameToLayer("MapHazards");

            base.Reset();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            /* if (!NetworkManager.Singleton.IsHost && regionCollider != null)
            {
                regionCollider.enabled = false;
            } */
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            int hazardsFound = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? hazardCollider = overlapBuffer?[i];

                if (hazardCollider == null || !hazardCollider.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                if (hazardCollider.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject)
                    && hazardNetworkObject.IsSpawned)
                {
                    FoundHazardsEachRpc(hazardNetworkObject);
                    hazardsFound++;
                }
            }

            if (hazardsFound > 0)
            {
                FoundHazardsAnyRpc(hazardsFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardReference"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundHazardsEachRpc(NetworkObjectReference hazardReference)
        {
            if (hazardReference.TryGet(out NetworkObject hazardNetworkObject))
            {
                onObjectsEach.Invoke(hazardNetworkObject.gameObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardsFound"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundHazardsAnyRpc(int hazardsFound)
        {
            onObjectsAny.Invoke(hazardsFound);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDespawn"></param>
        public void DespawnHazard(GameObject hazardToDespawn)
        {
            if (IsHost && hazardToDespawn != null && hazardToDespawn.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                hazardNetworkObject.Despawn(true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDeactivate"></param>
        public void DeactivateHazard(GameObject hazardToDeactivate)
        {
            if (IsHost && hazardToDeactivate != null && hazardToDeactivate.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                TerminalAccessibleObject? terminalCode = hazardNetworkObject.GetComponentInChildren<TerminalAccessibleObject>();

                if (terminalCode != null)
                {
                    terminalCode.CallFunctionFromTerminal();
                }
            }
        }
    }
}