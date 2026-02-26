using BepInEx.Logging;
using FacilityMeltdown;
using FacilityMeltdown.API;
using FacilityMeltdown.MeltdownSequence;
using HarmonyLib;
using itolib.PlayZone;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace itolib.Compatibility
{
    /// <summary>
    ///     Compatibility with <c>FacilityMeltdown</c> for <c>EventfulApparatus</c> and <c>ApparatusEvent</c>.
    /// </summary>
    [HarmonyPatch]
    internal sealed class FacilityMeltdownCompatibility
    {
        /// <summary>
        ///     Whether <c>FacilityMeltdown</c> is present in the BepInEx Chainloader or not.
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

        internal static event Action? OnMeltdownLightsOn;
        internal static event Action? OnMeltdownLightsOff;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void HalveTwinValue(TwinApparatus twinApparatus)
        {
            if (MeltdownPlugin.config.OverrideApparatusValue)
            {
                twinApparatus.SetScrapValue((int)(twinApparatus.scrapValue * 0.5));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void InitiateMeltdown()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                MeltdownAPI.StartMeltdown(Plugin.PLUGIN_GUID);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void RegisterMeltdownListener(Action listener, bool remove = false)
        {
            if (!remove)
            {
                MeltdownAPI.OnMeltdownStart += listener;
            }
            else
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                MeltdownAPI.OnMeltdownStart -= listener;
#pragma warning restore CS8601 // Possible null reference assignment.
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [HarmonyPatch(typeof(MeltdownEffects), nameof(MeltdownEffects.EmergencyLights), MethodType.Enumerator)]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> EmergencyLightsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo? meltdownLoggerInfo = typeof(MeltdownPlugin).GetField("logger", BindingFlags.Static | BindingFlags.NonPublic);
            if (meltdownLoggerInfo == null)
            {
                Plugin.StaticLogger.LogError("Failed to find logger field in 'FacilityMeltdown.MeltdownPlugin'!");
                return instructions;
            }

            MethodInfo logDebugInfo = typeof(ManualLogSource).GetMethod(nameof(ManualLogSource.LogDebug), BindingFlags.Instance | BindingFlags.Public);
            CodeMatcher codeMatcher = new CodeMatcher(instructions).MatchForward(useEnd: true,
                new(OpCodes.Ldsfld, meltdownLoggerInfo),
                new(OpCodes.Ldstr, "Switching lights ON"),
                new(OpCodes.Callvirt, logDebugInfo));

            if (codeMatcher.Advance(1).IsInvalid)
            {
                Plugin.StaticLogger.LogError("Failed to match lights switching on code in 'MeltdownEffects.EmergencyLights'!");
                return instructions;
            }

            MethodInfo onMeltdownLightsSwitchInfo = typeof(FacilityMeltdownCompatibility).GetMethod(nameof(OnMeltdownLightsSwitch), BindingFlags.Static | BindingFlags.NonPublic);
            _ = codeMatcher.Insert(
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Call, onMeltdownLightsSwitchInfo))
            .MatchForward(useEnd: true,
                new(OpCodes.Ldsfld, meltdownLoggerInfo),
                new(OpCodes.Ldstr, "Switching lights OFF"),
                new(OpCodes.Callvirt, logDebugInfo));

            if (codeMatcher.Advance(1).IsInvalid)
            {
                Plugin.StaticLogger.LogError("Failed to match lights switching off code in 'MeltdownEffects.EmergencyLights'!");
                return instructions;
            }

            return codeMatcher.Insert(
                new(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, onMeltdownLightsSwitchInfo))
            .InstructionEnumeration();
        }

        private static void OnMeltdownLightsSwitch(bool on)
        {
            if (on)
            {
                OnMeltdownLightsOn?.Invoke();
            }
            else
            {
                OnMeltdownLightsOff?.Invoke();
            }
        }
    }
}