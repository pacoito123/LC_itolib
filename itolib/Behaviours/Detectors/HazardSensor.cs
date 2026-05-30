using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class HazardSensor : DetectRegion<NetworkBehaviour>
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

                IIndoorMapHazard possibleHazard = hazardCollider.transform.root.GetComponentInChildren<IIndoorMapHazard>(includeInactive: true);
                if (possibleHazard is NetworkBehaviour hazardBehaviour && hazardBehaviour.IsSpawned)
                {
                    FoundHazardsEachRpc(hazardBehaviour);
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
        /// <param name="other"></param>
        protected override void OnTriggerEnter(Collider other)
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            IIndoorMapHazard possibleHazard = other.transform.root.GetComponentInChildren<IIndoorMapHazard>(includeInactive: true);
            if (possibleHazard is NetworkBehaviour hazardBehaviour && hazardBehaviour.IsSpawned)
            {
                RegionEnteredRpc(hazardBehaviour);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerExit(Collider other)
        {
            if (!IsSpawned || !IsHost)
            {
                return;
            }

            IIndoorMapHazard possibleHazard = other.transform.root.GetComponentInChildren<IIndoorMapHazard>(includeInactive: true);
            if (possibleHazard is NetworkBehaviour hazardBehaviour && hazardBehaviour.IsSpawned)
            {
                RegionEnteredRpc(hazardBehaviour, exiting: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardReference"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void FoundHazardsEachRpc(NetworkBehaviourReference hazardReference)
        {
            if (hazardReference.TryGet(out NetworkBehaviour hazardBehaviour))
            {
                onObjectsEach.Invoke(hazardBehaviour);
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
        /// <param name="hazardReference"></param>
        /// <param name="exiting"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void RegionEnteredRpc(NetworkBehaviourReference hazardReference, bool exiting = false)
        {
            if (hazardReference.TryGet(out EnemyAI hazardBehaviour))
            {
                OnRegionEnter(hazardBehaviour, exiting);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDespawn"></param>
        public void DespawnHazard(GameObject hazardToDespawn)
        {
            if (hazardToDespawn != null && hazardToDespawn.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                DespawnHazard(hazardNetworkObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDespawn"></param>
        public void DespawnHazard(NetworkObject hazardToDespawn)
        {
            if (IsHost && hazardToDespawn != null && hazardToDespawn.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                hazardNetworkObject.Despawn(destroy: true);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDeactivate"></param>
        public void DeactivateHazard(GameObject hazardToDeactivate)
        {
            if (hazardToDeactivate != null && hazardToDeactivate.transform.root.TryGetComponent(out NetworkObject hazardNetworkObject))
            {
                DeactivateHazard(hazardNetworkObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hazardToDeactivate"></param>
        public void DeactivateHazard(NetworkObject hazardToDeactivate)
        {
            if (IsHost && hazardToDeactivate != null)
            {
                TerminalAccessibleObject? terminalCode = hazardToDeactivate.GetComponentInChildren<TerminalAccessibleObject>();

                if (terminalCode != null)
                {
                    terminalCode.CallFunctionFromTerminal();
                }
            }
        }
    }
}