using GameNetcodeStuff;
using itolib.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemGrabbable : GrabbableObject, IEventfulItem
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public Action? FallWithCurveOverride { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int VariantIndex { get; set; } = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Header("Item Grabbable")]
        [field: Tooltip("")]
        [field: FormerlySerializedAs("saveMaterialVariant")]
        [field: SerializeField] public bool SaveMaterialVariant { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("saveMeshVariant")]
        [field: SerializeField] public bool SaveMeshVariant { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("hideOnPocket")]
        [field: SerializeField] public bool HideOnPocket { get; set; } = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Grabbable Events")]
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onActivate")]
        [field: SerializeField] public UnityEvent<bool, bool> OnActivate { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onActivatePhysicsTrigger")]
        [field: SerializeField] public UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onBatteryCharge")]
        [field: SerializeField] public UnityEvent OnBatteryCharge { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onBatteryDrain")]
        [field: SerializeField] public UnityEvent OnBatteryDrain { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onCollect")]
        [field: SerializeField] public UnityEvent OnCollect { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onCollectEarly")]
        [field: SerializeField] public UnityEvent OnCollectEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDestroyHeldEarly")]
        [field: SerializeField] public UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDiscard")]
        [field: SerializeField] public UnityEvent OnDiscard { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDiscardEarly")]
        [field: SerializeField] public UnityEvent OnDiscardEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDiscardSFX")]
        [field: SerializeField] public UnityEvent OnDiscardSFX { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDiscardSFXEarly")]
        [field: SerializeField] public UnityEvent OnDiscardSFXEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onEnemyGrab")]
        [field: SerializeField] public UnityEvent<EnemyAI> OnEnemyGrab { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onEnemyDiscard")]
        [field: SerializeField] public UnityEvent OnEnemyDiscard { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onEquip")]
        [field: SerializeField] public UnityEvent OnEquip { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onEquipEarly")]
        [field: SerializeField] public UnityEvent OnEquipEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onGrab")]
        [field: SerializeField] public UnityEvent OnGrab { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onHitGround")]
        [field: SerializeField] public UnityEvent OnGroundReached { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onHitGroundVariant")]
        [field: SerializeField] public UnityEvent<int> OnGroundReachedVariant { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onInspect")]
        [field: SerializeField] public UnityEvent OnInspect { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onInspectEarly")]
        [field: SerializeField] public UnityEvent OnInspectEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onInteract")]
        [field: SerializeField] public UnityEvent OnInteract { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onInteractLeft")]
        [field: SerializeField] public UnityEvent OnInteractLeft { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onInteractRight")]
        [field: SerializeField] public UnityEvent OnInteractRight { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onPlace")]
        [field: SerializeField] public UnityEvent OnPlace { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onPocket")]
        [field: SerializeField] public UnityEvent OnPocket { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onPocketEarly")]
        [field: SerializeField] public UnityEvent OnPocketEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            base.Start();

            OnDiscardEarly.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = false;
                    isBeingUsed = false;
                }
            });

            OnEquip.AddListener(() =>
            {
                if (playerHeldBy != null)
                {
                    playerHeldBy.equippedUsableItemQE = true;
                }
            });

            OnPocketEarly.AddListener(() =>
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
        ///     TODO.
        /// </summary>
        /// <param name="saveData"></param>
        public override void LoadItemSaveData(int saveData)
        {
            if (saveData < 0)
            {
                return;
            }

            if (SaveMeshVariant && saveData < itemProperties.meshVariants.Length
                && mainObjectRenderer.TryGetComponent(out MeshFilter itemMesh))
            {
                itemMesh.sharedMesh = itemProperties.meshVariants[saveData];
            }

            if (SaveMaterialVariant && saveData < itemProperties.materialVariants.Length)
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
            OnActivate.Invoke(used, buttonDown);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public override void ActivatePhysicsTrigger(Collider other)
        {
            base.ActivatePhysicsTrigger(other);
            OnActivatePhysicsTrigger.Invoke(other);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void ChargeBatteries()
        {
            base.ChargeBatteries();
            OnBatteryCharge.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            OnBatteryDrain.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnBroughtToShip()
        {
            OnCollectEarly.Invoke();
            base.OnBroughtToShip();
            OnCollect.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerHolding"></param>
        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            OnDestroyHeldEarly.Invoke(playerHolding);
            base.DestroyObjectInHand(playerHolding);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItem()
        {
            OnDiscardEarly.Invoke();
            base.DiscardItem();
            OnDiscard.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PlayDropSFX()
        {
            OnDiscardSFXEarly.Invoke();
            base.PlayDropSFX();
            OnDiscardSFX.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            OnEnemyGrab.Invoke(enemy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            OnEnemyDiscard.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void EquipItem()
        {
            OnEquipEarly.Invoke();
            base.EquipItem();
            OnEquip.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void GrabItem()
        {
            base.GrabItem();
            OnGrab.Invoke();
        }

        /// <summary>
        ///     TODO.
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
        ///     TODO.
        /// </summary>
        public override void InspectItem()
        {
            OnInspectEarly.Invoke();
            base.InspectItem();
            OnInspect.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void InteractItem()
        {
            base.InteractItem();
            OnInteract.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
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
        ///     TODO.
        /// </summary>
        public override void OnPlaceObject()
        {
            base.OnPlaceObject();
            OnPlace.Invoke();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void PocketItem()
        {
            OnPocketEarly.Invoke();

            if (HideOnPocket)
            {
                base.PocketItem();
            }

            OnPocket.Invoke();
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
            if (VariantIndex < 0 && (SaveMeshVariant || SaveMaterialVariant))
            {
                SyncItemVariantServerRpc(GetItemDataToSave());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SyncItemVariantServerRpc(int variantIndex)
        {
            SyncItemVariantClientRpc(variantIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        private void SyncItemVariantClientRpc(int variantIndex)
        {
            VariantIndex = variantIndex;
        }
    }
}