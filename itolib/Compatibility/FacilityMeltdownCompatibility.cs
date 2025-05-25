using System.Runtime.CompilerServices;
using FacilityMeltdown;
using FacilityMeltdown.API;
using itolib.PlayZone;
using Unity.Netcode;

namespace itolib.Compatibility
{
    /// <summary>
    ///     TODO.
    /// </summary>
    internal class FacilityMeltdownCompatibility
    {
        /// <summary>
        ///     Whether WeatherRegistry is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.loaforc.facilitymeltdown");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        /// <summary>
        ///     TODO.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void HalveTwinValue(TwinApparatus twinApparatus)
        {
            if (MeltdownPlugin.config.OverrideApparatusValue)
            {
                twinApparatus.scrapValue /= 2;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="bothPulled"></param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TwinMeltdown(bool bothPulled)
        {
            if (bothPulled && NetworkManager.Singleton.IsHost)
            {
                MeltdownAPI.StartMeltdown(Plugin.GUID);
            }
        }
    }
}