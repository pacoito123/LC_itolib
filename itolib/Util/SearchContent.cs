using DunGen.Graph;
using itolib.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Util
{
    /// <summary>
    ///     Helper methods for retrieving references to some registered content types.
    /// </summary>
    public static class SearchContent
    {
        internal static QuickMenuManager QuickMenuManager
        {
            get
            {
                if (field == null)
                {
                    field = UnityEngine.Object.FindFirstObjectByType<QuickMenuManager>(FindObjectsInactive.Exclude);
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

            if (string.IsNullOrEmpty(itemName))
            {
                return false;
            }

            item = StartOfRound.Instance.allItemsList.itemsList.Find(item => checkObjectName
                ? (item.spawnPrefab != null && string.Equals(item.spawnPrefab.name, itemName, StringComparison.OrdinalIgnoreCase))
                : (item != null && string.Equals(item.itemName, itemName, StringComparison.OrdinalIgnoreCase)));

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

            if (string.IsNullOrEmpty(enemyName))
            {
                return false;
            }

            foreach (EnemyType enemyType in FoundEnemies)
            {
                if (enemyType == null)
                {
                    continue;
                }

                if ((checkObjectName && string.Equals(enemyType.name, enemyName, StringComparison.OrdinalIgnoreCase))
                    || (!checkObjectName && string.Equals(enemyType.enemyName, enemyName, StringComparison.OrdinalIgnoreCase)))
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

            if (string.IsNullOrEmpty(levelName))
            {
                return false;
            }

            for (int i = 0; i < StartOfRound.Instance.levels?.Length; i++)
            {
                SelectableLevel? selectableLevel = StartOfRound.Instance.levels[i];

                if (selectableLevel != null && !string.IsNullOrEmpty(selectableLevel.PlanetName)
                    && string.Equals(selectableLevel.PlanetName.SkipToLetters(), levelName.SkipToLetters(), StringComparison.OrdinalIgnoreCase))
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

            if (string.IsNullOrEmpty(dungeonName))
            {
                return false;
            }

            for (int i = 0; i < RoundManager.Instance.dungeonFlowTypes?.Length; i++)
            {
                IndoorMapType? indoorMapType = RoundManager.Instance.dungeonFlowTypes[i];

                if (indoorMapType != null && indoorMapType.dungeonFlow != null
                    && string.Equals(indoorMapType.dungeonFlow.name, dungeonName, StringComparison.OrdinalIgnoreCase))
                {
                    dungeon = indoorMapType.dungeonFlow;

                    break;
                }
            }

            return dungeon != null;
        }
    }
}