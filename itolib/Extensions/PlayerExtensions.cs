using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsLocalClient(this PlayerControllerB player)
        {
            return player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsSpectatedClient(this PlayerControllerB player)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            return localPlayer.isPlayerDead && StartOfRound.Instance != null && !StartOfRound.Instance.overrideSpectateCamera
                && localPlayer.spectatedPlayerScript != null && localPlayer.spectatedPlayerScript.actualClientId == player.actualClientId;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="range"></param>
        /// <param name="proximityAwareness"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static bool HasLineOfSightToPosition(this PlayerControllerB player, Vector3 pos, float width = 45f, float range = 60,
            float proximityAwareness = -1f, LayerMask layerMask = default)
        {
            float sqrDistance = (player.transform.position - pos).sqrMagnitude;

            return sqrDistance < range * range && (Vector3.Angle(player.playerEye.transform.forward, pos - player.gameplayCamera.transform.position) < width
                || (proximityAwareness > 0 && sqrDistance < proximityAwareness * proximityAwareness)) && !Physics.Linecast(player.playerEye.transform.position,
                    pos, out player.hit, layerMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playerAction"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static bool TryFindMovementAction(this PlayerControllerB player, out InputAction playerAction, string actionId)
        {
            playerAction = null!;

            if (player.playerActions != null && player.playerActions.m_Movement != null)
            {
                playerAction = player.playerActions.m_Movement.FindAction(actionId);
            }

            return playerAction != null;
        }

        /// <summary>
        ///     Force a player to drop an item held in a specified item slot.
        /// </summary>
        /// <param name="player">Player who shall drop the item.</param>
        /// <param name="slot">Inventory slot to target.</param>
        public static void DiscardHeldObject(this PlayerControllerB player, int slot)
        {
            // Check if slot number is valid.
            if (slot <= -1 || slot >= player.ItemSlots.Length)
            {
                return;
            }

            // Attempt to obtain item at the specified item slot.
            GrabbableObject? item = player.ItemSlots[slot];

            // Check if item exists at the specified item slot.
            if (item == null)
            {
                return;
            }

            // Pretty much the same as 'PlayerControllerB.DropAllHeldItems()', except just for a single item:
            item.parentObject = null;
            item.heldByPlayerOnServer = false;

            if (item.isInElevator)
            {
                item.transform.SetParent(player.playersManager.elevatorTransform, true);
            }
            else
            {
                item.transform.SetParent(player.playersManager.propsContainer, true);
            }

            player.SetItemInElevator(player.isInHangarShipRoom, player.isInElevator, item);

            item.EnablePhysics(true);
            item.EnableItemMeshes(true);

            item.transform.localScale = item.originalScale;

            item.isHeld = false;
            item.isPocketed = false;

            item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.transform.position);
            item.FallToGround(randomizePosition: true);
            item.fallTime = Random.Range(-0.3f, 0.05f);

            player.ItemSlots[slot] = null;

            if (player.IsLocalClient())
            {
                item.DiscardItemOnClient();

                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.itemSlotIcons[slot].enabled = false;
                }
            }
            else if (!item.itemProperties.syncDiscardFunction)
            {
                item.playerHeldBy = null;
            }
            // ...
        }
    }
}