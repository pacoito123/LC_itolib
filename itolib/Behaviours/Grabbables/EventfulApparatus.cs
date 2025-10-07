using GameNetcodeStuff;
using itolib.Behaviours.Helpers;
using itolib.Compatibility;
using itolib.Extensions;
using itolib.Interfaces;
using LethalLevelLoader;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class EventfulApparatus : LungProp, IEventfulItem
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Eventful Apparatus")]
        [Tooltip("")]
        [SerializeField] protected AudioSource? apparatusAudio;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Apparatus Sequence")]
        [Tooltip("")]
        [SerializeField] protected bool playDisconnectSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool playSparkPFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool playRemoveFromMachineSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool modifyEnemySpawns = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool flickerLights = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool shutOffPower = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool shutOffPowerPermanently = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool displayRadiationWarning = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool awakenOldBirds = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool triggerFacilityMeltdown = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool triggerEscapeMusic = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Apparatus Events")]
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onActivate")]
        [field: SerializeField] public UnityEvent OnApparatusActivate { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDisconnectEarly")]
        [field: SerializeField] public UnityEvent OnDisconnectEarly { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDisconnect")]
        [field: SerializeField] public UnityEvent OnDisconnect { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onLightsFlicker")]
        [field: SerializeField] public UnityEvent OnLightsFlicker { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onLightsOff")]
        [field: SerializeField] public UnityEvent OnLightsOff { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: FormerlySerializedAs("onDisplayWarning")]
        [field: SerializeField] public UnityEvent OnDisplayWarning { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Item Grabbable")]
        [field: Tooltip("")]
        [field: SerializeField] public bool SaveMaterialVariant { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool SaveMeshVariant { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public bool HideOnPocket { get; set; } = true;


        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header("Grabbable Events")]
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<bool, bool> OnActivate { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<Collider> OnActivatePhysicsTrigger { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnBatteryCharge { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnBatteryDrain { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnCollect { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnCollectEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<PlayerControllerB> OnDestroyHeldEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnDiscard { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnDiscardEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnDiscardSFX { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnDiscardSFXEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<EnemyAI> OnEnemyGrab { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnEnemyDiscard { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnEquip { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnEquipEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnGrab { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnGroundReached { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent<int> OnGroundReachedVariant { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnInspect { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnInspectEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnInteract { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnInteractLeft { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnInteractRight { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnPlace { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnPocket { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnPocketEarly { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [field: Tooltip("")]
        [field: SerializeField] public UnityEvent OnReactToSellCounter { get; set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public Action? FallWithCurveOverride { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int VariantIndex { get; set; } = -1; // TODO: Use a NetworkVariable for this.

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            if (IsHost)
            {
                Activate();
            }

            base.Start();

            HandleCompatibility();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                Activate();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void Activate()
        {
            // if (!isInShipRoom && transform.GetParent() == roundManager.mapPropsContainer)
            if (!isInShipRoom)
            {
                isLungDocked = true;
                isLungPowered = true;

                radMechEnemyType = ActivateApparatus.OldBirdEnemyType;

                if (apparatusAudio != null)
                {
                    apparatusAudio.loop = true;
                    apparatusAudio.Play();
                }

                OnApparatusActivate.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator HandleDisconnect()
        {
            if (apparatusAudio != null)
            {
                apparatusAudio.Stop();

                if (playDisconnectSFX)
                {
                    apparatusAudio.PlayOneShot(disconnectSFX, 0.7f);
                }
            }
            OnDisconnectEarly.Invoke();

            yield return new WaitForSeconds(0.1f);
            if (playSparkPFX && sparkParticle != null)
            {
                sparkParticle.SetActive(true);
            }

            if (playRemoveFromMachineSFX && apparatusAudio != null)
            {
                apparatusAudio.PlayOneShot(removeFromMachineSFX);
            }

            if (modifyEnemySpawns && IsHost && UnityEngine.Random.Range(0, 100) < 70 && roundManager.minEnemiesToSpawn < 2)
            {
                roundManager.minEnemiesToSpawn = 2;
            }
            OnDisconnect.Invoke();

            yield return new WaitForSeconds(1.0f);
            if (flickerLights)
            {
                roundManager.FlickerLights(false, false);
            }
            OnLightsFlicker.Invoke();

            yield return new WaitForSeconds(2.5f);
            if (shutOffPower)
            {
                roundManager.SwitchPower(false);
                roundManager.powerOffPermanently = shutOffPowerPermanently;
            }
            OnLightsOff.Invoke();

            yield return new WaitForSeconds(0.75f);
            if (displayRadiationWarning)
            {
                HUDManager.Instance.RadiationWarningHUD();
            }
            OnDisplayWarning.Invoke();

            if (awakenOldBirds && IsHost && radMechEnemyType != null)
            {
                EnemyAINestSpawnObject[] enemyNests = FindObjectsByType<EnemyAINestSpawnObject>(FindObjectsSortMode.None);
                for (int i = 0; i < enemyNests.Length; i++)
                {
                    if (enemyNests[i].enemyType == radMechEnemyType)
                    {
                        _ = roundManager.SpawnEnemyGameObject(roundManager.outsideAINodes[i].transform.position,
                            0f, -1, radMechEnemyType);
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected virtual void HandleCompatibility()
        {
            if (triggerFacilityMeltdown && FacilityMeltdownCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(FacilityMeltdownCompatibility.InitiateMeltdown);
            }

            if (PizzaTowerEscapeMusicCompatibility.Enabled)
            {
                OnDisconnectEarly.AddListener(() => PizzaTowerEscapeMusicCompatibility.SwitchApparatus(triggerEscapeMusic ? this : null));
            }
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
            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = false;
                isBeingUsed = false;
            }

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
            if (isLungDocked)
            {
                isLungDocked = false;
                isLungPowered = false;

                if (disconnectAnimation != null)
                {
                    StopCoroutine(disconnectAnimation);
                }

                disconnectAnimation = StartCoroutine(HandleDisconnect());

                // Invoke LLL Apparatus pull events.
                if (DungeonManager.CurrentExtendedDungeonFlow != null)
                {
                    DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onApparatusTaken.Invoke(this);
                    DungeonManager.GlobalDungeonEvents.onApparatusTaken.Invoke(this);
                }
                if (LevelManager.CurrentExtendedLevel != null)
                {
                    LevelManager.CurrentExtendedLevel.LevelEvents.onApparatusTaken.Invoke(this);
                    LevelManager.GlobalLevelEvents.onApparatusTaken.Invoke(this);
                }
            }

            OnEquipEarly.Invoke();

            base.EquipItem();

            if (playerHeldBy != null)
            {
                playerHeldBy.equippedUsableItemQE = true;
            }

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
            if (playerHeldBy != null && playerHeldBy.IsLocalClient())
            {
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
        ///     TODO.
        /// </summary>
        public override void ReactToSellingItemOnCounter()
        {
            OnReactToSellCounter.Invoke();
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