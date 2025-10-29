using System.Runtime.CompilerServices;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility between WeatherConditional and WeatherTweaks.
    /// </summary>
    internal sealed class WeatherTweaksCompatibility
    {
        /// <summary>
        ///     Whether WeatherTweaks is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("WeatherTweaks");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool IsCombinedWeather(out LevelWeatherType[]? allWeathers, LevelWeatherType weather)
        {
            allWeathers = null;

            if (WeatherManager.GetWeather(weather) is CombinedWeatherType combinedWeather)
            {
                allWeathers = new LevelWeatherType[combinedWeather.WeatherTypes.Count];

                for (int i = 0; i < allWeathers.Length; i++)
                {
                    allWeathers[i] = combinedWeather.WeatherTypes[i].WeatherType;
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool IsProgressingWeather(LevelWeatherType weather)
        {
            return WeatherManager.GetWeather(weather) is ProgressingWeatherType;
        }
    }
}