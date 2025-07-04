using DunGen;
using HarmonyLib;
using itolib.Behaviours.Helpers;

namespace itolib.Patches
{
    [HarmonyPatch]
    internal sealed class DoorwayPatch
    {
        [HarmonyPatch(typeof(DoorwayPairFinder), nameof(DoorwayPairFinder.IsValidDoorwayPairing))]
        [HarmonyPostfix]
        private static void IsValidDoorwayPairingPost(ref bool __result, DoorwayProxy a, DoorwayProxy b, TileProxy previousTile, ref float weight)
        {
            if (!__result)
            {
                return;
            }

            if (a.DoorwayComponent is SpecificDoorway doorwayA)
            {
                if (doorwayA.allowSwap)
                {
                    foreach (DoorwayProxy previousProxy in previousTile.UsedDoorways)
                    {
                        if (previousProxy != a && previousProxy.DoorwayComponent is not null and SpecificDoorway previousDoorway
                            && previousDoorway.allowSwap && previousDoorway.doorwayType == doorwayA.doorwayType)
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
            if ((doorway.doorwayType == DoorwayType.Unspecified) || (next && doorway.doorwayType == DoorwayType.Exit && !doorway.allowSwap)
                || (!next && doorway.doorwayType == DoorwayType.Entrance))
            {
                weight = 0.0f;

                return false;
            }

            if (doorway.weightOverride != 0.0f)
            {
                weight = doorway.weightOverride;
            }
            else
            {
                weight *= doorway.weightMultiplier;
            }

            return true;
        }
    }
}