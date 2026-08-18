using DunGen.Graph;
using itolib.Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace itolib.Util
{
    /// <summary>
    ///     Helper methods for retrieving references to some registered content types.
    /// </summary>
    public static class SearchContent
    {
        /// <summary>
        ///     <c>Regex</c> pattern for skipping to the first letter in a given string.
        /// </summary>
        /// <example>
        ///     ("823 Bozoros") -> ("Bozoros").
        /// </example>
        public static readonly Regex skipToLetterRegex = new(@"^[^\p{L}]+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static QuickMenuManager QuickMenuManager
        {
            get
            {
                if (field == null)
                {
                    field = Object.FindFirstObjectByType<QuickMenuManager>(FindObjectsInactive.Exclude);
                }

                return field;
            }
        }

        private static HashSet<EnemyType>? FoundEnemies
        {
            get
            {
                if (field == null || field.Count == 0)
                {
                    if (QuickMenuManager != null && QuickMenuManager.testAllEnemiesLevel != null)
                    {
                        static void AddEnemiesToList(List<SpawnableEnemyWithRarity> spawnableEnemies)
                        {
                            field ??= [];

                            if (spawnableEnemies == null || spawnableEnemies.Count == 0)
                            {
                                return;
                            }

                            foreach (SpawnableEnemyWithRarity spawnableEnemy in spawnableEnemies)
                            {
                                if (spawnableEnemy == null || spawnableEnemy.enemyType == null || spawnableEnemy.enemyType.enemyPrefab == null)
                                {
                                    continue;
                                }

                                if (!field.Add(spawnableEnemy.enemyType))
                                {
                                    continue;
                                }

                                if (spawnableEnemy.enemyType.enemyPrefab.TryGetComponent(out ButlerEnemyAI butlerEnemy))
                                {
                                    _ = field.Add(butlerEnemy.butlerBeesEnemyType);
                                }

                                if (spawnableEnemy.enemyType.enemyPrefab.TryGetComponent(out CadaverGrowthAI cadaverEnemy))
                                {
                                    _ = field.Add(cadaverEnemy.bloomEnemyType);
                                }
                            }
                        }

                        AddEnemiesToList(QuickMenuManager.testAllEnemiesLevel.Enemies);
                        AddEnemiesToList(QuickMenuManager.testAllEnemiesLevel.OutsideEnemies);
                        AddEnemiesToList(QuickMenuManager.testAllEnemiesLevel.DaytimeEnemies);

                        if (RoundManager.Instance != null)
                        {
                            AddEnemiesToList(RoundManager.Instance.WeedEnemies);
                        }
                    }
                }

                return field;
            }
        }

        /// <summary>
        ///     Try search and obtain a registered <c>Item</c> type.
        /// </summary>
        /// <param name="item"><c>Item</c> obtained from the search, as an out parameter.</param>
        /// <param name="itemName">Name of the <c>Item</c> to search.</param>
        /// <param name="checkObjectName">Whether the <c>ScriptableObject</c> name should be checked or not.</param>
        /// <returns>Whether a registered <c>Item</c> was successfully obtained or not.</returns>
        public static bool TryFindItem(out Item item, string itemName, bool checkObjectName = false)
        {
            item = null!;

            if (StartOfRound.Instance == null || StartOfRound.Instance.allItemsList == null || StartOfRound.Instance.allItemsList.itemsList == null)
            {
                return false;
            }

            if (itemName.IsNullOrEmpty())
            {
                return false;
            }

            item = StartOfRound.Instance.allItemsList.itemsList.Find(item => checkObjectName
                ? (item.spawnPrefab != null && item.spawnPrefab.name.CompareOrdinal(itemName))
                : (item != null && item.itemName.CompareOrdinal(itemName)));

            return item != null;
        }

        /// <summary>
        ///     Try search and obtain a registered <c>EnemyType</c> type.
        /// </summary>
        /// <param name="enemy"><c>EnemyType</c> obtained from the search, as an out parameter.</param>
        /// <param name="enemyName">Name of the <c>EnemyType</c> to search.</param>
        /// <param name="checkObjectName">Whether the <c>ScriptableObject</c> name should be checked or not.</param>
        /// <returns>Whether a registered <c>EnemyType</c> was successfully obtained or not.</returns>
        public static bool TryFindEnemy(out EnemyType enemy, string enemyName, bool checkObjectName = false)
        {
            enemy = null!;

            if (RoundManager.Instance == null || FoundEnemies == null)
            {
                return false;
            }

            if (enemyName.IsNullOrEmpty())
            {
                return false;
            }

            foreach (EnemyType enemyType in FoundEnemies)
            {
                if (enemyType == null)
                {
                    continue;
                }

                if ((checkObjectName && enemyType.name.CompareOrdinal(enemyName))
                    || (!checkObjectName && enemyType.enemyName.CompareOrdinal(enemyName)))
                {
                    enemy = enemyType;

                    break;
                }
            }

            return enemy != null;
        }

        /// <summary>
        ///     Try search and obtain a registered <c>SelectableLevel</c> type.
        /// </summary>
        /// <param name="level"><c>SelectableLevel</c> obtained from the search, as an out parameter.</param>
        /// <param name="levelName">Name of the <c>SelectableLevel</c> to search, numberless.</param>
        /// <returns>Whether a registered <c>SelectableLevel</c> was successfully obtained or not.</returns>
        public static bool TryFindLevel(out SelectableLevel level, string levelName)
        {
            level = null!;

            if (StartOfRound.Instance == null)
            {
                return false;
            }

            if (levelName.IsNullOrEmpty())
            {
                return false;
            }

            for (int i = 0; i < StartOfRound.Instance.levels?.Length; i++)
            {
                SelectableLevel? selectableLevel = StartOfRound.Instance.levels[i];

                if (selectableLevel != null && skipToLetterRegex.Replace(selectableLevel.PlanetName, string.Empty).CompareOrdinal(levelName))
                {
                    level = selectableLevel;

                    break;
                }
            }

            return level != null;
        }

        /// <summary>
        ///     Try search and obtain a registered <c>DungeonFlow</c> type.
        /// </summary>
        /// <param name="dungeon"><c>DungeonFlow</c> obtained from the search, as an out parameter.</param>
        /// <param name="dungeonName">Name of the <c>DungeonFlow</c> to search.</param>
        /// <returns>Whether a registered <c>DungeonFlow</c> was successfully obtained or not.</returns>
        public static bool TryFindDungeon(out DungeonFlow dungeon, string dungeonName)
        {
            dungeon = null!;

            if (RoundManager.Instance == null)
            {
                return false;
            }

            if (dungeonName.IsNullOrEmpty())
            {
                return false;
            }

            for (int i = 0; i < RoundManager.Instance.dungeonFlowTypes?.Length; i++)
            {
                IndoorMapType? indoorMapType = RoundManager.Instance.dungeonFlowTypes[i];

                if (indoorMapType != null && indoorMapType.dungeonFlow != null && indoorMapType.dungeonFlow.name.CompareOrdinal(dungeonName))
                {
                    dungeon = indoorMapType.dungeonFlow;

                    break;
                }
            }

            return dungeon != null;
        }
    }
}