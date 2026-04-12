using DunGen;
using DunGen.Graph;
using DunGen.Tags;
using itolib.Enums;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     A <c>Doorway</c> with some added functionality, such as being able to specify if it's an <c>Entrance</c> or <c>Exit</c> to define more than one per <c>Tile</c>.
    /// </summary>
    /// <remarks><b>NOTE:</b> The first <c>Doorway</c> to generate in a <c>Dungeon</c> <b>MUST</b> be a <c>SpecificDoorway</c>, in order for them to work as intended.</remarks>
    public class SpecificDoorway : Doorway
    {
        /// <summary>
        ///     Specific type of <c>Doorway</c> this should generate as.
        /// </summary>
        /// <remarks><b>NOTE:</b> The first <c>Doorway</c> to generate in a <c>Dungeon</c> <b>MUST</b> be a <c>SpecificDoorway</c>, in order for them to work as intended.</remarks>
        [field: Space(5.0f)]
        [field: Header("Specific Doorway")]
        [field: Tooltip("Specific type of Doorway this should generate as. NOTE: The first Doorway to generate in a Dungeon MUST be a specific Doorway, "
            + "in order for them to work as intended.")]
        [field: SerializeField] public DoorwayType DoorwayType { get; private set; } = DoorwayType.Unspecified;

        /// <summary>
        ///     Whether to allow swapping <c>Doorway</c> entrances and exits or not.
        /// </summary>
        [field: Tooltip("Whether to allow swapping Doorway entrances and exits or not.")]
        [field: SerializeField] public bool AllowSwap { get; private set; }

        /// <summary>
        ///     Override the <c>Doorway</c>'s calculated weight for being chosen for a path.
        /// </summary>
        [field: Tooltip("Override the Doorway's calculated weight for being chosen for a path.")]
        [field: Range(0.0f, 1.0f)]
        [field: SerializeField] public float WeightOverride { get; private set; }

        /// <summary>
        ///     Apply a multiplier to the <c>Doorway</c>'s calculated weight (or its override).
        /// </summary>
        [field: Tooltip("Apply a multiplier to the Doorway's calculated weight (or its override).")]
        [field: Min(0.0f)]
        [field: SerializeField] public float WeightMultiplier { get; private set; } = 1.0f;

        /// <summary>
        ///     Mode in which this <c>SpecificDoorway</c> should handle connecting to any <c>Doorway</c> with a matching <c>Tag</c>.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Tags")]
        [field: Tooltip("Mode in which this specific Doorway should handle connecting to any Doorway with a matching Tag.")]
        [field: SerializeField] public DungeonFlow.TagConnectionMode DoorwayTagConnectionMode { get; private set; } = DungeonFlow.TagConnectionMode.Reject;

        /// <summary>
        ///     List of every <c>Tag</c> to search for when selecting a <c>Doorway</c> to connect to.
        /// </summary>
        /// <remarks><b>NOTE:</b> Only a single <c>Tag</c> needs to match, not all.</remarks>
        [field: Tooltip("List of every Tag to search for when selecting a Doorway to connect to. NOTE: Only a single Tag needs to match, not all.")]
        [field: SerializeField] public Tag[]? DoorwayConnectionTags { get; private set; }

        /// <summary>
        ///     Copy over configuration from an existing <c>Doorway</c>, to replace them easily.
        /// </summary>
        private void Reset()
        {
            if (TryGetComponent(out Doorway doorway))
            {
                DoorPrefabPriority = doorway.DoorPrefabPriority;

                ConnectorPrefabWeights = [.. doorway.ConnectorPrefabWeights];
                BlockerPrefabWeights = [.. doorway.BlockerPrefabWeights];

                AvoidRotatingDoorPrefab = doorway.AvoidRotatingDoorPrefab;
                AvoidRotatingBlockerPrefab = doorway.AvoidRotatingBlockerPrefab;

                ConnectorSceneObjects = [.. doorway.ConnectorSceneObjects];
                BlockerSceneObjects = [.. doorway.BlockerSceneObjects];
                Tags.Tags = [.. doorway.Tags.Tags];

                socket = doorway.Socket;
            }
        }
    }
}