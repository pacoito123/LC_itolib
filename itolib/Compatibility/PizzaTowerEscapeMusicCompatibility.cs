using itolib.Behaviours.Grabbables;
using PizzaTowerEscapeMusic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility for PizzaTowerEscapeMusic.
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
        /// <param name="apparatus"></param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void SwitchApparatus(EventfulApparatus? apparatus)
        {
            if (apparatus == null)
            {
                LungProp[] apparatuses = Object.FindObjectsByType<LungProp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                for (int i = 0; i < apparatuses.Length; i++)
                {
                    if (apparatuses[i] is not EventfulApparatus && apparatuses[i].isLungDocked)
                    {
                        GameEventListener.dockedApparatus = apparatuses[i];

                        return;
                    }
                }
            }

            GameEventListener.dockedApparatus = apparatus;
        }
    }
}