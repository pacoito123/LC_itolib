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
    public struct HazardReplacement()
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

            SelectableLevel currentLevel = LevelManager.CurrentExtendedLevel.SelectableLevel;
            ExtendedDungeonFlow currentDungeon = DungeonManager.CurrentExtendedDungeonFlow;

            string[] hazardNames = new string[currentLevel.indoorMapHazards.Length];

            for (int i = 0; i < hazardNames.Length; i++)
            {
                if (currentLevel.indoorMapHazards[i] != null && currentLevel.indoorMapHazards[i].hazardType != null
                    && currentLevel.indoorMapHazards[i].hazardType.prefabToSpawn != null)
                {
                    hazardNames[i] = currentLevel.indoorMapHazards[i].hazardType.prefabToSpawn.name;
                }
            }

            string[] extendedHazardNames = new string[currentDungeon.IndoorMapHazards.Count];

            for (int i = 0; i < extendedHazardNames.Length; i++)
            {
                if (currentDungeon.IndoorMapHazards[i] != null && currentDungeon.IndoorMapHazards[i].hazardType != null
                    && currentDungeon.IndoorMapHazards[i].hazardType.prefabToSpawn != null)
                {
                    extendedHazardNames[i] = currentDungeon.IndoorMapHazards[i].hazardType.prefabToSpawn.name;
                }
            }

            for (int i = 0; i < hazardReplacements.Length; i++)
            {
                IndoorMapHazard? originalHazard = null;

                for (int j = 0; j < hazardNames.Length; j++)
                {
                    if (hazardReplacements[i].originalHazard.CompareOrdinal(hazardNames[j]))
                    {
                        originalHazard = currentLevel.indoorMapHazards[j];

                        break;
                    }
                }

                if (originalHazard == null)
                {
                    Plugin.StaticLogger.LogWarning($"Could not find hazard '{hazardReplacements[i].originalHazard}' in the moon's "
                        + "SpawnableMapObject list; its spawn rates will not be modified.");

                    continue;
                }

                IndoorMapHazard? replacingHazard = null;

                for (int j = 0; j < extendedHazardNames.Length; j++)
                {
                    if (hazardReplacements[i].replacingHazard.CompareOrdinal(extendedHazardNames[j]))
                    {
                        replacingHazard = currentDungeon.IndoorMapHazards[j];

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