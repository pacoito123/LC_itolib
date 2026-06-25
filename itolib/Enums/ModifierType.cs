using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Type of operation to perform on a modifier.
    /// </summary>
    public enum ModifierType : byte
    {
        /// <summary>
        ///     Perform an additive operation for this modifier.
        /// </summary>
        [Tooltip("Perform an additive operation for this modifier.")]
        Additive,
        /// <summary>
        ///     Perform a multiplicative operation for this modifier.
        /// </summary>
        [Tooltip("Perform a multiplicative operation for this modifier.")]
        Multiplicative
    }
}