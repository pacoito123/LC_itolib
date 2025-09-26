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
        [Tooltip("")]
        [SerializeField] private ParticleSystem? poofParticleOverride;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip? openGiftAudioOverride;

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

            int scrapValue = isSeededRandom ? seededSelf.GetSeededRandom().Next(minValue, maxValue)
                : Random.RandomRangeInt(minValue, maxValue);

            if (RoundManager.Instance != null)
            {
                if (applyScrapMultiplier) // TODO: Separate scrap multiplier for contents?
                {
                    scrapValue = (int)(scrapValue * RoundManager.Instance.scrapValueMultiplier);
                }
            }

            gift.objectInPresentValue = scrapValue;

            if (poofParticleOverride != null)
            {
                gift.PoofParticle = Instantiate(poofParticleOverride, Vector3.zero, Quaternion.identity, gift.transform);
            }

            if (openGiftAudioOverride != null)
            {
                gift.openGiftAudio = openGiftAudioOverride;
            }

            gift.loadedItemFromSave = true;
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