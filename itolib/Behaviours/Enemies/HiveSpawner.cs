using itolib.Enums;
using itolib.Extensions;
using itolib.Structs;
using LethalLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct SyncedHive : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Synced Hive")]
        [Tooltip("")]
        public ItemInfo itemInfo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Hive Override")]
        [Tooltip("")]
        public bool overrideHive;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public NetworkBehaviourReference hiveReference = default;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Scan Nodes")]
        [Tooltip("")]
        public bool overrideBeesScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ScanNodeInfo beesScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool overrideHiveScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ScanNodeInfo hiveScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        public SyncedHive() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            itemInfo.NetworkSerialize(serializer);

            serializer.SerializeValue(ref overrideHive);
            serializer.SerializeValue(ref overrideBeesScanNode);
            serializer.SerializeValue(ref overrideHiveScanNode);

            if (overrideHive)
            {
                serializer.SerializeValue(ref hiveReference);
            }

            if (overrideBeesScanNode)
            {
                beesScanNode.NetworkSerialize(serializer);
            }

            if (overrideHiveScanNode)
            {
                hiveScanNode.NetworkSerialize(serializer);
            }
        }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class HiveSpawner : EnemySpawnerBase<RedLocustBees>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static EnemyType? CircuitBeesEnemyType
        {
            get
            {
                if (field == null)
                {
                    CircuitBeesEnemyType = OriginalContent.Enemies.Find(enemy => enemy.enemyName.CompareOrdinal("Red Locust Bees"));
                }

                return field;
            }
            private set;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<SyncedHive> HivesToSync { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Hive Spawner")]
        [Tooltip("")]
        [SerializeField] private Item? overrideHive;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int hiveMinValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int hiveMaxValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private bool applyScrapMultiplier = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int minDistanceFromShip = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int maxDistanceFromShip = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AnimationCurve distanceFromShipValueCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Scan Node")]
        [Tooltip("")]
        [SerializeField] private bool overrideBeesScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ScanNodeInfo beesScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool overrideHiveScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ScanNodeInfo hiveScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return (CircuitBeesEnemyType != null && CircuitBeesEnemyType.enemyPrefab != null)
                ? CircuitBeesEnemyType.enemyPrefab.GetComponent<NetworkObject>() : null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PerformSpawn()
        {
            base.PerformSpawn();

            if (activationTime is not ActivationTime.ScrapSpawn or ActivationTime.HazardSpawn)
            {
                SyncHiveValuesServerRpc();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="bees"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        /// <returns></returns>
        protected override bool AdditionalProcessing(RedLocustBees bees, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            int scrapValue = -1;

            if (hiveMinValue != -1 && hiveMaxValue != -1)
            {
                scrapValue = isSeededRandom ? seededSelf.GetSeededRandom().Next(hiveMinValue, hiveMaxValue)
                    : UnityEngine.Random.RandomRangeInt(hiveMinValue, hiveMaxValue);
            }
            else if (overrideHive != null && overrideHive.spawnPrefab != null)
            {
                scrapValue = isSeededRandom ? seededSelf.GetSeededRandom().Next(overrideHive.minValue, overrideHive.maxValue)
                        : UnityEngine.Random.RandomRangeInt(overrideHive.minValue, overrideHive.maxValue);
            }

            if (scrapValue != -1)
            {
                if (RoundManager.Instance != null && applyScrapMultiplier)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                if (minDistanceFromShip != -1 && maxDistanceFromShip != -1)
                {
                    float distanceFromShip = Vector3.Distance(spawnPosition, StartOfRound.Instance.shipLandingPosition.position),
                        distanceTime = (distanceFromShip <= minDistanceFromShip) ? 0.0f
                            : (distanceFromShip >= maxDistanceFromShip) ? 1.0f
                            : distanceFromShip / maxDistanceFromShip;

                    scrapValue = (int)(scrapValue * distanceFromShipValueCurve.Evaluate(distanceTime));
                }
            }

            SyncedHive serializedHive = new()
            {
                itemInfo = new()
                {
                    transformInfo = new()
                    {
                        position = spawnPosition,
                        rotation = spawnRotation
                    },
                    scrapValue = scrapValue,
                    meshVariant = -1,
                    materialVariant = -1
                }
            };

            if (overrideHive != null && overrideHive.spawnPrefab != null)
            {
                GameObject hivePrefab = Instantiate(overrideHive.spawnPrefab, spawnPosition, spawnRotation,
                    RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null);

                if (hivePrefab.TryGetComponent(out NetworkObject newHiveNetworkObject)
                    && hivePrefab.TryGetComponent(out GrabbableObject newHive))
                {
                    newHiveNetworkObject.Spawn(true);

                    serializedHive.overrideHive = true;
                    serializedHive.hiveReference = newHive;
                }
            }

            if (overrideBeesScanNode)
            {
                serializedHive.overrideBeesScanNode = true;
                serializedHive.beesScanNode = beesScanNode;
            }

            if (overrideHiveScanNode)
            {
                serializedHive.overrideHiveScanNode = true;
                serializedHive.hiveScanNode = hiveScanNode;
            }

            HivesToSync.Add(serializedHive);

            return base.AdditionalProcessing(bees, spawnPosition, spawnRotation);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            beesScanNode = new()
            {
                headerText = "Circuit bees",
                subText = string.Empty,
                minRange = 1,
                maxRange = 20,
                creatureScanID = 14,
                nodeType = 1,
                requiresLineOfSight = true
            };

            hiveScanNode = new()
            {
                headerText = "Bee hive",
                subText = "Value: ",
                minRange = 1,
                maxRange = 13,
                creatureScanID = -1,
                nodeType = 2,
                requiresLineOfSight = true
            };
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (overrideHive != null && overrideHive.spawnPrefab == null)
                {
                    ExtendedItem? extendedHive = PatchedContent.ExtendedItems.Find(extendedItem =>
                        extendedItem.Item.name.CompareOrdinal(overrideHive.name));

                    if (extendedHive != null)
                    {
                        overrideHive = extendedHive.Item;
                    }
                    else
                    {
                        // TODO: Log warning.
                    }
                }
            }

            base.Awake();
        }

        /// <summary>
        ///     TOOD.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost && activationTime is ActivationTime.HazardSpawn or ActivationTime.ScrapSpawn && StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncHiveValuesServerRpc);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc]
        public void SyncHiveValuesServerRpc()
        {
            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncHiveValuesServerRpc);
            }

            for (int i = 0; i < PrefabInstances.Count; i++)
            {
                if (PrefabInstances[i] != null)
                {
                    SyncHiveValuesClientRpc(PrefabInstances[i], HivesToSync[i]);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hiveReplacement"></param>
        public void SwitchHiveOverride(Item? hiveReplacement)
        {
            if (hiveReplacement == null)
            {
                overrideHive = null;
            }
            else if (hiveReplacement.spawnPrefab != null)
            {
                overrideHive = hiveReplacement;
            }
            else
            {
                ExtendedItem? extendedHive = PatchedContent.ExtendedItems.Find(extendedItem =>
                    extendedItem.Item.name.CompareOrdinal(hiveReplacement.name));

                overrideHive = extendedHive != null ? extendedHive.Item : null;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="beesReference"></param>
        /// <param name="syncedHive"></param>
        [ClientRpc]
        public void SyncHiveValuesClientRpc(NetworkBehaviourReference beesReference, SyncedHive syncedHive)
        {
            _ = StartCoroutine(SyncHiveValuesOnSpawn(beesReference, syncedHive));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="beesReference"></param>
        /// <param name="syncedHive"></param>
        /// <returns></returns>
        private static IEnumerator SyncHiveValuesOnSpawn(NetworkBehaviourReference beesReference, SyncedHive syncedHive)
        {
            RedLocustBees bees;

            float startTime = Time.realtimeSinceStartup;
            while ((!beesReference.TryGet(out bees) || bees.hive == null || !bees.hive.IsSpawned)
                && Time.realtimeSinceStartup - startTime < 8f)
            {
                yield return new WaitForSeconds(0.03f); // TODO: Replace with better method.
            }

            yield return new WaitForEndOfFrame();

            if (bees == null || bees.hive == null)
            {
                // TODO: Log warning.
                yield break;
            }

            Vector3 spawnPosition = syncedHive.itemInfo.transformInfo.position;
            Quaternion spawnRotation = syncedHive.itemInfo.transformInfo.rotation;

            if (bees.agent != null && bees.agent.Warp(spawnPosition))
            {
                bees.transform.position = spawnPosition;
                bees.serverPosition = spawnPosition;

                bees.OnSyncPositionFromServer(spawnPosition);
            }

            if (syncedHive.overrideBeesScanNode)
            {
                ScanNodeProperties? beesScanNode = bees.GetComponentInChildren<ScanNodeProperties>();
                ScanNodeInfo beesScanNodeInfo = syncedHive.beesScanNode;

                if (beesScanNode != null)
                {
                    beesScanNode.headerText = beesScanNodeInfo.headerText;
                    beesScanNode.subText = beesScanNodeInfo.subText;
                    beesScanNode.minRange = beesScanNodeInfo.minRange;
                    beesScanNode.maxRange = beesScanNodeInfo.maxRange;
                    beesScanNode.creatureScanID = beesScanNodeInfo.creatureScanID;
                    beesScanNode.nodeType = beesScanNodeInfo.nodeType;
                    beesScanNode.requiresLineOfSight = beesScanNodeInfo.requiresLineOfSight;
                }
            }

            GrabbableObject originalHive = bees.hive;

            if (syncedHive.overrideHive)
            {
                GrabbableObject? newHive;

                startTime = Time.realtimeSinceStartup;
                while (!syncedHive.hiveReference.TryGet(out newHive) && Time.realtimeSinceStartup - startTime < 8f)
                {
                    yield return new WaitForSeconds(0.03f); // TODO: Replace with better method.
                }

                yield return new WaitForEndOfFrame();

                if (newHive != null)
                {
                    // Vanilla item "despawning":
                    originalHive.deactivated = true;

                    if (originalHive.radarIcon != null)
                    {
                        Destroy(originalHive.radarIcon.gameObject);
                    }

                    foreach (Renderer renderer in originalHive.GetComponentsInChildren<Renderer>())
                    {
                        renderer.enabled = false;
                    }

                    foreach (Collider collider in originalHive.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                    // ...

                    bees.hive = newHive;
                }
            }

            bees.lastKnownHivePosition = spawnPosition;
            bees.syncedLastKnownHivePosition = true;

            bees.hive.fallTime = 1.0f;
            bees.hive.hasHitGround = true;
            bees.hive.reachedFloorTarget = true;

            bees.hive.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            bees.hive.startFallingPosition = spawnPosition;
            bees.hive.targetFloorPosition = spawnPosition;

            if (syncedHive.overrideHiveScanNode)
            {
                ScanNodeProperties? hiveScanNode = bees.hive.GetComponentInChildren<ScanNodeProperties>();
                ScanNodeInfo hiveScanNodeInfo = syncedHive.hiveScanNode;

                if (hiveScanNode != null)
                {
                    hiveScanNode.headerText = hiveScanNodeInfo.headerText;
                    hiveScanNode.subText = hiveScanNodeInfo.subText;
                    hiveScanNode.minRange = hiveScanNodeInfo.minRange;
                    hiveScanNode.maxRange = hiveScanNodeInfo.maxRange;
                    hiveScanNode.creatureScanID = hiveScanNodeInfo.creatureScanID;
                    hiveScanNode.nodeType = hiveScanNodeInfo.nodeType;
                    hiveScanNode.requiresLineOfSight = hiveScanNodeInfo.requiresLineOfSight;
                }
            }

            if (syncedHive.itemInfo.scrapValue != -1)
            {
                bees.hive.SetScrapValue(syncedHive.itemInfo.scrapValue);
            }

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.totalScrapValueInLevel -= originalHive.scrapValue;

                if (RoundManager.Instance.totalScrapValueInLevel < 0)
                {
                    RoundManager.Instance.totalScrapValueInLevel = 0;
                }

                RoundManager.Instance.totalScrapValueInLevel += syncedHive.itemInfo.scrapValue;
            }
        }
    }
}