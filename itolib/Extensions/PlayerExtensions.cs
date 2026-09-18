using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace itolib.Extensions
{
    /// <summary>
    ///     Extensions for the <c>PlayerControllerB</c> class.
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
        ///     Check if player has unobstructed line of sight towards a specified position.
        /// </summary>
        /// <param name="player">Player whose line of sight will be checked.</param>
        /// <param name="pos">Target position to check line of sight with.</param>
        /// <param name="width">Maximum angle from the target position to be considered within line of sight.</param>
        /// <param name="range">Maximum distance from the target position to be considered within line of sight.</param>
        /// <param name="proximityAwareness">Maximum distance from the target position to be considered within line of sight, regardless of where the player is looking at.</param>
        /// <param name="layerMask"><c>LayerMask</c> for all layers that should block line of sight.</param>
        /// <returns>Whether the player has unobstructed line of sight with the target position or not.</returns>
        public static bool HasLineOfSightToPosition(this PlayerControllerB player, Vector3 pos, float width = 45.0f, float range = 60.0f,
            float proximityAwareness = -1.0f, LayerMask layerMask = default)
        {
            float sqrRange = range * range,
                sqrDistance = (player.transform.position - pos).sqrMagnitude,
                sqrProximityAwareness = (proximityAwareness > 0.0f) ? proximityAwareness * proximityAwareness : -1.0f;

            return HasLineOfSightToPosition(player, pos, player.gameplayCamera.transform.position, sqrDistance, width, sqrRange, sqrProximityAwareness, layerMask);
        }

        /// <summary>
        ///     Check if player has unobstructed line of sight towards a specified position.
        /// </summary>
        /// <param name="player">Player whose line of sight will be checked.</param>
        /// <param name="pos">Target position to check line of sight with.</param>
        /// <param name="distance">Distance between the player camera and the target position.</param>
        /// <param name="width">Maximum angle from the target position to be considered within line of sight.</param>
        /// <param name="range">Maximum distance from the target position to be considered within line of sight.</param>
        /// <param name="proximityAwareness">Maximum distance from the target position to be considered within line of sight, regardless of where the player is looking at.</param>
        /// <param name="layerMask"><c>LayerMask</c> for all layers that should block line of sight.</param>
        /// <returns>Whether the player has unobstructed line of sight with the target position or not.</returns>
        public static bool HasLineOfSightToPosition(this PlayerControllerB player, Vector3 pos, float distance, float width = 45.0f, float range = 60.0f,
            float proximityAwareness = -1.0f, LayerMask layerMask = default)
        {
            return HasLineOfSightToPosition(player, pos, player.gameplayCamera.transform.position, distance, width, range, proximityAwareness, layerMask);
        }

        /// <summary>
        ///     Check if player has unobstructed line of sight towards a specified position.
        /// </summary>
        /// <param name="player">Player whose line of sight will be checked.</param>
        /// <param name="pos">Target position to check line of sight with.</param>
        /// <param name="cameraPos">Position of the player camera.</param>
        /// <param name="distance">Distance between the player camera and the target position.</param>
        /// <param name="width">Maximum angle from the target position to be considered within line of sight.</param>
        /// <param name="range">Maximum distance from the target position to be considered within line of sight.</param>
        /// <param name="proximityAwareness">Maximum distance from the target position to be considered within line of sight, regardless of where the player is looking at.</param>
        /// <param name="layerMask"><c>LayerMask</c> for all layers that should block line of sight.</param>
        /// <returns>Whether the player has unobstructed line of sight with the target position or not.</returns>
        public static bool HasLineOfSightToPosition(this PlayerControllerB player, Vector3 pos, Vector3 cameraPos, float distance, float width = 45.0f, float range = 60.0f,
            float proximityAwareness = -1.0f, LayerMask layerMask = default)
        {
            return distance < range && (distance < proximityAwareness || Vector3.Angle(player.playerEye.forward, pos - cameraPos) < width)
                && !Physics.Linecast(player.playerEye.position, pos, out player.hit, layerMask, QueryTriggerInteraction.Ignore);
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

                return true;
            }

            return false;
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

            Transform itemTransform = item.transform;

            if (item.isInElevator)
            {
                itemTransform.SetParent(player.playersManager.elevatorTransform, true);
            }
            else
            {
                itemTransform.SetParent(player.playersManager.propsContainer, true);
            }

            player.SetItemInElevator(player.isInHangarShipRoom, player.isInElevator, item);

            item.EnablePhysics(true);
            item.EnableItemMeshes(true);

            itemTransform.localScale = item.originalScale;

            item.isHeld = false;
            item.isPocketed = false;

            item.startFallingPosition = itemTransform.GetParent().InverseTransformPoint(itemTransform.position);
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