using DunGen;
using DunGen.Graph;
using HarmonyLib;
using itolib.Behaviours.Helpers;
using itolib.Enums;
using System.Reflection;

namespace itolib.Patches
{
    /// <summary>
    ///     Patch for <c>SpecificDoorway</c>'s functionality.
    /// </summary>
    internal static class DoorwayPatch
    {
        /// <summary>
        ///     Whether the current <c>Dungeon</c> is using <c>SpecificDoorway</c> or not. 
        /// </summary>
        internal static bool? specificDoorwayActive;

        [HarmonyPrepare]
        private static void PrepareDoorwayPatch(MethodBase original)
        {
            if (original == null)
            {
                // Reset SpecificDoorway state immediately before generation starts.
                DungeonGenerator.OnAnyDungeonGenerationStarted += static _ => specificDoorwayActive = null;
            }
        }

        [HarmonyPatch(typeof(DoorwayPairFinder), nameof(DoorwayPairFinder.IsValidDoorwayPairing))]
        [HarmonyPostfix]
        private static void IsValidDoorwayPairingPost(ref bool __result, DoorwayProxy previousDoorway, DoorwayProxy nextDoorway, TileProxy previousTile, ref float weight)
        {
            if (!__result || specificDoorwayActive == false)
            {
                return;
            }

            // Check if the first Doorway that generates is a SpecificDoorway and save the result.
            specificDoorwayActive ??= previousDoorway.DoorwayComponent is SpecificDoorway;

            if (specificDoorwayActive == false)
            {
                return;
            }

            if (previousDoorway.DoorwayComponent is SpecificDoorway doorwayA)
            {
                // Check if Doorway should be allowed to connect, if any specified Tag is matched.
                if (doorwayA.DoorwayConnectionTags?.Length > 0 && ValidateDoorwayTagConnection(doorwayA, nextDoorway.DoorwayComponent))
                {
                    __result = false;
                    weight = 0.0f;

                    return;
                }

                // Check if exits should be allowed to act as entrances, if chosen to generate as such.
                if (doorwayA.AllowSwap)
                {
                    // Look through every Doorway from the previous Tile.
                    foreach (DoorwayProxy previousProxy in previousTile.UsedDoorways)
                    {
                        // Check if previous Doorway was an entrance used as an exit (inverse logic).
                        if (previousProxy != previousDoorway && previousProxy.DoorwayComponent is SpecificDoorway previousSpecificDoorway
                            && previousSpecificDoorway.AllowSwap && previousSpecificDoorway.DoorwayType == doorwayA.DoorwayType)
                        {
                            __result = false;
                            weight = 0.0f;

                            return;
                        }
                    }
                }

                // Check if Doorway should be allowed to connect, and apply its modified weight.
                __result = ModifyWeights(doorwayA, next: false, ref weight);

                if (!__result)
                {
                    return;
                }
            }

            if (nextDoorway.DoorwayComponent is SpecificDoorway doorwayB)
            {
                // Check if Doorway should be allowed to be connected to, if any specified Tag is matched.
                if (doorwayB.DoorwayConnectionTags?.Length > 0 && ValidateDoorwayTagConnection(doorwayB, previousDoorway.DoorwayComponent))
                {
                    __result = false;
                    weight = 0.0f;

                    return;
                }

                __result = ModifyWeights(doorwayB, next: true, ref weight);
            }
        }

        /// <summary>
        ///     Modify a <c>SpecificDoorway</c>'s weight, based on its configuration.
        /// </summary>
        /// <param name="doorway"><c>SpecificDoorway</c> to modify weight for.</param>
        /// <param name="next">Whether this <c>SpecificDoorway</c> is the next <c>Tile</c>'s entrance or not.</param>
        /// <param name="weight">Weight for the <c>SpecificDoorway</c>.</param>
        /// <returns>Whether this <c>SpecificDoorway</c> should be allowed to attempt to generate the path or not.</returns>
        private static bool ModifyWeights(SpecificDoorway doorway, bool next, ref float weight)
        {
            // Check if Doorway should be allowed to generate a path, based on its configuration.
            if ((doorway.DoorwayType == DoorwayType.Neither) || (next && doorway.DoorwayType == DoorwayType.Exit && !doorway.AllowSwap)
                || (!next && doorway.DoorwayType == DoorwayType.Entrance))
            {
                weight = 0.0f;

                return false;
            }

            // Apply weight override to the Doorway.
            if (doorway.WeightOverride > 0.0f)
            {
                weight = doorway.WeightOverride * ((weight >= 1.0f) ? 100.0f : 1.0f); // Scale weight accordingly, if chosen as a straightened path.
            }

            // Apply weight multiplier to the Doorway.
            if (doorway.WeightMultiplier != 1.0f)
            {
                weight *= doorway.WeightMultiplier;
            }

            return true;
        }

        /// <summary>
        ///     Check if a connection beween a `SpecificDoorway` and a `Doorway` should be allowed or rejected.
        /// </summary>
        /// <param name="doorwayA">`SpecificDoorway` with specified tags.</param>
        /// <param name="doorwayB">`Doorway` with tags to check.</param>
        /// <returns>Whether the connection should be allowed or rejected.</returns>
        private static bool ValidateDoorwayTagConnection(SpecificDoorway doorwayA, Doorway doorwayB)
        {
            bool tagFound = false;

            // Check Doorway for any Tag that matches.
            for (int i = 0; i < doorwayA.DoorwayConnectionTags?.Length; i++)
            {
                if (doorwayB.Tags.HasTag(doorwayA.DoorwayConnectionTags[i]))
                {
                    tagFound = true;

                    break;
                }
            }

            return doorwayA.DoorwayTagConnectionMode is DungeonFlow.TagConnectionMode.Accept ? !tagFound
                : doorwayA.DoorwayTagConnectionMode is DungeonFlow.TagConnectionMode.Reject && tagFound;
        }
    }
}