using itolib.Interfaces;
using LethalLevelLoader;
using System;
using System.Collections.Generic;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    internal sealed class SimulateAnomaly : ISeededScript<SimulateAnomaly>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static Item? SingleItem
        {
            get
            {
                if (field == null)
                {
                    ISeededScript<SimulateAnomaly>.SeedOffset = 5;

                    SelectableLevel currentLevel = LevelManager.CurrentExtendedLevel.SelectableLevel;
                    Random anomalyRandom = ISeededScript<SimulateAnomaly>.SeededRandom;

                    SimulateChallengeFile(ref anomalyRandom, currentLevel);
                    int singleItemIndex = SimulateSpawnScrap(ref anomalyRandom, currentLevel);

                    field = (singleItemIndex != -1 && currentLevel.spawnableScrap?[singleItemIndex] != null)
                        ? currentLevel.spawnableScrap[singleItemIndex].spawnableItem : null;

                    LevelManager.GlobalLevelEvents.onLevelLoaded.AddListener(ResetItem);
                }

                return field;
            }
            private set;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="anomalyRandom"></param>
        /// <param name="currentLevel"></param>
        private static void SimulateChallengeFile(ref Random anomalyRandom, SelectableLevel currentLevel)
        {
            if (StartOfRound.Instance.isChallengeFile)
            {
                int[] challengeIndices = new int[5];

                for (int i = 0; i < challengeIndices.Length; i++)
                {
                    challengeIndices[i] = anomalyRandom.Next(0, 100);
                }

                if (challengeIndices[0] < 45)
                {
                    _ = anomalyRandom.Next(0, currentLevel.Enemies.Count);

                    if (currentLevel.Enemies[RoundManager.Instance.increasedInsideEnemySpawnRateIndex].enemyType.spawningDisabled)
                    {
                        _ = anomalyRandom.Next(0, currentLevel.Enemies.Count);
                    }
                }
                if (challengeIndices[1] < 45)
                {
                    _ = anomalyRandom.Next(0, currentLevel.OutsideEnemies.Count);
                }
                if (challengeIndices[2] < 45)
                {
                    _ = anomalyRandom.Next(0, currentLevel.spawnableMapObjects.Length);
                }
                if (challengeIndices[3] < 45)
                {
                    _ = anomalyRandom.Next(0, currentLevel.spawnableOutsideObjects.Length);
                }
                if (challengeIndices[4] < 45)
                {
                    _ = anomalyRandom.Next(0, currentLevel.spawnableScrap.Count);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="anomalyRandom"></param>
        /// <param name="currentLevel"></param>
        /// <returns></returns>
        private static int SimulateSpawnScrap(ref Random anomalyRandom, SelectableLevel currentLevel)
        {
            _ = (int)(anomalyRandom.Next(currentLevel.minScrap, currentLevel.maxScrap)
                * RoundManager.Instance.scrapAmountMultiplier);

            if (StartOfRound.Instance.isChallengeFile)
            {
                _ = anomalyRandom.Next(10, 30);
            }

            int singleItemIndex = -1;
            if (anomalyRandom.Next(0, 500) <= 20)
            {
                List<SpawnableItemWithRarity> spawnableScrap = currentLevel.spawnableScrap;

                singleItemIndex = anomalyRandom.Next(0, spawnableScrap.Count);
                bool twoHanded = false;

                for (int i = 0; i < 2; i++)
                {
                    if (spawnableScrap[singleItemIndex].rarity >= 5 && !spawnableScrap[singleItemIndex].spawnableItem.twoHanded)
                    {
                        twoHanded = true;
                        break;
                    }
                    singleItemIndex = anomalyRandom.Next(0, spawnableScrap.Count);
                }

                if (!twoHanded && anomalyRandom.Next(0, 100) < 60)
                {
                    singleItemIndex = -1;
                }
            }

            return singleItemIndex;
        }

        private static void ResetItem()
        {
            LevelManager.GlobalLevelEvents.onLevelLoaded.RemoveListener(ResetItem);

            SingleItem = null;
        }
    }
}