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
    }
}