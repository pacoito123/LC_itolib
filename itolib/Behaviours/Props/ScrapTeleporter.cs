using itolib.Enums;
using LethalLevelLoader;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class ScrapTeleporter : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static List<GrabbableObject>? AvailableScrap { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scrap Teleporter")]
        [Tooltip("")]
        public int minValue = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool allowOneHanded = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool allowTwoHanded = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.ScrapSpawn;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(ObtainSpawnedScrap);
            }

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(TeleportRandomScrap);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(TeleportRandomScrap);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(TeleportRandomScrap);
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            AvailableScrap = null;

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnedScrap"></param>
        public void ObtainSpawnedScrap(List<GrabbableObject> spawnedScrap)
        {
            if (!IsHost && AvailableScrap?.Count > 0)
            {
                return;
            }

            AvailableScrap ??= [.. spawnedScrap];
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void TeleportRandomScrap()
        {
            if (!IsHost)
            {
                return;
            }

            if (AvailableScrap?.Count > 0)
            {
                int index = Random.RandomRangeInt(0, AvailableScrap.Count);

                TeleportRandomScrapClientRpc(AvailableScrap[index].GetComponent<NetworkObject>());

                AvailableScrap.RemoveAt(index);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void TeleportRandomScrapClientRpc(NetworkObjectReference itemReference)
        {
            if (itemReference.TryGet(out NetworkObject itemNetworkObject)
                && itemNetworkObject.TryGetComponent(out GrabbableObject item))
            {
                item.transform.position = transform.position;

                item.startFallingPosition = transform.position + new Vector3(0.0f, item.itemProperties.verticalOffset, 0.0f);
                item.targetFloorPosition = transform.position;

                item.FallToGround();
            }
        }
    }
}