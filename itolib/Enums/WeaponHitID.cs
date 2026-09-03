using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Weapon identifier value for an <c>IHittable</c> hit.
    /// </summary>
    public enum WeaponHitID : sbyte
    {
        /// <summary>
        ///     ID for a hit dealt by a <c>Shovel</c>.
        /// </summary>
        [Tooltip("ID for a hit dealt by a Shovel.")]
        Shovel = 1,
        /// <summary>
        ///     ID for a hit dealt by a <c>KnifeItem</c>.
        /// </summary>
        [Tooltip("ID for a hit dealt by a Knife.")]
        Knife = 5,
        /// <summary>
        ///     Invalid or missing ID.
        /// </summary>
        [Tooltip("Invalid or missing ID.")]
        None = -1,
    }
}