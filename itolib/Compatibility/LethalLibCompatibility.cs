/* using itolib.Extensions;
using LethalLib.Modules;
using System.Runtime.CompilerServices;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility for <c>LethalLib</c>.
    /// </summary>
    internal sealed class LethalLibCompatibility
    {
        /// <summary>
        ///     Whether <c>LethalLib</c> is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("evaisa.lethallib");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static EnemyType? GetLethalLibEnemyType(string enemyName, bool checkObjectName = false)
        {
            for (int i = 0; i < Enemies.spawnableEnemies.Count; i++)
            {
                Enemies.SpawnableEnemy? enemy = Enemies.spawnableEnemies[i];

                if (enemy != null && enemy.enemy != null && (!checkObjectName ? enemy.enemy.enemyName.CompareOrdinal(enemyName)
                    : enemy.enemy.name.CompareOrdinal(enemyName)))
                {
                    return enemy.enemy;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static Item? GetLethalLibItem(string itemName, bool checkObjectName = false)
        {
            for (int i = 0; i < Items.LethalLibItemList.Count; i++)
            {
                Item? item = Items.LethalLibItemList[i];

                if (item != null && (!checkObjectName ? item.itemName.CompareOrdinal(itemName)
                    : item.name.CompareOrdinal(itemName)))
                {
                    return item;
                }
            }

            return null;
        }
    }
} */