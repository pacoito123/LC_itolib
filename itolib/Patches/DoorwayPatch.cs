using DunGen;
using HarmonyLib;
using itolib.Behaviours.Helpers;

namespace itolib.Patches
{
    [HarmonyPatch]
    internal sealed class DoorwayPatch
    {
        internal static bool? specificDoorwayActive;

        [HarmonyPatch(typeof(DoorwayPairFinder), nameof(DoorwayPairFinder.IsValidDoorwayPairing))]
        [HarmonyPostfix]
        private static void IsValidDoorwayPairingPost(ref bool __result, DoorwayProxy a, DoorwayProxy b, TileProxy previousTile, ref float weight)
        {
            specificDoorwayActive ??= a.DoorwayComponent is SpecificDoorway;

            if (!__result || specificDoorwayActive == false)
            {
                return;
            }

            if (a.DoorwayComponent is SpecificDoorway doorwayA)
            {
                if (doorwayA.AllowSwap)
                {
                    foreach (DoorwayProxy previousProxy in previousTile.UsedDoorways)
                    {
                        if (previousProxy != a && previousProxy.DoorwayComponent is not null and SpecificDoorway previousDoorway
                            && previousDoorway.AllowSwap && previousDoorway.DoorwayType == doorwayA.DoorwayType)
                        {
                            __result = false;
                            weight = 0.0f;

                            return;
                        }
                    }
                }

                __result = ModifyWeights(doorwayA, next: false, ref weight);

                if (!__result)
                {
                    return;
                }
            }

            if (b.DoorwayComponent is SpecificDoorway doorwayB)
            {
                __result = ModifyWeights(doorwayB, next: true, ref weight);
            }
        }

        private static bool ModifyWeights(SpecificDoorway doorway, bool next, ref float weight)
        {
            if ((doorway.DoorwayType == DoorwayType.Neither) || (next && doorway.DoorwayType == DoorwayType.Exit && !doorway.AllowSwap)
                || (!next && doorway.DoorwayType == DoorwayType.Entrance))
            {
                weight = 0.0f;

                return false;
            }

            if (doorway.WeightOverride != 0.0f)
            {
                weight = doorway.WeightOverride;
            }
            else
            {
                weight *= doorway.WeightMultiplier;
            }

            return true;
        }
    }
}