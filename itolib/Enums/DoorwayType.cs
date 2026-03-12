using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Types of <c>Doorway</c> that can be set for a <c>SpecificDoorway</c> component.
    /// </summary>
    public enum DoorwayType
    {
        /// <summary>
        ///     No specified type for this <c>SpecificDoorway</c>, meaning it can be chosen as an entrance, exit, or neither (branch path).
        /// </summary>
        [Tooltip("No specified type for this specific Doorway, meaning it can be chosen as an entrance, exit, or neither (branch path).")]
        Unspecified = -1,
        /// <summary>
        ///     An entrance to the <c>Tile</c>, meaning the path is eligible to generate through this <c>SpecificDoorway</c> and into the <c>Tile</c>.
        /// </summary>
        [Tooltip("An entrance to the Tile, meaning the path is eligible to generate through this specific Doorway and into the Tile.")]
        Entrance,
        /// <summary>
        ///     An exit to the <c>Tile</c>, meaning the path is eligible to generate past this <c>SpecificDoorway</c> and out of the <c>Tile</c>.
        /// </summary>
        [Tooltip("An exit to the Tile, meaning the path is eligible to generate past this specific Doorway and out of the Tile.")]
        Exit,
        /// <summary>
        ///     Neither an entrance nor an exit to the <c>Tile</c>, meaning only branch paths are eligible to generate past this <c>SpecificDoorway</c>.
        /// </summary>
        [Tooltip("Neither an entrance nor an exit to the Tile, meaning only branch paths are eligible to generate past this specific Doorway.")]
        Neither
    }
}