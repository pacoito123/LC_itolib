using DunGen;
using itolib.Patches;
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
        [Tooltip("")]
        Unspecified = -1,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Entrance,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Exit
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class SpecificDoorway : Doorway, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Header("Specific Doorway")]
        [field: Tooltip("")]
        [field: SerializeField] public DoorwayType DoorwayType { get; private set; } = DoorwayType.Unspecified;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool AllowSwap { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: Min(0.0f)]
        [field: SerializeField] public float WeightOverride { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: Min(0.0f)]
        [field: SerializeField] public float WeightMultiplier { get; private set; } = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            DoorwayPatch.specificDoorwayActive = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            DoorwayPatch.specificDoorwayActive = false;
        }
    }
}