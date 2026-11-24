using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace itolib.Patches
{
    internal static class OutOfBoundsPatch
    {
        internal static float currentOutOfBoundsHeight = float.MaxValue;

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LateUpdate))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Low)]
        private static IEnumerable<CodeInstruction> StartOfRoundLateUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo gameNetworkManagerGetter = typeof(GameNetworkManager).GetProperty(nameof(GameNetworkManager.Instance), BindingFlags.Static | BindingFlags.Public).GetGetMethod();
            FieldInfo localPlayerControllerInfo = typeof(GameNetworkManager).GetField(nameof(GameNetworkManager.localPlayerController), BindingFlags.Instance | BindingFlags.Public);
            MethodInfo componentTransformGetter = typeof(Component).GetProperty(nameof(Component.transform), BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            MethodInfo transformPositionGetter = typeof(Transform).GetProperty(nameof(Transform.position), BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            FieldInfo vectorYInfo = typeof(Vector3).GetField(nameof(Vector3.y), BindingFlags.Instance | BindingFlags.Public);
            CodeMatcher codeMatcher = new CodeMatcher(instructions).MatchForward(useEnd: true,
                new(OpCodes.Call, gameNetworkManagerGetter),
                new(OpCodes.Ldfld, localPlayerControllerInfo),
                new(OpCodes.Callvirt, componentTransformGetter),
                new(OpCodes.Callvirt, transformPositionGetter),
                new(OpCodes.Ldfld, vectorYInfo),
                new(OpCodes.Ldc_R4)); // -> -600.0f

            if (codeMatcher.IsInvalid)
            {
                return instructions;
            }

            MethodInfo obtainLowestHeightInfo = typeof(OutOfBoundsPatch).GetMethod(nameof(OverrideOutOfBoundsHeight), BindingFlags.Static | BindingFlags.NonPublic);
            return codeMatcher.Repeat(codeMatcher =>
            {
                object currentHeight = codeMatcher.Operand; // -> -600.0f

                _ = codeMatcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldc_R4, currentHeight))
                .Set(OpCodes.Call, obtainLowestHeightInfo);
            }).InstructionEnumeration();
        }

        private static float OverrideOutOfBoundsHeight(float y)
        {
            if (currentOutOfBoundsHeight > y)
            {
                currentOutOfBoundsHeight = y;
            }

            return currentOutOfBoundsHeight;
        }
    }
}