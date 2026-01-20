using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    /// 	Valid places to attach a player wearable to.
    /// </summary>
    public enum WearablePosition
    {
        /// <summary>
        ///     Attach to a specific bone, or none at all.
        /// </summary>
        [Tooltip("Attach to a specific bone, or none at all.")]
        Custom = -1,
        /// <summary>
        ///     Attach to the player's head costume container.
        /// </summary>
        [Tooltip("Attach to the player's head costume container.")]
        Head,
        /// <summary>
        ///     Attach to the player's lower torso costume container.
        /// </summary>
        [Tooltip("Attach to the player's lower torso costume container.")]
        Belt
    }
}