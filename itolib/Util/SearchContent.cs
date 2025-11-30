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

            if (DawnLibCompatibility.Enabled)
            {
                Item? dawnItem = DawnLibCompatibility.GetDawnItem(itemName);

                return true;
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

            if (DawnLibCompatibility.Enabled)
            {
                EnemyType? dawnEnemy = DawnLibCompatibility.GetDawnEnemyType(enemyName);

                return true;
            }

            return false;
        }
    }
}