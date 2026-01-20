using GameNetcodeStuff;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Interfaces
{
    /// <summary>
    ///     Adds <i>eventfulness</i> to any implementing <c>GrabbableObject</c> class.
    /// </summary>
    public interface IEventfulItem
    {
        /// <summary>
        ///     Callback invoked when the item is activated (<c>LMB</c>), with the first parameter being whether the item has already been used up or not, and the
        ///     second parameter being whether the button is being held down or not.
        /// </summary>
        UnityEvent<bool, bool> OnActivate { get; set; }

        /// <summary>
        ///     Callback invoked when the item's <c>GrabbableObjectPhysicsTrigger</c> is triggered by a player or an enemy, with the <c>Collider</c> in question
        ///     given as parameter.
        /// </summary>
        UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; }

        /// <summary>
        ///     Callback invoked when the item's battery is charged OR when the item's battery charge is synced with clients after being discarded.
        /// </summary>
        UnityEvent OnBatteryCharge { get; set; }

        /// <summary>
        ///     Callback invoked when the item's battery is being used up.
        /// </summary>
        /// <remarks><b>NOTE:</b> Called every frame.</remarks>
        UnityEvent OnBatteryDrain { get; set; }

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship.
        /// </summary>
        UnityEvent OnCollect { get; set; }

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship, but before the radar map icon is destroyed.
        /// </summary>
        UnityEvent OnCollectEarly { get; set; }

        /// <summary>
        ///     Callback invoked immediately before the item is destroyed while being held by a player.
        /// </summary>
        UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; }

        /// <summary>
        ///     Callback invoked when the item is dropped.
        /// </summary>
        UnityEvent OnDiscard { get; set; }

        /// <summary>
        ///     Callback invoked when the item is dropped, but before the reference to the player holding it is cleared.
        /// </summary>
        UnityEvent OnDiscardEarly { get; set; }

        /// <summary>
        ///     Callback invoked when the item plays its dropped sound effect.
        /// </summary>
        UnityEvent OnDiscardSFX { get; set; }

        /// <summary>
        ///     Callback invoked immediately before the item plays its dropped sound effect.
        /// </summary>
        UnityEvent OnDiscardSFXEarly { get; set; }

        /// <summary>
        ///     Callback invoked when the item is grabbed by an enemy.
        /// </summary>
        UnityEvent<EnemyAI> OnEnemyGrab { get; set; }

        /// <summary>
        ///     Callback invoked when the item is dropped by an enemy.
        /// </summary>
        UnityEvent OnEnemyDiscard { get; set; }

        /// <summary>
        ///     Callback invoked when the item is equipped by a player.
        /// </summary>
        UnityEvent OnEquip { get; set; }

        /// <summary>
        ///     Callback invoked immediately before the item is equipped by a player.
        /// </summary>
        UnityEvent OnEquipEarly { get; set; }

        /// <summary>
        ///     Callback invoked when the item is grabbed by a player.
        /// </summary>
        UnityEvent OnGrab { get; set; }

        /// <summary>
        ///     Callback invoked when the item finishes falling.
        /// </summary>
        UnityEvent OnGroundReached { get; set; }

        /// <summary>
        ///     Callback invoked when the item finishes falling, with the specific variant index given as parameter.
        /// </summary>
        UnityEvent<int> OnGroundReachedVariant { get; set; }

        /// <summary>
        ///     Callback invoked when the item is inspected.
        /// </summary>
        UnityEvent OnInspect { get; set; }

        /// <summary>
        ///     Callback invoked immediately before the item is inspected.
        /// </summary>
        UnityEvent OnInspectEarly { get; set; }

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed on the item, regardless of whether it was successfully picked up or not.
        /// </summary>
        UnityEvent OnInteract { get; set; }

        /// <summary>
        ///     Callback invoked when <c>Q</c> is pressed while the item is held.
        /// </summary>
        UnityEvent OnInteractLeft { get; set; }

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed while the item is held.
        /// </summary>
        UnityEvent OnInteractRight { get; set; }

        /// <summary>
        ///     Callback invoked when the item is placed onto a <c>PlaceableObjectsSurface</c>.
        /// </summary>
        UnityEvent OnPlace { get; set; }

        /// <summary>
        ///     Callback invoked when the item is pocketed.
        /// </summary>
        UnityEvent OnPocket { get; set; }

        /// <summary>
        ///     Callback invoked immediately before the item is pocketed.
        /// </summary>
        UnityEvent OnPocketEarly { get; set; }

        /// <summary>
        ///     Callback invoked when the item is placed on a <c>DepositItemsDesk</c>.
        /// </summary>
        UnityEvent OnReactToSellCounter { get; set; }

        /// <summary>
        ///     Function to override the item's fall curve logic with, if one is set.
        /// </summary>
        Action? FallWithCurveOverride { get; set; }

        /// <summary>
        ///     Whether to hide the item when pocketed or not.
        /// </summary>
        bool HideOnPocket { get; set; }

        /// <summary>
        ///     Whether the item should keep its material variant when reloading the save file or not.
        /// </summary>
        bool SaveMaterialVariant { get; set; }

        /// <summary>
        ///     Whether the item should keep its mesh variant when reloading the save file or not.
        /// </summary>
        bool SaveMeshVariant { get; set; }

        /// <summary>
        ///     Index corresponding to the item's mesh and/or material variant.
        /// </summary>
        int VariantIndex { get; set; }

        /// <summary>
        ///     Clear the item's fall curve override.
        /// </summary>
        void ResetCurveOverride()
        {
            FallWithCurveOverride = null;
        }

        /// <summary>
        ///     Clear the item's fall curve override.
        /// </summary>
        void ResetCurveOverride(EnemyAI _)
        {
            ResetCurveOverride();
        }
    }
}