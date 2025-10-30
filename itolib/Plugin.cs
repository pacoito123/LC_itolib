using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using itolib.Compatibility;
using itolib.Patches;
using itolib.Structs;
using System;
using Unity.Netcode;

namespace itolib
{
    /// <summary>
    ///     Wondrous gizmos and gadgets for the restless mind.
    /// </summary>
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID, LethalLevelLoader.Plugin.ModVersion)]
    [BepInDependency("WarpWorld.CrowdControl", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.teamxiaolan.dawnlib", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.loaforc.facilitymeltdown", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("bgn.pizzatowerescapemusic", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("rattenbonkers.TVLoader", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public const string PLUGIN_GUID = "pacoito.itolib", PLUGIN_NAME = "itolib", VERSION = "0.6.0";
        internal static ManualLogSource StaticLogger { get; private set; } = null!;

        /// <summary>
        ///     Harmony instance for patching.
        /// </summary>
        internal static Harmony Harmony { get; private set; } = null!;

        private void Awake()
        {
            StaticLogger = Logger;

            try
            {
                // Initialize 'Config' and 'Harmony' instances.
                // Settings = new(Config);
                Harmony = new(PLUGIN_GUID);
                // ...

                SerializeNetworkVariables();

                // Apply all patches.
                Harmony.PatchAll(typeof(DoorwayPatch));
                Harmony.PatchAll(typeof(LoadPatch));
                // ...

                // Patch LLL's ExtendedStoryLogs if on v1.4.11 or lower.
                if (BepInEx.Bootstrap.Chainloader.PluginInfos[LethalLevelLoader.Plugin.ModGUID].Metadata.Version
                    .CompareTo(new(1, 4, 11)) <= 0)
                {
                    LLLStoryLogPatch.LLLStoryLogNodes = [];
                    Harmony.PatchAll(typeof(LLLStoryLogPatch));
                }
                // ...

                if (WeatherRegistryCompatibility.Enabled)
                {
                    Harmony.PatchAll(typeof(WeatherRegistryCompatibility));
                }

                StaticLogger.LogInfo($"{PLUGIN_NAME} v{VERSION} loaded!");
            }
            catch (Exception e)
            {
                StaticLogger.LogError($"Error while initializing '{PLUGIN_NAME}': {e}");
            }
        }

        private static void SerializeNetworkVariables()
        {
            NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<ItemInfo>();
            NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<ItemInfo>();

            NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<HiveInfo>();
            NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<HiveInfo>();

            NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<NetworkObjectReference>();
            NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<NetworkObjectReference>();
        }
    }
}