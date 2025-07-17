using DunGen;
using itolib.Extensions;
using LethalLevelLoader;
using System;
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
        [Header("Hazard Replacement")]
        [Tooltip("")]
        public string originalHazard = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string replacingHazard = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        public HazardReplacement() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class HazardReplacer : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [SerializeField] private HazardReplacement[]? hazardReplacements;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            if (hazardReplacements == null)
            {
                // TODO: Log warning.
                return;
            }

            SelectableLevel? currentLevel = LevelManager.CurrentExtendedLevel != null ? LevelManager.CurrentExtendedLevel.SelectableLevel : null;
            ExtendedDungeonFlow? currentDungeon = DungeonManager.CurrentExtendedDungeonFlow;

            if (currentLevel == null || currentDungeon == null)
            {
                // TODO: Log warning.
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

            for (int i = 0; i < hazardReplacements.Length; i++)
            {
                SpawnableMapObject? originalHazard = null;
                for (int j = 0; j < hazardNames.Length; j++)
                {
                    if (hazardReplacements[i].originalHazard.CompareOrdinal(hazardNames[j]))
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
                    if (hazardReplacements[i].replacingHazard.CompareOrdinal(extendedHazardNames[j]))
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