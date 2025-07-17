using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using itolib.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace itolib.ScriptableObjects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct BoolEntry : IScriptableConfigEntry<bool>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [field: SerializeField] public string Section { get; set; } = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: SerializeField] public string Key { get; set; } = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: SerializeField] public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: SerializeField] public bool DefaultValue { get; set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public BoolEntry() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "itolib/Config/ScriptableConfig")]
    public class ScriptableConfig : ScriptableObject
    {
        /// <summary>
        ///     Plugin configuration instance.
        /// </summary>
        public ConfigFile Config { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<ConfigEntryBase> ConfigEntries { get; private set; } = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Scriptable Config")]
        [Tooltip("")]
        public string authorName = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string modName = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<BoolEntry> boolEntries = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            string path = Application.isEditor ? "." : Paths.ConfigPath;

            Config = new ConfigFile(Utility.CombinePaths(path, $"{authorName}.{modName}.cfg"), false, null)
            {
                // Disable saving config after a call to 'Bind()' is made.
                SaveOnConfigSet = false
            };

            foreach (BoolEntry entry in boolEntries)
            {
                ConfigEntry<bool> configEntry = Config.Bind(entry.Section, entry.Key, entry.DefaultValue, entry.Description);

                ConfigEntries.Add(configEntry);
            }

            // Remove old config settings.
            ClearOrphanedEntries(Config);

            // Re-enable saving and save config.
            Config.SaveOnConfigSet = true;
            Config.Save();
        }

        /// <summary>
        ///     Remove old (orphaned) configuration entries.
        /// </summary>
        /// <remarks>Obtained from: https://lethal.wiki/dev/intermediate/custom-configs#better-configuration</remarks>
        /// <param name="config">The config file to clear orphaned entries from.</param>
        public static void ClearOrphanedEntries(ConfigFile config)
        {
            // Obtain 'OrphanedEntries' dictionary from ConfigFile through reflection.
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string>? orphanedEntries = (Dictionary<ConfigDefinition, string>?)orphanedEntriesProp.GetValue(config);

            // Clear orphaned entries.
            orphanedEntries?.Clear();
        }
    }
}