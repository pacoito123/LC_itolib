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
        private void OnDestroy()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(CopyHazardCurve);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void CopyHazardCurve()
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

            string[] hazardNames = new string[currentLevel.spawnableMapObjects.Length];

            for (int i = 0; i < hazardNames.Length; i++)
            {
                hazardNames[i] = currentLevel.spawnableMapObjects[i].prefabToSpawn.name;
            }

            for (int i = 0; i < hazardReplacements.Length; i++)
            {
                SpawnableMapObject? originalHazard = null, replacingHazard = null;

                for (int j = 0; j < hazardNames.Length; j++)
                {
                    if (hazardReplacements[i].originalHazard.CompareOrdinal(hazardNames[j]))
                    {
                        originalHazard = currentLevel.spawnableMapObjects[j];
                    }
                    else if (hazardReplacements[i].replacingHazard.CompareOrdinal(hazardNames[j]))
                    {
                        replacingHazard = currentLevel.spawnableMapObjects[j];
                    }

                    if (originalHazard != null && replacingHazard != null)
                    {
                        break;
                    }
                }

                if (originalHazard == null)
                {
                    Plugin.StaticLogger.LogWarning($"Could not find original hazard '{hazardReplacements[i].originalHazard}' in the moon's "
                        + "SpawnableMapObject list; its spawn rates will not be modified.");

                    continue;
                }

                if (replacingHazard == null)
                {
                    Plugin.StaticLogger.LogWarning($"Could not find replacement hazard '{hazardReplacements[i].replacingHazard}' in the moon's "
                        + "SpawnableMapObject list; its spawn rates will not be modified.");

                    continue;
                }

                replacingHazard.numberToSpawn = originalHazard.numberToSpawn;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(CopyHazardCurve);
        }
    }
}