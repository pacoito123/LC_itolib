using itolib.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace itolib.Util
{
    /// <summary>
    ///     Simulates the current round's seeded <c>Random</c> instance to determine if any <c>Anomaly</c> (e.g. single-item day) is active.
    /// </summary>
    public sealed class SimulateAnomaly : ISeededScript<SimulateAnomaly>
    {
        /// <summary>
        ///     Obtain item used for the current round's single-item day, if there is one.
        /// </summary>
        public static Item? SingleItem
        {
            get
            {
                if (_isSingleItemDay == null && StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null && RoundManager.Instance != null)
                {
                    ISeededScript<SimulateAnomaly>.SeedOffset = 5;

                    SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
                    Random anomalyRandom = ISeededScript<SimulateAnomaly>.SeededRandom;

                    if (StartOfRound.Instance.isChallengeFile)
                    {
                        SimulateChallengeFile(ref anomalyRandom, currentLevel);
                    }
                    int singleItemIndex = SimulateSpawnScrap(ref anomalyRandom, currentLevel);

                    field = (singleItemIndex != -1 && currentLevel.spawnableScrap?[singleItemIndex] != null)
                        ? currentLevel.spawnableScrap[singleItemIndex].spawnableItem : null;

                    _isSingleItemDay = field != null;

                    SceneManager.sceneLoaded += ResetItem;
                }

                return field;
            }
            private set;
        }
        private static bool? _isSingleItemDay;

        private static void SimulateChallengeFile(ref Random anomalyRandom, SelectableLevel currentLevel)
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
                _ = anomalyRandom.Next(0, currentLevel.indoorMapHazards.Length);
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

        private static int SimulateSpawnScrap(ref Random anomalyRandom, SelectableLevel currentLevel)
        {
            _ = (int)(anomalyRandom.Next(currentLevel.minScrap, currentLevel.maxScrap) * RoundManager.Instance.scrapAmountMultiplier);

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

        private static void ResetItem(Scene scene, LoadSceneMode _)
        {
            SceneManager.sceneLoaded -= ResetItem;

            SingleItem = null;
            _isSingleItemDay = null;
        }

        /// <inheritdoc/>
        public ISeededScript<SimulateAnomaly> SeededSelf => throw new InvalidOperationException("SimulateAnomaly should have no instance.");
    }
}