using BepInEx.Logging;
using FacilityMeltdown;
using FacilityMeltdown.API;
using FacilityMeltdown.MeltdownSequence;
using HarmonyLib;
using itolib.PlayZone;
using System;
using System.Collections.Generic;
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
                MeltdownAPI.OnMeltdownStart -= listener;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [HarmonyPatch(typeof(MeltdownEffects), nameof(MeltdownEffects.EmergencyLights), MethodType.Enumerator)]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> EmergencyLightsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions).MatchForward(useEnd: true,
                new(OpCodes.Ldsfld, typeof(MeltdownPlugin).GetField(nameof(MeltdownPlugin.logger), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)),
                new(OpCodes.Ldstr, "Switching lights ON"),
                new(OpCodes.Callvirt, typeof(ManualLogSource).GetMethod(nameof(ManualLogSource.LogDebug), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)));

            if (codeMatcher.Advance(1).IsInvalid)
            {
                Plugin.StaticLogger.LogError("Failed to match code in 'MeltdownEffects.EmergencyLights'!");
                return instructions;
            }

            _ = codeMatcher.Insert(Transpilers.EmitDelegate(() => OnMeltdownLightsOn?.Invoke())).MatchForward(useEnd: true,
                new(OpCodes.Ldsfld, typeof(MeltdownPlugin).GetField(nameof(MeltdownPlugin.logger), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)),
                new(OpCodes.Ldstr, "Switching lights OFF"),
                new(OpCodes.Callvirt, typeof(ManualLogSource).GetMethod(nameof(ManualLogSource.LogDebug), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)));

            if (codeMatcher.Advance(1).IsInvalid)
            {
                Plugin.StaticLogger.LogError("Failed to match code in 'MeltdownEffects.EmergencyLights'!");
                return instructions;
            }

            return codeMatcher.Insert(Transpilers.EmitDelegate(() => OnMeltdownLightsOff?.Invoke()))
                .InstructionEnumeration();
        }
    }
}