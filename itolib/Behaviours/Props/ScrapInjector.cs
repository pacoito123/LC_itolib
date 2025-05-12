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
        public List<int>? ModifiedIndices { get; private set; }

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

            ModifiedIndices = new(currentDungeon.ExtendedMod.ExtendedItems.Count);

            foreach (ExtendedItem extendedItem in currentDungeon.ExtendedMod.ExtendedItems)
            {
                if (!extendedItem.Item.isScrap)
                {
                    continue;
                }

                int dungeonRarity = extendedItem.DungeonMatchingProperties.GetDynamicRarity(currentDungeon);

                if (dungeonRarity > 0)
                {
                    for (int i = 0; i < CurrentLevel.spawnableScrap.Count; i++)
                    {
                        SpawnableItemWithRarity item = CurrentLevel.spawnableScrap[i];

                        if (item.spawnableItem == extendedItem.Item && item.rarity < 1)
                        {
                            item.rarity = dungeonRarity;
                            ModifiedIndices.Add(i);

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
            if (CurrentLevel == null || ModifiedIndices == null || ModifiedIndices.Count < 1)
            {
                return;
            }

            ModifiedIndices.ForEach(index => CurrentLevel.spawnableScrap[index].rarity = 0);
        }
    }
}