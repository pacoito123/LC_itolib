using HarmonyLib;
using LethalCompanyTestMod;
using System;
using System.Runtime.CompilerServices;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility between WeatherConditional and CrowdControl.
    /// </summary>
    [HarmonyPatch]
    internal sealed class CrowdControlCompatibility
    {
        /// <summary>
        ///     Whether CrowdControl is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("WarpWorld.CrowdControl");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        public static LevelWeatherType CurrentWeather { get; internal set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public static event Action<LevelWeatherType, LevelWeatherType>? OnCCWeatherChanged;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [HarmonyPatch(typeof(TestMod), nameof(TestMod.CrowdControlCommands))]
        [HarmonyPostfix]
        internal static void CCWeatherCheck()
        {
            if (TimeOfDay.Instance != null && TimeOfDay.Instance.currentLevel != null && TimeOfDay.Instance.currentLevel.currentWeather != CurrentWeather)
            {
                OnCCWeatherChanged?.Invoke(CurrentWeather, TimeOfDay.Instance.currentLevel.currentWeather);
                CurrentWeather = TimeOfDay.Instance.currentLevel.currentWeather;
            }
        }
    }
}