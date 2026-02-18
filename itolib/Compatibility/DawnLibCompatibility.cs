using Dawn;
using itolib.Extensions;
using System.Runtime.CompilerServices;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility for <c>DawnLib</c>.
    /// </summary>
    internal sealed class DawnLibCompatibility
    {
        /// <summary>
        ///     Whether <c>DawnLib</c> is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.github.teamxiaolan.dawnlib");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static EnemyType? GetDawnEnemyType(string enemyName, bool checkObjectName = false)
        {
            foreach (DawnEnemyInfo dawnEnemyInfo in LethalContent.Enemies.Values)
            {
                // Skip any vanilla or non-DawnLib enemies.
                if (dawnEnemyInfo.Key.IsVanilla() || dawnEnemyInfo.HasTag(DawnLibTags.IsExternal))
                {
                    continue;
                }

                if (dawnEnemyInfo.EnemyType != null && (!checkObjectName ? dawnEnemyInfo.EnemyType.enemyName.CompareOrdinal(enemyName)
                    : dawnEnemyInfo.EnemyType.name.CompareOrdinal(enemyName)))
                {
                    return dawnEnemyInfo.EnemyType;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static Item? GetDawnItem(string itemName, bool checkObjectName = false)
        {
            foreach (DawnItemInfo dawnItemInfo in LethalContent.Items.Values)
            {
                // Skip any vanilla or non-DawnLib items.
                if (dawnItemInfo.Key.IsVanilla() || dawnItemInfo.HasTag(DawnLibTags.IsExternal))
                {
                    continue;
                }

                if (dawnItemInfo.Item != null && (!checkObjectName ? dawnItemInfo.Item.itemName.CompareOrdinal(itemName)
                    : dawnItemInfo.Item.name.CompareOrdinal(itemName)))
                {
                    return dawnItemInfo.Item;
                }
            }

            return null;
        }
    }
}