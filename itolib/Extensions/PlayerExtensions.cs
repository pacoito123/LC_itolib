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
    }
}