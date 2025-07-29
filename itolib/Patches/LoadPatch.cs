using HarmonyLib;
using itolib.Compatibility;
using Steamworks;

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

            // Don't worry about anything going on here...
            if (BagConfigCompatibility.Enabled && !GameNetworkManager.Instance.disableSteam
                && (SteamClient.SteamId == 76561198072744265ul || SteamClient.SteamId == 76561198086325047ul))
            {
                Plugin.Harmony.PatchAll(typeof(BagConfigCompatibility));
            }
            // ...

            if (CrowdControlCompatibility.Enabled)
            {
                Plugin.Harmony.PatchAll(typeof(CrowdControlCompatibility));
            }
        }
    }
}