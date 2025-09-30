using DunGen;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Structs;
using LethalLevelLoader;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class ScrapTeleporter : NetworkBehaviour, IActivationScript, ISeededScript<ScrapTeleporter>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private static List<GrabbableObject>? availableScrap;

        /// <summary>
        ///     TODO.
        /// </summary>
        public NetworkList<ItemInfo> SyncedItems { get; private set; } = null!;

        /// <summary>
        ///     Cached instance of the current <c>ScrapTeleporter</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>ScrapTeleporter</c> as an <c>ISeededScript</c>, to avoid having to cast.
        /// </summary>
        public ISeededScript<ScrapTeleporter> SeededSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

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
        ///     Desired <c>ActivationTime</c> for the teleport to be performed.
        /// </summary>
        [field: Tooltip("Desired activation time for the teleport to be performed.")]
        [field: FormerlySerializedAs("activationTime")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the teleport to be performed.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the teleport to be performed. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> and <c>ISeededScript</c> instances.
        /// </summary>
        private ScrapTeleporter()
        {
            ActivationSelf = this;
            SeededSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (activationTime is not ActivationTime.StartOfRound)
            {
                ActivationTime = activationTime;
            }

            SyncedItems = new();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedItems.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<ItemInfo>.EventType.Add)
                {
                    SyncTeleportedItem(changeEvent.Value);
                }
            };

            if (!IsHost)
            {
                return;
            }

            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(ObtainSpawnedScrap);

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            availableScrap = null;

            ActivationSelf.UnsubscribeFromEvents();

            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(ObtainSpawnedScrap);

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
            if (!IsHost || availableScrap == null || availableScrap.Count == 0)
            {
                return;
            }

            int itemsToTeleport = seededRandom ? SeededSelf.GetSeededRandom().Next(minAmount, maxAmount + 1)
                : Random.Range(minAmount, maxAmount + 1);

            for (int i = 0; i < itemsToTeleport; i++)
            {
                transform.GetPositionAndRotation(out Vector3 teleportPosition, out Quaternion teleportRotation);

                if (teleportPoints?.Count > 0)
                {
                    int positionIndex = seededRandom ? SeededSelf.GetSeededRandom().Next(0, teleportPoints.Count)
                        : Random.Range(0, teleportPoints.Count);

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
                    int areaIndex = seededRandom ? SeededSelf.GetSeededRandom().Next(0, teleportAreas.Count)
                        : Random.Range(0, teleportAreas.Count);
                    BoxCollider teleportArea = teleportAreas[areaIndex];

                    // TODO: Maybe find point in NavMesh instead?
                    Vector3 point = teleportArea.GetPointWithin(seededRandom ? SeededSelf.GetSeededRandom() : null);
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
                                GrabbableObject? item = availableScrap[j];

                                if (item == null || !item.IsSpawned)
                                {
                                    continue;
                                }

                                ItemInfo syncedItem = new()
                                {
                                    transformInfo = new()
                                    {
                                        position = teleportPosition,
                                        rotation = teleportRotation
                                    },
                                    itemReference = item
                                };

                                SyncedItems.Add(syncedItem);
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
                    int index = seededRandom ? SeededSelf.GetSeededRandom().Next(0, availableScrap.Count)
                        : Random.Range(0, availableScrap.Count);
                    GrabbableObject? item = availableScrap[index];

                    if (item == null || !item.IsSpawned)
                    {
                        return;
                    }

                    ItemInfo syncedItem = new()
                    {
                        transformInfo = new()
                        {
                            position = teleportPosition,
                            rotation = item.transform.rotation
                        },
                        itemReference = item
                    };

                    SyncedItems.Add(syncedItem);
                    availableScrap.RemoveAt(index);
                }
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            if (activationTime != ActivationTime.Immediate)
            {
                TeleportScrap();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="syncedItem"></param>
        /// <returns></returns>
        private void SyncTeleportedItem(ItemInfo syncedItem)
        {
            if (!syncedItem.itemReference.TryGet(out GrabbableObject item))
            {
                return;
            }

            item.fallTime = 1.0f;
            item.hasHitGround = true;
            item.reachedFloorTarget = true;

            item.transform.SetPositionAndRotation(syncedItem.transformInfo.position, syncedItem.transformInfo.rotation);

            item.startFallingPosition = syncedItem.transformInfo.position;
            item.targetFloorPosition = syncedItem.transformInfo.position;

            if (fallToGround)
            {
                item.FallToGround(randomizePosition);
            }
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon) { }
    }
}