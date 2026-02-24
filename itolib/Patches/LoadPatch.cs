using HarmonyLib;
using itolib.Compatibility;

namespace itolib.Patches
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [HarmonyPatch]
    internal sealed class LoadPatch
    {
        private static bool firstLoad = true;

        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
        [HarmonyPrefix]
        private static void MenuManagerStartPre()
        {
            if (!firstLoad)
            {
                return;
            }
            firstLoad = false;

            if (CrowdControlCompatibility.Enabled)
            {
                Plugin.Harmony.PatchAll(typeof(CrowdControlCompatibility));
            }

            if (FacilityMeltdownCompatibility.Enabled)
            {
                Plugin.Harmony.PatchAll(typeof(FacilityMeltdownCompatibility));
            }
        }
    }
}