using LethalLevelLoader;
using UnityEngine.Events;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class DungeonStoryLog : StoryLog
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<int> onLogSpawned = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<int> onAlreadyUnlocked = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Start()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow == null)
            {
                return;
            }

            foreach (ExtendedStoryLog extendedStoryLog in DungeonManager.CurrentExtendedDungeonFlow.ExtendedMod.ExtendedStoryLogs)
            {
                if (string.CompareOrdinal(extendedStoryLog.sceneName, DungeonManager.CurrentExtendedDungeonFlow.DungeonName) == 0
                    && storyLogID == extendedStoryLog.storyLogID)
                {
                    // Publicized LLL for access to 'ExtendedStoryLog.newStoryLogID' specifically...
                    if (!TerminalManager.Terminal.unlockedStoryLogs.Contains(extendedStoryLog.newStoryLogID))
                    {
                        onLogSpawned.Invoke(extendedStoryLog.newStoryLogID);
                        storyLogID = extendedStoryLog.newStoryLogID;
                    }
                    else
                    {
                        onAlreadyUnlocked.Invoke(extendedStoryLog.newStoryLogID);
                        RemoveLogCollectible();
                    }

                    break;
                }
            }
        }
    }
}