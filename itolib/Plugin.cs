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
        ///     BepInEx Plugin information.
        /// </summary>
        public const string PLUGIN_GUID = "pacoito.itolib", PLUGIN_NAME = "itolib", VERSION = "0.9.3";
        internal static new ManualLogSource Logger => field ??= BepInEx.Logging.Logger.CreateLogSource(PLUGIN_NAME);
        internal static Harmony Harmony => field ??= new(PLUGIN_GUID);

        private void Awake()
        {
            try
            {
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

                Logger.LogInfo($"{PLUGIN_NAME} v{VERSION} loaded!");
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while initializing '{PLUGIN_NAME}': {e}");
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