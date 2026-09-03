using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Valid activation times for a class implementing `IActivationScript`.
    /// </summary>
    public enum ActivationTime : sbyte
    {
        /// <summary>
        ///     Activate immediately, as soon as possible.
        /// </summary>
        [Tooltip("Activate immediately, as soon as possible.")]
        Immediate = -1,
        /// <summary>
        ///     Activate once the <c>Dungeon</c> finishes generating.
        /// </summary>
        [Tooltip("Activate once the Dungeon finishes generating.")]
        DungeonComplete,
        /// <summary>
        ///     Activate right before <c>SpawnSyncedObjects</c> begin spawning.
        /// </summary>
        /// <remarks><b>NOTE:</b> If used in a prefab spawned through a <c>SpawnSyncedObject</c>, <c>ActivationTime.Immediate</c> should be used instead.</remarks>
        [Tooltip("Activate right before SpawnSyncedObjects begin spawning. NOTE: If used in a prefab spawned through a SpawnSyncedObject, "
            + "'Immediate' should be used instead.")]
        SyncedSpawn = 5,
        /// <summary>
        ///     Activate right before scrap begins to spawn.
        /// </summary>
        [Tooltip("Activate right before scrap begins to spawn.")]
        ScrapSpawn = 1,
        /// <summary>
        ///     Activate right before map objects (hazards) begin to spawn.
        /// </summary>
        [Tooltip("Activate right before map objects (hazards) begin to spawn.")]
        HazardSpawn,
        /// <summary>
        ///     Activate once the round begins proper.
        /// </summary>
        [Tooltip("Activate once the round begins proper.")]
        StartOfRound,
        /// <summary>
        ///     Activate manually.
        /// </summary>
        [Tooltip("Activate manually.")]
        Manual
    }
}