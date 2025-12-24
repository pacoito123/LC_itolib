using itolib.Interfaces;
using itolib.Structs;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class GiftSpawner : ScrapSpawner, IWeightedScript<ScrapWeightEntry>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static Item? VanillaGiftbox
        {
            get
            {
                if (field == null)
                {
                    field = OriginalContent.Items.Find(item => item.itemId == 152767);
                }

                return field;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Gift Spawner")]
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int giftContentsMinValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(-1)]
        [SerializeField] private int giftContentsMaxValue = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Gift Overrides")]
        [Tooltip("")]
        [SerializeField] private ParticleSystem[]? poofParticleOverrides;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip[]? openGiftAudioOverrides;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Material[]? giftMaterialOverrides;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Scan Node")]
        [Tooltip("")]
        [SerializeField] private bool overrideGiftScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ScanNodeInfo giftScanNode;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int scanNodeLayer;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            scanNodeLayer = LayerMask.NameToLayer("ScanNode");

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spawnLocation"></param>
        protected override void SpawnPerformed(GrabbableObject? item, TransformInfo spawnLocation)
        {
            if (item == null || !item.IsSpawned || item.itemProperties == null || item is not GiftBoxItem gift)
            {
                return;
            }

            NetworkObject? giftContentsNetworkObject = base.GetPrefabToSpawn();

            if (giftContentsNetworkObject == null || !giftContentsNetworkObject.TryGetComponent(out GrabbableObject giftContents)
                || giftContents.itemProperties == null)
            {
                return;
            }

            gift.objectInPresentItem = giftContents.itemProperties;
            gift.objectInPresent = gift.objectInPresentItem.spawnPrefab;

            int minValue = giftContentsMinValue < 0 ? gift.objectInPresentItem.minValue : giftContentsMinValue,
                maxValue = giftContentsMaxValue < 0 ? gift.objectInPresentItem.maxValue : giftContentsMaxValue;

            int scrapValue = isSeededRandom ? SeededSelf.GetSeededRandom().Next(minValue, maxValue)
                : Random.RandomRangeInt(minValue, maxValue);

            if (RoundManager.Instance != null)
            {
                if (applyScrapMultiplier) // TODO: Separate scrap multiplier for contents?
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }
            }

            gift.objectInPresentValue = scrapValue;

            if (poofParticleOverrides?.Length > 0)
            {
                int particleIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, poofParticleOverrides.Length)
                    : Random.RandomRangeInt(0, poofParticleOverrides.Length);

                gift.PoofParticle = Instantiate(poofParticleOverrides[particleIndex], Vector3.zero, Quaternion.identity,
                    gift.transform);
            }

            if (openGiftAudioOverrides?.Length > 0)
            {
                int clipIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, openGiftAudioOverrides.Length)
                    : Random.RandomRangeInt(0, openGiftAudioOverrides.Length);

                gift.openGiftAudio = openGiftAudioOverrides[clipIndex];
            }

            if (giftMaterialOverrides?.Length > 0)
            {
                int materialIndex = isSeededRandom ? SeededSelf.GetSeededRandom().Next(0, giftMaterialOverrides.Length)
                    : Random.RandomRangeInt(0, giftMaterialOverrides.Length);

                MeshRenderer[] renderers = gift.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].gameObject.layer != scanNodeLayer)
                    {
                        renderers[i].sharedMaterial = giftMaterialOverrides[materialIndex];
                    }
                }
            }

            if (overrideGiftScanNode)
            {
                ScanNodeProperties? giftScanNode = gift.GetComponentInChildren<ScanNodeProperties>();
                ScanNodeInfo giftScanNodeInfo = this.giftScanNode;

                if (giftScanNode != null)
                {
                    giftScanNode.headerText = giftScanNodeInfo.headerText;
                    giftScanNode.subText = giftScanNodeInfo.subText;
                    giftScanNode.minRange = giftScanNodeInfo.minRange;
                    giftScanNode.maxRange = giftScanNodeInfo.maxRange;
                    giftScanNode.creatureScanID = giftScanNodeInfo.creatureScanID;
                    giftScanNode.nodeType = giftScanNodeInfo.nodeType;
                    giftScanNode.requiresLineOfSight = giftScanNodeInfo.requiresLineOfSight;
                }
            }

            gift.loadedItemFromSave = true;

            base.SpawnPerformed(item, spawnLocation);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override NetworkObject? GetPrefabToSpawn()
        {
            return (VanillaGiftbox != null && VanillaGiftbox.spawnPrefab != null)
                ? VanillaGiftbox.spawnPrefab.GetComponent<NetworkObject>() : null;
        }
    }
}