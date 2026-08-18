/* using DunGen;
using LethalLevelLoader;
using System.Collections.Generic;
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
        private List<SpawnableItemWithRarity>? modifiedRarities;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (!NetworkManager.Singleton.IsHost || !enabled)
            {
                return;
            }

            SelectableLevel currentLevel = LevelManager.CurrentExtendedLevel.SelectableLevel;
            ExtendedDungeonFlow currentDungeon = DungeonManager.CurrentExtendedDungeonFlow;

            modifiedRarities = new(currentDungeon.ExtendedMod.ExtendedItems.Count);

            foreach (ExtendedItem extendedItem in currentDungeon.ExtendedMod.ExtendedItems)
            {
                if (!extendedItem.Item.isScrap)
                {
                    continue;
                }

                int dungeonRarity = extendedItem.DungeonMatchingProperties.GetDynamicRarity(currentDungeon);

                if (dungeonRarity > 0)
                {
                    for (int i = 0; i < currentLevel.spawnableScrap.Count; i++)
                    {
                        SpawnableItemWithRarity item = currentLevel.spawnableScrap[i];

                        if (item.spawnableItem == extendedItem.Item && item.rarity < 1)
                        {
                            item.rarity = dungeonRarity;
                            modifiedRarities.Add(item);

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
            for (int i = 0; i < modifiedRarities?.Count; i++)
            {
                modifiedRarities[i].rarity = 0;
            }
        }
    }
} */