using BepInEx.Configuration;

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
            cfg.OrphanedEntries.Clear();

            // Re-enable saving and save config.
            cfg.SaveOnConfigSet = true;
            cfg.Save();
        }
    }
}