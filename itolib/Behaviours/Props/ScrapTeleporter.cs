using itolib.Enums;
using LethalLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct TeleportData : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public Vector3 position;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }
    }

    /// <summary>
    /// 	TODO.
    /// </summary>
    public class ScrapTeleporter : NetworkBehaviour
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random Random { get; internal set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public static List<GrabbableObject>? AvailableScrap { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scrap Teleporter")]
        [Tooltip("")]
        public List<string>? specificItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<Transform>? teleportPoints;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<BoxCollider>? teleportAreas;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int minAmount = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int maxAmount = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool fallToGround = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool randomizePosition = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustivePoints = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool exhaustiveAreas = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                return;
            }

            Random ??= new(StartOfRound.Instance.randomMapSeed + 55);
            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(ObtainSpawnedScrap);

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(TeleportScrap);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(TeleportScrap);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(TeleportScrap);
                    }
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
            Random = null!;

            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(ObtainSpawnedScrap);

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(TeleportScrap);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(TeleportScrap);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(TeleportScrap);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnedScrap"></param>
        public void ObtainSpawnedScrap(List<GrabbableObject> spawnedScrap)
        {
            if (!IsHost || AvailableScrap?.Count > 0)
            {
                return;
            }

            AvailableScrap ??= [.. spawnedScrap];

            _ = AvailableScrap.RemoveAll(item => item == null || item.isInShipRoom || item.isInElevator || item.itemProperties == null
                || !item.itemProperties.isScrap || item is LungProp || item.TryGetComponent(out NavMeshAgent agent));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void TeleportScrap()
        {
            if (!IsHost || AvailableScrap == null || AvailableScrap.Count == 0)
            {
                return;
            }

            int itemsToTeleport = Random.Next(minAmount, maxAmount + 1);

            for (int i = 0; i < itemsToTeleport; i++)
            {
                transform.GetPositionAndRotation(out Vector3 teleportPosition, out Quaternion teleportRotation);

                if (teleportPoints?.Count > 0)
                {
                    int positionIndex = Random.Next(0, teleportPoints.Count);

                    if (teleportPoints[positionIndex] != null)
                    {
                        teleportPoints[positionIndex].GetPositionAndRotation(out teleportPosition, out teleportRotation);
                    }

                    if (exhaustivePoints)
                    {
                        teleportPoints.RemoveAt(positionIndex);
                    }
                }
                else if (teleportAreas?.Count > 0)
                {
                    int areaIndex = Random.Next(0, teleportAreas.Count);
                    BoxCollider teleportArea = teleportAreas[areaIndex];

                    Vector3 extents = teleportArea.size / 2.0f;
                    Vector3 point = new(((float)Random.NextDouble() * 2 * extents.x) - extents.x,
                        ((float)Random.NextDouble() * 2 * extents.y) - extents.y,
                        ((float)Random.NextDouble() * 2 * extents.z) - extents.z);

                    teleportPosition = teleportArea.transform.TransformPoint(point + teleportArea.center); // TODO: Maybe find point in NavMesh instead?

                    if (exhaustiveAreas)
                    {
                        teleportAreas.RemoveAt(areaIndex);
                    }
                }

                if (specificItems?.Count > 0)
                {
                    for (int j = 0; j < AvailableScrap.Count; j++)
                    {
                        if (AvailableScrap[j] == null || AvailableScrap[j].itemProperties == null)
                        {
                            continue;
                        }

                        if (specificItems.FindIndex(specificItem => string.CompareOrdinal(AvailableScrap[j].itemProperties.itemName, specificItem) == 0) >= 0)
                        {
                            TeleportData teleport = new()
                            {
                                position = teleportPosition,
                                rotation = teleportRotation
                            };

                            TeleportScrapClientRpc(AvailableScrap[j], teleport);
                            AvailableScrap.RemoveAt(j);

                            break;
                        }
                    }
                }
                else
                {
                    int index = Random.Next(0, AvailableScrap.Count);
                    GrabbableObject? item = AvailableScrap[index];

                    if (item == null)
                    {
                        return;
                    }

                    TeleportData teleport = new()
                    {
                        position = teleportPosition,
                        rotation = item.transform.rotation
                    };

                    TeleportScrapClientRpc(item, teleport);
                    AvailableScrap.RemoveAt(index);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void TeleportScrapClientRpc(NetworkBehaviourReference itemReference, TeleportData teleport)
        {
            _ = StartCoroutine(TeleportScrapDelayed(itemReference, teleport));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="teleport"></param>
        /// <returns></returns>
        private IEnumerator TeleportScrapDelayed(NetworkBehaviourReference itemReference, TeleportData teleport)
        {
            GrabbableObject item;

            float startTime = Time.realtimeSinceStartup;
            while (!itemReference.TryGet(out item) && Time.realtimeSinceStartup - startTime < 8f)
            {
                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForEndOfFrame();

            if (item == null)
            {
                yield break;
            }

            item.fallTime = 1.0f;
            item.hasHitGround = true;
            item.reachedFloorTarget = true;

            item.transform.SetPositionAndRotation(teleport.position, teleport.rotation);

            item.startFallingPosition = teleport.position;
            item.targetFloorPosition = teleport.position;

            if (fallToGround)
            {
                item.FallToGround(randomizePosition);
            }
        }
    }
}