using System.Runtime.CompilerServices;

namespace itolib.Compatibility
{
    internal static class BeanieLibCompatibility
    {
        /// <summary>
        ///     Whether <c>BeanieLib</c> is present in the BepInEx Chainloader or not.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("LethalGravityControl"); // Gravity...

                return (bool)_enabled;
            }
        }
        private static bool? _enabled;

        /// <summary>
        ///     Check if the item being activated is a <c>CustomShotgun</c> that is not reloading, has shells, has its safety disabled, and is not on cooldown.
        /// </summary>
        /// <param name="heldItem"><c>GrabbableObject</c> held by the player.</param>
        /// <returns>Whether the <c>CustomShotgun</c> was fired or not.</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool CheckBeanieShotgunFire(GrabbableObject? heldItem)
        {
            return heldItem is CustomShotgun shotgun && !shotgun.isReloading && shotgun.shellsLoaded > 0 && !shotgun.safetyOn
                && shotgun.useCooldown > 0.0f && (!shotgun.itemProperties.holdButtonUse || !shotgun.isBeingUsed) && shotgun.currentUseCooldown <= 0.0f;
        }
    }
}