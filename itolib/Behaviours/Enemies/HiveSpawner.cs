using itolib.Extensions;
using itolib.Structs;
using itolib.Util;
using LethalLevelLoader;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
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
                    field = OriginalContent.Enemies.Find(enemy => enemy.enemyName.CompareOrdinal("Red Locust Bees"));
                }

                return field;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public NetworkList<HiveInfo> SyncedHives { get; private set; } = null!;

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
        /// <param name="bees"></param>
        /// <param name="spawnLocation"></param>
        protected override void SpawnPerformed(RedLocustBees? bees, TransformInfo spawnLocation)
        {
            if (bees == null || !bees.IsSpawned)
            {
                return;
            }

            _ = StartCoroutine(WaitForHiveSpawn(bees, spawnLocation));

            base.SpawnPerformed(bees, spawnLocation);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="bees"></param>
        /// <param name="spawnLocation"></param>
        /// <returns></returns>
        private IEnumerator WaitForHiveSpawn(RedLocustBees bees, TransformInfo spawnLocation)
        {
            float startTime = Time.realtimeSinceStartup;

            while (!bees.hasSpawnedHive && Time.realtimeSinceStartup - startTime < 8.0f)
            {
                yield return Yielders.WaitForSeconds(0.5f);
            }

            yield return Yielders.WaitForEndOfFrame;

            if (bees.hive == null)
            {
                yield break;
            }

            int scrapValue = -1;

            if (hiveMinValue != -1 && hiveMaxValue != -1)
            {
                scrapValue = isSeededRandom ? SeededSelf.GetSeededRandom().Next(hiveMinValue, hiveMaxValue)
                    : Random.RandomRangeInt(hiveMinValue, hiveMaxValue);
            }
            else if (overrideHive != null && overrideHive.spawnPrefab != null)
            {
                scrapValue = isSeededRandom ? SeededSelf.GetSeededRandom().Next(overrideHive.minValue, overrideHive.maxValue)
                        : Random.RandomRangeInt(overrideHive.minValue, overrideHive.maxValue);
            }

            if (scrapValue != -1)
            {
                if (RoundManager.Instance != null && applyScrapMultiplier)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }

                if (minDistanceFromShip != -1 && maxDistanceFromShip != -1)
                {
                    float distanceFromShip = Vector3.Distance(spawnLocation.position, StartOfRound.Instance.shipLandingPosition.position),
                        distanceTime = (distanceFromShip <= minDistanceFromShip) ? 0.0f
                            : (distanceFromShip >= maxDistanceFromShip) ? 1.0f
                            : distanceFromShip / maxDistanceFromShip;

                    scrapValue = (int)(scrapValue * distanceFromShipValueCurve.Evaluate(distanceTime));
                }
            }

            HiveInfo serializedHive = new()
            {
                itemInfo = new()
                {
                    transformInfo = spawnLocation,
                    itemReference = bees.hive,
                    scrapValue = scrapValue,
                    meshVariant = -1,
                    materialVariant = -1
                },
                beesReference = bees
            };

            if (overrideHive != null && overrideHive.spawnPrefab != null)
            {
                GameObject hivePrefab = Instantiate(overrideHive.spawnPrefab, spawnLocation.position, spawnLocation.rotation,
                    RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null);

                if (hivePrefab.TryGetComponent(out NetworkObject newHiveNetworkObject)
                    && hivePrefab.TryGetComponent(out GrabbableObject newHive))
                {
                    newHiveNetworkObject.Spawn(true);

                    serializedHive.overrideHive = true;
                    serializedHive.hiveReference = newHive;
                }
            }

            if (IsSpawned)
            {
                SyncedHives.Add(serializedHive);
            }
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
            SyncedHives = new();

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
                }
            }

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedHives.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<HiveInfo>.EventType.Add)
                {
                    SyncHiveValues(changeEvent.Value);
                }
            };
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
        /// <param name="syncedHive"></param>
        private void SyncHiveValues(HiveInfo syncedHive)
        {
            if (!syncedHive.beesReference.TryGet(out RedLocustBees bees))
            {
                return;
            }

            Vector3 spawnPosition = syncedHive.itemInfo.transformInfo.position;
            Quaternion spawnRotation = syncedHive.itemInfo.transformInfo.rotation;

            if (bees.agent != null && bees.agent.Warp(spawnPosition))
            {
                bees.transform.position = spawnPosition;
                bees.serverPosition = spawnPosition;

                bees.OnSyncPositionFromServer(spawnPosition);
            }

            if (overrideBeesScanNode)
            {
                ScanNodeProperties? beesScanNode = bees.GetComponentInChildren<ScanNodeProperties>();
                ScanNodeInfo beesScanNodeInfo = this.beesScanNode;

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

            if (syncedHive.overrideHive && syncedHive.hiveReference.TryGet(out GrabbableObject newHive))
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

                originalHive.isInFactory = true; // Stops deactivated hive from getting struck by lightning.
                // ...

                bees.hive = newHive;
            }

            bees.lastKnownHivePosition = spawnPosition;
            bees.syncedLastKnownHivePosition = true;

            bees.hive.fallTime = 1.0f;
            bees.hive.hasHitGround = true;
            bees.hive.reachedFloorTarget = true;

            bees.hive.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            bees.hive.startFallingPosition = spawnPosition;
            bees.hive.targetFloorPosition = spawnPosition;

            if (overrideHiveScanNode)
            {
                ScanNodeProperties? hiveScanNode = bees.hive.GetComponentInChildren<ScanNodeProperties>();
                ScanNodeInfo hiveScanNodeInfo = this.hiveScanNode;

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