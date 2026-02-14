using HarmonyLib;
using System;

namespace itolib.Patches
{
    [HarmonyPatch]
    internal sealed class ApparatusPatches
    {
        internal static event Action? OnRadiationWarningHUD;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.RadiationWarningHUD))]
        [HarmonyPrefix]
        private static void RadiationWarningHUDPre()
        {
            OnRadiationWarningHUD?.Invoke();
        }
    }
}