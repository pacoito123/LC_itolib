using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemGrabbable : GrabbableObject
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public int VariantIndex { get; internal set; } = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Action? FallWithCurveOverride { get; internal set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Grabbable")]
        [Tooltip("")]
        public bool saveMaterialVariant = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool saveMeshVariant = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<bool, bool>? onActivate;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<Collider>? onActivatePhysicsTrigger;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onBatteryCharge;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onBatteryDrain;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onCollect;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onCollectEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onDestroyHeldEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDiscard;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDiscardEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDiscardSFX;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onDiscardSFXEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<EnemyAI>? onEnemyGrab;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onEnemyDiscard;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onEquip;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onEquipEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onGrab;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onHitGround;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int>? onHitGroundVariant;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onInspect;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onInspectEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onInteract;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onInteractLeft;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onInteractRight;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPlace;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPocket;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent? onPocketEarly;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            base.Start();

            onDiscardEarly?.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = false;
                    isBeingUsed = false;
                }
            });

            onEquip?.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = true;
                }
            });

            onPocketEarly?.AddListener(() =>
            {
                if (IsOwner && playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = false;
                    isBeingUsed = false;
                }
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override int GetItemDataToSave()
        {
            if (saveMeshVariant && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                for (int i = 0; i < itemProperties.meshVariants.Length; i++)
                {
                    if (itemProperties.meshVariants[i] == itemMesh.sharedMesh)
                    {
                        return i;
                    }
                }
            }

            if (saveMaterialVariant)
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
        ///     TODO.
        /// </summary>
        /// <param name="variantIndex"></param>
        public override void LoadItemSaveData(int variantIndex)
        {
            if (variantIndex < 0)
            {
                return;
            }

            if (saveMeshVariant && variantIndex < itemProperties.meshVariants.Length
                && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                itemMesh.mesh = itemProperties.meshVariants[variantIndex]; // TODO: Test sharedMesh
            }

            if (saveMaterialVariant && variantIndex < itemProperties.materialVariants.Length)
            {
                mainObjectRenderer.sharedMaterial = itemProperties.materialVariants[variantIndex];
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            onActivate?.Invoke(used, buttonDown);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void ActivatePhysicsTrigger(Collider other)
        {
            base.ActivatePhysicsTrigger(other);
            onActivatePhysicsTrigger?.Invoke(other);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void ChargeBatteries()
        {
            base.ChargeBatteries();
            onBatteryCharge?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            onBatteryDrain?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnBroughtToShip()
        {
            onCollectEarly?.Invoke();
            base.OnBroughtToShip();
            onCollect?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerHolding"></param>
        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            onDestroyHeldEarly?.Invoke(playerHolding);
            base.DestroyObjectInHand(playerHolding);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItem()
        {
            onDiscardEarly?.Invoke();
            base.DiscardItem();
            onDiscard?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PlayDropSFX()
        {
            onDiscardSFXEarly?.Invoke();
            base.PlayDropSFX();
            onDiscardSFX?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            onEnemyGrab?.Invoke(enemy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            onEnemyDiscard?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void EquipItem()
        {
            onEquipEarly?.Invoke();
            base.EquipItem();
            onEquip?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItem()
        {
            base.GrabItem();
            onGrab?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnHitGround()
        {
            base.OnHitGround();

            if (VariantIndex < 0)
            {
                onHitGround?.Invoke();
            }
            else
            {
                onHitGroundVariant?.Invoke(VariantIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void InspectItem()
        {
            onInspectEarly?.Invoke();
            base.InspectItem();
            onInspect?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void InteractItem()
        {
            base.InteractItem();
            onInteract?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (right)
            {
                onInteractRight?.Invoke();
            }
            else
            {
                onInteractLeft?.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnPlaceObject()
        {
            base.OnPlaceObject();
            onPlace?.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PocketItem()
        {
            onPocketEarly?.Invoke();
            base.PocketItem();
            onPocket?.Invoke();
        }

        /// <summary>
        ///     TODO.
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
        ///     TODO.
        /// </summary>
        public void SyncItemVariant()
        {
            if (VariantIndex < 0 && (saveMeshVariant || saveMaterialVariant))
            {
                SyncItemVariantServerRpc(GetItemDataToSave());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SyncItemVariantServerRpc(int variantIndex)
        {
            SyncItemVariantClientRpc(variantIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void SyncItemVariantClientRpc(int variantIndex)
        {
            VariantIndex = variantIndex;
        }
    }
}