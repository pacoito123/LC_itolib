using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using itolib.Patches;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace itolib
{
    /// <summary>
    ///     
    /// </summary>
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID, LethalLevelLoader.Plugin.ModVersion)]
    [BepInPlugin(GUID, PLUGIN_NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string GUID = "pacoito.itolib", PLUGIN_NAME = "itolib", VERSION = "0.0.0";
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
                Harmony = new(GUID);
                //

                NetcodePatcher();

                // Apply all patches.
                Harmony.PatchAll(typeof(LoadPatch));
                // ...

                StaticLogger.LogInfo($"{PLUGIN_NAME} loaded!");
            }
            catch (Exception e)
            {
                StaticLogger.LogError($"Error while initializing '{PLUGIN_NAME}': {e}");
            }
        }

        private static void NetcodePatcher()
        {
            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = [.. e.Types.Where(type => type != null)];
            }

            foreach (Type type in types)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
                    {
                        _ = method.Invoke(null, null);
                    }
                }
            }
        }
    }
}