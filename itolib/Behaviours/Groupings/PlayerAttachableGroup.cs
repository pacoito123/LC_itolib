using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using System;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     Represents a group of <c>PlayerAttachable</c> components to perform bulk operations on.
    /// </summary>
    public class PlayerAttachableGroup : ComponentGroup<PlayerAttachable>
    {
        /// <summary>
        ///     All actions able to be performed on every <c>PlayerAttachable</c> component in the group.
        /// </summary>
        private enum AttachableActions : byte
        {
            AttachPlayer,
            DetachPlayer,
            TransferPlayer
        }

        /// <inheritdoc/>
        protected override void PerformSingleAction(PlayerAttachable attachable, Enum actionID, object? parameter = null)
        {
            if (actionID is not AttachableActions attachableActionID)
            {
                return;
            }

            PlayerControllerB playerToAttach = null!;
            if (attachableActionID is not AttachableActions.DetachPlayer)
            {
                if (parameter is not PlayerControllerB player)
                {
                    return;
                }
                playerToAttach = player;
            }

            switch (attachableActionID)
            {
                case AttachableActions.AttachPlayer:
                    attachable.AttachPlayer(playerToAttach);
                    break;
                case AttachableActions.TransferPlayer:
                    attachable.TransferPlayer(playerToAttach);
                    break;
                case AttachableActions.DetachPlayer:
                    attachable.DetachPlayer();
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void EnableSingleComponent(PlayerAttachable attachable, bool enabled)
        {
            attachable.EnableAttaching(enabled);
        }

        /// <inheritdoc/>
        protected override void ToggleSingleComponent(PlayerAttachable attachable)
        {
            attachable.ToggleAttaching();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void AttachPlayerAll(PlayerControllerB player)
        {
            PerformGroupAction(AttachableActions.AttachPlayer, player);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DetachPlayerAll()
        {
            PerformGroupAction(AttachableActions.DetachPlayer);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void TransferPlayerAll(PlayerControllerB player)
        {
            PerformGroupAction(AttachableActions.TransferPlayer, player);
        }
    }
}