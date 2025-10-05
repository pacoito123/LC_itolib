using GameNetcodeStuff;
using itolib.Extensions;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;
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
        [Space(5.0f)]
        [Header("Dungeon Story Log")]
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
        [SerializeField] private UnityEvent<int> onAlreadyUnlocked = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Start()
        {
            foreach (ExtendedStoryLog extendedStoryLog in DungeonManager.CurrentExtendedDungeonFlow.ExtendedMod.ExtendedStoryLogs)
            {
                if (extendedStoryLog.sceneName.CompareOrdinal(DungeonManager.CurrentExtendedDungeonFlow.DungeonName)
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