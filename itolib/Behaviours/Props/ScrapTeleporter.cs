using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
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
    public class ScrapTeleporter : NetworkBehaviour, ISeededScript<ScrapTeleporter>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private static List<GrabbableObject>? availableScrap;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scrap Teleporter")]
        [Tooltip("")]
        [SerializeField] private string[]? specificItems;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private List<Transform>? teleportPoints;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private List<BoxCollider>? teleportAreas;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int minAmount = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int maxAmount = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool fallToGround = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool randomizePosition = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool exhaustivePoints = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool exhaustiveAreas = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool seededRandom = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected bool performedActivation;

        /// <summary>
        ///     Cached instance of the current <c>ScrapTeleporter</c> as an <c>ISeededScript</c>, to avoid having to cast. 
        /// </summary>
        private ISeededScript<ScrapTeleporter> seededSelf;

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        private void Awake()
        {
            seededSelf = this;
        }

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
            availableScrap = null;

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="spawnedScrap"></param>
        private void ObtainSpawnedScrap(List<GrabbableObject> spawnedScrap)
        {
            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(ObtainSpawnedScrap);

            if (!IsHost || availableScrap?.Count > 0)
            {
                return;
            }

            availableScrap ??= [.. spawnedScrap];

            _ = availableScrap.RemoveAll(item => item == null || item.isInShipRoom || item.isInElevator || item.itemProperties == null
                || !item.itemProperties.isScrap || item is LungProp || item.TryGetComponent(out NavMeshAgent agent));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void TeleportScrap()
        {
            if (!performedActivation)
            {
                UnsubscribeFromEvents();

                performedActivation = true;
            }

            if (!IsHost || availableScrap == null || availableScrap.Count == 0)
            {
                return;
            }

            int itemsToTeleport = seededRandom ? seededSelf.GetSeededRandom().Next(minAmount, maxAmount + 1)
                : UnityEngine.Random.Range(minAmount, maxAmount + 1);

            for (int i = 0; i < itemsToTeleport; i++)
            {
                transform.GetPositionAndRotation(out Vector3 teleportPosition, out Quaternion teleportRotation);

                if (teleportPoints?.Count > 0)
                {
                    int positionIndex = seededRandom ? seededSelf.GetSeededRandom().Next(0, teleportPoints.Count)
                        : UnityEngine.Random.Range(0, teleportPoints.Count);

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
                    int areaIndex = seededRandom ? seededSelf.GetSeededRandom().Next(0, teleportAreas.Count)
                        : UnityEngine.Random.Range(0, teleportAreas.Count);
                    BoxCollider teleportArea = teleportAreas[areaIndex];

                    // TODO: Maybe find point in NavMesh instead?
                    Vector3 point = teleportArea.GetPointWithin(seededRandom ? seededSelf.GetSeededRandom() : null);
                    teleportPosition = teleportArea.transform.TransformPoint(point + teleportArea.center);

                    if (exhaustiveAreas)
                    {
                        teleportAreas.RemoveAt(areaIndex);
                    }
                }

                if (specificItems?.Length > 0)
                {
                    for (int j = 0; j < availableScrap.Count; j++)
                    {
                        if (availableScrap[j] == null || availableScrap[j].itemProperties == null)
                        {
                            continue;
                        }

                        bool foundItem = false;

                        for (int k = 0; k < specificItems.Length; k++)
                        {
                            if (specificItems[k].CompareOrdinal(availableScrap[j].itemProperties.itemName))
                            {
                                TeleportData teleport = new()
                                {
                                    position = teleportPosition,
                                    rotation = teleportRotation
                                };

                                TeleportScrapClientRpc(availableScrap[j], teleport);
                                availableScrap.RemoveAt(j);

                                foundItem = true;

                                break;
                            }
                        }

                        if (foundItem)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int index = seededRandom ? seededSelf.GetSeededRandom().Next(0, availableScrap.Count)
                        : UnityEngine.Random.Range(0, availableScrap.Count);
                    GrabbableObject? item = availableScrap[index];

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
                    availableScrap.RemoveAt(index);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        private void TeleportScrapClientRpc(NetworkBehaviourReference itemReference, TeleportData teleport)
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

        /// <summary>
        ///     Unsubscribe to the event that may have been subscribed to, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
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
        }
    }
}