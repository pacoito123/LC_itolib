using HarmonyLib;
using itolib.Compatibility;
using System;

namespace itolib.Patches
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [HarmonyPatch]
    internal class LoadPatch
    {
        public static bool FirstLoad { get; private set; } = true;

        public static event Action? OnFinishGeneratingLevelPost;

        public static event Action? OnFinishGeneratingNewLevelClientRpcPost;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingLevel))]
        [HarmonyPostfix]
        private static void FinishGeneratingLevelPost()
        {
            OnFinishGeneratingLevelPost?.Invoke();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc)), HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        private static void FinishGeneratingNewLevelClientRpcPost()
        {
            OnFinishGeneratingNewLevelClientRpcPost?.Invoke();
        }

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