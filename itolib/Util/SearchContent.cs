using DunGen.Graph;
using itolib.Compatibility;
using itolib.Extensions;
using LethalLevelLoader;

namespace itolib.Util
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class SearchContent
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemName"></param>
        /// <param name="checkObjectName"></param>
        /// <returns></returns>
        public static bool TryFindItem(out Item item, string itemName, bool checkObjectName = false)
        {
            item = null!;

            if (itemName.IsNullOrEmpty())
            {
                return false;
            }

            ExtendedItem? extendedItem = PatchedContent.ExtendedItems.Find(extendedItem => !checkObjectName
                ? extendedItem.Item.itemName.CompareOrdinal(itemName) : extendedItem.Item.name.CompareOrdinal(itemName));

            if (extendedItem != null)
            {
                item = extendedItem.Item;

                return true;
            }

            if (LethalLibCompatibility.Enabled)
            {
                Item? lethalLibItem = LethalLibCompatibility.GetLethalLibItem(itemName, checkObjectName);

                if (lethalLibItem != null)
                {
                    item = lethalLibItem;

                    return true;
                }
            }

            if (DawnLibCompatibility.Enabled)
            {
                Item? dawnItem = DawnLibCompatibility.GetDawnItem(itemName, checkObjectName);

                if (dawnItem != null)
                {
                    item = dawnItem;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="enemyName"></param>
        /// <param name="checkObjectName"></param>
        /// <returns></returns>
        public static bool TryFindEnemy(out EnemyType enemy, string enemyName, bool checkObjectName = false)
        {
            enemy = null!;

            if (enemyName.IsNullOrEmpty())
            {
                return false;
            }

            ExtendedEnemyType? extendedEnemy = PatchedContent.ExtendedEnemyTypes.Find(extendedEnemy => !checkObjectName
                ? extendedEnemy.EnemyType.enemyName.CompareOrdinal(enemyName) : extendedEnemy.EnemyType.name.CompareOrdinal(enemyName));

            if (extendedEnemy != null)
            {
                enemy = extendedEnemy.EnemyType;

                return true;
            }

            if (LethalLibCompatibility.Enabled)
            {
                EnemyType? lethalLibEnemy = LethalLibCompatibility.GetLethalLibEnemyType(enemyName, checkObjectName);

                if (lethalLibEnemy != null)
                {
                    enemy = lethalLibEnemy;

                    return true;
                }
            }

            if (DawnLibCompatibility.Enabled)
            {
                EnemyType? dawnEnemy = DawnLibCompatibility.GetDawnEnemyType(enemyName, checkObjectName);

                if (dawnEnemy != null)
                {
                    enemy = dawnEnemy;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="levelName"></param>
        /// <returns></returns>
        public static bool TryFindLevel(out SelectableLevel level, string levelName)
        {
            level = null!;

            if (levelName.IsNullOrEmpty())
            {
                return false;
            }

            ExtendedLevel? extendedLevel = PatchedContent.ExtendedLevels.Find(extendedLevel =>
                extendedLevel.SelectableLevel.GetNumberlessPlanetName().CompareOrdinal(levelName));

            if (extendedLevel != null)
            {
                level = extendedLevel.SelectableLevel;

                return true;
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        /// <param name="dungeonName"></param>
        /// <returns></returns>
        public static bool TryFindDungeon(out DungeonFlow dungeon, string dungeonName)
        {
            dungeon = null!;

            if (dungeonName.IsNullOrEmpty())
            {
                return false;
            }

            ExtendedDungeonFlow? extendedDungeon = PatchedContent.ExtendedDungeonFlows.Find(extendedDungeon =>
                extendedDungeon.DungeonFlow.name.CompareOrdinal(dungeonName));

            if (extendedDungeon != null)
            {
                dungeon = extendedDungeon.DungeonFlow;

                return true;
            }

            return false;
        }
    }
}