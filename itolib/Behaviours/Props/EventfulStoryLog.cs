using GameNetcodeStuff;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Props
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class EventfulStoryLog : StoryLog
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Eventful Story Log")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onLogSpawned = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onLogCollected = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onLogAlreadyUnlocked = new();

        /* /// <summary>
        ///     TODO.
        /// </summary>
        public new void Start()
        {
            foreach (ExtendedStoryLog extendedStoryLog in LevelManager.CurrentExtendedLevel.ExtendedMod.ExtendedStoryLogs)
            {
                if (extendedStoryLog.sceneName.CompareOrdinal(LevelManager.CurrentExtendedLevel.SelectableLevel.sceneName)
                    && storyLogID == extendedStoryLog.storyLogID)
                {
                    InitializeStoryLog(extendedStoryLog);

                    return;
                }
            }

            foreach (ExtendedStoryLog extendedStoryLog in DungeonManager.CurrentExtendedDungeonFlow.ExtendedMod.ExtendedStoryLogs)
            {
                if (extendedStoryLog.sceneName.CompareOrdinal(DungeonManager.CurrentExtendedDungeonFlow.DungeonName)
                    && storyLogID == extendedStoryLog.storyLogID)
                {
                    InitializeStoryLog(extendedStoryLog);

                    return;
                }
            }

            InitializeStoryLog();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="extendedStoryLog"></param>
        private void InitializeStoryLog(ExtendedStoryLog extendedStoryLog)
        {
            // Publicized LLL for access to 'ExtendedStoryLog.newStoryLogID' specifically...
            storyLogID = extendedStoryLog.newStoryLogID;

            InitializeStoryLog();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void InitializeStoryLog()
        {
            if (storyLogID < 0 || storyLogID >= TerminalManager.Terminal.logEntryFiles.Count)
            {
                // TODO: Log warning.
                return;
            }

            if (!TerminalManager.Terminal.unlockedStoryLogs.Contains(storyLogID))
            {
                onLogSpawned.Invoke(storyLogID);
            }
            else
            {
                onLogAlreadyUnlocked.Invoke(storyLogID);
                RemoveLogCollectible();
            }
        } */

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void CollectLogSynced(PlayerControllerB player)
        {
            CollectLogLocal(player);

            if (IsSpawned)
            {
                CollectLogRpc(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void CollectLogRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                CollectLogLocal(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        private void CollectLogLocal(PlayerControllerB player)
        {
            if (player.IsLocalClient())
            {
                CollectLog();
                onLogCollected.Invoke(storyLogID);

                return;
            }

            if (collected || storyLogID == -1)
            {
                return;
            }
            collected = true;

            RemoveLogCollectible();
            onLogCollected.Invoke(storyLogID);
        }
    }
}