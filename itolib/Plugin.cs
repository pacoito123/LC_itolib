using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using itolib.Patches;
using itolib.Structs;
using System;
using Unity.Netcode;

namespace itolib
{
    /// <summary>
    ///     Wondrous gizmos and gadgets for the restless mind.
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public const string PLUGIN_GUID = "pacoito.itolib", PLUGIN_NAME = "itolib", VERSION = "0.9.3";
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
                Harmony.PatchAll(typeof(ApparatusPatches));
                Harmony.PatchAll(typeof(DoorwayPatch));
                Harmony.PatchAll(typeof(EntranceTeleportPatch));
                Harmony.PatchAll(typeof(GameNetworkManagerPatch));
                Harmony.PatchAll(typeof(LoadPatch));
                Harmony.PatchAll(typeof(RoundManagerPatches));
                // ...

                // Special moon compatibilities:
                // BerunahCompatibility.RegisterCompat();
                // ...

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