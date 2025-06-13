using System.Runtime.CompilerServices;
using HarmonyLib;
using TVLoader.Patches;
using TVLoader.Utils;
using UnityEngine.Video;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility between DungeonTelevision and TVLoader.
    /// </summary>
    [HarmonyPatch]
    internal sealed class TVLoaderCompatibility
    {
        /// <summary>
        ///     Whether TVLoader is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("rattenbonkers.TVLoader");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void PrepareVideo(TVScript tv)
        {
            if (TVScriptPatches.currentVideoPlayer == null)
            {
                TVScriptPatches.currentVideoPlayer = tv.GetComponent<VideoPlayer>();
                TVScriptPatches.renderTexture = TVScriptPatches.currentVideoPlayer.targetTexture;

                if (VideoManager.Videos.Count > 0)
                {
                    TVScriptPatches.PrepareVideo(tv);
                }
            }
        }
    }
}