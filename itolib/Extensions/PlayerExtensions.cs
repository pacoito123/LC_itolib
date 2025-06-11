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
        extension(PlayerControllerB player)
        {
            /// <summary>
            ///     TODO.
            /// </summary>
            /// <returns></returns>
            public bool IsLocalClient()
            {
                return player.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId;
            }
        }
    }
}