using DunGen;
using System;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public enum DoorwayType
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        Unspecified = -1,
        /// <summary>
        ///     TODO.
        /// </summary>
        Entrance,
        /// <summary>
        ///     TODO.
        /// </summary>
        Exit
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class SpecificDoorway : Doorway
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public new void DebugDraw()
        {
            if (Socket == null)
            {
                return;
            }

            base.DebugDraw();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Entrance Doorway")]
        [Tooltip("")]
        public DoorwayType doorwayType = DoorwayType.Unspecified;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public bool allowSwap;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float weightOverride = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float weightMultiplier = 1.0f;
    }
}