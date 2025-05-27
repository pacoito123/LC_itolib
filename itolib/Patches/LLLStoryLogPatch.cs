using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;

namespace itolib.Patches
{
    /// <summary>
    ///     Patch for re-adding ExtendedStoryLog instances to the Terminal upon lobby reload; should only run on LLL v1.4.11 or lower.
    /// </summary>
    [HarmonyPatch]
    internal class LLLStoryLogPatch
    {
        /// <summary>
        ///     Cached list of TerminalNode instances corresponding to ExtendedStoryLog IDs.
        /// </summary>
        public static Dictionary<int, TerminalNode?>? LLLStoryLogNodes { get; internal set; }

        [HarmonyPatch(typeof(TerminalManager), nameof(TerminalManager.CreateStoryLogTerminalData))]
        [HarmonyPostfix]
        internal static void CacheStoryLog(ExtendedStoryLog newStoryLog)
        {
            // Cache TerminalNode instance of the last (newly added) element in the 'Terminal.logEntryFiles' list.
            LLLStoryLogNodes?.Add(newStoryLog.newStoryLogID, LethalLevelLoader.Patches.Terminal?.logEntryFiles[^1]);
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        [HarmonyAfter(LethalLevelLoader.Plugin.ModGUID)]
        [HarmonyPrefix]
        internal static void PopulateStoryLogs()
        {
            if (LLLStoryLogNodes?.Count > 0)
            {
                // Load ExtendedStoryLog journal entries.
                foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                {
                    if (extendedMod.ExtendedStoryLogs.Count > 0)
                    {
                        foreach (ExtendedStoryLog extendedStoryLog in extendedMod.ExtendedStoryLogs)
                        {
                            if (LLLStoryLogNodes?[extendedStoryLog.newStoryLogID] != null)
                            {
                                LethalLevelLoader.Patches.Terminal?.logEntryFiles.Add(LLLStoryLogNodes[extendedStoryLog.newStoryLogID]);
                            }
                        }
                    }
                }
                // ...
            }
        }
    }
}