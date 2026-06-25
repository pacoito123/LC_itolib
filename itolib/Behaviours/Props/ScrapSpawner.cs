using itolib.Behaviours.Networking;
using itolib.Interfaces;
using itolib.Structs;
using itolib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     Represents a single entry with weights to be used for weighted scrap item selection.
    /// </summary>
    /// <param name="itemWithRarity">Scrap item to copy name and weights from.</param>
    [Serializable]
    public struct ScrapWeightEntry(SpawnableItemWithRarity itemWithRarity) : IWeightedEntry
    {
        /// <summary>
        ///     Scrap item name corresponding to this specific entry.
        /// </summary>
        [Header("Scrap Weight Entry")]
        [Tooltip("Scrap item name corresponding to this specific entry.")]
        public string itemName = (itemWithRarity.spawnableItem != null) ? itemWithRarity.spawnableItem.itemName : string.Empty;

        /// <inheritdoc/>
        [field: Tooltip("Weight value for this specific entry.")]
        [field: Min(0)]
        [field: SerializeField] public int Weight { get; set; } = itemWithRarity.rarity;

        /// <inheritdoc/>
        [field: Tooltip("Weight modifiers to apply whenever this specific entry is used.")]
        [field: SerializeField] public WeightedModifier[]? WeightedModifiers { get; set; }

        /// <inheritdoc/>
        [field: Tooltip("Whether this specific entry can be used more than once or not.")]
        [field: SerializeField] public bool SingleUse { get; set; }

        /// <summary>
        ///     Scrap item corresponding to this specific entry.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Replace with the desired scrap item's 'itemName' field.")]
        [Obsolete("Replace with the desired scrap item's 'itemName' field.")]
        public Item? itemToSpawn = itemWithRarity.spawnableItem;
    }

    /// <summary>
    ///     Represents a scrap item spawner with weighted selection capabilities.
    /// </summary>
    public class ScrapSpawner : NetworkedSpawner<GrabbableObject>, IWeightedScript<ScrapWeightEntry>
    {
        /// <summary>
        ///     List of spawned items to be synced with clients.
        /// </summary>
        public NetworkList<ItemInfo> SyncedItems { get; private set; }

        /// <inheritdoc/>
        public IWeightedScript<ScrapWeightEntry> WeightedSelf { get; }

        /// <inheritdoc/>
        public int[]? CurrentWeights { get; set; }

        /// <inheritdoc/>
        public int TotalWeight { get; set; }

        /// <inheritdoc/>
        public bool InitializedWeights { get; set; }

        /// <inheritdoc/>
        [field: Space(5.0f)]
        [field: Header("Scrap Spawner")]
        [field: Tooltip("List of weighted entries of type ScrapWeightEntry.")]
        [field: SerializeField] public ScrapWeightEntry[]? WeightedEntries { get; set; }

        /// <summary>
        ///     Whether to add the current moon's loot pool to the scrap item spawns or not.
        /// </summary>
        [Tooltip("Whether to add the current moon's loot pool to the scrap item spawns or not.")]
        [FormerlySerializedAs("useMoonScrapSpawns")]
        [SerializeField] private bool addMoonScrapSpawns;

        /// <summary>
        ///     Minimum value for spawned scrap items.
        /// </summary>
        /// <remarks>Can be left at <c>-1</c> to disable value overriding.</remarks>
        [Space(5.0f)]
        [Header("Item Properties")]
        [Tooltip("Minimum value for spawned scrap items. Can be left at '-1' to disable value overriding.")]
        [Min(-1)]
        [SerializeField] private int overrideMinValue = -1;

        /// <summary>
        ///     Maximum value for spawned scrap items.
        /// </summary>
        /// <remarks>Can be left at <c>-1</c> to disable value overriding.</remarks>
        [Tooltip("Maximum value for spawned scrap items. Can be left at '-1' to disable value overriding.")]
        [Min(-1)]
        [SerializeField] private int overrideMaxValue = -1;

        /// <summary>
        ///     Whether to apply the current scrap value multiplier to spawned scrap items or not.
        /// </summary>
        [Tooltip("Whether to apply the current scrap value multiplier when overriding spawned scrap item values or not.")]
        [SerializeField] protected bool applyScrapMultiplier = true;

        /// <summary>
        ///     Whether to allow spawned scrap items to randomly select mesh variants or not.
        /// </summary>
        [Tooltip("Whether to allow spawned scrap to randomly select mesh variants or not.")]
        [SerializeField] private bool allowMeshVariants = true;

        /// <summary>
        ///     Whether to allow spawned scrap items to randomly select material variants or not.
        /// </summary>
        [Tooltip("Whether to allow spawned scrap to randomly select material variants or not.")]
        [SerializeField] private bool allowMaterialVariants = true;

        /// <summary>
        ///     Whether spawned scrap should fall to the ground or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Position and Rotation")]
        [Tooltip("Whether spawned scrap should fall to the ground or not.")]
        [SerializeField] private bool fallToGround = true;

        /// <summary>
        ///     Whether spawned scrap items should have their position slightly offset when falling to the ground upon spawning or not.
        /// </summary>
        [Tooltip("Whether spawned scrap items should have their position slightly offset when falling to the ground upon spawning or not.")]
        [SerializeField] private bool randomizePosition;

        /// <summary>
        ///     Whether spawned scrap items should have their defined resting rotation applied or not.
        /// </summary>
        [Tooltip("Whether spawned scrap items should have their defined resting rotation applied or not.")]
        [SerializeField] private bool applyRestingRotation;

        /// <summary>
        ///     Whether spawned scrap items should have every <c>AudioSource</c> in it stopped or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Other")]
        [Tooltip("Whether spawned scrap items should have every audio source in it stopped or not.")]
        [SerializeField] private bool muteSpawnedScrap;

        /// <summary>
        ///     Whether single item days should be respected when spawning scrap items or not.
        /// </summary>
                [Tooltip("Whether single item days should be respected when spawning scrap items or not.")]
        [SerializeField] private bool respectSingleItemDay;

        /// <summary>
        ///     Whether to use the current moon's loot pool as the scrap item spawns or not.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(10.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Use 'addMoonScrapSpawns' field instead.")]
        [Obsolete("Use 'addMoonScrapSpawns' field instead.")]
        [SerializeField] private bool useMoonScrapSpawns;

        /// <inheritdoc/>
        public override NetworkObject? GetPrefabToSpawn()
        {
            // Use single item day scrap item spawns instead of weighted selection, if set to respect it.
            if (respectSingleItemDay && TryGetNetworkObject(out NetworkObject itemNetworkObject, SimulateAnomaly.SingleItem))
            {
                return itemNetworkObject;
            }

            if (WeightedEntries?.Length > 0 && WeightedSelf.TryObtainRandomEntry(out ScrapWeightEntry entry, out int _, isSeededRandom
                ? SeededSelf.GetSeededRandom() : null))
            {
                if (SearchContent.TryFindItem(out Item item, entry.itemName) && TryGetNetworkObject(out itemNetworkObject, item))
                {
                    return itemNetworkObject;
                }
#pragma warning disable CS0618 // Type or member is obsolete.
                else if (entry.itemToSpawn != null && (TryGetNetworkObject(out itemNetworkObject, entry.itemToSpawn) || (SearchContent.TryFindItem(out item,
                    entry.itemToSpawn.name, checkObjectName: true) && TryGetNetworkObject(out itemNetworkObject, item))))
#pragma warning restore CS0618 // Type or member is obsolete.
                {
                    return itemNetworkObject;
                }
            }

            return null;
        }

        /// <summary>
        ///     Attempt to obtain the <c>NetworkObject</c> to spawn from a given <c>Item</c>.
        /// </summary>
        /// <param name="itemNetworkObject"><c>NetworkObject</c> of the scrap item to spawn, as an out parameter.</param>
        /// <param name="item">Item to obtain the <c>NetworkObject</c> off of.</param>
        /// <returns>Whether a <c>NetworkObject</c> was successfully obtained or not.</returns>
        private static bool TryGetNetworkObject(out NetworkObject itemNetworkObject, Item? item)
        {
            itemNetworkObject = null!;

            return item != null && item.spawnPrefab != null && item.spawnPrefab.TryGetComponent(out itemNetworkObject);
        }

        /// <inheritdoc/>
        protected override Transform? GetParentOverride()
        {
            return RoundManager.Instance != null ? RoundManager.Instance.spawnedScrapContainer : null;
        }

        /// <inheritdoc/>
        protected override void SpawnPerformed(GrabbableObject? item, TransformInfo spawnLocation)
        {
            if (item == null || !item.IsSpawned || item.itemProperties == null)
            {
                return;
            }

            int minValue = overrideMinValue < 0 ? item.itemProperties.minValue : overrideMinValue,
                maxValue = overrideMaxValue < 0 ? item.itemProperties.maxValue : overrideMaxValue;

            /* if (SimulateAnomaly.SingleItem != null)
            {
                // TODO: Apply single item day values.
            } */

            int scrapValue = (minValue >= maxValue) ? minValue
                : (isSeededRandom ? SeededSelf.GetSeededRandom().Next(minValue, maxValue)
                : UnityEngine.Random.RandomRangeInt(minValue, maxValue));

            if (RoundManager.Instance != null)
            {
                if (applyScrapMultiplier)
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }
            }

            if (applyRestingRotation)
            {
                spawnLocation.rotation *= Quaternion.Euler(item.itemProperties.restingRotation);
            }

            ItemInfo serializedItem = new()
            {
                transformInfo = spawnLocation,
                itemReference = item,
                scrapValue = scrapValue,
                meshVariant = (allowMeshVariants && item.itemProperties.meshVariants.Length > 0)
                    ? (isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, item.itemProperties.meshVariants.Length)
                        : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.meshVariants.Length)) : -1,
                materialVariant = (allowMaterialVariants && item.itemProperties.materialVariants.Length > 0)
                    ? (isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, item.itemProperties.materialVariants.Length)
                        : UnityEngine.Random.RandomRangeInt(0, item.itemProperties.materialVariants.Length)) : -1
            };

            if (IsSpawned)
            {
                SyncedItems.Add(serializedItem);
            }
            else
            {
                base.SpawnPerformed(item, spawnLocation);
            }
        }

        /// <inheritdoc/>
        public override void ClearDestroyed()
        {
            for (int i = PrefabInstances.Count - 1; i >= 0; i--)
            {
                GrabbableObject? spawnedItem = PrefabInstances[i];

                if (spawnedItem == null)
                {
                    PrefabInstances.RemoveAt(i);
                    SyncedItems.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///     Cache already-cast <c>IWeightedScript</c> instance.
        /// </summary>
        protected ScrapSpawner() : base()
        {
            WeightedSelf = this;

            SyncedItems = null!;
        }

        /// <summary>
        ///     Initialize weights for every defined <c>ScrapWeightEntry</c> and add the current moon's loot pool, if set to do so.
        /// </summary>
        protected override void Awake()
        {
            SyncedItems = new();

            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

#pragma warning disable CS0618 // Type or member is obsolete.
            if (useMoonScrapSpawns || addMoonScrapSpawns)
#pragma warning restore CS0618 // Type or member is obsolete.
            {
                if (StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null)
                {
                    List<SpawnableItemWithRarity> spawnableScrap = StartOfRound.Instance.currentLevel.spawnableScrap;

                    if (spawnableScrap?.Count > 0)
                    {
                        ScrapWeightEntry[] scrapEntries = [.. spawnableScrap.ConvertAll(static itemWithRarity => new ScrapWeightEntry(itemWithRarity))];
                        WeightedSelf.AddWeights(scrapEntries);
                    }
                }
            }

            base.Awake();
        }

        /// <summary>
        ///     Set relevant item values to be synced when spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SyncedItems.OnListChanged += changeEvent =>
            {
                if (changeEvent.Type is NetworkListEvent<ItemInfo>.EventType.Add
                    && changeEvent.Value.itemReference.TryGet(out GrabbableObject item))
                {
                    SyncItemValues(item, changeEvent.Value);
                }
            };
        }

        /// <summary>
        ///     Add a single weighted entry of type <c>ScrapWeightEntry</c>.
        /// </summary>
        /// <param name="entry">Entry of type <c>ScrapWeightEntry</c> to add.</param>
        public void AddWeightEntry(ScrapWeightEntry entry)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeight(entry);
        }

        /// <summary>
        ///     Add multiple weighted entries of type <c>ScrapWeightEntry></c>.
        /// </summary>
        /// <param name="entries">Entries of type <c>ScrapWeightEntry</c> to add.</param>
        public void AddWeightEntries(ScrapWeightEntry[] entries)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.AddWeights(entries);
        }

        /// <summary>
        ///     Remove weights for the weighted entry of type <c>ScrapWeightEntry</c> at the specified index.
        /// </summary>
        /// <remarks>Sets weights to <c>0</c> instead of actually removing them.</remarks>
        /// <param name="index">Index of the entry of type <c>ScrapWeightEntry</c> to remove.</param>
        public void RemoveWeightEntry(int index)
        {
            if (!WeightedSelf.InitializedWeights)
            {
                WeightedSelf.InitializeWeights();
            }

            WeightedSelf.RemoveWeight(index);
        }

        /// <summary>
        ///     Apply synced item values upon being spawned.
        /// </summary>
        /// <param name="item">Item to be synced.</param>
        /// <param name="itemInfo">Relevant item values to sync.</param>
        private void SyncItemValues(GrabbableObject item, ItemInfo itemInfo)
        {
            _ = StartCoroutine(WaitForItemFall(item, itemInfo.transformInfo));

            if (item.itemProperties != null)
            {
                if (itemInfo.meshVariant != -1 && item.TryGetComponent(out MeshFilter itemFilter))
                {
                    itemFilter.sharedMesh = item.itemProperties.meshVariants[itemInfo.meshVariant];
                }

                if (itemInfo.materialVariant != -1 && item.TryGetComponent(out MeshRenderer itemRenderer))
                {
                    itemRenderer.sharedMaterial = item.itemProperties.materialVariants[itemInfo.materialVariant];
                }
            }

            item.SetScrapValue(itemInfo.scrapValue);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.totalScrapValueInLevel += itemInfo.scrapValue;
            }
        }

        /// <summary>
        ///     Coroutine to wait an extra frame for the item to begin falling before overriding position and rotation.
        /// </summary>
        /// <param name="item">Item that has just spawned.</param>
        /// <param name="spawnLocation">Position and rotation to apply to the item.</param>
                private IEnumerator WaitForItemFall(GrabbableObject item, TransformInfo spawnLocation)
        {
            yield return Yielders.WaitForEndOfFrame;
            yield return Yielders.WaitForEndOfFrame;

            item.fallTime = 1.0f;
            item.hasHitGround = true;
            item.reachedFloorTarget = true;

            item.transform.SetPositionAndRotation(spawnLocation.position, spawnLocation.rotation);

            item.startFallingPosition = spawnLocation.position;
            item.targetFloorPosition = spawnLocation.position;

            if (fallToGround)
            {
                item.FallToGround(randomizePosition, true);
            }

            yield return Yielders.WaitForEndOfFrame;

            if (muteSpawnedScrap)
            {
                AudioSource[] sources = item.GetComponentsInChildren<AudioSource>();

                for (int i = 0; i < sources.Length; i++)
                {
                    sources[i].Stop();
                }
            }

            OnSpawnPerformed.Invoke(item);
        }
    }
}