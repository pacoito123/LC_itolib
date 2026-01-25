using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Variants for the player's body to spawn after dying.
    /// </summary>
    public enum PlayerRagdollVariant
    {
        /// <summary>
        ///     Normal player body ragdoll.
        /// </summary>
        [Tooltip("Normal player body ragdoll.")]
        Normal,
        /// <summary>
        ///     Headless player body ragdoll, with head burst animation (Ghost girl).
        /// </summary>
        [Tooltip("Headless player body ragdoll, with head burst animation (Ghost girl).")]
        HeadBurst,
        /// <summary>
        ///     Spring head body ragdoll (Coil-head).
        /// </summary>
        [Tooltip("Spring head body ragdoll (Coil-head).")]
        Spring,
        /// <summary>
        ///     Electrocuted body ragdoll (Circuit Bees).
        /// </summary>
        [Tooltip("Electrocuted body ragdoll (Circuit Bees).")]
        Electrocuted,
        /// <summary>
        ///     Masked player body ragdoll (Comedy).
        /// </summary>
        [Tooltip("Masked player body ragdoll (Comedy).")]
        ComedyMask,
        /// <summary>
        ///     Masked player body ragdoll (Tragedy).
        /// </summary>
        [Tooltip("Masked player body ragdoll (Tragedy).")]
        TragedyMask,
        /// <summary>
        ///     Burnt player body ragdoll (Old Bird blowtorch).
        /// </summary>
        [Tooltip("Burnt player body ragdoll (Old Bird blowtorch).")]
        Burnt,
        /// <summary>
        ///     Torso player body ragdoll (Barber).
        /// </summary>
        [Tooltip("Torso player body ragdoll (Barber).")]
        SlicedInHalf,
        /// <summary>
        ///     Headless player body ragdoll, without head burst animation (Kidnapper Fox).
        /// </summary>
        [Tooltip("Headless player body ragdoll, without head burst animation (Kidnapper Fox).")]
        HeadGone,
        /// <summary>
        ///     Various body parts ragdoll (Giant Sapsucker).
        /// </summary>
        [Tooltip("Various body parts ragdoll (Giant Sapsucker).")]
        Pieces,
    }
}