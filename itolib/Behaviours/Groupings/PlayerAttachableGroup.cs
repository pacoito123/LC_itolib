using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using System;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerAttachableGroup : ComponentGroup<PlayerAttachable>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private enum AttachableActions
        {
            AttachPlayer,
            DetachPlayer,
            TransferPlayer
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="attachable"></param>
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
        protected override void PerformSingleAction(PlayerAttachable attachable, Enum actionID, object? parameter = null)
        {
            if (actionID is not AttachableActions attachableActionID)
            {
                return;
            }

            if ((int)attachableActionID < 2 || parameter is not PlayerControllerB player)
            {
                return;
            }

            switch (attachableActionID)
            {
                case AttachableActions.AttachPlayer:
                    attachable.AttachPlayer(player);
                    break;
                case AttachableActions.TransferPlayer:
                    attachable.TransferPlayer(player);
                    break;
                case AttachableActions.DetachPlayer:
                    attachable.DetachPlayer();
                    break;
                default:
                    break;
            }
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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="attachable"></param>
        /// <param name="enabled"></param>
        protected override void EnableSingleComponent(PlayerAttachable attachable, bool enabled)
        {
            attachable.EnableAttaching(enabled);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="attachable"></param>
        protected override void ToggleSingleComponent(PlayerAttachable attachable)
        {
            attachable.ToggleAttaching();
        }
    }
}