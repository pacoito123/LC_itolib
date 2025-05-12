using System.Collections.Generic;
using DunGen;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ScrapInjector : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public SelectableLevel? CurrentLevel { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<SpawnableItemWithRarity>? ModifiedRarities { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDungeonComplete(Dungeon _)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            CurrentLevel = LevelManager.CurrentExtendedLevel?.SelectableLevel;
            ExtendedDungeonFlow currentDungeon = DungeonManager.CurrentExtendedDungeonFlow;

            if (CurrentLevel == null || currentDungeon == null)
            {
                return;
            }

            int scrapCount = CurrentLevel.spawnableScrap.Count;
            ModifiedRarities = new(currentDungeon.ExtendedMod.ExtendedItems.Count);

            foreach (ExtendedItem extendedItem in currentDungeon.ExtendedMod.ExtendedItems)
            {
                if (!extendedItem.Item.isScrap)
                {
                    continue;
                }

                int dungeonRarity = extendedItem.DungeonMatchingProperties.GetDynamicRarity(currentDungeon);

                if (dungeonRarity > 0)
                {
                    for (int i = 0; i < scrapCount; i++)
                    {
                        SpawnableItemWithRarity item = CurrentLevel.spawnableScrap[i];

                        if (item.spawnableItem == extendedItem.Item && item.rarity < 1)
                        {
                            item.rarity = dungeonRarity;
                            ModifiedRarities.Add(item);

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDestroy()
        {
            if (CurrentLevel == null || ModifiedRarities == null)
            {
                return;
            }

            int count = ModifiedRarities.Count;
            for (int i = 0; i < count; i++)
            {
                ModifiedRarities[i].rarity = 0;
            }
        }
    }
}