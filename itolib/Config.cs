using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace itolib
{
    /// <summary>
    ///     Class containing and defining various configuration options.
    /// </summary>
    public class Config
    {
        /// <summary>
        ///     Constructor for initializing plugin configuration.
        /// </summary>
        /// <param name="cfg">BepInEx configuration file.</param>
        public Config(ConfigFile cfg)
        {
            // Disable saving config after a call to 'Bind()' is made.
            cfg.SaveOnConfigSet = false;

            // Bind config entries to the config file.
            // ...

            // Remove old config settings.
            ClearOrphanedEntries(cfg);

            // Re-enable saving and save config.
            cfg.SaveOnConfigSet = true;
            cfg.Save();
        }

        /// <summary>
        ///     Remove old (orphaned) configuration entries.
        /// </summary>
        /// <remarks>Obtained from: https://lethal.wiki/dev/intermediate/custom-configs#better-configuration</remarks>
        /// <param name="config">The config file to clear orphaned entries from.</param>
        private void ClearOrphanedEntries(ConfigFile config)
        {
            // Obtain 'OrphanedEntries' dictionary from ConfigFile through reflection.
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string>? orphanedEntries = (Dictionary<ConfigDefinition, string>?)orphanedEntriesProp.GetValue(config);

            // Clear orphaned entries.
            orphanedEntries?.Clear();
        }
    }
}