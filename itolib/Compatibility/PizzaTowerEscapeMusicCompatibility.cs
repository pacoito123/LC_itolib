using itolib.PlayZone;
using PizzaTowerEscapeMusic;
using System.Runtime.CompilerServices;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility between TwinApparatus and PizzaTowerEscapeMusic.
    /// </summary>
    internal sealed class PizzaTowerEscapeMusicCompatibility
    {
        /// <summary>
        ///     Whether PizzaTowerEscapeMusic is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bgn.pizzatowerescapemusic");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="twin"></param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SwitchTwin(TwinApparatus? twin)
        {
            if (twin != null && twin.isLungDocked)
            {
                GameEventListener.dockedApparatus = twin;
            }
        }
    }
}