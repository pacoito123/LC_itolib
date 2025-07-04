using GameNetcodeStuff;

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
    }
}