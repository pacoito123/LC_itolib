using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Structs
{
    /// <summary>
    ///     Represents a modifier to apply to a specified entry.
    /// </summary>
    [Serializable]
    public struct WeightedModifier()
    {
        /// <summary>
        ///     Index of the weight to apply this modifier to.
        /// </summary>
        [Header("Weighted Modifier")]
        [Tooltip("Index of the weight to apply this modifier to.")]
        public int modifierIndex = 0;

        /// <summary>
        ///     Value to apply to the specified entry.
        /// </summary>
        [Tooltip("Value to apply to the specified entry.")]
        public float modifierValue = 0.0f;

        /// <summary>
        ///     Type of operation this modifier should perform.
        /// </summary>
        [Tooltip("Type of operation this modifier should perform.")]
        public ModifierType modifierType = ModifierType.Additive;
    }
}