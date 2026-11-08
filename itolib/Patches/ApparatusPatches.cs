using HarmonyLib;
using System;

namespace itolib.Patches
{
    internal static class ApparatusPatches
    {
        internal static event Action<LungProp>? OnApparatusPulled;
        internal static event Action? OnRadiationWarningHUD;

        [HarmonyPatch(typeof(LungProp), nameof(LungProp.EquipItem))]
        [HarmonyPrefix]
        private static void LungPropEquipItemPre(LungProp __instance)
        {
            if (__instance.isLungDocked)
            {
                InvokeApparatusPullEvent(__instance);
            }
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.RadiationWarningHUD))]
        [HarmonyPrefix]
        private static void RadiationWarningHUDPre()
        {
            try
            {
                OnRadiationWarningHUD?.Invoke();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Exception during 'RadiationWarningHUD' event: {e}");
            }
        }

        internal static void InvokeApparatusPullEvent(LungProp apparatus)
        {
            try
            {
                OnApparatusPulled?.Invoke(apparatus);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Exception during 'ApparatusPulled' event: {e}");
            }
        }
    }
}