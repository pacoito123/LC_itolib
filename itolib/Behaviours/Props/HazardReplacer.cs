using System;
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
    [Serializable]
    public struct HazardReplacement
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string originalHazard;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string replacingHazard;
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class HazardReplacer : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public List<HazardReplacement> hazardReplacements = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDungeonComplete(Dungeon _)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            SelectableLevel? currentLevel = LevelManager.CurrentExtendedLevel?.SelectableLevel;
            ExtendedDungeonFlow? currentDungeon = DungeonManager.CurrentExtendedDungeonFlow;

            if (currentLevel == null || currentDungeon == null)
            {
                return;
            }

            string[] hazardNames = new string[currentLevel.spawnableMapObjects.Length];
            for (int i = 0; i < hazardNames.Length; i++)
            {
                hazardNames[i] = currentLevel.spawnableMapObjects[i].prefabToSpawn.name;
            }

            string[] extendedHazardNames = new string[currentDungeon.SpawnableMapObjects.Count];
            for (int i = 0; i < extendedHazardNames.Length; i++)
            {
                extendedHazardNames[i] = currentDungeon.SpawnableMapObjects[i].prefabToSpawn.name;
            }

            int count = hazardReplacements.Count;
            for (int i = 0; i < count; i++)
            {
                SpawnableMapObject? originalHazard = null;
                for (int j = 0; j < hazardNames.Length; j++)
                {
                    if (string.CompareOrdinal(hazardReplacements[i].originalHazard, hazardNames[j]) == 0)
                    {
                        originalHazard = currentLevel.spawnableMapObjects[j];

                        break;
                    }
                }

                if (originalHazard == null)
                {
                    Plugin.StaticLogger.LogWarning($"Could not find hazard '{hazardReplacements[i].originalHazard}' in the moon's "
                        + "SpawnableMapObject list; its spawn rates will not be modified.");

                    continue;
                }

                SpawnableMapObject? replacingHazard = null;
                for (int j = 0; j < extendedHazardNames.Length; j++)
                {
                    if (string.CompareOrdinal(hazardReplacements[i].replacingHazard, extendedHazardNames[j]) == 0)
                    {
                        replacingHazard = currentDungeon.SpawnableMapObjects[j];

                        break;
                    }
                }

                if (replacingHazard == null)
                {
                    Plugin.StaticLogger.LogWarning($"Could not find hazard '{hazardReplacements[i].replacingHazard}' in the dungeon's "
                        + "SpawnableMapObject list; its spawn rates will not be modified.");

                    continue;
                }

                replacingHazard.numberToSpawn = originalHazard.numberToSpawn;
            }
        }
    }
}