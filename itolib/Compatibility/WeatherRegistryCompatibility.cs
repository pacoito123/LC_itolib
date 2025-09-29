using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using WeatherRegistry.Networking;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility between WeatherConditional and WeatherRegistry.
    /// </summary>
    [HarmonyPatch]
    internal sealed class WeatherRegistryCompatibility
    {
        /// <summary>
        ///     Whether WeatherRegistry is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrov.WeatherRegistry");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        /// <summary>
        ///     TODO.
        /// </summary>
        internal static event Action<LevelWeatherType[]>? OnWeatherEffectsApply;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [HarmonyPatch(typeof(WeatherEffectData), "ApplyWeatherEffects")]
        [HarmonyPostfix]
        internal static void ApplyWeatherEffectsPost(LevelWeatherType[] weatherType)
        {
            OnWeatherEffectsApply?.Invoke(weatherType);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool ApplyWeatherOverrides(Action<LevelWeatherType[]> weatherAction)
        {
            if (WeatherRegistry.WeatherManager.GetCurrentLevelWeather().Type is WeatherRegistry.WeatherType.Clear)
            {
                return false;
            }

            OnWeatherEffectsApply += weatherAction;
            return true;
        }
    }
}