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
        [Tooltip("")]
        public bool hideOnPocket = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<bool, bool> onActivate = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<Collider> onActivatePhysicsTrigger = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onBatteryCharge = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onBatteryDrain = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onCollect = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onCollectEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onDestroyHeldEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onDiscard = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onDiscardEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onDiscardSFX = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onDiscardSFXEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<EnemyAI> onEnemyGrab = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onEnemyDiscard = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onEquip = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onEquipEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onGrab = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onHitGround = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onHitGroundVariant = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onInspect = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onInspectEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onInteract = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onInteractLeft = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onInteractRight = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onPlace = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onPocket = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onPocketEarly = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            base.Start();

            onDiscardEarly.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = false;
                    isBeingUsed = false;
                }
            });

            onEquip.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = true;
                }
            });

            onPocketEarly.AddListener(() =>
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
        /// <param name="saveData"></param>
        public override void LoadItemSaveData(int saveData)
        {
            if (saveData < 0)
            {
                return;
            }

            if (saveMeshVariant && saveData < itemProperties.meshVariants.Length
                && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                itemMesh.mesh = itemProperties.meshVariants[saveData]; // TODO: Test sharedMesh
            }

            if (saveMaterialVariant && saveData < itemProperties.materialVariants.Length)
            {
                mainObjectRenderer.sharedMaterial = itemProperties.materialVariants[saveData];
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
            onActivate.Invoke(used, buttonDown);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void ActivatePhysicsTrigger(Collider other)
        {
            base.ActivatePhysicsTrigger(other);
            onActivatePhysicsTrigger.Invoke(other);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void ChargeBatteries()
        {
            base.ChargeBatteries();
            onBatteryCharge.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            onBatteryDrain.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnBroughtToShip()
        {
            onCollectEarly.Invoke();
            base.OnBroughtToShip();
            onCollect.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerHolding"></param>
        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            onDestroyHeldEarly.Invoke(playerHolding);
            base.DestroyObjectInHand(playerHolding);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItem()
        {
            onDiscardEarly.Invoke();
            base.DiscardItem();
            onDiscard.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PlayDropSFX()
        {
            onDiscardSFXEarly.Invoke();
            base.PlayDropSFX();
            onDiscardSFX.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            onEnemyGrab.Invoke(enemy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            onEnemyDiscard.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void EquipItem()
        {
            onEquipEarly.Invoke();
            base.EquipItem();
            onEquip.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItem()
        {
            base.GrabItem();
            onGrab.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnHitGround()
        {
            base.OnHitGround();

            if (VariantIndex < 0)
            {
                onHitGround.Invoke();
            }
            else
            {
                onHitGroundVariant.Invoke(VariantIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void InspectItem()
        {
            onInspectEarly.Invoke();
            base.InspectItem();
            onInspect.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void InteractItem()
        {
            base.InteractItem();
            onInteract.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (right)
            {
                onInteractRight.Invoke();
            }
            else
            {
                onInteractLeft.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnPlaceObject()
        {
            base.OnPlaceObject();
            onPlace.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PocketItem()
        {
            onPocketEarly.Invoke();

            if (hideOnPocket)
            {
                base.PocketItem();
            }

            onPocket.Invoke();
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