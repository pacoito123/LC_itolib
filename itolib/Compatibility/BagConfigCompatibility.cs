using BagConfig;
using BagConfig.Patches;
using HarmonyLib;
using LethalLevelLoader;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Compatibility
{
    [HarmonyPatch]
    internal sealed class BagConfigCompatibility
    {
        /// <summary>
        ///     Whether BagConfig is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mattymatty.BagConfig");

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        /// <summary>
        ///     Obtain Weed Killer prefab.
        /// </summary>
        public static GameObject? WeedKillerPrefab
        {
            get
            {
                if (field == null)
                {
                    WeedKillerPrefab = OriginalContent.Items.Find(item => item.itemId == 19).spawnPrefab;
                }

                return field;
            }
            private set;
        }

        private static readonly Vector3 BeltBagPocketDimensionPosition = new(3000.0f, -400.0f, 3000.0f);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        [HarmonyPatch(typeof(BeltBagPatch), nameof(BeltBagPatch.OverrideGrab))]
        [HarmonyPrefix]
        internal static void ManifestWeedKiller(BeltBagItem @this, bool right)
        {
            if (!right || !@this.IsHost)
            {
                return;
            }

            if (!PluginConfig.Misc.DropAll.Value || @this.objectsInBag.Count < 20 || WeedKillerPrefab == null
                || @this.objectsInBag.FindIndex(item => item != null && item.itemProperties != null && item.itemProperties.itemId == 19) != -1)
            {
                return;
            }

            // Obtain Weed Killer bottle from the Weed Killer dimension, and place it into the Belt Bag's pocket dimension.
            GameObject weedKillerPrefab = Object.Instantiate(WeedKillerPrefab, BeltBagPocketDimensionPosition, Quaternion.identity);

            if (weedKillerPrefab.TryGetComponent(out GrabbableObject weedKiller)
                && weedKillerPrefab.TryGetComponent(out NetworkObject weedKillerNetworkObject))
            {
                weedKiller.EnablePhysics(false);

                weedKiller.fallTime = 0.0f;
                weedKiller.targetFloorPosition = BeltBagPocketDimensionPosition;
                weedKiller.startFallingPosition = BeltBagPocketDimensionPosition;
                weedKillerNetworkObject.Spawn(false);

                // It's always been there; unable to be perceived. Now, behold, Weed Killer...
                @this.objectsInBag.Add(weedKiller);
            }
        }
    }
}