using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     Represents an <i>eventful</i> <c>GrabbableObject</c>, with event callbacks for everything that would otherwise require overriding from an inheriting class.
    /// </summary>
    public class ItemGrabbable : GrabbableObject, IEventfulItem
    {
        /// <summary>
        ///     Function to override the item's fall curve logic with, if one is set.
        /// </summary>
        public Action? FallWithCurveOverride { get; set; }

        /// <summary>
        ///     Index corresponding to the item's mesh and/or material variant.
        /// </summary>
        public int VariantIndex { get; set; } = -1; // TODO: Use a NetworkVariable for this.

        /// <summary>
        ///     Whether the item should keep its material variant when reloading the save file or not.
        /// </summary>
        [field: Header("Item Grabbable")]
        [field: Tooltip("Whether the item should keep its material variant when reloading the save file or not.")]
        [field: FormerlySerializedAs("saveMaterialVariant")]
        [field: SerializeField] public bool SaveMaterialVariant { get; set; }

        /// <summary>
        ///     Whether the item should keep its mesh variant when reloading the save file or not.
        /// </summary>
        [field: Tooltip("Whether the item should keep its mesh variant when reloading the save file or not.")]
        [field: FormerlySerializedAs("saveMeshVariant")]
        [field: SerializeField] public bool SaveMeshVariant { get; set; }

        /// <summary>
        ///     Whether to hide the item when pocketed or not.
        /// </summary>
        [field: Tooltip("Whether to hide the item when pocketed or not.")]
        [field: FormerlySerializedAs("hideOnPocket")]
        [field: SerializeField] public bool HideOnPocket { get; set; } = true;

        /// <summary>
        ///     Callback invoked when the item is activated (<c>LMB</c>), with the first parameter being whether the item has already been used up or not, and the
        ///     second parameter being whether the button is being held down or not.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Grabbable Events")]
        [field: Tooltip("Callback invoked when the item is activated (LMB), with the first parameter being whether the item has already been used up or not, "
            + "and the second parameter being whether the button is being held down or not.")]
        [field: FormerlySerializedAs("onActivate")]
        [field: SerializeField] public UnityEvent<bool, bool> OnActivate { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's <c>GrabbableObjectPhysicsTrigger</c> is triggered by a player or an enemy, with the <c>Collider</c> in question
        ///     given as parameter.
        /// </summary>
        [field: Tooltip("Callback invoked when the item's 'GrabbableObjectPhysicsTrigger' is triggered by a player or an enemy, with the collider in question "
            + "given as parameter.")]
        [field: FormerlySerializedAs("onActivatePhysicsTrigger")]
        [field: SerializeField] public UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's battery is charged OR when the item's battery charge is synced with clients after being discarded.
        /// </summary>
        [field: Tooltip("Callback invoked when the item's battery is charged OR when the item's battery charge is synced with clients after being discarded.")]
        [field: FormerlySerializedAs("onBatteryCharge")]
        [field: SerializeField] public UnityEvent OnBatteryCharge { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item's battery is being used up.
        /// </summary>
        /// <remarks><b>NOTE:</b> Called every frame.</remarks>
        [field: Tooltip("Callback invoked when the item's battery is being used up. NOTE: Called every frame")]
        [field: FormerlySerializedAs("onBatteryDrain")]
        [field: SerializeField] public UnityEvent OnBatteryDrain { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is brought back to the ship.")]
        [field: FormerlySerializedAs("onCollect")]
        [field: SerializeField] public UnityEvent OnCollect { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is brought back to the ship, but before the radar map icon is destroyed.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is brought back to the ship, but before the radar map icon is destroyed.")]
        [field: FormerlySerializedAs("onCollectEarly")]
        [field: SerializeField] public UnityEvent OnCollectEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is destroyed while being held by a player.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is destroyed while being held by a player.")]
        [field: FormerlySerializedAs("onDestroyHeldEarly")]
        [field: SerializeField] public UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped.")]
        [field: FormerlySerializedAs("onDiscard")]
        [field: SerializeField] public UnityEvent OnDiscard { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped, but before the reference to the player holding it is cleared.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped, but before the reference to the player holding it is cleared.")]
        [field: FormerlySerializedAs("onDiscardEarly")]
        [field: SerializeField] public UnityEvent OnDiscardEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item plays its dropped sound effect.
        /// </summary>
        [field: Tooltip("Callback invoked when the item plays its dropped sound effect.")]
        [field: FormerlySerializedAs("onDiscardSFX")]
        [field: SerializeField] public UnityEvent OnDiscardSFX { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item plays its dropped sound effect.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item plays its dropped sound effect.")]
        [field: FormerlySerializedAs("onDiscardSFXEarly")]
        [field: SerializeField] public UnityEvent OnDiscardSFXEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is grabbed by an enemy.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is grabbed by an enemy.")]
        [field: FormerlySerializedAs("onEnemyGrab")]
        [field: SerializeField] public UnityEvent<EnemyAI> OnEnemyGrab { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is dropped by an enemy.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is dropped by an enemy.")]
        [field: FormerlySerializedAs("onEnemyDiscard")]
        [field: SerializeField] public UnityEvent OnEnemyDiscard { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is equipped by a player.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is equipped by a player.")]
        [field: FormerlySerializedAs("onEquip")]
        [field: SerializeField] public UnityEvent OnEquip { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is equipped by a player.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is equipped by a player.")]
        [field: FormerlySerializedAs("onEquipEarly")]
        [field: SerializeField] public UnityEvent OnEquipEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is grabbed by a player.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is grabbed by a player.")]
        [field: FormerlySerializedAs("onGrab")]
        [field: SerializeField] public UnityEvent OnGrab { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item finishes falling.
        /// </summary>
        [field: Tooltip("Callback invoked when the item finishes falling.")]
        [field: FormerlySerializedAs("onHitGround")]
        [field: SerializeField] public UnityEvent OnGroundReached { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item finishes falling, with the specific variant index given as parameter.
        /// </summary>
        [field: Tooltip("Callback invoked when the item finishes falling, with the specific variant index given as parameter.")]
        [field: FormerlySerializedAs("onHitGroundVariant")]
        [field: SerializeField] public UnityEvent<int> OnGroundReachedVariant { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is inspected.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is inspected.")]
        [field: FormerlySerializedAs("onInspect")]
        [field: SerializeField] public UnityEvent OnInspect { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is inspected.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is inspected.")]
        [field: FormerlySerializedAs("onInspectEarly")]
        [field: SerializeField] public UnityEvent OnInspectEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed on the item, regardless of whether it was successfully picked up or not.
        /// </summary>
        [field: Tooltip("Callback invoked when 'E' is pressed on the item, regardless of whether it was successfully picked up or not.")]
        [field: FormerlySerializedAs("onInteract")]
        [field: SerializeField] public UnityEvent OnInteract { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>Q</c> is pressed while the item is held.
        /// </summary>
        [field: Tooltip("Callback invoked when 'Q' is pressed while the item is held.")]
        [field: FormerlySerializedAs("onInteractLeft")]
        [field: SerializeField] public UnityEvent OnInteractLeft { get; set; } = new();

        /// <summary>
        ///     Callback invoked when <c>E</c> is pressed while the item is held.
        /// </summary>
        [field: Tooltip("Callback invoked when 'E' is pressed while the item is held.")]
        [field: FormerlySerializedAs("onInteractRight")]
        [field: SerializeField] public UnityEvent OnInteractRight { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is placed onto a <c>PlaceableObjectsSurface</c>.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is placed onto a 'PlaceableObjectsSurface'.")]
        [field: FormerlySerializedAs("onPlace")]
        [field: SerializeField] public UnityEvent OnPlace { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is pocketed.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is pocketed.")]
        [field: FormerlySerializedAs("onPocket")]
        [field: SerializeField] public UnityEvent OnPocket { get; set; } = new();

        /// <summary>
        ///     Callback invoked immediately before the item is pocketed.
        /// </summary>
        [field: Tooltip("Callback invoked immediately before the item is pocketed.")]
        [field: FormerlySerializedAs("onPocketEarly")]
        [field: SerializeField] public UnityEvent OnPocketEarly { get; set; } = new();

        /// <summary>
        ///     Callback invoked when the item is placed on a <c>DepositItemsDesk</c>.
        /// </summary>
        [field: Tooltip("Callback invoked when the item is placed on a 'DepositItemsDesk'.")]
        [field: SerializeField] public UnityEvent OnReactToSellCounter { get; set; } = new();

        /// <summary>
        ///     Invoke brought to ship event callback upon being created (inside the ship).
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (isInShipRoom && IsHost)
            {
                OnBroughtToShip();
            }
        }

        /// <summary>
        ///     Invoke brought to ship event callback upon being spawned (inside the ship).
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (isInShipRoom && !IsHost)
            {
                OnBroughtToShip();
            }
        }

        /// <summary>
        ///     Save the item's mesh or material variant index.
        /// </summary>
        /// <returns>The item's variant index, or <c>-1</c> if no variants are set.</returns>
        public override int GetItemDataToSave()
        {
            if ((SaveMeshVariant || SaveMaterialVariant) && mainObjectRenderer == null)
            {
                Plugin.Logger.LogWarning($"Main object renderer not set for item '{name}', its variant will not be saved!");
                return -1;
            }

            // Return the mesh variant index, if set to save mesh variant.
            if (SaveMeshVariant && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                for (int i = 0; i < itemProperties.meshVariants.Length; i++)
                {
                    if (itemProperties.meshVariants[i] == itemMesh.sharedMesh)
                    {
                        return i;
                    }
                }
            }

            // Return the material variant index, if set to save material variant. Gets overridden by mesh index.
            if (SaveMaterialVariant)
            {
                for (int i = 0; i < itemProperties.materialVariants.Length; i++)
                {
                    if (itemProperties.materialVariants[i] == mainObjectRenderer.sharedMaterial)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        ///     Load the item's mesh or material variant index.
        /// </summary>
        /// <param name="saveData">Value present in the save file, corresponding to the item's variant index.</param>
        public override void LoadItemSaveData(int saveData)
        {
            if (saveData < 0 || mainObjectRenderer == null)
            {
                return;
            }

            // Load the mesh variant index, if saved.
            if (SaveMeshVariant && saveData < itemProperties.meshVariants.Length
                && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                itemMesh.sharedMesh = itemProperties.meshVariants[saveData];
            }

            // Load the material variant index, if saved.
            if (SaveMaterialVariant && saveData < itemProperties.materialVariants.Length)
            {
                mainObjectRenderer.sharedMaterial = itemProperties.materialVariants[saveData];
            }
        }

        /// <summary>
        ///     Handle the item being activated (<c>LMB</c>) by a player.
        /// </summary>
        /// <param name="used">Whether the item has already been used or not.</param>
        /// <param name="buttonDown">Whether the button is being held down or not.</param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            OnActivate.Invoke(used, buttonDown);
        }

        /// <summary>
        ///     Handle the item's <c>GrabbableObjectPhysicsTrigger</c> being triggered by a player or an enemy.
        /// </summary>
        /// <param name="other">Collider that triggered the physics trigger.</param>
        public override void ActivatePhysicsTrigger(Collider other)
        {
            base.ActivatePhysicsTrigger(other);
            OnActivatePhysicsTrigger.Invoke(other);
        }

        /// <summary>
        ///     Handle the item's battery being charged.
        /// </summary>
        public override void ChargeBatteries()
        {
            base.ChargeBatteries();
            OnBatteryCharge.Invoke();
        }

        /// <summary>
        ///     Handle the item's battery being used up.
        /// </summary>
        /// <remarks>Called every frame.</remarks>
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            OnBatteryDrain.Invoke();
        }

        /// <summary>
        ///     Handle the item being brought back to the ship.
        /// </summary>
        public override void OnBroughtToShip()
        {
            OnCollectEarly.Invoke();
            base.OnBroughtToShip();
            OnCollect.Invoke();
        }

        /// <summary>
        ///     Handle the item being destroyed from the player's hand.
        /// </summary>
        /// <param name="playerHolding">Player holding the item.</param>
        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            OnDestroyHeldEarly.Invoke(playerHolding);
            base.DestroyObjectInHand(playerHolding);
        }

        /// <summary>
        ///     Handle the item being dropped.
        /// </summary>
        public override void DiscardItem()
        {
            if (playerHeldBy != null)
            {
                // Set player as no longer holding an item with a left/right interact.
                playerHeldBy.equippedUsableItemQE = false;
                isBeingUsed = false;
            }

            OnDiscardEarly.Invoke();
            base.DiscardItem();
            OnDiscard.Invoke();
        }

        /// <summary>
        ///     Handle the item playing its drop sound effect.
        /// </summary>
        public override void PlayDropSFX()
        {
            OnDiscardSFXEarly.Invoke();
            base.PlayDropSFX();
            OnDiscardSFX.Invoke();
        }

        /// <summary>
        ///     Handle the item being grabbed by an enemy.
        /// </summary>
        /// <param name="enemy">Enemy that grabbed the item.</param>
        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            OnEnemyGrab.Invoke(enemy);
        }

        /// <summary>
        ///     Handle the item being dropped by an enemy.
        /// </summary>
        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            OnEnemyDiscard.Invoke();
        }

        /// <summary>
        ///     Handle the item being equipped in your hand.
        /// </summary>
        public override void EquipItem()
        {
            OnEquipEarly.Invoke();

            base.EquipItem();

            if (playerHeldBy != null)
            {
                // Set player as holding an item with a left/right interact, to allow their use.
                playerHeldBy.equippedUsableItemQE = true;
            }

            OnEquip.Invoke();
        }

        /// <summary>
        ///     Handle the item being grabbed by a player.
        /// </summary>
        public override void GrabItem()
        {
            base.GrabItem();
            OnGrab.Invoke();
        }

        /// <summary>
        ///     Handle the item landing on the ground.
        /// </summary>
        public override void OnHitGround()
        {
            base.OnHitGround();

            if (VariantIndex < 0)
            {
                OnGroundReached.Invoke();
            }
            else
            {
                OnGroundReachedVariant.Invoke(VariantIndex);
            }
        }

        /// <summary>
        ///     Handle the item being inspected by a player.
        /// </summary>
        public override void InspectItem()
        {
            OnInspectEarly.Invoke();
            base.InspectItem();
            OnInspect.Invoke();
        }

        /// <summary>
        ///     Handle <c>E</c> being pressed on the item by a player when trying to pick it up.
        /// </summary>
        public override void InteractItem()
        {
            base.InteractItem();
            OnInteract.Invoke();
        }

        /// <summary>
        ///     Handle <c>Q</c> or <c>E</c> being pressed by a player while holding the item.
        /// </summary>
        /// <param name="right">Whether right interact was triggered or not.</param>
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);

            if (right)
            {
                OnInteractRight.Invoke();
            }
            else
            {
                OnInteractLeft.Invoke();
            }
        }

        /// <summary>
        ///     Handle the item being placed onto a <c>PlaceableObjectsSurface</c>.
        /// </summary>
        public override void OnPlaceObject()
        {
            base.OnPlaceObject();
            OnPlace.Invoke();
        }

        /// <summary>
        ///     Handle the item being pocketed by the player holding it.
        /// </summary>
        public override void PocketItem()
        {
            if (playerHeldBy != null && playerHeldBy.IsLocalClient())
            {
                // Set player as no longer holding an item with a left/right interact.
                playerHeldBy.equippedUsableItemQE = false;
                isBeingUsed = false;
            }

            OnPocketEarly.Invoke();

            if (HideOnPocket)
            {
                base.PocketItem();
            }

            OnPocket.Invoke();
        }

        /// <summary>
        ///     Handle the item reacting to being placed on a <c>DepositItemsDesk</c>.
        /// </summary>
        public override void ReactToSellingItemOnCounter()
        {
            OnReactToSellCounter.Invoke();
        }

        /// <summary>
        ///     Handle the item's falling curve, or its override if one is set.
        /// </summary>
        public override void FallWithCurve()
        {
            if (FallWithCurveOverride != null)
            {
                FallWithCurveOverride();
            }
            else
            {
                base.FallWithCurve();
            }
        }

        /// <summary>
        ///     Sync the item's current mesh or material variant with other clients, if it has one.
        /// </summary>
        public void SyncItemVariant()
        {
            if (VariantIndex < 0 && (SaveMeshVariant || SaveMaterialVariant))
            {
                SyncItemVariantRpc(GetItemDataToSave());
            }
        }

        /// <summary>
        ///     Send the item's current mesh or material variant to everyone.
        /// </summary>
        /// <param name="variantIndex">Index of the item's mesh or material variant.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void SyncItemVariantRpc(int variantIndex)
        {
            VariantIndex = variantIndex;
        }
    }
}