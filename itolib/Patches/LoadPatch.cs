using HarmonyLib;
using itolib.Compatibility;

namespace itolib.Patches
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [HarmonyPatch]
    internal class LoadPatch
    {
        public static bool FirstLoad { get; private set; } = true;

        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
        [HarmonyPrefix]
        private static void MenuManagerStartPre()
        {
            if (!FirstLoad)
            {
                return;
            }

            FirstLoad = false;

            if (CrowdControlCompatibility.Enabled)
            {
                Plugin.Harmony.PatchAll(typeof(CrowdControlCompatibility));
            }
        }
    }
}